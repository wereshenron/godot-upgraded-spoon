shader_type spatial;
render_mode blend_mix, cull_front, unshaded;

uniform vec4 color : source_color = vec4(1, 0, 0, 1);
uniform float base_size : hint_range(1.0, 1.5, 0.01) = 1.05;
uniform float pulse_amount : hint_range(0.0, 0.1, 0.001) = 0.02;
uniform float pulse_speed : hint_range(0.1, 10.0, 0.1) = 2.0;
uniform float blend : hint_range(0.0, 1.0, 0.01) = 1.0;

void vertex() {
    float eased = smoothstep(0.0, 1.0, blend);
    float pulse = sin(TIME * pulse_speed) * pulse_amount;
    VERTEX *= mix(1.0, base_size + pulse, eased);
}

void fragment() {
    ALBEDO = color.rgb;
    ALPHA = COLOR.a * smoothstep(0.0, 1.0, blend);
}