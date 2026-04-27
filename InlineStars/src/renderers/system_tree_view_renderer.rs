use ratatui::{Frame, buffer::Buffer, layout::Rect, style::{Color, Style}, widgets::Padding};
use tui_tree_widget::{Block, Tree, TreeItem};

use crate::{App, UIInfo, channels::channels::{get_star_map_state, get_ui_state_from_channel}, entities::{GameEntity, planet::Body}, with_ui_info_mut};

pub struct SystemTreeViewRenderer{

}

impl SystemTreeViewRenderer {
    pub fn render(buf: &mut Buffer, main_area: Rect) {

        Self::draw_system_tree_view(buf, main_area);
    }

    fn draw_system_tree_view(buf: &mut Buffer, main_area: Rect) {
        let star_map = get_star_map_state();
        let Some(star) = star_map.stars.first() else {
            return;
        };

        let tree_items = vec![Self::build_star_tree_item(star)];
        let tree = Tree::new(&tree_items)
            .expect("star tree identifiers should be unique")
            .block(
                Block::bordered()
                    .style(Style::default().fg(Color::White))
                    .padding(Padding::left(1)),
            )
            .highlight_style(Style::default().fg(Color::Red).bold().slow_blink());

        let mut ui_state = get_ui_state_from_channel();

        // if ui_state.system_tree_state.selected().is_empty() {
        //     ui_state.system_tree_state.select(vec![star.uuid]);
        //     ui_state.system_tree_state.open(vec![star.uuid]);
        // }

        ratatui::widgets::StatefulWidget::render(tree, main_area, buf, &mut ui_state.system_tree_state);

        with_ui_info_mut(|ui_info: &mut UIInfo|{
            ui_info.system_tree_state = ui_state.system_tree_state.clone();
        });

    }

    fn build_star_tree_item(star: &crate::entities::star::Star) -> TreeItem<'static, String> {
        let mut bodies = star.bodies.clone();
        bodies.sort_by(|a, b| {
            let a_axis = a.orbit.as_ref().map(|o| o.semi_major_axis).unwrap_or(0.0);
            let b_axis = b.orbit.as_ref().map(|o| o.semi_major_axis).unwrap_or(0.0);
            a_axis.total_cmp(&b_axis)
        });

        let children = bodies
            .iter()
            .map(|body| Self::build_body_tree_item(body))
            .collect::<Vec<_>>();

        TreeItem::new(
            star.id.clone(),
            format!("Star: {}", star.get_name()),
            children,
        )
        .expect("star body identifiers should be unique")
    }

    fn build_body_tree_item(body: &Body) -> TreeItem<'static, String> {
        let mut moons = body.moons.clone();
        moons.sort_by(|a, b| {
            let a_axis = a.orbit.as_ref().map(|o| o.semi_major_axis).unwrap_or(0.0);
            let b_axis = b.orbit.as_ref().map(|o| o.semi_major_axis).unwrap_or(0.0);
            a_axis.total_cmp(&b_axis)
        });

        let children = moons
            .iter()
            .map(|moon| Self::build_body_tree_item(moon))
            .collect::<Vec<_>>();

        let label = if body.colony.is_some() {
            format!("{} [Colony]", body.name)
        } else {
            body.name.clone()
        };

        if children.is_empty() {
            TreeItem::new_leaf(body.id.clone(), label)
        } else {
            TreeItem::new(body.id.clone(), label, children)
                .expect("moon identifiers should be unique")
        }
    }

}