use ratatui::layout::Rect;

use crate::extentions::clickable_table::{CellInfo, ClickableTableState};

#[derive(Clone, Default)]
pub struct ColonyManagerUIInfo {
    pub queue_cells: Vec<CellInfo>,
    pub build_options_cells: Vec<CellInfo>,
    pub finished_cells: Vec<CellInfo>,
    pub queue_state: ClickableTableState<String>,
    pub build_options_state: ClickableTableState<String>,
    pub finished_state: ClickableTableState<String>,
    pub slipways_cells: Vec<CellInfo>,
    pub slipways_state: ClickableTableState<String>,
    pub tab_areas: Vec<(usize, Rect)>,
    pub retool_design_cells: Vec<CellInfo>,
    pub retool_design_state: ClickableTableState<String>,
}
