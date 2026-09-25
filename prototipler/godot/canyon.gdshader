// Kanyon arazisi: taban kum ve çakıl, duvarlar katman katman tortul kaya (Mars'taki katmanlı kayalıklar gibi).
// Dokular gerçek fotoğraflardır (ambientCG, CC0); renkleri Mars pasına çevrilir.
shader_type spatial;

uniform sampler2D ground_albedo : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D ground_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D ground_rough : hint_roughness_r, filter_linear_mipmap, repeat_enable;
uniform sampler2D dune_albedo : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D dune_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_albedo : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;

uniform vec3 mars_tint : source_color = vec3(1.0, 0.5, 0.32);
uniform float ground_scale = 0.45;
uniform float dune_scale = 0.08;
uniform float rock_scale = 0.12;

varying vec3 world_pos;
varying vec3 world_normal;

void vertex() {
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);
}

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash(i), hash(i + vec2(1, 0)), f.x), mix(hash(i + vec2(0, 1)), hash(i + vec2(1, 1)), f.x), f.y);
}

vec3 colorize(vec3 c, vec3 tint) {
	float lum = dot(c, vec3(0.3, 0.59, 0.11));
	return mix(c * tint, vec3(lum) * tint * 1.35, 0.55);
}

void fragment() {
	vec2 uv_a = world_pos.xz * ground_scale;
	vec2 uv_b = world_pos.xz * ground_scale * 0.23 + vec2(0.37, 0.11);
	vec3 ground = mix(texture(ground_albedo, uv_a).rgb, texture(ground_albedo, uv_b).rgb, 0.45);
	vec3 dune = texture(dune_albedo, world_pos.xz * dune_scale).rgb;
	float macro = vnoise(world_pos.xz * 0.06) * 0.7 + vnoise(world_pos.xz * 0.21) * 0.3;
	float dune_w = smoothstep(0.42, 0.72, macro);
	vec3 floor_col = colorize(mix(ground, dune, dune_w), mars_tint);

	// duvarlar: iki yönden izdüşümlü kaya dokusu + yüksekliğe göre renk katmanları
	float slope = 1.0 - clamp(world_normal.y, 0.0, 1.0);
	float rock_w = smoothstep(0.2, 0.42, slope);
	vec3 rx = texture(rock_albedo, world_pos.zy * rock_scale).rgb;
	vec3 rz = texture(rock_albedo, world_pos.xy * rock_scale).rgb;
	vec3 rock = mix(rz, rx, abs(world_normal.x));
	float wob = vnoise(world_pos.xz * 0.05) * 1.6 + vnoise(world_pos.xz * 0.4) * 0.25;
	float band = world_pos.y * 0.55 + wob;
	float layer = fract(band);
	float id = floor(band);
	vec3 strata_a = vec3(1.0, 0.5, 0.3);
	vec3 strata_b = vec3(0.82, 0.55, 0.42);
	vec3 strata_c = vec3(0.62, 0.3, 0.2);
	vec3 strata = mix(strata_a, strata_b, step(0.5, hash(vec2(id, 3.0))));
	strata = mix(strata, strata_c, step(0.72, hash(vec2(id, 9.0))));
	strata *= 0.8 + 0.25 * smoothstep(0.0, 0.12, layer) * smoothstep(1.0, 0.85, layer);
	vec3 rock_col = colorize(rock, strata);
	vec3 col = mix(floor_col, rock_col, rock_w);

	col *= 0.82 + 0.3 * vnoise(world_pos.xz * 0.025);
	ALBEDO = col;
	vec3 n_ground = mix(texture(ground_normal, uv_a).rgb, texture(dune_normal, world_pos.xz * dune_scale).rgb, dune_w);
	vec3 n_rock = texture(rock_normal, mix(world_pos.xy, world_pos.zy, abs(world_normal.x)) * rock_scale).rgb;
	NORMAL_MAP = mix(n_ground, n_rock, rock_w);
	NORMAL_MAP_DEPTH = 1.3;
	ROUGHNESS = mix(texture(ground_rough, uv_a).r, 0.9, rock_w);
	SPECULAR = 0.28;
}
