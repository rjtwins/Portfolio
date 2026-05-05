use ratatui::{
    layout::{Position, Rect},
    style::{Color, Style},
    widgets::{canvas::Canvas, Block, Clear, Paragraph, Widget},
};

use crate::{
    channels::channels::{get_camera_state, get_selected_body_id, get_star_map_state},
    entities::{planet::{body::BodyType, Body}, GameEntity},
    get_fleets,
    renderers::body_renderer::BodyRenderer,
    should_render_moons,
    with_ui_info_mut,
    with_ui_state,
    MOON,
    PLANET,
};
use crate::renderers::star_renderer::StarRenderer;

const STAR_MAP_DETAIL_LABELS: [&str; 4] = [
    "Show orbits (O)",
    "Show names (L)",
    "Show asteroids (A)",
    "Show comets (C)",
];

#[derive(Clone)]
pub struct StarMapRenderer {
    block: Option<Block<'static>>,
}

impl StarMapRenderer {
    pub fn block(mut self, block: Block<'static>) -> Self {
        self.block = Some(block);
        self
    }

    pub fn new() -> Self {
        Self { block: None }
    }

    pub fn render(&self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) -> Rect
    where
        Self: Sized,
    {
        let ui_state = with_ui_state(|ui_state| ui_state.clone());

        Clear.render(area, buf);
        self.block.as_ref().render(area, buf);
        let area = self.block.as_ref().map(|f| f.inner(area)).unwrap_or(area);
        let map_area = area;

        let star_map = get_star_map_state();
        let mut camera = get_camera_state().as_ref().clone();
        if get_selected_body_id().is_some() {
            camera.update(0.0);
        }

        let star = star_map.stars.first().cloned().unwrap();
        let filter_text = ui_state.star_map_filter_text.trim().to_lowercase();

        let bodies: Vec<Body> = star.bodies.into_iter()
            .filter(|b| {
                if !ui_state.star_map_show_asteroids && b.body_type.is_asteroid() {
                    return false;
                }
                if !ui_state.star_map_show_comets && b.body_type == BodyType::Comet {
                    return false;
                }
                true
            })
            .collect();

        let planets: Vec<Body> = bodies
            .iter()
            .filter(|b| fuzzy_name_match(&filter_text, &b.get_name()))
            .cloned()
            .collect();

        let moons: Vec<Body> = bodies
            .iter()
            .flat_map(|p| p.moons.clone())
            .filter(|moon| fuzzy_name_match(&filter_text, &moon.get_name()))
            .collect();

        let mut planet_renderers: Vec<BodyRenderer<Body>> = planets
            .iter()
            .map(|planet| BodyRenderer::new_from_body(planet, Color::Blue, PLANET.to_string(), "P".to_string()))
            .collect();

        let mut moon_renderers: Vec<BodyRenderer<Body>> = if should_render_moons() {
            moons
                .iter()
                .map(|moon| BodyRenderer::new_from_body(moon, Color::Gray, MOON.to_string(), "M".to_string()))
                .collect()
        } else {
            vec![]
        };

        let fleets = get_fleets()
            .into_iter()
            .filter(|fleet| fuzzy_name_match(&filter_text, &fleet.get_name()))
            .collect::<Vec<_>>();
        let mut fleet_renderers: Vec<BodyRenderer<_>> = fleets
            .iter()
            .map(|f| BodyRenderer::new_from_fleet(f, Color::Red, "*".to_string(), "F".to_string()))
            .collect();

        // update_render_state populates is_selected, is_mouse_over, should_be_rendered,
        // and screen_position — all needed for the label overlay pass.
        planet_renderers.iter_mut().for_each(|r| r.update_render_state(map_area));
        moon_renderers.iter_mut().for_each(|r| r.update_render_state(map_area));
        fleet_renderers.iter_mut().for_each(|r| r.update_render_state_fleet(map_area));

        planet_renderers.sort_by_key(|r| (r.is_selected, r.is_mouse_over));
        moon_renderers.sort_by_key(|r| (r.is_selected, r.is_mouse_over));
        fleet_renderers.sort_by_key(|r| (r.is_selected, r.is_mouse_over));

        with_ui_info_mut(|ui_info | {
            planet_renderers.iter().for_each(|p| { ui_info.star_map_info.insert(p.id.clone(), Position { x: p.buf_pos.0, y: p.buf_pos.1 }); });
            moon_renderers.iter().for_each(|m| { ui_info.star_map_info.insert(m.id.clone(), Position { x: m.buf_pos.0, y: m.buf_pos.1 }); });
            fleet_renderers.iter().for_each(|f| { ui_info.star_map_info.insert(f.id.clone(), Position { x: f.buf_pos.0, y: f.buf_pos.1 }); });
        });

        let star_renderers: Vec<StarRenderer> = star_map.stars.iter().map(|s| StarRenderer { star: s }).collect();

        let (x_bounds, y_bounds) = camera.canvas_bounds(map_area);

        // --- Canvas pass: all spatial geometry ---
        let canvas = Canvas::default()
            .x_bounds(x_bounds)
            .y_bounds(y_bounds)
            .paint(|ctx| {
                for sr in &star_renderers {
                    sr.draw_on_canvas(ctx);
                }
                for r in &moon_renderers {
                    r.draw_on_canvas(ctx);
                }
                for r in &planet_renderers {
                    r.draw_on_canvas(ctx);
                }
                for r in &fleet_renderers {
                    r.draw_on_canvas(ctx);
                }
            });

        canvas.render(map_area, buf);

        // --- Label overlay pass: Popup widgets written directly to buf ---
        let show_names = ui_state.star_map_show_names;
        for r in planet_renderers.iter().chain(moon_renderers.iter()) {
            if r.should_be_rendered && (r.is_selected || r.is_mouse_over || show_names) {
                r.render_label_pub(r.in_area_pos, map_area, buf);
            }
        }
        for r in &fleet_renderers {
            if r.should_be_rendered && (r.is_selected || r.is_mouse_over || show_names) {
                r.render_label_pub(r.in_area_pos, map_area, buf);
            }
        }

        // --- Context menu overlay ---
        let context_menu = with_ui_state(|s| s.context_menu.clone());
        if context_menu.visible {
            if let Some(ref pi) = context_menu.pending_input {
                // --- Distance input popup ---
                let prompt = format!("Distance (km  or  N au):");
                let input_line = format!("> {}_", pi.value);
                let hint = "Enter: confirm | ESC: cancel";
                let popup_w = (prompt.len().max(input_line.len()).max(hint.len()) as u16 + 4).min(buf.area.width);
                let popup_h = 5;
                let x = context_menu.screen_pos.0.min(buf.area.width.saturating_sub(popup_w));
                let y = context_menu.screen_pos.1.min(buf.area.height.saturating_sub(popup_h));
                let popup_rect = Rect::new(x, y, popup_w, popup_h);

                Clear.render(popup_rect, buf);
                Block::bordered()
                    .title("Keep Distance")
                    .style(Style::default().fg(Color::Yellow))
                    .render(popup_rect, buf);

                Paragraph::new(prompt).style(Style::default().fg(Color::Gray))
                    .render(Rect::new(x + 1, y + 1, popup_w - 2, 1), buf);
                Paragraph::new(input_line).style(Style::default().fg(Color::White))
                    .render(Rect::new(x + 1, y + 2, popup_w - 2, 1), buf);
                Paragraph::new(hint).style(Style::default().fg(Color::DarkGray))
                    .render(Rect::new(x + 1, y + 3, popup_w - 2, 1), buf);

                with_ui_info_mut(|ui_info| { ui_info.context_menu_option_areas.clear(); });

            } else if !context_menu.entries.is_empty() {
                // --- Normal orders list ---
                let entry_count = context_menu.entries.len() as u16;
                let max_label = context_menu.entries.iter().map(|e| e.label.len()).max().unwrap_or(10) as u16;
                let popup_w = (max_label + 4).min(buf.area.width);
                let popup_h = entry_count + 2;
                let x = context_menu.screen_pos.0.min(buf.area.width.saturating_sub(popup_w));
                let y = context_menu.screen_pos.1.min(buf.area.height.saturating_sub(popup_h));
                let popup_rect = Rect::new(x, y, popup_w, popup_h);

                Clear.render(popup_rect, buf);
                Block::bordered()
                    .title("Orders")
                    .style(Style::default().fg(Color::Yellow))
                    .render(popup_rect, buf);

                let option_areas: Vec<Rect> = context_menu.entries.iter().enumerate().map(|(i, entry)| {
                    let row = Rect::new(x + 1, y + 1 + i as u16, popup_w - 2, 1);
                    Paragraph::new(entry.label.as_str())
                        .style(Style::default().fg(Color::White))
                        .render(row, buf);
                    row
                }).collect();

                with_ui_info_mut(|ui_info| { ui_info.context_menu_option_areas = option_areas; });
            } else {
                with_ui_info_mut(|ui_info| { ui_info.context_menu_option_areas.clear(); });
            }
        } else {
            with_ui_info_mut(|ui_info| { ui_info.context_menu_option_areas.clear(); });
        }

        self.render_details_menu(area, buf, &ui_state);

        map_area
    }

    fn render_details_menu(
        &self,
        area: Rect,
        buf: &mut ratatui::prelude::Buffer,
        ui_state: &crate::UIState,
    ) -> Rect {
        if area.is_empty() {
            with_ui_info_mut(|ui_info| {
                ui_info.star_map_details_toggle_area = Rect::default();
                ui_info.star_map_details_option_areas.clear();
                ui_info.star_map_details_filter_area = Rect::default();
            });
            return area;
        }

        let toggle_label = if ui_state.star_map_details_menu_expanded {
            "Map details [-]"
        } else {
            "Map details [+]"
        };
        let filter_value = if ui_state.star_map_filter_editing {
            format!("Filter (F): {}_", ui_state.star_map_filter_text)
        } else {
            format!("Filter (F): {}", ui_state.star_map_filter_text)
        };
        let max_option_width = STAR_MAP_DETAIL_LABELS
            .iter()
            .map(|label| label.len() + 4)
            .max()
            .unwrap_or(0);
        let inner_width = toggle_label
            .len()
            .max(max_option_width)
            .max(filter_value.len()) as u16;
        let inner_height = if ui_state.star_map_details_menu_expanded {
            1 + STAR_MAP_DETAIL_LABELS.len() as u16 + 1
        } else {
            1
        };
        let menu_width = (inner_width + 2).min(area.width);
        let menu_height = (inner_height + 2).min(area.height);
        let menu_area = Rect::new(
            area.x.saturating_sub(1),
            area.y.saturating_sub(1),
            menu_width,
            menu_height,
        );
        let menu_block = Block::bordered().style(Style::default().fg(Color::White));
        let inner_area = menu_block.inner(menu_area);
        let toggle_area = Rect::new(inner_area.x, inner_area.y, inner_area.width, 1);

        Clear.render(menu_area, buf);
        menu_block.render(menu_area, buf);
        Paragraph::new(toggle_label)
            .style(Style::default().fg(Color::White))
            .render(toggle_area, buf);

        let (option_areas, filter_area) = if ui_state.star_map_details_menu_expanded {
            let option_areas: Vec<Rect> = STAR_MAP_DETAIL_LABELS
                .iter()
                .enumerate()
                .take(inner_area.height.saturating_sub(2) as usize)
                .map(|(index, label)| {
                    let checked = match index {
                        0 => ui_state.star_map_show_orbits,
                        1 => ui_state.star_map_show_names,
                        2 => ui_state.star_map_show_asteroids,
                        3 => ui_state.star_map_show_comets,
                        _ => false,
                    };
                    let row = Rect::new(
                        inner_area.x,
                        inner_area.y + 1 + index as u16,
                        inner_area.width,
                        1,
                    );
                    let checkbox = if checked { "[x]" } else { "[ ]" };
                    Paragraph::new(format!("{checkbox} {label}"))
                        .style(Style::default().fg(Color::White))
                        .render(row, buf);
                    row
                })
                .collect();

            let filter_row = Rect::new(
                inner_area.x,
                inner_area.y + 1 + STAR_MAP_DETAIL_LABELS.len() as u16,
                inner_area.width,
                1,
            );
            Paragraph::new(filter_value.as_str())
                .style(Style::default().fg(Color::White))
                .render(filter_row, buf);

            (option_areas, filter_row)
        } else {
            (Vec::new(), Rect::default())
        };

        with_ui_info_mut(|ui_info| {
            ui_info.star_map_details_toggle_area = toggle_area;
            ui_info.star_map_details_option_areas = option_areas;
            ui_info.star_map_details_filter_area = filter_area;
        });

        area
    }
}

fn fuzzy_name_match(filter_text: &str, candidate: &str) -> bool {
    if filter_text.is_empty() {
        return true;
    }

    let needle: Vec<char> = filter_text.chars().filter(|c| !c.is_whitespace()).collect();
    if needle.is_empty() {
        return true;
    }

    let mut needle_index = 0usize;
    for candidate_char in candidate.to_lowercase().chars() {
        if candidate_char == needle[needle_index] {
            needle_index += 1;
            if needle_index == needle.len() {
                return true;
            }
        }
    }

    false
}
