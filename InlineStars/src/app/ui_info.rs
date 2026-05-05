use std::collections::HashMap;

use ratatui::layout::{Position, Rect};
use tui_tree_widget::TreeState;

use crate::app::{ColonyManagerUIInfo, FleetManagerUIInfo, ui_state::{ScienceRendererState, ShipDesignerState, SubsystemRendererState}};

#[derive(Clone, Default)]
pub struct UIInfo {
    pub tab_area: Rect,
    pub star_map_area: Rect,
    pub detail_area: Rect,
    pub tab_areas: Vec<(usize, Rect)>,
    pub detail_tab_areas: Vec<(usize, Rect)>,
    pub colony_manager_info: ColonyManagerUIInfo,
    pub fleet_manager_info: FleetManagerUIInfo,
    pub system_overview_info: Vec<(String, Rect)>,
    pub system_tree_state: TreeState<String>,
    pub fleets_tree_state: TreeState<String>,
    pub star_map_info: HashMap<String, Position>,
    pub context_menu_option_areas: Vec<Rect>,
    pub ship_designer_menu_button_area: Rect,
    pub ship_designer_menu_popup_area: Rect,
    pub ship_designer_menu_item_areas: Vec<Rect>,
    pub ship_designer_design_tree_area: Rect,
    pub ship_designer_subsystem_tree_area: Rect,
    pub ship_designer_state: ShipDesignerState,
    pub science_renderer_info: ScienceRendererState,
    pub subsystem_renderer_info: SubsystemRendererState,
    pub star_map_details_toggle_area: Rect,
    pub star_map_details_option_areas: Vec<Rect>,
    pub star_map_details_filter_area: Rect,
    /// Clickable area for collapsing/expanding the sidebar (the toggle button or collapsed strip).
    pub sidebar_toggle_area: Rect,
    /// Clickable area for the sidebar side-switch button.
    pub sidebar_side_button_area: Rect,
    /// Clickable area for the planets section header (click to toggle collapse).
    pub sidebar_planets_header_area: Rect,
    /// Clickable area for the fleets section header (click to toggle collapse).
    pub sidebar_fleets_header_area: Rect,
    /// Full area of the planets tree content (used for scroll hit-testing).
    pub sidebar_planets_content_area: Rect,
    /// Full area of the fleets tree content (used for scroll hit-testing).
    pub sidebar_fleets_content_area: Rect,
    /// Clickable area for the colonies section header (click to toggle collapse).
    pub sidebar_colonies_header_area: Rect,
    /// Full area of the colonies list content (used for scroll/click hit-testing).
    pub sidebar_colonies_content_area: Rect,
    /// Tree state for the colonies list (used for click_at).
    pub colonies_tree_state: TreeState<String>,
}
