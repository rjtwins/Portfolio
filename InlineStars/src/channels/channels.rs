/// Handles all channels for global state. This is a bit of an anti-pattern, but it allows us to avoid passing around a lot of state and makes it easier to access global state from anywhere in the code.
/// This also insures that it doesn't matter in which order the entities are initialized, as they can all access the channels without worrying about whether the other entity has been initialized yet.
/// It also allows us to easily add new channels in the future without having to change a lot of code.
use std::{collections::HashMap, sync::{Arc, LazyLock, Mutex, OnceLock, RwLock, atomic::AtomicBool}};
use tokio::sync::watch;
use crate::{app::InputState, entities::{camera::Camera, fleet::{Fleet, FleetOrder}, player_state::PlayerState, ship::{Ship, ShipDesign}, star_map::StarMap}, input_handle::input_handle::ColonyAction};

#[derive(Clone)]
pub enum StarAction {
    AddFleet(Fleet),
    UpdateFleet(Fleet),
    AddShipToFleet(String, Ship), //Fleet id, ship design id
    RemoveShipFromFleet(String, String), //Fleet id, ship design id
    RemoveFleet(String), // fleet_id
    AddColonyToBody(String, crate::entities::planet::Colony), // body_id, colony
}

static STAR_MAP_RX: OnceLock<tokio::sync::watch::Receiver<Arc<StarMap>>> = OnceLock::new();
static STAR_MAP_TX: OnceLock<tokio::sync::watch::Sender<Arc<StarMap>>> = OnceLock::new();

static CAMERA_RX: OnceLock<tokio::sync::watch::Receiver<Arc<Camera>>> = OnceLock::new();
static CAMERA_TX: OnceLock<tokio::sync::watch::Sender<Arc<Camera>>> = OnceLock::new();

static INPUT_RX: OnceLock<tokio::sync::watch::Receiver<Arc<InputState>>> = OnceLock::new();
static INPUT_TX: OnceLock<tokio::sync::watch::Sender<Arc<InputState>>> = OnceLock::new();

static SELECTED_BODY_UUID_RX: OnceLock<tokio::sync::watch::Receiver<Option<String>>> = OnceLock::new();
static SELECTED_BODY_UUID_TX: OnceLock<tokio::sync::watch::Sender<Option<String>>> = OnceLock::new();

static SELECTED_FLEET_UUID_RX: OnceLock<tokio::sync::watch::Receiver<Option<String>>> = OnceLock::new();
static SELECTED_FLEET_UUID_TX: OnceLock<tokio::sync::watch::Sender<Option<String>>> = OnceLock::new();

static UI_STATE_RX: OnceLock<tokio::sync::watch::Receiver<Arc<crate::UIState>>> = OnceLock::new();
static UI_STATE_TX: OnceLock<tokio::sync::watch::Sender<Arc<crate::UIState>>> = OnceLock::new();

static UI_INFO_RX: OnceLock<tokio::sync::watch::Receiver<Arc<crate::UIInfo>>> = OnceLock::new();
static UI_INFO_TX: OnceLock<tokio::sync::watch::Sender<Arc<crate::UIInfo>>> = OnceLock::new();

static FLEET_ORDERS: OnceLock<RwLock<HashMap<String, Vec<FleetOrder>>>> = OnceLock::new();
static COLONY_ACTIONS : OnceLock<RwLock<HashMap<String, Vec<ColonyAction>>>> = OnceLock::new();
static STAR_ACTIONS: OnceLock<RwLock<HashMap<String, Vec<StarAction>>>> = OnceLock::new();

static PLAYER_STATE_RX: OnceLock<tokio::sync::watch::Receiver<Arc<Vec<PlayerState>>>> = OnceLock::new();
static PLAYER_STATE_TX: OnceLock<tokio::sync::watch::Sender<Arc<Vec<PlayerState>>>> = OnceLock::new();

pub static HAS_JUST_TAB: AtomicBool = AtomicBool::new(true);

pub fn setup(){
    let (cam_tx, cam_rx) = watch::channel(Arc::new(Camera::default()));
    CAMERA_RX.set(cam_rx.clone()).ok();
    CAMERA_TX.set(cam_tx.clone()).ok();

    let (star_map_tx, star_map_rx) = watch::channel(Arc::new(StarMap::default()));
    STAR_MAP_RX.set(star_map_rx.clone()).ok();
    STAR_MAP_TX.set(star_map_tx.clone()).ok();

    let (input_tx, input_rx) = watch::channel(Arc::new(InputState::default()));
    INPUT_RX.set(input_rx.clone()).ok();
    INPUT_TX.set(input_tx.clone()).ok();

    let (selected_body_tx, selected_body_rx) = watch::channel(None);
    SELECTED_BODY_UUID_RX.set(selected_body_rx.clone()).ok();
    SELECTED_BODY_UUID_TX.set(selected_body_tx.clone()).ok();

    let (ui_tx, ui_rx) = watch::channel(Arc::new(crate::UIState::default()));
    UI_STATE_RX.set(ui_rx.clone()).ok();
    UI_STATE_TX.set(ui_tx.clone()).ok();

    let (ui_info_tx, ui_info_rx) = watch::channel(Arc::new(crate::UIInfo::default()));
    UI_INFO_RX.set(ui_info_rx.clone()).ok();
    UI_INFO_TX.set(ui_info_tx.clone()).ok();

    let (selected_fleet_tx, selected_fleet_rx) = watch::channel(None);
    SELECTED_FLEET_UUID_RX.set(selected_fleet_rx.clone()).ok();
    SELECTED_FLEET_UUID_TX.set(selected_fleet_tx.clone()).ok();

    let fleet_orders = RwLock::new(HashMap::<String, Vec<FleetOrder>>::default());
    FLEET_ORDERS.set(fleet_orders).ok();

    let (player_state_tx, player_state_rx) = watch::channel(Arc::new(Vec::<PlayerState>::new()));
    PLAYER_STATE_RX.set(player_state_rx.clone()).ok();
    PLAYER_STATE_TX.set(player_state_tx.clone()).ok();

    let colony_actions = RwLock::new(HashMap::<String, Vec<ColonyAction>>::default());
    COLONY_ACTIONS.set(colony_actions).ok();

    let star_actions = RwLock::new(HashMap::<String, Vec<StarAction>>::default());
    STAR_ACTIONS.set(star_actions).ok();
}

pub fn get_camera_state() -> Arc<Camera> {
    CAMERA_RX.get().unwrap().borrow().clone()
}

pub fn set_camera(camera: Camera) {
    if let Some(sender) = CAMERA_TX.get() {
        sender.send(Arc::new(camera)).ok();
    }
}

pub fn get_star_map_state() -> Arc<StarMap> {
    STAR_MAP_RX.get().unwrap().borrow().clone()
}

pub fn set_star_map(star_map: StarMap) {
    if let Some(sender) = STAR_MAP_TX.get() {
        sender.send(Arc::new(star_map)).ok();
    }
}

pub fn get_input_state() -> Arc<InputState> {
    INPUT_RX.get().unwrap().borrow().clone()
}

pub fn set_input_state(input_state: InputState) {
    if let Some(sender) = INPUT_TX.get() {
        sender.send(Arc::new(input_state)).ok();
    }
}

pub fn get_selected_body_id() -> Option<String> {
    (*SELECTED_BODY_UUID_RX.get().unwrap().borrow()).clone()
}

pub fn set_selected_body_id(id: Option<String>) {
    let is_some = id.is_some();
    if let Some(sender) = SELECTED_BODY_UUID_TX.get() {
        sender.send(id).ok();
    }
    // Only clear fleet when actively selecting a body (not when just clearing)
    if is_some {
        if let Some(sender) = SELECTED_FLEET_UUID_TX.get() {
            sender.send(None).ok();
        }
    }
}

pub fn get_ui_state_from_channel() -> crate::UIState {
    UI_STATE_RX.get().unwrap().borrow().as_ref().clone()
}

pub fn set_ui_state_to_channel(ui_state: crate::UIState) {
    if let Some(sender) = UI_STATE_TX.get() {
        sender.send(Arc::new(ui_state)).ok();
    }
}

pub fn get_ui_info_from_channel() -> crate::UIInfo {
    UI_INFO_RX.get().unwrap().borrow().as_ref().clone()
}

pub fn set_ui_info_to_channel(ui_info: crate::UIInfo) {
    if let Some(sender) = UI_INFO_TX.get() {
        sender.send(Arc::new(ui_info)).ok();
    }
}

pub fn get_selected_fleet_id() -> Option<String> {
    (*SELECTED_FLEET_UUID_RX.get().unwrap().borrow()).clone()
}

pub fn set_selected_fleet_id(id: Option<String>) {
    let is_some = id.is_some();
    if let Some(sender) = SELECTED_FLEET_UUID_TX.get() {
        sender.send(id).ok();
    }
    // Only clear body when actively selecting a fleet (not when just clearing)
    if is_some {
        if let Some(sender) = SELECTED_BODY_UUID_TX.get() {
            sender.send(None).ok();
        }
    }
}

pub fn get_fleet_order_by_id(id: String) -> Option<Vec<FleetOrder>> {
    if let Some(fleet_orders) = FLEET_ORDERS.get() {
        fleet_orders.write().ok()?.remove(&id)
    } else {
        None
    }
}

pub fn insert_fleet_order(order: FleetOrder) {
    let fleet_orders = match FLEET_ORDERS.get() {
        Some(orders) => orders,
        None => return,
    };

    match fleet_orders.write() {
        Ok(mut orders) => {
            orders.entry(order.fleet_id.clone()).or_insert_with(Vec::new).push(order);
        },
        Err(_) => return,
    }
}

pub fn remove_player(player_id: String) {
    let mut player_states = match PLAYER_STATE_RX.get() {
        Some(rx) => (*rx.borrow().clone()).clone(),
        None => return,
    };

    player_states.retain(|player| player.id != player_id);
    set_player_states(Arc::new(player_states));
}

pub fn add_player(player_state: PlayerState) {
    let mut player_states = match PLAYER_STATE_RX.get() {
        Some(rx) => (*rx.borrow().clone()).clone(),
        None => return,
    };

    player_states.push(player_state);
    set_player_states(Arc::new(player_states));
}

pub fn get_player_states() -> Arc<Vec<PlayerState>> {
    PLAYER_STATE_RX.get().unwrap().borrow().clone()
}

pub fn set_player_states(player_states: Arc<Vec<PlayerState>>) {
    if let Some(sender) = PLAYER_STATE_TX.get() {
        sender.send(player_states).ok();
    }
}

/// Adds a colony action for the given colony id. This will be consumed by the sim and used to update the colony state.
pub fn add_colony_action(colony_id: String, action: ColonyAction) {
    let colony_actions = match COLONY_ACTIONS.get() {
        Some(actions) => actions,
        None => return,
    };

    match colony_actions.write() {
        Ok(mut actions) => {
            actions.entry(colony_id).or_insert_with(Vec::new).push(action);
        },
        Err(_) => return,
    }
}

/// Consumes and returns all actions for the given colony id.
pub fn consume_colony_actions(colony_id: String) -> Vec<ColonyAction> {
    let colony_actions = match COLONY_ACTIONS.get() {
        Some(actions) => actions,
        None => return Vec::new(),
    };

    match colony_actions.write() {
        Ok(mut actions) => {
            actions.remove(&colony_id).unwrap_or_default()
        },
        Err(_) => Vec::new(),
    }
}

/// Queues a star action for the given star id. Consumed by the sim thread in Star::update().
pub fn add_star_action(star_id: String, action: StarAction) {
    let star_actions = match STAR_ACTIONS.get() {
        Some(actions) => actions,
        None => return,
    };

    match star_actions.write() {
        Ok(mut actions) => {
            actions.entry(star_id).or_insert_with(Vec::new).push(action);
        },
        Err(_) => return,
    }
}

/// Consumes and returns all star actions for the given star id.
pub fn consume_star_actions(star_id: &str) -> Vec<StarAction> {
    let star_actions = match STAR_ACTIONS.get() {
        Some(actions) => actions,
        None => return Vec::new(),
    };

    match star_actions.write() {
        Ok(mut actions) => {
            actions.remove(star_id).unwrap_or_default()
        },
        Err(_) => Vec::new(),
    }
}