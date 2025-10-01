use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RoundData {
    pub round_id: i32,
    pub ct_live_players: i32,
    pub t_live_players: i32,
    pub ct_equipment_value: i32,
    pub t_equipment_value: i32,
    pub ct_start_money: i32,
    pub t_start_money: i32,
    pub bomb_planted: f32,
    pub duration: f32,
    pub ct_score: f32,
    pub t_score: f32,
    pub ct_wins: i32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PlayerData {
    pub steam_id: String,
    pub player_name: String,
    pub avg_kills: f32,
    pub avg_deaths: f32,
    pub avg_headshots: f32,
    pub avg_damage: f32,
    pub total_clutches: i32,
    pub total_clutch_attempts: i32,
    pub avg_first_kills: f32,
    pub total_rounds: i32,
    // Derived features (calculated in Rust)
    pub kd_ratio: f32,
    pub headshot_pct: f32,
    pub clutch_rate: f32,
    pub adr: f32,
}