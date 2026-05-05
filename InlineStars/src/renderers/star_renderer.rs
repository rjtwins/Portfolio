use ratatui::{
    style::{Color, Style},
    text::Line,
    widgets::{canvas::Context, Widget},
};

use crate::{
    channels::channels::get_camera_state,
    entities::{star::Star, GameEntity},
    set_map_pos, with_ui_info_mut, STAR,
};

#[derive(Clone)]
pub struct StarRenderer<'a> {
    pub star: &'a Star,
}

impl<'a> StarRenderer<'a> {
    pub fn draw_on_canvas(&self, ctx: &mut Context<'_>) {
        let (x, y) = self.star.get_global_position();
        ctx.print(
            x,
            -y,
            Line::styled(STAR.to_string(), Style::default().fg(Color::White)),
        );
    }
}

/*
impl<'a> Widget for &StarRenderer<'a> {
    fn render(self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer)
    where
        Self: Sized,
    {
        set_map_pos(area.x, area.y);

        let camera = get_camera_state();
        let (x, y) = camera.world_to_screen_coordinates(self.star.get_global_position());

        if x <= 0 || y <= 0 {
            return;
        }

        if x >= area.width || y >= area.height {
            return;
        }

        let buff_x = area.left() + x;
        let buff_y = area.top() + y;

        let brightness = self.star.brightness;
        let symbol = &*STAR.to_string();

        buf[(buff_x, buff_y)]
            .set_symbol(symbol)
            .set_style(Style::default().fg(Color::White));

        with_ui_info_mut(|ui_info| {
            ui_info.star_map_info.insert(self.star.get_uuid(), area);
        });
    }
}
*/
