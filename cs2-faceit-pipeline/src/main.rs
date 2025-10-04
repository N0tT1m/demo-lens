mod api;
mod utils;

fn main() {
    let env = utils::load_env();
    println!("{}", env["api_key"]);


}
