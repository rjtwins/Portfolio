use crossterm::event::KeyCode;
use ratatui::{layout::Position, widgets::ListState};

use crate::{
    GameScreenTab,
    app::{
        ship_desginer::{with_mut_ship_designer, with_ship_designer},
        ui_state::{ShipDesignerMenuItem, ShipDesignerPanel, build_ship_designer_menu_entries},
    },
    channels::channels::{get_ui_info_from_channel, get_ui_state_from_channel, set_ui_state_to_channel},
    entities::ship::ShipDesign,
};

pub(super) fn handle_key_down_on_ship_designer(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();

    if ui_state.selected_tab == GameScreenTab::ShipDesigner{
        //ui_state.ship_designer_menu_state.activate();
    }else{
        ui_state.ship_designer_state.menu_popup_open = false;
        set_ui_state_to_channel(ui_state);
        return;
    }

    // Rename mode intercepts all input until Enter (confirm) or Esc (cancel).
    if ui_state.ship_designer_state.rename_buffer.is_some() {
        match key {
            KeyCode::Enter => {
                let new_name = ui_state.ship_designer_state.rename_buffer.take().unwrap_or_default();
                if !new_name.is_empty() {
                    with_mut_ship_designer(|sd| {
                        if let Some(design) = sd.current_design.as_mut() {
                            design.name = new_name;
                        }
                    });
                }
            }
            KeyCode::Esc => {
                ui_state.ship_designer_state.rename_buffer = None;
            }
            KeyCode::Backspace => {
                if let Some(buf) = ui_state.ship_designer_state.rename_buffer.as_mut() {
                    buf.pop();
                }
            }
            KeyCode::Char(c) => {
                if let Some(buf) = ui_state.ship_designer_state.rename_buffer.as_mut() {
                    buf.push(c);
                }
            }
            _ => {}
        }
        set_ui_state_to_channel(ui_state);
        return;
    }

    if matches!(key, KeyCode::Char('m') | KeyCode::Char('M')) {
        if ui_state.ship_designer_state.menu_popup_open {
            close_menu_popup(&mut ui_state);
        } else {
            open_menu_popup(&mut ui_state);
        }
        set_ui_state_to_channel(ui_state);
        return;
    }

    if ui_state.ship_designer_state.menu_popup_open {
        handle_key_down_on_ship_designer_menu(key);
        return;
    }

    match key {
        KeyCode::Char('q') | KeyCode::Char('Q') => {
            ui_state.ship_designer_state.active_panel = ui_state.ship_designer_state.active_panel.previous();
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('e') | KeyCode::Char('E') => {
            ui_state.ship_designer_state.active_panel = ui_state.ship_designer_state.active_panel.next();
            set_ui_state_to_channel(ui_state);
        }
        _ => {}
    }

    handle_key_down_on_ship_designer_menu(key);
    handle_key_down_on_design_tree(key);
    handle_key_down_on_subsystem_tree(key);
    handle_key_down_on_installed_subsystems(key);
}

fn handle_key_down_on_design_tree(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    let mut ui_info = get_ui_info_from_channel();
    let active = ui_state.ship_designer_state.active_panel == ShipDesignerPanel::ShipDesigns;

    if !active {
        return;
    }

    match key {
        KeyCode::Up | KeyCode::Char('w') => {
            ui_info.ship_designer_state.design_tree_state.key_up();
        }
        KeyCode::Down | KeyCode::Char('s') => {
            ui_info.ship_designer_state.design_tree_state.key_down();
        }
        KeyCode::Enter | KeyCode::Right | KeyCode::Char('d') => {
            if let Some(design_id) = ui_info.ship_designer_state.design_tree_state.selected().last().cloned() {
                select_ship_design(&design_id);
            }
        }
        _ => {
            return;
        }
    }

    ui_state.ship_designer_state.design_tree_state = ui_info.ship_designer_state.design_tree_state.clone();
    sync_design_tree_selection(&mut ui_state);
    set_ui_state_to_channel(ui_state);
}

fn handle_key_down_on_installed_subsystems(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    let active = ui_state.ship_designer_state.active_panel == ShipDesignerPanel::SubsystemList;

    if !active {
        ui_state.ship_designer_state.installed_subsystems_state.select(None);
        set_ui_state_to_channel(ui_state);
        return;
    }

    let design_locked = with_ship_designer(|sd| sd.current_design.as_ref().map(|d| d.locked).unwrap_or(false));

    match key {
        KeyCode::Up | KeyCode::Char('w') => ui_state.ship_designer_state.installed_subsystems_state.select_previous(),
        KeyCode::Down | KeyCode::Char('s') => ui_state.ship_designer_state.installed_subsystems_state.select_next(),
        KeyCode::Left | KeyCode::Char('a') if !design_locked => {
            let selected_idx = match ui_state.ship_designer_state.installed_subsystems_state.selected() {
                Some(s) => s,
                None => { set_ui_state_to_channel(ui_state); return; }
            };
            let id = match get_grouped_installed_id(selected_idx) {
                Some(id) => id,
                None => { set_ui_state_to_channel(ui_state); return; }
            };
            let remaining = remove_subsystem_from_ship(&id);
            if remaining == 0 {
                ui_state.ship_designer_state.installed_subsystems_state.select(None);
            } else if selected_idx >= remaining {
                ui_state.ship_designer_state.installed_subsystems_state.select(Some(remaining - 1));
            }
        }
        KeyCode::Right | KeyCode::Char('d') if !design_locked => {
            let selected_idx = match ui_state.ship_designer_state.installed_subsystems_state.selected() {
                Some(s) => s,
                None => { set_ui_state_to_channel(ui_state); return; }
            };
            if let Some(id) = get_grouped_installed_id(selected_idx) {
                add_subsystem_to_ship(&id);
            }
        }
        _ => { return; }
    }

    set_ui_state_to_channel(ui_state);
}

fn handle_key_down_on_subsystem_tree(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    let mut ui_info = get_ui_info_from_channel();
    let active = ui_state.ship_designer_state.active_panel == ShipDesignerPanel::SubsystemLibrary;

    if !active {
        return;
    }

    let design_locked = with_ship_designer(|sd| sd.current_design.as_ref().map(|d| d.locked).unwrap_or(false));

    match key {
        KeyCode::Up | KeyCode::Char('w') => { ui_info.ship_designer_state.subsystem_tree_state.key_up(); }
        KeyCode::Down | KeyCode::Char('s') => { ui_info.ship_designer_state.subsystem_tree_state.key_down(); }
        KeyCode::Right | KeyCode::Char('d') | KeyCode::Enter => {
            let selected_id = ui_info.ship_designer_state.subsystem_tree_state.selected().last().cloned();
            if let Some(id) = selected_id {
                let is_leaf = with_ship_designer(|sd| sd.subsystem_library.contains_key(&id));
                if is_leaf {
                    if !design_locked { add_subsystem_to_ship(&id); }
                } else {
                    ui_info.ship_designer_state.subsystem_tree_state.key_right();
                }
            }
        }
        KeyCode::Left | KeyCode::Char('a') => {
            let selected_id = ui_info.ship_designer_state.subsystem_tree_state.selected().last().cloned();
            if let Some(id) = selected_id {
                let is_leaf = with_ship_designer(|sd| sd.subsystem_library.contains_key(&id));
                if is_leaf {
                    if !design_locked { remove_subsystem_from_ship(&id); }
                } else {
                    ui_info.ship_designer_state.subsystem_tree_state.key_left();
                }
            }
        }
        _ => { return; }
    };

    ui_state.ship_designer_state.subsystem_tree_state = ui_info.ship_designer_state.subsystem_tree_state.clone();
    set_ui_state_to_channel(ui_state);
}

/// Adds one copy of the library subsystem with `id` to the current ship design.
fn add_subsystem_to_ship(id: &str) {
    let subsystem = with_mut_ship_designer(|sd| sd.subsystem_library.get(id).cloned());
    let Some(subsystem) = subsystem else { return; };
    with_mut_ship_designer(|sd| {
        let Some(ship) = sd.current_design.as_mut() else { return; };
        ship.subsystems.push(subsystem);
    });
}

/// Removes one instance of the subsystem with `id` from the current ship design.
/// Returns the number of remaining unique groups after removal.
fn remove_subsystem_from_ship(id: &str) -> usize {
    with_mut_ship_designer(|sd| {
        let Some(ship) = sd.current_design.as_mut() else { return 0; };
        if let Some(pos) = ship.subsystems.iter().position(|ss| ss.id == id) {
            ship.subsystems.remove(pos);
        }
        let mut unique_ids: Vec<&str> = Vec::new();
        for ss in &ship.subsystems {
            if !unique_ids.contains(&ss.id.as_str()) {
                unique_ids.push(&ss.id);
            }
        }
        unique_ids.len()
    })
}

/// Returns the subsystem id at the given grouped display index from the current ship design.
fn get_grouped_installed_id(idx: usize) -> Option<String> {
    with_mut_ship_designer(|sd| {
        let ship = sd.current_design.as_ref()?;
        let mut unique_ids: Vec<String> = Vec::new();
        for ss in &ship.subsystems {
            if !unique_ids.contains(&ss.id) {
                unique_ids.push(ss.id.clone());
            }
        }
        unique_ids.into_iter().nth(idx)
    })
}

fn handle_key_down_on_ship_designer_menu(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    let mut ui_info = get_ui_info_from_channel();

    if !ui_state.ship_designer_state.menu_popup_open {
        return;
    }

    let item_count = ship_designer_menu_item_count();
    sync_menu_popup_state(&mut ui_info.ship_designer_state.menu_popup_state, item_count);

    match key {
        KeyCode::Esc => {
            close_menu_popup(&mut ui_state);
        }
        KeyCode::Up | KeyCode::Char('w') => {
            ui_info.ship_designer_state.menu_popup_state.select_previous();
            ui_state.ship_designer_state.menu_popup_state = ui_info.ship_designer_state.menu_popup_state.clone();
        }
        KeyCode::Down | KeyCode::Char('s') => {
            ui_info.ship_designer_state.menu_popup_state.select_next();
            ui_state.ship_designer_state.menu_popup_state = ui_info.ship_designer_state.menu_popup_state.clone();
        }
        KeyCode::Enter => {
            let selected = ui_info.ship_designer_state.menu_popup_state.selected().unwrap_or(0);
            ui_state.ship_designer_state.menu_popup_state = ui_info.ship_designer_state.menu_popup_state.clone();
            activate_ship_designer_menu_selection(&mut ui_state, selected);
        }
        _ => return,
    }

    set_ui_state_to_channel(ui_state);
}

pub(super) fn handle_ship_designer_menu_click(pos: (u16, u16)) -> bool {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();

    if ui_state.selected_tab != GameScreenTab::ShipDesigner {
        return false;
    }

    if ui_state.ship_designer_state.rename_buffer.is_some() {
        return false;
    }

    let click_pos = Position::new(pos.0, pos.1);

    if ui_state.ship_designer_state.menu_popup_open {
        if ui_info.ship_designer_menu_button_area.contains(click_pos) {
            close_menu_popup(&mut ui_state);
            set_ui_state_to_channel(ui_state);
            return true;
        }

        if ui_info.ship_designer_menu_popup_area.contains(click_pos) {
            if let Some(visible_idx) = ui_info
                .ship_designer_menu_item_areas
                .iter()
                .position(|rect| rect.contains(click_pos))
            {
                let selected_idx = ui_info.ship_designer_state.menu_popup_state.offset() + visible_idx;
                ui_state.ship_designer_state.menu_popup_state.select(Some(selected_idx));
                activate_ship_designer_menu_selection(&mut ui_state, selected_idx);
            }
            set_ui_state_to_channel(ui_state);
            return true;
        }

        close_menu_popup(&mut ui_state);
        set_ui_state_to_channel(ui_state);
        return true;
    }

    if ui_info.ship_designer_menu_button_area.contains(click_pos) {
        open_menu_popup(&mut ui_state);
        set_ui_state_to_channel(ui_state);
        return true;
    }

    if ui_info.ship_designer_design_tree_area.contains(click_pos) {
        let mut tree_state = ui_info.ship_designer_state.design_tree_state.clone();
        tree_state.click_at(click_pos);
        ui_state.ship_designer_state.active_panel = ShipDesignerPanel::ShipDesigns;
        ui_state.ship_designer_state.design_tree_state = tree_state.clone();

        if let Some(design_id) = tree_state.selected().last().cloned() {
            select_ship_design(&design_id);
            sync_design_tree_selection(&mut ui_state);
        }

        set_ui_state_to_channel(ui_state);
        return true;
    }

    if ui_info.ship_designer_subsystem_tree_area.contains(click_pos) {
        let mut tree_state = ui_info.ship_designer_state.subsystem_tree_state.clone();
        tree_state.click_at(click_pos);
        ui_state.ship_designer_state.active_panel = ShipDesignerPanel::SubsystemLibrary;
        ui_state.ship_designer_state.subsystem_tree_state = tree_state;
        set_ui_state_to_channel(ui_state);
        return true;
    }

    false
}

fn open_menu_popup(ui_state: &mut crate::UIState) {
    ui_state.ship_designer_state.menu_popup_open = true;
    sync_menu_popup_state(
        &mut ui_state.ship_designer_state.menu_popup_state,
        ship_designer_menu_item_count(),
    );
}

fn close_menu_popup(ui_state: &mut crate::UIState) {
    ui_state.ship_designer_state.menu_popup_open = false;
}

fn ship_designer_menu_item_count() -> usize {
    with_ship_designer(|sd| build_ship_designer_menu_entries(&sd.ship_designs).len())
}

fn sync_menu_popup_state(state: &mut ListState, item_count: usize) {
    if item_count == 0 {
        state.select(None);
        *state.offset_mut() = 0;
        return;
    }

    let selected = state.selected().unwrap_or(0).min(item_count - 1);
    let offset = state.offset().min(selected).min(item_count - 1);
    state.select(Some(selected));
    *state.offset_mut() = offset;
}

fn activate_ship_designer_menu_selection(ui_state: &mut crate::UIState, selected_idx: usize) {
    let selected_item = with_ship_designer(|sd| {
        build_ship_designer_menu_entries(&sd.ship_designs)
            .get(selected_idx)
            .map(|entry| entry.item.clone())
    });

    let Some(selected_item) = selected_item else {
        close_menu_popup(ui_state);
        sync_menu_popup_state(
            &mut ui_state.ship_designer_state.menu_popup_state,
            ship_designer_menu_item_count(),
        );
        return;
    };

    with_mut_ship_designer(|designer| {
        let design_locked = designer.current_design.as_ref().map(|d| d.locked).unwrap_or(false);
        match selected_item {
            ShipDesignerMenuItem::NewDesign => {
                designer.current_design = Some(ShipDesign::default());
            }
            ShipDesignerMenuItem::SaveDesign if !design_locked => {
                designer.save_current_design();
            }
            ShipDesignerMenuItem::RenameDesign if !design_locked => {
                let current_name = designer.current_design.as_ref()
                    .map(|s| s.name.clone())
                    .unwrap_or_default();
                ui_state.ship_designer_state.rename_buffer = Some(current_name);
            }
            ShipDesignerMenuItem::DeleteDesign if !design_locked => {
                designer.delete_current_design();
            }
            ShipDesignerMenuItem::LockDesign if !design_locked => {
                designer.lock_current_design();
            }
            _ => {}
        }
    });

    close_menu_popup(ui_state);
    sync_design_tree_selection(ui_state);
    sync_menu_popup_state(
        &mut ui_state.ship_designer_state.menu_popup_state,
        ship_designer_menu_item_count(),
    );
}

fn select_ship_design(design_id: &str) {
    with_mut_ship_designer(|designer| {
        if let Some(design) = designer.ship_designs.iter().find(|design| design.id == design_id).cloned() {
            designer.current_design = Some(design);
        }
    });
}

fn sync_design_tree_selection(ui_state: &mut crate::UIState) {
    let current_design_id = with_ship_designer(|designer| designer.current_design.as_ref().map(|design| design.id.clone()));
    let saved_design_ids = with_ship_designer(|designer| {
        designer.ship_designs.iter().map(|design| design.id.clone()).collect::<Vec<_>>()
    });

    if let Some(current_id) = current_design_id.filter(|id| saved_design_ids.contains(id)) {
        ui_state.ship_designer_state.design_tree_state.select(vec![current_id]);
    } else if ui_state
        .ship_designer_state
        .design_tree_state
        .selected()
        .last()
        .is_some_and(|id| !saved_design_ids.contains(id))
    {
        ui_state.ship_designer_state.design_tree_state.select(vec![]);
    }
}
