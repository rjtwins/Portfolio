use std::{rc::Rc, sync::Arc};

use crate::entities::{GameEntity, fleet::Fleet, orbit::Orbit, planet::Body};
use crate::channels::channels::{consume_star_actions, StarAction};
use rayon::prelude::*;
use serde::{Deserialize, Serialize};

#[derive(Clone, Serialize, Deserialize)]
pub struct Star{
    pub id: String,
    pub parent_x: f64,
    pub parent_y: f64,
    pub brightness: u8,
    //pub velocity: (f64, f64),

    pub orbit: Option<Orbit>,
    pub bodies: Vec<Body>,
    pub fleets: Vec<Fleet>,

}

impl Star{
    pub fn new(x: f64, y: f64, brightness: u8) -> Self {
        Self {
            id: uuid::Uuid::new_v4().to_string(),
            parent_x: x,
            parent_y: y,
            brightness,
            orbit: None,
            bodies: Vec::new(),
            fleets: Vec::new(),
        }
    }
}

impl GameEntity<Star> for Star {
    fn update(&mut self, delta_time: f64) {
        // Consume any queued star actions (e.g. fleet additions from slipways)
        for action in consume_star_actions(&self.id) {
            match action {
                StarAction::AddFleet(fleet) => self.fleets.push(fleet),
                StarAction::UpdateFleet(fleet) => {
                    if let Some(existing) = self.fleets.iter_mut().find(|f| f.id == fleet.id) {
                        *existing = fleet;
                    }
                }

                StarAction::AddShipToFleet(fleet_id, ship) => {
                    if let Some(fleet) = self.fleets.iter_mut().find(|f| f.id == fleet_id) {
                        fleet.members.push(ship);
                    }
                },

                StarAction::RemoveShipFromFleet(fleet_id, ship_id) => {
                    if let Some(fleet) = self.fleets.iter_mut().find(|f| f.id == fleet_id) {
                        fleet.members.retain(|s| s.id != ship_id);
                    }
                },

                StarAction::RemoveFleet(fleet_id) => {
                    self.fleets.retain(|f| f.id != fleet_id);
                },
                StarAction::AddColonyToBody(body_id, colony) => {
                    for body in self.bodies.iter_mut() {
                        if body.id == body_id {
                            body.colony = Some(colony);
                            break;
                        }
                        if let Some(moon) = body.moons.iter_mut().find(|m| m.id == body_id) {
                            moon.colony = Some(colony);
                            break;
                        }
                    }
                },
            }
        }

        if let Some(orbit) = &mut self.orbit {
            orbit.update_orbit(delta_time);
            orbit.x;
            orbit.y;
        }

        self.bodies.par_iter_mut().for_each(|body| body.update(delta_time));

        self.fleets.par_iter_mut().for_each(|fleet| fleet.update(delta_time));

        // Keep every slipway fleet anchored to its parent body/moon position.
        for body in &self.bodies {
            let body_pos = body.get_global_position();
            if let Some(colony) = &body.colony {
                for slipway in &colony.slipways {
                    if let Some(fleet) = self.fleets.iter_mut().find(|f| f.id == slipway.fleet_id) {
                        fleet.x = body_pos.0;
                        fleet.y = body_pos.1;
                    }
                }
            }
            for moon in &body.moons {
                let moon_pos = moon.get_global_position();
                if let Some(colony) = &moon.colony {
                    for slipway in &colony.slipways {
                        if let Some(fleet) = self.fleets.iter_mut().find(|f| f.id == slipway.fleet_id) {
                            fleet.x = moon_pos.0;
                            fleet.y = moon_pos.1;
                        }
                    }
                }
            }
        }
    }
    
    fn get_global_position(&self) -> (f64, f64) {
        if let Some(orbit) = &self.orbit {
            (orbit.x + self.parent_x, orbit.y + self.parent_y)
        } else {
            (self.parent_x, self.parent_y)
        }
    }
    
    fn get_name(&self) -> String {
        "star".to_string()
    }
    
    fn get_id(&self) -> String {
        self.id.clone()
    }
    
    fn get_orbit(&self) -> Option<Orbit> {
        self.orbit.clone()
    }
    
    fn get_parent_position(&self) -> (f64, f64) {
        (self.parent_x, self.parent_y)
    }
}
