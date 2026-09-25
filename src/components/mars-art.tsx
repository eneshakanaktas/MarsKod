// Mars dünyasının çizimleri (Skia). Her fonksiyon bir karenin sol üst köşesi (x, y) ve
// kare boyutu (s) alır. Işık sol üstten gelir: üst yüzler açık, ön yüzler koyu, gölgeler sağ alta.

import {
  BlurMask,
  Circle,
  Group,
  Line,
  LinearGradient,
  Oval,
  Path,
  RadialGradient,
  Rect,
  RoundedRect,
  Skia,
  useClock,
  vec,
} from '@shopify/react-native-skia';
import { useDerivedValue } from 'react-native-reanimated';

import { colors } from '../theme/colors';

/** Kareye özgü, her seferinde aynı çıkan rastgele sayı (0–1). Süslerin yeri sabit kalsın diye. */
export function rand(x: number, y: number, salt = 0): number {
  let h = Math.imul(x + 1, 374761393) ^ Math.imul(y + 1, 668265263) ^ Math.imul(salt + 7, 2147483647);
  h = Math.imul(h ^ (h >>> 13), 1274126177);
  h ^= h >>> 16;
  return (h >>> 0) / 4294967296;
}

type Pt = [number, number];

export function polygon(points: Pt[]) {
  const d = points.map(([px, py], i) => `${i === 0 ? 'M' : 'L'}${px.toFixed(2)} ${py.toFixed(2)}`).join(' ');
  return Skia.Path.MakeFromSVGString(`${d} Z`)!;
}

/** Yumuşak temas gölgesi (nesnenin yere değdiği yer) */
export function ContactShadow({ cx, cy, w, h, strength = 0.45 }: { cx: number; cy: number; w: number; h: number; strength?: number }) {
  return (
    <Group>
      <Oval x={cx - w / 2 + w * 0.08} y={cy - h / 2 + h * 0.2} width={w} height={h} color={`rgba(35, 8, 0, ${strength * 0.6})`}>
        <BlurMask blur={h * 0.6} style="normal" />
      </Oval>
      <Oval x={cx - w * 0.36} y={cy - h * 0.3} width={w * 0.72} height={h * 0.6} color={`rgba(25, 5, 0, ${strength})`}>
        <BlurMask blur={h * 0.25} style="normal" />
      </Oval>
    </Group>
  );
}

export function Glow({ cx, cy, r, color }: { cx: number; cy: number; r: number; color: string }) {
  return (
    <Circle cx={cx} cy={cy} r={r} color={color}>
      <BlurMask blur={r * 0.8} style="normal" />
    </Circle>
  );
}

export function Sparkle({ cx, cy, r }: { cx: number; cy: number; r: number }) {
  const t = r * 0.18;
  return (
    <Group>
      <Path path={polygon([[cx, cy - r], [cx + t, cy - t], [cx + r, cy], [cx + t, cy + t], [cx, cy + r], [cx - t, cy + t], [cx - r, cy], [cx - t, cy - t]])} color="#FFFFFF">
        <BlurMask blur={0.6} style="solid" />
      </Path>
    </Group>
  );
}

// --- zemin süsleri ---

export function Crater({ cx, cy, r }: { cx: number; cy: number; r: number }) {
  return (
    <Group>
      {/* çukur: ışık sol üstten → iç sol üst duvar gölgede */}
      <Oval x={cx - r} y={cy - r * 0.75} width={r * 2} height={r * 1.5}>
        <RadialGradient
          c={vec(cx + r * 0.35, cy + r * 0.3)}
          r={r * 1.6}
          colors={['rgba(255, 170, 120, 0.10)', 'rgba(70, 18, 6, 0.30)', 'rgba(50, 12, 4, 0.55)']}
        />
      </Oval>
      {/* kenar: sağ alt dudak aydınlık */}
      <Oval x={cx - r * 1.05} y={cy - r * 0.78} width={r * 2.1} height={r * 1.6} style="stroke" strokeWidth={Math.max(1, r * 0.12)}>
        <LinearGradient
          start={vec(cx - r, cy - r)}
          end={vec(cx + r, cy + r)}
          colors={['rgba(60, 15, 5, 0.35)', 'rgba(255, 190, 150, 0.0)', 'rgba(255, 190, 150, 0.45)']}
        />
      </Oval>
    </Group>
  );
}

export function Pebble({ cx, cy, r, seed }: { cx: number; cy: number; r: number; seed: number }) {
  const pts: Pt[] = [];
  for (let i = 0; i < 6; i++) {
    const a = (i / 6) * Math.PI * 2;
    const k = 0.75 + rand(seed, i, 5) * 0.4;
    pts.push([cx + Math.cos(a) * r * k, cy + Math.sin(a) * r * k * 0.75]);
  }
  return (
    <Group>
      <Oval x={cx - r * 0.9} y={cy + r * 0.1} width={r * 2.1} height={r * 0.9} color="rgba(30, 6, 0, 0.35)">
        <BlurMask blur={r * 0.4} style="normal" />
      </Oval>
      <Path path={polygon(pts)}>
        <RadialGradient c={vec(cx - r * 0.35, cy - r * 0.4)} r={r * 1.6} colors={['#A0624F', '#5E372D', '#35201B']} />
      </Path>
    </Group>
  );
}

// --- kaynaklar ---

export function Ice({ x, y, s, seed = 0 }: { x: number; y: number; s: number; seed?: number }) {
  const cx = x + s / 2;
  const baseY = y + s * 0.72;
  // parıltılar sırayla yanıp söner; ışıma hafifçe nefes alır
  const clock = useClock();
  const twinkleA = useDerivedValue(() => Math.max(0, Math.sin(clock.value / 520 + seed * 1.7)));
  const twinkleB = useDerivedValue(() => Math.max(0, Math.sin(clock.value / 610 + seed * 2.3 + 2)));
  const breathe = useDerivedValue(() => 0.75 + 0.25 * Math.sin(clock.value / 900 + seed));
  // [yatay konum, boy, eğim]
  const shards: [number, number, number][] = [
    [-0.2, 0.55, -0.35],
    [0.2, 0.62, 0.3],
    [-0.08, 0.85, -0.1],
    [0.09, 1, 0.08],
  ];
  return (
    <Group>
      {/* kristallerin yere vurduğu soğuk ışık ve buzlanma */}
      <Oval x={cx - s * 0.38} y={baseY - s * 0.1} width={s * 0.76} height={s * 0.26}>
        <RadialGradient c={vec(cx, baseY + s * 0.02)} r={s * 0.38} colors={['rgba(210, 248, 255, 0.75)', 'rgba(150, 225, 245, 0.25)', 'rgba(150, 225, 245, 0)']} />
      </Oval>
      <Group opacity={breathe}>
        <Glow cx={cx} cy={baseY - s * 0.22} r={s * 0.3} color="rgba(110, 220, 255, 0.45)" />
      </Group>
      {shards.map(([dx, k, lean], i) => {
        const bx = cx + dx * s;
        const w = 0.085 * s * (0.7 + k * 0.3);
        const h = 0.46 * s * k;
        const tip: Pt = [bx, baseY - h];
        return (
          <Group key={i} transform={[{ rotate: lean }]} origin={vec(bx, baseY)}>
            <Path path={polygon([[bx - w, baseY], [bx - w * 1.1, baseY - h * 0.72], tip, [bx, baseY]])}>
              <LinearGradient start={vec(bx - w, baseY - h)} end={vec(bx, baseY)} colors={['#FFFFFF', '#D8F7FF']} />
            </Path>
            <Path path={polygon([[bx, baseY], tip, [bx + w * 1.1, baseY - h * 0.72], [bx + w, baseY]])}>
              <LinearGradient start={vec(bx, baseY - h)} end={vec(bx + w, baseY)} colors={['#A6E8FA', '#4FA9CC']} />
            </Path>
            <Line p1={vec(bx, baseY - h * 0.05)} p2={vec(tip[0], tip[1])} color="rgba(255,255,255,0.8)" strokeWidth={0.8} />
          </Group>
        );
      })}
      <Group opacity={twinkleA}>
        <Sparkle cx={cx - s * 0.1} cy={baseY - s * 0.44} r={s * 0.07} />
      </Group>
      <Group opacity={twinkleB}>
        <Sparkle cx={cx + s * 0.2} cy={baseY - s * 0.24} r={s * 0.045} />
      </Group>
    </Group>
  );
}

export function Rock({ gx, gy, x, y, s }: { gx: number; gy: number; x: number; y: number; s: number }) {
  const cx = x + s / 2;
  const cy = y + s * 0.56;
  const boulder = (bx: number, by: number, r: number, salt: number) => {
    const pts: Pt[] = [];
    for (let i = 0; i < 8; i++) {
      const a = (i / 8) * Math.PI * 2 - Math.PI / 2;
      const k = 0.78 + rand(gx, gy, salt + i) * 0.35;
      pts.push([bx + Math.cos(a) * r * k, by + Math.sin(a) * r * k * 0.8]);
    }
    return (
      <Group key={salt}>
        <Path path={polygon(pts)}>
          <RadialGradient c={vec(bx - r * 0.4, by - r * 0.45)} r={r * 1.7} colors={['#A56B5B', '#6A4136', '#3B2420', '#24150F']} />
        </Path>
        {/* üst yüzdeki kırık düzlem */}
        <Path
          path={polygon([
            [bx - r * 0.5, by - r * 0.35],
            [bx + r * 0.05, by - r * 0.7],
            [bx + r * 0.35, by - r * 0.3],
            [bx - r * 0.1, by - r * 0.1],
          ])}
          color="rgba(255, 200, 170, 0.18)"
        />
        <Circle cx={bx + r * 0.25} cy={by + r * 0.15} r={r * 0.08} color={colors.rockSpeck}>
          <BlurMask blur={1} style="solid" />
        </Circle>
      </Group>
    );
  };
  return (
    <Group>
      <ContactShadow cx={cx + s * 0.02} cy={cy + s * 0.17} w={s * 0.78} h={s * 0.2} />
      {boulder(cx - s * 0.17, cy + s * 0.06, s * 0.15, 40)}
      {boulder(cx + s * 0.04, cy - s * 0.02, s * 0.25, 50)}
      {boulder(cx + s * 0.25, cy + s * 0.1, s * 0.1, 60)}
    </Group>
  );
}

// --- robot ---

export function Robot({ x, y, s }: { x: number; y: number; s: number }) {
  const cx = x + s / 2;
  const top = y + s * 0.36; // gövdenin üst yüzü
  const w = s * 0.56;
  const topH = s * 0.2;
  const frontH = s * 0.16;
  // beklerken canlı dursun: gövde hafifçe yaylanır, anten yanıp söner, vizör nabız gibi atar
  const clock = useClock();
  const bob = useDerivedValue(() => [{ translateY: Math.sin(clock.value / 420) * s * 0.012 }]);
  const blink = useDerivedValue(() => (Math.floor(clock.value / 650) % 3 === 0 ? 0.15 : 1));
  const pulse = useDerivedValue(() => 0.6 + 0.4 * Math.sin(clock.value / 320));
  return (
    <Group>
      {/* farların yere düşen ışığı */}
      <Oval x={cx - s * 0.3} y={top + topH + frontH + s * 0.02} width={s * 0.6} height={s * 0.22} color="rgba(120, 230, 255, 0.22)">
        <BlurMask blur={s * 0.08} style="normal" />
      </Oval>
      <ContactShadow cx={cx} cy={top + topH + frontH + s * 0.03} w={s * 0.78} h={s * 0.18} strength={0.55} />
      <Group transform={bob}>

      {/* tekerlekler */}
      {[
        [-1, 0],
        [1, 0],
        [-1, 1],
        [1, 1],
      ].map(([side, row]) => {
        const wx = cx + side * (w / 2 + s * 0.01) - s * 0.06;
        const wy = top + s * 0.04 + row * s * 0.2;
        return (
          <Group key={`${side}${row}`}>
            <RoundedRect x={wx} y={wy} width={s * 0.12} height={s * 0.17} r={s * 0.04} color="#23242E" />
            <RoundedRect x={wx + s * 0.015} y={wy + s * 0.02} width={s * 0.05} height={s * 0.13} r={s * 0.02} color="#3C3E4E" />
          </Group>
        );
      })}

      {/* gövde: ön yüz (koyu) + üst yüz (açık) */}
      <RoundedRect x={cx - w / 2} y={top + topH - s * 0.03} width={w} height={frontH + s * 0.03} r={s * 0.05}>
        <LinearGradient start={vec(cx, top + topH)} end={vec(cx, top + topH + frontH)} colors={['#CFC3B5', '#9E9285']} />
      </RoundedRect>
      <Rect x={cx - w / 2 + s * 0.02} y={top + topH + frontH * 0.35} width={w - s * 0.04} height={s * 0.035} color={colors.accent} />
      <Circle cx={cx - w * 0.32} cy={top + topH + frontH * 0.62} r={s * 0.022} color="#FFF3C4">
        <BlurMask blur={2} style="solid" />
      </Circle>
      <Circle cx={cx + w * 0.32} cy={top + topH + frontH * 0.62} r={s * 0.022} color="#FFF3C4">
        <BlurMask blur={2} style="solid" />
      </Circle>
      <RoundedRect x={cx - w / 2} y={top} width={w} height={topH} r={s * 0.06}>
        <LinearGradient start={vec(cx - w / 2, top)} end={vec(cx + w / 2, top + topH)} colors={['#FFFFFF', '#F1EAE0', '#D9CEC0']} />
      </RoundedRect>
      {/* güneş paneli */}
      <RoundedRect x={cx - w / 2 + s * 0.04} y={top + s * 0.035} width={s * 0.2} height={topH - s * 0.07} r={s * 0.015}>
        <LinearGradient start={vec(cx - w / 2, top)} end={vec(cx - w / 2 + s * 0.22, top + topH)} colors={['#3B5BA8', '#1D2B5C']} />
      </RoundedRect>
      {[1, 2].map((i) => (
        <Line
          key={i}
          p1={vec(cx - w / 2 + s * 0.04 + (i * s * 0.2) / 3, top + s * 0.035)}
          p2={vec(cx - w / 2 + s * 0.04 + (i * s * 0.2) / 3, top + topH - s * 0.035)}
          color="rgba(160, 200, 255, 0.5)"
          strokeWidth={0.7}
        />
      ))}

      {/* baş */}
      <Group opacity={pulse}>
        <Glow cx={cx + s * 0.1} cy={top - s * 0.02} r={s * 0.16} color="rgba(57, 213, 255, 0.45)" />
      </Group>
      <Circle cx={cx + s * 0.1} cy={top - s * 0.02} r={s * 0.14}>
        <RadialGradient c={vec(cx + s * 0.05, top - s * 0.09)} r={s * 0.2} colors={['#FFFFFF', '#E9E1D6', '#A99C8E']} />
      </Circle>
      <RoundedRect x={cx + s * 0.01} y={top - s * 0.03} width={s * 0.18} height={s * 0.07} r={s * 0.035} color="#0E2233" />
      <RoundedRect x={cx + s * 0.025} y={top - s * 0.02} width={s * 0.15} height={s * 0.045} r={s * 0.022} color={colors.visor}>
        <BlurMask blur={1.5} style="solid" />
      </RoundedRect>
      <Circle cx={cx + s * 0.05} cy={top - s * 0.1} r={s * 0.025} color="rgba(255,255,255,0.9)" />

      {/* anten */}
      <Line p1={vec(cx - s * 0.12, top + s * 0.03)} p2={vec(cx - s * 0.17, top - s * 0.2)} color="#BFB3A5" strokeWidth={1.8} />
      <Group opacity={blink}>
        <Glow cx={cx - s * 0.17} cy={top - s * 0.2} r={s * 0.07} color="rgba(255, 122, 61, 0.75)" />
        <Circle cx={cx - s * 0.17} cy={top - s * 0.2} r={s * 0.028} color="#FFB08A" />
      </Group>
      </Group>
    </Group>
  );
}

// --- yapılar ---

export type BuildingKind = 'depot' | 'lab' | 'greenhouse';

/** Kutu biçimli yapı: üst yüz + ön yüz */
function Box({ x, y, w, topH, frontH, r, top: topColors, front: frontColors }: {
  x: number;
  y: number;
  w: number;
  topH: number;
  frontH: number;
  r: number;
  top: string[];
  front: string[];
}) {
  return (
    <Group>
      <RoundedRect x={x} y={y + topH - r} width={w} height={frontH + r} r={r}>
        <LinearGradient start={vec(x, y + topH)} end={vec(x, y + topH + frontH)} colors={frontColors} />
      </RoundedRect>
      <RoundedRect x={x} y={y} width={w} height={topH} r={r}>
        <LinearGradient start={vec(x, y)} end={vec(x + w, y + topH)} colors={topColors} />
      </RoundedRect>
    </Group>
  );
}

const HULL_TOP = ['#FFFFFF', '#F1EAE0', '#DCD2C5'];
const HULL_FRONT = ['#C9BDAF', '#978B7E'];

export function Building({ kind, x, y, s }: { kind: BuildingKind; x: number; y: number; s: number }) {
  const cx = x + s / 2;
  switch (kind) {
    case 'depot': {
      const bx = x + s * 0.1;
      const by = y + s * 0.2;
      const w = s * 0.8;
      const topH = s * 0.34;
      const frontH = s * 0.3;
      const frontY = by + topH;
      return (
        <Group>
          <ContactShadow cx={cx} cy={frontY + frontH} w={s * 1.0} h={s * 0.22} />
          <Box x={bx} y={by} w={w} topH={topH} frontH={frontH} r={s * 0.05} top={HULL_TOP} front={HULL_FRONT} />
          {/* çatı panelleri */}
          {[0, 1, 2].map((i) => (
            <RoundedRect key={i} x={bx + s * (0.05 + i * 0.25)} y={by + s * 0.06} width={s * 0.2} height={topH - s * 0.12} r={s * 0.02} color="rgba(120, 105, 90, 0.18)" />
          ))}
          {/* kapı ve ışığı */}
          <Glow cx={cx} cy={frontY + frontH * 0.55} r={s * 0.16} color="rgba(57, 213, 255, 0.3)" />
          <RoundedRect x={cx - s * 0.13} y={frontY + s * 0.04} width={s * 0.26} height={frontH - s * 0.04} r={s * 0.02} color="#1B2233" />
          <Rect x={cx - s * 0.11} y={frontY + s * 0.06} width={s * 0.22} height={s * 0.02} color={colors.visor} />
          {/* uyarı şeritleri */}
          {[0, 1, 2, 3, 4, 5].map((i) => {
            const sx = bx + s * 0.02 + i * s * 0.05;
            return (
              <Path
                key={`l${i}`}
                path={polygon([[sx, frontY + frontH], [sx + s * 0.03, frontY + frontH - s * 0.07], [sx + s * 0.055, frontY + frontH - s * 0.07], [sx + s * 0.025, frontY + frontH]])}
                color={colors.accent}
              />
            );
          })}
          {/* sandıklar */}
          <Box x={bx + w - s * 0.14} y={frontY + s * 0.06} w={s * 0.16} topH={s * 0.07} frontH={s * 0.12} r={s * 0.015} top={['#F6B26B', '#E38B3E']} front={['#B8611F', '#8A4412']} />
          <Circle cx={bx + s * 0.08} cy={by + s * 0.08} r={s * 0.02} color="#8CFFB0">
            <BlurMask blur={2} style="solid" />
          </Circle>
        </Group>
      );
    }

    case 'lab': {
      const cy = y + s * 0.44;
      const rx = s * 0.38;
      const ry = s * 0.2;
      const depth = s * 0.22;
      const hex = (oy: number): Pt[] =>
        [0, 1, 2, 3, 4, 5].map((i) => {
          const a = (i / 6) * Math.PI * 2;
          return [cx + Math.cos(a) * rx, oy + Math.sin(a) * ry];
        });
      const topHex = hex(cy);
      const bottomHex = hex(cy + depth);
      // ön yüzler: altıgenin alt yarısındaki köşeler (açı 0, 60, 120, 180)
      const front = [topHex[0], topHex[1], topHex[2], topHex[3], bottomHex[3], bottomHex[2], bottomHex[1], bottomHex[0]];
      return (
        <Group>
          <ContactShadow cx={cx} cy={cy + depth + ry * 0.8} w={s * 0.95} h={s * 0.22} />
          <Path path={polygon(front)}>
            <LinearGradient start={vec(cx - rx, cy)} end={vec(cx + rx, cy + depth)} colors={['#D8CDBF', '#A59889', '#857a6e']} />
          </Path>
          {/* pencere bandı */}
          {[-0.2, -0.07, 0.07, 0.2].map((dx) => (
            <Group key={dx}>
              <RoundedRect x={cx + dx * s - s * 0.045} y={cy + ry * 0.75} width={s * 0.09} height={depth * 0.45} r={s * 0.015} color="#132033" />
              <RoundedRect x={cx + dx * s - s * 0.035} y={cy + ry * 0.75 + s * 0.01} width={s * 0.07} height={depth * 0.3} r={s * 0.012} color="rgba(57, 213, 255, 0.85)">
                <BlurMask blur={1.5} style="solid" />
              </RoundedRect>
            </Group>
          ))}
          <Path path={polygon(topHex)}>
            <LinearGradient start={vec(cx - rx, cy - ry)} end={vec(cx + rx, cy + ry)} colors={HULL_TOP} />
          </Path>
          <Path path={polygon(hex(cy).map(([px, py]) => [cx + (px - cx) * 0.6, cy + (py - cy) * 0.6] as Pt))} color="rgba(130, 115, 100, 0.2)" />
          {/* çanak anten */}
          <Line p1={vec(cx + s * 0.08, cy - s * 0.02)} p2={vec(cx + s * 0.16, cy - s * 0.2)} color="#BFB3A5" strokeWidth={2} />
          <Oval x={cx + s * 0.03} y={cy - s * 0.33} width={s * 0.28} height={s * 0.17}>
            <RadialGradient c={vec(cx + s * 0.12, cy - s * 0.28)} r={s * 0.18} colors={['#FFFFFF', '#CFC3B5']} />
          </Oval>
          <Oval x={cx + s * 0.07} y={cy - s * 0.31} width={s * 0.2} height={s * 0.11} color="rgba(120, 105, 90, 0.35)" />
          <Circle cx={cx + s * 0.17} cy={cy - s * 0.255} r={s * 0.02} color={colors.accent} />
        </Group>
      );
    }

    case 'greenhouse': {
      const cy = y + s * 0.56;
      const r = s * 0.36;
      return (
        <Group>
          <ContactShadow cx={cx} cy={cy + r * 0.55} w={s * 0.95} h={s * 0.22} />
          {/* taban halkası */}
          <Oval x={cx - r * 1.05} y={cy + r * 0.2} width={r * 2.1} height={r * 0.55} color="#8F8377" />
          <Oval x={cx - r * 1.05} y={cy + r * 0.12} width={r * 2.1} height={r * 0.5}>
            <LinearGradient start={vec(cx - r, cy)} end={vec(cx + r, cy + r * 0.5)} colors={HULL_TOP} />
          </Oval>
          {/* içerideki bitkiler */}
          <Glow cx={cx} cy={cy + r * 0.1} r={r * 0.7} color="rgba(120, 255, 150, 0.25)" />
          {[
            [-0.45, 0.2, 0.26, '#3E9E57'],
            [0.4, 0.22, 0.24, '#3E9E57'],
            [-0.1, 0.1, 0.34, '#5BC96B'],
            [0.2, 0.05, 0.22, '#78DB7F'],
            [-0.3, -0.05, 0.18, '#78DB7F'],
          ].map(([dx, dy, pr, col], i) => (
            <Circle key={i} cx={cx + (dx as number) * r} cy={cy + (dy as number) * r} r={(pr as number) * r}>
              <RadialGradient c={vec(cx + (dx as number) * r - r * 0.08, cy + (dy as number) * r - r * 0.1)} r={(pr as number) * r * 1.4} colors={['#B6F5A8', col as string, '#2A6E3D']} />
            </Circle>
          ))}
          {/* cam kubbe */}
          <Path path={Skia.Path.MakeFromSVGString(`M ${cx - r} ${cy + r * 0.3} A ${r} ${r} 0 0 1 ${cx + r} ${cy + r * 0.3} Z`)!}>
            <RadialGradient c={vec(cx - r * 0.35, cy - r * 0.45)} r={r * 1.5} colors={['rgba(255,255,255,0.55)', 'rgba(170,230,250,0.18)', 'rgba(90,170,210,0.35)']} />
          </Path>
          <Path
            path={Skia.Path.MakeFromSVGString(`M ${cx - r} ${cy + r * 0.3} A ${r} ${r} 0 0 1 ${cx + r} ${cy + r * 0.3}`)!}
            style="stroke"
            strokeWidth={1.6}
            color="#EFE8DE"
          />
          {[-0.5, 0, 0.5].map((k) => (
            <Path
              key={k}
              path={Skia.Path.MakeFromSVGString(`M ${cx + k * r} ${cy + r * 0.3} Q ${cx + k * r * 0.7} ${cy - r * 0.5} ${cx} ${cy - r * 0.68}`)!}
              style="stroke"
              strokeWidth={0.8}
              color="rgba(240, 232, 222, 0.6)"
            />
          ))}
          {/* parlama */}
          <Path
            path={Skia.Path.MakeFromSVGString(`M ${cx - r * 0.72} ${cy - r * 0.05} Q ${cx - r * 0.62} ${cy - r * 0.55} ${cx - r * 0.2} ${cy - r * 0.62}`)!}
            style="stroke"
            strokeWidth={2.5}
            color="rgba(255,255,255,0.8)"
            strokeCap="round"
          >
            <BlurMask blur={1} style="solid" />
          </Path>
        </Group>
      );
    }
  }
}
