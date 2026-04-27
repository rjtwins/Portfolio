use crossterm::event::KeyCode;

use crate::{
    channels::channels::{self, get_selected_body_id, get_ui_info_from_channel, get_ui_state_from_channel, set_ui_state_to_channel},
    get_body_by_id,
    renderers::colony_manager_renderer::{ColonyMangerPanel, ColonyManagerTab},
};

use super::common::ColonyAction;

pub(super) fn handle_key_down_on_colony_manager(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    let mut ui_info = get_ui_info_from_channel();

    match ui_state.colony_manager_state.active_tab {
        ColonyManagerTab::Buildings => {
            handle_buildings_tab_key(key, &mut ui_state, &mut ui_info);
        }
        ColonyManagerTab::Shipyards => {
            handle_shipyards_tab_key(key, &mut ui_state, &mut ui_info);
        }
    }

    set_ui_state_to_channel(ui_state);
}

fn handle_buildings_tab_key(
    key: KeyCode,
    ui_state: &mut crate::app::ui_state::UIState,
    ui_info: &mut crate::app::ui_info::UIInfo,
) {
    match key {
        KeyCode::Left | KeyCode::Char('q') => {
            ui_state.colony_manager_state.selected_panel = ui_state.colony_manager_state.selected_panel.previous();
        }
        KeyCode::Right | KeyCode::Char('e') => {
            ui_state.colony_manager_state.selected_panel = ui_state.colony_manager_state.selected_panel.next();
        }
        KeyCode::Up | KeyCode::Char('w') => {
            match ui_state.colony_manager_state.selected_panel {
                ColonyMangerPanel::Queue => { ui_info.colony_manager_info.queue_state.previous(); },
                ColonyMangerPanel::BuildOptions => { ui_info.colony_manager_info.build_options_state.previous(); },
                ColonyMangerPanel::Finished => { ui_info.colony_manager_info.finished_state.previous(); },
            }
        }
        KeyCode::Down | KeyCode::Char('s') => {
            match ui_state.colony_manager_state.selected_panel {
                ColonyMangerPanel::Queue => { ui_info.colony_manager_info.queue_state.next(); },
                ColonyMangerPanel::BuildOptions => { ui_info.colony_manager_info.build_options_state.next(); },
                ColonyMangerPanel::Finished => { ui_info.colony_manager_info.finished_state.next(); },
            }
        }
        _ => {}
    }

    // Sync navigation state from ui_info back to ui_state
    ui_state.colony_manager_state.queue_state = ui_info.colony_manager_info.queue_state.clone();
    ui_state.colony_manager_state.build_options_state = ui_info.colony_manager_info.build_options_state.clone();
    ui_state.colony_manager_state.finished_state = ui_info.colony_manager_info.finished_state.clone();

    // Dispatch actions using the selected building name key
    match ui_state.colony_manager_state.selected_panel {
        ColonyMangerPanel::Queue => {
            if let Some(name) = ui_state.colony_manager_state.queue_state.selected().cloned() {
                match key {
                    KeyCode::Char('+') => { queue_increase(&name); },
                    KeyCode::Char('-') => { queue_decrease(&name); },
                    KeyCode::Char('c') => { queue_toggle(&name); },
                    KeyCode::Char('p') => { queue_pause(&name); },
                    _ => {}
                }
            }
        },
        ColonyMangerPanel::BuildOptions => {
            if let Some(name) = ui_state.colony_manager_state.build_options_state.selected().cloned() {
                match key {
                    KeyCode::Char('+') => { build_add(&name); },
                    KeyCode::Char('c') => { build_add_inf(&name); },
                    _ => {}
                }
            }
        },
        ColonyMangerPanel::Finished => {
            if let Some(name) = ui_state.colony_manager_state.finished_state.selected().cloned() {
                match key {
                    KeyCode::Char('-') => { demolish_finished_building(&name); },
                    _ => {}
                }
            }
        },
    }
}

fn handle_shipyards_tab_key(
    key: KeyCode,
    ui_state: &mut crate::app::ui_state::UIState,
    ui_info: &mut crate::app::ui_info::UIInfo,
) {
    if ui_state.colony_manager_state.retooling {
        match key {
            KeyCode::Esc => {
                ui_state.colony_manager_state.retooling = false;
            }
            KeyCode::Enter | KeyCode::Char('r') => {
                let design_uuid = ui_info.colony_manager_info.retool_design_state.selected().cloned();
                let slipway_id = ui_state.colony_manager_state.slipways_state.selected().cloned();
                if let (Some(slipway_id), Some(design_uuid)) = (slipway_id, design_uuid) {
                    slipway_retool(&slipway_id, &design_uuid);
                }
                ui_state.colony_manager_state.retooling = false;
            }
            KeyCode::Up | KeyCode::Char('w') => {
                ui_info.colony_manager_info.retool_design_state.previous();
                ui_state.colony_manager_state.retool_design_state = ui_info.colony_manager_info.retool_design_state.clone();
            }
            KeyCode::Down | KeyCode::Char('s') => {
                ui_info.colony_manager_info.retool_design_state.next();
                ui_state.colony_manager_state.retool_design_state = ui_info.colony_manager_info.retool_design_state.clone();
            }
            _ => {}
        }
        return;
    }

    match key {
        KeyCode::Up | KeyCode::Char('w') => {
            ui_info.colony_manager_info.slipways_state.previous();
        }
        KeyCode::Down | KeyCode::Char('s') => {
            ui_info.colony_manager_info.slipways_state.next();
        }
        _ => {}
    }

    // Sync slipways navigation back to ui_state
    ui_state.colony_manager_state.slipways_state = ui_info.colony_manager_info.slipways_state.clone();

    // Dispatch slipyard actions
    match key {
        KeyCode::Char('n') => {
            slipway_build();
        }
        KeyCode::Char('e') => {
            if let Some(slipway_id) = ui_state.colony_manager_state.slipways_state.selected().cloned() {
                slipway_extend(&slipway_id);
            }
        }
        KeyCode::Char('r') => {
            ui_state.colony_manager_state.retooling = true;
        }
        KeyCode::Char('+') | KeyCode::Char('=') => {
            if let Some(slipway_id) = ui_state.colony_manager_state.slipways_state.selected().cloned() {
                slipway_queue_increase(&slipway_id);
            }
        }
        KeyCode::Char('-') => {
            if let Some(slipway_id) = ui_state.colony_manager_state.slipways_state.selected().cloned() {
                slipway_queue_decrease(&slipway_id);
            }
        }
        _ => {}
    }
}

fn queue_pause(building_name: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::QueuePause(building_name.to_string()));
}

fn build_add(building_name: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::BuildAdd(building_name.to_string()));
}

fn build_add_inf(building_name: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::BuildAddInf(building_name.to_string()));
}

fn demolish_finished_building(building_name: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c,
        None => return,
    };
    channels::add_colony_action(colony.id.clone(), ColonyAction::FinishedDemolish(building_name.to_string()));
}

fn queue_toggle(building_name: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c,
        None => return,
    };
    channels::add_colony_action(colony.id.clone(), ColonyAction::QueueToggleInf(building_name.to_string()));
}

fn queue_decrease(building_name: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::QueueDecrease(building_name.to_string()));
}

fn queue_increase(building_name: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::QueueIncrease(building_name.to_string()));
}

fn slipway_build() {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::SlipwayBuild);
}

fn slipway_extend(slipway_id: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::SlipwayExtend(slipway_id.to_string()));
}

fn slipway_retool(slipway_id: &str, design_uuid: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::SlipwayRetool(slipway_id.to_string(), design_uuid.to_string()));
}

fn slipway_queue_increase(slipway_id: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::SlipwayQueueIncrease(slipway_id.to_string()));
}

fn slipway_queue_decrease(slipway_id: &str) {
    let body_uuid = match get_selected_body_id() { Some(u) => u, None => return };
    let colony_uuid = match get_body_by_id(body_uuid).and_then(|b| b.colony.clone()) {
        Some(c) => c.id.clone(),
        None => return,
    };
    channels::add_colony_action(colony_uuid, ColonyAction::SlipwayQueueDecrease(slipway_id.to_string()));
}
