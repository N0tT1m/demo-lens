use anyhow::Result;
use tiberius::Client;
use tokio::net::TcpStream;
use tokio_util::compat::Compat;
use csv::Writer;
use rayon::prelude::*;
use tracing::info;
use rust_decimal::prelude::*;
use futures_util::stream::StreamExt;
use futures_util::TryStreamExt;
use tiberius::Row;

use crate::models::{RoundData, PlayerData};

pub struct RoundDataExtractor<'a> {
    client: &'a mut Client<Compat<TcpStream>>,
}

impl<'a> ClutchDataExtractor<'a> {
    pub fn new(client: &'a mut Client<Compat<TcpStream>>) -> Self { client }

    pub async fn extract(&mut self) -> Result<Vec<>> {

    }
}

impl<'a> RoundDataExtractor<'a> {
    pub fn new(client: &'a mut Client<Compat<TcpStream>>) -> Self {
        Self { client }
    }

    pub async fn extract(&mut self) -> Result<Vec<RoundData>> {
        info!("Extracting round data from database...");

        let query = r#"
            SELECT
                r.Id as round_id,
                r.CTLivePlayers as ct_live_players,
                r.TLivePlayers as t_live_players,
                r.CTEquipmentValue as ct_equipment_value,
                r.TEquipmentValue as t_equipment_value,
                r.CTStartMoney as ct_start_money,
                r.TStartMoney as t_start_money,
                CAST(r.BombPlanted AS REAL) as bomb_planted,
                r.Duration as duration,
                CAST(r.CTScore AS REAL) as ct_score,
                CAST(r.TScore AS REAL) as t_score,
                CASE WHEN r.WinnerTeam = 'CT' THEN 1 ELSE 0 END as ct_wins
            FROM Rounds r
            WHERE r.EndTick IS NOT NULL
              AND r.WinnerTeam IS NOT NULL
            ORDER BY r.Id
        "#;

        let stream = self.client.query(query, &[]).await?;
        let rows = stream.into_first_result().await?;

        let rounds: Vec<RoundData> = rows.into_iter().map(|row| {
            RoundData {
                round_id: row.get::<i32, _>(0).unwrap(),
                ct_live_players: row.get::<i32, _>(1).unwrap(),
                t_live_players: row.get::<i32, _>(2).unwrap(),
                ct_equipment_value: row.get::<i32, _>(3).unwrap(),
                t_equipment_value: row.get::<i32, _>(4).unwrap(),
                ct_start_money: row.get::<i32, _>(5).unwrap(),
                t_start_money: row.get::<i32, _>(6).unwrap(),
                bomb_planted: row.get::<f32, _>(7).unwrap(),
                duration: row.get::<f32, _>(8).unwrap(),
                ct_score: row.get::<f32, _>(9).unwrap(),
                t_score: row.get::<f32, _>(10).unwrap(),
                ct_wins: row.get::<i32, _>(11).unwrap(),
            }
        }).collect();

        info!("✓ Extracted {} rounds", rounds.len());
        Ok(rounds)
    }

    pub fn save_to_csv(&self, rounds: &[RoundData], path: &str) -> Result<()> {
        let mut wtr = Writer::from_path(path)?;

        for round in rounds {
            wtr.serialize(round)?;
        }

        wtr.flush()?;
        info!("✓ Saved rounds to {}", path);
        Ok(())
    }
}

pub struct PlayerDataExtractor<'a> {
    client: &'a mut Client<Compat<TcpStream>>,
}

impl<'a> PlayerDataExtractor<'a> {
    pub fn new(client: &'a mut Client<Compat<TcpStream>>) -> Self {
        Self { client }
    }

    pub async fn extract(&mut self) -> Result<Vec<PlayerData>> {
        info!("Extracting player statistics...");

        struct RawPlayerData {
            steam_id: String,
            player_name: String,
            avg_kills: f32,
            avg_deaths: f32,
            avg_headshots: f32,
            avg_damage: f32,
            total_clutches: i32,
            total_clutch_attempts: i32,
            avg_first_kills: f32,
            total_rounds: i32,
        }

        let query = r#"
            SELECT
                p.SteamId as steam_id,
                p.PlayerName as player_name,
                AVG(CAST(pms.Kills AS FLOAT)) as avg_kills,
                AVG(CAST(pms.Deaths AS FLOAT)) as avg_deaths,
                AVG(CAST(pms.HeadshotKills AS FLOAT)) as avg_headshots,
                AVG(CAST(pms.TotalDamageDealt AS FLOAT)) as avg_damage,
                AVG(CAST(pms.FirstKills AS FLOAT)) as avg_first_kills,
                SUM(pms.RoundsPlayed) as total_rounds
            FROM Players p
            INNER JOIN PlayerMatchStats pms ON p.Id = pms.PlayerId
            WHERE pms.Kills > 0
            GROUP BY p.SteamId, p.PlayerName
            HAVING SUM(pms.RoundsPlayed) > 10
        "#;

        let stream = self.client.query(query, &[]).await?;
        let rows = stream.into_first_result().await?;

        let raw_players: Vec<RawPlayerData> = rows.into_iter().map(|row| {
            // AVG() returns f64, not Decimal
            let avg_kills: f64 = row.get(2).unwrap();
            let avg_deaths: f64 = row.get(3).unwrap();
            let avg_headshots: f64 = row.get(4).unwrap();
            let avg_damage: f64 = row.get(5).unwrap();
            let avg_first_kills: f64 = row.get(8).unwrap();

            let steam_id: Decimal = row.get(0).unwrap();

            RawPlayerData {
                steam_id: steam_id.to_string(),
                player_name: row.get::<&str, _>(1).unwrap().to_string(),
                avg_kills: avg_kills as f32,
                avg_deaths: avg_deaths as f32,
                avg_headshots: avg_headshots as f32,
                avg_damage: avg_damage as f32,
                total_clutches: row.get::<i32, _>(6).unwrap(),
                total_clutch_attempts: row.get::<i32, _>(7).unwrap(),
                avg_first_kills: avg_first_kills as f32,
                total_rounds: row.get::<i32, _>(9).unwrap(),
            }
        }).collect();

        // Calculate derived features in parallel (FAST!)
        let players: Vec<PlayerData> = raw_players
            .par_iter()
            .map(|p| {
                let kd_ratio = if p.avg_deaths > 0.0 {
                    p.avg_kills / p.avg_deaths
                } else {
                    p.avg_kills
                };

                let headshot_pct = if p.avg_kills > 0.0 {
                    p.avg_headshots / p.avg_kills
                } else {
                    0.0
                };

                let clutch_rate = if p.total_clutch_attempts > 0 {
                    p.total_clutches as f32 / p.total_clutch_attempts as f32
                } else {
                    0.0
                };

                let adr = if p.total_rounds > 0 {
                    p.avg_damage / p.total_rounds as f32
                } else {
                    0.0
                };

                PlayerData {
                    steam_id: p.steam_id.clone(),
                    player_name: p.player_name.clone(),
                    avg_kills: p.avg_kills,
                    avg_deaths: p.avg_deaths,
                    avg_headshots: p.avg_headshots,
                    avg_damage: p.avg_damage,
                    total_clutches: p.total_clutches,
                    total_clutch_attempts: p.total_clutch_attempts,
                    avg_first_kills: p.avg_first_kills,
                    total_rounds: p.total_rounds,
                    kd_ratio,
                    headshot_pct,
                    clutch_rate,
                    adr,
                }
            })
            .collect();

        info!("✓ Extracted {} players", players.len());
        Ok(players)
    }

    pub fn save_to_csv(&self, players: &[PlayerData], path: &str) -> Result<()> {
        let mut wtr = Writer::from_path(path)?;

        for player in players {
            wtr.serialize(player)?;
        }

        wtr.flush()?;
        info!("✓ Saved players to {}", path);
        Ok(())
    }
}