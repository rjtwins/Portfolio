shader_type spatial;

uniform sampler2D terrain_grass;
uniform sampler2D terrain_rock;

uniform float min_grass_height = -0.30;
uniform float max_rock_height = 1.6;
varying float vertex_y;
uniform vec2 uvscale_rock = vec2(5);
uniform vec2 uvscale_grass = vec2(5);

void vertex() {
	vertex_y = VERTEX.y;
}

void fragment() {
	float vert_weight = vertex_y;
	
	vec3 grass = texture(terrain_grass, UV*uvscale_grass).rgb;
	vec3 rock = texture(terrain_rock, UV*uvscale_rock).rgb;
	
	float weight = smoothstep(min_grass_height, max_rock_height, vert_weight);
	vec3 final_color = mix(grass, rock, weight);
	
	ALBEDO = final_color;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
