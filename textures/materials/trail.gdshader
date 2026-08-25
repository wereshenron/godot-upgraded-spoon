shader_type spatial;
//render_mode unshaded, blend_add, depth_draw_never, cull_disabled;
render_mode unshaded, cull_disabled, depth_draw_never;

uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform vec4 tracer_color : source_color = vec4(1.0);
uniform float fade_width : hint_range(0.01, 1.0) = 0.15;

varying float v_pos;

void vertex() {
	v_pos = UV.x; // 0 at start vertex, 1 at end vertex
}

void fragment() {
	float alpha = smoothstep(progress - fade_width, progress, v_pos)
	            * (1.0 - smoothstep(progress, progress + fade_width, v_pos));
	ALBEDO = tracer_color.rgb;
	//ALPHA = tracer_color.a * alpha;
	ALPHA = alpha;
}