use ratatui::{
    layout::{Constraint, Layout},
    style::{Color, Style, palette::material::INDIGO},
    text::Line,
    widgets::{Block, Paragraph, StatefulWidget, Widget},
};

use tui_tree_widget::*;

use crate::{ACTIVE_COLOR, INACTIVE_COLOR, app::{application::FRAME_TIME, effects::{self, COALESCE}, science_manager::with_science_manager, ui_state::ScienceRendererPanel}, channels::channels::HAS_JUST_TAB, with_ui_info_mut, with_ui_state};

pub fn render(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    //let queue = with_ui_state(|state| state.science_renderer_state.research_queue.clone());

    let [list_area, right_area] =
        Layout::horizontal(vec![Constraint::Percentage(30), Constraint::Percentage(70)])
            .areas(area);
    let [queue_area, detail_area] =
        Layout::vertical(vec![Constraint::Percentage(50), Constraint::Percentage(50)])
            .areas(right_area);

    let detail_block = Block::bordered().title("Details");
    let detail_inner_area = detail_block.inner(detail_area);
    detail_block.render(detail_area, buf);

    render_research_items(list_area, buf);
    render_research_queue(queue_area, buf);
    render_detail(detail_inner_area, buf);

    COALESCE.with(|effect| { 
        if HAS_JUST_TAB.swap(false, std::sync::atomic::Ordering::Relaxed){
            effect.borrow_mut().reset();
        }
        effect.borrow_mut().process(FRAME_TIME.into(), buf, area);
    });
}

fn render_research_queue(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let mut queue_state = with_ui_state(|state| state.science_renderer_state.research_queue.clone());
    let queue_items = with_science_manager(|m| m.get_items_in_queue());

    let tree_items: Vec<TreeItem<String>> = queue_items
        .iter()
        .map(|item| TreeItem::new_leaf(item.id.clone(), item.name.clone()))
        .collect();

    let active = with_ui_state(|state| state.science_renderer_state.active_panel.clone()) == ScienceRendererPanel::ResearchQueue;
    let color = if active {
        ACTIVE_COLOR
    } else {
        INACTIVE_COLOR
    };

    let tree = Tree::new(&tree_items)
        .expect("all item ids are unique")
        .highlight_style(Style::default().fg(Color::Red))
        .block(
            Block::bordered()
            .style(Style::default().fg(color))
            .title("[DEL: dequeue][+: up][-: down]")
        );
    
    StatefulWidget::render(tree, area, buf, &mut queue_state);

    with_ui_info_mut(|ui_info| {
        ui_info.science_renderer_info.research_queue = queue_state;
    });
}

fn render_research_items(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let mut list_state = with_ui_state(|state| state.science_renderer_state.research_list.clone());
    let items = with_science_manager(|m| m.get_possible_items());
    let tree_items: Vec<TreeItem<String>> = items
        .iter()
        .map(|item| TreeItem::new_leaf(item.id.clone(), item.name.clone()))
        .collect();

    let active = with_ui_state(|state| state.science_renderer_state.active_panel.clone()) == ScienceRendererPanel::ResearchList;
    let color = if active {
        ACTIVE_COLOR
    } else {
        INACTIVE_COLOR
    };

    let tree = Tree::new(&tree_items)
        .expect("all item ids are unique")
        .highlight_style(Style::default().fg(Color::Red))
        .block(
            Block::bordered()
            .style(Style::default().fg(color))
            .title("[ENTER: queue]")
        );

    StatefulWidget::render(tree, area, buf, &mut list_state);

    with_ui_info_mut(|ui_info| {
        ui_info.science_renderer_info.research_list = list_state;
    });
}

fn render_detail(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let active_panel = with_ui_state(|state| state.science_renderer_state.active_panel.clone());
    let mut state: TreeState<String> = TreeState::default();


    if active_panel == ScienceRendererPanel::ResearchList {
        state = with_ui_state(|ui_state| ui_state.science_renderer_state.research_list.clone());
    } else {
        state = with_ui_state(|ui_state| ui_state.science_renderer_state.research_queue.clone());
    }

    let selected_item = match state.selected().last() {
        Some(item) => item,
        None => return,
    };

    let item = match with_science_manager(|m| m.items.get(selected_item).cloned()) {
        Some(item) => item,
        None => return,
    };

    let mut text = vec![
        Line::from(format!("Name: {}", item.name)),
        Line::from(format!("Description: {}", item.description)),
        Line::from(format!("Cost: {}", item.cost)),
        Line::from(format!(
            "Prerequisites: {}",
            item.prerequisites
                .iter()
                .filter_map(|id| with_science_manager(|m| m.items.get(id).map(|i| i.name.clone())))
                .collect::<Vec<String>>()
                .join(", ")
        )),
    ];

    if active_panel == ScienceRendererPanel::ResearchQueue {
        let progress_percentage = (item.progress * 100.0).round();
        let progress_line = Line::from(format!("Progress: {}%", progress_percentage));
        text.push(progress_line);
    }

    Paragraph::new(text).render(area, buf);
}
