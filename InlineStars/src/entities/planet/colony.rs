use serde::{Deserialize, Serialize};

use super::ColonyBuilding;
use crate::{
    entities::planet::{
        Body, body::BodyResources, colony_building::{ColonyBuildingProduction, SlipWay}, ship_building::ShipBuilding
    }, get_body_by_id, input_handle::input_handle::ColonyAction
};

#[derive(Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum ResourceType {
    LightElements,
    LightMetals,
    HeavyMetals,
    RareElements,
    SuperElements,
}

impl ResourceType {
    pub fn as_str(&self) -> &'static str {
        match self {
            ResourceType::LightElements => "Light Elements",
            ResourceType::LightMetals => "Light Metals",
            ResourceType::HeavyMetals => "Heavy Metals",
            ResourceType::RareElements => "Rare Elements",
            ResourceType::SuperElements => "Super Elements",
        }
    }
}

impl std::fmt::Display for ResourceType {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_str(self.as_str())
    }
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ColonyResources {
    pub population: u32,
    pub base_ic: f64, //Base IC is the amount of IC produced without any buildings.
    pub ic: f64, //IC produced each sec, this is not stockpiled but rather the current production rate based on buildings.

    pub light_elements: f64, //Light elements stockpiles (fuel)

    pub light_metals: f64, //Light metals stockpiles
    pub heavy_metals: f64, //Heavy metals stockpiles

    pub rare_elements: f64,  //Rare elements stockpiles
    pub super_elements: f64, //Super elements stockpiles
}

#[derive(Clone, Serialize, Deserialize)]
pub struct Colony {
    pub id: String,
    pub planet_id: String,
    pub name: String,
    pub owner_id: Option<String>,
    pub buildings: Vec<ColonyBuilding>,
    pub slipways: Vec<SlipWay>,
    pub resources: ColonyResources,
}

impl Colony {
    pub fn new(name: String, planet_id: String, resources: ColonyResources) -> Self {
        Self {
            name,
            id: uuid::Uuid::new_v4().to_string(),
            planet_id,
            owner_id: None,
            buildings: Vec::new(),
            slipways: Vec::new(),
            resources,
        }
    }

    pub fn update(&mut self, body: &mut Body, dt: f64) {
        let colony_actions = crate::channels::channels::consume_colony_actions(self.id.clone());
        self.process_colony_actions(colony_actions);

        let mut buildings = std::mem::take(&mut self.buildings);

        self.resources.ic = self.get_ic_production() * dt / 60.0 / 60.0 / 24.0; //Convert IC per second to IC per day

        let mut under_construction: Vec<&mut ColonyBuilding> =
            buildings.iter_mut().filter(|b| b.is_building()).collect();

        under_construction.sort_by_key(|f| f.get_build_priority());

        for building in under_construction {
            building.build(self, dt);
        }

        let mut completed: Vec<&mut ColonyBuilding> = buildings
            .iter_mut()
            .filter(|b| b.amount_built > 0)
            .collect();

        for building in &mut completed {
            building.affect(self, body, dt);
        }

        self.buildings = buildings;

        let mut slipways = std::mem::take(&mut self.slipways);
        for slipway in &mut slipways {
            slipway.update(self, dt);
        }
        self.slipways = slipways;
    }

    fn process_colony_actions(&mut self, actions: Vec<ColonyAction>) {
        for action in actions {
            self.process_colony_action(action);
        }
    }

    fn find_slipway_mut(&mut self, slipway_id: &str) -> Option<&mut SlipWay> {
        self.slipways.iter_mut().find(|s| s.id == slipway_id)
    }

    fn find_building_index(&self, name: &str) -> Option<usize> {
        self.buildings
            .iter()
            .position(|building| building.matches_name(name))
    }

    fn find_queued_building_index(&self, name: &str) -> Option<usize> {
        self.buildings
            .iter()
            .position(|building| building.is_building() && building.matches_name(name))
    }

    fn find_built_building_index(&self, name: &str) -> Option<usize> {
        self.buildings
            .iter()
            .position(|building| building.get_built_amount() > 0 && building.matches_name(name))
    }

    fn queue_extend_slipway(&mut self, slipway_id: String) {
        if let Some(b) = self.buildings.iter_mut().find(|b| {
            b.template.id == "extend_slipway"
                && b.for_slipway_with_id.as_deref() == Some(&slipway_id)
        }) {
            b.queue_one(false);
            return;
        }
        let template = crate::entities::planet::colony_building::BUILDING_TEMPLATES
            .iter()
            .find(|t| t.id == "extend_slipway")
            .expect("extend_slipway template not found")
            .clone();
        let building = ColonyBuilding::new_for_slipway(template, slipway_id);
        self.buildings.push(building);
    }

    fn queue_building(&mut self, name: &str, infinite: bool) {
        if let Some(index) = self.find_building_index(name) {
            self.buildings[index].queue_one(infinite);
        } else {
            let mut building = ColonyBuilding::from_name(name);
            building.set_infinite(infinite);
            self.buildings.push(building);
        }
    }

    fn remove_building_if_empty(&mut self, index: usize) {
        let should_remove = self
            .buildings
            .get(index)
            .is_some_and(ColonyBuilding::is_empty);

        if should_remove {
            self.buildings.remove(index);
        }
    }

    fn process_colony_action(&mut self, action: ColonyAction) {
        match action {
            ColonyAction::QueueIncrease(building) | ColonyAction::BuildAdd(building) => {
                self.queue_building(&building, false);
            }
            ColonyAction::QueueDecrease(building) => {
                if let Some(index) = self.find_queued_building_index(&building) {
                    self.buildings[index].decrease_queue();
                    self.remove_building_if_empty(index);
                }
            }
            ColonyAction::QueueToggleInf(building) => {
                if let Some(index) = self.find_queued_building_index(&building) {
                    self.buildings[index].toggle_infinite();
                }
            }
            ColonyAction::QueuePause(building) => {
                if let Some(index) = self.find_queued_building_index(&building) {
                    self.buildings[index].toggle_paused();
                }
            }
            ColonyAction::BuildAddInf(building) => {
                self.queue_building(&building, true);
            }
            ColonyAction::FinishedDemolish(building) => {
                if let Some(index) = self.find_built_building_index(&building) {
                    self.buildings[index].demolish_one();
                    self.remove_building_if_empty(index);
                }
            }
            ColonyAction::SlipwayBuild => {
                self.queue_building("Build SlipWay", false);
            }
            ColonyAction::SlipwayExtend(slipway_id) => {
                self.queue_extend_slipway(slipway_id);
            }
            ColonyAction::SlipwayRetool(slipway_id, design_id) => {
                if let Some(slipway) = self.find_slipway_mut(&slipway_id) {
                    slipway.assign_ship_building(design_id);
                }
            }
            ColonyAction::SlipwayQueueIncrease(slipway_id) => {
                if let Some(slipway) = self.find_slipway_mut(&slipway_id) {
                    slipway.increase_queue();
                }
            }
            ColonyAction::SlipwayQueueDecrease(slipway_id) => {
                if let Some(slipway) = self.find_slipway_mut(&slipway_id) {
                    slipway.decrease_queue();
                }
            }
        }
    }

    pub fn get_ic_production(&self) -> f64 {
        let ic = self
            .buildings
            .iter()
            .map(|b| b.get_ic_production())
            .sum::<f64>();
        return ic + self.resources.base_ic;
    }

    pub fn get_build_queue_status(&self) -> Vec<BuildingStatus> {
        self.buildings
            .iter()
            .filter(|b| b.get_queue_amount() > 0)
            .map(|b: &ColonyBuilding| {
                let time_until_completion = b.get_time_to_complete(self.get_ic_production());
                let time_until_next_completion =
                    b.time_until_next_completion(self.get_ic_production());

                BuildingStatus {
                    name: b.get_name(),
                    queue_amount: b.get_queue_amount(),
                    build_amount: b.get_built_amount(),
                    is_infinite: b.is_infinite(),
                    is_paused: b.is_paused(),
                    time_until_completion,
                    time_until_next_completion,
                }
            })
            .collect()
    }

    pub fn get_build_statuses(&self) -> Vec<BuildingStatus> {
        self.buildings
            .iter()
            .filter(|b: &&ColonyBuilding| b.get_built_amount() > 0)
            .map(|b: &ColonyBuilding| BuildingStatus {
                name: b.get_name(),
                queue_amount: b.get_queue_amount(),
                build_amount: b.get_built_amount(),
                is_infinite: b.is_infinite(),
                is_paused: b.is_paused(),
                time_until_completion: 0.0,
                time_until_next_completion: 0.0,
            })
            .collect()
    }

    /// Sums up the last production of all buildings to get the total production of the colony in the last update.
    pub fn get_last_production(&self) -> ColonyBuildingProduction {
        let last_production: ColonyBuildingProduction = self
            .buildings
            .iter()
            .map(|b| b.last_production.clone())
            .sum::<ColonyBuildingProduction>();
        last_production
    }

    pub fn build_new_slip_way(&mut self) {
        let planet = get_body_by_id(self.planet_id.clone());
        let planet = if let Some(planet) = planet {
            planet
        } else {
            return;
        };

        let mut new = SlipWay::new(planet.star_id.clone());
        new.expanding = false;
        new.size = 1000;
        self.slipways.push(new);
    }

    pub fn get_slip_ways(&self) -> Vec<&SlipWay> {
        self.slipways.iter().collect()
    }
}

pub struct BuildingStatus {
    pub name: String,
    pub queue_amount: u32,
    pub build_amount: u32,
    pub is_infinite: bool,
    pub is_paused: bool,
    pub time_until_completion: f64,
    pub time_until_next_completion: f64,
}
