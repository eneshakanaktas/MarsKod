// Sinema görünümü: renk ayarı (gölgeler soğuk, ışıklar sıcak), kenar karartma, film greni,
// hafif mercek renk kayması ve giriş sahnesindeki siyah film şeritleri.
shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;
uniform float grain = 0.045;
uniform float vignette = 0.42;
uniform float aberration = 0.0018;
uniform float bars = 0.0;
uniform float fade = 0.0;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 dir = uv - 0.5;
	vec3 col;
	col.r = texture(screen_tex, uv - dir * aberration).r;
	col.g = texture(screen_tex, uv).g;
	col.b = texture(screen_tex, uv + dir * aberration).b;

	float lum = dot(col, vec3(0.299, 0.587, 0.114));
	col = mix(col * vec3(0.9, 0.98, 1.1), col, smoothstep(0.0, 0.45, lum));
	col = mix(col, col * vec3(1.06, 1.0, 0.92), smoothstep(0.45, 1.0, lum));

	float v = length(dir * vec2(1.0, 0.75)) * 1.45;
	col *= 1.0 - vignette * smoothstep(0.35, 1.1, v);

	float n = hash(uv * vec2(1080.0, 2400.0) + fract(TIME * 7.13) * 91.0) - 0.5;
	col += n * grain;

	float bar = bars * 0.11;
	col *= step(bar, uv.y) * step(uv.y, 1.0 - bar);
	col = mix(col, vec3(0.0), fade);
	COLOR = vec4(col, 1.0);
}
