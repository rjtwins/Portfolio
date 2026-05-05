shader_type canvas_item;

// ======== PARAMETERS YOU SET FROM C# ========
uniform float world_width;
uniform float world_height;
uniform vec2 camera_position; // XZ world pos
uniform float frequency = 0.01;
uniform float terrain_max_height = 5.0;

// ============================================
// OpenSimplex2 noise for 2D (matches FastNoiseLite fairly closely)
// ============================================

vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec2 mod289(vec2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }

vec3 perm(vec3 x) { return mod289(((x * 34.0) + 1.0) * x); }

float openSimplex2(vec2 v) {
    const float C = 0.211324865405187; // (3.0-sqrt(3.0))/6.0
    vec2 i = floor(v + dot(v, vec2(0.5, 0.5)));
    vec2 x0 = v - i + dot(i, vec2(C, C));

    vec2 i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec2 x1 = x0 - i1 + vec2(C, C);
    vec2 x2 = x0 - 1.0 + 2.0 * vec2(C, C);

    i = mod289(i);

    vec3 p = perm(perm(vec3(i.y + vec2(0.0, i1.y), i.y + 1.0)) +
                  vec3(i.x + vec2(0.0, i1.x), i.x + 1.0));

    vec3 m = max(0.5 - vec3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
    m = m*m;
    m = m*m;

    vec3 x = 2.0 * fract(p * 0.0243902439) - 1.0;
    vec3 h = abs(x) - 0.5;
    vec3 ox = floor(x + 0.5);
    vec3 a0 = x - ox;

    m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);

    vec3 g;
    g.x  = a0.x * x0.x + h.x * x0.y;
    g.y  = a0.y * x1.x + h.y * x1.y;
    g.z  = a0.z * x2.x + h.z * x2.y;

    return dot(m, g) * 1.0;
}


// ============================================
// MAIN FRAGMENT
// ============================================

void fragment() {
    // Convert minimap UV → world coordinates
    vec2 world_pos;
    world_pos.x = camera_position.x + (UV.x - 0.5) * world_width;
    world_pos.y = camera_position.y + (UV.y - 0.5) * world_height;

    // Sample noise
    float n = openSimplex2(world_pos * frequency);

    // Match your terrain processing
    if (n < 0.0) {
        n *= 0.3;
    }
    n *= terrain_max_height;

    float v = clamp(n / terrain_max_height, 0.0, 1.0);

    COLOR = vec4(v, v, v, 1.0);
}