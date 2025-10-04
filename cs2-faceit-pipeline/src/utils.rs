use dotenv::dotenv;
use std::env;
use std::collections::HashMap;

pub fn load_env() -> HashMap<String, String> {
    // Load environment variables from .env file
    dotenv().ok();

    // Access environment variables using std::env::var
    let api_key = env::var("API_KEY")
        .expect("API_KEY must be set in .env file");

    let mut env = HashMap::new();

     env.insert("api_key".to_string(), api_key);

    return env
}