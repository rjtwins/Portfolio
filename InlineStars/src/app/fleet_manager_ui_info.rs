use crate::extentions::clickable_table::{CellInfo, ClickableTableState};

#[derive(Clone, Default)]
pub struct FleetManagerUIInfo {
    pub order_cells: Vec<CellInfo>,
    pub order_state: ClickableTableState<String>,
    pub ships_cells: Vec<CellInfo>,
    pub ships_state: ClickableTableState<String>,
    pub add_cells: Vec<CellInfo>,
    pub add_state: ClickableTableState<String>,
    pub split_cells: Vec<CellInfo>,
    pub split_state: ClickableTableState<String>,
}
