use std::sync::atomic;

use ratatui::layout::Rect;
use crate::{MAP_SIZE, channels::channels::{get_selected_body_id, get_ui_info_from_channel, set_camera}, entities::GameEntity, get_body_by_id, get_map_pos, get_mouse_position, get_pixel_size_km, mouse_pos_in_rect};

#[derive(Clone)]
pub struct Camera {
    pub x: f64,
    pub y: f64,
    pub zoom: f64,
}

impl Camera {
    pub fn pan(&mut self, delta_x: i16, delta_y: i16) {
        self.x += (delta_x as f64 / self.zoom) / 2.0;
        self.y += delta_y as f64 / self.zoom;
        
        set_camera(self.clone());
    }

    pub fn zoom(&mut self, factor: f64) {
        let ui_info = get_ui_info_from_channel();

        let center = mouse_pos_in_rect(get_mouse_position(), ui_info.star_map_area);

        // For now, using a fixed screen size or you can pass it as parameter
        let screen_center_x = center.0.saturating_sub(ui_info.star_map_area.left() / 2); // Adjust for the 2:1 pixel ratio
        let screen_center_y = center.1;
        
        // Convert screen center to world coordinates before zoom
        let world_x = (screen_center_x as f64 / self.zoom) + self.x;
        let world_y = (screen_center_y as f64 / self.zoom) + self.y;
        
        // Apply zoom
        self.zoom *= factor;
        
        // Adjust camera position to keep center point fixed
        self.x = world_x - (screen_center_x as f64 / self.zoom);
        self.y = world_y - (screen_center_y as f64 / self.zoom);

        set_camera(self.clone());
    }
}

impl Default for Camera{
    fn default() -> Self {
        Self { x: Default::default(), y: Default::default(), zoom: 0.0000001 }
    }
}

impl GameEntity<Camera> for Camera {
    fn update(&mut self, _: f64) {
        if let Some(uuid) = get_selected_body_id() {
            if let Some(body) = get_body_by_id(uuid) {
                let global_pos = body.get_global_position();
                let map_size = (MAP_SIZE.0.load(atomic::Ordering::Relaxed) as f64, MAP_SIZE.1.load(atomic::Ordering::Relaxed) as f64);
                let pixel_size = get_pixel_size_km();

                //x by 4 because 2 hor pixels are 1 "real pixel"
                self.x = global_pos.0 - (map_size.0 / 4.0) * pixel_size;
                self.y = global_pos.1 - (map_size.1 / 2.0) * pixel_size;
            }
        }

        crate::channels::channels::set_camera(self.clone());
    }

    fn get_global_position(&self) -> (f64, f64) {
        (self.x, self.y)
    }
    
    fn get_name(&self) -> String {
        "Camera".to_string()
    }
    
    fn get_id(&self) -> String {
        String::new()
    }
    
    fn get_orbit(&self) -> Option<super::orbit::Orbit> {
        None
    }
    
    fn get_parent_position(&self) -> (f64, f64) {
        (0.0,0.0)
    }
}

impl Camera {
    pub fn canvas_bounds(&self, area: Rect) -> ([f64; 2], [f64; 2]) {
        // x: divide width by 2 to account for the 2:1 terminal cell aspect ratio
        let world_width = area.width as f64 / 2.0 / self.zoom;
        let world_height = area.height as f64 / self.zoom;

        let x_bounds = [self.x, self.x + world_width];
        // Canvas y increases upward; our world y increases downward.
        // Negate all world-y values so cam.y (top of screen) maps to y_bounds[1] (canvas top).
        let y_bounds = [-(self.y + world_height), -self.y];

        (x_bounds, y_bounds)
    }

    pub fn world_to_screen_coordinates(&self, world_position: (f64, f64)) -> (u16, u16) {
        let mut x = ((world_position.0 - self.x) * self.zoom) as u16;
        let y = ((world_position.1 - self.y) * self.zoom) as u16;
        x = x.saturating_mul(2);
        (x, y)
    }

    pub fn screen_to_world_coordinates(&self, screen_position: (u16, u16)) -> (f64, f64) {
        let x = (screen_position.0 as f64 / 2.0 / self.zoom) + self.x;
        let y = (screen_position.1 as f64 / self.zoom) + self.y;
        (x, y)
    }
}
