use std::{cell::RefCell, sync::LazyLock};

use ratatui::{layout::{Constraint, Direction, Layout, Margin}, style::Color};
use tachyonfx::{CellFilter, Effect, Interpolation, fx, pattern::{RadialPattern, SpiralPattern, SweepPattern}};

thread_local! {
    pub static COALESCE: LazyLock<RefCell<Effect>> = LazyLock::new(|| { 
        //let fade = fx::fade_from(Color::Black, Color::White, EffectTimer::from_ms(500, Interpolation::QuadOut));
        let coalesce = fx::coalesce((100, Interpolation::QuadIn));
        RefCell::new(coalesce)
    });

    pub static BLINK_BORDER: LazyLock<RefCell<Effect>> = LazyLock::new(|| { 
        let from_timer = (250, Interpolation::Linear);
        let from = fx::paint_fg(Color::Red, from_timer)
            .with_filter(CellFilter::Outer(Margin { horizontal: 1, vertical: 1 }));
        let too = fx::paint_fg(Color::Yellow, from_timer)
            .with_filter(CellFilter::Outer(Margin { horizontal: 1, vertical: 1 }));
        //let from = fx::ping_pong(from);
        let sequence = fx::sequence(&[from, too]);
        let repeating = fx::repeating(sequence);
        RefCell::new(repeating)
    });
}