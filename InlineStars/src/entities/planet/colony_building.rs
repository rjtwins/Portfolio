use std::{default, iter::Sum, sync::LazyLock};

use serde::{Deserialize, Serialize};

use crate::{channels::channels::{StarAction, add_star_action, insert_fleet_order}, entities::{fleet::{self, Fleet, FleetOrder, FleetOrderType}, planet::{Body, ship_building::{self, ShipBuilding}}, ship::Ship}, get_body_by_id, get_star_by_id};

use super::Colony;

pub static BUILDING_TEMPLATES: LazyLock<Vec<ColonyBuildingTemplate>> = LazyLock::new(|| {
    let path = if cfg!(debug_assertions) {
        "src/data/colony_building.json"
    } else {
        "colony_building.json"
    };
    let file = std::fs::File::open(path).expect("Failed to open colony_building.json");
    serde_json::from_reader(file).expect("Failed to parse colony_building.json")
});

#[derive(Clone, Default, Serialize, Deserialize)]
pub struct ColonyBuildingProduction{
    //Generic production
    pub fuel_production: f64,
    pub light_metals_production: f64,
    pub heavy_metals_production: f64,
    pub rare_elements_production: f64,
    pub super_elements_production: f64,

    pub science_production: f64,
    pub wealth_production: f64,

    //Mining:
    pub fuel_mining_production: f64,
    pub light_metals_mining_production: f64,
    pub heavy_metals_mining_production: f64,
    pub rare_elements_mining_production: f64,
    pub super_elements_mining_production: f64,
}

impl Sum for ColonyBuildingProduction {
    fn sum<I: Iterator<Item = Self>>(iter: I) -> Self {
        let mut total = ColonyBuildingProduction::default();
        for prod in iter {
            total.fuel_production += prod.fuel_production;
            total.light_metals_production += prod.light_metals_production;
            total.heavy_metals_production += prod.heavy_metals_production;
            total.rare_elements_production += prod.rare_elements_production;
            total.super_elements_production += prod.super_elements_production;
            total.science_production += prod.science_production;
            total.wealth_production += prod.wealth_production;

            total.fuel_mining_production += prod.fuel_mining_production;
            total.light_metals_mining_production += prod.light_metals_mining_production;
            total.heavy_metals_mining_production += prod.heavy_metals_mining_production;
            total.rare_elements_mining_production += prod.rare_elements_mining_production;
            total.super_elements_mining_production += prod.super_elements_mining_production;
        }
        total
    }
}

/* COLONY BUILDING BUILDER NO LONGER IN USE
#[derive(Clone, Default)]
pub struct BuildingBuilder {
    name: String,
    max_health: u32,
    fuel_cost: f64,
    ic_cost: f64,
    light_metals_cost: f64,
    heavy_metals_cost: f64,
    rare_elements_cost: f64,
    super_elements_cost: f64,
    ic_run_cost: f64,
    fuel_run_cost: f64,
    light_metals_run_cost: f64,
    heavy_metals_run_cost: f64,
    rare_elements_run_cost: f64,
    super_elements_run_cost: f64,
    manpower_run_cost: f64,
    ic_production: f64,
    fuel_production: f64,
    light_metals_production: f64,
    heavy_metals_production: f64,
    rare_elements_production: f64,
    super_elements_production: f64,
    mine_production: f64,
    science_production: f64,
    wealth_production: f64,
    shipway_size: u32,
}

impl BuildingBuilder {


    pub fn new() -> Self {
        Self::default()
    }

    pub fn name(mut self, name: &str) -> BuildingBuilder {
        self.name = name.to_string();
        self
    }

    pub fn max_health(mut self, max_health: u32) -> BuildingBuilder {
        self.max_health = max_health;
        self
    }

    pub fn fuel_cost(mut self, fuel_cost: f64) -> BuildingBuilder {
        self.fuel_cost = fuel_cost;
        self
    }

    pub fn ic_cost(mut self, ic_cost: f64) -> BuildingBuilder {
        self.ic_cost = ic_cost;
        self
    }

    pub fn ic_production(mut self, ic_production: f64) -> BuildingBuilder {
        self.ic_production = ic_production;
        self
    }

    pub fn light_metals_cost(mut self, light_metals_cost: f64) -> BuildingBuilder {
        self.light_metals_cost = light_metals_cost;
        self
    }

    pub fn heavy_metals_cost(mut self, heavy_metals_cost: f64) -> BuildingBuilder {
        self.heavy_metals_cost = heavy_metals_cost;
        self
    }

    pub fn rare_elements_cost(mut self, rare_elements_cost: f64) -> BuildingBuilder {
        self.rare_elements_cost = rare_elements_cost;
        self
    }

    pub fn super_elements_cost(mut self, super_elements_cost: f64) -> BuildingBuilder {
        self.super_elements_cost = super_elements_cost;
        self
    }

    pub fn ic_run_cost(mut self, ic_run_cost: f64) -> BuildingBuilder {
        self.ic_run_cost = ic_run_cost;
        self
    }

    pub fn fuel_run_cost(mut self, fuel_run_cost: f64) -> BuildingBuilder {
        self.fuel_run_cost = fuel_run_cost;
        self
    }

    pub fn light_metals_run_cost(mut self, light_metals_run_cost: f64) -> BuildingBuilder {
        self.light_metals_run_cost = light_metals_run_cost;
        self
    }

    pub fn heavy_metals_run_cost(mut self, heavy_metals_run_cost: f64) -> BuildingBuilder {
        self.heavy_metals_run_cost = heavy_metals_run_cost;
        self
    }

    pub fn rare_elements_run_cost(mut self, rare_elements_run_cost: f64) -> BuildingBuilder {
        self.rare_elements_run_cost = rare_elements_run_cost;
        self
    }

    pub fn super_elements_run_cost(mut self, super_elements_run_cost: f64) -> BuildingBuilder {
        self.super_elements_run_cost = super_elements_run_cost;
        self
    }

    pub fn manpower_run_cost(mut self, manpower_run_cost: f64) -> BuildingBuilder {
        self.manpower_run_cost = manpower_run_cost;
        self
    }

    pub fn fuel_production(mut self, fuel_production: f64) -> BuildingBuilder {
        self.fuel_production = fuel_production;
        self
    }

    pub fn light_metals_production(mut self, light_metals_production: f64) -> BuildingBuilder {
        self.light_metals_production = light_metals_production;
        self
    }

    pub fn heavy_metals_production(mut self, heavy_metals_production: f64) -> BuildingBuilder {
        self.heavy_metals_production = heavy_metals_production;
        self
    }

    pub fn rare_elements_production(mut self, rare_elements_production: f64) -> BuildingBuilder {
        self.rare_elements_production = rare_elements_production;
        self
    }

    pub fn super_elements_production(mut self, super_elements_production: f64) -> BuildingBuilder {
        self.super_elements_production = super_elements_production;
        self
    }

    pub fn mine_production(mut self, mine_production: f64) -> BuildingBuilder {
        self.mine_production = mine_production;
        self
    }

    pub fn science_production(mut self, science_production: f64) -> BuildingBuilder {
        self.science_production = science_production;
        self
    }

    pub fn wealth_production(mut self, wealth_production: f64) -> BuildingBuilder {
        self.wealth_production = wealth_production;
        self
    }

    pub fn shipway_size(mut self, shipway_size: u32) -> BuildingBuilder {
        self.shipway_size = shipway_size;
        self
    }

    pub fn build(self) -> ColonyBuilding {
        ColonyBuilding {
            nickname: self.name.clone(),
            current_health: self.max_health,
            template: ColonyBuildingTemplate {
                name: self.name,
                max_health: self.max_health,
                fuel_cost: self.fuel_cost,
                ic_cost: self.ic_cost,
                ic_production: self.ic_production,
                light_metals_cost: self.light_metals_cost,
                heavy_metals_cost: self.heavy_metals_cost,
                rare_elements_cost: self.rare_elements_cost,
                super_elements_cost: self.super_elements_cost,
                ic_run_cost: self.ic_run_cost,
                fuel_run_cost: self.fuel_run_cost,
                light_metals_run_cost: self.light_metals_run_cost,
                heavy_metals_run_cost: self.heavy_metals_run_cost,
                rare_elements_run_cost: self.rare_elements_run_cost,
                super_elements_run_cost: self.super_elements_run_cost,
                manpower_run_cost: self.manpower_run_cost,
                fuel_production: self.fuel_production,
                light_metals_production: self.light_metals_production,
                heavy_metals_production: self.heavy_metals_production,
                rare_elements_production: self.rare_elements_production,
                super_elements_production: self.super_elements_production,
                mine_production: self.mine_production,
                science_production: self.science_production,
                wealth_production: self.wealth_production,
                is_slipway: false,
            },
            last_production: ColonyBuildingProduction::default(),
            progress: 0.0,
            amount_built: 0,
            queue_amount: 1,
            paused: false,
            infinite: false,
            build_priority: 0,
        }
    }
}

*/

#[derive(Clone, Serialize, Deserialize)]
pub struct SlipWay {
    pub id: String,
    pub name: String,
    pub size: u32,
    pub ship_building: Option<ShipBuilding>,
    pub expanding: bool,
    pub fleet_id: String,
}

impl SlipWay {
    pub fn new(star_id: String) -> Self {
        let mut new_fleet = Fleet::default();
        new_fleet.slipway_fleet = true;
        let fleet_id = new_fleet.id.clone();

        crate::channels::channels::add_star_action(star_id, crate::channels::channels::StarAction::AddFleet(new_fleet));

        Self {
            id: uuid::Uuid::new_v4().to_string(),
            name: "Slipway".to_string(),
            size: 0,
            ship_building: None,
            expanding: false,
            fleet_id: fleet_id
        }
    }

    pub fn is_available(&self) -> bool {
        self.size > 0 && self.ship_building.is_none()
    }

    pub fn assign_ship_building(&mut self, design_id: String) {
        if !self.is_available() {
            return; // silently ignore if already building or size == 0
        }
        self.ship_building = Some(ShipBuilding::new(design_id));
    }

    pub fn update(&mut self, colony: &mut Colony, dt: f64) {
        if let Some(ship_building) = &mut self.ship_building {
            if ship_building.is_paused || ship_building.queue_amount == 0 {
                return;
            }
        }
        else {
            return; // no ship building assigned, nothing to do.
        }

        //SAFETY: We can unwrap here because if ship_building is None, we would have returned already.
        let ship_building = self.ship_building.as_mut().unwrap();
        let resource_cost = ship_building.get_resource_cost();
        let bp = resource_cost.total_bp() + ship_building.ic_cost;

        let queue_amount = if ship_building.is_infinite {
            u32::MAX
        } else {
            ship_building.queue_amount
        };

        let possible_build = (colony.resources.ic / bp)
            .min(colony.resources.light_elements / resource_cost.fuel)
            .min(colony.resources.light_metals / resource_cost.light_metals)
            .min(colony.resources.heavy_metals / resource_cost.heavy_metals)
            .min(colony.resources.rare_elements / resource_cost.rare_elements)
            .min(colony.resources.super_elements / resource_cost.super_elements)
            .min(queue_amount as f64);

        if possible_build == 0.0 {
            return;
        }

        let fraction: f64 = possible_build - possible_build.floor();
        let mut whole_builds = possible_build.floor() as u32;

        ship_building.progress += fraction;
        if ship_building.progress >= 1.0 {
            ship_building.progress -= 1.0;
            whole_builds += 1;
        }

        ship_building.build_amount += whole_builds;

        colony.resources.ic -= possible_build * ship_building.ic_cost;
        colony.resources.light_elements -= possible_build * resource_cost.fuel;
        colony.resources.light_metals -= possible_build * resource_cost.light_metals;
        colony.resources.heavy_metals -= possible_build * resource_cost.heavy_metals;
        colony.resources.rare_elements -= possible_build * resource_cost.rare_elements;
        colony.resources.super_elements -= possible_build * resource_cost.super_elements;

        ship_building.queue_amount = ship_building.queue_amount.saturating_sub(whole_builds);

        if whole_builds == 0 {
            return;
        }

        let new_ships = [0..whole_builds].iter().map(|_| Ship::new(ship_building.ship_design.clone())).collect::<Vec<Ship>>();
        let fleet_id = self.fleet_id.clone();
        insert_fleet_order(FleetOrder{
            fleet_id: fleet_id.clone(),
            order: FleetOrderType::AddMembers(new_ships),
            ..Default::default()
        });
    }

    pub fn increase_queue(&mut self) {
        if let Some(ship_building) = &mut self.ship_building {
            if !ship_building.is_infinite {
                ship_building.queue_amount += 1;
            }
        }
    }

    pub fn decrease_queue(&mut self) {
        if let Some(ship_building) = &mut self.ship_building {
            if ship_building.queue_amount > 0 {
                ship_building.queue_amount -= 1;
            }
        }
    }

    pub fn toggle_infinite(&mut self) {
        if let Some(ship_building) = &mut self.ship_building {
            ship_building.is_infinite = !ship_building.is_infinite;
        } else {
            panic!("No ship building assigned to this slipway.");
        }
    }
}


#[derive(Clone, Serialize, Deserialize)]
pub struct ColonyBuilding {
    pub template: ColonyBuildingTemplate,
    pub progress: f64,
    pub current_health: u32,
    pub amount_built: u32,
    pub queue_amount: u32,
    pub paused: bool,
    pub infinite: bool,
    pub build_priority: u32,

    pub last_production: ColonyBuildingProduction,

    //slipway extension logic:
    pub for_slipway_with_id: Option<String>, //if this building is an extension for a slipway, the id of that slipway.
}

#[derive(Clone, Serialize, Deserialize)]
pub struct ColonyBuildingTemplate {
    pub id: String,
    pub name: String,
    pub max_health: u32,

    //Build costs:
    pub fuel_cost: f64,
    pub ic_cost: f64,
    pub light_metals_cost: f64,
    pub heavy_metals_cost: f64,
    pub rare_elements_cost: f64,
    pub super_elements_cost: f64,

    //Run costs:
    pub ic_run_cost: f64,
    pub fuel_run_cost: f64,
    pub light_metals_run_cost: f64,
    pub heavy_metals_run_cost: f64,
    pub rare_elements_run_cost: f64,
    pub super_elements_run_cost: f64,
    pub manpower_run_cost: f64,

    //Production:
    pub ic_production: f64,
    pub fuel_production: f64,
    pub light_metals_production: f64,
    pub heavy_metals_production: f64,
    pub rare_elements_production: f64,
    pub super_elements_production: f64,
    pub wealth_production: f64,

    //Mining:
    pub mine_production: f64,

    //Science:
    pub science_production: f64,
}

impl ColonyBuilding {
    pub fn new(template: ColonyBuildingTemplate) -> Self {
        Self {
            current_health: template.max_health,
            template,
            progress: 0.0,
            amount_built: 0,
            queue_amount: 1,
            paused: false,
            infinite: false,
            build_priority: 0,

            last_production: ColonyBuildingProduction::default(),
            for_slipway_with_id: None,
        }
    }

    pub fn new_for_slipway(template: ColonyBuildingTemplate, slipway_id: String) -> Self {
        let mut building = Self::new(template);
        building.for_slipway_with_id = Some(slipway_id);
        building
    }

    pub fn is_building(&self) -> bool {
        self.queue_amount > 0
    }

    pub fn get_name(&self) -> String {
        self.template.name.clone()
    }

    pub fn get_max_health(&self) -> u32 {
        self.template.max_health
    }

    pub fn get_progress(&self) -> f64 {
        self.progress
    }

    pub fn get_time_to_complete(&self, ic_per_sec: f64) -> f64 {
        ((self.queue_amount as f64) * self.template.ic_cost as f64) / ic_per_sec
            - self.progress * self.template.ic_cost as f64 / ic_per_sec
    }

    pub fn time_until_next_completion(&self, ic_per_sec: f64) -> f64 {
        let mut time = 0.0;
        if !self.is_building() {
            time = 0.0;
            return time;
        } else {
            time = (1.0 - self.progress) * self.template.ic_cost as f64 / ic_per_sec;
        }   

        time = time * 60.0 * 60.0 * 24.0;
        time
    }

    pub fn is_infinite(&self) -> bool {
        self.infinite
    }

    pub fn is_paused(&self) -> bool {
        self.paused
    }

    pub fn get_queue_amount(&self) -> u32 {
        self.queue_amount
    }

    pub fn get_built_amount(&self) -> u32 {
        self.amount_built
    }

    pub fn get_ic_cost(&self) -> f64 {
        self.template.ic_cost
    }

    pub fn build(&mut self, colony: &mut Colony, dt: f64) {
        if self.is_paused() || !self.is_building() {
            return;
        }

        let queue_amount = if self.infinite {
            u32::MAX
        } else {
            self.queue_amount
        };

        if queue_amount == 0 {
            return;
        }

        //how many can we build in this time step.
        let possible_build = (colony.resources.ic / self.template.ic_cost)
            .min(colony.resources.light_elements / self.template.fuel_cost)
            .min(colony.resources.light_metals / self.template.light_metals_cost)
            .min(colony.resources.heavy_metals / self.template.heavy_metals_cost)
            .min(colony.resources.rare_elements / self.template.rare_elements_cost)
            .min(colony.resources.super_elements / self.template.super_elements_cost)
            .min(queue_amount as f64);

        if possible_build == 0.0 {
            return;
        }

        let fraction: f64 = possible_build - possible_build.floor();
        let mut whole_builds = possible_build.floor() as u32;

        self.progress += fraction;
        if self.progress >= 1.0 {
            self.progress -= 1.0;
            whole_builds += 1;
        }
        self.amount_built += whole_builds;

        colony.resources.ic -= possible_build * self.template.ic_cost;
        colony.resources.light_elements -= possible_build * self.template.fuel_cost;
        colony.resources.light_metals -= possible_build * self.template.light_metals_cost;
        colony.resources.heavy_metals -= possible_build * self.template.heavy_metals_cost;
        colony.resources.rare_elements -= possible_build * self.template.rare_elements_cost;
        colony.resources.super_elements -= possible_build * self.template.super_elements_cost;

        self.queue_amount = self.queue_amount.saturating_sub(whole_builds);

        if whole_builds > 0 && self.template.id == "build_slipway" {
            colony.build_new_slip_way();
        }

        if whole_builds > 0 && self.template.id == "extend_slipway" {
            let slipway_id = self.for_slipway_with_id.clone().expect("extend_slipway building must have for_slipway_with_id set.");
            if let Some(slipway) = colony.slipways.iter_mut().find(|s| s.id == slipway_id) {
                slipway.size += 1000;
            } else {
                panic!("Slipway with id {} not found for extend_slipway building.", slipway_id);
            }
        }
    }

    pub fn matches_name(&self, name: &str) -> bool {
        self.template.name == name
    }

    pub fn queue_one(&mut self, infinite: bool) {
        self.queue_amount += 1;
        self.infinite |= infinite;
    }

    pub fn set_infinite(&mut self, infinite: bool) {
        self.infinite = infinite;
    }

    pub fn is_empty(&self) -> bool {
        self.queue_amount == 0 && self.amount_built == 0
    }

    pub fn decrease_queue(&mut self) {
        if self.queue_amount > 0 {
            self.queue_amount -= 1;
        }

        if self.queue_amount == 0 {
            self.infinite = false;
            self.paused = false;
        }
    }

    pub fn toggle_infinite(&mut self) {
        self.infinite = !self.infinite;
    }

    pub fn toggle_paused(&mut self) {
        self.paused = !self.paused;
    }

    pub fn demolish_one(&mut self) {
        self.amount_built = self.amount_built.saturating_sub(1);
    }

    pub fn get_ic_production(&self) -> f64 {
        return self.template.ic_production * (self.amount_built as f64);
    }

    pub fn get_build_priority(&self) -> u32 {
        return self.build_priority;
    }

    pub fn from_name(name: &str) -> ColonyBuilding {
        let template = BUILDING_TEMPLATES
            .iter()
            .find(|t| t.name == name)
            .unwrap_or_else(|| panic!("Unknown building name: {}", name))
            .clone();
        ColonyBuilding::new(template)
    }

    pub fn affect(&mut self, colony:&mut Colony, body: &mut Body, dt: f64) {

        let production = self.get_production(body, dt);

        colony.resources.light_elements += production.fuel_production * dt;
        colony.resources.light_metals += production.light_metals_production * dt;
        colony.resources.heavy_metals += production.heavy_metals_production * dt;
        colony.resources.rare_elements += production.rare_elements_production * dt;
        colony.resources.super_elements += production.super_elements_production * dt;

        
        colony.resources.light_elements += production.fuel_mining_production * dt;
        colony.resources.light_metals += production.light_metals_mining_production * dt;
        colony.resources.heavy_metals += production.heavy_metals_mining_production * dt;
        colony.resources.rare_elements += production.rare_elements_mining_production * dt;
        colony.resources.super_elements += production.super_elements_mining_production * dt;
        self.last_production = production;

        //wealth:
        //TODO: Add generated wealth to player status.

        //Research:
        //TODO: Add generated research to player status.
    }

    fn get_production(&self, body: &mut Body, dt: f64) -> ColonyBuildingProduction {
        let mut production = ColonyBuildingProduction {
            fuel_production: self.template.fuel_production * (self.amount_built as f64),
            light_metals_production: self.template.light_metals_production * (self.amount_built as f64),
            heavy_metals_production: self.template.heavy_metals_production * (self.amount_built as f64),
            rare_elements_production: self.template.rare_elements_production * (self.amount_built as f64),
            super_elements_production: self.template.super_elements_production * (self.amount_built as f64),
            science_production: self.template.science_production * (self.amount_built as f64),
            wealth_production: self.template.wealth_production * (self.amount_built as f64),
            ..Default::default()
        };

        //mining:
        let mine_production = self.template.mine_production * (self.amount_built as f64);
        let mined = body.mine_resources(mine_production, dt);

        production.fuel_mining_production += mined.fuel;
        production.light_metals_mining_production += mined.light_metals;
        production.heavy_metals_mining_production += mined.heavy_metals;
        production.rare_elements_mining_production += mined.rare_elements;
        production.super_elements_mining_production += mined.super_elements;

        //Research:
        //TODO: Add generated research to player status.

        production
    }

    pub fn get_all_building_options() -> Vec<ColonyBuilding> {
        BUILDING_TEMPLATES
        .iter()
        .filter(|t| t.id != "extend_slipway" )
        .map(|template| ColonyBuilding::new(template.clone())).collect()
    }
}
