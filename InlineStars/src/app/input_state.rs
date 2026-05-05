#[derive(Clone)]
pub struct InputState {
    pub mouse_position: (u16, u16),
    pub mouse_down: bool,
    pub dragging: bool,
    pub keys_down: Vec<char>,
    pub last_size: (u16, u16),
    pub terminate: bool,
}

impl Default for InputState {
    fn default() -> Self {
        Self {
            mouse_position: Default::default(),
            mouse_down: Default::default(),
            dragging: Default::default(),
            keys_down: Default::default(),
            last_size: Default::default(),
            terminate: Default::default(),
        }
    }
}
