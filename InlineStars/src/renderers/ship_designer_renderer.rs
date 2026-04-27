use ratatui::{
    layout::{Alignment, Constraint, Layout, Rect},
    style::{Color, Style, Stylize},
    text::Line,
    widgets::{Block, Clear, List, ListItem, Padding, Paragraph, StatefulWidget, Widget},
};

use tui_tree_widget::*;

use crate::{ACTIVE_COLOR, INACTIVE_COLOR, app::{application::FRAME_TIME, effects::{BLINK_BORDER, COALESCE}, ship_desginer::with_ship_designer, ui_state::{ShipDesignerPanel, build_ship_designer_menu_entries}}, channels::channels::HAS_JUST_TAB, entities::ship::*, with_ui_info_mut, with_ui_state};

pub fn render(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let active_panel = with_ui_state(|state| state.ship_designer_state.active_panel.clone());
    let menu_popup_open = with_ui_state(|state| state.ship_designer_state.menu_popup_open);
    let rename_buffer = with_ui_state(|state| state.ship_designer_state.rename_buffer.clone());

    let [top, main] = Layout::vertical(vec![Constraint::Length(3), Constraint::Fill(100)]).areas(area);
    let [menu_area, name_area] = Layout::horizontal(vec![Constraint::Length(16), Constraint::Fill(1)]).areas(top);

    let [left_area, installed_area, main_info_area] =
        Layout::horizontal(vec![Constraint::Percentage(15), Constraint::Percentage(15), Constraint::Percentage(70)])
            .areas(main);
    let [designs_area, component_area] =
        Layout::vertical(vec![Constraint::Percentage(50), Constraint::Percentage(50)]).areas(left_area);
    let [ship_info_area, subsystem_info_area] =
        Layout::vertical(vec![Constraint::Percentage(75), Constraint::Percentage(25)])
            .areas(main_info_area);

    render_design_tree(designs_area, buf);
    render_subsystem_list(component_area, buf);
    render_installed_systems(installed_area, buf);
    get_ship_info_paragraph().render(ship_info_area, buf);

    let info_subsystem: Option<ShipSubsystem> = match active_panel {
        ShipDesignerPanel::SubsystemList => {
            let selected_idx = with_ui_state(|state| state.ship_designer_state.installed_subsystems_state.selected());
            selected_idx.and_then(|idx| {
                with_ship_designer(|sd| {
                    sd.current_design.as_ref().and_then(|ship| {
                        let grouped_ids = grouped_subsystem_ids(&ship.subsystems);
                        grouped_ids.get(idx)
                            .and_then(|id| ship.subsystems.iter().find(|ss| &ss.id == id).cloned())
                    })
                })
            })
        }
        ShipDesignerPanel::ShipDesigns => None,
        _ => {
            let selected = with_ui_state(|state| state.ship_designer_state.subsystem_tree_state.selected().last().cloned());
            selected.and_then(|id| with_ship_designer(|sd| sd.subsystem_library.get(&id).cloned()))
        }
    };

    match info_subsystem {
        Some(subsystem) => get_subsystem_paragraph(&subsystem).render(subsystem_info_area, buf),
        None => Paragraph::new(vec![Line::from("No subsystem selected")])
            .block(Block::bordered().title("Subsystem Info"))
            .render(subsystem_info_area, buf),
    }

    // Design name indicator — shows rename input when active, otherwise current design name + lock state.
    let rename_mode = rename_buffer.is_some();
    let (name_line, hint_line) = match rename_buffer.as_ref() {
        Some(buf) => (
            Line::from(format!(" Rename: {}_", buf)).style(Style::default().fg(Color::Yellow)),
            Line::from(" Enter: confirm rename | Esc: cancel")
                .style(Style::default().fg(INACTIVE_COLOR)),
        ),
        None => {
            let (name, locked) = with_ship_designer(|sd| {
                sd.current_design.as_ref()
                    .map(|s| (s.name.clone(), s.locked))
                    .unwrap_or_else(|| ("—".to_string(), false))
            });
            let label = if locked {
                format!(" {} [LOCKED]", name)
            } else {
                format!(" Design: {}", name)
            };
            (
                Line::from(label).style(Style::default().fg(INACTIVE_COLOR)),
                Line::from(" Press M or click Menu to open actions")
                    .style(Style::default().fg(INACTIVE_COLOR)),
            )
        }
    };
    let name_block = Block::bordered().title(" Current Design ")
        .border_style(Style::default().fg(INACTIVE_COLOR));
    let name_inner = name_block.inner(name_area);
    name_block.render(name_area, buf);
    if rename_mode {
        Paragraph::new(vec![name_line, hint_line]).render(name_inner, buf);
    } else {
        Paragraph::new(vec![name_line, hint_line]).render(name_inner, buf);
    }

    let menu_block = Block::bordered()
        .title(" Actions ")
        .border_style(Style::default().fg(if menu_popup_open { ACTIVE_COLOR } else { INACTIVE_COLOR }));
    let menu_inner = menu_block.inner(menu_area);
    menu_block.render(menu_area, buf);
    Paragraph::new("Menu (M)")
        .alignment(Alignment::Center)
        .style(Style::default().fg(if menu_popup_open { ACTIVE_COLOR } else { INACTIVE_COLOR }))
        .render(menu_inner, buf);

    with_ui_info_mut(|ui_info| {
        ui_info.ship_designer_menu_button_area = menu_area;
        ui_info.ship_designer_menu_popup_area = Rect::default();
        ui_info.ship_designer_menu_item_areas.clear();
        ui_info.ship_designer_design_tree_area = designs_area;
        ui_info.ship_designer_subsystem_tree_area = component_area;
    });

    if menu_popup_open {
        render_menu_popup(area, buf);
    }

    COALESCE.with(|effect| { 
        if HAS_JUST_TAB.swap(false, std::sync::atomic::Ordering::Relaxed){
            effect.borrow_mut().reset();
        }
        effect.borrow_mut().process(FRAME_TIME.into(), buf, area);
    });
}

fn render_design_tree(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let ship_designs = with_ship_designer(|sd| sd.ship_designs.clone());
    let current_design_id = with_ship_designer(|sd| sd.current_design.as_ref().map(|d| d.id.clone()));
    let mut design_tree_state = with_ui_state(|ui_state| ui_state.ship_designer_state.design_tree_state.clone());
    let is_active = with_ui_state(|ui_state| ui_state.ship_designer_state.active_panel == crate::app::ui_state::ShipDesignerPanel::ShipDesigns);

    let mut designs = ship_designs;
    designs.sort_by(|a, b| a.name.cmp(&b.name));

    let items: Vec<TreeItem<String>> = designs
        .iter()
        .map(|design| {
            let label = if current_design_id.as_deref() == Some(design.id.as_str()) {
                format!("{} [Current]", design.name)
            } else {
                design.name.clone()
            };
            TreeItem::new_leaf(design.id.clone(), label)
        })
        .collect();

    let block = Block::bordered()
        .title("Ship Designs")
        .padding(Padding::left(1))
        .border_style(Style::default().fg(if is_active { ACTIVE_COLOR } else { INACTIVE_COLOR }));
    let inner = block.inner(area);
    block.render(area, buf);

    if items.is_empty() {
        Paragraph::new("No saved designs")
            .style(Style::default().fg(INACTIVE_COLOR))
            .render(inner, buf);
        with_ui_info_mut(|ui_info| {
            ui_info.ship_designer_state.design_tree_state = design_tree_state;
        });
        return;
    }

    let current_is_saved = current_design_id
        .as_ref()
        .is_some_and(|id| designs.iter().any(|design| design.id == *id));
    let selected_is_valid = design_tree_state
        .selected()
        .last()
        .is_some_and(|id| designs.iter().any(|design| design.id == *id));

    if !selected_is_valid {
        if let Some(current_id) = current_design_id.filter(|_| current_is_saved) {
            design_tree_state.select(vec![current_id]);
        } else {
            design_tree_state.select(vec![]);
        }
    }

    let tree = Tree::new(&items).unwrap()
        .highlight_style(Style::default().fg(Color::Red));
    StatefulWidget::render(tree, inner, buf, &mut design_tree_state);

    with_ui_info_mut(|ui_info| {
        ui_info.ship_designer_state.design_tree_state = design_tree_state;
    });
}

fn render_installed_systems(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let sub_systems = with_ship_designer(|sd| sd.current_design.clone());
    let is_active = with_ui_state(|ui_state| ui_state.ship_designer_state.active_panel == crate::app::ui_state::ShipDesignerPanel::SubsystemList);
    let color = if is_active { ACTIVE_COLOR } else { INACTIVE_COLOR };

    let ship = match sub_systems {
        Some(ship) => ship,
        None => {
            Paragraph::new(vec![Line::from("")])
                .block(Block::bordered().title("Installed Subsystems").fg(color))
                .render(area, buf);
            return;
        }
    };

    let mut installed_subsystems_state = with_ui_state(|ui_state| ui_state.ship_designer_state.installed_subsystems_state.clone());

    let items: Vec<ListItem> = {
        let mut grouped: Vec<(String, usize)> = Vec::new(); // (name, count)
        for ss in &ship.subsystems {
            if let Some(entry) = grouped.iter_mut().find(|(name, _)| name == &ss.name) {
                entry.1 += 1;
            } else {
                grouped.push((ss.name.clone(), 1));
            }
        }
        grouped.into_iter()
            .map(|(name, count)| {
                let label = if count > 1 { format!("{} x{}", name, count) } else { name };
                ListItem::new(label)
            })
            .collect()
    };

    let list = List::new(items)
        .block(
            Block::bordered()
            .title("Installed Subsystems")
            .border_style(
                Style::default().fg(color)
            )
        )
        .highlight_style(Style::default().fg(Color::Red));

    StatefulWidget::render(list, area, buf, &mut installed_subsystems_state);

    with_ui_info_mut(|ui_info|{
        ui_info.ship_designer_state.installed_subsystems_state = installed_subsystems_state;
    });
}

fn render_subsystem_list(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    // Only locked subsystems are available for use in ship designs.
    let sub_systems: std::collections::HashMap<_, _> = with_ship_designer(|sd| {
        sd.subsystem_library.iter()
            .filter(|(_, ss)| ss.locked)
            .map(|(k, v)| (k.clone(), v.clone()))
            .collect()
    });
    
    let mut subsystem_tree_state = with_ui_state(|ui_state| ui_state.ship_designer_state.subsystem_tree_state.clone());
    let is_active = with_ui_state(|ui_state| ui_state.ship_designer_state.active_panel == crate::app::ui_state::ShipDesignerPanel::SubsystemLibrary);

    // Group subsystems by their type category.
    let mut categories: Vec<(String, Vec<(String, String)>)> = Vec::new(); // (category_label, [(id, name)])
    for (id, ss) in &sub_systems {
        let cat = ss.subsystem_type.get_subsystem_type_name();
        if let Some(entry) = categories.iter_mut().find(|(c, _)| c == &cat) {
            entry.1.push((id.clone(), ss.name.clone()));
        } else {
            categories.push((cat, vec![(id.clone(), ss.name.clone())]));
        }
    }
    categories.sort_by(|a, b| a.0.cmp(&b.0));

    let items: Vec<TreeItem<String>> = categories
        .into_iter()
        .map(|(cat, mut children)| {
            children.sort_by(|a, b| a.1.cmp(&b.1));
            let leaves: Vec<TreeItem<String>> = children
                .into_iter()
                .map(|(id, name)| TreeItem::new_leaf(id, name))
                .collect();
            TreeItem::new(cat.clone(), cat, leaves).unwrap()
        })
        .collect();

    let block = Block::bordered()
        .title("Subsystem Library")
        .padding(Padding::left(1))
        .border_style(
            Style::default()
            .fg(if is_active { ACTIVE_COLOR } else { INACTIVE_COLOR })
        );
    let inner = block.inner(area);

    block.render(area, buf);
    
    let tree = Tree::new(&items).unwrap()
        .highlight_style(Style::default().fg(Color::Red));
    StatefulWidget::render(tree, inner, buf, &mut subsystem_tree_state);

    with_ui_info_mut(|ui_info|{
        ui_info.ship_designer_state.subsystem_tree_state = subsystem_tree_state;
    });
}

fn render_menu_popup(area: Rect, buf: &mut ratatui::prelude::Buffer) {
    let items = with_ship_designer(|sd| build_ship_designer_menu_entries(&sd.ship_designs));
    let mut menu_popup_state = with_ui_state(|state| state.ship_designer_state.menu_popup_state.clone());

    if items.is_empty() {
        return;
    }

    let selected = menu_popup_state.selected().unwrap_or(0).min(items.len() - 1);
    menu_popup_state.select(Some(selected));

    let longest_label = items.iter().map(|item| item.label.len()).max().unwrap_or(0) as u16;
    let popup_width = (longest_label + 6).max(28).min(area.width.saturating_sub(2).max(1));
    let popup_height = (items.len() as u16 + 4).min(area.height.saturating_sub(2).max(1));
    let popup_area = centered_popup_rect(popup_width, popup_height, area);

    Clear.render(popup_area, buf);

    let block = Block::bordered()
        .title(" Ship Designer Menu ")
        .border_style(Style::default().fg(ACTIVE_COLOR));
    let inner = block.inner(popup_area);
    block.render(popup_area, buf);

    let [list_area, hint_area] =
        Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);

    let list_items: Vec<ListItem> = items
        .iter()
        .map(|item| ListItem::new(item.label.clone()))
        .collect();
    let list = List::new(list_items)
        .highlight_symbol("❯ ")
        .highlight_style(Style::default().fg(ACTIVE_COLOR).bold());
    StatefulWidget::render(list, list_area, buf, &mut menu_popup_state);

    let visible_items = list_area.height as usize;
    let offset = menu_popup_state.offset();
    let remaining = items.len().saturating_sub(offset);
    let item_count = visible_items.min(remaining);
    let item_areas = (0..item_count)
        .map(|row| Rect {
            x: list_area.x,
            y: list_area.y + row as u16,
            width: list_area.width,
            height: 1,
        })
        .collect();

    with_ui_info_mut(|ui_info| {
        ui_info.ship_designer_state.menu_popup_state = menu_popup_state;
        ui_info.ship_designer_menu_popup_area = popup_area;
        ui_info.ship_designer_menu_item_areas = item_areas;
    });

    Paragraph::new("↑/W ↓/S: select | Enter/click: accept | Esc: close")
        .style(Style::default().fg(INACTIVE_COLOR))
        .render(hint_area, buf);
}

fn centered_popup_rect(width: u16, height: u16, area: Rect) -> Rect {
    let width = width.min(area.width);
    let height = height.min(area.height);
    let x = area.x + area.width.saturating_sub(width) / 2;
    let y = area.y + area.height.saturating_sub(height) / 2;
    Rect { x, y, width, height }
}

fn get_subsystem_paragraph(subsystem: &ShipSubsystem) -> Paragraph<'_> {
    let mut text = vec![
        Line::from(subsystem.name.clone()),
        Line::from(format!("Mass: {}", subsystem.mass)),
    ];

    if subsystem.power_output > 0.0 {
        text.push(Line::from(format!(
            "Power Output: {}",
            subsystem.power_output
        )));
    }

    if subsystem.power_consumption > 0.0 {
        text.push(Line::from(format!(
            "Power Consumption: {}",
            subsystem.power_consumption
        )));
    }

    if let SubsystemType::Engines(engine) = &subsystem.subsystem_type {
        text.push(Line::from(format!("Thrust: {}", engine.get_thrust(&subsystem))));
        text.push(Line::from(format!(
            "Fuel Consumption: {}",
            engine.get_fuel_consumption(&subsystem)
        )));
    }

    Paragraph::new(text)
        .block(Block::bordered()
            .title("Subsystem Info"))
}

fn get_ship_info_paragraph() -> Paragraph<'static> {
    // Clone the ship design OUTSIDE any lock so that Engine methods can freely
    // acquire the science-manager lock when computing thrust/fuel stats.
    let ship = with_ship_designer(|sd| sd.current_design.clone());

    let text = if let Some(ship) = ship {
        vec![
            Line::from(format!("Total Mass:              {:.1} t", ship.total_mass() / 1000.0)),
            Line::from(format!("Total Power Output:      {:.1} MW", ship.total_power_output())),
            Line::from(format!("Total Power Consumption: {:.1} MW", ship.total_power_consumption())),
            Line::from(format!("Power Balance:           {:.1} MW", ship.get_power_balance())),
            Line::from(format!("Total Thrust:            {:.1} kN", ship.total_thrust())),
            Line::from(format!("Speed:                   {:.2} km/s", ship.get_speed())),
            Line::from(format!("Endurance:               {}", format_duration(ship.get_endurance()))),
            Line::from(format!("Range:                   {}", format_range(ship.get_total_range()))),
            Line::from(format!("Total Fuel Capacity:     {:.1} t", ship.get_total_fuel_capacity() / 1000.0)),
            Line::from(format!("Total Fuel Consumption:  {:.2} kg/s", ship.total_fuel_consumption())),
        ]
    } else {
        vec![Line::from("No ship design selected")]
    };

    Paragraph::new(text)
        .block(Block::bordered()
            .title("Stats"))
}

fn format_duration(seconds: f64) -> String {
    if !seconds.is_finite() || seconds <= 0.0 {
        return "0s".to_string();
    }
    let total_secs = seconds as u64;
    let days = total_secs / 86400;
    let hours = (total_secs % 86400) / 3600;
    let secs = total_secs % 3600;

    match (days, hours) {
        (0, 0) => format!("{}s", secs),
        (0, _)  => format!("{}h {}s", hours, secs),
        _       => format!("{}d {}h {}s", days, hours, secs),
    }
}

const AU_KM: f64 = 149_597_870.7;

fn format_range(km: f64) -> String {
    if !km.is_finite() || km <= 0.0 {
        return "0 km".to_string();
    }
    let au = km / AU_KM;
    if au < 0.1 {
        format!("{:.1} km", km)
    } else {
        format!("{:.3} AU", au)
    }
}

/// Returns the unique subsystem IDs in order of first appearance (the grouped display order).
fn grouped_subsystem_ids(subsystems: &[ShipSubsystem]) -> Vec<String> {
    let mut ids: Vec<String> = Vec::new();
    for ss in subsystems {
        if !ids.contains(&ss.id) {
            ids.push(ss.id.clone());
        }
    }
    ids
}
