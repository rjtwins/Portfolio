use ratatui::{
    layout::Rect,
    prelude::Buffer,
    style::Style,
    text::{Line, Span},
    widgets::{Block, Tabs, Widget},
};

#[derive(Clone)]
pub struct ClickableTabs<'a> {
    tabs: Tabs<'a>,
    titles: Vec<Line<'a>>,
    block: Option<Block<'a>>,
    divider: Span<'a>,
    padding_left: Line<'a>,
    padding_right: Line<'a>,
}

impl<'a> ClickableTabs<'a> {
    pub fn new<T>(titles: T) -> Self
    where
        T: IntoIterator,
        T::Item: Into<Line<'a>>,
    {
        let titles: Vec<Line<'a>> = titles.into_iter().map(Into::into).collect();

        Self {
            tabs: Tabs::new(titles.clone()),
            titles,
            block: None,
            divider: Span::raw(""),
            padding_left: Line::from(""),
            padding_right: Line::from(""),
        }
    }

    pub fn style<S: Into<Style>>(mut self, style: S) -> Self {
        self.tabs = self.tabs.style(style.into());
        self
    }

    pub fn block(mut self, block: Block<'a>) -> Self {
        self.block = Some(block.clone());
        self.tabs = self.tabs.block(block);
        self
    }

    pub fn highlight_style<S: Into<Style>>(mut self, style: S) -> Self {
        self.tabs = self.tabs.highlight_style(style.into());
        self
    }

    pub fn select(mut self, selected: usize) -> Self {
        self.tabs = self.tabs.select(selected);
        self
    }

    pub fn divider<T: Into<Span<'a>>>(mut self, divider: T) -> Self {
        let divider_span: Span<'a> = divider.into();
        self.divider = divider_span.clone();
        self.tabs = self.tabs.divider(self.divider.clone());
        self
    }

    pub fn padding<L, R>(mut self, left: L, right: R) -> Self
    where
        L: Into<Line<'a>>,
        R: Into<Line<'a>>,
    {
        let left = left.into();
        let right = right.into();
        self.padding_left = left.clone();
        self.padding_right = right.clone();
        self.tabs = self.tabs.padding(left, right);
        self
    }

    pub fn render_into_areas(self, area: Rect, buf: &mut Buffer) -> Vec<(usize, Rect)> {
        self.tabs.clone().render(area, buf);
        self.calculate_tab_areas(area)
    }

    fn calculate_tab_areas(&self, area: Rect) -> Vec<(usize, Rect)> {
        let area = match &self.block {
            Some(block) => block.inner(area),
            None => area,
        };

        let mut x = area.x;
        let tab_height = area.height.max(1);
        let divider_width = self.divider.width() as u16;
        let left_padding_width = self.padding_left.width() as u16;
        let right_padding_width = self.padding_right.width() as u16;

        self.titles
            .iter()
            .enumerate()
            .map(|(index, title)| {
                let title_width = title.width() as u16;
                let width = left_padding_width
                    .saturating_add(title_width)
                    .saturating_add(right_padding_width);

                let rect = Rect {
                    x,
                    y: area.y,
                    width,
                    height: tab_height,
                };

                x = x.saturating_add(width);
                if index + 1 < self.titles.len() {
                    x = x.saturating_add(divider_width);
                }

                (index, rect)
            })
            .collect()
    }
}
