// Kod bölgesi: gezginin yere yansıttığı hologram ızgara. Çizgiler ışıktır; zemin görünmeye devam eder.
shader_type spatial;
render_mode unshaded, blend_add, depth_draw_never, cull_disabled, shadows_disabled;

uniform float cells = 6.0;
uniform vec3 color : source_color = vec3(0.3, 0.85, 1.0);

varying vec3 local_pos;

void vertex() {
	local_pos = VERTEX;
}

void fragment() {
	vec2 g = local_pos.xz + cells * 0.5;
	vec2 f = fract(g);
	vec2 c = min(f, 1.0 - f);
	float edge = min(c.x, c.y);
	float line = 1.0 - smoothstep(0.0, 0.02, edge);
	// her karenin köşesinde köşebent işareti
	float bracket = max(step(c.x, 0.022) * step(c.y, 0.17), step(c.y, 0.022) * step(c.x, 0.17));
	// bölgenin dış çerçevesi
	float d_edge = min(min(g.x, cells - g.x), min(g.y, cells - g.y));
	float border = 1.0 - smoothstep(0.0, 0.045, d_edge);
	float border_glow = (1.0 - smoothstep(0.0, 0.5, d_edge)) * 0.12;
	// merkezden dışa yayılan tarama dalgası
	float r = length(g - cells * 0.5);
	float wave = 0.5 + 0.5 * sin(TIME * 1.8 - r * 1.4);
	float scan = smoothstep(0.35, 0.0, abs(fract(TIME * 0.18) * cells * 0.8 - r)) * 0.35;
	float a = line * 0.28 * wave + bracket * 0.75 + border * 1.4 + border_glow + scan * (line + 0.15);
	ALBEDO = color * a;
}
