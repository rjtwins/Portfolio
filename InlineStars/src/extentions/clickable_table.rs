use ratatui::{
    layout::{Constraint, Layout, Rect}, widgets::{Row, Table, TableState}
};

#[derive(Clone, Copy)]
pub struct CellInfo {
    pub row: usize,
    pub col: usize,
    pub rect: Rect,
}

/// Wraps [`TableState`] with key-based row selection.
///
/// Keys are stored alongside the index so that selections survive row
/// reorders / re-renders and can be driven from domain identifiers (e.g. UUIDs)
/// rather than fragile numeric indices.
#[derive(Clone)]
pub struct ClickableTableState<K> {
    inner: TableState,
    selected_key: Option<K>,
    keys: Vec<K>,
}

impl<K: Clone + PartialEq> Default for ClickableTableState<K> {
    fn default() -> Self {
        Self::new(vec![])
    }
}

impl<K: Clone + PartialEq> ClickableTableState<K> {
    pub fn new(keys: Vec<K>) -> Self {
        Self {
            inner: TableState::default(),
            selected_key: None,
            keys,
        }
    }

    /// Select a row by its key. No-ops if the key is not found.
    pub fn select_by_key(&mut self, key: &K) {
        if let Some(index) = self.keys.iter().position(|k| k == key) {
            self.selected_key = Some(key.clone());
            self.inner.select(Some(index));
        }
    }

    /// Select a row by its index and derive the key from the stored key list.
    pub fn select_by_index(&mut self, index: usize) {
        self.selected_key = self.keys.get(index).cloned();
        self.inner.select(Some(index));
    }

    pub fn deselect(&mut self) {
        self.selected_key = None;
        self.inner.select(None);
    }

    /// Returns the key for the given row index, if any.
    pub fn key_for_index(&self, index: usize) -> Option<&K> {
        self.keys.get(index)
    }

    /// Returns the key of the currently selected row.
    ///
    /// Mirrors the `TreeState::selected()` API from `tui-tree-widget`.
    pub fn selected(&self) -> Option<&K> {
        self.selected_key.as_ref()
    }

    /// Select a row by its key. No-ops if the key is not found.
    ///
    /// Mirrors the `TreeState::select()` API from `tui-tree-widget`.
    pub fn select(&mut self, key: &K) {
        self.select_by_key(key);
    }

    /// Select the next row, wrapping around to the first if at the end.
    /// If nothing is selected, selects the first row.
    pub fn next(&mut self) {
        if self.keys.is_empty() {
            return;
        }
        let next_index = match self.inner.selected() {
            Some(i) => (i + 1) % self.keys.len(),
            None => 0,
        };
        self.select_by_index(next_index);
    }

    /// Select the previous row, wrapping around to the last if at the start.
    /// If nothing is selected, selects the last row.
    pub fn previous(&mut self) {
        if self.keys.is_empty() {
            return;
        }
        let prev_index = match self.inner.selected() {
            Some(i) => if i == 0 { self.keys.len() - 1 } else { i - 1 },
            None => self.keys.len().saturating_sub(1),
        };
        self.select_by_index(prev_index);
    }

    pub fn selected_key(&self) -> Option<&K> {
        self.selected_key.as_ref()
    }

    pub fn selected_index(&self) -> Option<usize> {
        self.inner.selected()
    }

    pub fn inner(&self) -> &TableState {
        &self.inner
    }

    pub fn inner_mut(&mut self) -> &mut TableState {
        &mut self.inner
    }

    /// Replace the key list (e.g. when underlying data changes).
    /// Re-syncs the selected index from the stored key; deselects if the key
    /// is no longer present.
    pub fn update_keys(&mut self, keys: Vec<K>) {
        self.keys = keys;
        if let Some(key) = &self.selected_key {
            let index = self.keys.iter().position(|k| k == key);
            self.inner.select(index);
            if index.is_none() {
                self.selected_key = None;
            }
        }
    }
}

/// A row paired with a typed key for use with [`ClickableTable::new_keyed`].
pub struct KeyedRow<'a, K> {
    pub key: K,
    pub row: Row<'a>,
}

impl<'a, K> KeyedRow<'a, K> {
    pub fn new(key: K, row: impl Into<Row<'a>>) -> Self {
        Self { key, row: row.into() }
    }
}

/// A [`Table`] wrapper that tracks per-cell hit areas for mouse interaction
/// and optionally associates each row with a typed key.
///
/// The default key type is `usize` (row index) for backward compatibility.
/// Use [`ClickableTable::new_keyed`] to supply custom keys.
pub struct ClickableTable<'a, K = usize> {
    table: Table<'a>,
    cells: Vec<CellInfo>,
    widths: Vec<Constraint>,
    block: Option<ratatui::widgets::Block<'a>>,
    row_count: usize,
    col_count: usize,
    has_header: bool,
    column_spacing: u16,
    keys: Vec<K>,
}

// ---------------------------------------------------------------------------
// Constructors
// ---------------------------------------------------------------------------

impl<'a> ClickableTable<'a, usize> {
    /// Create a table whose rows are keyed by their zero-based index.
    pub fn new(rows: Vec<Row<'a>>, widths: Vec<Constraint>) -> Self {
        let len = widths.len();
        let row_count = rows.len();
        let keys: Vec<usize> = (0..row_count).collect();
        Self {
            table: Table::new(rows, widths.clone()),
            cells: Vec::new(),
            widths,
            row_count,
            col_count: len,
            block: None,
            has_header: false,
            column_spacing: 0,
            keys,
        }
    }
}

impl<'a, K: Clone> ClickableTable<'a, K> {
    /// Create a table where each row carries an explicit key.
    pub fn new_keyed(rows: Vec<KeyedRow<'a, K>>, widths: Vec<Constraint>) -> Self {
        let len = widths.len();
        let row_count = rows.len();
        let (keys, raw_rows): (Vec<K>, Vec<Row<'a>>) = rows.into_iter().map(|kr| (kr.key, kr.row)).unzip();
        Self {
            table: Table::new(raw_rows, widths.clone()),
            cells: Vec::new(),
            widths,
            row_count,
            col_count: len,
            block: None,
            has_header: false,
            column_spacing: 0,
            keys,
        }
    }
}

impl<'a, K> ClickableTable<'a, K> {
    fn into_cells(self) -> Vec<CellInfo> {
        self.cells
    }

    /// Returns the ordered key slice for this table.
    pub fn keys(&self) -> &[K] {
        &self.keys
    }

    pub fn widths<I>(mut self, widths: I) -> Self
    where
        I: IntoIterator,
        I::Item: Into<Constraint>,
    {
        self.widths = widths.into_iter().map(|w| w.into()).collect();
        self.col_count = self.widths.len();
        self.table = self.table.widths(self.widths.clone());
        self
    }

    pub fn header(mut self, header: Row<'a>) -> Self {
        self.table = self.table.header(header);
        self.has_header = true;
        self
    }

    pub fn block(mut self, block: ratatui::widgets::Block<'a>) -> Self {
        self.block = Some(block.clone());
        self.table = self.table.block(block);
        self
    }

    pub fn column_spacing(mut self, spacing: u16) -> Self {
        self.table = self.table.column_spacing(spacing);
        self.column_spacing = spacing;
        self
    }

    pub fn style<S: Into<ratatui::style::Style>>(mut self, style: S) -> Self {
        self.table = self.table.style(style.into());
        self
    }

    pub fn row_highlight_style<S: Into<ratatui::style::Style>>(mut self, style: S) -> Self {
        self.table = self.table.row_highlight_style(style.into());
        self
    }

    pub fn cell_highlight_style<S: Into<ratatui::style::Style>>(mut self, style: S) -> Self {
        self.table = self.table.cell_highlight_style(style.into());
        self
    }

    pub fn column_highlight_style<S: Into<ratatui::style::Style>>(mut self, style: S) -> Self {
        self.table = self.table.column_highlight_style(style.into());
        self
    }

    pub fn render_into_cells(mut self, area: Rect, buf: &mut ratatui::prelude::Buffer) -> Vec<CellInfo> {
        ratatui::widgets::Widget::render(self.table.clone(), area, buf);
        self.cells = self.calculate_cell_info(area);
        self.into_cells()
    }

    /// Render with a raw [`TableState`] (backward-compatible, no key tracking).
    pub fn render_stateful_into_cells(mut self, area: Rect, buf: &mut ratatui::prelude::Buffer, state: &mut TableState) -> Vec<CellInfo> {
        ratatui::widgets::StatefulWidget::render(self.table.clone(), area, buf, state);
        self.cells = self.calculate_cell_info(area);
        self.into_cells()
    }

    /// Render with a [`ClickableTableState`] for key-aware selection.
    pub fn render_stateful_keyed_into_cells(mut self, area: Rect, buf: &mut ratatui::prelude::Buffer, state: &mut ClickableTableState<K>) -> Vec<CellInfo>
    where
        K: Clone + PartialEq,
    {
        ratatui::widgets::StatefulWidget::render(self.table.clone(), area, buf, state.inner_mut());
        self.cells = self.calculate_cell_info(area);
        self.into_cells()
    }

    fn calculate_cell_info(&self, area: Rect) -> Vec<CellInfo> {
        let mut cells = Vec::new();

        let area = if let Some(block) = &self.block {
            block.inner(area)
        } else {
            area
        };

        let colls: Vec<Rect> = Layout::horizontal(self.widths.clone())
            .split(area)
            .to_vec();

        let col_widths = colls.iter().map(|c| c.width).collect::<Vec<u16>>();

        let mut y = area.y;
        if self.has_header {
            y = y.saturating_add(2); // still assumes header height == 2
        }

        for row in 0..self.row_count {
            let mut x = area.x;

            for col in 0..self.col_count {
                let w = col_widths.get(col).copied().unwrap_or(0);

                cells.push(CellInfo {
                    row,
                    col,
                    rect: Rect {
                        x,
                        y,
                        width: w,
                        height: 1, // still assumes row height == 1
                    },
                });

                x = x.saturating_add(w);
                if col + 1 < self.col_count {
                    x = x.saturating_add(self.column_spacing);
                }
            }

            y = y.saturating_add(1);
        }

        cells
    }
}
