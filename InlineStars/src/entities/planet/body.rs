use rayon::prelude::*;
use serde::{Deserialize, Serialize};

use crate::entities::{GameEntity, orbit::Orbit};

use super::Colony;



#[derive(Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum BodyType {
    Star,
    TerrestrialPlanet,
    IceGiant,
    GasGiant,
    RockyMoon,
    IcyMoon,
    CTypeAsteroid,
    STypeAsteroid,
    MTypeAsteroid,
    Comet,
    Fleet,
}

impl BodyType {
    pub fn display_name(&self) -> &'static str {
        match self {
            BodyType::Star => "Star",
            BodyType::TerrestrialPlanet => "Terrestrial Planet",
            BodyType::IceGiant => "Ice Giant",
            BodyType::GasGiant => "Gas Giant",
            BodyType::RockyMoon => "Rocky Moon",
            BodyType::IcyMoon => "Icy Moon",
            BodyType::CTypeAsteroid => "C-type Asteroid",
            BodyType::STypeAsteroid => "S-type Asteroid",
            BodyType::MTypeAsteroid => "M-type Asteroid",
            BodyType::Comet => "Comet",
            BodyType::Fleet => "Fleet",
        }
    }

    pub fn is_asteroid(&self) -> bool {
        matches!(
            self,
            BodyType::CTypeAsteroid | BodyType::STypeAsteroid | BodyType::MTypeAsteroid
        )
    }

    pub fn is_major_orbital(&self) -> bool {
        matches!(
            self,
            BodyType::TerrestrialPlanet | BodyType::GasGiant | BodyType::IceGiant
        )
    }

    pub fn surface_layer_name(&self) -> &'static str {
        match self {
            BodyType::TerrestrialPlanet => "Crust",
            BodyType::GasGiant => "Atmosphere",
            BodyType::IceGiant => "Mantle",
            _ => "Surface",
        }
    }

    pub fn mantle_layer_name(&self) -> Option<&'static str> {
        match self {
            BodyType::TerrestrialPlanet => Some("Mantle"),
            _ => None,
        }
    }

    pub fn core_layer_name(&self) -> Option<&'static str> {
        match self {
            BodyType::TerrestrialPlanet | BodyType::GasGiant | BodyType::IceGiant => Some("Core"),
            _ => None,
        }
    }
}

#[derive(Clone, Copy, Default, Serialize, Deserialize)]
pub struct ResourceDeposit {
    pub amount: f64,
    pub extraction_difficulty: f64,
}

#[derive(Clone, Copy, Default)]
pub struct ResourceAmounts {
    pub fuel: f64,
    pub light_metals: f64,
    pub heavy_metals: f64,
    pub rare_elements: f64,
    pub super_elements: f64,
}

impl ResourceAmounts {
    pub fn add_assign(&mut self, other: ResourceAmounts) {
        self.fuel += other.fuel;
        self.light_metals += other.light_metals;
        self.heavy_metals += other.heavy_metals;
        self.rare_elements += other.rare_elements;
        self.super_elements += other.super_elements;
    }
}


//Natural resources available on a body for extraction.
#[derive(Clone, Default, Serialize, Deserialize)]
pub struct BodyResources{
    pub fuel: ResourceDeposit,
    pub light_metals: ResourceDeposit,
    pub heavy_metals: ResourceDeposit,
    pub rare_elements: ResourceDeposit,
    pub super_elements: ResourceDeposit,
}

impl BodyResources{
    pub fn new(
        fuel: ResourceDeposit,
        light_metals: ResourceDeposit,
        heavy_metals: ResourceDeposit,
        rare_elements: ResourceDeposit,
        super_elements: ResourceDeposit,
    ) -> Self {
        Self {
            fuel,
            light_metals,
            heavy_metals,
            rare_elements,
            super_elements,
        }
    }

    pub fn amounts(&self) -> ResourceAmounts {
        ResourceAmounts {
            fuel: self.fuel.amount,
            light_metals: self.light_metals.amount,
            heavy_metals: self.heavy_metals.amount,
            rare_elements: self.rare_elements.amount,
            super_elements: self.super_elements.amount,
        }
    }
}

#[derive(Clone, Serialize, Deserialize)]
pub struct Body {
    pub id: String,
    pub star_id: String,
    pub name: String,
    pub mass_kg: f64,
    pub radius_km: f64,
    pub orbit: Option<Orbit>,
    pub parent_x: f64,
    pub parent_y: f64,
    pub moons: Vec<Body>,
    pub colony: Option<Colony>,
    pub body_type: BodyType,
    pub surface_resources: BodyResources,
    pub mantle_resources: BodyResources,
    pub core_resources: BodyResources,
}

impl Body {
    pub fn new(
        name: String,
        star_id: String,
        parent_x: f64,
        parent_y: f64,
        body_type: BodyType,
        mass_kg: f64,
        radius_km: f64,
        surface_resources: BodyResources,
        mantle_resources: BodyResources,
        core_resources: BodyResources,
    ) -> Self {
        Self {
            id: uuid::Uuid::new_v4().to_string(),
            star_id,
            name,
            mass_kg,
            radius_km,
            parent_x,
            parent_y,
            orbit: None,
            moons: Vec::new(),
            colony: None,
            body_type,
            surface_resources,
            mantle_resources,
            core_resources,
        }
    }

    pub fn total_resource_amounts(&self) -> ResourceAmounts {
        let mut total = self.surface_resources.amounts();
        total.add_assign(self.mantle_resources.amounts());
        total.add_assign(self.core_resources.amounts());
        total
    }

    pub fn mine_resources(&mut self, mine_production: f64, dt: f64) -> ResourceAmounts {
        if mine_production <= 0.0 || dt <= 0.0 {
            return ResourceAmounts::default();
        }

        ResourceAmounts {
            fuel: extract_resource_in_order(
                &mut self.surface_resources.fuel,
                &mut self.mantle_resources.fuel,
                &mut self.core_resources.fuel,
                mine_production,
                dt,
            ),
            light_metals: extract_resource_in_order(
                &mut self.surface_resources.light_metals,
                &mut self.mantle_resources.light_metals,
                &mut self.core_resources.light_metals,
                mine_production,
                dt,
            ),
            heavy_metals: extract_resource_in_order(
                &mut self.surface_resources.heavy_metals,
                &mut self.mantle_resources.heavy_metals,
                &mut self.core_resources.heavy_metals,
                mine_production,
                dt,
            ),
            rare_elements: extract_resource_in_order(
                &mut self.surface_resources.rare_elements,
                &mut self.mantle_resources.rare_elements,
                &mut self.core_resources.rare_elements,
                mine_production,
                dt,
            ),
            super_elements: extract_resource_in_order(
                &mut self.surface_resources.super_elements,
                &mut self.mantle_resources.super_elements,
                &mut self.core_resources.super_elements,
                mine_production,
                dt,
            ),
        }
    }
}

fn extract_resource_in_order(
    surface: &mut ResourceDeposit,
    mantle: &mut ResourceDeposit,
    core: &mut ResourceDeposit,
    mine_production: f64,
    dt: f64,
) -> f64 {
    for layer in [surface, mantle, core] {
        if layer.amount <= f64::EPSILON {
            continue;
        }

        let difficulty = layer.extraction_difficulty.max(1.0);
        let per_second = mine_production * layer.amount / difficulty;
        let extracted = (per_second * dt).min(layer.amount);
        layer.amount = (layer.amount - extracted).max(0.0);

        return extracted / dt;
    }

    0.0
}

impl GameEntity<Body> for Body {
    fn update(&mut self, delta_time: f64) {
        if let Some(orbit) = &mut self.orbit {
            orbit.update_orbit(delta_time);
        }

        let global_pos = self.get_global_position();
        self.moons.par_iter_mut().for_each(|moon| {
            moon.parent_x = global_pos.0;
            moon.parent_y = global_pos.1;
            moon.update(delta_time);
        });

        if let Some(mut colony) = self.colony.take() {
            colony.update(self, delta_time);
            self.colony = Some(colony);
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
        self.name.clone()
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
