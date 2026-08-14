// trail.gdshader
shader_type spatial;
render_mode unshaded, cull_disabled, blend_add, depth_draw_never, shadows_disabled;

uniform vec4 trail_color : source_color = vec4(1.0, 0.85, 0.4, 1.0);
uniform float fade_power : hint_range(0.1, 8.0) = 2.0;
instance uniform float speed_factor : hint_range(0.0, 1.0) = 1.0;

void fragment() {
	float fade = pow(1.0 - UV.y, fade_power);
	ALBEDO = trail_color.rgb;
	ALPHA = trail_color.a * fade * speed_factor;
}