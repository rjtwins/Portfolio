use ratatui::{
    style::{Color, Style},
    widgets::{Block, StatefulWidget},
};
use tui_tree_widget::{Tree, TreeItem};

use crate::{
    ACTIVE_COLOR,
    channels::channels::get_star_map_state,
    with_ui_info_mut,
    with_ui_state,
};

pub fn render(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let mut tree_state = with_ui_state(|s| s.fleets_tree_state.clone());

    let star_map = get_star_map_state();
    let tree_items: Vec<TreeItem<String>> = star_map.stars.first()
        .map(|star| {
            star.fleets
                .iter()
                //.filter(|f| !f.slipway_fleet)
                .map(|f| {
                    let slipway = if f.slipway_fleet {
                        "(SW)"
                    } else {
                        ""
                    };

                    let label = if f.name.is_empty() {
                        format!("{} Fleet ({})", slipway, &f.id[..8.min(f.id.len())])
                    } else {
                        format!("{}{}", slipway, f.name)
                    };
                    TreeItem::new_leaf(f.id.clone(), label)
                })
                .collect()
        })
        .unwrap_or_default();

    let tree = Tree::new(&tree_items)
        .expect("fleet ids are unique")
        .highlight_style(Style::default().fg(ACTIVE_COLOR))
        .block(
            Block::bordered()
                .title("Fleets [↑↓: navigate | Enter: select]")
                .style(Style::default().fg(Color::White)),
        );

    StatefulWidget::render(tree, area, buf, &mut tree_state);

    with_ui_info_mut(|ui_info| {
        ui_info.fleets_tree_state = tree_state;
    });
}
