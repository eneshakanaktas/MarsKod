// "Did you mean: 'enerji'?" önerileri. CPython 3.12 Python/suggestions.c ile birebir aynı algoritma.

import { STDLIB_MODULE_NAMES } from './python-names';

const MAX_CANDIDATE_ITEMS = 750;
const MAX_STRING_SIZE = 40;
const MOVE_COST = 2;
const CASE_COST = 1;

// CPython karakterleri UTF-8 baytları olarak karşılaştırır.
function utf8(s: string): Uint8Array {
  const bytes: number[] = [];
  for (const ch of s) {
    const cp = ch.codePointAt(0)!;
    if (cp < 0x80) bytes.push(cp);
    else if (cp < 0x800) bytes.push(0xc0 | (cp >> 6), 0x80 | (cp & 63));
    else if (cp < 0x10000) bytes.push(0xe0 | (cp >> 12), 0x80 | ((cp >> 6) & 63), 0x80 | (cp & 63));
    else bytes.push(0xf0 | (cp >> 18), 0x80 | ((cp >> 12) & 63), 0x80 | ((cp >> 6) & 63), 0x80 | (cp & 63));
  }
  return Uint8Array.from(bytes);
}

function substitutionCost(a: number, b: number): number {
  if ((a & 31) !== (b & 31)) return MOVE_COST;
  if (a === b) return 0;
  if (a >= 65 && a <= 90) a += 32;
  if (b >= 65 && b <= 90) b += 32;
  return a === b ? CASE_COST : MOVE_COST;
}

function levenshtein(aFull: Uint8Array, bFull: Uint8Array, maxCost: number): number {
  let aStart = 0;
  let bStart = 0;
  let aEnd = aFull.length;
  let bEnd = bFull.length;
  // Ortak baş ve sonları at
  while (aStart < aEnd && bStart < bEnd && aFull[aStart] === bFull[bStart]) {
    aStart++;
    bStart++;
  }
  while (aStart < aEnd && bStart < bEnd && aFull[aEnd - 1] === bFull[bEnd - 1]) {
    aEnd--;
    bEnd--;
  }
  let a = aFull.subarray(aStart, aEnd);
  let b = bFull.subarray(bStart, bEnd);
  if (a.length === 0 || b.length === 0) return (a.length + b.length) * MOVE_COST;
  if (a.length > MAX_STRING_SIZE || b.length > MAX_STRING_SIZE) return maxCost + 1;
  if (b.length < a.length) [a, b] = [b, a];
  if ((b.length - a.length) * MOVE_COST > maxCost) return maxCost + 1;

  const buffer: number[] = [];
  let tmp = MOVE_COST;
  for (let i = 0; i < a.length; i++) {
    buffer[i] = tmp;
    tmp += MOVE_COST;
  }
  let result = 0;
  for (let bIndex = 0; bIndex < b.length; bIndex++) {
    const code = b[bIndex];
    let distance = (result = bIndex * MOVE_COST);
    let minimum = Infinity;
    for (let index = 0; index < a.length; index++) {
      const substitute = distance + substitutionCost(code, a[index]);
      distance = buffer[index];
      const insertDelete = Math.min(result, distance) + MOVE_COST;
      result = Math.min(insertDelete, substitute);
      buffer[index] = result;
      if (result < minimum) minimum = result;
    }
    if (minimum > maxCost) return maxCost + 1;
  }
  return result;
}

/** Adaylar arasından en yakın ismi bulur (CPython calculate_suggestions); yoksa null. */
export function calculateSuggestion(candidates: readonly string[], name: string): string | null {
  if (candidates.length >= MAX_CANDIDATE_ITEMS) return null;
  const nameBytes = utf8(name);
  let best: string | null = null;
  let bestDistance = Infinity;
  for (const item of candidates) {
    if (item === name) continue;
    const itemBytes = utf8(item);
    const maxDistance = Math.min(
      Math.floor(((nameBytes.length + itemBytes.length + 3) * MOVE_COST) / 6),
      bestDistance - 1,
    );
    const distance = levenshtein(nameBytes, itemBytes, maxDistance);
    if (distance > maxDistance) continue;
    if (best === null || distance < bestDistance) {
      best = item;
      bestDistance = distance;
    }
  }
  return best;
}

/**
 * NameError mesajı. Aday sırası CPython'daki gibi: önce fonksiyonun yerel isimleri,
 * bulunamazsa globaller, sonra yerleşikler. Standart kütüphane modülüyse import ipucu eklenir.
 */
export function nameErrorMessage(name: string, scopes: readonly (readonly string[])[]): string {
  let message = `name '${name}' is not defined`;
  let suggestion: string | null = null;
  for (const scope of scopes) {
    suggestion = calculateSuggestion(scope, name);
    if (suggestion) break;
  }
  if (suggestion) message += `. Did you mean: '${suggestion}'?`;
  if ((STDLIB_MODULE_NAMES as readonly string[]).includes(name)) {
    message += suggestion ? ` Or did you forget to import '${name}'?` : `. Did you forget to import '${name}'?`;
  }
  return message;
}
