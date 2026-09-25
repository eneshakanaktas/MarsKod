// Toz hortumu: Mars'ta gerçekten görülen dönen toz sütunu. Huni biçimli şeffaf bir yüzeyde kayan, dönen toz dokusu.
shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_never, cull_disabled, shadows_disabled;

uniform vec3 dust : source_color = vec3(0.66, 0.44, 0.32);
uniform float strength = 0.4;
uniform float speed = 1.0;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash(i), hash(i + vec2(1, 0)), f.x), mix(hash(i + vec2(0, 1)), hash(i + vec2(1, 1)), f.x), f.y);
}

void fragment() {
	float t = TIME * speed;
	vec2 p = vec2(UV.x * 10.0 - t * 3.0 + UV.y * 4.0, UV.y * 6.0 - t * 0.8);
	// yatay dikişte kesinti olmasın diye x yönünde tekrarlanan gürültü
	float n = mix(vnoise(p), vnoise(p - vec2(10.0, 0.0)), UV.x);
	n = n * 0.65 + vnoise(p * 2.3) * 0.35;
	float facing = abs(dot(NORMAL, VIEW));
	float ends = smoothstep(0.0, 0.25, UV.y) * smoothstep(1.0, 0.7, UV.y);
	ALBEDO = dust * (0.85 + n * 0.3);
	ALPHA = clamp(smoothstep(0.35, 0.85, n) * pow(facing, 1.4) * ends * strength, 0.0, 1.0);
}
