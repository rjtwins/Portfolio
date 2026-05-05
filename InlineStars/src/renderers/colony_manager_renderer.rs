use ratatui::{layout::{Constraint, Layout}, style::{Color, Style, Stylize}, text::{Line, Span, Text}, widgets::{Block, Cell, Clear, Padding, Paragraph, Row, Table, Widget}};

use crate::{ACTIVE_COLOR, FUEL, HEAVY_METALS, INACTIVE_COLOR, LIGHT_METALS, RARE_ELEMENTS, SUPER_ELEMENTS, app::{application::FRAME_TIME, effects::COALESCE, ship_desginer::{with_mut_ship_designer, with_ship_designer}}, channels::channels::HAS_JUST_TAB, entities::{planet::{Body, Colony, ColonyBuilding, colony_building::SlipWay}, ship}, extentions::clickable_table::{CellInfo, ClickableTable, ClickableTableState, KeyedRow}, get_body_by_id, renderers::system_overview_renderer::SystemOverviewRenderer, with_ui_info_mut, with_ui_state, with_ui_state_mut};

#[derive(Clone, Copy, PartialEq, Eq)]
pub enum ColonyMangerPanel{
    Queue = 0,
    BuildOptions = 1,
    Finished = 2,
}

impl ColonyMangerPanel{
    pub fn next(&self) -> Self {
        match self {
            ColonyMangerPanel::Queue => ColonyMangerPanel::BuildOptions,
            ColonyMangerPanel::BuildOptions => ColonyMangerPanel::Finished,
            ColonyMangerPanel::Finished => ColonyMangerPanel::Queue,
        }
    }

    pub fn previous(&self) -> Self {
        match self {
            ColonyMangerPanel::Queue => ColonyMangerPanel::Finished,
            ColonyMangerPanel::BuildOptions => ColonyMangerPanel::Queue,
            ColonyMangerPanel::Finished => ColonyMangerPanel::BuildOptions,
        }
    }
}

#[derive(Clone, Copy, PartialEq, Eq, Default)]
pub enum ColonyManagerTab {
    #[default]
    Buildings,
    Shipyards,
}

impl ColonyManagerTab {
    pub fn next(self) -> Self {
        match self {
            ColonyManagerTab::Buildings => ColonyManagerTab::Shipyards,
            ColonyManagerTab::Shipyards => ColonyManagerTab::Buildings,
        }
    }

    pub fn previous(self) -> Self {
        self.next() // only 2 tabs so same as next
    }
}

#[derive(Clone)]
pub struct ColonyMangerState {
    pub queue_state: ClickableTableState<String>,
    pub build_options_state: ClickableTableState<String>,
    pub finished_state: ClickableTableState<String>,
    pub selected_panel: ColonyMangerPanel,
    pub active_tab: ColonyManagerTab,
    pub slipways_state: ClickableTableState<String>,
    pub retooling: bool,
    pub retool_design_state: ClickableTableState<String>,
}

#[derive(Clone)]
pub struct ColonyManagerRenderer
{

}

impl ColonyManagerRenderer {
    pub fn new() -> Self {
        Self 
        {

        }
    }

    pub fn render(&self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer) {
        let selected_body_id = crate::channels::channels::get_selected_body_id();
        let selected_body = match selected_body_id {
            Some(id) => get_body_by_id(id),
            None => return,
        };
        let selected_body = match selected_body {
            Some(body) => body,
            None => return,
        };
        let [tab_bar_area, content_area] = Layout::vertical([
            Constraint::Length(3),
            Constraint::Min(0),
        ]).areas(area);

        let active_tab = with_ui_state(|s| s.colony_manager_state.active_tab);
        let tab_areas = Self::render_tab_bar(tab_bar_area, buf, active_tab);
        with_ui_info_mut(|ui_info| ui_info.colony_manager_info.tab_areas = tab_areas);

        match (active_tab, selected_body.colony.as_ref()) {
            (ColonyManagerTab::Buildings, Some(colony)) => {
                self.render_buildings_tab(content_area, buf, colony, &selected_body)
            }
            (ColonyManagerTab::Shipyards, Some(colony)) => {
                self.render_shipyards_tab(content_area, buf, colony)
            }
            _ => self.render_body_info_only(content_area, buf, &selected_body),
        }
    }

    fn render_buildings_tab(&self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer, colony: &Colony, selected_body: &Body) {
        let buildings = &colony.buildings;
        let in_queue = buildings.iter().filter(|b| b.is_building()).collect::<Vec<_>>();
        let finished = buildings.iter()
            .filter(|b| b.get_built_amount() > 0)
            .filter(|b| b.template.id != "extend_slipway" && b.template.id != "build_slipway") // Filter out extended slipways from finished list
            .collect::<Vec<_>>();

        let rects = Layout::vertical(vec![Constraint::Percentage(50), Constraint::Percentage(50)]).split(area);
        let in_queue_area = rects[0];
        let finished_area = rects[1];
        let [in_queue_area, building_list_area] = Layout::horizontal(vec![Constraint::Percentage(50), Constraint::Percentage(50)]).areas(in_queue_area);
        let [finished_area, detail_area] = Layout::horizontal(vec![Constraint::Percentage(50), Constraint::Percentage(50)]).areas(finished_area);

        let queue_keys: Vec<String> = in_queue.iter().map(|b| b.get_name()).collect();
        let finished_keys: Vec<String> = finished.iter().map(|b| b.get_name()).collect();

        let selected_panel = with_ui_state(|ui_state| ui_state.colony_manager_state.selected_panel);
        let queue_table = Self::get_in_queue_table(colony, in_queue, selected_panel == ColonyMangerPanel::Queue);
        let finished_table = Self::get_finished_table(finished, selected_panel == ColonyMangerPanel::Finished);
        let detail_table = Self::get_resource_table(selected_body);

        let options = ColonyBuilding::get_all_building_options();
        let build_keys: Vec<String> = options.iter().map(|b| b.get_name()).collect();
        let building_list_table = Self::get_build_table(options.iter().collect(), selected_panel == ColonyMangerPanel::BuildOptions);

        let (queue_table_cells, queue_state) =
            with_ui_state_mut(|ui_state| {
                ui_state.colony_manager_state.queue_state.update_keys(queue_keys.clone());
                let cells = queue_table.render_stateful_keyed_into_cells(
                    in_queue_area, buf, &mut ui_state.colony_manager_state.queue_state,
                );
                (cells, ui_state.colony_manager_state.queue_state.clone())
            });

        let (building_table_cells, build_options_state) =
            with_ui_state_mut(|ui_state| {
                ui_state.colony_manager_state.build_options_state.update_keys(build_keys.clone());
                let cells = building_list_table.render_stateful_keyed_into_cells(
                    building_list_area, buf, &mut ui_state.colony_manager_state.build_options_state,
                );
                (cells, ui_state.colony_manager_state.build_options_state.clone())
            });

        let (finished_table_cells, finished_state) =
            with_ui_state_mut(|ui_state| {
                ui_state.colony_manager_state.finished_state.update_keys(finished_keys.clone());
                let cells = finished_table.render_stateful_keyed_into_cells(
                    finished_area, buf, &mut ui_state.colony_manager_state.finished_state,
                );
                (cells, ui_state.colony_manager_state.finished_state.clone())
            });

        let body_info_height = SystemOverviewRenderer::selected_body_info_height(selected_body)
            .min(detail_area.height);
        let [resource_area, body_info_area] = Layout::vertical([
            Constraint::Min(detail_area.height.saturating_sub(body_info_height)),
            Constraint::Length(body_info_height),
        ]).areas(detail_area);

        detail_table.render(resource_area, buf);
        SystemOverviewRenderer::render_selected_body_info(buf, body_info_area, selected_body);

        with_ui_info_mut(|ui_info| {
            ui_info.colony_manager_info.queue_cells = queue_table_cells;
            ui_info.colony_manager_info.build_options_cells = building_table_cells;
            ui_info.colony_manager_info.finished_cells = finished_table_cells;
            ui_info.colony_manager_info.queue_state = queue_state;
            ui_info.colony_manager_info.build_options_state = build_options_state;
            ui_info.colony_manager_info.finished_state = finished_state;
        });
    }

    fn render_tab_bar(area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer, active_tab: ColonyManagerTab) -> Vec<(usize, ratatui::layout::Rect)> {
        let selected = match active_tab {
            ColonyManagerTab::Buildings => 0,
            ColonyManagerTab::Shipyards => 1,
        };

        crate::extentions::tabs_extentions::ClickableTabs::new(["Buildings", "Shipyards"])
            .select(selected)
            .block(Block::bordered())
            .style(Style::default().fg(INACTIVE_COLOR))
            .highlight_style(Style::default().fg(ACTIVE_COLOR).bold())
            .padding(" ", " ")
            .render_into_areas(area, buf)
    }

    fn render_body_info_only(
        &self,
        area: ratatui::prelude::Rect,
        buf: &mut ratatui::prelude::Buffer,
        selected_body: &Body,
    ) {
        let rects = Layout::vertical(vec![Constraint::Percentage(50), Constraint::Percentage(50)]).split(area);
        let [_, _building_list_area] = Layout::horizontal(vec![Constraint::Percentage(50), Constraint::Percentage(50)]).areas(rects[0]);
        let [_finished_area, detail_area] = Layout::horizontal(vec![Constraint::Percentage(50), Constraint::Percentage(50)]).areas(rects[1]);

        SystemOverviewRenderer::render_selected_body_info(buf, detail_area, selected_body);

        with_ui_info_mut(|ui_info| {
            ui_info.colony_manager_info.queue_cells = Vec::new();
            ui_info.colony_manager_info.build_options_cells = Vec::new();
            ui_info.colony_manager_info.finished_cells = Vec::new();
        });
    }

    fn render_shipyards_tab(&self, area: ratatui::prelude::Rect, buf: &mut ratatui::prelude::Buffer, colony: &Colony) {
        let slipways = colony.get_slip_ways();

        let [list_area, right_area] = Layout::horizontal([
            Constraint::Percentage(40),
            Constraint::Percentage(60),
        ]).areas(area);

        let [ship_queue_area, detail_area, hint_area] = Layout::vertical([
            Constraint::Percentage(60),
            Constraint::Min(0),
            Constraint::Length(1),
        ]).areas(right_area);

        let slipway_keys: Vec<String> = slipways.iter().map(|s| s.id.clone()).collect();
        let border_color = ACTIVE_COLOR;

        let rows: Vec<KeyedRow<String>> = slipways.iter().map(|s| {
            let status = if let Some(sb) = &s.ship_building {
                let ship_design = with_ship_designer(|sd| sd.ship_designs.iter().find(|sd| sd.id == sb.ship_design.id).cloned()).unwrap();
                format!("Building {} {}", ship_design.name, sb.queue_amount)
            } else if s.size == 0 {
                "Under Construction".to_string()
            } else {
                "Available".to_string()
            };
            KeyedRow::new(s.id.clone(), Row::new([
                s.name.clone(),
                format!("{} kt", s.size),
                status.to_string(),
            ]))
        }).collect();

        let slipway_widths = [
            Constraint::Percentage(50),
            Constraint::Percentage(20),
            Constraint::Percentage(30),
        ];

        let slipways_table = ClickableTable::new_keyed(rows, slipway_widths.into())
            .header(Row::new(["Name", "Size", "Status"]).style(Style::new().bold()).bottom_margin(1))
            .column_spacing(1)
            .style(Color::White)
            .block(Block::bordered()
                .title("Slipways [n: New Slipway | e: Extend | r: Retool | +: Queue Ship | -: Dequeue Ship]")
                .style(Style::default().fg(border_color))
                .padding(Padding::left(1)))
            .row_highlight_style(Style::new().red().slow_blink());

        let (slipways_cells, slipways_state) = with_ui_state_mut(|ui_state| {
            ui_state.colony_manager_state.slipways_state.update_keys(slipway_keys.clone());
            let cells = slipways_table.render_stateful_keyed_into_cells(
                list_area, buf, &mut ui_state.colony_manager_state.slipways_state,
            );
            (cells, ui_state.colony_manager_state.slipways_state.clone())
        });

        let selected_slipway = slipways_state.selected()
            .and_then(|id| slipways.iter().find(|s| &s.id == id));

        // Always render normal ship build queue
        Paragraph::new("Ship building queue — coming soon")
            .block(Block::bordered().title("Ship Build Queue").padding(Padding::left(1)))
            .render(ship_queue_area, buf);

        // Always render slipway detail
        let detail_text = if let Some(s) = selected_slipway {
            match &s.ship_building {
                Some(sb) => vec![
                    Line::from(format!("Design:   {}", sb.ship_design.id)),
                    Line::from(format!("Queue:    {}", sb.queue_amount)),
                    Line::from(format!("Progress: {:.0}%", sb.progress * 100.0)),
                ],
                None => vec![Line::from("No ship assigned. R: retool")],
            }
        } else {
            vec![Line::from("No slipway selected")]
        };
        Paragraph::new(detail_text)
            .block(Block::bordered().title("Slipway Detail").padding(Padding::left(1)))
            .render(detail_area, buf);

        Paragraph::new("n: New | e: Extend | r: Retool | +/-: Queue")
            .style(Style::default().fg(INACTIVE_COLOR))
            .render(hint_area, buf);

        with_ui_info_mut(|ui_info| {
            ui_info.colony_manager_info.slipways_cells = slipways_cells;
            ui_info.colony_manager_info.slipways_state = slipways_state;
            ui_info.colony_manager_info.retool_design_cells = Vec::new();
        });

        // Retool popup overlay
        let retooling = with_ui_state(|s| s.colony_manager_state.retooling);
        if retooling {
            let popup_area = Self::centered_rect(60, 70, area);
            Clear.render(popup_area, buf);

            let popup_block = Block::bordered()
                .title("Retool: Select Design  [↑↓: navigate | Enter: confirm | ESC: cancel]")
                .style(Style::default().fg(ACTIVE_COLOR));
            let inner = popup_block.inner(popup_area);
            popup_block.render(popup_area, buf);

            let designs = with_ship_designer(|sd| sd.ship_designs.clone());
            let design_keys: Vec<String> = designs.iter().map(|d| d.id.to_string()).collect();
            let design_rows: Vec<KeyedRow<String>> = designs.iter().map(|d| {
                KeyedRow::new(d.id.to_string(), Row::new([d.name.clone()]))
            }).collect();

            let design_table = ClickableTable::new_keyed(design_rows, [Constraint::Percentage(100)].into())
                .header(Row::new(["Ship Design"]).style(Style::new().bold()).bottom_margin(1))
                .column_spacing(1)
                .style(Color::White)
                .row_highlight_style(Style::new().red().slow_blink());

            let (retool_design_cells, retool_design_state) = with_ui_state_mut(|ui_state| {
                ui_state.colony_manager_state.retool_design_state.update_keys(design_keys);
                let cells = design_table.render_stateful_keyed_into_cells(
                    inner, buf, &mut ui_state.colony_manager_state.retool_design_state,
                );
                (cells, ui_state.colony_manager_state.retool_design_state.clone())
            });

            with_ui_info_mut(|ui_info| {
                ui_info.colony_manager_info.retool_design_cells = retool_design_cells;
                ui_info.colony_manager_info.retool_design_state = retool_design_state;
            });
        }
    }

    fn centered_rect(percent_x: u16, percent_y: u16, area: ratatui::prelude::Rect) -> ratatui::prelude::Rect {
        let vertical = Layout::vertical([
            Constraint::Percentage((100 - percent_y) / 2),
            Constraint::Percentage(percent_y),
            Constraint::Percentage((100 - percent_y) / 2),
        ]).split(area);

        let horizontal = Layout::horizontal([
            Constraint::Percentage((100 - percent_x) / 2),
            Constraint::Percentage(percent_x),
            Constraint::Percentage((100 - percent_x) / 2),
        ]).split(vertical[1]);

        horizontal[1]
    }

    fn get_build_table(buildings: Vec<&ColonyBuilding>, selected: bool) -> ClickableTable<'static, String> {
        let header = Row::new(["Name", "Build Time"])
            .style(Style::new().bold())
            .bottom_margin(1);

        let rows = buildings.iter().map(|b| KeyedRow::new(
            b.get_name(),
            Row::new([b.get_name(), format!("{:.1} ic", b.get_ic_cost())]),
        )).collect::<Vec<_>>();

        let widths = [
            Constraint::Percentage(33),
            Constraint::Percentage(33),
            Constraint::Percentage(33),
        ];

        let border_color = if selected { ACTIVE_COLOR } else { INACTIVE_COLOR };

        ClickableTable::new_keyed(rows, widths.into())
            .header(header)
            .column_spacing(1)
            .style(Color::White)
            .block(Block::bordered()
                .title("Build Options [+: add | c: add ∞]")
                .style(Style::default().fg(border_color))
                .padding(Padding::left(1)))
            .row_highlight_style(Style::new().red().slow_blink())
    }

    fn get_finished_table(buildings: Vec<&ColonyBuilding>, selected: bool) -> ClickableTable<'static, String> {
        let header = Row::new(["Name"])
            .style(Style::new().bold())
            .bottom_margin(1);

        let rows = buildings.iter().map(|b| KeyedRow::new(
            b.get_name(),
            Row::new([format!("{} {}", b.get_name(), b.get_built_amount())]),
        )).collect::<Vec<_>>();

        let widths = [
            Constraint::Percentage(33),
            Constraint::Percentage(33),
            Constraint::Percentage(33),
        ];

        let border_color = if selected { ACTIVE_COLOR } else { INACTIVE_COLOR };

        ClickableTable::new_keyed(rows, widths.into())
            .header(header)
            .column_spacing(1)
            .style(Color::White)
            .block(Block::bordered()
                .title("Buildings [-: demolish]")
                .style(Style::default().fg(border_color))
                .padding(Padding::left(1)))
            .row_highlight_style(Style::new().red().slow_blink())
    }

    fn get_in_queue_table<'a>(colony: &'a Colony, in_queue: Vec<&'a ColonyBuilding>, selected: bool) -> ClickableTable<'a, String> {
        let header = Row::new(["Name", "Progress", "Time Remaining"])
            .style(Style::new().bold())
            .bottom_margin(1);

        let rows = in_queue.iter().map(|b| {
            let progress = if b.is_infinite() {
            "INF".to_string()
            } else {
                format!("{:.0}%", b.get_progress() * 100.0)
            };
                let time_until_next = format!("{:.1} d", b.time_until_next_completion(colony.get_ic_production()) / 86400.0);
                let total_time_remaining = if b.is_infinite() {
            "INF".to_string()
            } else {
                format!("{:.1} d", b.get_time_to_complete(colony.get_ic_production()) / 86400.0)
            };
            let time_text = format!("{} / {}", time_until_next, total_time_remaining);
            let name_text = format!("{} {}", b.get_name(), b.get_queue_amount());

            KeyedRow::new(b.get_name(), Row::new([name_text, progress, time_text]))
        }).collect::<Vec<_>>();

        let widths = [
            Constraint::Percentage(33),
            Constraint::Percentage(33),
            Constraint::Percentage(33),
        ];

        let border_color = if selected { ACTIVE_COLOR } else { INACTIVE_COLOR };

        ClickableTable::new_keyed(rows, widths.into())
            .header(header)
            .column_spacing(1)
            .style(Color::White)
            .block(Block::bordered()
                .title("In Queue [+: increase | -: decrease | c: ∞ | p: pause]")
                .style(Style::default().fg(border_color))
                .padding(Padding::left(1)))
            .row_highlight_style(Style::new().red().slow_blink())
    }
    
    fn get_resource_table(body: &Body) -> ratatui::widgets::Table {
        //SAFETY: We cannot get here if colony is None.
        let colony = body.colony.as_ref().unwrap();
        let last_production = colony.get_last_production();

        let surface_label = body.body_type.surface_layer_name();
        let mantle_label = body.body_type.mantle_layer_name().unwrap_or("-");
        let core_label = body.body_type.core_layer_name().unwrap_or("-");
        let header = Row::new(["", "Stockpile", "Production", surface_label, mantle_label, core_label]);
        let rows: Vec<Row<'_>> = vec![
            Row::new([
                Cell::from("Population"), 
                Cell::from(colony.resources.population.to_string()),
                Cell::from(""),
                Cell::from(""),
                Cell::from(""),
                Cell::from(""),
                ]),
            Row::new([
                Cell::from("IC"), 
                Cell::from(""),
                Cell::from(Self::format_resource_value(colony.get_ic_production())),
                Cell::from(""),
                Cell::from(""),
                Cell::from(""),
                ]),
            Row::new([
                Cell::from(FUEL.to_string()), 
                Cell::from(Self::format_resource_value(colony.resources.light_elements)),
                Cell::from(Self::format_resource_value(last_production.fuel_production + last_production.fuel_mining_production)),
                Cell::from(Self::format_resource_value(body.surface_resources.fuel.amount)),
                Cell::from(Self::format_resource_value(body.mantle_resources.fuel.amount)),
                Cell::from(Self::format_resource_value(body.core_resources.fuel.amount)),
                ]),
            Row::new([
                Cell::from(LIGHT_METALS.to_string()), 
                Cell::from(Self::format_resource_value(colony.resources.light_metals)),
                Cell::from(Self::format_resource_value(last_production.light_metals_production + last_production.light_metals_mining_production)),
                Cell::from(Self::format_resource_value(body.surface_resources.light_metals.amount)),
                Cell::from(Self::format_resource_value(body.mantle_resources.light_metals.amount)),
                Cell::from(Self::format_resource_value(body.core_resources.light_metals.amount)),
                ]),
            Row::new([
                Cell::from(HEAVY_METALS.to_string()), 
                Cell::from(Self::format_resource_value(colony.resources.heavy_metals)),
                Cell::from(Self::format_resource_value(last_production.heavy_metals_production + last_production.heavy_metals_mining_production)),
                Cell::from(Self::format_resource_value(body.surface_resources.heavy_metals.amount)),
                Cell::from(Self::format_resource_value(body.mantle_resources.heavy_metals.amount)),
                Cell::from(Self::format_resource_value(body.core_resources.heavy_metals.amount)),
                ]),
            Row::new([
                Cell::from(RARE_ELEMENTS.to_string()), 
                Cell::from(Self::format_resource_value(colony.resources.rare_elements)),
                Cell::from(Self::format_resource_value(last_production.rare_elements_production + last_production.rare_elements_mining_production)),
                Cell::from(Self::format_resource_value(body.surface_resources.rare_elements.amount)),
                Cell::from(Self::format_resource_value(body.mantle_resources.rare_elements.amount)),
                Cell::from(Self::format_resource_value(body.core_resources.rare_elements.amount)),
                ]),
            Row::new([
                Cell::from(SUPER_ELEMENTS.to_string()), 
                Cell::from(Self::format_resource_value(colony.resources.super_elements)),
                Cell::from(Self::format_resource_value(last_production.super_elements_production + last_production.super_elements_mining_production)),
                Cell::from(Self::format_resource_value(body.surface_resources.super_elements.amount)),
                Cell::from(Self::format_resource_value(body.mantle_resources.super_elements.amount)),
                Cell::from(Self::format_resource_value(body.core_resources.super_elements.amount)),
                ]),
        ];

        let widths = [
            Constraint::Percentage(16),
            Constraint::Percentage(18),
            Constraint::Percentage(18),
            Constraint::Percentage(16),
            Constraint::Percentage(16),
            Constraint::Percentage(16),
        ];
        
        let table = Table::new(rows, widths)
            .header(header)
            //.footer(footer.italic())
            .column_spacing(1)
            .style(Color::White)
            .block(Block::bordered().title("Resources").style(Style::default()).padding(Padding::left(1)))
            .row_highlight_style(Style::new().bold())
            .column_highlight_style(Color::Gray)
            .cell_highlight_style(Style::new().reversed().yellow());

        return table;
    }

    fn format_resource_value(value: f64) -> String {
        let abs = value.abs();
        if abs >= 1_000_000_000_000.0 {
            format!("{:.1}T", value / 1_000_000_000_000.0)
        } else if abs >= 1_000_000_000.0 {
            format!("{:.1}B", value / 1_000_000_000.0)
        } else if abs >= 1_000_000.0 {
            format!("{:.1}M", value / 1_000_000.0)
        } else if abs >= 1_000.0 {
            format!("{:.1}K", value / 1_000.0)
        } else {
            format!("{value:.2}")
        }
    }
}
