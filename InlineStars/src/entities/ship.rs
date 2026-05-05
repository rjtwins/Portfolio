use std::{iter::Sum, ops::Add};
use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::{app::science_manager::{ScienceItemEffect, with_science_manager}, entities::planet::colony::ResourceType};

const BASE_TTW: f64 = 820.0; //thrust/wait (kg/kg)
const BASE_FUEL_EFFICIENCY: f64 = 0.0000018; //fuel/s/mass of engine (kg)

#[derive(Clone, Serialize, Deserialize)]
pub struct Ship {
    pub id: String,
    pub design: ShipDesign,
}

impl Ship{
    pub fn get_speed(&self) -> f64 {
        self.design.get_speed()
    }

    pub fn get_endurance(&self) -> f64 {
        self.design.get_endurance()
    }

    pub fn get_range(&self) -> f64 {
        self.design.get_total_range()
    }

    
}


impl Ship{
    pub fn new(design: ShipDesign) -> Self {
        Self {
            id: Uuid::new_v4().to_string(),
            design,
        }
    }
}


#[derive(Clone, Serialize, Deserialize)]
pub enum SubsystemType{
    Misc,
    Reactor,
    Engines(Engine),
    Sensors(Sensor),
    Weapons(WeaponSystem),
    Hanger(Hanger),
    Storage(Storage),
    ColonyModule,
}

impl PartialEq for SubsystemType {
    fn eq(&self, other: &Self) -> bool {
        match (self, other) {
            (SubsystemType::Misc, SubsystemType::Misc) => true,
            (SubsystemType::Reactor, SubsystemType::Reactor) => true,
            (SubsystemType::Engines(_), SubsystemType::Engines(_)) => true,
            (SubsystemType::Sensors(_), SubsystemType::Sensors(_)) => true,
            (SubsystemType::Weapons(_), SubsystemType::Weapons(_)) => true,
            (SubsystemType::Hanger(_), SubsystemType::Hanger(_)) => true,
            (SubsystemType::Storage(_), SubsystemType::Storage(_)) => true,
            (SubsystemType::ColonyModule, SubsystemType::ColonyModule) => true,
            _ => false,
        }
    }
}

impl SubsystemType {
    pub fn get_subsystem_type_name(&self) -> String {
        match self {
            SubsystemType::Misc => "Misc".to_string(),
            SubsystemType::Reactor => "Reactor".to_string(),
            SubsystemType::Engines(_) => "Engines".to_string(),
            SubsystemType::Sensors(_) => "Sensors".to_string(),
            SubsystemType::Weapons(_) => "Weapons".to_string(),
            SubsystemType::Hanger(_) => "Hanger".to_string(),
            SubsystemType::Storage(_) => "Storage".to_string(),
            SubsystemType::ColonyModule => "Colony Module".to_string(),
        }
    }
}

impl Eq for SubsystemType {}

#[derive(Clone, Serialize, Deserialize)]
pub struct Storage {
    pub human_rated: bool,
    pub capacity: f64,
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ShipDesign {
    pub id: String,
    pub name: String,

    pub mass: f64, //kg
    //pub speed: f64, //km/s

    pub subsystems: Vec<ShipSubsystem>,
    pub locked : bool,

    pub armor: f64,

    pub cost: ResourceCost,
}

impl Default for ShipDesign {
    fn default() -> Self {
        Self {
            id: Uuid::new_v4().to_string(),
            name: "New Ship".to_string(),
            mass: 0.0,
            subsystems: Vec::new(),
            locked: false,
            armor: 0.0,
            cost: ResourceCost {
                light_metals: 0.0,
                heavy_metals: 0.0,
                rare_elements: 0.0,
                super_elements: 0.0,
                fuel: 0.0,
                ic: 0.0,
            },
        }
    }
}

impl ShipDesign{
    pub fn get_total_cost(&self) -> ResourceCost {
        self.cost.clone()
            + self.subsystems.iter().map(|s| s.cost.clone()).sum::<ResourceCost>()
    }

    //Secondes of operation at max speed
    pub fn get_endurance(&self) -> f64 {
        self.get_total_fuel_capacity() / self.total_fuel_consumption()
    }

    //Maximum distance at max speed
    pub fn get_total_range(&self) -> f64 {
        self.get_total_fuel_capacity() / self.total_fuel_consumption() * self.get_speed()
    }

    pub fn get_total_fuel_capacity(&self) -> f64 {
        self.subsystems.iter().map(|ss| ss.fuel_storage).sum()
    }

    pub fn get_speed(&self) -> f64 {
        self.total_thrust() / self.total_mass()
    }

    pub fn get_power_balance(&self) -> f64 {
        self.total_power_output() - self.total_power_consumption()
    }


    pub fn total_mass(&self) -> f64 {
        self.mass + self.subsystems.iter().map(|s| s.mass).sum::<f64>()
    }

    pub fn total_power_output(&self) -> f64 {
        self.subsystems.iter().map(|s| s.power_output).sum()
    }

    pub fn total_power_consumption(&self) -> f64 {
        self.subsystems.iter().map(|s| s.power_consumption).sum()
    }

    pub fn total_thrust(&self) -> f64 {
        self.subsystems.iter().map(|s| {
            if let SubsystemType::Engines(engine) = &s.subsystem_type {
                engine.get_thrust(s)
            } else {
                0.0
            }
        }).sum()
    }

    pub fn total_fuel_consumption(&self) -> f64 {
        self.subsystems.iter().map(|s| {
            if let SubsystemType::Engines(engine) = &s.subsystem_type {
                engine.get_fuel_consumption(s)
            } else {
                0.0
            }
        }).sum()
    }
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ShipSubsystem {
    pub id: String,

    pub cost: ResourceCost,

    pub name: String,
    pub health: f64,

    pub power_consumption: f64,
    pub power_output: f64,

    pub fuel_storage: f64, //Kg
    pub power_storage: f64,

    pub mass: f64, //Kg

    pub subsystem_type: SubsystemType,

    pub science_tech_required: Option<String>,

    pub locked: bool,
    pub obsolete: bool,
}

#[derive(Clone, Serialize, Deserialize)]
pub struct Hanger {
    pub capacity: usize,
    //pub stored_ships: Vec<Ship>,
}

#[derive(Clone, Default, Serialize, Deserialize)]
pub struct Sensor{
    pub sensor_range: f64,
    pub sensor_resolution: f64,
}

#[derive(Clone, Default, Serialize, Deserialize)]
pub struct WeaponSystem {
    pub damage: f64,
    pub range: f64,
    pub fire_rate: f64,
}

#[derive(Clone, Serialize, Deserialize)]
pub struct Engine {
    pub engine_tech_id: String,
}

impl Default for Engine {
    fn default() -> Self {
        Self {
            engine_tech_id: String::new(),
        }
    }
}

impl Engine{
    pub fn get_thrust(&self, parent_component: &ShipSubsystem) -> f64 {
        let tech = with_science_manager(|sm| sm.items.get(&self.engine_tech_id).cloned());
        let tech_multiplier = match tech {
            Some(tech) => tech.effects.iter().filter_map(|effect| match effect {
                ScienceItemEffect::EngineTech(tech) => Some(tech.thrust_multiplier),
                _ => None,
            }).product::<f64>(),
            None => 1.0,
        };

        BASE_TTW * tech_multiplier * parent_component.mass
    }

    pub fn get_fuel_consumption(&self, parent_component: &ShipSubsystem) -> f64 {
        let tech = with_science_manager(|sm| sm.items.get(&self.engine_tech_id).cloned());
        let tech_multiplier = match tech {
            Some(tech) => tech.effects.iter().filter_map(|effect| match effect {
                ScienceItemEffect::EngineTech(tech) => Some(tech.fuel_efficiency_multiplier),
                _ => None,
            }).product::<f64>(),
            None => 1.0,
        };

        BASE_FUEL_EFFICIENCY * tech_multiplier * parent_component.mass
    }

    pub fn get_fuel_type(&self) -> ResourceType {
        let tech = with_science_manager(|sm| sm.items.get(&self.engine_tech_id).cloned());
        match tech.and_then(|tech| tech.effects.into_iter().find_map(|effect| match effect {
            ScienceItemEffect::EngineTech(tech) => Some(tech.fuel_type),
            _ => None,
        })) {
            Some(fuel_type) => fuel_type,
            None => ResourceType::LightElements, //Default fuel type
        }
    }
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ResourceCost{
    pub light_metals: f64, //Light metals cost
    pub heavy_metals: f64, //Heavy metals cost
    pub rare_elements: f64, //Rare elements cost
    pub super_elements: f64, //Super elements cost
    pub fuel: f64, //Fuel cost
    pub ic: f64, //Industrial capacity cost
}

impl ResourceCost{
    pub fn total_bp(&self) -> f64 {
        self.light_metals + self.heavy_metals + self.rare_elements + self.super_elements + self.fuel + self.ic
    }

    pub fn default() -> Self {
        Self {
            light_metals: 0.0,
            heavy_metals: 0.0,
            rare_elements: 0.0,
            super_elements: 0.0,
            fuel: 0.0,
            ic: 0.0,
        }
    }
}

impl Sum for ResourceCost {
    fn sum<I: Iterator<Item = Self>>(iter: I) -> Self {
        iter.fold(ResourceCost {
            light_metals: 0.0,
            heavy_metals: 0.0,
            rare_elements: 0.0,
            super_elements: 0.0,
            fuel: 0.0,
            ic: 0.0,
        }, |acc, x| ResourceCost {
            light_metals: acc.light_metals + x.light_metals,
            heavy_metals: acc.heavy_metals + x.heavy_metals,
            rare_elements: acc.rare_elements + x.rare_elements,
            super_elements: acc.super_elements + x.super_elements,
            fuel: acc.fuel + x.fuel,
            ic: acc.ic + x.ic,
        })
    }
}

impl Add for ResourceCost {
    type Output = Self;

    fn add(self, other: Self) -> Self {
        ResourceCost {
            light_metals: self.light_metals + other.light_metals,
            heavy_metals: self.heavy_metals + other.heavy_metals,
            rare_elements: self.rare_elements + other.rare_elements,
            super_elements: self.super_elements + other.super_elements,
            fuel: self.fuel + other.fuel,
            ic: self.ic + other.ic,
        }
    }
}