use crossterm::event::KeyCode;

use crate::{
    app::{
        science_manager::with_mut_science_manager,
        ui_state::ScienceRendererPanel,
    },
    channels::channels::{get_ui_info_from_channel, get_ui_state_from_channel, set_ui_state_to_channel},
};

pub(super) fn handle_key_down_on_research(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    match key {
        KeyCode::Char('q') | KeyCode::Char('Q') => {
            ui_state.science_renderer_state.active_panel = ui_state.science_renderer_state.active_panel.previous();
            set_ui_state_to_channel(ui_state.clone());
        }
        KeyCode::Char('e') | KeyCode::Char('E') => {
            ui_state.science_renderer_state.active_panel = ui_state.science_renderer_state.active_panel.next();
            set_ui_state_to_channel(ui_state.clone());
        }
        _ => {}
    }

    match ui_state.science_renderer_state.active_panel {
        ScienceRendererPanel::ResearchList => handle_key_down_on_research_list(key),
        ScienceRendererPanel::ResearchQueue => handle_key_down_on_research_queue(key),
    }
}

fn handle_key_down_on_research_queue(key: KeyCode) {
    let mut queue_state = get_ui_info_from_channel();
    let mut ui_state = get_ui_state_from_channel();
    match key{
        KeyCode::Up | KeyCode::Char('w') => { queue_state.science_renderer_info.research_queue.key_up(); },
        KeyCode::Down | KeyCode::Char('s') => { queue_state.science_renderer_info.research_queue.key_down(); },
        KeyCode::Backspace => {
            let selected = queue_state.science_renderer_info.research_queue.selected().last().cloned();
            match selected {
                Some(s) => {
                    with_mut_science_manager(|m| m.remove_from_research_queue(s));
                },
                None => {},
            }
        },
        KeyCode::Char('+') => {
            let selected = queue_state.science_renderer_info.research_queue.selected().last().cloned();
            match selected {
                Some(s) => {
                    with_mut_science_manager(|m| m.move_item_up_in_queue(s));
                },
                None => {},
            }
        },
        KeyCode::Char('-') => {
            let selected = queue_state.science_renderer_info.research_queue.selected().last().cloned();
            match selected {
                Some(s) => {
                    with_mut_science_manager(|m| m.move_item_down_in_queue(s));
                },
                None => {},
            }
        },

        _ => {},
    }

    ui_state.science_renderer_state.research_queue = queue_state.science_renderer_info.research_queue;
    set_ui_state_to_channel(ui_state);
}

fn handle_key_down_on_research_list(key: KeyCode) {
    let mut queue_state = get_ui_info_from_channel();
    let mut ui_state = get_ui_state_from_channel();
    match key{
        KeyCode::Up | KeyCode::Char('w') => { queue_state.science_renderer_info.research_list.key_up(); },
        KeyCode::Down | KeyCode::Char('s') => { queue_state.science_renderer_info.research_list.key_down(); },
        KeyCode::Enter => {
            let selected = queue_state.science_renderer_info.research_list.selected().last().cloned();
            match selected {
                Some(s) => {
                    with_mut_science_manager(|m| m.add_to_research_queue(s));
                },
                None => {},
            }
        }
        _ => {},
    }

    ui_state.science_renderer_state.research_list = queue_state.science_renderer_info.research_list;
    set_ui_state_to_channel(ui_state);
}
