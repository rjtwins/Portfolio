shader_type canvas_item;

uniform vec2 vision_world_pos; // world position of player
uniform float vision_radius;   // radius in pixels
uniform float softness : hint_range(0.0, 50.0) = 5.0; // optional smooth edges

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
    vec4 col = vec4(0,0,0,1);
	
    // Compute texture size for 2D shaders
    vec2 tex_size = 1.0 / TEXTURE_PIXEL_SIZE;

    // Convert UV → pixel coordinates
    vec2 pixel_pos = UV * tex_size;

    // Distance from center
    float dist = distance(pixel_pos, vision_world_pos);

    // Soft cutout edge
    float edge = smoothstep(vision_radius - softness, vision_radius, dist);
    col.a *= edge;

    COLOR = col;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
