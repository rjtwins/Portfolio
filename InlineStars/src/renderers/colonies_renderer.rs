use ratatui::{
    layout::Rect,
    buffer::Buffer,
    style::{Color, Style},
    widgets::{Block, StatefulWidget},
};
use tui_tree_widget::{Tree, TreeItem};

use crate::{
    ACTIVE_COLOR,
    get_bodies,
    with_ui_info_mut,
    with_ui_state,
};

pub fn render(area: Rect, buf: &mut Buffer) {
    let mut tree_state = with_ui_state(|s| s.colonies_list_state.clone());

    let bodies = get_bodies();
    let tree_items: Vec<TreeItem<String>> = bodies
        .iter()
        .filter(|b| b.colony.is_some())
        .map(|b| {
            let colony = b.colony.as_ref().unwrap();
            let label = if colony.name == b.name {
                b.name.clone()
            } else {
                format!("{} ({})", colony.name, b.name)
            };
            TreeItem::new_leaf(b.id.clone(), label)
        })
        .collect();

    let tree = Tree::new(&tree_items)
        .expect("colony body ids are unique")
        .highlight_style(Style::default().fg(ACTIVE_COLOR))
        .block(
            Block::bordered()
                .title("Colonies [↑↓: navigate]")
                .style(Style::default().fg(Color::White)),
        );

    StatefulWidget::render(tree, area, buf, &mut tree_state);

    with_ui_info_mut(|ui_info| {
        ui_info.colonies_tree_state = tree_state;
    });
}
