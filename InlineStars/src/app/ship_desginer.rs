use std::collections::HashMap;

use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::{entities::{player_state::{with_player_state, with_mut_player_state}, ship::{Engine, Hanger, ResourceCost, Sensor, ShipDesign, ShipSubsystem, SubsystemType, WeaponSystem}}};

pub fn with_mut_ship_designer<T, F: FnOnce(&mut ShipDesigner) -> T>(f: F) -> T {
    with_mut_player_state(|ps| f(&mut ps.ship_designer))
}

pub fn with_ship_designer<T, F: FnOnce(&ShipDesigner) -> T>(f: F) -> T {
    with_player_state(|ps| f(&ps.ship_designer))
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ShipDesigner{
    pub ship_designs: Vec<ShipDesign>,
    pub current_design: Option<ShipDesign>, // Current open design.
    pub subsystem_library: HashMap<String, ShipSubsystem>,
}

impl ShipDesigner {
    pub fn new() -> Self {
        Self {
            ship_designs: Vec::new(),
            current_design: None,
            subsystem_library: HashMap::new(),
        }
    }

    pub fn add_subsystem_to_current_design(&mut self, subsystem: ShipSubsystem) {
        if let Some(design) = &mut self.current_design {
            design.subsystems.push(subsystem);
        }
    }

    pub fn start_new_design(&mut self, name: String) {
        self.current_design = Some(ShipDesign {
            id: Uuid::new_v4().to_string(),
            //parent_fleet: Uuid::nil(),
            name,
            mass: 0.0,
            subsystems: Vec::new(),
            armor: 0.0,
            cost: ResourceCost {
                light_metals: 0.0,
                heavy_metals: 0.0,
                rare_elements: 0.0,
                super_elements: 0.0,
                fuel: 0.0,
                ic: 0.0,
            },
            locked: false,
        });
    }

    pub fn add_ship(&mut self, ship: ShipDesign) {
        self.ship_designs.push(ship);
    }

    /// Saves the current design: updates existing entry by UUID or adds it as new.
    pub fn save_current_design(&mut self) {
        let Some(design) = self.current_design.clone() else { return; };
        if let Some(existing) = self.ship_designs.iter_mut().find(|s| s.id == design.id) {
            *existing = design;
        } else {
            self.ship_designs.push(design);
        }
    }

    /// Locks the current ship design permanently. Cannot be undone.
    pub fn lock_current_design(&mut self) {
        if let Some(design) = self.current_design.as_mut() {
            design.locked = true;
        }
    }

    /// Locks a subsystem in the library permanently. Cannot be undone.
    pub fn lock_subsystem(&mut self, id: &str) {
        if let Some(ss) = self.subsystem_library.get_mut(id) {
            ss.locked = true;
        }
    }

    /// Removes the current design from saved designs and clears the workspace.
    pub fn delete_current_design(&mut self) {
        if let Some(design) = &self.current_design {
            self.ship_designs.retain(|s| s.id != design.id);
        }
        self.current_design = None;
    }
    
    pub fn new_sub_system_from_ui(&mut self, subsystem_type: SubsystemType) -> String{
        let ss_type = match subsystem_type {
            SubsystemType::Engines(_) => SubsystemType::Engines(Engine::default()),
            SubsystemType::Reactor => SubsystemType::Reactor,
            SubsystemType::Sensors(_) => SubsystemType::Sensors(Sensor::default()),
            SubsystemType::Weapons(_) => SubsystemType::Weapons(WeaponSystem::default()),
            _ => unreachable!("We cannot add subsystems of this type through the UI"),
        };

        let ss = ShipSubsystem{
            id: uuid::Uuid::new_v4().to_string(),
            cost: ResourceCost { light_metals: 0.0, heavy_metals: 0.0, rare_elements: 0.0, super_elements: 0.0, fuel: 0.0, ic: 0.0 },
            name: "New Subsystem".to_string(),
            health: 0.0,
            power_consumption: 0.0,
            power_output: 0.0,
            fuel_storage: 0.0,
            power_storage: 0.0,
            mass: 0.0,
            subsystem_type: ss_type,
            locked: false,
            obsolete: false,
            science_tech_required: None,
        };

        let id = ss.id.clone();
        self.subsystem_library.insert(ss.id.clone(), ss);

        id
    }
    
    pub fn update_subsystem_mass_from_ui(&mut self, subsystem_key: String, delta: f64) {
        let subsystem = self.subsystem_library.get_mut(&subsystem_key);
        if subsystem.is_none() {
            return;
        }
        let subsystem = subsystem.unwrap();
        subsystem.mass += delta;
        subsystem.mass = subsystem.mass.max(0.0); // Ensure mass doesn't go negative
    }

    pub fn update_subsystem_engine_tech_from_ui(&mut self, subsystem_key: String, tech_key: String) {
        let subsystem = self.subsystem_library.get_mut(&subsystem_key);
        if subsystem.is_none() {
            return;
        }
        let subsystem = subsystem.unwrap();
        let mut engine = Engine::default();
        engine.engine_tech_id = tech_key;
        subsystem.subsystem_type = SubsystemType::Engines(engine);
    }

    pub fn load_subsystem_library(&mut self) {
        #[cfg(debug_assertions)]
        let file_path = "src/data/subsystem.json";
        #[cfg(not(debug_assertions))]
        let file_path = "subsystem.json";

        let file = std::fs::File::open(file_path).expect("Failed to open subsystem.json");
        let subsystems: Vec<ShipSubsystem> =
            serde_json::from_reader(file).expect("Failed to parse subsystem.json");

        for ss in subsystems {
            self.subsystem_library.insert(ss.id.clone(), ss);
        }
    }

    /// Returns all non-obsolete subsystems whose required science tech (if any) has been researched.
    pub fn get_available_subsystems(&self, science_manager: &crate::app::science_manager::ScienceManager) -> Vec<ShipSubsystem> {
        self.subsystem_library
            .values()
            .filter(|ss| !ss.obsolete)
            .filter(|ss| match &ss.science_tech_required {
                None => true,
                Some(tech_id) => science_manager.is_researched(tech_id),
            })
            .cloned()
            .collect()
    }
}
