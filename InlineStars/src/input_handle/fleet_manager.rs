use crossterm::event::KeyCode;

use crate::{
    app::ui_state::{FleetAddStep, FleetManagerPanel},
    channels::channels::{get_selected_fleet_id, get_ui_info_from_channel, get_ui_state_from_channel, insert_fleet_order, set_ui_state_to_channel},
    entities::fleet::{FleetOrder, FleetOrderType, OrderAddType},
};

pub(super) fn handle_key_down_on_fleet_manager(key: KeyCode) {
    let mut ui_state = get_ui_state_from_channel();
    let mut ui_info = get_ui_info_from_channel();

    //Order generation state machine:
    match ui_state.fleet_manager_state.add_step.clone() {
        FleetAddStep::Idle => {
            match key {
                KeyCode::Char('a') => {
                    if let Some(fleet) = get_selected_fleet_id().and_then(|id| crate::get_fleet_by_id(id)) {
                        ui_state.fleet_manager_state.available_order_types = fleet
                            .available_orders()
                            .into_iter()
                            .filter(|o| !matches!(o, FleetOrderType::AddMembers(_) | FleetOrderType::RemoveMembers(_)))
                            .collect();
                    }
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectType;
                    ui_state.fleet_manager_state.add_type_index = 0;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Char('d') => {
                    if ui_state.fleet_manager_state.active_panel == FleetManagerPanel::OrderQueue {
                        if let Some(key) = ui_state.fleet_manager_state.order_queue_state.selected().cloned() {
                            if let Ok(index) = key.parse::<usize>() {
                                if let Some(fleet_id) = get_selected_fleet_id() {
                                    insert_fleet_order(FleetOrder {
                                        fleet_id,
                                        add_type: OrderAddType::Enqueue,
                                        order: FleetOrderType::RemoveOrder(index),
                                    });
                                }
                            }
                        }
                    }
                }
                KeyCode::Tab => {
                    ui_state.fleet_manager_state.active_panel =
                        if ui_state.fleet_manager_state.active_panel == FleetManagerPanel::OrderQueue {
                            FleetManagerPanel::Ships
                        } else {
                            FleetManagerPanel::OrderQueue
                        };
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Up | KeyCode::Char('w') => {
                    match ui_state.fleet_manager_state.active_panel {
                        FleetManagerPanel::OrderQueue => {
                            ui_info.fleet_manager_info.order_state.previous();
                            ui_state.fleet_manager_state.order_queue_state =
                                ui_info.fleet_manager_info.order_state.clone();
                        }
                        FleetManagerPanel::Ships => {
                            ui_info.fleet_manager_info.ships_state.previous();
                            ui_state.fleet_manager_state.ships_state =
                                ui_info.fleet_manager_info.ships_state.clone();
                        }
                    }
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    match ui_state.fleet_manager_state.active_panel {
                        FleetManagerPanel::OrderQueue => {
                            ui_info.fleet_manager_info.order_state.next();
                            ui_state.fleet_manager_state.order_queue_state =
                                ui_info.fleet_manager_info.order_state.clone();
                        }
                        FleetManagerPanel::Ships => {
                            ui_info.fleet_manager_info.ships_state.next();
                            ui_state.fleet_manager_state.ships_state =
                                ui_info.fleet_manager_info.ships_state.clone();
                        }
                    }
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::SelectType => {
            match key {
                KeyCode::Up | KeyCode::Char('w') => {
                    let len = ui_state.fleet_manager_state.available_order_types.len().max(1);
                    ui_state.fleet_manager_state.add_type_index =
                        (ui_state.fleet_manager_state.add_type_index + len - 1) % len;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    let len = ui_state.fleet_manager_state.available_order_types.len().max(1);
                    ui_state.fleet_manager_state.add_type_index =
                        (ui_state.fleet_manager_state.add_type_index + 1) % len;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Enter => {
                    let selected = ui_state.fleet_manager_state
                        .available_order_types
                        .get(ui_state.fleet_manager_state.add_type_index)
                        .cloned();
                    match selected {
                        Some(ref t) if t.needs_ships() => {
                            ui_state.fleet_manager_state.split_selected_ship_ids.clear();
                            ui_state.fleet_manager_state.add_step = FleetAddStep::SelectShipsToSplit;
                        }
                        Some(ref t) if t.needs_fleet_only() => {
                            ui_state.fleet_manager_state.add_step = FleetAddStep::SelectFleet;
                        }
                        Some(ref t) if t.needs_body_only() => {
                            ui_state.fleet_manager_state.add_step = FleetAddStep::SelectBody;
                        }
                        Some(ref t) if t.needs_object() => {
                            ui_state.fleet_manager_state.add_step = FleetAddStep::SelectObject;
                        }
                        Some(_) => {
                            // Idle / MoveToPosition: skip object selection
                            ui_state.fleet_manager_state.add_selected_object_id = None;
                            ui_state.fleet_manager_state.add_step = FleetAddStep::SelectAddType;
                        }
                        None => {}
                    }
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Esc => {
                    ui_state.fleet_manager_state.add_step = FleetAddStep::Idle;
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::SelectObject => {
            match key {
                KeyCode::Up | KeyCode::Char('w') => {
                    ui_info.fleet_manager_info.add_state.previous();
                    ui_state.fleet_manager_state.add_object_state =
                        ui_info.fleet_manager_info.add_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    ui_info.fleet_manager_info.add_state.next();
                    ui_state.fleet_manager_state.add_object_state =
                        ui_info.fleet_manager_info.add_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Enter => {
                    let selected = ui_state.fleet_manager_state.add_object_state.selected().cloned();
                    ui_state.fleet_manager_state.add_selected_object_id = selected;
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectAddType;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Esc => {
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectType;
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::SelectFleet => {
            match key {
                KeyCode::Up | KeyCode::Char('w') => {
                    ui_info.fleet_manager_info.add_state.previous();
                    ui_state.fleet_manager_state.add_object_state =
                        ui_info.fleet_manager_info.add_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    ui_info.fleet_manager_info.add_state.next();
                    ui_state.fleet_manager_state.add_object_state =
                        ui_info.fleet_manager_info.add_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Enter => {
                    let selected = ui_state.fleet_manager_state.add_object_state.selected().cloned();
                    ui_state.fleet_manager_state.add_selected_object_id = selected;
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectAddType;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Esc => {
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectType;
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::SelectBody => {
            match key {
                KeyCode::Up | KeyCode::Char('w') => {
                    ui_info.fleet_manager_info.add_state.previous();
                    ui_state.fleet_manager_state.add_object_state =
                        ui_info.fleet_manager_info.add_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    ui_info.fleet_manager_info.add_state.next();
                    ui_state.fleet_manager_state.add_object_state =
                        ui_info.fleet_manager_info.add_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Enter => {
                    let selected = ui_state.fleet_manager_state.add_object_state.selected().cloned();
                    ui_state.fleet_manager_state.add_selected_object_id = selected;
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectAddType;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Esc => {
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectType;
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::SelectShipsToSplit => {
            match key {
                KeyCode::Up | KeyCode::Char('w') => {
                    ui_info.fleet_manager_info.split_state.previous();
                    ui_state.fleet_manager_state.split_ships_state =
                        ui_info.fleet_manager_info.split_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    ui_info.fleet_manager_info.split_state.next();
                    ui_state.fleet_manager_state.split_ships_state =
                        ui_info.fleet_manager_info.split_state.clone();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Char(' ') => {
                    if let Some(id) = ui_state.fleet_manager_state.split_ships_state.selected().cloned() {
                        if ui_state.fleet_manager_state.split_selected_ship_ids.contains(&id) {
                            ui_state.fleet_manager_state.split_selected_ship_ids.remove(&id);
                        } else {
                            ui_state.fleet_manager_state.split_selected_ship_ids.insert(id);
                        }
                        set_ui_state_to_channel(ui_state);
                    }
                }
                KeyCode::Enter => {
                    if !ui_state.fleet_manager_state.split_selected_ship_ids.is_empty() {
                        //dispatch_add_order(&mut ui_state);
                        ui_state.fleet_manager_state.add_step = FleetAddStep::SelectAddType;
                        set_ui_state_to_channel(ui_state);
                    }
                }
                KeyCode::Esc => {
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectType;
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::SelectAddType => {
            match key {
                KeyCode::Up | KeyCode::Char('w') => {
                    ui_state.fleet_manager_state.add_add_type_index =
                        (ui_state.fleet_manager_state.add_add_type_index + 3) % 4;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Down | KeyCode::Char('s') => {
                    ui_state.fleet_manager_state.add_add_type_index =
                        (ui_state.fleet_manager_state.add_add_type_index + 1) % 4;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Enter => {
                    let selected_needs_dist = ui_state.fleet_manager_state
                        .available_order_types
                        .get(ui_state.fleet_manager_state.add_type_index)
                        .map(|t| t.needs_distance())
                        .unwrap_or(false);
                    let is_insert_n = ui_state.fleet_manager_state.add_add_type_index == 3;
                    if is_insert_n {
                        ui_state.fleet_manager_state.add_insert_n_input = "0".to_string();
                        ui_state.fleet_manager_state.add_step = FleetAddStep::EnterN;
                        set_ui_state_to_channel(ui_state);
                    } else if selected_needs_dist {
                        ui_state.fleet_manager_state.add_distance_input = "1 au".to_string();
                        ui_state.fleet_manager_state.add_step = FleetAddStep::EnterDistance;
                        set_ui_state_to_channel(ui_state);
                    } else {
                        dispatch_add_order(&mut ui_state);
                        ui_state.fleet_manager_state.add_step = FleetAddStep::Idle;
                        set_ui_state_to_channel(ui_state);
                    }
                }
                KeyCode::Esc => {
                    let back_step = {
                        let selected = ui_state.fleet_manager_state
                            .available_order_types
                            .get(ui_state.fleet_manager_state.add_type_index);
                        if selected.map(|t| t.needs_fleet_only()).unwrap_or(false) {
                            FleetAddStep::SelectFleet
                        } else if selected.map(|t| t.needs_body_only()).unwrap_or(false) {
                            FleetAddStep::SelectBody
                        } else {
                            FleetAddStep::SelectObject
                        }
                    };
                    ui_state.fleet_manager_state.add_step = back_step;
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::EnterN => {
            match key {
                KeyCode::Char(c) if c.is_ascii_digit() => {
                    ui_state.fleet_manager_state.add_insert_n_input.push(c);
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Backspace => {
                    ui_state.fleet_manager_state.add_insert_n_input.pop();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Enter => {
                    let is_keep_dist = ui_state.fleet_manager_state
                        .available_order_types
                        .get(ui_state.fleet_manager_state.add_type_index)
                        .map(|t| t.needs_distance())
                        .unwrap_or(false);
                    if is_keep_dist {
                        ui_state.fleet_manager_state.add_distance_input = "1 au".to_string();
                        ui_state.fleet_manager_state.add_step = FleetAddStep::EnterDistance;
                    } else {
                        dispatch_add_order(&mut ui_state);
                        ui_state.fleet_manager_state.add_step = FleetAddStep::Idle;
                    }
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Esc => {
                    ui_state.fleet_manager_state.add_step = FleetAddStep::SelectAddType;
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
        FleetAddStep::EnterDistance => {
            match key {
                KeyCode::Char(c) => {
                    ui_state.fleet_manager_state.add_distance_input.push(c);
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Backspace => {
                    ui_state.fleet_manager_state.add_distance_input.pop();
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Enter => {
                    dispatch_add_order(&mut ui_state);
                    ui_state.fleet_manager_state.add_step = FleetAddStep::Idle;
                    set_ui_state_to_channel(ui_state);
                }
                KeyCode::Esc => {
                    ui_state.fleet_manager_state.add_step =
                        if ui_state.fleet_manager_state.add_add_type_index == 3 {
                            FleetAddStep::EnterN
                        } else {
                            FleetAddStep::SelectAddType
                        };
                    set_ui_state_to_channel(ui_state);
                }
                _ => {}
            }
        }
    }
}

fn dispatch_add_order(ui_state: &mut crate::app::ui_state::UIState) {
    let fleet_id = match get_selected_fleet_id() {
        Some(id) => id,
        None => return,
    };

    let selected_type = match ui_state.fleet_manager_state
        .available_order_types
        .get(ui_state.fleet_manager_state.add_type_index)
        .cloned()
    {
        Some(t) => t,
        None => return,
    };

    let order_type = match selected_type {
        FleetOrderType::MoveToObject(_) => {
            let id = match &ui_state.fleet_manager_state.add_selected_object_id {
                Some(id) => id.clone(),
                None => return,
            };
            FleetOrderType::MoveToObject(id)
        }
        FleetOrderType::KeepDistanceToObject(_, _) => {
            let id = match &ui_state.fleet_manager_state.add_selected_object_id {
                Some(id) => id.clone(),
                None => return,
            };
            let dist = parse_fleet_distance(&ui_state.fleet_manager_state.add_distance_input)
                .unwrap_or(149_597_870.7);
            FleetOrderType::KeepDistanceToObject(id, dist)
        }
        FleetOrderType::Idle => FleetOrderType::Idle,
        FleetOrderType::Join(_) => {
            let id = match &ui_state.fleet_manager_state.add_selected_object_id {
                Some(id) => id.clone(),
                None => return,
            };
            FleetOrderType::Join(id)
        }
        FleetOrderType::Split(_) => {
            let ship_ids: Vec<String> = ui_state
                .fleet_manager_state
                .split_selected_ship_ids
                .iter()
                .cloned()
                .collect();
            FleetOrderType::Split(ship_ids)
        }
        FleetOrderType::Colonize(_) => {
            let id = match &ui_state.fleet_manager_state.add_selected_object_id {
                Some(id) => id.clone(),
                None => return,
            };
            FleetOrderType::Colonize(id)
        }
        _ => return, // MoveToPosition and member ops handled elsewhere
    };

    let add_type = match ui_state.fleet_manager_state.add_add_type_index {
        0 => OrderAddType::Replace,
        1 => OrderAddType::InFront,
        2 => OrderAddType::Enqueue,
        3 => {
            let n = ui_state
                .fleet_manager_state
                .add_insert_n_input
                .trim()
                .parse::<usize>()
                .unwrap_or(0);
            OrderAddType::Insert(n)
        }
        _ => OrderAddType::Enqueue,
    };

    insert_fleet_order(FleetOrder { fleet_id, add_type, order: order_type });
}

fn parse_fleet_distance(input: &str) -> Option<f64> {
    let s = input.trim().to_lowercase();
    if s.ends_with("au") {
        s[..s.len() - 2]
            .trim()
            .parse::<f64>()
            .ok()
            .map(|v| v * 149_597_870.7)
    } else {
        s.parse::<f64>().ok()
    }
}
