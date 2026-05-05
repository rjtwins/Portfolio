use ratatui::{
    layout::{Constraint, Layout},
    style::{Color, Style, Stylize},
    text::Line,
    widgets::{Block, List, ListItem, Padding, Paragraph, StatefulWidget, Widget},
};

use rayon::vec;
use tui_tree_widget::*;

use crate::{
    ACTIVE_COLOR, INACTIVE_COLOR, app::{
        application::FRAME_TIME, effects::COALESCE, science_manager::with_science_manager,
        ship_desginer::with_ship_designer, ui_state::SubSystemDesignerPanel,
    }, channels::channels::HAS_JUST_TAB, entities::ship::Engine, with_ui_info_mut, with_ui_state
};

pub fn render(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let [left, right] = Layout::default()
        .direction(ratatui::layout::Direction::Horizontal)
        .constraints([Constraint::Percentage(30), Constraint::Percentage(70)])
        .areas(area);

    render_subsystem_tree(left, buf);
    render_subsystem_details(right, buf);

    COALESCE.with(|effect| {
        if HAS_JUST_TAB.swap(false, std::sync::atomic::Ordering::Relaxed) {
            effect.borrow_mut().reset();
        }
        effect.borrow_mut().process(FRAME_TIME.into(), buf, area);
    });
}

fn render_subsystem_tree(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let is_active = with_ui_state(|state| {
        state.subsystem_renderer_state.panel_state.active_panel() == SubSystemDesignerPanel::SubsystemLibrary
    });

    let mut tree_state =
        with_ui_state(|state| state.subsystem_renderer_state.subsystem_tree_state.clone());

    let sub_systems = with_ship_designer(|sd| sd.subsystem_library.clone());

    let mut storage = TreeItem::new_leaf("Storage".to_string(), "STORAGE");
    let mut engines = TreeItem::new_leaf("engines".to_string(), "ENGINES");
    let mut reactors = TreeItem::new_leaf("reactors".to_string(), "REACTORS");
    let mut sensors = TreeItem::new_leaf("sensors".to_string(), "SENSORS");
    let mut weapons = TreeItem::new_leaf("weapons".to_string(), "WEAPONS");
    let mut misc = TreeItem::new_leaf("misc".to_string(), "MISC");

    sub_systems
        .into_iter()
        .for_each(|(id, ss)| {
            let label = if ss.locked {
                format!("L {}", ss.name)
            } else {
                ss.name.clone()
            };
            match ss.subsystem_type {
                crate::entities::ship::SubsystemType::Hanger(_) => {}
                crate::entities::ship::SubsystemType::Misc => {
                    let _ = misc.add_child(TreeItem::new_leaf(id, label));
                }
                crate::entities::ship::SubsystemType::Engines(_) => {
                    let _ = engines.add_child(TreeItem::new_leaf(id, label));
                }
                crate::entities::ship::SubsystemType::Reactor => {
                    let _ = reactors.add_child(TreeItem::new_leaf(id, label));
                }
                crate::entities::ship::SubsystemType::Sensors(_) => {
                    let _ = sensors.add_child(TreeItem::new_leaf(id, label));
                }
                crate::entities::ship::SubsystemType::Weapons(_) => {
                    let _ = weapons.add_child(TreeItem::new_leaf(id, label));
                }
                crate::entities::ship::SubsystemType::Storage(_) => {
                    let _ = storage.add_child(TreeItem::new_leaf(id, label));
                }
                crate::entities::ship::SubsystemType::ColonyModule => {
                    let _ = misc.add_child(TreeItem::new_leaf(id, label));
                }
            }
        });

    let border_color = if is_active {
        ACTIVE_COLOR
    } else {
        INACTIVE_COLOR
    };

    let bock = Block::bordered()
        .border_style(Style::default().fg(border_color))
        .padding(Padding::top(0));

    let inner = bock.inner(area);

    let [instructions_area, tree_area] = Layout::vertical(vec![
        Constraint::Min(6),
        Constraint::Fill(100)
    ]).areas(inner);

    let instructions = vec![
        Line::from("[A: add]"),
        Line::from("[E: copy+edit]"),
        Line::from("[O: obsolete]"),
        Line::from("[L: lock]"),
        Line::from("[D: delete]"),
    ];

    let instructions_paragraph = Paragraph::new(instructions);

    let full_tree = vec![storage, engines, reactors, sensors, weapons, misc];
    let tree = Tree::new(&full_tree)
        .unwrap()
        // .block(
        //     Block::bordered()
        //         .title("[A to Add][E to Copy+Edit][O to toggle Obsolete][D to Delete]")
        //         .border_style(Style::default().fg(border_color))
        //         .padding(Padding::top(1)),
        // )
        .highlight_style(Style::default().fg(Color::Red));

    bock.render(area, buf);
    instructions_paragraph.render(instructions_area, buf);
    StatefulWidget::render(tree, tree_area, buf, &mut tree_state);

    with_ui_info_mut(|ui_info| {
        ui_info.subsystem_renderer_info.subsystem_tree_state = tree_state;
    });
}

fn render_subsystem_details(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let selected = with_ui_state(|state| {
        state
            .subsystem_renderer_state
            .subsystem_tree_state
            .selected()
            .last()
            .cloned()
    });

    let selected = match selected {
        Some(id) => {
            let from_library = with_ship_designer(|sd| sd.subsystem_library.get(&id).cloned());
            if from_library.is_none() {
                return;
            }
            from_library.unwrap()
        }
        None => {
            let paragraph = Paragraph::new("No subsystem selected")
                .block(Block::bordered().title("Subsystem Details"));
            Widget::render(paragraph, area, buf);
            return;
        }
    };

    match selected.subsystem_type {
        crate::entities::ship::SubsystemType::Engines(_) => {
            render_engine_details(area, buf, &selected);
        }
        crate::entities::ship::SubsystemType::Reactor => {

        }
        crate::entities::ship::SubsystemType::Sensors(_) => {

        }
        crate::entities::ship::SubsystemType::Weapons(_) => {

        }
        _ => {
            return;
        }
    }
}

fn render_engine_details(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer, selected: &crate::entities::ship::ShipSubsystem) {
    let is_engine_tech_panel_active = with_ui_state(|state| {
        state.subsystem_renderer_state.panel_state.active_panel() == SubSystemDesignerPanel::EngineTechs
    });

    let is_engine_size_panel_active = with_ui_state(|state| {
        state.subsystem_renderer_state.panel_state.active_panel() == SubSystemDesignerPanel::EngineSizes
    });

    let engine_techs_border_color = if is_engine_tech_panel_active {
        ACTIVE_COLOR
    } else {
        INACTIVE_COLOR
    }; 
    
    let engine_sizes_border_color = if is_engine_size_panel_active {
        ACTIVE_COLOR
    } else {
        INACTIVE_COLOR
    }; 

    let [left, right] = Layout::horizontal(vec![
        Constraint::Percentage(20),
        Constraint::Percentage(80),
    ]).areas(area);

    let [left_top, left_bottom, left_fill] = Layout::vertical(vec![
        Constraint::Max(3),
        Constraint::Max(3),
        Constraint::Fill(100)
    ]).areas(left);

    let mut engine_tech_tree_state = with_ui_state(|state| state.subsystem_renderer_state.engine_tech_tree_state.clone());

    let engine_tech_list = with_science_manager(|sm| sm.get_engine_techs());
    let engine_tech_tree_items = engine_tech_list
        .iter()
        .map(|tech| TreeItem::new_leaf(tech.id.clone(), tech.name.clone()))
        .collect::<Vec<_>>();

    let engine_techs_tree = Tree::new(&engine_tech_tree_items)
        .unwrap()
        .block(
            Block::bordered()
            .title("Associated Engine Techs")
            .border_style(
                Style::default()
                .fg(engine_techs_border_color)
            )
        )
        .highlight_style(Style::default().fg(Color::Green));

    StatefulWidget::render(engine_techs_tree, left_top, buf, &mut engine_tech_tree_state);

    let engine_size = format!("{:.3}t", (selected.clone().mass / 1000.0));

    let engine_sizes_paragraph = Paragraph::new(engine_size)
        .block(
            Block::bordered()
            .title("Engine Size [⇅ to change]")
            .padding(Padding::left(1))
            .border_style(
                Style::default()
                .fg(engine_sizes_border_color)
            )
        );

    engine_sizes_paragraph.render(left_bottom, buf);

    let subsystem_engine = match selected.clone().subsystem_type {
        crate::entities::ship::SubsystemType::Engines(engine) => engine,
        _ => return,
    };

    //INFO
    let engine_info = vec![
        Line::from(format!("Mass: {:.3}t", selected.mass / 1000.0)),
        Line::from(format!("Thrust: {:.1} kg", subsystem_engine.get_thrust(selected))),
        Line::from(format!("Fuel Consumption: {:.1} kg/h", subsystem_engine.get_fuel_consumption(selected) * 60.0 * 60.0)),
        Line::from(format!("Fuel Type: {}", subsystem_engine.get_fuel_type().as_str())),
    ];

    let engine_info_paragraph = Paragraph::new(engine_info)
        .block(Block::bordered().title("Engine Info").padding(Padding{left: 1, top: 1, ..Default::default()}));

    engine_info_paragraph.render(right, buf);

    with_ui_info_mut(|ui_info| {
        ui_info.subsystem_renderer_info.engine_tech_tree_state = engine_tech_tree_state;
    });
}
