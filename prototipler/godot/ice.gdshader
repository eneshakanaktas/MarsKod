// Buz kristali: arkasındaki görüntüyü hafifçe kırar, kenarlarda ışıldar, içten soğuk mavi parlar.
shader_type spatial;
render_mode blend_mix, cull_back, depth_draw_always;

uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;
uniform vec3 ice_color : source_color = vec3(0.62, 0.9, 1.0);
uniform float glow = 1.0;

void fragment() {
	float facing = clamp(dot(NORMAL, VIEW), 0.0, 1.0);
	float fresnel = pow(1.0 - facing, 3.0);
	vec2 offset = NORMAL.xy * 0.035;
	vec3 behind = textureLod(screen_tex, SCREEN_UV + offset, 1.5).rgb;
	vec3 body = mix(behind * ice_color * 1.25, vec3(0.88, 0.97, 1.0), 0.28 + fresnel * 0.55);
	ALBEDO = body;
	ROUGHNESS = 0.06;
	SPECULAR = 0.9;
	EMISSION = ice_color * (0.22 + fresnel * 1.6) * glow;
}
