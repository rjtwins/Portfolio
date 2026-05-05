use ratatui::widgets::ListState;
use crate::extentions::clickable_table::ClickableTableState;
use tui_tree_widget::TreeState;
use tui_widgets::scrollview::ScrollViewState;

use crate::entities::fleet::FleetOrderType;
use crate::entities::ship::{ShipDesign, SubsystemType};
use crate::renderers::colony_manager_renderer;
use crate::{GameScreenTab, UIScreen};

#[derive(Clone)]
pub enum ContextMenuAction {
    MoveToPosition(f64, f64),
    MoveToObject(String),
    KeepDistanceToObject(String, f64),
}

#[derive(Clone)]
pub struct ContextMenuEntry {
    pub label: String,
    pub action: ContextMenuAction,
}

#[derive(Clone)]
pub struct ContextMenuPendingInput {
    pub prompt: String,
    pub value: String,
    pub target_id: String,
}

#[derive(Clone, Default)]
pub struct ContextMenu {
    pub visible: bool,
    pub screen_pos: (u16, u16),
    pub entries: Vec<ContextMenuEntry>,
    pub pending_input: Option<ContextMenuPendingInput>,
}

#[derive(Clone, PartialEq, Eq)]
pub enum SubSystemDesignerPanel{
    SubsystemLibrary,
    EngineTechs,
    EngineSizes,
}

#[derive(Clone)]
pub struct SubSystemDesignerPanelState {
    current_subsystem_type: Option<SubsystemType>,
    panels: Vec<SubSystemDesignerPanel>,
    active_panel_index: usize,
}

impl Default for SubSystemDesignerPanelState {
    fn default() -> Self {
        Self::new()
    }
}

impl SubSystemDesignerPanelState {
    pub fn new() -> Self {
        Self {
            current_subsystem_type: None,
            panels: vec![
                SubSystemDesignerPanel::SubsystemLibrary,
            ],
            active_panel_index: 0,
        }
    }

    pub fn next_panel(&mut self) {
        let current_index = self.active_panel_index;
        let next_index = (current_index + 1) % self.panels.len();
        self.active_panel_index = next_index;

        //let mut active_panel = &self.panels[self.active_panel_index];
    }

    pub fn previous_panel(&mut self) {
        let current_index = self.active_panel_index;
        let next_index = if current_index == 0 {
            self.panels.len() - 1
        } else {
            current_index - 1
        };
        self.active_panel_index = next_index;

        //let mut active_panel = &self.panels[self.active_panel_index];
    }

    /// Returns a reference to the currently active panel.
    /// Assumes that this panel was selected using the next_panel or previous_panel methods, which ensure that the active panel is always unlocked.
    pub fn active_panel(&self) -> SubSystemDesignerPanel {
        self.panels[self.active_panel_index].clone()
    }

    pub fn set_active(&mut self, panel: SubSystemDesignerPanel) {
        if let Some(index) = self.panels.iter().position(|p| std::mem::discriminant(p) == std::mem::discriminant(&panel)) {
            self.panels[index] = panel;
        }
    }

    pub fn set_inactive (&mut self, panel: SubSystemDesignerPanel) {
        if let Some(index) = self.panels.iter().position(|p| std::mem::discriminant(p) == std::mem::discriminant(&panel)) {
            self.panels[index] = panel;
            if index == self.active_panel_index {
                self.next_panel();
            }
        }
    }
    
    pub fn update_available_panels_for_subsystem_type(&mut self, subsystem_type: &SubsystemType) {
        if let Some(current) = &self.current_subsystem_type {
            if std::mem::discriminant(current) == std::mem::discriminant(subsystem_type) {
                return;
            }
        }

        self.current_subsystem_type = Some(subsystem_type.clone());
        match subsystem_type {
            SubsystemType::Engines(_) => {
                self.panels.clear();
                self.panels.push(SubSystemDesignerPanel::SubsystemLibrary);
                self.panels.push(SubSystemDesignerPanel::EngineTechs);
                self.panels.push(SubSystemDesignerPanel::EngineSizes);
                self.active_panel_index = 0;
            },
            SubsystemType::Reactor => {
                self.panels.clear();
                self.panels.push(SubSystemDesignerPanel::SubsystemLibrary);
                self.active_panel_index = 0;
            },
            SubsystemType::Sensors(_) => {
                self.panels.clear();
                self.panels.push(SubSystemDesignerPanel::SubsystemLibrary);
                self.active_panel_index = 0;
            },
            SubsystemType::Weapons(_) => {
                self.panels.clear();
                self.panels.push(SubSystemDesignerPanel::SubsystemLibrary);
                self.active_panel_index = 0;
            },
            _ => {},
        }
    }
}

#[derive(Clone, Default)]
pub struct SubsystemRendererState {

    pub subsystem_tree_state: TreeState<String>,
    pub engine_tech_tree_state: TreeState<String>,
    pub panel_state: SubSystemDesignerPanelState,
}


#[derive(Clone, Default, PartialEq, Eq)]
pub enum ScienceRendererPanel{
    #[default]
    ResearchList,
    ResearchQueue,
}

impl ScienceRendererPanel {
    pub fn next(self) -> Self {
        match self {
            ScienceRendererPanel::ResearchList => ScienceRendererPanel::ResearchQueue,
            ScienceRendererPanel::ResearchQueue => ScienceRendererPanel::ResearchList,
        }
    }

    pub fn previous(self) -> Self {
        match self {
            ScienceRendererPanel::ResearchList => ScienceRendererPanel::ResearchQueue,
            ScienceRendererPanel::ResearchQueue => ScienceRendererPanel::ResearchList,
        }
    }
}

#[derive(Clone, Default)]
pub struct ScienceRendererState{
    pub active_panel: ScienceRendererPanel,
    pub research_queue: TreeState<String>,
    pub research_list: TreeState<String>,
}

#[derive(Clone)]
pub enum ShipDesignerMenuItem {
    NewDesign,
    SaveDesign,
    RenameDesign,
    DeleteDesign,
    LockDesign,
}

#[derive(Clone, PartialEq, Eq)]
pub enum ShipDesignerPanel {
    ShipDesigns,
    SubsystemLibrary,
    SubsystemList,
}

impl Default for ShipDesignerPanel {
    fn default() -> Self {
        ShipDesignerPanel::SubsystemLibrary
    }
}

impl ShipDesignerPanel{
    pub fn next(self) -> Self {
        match self {
            ShipDesignerPanel::ShipDesigns => ShipDesignerPanel::SubsystemLibrary,
            ShipDesignerPanel::SubsystemLibrary => ShipDesignerPanel::SubsystemList,
            ShipDesignerPanel::SubsystemList => ShipDesignerPanel::ShipDesigns,
        }
    }

    pub fn previous(self) -> Self {
        match self {
            ShipDesignerPanel::ShipDesigns => ShipDesignerPanel::SubsystemList,
            ShipDesignerPanel::SubsystemLibrary => ShipDesignerPanel::ShipDesigns,
            ShipDesignerPanel::SubsystemList => ShipDesignerPanel::SubsystemLibrary,
        }
    }
}

#[derive(Clone)]
pub struct ShipDesignerState {
    pub active_panel: ShipDesignerPanel,
    pub menu_popup_open: bool,
    pub menu_popup_state: ListState,
    pub design_tree_state: TreeState<String>,
    pub subsystem_tree_state: TreeState<String>,
    pub installed_subsystems_state: ListState,
    /// When `Some`, the user is actively typing a new name for the current design.
    pub rename_buffer: Option<String>,
}

impl Default for ShipDesignerState {
    fn default() -> Self {
        Self {
            active_panel: ShipDesignerPanel::default(),
            menu_popup_open: false,
            menu_popup_state: ListState::default(),
            design_tree_state: TreeState::default(),
            subsystem_tree_state: TreeState::default(),
            installed_subsystems_state: ListState::default(),
            rename_buffer: None,
        }
    }
}

#[derive(Clone)]
pub struct ShipDesignerMenuEntry {
    pub label: String,
    pub item: ShipDesignerMenuItem,
}

/// Rebuilds the ship designer popup entries for design actions.
pub fn build_ship_designer_menu_entries(_ship_designs: &[ShipDesign]) -> Vec<ShipDesignerMenuEntry> {
    vec![
        ShipDesignerMenuEntry { label: "New".to_string(), item: ShipDesignerMenuItem::NewDesign },
        ShipDesignerMenuEntry { label: "Save".to_string(), item: ShipDesignerMenuItem::SaveDesign },
        ShipDesignerMenuEntry { label: "Rename".to_string(), item: ShipDesignerMenuItem::RenameDesign },
        ShipDesignerMenuEntry { label: "Delete".to_string(), item: ShipDesignerMenuItem::DeleteDesign },
        ShipDesignerMenuEntry { label: "Lock/Unlock".to_string(), item: ShipDesignerMenuItem::LockDesign },
    ]
}


#[derive(Clone, Copy, PartialEq, Eq)]
pub enum SelectedDetailTab {
    Overview = 0,
    TreeView = 1,
    Fleets = 2,
}

impl SelectedDetailTab {
    pub fn next(self) -> Self {
        match self {
            SelectedDetailTab::Overview => SelectedDetailTab::TreeView,
            SelectedDetailTab::TreeView => SelectedDetailTab::Fleets,
            SelectedDetailTab::Fleets => SelectedDetailTab::Overview,
        }
    }

    pub fn previous(self) -> Self {
        match self {
            SelectedDetailTab::Overview => SelectedDetailTab::Fleets,
            SelectedDetailTab::TreeView => SelectedDetailTab::Overview,
            SelectedDetailTab::Fleets => SelectedDetailTab::TreeView,
        }
    }
}

/// Which side of the screen the sidebar lives on.
#[derive(Clone, Copy, PartialEq, Eq, Default)]
pub enum SidebarSide {
    #[default]
    Left,
    Right,
}

/// Which section of the sidebar has keyboard focus.
#[derive(Clone, Copy, PartialEq, Eq, Default)]
pub enum SidebarFocus {
    #[default]
    Colonies,
    Planets,
    Fleets,
}

/// Steps in the "add order" wizard for the fleet manager.
#[derive(Clone, PartialEq, Eq, Default)]
pub enum FleetAddStep {
    #[default]
    Idle,
    SelectType,
    SelectObject,
    /// Like SelectObject but only shows bodies (no fleets). Used for Colonize.
    SelectBody,
    SelectFleet,
    SelectShipsToSplit,
    SelectAddType,
    EnterN,
    EnterDistance,
}

#[derive(Clone, PartialEq, Eq, Default)]
pub enum FleetManagerPanel {
    #[default]
    OrderQueue,
    Ships,
}

#[derive(Clone, Default)]
pub struct FleetManagerState {
    pub active_panel: FleetManagerPanel,
    pub order_queue_state: ClickableTableState<String>,
    pub ships_state: ClickableTableState<String>,
    pub add_step: FleetAddStep,
    /// Populated from `fleet.available_orders()` when the add wizard starts.
    pub available_order_types: Vec<FleetOrderType>,
    pub add_type_index: usize,
    pub add_object_state: ClickableTableState<String>,
    pub add_add_type_index: usize,
    pub add_insert_n_input: String,
    pub add_distance_input: String,
    pub add_selected_object_id: Option<String>,
    /// Ships currently checked in the Split ship-picker.
    pub split_selected_ship_ids: std::collections::HashSet<String>,
    /// Navigation state for the Split ship-picker list.
    pub split_ships_state: ClickableTableState<String>,
}

/// State for the save/load game overlay popup.
#[derive(Clone, Default, PartialEq)]
pub enum SaveLoadPopup {
    #[default]
    Hidden,
    /// Ctrl+S: user is typing a save-file name.
    Save { name: String },
    /// Ctrl+L: user is browsing existing saves to load.
    Load { saves: Vec<String>, selected: usize },
}

#[derive(Clone, Copy)]
pub enum StarMapDetailOption {
    Orbits,
    Names,
    Asteroids,
    Comets,
}

#[derive(Clone)]
pub struct UIState {
    pub selected_tab: GameScreenTab,
    pub scroll_view_state: ScrollViewState,
    pub selected_detail_tab: SelectedDetailTab,
    pub selected_screen: UIScreen,
    pub main_menu_state: ListState,
    pub colony_manager_state: colony_manager_renderer::ColonyMangerState,
    pub fleet_manager_state: FleetManagerState,
    pub star_map_details_menu_expanded: bool,
    pub star_map_filter_text: String,
    pub star_map_filter_editing: bool,
    pub star_map_show_orbits: bool,
    pub star_map_show_names: bool,
    pub star_map_show_asteroids: bool,
    pub star_map_show_comets: bool,
    pub system_tree_state: TreeState<String>,
    pub fleets_tree_state: TreeState<String>,
    pub context_menu: ContextMenu,
    pub ship_designer_state: ShipDesignerState,
    pub science_renderer_state: ScienceRendererState,
    pub subsystem_renderer_state: SubsystemRendererState,
    pub save_load_popup: SaveLoadPopup,
    pub sidebar_side: SidebarSide,
    pub sidebar_collapsed: bool,
    pub sidebar_colonies_collapsed: bool,
    pub sidebar_planets_collapsed: bool,
    pub sidebar_fleets_collapsed: bool,
    pub sidebar_focus: SidebarFocus,
    pub colonies_list_state: TreeState<String>,
}

impl Default for UIState {
    fn default() -> Self {
        Self {
            selected_tab: GameScreenTab::SystemView,
            scroll_view_state: ScrollViewState::default(),
            selected_screen: UIScreen::Splash,
            main_menu_state: ListState::default(),
            colony_manager_state: colony_manager_renderer::ColonyMangerState {
                queue_state: ClickableTableState::new(vec![]),
                build_options_state: ClickableTableState::new(vec![]),
                finished_state: ClickableTableState::new(vec![]),
                selected_panel: colony_manager_renderer::ColonyMangerPanel::Queue,
                active_tab: colony_manager_renderer::ColonyManagerTab::Buildings,
                slipways_state: ClickableTableState::new(vec![]),
                retooling: false,
                retool_design_state: ClickableTableState::new(vec![]),
            },
            fleet_manager_state: FleetManagerState::default(),
            star_map_details_menu_expanded: false,
            star_map_filter_text: String::new(),
            star_map_filter_editing: false,
            star_map_show_orbits: true,
            star_map_show_names: true,
            star_map_show_asteroids: true,
            star_map_show_comets: true,
            system_tree_state: TreeState::default(),
            fleets_tree_state: TreeState::default(),
            context_menu: ContextMenu::default(),
            selected_detail_tab: SelectedDetailTab::Overview,
            ship_designer_state: ShipDesignerState::default(),
            science_renderer_state: ScienceRendererState::default(),
            subsystem_renderer_state: SubsystemRendererState {
                subsystem_tree_state: TreeState::default(),
                engine_tech_tree_state: TreeState::default(),
                panel_state: SubSystemDesignerPanelState::default(),
            },
            save_load_popup: SaveLoadPopup::Hidden,
            sidebar_side: SidebarSide::Left,
            sidebar_collapsed: false,
            sidebar_colonies_collapsed: false,
            sidebar_planets_collapsed: false,
            sidebar_fleets_collapsed: false,
            sidebar_focus: SidebarFocus::Colonies,
            colonies_list_state: TreeState::default(),
        }
    }
}

impl UIState {
    pub fn toggle_star_map_details_menu(&mut self) {
        self.star_map_details_menu_expanded = !self.star_map_details_menu_expanded;
        if !self.star_map_details_menu_expanded {
            self.star_map_filter_editing = false;
        }
    }

    pub fn toggle_star_map_detail(&mut self, option: StarMapDetailOption) {
        self.star_map_filter_editing = false;
        match option {
            StarMapDetailOption::Orbits => {
                self.star_map_show_orbits = !self.star_map_show_orbits;
            }
            StarMapDetailOption::Names => {
                self.star_map_show_names = !self.star_map_show_names;
            }
            StarMapDetailOption::Asteroids => {
                self.star_map_show_asteroids = !self.star_map_show_asteroids;
            }
            StarMapDetailOption::Comets => {
                self.star_map_show_comets = !self.star_map_show_comets;
            }
        }
    }

    pub fn toggle_star_map_detail_by_index(&mut self, index: usize) -> bool {
        let option = match index {
            0 => StarMapDetailOption::Orbits,
            1 => StarMapDetailOption::Names,
            2 => StarMapDetailOption::Asteroids,
            3 => StarMapDetailOption::Comets,
            _ => return false,
        };

        self.toggle_star_map_detail(option);
        true
    }

    pub fn activate_star_map_filter(&mut self) {
        self.star_map_details_menu_expanded = true;
        self.star_map_filter_editing = true;
    }
}
