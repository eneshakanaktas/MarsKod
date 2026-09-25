// Doğal buz: yarı saydam, arkasını hafifçe kırar, üstü tozlu ve buğulu; sadece kenarlarda hafif soğuk ışıltı.
shader_type spatial;
render_mode blend_mix, cull_back, depth_draw_always;

uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;
uniform vec3 ice_color : source_color = vec3(0.7, 0.88, 0.96);
uniform vec3 dust_color : source_color = vec3(0.62, 0.42, 0.33);
uniform float glow = 1.0;

varying vec3 world_pos;
varying vec3 world_normal;

void vertex() {
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);
}

float hash(vec3 p) {
	p = fract(p * 0.3183099 + 0.1);
	p *= 17.0;
	return fract(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float noise(vec3 p) {
	vec3 i = floor(p);
	vec3 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	return mix(
		mix(mix(hash(i), hash(i + vec3(1, 0, 0)), f.x), mix(hash(i + vec3(0, 1, 0)), hash(i + vec3(1, 1, 0)), f.x), f.y),
		mix(mix(hash(i + vec3(0, 0, 1)), hash(i + vec3(1, 0, 1)), f.x), mix(hash(i + vec3(0, 1, 1)), hash(i + vec3(1, 1, 1)), f.x), f.y),
		f.z);
}

void fragment() {
	float facing = clamp(dot(NORMAL, VIEW), 0.0, 1.0);
	float fresnel = pow(1.0 - facing, 3.0);
	vec3 behind = textureLod(screen_tex, SCREEN_UV + NORMAL.xy * 0.02, 2.0).rgb;
	float cloudy = noise(world_pos * 14.0) * 0.6 + noise(world_pos * 40.0) * 0.4;
	vec3 body = mix(behind * ice_color, ice_color * 0.75, 0.35 + cloudy * 0.3);
	// üst yüzeylerde ve alt kenarda toz birikintisi
	float dust = smoothstep(0.55, 0.95, world_normal.y) * smoothstep(0.35, 0.7, noise(world_pos * 9.0));
	dust = max(dust, smoothstep(0.12, 0.0, world_pos.y) * 0.8);
	ALBEDO = mix(body, dust_color, dust * 0.85);
	ROUGHNESS = mix(0.12 + cloudy * 0.2, 0.85, dust);
	SPECULAR = 0.7;
	EMISSION = ice_color * (0.03 + fresnel * 0.35) * glow * (1.0 - dust);
}
