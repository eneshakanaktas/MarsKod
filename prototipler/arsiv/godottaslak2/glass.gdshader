// Buzlu cam panel: arkasındaki sahneyi bulanık gösterir, köşeleri yuvarlak, üst kenarı ışıklı.
shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;
uniform vec2 rect_size = vec2(100.0, 100.0);
uniform float radius = 36.0;
uniform vec4 tint : source_color = vec4(0.05, 0.07, 0.14, 0.72);
uniform vec4 border : source_color = vec4(0.55, 0.85, 1.0, 0.35);
uniform float blur = 3.5;

float sd_round_rect(vec2 p, vec2 b, float r) {
	vec2 q = abs(p) - b + r;
	return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
}

void fragment() {
	vec2 p = UV * rect_size - rect_size * 0.5;
	float d = sd_round_rect(p, rect_size * 0.5, radius);
	float alpha = 1.0 - smoothstep(-1.0, 0.5, d);
	vec3 bg = textureLod(screen_tex, SCREEN_UV, blur).rgb;
	vec3 col = mix(bg, tint.rgb, tint.a);
	// üstten aşağı hafif ışık
	col += vec3(0.06, 0.08, 0.12) * (1.0 - UV.y) * 0.6;
	// kenar çizgisi: üst kenarda daha parlak
	float rim = 1.0 - smoothstep(0.0, 2.2, abs(d + 1.2));
	float top = mix(1.0, 0.35, UV.y);
	col = mix(col, border.rgb, rim * border.a * top);
	COLOR = vec4(col, alpha);
}
