use ratatui::{
    layout::{Constraint, Layout, Rect},
    style::{Style, Stylize},
    text::Line,
    widgets::{Block, Clear, Paragraph, Row, Widget},
};

use crate::{
    ACTIVE_COLOR, INACTIVE_COLOR,
    app::ui_state::{FleetAddStep, FleetManagerPanel},
    channels::channels::get_selected_fleet_id,
    entities::{GameEntity, fleet::FleetOrderType},
    extentions::clickable_table::{ClickableTable, ClickableTableState, KeyedRow},
    get_bodies, get_body_by_id, get_fleet_by_id, get_fleets,
    with_ui_info_mut, with_ui_state, with_ui_state_mut,
};

const AU: f64 = 149_597_870.7;

fn format_range_km(km: f64) -> String {
    if !km.is_finite() || km <= 0.0 {
        return "0 km".to_string();
    }
    let au = km / AU;
    if au < 0.1 {
        format!("{:.1} km", km)
    } else {
        format!("{:.3} AU", au)
    }
}

fn order_label(order: &FleetOrderType) -> String {
    match order {
        FleetOrderType::MoveToPosition((x, y)) => {
            format!("Move to ({:.2} AU, {:.2} AU)", x / AU, y / AU)
        }
        FleetOrderType::MoveToObject(id) => {
            let name = get_body_by_id(id.clone())
                .map(|b| b.get_name())
                .or_else(|| get_fleet_by_id(id.clone()).map(|f| f.get_name()))
                .unwrap_or_else(|| id.clone());
            format!("Move → {}", name)
        }
        FleetOrderType::KeepDistanceToObject(id, dist) => {
            let name = get_body_by_id(id.clone())
                .map(|b| b.get_name())
                .or_else(|| get_fleet_by_id(id.clone()).map(|f| f.get_name()))
                .unwrap_or_else(|| id.clone());
            format!("Keep {:.2} AU from {}", dist / AU, name)
        }
        FleetOrderType::Split(ids) => format!("Split ({} ships)", ids.len()),
        FleetOrderType::Join(id) => {
            let name = get_fleet_by_id(id.clone())
                .map(|f| f.get_name())
                .unwrap_or_else(|| id.clone());
            format!("Join → {}", name)
        }
        FleetOrderType::Colonize(id) => {
            let name = get_body_by_id(id.clone())
                .map(|b| b.get_name())
                .unwrap_or_else(|| id.clone());
            format!("Colonize → {}", name)
        }
        FleetOrderType::Idle => "Idle".to_string(),
        _ => "…".to_string(),
    }
}

fn centered_rect(percent_x: u16, percent_y: u16, area: Rect) -> Rect {
    let popup_height = area.height * percent_y / 100;
    let popup_width = area.width * percent_x / 100;
    let y = area.y + (area.height.saturating_sub(popup_height)) / 2;
    let x = area.x + (area.width.saturating_sub(popup_width)) / 2;
    Rect { x, y, width: popup_width, height: popup_height }
}

pub fn render(area: Rect, buf: &mut ratatui::prelude::Buffer) {
    let fleet_id = match get_selected_fleet_id() {
        Some(id) => id,
        None => return,
    };

    let fleet = match get_fleet_by_id(fleet_id.clone()) {
        Some(f) => f,
        None => return,
    };

    let [info_area, lists_area, hint_area] = Layout::vertical([
        Constraint::Length(5),
        Constraint::Fill(1),
        Constraint::Length(1),
    ])
    .areas(area);

    // Info panel
    let pos_x_au = fleet.x / AU;
    let pos_y_au = fleet.y / AU;
    let fleet_name = if fleet.name.is_empty() {
        format!("Fleet ({})", &fleet.id[..8.min(fleet.id.len())])
    } else {
        fleet.name.clone()
    };
    let info_text = vec![
        Line::from(format!(" {}", fleet_name)).bold(),
        Line::from(format!(" Position: ({:.2} AU, {:.2} AU)", pos_x_au, pos_y_au)),
        Line::from(format!(" Members: {}  |  Orders: {}", fleet.members.len(), fleet.order_queue.len())),
    ];
    Paragraph::new(info_text)
        .block(Block::bordered().title(" Fleet Info "))
        .render(info_area, buf);

    // Lists area: order queue left, ships right
    let [queue_area, ships_column] =
        Layout::horizontal([Constraint::Percentage(50), Constraint::Percentage(50)]).areas(lists_area);

    // Ships column: list on top, detail panel below
    let [ships_area, ship_detail_area] =
        Layout::vertical([Constraint::Fill(1), Constraint::Length(6)]).areas(ships_column);

    let active_panel = with_ui_state(|s| s.fleet_manager_state.active_panel.clone());

    // --- Order queue ---
    let order_keys: Vec<String> = (0..fleet.order_queue.len()).map(|i| i.to_string()).collect();
    let queue_border_color = if active_panel == FleetManagerPanel::OrderQueue {
        ACTIVE_COLOR
    } else {
        INACTIVE_COLOR
    };

    let order_rows: Vec<KeyedRow<String>> = if fleet.order_queue.is_empty() {
        vec![KeyedRow::new(
            "empty".to_string(),
            Row::new(["[empty]"]),
        )]
    } else {
        fleet
            .order_queue
            .iter()
            .enumerate()
            .map(|(i, o)| KeyedRow::new(i.to_string(), Row::new([order_label(&o.order)])))
            .collect()
    };

    let queue_table = ClickableTable::new_keyed(order_rows, vec![Constraint::Fill(1)])
        .block(
            Block::bordered()
                .title(" Order Queue ")
                .border_style(Style::default().fg(queue_border_color)),
        )
        .row_highlight_style(Style::default().fg(ACTIVE_COLOR).bold());

    let (queue_cells, queue_state) = with_ui_state_mut(|s| {
        s.fleet_manager_state.order_queue_state.update_keys(order_keys.clone());
        let cells = queue_table.render_stateful_keyed_into_cells(
            queue_area,
            buf,
            &mut s.fleet_manager_state.order_queue_state,
        );
        (cells, s.fleet_manager_state.order_queue_state.clone())
    });

    // --- Ships ---
    let ship_keys: Vec<String> = fleet.members.iter().map(|s| s.id.clone()).collect();
    let ships_border_color = if active_panel == FleetManagerPanel::Ships {
        ACTIVE_COLOR
    } else {
        INACTIVE_COLOR
    };

    let ship_rows: Vec<KeyedRow<String>> = fleet
        .members
        .iter()
        .map(|s| {
            KeyedRow::new(
                s.id.clone(),
                Row::new([s.design.name.clone()]),
            )
        })
        .collect();

    let ships_table = ClickableTable::new_keyed(ship_rows, vec![Constraint::Fill(1)])
        .block(
            Block::bordered()
                .title(" Ships ")
                .border_style(Style::default().fg(ships_border_color)),
        )
        .row_highlight_style(Style::default().fg(ACTIVE_COLOR).bold());

    let (ships_cells, ships_state) = with_ui_state_mut(|s| {
        s.fleet_manager_state.ships_state.update_keys(ship_keys.clone());
        let cells = ships_table.render_stateful_keyed_into_cells(
            ships_area,
            buf,
            &mut s.fleet_manager_state.ships_state,
        );
        (cells, s.fleet_manager_state.ships_state.clone())
    });

    with_ui_info_mut(|ui_info| {
        ui_info.fleet_manager_info.order_cells = queue_cells;
        ui_info.fleet_manager_info.order_state = queue_state;
        ui_info.fleet_manager_info.ships_cells = ships_cells;
        ui_info.fleet_manager_info.ships_state = ships_state;
    });

    // --- Ship detail panel ---
    let selected_ship_id = with_ui_state(|s| s.fleet_manager_state.ships_state.selected_key().cloned());
    let selected_ship = selected_ship_id.and_then(|id| fleet.members.iter().find(|s| s.id == id).cloned());

    let detail_lines: Vec<Line> = if let Some(ship) = selected_ship {
        let short_id = &ship.id[..8.min(ship.id.len())];
        let speed = ship.get_speed();
        let max_range_km = ship.get_range();
        let max_range_str = format_range_km(max_range_km);
        vec![
            Line::from(format!(" Name:      {} ({})", ship.design.name, short_id)),
            Line::from(format!(" Design:    {}", ship.design.name)),
            Line::from(format!(" Speed:     {:.2} km/s", speed)),
            Line::from(format!(" Max Range: {}", max_range_str)),
        ]
    } else {
        vec![Line::from(" No ship selected").fg(INACTIVE_COLOR)]
    };
    Paragraph::new(detail_lines)
        .block(Block::bordered().title(" Ship Details "))
        .render(ship_detail_area, buf);

    // Hint bar
    Paragraph::new(Line::from(" a: add order | d: remove selected order | ↑↓: navigate"))
        .style(Style::default().fg(INACTIVE_COLOR))
        .render(hint_area, buf);

    // Add order popup
    let add_step = with_ui_state(|s| s.fleet_manager_state.add_step.clone());
    if add_step != FleetAddStep::Idle {
        render_add_popup(area, buf, &fleet_id);
    }
}

fn render_add_popup(area: Rect, buf: &mut ratatui::prelude::Buffer, fleet_id: &str) {
    let popup_area = centered_rect(60, 80, area);
    Clear.render(popup_area, buf);

    let outer_block = Block::bordered()
        .title(" Add Order ")
        .border_style(Style::default().fg(ACTIVE_COLOR));
    outer_block.render(popup_area, buf);

    let inner = popup_area.inner(ratatui::layout::Margin { horizontal: 1, vertical: 1 });

    let add_step = with_ui_state(|s| s.fleet_manager_state.add_step.clone());

    match add_step {
        FleetAddStep::SelectType => {
            let (add_type_index, labels): (usize, Vec<&'static str>) = with_ui_state(|s| {
                let idx = s.fleet_manager_state.add_type_index;
                let labels = s.fleet_manager_state.available_order_types
                    .iter()
                    .map(|t| t.label())
                    .collect();
                (idx, labels)
            });
            let lines: Vec<Line> = labels
                .iter()
                .enumerate()
                .map(|(i, &label)| {
                    if i == add_type_index {
                        Line::from(format!("❯ {}", label)).fg(ACTIVE_COLOR).bold()
                    } else {
                        Line::from(format!("  {}", label))
                    }
                })
                .collect();
            let [list_area, hint_a] =
                Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);
            Paragraph::new(lines).render(list_area, buf);
            Paragraph::new("↑↓: select | Enter: confirm | ESC: cancel")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::SelectObject => {
            let bodies = get_bodies();
            let fleets = get_fleets();

            let mut rows: Vec<KeyedRow<String>> = vec![];
            let mut keys: Vec<String> = vec![];

            for body in &bodies {
                let type_label = if body.moons.is_empty() && body.colony.is_none() {
                    "Moon"
                } else {
                    "Planet"
                };
                keys.push(body.id.clone());
                rows.push(KeyedRow::new(
                    body.id.clone(),
                    Row::new([body.get_name(), type_label.to_string()]),
                ));
            }
            for fleet in &fleets {
                if fleet.slipway_fleet || fleet.id == fleet_id {
                    continue;
                }
                keys.push(fleet.id.clone());
                rows.push(KeyedRow::new(
                    fleet.id.clone(),
                    Row::new([fleet.get_name(), "Fleet".to_string()]),
                ));
            }

            let [list_area, hint_a] =
                Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);

            let table = ClickableTable::new_keyed(
                rows,
                vec![Constraint::Percentage(70), Constraint::Percentage(30)],
            )
            .row_highlight_style(Style::default().fg(ACTIVE_COLOR).bold());

            let add_cells = with_ui_state_mut(|s| {
                s.fleet_manager_state.add_object_state.update_keys(keys.clone());
                let cells = table.render_stateful_keyed_into_cells(
                    list_area,
                    buf,
                    &mut s.fleet_manager_state.add_object_state,
                );
                cells
            });

            with_ui_info_mut(|ui_info| {
                ui_info.fleet_manager_info.add_cells = add_cells;
                ui_info.fleet_manager_info.add_state = with_ui_state(|s| {
                    s.fleet_manager_state.add_object_state.clone()
                });
            });

            Paragraph::new("↑↓: select | Enter: confirm | ESC: back")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::SelectAddType => {
            let add_add_type_index = with_ui_state(|s| s.fleet_manager_state.add_add_type_index);
            let options = [
                "Replace orders",
                "Insert at front",
                "Append to queue",
                "Insert at position N",
            ];
            let lines: Vec<Line> = options
                .iter()
                .enumerate()
                .map(|(i, &label)| {
                    if i == add_add_type_index {
                        Line::from(format!("❯ {}", label)).fg(ACTIVE_COLOR).bold()
                    } else {
                        Line::from(format!("  {}", label))
                    }
                })
                .collect();
            let [list_area, hint_a] =
                Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);
            Paragraph::new(lines).render(list_area, buf);
            Paragraph::new("↑↓: select | Enter: confirm | ESC: back")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::EnterN => {
            let input = with_ui_state(|s| s.fleet_manager_state.add_insert_n_input.clone());
            let [prompt_a, input_a, hint_a] = Layout::vertical([
                Constraint::Length(1),
                Constraint::Length(1),
                Constraint::Length(1),
            ])
            .areas(inner);
            Paragraph::new("Insert at position (0 = front):").render(prompt_a, buf);
            Paragraph::new(format!("> {}_", input))
                .style(Style::default().fg(ACTIVE_COLOR))
                .render(input_a, buf);
            Paragraph::new("Type number | Enter: confirm | ESC: back")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::EnterDistance => {
            let input = with_ui_state(|s| s.fleet_manager_state.add_distance_input.clone());
            let [prompt_a, input_a, hint_a] = Layout::vertical([
                Constraint::Length(1),
                Constraint::Length(1),
                Constraint::Length(1),
            ])
            .areas(inner);
            Paragraph::new("Distance (km  or  N au):").render(prompt_a, buf);
            Paragraph::new(format!("> {}_", input))
                .style(Style::default().fg(ACTIVE_COLOR))
                .render(input_a, buf);
            Paragraph::new("Type distance | Enter: confirm | ESC: back")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::SelectFleet => {
            let fleets = get_fleets();

            let mut rows: Vec<KeyedRow<String>> = vec![];
            let mut keys: Vec<String> = vec![];

            for fleet in &fleets {
                if fleet.slipway_fleet || fleet.id == fleet_id {
                    continue;
                }
                keys.push(fleet.id.clone());
                rows.push(KeyedRow::new(
                    fleet.id.clone(),
                    Row::new([fleet.get_name(), format!("{} ships", fleet.members.len())]),
                ));
            }

            let [list_area, hint_a] =
                Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);

            let table = ClickableTable::new_keyed(
                rows,
                vec![Constraint::Percentage(70), Constraint::Percentage(30)],
            )
            .row_highlight_style(Style::default().fg(ACTIVE_COLOR).bold());

            let add_cells = with_ui_state_mut(|s| {
                s.fleet_manager_state.add_object_state.update_keys(keys.clone());
                table.render_stateful_keyed_into_cells(
                    list_area,
                    buf,
                    &mut s.fleet_manager_state.add_object_state,
                )
            });

            with_ui_info_mut(|ui_info| {
                ui_info.fleet_manager_info.add_cells = add_cells;
                ui_info.fleet_manager_info.add_state =
                    with_ui_state(|s| s.fleet_manager_state.add_object_state.clone());
            });

            Paragraph::new("↑↓: select | Enter: confirm | ESC: back")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::SelectBody => {
            let bodies = get_bodies();

            let mut rows: Vec<KeyedRow<String>> = vec![];
            let mut keys: Vec<String> = vec![];

            for body in &bodies {
                // Skip already-colonized bodies
                if body.colony.is_some() {
                    continue;
                }
                keys.push(body.id.clone());
                rows.push(KeyedRow::new(
                    body.id.clone(),
                    Row::new([body.get_name(), if body.moons.is_empty() { "Moon" } else { "Planet" }.to_string()]),
                ));
            }

            let [list_area, hint_a] =
                Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);

            let table = ClickableTable::new_keyed(
                rows,
                vec![Constraint::Percentage(70), Constraint::Percentage(30)],
            )
            .row_highlight_style(Style::default().fg(ACTIVE_COLOR).bold());

            let add_cells = with_ui_state_mut(|s| {
                s.fleet_manager_state.add_object_state.update_keys(keys.clone());
                table.render_stateful_keyed_into_cells(
                    list_area,
                    buf,
                    &mut s.fleet_manager_state.add_object_state,
                )
            });

            with_ui_info_mut(|ui_info| {
                ui_info.fleet_manager_info.add_cells = add_cells;
                ui_info.fleet_manager_info.add_state =
                    with_ui_state(|s| s.fleet_manager_state.add_object_state.clone());
            });

            Paragraph::new("↑↓: select | Enter: confirm | ESC: back")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::SelectShipsToSplit => {
            let fleet = match get_fleet_by_id(fleet_id.to_string()) {
                Some(f) => f,
                None => return,
            };

            let ship_keys: Vec<String> = fleet.members.iter().map(|s| s.id.clone()).collect();

            let selected_ids = with_ui_state(|s| {
                s.fleet_manager_state.split_selected_ship_ids.clone()
            });

            let rows: Vec<KeyedRow<String>> = fleet
                .members
                .iter()
                .map(|s| {
                    let check = if selected_ids.contains(&s.id) { "✓" } else { "☐" };
                    KeyedRow::new(
                        s.id.clone(),
                        Row::new([format!("{} {}", check, s.design.name)]),
                    )
                })
                .collect();

            let [list_area, hint_a] =
                Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);

            let table = ClickableTable::new_keyed(rows, vec![Constraint::Fill(1)])
                .row_highlight_style(Style::default().fg(ACTIVE_COLOR).bold());

            let split_cells = with_ui_state_mut(|s| {
                s.fleet_manager_state.split_ships_state.update_keys(ship_keys.clone());
                table.render_stateful_keyed_into_cells(
                    list_area,
                    buf,
                    &mut s.fleet_manager_state.split_ships_state,
                )
            });

            with_ui_info_mut(|ui_info| {
                ui_info.fleet_manager_info.split_cells = split_cells;
                ui_info.fleet_manager_info.split_state =
                    with_ui_state(|s| s.fleet_manager_state.split_ships_state.clone());
            });

            Paragraph::new("↑↓: navigate | Space: toggle | Enter: confirm | ESC: back")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        FleetAddStep::Idle => {}
    }
}
