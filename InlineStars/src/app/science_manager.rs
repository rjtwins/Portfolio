use std::{
    collections::{HashMap, HashSet},
};

use serde::{Deserialize, Serialize};

use crate::entities::{planet::colony::ResourceType, player_state::{with_player_state, with_mut_player_state}};

pub fn with_mut_science_manager<T, F: FnOnce(&mut ScienceManager) -> T>(f: F) -> T {
    with_mut_player_state(|ps| f(&mut ps.science_manager))
}

pub fn with_science_manager<T, F: FnOnce(&ScienceManager) -> T>(f: F) -> T {
    with_player_state(|ps| f(&ps.science_manager))
}

#[derive(Clone, Serialize, Deserialize)]
pub enum ScienceItemEffect {
    EngineTech(EngineTech),
    ResearchSpeedMultiplier(f64),
    MiningYieldMultiplier(f64),
    IndustrialCapacityMultiplier(f64),
    BuildingSpeedMultiplier(f64),
    BuildingCostMultiplier(f64),
    ShipBuildingSpeedMultiplier(f64),
    ShipBuildingCostMultiplier(f64),
}

#[derive(Clone, Serialize, Deserialize)]
pub struct EngineTech {
    pub thrust_multiplier: f64,
    pub fuel_efficiency_multiplier: f64,
    pub fuel_type: ResourceType,
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ScienceManager {
    //Currently active research item(s), if any
    pub research_queue: Vec<String>,

    //All possible science items, indexed by their ID
    pub items: HashMap<String, ScienceItem>,

    //Set of researched item IDs for quick lookup
    researched_items: HashSet<String>,
    base_research_speed: f64,
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ScienceItem {
    pub id: String,
    pub prerequisites: Vec<String>,
    pub name: String,
    pub description: String,
    pub cost: u32,
    pub progress: f64,
    pub effects: Vec<ScienceItemEffect>,
}

impl ScienceManager {
    pub fn new() -> Self {
        #[cfg(debug_assertions)]
        let file_path = "src/data/science_items.json";
        #[cfg(not(debug_assertions))]
        let file_path = "science_items.json";
        let file = std::fs::File::open(file_path)
            .expect("Failed to open science_items.json");
        let items: HashMap<String, ScienceItem> =
            serde_json::from_reader::<_, Vec<ScienceItem>>(file)
                .expect("Failed to parse science_items.json")
                .into_iter()
                .map(|item| (item.id.clone(), item))
                .collect();

        let completed_items = items
            .iter()
            .filter(|item| item.1.progress >= 1.0)
            .map(|item| item.0.clone())
            .collect::<HashSet<String>>();

        ScienceManager {
            research_queue: Vec::new(),
            items,
            researched_items: completed_items,
            base_research_speed: 1.0,
        }
    }

    pub fn add_to_research_queue(&mut self, item_id: String) {
        if self.can_research(&item_id) && !self.research_queue.contains(&item_id) {
            self.research_queue.push(item_id);
        }
    }

    pub fn get_items_in_queue(&self) -> Vec<ScienceItem> {
        self.research_queue
            .iter()
            .filter_map(|id| self.items.get(id))
            .cloned()
            .collect()
    }

    pub fn can_research(&self, item_id: &String) -> bool {
        if let Some(item) = self.items.get(item_id) {
            item.prerequisites
                .iter()
                .all(|prereq| self.researched_items.contains(prereq))
        } else {
            false
        }
    }

    pub fn get_prerequisites(&self, item_id: &String) -> Vec<ScienceItem> {
        self.items
            .get(item_id)
            .map(|item| {
                item.prerequisites
                    .iter()
                    .filter_map(|prereq_id| self.items.get(prereq_id))
                    .cloned()
                    .collect()
            })
            .unwrap_or_else(Vec::new)
    }

    pub fn get_active_effects(&self) -> Vec<ScienceItemEffect> {
        self.researched_items
            .iter()
            .filter_map(|id| self.items.get(id))
            .flat_map(|item| item.effects.clone())
            .collect()
    }

    pub fn get_engine_techs(&self) -> Vec<ScienceItem> {
        let items: Vec<ScienceItem> = self.researched_items
            .iter()
            .map(|key| self.items.get(key))
            .filter_map(|item_opt| item_opt)
            .filter(|item| item.effects.iter().any(|effect| matches!(effect, ScienceItemEffect::EngineTech(_))))
            .cloned()
            .collect();

        return items;
    }

    pub fn update(&mut self, dt: f64) {
        // Update research progress, apply effects of researched items, etc.
        let progress = self
            .get_active_effects()
            .iter()
            .filter_map(|effect| {
                if let ScienceItemEffect::ResearchSpeedMultiplier(mult) = effect {
                    Some(*mult)
                } else {
                    None
                }
            })
            .product::<f64>()
            * self.base_research_speed as f64
            * dt;

        let current_id = match self.research_queue.first() {
            Some(id) => id.clone(),
            None => return,
        };

        let current_item = match self.items.get_mut(&current_id) {
            Some(item) => item,
            None => return,
        };

        let frac_progress_made = (progress / current_item.cost as f64);

        current_item.progress += frac_progress_made;
        if current_item.progress >= 1.0{
            self.researched_items.insert(current_id.clone());
            self.research_queue.remove(0);
        }
    }
    
    pub fn get_possible_items(&self) -> Vec<ScienceItem> {
        let possible_items: Vec<ScienceItem> = self.items
            .values()
            .filter(|item| self.can_research(&item.id))
            .filter(|item| !self.researched_items.contains(&item.id))
            .filter(|item| !self.research_queue.contains(&item.id))
            .cloned()
            .collect();

        possible_items
    }
    
    pub fn move_item_up_in_queue(&mut self, item_id: String) {
        if let Some(pos) = self.research_queue.iter().position(|id| *id == item_id) {
            if pos > 0 {
                self.research_queue.swap(pos, pos - 1);
            }
        }
    }
    
    pub fn move_item_down_in_queue(&mut self, item_id: String) {
        if let Some(pos) = self.research_queue.iter().position(|id| *id == item_id) {
            if pos < self.research_queue.len() - 1 {
                self.research_queue.swap(pos, pos + 1);
            }
        }
    }
    
    pub fn remove_from_research_queue(&mut self, item_id: String) {
        if let Some(pos) = self.research_queue.iter().position(|id| *id == item_id) {
            self.research_queue.remove(pos);
        }
    }

    pub fn is_researched(&self, item_id: &str) -> bool {
        self.researched_items.contains(item_id)
    }
}
