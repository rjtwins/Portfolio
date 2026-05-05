use std::cell::RefCell;
use std::io;
use std::sync::{atomic, LazyLock};

use ratatui::layout::{self, Constraint, Layout, Margin, Rect};
use ratatui::style::{Color, Style, Stylize};
use ratatui::symbols::{self, border};
use ratatui::text::{Line, Span};
use ratatui::widgets::{Block, List, Padding, Paragraph, Widget};
use ratatui::{DefaultTerminal, Frame};
use tachyonfx::{fx, CellFilter, Effect, EffectTimer, Interpolation};
use tokio::task::block_in_place;

use crate::app::effects::COALESCE;
use crate::app::science_manager::{with_mut_science_manager, with_science_manager};
use crate::app::TimeScale;
use crate::channels::channels::{
    get_input_state, get_player_states, get_selected_body_id, get_star_map_state,
    get_ui_info_from_channel, get_ui_state_from_channel, set_ui_info_to_channel, HAS_JUST_TAB,
};
use crate::entities::GameEntity;
use crate::extentions::tabs_extentions::ClickableTabs;
use crate::renderers::detail_renderer::DetailRenderer;
use crate::renderers::sidebar_renderer::SidebarRenderer;
use crate::renderers::star_map_renderer::StarMapRenderer;
use crate::renderers::{
    colony_manager_renderer, fleet_manager_renderer, science_renderer, ship_designer_renderer,
    sim_info_renderer, subsystem_designer_renderer,
};
use crate::{
    ELAPSED_FULL_SIM, ELAPSED_SIM, GameScreenTab, MAP_SIZE, JUST_LOADED_GAME, TIME_SCALE, UIInfo, UIScreen, UIState, clear_star_map_info, current_ui_info, get_pixel_size_km, get_player_colonies, input_handle, new_game, replace_ui_info, replace_ui_state, with_ui_info_mut, with_ui_state
};
use crate::app::ui_state::SaveLoadPopup;
use crate::{ACTIVE_COLOR, INACTIVE_COLOR};

pub const FRAME_TIME: std::time::Duration = std::time::Duration::from_millis(16);

pub struct App {
    pub tabs: ClickableTabs<'static>,
    pub star_map: StarMapRenderer,
}

impl App {
    pub fn new() -> Self {
        let tab_content = vec![
            "SYSTEM",
            "MANAGER",
            "SHIP DESIGNER",
            "SUBSYSTEM DESIGNER",
            "RESEARCH",
        ];
        let tabs = ClickableTabs::new(tab_content.clone())
            .style(Color::White)
            .block(Block::bordered().style(Style::default().fg(Color::White)))
            .highlight_style(Style::default().magenta().bold())
            .select(0)
            .divider(symbols::line::VERTICAL)
            .padding(" ", " ");

        let star_map_renderer = StarMapRenderer::new().block(
            Block::bordered()
                .border_set(border::PLAIN)
                .style(Style::default().fg(Color::White)),
        );

        App {
            tabs: tabs,
            star_map: star_map_renderer,
        }
    }

    pub async fn run(&mut self, terminal: &mut DefaultTerminal) -> io::Result<()> {
        //Setup state channels:
        crate::channels::channels::setup();

        input_handle::input_handle::run_input_worker();
        self.run_sim_worker();
        self.run_render_worker(terminal).await
    }

    fn draw(&mut self, frame: &mut Frame) {
        let selected_screen = with_ui_state(|ui_state| ui_state.selected_screen);
        match selected_screen {
            UIScreen::Splash => {
                self.draw_splash(frame);
            }
            UIScreen::MainMenu => {
                self.draw_main_menu(frame);
            }
            UIScreen::Game => {
                self.draw_game_screen(frame);
            }
            _ => {}
        }
    }

    fn draw_map_tab(&mut self, frame: &mut Frame, main_area: Rect) {
        self.star_map = self.star_map.clone().block(
            Block::bordered()
                .border_set(border::PLAIN)
                .style(Style::default().fg(Color::White)),
        );

        clear_star_map_info();
        let buf = frame.buffer_mut();

        // Render sidebar and get the remaining map area
        let map_area = SidebarRenderer::render_and_split(main_area, buf);

        let map_rendered_rect = self.star_map.render(map_area, buf);

        MAP_SIZE
            .0
            .store(map_rendered_rect.width as i32, atomic::Ordering::Relaxed);
        MAP_SIZE
            .1
            .store(map_rendered_rect.height as i32, atomic::Ordering::Relaxed);

        //Render scale:
        let scale_1_px = get_pixel_size_km();
        let area_width = map_rendered_rect.width;
        let scale_width = (area_width as f32 * 0.2) as usize;
        let scale_km = scale_1_px * scale_width as f64;

        let scale_text = if scale_km > 0.1 * 149_597_870.7 {
            format!("{:.1}au", scale_km / 149_597_870.7)
        } else {
            format!("{:.0}km", scale_km)
        };

        let scale_measure = format!("|{}| {}", "-".repeat(scale_width), scale_text);
        let scale = Span::from(scale_measure).fg(Color::Red);

        scale.render(
            Rect::new(
                map_rendered_rect.left() + 1,
                map_rendered_rect.bottom() - 2,
                area_width,
                1,
            ),
            buf,
        );

        let mouse_pos = get_input_state().mouse_position;

        //Render mouse position for debugging:
        let mouse_pos = (
            mouse_pos.0.clamp(0, buf.area.width - 1),
            mouse_pos.1.clamp(0, buf.area.height - 1),
        );
        buf[(mouse_pos.0, mouse_pos.1)]
            .set_symbol("#")
            .set_style(Style::default().fg(Color::Green));

        with_ui_info_mut(|ui_info| {
            ui_info.star_map_area = map_rendered_rect;
        });

        COALESCE.with(|effect| {
            let mut effect = effect.borrow_mut();
            if HAS_JUST_TAB.swap(false, std::sync::atomic::Ordering::Relaxed) {
                effect.reset();
            }
            effect.process(FRAME_TIME.into(), buf, main_area);
        });
    }

    fn draw_manager_tab(&self, frame: &mut Frame, main_area: Rect) {
        let buf = frame.buffer_mut();

        // Render sidebar and get the remaining content area
        let content_area = SidebarRenderer::render_and_split(main_area, buf);

        let selected_fleet = crate::channels::channels::get_selected_fleet_id();
        if selected_fleet.is_some() {
            fleet_manager_renderer::render(content_area, buf);
        } else {
            let colony_manager_renderer = colony_manager_renderer::ColonyManagerRenderer::new();
            colony_manager_renderer.render(content_area, buf);
        }

        COALESCE.with(|effect| {
            if HAS_JUST_TAB.swap(false, std::sync::atomic::Ordering::Relaxed) {
                effect.borrow_mut().reset();
            }
            effect
                .borrow_mut()
                .process(FRAME_TIME.into(), buf, main_area);
        });
    }

    fn draw_splash(&mut self, frame: &mut Frame) {
        let area = frame.area();

        let art = vec![
            Line::from(
                "                .                                            .                  ",
            ),
            Line::from(
                "     *   .                  .              .        .   *          .            ",
            ),
            Line::from(
                "  .         .                     .       .           .      .        .         ",
            ),
            Line::from(
                "        o                             .                   .                     ",
            ),
            Line::from(
                "         .              .                  .           .                        ",
            ),
            Line::from(
                "          0     .                                                               ",
            ),
            Line::from(
                "                 .          .                 ,                ,    ,           ",
            ),
            Line::from(
                " .          \\          .                         .                             ",
            ),
            Line::from(
                "      .      \\   ,                                                             ",
            ),
            Line::from(
                "   .          o     .                 .                   .            .        ",
            ),
            Line::from(
                "     .         \\                 ,             .                .              ",
            ),
            Line::from(
                "               #\\##\\#      .                              .        .          ",
            ),
            Line::from(
                "             #  #O##\\###                .                        .             ",
            ),
            Line::from(
                "   .        #*#  #\\##\\###                       .                     ,       ",
            ),
            Line::from(
                "        .   ##*#  #\\##\\##               .                     .               ",
            ),
            Line::from(
                "      .      ##*#  #o##\\#         .                             ,       .      ",
            ),
            Line::from(
                "          .     *#  #\\#     .                    .             .          ,    ",
            ),
            Line::from(
                "                      \\          .                         .                   ",
            ),
            Line::from(
                "____^/\\___^--____/\\____O______________/\\/\\---/\\___________---______________",
            ),
            Line::from(
                "   /\\^   ^  ^    ^                  ^^ ^  '\\ ^          ^       ---           ",
            ),
            Line::from(
                "   --  __                      ___--  ^  ^                         --  __       ",
            ),
        ];

        // let text = vec![
        //     Line::from("██╗███╗   ██╗██╗     ██╗███╗   ██╗███████╗    ██████╗ ██╗      █████╗ ███╗   ██╗███████╗████████╗███████╗"),
        //     Line::from("██║████╗  ██║██║     ██║████╗  ██║██╔════╝    ██╔══██╗██║     ██╔══██╗████╗  ██║██╔════╝╚══██╔══╝██╔════╝"),
        //     Line::from("██║██╔██╗ ██║██║     ██║██╔██╗ ██║█████╗      ██████╔╝██║     ███████║██╔██╗ ██║█████╗     ██║   ███████╗"),
        //     Line::from("██║██║╚██╗██║██║     ██║██║╚██╗██║██╔══╝      ██╔═══╝ ██║     ██╔══██║██║╚██╗██║██╔══╝     ██║   ╚════██║"),
        //     Line::from("██║██║ ╚████║███████╗██║██║ ╚████║███████╗    ██║     ███████╗██║  ██║██║ ╚████║███████╗   ██║   ███████║"),
        //     Line::from("╚═╝╚═╝  ╚═══╝╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝    ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚══════╝"),
        // ];

        let text = vec![
            Line::from("                                                                                       "),
            Line::from("██╗███╗   ██╗██╗     ██╗███╗   ██╗███████╗    ███████╗████████╗ █████╗ ██████╗ ███████╗"),
            Line::from("██║████╗  ██║██║     ██║████╗  ██║██╔════╝    ██╔════╝╚══██╔══╝██╔══██╗██╔══██╗██╔════╝"),
            Line::from("██║██╔██╗ ██║██║     ██║██╔██╗ ██║█████╗      ███████╗   ██║   ███████║██████╔╝███████╗"),
            Line::from("██║██║╚██╗██║██║     ██║██║╚██╗██║██╔══╝      ╚════██║   ██║   ██╔══██║██╔══██╗╚════██║"),
            Line::from("██║██║ ╚████║███████╗██║██║ ╚████║███████╗    ███████║   ██║   ██║  ██║██║  ██║███████║"),
            Line::from("╚═╝╚═╝  ╚═══╝╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝    ╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝"),
            Line::from("                                                                                       "),
        ];

        let header_p = Paragraph::new(text)
            .alignment(layout::HorizontalAlignment::Center)
            .fg(Color::Yellow);

        let art_p = Paragraph::new(art)
            .alignment(layout::HorizontalAlignment::Center)
            .fg(Color::Green);

        let contiue_text = Paragraph::new(">Press any key to continue...<")
            .alignment(layout::HorizontalAlignment::Center)
            .fg(Color::Red);

        let block = Block::bordered()
            .style(Style::default().fg(Color::White))
            .padding(Padding::bottom(5));

        let inner_area = block.inner(area);
        block.render(area, frame.buffer_mut());

        let layout = layout::Layout::default()
            .direction(layout::Direction::Vertical)
            .constraints(vec![
                Constraint::Percentage(50),
                Constraint::Percentage(50),
                Constraint::Min(1),
            ])
            .flex(layout::Flex::Center)
            .split(inner_area);

        let art_area = layout[1];
        let header_area = layout[0];

        art_p.render(art_area, frame.buffer_mut());
        header_p.render(header_area, frame.buffer_mut());
        contiue_text.render(layout[2], frame.buffer_mut());
    }

    fn draw_main_menu(&mut self, frame: &mut Frame) {
        let area = frame.area();
        let area = Layout::vertical(vec![
            Constraint::Percentage(33),
            Constraint::Percentage(33),
            Constraint::Percentage(33),
        ])
        .flex(layout::Flex::Center)
        .split(area)[1];
        let area = Layout::horizontal(vec![
            Constraint::Percentage(33),
            Constraint::Percentage(33),
            Constraint::Percentage(33),
        ])
        .flex(layout::Flex::Center)
        .split(area)[1];

        let menu_items = vec![
            Line::from("Start New Game"),
            Line::from("Load Game"),
            Line::from("Settings"),
            Line::from("Exit"),
        ];

        let menu = List::new(menu_items)
            .block(
                Block::bordered()
                    .style(Style::default())
                    .padding(Padding {
                        left: 1,
                        right: 1,
                        top: 1,
                        bottom: 1,
                    })
                    .title(Line::from(" Main Menu ")),
            )
            .style(Style::default())
            .highlight_style(Style::default().red().bold())
            .highlight_symbol("->");

        let mut state = with_ui_state(|ui_state| ui_state.main_menu_state.clone());
        ratatui::widgets::StatefulWidget::render(menu, area, frame.buffer_mut(), &mut state);

        // Popup renders on top of the main menu
        let popup = with_ui_state(|s| s.save_load_popup.clone());
        if popup != SaveLoadPopup::Hidden {
            render_save_load_popup(frame.area(), frame.buffer_mut(), &popup);
        }
    }

    fn draw_game_screen(&mut self, frame: &mut Frame) {
        let [top_bar_area, tab_area, main_area] = Layout::vertical([
            Constraint::Min(3),
            Constraint::Min(3),
            Constraint::Fill(100),
        ])
        .areas(frame.area());

        let player_states = get_player_states();
        let player_state = player_states.first().unwrap();
        let player_id = player_state.id.clone();
        let colonies = get_player_colonies(player_id);
        let total_population: u32 = colonies.iter().map(|c| c.resources.population).sum();

        let [tab_area, sim_info_area] =
            Layout::horizontal([Constraint::Percentage(50), Constraint::Percentage(50)])
                .flex(layout::Flex::SpaceBetween)
                .areas(tab_area);

        let text = format!(
            " {} Population: {} Wealth: {}",
            player_state.name, total_population, player_state.wealth
        );
        let top_bar =
            Paragraph::new(text).block(Block::bordered().style(Style::default().fg(Color::White)));

        top_bar.render(top_bar_area, frame.buffer_mut());
        //Set selected tab from state
        self.tabs = self
            .tabs
            .clone()
            .select(with_ui_state(|ui_state| ui_state.selected_tab as usize));

        sim_info_renderer::render(sim_info_area, frame.buffer_mut());

        let tab_areas = self
            .tabs
            .clone()
            .render_into_areas(tab_area, frame.buffer_mut());

        let selected_tab = with_ui_state(|ui_state| ui_state.selected_tab);

        match selected_tab {
            GameScreenTab::Research => {
                science_renderer::render(main_area, frame.buffer_mut());
            }
            GameScreenTab::SubsystemDesigner => {
                subsystem_designer_renderer::render(main_area, frame.buffer_mut());
            }
            GameScreenTab::ShipDesigner => {
                ship_designer_renderer::render(main_area, frame.buffer_mut());
            }
            GameScreenTab::SystemView => {
                self.draw_map_tab(frame, main_area);
            }
            GameScreenTab::Manager => {
                self.draw_manager_tab(frame, main_area);
            }
            _ => return,
        }

        with_ui_info_mut(|ui_info| {
            ui_info.tab_area = tab_area;
            ui_info.tab_areas = tab_areas;
        });

        // Save/load popup renders on top of everything else
        let popup = with_ui_state(|s| s.save_load_popup.clone());
        if popup != SaveLoadPopup::Hidden {
            render_save_load_popup(frame.area(), frame.buffer_mut(), &popup);
        }
    }

    fn run_sim_worker(&self) {
        //Start sim thread:
        tokio::spawn(async move {
            //INIT
            block_in_place(|| {
                new_game();
            });

            //This now lives in this thread, so we can update it at a fixed timestep, and publish the latest snapshot to the channel;
            let mut star_map = (*get_star_map_state()).clone();
            let mut time_scale_index: usize = 0; //Start at 0 realtime;

            //END_INIT
            loop {
                let start = std::time::Instant::now();
                time_scale_index = TIME_SCALE.load(atomic::Ordering::Relaxed) as usize;
                let time_per_tick =
                    FRAME_TIME.as_secs_f64() * TimeScale::SCALE_ARRAY[time_scale_index];

                //Update sim state:
                tokio::task::block_in_place(|| {
                    star_map.update(time_per_tick);
                    with_mut_science_manager(|m| m.update(time_per_tick));
                });

                //Measure
                let elapsed = start.elapsed();
                let elapsed_ms = (elapsed.as_secs_f64() * 1000_000.0) as i32;
                ELAPSED_SIM.store(elapsed_ms, atomic::Ordering::Relaxed);

                //Pad the sim time to ensure a consistent frame time, even if the sim update is very fast.
                //This prevents the sim loop from running too fast and consuming 100% CPU.
                let sleep_time = FRAME_TIME.saturating_sub(elapsed);
                tokio::time::sleep(sleep_time).await;

                if get_input_state().terminate {
                    break;
                }

                if JUST_LOADED_GAME.swap(false, atomic::Ordering::Relaxed){
                    star_map = (*get_star_map_state()).clone();
                }

                let elapsed = start.elapsed();
                let elapsed_full_ms = (elapsed.as_secs_f64() * 1000_000.0) as i32;
                ELAPSED_FULL_SIM.store(elapsed_full_ms, atomic::Ordering::Relaxed);
            }
        });
    }

    async fn run_render_worker(&mut self, terminal: &mut DefaultTerminal) -> io::Result<()> {
        // Main loop
        loop {
            // Pull interaction state from other workers and layout info from the last frame.
            let ui_state: UIState = get_ui_state_from_channel();
            let ui_info: UIInfo = get_ui_info_from_channel();
            replace_ui_state(ui_state);
            replace_ui_info(ui_info);

            terminal.draw(|frame| {
                self.draw(frame);
            })?;

            set_ui_info_to_channel(current_ui_info());

            tokio::time::sleep(FRAME_TIME).await;

            if get_input_state().terminate {
                break;
            }
        }

        Ok(())
    }

    pub fn pause(){

    }
}

fn centered_popup_rect(width: u16, height: u16, area: Rect) -> Rect {
    let x = area.x + area.width.saturating_sub(width) / 2;
    let y = area.y + area.height.saturating_sub(height) / 2;
    Rect { x, y, width: width.min(area.width), height: height.min(area.height) }
}

fn render_save_load_popup(area: Rect, buf: &mut ratatui::prelude::Buffer, popup: &SaveLoadPopup) {
    use ratatui::widgets::Clear;

    match popup {
        SaveLoadPopup::Save { name } => {
            let popup_area = centered_popup_rect(60, 7, area);
            Clear.render(popup_area, buf);

            let block = Block::bordered()
                .title(" Save Game ")
                .border_style(Style::default().fg(ACTIVE_COLOR));
            let inner = block.inner(popup_area);
            block.render(popup_area, buf);

            let [prompt_a, input_a, _, hint_a] = Layout::vertical([
                Constraint::Length(1),
                Constraint::Length(1),
                Constraint::Fill(1),
                Constraint::Length(1),
            ])
            .areas(inner);

            Paragraph::new("Save file name:").render(prompt_a, buf);
            Paragraph::new(format!("> {}_", name))
                .style(Style::default().fg(ACTIVE_COLOR))
                .render(input_a, buf);
            Paragraph::new("Enter: save | ESC: cancel")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        SaveLoadPopup::Load { saves, selected } => {
            let height = (saves.len() as u16 + 4).max(6).min(area.height);
            let popup_area = centered_popup_rect(60, height, area);
            Clear.render(popup_area, buf);

            let block = Block::bordered()
                .title(" Load Game ")
                .border_style(Style::default().fg(ACTIVE_COLOR));
            let inner = block.inner(popup_area);
            block.render(popup_area, buf);

            let [list_a, hint_a] =
                Layout::vertical([Constraint::Fill(1), Constraint::Length(1)]).areas(inner);

            if saves.is_empty() {
                Paragraph::new("  [no save files found]")
                    .style(Style::default().fg(INACTIVE_COLOR))
                    .render(list_a, buf);
            } else {
                let lines: Vec<Line> = saves
                    .iter()
                    .enumerate()
                    .map(|(i, s)| {
                        if i == *selected {
                            Line::from(format!("❯ {}", s)).fg(ACTIVE_COLOR).bold()
                        } else {
                            Line::from(format!("  {}", s))
                        }
                    })
                    .collect();
                Paragraph::new(lines).render(list_a, buf);
            }

            Paragraph::new("↑↓: select | Enter: load | ESC: cancel")
                .style(Style::default().fg(INACTIVE_COLOR))
                .render(hint_a, buf);
        }
        SaveLoadPopup::Hidden => {}
    }
}
