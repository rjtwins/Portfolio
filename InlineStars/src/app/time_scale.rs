pub struct TimeScale;

impl TimeScale {
    pub const REALTIME: f64 = 1.0;
    pub const SEC_10: f64 = 10.0;
    pub const SEC_30: f64 = 30.0;
    pub const MINUTE: f64 = 60.0;
    pub const MINUTES_5: f64 = 60.0 * 5.0;
    pub const MINUTES_10: f64 = 60.0 * 10.0;
    pub const MINUTES_30: f64 = 60.0 * 30.0;
    pub const HOUR: f64 = 60.0 * 60.0;
    pub const HOURS_6: f64 = 60.0 * 60.0 * 6.0;
    pub const HOURS_12: f64 = 60.0 * 60.0 * 12.0;
    pub const DAY: f64 = 60.0 * 60.0 * 24.0;
    pub const WEEK: f64 = Self::DAY * 7.0;
    pub const MONTH: f64 = Self::DAY * 30.0;
    pub const MONTHS_3: f64 = Self::DAY * 30.0 * 3.0;
    pub const MONTHS_6: f64 = Self::DAY * 30.0 * 6.0;
    pub const MONTHS_9: f64 = Self::DAY * 30.0 * 9.0;
    pub const YEAR: f64 = Self::DAY * 365.0;

    pub const SCALE_ARRAY: [f64; 17] = [
        Self::REALTIME,
        Self::SEC_10,
        Self::SEC_30,
        Self::MINUTE,
        Self::MINUTES_5,
        Self::MINUTES_10,
        Self::MINUTES_30,
        Self::HOUR,
        Self::HOURS_6,
        Self::HOURS_12,
        Self::DAY,
        Self::WEEK,
        Self::MONTH,
        Self::MONTHS_3,
        Self::MONTHS_6,
        Self::MONTHS_9,
        Self::YEAR,
    ];
}
