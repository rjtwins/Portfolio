use serde::{Deserialize, Serialize};

use crate::{
    app::ship_desginer::with_ship_designer,
    entities::ship::{ResourceCost, ShipDesign},
};

#[derive(Clone, Serialize, Deserialize)]
pub struct ShipBuilding {
    //pub design_id: String,
    pub ship_design: ShipDesign,
    pub acquired_resources: ResourceCost, //Resources that have been acquired towards building this ship so far, used to calculate progress
    pub ic_cost: f64,                     //IC cost of the ship being built
    pub progress: f64,                    //Progress towards building the ship, from 0.0 to 1.0
    pub build_amount: u32,                //Total amount of ships to build in this queue entry
    pub queue_amount: u32, //Amount of ships still in the queue (decreases as ships are completed)
    pub is_infinite: bool, //Whether this queue entry is infinite (never decreases queue_amount)
    pub is_paused: bool, //Whether this queue entry is paused (does not progress towards completion)
}

impl ShipBuilding {
    pub fn new(design_id: String) -> Self {
        let design =
            with_ship_designer(|sd| sd.ship_designs.iter().find(|s| s.id == design_id).cloned())
                .expect("Ship design not found for new ship building");

        //let ic_cost = design.cost.ic;
        let bp = design.cost.total_bp();

        Self {
            ship_design: design,
            acquired_resources: ResourceCost::default(),
            ic_cost: bp,
            progress: 0.0,
            build_amount: 0,
            queue_amount: 0,
            is_infinite: false,
            is_paused: false,
        }
    }

    pub fn get_resource_cost(&self) -> ResourceCost {
        self.ship_design.get_total_cost().clone()
    }
}
