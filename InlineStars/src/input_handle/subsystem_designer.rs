use crossterm::event::KeyCode;

use crate::{
    app::{
        ship_desginer::with_mut_ship_designer,
        ui_state::SubSystemDesignerPanel,
    },
    channels::channels::{get_ui_info_from_channel, get_ui_state_from_channel, set_ui_state_to_channel},
    entities::ship::{Engine, Sensor, SubsystemType, WeaponSystem},
};

pub(super) fn handle_key_down_on_subsystem_designer(key: KeyCode) {
    let mut tree_state = get_ui_info_from_channel().subsystem_renderer_info.subsystem_tree_state.clone();
    let mut engine_tech_tree_state = get_ui_info_from_channel().subsystem_renderer_info.engine_tech_tree_state.clone();
    let mut panel_state = get_ui_state_from_channel().subsystem_renderer_state.panel_state.clone();
    let active_panel = panel_state.active_panel();

    let selected = tree_state.selected().last().and_then(|key| {
        with_mut_ship_designer(|f| f.subsystem_library.get(key).cloned())
    });

    let mut selected_category = selected.as_ref().and_then(|s| Some(s.subsystem_type.clone()));

    match tree_state.selected().last() {
        Some(id) => match id.as_str() {
            "reactors" => {selected_category = Some(SubsystemType::Reactor);},
            "engines" => {selected_category = Some(SubsystemType::Engines(Engine::default()));},
            "weapons" => {selected_category = Some(SubsystemType::Weapons(WeaponSystem::default()));},
            "sensors" => {selected_category = Some(SubsystemType::Sensors(Sensor::default()));},
            _ => {},
        },
        None => {},
    }

    match key {
        KeyCode::Char('A') if selected_category.is_some() => {
            let id = with_mut_ship_designer(|f| f.new_sub_system_from_ui(selected_category.unwrap()));
            tree_state.select(vec![id]);
        }
        KeyCode::Char('E') if selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {

        }
        KeyCode::Char('D') if selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {

        }
        KeyCode::Char('O') if selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {

        }
        KeyCode::Char('L') if selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {
            with_mut_ship_designer(|designer| {
                designer.lock_subsystem(&selected.as_ref().unwrap().id);
            });
        }

        KeyCode::Left | KeyCode::Char('a') if active_panel == SubSystemDesignerPanel::SubsystemLibrary => {
            tree_state.key_left();
        }
        KeyCode::Right | KeyCode::Char('d') if active_panel == SubSystemDesignerPanel::SubsystemLibrary => {
            tree_state.key_right();
        }
        KeyCode::Up | KeyCode::Char('w') if active_panel == SubSystemDesignerPanel::SubsystemLibrary => {
            tree_state.key_up();
        }
        KeyCode::Down | KeyCode::Char('s') if active_panel == SubSystemDesignerPanel::SubsystemLibrary => {
            tree_state.key_down();
        }
        KeyCode::Up | KeyCode::Char('w') if active_panel == SubSystemDesignerPanel::EngineTechs && selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {
            engine_tech_tree_state.key_up();
            engine_tech_tree_state.selected().last().cloned().and_then(|key| {
                with_mut_ship_designer(|designer| {
                    designer.update_subsystem_engine_tech_from_ui(selected.unwrap().id.clone(), key);
                });
                Some(())
            });
        }
        KeyCode::Down | KeyCode::Char('s') if active_panel == SubSystemDesignerPanel::EngineTechs && selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {
            engine_tech_tree_state.key_down();
            engine_tech_tree_state.selected().last().cloned().and_then(|key| {
                with_mut_ship_designer(|designer| {
                    designer.update_subsystem_engine_tech_from_ui(selected.unwrap().id.clone(), key);
                });
                Some(())
            });
        }
        KeyCode::Up if active_panel == SubSystemDesignerPanel::EngineSizes && selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {
            with_mut_ship_designer(|designer| {
                designer.update_subsystem_mass_from_ui(selected.as_ref().unwrap().id.clone(), 1000.0);
            });
        }
        
        KeyCode::Down if active_panel == SubSystemDesignerPanel::EngineSizes && selected.as_ref().map(|s| !s.locked).unwrap_or(false) => {
            with_mut_ship_designer(|designer| {
                designer.update_subsystem_mass_from_ui(selected.as_ref().unwrap().id.clone(), -1000.0);
            });
        }
        KeyCode::Char('q') if selected.is_some() => {
            panel_state.previous_panel();
        }
        KeyCode::Char('e') if selected.is_some() => {
            panel_state.next_panel();
        }
        _ => {}
    }

    //Update the active panels:
    let selected = tree_state.selected().last().and_then(|key| {
        with_mut_ship_designer(|f| f.subsystem_library.get(key).cloned())
    });

    match selected {
        Some(selected) => {
            panel_state.update_available_panels_for_subsystem_type(&selected.subsystem_type);
        },
        None => {},
    }
    
    let mut ui_state = get_ui_state_from_channel();
    ui_state.subsystem_renderer_state.subsystem_tree_state = tree_state;
    ui_state.subsystem_renderer_state.engine_tech_tree_state = engine_tech_tree_state;
    ui_state.subsystem_renderer_state.panel_state = panel_state;
    set_ui_state_to_channel(ui_state);
}
