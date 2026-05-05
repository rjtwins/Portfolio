shader_type canvas_item;
uniform vec2 global_position;
uniform vec2 screen_size;
uniform float force;
uniform float size;
uniform float thickness;

void fragment(){
	vec2 center = global_position;
	float ratio = TEXTURE_PIXEL_SIZE.x / TEXTURE_PIXEL_SIZE.y;
	center.x = center.x / screen_size.x;
	center.x = (center.x - 0.5) / ratio + 0.5;
	center.y = (screen_size.y - center.y) / screen_size.y;
	float mask = (1.0 - smoothstep(size-0.1, size, length(UV - center))) * smoothstep(size-thickness-0.1, size-thickness, length(UV - center));
	vec2 disp = normalize(UV - center) * force * mask;
	COLOR = texture(TEXTURE, UV - disp);
}