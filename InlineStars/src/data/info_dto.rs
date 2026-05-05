use serde::Deserialize;
#[derive(Clone, Deserialize)]
pub struct InfoDTO {
    pub name: String,
    pub description: String,
    pub mass: f64,
    pub radius: f64,
    pub distance_from_parent: f64,
    pub orbital_period: f64,
    pub parent_name: Option<String>,

    #[serde(skip_deserializing)]
    pub full_horizon_text: String,
}

impl InfoDTO{
    pub fn new(name: &str) -> Self {
        InfoDTO {
            name: String::from(name),
            description: String::from("No description available."),
            mass: 0.0,
            radius: 0.0,
            distance_from_parent: 0.0,
            orbital_period: 0.0,
            parent_name: None,
            full_horizon_text: String::from("No full horizon text available."),
        }
    }
}