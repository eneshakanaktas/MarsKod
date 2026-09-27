// Mars arazisi: ekran kartında (GPU) hesaplanan doku. Kumullar, ışık-gölge, ince taneler ve
// kareleri gösteren hafif çizgiler. Işık sol üstten gelir.

import { Skia } from '@shopify/react-native-skia';

const SKSL = `
uniform float2 size;
uniform float cell;

float hash(float2 p) {
  p = fract(p * float2(123.34, 456.21));
  p += dot(p, p + 45.32);
  return fract(p.x * p.y);
}

float noise(float2 p) {
  float2 i = floor(p);
  float2 f = fract(p);
  float a = hash(i);
  float b = hash(i + float2(1.0, 0.0));
  float c = hash(i + float2(0.0, 1.0));
  float d = hash(i + float2(1.0, 1.0));
  float2 u = f * f * (3.0 - 2.0 * f);
  return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm(float2 p) {
  float v = 0.0;
  float amp = 0.5;
  for (int i = 0; i < 5; i++) {
    v += amp * noise(p);
    p = p * 2.03 + float2(17.0, 9.0);
    amp *= 0.5;
  }
  return v;
}

float height(float2 xy) {
  float2 p = xy / cell;
  // uzun kumul dalgaları + düzensiz tepeler
  float dunes = sin(p.x * 1.3 + p.y * 0.7 + fbm(p * 0.6) * 3.0) * 0.5 + 0.5;
  return fbm(p * 0.8) * 0.75 + dunes * 0.25;
}

half4 main(float2 xy) {
  float h = height(xy);
  float hx = height(xy + float2(2.0, 0.0));
  float hy = height(xy + float2(0.0, 2.0));
  // eğime göre ışık: sol üste bakan yamaçlar aydınlık
  float slope = (hx - h) + (hy - h);
  float light = clamp(0.5 - slope * 9.0, 0.0, 1.0);

  float3 deep = float3(0.42, 0.14, 0.07);
  float3 mid = float3(0.72, 0.30, 0.15);
  float3 warm = float3(0.86, 0.45, 0.24);
  float3 dust = float3(0.95, 0.66, 0.46);

  float3 col = mix(deep, mid, smoothstep(0.2, 0.62, h));
  col = mix(col, warm, smoothstep(0.55, 0.85, h) * 0.8);
  col = mix(col * 0.78, col * 1.15, light);
  col = mix(col, dust, smoothstep(0.62, 0.95, light) * 0.25);

  // ince tane
  col *= 0.93 + 0.14 * noise(xy * 0.45);

  // kare çizgileri: oyuncunun kareleri sayabilmesi için, ama göze batmadan
  float2 g = fract(xy / cell);
  float edge = min(min(g.x, 1.0 - g.x), min(g.y, 1.0 - g.y)) * cell;
  float line = 1.0 - smoothstep(0.0, 1.4, edge);
  col = mix(col, col * 0.62, line * 0.45);
  // karelerin köşelerinde küçük işaret noktaları
  float2 c = min(g, 1.0 - g) * cell;
  float corner = 1.0 - smoothstep(1.5, 3.2, length(c));
  col = mix(col, float3(1.0, 0.85, 0.7), corner * 0.25);

  // kenarlara doğru hafif kararma
  float2 uv = xy / size - 0.5;
  col *= 1.0 - dot(uv, uv) * 0.55;

  return half4(half3(col), 1.0);
}
`;

export const terrainShader = Skia.RuntimeEffect.Make(SKSL);
