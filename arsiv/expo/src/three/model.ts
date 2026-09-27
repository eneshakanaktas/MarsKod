// Gömülü model verisinden Three.js nesnesi kurar. Aynı model defalarca kullanılırsa
// geometri ve malzemeler paylaşılır (bellek ve hız için).

import * as THREE from 'three';

import { MODELS, type ModelName } from './models.generated';

export interface ModelPart {
  material: string;
  color: readonly number[];
  metalness: number;
  roughness: number;
  positions: string;
  normals: string;
  indices: string | null;
  indices32: boolean;
}

export type ModelData = readonly ModelPart[];

/** Malzeme adına göre renk ve görünüm ayarı (örn. yeşil kristalleri buz mavisine çevirmek). */
export type MaterialOverrides = Record<string, Partial<THREE.MeshStandardMaterialParameters>>;

const B64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
const LOOKUP = new Uint8Array(128);
for (let i = 0; i < B64.length; i++) LOOKUP[B64.charCodeAt(i)] = i;

/** base64 → bayt dizisi (telefonda atob'a güvenmemek için kendi çözücümüz) */
function decodeBase64(s: string): ArrayBuffer {
  const clean = s.replace(/=+$/, '');
  const bytes = new Uint8Array(Math.floor((clean.length * 3) / 4));
  let o = 0;
  for (let i = 0; i < clean.length; i += 4) {
    const a = LOOKUP[clean.charCodeAt(i)];
    const b = LOOKUP[clean.charCodeAt(i + 1)];
    const c = LOOKUP[clean.charCodeAt(i + 2)];
    const d = LOOKUP[clean.charCodeAt(i + 3)];
    bytes[o++] = (a << 2) | (b >> 4);
    if (i + 2 < clean.length) bytes[o++] = ((b & 15) << 4) | (c >> 2);
    if (i + 3 < clean.length) bytes[o++] = ((c & 3) << 6) | d;
  }
  return bytes.buffer;
}

const geometryCache = new Map<string, THREE.BufferGeometry[]>();
const materialCache = new Map<string, THREE.Material>();

function geometries(name: ModelName): THREE.BufferGeometry[] {
  let list = geometryCache.get(name);
  if (!list) {
    list = (MODELS[name] as ModelData).map((part) => {
      const g = new THREE.BufferGeometry();
      g.setAttribute('position', new THREE.BufferAttribute(new Float32Array(decodeBase64(part.positions)), 3));
      g.setAttribute('normal', new THREE.BufferAttribute(new Float32Array(decodeBase64(part.normals)), 3));
      if (part.indices) {
        const raw = decodeBase64(part.indices);
        g.setIndex(new THREE.BufferAttribute(part.indices32 ? new Uint32Array(raw) : new Uint16Array(raw), 1));
      }
      g.computeBoundingBox();
      return g;
    });
    geometryCache.set(name, list);
  }
  return list;
}

function material(part: ModelPart, overrides?: MaterialOverrides): THREE.Material {
  const extra = overrides?.[part.material];
  const key = `${part.material}|${part.color.join(',')}|${JSON.stringify(extra ?? {})}`;
  let m = materialCache.get(key);
  if (!m) {
    m = new THREE.MeshStandardMaterial({
      // Kenney renkleri sRGB olarak tasarlanmış (önizleme görselleri böyle); doğrusal okunursa soluk kalır
      color: new THREE.Color().setRGB(part.color[0], part.color[1], part.color[2], THREE.SRGBColorSpace),
      metalness: Math.min(part.metalness, 0.3),
      roughness: Math.max(part.roughness, 0.55),
      flatShading: true,
      // Bazı Kenney modellerinde yüz yönleri karışık; iki yüzü de çiz ki delik görünmesin
      side: THREE.DoubleSide,
      ...extra,
    });
    materialCache.set(key, m);
  }
  return m;
}

/** Modeli, gölge atan/alan parçalardan oluşan bir grup olarak kurar. */
export function buildModel(name: ModelName, overrides?: MaterialOverrides): THREE.Group {
  const group = new THREE.Group();
  const parts = MODELS[name] as ModelData;
  geometries(name).forEach((g, i) => {
    const mesh = new THREE.Mesh(g, material(parts[i], overrides));
    mesh.castShadow = true;
    mesh.receiveShadow = true;
    group.add(mesh);
  });
  return group;
}

/** Modelin yatay (x, z) boyutu: kareye sığdırmak için */
export function modelFootprint(name: ModelName): { width: number; depth: number; height: number } {
  const box = new THREE.Box3();
  for (const g of geometries(name)) box.union(g.boundingBox!);
  return { width: box.max.x - box.min.x, depth: box.max.z - box.min.z, height: box.max.y - box.min.y };
}
