mod app;
mod channels;
mod data;
mod entities;
mod input_handle;
mod renderers;
mod extentions;

pub use app::{App, UIInfo, UIState};
use ratatui::style::Color;
use ratatui::layout::Rect;

use std::alloc::System;
use std::cell::RefCell;
use std::ops::Deref;
use std::sync::atomic::{self, AtomicBool, AtomicI32};
use std::io;
use std::thread::Thread;

use crate::channels::channels::{
    add_player, get_camera_state, get_input_state, get_star_map_state, set_star_map,
};
use crate::entities::fleet::Fleet;
use crate::entities::planet::colony::ColonyResources;
use crate::entities::planet::{Body, Colony};
use crate::entities::player_state::{self, PlayerState, with_mut_player_state, with_player_state};
use crate::entities::star_map::{self, StarMap};
use crate::entities::planet;

//TODO: Move into UIInfo
pub static MAP_X: AtomicI32 = AtomicI32::new(1000);
pub static MAP_Y: AtomicI32 = AtomicI32::new(1000);
pub static MAP_SIZE: (AtomicI32, AtomicI32) = (AtomicI32::new(2000), AtomicI32::new(2000));


pub static ELAPSED_SIM: AtomicI32 = AtomicI32::new(0);
pub static ELAPSED_FULL_SIM: AtomicI32 = AtomicI32::new(0);
pub static TIME_SCALE: AtomicI32 = AtomicI32::new(0);

pub static JUST_LOADED_GAME: AtomicBool = AtomicBool::new(false);

//Symbols:
pub static FUEL: char = '∴';
pub static LIGHT_METALS: char = '⟛';
pub static HEAVY_METALS: char = '⟚';
pub static RARE_ELEMENTS: char = '⊛';
pub static SUPER_ELEMENTS : char = '⟁';
pub static MOON: char = '◑';
pub static PLANET: char = '⊚';
pub static STAR: char = '𖤓';

//Colors:
pub static ACTIVE_COLOR: Color = Color::Cyan;
pub static INACTIVE_COLOR: Color = Color::White;

thread_local! {
    static APP_UI_STATE: RefCell<UIState> = RefCell::new(UIState::default());
    static APP_UI_INFO: RefCell<UIInfo> = RefCell::new(UIInfo::default());
}

#[tokio::main]
async fn main() -> io::Result<()> {
    //println!("MAIN");

    color_eyre::install().unwrap();

    let mut terminal = ratatui::init();

    crossterm::execute!(io::stdout(), crossterm::event::EnableMouseCapture)?;

    let mut app = App::new();

    app.run(&mut terminal).await?;

    ratatui::restore();

    Ok(())
}



#[derive(Clone, Copy, PartialEq, Eq)]
pub enum UIScreen {
    Splash,
    MainMenu,
    Game,
}

#[derive(Clone, Copy, PartialEq, Eq)]
pub enum GameScreenTab {
    SystemView = 0,
    Manager = 1,
    ShipDesigner = 2,
    SubsystemDesigner = 3,
    Research = 4,
}

impl GameScreenTab {
    pub fn next(self) -> Self {
        match self {
            GameScreenTab::SystemView => GameScreenTab::Manager,
            GameScreenTab::Manager => GameScreenTab::ShipDesigner,
            GameScreenTab::ShipDesigner => GameScreenTab::SubsystemDesigner,
            GameScreenTab::SubsystemDesigner => GameScreenTab::Research,
            GameScreenTab::Research => GameScreenTab::SystemView,
        }
    }

    pub fn previous(self) -> Self {
        match self {
            GameScreenTab::SystemView => GameScreenTab::Research,
            GameScreenTab::Manager => GameScreenTab::SystemView,
            GameScreenTab::ShipDesigner => GameScreenTab::Manager,
            GameScreenTab::SubsystemDesigner => GameScreenTab::ShipDesigner,
            GameScreenTab::Research => GameScreenTab::SubsystemDesigner,
        }
    }
}

pub fn with_ui_state<R>(f: impl FnOnce(&UIState) -> R) -> R {
    APP_UI_STATE.with(|ui_state| {
        let ui_state = ui_state.borrow();
        f(&ui_state)
    })
}

pub fn with_ui_state_mut<R>(f: impl FnOnce(&mut UIState) -> R) -> R {
    APP_UI_STATE.with(|ui_state| {
        let mut ui_state = ui_state.borrow_mut();
        f(&mut ui_state)
    })
}

pub fn with_ui_info<R>(f: impl FnOnce(&UIInfo) -> R) -> R {
    APP_UI_INFO.with(|ui_info| {
        let ui_info = ui_info.borrow();
        f(&ui_info)
    })
}

pub fn with_ui_info_mut<R>(f: impl FnOnce(&mut UIInfo) -> R) -> R {
    APP_UI_INFO.with(|ui_info| {
        let mut ui_info = ui_info.borrow_mut();
        f(&mut ui_info)
    })
}

fn replace_ui_state(ui_state: UIState) {
    with_ui_state_mut(|state| {
        *state = ui_state;
    });
}

fn replace_ui_info(ui_info: UIInfo) {
    with_ui_info_mut(|info| {
        *info = ui_info;
    });
}

fn current_ui_info() -> UIInfo {
    with_ui_info(|ui_info| ui_info.clone())
}

pub fn clear_star_map_info() {
    with_ui_info_mut(|ui_info| ui_info.star_map_info.clear());
}

//Temp version
pub fn new_game(){
    let mut star_map = StarMap::new();
    let star = &mut star_map.stars[0];

    let resources = ColonyResources{
        population: 1_000_000,
        base_ic: 1.0,
        ic: 0.0,
        light_elements: 1_000_000.0,
        light_metals: 1_000_000.0,
        heavy_metals: 1_000_000.0,
        rare_elements: 1_000_000.0,
        super_elements: 1_000_000.0,
    };

    let mut colony = Colony::new("Colony1".to_string(), star.bodies[0].id.clone(), resources);
    colony.resources.population = 1_000_000;
    colony.buildings = Vec::new();
    
    // Configure the authoritative PlayerState singleton.
    let player_id = with_player_state(|ps| ps.id.clone());
    colony.owner_id = Some(player_id);
    let colony_id = colony.id.clone();
    star.bodies[0].colony = Some(colony);

    with_mut_player_state(|ps| {
        ps.name = "player1".to_string();
        ps.add_colony(colony_id);
        ps.ship_designer.load_subsystem_library();
    });

    // Publish a snapshot to the channel so the renderer can read basic player info.
    let snapshot = with_player_state(|ps| ps.clone());
    add_player(snapshot);
    let fleet = Fleet::load_snapshot();
    star.fleets.push(fleet);
    set_star_map(star_map);
}

pub fn save_game(name: String){
    let star_map = get_star_map_state().deref().clone();
    let player_state = with_player_state(|ps| ps.clone());

    let save_data = (star_map, player_state);
    
    let json = serde_json::to_string(&save_data).expect("Failed to serialize save data");
    let dir = std::path::Path::new("saves");
    std::fs::create_dir_all(dir).expect("Failed to create saves directory");
    std::fs::write(dir.join(format!("{}.json", name)), json).expect("Failed to write save file");
}

pub fn list_saves() -> Vec<String> {
    let mut saves = Vec::new();
    if let Ok(entries) = std::fs::read_dir("saves") {
        for entry in entries.flatten() {
            let path = entry.path();
            if path.extension().and_then(|e| e.to_str()) == Some("json") {
                if let Some(name) = path.file_stem().and_then(|s| s.to_str()) {
                    saves.push(name.to_string());
                }
            }
        }
    }
    saves.sort();
    saves
}

pub fn load_game(name: String){    
    let path = std::path::Path::new("saves").join(format!("{}.json", name));
    let json = std::fs::read_to_string(path).expect("Failed to read save file");
    let (star_map, player_state): (StarMap, PlayerState) = serde_json::from_str(&json).expect("Failed to deserialize save data");
    
    set_star_map(star_map);
    with_mut_player_state(|ps| {
        *ps = player_state;
    });
    
    JUST_LOADED_GAME.store(true, atomic::Ordering::Relaxed);
}

/// Returns a timestamp string in the format YYYY-MM-DD-HH-MM-SS using the UTC time.
pub fn now_timestamp() -> String {
    use std::time::{SystemTime, UNIX_EPOCH};
    let secs = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default()
        .as_secs();

    // Gregorian calendar decomposition from Unix epoch (days since 1970-01-01)
    let sec = secs % 60;
    let min = (secs / 60) % 60;
    let hour = (secs / 3600) % 24;
    let days = (secs / 86400) as i64;

    let z = days + 719_468;
    let era = if z >= 0 { z } else { z - 146_096 } / 146_097;
    let doe = z - era * 146_097;
    let yoe = (doe - doe / 1_460 + doe / 36_524 - doe / 146_096) / 365;
    let y = yoe + era * 400;
    let doy = doe - (365 * yoe + yoe / 4 - yoe / 100);
    let mp = (5 * doy + 2) / 153;
    let d = doy - (153 * mp + 2) / 5 + 1;
    let m = if mp < 10 { mp + 3 } else { mp - 9 };
    let y = if m <= 2 { y + 1 } else { y };

    format!("{:04}-{:02}-{:02}-{:02}-{:02}-{:02}", y, m, d, hour, min, sec)
}


pub fn get_pixel_size_km() -> f64 {
    let camera = get_camera_state();
    let zoom = camera.zoom;
    1.0 / zoom
}

pub fn should_render_moons() -> bool {
    if get_pixel_size_km() < 2_500_000.0 {
        return true;
    } else {
        return false;
    }
}

pub fn get_keys_down() -> Option<Vec<char>> {
    let input_state = get_input_state();
    if (input_state.keys_down.is_empty()) {
        None
    } else {
        Some(input_state.keys_down.clone())
    }
}

pub fn get_mouse_position() -> (u16, u16) {
    let input_state = get_input_state();
    input_state.mouse_position
}

pub fn distance_u16(a: (u16, u16), b: (u16, u16)) -> f64 {
    let dx = a.0 as f64 - b.0 as f64;
    let dy = a.1 as f64 - b.1 as f64;
    (dx * dx + dy * dy).sqrt()
}

pub fn distance(a: (f64, f64), b: (f64, f64)) -> f64 {
    let dx = a.0 - b.0;
    let dy = a.1 - b.1;
    (dx * dx + dy * dy).sqrt()
}

/// Checks if the mouse is over the given position, within a certain radius.
/// This check is relative to the GLOBAL screen not any area.
pub fn is_over_pos(pos: (u16, u16), mouse_position: (u16, u16)) -> bool {
    // Check if the mouse is within a certain radius of the planet's screen position
    let radius = 3.0; // Adjust as needed
    let distance = distance_u16(pos, mouse_position);
    return distance <= radius;
}

pub fn is_in_area(pos: (u16, u16), area: ratatui::prelude::Rect) -> bool {
    pos.0 >= area.left() && pos.0 < area.right() && pos.1 >= area.top() && pos.1 < area.bottom()
}

pub fn mouse_pos_in_rect(mouse_pos: (u16, u16), area: Rect) -> (u16, u16) {
    (
        mouse_pos.0.saturating_sub(area.x),
        mouse_pos.1.saturating_sub(area.y),
    )
}

pub fn set_map_pos(x: u16, y: u16) {
    MAP_X.store(x as i32, std::sync::atomic::Ordering::Relaxed);
    MAP_Y.store(y as i32, std::sync::atomic::Ordering::Relaxed);
}

pub fn get_map_pos() -> (u16, u16) {
    let x = MAP_X.load(std::sync::atomic::Ordering::Relaxed) as u16;
    let y = MAP_Y.load(std::sync::atomic::Ordering::Relaxed) as u16;
    (x, y)
}

/// Checks if the mouse is over a given position, within a certain radius, relative to a given area.
pub fn is_mouse_over_position_in_area_coordinates(
    pos: (u16, u16),
    mouse_position: (u16, u16),
    area: Rect,
) -> bool {
    let relative_mouse_pos = mouse_pos_in_rect(mouse_position, area);
    let radius = 2; // Adjust as needed
    let dx = relative_mouse_pos.0 as i32 - pos.0 as i32;
    let dy = relative_mouse_pos.1 as i32 - pos.1 as i32;

    dx * dx + dy * dy <= radius * radius
}

pub fn get_star_by_id(id: String) -> Option<entities::star::Star> {
    let star_map = get_star_map_state();

    for star in &star_map.stars {
        if star.id == id {
            return Some(star.clone());
        }
    }
    None
}

pub fn get_body_by_id(id: String) -> Option<planet::Body> {
    let star_map = get_star_map_state();

    for star in &star_map.stars {
        for planet in &star.bodies {
            if planet.id == id {
                return Some(planet.clone());
            }

            for moon in &planet.moons {
                if moon.id == id {
                    return Some(moon.clone());
                }
            }
        }
    }
    None
}

pub fn get_bodies() -> Vec<Body> {
    let star_map = get_star_map_state();
    let mut bodies = vec![];

    for star in &star_map.stars {
        for planet in &star.bodies {
            bodies.push(planet.clone());

            for moon in &planet.moons {
                bodies.push(moon.clone());
            }
        }
    }

    bodies
}

pub fn get_player_colonies(player_id: String) -> Vec<Colony> {
    let colonies = get_bodies()
        .iter()
        .filter_map(|b| b.colony.as_ref())
        .filter(|colony| colony.owner_id.as_deref() == Some(player_id.as_str()))
        .cloned()
        .collect();

    colonies

}

pub fn get_fleet_by_id(id: String) -> Option<Fleet> {
    let star_map = get_star_map_state();

    for star in &star_map.stars {
        for fleet in &star.fleets {
            if fleet.id == id {
                return Some(fleet.clone());
            }
        }
    }
    None
}

pub fn get_star_id_for_fleet(fleet_id: &str) -> Option<String> {
    let star_map = get_star_map_state();
    for star in &star_map.stars {
        if star.fleets.iter().any(|f| f.id == fleet_id) {
            return Some(star.id.clone());
        }
    }
    None
}

pub fn get_star_id_for_body(body_id: &str) -> Option<String> {
    let star_map = get_star_map_state();
    for star in &star_map.stars {
        for body in &star.bodies {
            if body.id == body_id {
                return Some(star.id.clone());
            }
            if body.moons.iter().any(|m| m.id == body_id) {
                return Some(star.id.clone());
            }
        }
    }
    None
}

pub fn get_fleets() -> Vec<Fleet> {
    let star_map = get_star_map_state();
    let fleets = star_map
        .stars
        .iter()
        .flat_map(|s| s.fleets.clone())
        .collect();

    fleets
}

fn line_points(x0: i32, y0: i32, x1: i32, y1: i32) -> Vec<(i32, i32)> {
    let mut points = Vec::new();

    let dx = (x1 - x0).abs();
    let dy = -(y1 - y0).abs();
    let sx = if x0 < x1 { 1 } else { -1 };
    let sy = if y0 < y1 { 1 } else { -1 };
    let mut err = dx + dy;

    let mut x = x0;
    let mut y = y0;

    loop {
        points.push((x, y));

        if x == x1 && y == y1 {
            break;
        }

        let e2 = 2 * err;

        if e2 >= dy {
            err += dy;
            x += sx;
        }

        if e2 <= dx {
            err += dx;
            y += sy;
        }
    }

    points
}

fn fit_system_line(name: &str, moons: &str, max_width: usize) -> String {
    if max_width == 0 {
        return String::new();
    }

    let mut left = format!("({name}) ");
    let mut right = moons.to_string();

    let pad_to_center = |left: &mut String, right: &mut String| {
        let l = left.chars().count();
        let r = right.chars().count();
        if l < r {
            *left = format!("{}{}", " ".repeat(r - l), left);
        } else if r < l {
            right.push_str(&" ".repeat(l - r));
        }
    };

    pad_to_center(&mut left, &mut right);

    let mut full = format!("{left}◯{right}");
    if full.chars().count() <= max_width {
        return full;
    }

    let circle = "◯";
    let reserve = circle.chars().count();
    let available = max_width.saturating_sub(reserve);
    let left_budget = available / 2;
    let right_budget = available - left_budget;

    let left_trimmed: String = left
        .chars()
        .rev()
        .take(left_budget)
        .collect::<Vec<_>>()
        .into_iter()
        .rev()
        .collect();

    let right_trimmed: String = right.chars().take(right_budget).collect();

    full = format!("{left_trimmed}{circle}{right_trimmed}");
    full
}

fn centered_line_rect(area: Rect, line_index: u16, line_width: u16) -> Rect {
    let clamped_width = line_width.min(area.width);
    let x_offset = area.width.saturating_sub(clamped_width) / 2;

    Rect {
        x: area.x.saturating_add(x_offset),
        y: area.y.saturating_add(line_index),
        width: clamped_width,
        height: 1,
    }
}
