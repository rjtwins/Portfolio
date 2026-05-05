use std::sync::Arc;

use ratatui::layout::Position;
use ratatui::style::{Style, Stylize};
use ratatui::text::Line;
use ratatui::widgets::canvas::{Circle, Context, Line as CanvasLine, Points, Shape};
use ratatui::{layout::Rect, style::Color, text::Span, widgets::Widget};
use tui_widgets::popup::Popup;
use crate::channels::channels::{get_camera_state, get_selected_body_id, get_selected_fleet_id};
use crate::entities::GameEntity;
use crate::entities::camera::Camera;
use crate::entities::fleet::Fleet;
use crate::entities::orbit::Orbit;
use crate::entities::planet::Body;
use crate::{ACTIVE_COLOR, INACTIVE_COLOR, get_mouse_position, get_pixel_size_km, is_mouse_over_position_in_area_coordinates, line_points, with_ui_info, with_ui_info_mut, with_ui_state};

pub struct BodyRenderer<T> 
where 
    T: GameEntity<T> + Clone
{
    pub data: T,
    pub name: String,
    pub id: String,
    pub world_pos: (f64, f64),
    pub parent_pos: (f64, f64),
    pub orbit: Option<Orbit>,
    pub is_selected: bool,
    pub is_mouse_over: bool,
    pub should_be_rendered: bool,
    pub in_area_pos: (u16, u16),
    pub buf_pos: (u16, u16),
    color: Color,
    symbol: String,
    label_symbol: String,
}

// impl<T> Widget for &BodyRenderer<T>
// where 
//     T: GameEntity<T> + Clone
// {
//     // fn render(self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer)
//     // where
//     //     Self: Sized,
//     // {
//     //     if self.should_be_rendered == false {
//     //         return;
//     //     }

//     //     let show = with_ui_state(|ui_state| ui_state.star_map_show_names);

//     //     if self.is_selected || self.is_mouse_over || show {
//     //         self.render_label(self.screen_position, area, buf);
//     //     }

//     //     let color: Color = if self.is_selected {
//     //         ACTIVE_COLOR
//     //     } else if self.is_mouse_over {
//     //         Color::Green
//     //     } else {
//     //         self.color
//     //     };

//     //     let mut planet_widget = Span::raw(self.symbol.as_str()).fg(color).bg(Color::Reset);

//     //     if self.is_selected {
//     //         planet_widget = planet_widget.rapid_blink();
//     //     }

//     //     let area = Rect::new(self.buf_pos.0, self.buf_pos.1, 1, 1);
//     //     planet_widget.render(area.clone(), buf);

//     //     with_ui_info_mut(|ui_info| {
//     //         ui_info.star_map_info.insert(self.uuid, area);
//     //     });
//     // }
// }

impl<T> BodyRenderer<T> 
where 
    T: GameEntity<T> + Clone
{
    pub fn new_from_body(body: &T, color: Color, symbol: String, label_symbol: String) -> Self {
        Self {
            data: body.clone(),
            name: body.get_name(),
            id: body.get_id(),
            world_pos: body.get_global_position(),
            orbit: body.get_orbit(),
            parent_pos: body.get_parent_position(),
            is_selected: false,
            is_mouse_over: false,
            should_be_rendered: false,
            in_area_pos: (0, 0),
            buf_pos: (0, 0),
            color,
            symbol,
            label_symbol,
        }
    }

    pub fn new_from_fleet(data: &T, color: Color, symbol: String, label_symbol: String) -> Self {
        Self {
            data: data.clone(),
            name: data.get_name(),
            id: data.get_id(),
            orbit: data.get_orbit(),
            parent_pos: data.get_parent_position(),
            world_pos: data.get_global_position(),
            is_selected: false,
            is_mouse_over: false,
            should_be_rendered: false,
            in_area_pos: (0, 0),
            buf_pos: (0, 0),
            color,
            symbol,
            label_symbol,
        }
    }

    fn update_hover_and_selected(&mut self, area: Rect) {
        let camera = get_camera_state();
        let (x, y) = camera.world_to_screen_coordinates(self.world_pos);
        let pos = (x, y);

        let mouse_pos = get_mouse_position();
        self.is_mouse_over = is_mouse_over_position_in_area_coordinates(pos, mouse_pos, area);

        let selected_as_body = match get_selected_body_id() {
            Some(id) if id == self.id => true,
            _ => false,
        };

        let selected_as_fleet = match get_selected_fleet_id() {
            Some(id) if id == self.id => true,
            _ => false,
        };

        self.is_selected = selected_as_body || selected_as_fleet;
    }

    pub fn render_label_pub(
        &self,
        pos: (u16, u16),
        area: ratatui::prelude::Rect,
        buf: &mut ratatui::prelude::Buffer,
    ) {
        self.render_label(pos, area, buf);
    }

    fn render_label(
        &self,
        pos: (u16, u16),
        area: ratatui::prelude::Rect,
        buf: &mut ratatui::prelude::Buffer,
    ) {
        let name = &self.name;

        let popup = Popup::new(name.as_str())
            .title(self.label_symbol.as_str())
            .style(Style::new().white().on_black().not_dim());

        let width = name.len() as u16 + 2;
        let height = 3;
        let popup_area = Rect::new(
            area.left() + pos.0.saturating_sub(width / 2),
            area.top() + pos.1.saturating_sub(height),
            width,
            height,
        );

        popup.render(popup_area, buf);
    }

    pub fn update_render_state(&mut self, area: Rect) {
        self.should_be_rendered = false;

        let camera = get_camera_state();
        let (mut x, y) = camera.world_to_screen_coordinates(self.world_pos);

        //If we are not really visible, we don't need to calculate the rest of the render state.
        match self.orbit {
            Some(ref o) => {                
                if get_pixel_size_km() > o.semi_major_axis * 10.0 {
                    self.should_be_rendered = false;
                    return;
                }
            },
            None => {}
        };

        if x <= 0 || y <= 0 {
            self.should_be_rendered = false;
            return;
        }

        if x >= area.width || y >= area.height {
            self.should_be_rendered = false;
            return;
        }

        self.in_area_pos = (x, y);
        self.buf_pos = (area.left() + x, area.top() + y);

        self.update_hover_and_selected(area);
        self.should_be_rendered = true;
    }
}

impl BodyRenderer<Fleet>{
    /// Fleet-specific render state update: always marks fleets as renderable (canvas clips
    /// anything outside the viewport), and skips hover/selection for slipway fleets.
    pub fn update_render_state_fleet(&mut self, area: Rect) {
        let camera = get_camera_state();
        let (x, y) = camera.world_to_screen_coordinates(self.world_pos);
        self.in_area_pos = (x, y);
        self.buf_pos = (area.left() + x, area.top() + y);

        if !self.data.slipway_fleet {
            self.update_hover_and_selected(area);
        }

        self.should_be_rendered = true;
    }

    // pub fn render_fleet(&self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer){
    //     self.render_target_line(area, buf);
    //     self.render(area, buf);
    // }

    pub fn draw_on_canvas(&self, ctx: &mut Context<'_>) {
        if !self.should_be_rendered {
            return;
        }

        self.draw_vector_on_canvas(ctx);

        let color = if self.is_selected {
            ACTIVE_COLOR
        } else if self.is_mouse_over {
            Color::Green
        } else {
            self.color
        };

        let (x, y) = self.world_pos;
        ctx.print(x, -y, Line::styled(self.symbol.clone(), Style::default().fg(color)));
    }

    pub fn draw_vector_on_canvas(&self, ctx: &mut Context<'_>) {
        let target = match self.data.target_position {
            Some(t) => t,
            None => return,
        };

        let (x1, y1) = self.world_pos;
        let (x2, y2) = target;

        ctx.draw(&CanvasLine { x1, y1: -y1, x2, y2: -y2, color: Color::Red });
    }

    // pub fn render_target_line(&self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer){
    //     let pos = self.world_pos;
    //     let target = match self.data.target_position{
    //         Some(target) => target,
    //         None => return,
    //     };

    //     let camera = get_camera_state();
    //     let star_map_area = with_ui_info(|ui_info| ui_info.star_map_area);

    //     let (mut x1, mut y1) = camera.world_to_screen_coordinates(pos);
    //     x1 = x1.saturating_add(star_map_area.left());
    //     y1 = y1.saturating_add(star_map_area.top());

    //     let (mut x2, mut y2) = camera.world_to_screen_coordinates(target);
    //     x2 = x2.saturating_add(star_map_area.left());
    //     y2 = y2.saturating_add(star_map_area.top());

    //     let line_points = line_points(x1 as i32, y1 as i32, x2 as i32, y2 as i32);
        
    //     for point in line_points{
    //         let point = (point.0 as u16, point.1 as u16);
    //         let pos = Position::new(point.0, point.1);
    //         if !area.contains(pos){
    //             continue;
    //         }

    //         if !buf.area.contains(pos){
    //             continue;
    //         }

    //         buf[(pos.x, pos.y)]
    //             .set_symbol("·")
    //             .set_style(Style::default().fg(Color::Red));
    //     }
    // }
}

impl BodyRenderer<Body> {
    pub fn draw_on_canvas(&self, ctx: &mut Context<'_>) {
        // Draw orbit regardless of whether the planet body is on-screen.
        // Canvas clips segments outside the viewport automatically.
        if let Some(orbit) = &self.orbit {
            if get_pixel_size_km() <= orbit.semi_major_axis * 10.0 {
                self.draw_orbit_on_canvas(ctx, orbit);
            }
        }

        if !self.should_be_rendered {
            return;
        }

        let color = if self.is_selected {
            ACTIVE_COLOR
        } else if self.is_mouse_over {
            Color::Green
        } else {
            self.color
        };

        let (x, y) = self.world_pos;

        let camera = get_camera_state();
        if camera.zoom * 6000.0 > 2.0 {
            let circle = Circle { 
                x: x,
                y: -y,
                radius: 6000.0,
                color,
            };
    
            ctx.draw(&circle);
        }

        ctx.print(x, -y, Line::styled(self.symbol.clone(), Style::default().fg(color)));
    }

    fn draw_orbit_on_canvas(&self, ctx: &mut Context<'_>, orbit: &Orbit) {
        let show = with_ui_state(|ui_state| ui_state.star_map_show_orbits);
        if !show {
            return;
        }

        let color = if self.is_mouse_over || self.is_selected {
            ACTIVE_COLOR
        } else {
            INACTIVE_COLOR
        };

        let (px, py) = self.parent_pos;
        let coords: Vec<(f64, f64)> = orbit
            .orbit_segments
            .iter()
            .map(|(ox, oy)| (ox + px, -(oy + py)))
            .collect();

        ctx.draw(&Points { coords: &coords, color });
    }

    /*
    
    // fn render_orbit(
    //     &self,
    //     orbit: &Orbit,
    //     highlight: bool,
    //     area: ratatui::prelude::Rect,
    //     buf: &mut ratatui::prelude::Buffer,
    // ) {
    //     let show = with_ui_state(|ui_state| ui_state.star_map_show_orbits);
    //     if !show {
    //         return;
    //     }

    //     let camera: Arc<Camera> = get_camera_state();

    //     for (x, y) in orbit.orbit_segments.iter() {
    //         let world_pos = self.parent_pos;
    //         let (x, y) = camera.world_to_screen_coordinates((x + world_pos.0, y + world_pos.1));

    //         if x <= 0 || y <= 0 {
    //             continue;
    //         }

    //         if x >= area.width || y >= area.height {
    //             continue;
    //         }

    //         let buff_x = area.left() + x;
    //         let buff_y = area.top() + y;

    //         let color = if highlight {
    //             ACTIVE_COLOR
    //         } else {
    //             INACTIVE_COLOR
    //         };

    //         buf[(buff_x, buff_y)]
    //             .set_symbol("·")
    //             .set_style(Style::default().fg(color));
    //     }
    // }
    */
}
