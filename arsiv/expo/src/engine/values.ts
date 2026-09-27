// Python değerlerinin motordaki karşılıkları ve ekrana yazılış biçimleri (repr/str).
//
// Basit türler JavaScript'in kendi türleriyle tutulur:
//   int → bigint (Python tam sayıları sınırsızdır), float → number, str → string,
//   bool → boolean, None → null

import type * as A from './ast';
import { PythonException } from './errors';

export type Value = bigint | number | string | boolean | null | PyObject;

export type PyObject = PyList | PyTuple | PyRange | PyFunction | PyBuiltin | PyType | PyMethod;

export interface PyList {
  kind: 'list';
  items: Value[];
}

export interface PyTuple {
  kind: 'tuple';
  items: Value[];
}

export interface PyRange {
  kind: 'range';
  start: bigint;
  stop: bigint;
  step: bigint;
}

/** Oyuncunun def ile yazdığı fonksiyon */
export interface PyFunction {
  kind: 'function';
  def: A.StmtOf<'FunctionDef'>;
  /** Hata mesajlarındaki ad, örn. "dis.<locals>.ic" */
  qualname: string;
  /** Varsayılan değerler (tanım anında hesaplanır), parametre adıyla */
  defaults: Map<string, Value>;
  /** Tanımlandığı fonksiyonun çerçevesi (iç içe fonksiyonlar için); modül düzeyindeyse null */
  closure: unknown;
  /** Fonksiyonun yerel değişkenleri (parametreler + atananlar, CPython co_varnames sırasıyla) */
  localNames: string[];
  globalNames: Set<string>;
  nonlocalNames: Set<string>;
}

export type Kwargs = Map<string, Value>;

/** Yerleşik fonksiyon (print, len...) ve oyunun dışarıdan verdiği komutlar */
export interface PyBuiltin {
  kind: 'builtin';
  name: string;
  call: (args: Value[], kwargs: Kwargs) => Value;
}

export type TypeName = 'int' | 'float' | 'str' | 'bool' | 'NoneType' | 'list' | 'tuple' | 'range';

/** int, str, list gibi türler: hem çağrılabilir hem type(x) sonucudur */
export interface PyType {
  kind: 'type';
  name: TypeName | 'function' | 'builtin_function_or_method' | 'type';
  call?: (args: Value[], kwargs: Kwargs) => Value;
}

/** Bir değere bağlı yerleşik metot, örn. liste.append */
export interface PyMethod {
  kind: 'method';
  self: Value;
  name: string;
  call: (args: Value[], kwargs: Kwargs) => Value;
}

export const pyList = (items: Value[]): PyList => ({ kind: 'list', items });
export const pyTuple = (items: Value[]): PyTuple => ({ kind: 'tuple', items });

export function isInt(v: Value): v is bigint {
  return typeof v === 'bigint';
}

/** Python'da bool da bir tam sayıdır (True == 1). Sayı değilse null. */
export function asNumber(v: Value): bigint | number | null {
  if (typeof v === 'bigint' || typeof v === 'number') return v;
  if (typeof v === 'boolean') return v ? 1n : 0n;
  return null;
}

export function typeName(v: Value): string {
  switch (typeof v) {
    case 'bigint':
      return 'int';
    case 'number':
      return 'float';
    case 'string':
      return 'str';
    case 'boolean':
      return 'bool';
  }
  if (v === null) return 'NoneType';
  switch (v.kind) {
    case 'list':
    case 'tuple':
    case 'range':
    case 'function':
      return v.kind;
    case 'builtin':
    case 'method':
      return 'builtin_function_or_method';
    case 'type':
      return 'type';
  }
}

export function pyError(type: string, message: string): PythonException {
  return new PythonException(type, message, null);
}

export function truthy(v: Value): boolean {
  switch (typeof v) {
    case 'bigint':
      return v !== 0n;
    case 'number':
      return v !== 0;
    case 'string':
      return v.length > 0;
    case 'boolean':
      return v;
  }
  if (v === null) return false;
  switch (v.kind) {
    case 'list':
    case 'tuple':
      return v.items.length > 0;
    case 'range':
      return rangeLength(v) > 0n;
    default:
      return true;
  }
}

export function rangeLength(r: PyRange): bigint {
  if (r.step > 0n) return r.stop > r.start ? (r.stop - r.start + r.step - 1n) / r.step : 0n;
  return r.start > r.stop ? (r.start - r.stop - r.step - 1n) / -r.step : 0n;
}

// --- ekrana yazılış ---

/** CPython'un float repr'i: en kısa geri dönüşümlü basamaklar, 1e16 ve üstü / 1e-5 ve altı üslü. */
export function floatRepr(x: number): string {
  if (Number.isNaN(x)) return 'nan';
  if (!Number.isFinite(x)) return x > 0 ? 'inf' : '-inf';
  if (x === 0) return Object.is(x, -0) ? '-0.0' : '0.0';

  const [mantissa, expText] = Math.abs(x).toExponential().split('e');
  const digits = mantissa.replace('.', '');
  const decpt = parseInt(expText, 10) + 1; // ondalık noktanın basamaklara göre yeri
  const sign = x < 0 ? '-' : '';

  if (decpt > -4 && decpt <= 16) {
    if (decpt <= 0) return `${sign}0.${'0'.repeat(-decpt)}${digits}`;
    if (decpt >= digits.length) return `${sign}${digits}${'0'.repeat(decpt - digits.length)}.0`;
    return `${sign}${digits.slice(0, decpt)}.${digits.slice(decpt)}`;
  }
  const m = digits.length > 1 ? `${digits[0]}.${digits.slice(1)}` : digits;
  const e = decpt - 1;
  return `${sign}${m}e${e < 0 ? '-' : '+'}${String(Math.abs(e)).padStart(2, '0')}`;
}

const NON_PRINTABLE = /[\p{C}\p{Z}]/u;

export function strRepr(s: string): string {
  const quote = s.includes("'") && !s.includes('"') ? '"' : "'";
  let out = quote;
  for (const ch of s) {
    if (ch === quote || ch === '\\') out += '\\' + ch;
    else if (ch === '\n') out += '\\n';
    else if (ch === '\r') out += '\\r';
    else if (ch === '\t') out += '\\t';
    else if (ch !== ' ' && NON_PRINTABLE.test(ch)) {
      const cp = ch.codePointAt(0)!;
      if (cp < 0x100) out += '\\x' + cp.toString(16).padStart(2, '0');
      else if (cp < 0x10000) out += '\\u' + cp.toString(16).padStart(4, '0');
      else out += '\\U' + cp.toString(16).padStart(8, '0');
    } else out += ch;
  }
  return out + quote;
}

export function repr(v: Value, seen: Set<PyObject> = new Set()): string {
  switch (typeof v) {
    case 'bigint':
      return v.toString();
    case 'number':
      return floatRepr(v);
    case 'string':
      return strRepr(v);
    case 'boolean':
      return v ? 'True' : 'False';
  }
  if (v === null) return 'None';
  switch (v.kind) {
    case 'list':
    case 'tuple': {
      if (seen.has(v)) return v.kind === 'list' ? '[...]' : '(...)';
      seen.add(v);
      const inner = v.items.map((item) => repr(item, seen)).join(', ');
      seen.delete(v);
      if (v.kind === 'list') return `[${inner}]`;
      return v.items.length === 1 ? `(${inner},)` : `(${inner})`;
    }
    case 'range':
      return v.step === 1n ? `range(${v.start}, ${v.stop})` : `range(${v.start}, ${v.stop}, ${v.step})`;
    case 'function':
      return `<function ${v.qualname}>`;
    case 'builtin':
      return `<built-in function ${v.name}>`;
    case 'method':
      return `<built-in method ${v.name} of ${typeName(v.self)} object>`;
    case 'type':
      return `<class '${v.name}'>`;
  }
}

export function str(v: Value): string {
  return typeof v === 'string' ? v : repr(v);
}
