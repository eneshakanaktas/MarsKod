// Mars gökyüzü, altın saat: ufukta sıcak toz rengi, yukarı doğru koyulaşan mor-kahve.
// Gerçek Mars'ta olduğu gibi güneşin çevresi mavimsi parlar.
shader_type sky;

uniform vec3 zenith : source_color = vec3(0.16, 0.11, 0.16);
uniform vec3 mid : source_color = vec3(0.55, 0.30, 0.22);
uniform vec3 horizon : source_color = vec3(0.93, 0.62, 0.40);
uniform vec3 below : source_color = vec3(0.42, 0.19, 0.10);
uniform vec3 sun_halo : source_color = vec3(0.55, 0.74, 1.0);

void sky() {
	vec3 d = normalize(EYEDIR);
	float t = d.y;
	vec3 col;
	if (t > 0.0) {
		col = mix(horizon, mid, smoothstep(0.0, 0.18, t));
		col = mix(col, zenith, smoothstep(0.18, 0.75, t));
	} else {
		col = mix(horizon, below, smoothstep(0.0, 0.25, -t));
	}
	// güneş: mavimsi hale + sıcak parıltı + parlak disk (bloom yakalar)
	float s = max(dot(d, LIGHT0_DIRECTION), 0.0);
	col += sun_halo * pow(s, 40.0) * 0.55;
	col += vec3(1.0, 0.72, 0.48) * pow(s, 6.0) * 0.22;
	col += vec3(1.0, 0.96, 0.9) * pow(s, 1200.0) * 14.0;
	COLOR = col;
}
