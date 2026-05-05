use std::sync::atomic;

use crossterm::event::{KeyCode, KeyModifiers};

use crate::{
    GameScreenTab,
    app::ui_state::{self, SaveLoadPopup, SidebarFocus},
    channels::channels::{HAS_JUST_TAB, get_selected_fleet_id, get_ui_state_from_channel, set_ui_state_to_channel},
};

use super::common::INPUT_STATE;
use super::game_window::{handle_key_down_on_colonies, handle_key_down_on_game_window, handle_key_down_on_fleets, handle_key_down_on_system_tree_view};
use super::ship_designer::handle_key_down_on_ship_designer;
use super::subsystem_designer::handle_key_down_on_subsystem_designer;
use super::research::handle_key_down_on_research;
use super::fleet_manager::handle_key_down_on_fleet_manager;
use super::colony_manager::handle_key_down_on_colony_manager;

pub(super) fn handle_key_press(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    let mods: KeyModifiers = INPUT_STATE.with_borrow(|key_event| key_event.modifiers);

    // Save/load popup captures all input when visible
    if ui_state.save_load_popup != SaveLoadPopup::Hidden {
        handle_save_load_popup_key(key, &mut ui_state);
        set_ui_state_to_channel(ui_state);
        return;
    }

    // Ctrl+S / Ctrl+L: open save/load popup (game screen only)
    if mods.contains(KeyModifiers::CONTROL) && ui_state.selected_screen == crate::UIScreen::Game {
        match key {
            KeyCode::Char('s') => {
                let name = format!("quicksave-{}", crate::now_timestamp());
                ui_state.save_load_popup = SaveLoadPopup::Save { name };
                set_ui_state_to_channel(ui_state);
                return;
            }
            KeyCode::Char('l') => {
                let saves = crate::list_saves();
                ui_state.save_load_popup = SaveLoadPopup::Load { saves, selected: 0 };
                set_ui_state_to_channel(ui_state);
                return;
            }
            _ => {}
        }
    }

    //Handle tabing between tabs on the game screen:
    if ui_state.selected_screen == crate::UIScreen::Game && !mods.contains(KeyModifiers::SHIFT) {
        match key {
            KeyCode::Tab => {
                ui_state.selected_tab = ui_state.selected_tab.next();
                set_ui_state_to_channel(ui_state);
                HAS_JUST_TAB.store(true, atomic::Ordering::Relaxed);
                return;
            }
            KeyCode::BackTab => {
                ui_state.selected_tab = ui_state.selected_tab.previous();
                set_ui_state_to_channel(ui_state);
                HAS_JUST_TAB.store(true, atomic::Ordering::Relaxed);
                return;
            }
            _ => {}
        }
    }

    // Global sidebar controls (active on all game tabs)
    if ui_state.selected_screen == crate::UIScreen::Game {
        match key {
            KeyCode::Char('B') => {
                ui_state.sidebar_collapsed = !ui_state.sidebar_collapsed;
                set_ui_state_to_channel(ui_state);
                return;
            }
            KeyCode::Char('\\') => {
                ui_state.sidebar_side = match ui_state.sidebar_side {
                    ui_state::SidebarSide::Left => ui_state::SidebarSide::Right,
                    ui_state::SidebarSide::Right => ui_state::SidebarSide::Left,
                };
                set_ui_state_to_channel(ui_state);
                return;
            }
            KeyCode::Char('[') => {
                ui_state.sidebar_planets_collapsed = !ui_state.sidebar_planets_collapsed;
                set_ui_state_to_channel(ui_state);
                return;
            }
            KeyCode::Char(']') => {
                ui_state.sidebar_fleets_collapsed = !ui_state.sidebar_fleets_collapsed;
                set_ui_state_to_channel(ui_state);
                return;
            }
            KeyCode::Char('{') => {
                ui_state.sidebar_colonies_collapsed = !ui_state.sidebar_colonies_collapsed;
                set_ui_state_to_channel(ui_state);
                return;
            }
            _ => {}
        }
    }

    match ui_state.selected_screen {
        crate::UIScreen::Splash => {
            ui_state.selected_screen = crate::UIScreen::MainMenu;
            if ui_state.main_menu_state.selected().is_none() {
                ui_state.main_menu_state.select(Some(0));
            }
            set_ui_state_to_channel(ui_state);
            match key {
                KeyCode::Char('w') | KeyCode::Up | KeyCode::Char('s') | KeyCode::Down | KeyCode::Enter => {
                    handle_main_menu_key_press(key);
                }
                _ => {}
            }
        }
        crate::UIScreen::MainMenu => {handle_main_menu_key_press(key);}
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::SystemView && ui_state.sidebar_focus == SidebarFocus::Colonies => {handle_key_down_on_game_window(key); handle_key_down_on_colonies(key);}
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::SystemView && ui_state.sidebar_focus == SidebarFocus::Planets => {handle_key_down_on_game_window(key); handle_key_down_on_system_tree_view(key);}
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::SystemView && ui_state.sidebar_focus == SidebarFocus::Fleets => {handle_key_down_on_game_window(key); handle_key_down_on_fleets(key);}
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::Manager
            && get_selected_fleet_id().is_some() => { handle_key_down_on_fleet_manager(key); }
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::Manager => {handle_key_down_on_colony_manager(key);}
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::ShipDesigner => {handle_key_down_on_ship_designer(key);}
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::Research => {handle_key_down_on_research(key);}
        crate::UIScreen::Game if ui_state.selected_tab == GameScreenTab::SubsystemDesigner => {handle_key_down_on_subsystem_designer(key);}
        _ => {}
    }
}

fn handle_save_load_popup_key(key: KeyCode, ui_state: &mut crate::app::ui_state::UIState) {
    match &mut ui_state.save_load_popup {
        SaveLoadPopup::Save { name } => {
            match key {
                KeyCode::Esc => {
                    ui_state.save_load_popup = SaveLoadPopup::Hidden;
                }
                KeyCode::Enter => {
                    let name = name.clone();
                    if !name.is_empty() {
                        crate::save_game(name);
                    }
                    ui_state.save_load_popup = SaveLoadPopup::Hidden;
                }
                KeyCode::Backspace => {
                    name.pop();
                }
                KeyCode::Char(c) => {
                    // Allow printable ASCII except path separators
                    if c.is_ascii() && !matches!(c, '/' | '\\' | ':' | '*' | '?' | '"' | '<' | '>' | '|') {
                        name.push(c);
                    }
                }
                _ => {}
            }
        }
        SaveLoadPopup::Load { saves, selected } => {
            let len = saves.len();
            match key {
                KeyCode::Esc => {
                    ui_state.save_load_popup = SaveLoadPopup::Hidden;
                }
                KeyCode::Up | KeyCode::Char('w') => {
                    if len > 0 {
                        *selected = (*selected + len - 1) % len;
                    }
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    if len > 0 {
                        *selected = (*selected + 1) % len;
                    }
                }
                KeyCode::Enter => {
                    if len > 0 {
                        let name = saves[*selected].clone();
                        let from_main_menu = ui_state.selected_screen != crate::UIScreen::Game;
                        crate::load_game(name);
                        if from_main_menu {
                            ui_state.selected_screen = crate::UIScreen::Game;
                        }
                    }
                    ui_state.save_load_popup = SaveLoadPopup::Hidden;
                }
                _ => {}
            }
        }
        SaveLoadPopup::Hidden => {}
    }
}

fn handle_main_menu_key_press(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();

    match key {
        KeyCode::Char('w') | KeyCode::Up => {
            ui_state.main_menu_state.select_previous();
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Char('s') | KeyCode::Down => {
            ui_state.main_menu_state.select_next();
            set_ui_state_to_channel(ui_state);
        }

        KeyCode::Enter if ui_state.main_menu_state.selected() == Some(0) => {
            //new_game();
            ui_state.selected_screen = crate::UIScreen::Game;
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Enter if ui_state.main_menu_state.selected() == Some(1) => {
            let saves = crate::list_saves();
            ui_state.save_load_popup = SaveLoadPopup::Load { saves, selected: 0 };
            set_ui_state_to_channel(ui_state);
        }
        KeyCode::Enter if ui_state.main_menu_state.selected() == Some(2) => {
            //Open_settings();
        }
        KeyCode::Enter if ui_state.main_menu_state.selected() == Some(3) => {
            std::process::exit(0);
        }
        _ => {}
    }
}
