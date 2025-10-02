use clap::Parser;
use thirtyfour::error::WebDriverErrorInfo;
use thirtyfour::prelude::*;
use tokio::time::{sleep, Duration};

#[derive(Parser, Debug)]
#[command(version, about, long_about = None)]
struct Args {
    /// Name of the person to pull demos from
    #[arg(long)]
    name: String,

    /// Number of demos to get
    #[arg(long, default_value_t = 0)]
    number_of_demos: u8,
}

#[tokio::main]
async fn main() -> WebDriverResult<()> {
    let args = Args::parse();
    let url = format!("https://www.faceit.com/en/players/{}/stats/cs2", args.name);
    println!("{}", url);

    let caps = DesiredCapabilities::chrome();
    let driver = WebDriver::new("http://127.0.0.1:9515", caps).await?;

    driver.goto(&url).await?;

    // Wait a bit for the page to settle
    sleep(Duration::from_secs(2)).await;

    // Try to handle cookie consent first (might appear before modal)
    if let Ok(_) = click_accept_all(&driver).await {
        println!("Cookie consent accepted (before modal)");
    }

    // Now try to close the login modal if it exists
    match close_login_modal(&driver).await {
        Ok(_) => {
            println!("Closed login modal");
            // After closing modal, wait and try cookies again
            sleep(Duration::from_secs(2)).await;
            match click_accept_all(&driver).await {
                Ok(_) => println!("Cookie consent accepted (after modal)"),
                Err(e) => println!("No cookie banner found after closing modal: {}", e),
            }
        }
        Err(e) => println!("No login modal to close: {}", e),
    }

    sleep(Duration::from_secs(5)).await;

    driver.execute(r#"window.scrollTo(0, document.body.scrollHeight);"#, Vec::new()).await?;

    sleep(Duration::from_secs(30)).await;

    driver.quit().await?;

    Ok(())
}

async fn close_login_modal(driver: &WebDriver) -> WebDriverResult<()> {
    let close_button = driver
        .query(By::Css(".style__CloseIconHolder-sc-7049d1c2-4"))
        .wait(Duration::from_secs(5), Duration::from_millis(500))
        .first()
        .await?;

    close_button.click().await?;
    sleep(Duration::from_millis(1000)).await;

    Ok(())
}

async fn click_accept_all(driver: &WebDriver) -> WebDriverResult<()> {
    // Try multiple selectors
    let selectors = vec![
        "button[data-testid='uc-accept-all-button']",
        "#usercentrics-root button",
        "button:has-text('Accept all')",
        "[id*='usercentrics'] button",
    ];

    for selector in selectors {
        if let Ok(button) = driver
            .query(By::Css(selector))
            .wait(Duration::from_secs(3), Duration::from_millis(500))
            .first()
            .await
        {
            println!("Found button with selector: {}", selector);
            button.click().await?;
            sleep(Duration::from_millis(1000)).await;
            return Ok(());
        }
    }

    // If all selectors fail, check if usercentrics is even present
    let uc_present = driver.find(By::Css("#usercentrics-root")).await.is_ok();
    println!("Usercentrics root present: {}", uc_present);

    Err(WebDriverError::NoSuchElement(WebDriverErrorInfo::new("Accept button not found with any selector".to_string())))
}