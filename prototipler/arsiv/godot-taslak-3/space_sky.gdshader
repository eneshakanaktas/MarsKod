// Uzay gökyüzü: koyu gradyan, hafif nebula ve yanıp sönen yıldızlar.
shader_type sky;

uniform vec3 top_color : source_color = vec3(0.02, 0.02, 0.07);
uniform vec3 horizon_color : source_color = vec3(0.13, 0.05, 0.18);
uniform vec3 nebula_color : source_color = vec3(0.45, 0.15, 0.45);

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

void sky() {
	vec3 d = normalize(EYEDIR);
	vec3 col = mix(horizon_color, top_color, smoothstep(-0.3, 0.8, d.y));

	// nebula: birkaç kat gürültü
	float n = noise(d * 3.0) * 0.6 + noise(d * 7.0) * 0.3 + noise(d * 15.0) * 0.1;
	col += nebula_color * pow(n, 3.0) * 0.55;

	// yıldızlar: küçük, keskin, yanıp sönen
	vec3 cell = floor(d * 400.0);
	float h = hash(cell);
	vec3 local = fract(d * 400.0) - 0.5;
	float dist = length(local);
	float star = step(0.9975, h) * smoothstep(0.35, 0.0, dist);
	float twinkle = 0.55 + 0.45 * sin(TIME * (1.5 + h * 3.0) + h * 60.0);
	col += vec3(0.9, 0.95, 1.0) * star * twinkle * 2.2;

	COLOR = col;
}
