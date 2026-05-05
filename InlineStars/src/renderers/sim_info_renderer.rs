use std::sync::atomic;

use ratatui::{
    style::{Style, Stylize},
    text::Line,
    widgets::{Block, Padding, Paragraph, Widget},
};

use crate::{app::TimeScale, ELAPSED_FULL_SIM, TIME_SCALE};

pub fn render(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
    let index = TIME_SCALE.load(atomic::Ordering::Relaxed) as usize;
    let sim_time = TimeScale::SCALE_ARRAY.get(index).cloned().unwrap_or(0.0);
    let days = (sim_time as u32) / 86400;
    let hours = ((sim_time as u32) % 86400) / 3600;
    let minutes = ((sim_time as u32) % 3600) / 60;
    let secs = (sim_time as u32) % 60;
    let formatted = format!("{days}d {hours}h {minutes}m {secs}s");

    let step_size =
        ELAPSED_FULL_SIM.load(atomic::Ordering::Relaxed) as f64 / 1_000_000.0 * sim_time;

    let paragraph = Paragraph::new(Line::from(format!(
        "SimTick: {:?}ms Step size: {:.2}s Game speed: {}/s",
        ELAPSED_FULL_SIM.load(atomic::Ordering::Relaxed) / 1000,
        step_size,
        formatted,
    )))
    .white()
    .block(
        Block::bordered()
            .style(Style::default().white())
            .padding(Padding::left(1)),
    );

    paragraph.render(area, buf);
}
