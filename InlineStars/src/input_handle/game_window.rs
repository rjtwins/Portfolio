use std::sync::atomic;

use crossterm::event::KeyCode;

use crate::{
    TIME_SCALE,
    app::TimeScale,
    app::ui_state::{SidebarFocus, StarMapDetailOption},
    channels::channels::{get_selected_fleet_id, get_ui_info_from_channel, get_ui_state_from_channel, insert_fleet_order, set_selected_body_id, set_selected_fleet_id, set_ui_state_to_channel},
    entities::fleet::{FleetOrder, FleetOrderType, OrderAddType},
};

use super::mouse::parse_distance;

pub(super) fn handle_key_down_on_game_window(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();

    // Capture all keys for the distance input field when active
    if ui_state.context_menu.visible && ui_state.context_menu.pending_input.is_some() {
        match key {
            KeyCode::Esc => {
                ui_state.context_menu.visible = false;
                ui_state.context_menu.pending_input = None;
            }
            KeyCode::Enter => {
                if let Some(pi) = ui_state.context_menu.pending_input.take() {
                    if let (Some(fleet_id), Some(dist)) = (get_selected_fleet_id(), parse_distance(&pi.value)) {
                        insert_fleet_order(FleetOrder {
                            fleet_id,
                            add_type: OrderAddType::Replace,
                            order: FleetOrderType::KeepDistanceToObject(pi.target_id, dist),
                        });
                    }
                }
                ui_state.context_menu.visible = false;
            }
            KeyCode::Backspace => {
                if let Some(pi) = ui_state.context_menu.pending_input.as_mut() {
                    pi.value.pop();
                }
            }
            KeyCode::Char(c) => {
                if let Some(pi) = ui_state.context_menu.pending_input.as_mut() {
                    pi.value.push(c);
                }
            }
            _ => {}
        }
        set_ui_state_to_channel(ui_state);
        return;
    }

    if ui_state.star_map_details_menu_expanded && ui_state.star_map_filter_editing {
        match key {
            KeyCode::Esc | KeyCode::Enter => {
                ui_state.star_map_filter_editing = false;
            }
            KeyCode::Backspace => {
                ui_state.star_map_filter_text.pop();
            }
            KeyCode::Char(c) => {
                ui_state.star_map_filter_text.push(c);
            }
            _ => {}
        }
        set_ui_state_to_channel(ui_state);
        return;
    }

    match key {
        KeyCode::Esc if ui_state.context_menu.visible => {
            ui_state.context_menu.visible = false;
            set_ui_state_to_channel(ui_state);
        }
        // Toggle sidebar focus between Colonies, Planets, and Fleets
        KeyCode::Char('q') | KeyCode::Char('Q') => {
            ui_state.sidebar_focus = match ui_state.sidebar_focus {
                SidebarFocus::Colonies => SidebarFocus::Fleets,
                SidebarFocus::Planets => SidebarFocus::Colonies,
                SidebarFocus::Fleets => SidebarFocus::Planets,
            };
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('e') | KeyCode::Char('E') => {
            ui_state.sidebar_focus = match ui_state.sidebar_focus {
                SidebarFocus::Colonies => SidebarFocus::Planets,
                SidebarFocus::Planets => SidebarFocus::Fleets,
                SidebarFocus::Fleets => SidebarFocus::Colonies,
            };
            set_ui_state_to_channel(ui_state);
        }

        KeyCode::Char('+') => {
            update_game_speed(true);
        }
        KeyCode::Char('-') => {
            update_game_speed(false);
        }
        KeyCode::Char('m') | KeyCode::Char('M') => {
            ui_state.toggle_star_map_details_menu();
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('f') | KeyCode::Char('F') => {
            ui_state.activate_star_map_filter();
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('o') | KeyCode::Char('O') => {
            ui_state.toggle_star_map_detail(StarMapDetailOption::Orbits);
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('l') | KeyCode::Char('L') => {
            ui_state.toggle_star_map_detail(StarMapDetailOption::Names);
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('a') | KeyCode::Char('A') => {
            ui_state.toggle_star_map_detail(StarMapDetailOption::Asteroids);
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('c') | KeyCode::Char('C') => {
            ui_state.toggle_star_map_detail(StarMapDetailOption::Comets);
            set_ui_state_to_channel(ui_state);
        }
        _ => {}
    }
}

fn update_game_speed(increase: bool) {
    let mut time_scale_index = TIME_SCALE.load(atomic::Ordering::Relaxed) as usize;

    if increase {
        if time_scale_index < TimeScale::SCALE_ARRAY.len() - 1 {
            time_scale_index += 1;
        }
    } else {
        if time_scale_index > 0 {
            time_scale_index -= 1;
        }
    }
    TIME_SCALE.store(time_scale_index as i32, atomic::Ordering::Relaxed);
}

pub(super) fn handle_key_down_on_system_tree_view(key: KeyCode) {
    let mut tree_state = get_ui_info_from_channel().system_tree_state.clone();
    let mut ui_state = get_ui_state_from_channel();

    match key {
        KeyCode::Up | KeyCode::Char('w') => {
            tree_state.key_up();
        }
        KeyCode::Down | KeyCode::Char('s') => {
            tree_state.key_down();
        }
        KeyCode::Left | KeyCode::Char('a') => {
            tree_state.key_left();
        }
        KeyCode::Right | KeyCode::Char('d') => {
            tree_state.key_right();
        }

        _ => {}
    }

    ui_state.system_tree_state = tree_state;

    if ui_state.system_tree_state.selected().is_empty(){
        set_ui_state_to_channel(ui_state);
        return;
    }

    let selected_uuid = ui_state.system_tree_state.selected().last().unwrap().clone();
    ui_state.colonies_list_state.select(Vec::new());
    set_selected_body_id(Some(selected_uuid));
    set_ui_state_to_channel(ui_state);
}

pub(super) fn handle_key_down_on_fleets(key: KeyCode) {
    let mut tree_state = get_ui_info_from_channel().fleets_tree_state.clone();
    let mut ui_state = get_ui_state_from_channel();

    match key {
        KeyCode::Up | KeyCode::Char('w') => { tree_state.key_up(); }
        KeyCode::Down | KeyCode::Char('s') => { tree_state.key_down(); }
        KeyCode::Enter => {
            if let Some(fleet_id) = tree_state.selected().last().cloned() {
                set_selected_fleet_id(Some(fleet_id));
                set_selected_body_id(None);
            }
        }
        _ => {}
    }

    ui_state.fleets_tree_state = tree_state;
    set_ui_state_to_channel(ui_state);
}

pub(super) fn handle_key_down_on_colonies(key: KeyCode) {
    let mut tree_state = get_ui_info_from_channel().colonies_tree_state.clone();
    let mut ui_state = get_ui_state_from_channel();

    match key {
        KeyCode::Up | KeyCode::Char('w') => { tree_state.key_up(); }
        KeyCode::Down | KeyCode::Char('s') => { tree_state.key_down(); }
        KeyCode::Enter => {
            if let Some(body_id) = tree_state.selected().last().cloned() {
                set_selected_body_id(Some(body_id));
                set_selected_fleet_id(None);
                ui_state.system_tree_state.select(Vec::new());
            }
        }
        _ => {}
    }

    ui_state.colonies_list_state = tree_state;
    set_ui_state_to_channel(ui_state);
}
