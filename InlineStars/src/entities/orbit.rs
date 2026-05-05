use serde::{Deserialize, Serialize};

#[derive(Clone, Serialize)]
pub struct Orbit {
    //Orbital parameters
    pub semi_major_axis: f64, // a
    pub eccentricity: f64,    // e (0 ≤ e < 1)
    pub arg_periapsis: f64,   // ω
    pub mean_motion: f64,     // n = 2π / period
    pub mean_anomaly: f64,    // M
    pub x: f64,
    pub y: f64,
    #[serde(skip)]
    pub orbit_segments: Vec<(f64, f64)>,
}

impl<'de> Deserialize<'de> for Orbit {
    fn deserialize<D>(deserializer: D) -> Result<Self, D::Error>
    where
        D: serde::Deserializer<'de>,
    {
        #[derive(Deserialize)]
        struct OrbitData {
            semi_major_axis: f64,
            eccentricity: f64,
            arg_periapsis: f64,
            mean_motion: f64,
            mean_anomaly: f64,
            x: f64,
            y: f64,
        }

        let data = OrbitData::deserialize(deserializer)?;
        let mut orbit = Orbit {
            semi_major_axis: data.semi_major_axis,
            eccentricity: data.eccentricity,
            arg_periapsis: data.arg_periapsis,
            mean_motion: data.mean_motion,
            mean_anomaly: data.mean_anomaly,
            x: data.x,
            y: data.y,
            orbit_segments: Vec::new(),
        };
        orbit.orbit_segments = orbit.get_orbital_segments(1000);
        Ok(orbit)
    }
}

impl Orbit {
    pub fn new(
        semi_major_axis: f64,
        eccentricity: f64,
        arg_periapsis: f64,
        mean_motion: f64,
    ) -> Self {
        let mut i = Self {
            semi_major_axis,
            eccentricity,
            arg_periapsis,
            mean_motion,
            mean_anomaly: 0.0,
            x: 0.0,
            y: 0.0,
            orbit_segments: Vec::new(),
        };

        i.orbit_segments = i.get_orbital_segments(1000);
        i
    }

    pub fn update_orbit(&mut self, delta_time: f64) {
        // advance mean anomaly

        let pi = std::f64::consts::PI;
        self.mean_anomaly += self.mean_motion * delta_time;
        self.mean_anomaly %= 2.0 * pi;

        let a = self.semi_major_axis;
        let e = self.eccentricity;

        // solve Kepler's equation: M = E - e sin(E)
        let mut E = self.mean_anomaly;
        for _ in 0..5 {
            let f = E - e * E.sin() - self.mean_anomaly;
            let fp = 1.0 - e * E.cos();
            E -= f / fp;
        }

        // ellipse parameters
        let b = a * (1.0 - e * e).sqrt();

        // position in orbital frame
        let x_prime = a * (E.cos() - e);
        let y_prime = b * E.sin();

        // rotate by argument of periapsis
        let cos_w = self.arg_periapsis.cos();
        let sin_w = self.arg_periapsis.sin();

        self.x = x_prime * cos_w - y_prime * sin_w;
        self.y = x_prime * sin_w + y_prime * cos_w;
    }

    pub fn get_position_at_time_from_now(&self, time: f64) -> (f64, f64) {
        let mut temp_orbit = self.clone();
        temp_orbit.update_orbit(time);
        (temp_orbit.x, temp_orbit.y)
    }

    pub fn progress_orbit_by_time(&mut self, time: f64) {
        self.update_orbit(time);
    }

    pub fn get_orbital_period(&self) -> f64 {
        if self.mean_motion == 0.0 {
            return f64::INFINITY;
        }
        2.0 * std::f64::consts::PI / self.mean_motion
    }

    fn get_orbital_segments(&mut self, num_segments: usize) -> Vec<(f64, f64)> {
        if (self.mean_motion == 0.0) {
            return vec![(self.x, self.y)];
        }

        let mut segments = Vec::new();
        let period = self.get_orbital_period();
        for i in 0..num_segments {
            let time = (i as f64 / num_segments as f64) * period;
            segments.push(self.get_position_at_time_from_now(time));
        }

        segments
    }
}
