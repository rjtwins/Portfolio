use ratatui::{
    buffer::Buffer,
    layout::{self, Constraint, Layout, Rect},
    style::{Color, Style},
    text::{Line, Text},
    widgets::{Padding, Paragraph, Widget},
};
use tui_tree_widget::Block;

use crate::{
    centered_line_rect,
    channels::channels::{get_selected_body_id, get_star_map_state},
    entities::{
        GameEntity,
        planet::{body::BodyResources, Body},
    },
    fit_system_line, get_body_by_id, with_ui_info_mut,
};

pub struct SystemOverviewRenderer {}

impl SystemOverviewRenderer {
    pub fn render(buf: &mut Buffer, main_area: Rect) {
        let star_map = get_star_map_state();
        let star = &star_map.stars[0];
        let selected_body = get_selected_body_id().and_then(get_body_by_id);
        let info_height = if selected_body.is_some() { 7 } else { 0 };
        let [info_area, overview_area] =
            Layout::vertical([Constraint::Length(info_height), Constraint::Min(0)]).areas(main_area);

        if let Some(body) = selected_body {
            Self::render_selected_body_info(buf, info_area, &body);
        }

        let mut orbitals = star
            .bodies
            .iter()
            .filter(|body| body.body_type.is_major_orbital())
            .cloned()
            .collect::<Vec<_>>();

        orbitals.sort_by(|a, b| {
            let a_axis = a.orbit.as_ref().map(|o| o.semi_major_axis).unwrap_or(0.0);
            let b_axis = b.orbit.as_ref().map(|o| o.semi_major_axis).unwrap_or(0.0);
            a_axis.total_cmp(&b_axis)
        });

        let block = Block::bordered().padding(Padding::top(1));
        let inner = block.inner(overview_area);
        let max_width = inner.width as usize;

        let mut lines: Vec<Line> = Vec::new();
        let mut system_overview_info: Vec<(String, Rect)> = Vec::new();
        let star_name = format!("({})", star.get_name());
        let left_padding = " ".repeat(star_name.len());
        let star_line = Line::from(format!("{left_padding} ☼ {star_name}"));

        system_overview_info.push((
            star.id.clone(),
            centered_line_rect(inner, lines.len() as u16, star_line.width() as u16),
        ));
        lines.push(star_line);
        lines.push(Line::from("│"));
        lines.push(Line::from("──────────────────┼─────────────────"));

        for orbital in &orbitals {
            let moons = orbital
                .moons
                .iter()
                .map(|_| "o".to_string())
                .collect::<Vec<String>>()
                .join("─");
            let moons = if moons.is_empty() {
                String::new()
            } else {
                format!("─{moons}")
            };

            let mut line = Line::from(fit_system_line(&orbital.name, &moons, max_width));

            if let Some(selected_id) = get_selected_body_id() {
                if selected_id == orbital.id.as_str() {
                    line = line.style(Style::default().fg(Color::Red).slow_blink());
                } else if orbital
                    .moons
                    .iter()
                    .any(|moon| moon.id.as_str() == selected_id)
                {
                    line = line.style(Style::default().fg(Color::Yellow).slow_blink());
                }
            }

            lines.push(Line::from("│"));
            system_overview_info.push((
                orbital.id.clone(),
                centered_line_rect(inner, lines.len() as u16, line.width() as u16),
            ));
            lines.push(line);
            lines.push(Line::from("──────────────────┼─────────────────"));
        }

        with_ui_info_mut(|ui_info| {
            ui_info.system_overview_info = system_overview_info;
        });

        block.render(overview_area, buf);
        Paragraph::new(lines)
            .alignment(layout::HorizontalAlignment::Center)
            .render(inner, buf);
    }

    pub fn selected_body_info_height(body: &Body) -> u16 {
        let mut content_lines = 3u16;
        if body.body_type.mantle_layer_name().is_some() {
            content_lines += 1;
        }
        if body.body_type.core_layer_name().is_some() {
            content_lines += 1;
        }

        content_lines + 2
    }

    pub fn render_selected_body_info(buf: &mut Buffer, area: Rect, body: &Body) {
        if area.height == 0 {
            return;
        }

        let block = Block::bordered().title("Selected Body");
        let inner = block.inner(area);
        block.render(area, buf);

        let mut lines = vec![Line::from(format!(
            "{} ({})",
            body.name,
            body.body_type.display_name()
        ))];

        lines.push(Line::from(format!(
            "Mass: {} | Radius: {}",
            format_mass(body.mass_kg),
            format_radius(body.radius_km),
        )));

        lines.push(Self::format_resource_line(
            body.body_type.surface_layer_name(),
            &body.surface_resources,
        ));

        if let Some(label) = body.body_type.mantle_layer_name() {
            lines.push(Self::format_resource_line(label, &body.mantle_resources));
        }

        if let Some(label) = body.body_type.core_layer_name() {
            lines.push(Self::format_resource_line(label, &body.core_resources));
        }

        Paragraph::new(Text::from(lines)).render(inner, buf);
    }

    fn format_resource_line(label: &str, resources: &BodyResources) -> Line<'static> {
        let values = resources.amounts();
        Line::from(format!(
            "{label}: F {} | LM {} | HM {} | RE {} | SE {}",
            format_resource_amount(values.fuel),
            format_resource_amount(values.light_metals),
            format_resource_amount(values.heavy_metals),
            format_resource_amount(values.rare_elements),
            format_resource_amount(values.super_elements),
        ))
    }
}

fn format_resource_amount(value: f64) -> String {
    let abs = value.abs();
    if abs >= 1_000_000_000_000.0 {
        format!("{:.1}T", value / 1_000_000_000_000.0)
    } else if abs >= 1_000_000_000.0 {
        format!("{:.1}B", value / 1_000_000_000.0)
    } else if abs >= 1_000_000.0 {
        format!("{:.1}M", value / 1_000_000.0)
    } else if abs >= 1_000.0 {
        format!("{:.1}K", value / 1_000.0)
    } else {
        format!("{value:.1}")
    }
}

fn format_mass(value: f64) -> String {
    let abs = value.abs();
    if abs >= 1_000_000_000_000_000.0 {
        format!("{value:.2e} kg")
    } else if abs >= 1_000_000_000_000.0 {
        format!("{:.2}Tkg", value / 1_000_000_000_000.0)
    } else if abs >= 1_000_000_000.0 {
        format!("{:.2}Bkg", value / 1_000_000_000.0)
    } else if abs >= 1_000_000.0 {
        format!("{:.2}Mkg", value / 1_000_000.0)
    } else if abs >= 1_000.0 {
        format!("{:.2}Kkg", value / 1_000.0)
    } else {
        format!("{value:.2} kg")
    }
}

fn format_radius(value: f64) -> String {
    if value.abs() >= 1_000_000.0 {
        format!("{:.2}M km", value / 1_000_000.0)
    } else if value.abs() >= 1_000.0 {
        format!("{:.2}K km", value / 1_000.0)
    } else {
        format!("{value:.2} km")
    }
}
