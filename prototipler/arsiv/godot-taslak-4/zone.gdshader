// Kod bölgesi zemini: kare kare hafif ton farkı, ince toz dokusu ve nabız gibi atan ışık çizgileri.
shader_type spatial;

uniform vec3 base_a : source_color = vec3(0.64, 0.30, 0.18);
uniform vec3 base_b : source_color = vec3(0.56, 0.25, 0.15);
uniform vec3 line_color : source_color = vec3(0.30, 0.86, 1.0);
uniform float cells = 6.0;

varying vec3 local_pos;

void vertex() {
	local_pos = VERTEX;
}

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

void fragment() {
	vec2 g = local_pos.xz + cells * 0.5;
	vec2 cell = floor(g);
	vec2 f = fract(g);
	vec3 col = mix(base_a, base_b, hash(cell) * 0.7);
	// ince toz tanesi ve hafif leke
	col *= 0.9 + 0.1 * hash(floor(g * 32.0));
	col *= 0.94 + 0.06 * sin(g.x * 1.7 + g.y * 2.3);
	// kareleri ayıran çizgiler
	float edge = min(min(f.x, 1.0 - f.x), min(f.y, 1.0 - f.y));
	float line = 1.0 - smoothstep(0.0, 0.028, edge);
	// merkezden dışa yayılan ışık dalgası
	float wave = 0.55 + 0.45 * sin(TIME * 1.6 - length(g - cells * 0.5) * 1.3);
	// kenar çerçevesi: bölgenin dışında kalan ince bant daha parlak
	float border = 1.0 - smoothstep(0.0, 0.05, min(min(g.x, cells - g.x), min(g.y, cells - g.y)));
	ALBEDO = mix(col, col * 0.55, line);
	EMISSION = line_color * (line * 0.55 * wave + border * 1.4);
	ROUGHNESS = 0.92;
}
