// Mars arazisi: gerçek fotoğraf dokuları (ambientCG, CC0) Mars kırmızısına boyanır.
// Tekrar deseni belli olmasın diye iki ölçek karıştırılır; dik yamaçlar kendiliğinden kaya dokusuna geçer.
shader_type spatial;

uniform sampler2D ground_albedo : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D ground_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D ground_rough : hint_roughness_r, filter_linear_mipmap, repeat_enable;
uniform sampler2D dune_albedo : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D dune_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_albedo : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;

uniform vec3 mars_tint : source_color = vec3(1.0, 0.52, 0.34);
uniform float ground_scale = 0.42;
uniform float dune_scale = 0.09;
uniform float rock_scale = 0.16;

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

vec3 colorize(vec3 c) {
	// dokunun ışık-gölge bilgisini koru, rengini Mars pasına çevir
	float lum = dot(c, vec3(0.3, 0.59, 0.11));
	return mix(c * mars_tint, vec3(lum) * mars_tint * 1.35, 0.55);
}

void fragment() {
	vec2 uv_a = world_pos.xz * ground_scale;
	vec2 uv_b = world_pos.xz * ground_scale * 0.27 + vec2(0.37, 0.11);
	vec3 ground = mix(texture(ground_albedo, uv_a).rgb, texture(ground_albedo, uv_b).rgb, 0.4);
	vec3 dune = texture(dune_albedo, world_pos.xz * dune_scale).rgb;

	// büyük ölçekli leke: bazı yerler ince kum, bazı yerler çakıllı
	float macro = vnoise(world_pos.xz * 0.07) * 0.7 + vnoise(world_pos.xz * 0.23) * 0.3;
	float dune_w = smoothstep(0.45, 0.75, macro);
	vec3 col = mix(ground, dune, dune_w);

	// eğim: dik yerlerde katmanlı kaya
	float slope = 1.0 - clamp(world_normal.y, 0.0, 1.0);
	float rock_w = smoothstep(0.22, 0.45, slope);
	vec3 rx = texture(rock_albedo, world_pos.zy * rock_scale).rgb;
	vec3 rz = texture(rock_albedo, world_pos.xy * rock_scale).rgb;
	vec3 rock = mix(rz, rx, abs(world_normal.x));
	col = mix(col, rock, rock_w);

	// uzaklaştıkça hafif koyulaşan ve renk değiştiren leke (gerçek fotoğraflardaki gibi)
	col *= 0.85 + 0.3 * vnoise(world_pos.xz * 0.03);

	ALBEDO = colorize(col);
	vec3 n_ground = mix(texture(ground_normal, uv_a).rgb, texture(dune_normal, world_pos.xz * dune_scale).rgb, dune_w);
	NORMAL_MAP = mix(n_ground, vec3(0.5, 0.5, 1.0), rock_w * 0.6);
	NORMAL_MAP_DEPTH = 1.4;
	ROUGHNESS = mix(texture(ground_rough, uv_a).r, 0.92, rock_w);
	SPECULAR = 0.3;
}
