use ratatui::{
    buffer::Buffer,
    layout::{Constraint, Layout, Rect},
    style::{Color, Modifier, Style},
    widgets::{Block, Paragraph, Widget},
};

use crate::{
    ACTIVE_COLOR,
    app::ui_state::{SidebarFocus, SidebarSide},
    renderers::{colonies_renderer, fleets_renderer, system_tree_view_renderer::SystemTreeViewRenderer},
    with_ui_info_mut, with_ui_state,
};

/// Width of the sidebar when fully expanded (columns).
pub const SIDEBAR_WIDTH: u16 = 30;
/// Width of the sidebar when collapsed to a thin strip (columns).
pub const SIDEBAR_COLLAPSED_WIDTH: u16 = 3;

pub struct SidebarRenderer;

impl SidebarRenderer {
    /// Renders the sidebar into `full_area` and returns the remaining content area.
    pub fn render_and_split(full_area: Rect, buf: &mut Buffer) -> Rect {
        let (collapsed, side, colonies_collapsed, planets_collapsed, fleets_collapsed, focus) = with_ui_state(|s| {
            (
                s.sidebar_collapsed,
                s.sidebar_side,
                s.sidebar_colonies_collapsed,
                s.sidebar_planets_collapsed,
                s.sidebar_fleets_collapsed,
                s.sidebar_focus,
            )
        });

        let sidebar_width = if collapsed {
            SIDEBAR_COLLAPSED_WIDTH
        } else {
            SIDEBAR_WIDTH
        };

        let (sidebar_area, content_area) = Self::split_area(full_area, sidebar_width, side);

        if collapsed {
            Self::render_collapsed(sidebar_area, buf, side);
        } else {
            Self::render_expanded(sidebar_area, buf, colonies_collapsed, planets_collapsed, fleets_collapsed, focus, side);
        }

        content_area
    }

    fn split_area(full_area: Rect, sidebar_width: u16, side: SidebarSide) -> (Rect, Rect) {
        match side {
            SidebarSide::Left => {
                let chunks = Layout::horizontal([
                    Constraint::Length(sidebar_width),
                    Constraint::Fill(1),
                ])
                .split(full_area);
                (chunks[0], chunks[1])
            }
            SidebarSide::Right => {
                let chunks = Layout::horizontal([
                    Constraint::Fill(1),
                    Constraint::Length(sidebar_width),
                ])
                .split(full_area);
                (chunks[1], chunks[0])
            }
        }
    }

    fn render_collapsed(area: Rect, buf: &mut Buffer, side: SidebarSide) {
        let arrow = match side {
            SidebarSide::Left => "›",
            SidebarSide::Right => "‹",
        };
        Block::bordered()
            .style(Style::default().fg(Color::DarkGray))
            .render(area, buf);
        let inner = Block::bordered().inner(area);
        if inner.height > 0 && inner.width > 0 {
            let mid_y = inner.y + inner.height / 2;
            Paragraph::new(arrow)
                .style(Style::default().fg(Color::Cyan))
                .render(Rect::new(inner.x, mid_y, inner.width, 1), buf);
        }

        // The entire collapsed strip is the toggle area; no side button in collapsed mode.
        with_ui_info_mut(|info| {
            info.sidebar_toggle_area = area;
            info.sidebar_side_button_area = Rect::default();
            info.sidebar_colonies_header_area = Rect::default();
            info.sidebar_colonies_content_area = Rect::default();
            info.sidebar_planets_header_area = Rect::default();
            info.sidebar_fleets_header_area = Rect::default();
            info.sidebar_planets_content_area = Rect::default();
            info.sidebar_fleets_content_area = Rect::default();
        });
    }

    fn render_expanded(
        area: Rect,
        buf: &mut Buffer,
        colonies_collapsed: bool,
        planets_collapsed: bool,
        fleets_collapsed: bool,
        focus: SidebarFocus,
        side: SidebarSide,
    ) {
        let colonies_content_c = if colonies_collapsed { Constraint::Length(0) } else { Constraint::Fill(1) };
        let planets_content_c  = if planets_collapsed  { Constraint::Length(0) } else { Constraint::Fill(1) };
        let fleets_content_c   = if fleets_collapsed   { Constraint::Length(0) } else { Constraint::Fill(1) };

        let sections = Layout::vertical(vec![
            Constraint::Length(1), // [0] header bar (collapse + side-switch buttons)
            Constraint::Length(1), // [1] colonies header
            colonies_content_c,    // [2] colonies list
            Constraint::Length(1), // [3] planets header
            planets_content_c,     // [4] planets tree
            Constraint::Length(1), // [5] fleets header
            fleets_content_c,      // [6] fleets tree
        ])
        .split(area);

        // Header bar
        let collapse_sym = match side {
            SidebarSide::Left => "[‹]",
            SidebarSide::Right => "[›]",
        };
        let header_chunks = Layout::horizontal([
            Constraint::Length(3),
            Constraint::Fill(1),
            Constraint::Length(3),
        ])
        .split(sections[0]);

        let (collapse_area, side_area) = match side {
            SidebarSide::Left => (header_chunks[0], header_chunks[2]),
            SidebarSide::Right => (header_chunks[2], header_chunks[0]),
        };
        let (left_text, right_text) = match side {
            SidebarSide::Left => (collapse_sym, "[↔]"),
            SidebarSide::Right => ("[↔]", collapse_sym),
        };
        Paragraph::new(left_text)
            .style(Style::default().fg(Color::Cyan))
            .render(header_chunks[0], buf);
        Paragraph::new(right_text)
            .style(Style::default().fg(Color::DarkGray))
            .render(header_chunks[2], buf);

        // ── Colonies section ──────────────────────────────────────────────
        let c_sym = if colonies_collapsed { "▶" } else { "▼" };
        let c_focused = focus == SidebarFocus::Colonies && !colonies_collapsed;
        let c_style = if c_focused {
            Style::default().fg(ACTIVE_COLOR).add_modifier(Modifier::BOLD)
        } else {
            Style::default().fg(Color::White)
        };
        Paragraph::new(format!(" {} COLONIES", c_sym))
            .style(c_style)
            .render(sections[1], buf);
        if !colonies_collapsed && sections[2].height > 0 {
            colonies_renderer::render(sections[2], buf);
        }

        // ── Planets section ───────────────────────────────────────────────
        let p_sym = if planets_collapsed { "▶" } else { "▼" };
        let p_focused = focus == SidebarFocus::Planets && !planets_collapsed;
        let p_style = if p_focused {
            Style::default().fg(ACTIVE_COLOR).add_modifier(Modifier::BOLD)
        } else {
            Style::default().fg(Color::White)
        };
        Paragraph::new(format!(" {} PLANETS", p_sym))
            .style(p_style)
            .render(sections[3], buf);
        if !planets_collapsed && sections[4].height > 0 {
            SystemTreeViewRenderer::render(buf, sections[4]);
        }

        // ── Fleets section ────────────────────────────────────────────────
        let f_sym = if fleets_collapsed { "▶" } else { "▼" };
        let f_focused = focus == SidebarFocus::Fleets && !fleets_collapsed;
        let f_style = if f_focused {
            Style::default().fg(ACTIVE_COLOR).add_modifier(Modifier::BOLD)
        } else {
            Style::default().fg(Color::White)
        };
        Paragraph::new(format!(" {} FLEETS", f_sym))
            .style(f_style)
            .render(sections[5], buf);
        if !fleets_collapsed && sections[6].height > 0 {
            fleets_renderer::render(sections[6], buf);
        }

        // Store all clickable/scroll areas for mouse handler
        with_ui_info_mut(|info| {
            info.sidebar_toggle_area = collapse_area;
            info.sidebar_side_button_area = side_area;

            info.sidebar_colonies_header_area = sections[1];
            info.sidebar_colonies_content_area = if !colonies_collapsed && sections[2].height > 0 {
                Rect { y: sections[1].y, height: sections[1].height + sections[2].height, ..sections[2] }
            } else {
                sections[1]
            };

            info.sidebar_planets_header_area = sections[3];
            info.sidebar_planets_content_area = if !planets_collapsed && sections[4].height > 0 {
                Rect { y: sections[3].y, height: sections[3].height + sections[4].height, ..sections[4] }
            } else {
                sections[3]
            };

            info.sidebar_fleets_header_area = sections[5];
            info.sidebar_fleets_content_area = if !fleets_collapsed && sections[6].height > 0 {
                Rect { y: sections[5].y, height: sections[5].height + sections[6].height, ..sections[6] }
            } else {
                sections[5]
            };
        });
    }
}
