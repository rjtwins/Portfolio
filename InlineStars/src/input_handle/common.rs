use crossterm::event::{KeyCode, KeyEvent, KeyModifiers};

thread_local! {
    pub(super) static INPUT_STATE: std::cell::RefCell<KeyEvent> = std::cell::RefCell::new(KeyEvent::new(KeyCode::Null, KeyModifiers::empty()));
}

#[derive(Clone, PartialEq, Eq)]
pub enum ColonyAction {
    QueueIncrease(String),
    QueueDecrease(String),
    QueueToggleInf(String),
    QueuePause(String),

    BuildAdd(String),
    BuildAddInf(String),

    FinishedDemolish(String),

    SlipwayBuild,
    SlipwayExtend(String),
    SlipwayRetool(String, String),    // (slipway_id, design_uuid/name)
    SlipwayQueueIncrease(String),     // slipway_id
    SlipwayQueueDecrease(String),     // slipway_id
}

#[derive(Clone)]
pub(super) struct MouseDragState {
    pub(super) dragging: bool,
    pub(super) start_position: (u16, u16),
    pub(super) last_position: (u16, u16),
}

impl Default for MouseDragState {
    fn default() -> Self {
        Self {
            dragging: false,
            start_position: (0, 0),
            last_position: (0, 0),
        }
    }
}
