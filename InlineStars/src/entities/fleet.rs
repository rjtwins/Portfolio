use crate::entities::fleet;
use crate::channels::channels::{add_star_action, insert_fleet_order, StarAction};
use crate::entities::ship::SubsystemType;
use crate::{distance, get_body_by_id, get_fleet_by_id, get_star_id_for_body, get_star_id_for_fleet};
use crate::entities::{GameEntity, ship::Ship};
use crate::entities::planet::colony::{Colony, ColonyResources};
use serde::{Deserialize, Serialize};
use serde_json;
use std::fs;
use std::path::Path;

#[derive(Clone, Serialize, Deserialize)]
pub struct Fleet {
    pub id: String,

    pub name: String,

    pub x: f64,
    pub y: f64,

    pub target_position: Option<(f64, f64)>,
    pub target_object: Option<String>,

    pub order_queue: Vec<FleetOrder>,

    pub members: Vec<Ship>,

    pub slipway_fleet: bool,
}

impl GameEntity<Fleet> for Fleet {
    fn update(&mut self, dt: f64) {
        //Retrieve the latest order for this fleet
        let fleet_order = crate::channels::channels::get_fleet_order_by_id(self.id.clone());
        if let Some(orders) = fleet_order {
            for order in orders {
                self.consume_fleet_order(order);
            }
        }

        self.process_fleet_order();
        
        if self.target_position.is_none(){
            if let Some(ref target_uuid) = self.target_object {
                if let Some(fleet) = get_fleet_by_id(target_uuid.clone()) {
                    self.target_position = Some(fleet.get_global_position());
                } else if let Some(body) = get_body_by_id(target_uuid.clone()) {
                    self.target_position = Some(body.get_global_position());
                }
            }else{
                self.target_position = Some(self.get_global_position());
            }
        }

        let target_pos = self.target_position.expect("We must always have a target position even if we are idle.");
        
        let mut speed = self.members.iter().map(|m| m.get_speed()).min_by(|a, b| a.partial_cmp(b).unwrap_or(std::cmp::Ordering::Equal)).unwrap_or(0.0);
        speed *= dt;
        
        let distance = distance(target_pos, self.get_global_position());

        if distance < speed {
            self.x = target_pos.0;
            self.y = target_pos.1;
            self.target_position = None;
        } else {
            let direction_x = (target_pos.0 - self.x) / distance;
            let direction_y = (target_pos.1 - self.y) / distance;

            self.x += direction_x * speed;
            self.y += direction_y * speed;
        }
    }

    fn get_global_position(&self) -> (f64, f64) {
        (self.x, self.y)
    }
    
    fn get_name(&self) -> String {
        self.name.clone()
    }
    
    fn get_id(&self) -> String {
        self.id.clone()
    }
    
    fn get_orbit(&self) -> Option<super::orbit::Orbit> {
        None
    }
    
    fn get_parent_position(&self) -> (f64, f64) {
        (0.0, 0.0)
    }
}

impl Default for Fleet {
    fn default() -> Self {
        Self {name: String::new(), id: uuid::Uuid::new_v4().to_string(), x: Default::default(), y: Default::default(), target_position: None, target_object: None, order_queue: vec![], members: vec![], slipway_fleet: false}
    }
}

impl Fleet {
    pub fn add_member(&mut self, ship: Ship) {
        self.members.push(ship);

        //#[cfg(debug_assertions)]
        //self.save_snapshot();
    }

    pub fn remove_member(&mut self, ship: Ship) {
        self.members.retain(|s| s.id != ship.id);
    }

    pub fn set_target_position(&mut self, x: f64, y: f64) {
        self.target_position = Some((x, y));
        self.target_object = None;
    }

    pub fn set_target_object(&mut self, target_id: String) {
        self.target_object = Some(target_id);
        self.target_position = None;
    }

    fn consume_fleet_order(&mut self, order: FleetOrder) {
        let order_type = order.order.clone();

        match order_type {
            FleetOrderType::AddMembers(members) => {
                for member in members {
                    self.add_member(member);
                }
                return;
            }
            FleetOrderType::RemoveMembers(members) => {
                for member_id in members {
                    self.members.retain(|s| s.id != member_id);
                }
                return;
            }
            FleetOrderType::RemoveOrder(index) => {
                if index < self.order_queue.len() {
                    self.order_queue.remove(index);
                }
                return;
            }
            _ => {}
        }

        match order.add_type {
            OrderAddType::Enqueue => {
                self.order_queue.push(order);
            }
            OrderAddType::InFront => {
                self.order_queue.insert(0, order);
            }
            OrderAddType::Replace => {
                self.order_queue.clear();
                self.order_queue.push(order);
            }
            OrderAddType::Insert(index) => {
                let insert_pos = index.min(self.order_queue.len());
                self.order_queue.insert(insert_pos, order);
            }
        }
    }

    fn process_fleet_order(&mut self) {
        let default = FleetOrder::default();
        let current = self.order_queue.first().unwrap_or(&default);

        match current.order.clone() {
            FleetOrderType::MoveToPosition(pos) => {
                if get_distance_between(pos, self.get_global_position()) < 100.0 {
                    self.order_queue.remove(0);
                    return;
                }

                self.target_position = Some(pos);
                self.target_object = None;

            }
            FleetOrderType::KeepDistanceToObject(object_id, distance) => {
                self.target_object = Some(object_id.clone());
                if let Some(body) = get_body_by_id(object_id.clone()) {
                    let target_pos = body.get_global_position();
                    let current_pos = self.get_global_position();
                    let current_distance = get_distance_between(target_pos, current_pos);
                    
                    if current_distance < distance {
                        let direction_x = (current_pos.0 - target_pos.0) / current_distance;
                        let direction_y = (current_pos.1 - target_pos.1) / current_distance;
                        self.target_position = Some((target_pos.0 + direction_x * distance, target_pos.1 + direction_y * distance));
                    } else {
                        self.target_position = Some(target_pos);
                    }
                } else if let Some(fleet) = get_fleet_by_id(object_id.clone()) {
                    let target_pos = fleet.get_global_position();
                    let current_pos = self.get_global_position();
                    let current_distance = get_distance_between(target_pos, current_pos);
                    
                    if current_distance < distance {
                        let direction_x = (current_pos.0 - target_pos.0) / current_distance;
                        let direction_y = (current_pos.1 - target_pos.1) / current_distance;
                        self.target_position = Some((target_pos.0 + direction_x * distance, target_pos.1 + direction_y * distance));
                    } else {
                        self.target_position = Some(target_pos);
                    }
                } else {
                    //Target does not exist (anymore).
                    //SAFETY: There MUST be a order 0 in the queue, because we are processing the first order in the queue. If there is no order, we will process a default order which is idle and will not enter this branch.
                    self.order_queue.remove(0);
                }
            }
            FleetOrderType::MoveToObject(obj) => {
                self.target_object = Some(obj);
                self.target_position = None;
            }
            FleetOrderType::Split(ship_ids) => {
                let ships_to_split: Vec<Ship> = self
                    .members
                    .iter()
                    .filter(|s| ship_ids.contains(&s.id))
                    .cloned()
                    .collect();

                if !ships_to_split.is_empty() {
                    self.members.retain(|s| !ship_ids.contains(&s.id));

                    let mut new_fleet = Fleet::default();
                    new_fleet.x = self.x;
                    new_fleet.y = self.y;
                    new_fleet.members = ships_to_split;

                    if let Some(star_id) = get_star_id_for_fleet(&self.id) {
                        add_star_action(star_id.clone(), StarAction::AddFleet(new_fleet));

                        if self.members.is_empty() && !self.slipway_fleet {
                            add_star_action(star_id, StarAction::RemoveFleet(self.id.clone()));
                        }
                    }
                }

                self.order_queue.remove(0);
            }
            FleetOrderType::Join(target_fleet_id) => {
                if let Some(target_fleet) = get_fleet_by_id(target_fleet_id.clone()) {
                    let distance = get_distance_between(target_fleet.get_global_position(), self.get_global_position());
                    if distance > 100.0 {
                        self.target_object = Some(target_fleet_id.clone());
                        self.target_position = None;
                        return;
                    }
                }
                let members = std::mem::take(&mut self.members);
                if !members.is_empty() {
                    insert_fleet_order(FleetOrder {
                        fleet_id: target_fleet_id.clone(),
                        add_type: OrderAddType::Enqueue,
                        order: FleetOrderType::AddMembers(members),
                    });
                }

                if !self.slipway_fleet {
                    if let Some(star_id) = get_star_id_for_fleet(&self.id) {
                        add_star_action(star_id, StarAction::RemoveFleet(self.id.clone()));
                    }
                }

                self.order_queue.remove(0);
            }
            FleetOrderType::Colonize(body_id) => {
                let Some(body) = get_body_by_id(body_id.clone()) else {
                    self.order_queue.remove(0);
                    return;
                };

                if body.colony.is_some() {
                    // Already colonized — cancel order
                    self.order_queue.remove(0);
                    return;
                }

                let target_pos = body.get_global_position();
                let dist = get_distance_between(target_pos, self.get_global_position());

                if dist > 1000.0 {
                    self.target_position = Some(target_pos);
                    self.target_object = None;
                    return;
                }

                let Some(idx) = self.members.iter().position(|s| {
                    s.design.subsystems.iter().any(|ss| ss.subsystem_type == SubsystemType::ColonyModule)
                }) else {
                    self.order_queue.remove(0);
                    return;
                };

                self.members.remove(idx);

                let colony = Colony::new(
                    body.get_name(),
                    body_id.clone(),
                    ColonyResources {
                        population: 1,
                        base_ic: 1.0,
                        ic: 0.0,
                        light_elements: 0.0,
                        light_metals: 0.0,
                        heavy_metals: 0.0,
                        rare_elements: 0.0,
                        super_elements: 0.0,
                    },
                );

                if let Some(star_id) = get_star_id_for_body(&body_id) {
                    add_star_action(star_id.clone(), StarAction::AddColonyToBody(body_id.clone(), colony));

                    if self.members.is_empty() && !self.slipway_fleet {
                        add_star_action(star_id, StarAction::RemoveFleet(self.id.clone()));
                    }
                }

                self.order_queue.remove(0);
            }
            _ => { }
        }
    }
    
    fn save_snapshot(&self){
        let json = serde_json::to_string_pretty(self).expect("Failed to serialize fleet");
        #[cfg(debug_assertions)]
        let path = Path::new("src/data/mock_fleet.json");
        #[cfg(not(debug_assertions))]
        let path = Path::new("mock_fleet.json");
        fs::write(path, json).expect("Failed to write to mock_fleet.json");
    }

    pub fn load_snapshot() -> Self{
        #[cfg(debug_assertions)]
        let path = Path::new("src/data/mock_fleet.json");
        #[cfg(not(debug_assertions))]
        let path = Path::new("mock_fleet.json");
        
        let json = fs::read_to_string(path).expect("Failed to read mock_fleet.json");
        let snapshot: Fleet = serde_json::from_str(&json).expect("Failed to deserialize fleet");
        snapshot
    }

    /// Returns the order types this fleet is currently able to receive.
    /// Variants carry placeholder data — callers should only match on the discriminant.
    pub fn available_orders(&self) -> Vec<FleetOrderType> {
        // Slipway fleets are stationary and managed by the colony — no player orders.
        if self.slipway_fleet {
            return vec![
                FleetOrderType::AddMembers(vec![]),
                FleetOrderType::RemoveMembers(vec![]),
            ];
        }

        let mut orders = vec![
            FleetOrderType::Idle,
            FleetOrderType::MoveToPosition((0.0, 0.0)),
            FleetOrderType::MoveToObject(String::new()),
            FleetOrderType::KeepDistanceToObject(String::new(), 0.0),
            FleetOrderType::Join(String::new()),
        ];
        if self.members.len() > 1 {
            orders.push(FleetOrderType::Split(vec![]));
        }

        if self.members.iter().any(|f| f.design.subsystems.iter().any(|ss| ss.subsystem_type == SubsystemType::ColonyModule)){
            orders.push(FleetOrderType::Colonize(String::new()));
        }
        orders
    }
    
    // pub fn get_sensor_range(&self) -> f64 {
    //     self.members.iter().map(|m| m.sensors.iter().map(|s| s.sensor_range).sum::<f64>()).sum()
    // }
}

fn get_distance_between(target_pos: (f64, f64), current_pos: (f64, f64)) -> f64 {
    let dx = target_pos.0 - current_pos.0;
    let dy = target_pos.1 - current_pos.1;
    (dx * dx + dy * dy).sqrt()
}

#[derive(Clone, Default, Serialize, Deserialize)]
pub enum FleetOrderType{
    MoveToPosition((f64, f64)),
    MoveToObject(String),
    KeepDistanceToObject(String, f64),
    AddMembers(Vec<Ship>),
    RemoveMembers(Vec<String>),
    Colonize(String),
    /// Remove the order at the given index from the queue. Consumed immediately, never queued.
    RemoveOrder(usize),
    /// Split the fleet: move the listed ship IDs into a new fleet at the same position. Consumed immediately, never queued.
    Split(Vec<String>),
    /// Join another fleet: move all members into the target fleet, then delete self.
    Join(String),
    #[default]
    Idle,
}

impl FleetOrderType {
    pub fn label(&self) -> &'static str {
        match self {
            FleetOrderType::MoveToPosition(_) => "Move to position",
            FleetOrderType::MoveToObject(_) => "Move to object",
            FleetOrderType::KeepDistanceToObject(_, _) => "Keep distance to object",
            FleetOrderType::AddMembers(_) => "Add members",
            FleetOrderType::RemoveMembers(_) => "Remove members",
            FleetOrderType::RemoveOrder(_) => "Remove order",
            FleetOrderType::Split(_) => "Split",
            FleetOrderType::Join(_) => "Join fleet",
            FleetOrderType::Colonize(_) => "Colonize",
            FleetOrderType::Idle => "Idle (clear orders)",
        }
    }

    /// Whether this order type requires selecting a body only (no fleets).
    pub fn needs_body_only(&self) -> bool {
        matches!(self, FleetOrderType::Colonize(_))
    }

    /// Whether this order type requires selecting a target object (body or fleet).
    pub fn needs_object(&self) -> bool {
        matches!(self, FleetOrderType::MoveToObject(_) | FleetOrderType::KeepDistanceToObject(_, _))
    }

    /// Whether this order type requires selecting a target fleet only (no bodies).
    pub fn needs_fleet_only(&self) -> bool {
        matches!(self, FleetOrderType::Join(_))
    }

    /// Whether this order type requires a distance input after object selection.
    pub fn needs_distance(&self) -> bool {
        matches!(self, FleetOrderType::KeepDistanceToObject(_, _))
    }

    /// Whether this order type requires selecting ships from the fleet.
    pub fn needs_ships(&self) -> bool {
        matches!(self, FleetOrderType::Split(_))
    }

    /// Whether this order type is placed by clicking a world position.
    pub fn needs_position(&self) -> bool {
        matches!(self, FleetOrderType::MoveToPosition(_))
    }
}

#[derive(Clone, Default, Serialize, Deserialize)]
pub struct FleetOrder{
    pub fleet_id: String,
    pub add_type: OrderAddType,
    pub order: FleetOrderType,
}

#[derive(Clone, Default, PartialEq, Eq, Serialize, Deserialize)]
pub enum OrderAddType {
    #[default]
    Enqueue,
    InFront,
    Replace,
    Insert(usize),
}