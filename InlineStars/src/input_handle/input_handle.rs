use crate::app::InputState;
use crate::channels::channels::get_camera_state;

use super::common::{INPUT_STATE, MouseDragState};
use super::key_press::handle_key_press;
use super::mouse::{handle_mouse_down, handle_mouse_scroll};

pub use super::common::ColonyAction;

pub fn run_input_worker() {
    tokio::task::spawn_blocking(move || {
        let mut input_state = InputState::default();
        let mut mouse_drag_state = MouseDragState::default();

        //let (event_tx, event_rx) = tokio::sync::mpsc::unbounded_channel();

        loop {
            if input_state.terminate == true {
                break;
            }
            let mut camera = get_camera_state().as_ref().clone();
            let event = crossterm::event::read().unwrap();
            match event {
                crossterm::event::Event::FocusGained => {}
                crossterm::event::Event::FocusLost => {}
                crossterm::event::Event::Key(key_event) if key_event.is_press() => {
                    input_state
                        .keys_down
                        .push(key_event.code.as_char().unwrap_or_default());

                    INPUT_STATE.with_borrow_mut(|input_state| {
                        *input_state = key_event.clone();
                    });

                    handle_key_press(key_event.code);
                }
                crossterm::event::Event::Key(key_event) if key_event.is_release() => {
                    input_state
                        .keys_down
                        .retain(|&k| k != key_event.code.as_char().unwrap_or_default());
                    //input_state.last_key_press = None;
                }
                crossterm::event::Event::Mouse(mouse_event) => match mouse_event.kind {
                    crossterm::event::MouseEventKind::Down(mouse_button) => {
                        input_state.mouse_down = true;
                        input_state.mouse_position = (mouse_event.column, mouse_event.row);
                        handle_mouse_down(mouse_event);
                    }
                    crossterm::event::MouseEventKind::Up(mouse_button) => {
                        input_state.mouse_down = false;
                        input_state.mouse_position = (mouse_event.column, mouse_event.row);

                        mouse_drag_state.dragging = false;
                        handle_mouse_drag(&mouse_drag_state);
                    }

                    crossterm::event::MouseEventKind::Drag(mouse_button) => {
                        input_state.dragging = true;
                        //input_state.mouse_down = true;
                        input_state.mouse_position = (mouse_event.column, mouse_event.row);

                        if !mouse_drag_state.dragging {
                            mouse_drag_state.dragging = true;
                            mouse_drag_state.start_position = input_state.mouse_position;
                        }

                        if mouse_drag_state.dragging {
                            let (last_x, last_y) = mouse_drag_state.last_position;
                            let delta_x =
                                (input_state.mouse_position.0 as i16 - (last_x as i16)) * -1;
                            let delta_y =
                                (input_state.mouse_position.1 as i16 - (last_y as i16)) * -1;

                            mouse_drag_state.last_position = (mouse_event.column, mouse_event.row);
                            camera.pan(delta_x, delta_y);
                        }

                        mouse_drag_state.last_position = (mouse_event.column, mouse_event.row);

                        //handle_mouse_drag(&mouse_drag_state);
                    }
                    crossterm::event::MouseEventKind::Moved => {
                        input_state.mouse_position = (mouse_event.column, mouse_event.row);
                        mouse_drag_state.last_position = (mouse_event.column, mouse_event.row);
                    }
                    crossterm::event::MouseEventKind::ScrollDown => {
                        //input_state.scroll_down = true;
                        handle_mouse_scroll(mouse_event);
                    }
                    crossterm::event::MouseEventKind::ScrollUp => {
                        //input_state.scroll_up = true;
                        handle_mouse_scroll(mouse_event);
                    }
                    crossterm::event::MouseEventKind::ScrollLeft => todo!(),
                    crossterm::event::MouseEventKind::ScrollRight => todo!(),
                },
                crossterm::event::Event::Paste(_) => todo!(),
                crossterm::event::Event::Resize(x, y) => {
                    input_state.last_size = (x, y);
                }
                _ => {}
            }

            if input_state.keys_down.contains(&'`') {
                input_state.terminate = true;
            }

            crate::channels::channels::set_input_state(input_state.clone());
        }
    });
}

fn handle_mouse_drag(_: &MouseDragState) {}
