use ratatui::{
    layout::{Constraint, Layout, Rect},
    style::{Color, Style, Stylize},
    symbols,
    widgets::{Block, Clear, List, Widget},
};

use crate::{
    app::{ui_info, ui_state},
    channels::channels::{get_ui_info_from_channel, get_ui_state_from_channel},
    extentions::tabs_extentions::ClickableTabs,
    renderers::{
        fleets_renderer,
        system_overview_renderer::SystemOverviewRenderer,
        system_tree_view_renderer::SystemTreeViewRenderer,
    },
    with_ui_info_mut, with_ui_state_mut,
};

pub struct DetailRenderer {}

impl DetailRenderer {
    pub fn render(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
        let ui_state: crate::app::ui_state::UIState = get_ui_state_from_channel();

        let [tab_area, content_area] =
            Layout::vertical(vec![Constraint::Min(3), Constraint::Fill(100)]).areas(area);

        let tab_content = vec!["OVERVIEW", "TREE", "FLEETS"];
        let tabs = ClickableTabs::new(tab_content.clone())
            .style(Color::White)
            .block(Block::bordered().style(Style::default().fg(Color::White)))
            .highlight_style(Style::default().fg(Color::Red).bold())
            .select(0)
            .divider(symbols::line::VERTICAL)
            .padding(" ", " ")
            .select(ui_state.selected_detail_tab as usize);

        let tab_areas = tabs.render_into_areas(tab_area, buf);
        with_ui_info_mut(|ui_info| {
            ui_info.detail_tab_areas = tab_areas;
        });

        match ui_state.selected_detail_tab {
            ui_state::SelectedDetailTab::Overview => {
                SystemOverviewRenderer::render(buf, content_area);
            }
            ui_state::SelectedDetailTab::TreeView => {
                SystemTreeViewRenderer::render(buf, content_area);
            }
            ui_state::SelectedDetailTab::Fleets => {
                fleets_renderer::render(content_area, buf);
            }
        }

        with_ui_info_mut(|ui_info| {
            ui_info.detail_area = area;
        });
    }
}
