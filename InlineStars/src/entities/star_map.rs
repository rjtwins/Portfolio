use serde::{Deserialize, Serialize};

use crate::{channels::channels::set_star_map, entities::{GameEntity, generate_realistic_random_star_system, star::Star}};

#[derive(Clone, Serialize, Deserialize)]
pub struct StarMap {
    pub stars: Vec<Star>,
}

impl StarMap {
    pub fn new() -> Self {
        
        let mut stars = Vec::new();
        // let x = 0.0; //rand::random::<f64>() * 99000.0 + 1000.0;
        // let y = 0.0; //rand::random::<f64>() * 99000.0 + 1000.0;
        // let brightness = 3; //rand::random::<f64>();
        // let mut star = Star::new(x, y, brightness);
        let star = generate_realistic_random_star_system();
        stars.push(star);
        Self {
            stars,
        }
    }

    pub fn default() -> Self {
        Self{
            stars: Vec::new(),
        }
    }
}

impl GameEntity<StarMap> for StarMap {
    fn update(&mut self, delta_time: f64) {
        // Update star map properties based on simulation logic
        // For example, you could implement star movement or changes in brightness here

        //TODO: parallelize this
        let stars = self.stars.iter_mut();

        for star in stars {
            star.update(delta_time);
        }

        set_star_map(self.clone());
    }

    fn get_global_position(&self) -> (f64, f64) {
        (0.0, 0.0)
    }
    
    fn get_name(&self) -> String {
        "star map".to_string()
    }
    
    fn get_id(&self) -> String {
        String::new()
    }
    
    fn get_orbit(&self) -> Option<super::orbit::Orbit> {
        None
    }
    
    fn get_parent_position(&self) -> (f64, f64) {
        (0.0, 0.0)
    }
}