import {
  BlurMask,
  Canvas,
  Circle,
  Group,
  Image,
  Line,
  LinearGradient,
  Oval,
  Path,
  RadialGradient,
  Rect,
  rrect,
  rect,
  Shader,
  useClock,
  vec,
  type SkImage,
} from '@shopify/react-native-skia';
import { useDerivedValue } from 'react-native-reanimated';

import type { Cell, Position } from '../world/grid';

import { Building, ContactShadow, Crater, Glow, Ice, Pebble, polygon, rand, Robot, type BuildingKind } from './mars-art';
import { useSprites, type Sprites } from './sprites';
import { terrainShader } from './terrain-shader';

export interface MapBuilding extends Position {
  kind: BuildingKind;
}

interface Props {
  cells: Cell[][];
  robot: Position;
  buildings?: MapBuilding[];
  /** Haritanın piksel genişliği */
  width: number;
}

/** Yüzeyin altındaki yamacın yüksekliği (kare boyunun katı) */
const CLIFF = 0.55;
/** Diorama'nın altında, uzaya düşen ışıma için boşluk */
const BOTTOM_SPACE = 0.35;

export function mapHeight(cols: number, rows: number, width: number): number {
  const s = width / cols;
  return s * (rows + CLIFF + BOTTOM_SPACE);
}

export function MarsMap({ cells, robot, buildings = [], width }: Props) {
  const cols = cells[0].length;
  const rows = cells.length;
  const s = width / cols;
  const surfaceH = s * rows;
  const cliffH = s * CLIFF;
  const height = mapHeight(cols, rows, width);
  const radius = s * 0.22;
  const sprites = useSprites();
  // havadaki toz yavaşça sürüklenir
  const clock = useClock();
  const drift = useDerivedValue(() => [
    { translateX: Math.sin(clock.value / 3800) * s * 0.12 },
    { translateY: Math.cos(clock.value / 5200) * s * 0.06 },
  ]);

  const occupied = (gx: number, gy: number) =>
    cells[gy][gx] !== 'empty' || buildings.some((b) => b.x === gx && b.y === gy) || (robot.x === gx && robot.y === gy);

  return (
    <Canvas style={{ width, height }}>
      {/* uzaya yansıyan sıcak ışıma */}
      <Oval x={width * 0.08} y={surfaceH + cliffH * 0.5} width={width * 0.84} height={s * 0.7} color="rgba(255, 110, 50, 0.25)">
        <BlurMask blur={s * 0.4} style="normal" />
      </Oval>

      <Cliff width={width} top={surfaceH - radius} height={cliffH + radius} s={s} />

      <Group clip={rrect(rect(0, 0, width, surfaceH), radius, radius)}>
        {terrainShader && (
          <Rect x={0} y={0} width={width} height={surfaceH}>
            <Shader source={terrainShader} uniforms={{ size: [width, surfaceH], cell: s }} />
          </Rect>
        )}

        {/* boş karelerde krater ve çakıllar (yerleri sabit) */}
        {cells.map((row, gy) =>
          row.map((_, gx) => {
            const x = gx * s;
            const y = gy * s;
            const items = [];
            if (!occupied(gx, gy) && rand(gx, gy, 1) < 0.28) {
              items.push(
                <Crater key={`c${gx}${gy}`} cx={x + s * (0.3 + rand(gx, gy, 2) * 0.4)} cy={y + s * (0.3 + rand(gx, gy, 3) * 0.4)} r={s * (0.1 + rand(gx, gy, 4) * 0.1)} />,
              );
            }
            const pebbles = occupied(gx, gy) ? 0 : Math.floor(rand(gx, gy, 6) * 3);
            for (let i = 0; i < pebbles; i++) {
              items.push(
                <Pebble
                  key={`p${gx}${gy}${i}`}
                  cx={x + s * (0.12 + rand(gx, gy, 10 + i) * 0.76)}
                  cy={y + s * (0.12 + rand(gx, gy, 20 + i) * 0.76)}
                  r={s * (0.025 + rand(gx, gy, 30 + i) * 0.03)}
                  seed={gx * 31 + gy * 7 + i}
                />,
              );
            }
            return items;
          }),
        )}

        {/* nesneler satır satır: alttaki satır üsttekinin önünde görünür */}
        {sprites &&
          cells.map((row, gy) => (
            <Group key={`r${gy}`}>
              {row.map((cell, gx) => {
                const x = gx * s;
                const y = gy * s;
                // Buz kendi çizimimiz: kristal biçimi "buz" olarak tek bakışta tanınıyor
                if (cell === 'ice') return <Ice key={gx} x={x} y={y} s={s} seed={gx * 3 + gy * 5} />;
                if (cell === 'rock') return <RockSprite key={gx} sprites={sprites} gx={gx} gy={gy} x={x} y={y} s={s} />;
                return null;
              })}
              {buildings
                .filter((b) => b.y === gy)
                .map((b) => (
                  <BuildingSprite key={`b${b.x}`} sprites={sprites} kind={b.kind} x={b.x * s} y={b.y * s} s={s} />
                ))}
              {/* Robot kendi çizimimiz: oyunun maskotu, karakteri olmalı */}
              {robot.y === gy && <Robot x={robot.x * s} y={robot.y * s} s={s} />}
            </Group>
          ))}

        {/* sahne ışığı: sol üst sıcak, sağ alt serin */}
        <Rect x={0} y={0} width={width} height={surfaceH} blendMode="softLight">
          <LinearGradient start={vec(0, 0)} end={vec(width, surfaceH)} colors={['rgba(255, 220, 180, 0.55)', 'rgba(128, 128, 128, 0)', 'rgba(40, 20, 90, 0.5)']} />
        </Rect>

        {/* havada asılı toz */}
        <Group transform={drift}>
        {Array.from({ length: 28 }, (_, i) => (
          <Circle
            key={`d${i}`}
            cx={rand(i, 1, 90) * width}
            cy={rand(i, 2, 90) * surfaceH}
            r={0.6 + rand(i, 3, 90) * 1.4}
            color="rgba(255, 215, 180, 0.35)"
          >
            <BlurMask blur={1} style="normal" />
          </Circle>
        ))}
        </Group>
      </Group>

      {/* yüzeyin üst kenarındaki ince ışık */}
      <Group clip={rrect(rect(0, 0, width, surfaceH), radius, radius)}>
        <Line p1={vec(radius, 1)} p2={vec(width - radius, 1)} color="rgba(255, 210, 170, 0.45)" strokeWidth={1.5} />
      </Group>
    </Canvas>
  );
}

// --- çizimli nesneler ---

interface SpriteProps {
  sprites: Sprites;
  x: number;
  y: number;
  s: number;
}

/** Çizimi karenin ortasına yerleştirir; scale kareye göre boyut, dy yukarı/aşağı kaydırma. */
function Sprite({ image, x, y, s, scale = 1, dy = 0, flip = false }: { image: SkImage; x: number; y: number; s: number; scale?: number; dy?: number; flip?: boolean }) {
  const size = s * scale;
  const ix = x + (s - size) / 2;
  const iy = y + (s - size) / 2 + dy * s;
  const img = <Image image={image} x={ix} y={iy} width={size} height={size} fit="contain" />;
  if (!flip) return img;
  return (
    <Group transform={[{ scaleX: -1 }]} origin={vec(x + s / 2, y + s / 2)}>
      {img}
    </Group>
  );
}

function RockSprite({ sprites, gx, gy, x, y, s }: SpriteProps & { gx: number; gy: number }) {
  const variant = (['rock-3', 'ore-1', 'rock-2', 'ore-2'] as const)[Math.floor(rand(gx, gy, 9) * 4)];
  const cx = x + s / 2;
  return (
    <Group>
      {/* turuncu kaya turuncu zeminde kaybolmasın: koyu, geniş bir gölge yatağı */}
      <Oval x={cx - s * 0.42} y={y + s * 0.5} width={s * 0.88} height={s * 0.38} color="rgba(40, 8, 0, 0.35)">
        <BlurMask blur={s * 0.07} style="normal" />
      </Oval>
      <ContactShadow cx={cx + s * 0.02} cy={y + s * 0.76} w={s * 0.8} h={s * 0.2} strength={0.6} />
      <Sprite image={sprites[variant]} x={x} y={y} s={s} scale={1.0} dy={-0.02} flip={rand(gx, gy, 11) < 0.5} />
    </Group>
  );
}

/** Depo ve laboratuvar Kenney çizimi; sera kendi cam kubbe çizimimiz. */
function BuildingSprite({ sprites, kind, x, y, s }: SpriteProps & { kind: BuildingKind }) {
  if (kind === 'greenhouse') return <Building kind="greenhouse" x={x} y={y} s={s} />;
  const cx = x + s / 2;
  return (
    <Group>
      <ContactShadow cx={cx} cy={y + s * 0.88} w={s * 1.05} h={s * 0.24} strength={0.55} />
      {kind === 'lab' && <Glow cx={cx} cy={y + s * 0.35} r={s * 0.32} color="rgba(57, 213, 255, 0.25)" />}
      {kind === 'depot' && <Glow cx={cx - s * 0.05} cy={y + s * 0.62} r={s * 0.2} color="rgba(255, 150, 80, 0.3)" />}
      <Sprite image={sprites[kind]} x={x} y={y} s={s} scale={1.08} dy={-0.06} />
    </Group>
  );
}

/** Yüzeyin altındaki kaya katmanlı yamaç; alt kenarı düzensiz. */
function Cliff({ width, top, height, s }: { width: number; top: number; height: number; s: number }) {
  const bottom = top + height;
  const pts: [number, number][] = [[0, top], [width, top], [width, bottom - s * 0.12]];
  const steps = 14;
  for (let i = steps; i >= 0; i--) {
    const px = (i / steps) * width;
    const dip = rand(i, 0, 70) * s * 0.28 + (i % 2) * s * 0.06;
    pts.push([px, bottom - s * 0.12 + dip * (0.4 + 0.6 * Math.sin((i / steps) * Math.PI))]);
  }
  const strata = [0.3, 0.52, 0.72];
  return (
    <Group>
      <Path path={polygon(pts)}>
        <LinearGradient start={vec(0, top)} end={vec(0, bottom + s * 0.3)} colors={['#8C3519', '#6B2511', '#461607', '#2A0C04']} />
      </Path>
      {/* katmanlar */}
      {strata.map((k, i) => (
        <Path
          key={i}
          path={polygon([
            [0, top + height * k],
            [width, top + height * (k - 0.04)],
            [width, top + height * (k - 0.04) + 2],
            [0, top + height * k + 2],
          ])}
          color={i % 2 ? 'rgba(255, 170, 120, 0.12)' : 'rgba(20, 5, 0, 0.3)'}
        />
      ))}
      {/* yamaçta sağ tarafa düşen gölge */}
      <Rect x={width * 0.55} y={top} width={width * 0.45} height={height + s * 0.3}>
        <LinearGradient start={vec(width * 0.55, 0)} end={vec(width, 0)} colors={['rgba(0,0,0,0)', 'rgba(10, 0, 25, 0.45)']} />
      </Rect>
    </Group>
  );
}
