// Mars haritasının 3 boyutlu sahnesi (Three.js + React Three Fiber).
// Koordinatlar: kare (gx, gy) → dünya (x, z). Kuzey (gy = 0) ekranın üstünde, uzakta.
// Işık sol üst-arkadan gelir; gölgeler sağ alt-öne düşer.

import { Canvas, useFrame } from '@react-three/fiber/native';
import { useMemo, useRef } from 'react';
import { LogBox, View } from 'react-native';
import * as THREE from 'three';

import type { Cell, Position } from '../world/grid';

import { buildModel, modelFootprint, type MaterialOverrides } from './model';
import type { ModelName } from './models.generated';

// Telefonun grafik sürücüsü gölgelendirici derlerken zararsız bilgi notları yazıyor; ekranda göstermeyelim.
LogBox.ignoreLogs(['THREE.WebGLProgram', 'THREE.WebGLRenderer', 'THREE.WebGLShadowMap', 'THREE.Clock']);

export type BuildingKind = 'depot' | 'lab' | 'greenhouse';

export interface SceneBuilding extends Position {
  kind: BuildingKind;
}

interface Props {
  cells: Cell[][];
  robot: Position;
  buildings?: SceneBuilding[];
  width: number;
  height: number;
}

/** Kareye özgü, her seferinde aynı çıkan rastgele sayı (0–1): süslerin yeri sabit kalsın. */
function rand(x: number, y: number, salt = 0): number {
  let h = Math.imul(x + 1, 374761393) ^ Math.imul(y + 1, 668265263) ^ Math.imul(salt + 7, 2147483647);
  h = Math.imul(h ^ (h >>> 13), 1274126177);
  h ^= h >>> 16;
  return (h >>> 0) / 4294967296;
}

// Kristaller buz: açık mavi, hafif kendi ışığıyla parlıyor
const ICE: MaterialOverrides = {
  rock: { color: new THREE.Color('#DDF6FF'), emissive: new THREE.Color('#4FB0E0'), emissiveIntensity: 0.32, roughness: 0.35 },
  rockTrack: { color: new THREE.Color('#BCEBFA'), emissive: new THREE.Color('#4FB0E0'), emissiveIntensity: 0.25, roughness: 0.35 },
  crystal: { color: new THREE.Color('#8FE6FF'), emissive: new THREE.Color('#1FA8E8'), emissiveIntensity: 0.7, roughness: 0.15 },
};
// Seranın kubbesi: içeriden yeşil ışık vuran cam
const GLASS: MaterialOverrides = {
  dark: { color: new THREE.Color('#7FE0C8'), emissive: new THREE.Color('#2E9E6A'), emissiveIntensity: 0.5, roughness: 0.1, transparent: true, opacity: 0.85 },
};

/** turn: modelin kapısı kameraya (güneye) baksın diye dönüş */
const BUILDING_MODEL: Record<BuildingKind, { model: ModelName; size: number; turn: number; overrides?: MaterialOverrides }> = {
  depot: { model: 'hangar_smallA', size: 0.96, turn: -Math.PI / 2 },
  lab: { model: 'hangar_roundB', size: 0.88, turn: 0 },
  greenhouse: { model: 'hangar_roundGlass', size: 0.88, turn: 0, overrides: GLASS },
};

/** Modeli kurar, yatayda `size` kareye sığacak şekilde ölçekler ve (x, z)'ye yerleştirir. */
function place(name: ModelName, x: number, z: number, size: number, rotationY = 0, overrides?: MaterialOverrides) {
  const obj = buildModel(name, overrides);
  const { width, depth } = modelFootprint(name);
  const k = size / Math.max(width, depth);
  obj.scale.setScalar(k);
  obj.position.set(x, 0, z);
  obj.rotation.y = rotationY;
  return obj;
}

function buildWorld(cells: Cell[][], buildings: SceneBuilding[], robot: Position): THREE.Group {
  const rows = cells.length;
  const cols = cells[0].length;
  const world = new THREE.Group();
  const X = (gx: number) => gx - (cols - 1) / 2;
  const Z = (gy: number) => gy - (rows - 1) / 2;

  // --- diorama tabanı: pürüzlü kenarlı, aşağı doğru daralan kaya katmanları ---
  const tiers = [
    // üst katman zeminin biraz altında: aynı yükseklikte olursa iki yüzey titreşip çizgi çizgi görünür
    { inset: -0.06, top: -0.015, depth: 0.5, color: '#C4623F', jag: 0.08, seed: 1 },
    { inset: 0.3, top: -0.5, depth: 0.55, color: '#9C4830', jag: 0.18, seed: 2 },
    { inset: 0.9, top: -1.05, depth: 0.6, color: '#733324', jag: 0.3, seed: 3 },
    { inset: 1.8, top: -1.65, depth: 0.55, color: '#4E2219', jag: 0.35, seed: 4 },
  ];
  for (const tier of tiers) {
    const mesh = rockTier(cols - tier.inset * 2, rows - tier.inset * 2, tier.depth, tier.jag, tier.seed, tier.color);
    mesh.position.y = tier.top;
    world.add(mesh);
  }

  // --- zemin kareleri ---
  for (let gy = 0; gy < rows; gy++) {
    for (let gx = 0; gx < cols; gx++) {
      const tile = buildModel('terrain');
      tile.position.set(X(gx), 0, Z(gy));
      // zemin gölge alır ama atmaz: ince zemin kendine gölge düşürüp çizgi çizgi görünmesin
      tile.traverse((o) => (o.castShadow = false));
      tile.userData.ground = true;
      world.add(tile);
    }
  }

  // --- kare çizgileri: oyuncu kareleri sayabilsin ---
  const lines: number[] = [];
  for (let i = 0; i <= cols; i++) lines.push(i - cols / 2, 0.004, -rows / 2, i - cols / 2, 0.004, rows / 2);
  for (let j = 0; j <= rows; j++) lines.push(-cols / 2, 0.004, j - rows / 2, cols / 2, 0.004, j - rows / 2);
  const grid = new THREE.LineSegments(
    new THREE.BufferGeometry().setAttribute('position', new THREE.Float32BufferAttribute(lines, 3)),
    new THREE.LineBasicMaterial({ color: '#6B2A16', transparent: true, opacity: 0.35 }),
  );
  world.add(grid);

  const occupied = (gx: number, gy: number) =>
    cells[gy][gx] !== 'empty' || buildings.some((b) => b.x === gx && b.y === gy) || (robot.x === gx && robot.y === gy);

  // --- süsler, kaynaklar, yapılar ---
  for (let gy = 0; gy < rows; gy++) {
    for (let gx = 0; gx < cols; gx++) {
      const x = X(gx);
      const z = Z(gy);
      const turn = Math.floor(rand(gx, gy, 5) * 4) * (Math.PI / 2);
      const cell = cells[gy][gx];
      if (cell === 'ice') {
        const model: ModelName = rand(gx, gy, 6) < 0.5 ? 'rock_crystalsLargeA' : 'rock_crystalsLargeB';
        world.add(place(model, x, z, 0.9, turn, ICE));
      } else if (cell === 'rock') {
        const model: ModelName = rand(gx, gy, 7) < 0.5 ? 'rock_largeA' : 'rock_largeB';
        world.add(place(model, x, z, 0.72, turn));
      } else if (!occupied(gx, gy)) {
        const r = rand(gx, gy, 8);
        if (r < 0.18) world.add(place('crater', x + (rand(gx, gy, 9) - 0.5) * 0.3, z + (rand(gx, gy, 10) - 0.5) * 0.3, 0.5, turn));
        else if (r < 0.34) world.add(place(r < 0.26 ? 'rocks_smallA' : 'rocks_smallB', x, z, 0.4, turn));
      }
    }
  }
  for (const b of buildings) {
    const spec = BUILDING_MODEL[b.kind];
    world.add(place(spec.model, X(b.x), Z(b.y), spec.size, spec.turn, spec.overrides));
  }

  return world;
}

/**
 * Bir kaya katmanı: kenarı pürüzlü dikdörtgen, `depth` kadar aşağı uzanır. Köşeli (flat) gölgeleme
 * kaya yüzeyi hissi verir. Üst yüzü y = 0'dadır.
 */
function rockTier(width: number, depth: number, height: number, jag: number, seed: number, color: string): THREE.Mesh {
  const shape = new THREE.Shape();
  const pts: THREE.Vector2[] = [];
  const perSide = 10;
  const corners: [number, number][] = [
    [-width / 2, -depth / 2],
    [width / 2, -depth / 2],
    [width / 2, depth / 2],
    [-width / 2, depth / 2],
  ];
  for (let c = 0; c < 4; c++) {
    const [ax, az] = corners[c];
    const [bx, bz] = corners[(c + 1) % 4];
    for (let i = 0; i < perSide; i++) {
      const t = i / perSide;
      // kenara dik yönde rastgele girinti/çıkıntı
      const nx = -(bz - az);
      const nz = bx - ax;
      const len = Math.hypot(nx, nz);
      const off = (rand(c * 31 + i, seed, 50) - 0.5) * 2 * jag;
      pts.push(new THREE.Vector2(ax + (bx - ax) * t + (nx / len) * off, az + (bz - az) * t + (nz / len) * off));
    }
  }
  shape.setFromPoints(pts);
  const geometry = new THREE.ExtrudeGeometry(shape, { depth: height, bevelEnabled: false });
  // Şekil XY düzleminde çizildi; yatay yap ve aşağı doğru uzat
  geometry.rotateX(Math.PI / 2);
  const mesh = new THREE.Mesh(geometry, new THREE.MeshStandardMaterial({ color, roughness: 1, flatShading: true }));
  mesh.receiveShadow = true;
  return mesh;
}

function Moon() {
  return (
    <mesh position={[-11, 5, -18]}>
      <icosahedronGeometry args={[3.2, 2]} />
      <meshStandardMaterial color="#8C7CA8" roughness={1} flatShading emissive="#2A1F45" emissiveIntensity={0.4} />
    </mesh>
  );
}

function Stars() {
  const points = useMemo(() => {
    const positions: number[] = [];
    for (let i = 0; i < 400; i++) {
      const theta = rand(i, 1, 40) * Math.PI * 2;
      const phi = Math.acos(2 * rand(i, 2, 40) - 1);
      const r = 30 + rand(i, 3, 40) * 10;
      positions.push(r * Math.sin(phi) * Math.cos(theta), r * Math.cos(phi), r * Math.sin(phi) * Math.sin(theta));
    }
    const g = new THREE.BufferGeometry().setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
    return new THREE.Points(g, new THREE.PointsMaterial({ color: '#FFFFFF', size: 0.12, sizeAttenuation: true, transparent: true, opacity: 0.8 }));
  }, []);
  return <primitive object={points} />;
}

function Rover({ x, z }: { x: number; z: number }) {
  const ref = useRef<THREE.Group>(null);
  const rover = useMemo(() => place('rover', 0, 0, 0.78, -Math.PI / 2), []);
  useFrame(({ clock }) => {
    if (ref.current) ref.current.position.y = Math.abs(Math.sin(clock.elapsedTime * 3)) * 0.02;
  });
  return (
    <group position={[x, 0, z]}>
      <group ref={ref}>
        <primitive object={rover} />
      </group>
      {/* robotun üstündeki küçük sinyal ışığı */}
      <pointLight position={[0, 0.6, 0]} color="#39D5FF" intensity={0.6} distance={1.6} />
    </group>
  );
}

function Scene({ cells, robot, buildings = [] }: Omit<Props, 'width' | 'height'>) {
  const world = useMemo(() => buildWorld(cells, buildings, robot), [cells, buildings, robot]);
  const rows = cells.length;
  const cols = cells[0].length;
  return (
    <>
      <color attach="background" args={['#0B0C1D']} />
      <hemisphereLight args={['#FFE9DA', '#4A2230', 0.95]} />
      <directionalLight
        position={[-4, 9, 5]}
        intensity={1.9}
        color="#FFF1E0"
        castShadow
        shadow-mapSize={[1024, 1024]}
        shadow-camera-left={-6}
        shadow-camera-right={6}
        shadow-camera-top={6}
        shadow-camera-bottom={-6}
        shadow-bias={-0.0004}
        shadow-normalBias={0.035}
      />
      {/* alttan uzaya yansıyan sıcak ışık */}
      <pointLight position={[0, -2.5, 2]} color="#FF7A3D" intensity={3} distance={8} />
      <Stars />
      <Moon />
      <primitive object={world} />
      <Rover x={robot.x - (cols - 1) / 2} z={robot.y - (rows - 1) / 2} />
    </>
  );
}

export function MarsScene3D({ width, height, ...scene }: Props) {
  return (
    <View style={{ width, height }}>
      <Canvas
        shadows={{ type: THREE.PCFShadowMap }}
        camera={{ position: [0, 7.2, 8.4], fov: 40, near: 0.1, far: 100 }}
        onCreated={({ camera, gl }) => {
          camera.lookAt(0, -0.9, 0.4);
          gl.toneMapping = THREE.ACESFilmicToneMapping;
          gl.toneMappingExposure = 0.95;
        }}
      >
        <Scene {...scene} />
      </Canvas>
    </View>
  );
}
