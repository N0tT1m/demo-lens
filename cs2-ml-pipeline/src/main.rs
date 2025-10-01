use anyhow::Result;
use clap::{Parser, Subcommand};
use tiberius::{AuthMethod, Client, Config, EncryptionLevel};
use tokio::net::TcpStream;
use tokio_util::compat::TokioAsyncWriteCompatExt;
use tracing::{info, warn};

mod extractors;
mod models;

use extractors::{RoundDataExtractor, PlayerDataExtractor};

#[derive(Parser)]
#[command(name = "cs2-ml-pipeline")]
#[command(about = "High-performance ML data pipeline for CS2 Demo Lens")]
struct Cli {
    #[command(subcommand)]
    command: Commands,

    /// Database connection string
    #[arg(long, env = "DATABASE_URL")]
    database_url: String,

    /// Output directory for CSV files
    #[arg(long, default_value = "data")]
    output_dir: String,
}

#[derive(Subcommand)]
enum Commands {
    /// Extract round data for win prediction
    ExtractRounds,

    /// Extract player statistics
    ExtractPlayers,

    /// Extract all datasets
    ExtractAll,
}

#[tokio::main]
async fn main() -> Result<()> {
    // Initialize logging
    tracing_subscriber::fmt::init();

    let cli = Cli::parse();

    // Parse connection string and create config
    info!("Connecting to database...");
    let config = parse_connection_string(&cli.database_url)?;

    let tcp = TcpStream::connect(config.get_addr()).await?;
    tcp.set_nodelay(true)?;

    let mut client = Client::connect(config, tcp.compat_write()).await?;
    info!("✓ Connected successfully!");

    // Create output directory
    std::fs::create_dir_all(&cli.output_dir)?;

    match cli.command {
        Commands::ExtractRounds => {
            extract_rounds(&mut client, &cli.output_dir).await?;
        }
        Commands::ExtractPlayers => {
            extract_players(&mut client, &cli.output_dir).await?;
        }
        Commands::ExtractAll => {
            extract_rounds(&mut client, &cli.output_dir).await?;
            extract_players(&mut client, &cli.output_dir).await?;
        }
    }

    Ok(())
}

fn parse_connection_string(conn_str: &str) -> Result<Config> {
    let mut config = Config::new();

    // Check if it's a URL format (mssql://...)
    if conn_str.starts_with("mssql://") {
        // Parse URL format: mssql://user:password@host:port/database
        let without_prefix = conn_str.strip_prefix("mssql://").unwrap();

        // Split at @ to separate credentials from host
        if let Some((creds_part, host_part)) = without_prefix.split_once('@') {
            // Parse credentials
            if let Some((username, password)) = creds_part.split_once(':') {
                config.authentication(AuthMethod::sql_server(username, password));
            }

            // Parse host:port/database
            if let Some((host_port, database)) = host_part.split_once('/') {
                config.database(database);

                if let Some((host, port)) = host_port.split_once(':') {
                    config.host(host);
                    config.port(port.parse()?);
                } else {
                    config.host(host_port);
                }
            } else {
                // No database specified
                if let Some((host, port)) = host_part.split_once(':') {
                    config.host(host);
                    config.port(port.parse()?);
                } else {
                    config.host(host_part);
                }
            }
        }

        config.encryption(EncryptionLevel::NotSupported); // Disable encryption for local dev
        return Ok(config);
    }

    // Parse ADO.NET connection string format (Server=...;Database=...)
    for part in conn_str.split(';') {
        let part = part.trim();
        if part.is_empty() {
            continue;
        }

        if let Some((key, value)) = part.split_once('=') {
            let key = key.trim().to_lowercase();
            let value = value.trim();

            match key.as_str() {
                "server" | "data source" => {
                    if let Some((host, port)) = value.split_once(',') {
                        config.host(host);
                        config.port(port.parse()?);
                    } else {
                        config.host(value);
                    }
                }
                "database" | "initial catalog" => {
                    config.database(value);
                }
                "user id" | "uid" => {
                    config.authentication(AuthMethod::sql_server(value, ""));
                }
                "password" | "pwd" => {
                    // Password will be set with user
                }
                "integrated security" | "trusted_connection" => {
                    // Integrated/Windows authentication not supported in tiberius 0.12
                    // Use SQL Server authentication instead
                }
                "trustservercertificate" => {
                    if value.eq_ignore_ascii_case("true") {
                        config.trust_cert();
                    }
                }
                _ => {}
            }
        }
    }

    // Handle username/password together for ADO.NET format
    let mut username = String::new();
    let mut password = String::new();

    for part in conn_str.split(';') {
        if let Some((key, value)) = part.split_once('=') {
            let key = key.trim().to_lowercase();
            match key.as_str() {
                "user id" | "uid" => username = value.trim().to_string(),
                "password" | "pwd" => password = value.trim().to_string(),
                _ => {}
            }
        }
    }

    if !username.is_empty() {
        config.authentication(AuthMethod::sql_server(&username, &password));
    }

    Ok(config)
}

async fn extract_rounds(client: &mut Client<tokio_util::compat::Compat<TcpStream>>, output_dir: &str) -> Result<()> {
    let mut extractor = RoundDataExtractor::new(client);
    let rounds = extractor.extract().await?;

    if rounds.len() < 100 {
        warn!("Only {} rounds found. Minimum 100 recommended.", rounds.len());
    }

    let output_path = format!("{}/round_data.csv", output_dir);
    extractor.save_to_csv(&rounds, &output_path)?;

    Ok(())
}

async fn extract_players(client: &mut Client<tokio_util::compat::Compat<TcpStream>>, output_dir: &str) -> Result<()> {
    let mut extractor = PlayerDataExtractor::new(client);
    let players = extractor.extract().await?;

    let output_path = format!("{}/player_data.csv", output_dir);
    extractor.save_to_csv(&players, &output_path)?;

    Ok(())
}