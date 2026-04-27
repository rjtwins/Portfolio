use std::sync::{LazyLock, Mutex};

use serde::{Deserialize, Serialize};

use crate::{app::{science_manager::ScienceManager, ship_desginer::ShipDesigner}, entities::planet::Colony};

static PLAYER_STATE: LazyLock<Mutex<PlayerState>> =
    LazyLock::new(|| Mutex::new(PlayerState::new("Player".to_string())));

pub fn with_player_state<T, F: FnOnce(&PlayerState) -> T>(f: F) -> T {
    f(&PLAYER_STATE.lock().unwrap())
}

pub fn with_mut_player_state<T, F: FnOnce(&mut PlayerState) -> T>(f: F) -> T {
    f(&mut PLAYER_STATE.lock().unwrap())
}

#[derive(Clone, Serialize, Deserialize)]
pub struct PlayerState {
    pub id: String,
    pub name: String,
    pub wealth: u32,
    colonies: Vec<String>,
    pub ship_designer: ShipDesigner,
    pub science_manager: ScienceManager,
}

impl PlayerState {
    pub fn new(name: String) -> Self {
        Self {
            id: uuid::Uuid::new_v4().to_string(),
            name,
            wealth: 0,
            colonies: Vec::new(),
            ship_designer: ShipDesigner::new(),
            science_manager: ScienceManager::new(),
        }
    }

    pub fn new_with_id(name: String, id: String) -> Self {
        Self {
            id,
            name,
            wealth: 0,
            colonies: Vec::new(),
            ship_designer: ShipDesigner::new(),
            science_manager: ScienceManager::new(),
        }
    }

    pub fn add_colony(&mut self, colony_id: String) {
        self.colonies.push(colony_id);
    }

    pub fn remove_colony(&mut self, colony_id: String) {
        self.colonies.retain(|id| *id != colony_id);
    }
}
