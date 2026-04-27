use rand::RngExt;
use serde::Deserialize;
use crate::entities::planet::body::{BodyResources, ResourceDeposit};
use crate::entities::{orbit::Orbit, planet::{body::BodyType, Body}, star::Star};
pub mod star;
pub mod star_map;
pub mod planet;
pub mod orbit;
pub mod camera;
pub mod fleet;
pub mod player_state;
pub mod ship;

pub trait GameEntity<T> {
    fn get_name(&self) -> String;
    fn get_id(&self) -> String;
    fn get_orbit(&self) -> Option<Orbit>;
    fn update(&mut self, delta_time: f64);
    fn get_parent_position(&self) -> (f64, f64);
    fn get_global_position(&self) -> (f64, f64);
}

const AU_KM: f64 = 149_597_870.7;
const TWO_PI: f64 = std::f64::consts::PI * 2.0;
const SECONDS_PER_DAY: f64 = 24.0 * 60.0 * 60.0;
const DAYS_PER_YEAR: f64 = 365.256363004;
const BODY_MASS_SCALE_MIN: f64 = 0.7;
const BODY_MASS_SCALE_MAX: f64 = 1.3;
const RESOURCE_REAL_NUMBER_SCALE: f64 = 1.0e-16;

#[derive(Clone, Copy)]
struct ResourceTemplateEntry {
    amount_fraction: f64,
    extraction_difficulty: f64,
}

#[derive(Clone, Copy)]
struct LayerResourceTemplate {
    average_mass_kg: f64,
    fuel: ResourceTemplateEntry,
    light_metals: ResourceTemplateEntry,
    heavy_metals: ResourceTemplateEntry,
    rare_elements: ResourceTemplateEntry,
    super_elements: ResourceTemplateEntry,
}

#[derive(Clone, Copy)]
struct BodyGenerationTemplate {
    average_density_kg_per_km3: f64,
    surface: LayerResourceTemplate,
    mantle: LayerResourceTemplate,
    core: LayerResourceTemplate,
}

const fn resource_entry(amount_fraction: f64, extraction_difficulty: f64) -> ResourceTemplateEntry {
    ResourceTemplateEntry {
        amount_fraction,
        extraction_difficulty,
    }
}

const fn layer_template(
    average_mass_kg: f64,
    fuel: (f64, f64),
    light_metals: (f64, f64),
    heavy_metals: (f64, f64),
    rare_elements: (f64, f64),
    super_elements: (f64, f64),
) -> LayerResourceTemplate {
    LayerResourceTemplate {
        average_mass_kg,
        fuel: resource_entry(fuel.0, fuel.1),
        light_metals: resource_entry(light_metals.0, light_metals.1),
        heavy_metals: resource_entry(heavy_metals.0, heavy_metals.1),
        rare_elements: resource_entry(rare_elements.0, rare_elements.1),
        super_elements: resource_entry(super_elements.0, super_elements.1),
    }
}

impl LayerResourceTemplate {
    const fn empty() -> Self {
        layer_template(0.0, (0.0, 1.0), (0.0, 1.0), (0.0, 1.0), (0.0, 1.0), (0.0, 1.0))
    }

    fn generate_resources(self, layer_mass_kg: f64) -> BodyResources {
        BodyResources::new(
            calculate_resource_deposit(self.fuel, layer_mass_kg),
            calculate_resource_deposit(self.light_metals, layer_mass_kg),
            calculate_resource_deposit(self.heavy_metals, layer_mass_kg),
            calculate_resource_deposit(self.rare_elements, layer_mass_kg),
            calculate_resource_deposit(self.super_elements, layer_mass_kg),
        )
    }
}

impl BodyGenerationTemplate {
    fn average_mass_kg(self) -> f64 {
        self.surface.average_mass_kg + self.mantle.average_mass_kg + self.core.average_mass_kg
    }

    fn generate(self) -> (f64, f64, BodyResources, BodyResources, BodyResources) {
        let average_mass_kg = self.average_mass_kg();
        if average_mass_kg <= 0.0 {
            return (
                0.0,
                0.0,
                BodyResources::default(),
                BodyResources::default(),
                BodyResources::default(),
            );
        }

        let mass_kg = average_mass_kg * random_f64_in_range(BODY_MASS_SCALE_MIN, BODY_MASS_SCALE_MAX);
        let radius_km = calculate_radius_from_mass_and_density(mass_kg, self.average_density_kg_per_km3);

        let surface_mass_kg = mass_kg * self.surface.average_mass_kg / average_mass_kg;
        let mantle_mass_kg = mass_kg * self.mantle.average_mass_kg / average_mass_kg;
        let core_mass_kg = mass_kg * self.core.average_mass_kg / average_mass_kg;

        (
            mass_kg,
            radius_km,
            self.surface.generate_resources(surface_mass_kg),
            self.mantle.generate_resources(mantle_mass_kg),
            self.core.generate_resources(core_mass_kg),
        )
    }
}

fn calculate_resource_deposit(entry: ResourceTemplateEntry, layer_mass_kg: f64) -> ResourceDeposit {
    ResourceDeposit {
        amount: layer_mass_kg * entry.amount_fraction * RESOURCE_REAL_NUMBER_SCALE,
        extraction_difficulty: entry.extraction_difficulty,
    }
}

fn generation_template_for_body_type(body_type: BodyType) -> BodyGenerationTemplate {
    match body_type {
        BodyType::TerrestrialPlanet => BodyGenerationTemplate {
            average_density_kg_per_km3: 5.50e12,
            surface: layer_template(2.00e22, (1.0, 60.0), (15.0, 40.0), (5.0, 60.0), (3.0, 80.0), (1.5, 120.0)),
            mantle: layer_template(4.00e24, (2.0, 80.0), (50.0, 100.0), (60.0, 100.0), (5.0, 100.0), (2.5, 150.0)),
            core: layer_template(1.80e24, (10.0, 100.0), (5.0, 100.0), (100.0, 100.0), (10.0, 100.0), (5.0, 150.0)),
        },
        BodyType::RockyMoon => BodyGenerationTemplate {
            average_density_kg_per_km3: 3.20e12,
            surface: layer_template(7.00e22, (1.0, 40.0), (8.0, 40.0), (3.0, 40.0), (1.0, 60.0), (0.5, 90.0)),
            mantle: LayerResourceTemplate::empty(),
            core: LayerResourceTemplate::empty(),
        },
        BodyType::IcyMoon | BodyType::Comet => BodyGenerationTemplate {
            average_density_kg_per_km3: 1.50e12,
            surface: layer_template(3.00e22, (20.0, 40.0), (4.0, 60.0), (1.0, 80.0), (0.2, 80.0), (0.1, 120.0)),
            mantle: LayerResourceTemplate::empty(),
            core: LayerResourceTemplate::empty(),
        },
        BodyType::CTypeAsteroid => BodyGenerationTemplate {
            average_density_kg_per_km3: 1.70e12,
            surface: layer_template(1.00e17, (3.0, 20.0), (0.5, 40.0), (0.2, 60.0), (0.1, 80.0), (0.05, 120.0)),
            mantle: LayerResourceTemplate::empty(),
            core: LayerResourceTemplate::empty(),
        },
        BodyType::STypeAsteroid => BodyGenerationTemplate {
            average_density_kg_per_km3: 3.00e12,
            surface: layer_template(5.00e16, (0.2, 60.0), (1.5, 60.0), (0.5, 60.0), (0.2, 80.0), (0.1, 120.0)),
            mantle: LayerResourceTemplate::empty(),
            core: LayerResourceTemplate::empty(),
        },
        BodyType::MTypeAsteroid => BodyGenerationTemplate {
            average_density_kg_per_km3: 4.50e12,
            surface: layer_template(2.00e16, (0.1, 60.0), (1.0, 60.0), (2.0, 20.0), (1.5, 40.0), (0.75, 60.0)),
            mantle: LayerResourceTemplate::empty(),
            core: LayerResourceTemplate::empty(),
        },
        BodyType::GasGiant => BodyGenerationTemplate {
            average_density_kg_per_km3: 1.20e12,
            surface: layer_template(1.00e27, (500.0, 40.0), (5.0, 100.0), (1.0, 100.0), (0.0, 100.0), (0.0, 150.0)),
            mantle: LayerResourceTemplate::empty(),
            core: layer_template(1.00e25, (0.0, 100.0), (10.0, 100.0), (40.0, 100.0), (5.0, 100.0), (2.5, 150.0)),
        },
        BodyType::IceGiant => BodyGenerationTemplate {
            average_density_kg_per_km3: 1.60e12,
            surface: layer_template(8.00e25, (300.0, 80.0), (80.0, 100.0), (20.0, 100.0), (2.0, 100.0), (1.0, 150.0)),
            mantle: LayerResourceTemplate::empty(),
            core: layer_template(2.00e25, (0.0, 100.0), (10.0, 100.0), (30.0, 100.0), (3.0, 100.0), (1.5, 150.0)),
        },
        BodyType::Star | BodyType::Fleet => BodyGenerationTemplate {
            average_density_kg_per_km3: 1.0,
            surface: LayerResourceTemplate::empty(),
            mantle: LayerResourceTemplate::empty(),
            core: LayerResourceTemplate::empty(),
        },
    }
}

fn create_body_properties_for_body_type(
    body_type: BodyType,
) -> (f64, f64, BodyResources, BodyResources, BodyResources) {
    generation_template_for_body_type(body_type).generate()
}

fn calculate_radius_from_mass_and_density(mass_kg: f64, density_kg_per_km3: f64) -> f64 {
    let volume_km3 = mass_kg / density_kg_per_km3.max(f64::EPSILON);
    ((3.0 * volume_km3) / (4.0 * std::f64::consts::PI)).cbrt()
}

fn calculate_mean_motion_rads_per_sec(semi_major_axis_km: f64) -> f64 {
    let semi_major_axis_au = (semi_major_axis_km / AU_KM).max(f64::EPSILON);
    let orbital_period_days = DAYS_PER_YEAR * semi_major_axis_au.powf(1.5);
    let mean_motion_rads_per_day = TWO_PI / orbital_period_days;

    mean_motion_rads_per_day / SECONDS_PER_DAY
}

fn random_count_in_range(min_count: usize, max_count: usize) -> usize {
    if max_count <= min_count {
        return min_count;
    }

    rand::rng().random_range(min_count..=max_count)
}

fn random_f64_in_range(min_value: f64, max_value: f64) -> f64 {
    if max_value <= min_value {
        return min_value;
    }

    rand::rng().random_range(min_value..=max_value)
}

fn create_random_orbit(
    min_semi_major_axis: f64,
    max_semi_major_axis: f64,
    min_eccentricity: f64,
    max_eccentricity: f64,
) -> Orbit {
    let semi_major_axis = random_f64_in_range(min_semi_major_axis, max_semi_major_axis);
    let eccentricity = random_f64_in_range(min_eccentricity, max_eccentricity).clamp(0.0, 0.95);
    let arg_periapsis = random_f64_in_range(0.0, TWO_PI);
    let mean_motion = calculate_mean_motion_rads_per_sec(semi_major_axis);

    let mut orbit = Orbit::new(semi_major_axis, eccentricity, arg_periapsis, mean_motion);
    let period = orbit.get_orbital_period();
    if period.is_finite() {
        orbit.progress_orbit_by_time(random_f64_in_range(0.0, period));
    }

    orbit
}

fn generate_body_with_random_orbit(
    name: String,
    star_id: String,
    parent_x: f64,
    parent_y: f64,
    body_type: BodyType,
    min_semi_major_axis: f64,
    max_semi_major_axis: f64,
    min_eccentricity: f64,
    max_eccentricity: f64,
) -> Body {
    let (mass_kg, radius_km, surface_resources, mantle_resources, core_resources) =
        create_body_properties_for_body_type(body_type);
    let mut body = Body::new(
        name,
        star_id,
        parent_x,
        parent_y,
        body_type,
        mass_kg,
        radius_km,
        surface_resources,
        mantle_resources,
        core_resources,
    );
    body.orbit = Some(create_random_orbit(
        min_semi_major_axis,
        max_semi_major_axis,
        min_eccentricity,
        max_eccentricity,
    ));
    body
}

fn generate_moon_type_for_parent(parent: &Body) -> BodyType {
    match parent.body_type {
        BodyType::TerrestrialPlanet => {
            if random_f64_in_range(0.0, 1.0) < 0.7 {
                BodyType::RockyMoon
            } else {
                BodyType::IcyMoon
            }
        }
        BodyType::GasGiant | BodyType::IceGiant => {
            if random_f64_in_range(0.0, 1.0) < 0.7 {
                BodyType::IcyMoon
            } else {
                BodyType::RockyMoon
            }
        }
        _ => {
            if random_f64_in_range(0.0, 1.0) < 0.5 {
                BodyType::RockyMoon
            } else {
                BodyType::IcyMoon
            }
        }
    }
}

fn generate_moons_for_body(
    body: &Body,
    name_prefix: &str,
    min_moons: usize,
    max_moons: usize,
    min_semi_major_axis: f64,
    max_semi_major_axis: f64,
    min_eccentricity: f64,
    max_eccentricity: f64,
) -> Vec<Body> {
    let moon_count = random_count_in_range(min_moons, max_moons);
    if moon_count == 0 {
        return Vec::new();
    }

    let band_width = (max_semi_major_axis - min_semi_major_axis).max(1.0);
    let slot_width = band_width / moon_count as f64;

    (0..moon_count)
        .map(|index| {
            let slot_start = min_semi_major_axis + slot_width * index as f64;
            let slot_end = min_semi_major_axis + slot_width * (index + 1) as f64;
            let margin = (slot_width * 0.15).min(slot_width / 2.0);

            generate_body_with_random_orbit(
                format!("{name_prefix} {}", index + 1),
                body.star_id.clone(),
                body.parent_x,
                body.parent_y,
                generate_moon_type_for_parent(body),
                (slot_start + margin).min(slot_end),
                (slot_end - margin).max(slot_start + margin),
                min_eccentricity,
                max_eccentricity,
            )
        })
        .collect()
}

fn generate_planets_for_star_in_band(
    star: &Star,
    name_prefix: &str,
    start_index: usize,
    min_planets: usize,
    max_planets: usize,
    min_semi_major_axis: f64,
    max_semi_major_axis: f64,
    min_eccentricity: f64,
    max_eccentricity: f64,
    min_moons: usize,
    max_moons: usize,
    moon_min_semi_major_axis: f64,
    moon_max_semi_major_axis: f64,
    moon_min_eccentricity: f64,
    moon_max_eccentricity: f64,
) -> Vec<Body> {
    let planet_count = random_count_in_range(min_planets, max_planets);
    if planet_count == 0 {
        return Vec::new();
    }

    let band_width = (max_semi_major_axis - min_semi_major_axis).max(1.0);
    let slot_width = band_width / planet_count as f64;

    (0..planet_count)
        .map(|index| {
            let slot_start = min_semi_major_axis + slot_width * index as f64;
            let slot_end = min_semi_major_axis + slot_width * (index + 1) as f64;
            let margin = (slot_width * 0.2).min(slot_width / 2.0);

            let mut planet = generate_body_with_random_orbit(
                format!("{name_prefix} {}", start_index + index),
                star.id.clone(),
                star.parent_x,
                star.parent_y,
                BodyType::TerrestrialPlanet,
                (slot_start + margin).min(slot_end),
                (slot_end - margin).max(slot_start + margin),
                min_eccentricity,
                max_eccentricity,
            );

            planet.moons = generate_moons_for_body(
                &planet,
                &format!("{} Moon", planet.name),
                min_moons,
                max_moons,
                moon_min_semi_major_axis,
                moon_max_semi_major_axis,
                moon_min_eccentricity,
                moon_max_eccentricity,
            );

            planet
        })
        .collect()
}

fn generate_giant_planets_for_star_in_band(
    star: &Star,
    name_prefix: &str,
    start_index: usize,
    min_planets: usize,
    max_planets: usize,
    min_semi_major_axis: f64,
    max_semi_major_axis: f64,
    min_eccentricity: f64,
    max_eccentricity: f64,
    min_moons: usize,
    max_moons: usize,
    moon_min_semi_major_axis: f64,
    moon_max_semi_major_axis: f64,
    moon_min_eccentricity: f64,
    moon_max_eccentricity: f64,
) -> Vec<Body> {
    let planet_count = random_count_in_range(min_planets, max_planets);
    if planet_count == 0 {
        return Vec::new();
    }

    let band_width = (max_semi_major_axis - min_semi_major_axis).max(1.0);
    let slot_width = band_width / planet_count as f64;

    (0..planet_count)
        .map(|index| {
            let slot_start = min_semi_major_axis + slot_width * index as f64;
            let slot_end = min_semi_major_axis + slot_width * (index + 1) as f64;
            let margin = (slot_width * 0.2).min(slot_width / 2.0);
            let position_ratio = if planet_count == 1 {
                0.5
            } else {
                index as f64 / (planet_count - 1) as f64
            };
            let body_type = if random_f64_in_range(0.0, 1.0) < (0.2 + position_ratio * 0.5) {
                BodyType::IceGiant
            } else {
                BodyType::GasGiant
            };

            let mut giant = generate_body_with_random_orbit(
                format!("{name_prefix} {}", start_index + index),
                star.id.clone(),
                star.parent_x,
                star.parent_y,
                body_type,
                (slot_start + margin).min(slot_end),
                (slot_end - margin).max(slot_start + margin),
                min_eccentricity,
                max_eccentricity,
            );

            giant.moons = generate_moons_for_body(
                &giant,
                &format!("{} Moon", giant.name),
                min_moons,
                max_moons,
                moon_min_semi_major_axis,
                moon_max_semi_major_axis,
                moon_min_eccentricity,
                moon_max_eccentricity,
            );

            giant
        })
        .collect()
}

fn generate_asteroids_for_star_in_band(
    star: &Star,
    name_prefix: &str,
    start_index: usize,
    min_bodies: usize,
    max_bodies: usize,
    min_semi_major_axis: f64,
    max_semi_major_axis: f64,
    min_eccentricity: f64,
    max_eccentricity: f64,
) -> Vec<Body> {
    let body_count = random_count_in_range(min_bodies, max_bodies);
    if body_count == 0 {
        return Vec::new();
    }

    let band_width = (max_semi_major_axis - min_semi_major_axis).max(1.0);
    let slot_width = band_width / body_count as f64;

    (0..body_count)
        .map(|index| {
            let slot_start = min_semi_major_axis + slot_width * index as f64;
            let slot_end = min_semi_major_axis + slot_width * (index + 1) as f64;
            let margin = (slot_width * 0.05).min(slot_width / 3.0);
            let position_ratio = if body_count == 1 {
                0.5
            } else {
                index as f64 / (body_count - 1) as f64
            };
            let roll = random_f64_in_range(0.0, 1.0);
            let body_type = if roll < (0.25 + position_ratio * 0.35) {
                BodyType::CTypeAsteroid
            } else if roll < 0.8 {
                BodyType::STypeAsteroid
            } else {
                BodyType::MTypeAsteroid
            };

            generate_body_with_random_orbit(
                format!("{name_prefix} {}", start_index + index),
                star.id.clone(),
                star.parent_x,
                star.parent_y,
                body_type,
                (slot_start + margin).min(slot_end),
                (slot_end - margin).max(slot_start + margin),
                min_eccentricity,
                max_eccentricity,
            )
        })
        .collect()
}

fn generate_small_bodies_for_star_in_band(
    star: &Star,
    name_prefix: &str,
    body_type: BodyType,
    start_index: usize,
    min_bodies: usize,
    max_bodies: usize,
    min_semi_major_axis: f64,
    max_semi_major_axis: f64,
    min_eccentricity: f64,
    max_eccentricity: f64,
) -> Vec<Body> {
    let body_count = random_count_in_range(min_bodies, max_bodies);
    if body_count == 0 {
        return Vec::new();
    }

    let band_width = (max_semi_major_axis - min_semi_major_axis).max(1.0);
    let slot_width = band_width / body_count as f64;

    (0..body_count)
    .map(|index| {
        let slot_start = min_semi_major_axis + slot_width * index as f64;
        let slot_end = min_semi_major_axis + slot_width * (index + 1) as f64;
        let margin = (slot_width * 0.05).min(slot_width / 3.0);

        generate_body_with_random_orbit(
            format!("{name_prefix} {}", start_index + index),
            star.id.clone(),
            star.parent_x,
            star.parent_y,
            body_type,
            (slot_start + margin).min(slot_end),
            (slot_end - margin).max(slot_start + margin),
            min_eccentricity,
            max_eccentricity,
        )
    })
    .collect()
}

pub fn generate_realistic_random_star_system() -> Star {
    let mut star = Star::new(0.0, 0.0, rand::rng().random_range(3..=8));
    let mut next_planet_index = 1;

    let mut inner_rocky = generate_planets_for_star_in_band(
        &star,
        "Inner",
        next_planet_index,
        2,
        4,
        0.25 * AU_KM,
        2.2 * AU_KM,
        0.0,
        0.12,
        0,
        2,
        40_000.0,
        600_000.0,
        0.0,
        0.08,
    );
    next_planet_index += inner_rocky.len();

    let mut gas_giants = generate_giant_planets_for_star_in_band(
        &star,
        "Giant",
        next_planet_index,
        1,
        3,
        3.5 * AU_KM,
        12.0 * AU_KM,
        0.01,
        0.18,
        4,
        10,
        120_000.0,
        3_500_000.0,
        0.0,
        0.1,
    );
    next_planet_index += gas_giants.len();

    let mut asteroid_belt = generate_asteroids_for_star_in_band(
        &star,
        "Asteroid",
        1,
        10,
        50,
        2.3 * AU_KM,
        3.4 * AU_KM,
        0.01,
        0.12,
    );

    let mut outer_ice = generate_planets_for_star_in_band(
        &star,
        "Outer",
        next_planet_index,
        0,
        2,
        15.0 * AU_KM,
        35.0 * AU_KM,
        0.02,
        0.22,
        1,
        6,
        80_000.0,
        1_500_000.0,
        0.0,
        0.12,
    );
    next_planet_index += outer_ice.len();

    let mut comets = generate_small_bodies_for_star_in_band(
        &star,
        "Comet",
        BodyType::Comet,
        1,
        4,
        50,
        25.0 * AU_KM,
        80.0 * AU_KM,
        0.35,
        0.85,
    );

    star.bodies.append(&mut inner_rocky);
    star.bodies.append(&mut asteroid_belt);
    star.bodies.append(&mut gas_giants);
    star.bodies.append(&mut outer_ice);
    star.bodies.append(&mut comets);
    star.bodies.sort_by(|a, b| {
        let a_axis = a.orbit.as_ref().map(|orbit| orbit.semi_major_axis).unwrap_or(0.0);
        let b_axis = b.orbit.as_ref().map(|orbit| orbit.semi_major_axis).unwrap_or(0.0);
        a_axis.total_cmp(&b_axis)
    });

    star
}

#[derive(Clone, Deserialize)]
struct OrbitDTO {
    id: String,
    name: String,
    semi_major_axis: f64,
    eccentricity: f64,
    arg_periapsis: f64,
    mean_motion: f64,
    satellites: Vec<OrbitDTO>,
}

/*
fn generate_solar_system_for_star(star: &mut Star){


    let horizon_texts_map: std::collections::HashMap<String, String> = std::fs::read_dir("src/horizon_data")
        .expect("Failed to read horizon_data directory") // Returns ReadDir
        .filter_map(|entry| entry.ok())
        .filter(|entry| entry.file_type().unwrap().is_file())
        .map(|entry| {
            let mut file = File::open(entry.path()).unwrap();
            let mut string_buffer = String::new();
            file.read_to_string(&mut string_buffer).unwrap();
            let name = entry.file_name().into_string().unwrap().replace(".txt", "");
            (name, string_buffer)
        })
        .collect();

    let body_info_file = std::fs::File::open("src/data/body_info.json").expect("Failed to open planet info data file");
    let body_info_vec: std::collections::HashMap<String, InfoDTO> = serde_json::from_reader::<_, Vec<InfoDTO>>(body_info_file).expect("Failed to parse planet info data file")
        .into_iter().map(|f| (f.name.clone(), f)).collect();

    let mut info_dict: HashMap<uuid::Uuid, InfoDTO> = std::collections::HashMap::new();
    
    let file = std::fs::File::open("src/data/horizon_offline_body_output.json").expect("Failed to open solar system data file");
    serde_json::from_reader::<_,Vec<OrbitDTO>>(file).expect("Failed to parse solar system data file")
        .into_iter()
        .for_each(|orbit_dto| {
            let mut body = Body::new(orbit_dto.name, star.parent_x, star.parent_y, BodyType::Planet);
            let semi_major_axis = orbit_dto.semi_major_axis;
            let eccentricity = orbit_dto.eccentricity;
            let arg_periapsis = orbit_dto.arg_periapsis;
            let mean_motion = orbit_dto.mean_motion;
            
            let mut orbit = Orbit::new(semi_major_axis, eccentricity, arg_periapsis, mean_motion);
            let period = orbit.get_orbital_period();
            let random_time = rand::random::<f64>() * period;

            orbit.progress_orbit_by_time(random_time);
            body.orbit = Some(orbit);
            let horizon_full_text = horizon_texts_map.get(&orbit_dto.id).cloned().unwrap_or("No horizon text found".into());
            let mut dict_entry = body_info_vec.get(&body.name).cloned().unwrap_or(InfoDTO::new(&body.name));
            dict_entry.full_horizon_text = horizon_full_text;

            info_dict.insert(body.uuid, dict_entry);

            for sat_orbit in orbit_dto.satellites {
                let mut satellite = Body::new(sat_orbit.name, body.parent_x, body.parent_y, BodyType::Moon);
                let semi_major_axis = sat_orbit.semi_major_axis;
                let eccentricity = sat_orbit.eccentricity;
                let arg_periapsis = sat_orbit.arg_periapsis;
                let mean_motion = sat_orbit.mean_motion;
                
                let mut moon_orbit = Orbit::new(semi_major_axis, eccentricity, arg_periapsis, mean_motion);
                let period = moon_orbit.get_orbital_period();
                let random_time = rand::random::<f64>() * period;

                moon_orbit.progress_orbit_by_time(random_time);

                satellite.orbit = Some(moon_orbit);

                let horizon_full_text = horizon_texts_map.get(&orbit_dto.id).cloned().unwrap_or("No horizon text found".into());
                let mut dict_entry = body_info_vec.get(&body.name).cloned().unwrap_or(InfoDTO::new(&body.name));
                dict_entry.full_horizon_text = horizon_full_text;

                info_dict.insert(body.uuid, dict_entry);
                body.moons.push(satellite);
            }

            star.bodies.push(body);

        });

        
    let mut fleet = Fleet{
        name: "Earth Fleet".to_string(),
        uuid: Uuid::new_v4(),
        x: 0.0,
        y: 1_000_000.0,
        target_position: None,
        target_object: None,
        members: vec![],
    };

    fleet.members.push(
        Ship{name:"Flagship".to_string(),speed:10_000.0, uuid: Uuid::new_v4(), parent_fleet: fleet.uuid }
    );

    star.fleets.push(fleet);

    let _ = BODY_INFO_DICT.set(info_dict);
}
*/
