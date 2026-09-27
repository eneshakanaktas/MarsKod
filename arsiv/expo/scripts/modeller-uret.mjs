// assets/models/**/*.glb dosyalarını src/three/models.generated.ts içine gömülü veri olarak yazar.
//
// Neden: Telefonda 3D model dosyası yüklemek (expo-asset, dosya sistemi) kırılgan. Modeller çok
// küçük (1–25 KB) ve doku resmi içermiyor; geometri ve renkleri doğrudan koda gömmek en sağlamı.
// Çalıştırma: node scripts/modeller-uret.mjs

import { readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { basename, join } from 'node:path';

import * as THREE from 'three';

const ROOT = join(import.meta.dirname, '..');
const SOURCE = join(ROOT, 'assets', 'models', 'kenney-space-kit');
const OUT = join(ROOT, 'src', 'three', 'models.generated.ts');

const COMPONENT = { 5120: Int8Array, 5121: Uint8Array, 5122: Int16Array, 5123: Uint16Array, 5125: Uint32Array, 5126: Float32Array };
const SIZE = { SCALAR: 1, VEC2: 2, VEC3: 3, VEC4: 4 };

function readGlb(file) {
  const buf = readFileSync(file);
  const jsonLength = buf.readUInt32LE(12);
  const json = JSON.parse(buf.subarray(20, 20 + jsonLength).toString('utf8'));
  const binStart = 20 + jsonLength + 8;
  const bin = buf.subarray(binStart);
  return { json, bin };
}

function accessorData({ json, bin }, index) {
  const acc = json.accessors[index];
  const view = json.bufferViews[acc.bufferView];
  const Type = COMPONENT[acc.componentType];
  const n = SIZE[acc.type];
  const start = (view.byteOffset ?? 0) + (acc.byteOffset ?? 0);
  const stride = view.byteStride ?? n * Type.BYTES_PER_ELEMENT;
  const out = new Type(acc.count * n);
  const dv = new DataView(bin.buffer, bin.byteOffset + start);
  const read = {
    5126: (o) => dv.getFloat32(o, true),
    5125: (o) => dv.getUint32(o, true),
    5123: (o) => dv.getUint16(o, true),
    5122: (o) => dv.getInt16(o, true),
    5121: (o) => dv.getUint8(o),
    5120: (o) => dv.getInt8(o),
  }[acc.componentType];
  for (let i = 0; i < acc.count; i++) {
    for (let k = 0; k < n; k++) out[i * n + k] = read(i * stride + k * Type.BYTES_PER_ELEMENT);
  }
  return out;
}

const b64 = (typed) => Buffer.from(typed.buffer, typed.byteOffset, typed.byteLength).toString('base64');

function convert(file) {
  const glb = readGlb(file);
  const { json } = glb;
  const parts = [];

  const visit = (nodeIndex, parent) => {
    const node = json.nodes[nodeIndex];
    const local = new THREE.Matrix4();
    // Kenney dosyalarındaki kök düğüm kaydırması (kit düzeninden kalma) atılır; dönüş ve ölçek korunur.
    const q = new THREE.Quaternion(...(node.rotation ?? [0, 0, 0, 1]));
    const s = new THREE.Vector3(...(node.scale ?? [1, 1, 1]));
    local.compose(new THREE.Vector3(), q, s);
    const world = parent.clone().multiply(local);
    if (node.mesh !== undefined) {
      const normalMatrix = new THREE.Matrix3().getNormalMatrix(world);
      for (const prim of json.meshes[node.mesh].primitives) {
        const pos = accessorData(glb, prim.attributes.POSITION);
        const nor = accessorData(glb, prim.attributes.NORMAL);
        const v = new THREE.Vector3();
        for (let i = 0; i < pos.length; i += 3) {
          v.set(pos[i], pos[i + 1], pos[i + 2]).applyMatrix4(world);
          pos.set([v.x, v.y, v.z], i);
          v.set(nor[i], nor[i + 1], nor[i + 2]).applyMatrix3(normalMatrix).normalize();
          nor.set([v.x, v.y, v.z], i);
        }
        const idx = prim.indices !== undefined ? accessorData(glb, prim.indices) : null;
        const mat = json.materials?.[prim.material] ?? {};
        const pbr = mat.pbrMetallicRoughness ?? {};
        parts.push({
          material: mat.name ?? 'default',
          color: (pbr.baseColorFactor ?? [1, 1, 1, 1]).slice(0, 3).map((c) => +c.toFixed(4)),
          metalness: pbr.metallicFactor ?? 0,
          roughness: pbr.roughnessFactor ?? 1,
          positions: b64(pos),
          normals: b64(nor),
          indices: idx ? b64(idx instanceof Uint32Array ? idx : new Uint16Array(idx)) : null,
          indices32: idx instanceof Uint32Array,
        });
      }
    }
    for (const child of node.children ?? []) visit(child, world);
  };

  const scene = json.scenes[json.scene ?? 0];
  for (const root of scene.nodes) visit(root, new THREE.Matrix4());
  return parts;
}

const models = {};
for (const file of readdirSync(SOURCE).filter((f) => f.endsWith('.glb')).sort()) {
  models[basename(file, '.glb')] = convert(join(SOURCE, file));
}

const body = JSON.stringify(models);
writeFileSync(
  OUT,
  `// OTOMATİK ÜRETİLDİ: node scripts/modeller-uret.mjs — elle değiştirme.\n` +
    `// Kaynak: Kenney Space Kit (CC0), assets/models/kenney-space-kit/\n\n` +
    `import type { ModelData } from './model';\n\n` +
    `export const MODELS = ${body} as const satisfies Record<string, ModelData>;\n\n` +
    `export type ModelName = keyof typeof MODELS;\n`,
);
console.log(`${Object.keys(models).length} model yazıldı (${(body.length / 1024).toFixed(0)} KB)`);
