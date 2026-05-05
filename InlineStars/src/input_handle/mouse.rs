use std::sync::atomic;

use ratatui::layout::Position;

use crate::{
    GameScreenTab,
    app::ui_state::{self, ContextMenu, ContextMenuAction, ContextMenuEntry, ContextMenuPendingInput, FleetManagerPanel},
    channels::channels::{HAS_JUST_TAB, get_camera_state, get_selected_body_id, get_selected_fleet_id, get_ui_info_from_channel, get_ui_state_from_channel, insert_fleet_order, set_camera, set_selected_body_id, set_selected_fleet_id, set_ui_state_to_channel},
    entities::{GameEntity, fleet::{FleetOrder, FleetOrderType, OrderAddType}},
    get_body_by_id, get_fleets, mouse_pos_in_rect,
};

use super::ship_designer::handle_ship_designer_menu_click;

pub(super) fn handle_mouse_down(mouse_event: crossterm::event::MouseEvent) {
    if mouse_event.kind
        == crossterm::event::MouseEventKind::Down(crossterm::event::MouseButton::Left)
    {
        if handle_context_menu_click((mouse_event.column, mouse_event.row)) {
            return;
        }
        if handle_ship_designer_menu_click((mouse_event.column, mouse_event.row)) {
            return;
        }
        if handle_sidebar_click((mouse_event.column, mouse_event.row)) {
            return;
        }
        if handle_star_map_details_menu_click((mouse_event.column, mouse_event.row)) {
            return;
        }
        handle_object_selection((mouse_event.column, mouse_event.row));
        handle_object_selection_in_system_overview((mouse_event.column, mouse_event.row));
        handle_object_selection_in_system_tree((mouse_event.column, mouse_event.row));
        handle_fleet_tree_click((mouse_event.column, mouse_event.row));
        handle_colonies_tree_click((mouse_event.column, mouse_event.row));
        handle_tab_click(mouse_event.column, mouse_event.row);
        handle_tab_click_in_detail_area((mouse_event.column, mouse_event.row));
        handle_table_selection(mouse_event.column, mouse_event.row);
    }

    if mouse_event.kind == crossterm::event::MouseEventKind::Down(crossterm::event::MouseButton::Right) {
        handle_right_click_on_map((mouse_event.column, mouse_event.row));
    }

    if mouse_event.kind
        == crossterm::event::MouseEventKind::Down(crossterm::event::MouseButton::Middle)
    {
        if let Some(_) = get_selected_body_id() {
            set_selected_body_id(None);
        }
    }
}

pub(super) fn handle_mouse_scroll(mouse_event: crossterm::event::MouseEvent) {
    let ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();
    let pos = Position { x: mouse_event.column, y: mouse_event.row };

    if ui_state.selected_tab == GameScreenTab::Manager {
        handle_mouse_scroll_horizon_text(mouse_event);
    }

    if ui_info.star_map_area.contains(pos) {
        handle_mouse_scroll_star_map(mouse_event);
    }

    if ui_info.sidebar_colonies_content_area.contains(pos) {
        handle_mouse_scroll_sidebar_colonies(mouse_event);
    } else if ui_info.sidebar_planets_content_area.contains(pos) {
        handle_mouse_scroll_sidebar_planets(mouse_event);
    } else if ui_info.sidebar_fleets_content_area.contains(pos) {
        handle_mouse_scroll_sidebar_fleets(mouse_event);
    }
}

fn handle_mouse_scroll_horizon_text(mouse_event: crossterm::event::MouseEvent) {
    let mut ui_state = get_ui_state_from_channel();

    match mouse_event.kind {
        crossterm::event::MouseEventKind::ScrollDown => {
            ui_state.scroll_view_state.scroll_down();
        }
        crossterm::event::MouseEventKind::ScrollUp => {
            ui_state.scroll_view_state.scroll_up();
        }
        _ => {}
    }

    set_ui_state_to_channel(ui_state);
}

fn handle_mouse_scroll_star_map(mouse_event: crossterm::event::MouseEvent) {
    let mut camera = get_camera_state().as_ref().clone();
    match mouse_event.kind {
        crossterm::event::MouseEventKind::ScrollDown => {
            camera.zoom(0.9);
        }
        crossterm::event::MouseEventKind::ScrollUp => {
            camera.zoom(1.1);
        }
        _ => {}
    }

    camera.update(0.0);
    set_camera(camera);
}

fn handle_mouse_scroll_sidebar_colonies(mouse_event: crossterm::event::MouseEvent) {
    let mut ui_state = get_ui_state_from_channel();
    match mouse_event.kind {
        crossterm::event::MouseEventKind::ScrollDown => {
            ui_state.colonies_list_state.scroll_down(1);
        }
        crossterm::event::MouseEventKind::ScrollUp => {
            ui_state.colonies_list_state.scroll_up(1);
        }
        _ => {}
    }
    set_ui_state_to_channel(ui_state);
}

fn handle_mouse_scroll_sidebar_planets(mouse_event: crossterm::event::MouseEvent) {
    let mut ui_state = get_ui_state_from_channel();
    match mouse_event.kind {
        crossterm::event::MouseEventKind::ScrollDown => {
            ui_state.system_tree_state.scroll_down(1);
        }
        crossterm::event::MouseEventKind::ScrollUp => {
            ui_state.system_tree_state.scroll_up(1);
        }
        _ => {}
    }
    set_ui_state_to_channel(ui_state);
}

fn handle_mouse_scroll_sidebar_fleets(mouse_event: crossterm::event::MouseEvent) {
    let mut ui_state = get_ui_state_from_channel();
    match mouse_event.kind {
        crossterm::event::MouseEventKind::ScrollDown => {
            ui_state.fleets_tree_state.scroll_down(1);
        }
        crossterm::event::MouseEventKind::ScrollUp => {
            ui_state.fleets_tree_state.scroll_up(1);
        }
        _ => {}
    }
    set_ui_state_to_channel(ui_state);
}

/// Opens a context menu at `pos` when a fleet is selected and the click is on the star map.
/// Detects nearby bodies/fleets to offer targeted orders; falls back to move-to-position.
fn handle_right_click_on_map(pos: (u16, u16)) {
    let ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();

    if ui_state.selected_screen != crate::UIScreen::Game { return; }
    if ui_state.selected_tab != GameScreenTab::SystemView { return; }

    let fleet_id = match get_selected_fleet_id() { Some(id) => id, None => return };
    let fleet = match crate::get_fleet_by_id(fleet_id.clone()) { Some(f) => f, None => return };

    if !ui_info.star_map_area.contains(ratatui::layout::Position { x: pos.0, y: pos.1 }) { return; }

    let camera = get_camera_state();
    let area_pos = mouse_pos_in_rect(pos, ui_info.star_map_area);
    let world_pos = camera.screen_to_world_coordinates(area_pos);

    // Check if the click lands near a known entity (body or fleet), excluding the selected fleet.
    let hit_id = ui_info.star_map_info.iter()
        .filter(|(id, _)| **id != fleet_id)
        .find(|(_, screen_pos)| {
            pos.0.abs_diff(screen_pos.x) <= 2 && pos.1.abs_diff(screen_pos.y) <= 1
        })
        .map(|(id, _)| id.clone());

    let available = fleet.available_orders();

    let entries: Vec<ContextMenuEntry> = if let Some(target_id) = hit_id {
        available.iter()
            .filter(|o| o.needs_object())
            .map(|o| ContextMenuEntry {
                label: o.label().to_string(),
                action: match o {
                    FleetOrderType::MoveToObject(_) =>
                        ContextMenuAction::MoveToObject(target_id.clone()),
                    FleetOrderType::KeepDistanceToObject(_, _) =>
                        ContextMenuAction::KeepDistanceToObject(target_id.clone(), 149_597_870.7),
                    _ => unreachable!(),
                },
            })
            .collect()
    } else {
        available.iter()
            .filter(|o| o.needs_position())
            .map(|o| ContextMenuEntry {
                label: o.label().to_string(),
                action: ContextMenuAction::MoveToPosition(world_pos.0, world_pos.1),
            })
            .collect()
    };

    if entries.is_empty() { return; }

    let mut ui_state = get_ui_state_from_channel();
    ui_state.context_menu = ContextMenu { visible: true, screen_pos: pos, entries, pending_input: None };
    set_ui_state_to_channel(ui_state);
}

/// Handles a left-click when a context menu is open.
/// Returns `true` if the event was consumed (menu was visible).
fn handle_context_menu_click(pos: (u16, u16)) -> bool {
    let ui_state = get_ui_state_from_channel();
    if !ui_state.context_menu.visible { return false; }

    // While in pending input mode, block all clicks (keyboard only)
    if ui_state.context_menu.pending_input.is_some() { return true; }

    let ui_info = get_ui_info_from_channel();
    let click_pos = ratatui::layout::Position { x: pos.0, y: pos.1 };

    let hit_idx = ui_info.context_menu_option_areas.iter()
        .position(|rect| rect.contains(click_pos));

    if let Some(idx) = hit_idx {
        let fleet_id = match get_selected_fleet_id() { Some(id) => id, None => {
            let mut s = get_ui_state_from_channel();
            s.context_menu.visible = false;
            set_ui_state_to_channel(s);
            return true;
        }};

        match ui_state.context_menu.entries[idx].action.clone() {
            ContextMenuAction::MoveToPosition(x, y) => {
                insert_fleet_order(FleetOrder { fleet_id, add_type: OrderAddType::Replace, order: FleetOrderType::MoveToPosition((x, y)) });
                let mut s = get_ui_state_from_channel();
                s.context_menu.visible = false;
                set_ui_state_to_channel(s);
            }
            ContextMenuAction::MoveToObject(id) => {
                insert_fleet_order(FleetOrder { fleet_id, add_type: OrderAddType::Replace, order: FleetOrderType::MoveToObject(id) });
                let mut s = get_ui_state_from_channel();
                s.context_menu.visible = false;
                set_ui_state_to_channel(s);
            }
            ContextMenuAction::KeepDistanceToObject(target_id, _) => {
                // Transition to distance input — don't close menu yet
                let mut s = get_ui_state_from_channel();
                s.context_menu.pending_input = Some(ContextMenuPendingInput {
                    prompt: "Distance (km  or  N au)".to_string(),
                    value: "1 au".to_string(),
                    target_id,
                });
                set_ui_state_to_channel(s);
            }
        }
    } else {
        // Click outside options — close
        let mut s = get_ui_state_from_channel();
        s.context_menu.visible = false;
        set_ui_state_to_channel(s);
    }

    true
}

/// Handles clicks on sidebar interactive elements (toggle, side-switch, section headers).
/// Returns `true` if the click was consumed.
fn handle_sidebar_click(pos: (u16, u16)) -> bool {
    let ui_state = get_ui_state_from_channel();
    if ui_state.selected_screen != crate::UIScreen::Game {
        return false;
    }
    if ui_state.selected_tab != GameScreenTab::SystemView
        && ui_state.selected_tab != GameScreenTab::Manager
    {
        return false;
    }

    let ui_info = get_ui_info_from_channel();
    let click = Position { x: pos.0, y: pos.1 };

    // Side-switch button takes priority over the toggle area
    if !ui_info.sidebar_side_button_area.is_empty()
        && ui_info.sidebar_side_button_area.contains(click)
    {
        let mut s = get_ui_state_from_channel();
        s.sidebar_side = match s.sidebar_side {
            ui_state::SidebarSide::Left => ui_state::SidebarSide::Right,
            ui_state::SidebarSide::Right => ui_state::SidebarSide::Left,
        };
        set_ui_state_to_channel(s);
        return true;
    }

    if !ui_info.sidebar_toggle_area.is_empty() && ui_info.sidebar_toggle_area.contains(click) {
        let mut s = get_ui_state_from_channel();
        s.sidebar_collapsed = !s.sidebar_collapsed;
        set_ui_state_to_channel(s);
        return true;
    }

    if !ui_info.sidebar_colonies_header_area.is_empty()
        && ui_info.sidebar_colonies_header_area.contains(click)
    {
        let mut s = get_ui_state_from_channel();
        s.sidebar_colonies_collapsed = !s.sidebar_colonies_collapsed;
        set_ui_state_to_channel(s);
        return true;
    }

    if !ui_info.sidebar_planets_header_area.is_empty()
        && ui_info.sidebar_planets_header_area.contains(click)
    {
        let mut s = get_ui_state_from_channel();
        s.sidebar_planets_collapsed = !s.sidebar_planets_collapsed;
        set_ui_state_to_channel(s);
        return true;
    }

    if !ui_info.sidebar_fleets_header_area.is_empty()
        && ui_info.sidebar_fleets_header_area.contains(click)
    {
        let mut s = get_ui_state_from_channel();
        s.sidebar_fleets_collapsed = !s.sidebar_fleets_collapsed;
        set_ui_state_to_channel(s);
        return true;
    }

    false
}

fn handle_star_map_details_menu_click(pos: (u16, u16)) -> bool {
    let ui_state = get_ui_state_from_channel();
    if ui_state.selected_screen != crate::UIScreen::Game {
        return false;
    }
    if ui_state.selected_tab != GameScreenTab::SystemView {
        return false;
    }

    let ui_info = get_ui_info_from_channel();
    let click = Position { x: pos.0, y: pos.1 };

    if !ui_info.star_map_details_toggle_area.is_empty()
        && ui_info.star_map_details_toggle_area.contains(click)
    {
        let mut s = get_ui_state_from_channel();
        s.toggle_star_map_details_menu();
        set_ui_state_to_channel(s);
        return true;
    }

    if let Some(index) = ui_info
        .star_map_details_option_areas
        .iter()
        .position(|rect| rect.contains(click))
    {
        let mut s = get_ui_state_from_channel();
        if s.toggle_star_map_detail_by_index(index) {
            set_ui_state_to_channel(s);
            return true;
        }
    }

    if !ui_info.star_map_details_filter_area.is_empty()
        && ui_info.star_map_details_filter_area.contains(click)
    {
        let mut s = get_ui_state_from_channel();
        s.activate_star_map_filter();
        set_ui_state_to_channel(s);
        return true;
    }

    if ui_state.star_map_filter_editing {
        let mut s = get_ui_state_from_channel();
        s.star_map_filter_editing = false;
        set_ui_state_to_channel(s);
    }

    false
}

fn handle_object_selection_in_system_tree(pos: (u16, u16)) {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();

    if ui_state.selected_screen != crate::UIScreen::Game {
        return;
    }

    if ui_state.selected_tab != GameScreenTab::SystemView
        && ui_state.selected_tab != GameScreenTab::Manager
    {
        return;
    }

    if !ui_info
        .sidebar_planets_content_area
        .contains(Position { x: pos.0, y: pos.1 })
    {
        return;
    }

    let mut tree_state = ui_info.system_tree_state;
    tree_state.click_at(Position { x: pos.0, y: pos.1 });

    let selected = tree_state.selected().last().cloned();
    if selected.is_some() {
        ui_state.colonies_list_state.select(Vec::new());
    }
    set_selected_body_id(selected);

    ui_state.system_tree_state = tree_state;
    set_ui_state_to_channel(ui_state);
}

fn handle_fleet_tree_click(pos: (u16, u16)) {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();

    if ui_state.selected_screen != crate::UIScreen::Game {
        return;
    }

    if ui_state.selected_tab != GameScreenTab::SystemView
        && ui_state.selected_tab != GameScreenTab::Manager
    {
        return;
    }

    if !ui_info
        .sidebar_fleets_content_area
        .contains(Position { x: pos.0, y: pos.1 })
    {
        return;
    }

    let mut tree_state = ui_info.fleets_tree_state;
    tree_state.click_at(Position { x: pos.0, y: pos.1 });

    if let Some(fleet_id) = tree_state.selected().last().cloned() {
        set_selected_fleet_id(Some(fleet_id));
        set_selected_body_id(None);
    }

    ui_state.fleets_tree_state = tree_state;
    set_ui_state_to_channel(ui_state);
}

fn handle_colonies_tree_click(pos: (u16, u16)) {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();

    if ui_state.selected_screen != crate::UIScreen::Game {
        return;
    }

    if ui_state.selected_tab != GameScreenTab::SystemView
        && ui_state.selected_tab != GameScreenTab::Manager
    {
        return;
    }

    if !ui_info
        .sidebar_colonies_content_area
        .contains(Position { x: pos.0, y: pos.1 })
    {
        return;
    }

    let mut tree_state = ui_info.colonies_tree_state;
    tree_state.click_at(Position { x: pos.0, y: pos.1 });

    if let Some(body_id) = tree_state.selected().last().cloned() {
        set_selected_body_id(Some(body_id));
        set_selected_fleet_id(None);
        ui_state.system_tree_state.select(Vec::new());
    }

    ui_state.colonies_list_state = tree_state;
    set_ui_state_to_channel(ui_state);
}

fn handle_tab_click_in_detail_area(pos: (u16, u16)) {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();
    let mouse_pos = Position::new(pos.0, pos.1);

    //Find hit tab
    let hit = ui_info.detail_tab_areas.iter().find(|entry| entry.1.contains(mouse_pos));
    let hit = match hit {
        Some(hit) => hit,
        None => return,
    };

    ui_state.selected_detail_tab = match hit.0 {
        0 => ui_state::SelectedDetailTab::Overview,
        1 => ui_state::SelectedDetailTab::TreeView,
        2 => ui_state::SelectedDetailTab::Fleets,
        _ => unreachable!("Invalid tab index"),
    };
    
    set_ui_state_to_channel(ui_state);
}

fn handle_table_selection(column: u16, row: u16) {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();
    let pos = Position::new(column, row);

    if !(ui_state.selected_screen == crate::UIScreen::Game) {
        return;
    }

    if ui_state.selected_tab != GameScreenTab::Manager {
        return;
    }

    // Fleet manager mouse clicks
    if get_selected_fleet_id().is_some() {
        if let Some(hit) = ui_info.fleet_manager_info.order_cells.iter().find(|c| c.rect.contains(pos)) {
            ui_state.fleet_manager_state.order_queue_state.select_by_index(hit.row);
            ui_state.fleet_manager_state.active_panel = FleetManagerPanel::OrderQueue;
            set_ui_state_to_channel(ui_state);
            return;
        }
        if let Some(hit) = ui_info.fleet_manager_info.ships_cells.iter().find(|c| c.rect.contains(pos)) {
            ui_state.fleet_manager_state.ships_state.select_by_index(hit.row);
            ui_state.fleet_manager_state.active_panel = FleetManagerPanel::Ships;
            set_ui_state_to_channel(ui_state);
            return;
        }
        set_ui_state_to_channel(ui_state);
        return;
    }

    // Check table cells for hit (priority: queue -> build options -> finished).
    let queue_hit = ui_info
        .colony_manager_info
        .queue_cells
        .iter()
        .find(|c| c.rect.contains(pos));

    let build_hit = ui_info
        .colony_manager_info
        .build_options_cells
        .iter()
        .find(|c| c.rect.contains(pos));

    let finished_hit = ui_info
        .colony_manager_info
        .finished_cells
        .iter()
        .find(|c| c.rect.contains(pos));

    match (queue_hit, build_hit, finished_hit) {
        (Some(hit), _, _) => {
            ui_state.colony_manager_state.queue_state.select_by_index(hit.row);
            ui_state.colony_manager_state.build_options_state.deselect();
            ui_state.colony_manager_state.finished_state.deselect();
            ui_state.colony_manager_state.selected_panel = crate::renderers::colony_manager_renderer::ColonyMangerPanel::Queue;
        }
        (None, Some(hit), _) => {
            ui_state.colony_manager_state.queue_state.deselect();
            ui_state.colony_manager_state.build_options_state.select_by_index(hit.row);
            ui_state.colony_manager_state.finished_state.deselect();
            ui_state.colony_manager_state.selected_panel = crate::renderers::colony_manager_renderer::ColonyMangerPanel::BuildOptions;
        }
        (None, None, Some(hit)) => {
            ui_state.colony_manager_state.queue_state.deselect();
            ui_state.colony_manager_state.build_options_state.deselect();
            ui_state.colony_manager_state.finished_state.select_by_index(hit.row);
            ui_state.colony_manager_state.selected_panel = crate::renderers::colony_manager_renderer::ColonyMangerPanel::Finished;
        }
        (None, None, None) => {
            ui_state.colony_manager_state.queue_state.deselect();
            ui_state.colony_manager_state.build_options_state.deselect();
            ui_state.colony_manager_state.finished_state.deselect();
        }
    }

    // Check for tab bar click — switches active tab
    let tab_hit = ui_info
        .colony_manager_info
        .tab_areas
        .iter()
        .find(|(_, rect)| rect.contains(pos));

    if let Some((index, _)) = tab_hit {
        ui_state.colony_manager_state.active_tab = match index {
            0 => crate::renderers::colony_manager_renderer::ColonyManagerTab::Buildings,
            _ => crate::renderers::colony_manager_renderer::ColonyManagerTab::Shipyards,
        };
        set_ui_state_to_channel(ui_state);
        return;
    }

    // Handle slipway mouse clicks when on the Shipyards tab
    if ui_state.colony_manager_state.active_tab == crate::renderers::colony_manager_renderer::ColonyManagerTab::Shipyards {
        // When in retooling mode, clicks on retool_design_cells select a design
        if ui_state.colony_manager_state.retooling {
            let retool_hit = ui_info
                .colony_manager_info
                .retool_design_cells
                .iter()
                .find(|c| c.rect.contains(pos));

            if let Some(hit) = retool_hit {
                ui_state.colony_manager_state.retool_design_state.select_by_index(hit.row);
                set_ui_state_to_channel(ui_state);
                return;
            }
        }

        let slipway_hit = ui_info
            .colony_manager_info
            .slipways_cells
            .iter()
            .find(|c| c.rect.contains(pos));

        if let Some(hit) = slipway_hit {
            ui_state.colony_manager_state.slipways_state.select_by_index(hit.row);
        }
    }

    set_ui_state_to_channel(ui_state);
}

fn handle_tab_click(column: u16, row: u16) {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();
    let mouse_pos = Position::new(column, row);

    //Find hit tab
    let hit = ui_info.tab_areas.iter().find(|entry| entry.1.contains(mouse_pos));
    let hit = match hit {
        Some(hit) => hit,
        None => return,
    };

    ui_state.selected_tab = match hit.0 {
        0 => GameScreenTab::SystemView,
        1 => GameScreenTab::Manager,
        2 => GameScreenTab::ShipDesigner,
        3 => GameScreenTab::SubsystemDesigner,
        4 => GameScreenTab::Research,
        _ => unreachable!("Invalid tab index {}", hit.0),
    };

    HAS_JUST_TAB.store(true, atomic::Ordering::Relaxed);
    
    set_ui_state_to_channel(ui_state);
}

fn handle_object_selection_in_system_overview(mouse_pos: (u16, u16)) {
    let ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();

    if ui_state.selected_screen != crate::UIScreen::Game {
        return;
    }

    if ui_state.selected_tab != GameScreenTab::SystemView
        && ui_state.selected_tab != GameScreenTab::Manager
    {
        return;
    }

    if ui_state.selected_detail_tab != ui_state::SelectedDetailTab::Overview {
        return;
    }

    if !ui_info.detail_area.contains(Position::new(mouse_pos.0, mouse_pos.1)){
        return;
    }

    let uuid = ui_info.system_overview_info.iter().find_map(|(uuid, rect)| {
        if rect.contains(Position { x: mouse_pos.0, y: mouse_pos.1 }) {
            Some(uuid.clone())
        } else {
            None
        }
    });

    if uuid.is_some() {
        let mut next_ui_state = ui_state.clone();
        next_ui_state.colonies_list_state.select(Vec::new());
        set_ui_state_to_channel(next_ui_state);
    }
    set_selected_body_id(uuid);
}

fn handle_object_selection(mouse_pos: (u16, u16)) {
    let mut ui_state = get_ui_state_from_channel();
    let ui_info = get_ui_info_from_channel();

    if ui_state.selected_screen != crate::UIScreen::Game {
        return;
    }

    if ui_state.selected_tab != GameScreenTab::SystemView {
        return;
    }

    if !ui_info.star_map_area.contains(Position { x: mouse_pos.0, y: mouse_pos.1 }){
        return;
    }

    set_selected_fleet_id(None);
    set_selected_body_id(None);
    ui_state.system_tree_state.select(Vec::new());
    ui_state.colonies_list_state.select(Vec::new());
    set_ui_state_to_channel(ui_state.clone());

    ui_info.star_map_info.iter().for_each(|(uuid, pos)| {
        let dif_x = (mouse_pos.0 as i16).saturating_sub(pos.x as i16);
        let dif_y = (mouse_pos.1 as i16).saturating_sub(pos.y as i16);

        let distance = (((dif_x as u32).saturating_mul(dif_x as u32).saturating_add((dif_y as u32).saturating_mul(dif_y as u32))) as f64).sqrt();

        if distance > 10.0{
            return;
        }

        if(get_body_by_id(uuid.clone())).is_some(){
            set_selected_body_id(Some(uuid.clone()));
            set_selected_fleet_id(None);
            ui_state.system_tree_state.select(vec![uuid.clone()]);
            ui_state.colonies_list_state.select(Vec::new());
            set_ui_state_to_channel(ui_state.clone());
            return;
        }

        if(get_fleets().iter().find(|f| f.id == *uuid && !f.slipway_fleet)).is_some(){
            set_selected_body_id(None);
            set_selected_fleet_id(Some(uuid.clone()));
            set_ui_state_to_channel(ui_state.clone());
            return;
        }
    });
}

/// Parses a distance string. Supports:
/// - `"1.5 au"` / `"1.5AU"` → kilometres (×149,597,870.7)
/// - `"1000000"` → raw kilometres
pub(super) fn parse_distance(input: &str) -> Option<f64> {
    let s = input.trim().to_lowercase();
    if s.ends_with("au") {
        s[..s.len() - 2].trim().parse::<f64>().ok().map(|v| v * 149_597_870.7)
    } else {
        s.parse::<f64>().ok()
    }
}
