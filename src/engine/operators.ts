// İşlemler: aritmetik, karşılaştırma, indeksleme, yineleme. Hata mesajları CPython 3.12 ile aynı.

import type * as A from './ast';
import { ExecutionLimit, UnsupportedFeature } from './errors';
import {
  asNumber,
  pyError,
  pyList,
  pyTuple,
  rangeLength,
  truthy,
  typeName,
  type PyRange,
  type Value,
} from './values';

/** Tek seferde üretilebilecek en uzun metin/liste (telefonun belleğini korumak için) */
const MAX_SEQUENCE = 10_000_000;
/** Bir tam sayının en fazla bit uzunluğu (2 ** 1000000 gibi hesaplar oyunu dondurmasın) */
const MAX_INT_BITS = 1_000_000;

// --- metinler: Python kod noktası (code point) sayar, JavaScript UTF-16 birimi ---

const SURROGATE = /[\uD800-\uDFFF]/;

export function strChars(s: string): string[] {
  return SURROGATE.test(s) ? Array.from(s) : s.split('');
}

export function strLength(s: string): number {
  return SURROGATE.test(s) ? Array.from(s).length : s.length;
}

// --- sayılar ---

function toFloat(x: bigint | number): number {
  if (typeof x === 'number') return x;
  const f = Number(x);
  if (!Number.isFinite(f)) throw pyError('OverflowError', 'int too large to convert to float');
  return f;
}

function checkIntSize(x: bigint): bigint {
  const bits = (x < 0n ? -x : x).toString(16).length * 4;
  if (bits > MAX_INT_BITS) throw new ExecutionLimit('size', null);
  return x;
}

function intFloorDiv(a: bigint, b: bigint): bigint {
  const q = a / b;
  return (a % b !== 0n && (a < 0n) !== (b < 0n)) ? q - 1n : q;
}

function intMod(a: bigint, b: bigint): bigint {
  const m = a % b;
  return m !== 0n && (m < 0n) !== (b < 0n) ? m + b : m;
}

/** CPython float_divmod */
function floatDivMod(vx: number, wx: number): [number, number] {
  let mod = vx % wx;
  let div = (vx - mod) / wx;
  if (mod) {
    if (wx < 0 !== mod < 0) {
      mod += wx;
      div -= 1;
    }
  } else {
    mod = copySign(0, wx);
  }
  let floordiv: number;
  if (div) {
    floordiv = Math.floor(div);
    if (div - floordiv > 0.5) floordiv += 1;
  } else {
    floordiv = copySign(0, vx / wx);
  }
  return [floordiv, mod];
}

function copySign(x: number, sign: number): number {
  const negative = sign < 0 || Object.is(sign, -0);
  return negative ? -Math.abs(x) : Math.abs(x);
}

function floatPow(a: number, b: number): number {
  if (a === 0 && b < 0) throw pyError('ZeroDivisionError', '0.0 cannot be raised to a negative power');
  if (a < 0 && Number.isFinite(b) && !Number.isInteger(b)) throw new UnsupportedFeature('complex', null);
  const r = a ** b;
  if (!Number.isFinite(r) && Number.isFinite(a) && Number.isFinite(b)) {
    throw pyError('OverflowError', "(34, 'Numerical result out of range')");
  }
  return r;
}

function numericOp(op: A.BinOperator, x: bigint | number, y: bigint | number): Value | undefined {
  if (typeof x === 'bigint' && typeof y === 'bigint') {
    switch (op) {
      case '+':
        return checkIntSize(x + y);
      case '-':
        return checkIntSize(x - y);
      case '*':
        return checkIntSize(x * y);
      case '/':
        if (y === 0n) throw pyError('ZeroDivisionError', 'division by zero');
        return toFloat(x) / toFloat(y);
      case '//':
        if (y === 0n) throw pyError('ZeroDivisionError', 'integer division or modulo by zero');
        return intFloorDiv(x, y);
      case '%':
        if (y === 0n) throw pyError('ZeroDivisionError', 'integer modulo by zero');
        return intMod(x, y);
      case '**':
        if (y < 0n) {
          if (x === 0n) throw pyError('ZeroDivisionError', '0.0 cannot be raised to a negative power');
          return floatPow(toFloat(x), toFloat(y));
        }
        if (x !== 0n && x !== 1n && x !== -1n && BigInt((x < 0n ? -x : x).toString(2).length) * y > BigInt(MAX_INT_BITS)) {
          throw new ExecutionLimit('size', null);
        }
        return x ** y;
      case '<<':
        if (y < 0n) throw pyError('ValueError', 'negative shift count');
        if (x !== 0n && y > BigInt(MAX_INT_BITS)) throw new ExecutionLimit('size', null);
        return checkIntSize(x << y);
      case '>>':
        if (y < 0n) throw pyError('ValueError', 'negative shift count');
        return x >> y;
      case '&':
        return x & y;
      case '|':
        return x | y;
      case '^':
        return x ^ y;
    }
    return undefined;
  }

  const a = toFloat(x);
  const b = toFloat(y);
  switch (op) {
    case '+':
      return a + b;
    case '-':
      return a - b;
    case '*':
      return a * b;
    case '/':
      if (b === 0) throw pyError('ZeroDivisionError', 'float division by zero');
      return a / b;
    case '//':
      if (b === 0) throw pyError('ZeroDivisionError', 'float floor division by zero');
      return floatDivMod(a, b)[0];
    case '%':
      if (b === 0) throw pyError('ZeroDivisionError', 'float modulo');
      return floatDivMod(a, b)[1];
    case '**':
      return floatPow(a, b);
  }
  return undefined;
}

function repeatCount(n: Value): bigint | null {
  if (typeof n === 'bigint') return n;
  if (typeof n === 'boolean') return n ? 1n : 0n;
  return null;
}

function repeat<T>(items: T[], count: bigint): T[] {
  if (count <= 0n || items.length === 0) return [];
  if (BigInt(items.length) * count > BigInt(MAX_SEQUENCE)) throw new ExecutionLimit('size', null);
  const out: T[] = [];
  for (let i = 0n; i < count; i++) out.push(...items);
  return out;
}

function sequenceRepeat(seq: Value, count: bigint): Value {
  if (typeof seq === 'string') {
    if (count <= 0n || seq === '') return '';
    if (BigInt(seq.length) * count > BigInt(MAX_SEQUENCE)) throw new ExecutionLimit('size', null);
    return seq.repeat(Number(count));
  }
  if (seq !== null && typeof seq === 'object' && seq.kind === 'list') return pyList(repeat(seq.items, count));
  if (seq !== null && typeof seq === 'object' && seq.kind === 'tuple') return pyTuple(repeat(seq.items, count));
  throw new Error('dizi bekleniyordu');
}

function isSequence(v: Value): boolean {
  return typeof v === 'string' || (v !== null && typeof v === 'object' && (v.kind === 'list' || v.kind === 'tuple'));
}

export function binaryOp(op: A.BinOperator, a: Value, b: Value): Value {
  const x = asNumber(a);
  const y = asNumber(b);
  if (x !== null && y !== null) {
    const result = numericOp(op, x, y);
    if (result !== undefined) return result;
  } else if (op === '+') {
    if (typeof a === 'string') {
      if (typeof b === 'string') {
        if (a.length + b.length > MAX_SEQUENCE) throw new ExecutionLimit('size', null);
        return a + b;
      }
      throw pyError('TypeError', `can only concatenate str (not "${typeName(b)}") to str`);
    }
    if (a !== null && typeof a === 'object' && (a.kind === 'list' || a.kind === 'tuple')) {
      if (b !== null && typeof b === 'object' && b.kind === a.kind) {
        const items = [...a.items, ...b.items];
        return a.kind === 'list' ? pyList(items) : pyTuple(items);
      }
      throw pyError('TypeError', `can only concatenate ${a.kind} (not "${typeName(b)}") to ${a.kind}`);
    }
  } else if (op === '*') {
    if (isSequence(a)) {
      const count = repeatCount(b);
      if (count === null) throw pyError('TypeError', `can't multiply sequence by non-int of type '${typeName(b)}'`);
      return sequenceRepeat(a, count);
    }
    if (isSequence(b)) {
      const count = repeatCount(a);
      if (count === null) throw pyError('TypeError', `can't multiply sequence by non-int of type '${typeName(a)}'`);
      return sequenceRepeat(b, count);
    }
  } else if (op === '%' && typeof a === 'string') {
    throw new UnsupportedFeature('str-format', null);
  }
  throw pyError(
    'TypeError',
    `unsupported operand type(s) for ${op}: '${typeName(a)}' and '${typeName(b)}'`,
  );
}

/** x += y: listelerde yerinde değiştirir (Python'daki gibi aynı liste nesnesi kalır). */
export function inplaceOp(op: A.BinOperator, a: Value, b: Value): Value {
  if (a !== null && typeof a === 'object' && a.kind === 'list') {
    if (op === '+') {
      a.items.push(...iterate(b));
      return a;
    }
    if (op === '*') {
      const count = repeatCount(b);
      if (count !== null) {
        a.items = repeat(a.items, count);
        return a;
      }
    }
  }
  return binaryOp(op, a, b);
}

export function unaryOp(op: A.UnaryOperator, v: Value): Value {
  if (op === 'not') return !truthy(v);
  const n = asNumber(v);
  if (n !== null) {
    if (op === '-') return -n;
    if (op === '+') return n;
    if (typeof n === 'bigint') return ~n;
  }
  throw pyError('TypeError', `bad operand type for unary ${op}: '${typeName(v)}'`);
}

// --- karşılaştırma ---

export function pyEquals(a: Value, b: Value): boolean {
  const x = asNumber(a);
  const y = asNumber(b);
  if (x !== null && y !== null) {
    // bigint ile number arasında == tam (matematiksel) karşılaştırma yapar
    // eslint-disable-next-line eqeqeq
    return x == y;
  }
  if (typeof a === 'string' || typeof b === 'string') return a === b;
  if (a === null || b === null) return a === b;
  if (typeof a !== 'object' || typeof b !== 'object') return false;
  if (a === b) return true;
  if ((a.kind === 'list' || a.kind === 'tuple') && b.kind === a.kind) {
    return a.items.length === b.items.length && a.items.every((item, i) => pyEquals(item, b.items[i]));
  }
  if (a.kind === 'range' && b.kind === 'range') {
    const la = rangeLength(a);
    if (la !== rangeLength(b)) return false;
    if (la === 0n) return true;
    if (a.start !== b.start) return false;
    return la === 1n || a.step === b.step;
  }
  return false;
}

type OrderOp = '<' | '>' | '<=' | '>=';

function ordered<T extends bigint | number | string>(op: OrderOp, x: T, y: T): boolean {
  switch (op) {
    case '<':
      return x < y;
    case '>':
      return x > y;
    case '<=':
      return x <= y;
    case '>=':
      return x >= y;
  }
}

/** Sıralama karşılaştırması; listeler/demetler ilk farklı öğeye göre karşılaştırılır. */
export function orderCompare(op: OrderOp, a: Value, b: Value): boolean {
  const x = asNumber(a);
  const y = asNumber(b);
  // bigint ile number arasında < ve > tam (matematiksel) karşılaştırma yapar
  if (x !== null && y !== null) return ordered(op, x as number, y as number);
  if (typeof a === 'string' && typeof b === 'string') return ordered(op, a, b);
  if (
    a !== null && b !== null && typeof a === 'object' && typeof b === 'object' &&
    (a.kind === 'list' || a.kind === 'tuple') && a.kind === b.kind
  ) {
    const n = Math.min(a.items.length, b.items.length);
    for (let i = 0; i < n; i++) {
      if (!pyEquals(a.items[i], b.items[i])) return orderCompare(op, a.items[i], b.items[i]);
    }
    return ordered(op, a.items.length, b.items.length);
  }
  throw pyError('TypeError', `'${op}' not supported between instances of '${typeName(a)}' and '${typeName(b)}'`);
}

/** a < b; sıralama (sorted, min, max) için */
export function pyLess(a: Value, b: Value): boolean {
  return orderCompare('<', a, b);
}

export function compare(op: A.CompareOperator, a: Value, b: Value): boolean {
  switch (op) {
    case '==':
      return pyEquals(a, b);
    case '!=':
      return !pyEquals(a, b);
    case '<':
    case '>':
    case '<=':
    case '>=':
      return orderCompare(op, a, b);
    case 'in':
      return contains(b, a);
    case 'not in':
      return !contains(b, a);
    case 'is':
      return a === b;
    case 'is not':
      return a !== b;
  }
}

export function contains(container: Value, item: Value): boolean {
  if (typeof container === 'string') {
    if (typeof item !== 'string') {
      throw pyError('TypeError', `'in <string>' requires string as left operand, not ${typeName(item)}`);
    }
    return container.includes(item);
  }
  if (container !== null && typeof container === 'object') {
    if (container.kind === 'list' || container.kind === 'tuple') return container.items.some((x) => pyEquals(x, item));
    if (container.kind === 'range') {
      const n = asNumber(item);
      if (typeof n === 'bigint') return rangeIndex(container, n) !== null;
      if (typeof n === 'number') return Number.isInteger(n) && rangeIndex(container, BigInt(n)) !== null;
      return false;
    }
  }
  throw pyError('TypeError', `argument of type '${typeName(container)}' is not iterable`);
}

function rangeIndex(r: PyRange, n: bigint): bigint | null {
  const len = rangeLength(r);
  if (len === 0n) return null;
  const offset = n - r.start;
  if (offset % r.step !== 0n) return null;
  const i = offset / r.step;
  return i >= 0n && i < len ? i : null;
}

// --- yineleme ---

export function* iterate(v: Value): Generator<Value, void, undefined> {
  if (typeof v === 'string') {
    yield* strChars(v);
    return;
  }
  if (v !== null && typeof v === 'object') {
    if (v.kind === 'list' || v.kind === 'tuple') {
      // Python listeyi sıra numarasıyla gezer: döngüde liste değişirse bu görülür.
      for (let i = 0; i < v.items.length; i++) yield v.items[i];
      return;
    }
    if (v.kind === 'range') {
      if (v.step > 0n) for (let i = v.start; i < v.stop; i += v.step) yield i;
      else for (let i = v.start; i > v.stop; i += v.step) yield i;
      return;
    }
  }
  throw pyError('TypeError', `'${typeName(v)}' object is not iterable`);
}

// --- indeksleme ---

function indexOf(index: Value, container: string): bigint {
  if (typeof index === 'bigint') return index;
  if (typeof index === 'boolean') return index ? 1n : 0n;
  if (container === 'str') throw pyError('TypeError', `string indices must be integers, not '${typeName(index)}'`);
  throw pyError('TypeError', `${container} indices must be integers or slices, not ${typeName(index)}`);
}

function sliceIndices(slice: SliceValue, length: number): [number, number, number] {
  const toInt = (v: Value, fallback: number): number => {
    if (v === null) return fallback;
    const n = asNumber(v);
    if (typeof n !== 'bigint') {
      throw pyError('TypeError', 'slice indices must be integers or None or have an __index__ method');
    }
    return Number(n);
  };
  const step = toInt(slice.step, 1);
  if (step === 0) throw pyError('ValueError', 'slice step cannot be zero');
  const clamp = (i: number, lo: number, hi: number) => {
    if (i < 0) i += length;
    return Math.min(Math.max(i, lo), hi);
  };
  if (step > 0) {
    return [clamp(toInt(slice.lower, 0), 0, length), clamp(toInt(slice.upper, length), 0, length), step];
  }
  return [clamp(toInt(slice.lower, length - 1), -1, length - 1), clamp(toInt(slice.upper, -length - 1), -1, length - 1), step];
}

export interface SliceValue {
  lower: Value;
  upper: Value;
  step: Value;
}

function applySlice<T>(items: T[], slice: SliceValue): T[] {
  const [start, stop, step] = sliceIndices(slice, items.length);
  const out: T[] = [];
  if (step > 0) for (let i = start; i < stop; i += step) out.push(items[i]);
  else for (let i = start; i > stop; i += step) out.push(items[i]);
  return out;
}

export function getItem(container: Value, index: Value | SliceValue): Value {
  const isSlice = index !== null && typeof index === 'object' && 'lower' in index;
  if (typeof container === 'string') {
    const chars = strChars(container);
    if (isSlice) return applySlice(chars, index).join('');
    const i = normalizeIndex(indexOf(index as Value, 'str'), chars.length, 'string index out of range');
    return chars[i];
  }
  if (container !== null && typeof container === 'object') {
    if (container.kind === 'list' || container.kind === 'tuple') {
      if (isSlice) {
        const items = applySlice(container.items, index);
        return container.kind === 'list' ? pyList(items) : pyTuple(items);
      }
      const i = normalizeIndex(
        indexOf(index as Value, container.kind),
        container.items.length,
        `${container.kind} index out of range`,
      );
      return container.items[i];
    }
    if (container.kind === 'range') {
      if (isSlice) throw new UnsupportedFeature('method', null, 'range slice');
      const len = rangeLength(container);
      let i = indexOf(index as Value, 'range');
      if (i < 0n) i += len;
      if (i < 0n || i >= len) throw pyError('IndexError', 'range object index out of range');
      return container.start + i * container.step;
    }
  }
  throw pyError('TypeError', `'${typeName(container)}' object is not subscriptable`);
}

function normalizeIndex(i: bigint, length: number, message: string): number {
  const n = i < 0n ? i + BigInt(length) : i;
  if (n < 0n || n >= BigInt(length)) throw pyError('IndexError', message);
  return Number(n);
}

export function setItem(container: Value, index: Value | SliceValue, value: Value): void {
  const isSlice = index !== null && typeof index === 'object' && 'lower' in index;
  if (container !== null && typeof container === 'object' && container.kind === 'list') {
    if (isSlice) throw new UnsupportedFeature('slice-assign', null);
    const i = normalizeIndex(indexOf(index as Value, 'list'), container.items.length, 'list assignment index out of range');
    container.items[i] = value;
    return;
  }
  throw pyError('TypeError', `'${typeName(container)}' object does not support item assignment`);
}

export function length(v: Value): bigint {
  if (typeof v === 'string') return BigInt(strLength(v));
  if (v !== null && typeof v === 'object') {
    if (v.kind === 'list' || v.kind === 'tuple') return BigInt(v.items.length);
    if (v.kind === 'range') return rangeLength(v);
  }
  throw pyError('TypeError', `object of type '${typeName(v)}' has no len()`);
}
