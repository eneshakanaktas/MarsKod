// Mars alacakaranlığı: gerçek Mars'ta gün batımı mavidir; güneşin çevresi mavi, uzak ufuk tozlu karamel.
// Yukarıda yıldızlar ve Samanyolu belirir; Dünya parlak mavi bir nokta olarak görünür; kayan yıldızlar geçer.
shader_type sky;

uniform vec3 zenith : source_color = vec3(0.015, 0.02, 0.05);
uniform vec3 horizon_sun : source_color = vec3(0.36, 0.55, 0.85);
uniform vec3 horizon_far : source_color = vec3(0.52, 0.30, 0.19);
uniform vec3 below : source_color = vec3(0.10, 0.05, 0.035);
uniform vec3 earth_dir = vec3(-0.62, 0.22, -0.75);

float hash(vec3 p) {
	p = fract(p * 0.3183099 + 0.1);
	p *= 17.0;
	return fract(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float hash1(float n) {
	return fract(sin(n) * 43758.5453);
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

float stars(vec3 d, float density, float size) {
	vec3 cell = floor(d * density);
	float h = hash(cell);
	vec3 local = fract(d * density) - 0.5;
	float s = step(0.9965, h) * smoothstep(size, 0.0, length(local));
	float tw = 0.6 + 0.4 * sin(TIME * (1.2 + h * 3.0) + h * 70.0);
	return s * tw * (0.5 + h * 1.5);
}

void sky() {
	vec3 d = normalize(EYEDIR);
	vec3 sun = normalize(LIGHT0_DIRECTION);
	float h = d.y;
	float toward = smoothstep(-0.4, 1.0, dot(normalize(d.xz + 1e-4), normalize(sun.xz + 1e-4)));
	vec3 horizon = mix(horizon_far, horizon_sun, toward * toward);
	vec3 col = mix(horizon, zenith, smoothstep(0.0, 0.5, h));
	if (h < 0.0) {
		col = mix(horizon * 0.55, below, smoothstep(0.0, 0.18, -h));
	}
	float sd = max(dot(d, sun), 0.0);
	col += vec3(0.3, 0.52, 0.95) * pow(sd, 18.0) * 0.8;
	col += vec3(0.95, 0.9, 1.0) * pow(sd, 400.0) * 1.5;
	col += vec3(1.0, 0.97, 0.92) * pow(sd, 2500.0) * 30.0;

	// yıldızlar ve Samanyolu: yükseklere ve güneşten uzağa doğru belirginleşir
	float vis = smoothstep(0.04, 0.35, h) * (1.0 - smoothstep(0.55, 0.95, sd));
	vec3 mw_n = normalize(vec3(0.35, 0.4, 0.85));
	float band = exp(-pow(dot(d, mw_n) * 3.2, 2.0));
	float clouds = noise(d * 5.0) * 0.55 + noise(d * 12.0) * 0.3 + noise(d * 28.0) * 0.15;
	float lanes = smoothstep(0.45, 0.7, noise(d * 9.0 + 3.0));
	col += vec3(0.72, 0.68, 0.86) * band * pow(clouds, 1.6) * (1.0 - lanes * 0.6) * 0.45 * vis;
	col += vec3(0.9, 0.95, 1.0) * (stars(d, 520.0, 0.32) + stars(d, 900.0, 0.3) * (0.4 + band * 1.5)) * 1.6 * vis;

	// Dünya: güneşin yakınında parlak mavi bir "akşam yıldızı"
	float e = max(dot(d, normalize(earth_dir)), 0.0);
	col += vec3(0.4, 0.65, 1.0) * (pow(e, 60000.0) * 8.0 + pow(e, 3000.0) * 0.6) * smoothstep(0.0, 0.1, h);

	// kayan yıldızlar
	for (int k = 0; k < 3; k++) {
		float period = 3.6 + float(k) * 1.9;
		float tt = TIME + float(k) * 2.3;
		float t = mod(tt, period);
		float seed = floor(tt / period) * 7.0 + float(k) * 13.0;
		if (t < 0.8) {
			vec3 a = normalize(vec3(hash1(seed) * 2.0 - 1.0, 0.35 + hash1(seed + 1.0) * 0.45, -0.5 - hash1(seed + 2.0) * 0.7));
			vec3 m = normalize(cross(a, normalize(vec3(hash1(seed + 3.0) - 0.5, 0.8, hash1(seed + 5.0) - 0.5))));
			float prog = t / 0.8;
			vec3 head = normalize(a + m * prog * 0.45);
			vec3 tail = normalize(a + m * max(prog * 0.45 - 0.16, 0.0));
			vec3 pa = d - tail;
			vec3 ba = head - tail;
			float hh = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
			float dist = length(pa - ba * hh);
			float streak = smoothstep(0.0022, 0.0, dist) * hh * sin(prog * 3.1416);
			col += vec3(0.85, 0.93, 1.0) * streak * 4.0 * smoothstep(0.02, 0.2, h);
		}
	}
	COLOR = col;
}
