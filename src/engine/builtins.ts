// Yerleşik fonksiyonlar (print, len, range...), türler (int, str...) ve metotlar (liste.append...).

import { ExecutionLimit, UnsupportedFeature } from './errors';
import {
  binaryOp,
  iterate,
  length,
  orderCompare,
  pyEquals,
  pyLess,
  strChars,
} from './operators';
import { BUILTIN_NAMES, TYPE_DIR } from './python-names';
import { calculateSuggestion } from './suggestions';
import {
  asNumber,
  pyError,
  pyList,
  pyTuple,
  rangeLength,
  repr,
  str,
  truthy,
  typeName,
  type Kwargs,
  type PyBuiltin,
  type PyType,
  type Value,
} from './values';

export interface BuiltinContext {
  write(text: string): void;
  /** Uzun süren işlemlerde adım sayar (bitmeyen hesaplara karşı) */
  tick(): void;
}

type Fn = (args: Value[], kwargs: Kwargs) => Value;

const MAX_LIST = 10_000_000;

// --- yardımcılar ---

function noKwargs(name: string, kwargs: Kwargs): void {
  if (kwargs.size > 0) throw pyError('TypeError', `${name}() takes no keyword arguments`);
}

function argCount(name: string, args: Value[], min: number, max: number): void {
  const n = args.length;
  if (n >= min && n <= max) return;
  if (min === max) {
    if (min === 0) throw pyError('TypeError', `${name}() takes no arguments (${n} given)`);
    if (min === 1) throw pyError('TypeError', `${name}() takes exactly one argument (${n} given)`);
    throw pyError('TypeError', `${name} expected ${min} arguments, got ${n}`);
  }
  if (n < min) throw pyError('TypeError', `${name} expected at least ${min} argument${min === 1 ? '' : 's'}, got ${n}`);
  throw pyError('TypeError', `${name} expected at most ${max} argument${max === 1 ? '' : 's'}, got ${n}`);
}

function toIndex(v: Value): bigint {
  if (typeof v === 'bigint') return v;
  if (typeof v === 'boolean') return v ? 1n : 0n;
  throw pyError('TypeError', `'${typeName(v)}' object cannot be interpreted as an integer`);
}

function collect(v: Value, ctx: BuiltinContext): Value[] {
  if (v !== null && typeof v === 'object' && v.kind === 'range' && rangeLength(v) > BigInt(MAX_LIST)) {
    throw new ExecutionLimit('size', null);
  }
  const out: Value[] = [];
  for (const item of iterate(v)) {
    ctx.tick();
    out.push(item);
  }
  return out;
}


// --- sayı dönüşümleri ---

function parseIntText(text: string, base: number): bigint | null {
  let s = text.trim();
  let sign = 1n;
  if (s[0] === '+' || s[0] === '-') {
    if (s[0] === '-') sign = -1n;
    s = s.slice(1);
  }
  const prefixes: Record<number, RegExp> = { 16: /^0[xX]_?/, 8: /^0[oO]_?/, 2: /^0[bB]_?/ };
  if (prefixes[base]) s = s.replace(prefixes[base], '');
  if (!/^[0-9a-zA-Z](_?[0-9a-zA-Z])*$/.test(s)) return null;
  let value = 0n;
  const b = BigInt(base);
  for (const ch of s.replace(/_/g, '')) {
    const digit = parseInt(ch, 36);
    if (Number.isNaN(digit) || digit >= base) return null;
    value = value * b + BigInt(digit);
  }
  return sign * value;
}

function intFromFloat(x: number): bigint {
  if (Number.isNaN(x)) throw pyError('ValueError', 'cannot convert float NaN to integer');
  if (!Number.isFinite(x)) throw pyError('OverflowError', 'cannot convert float infinity to integer');
  return BigInt(Math.trunc(x));
}

function parseFloatText(text: string): number | null {
  const s = text.trim();
  const special = /^([+-]?)(inf|infinity|nan)$/i.exec(s);
  if (special) {
    const v = special[2].toLowerCase() === 'nan' ? NaN : Infinity;
    return special[1] === '-' ? -v : v;
  }
  const D = '[0-9](?:_?[0-9])*';
  if (!new RegExp(`^[+-]?(?:${D}(?:\\.(?:${D})?)?|\\.${D})(?:[eE][+-]?${D})?$`).test(s)) return null;
  return Number(s.replace(/_/g, ''));
}

/** Bir double'ın tam ondalık değeri: |x| = digits × 10^exp */
function exactDecimal(x: number): { digits: bigint; exp: number } {
  const view = new DataView(new ArrayBuffer(8));
  view.setFloat64(0, Math.abs(x));
  const hi = view.getUint32(0);
  const lo = view.getUint32(4);
  const biased = (hi >>> 20) & 0x7ff;
  let mantissa = (BigInt(hi & 0xfffff) << 32n) | BigInt(lo);
  let e2: number;
  if (biased === 0) e2 = -1074;
  else {
    mantissa |= 1n << 52n;
    e2 = biased - 1075;
  }
  if (e2 >= 0) return { digits: mantissa << BigInt(e2), exp: 0 };
  return { digits: mantissa * 5n ** BigInt(-e2), exp: e2 };
}

/** Yarımda çifte yuvarlayan tam sayı bölmesi */
function divRoundHalfEven(n: bigint, d: bigint): bigint {
  let q = n / d;
  const r2 = (n % d) * 2n;
  if (r2 > d || (r2 === d && q % 2n !== 0n)) q += 1n;
  return q;
}

function roundFloat(x: number, ndigits: bigint | null): Value {
  if (ndigits === null) {
    if (Number.isNaN(x)) throw pyError('ValueError', 'cannot convert float NaN to integer');
    if (!Number.isFinite(x)) throw pyError('OverflowError', 'cannot convert float infinity to integer');
    const f = Math.floor(x);
    const d = x - f;
    return BigInt(d > 0.5 || (d === 0.5 && f % 2 !== 0) ? f + 1 : f);
  }
  if (!Number.isFinite(x) || x === 0) return x;
  const n = Number(ndigits);
  if (n > 400) return x;
  if (n < -400) return x < 0 ? -0 : 0;
  const { digits, exp } = exactDecimal(x);
  const shift = exp + n;
  if (shift >= 0) return x;
  const q = divRoundHalfEven(digits, 10n ** BigInt(-shift));
  const value = parseFloat(`${q}e${-n}`);
  return x < 0 ? -value : value;
}

function roundInt(x: bigint, ndigits: bigint | null): bigint {
  if (ndigits === null || ndigits >= 0n) return x;
  const pow = 10n ** -ndigits;
  const negative = x < 0n;
  const q = divRoundHalfEven(negative ? -x : x, pow) * pow;
  return negative ? -q : q;
}

// --- türler ---

function makeTypes(ctx: BuiltinContext): Record<string, PyType> {
  const types: Record<string, PyType> = {};
  const def = (name: PyType['name'], call?: Fn) => (types[name] = { kind: 'type', name, call });

  def('int', (args, kwargs) => {
    const base = kwargs.has('base') ? kwargs.get('base')! : args[1];
    if (args.length > 2) throw pyError('TypeError', `int() takes at most 2 arguments (${args.length} given)`);
    if (args.length === 0) return 0n;
    const x = args[0];
    if (base !== undefined) {
      if (typeof x !== 'string') throw pyError('TypeError', "int() can't convert non-string with explicit base");
      const b = Number(toIndex(base));
      const v = parseIntText(x, b);
      if (v === null) throw pyError('ValueError', `invalid literal for int() with base ${b}: ${repr(x)}`);
      return v;
    }
    if (typeof x === 'bigint') return x;
    if (typeof x === 'boolean') return x ? 1n : 0n;
    if (typeof x === 'number') return intFromFloat(x);
    if (typeof x === 'string') {
      const v = parseIntText(x, 10);
      if (v === null) throw pyError('ValueError', `invalid literal for int() with base 10: ${repr(x)}`);
      return v;
    }
    throw pyError('TypeError', `int() argument must be a string, a bytes-like object or a real number, not '${typeName(x)}'`);
  });

  def('float', (args, kwargs) => {
    noKwargs('float', kwargs);
    argCount('float', args, 0, 1);
    if (args.length === 0) return 0;
    const x = args[0];
    const n = asNumber(x);
    if (typeof n === 'number') return n;
    if (typeof n === 'bigint') {
      const f = Number(n);
      if (!Number.isFinite(f)) throw pyError('OverflowError', 'int too large to convert to float');
      return f;
    }
    if (typeof x === 'string') {
      const v = parseFloatText(x);
      if (v === null) throw pyError('ValueError', `could not convert string to float: ${repr(x)}`);
      return v;
    }
    throw pyError('TypeError', `float() argument must be a string or a real number, not '${typeName(x)}'`);
  });

  def('str', (args, kwargs) => {
    noKwargs('str', kwargs);
    argCount('str', args, 0, 1);
    return args.length === 0 ? '' : str(args[0]);
  });

  def('bool', (args, kwargs) => {
    noKwargs('bool', kwargs);
    argCount('bool', args, 0, 1);
    return args.length === 0 ? false : truthy(args[0]);
  });

  def('list', (args, kwargs) => {
    noKwargs('list', kwargs);
    argCount('list', args, 0, 1);
    return pyList(args.length === 0 ? [] : collect(args[0], ctx));
  });

  def('tuple', (args, kwargs) => {
    noKwargs('tuple', kwargs);
    argCount('tuple', args, 0, 1);
    return pyTuple(args.length === 0 ? [] : collect(args[0], ctx));
  });

  def('range', (args, kwargs) => {
    noKwargs('range', kwargs);
    if (args.length === 0) throw pyError('TypeError', 'range expected at least 1 argument, got 0');
    if (args.length > 3) throw pyError('TypeError', `range expected at most 3 arguments, got ${args.length}`);
    const [a, b, c] = args.map(toIndex);
    if (args.length === 1) return { kind: 'range', start: 0n, stop: a, step: 1n };
    if (c === 0n) throw pyError('ValueError', 'range() arg 3 must not be zero');
    return { kind: 'range', start: a, stop: b, step: c ?? 1n };
  });

  def('type', (args) => {
    if (args.length !== 1) throw new UnsupportedFeature('class', null);
    return typeOf(args[0]);
  });

  def('NoneType');
  def('function');
  def('builtin_function_or_method');

  function typeOf(v: Value): PyType {
    return types[typeName(v)];
  }
  return types;
}

// --- yerleşik fonksiyonlar ---

export function createBuiltins(ctx: BuiltinContext): Map<string, Value> {
  const types = makeTypes(ctx);
  const fns: Record<string, Fn> = {};

  fns.print = (args, kwargs) => {
    const option = (name: 'sep' | 'end', fallback: string): string => {
      const v = kwargs.get(name);
      if (v === undefined || v === null) return fallback;
      if (typeof v !== 'string') throw pyError('TypeError', `${name} must be None or a string, not ${typeName(v)}`);
      return v;
    };
    for (const key of kwargs.keys()) {
      if (key === 'file') throw new UnsupportedFeature('builtin', null, 'print(file=...)');
      if (key !== 'sep' && key !== 'end' && key !== 'flush') {
        throw pyError('TypeError', `'${key}' is an invalid keyword argument for print()`);
      }
    }
    ctx.write(args.map(str).join(option('sep', ' ')) + option('end', '\n'));
    return null;
  };

  fns.len = (args, kwargs) => {
    noKwargs('len', kwargs);
    argCount('len', args, 1, 1);
    return length(args[0]);
  };

  fns.repr = (args, kwargs) => {
    noKwargs('repr', kwargs);
    argCount('repr', args, 1, 1);
    return repr(args[0]);
  };

  fns.abs = (args, kwargs) => {
    noKwargs('abs', kwargs);
    argCount('abs', args, 1, 1);
    const n = asNumber(args[0]);
    if (n === null) throw pyError('TypeError', `bad operand type for abs(): '${typeName(args[0])}'`);
    return n < 0 ? -n : typeof n === 'number' ? Math.abs(n) : n;
  };

  fns.round = (args, kwargs) => {
    const allArgs = [...args];
    if (kwargs.has('ndigits')) allArgs[1] = kwargs.get('ndigits')!;
    if (allArgs.length === 0) throw pyError('TypeError', "round() missing required argument 'number' (pos 1)");
    const ndigits = allArgs[1] === undefined || allArgs[1] === null ? null : toIndex(allArgs[1]);
    const x = asNumber(allArgs[0]);
    if (typeof x === 'number') return roundFloat(x, ndigits);
    if (typeof x === 'bigint') return roundInt(x, ndigits);
    throw pyError('TypeError', `type ${typeName(allArgs[0])} doesn't define __round__ method`);
  };

  const minMax = (name: 'min' | 'max'): Fn => (args, kwargs) => {
    for (const key of kwargs.keys()) {
      if (key === 'key') throw new UnsupportedFeature('builtin', null, `${name}(key=...)`);
      if (key !== 'default') throw pyError('TypeError', `${name}() got an unexpected keyword argument '${key}'`);
    }
    if (args.length === 0) throw pyError('TypeError', `${name} expected at least 1 argument, got 0`);
    let items: Value[];
    if (args.length === 1) {
      items = collect(args[0], ctx);
      if (items.length === 0) {
        if (kwargs.has('default')) return kwargs.get('default')!;
        throw pyError('ValueError', `${name}() iterable argument is empty`);
      }
    } else {
      if (kwargs.has('default')) {
        throw pyError('TypeError', `Cannot specify a default for ${name}() with multiple positional arguments`);
      }
      items = args;
    }
    let best = items[0];
    for (const item of items.slice(1)) {
      if (orderCompare(name === 'min' ? '<' : '>', item, best)) best = item;
    }
    return best;
  };
  fns.min = minMax('min');
  fns.max = minMax('max');

  // CPython 3.12 builtin_sum: tam sayılar kesin, ondalıklar Neumaier düzeltmesiyle toplanır.
  fns.sum = (args, kwargs) => {
    const allArgs = [...args];
    if (kwargs.has('start')) allArgs[1] = kwargs.get('start')!;
    if (allArgs.length === 0) throw pyError('TypeError', 'sum() takes at least 1 positional argument (0 given)');
    let result: Value = allArgs[1] ?? 0n;
    if (typeof result === 'string') throw pyError('TypeError', "sum() can't sum strings [use ''.join(seq) instead]");
    const items = iterate(allArgs[0]);
    let floatSum: number | null = null;
    let compensation = 0;
    for (const item of items) {
      ctx.tick();
      if (floatSum === null && typeof result === 'number') floatSum = result;
      if (floatSum !== null) {
        if (typeof item === 'number') {
          const t: number = floatSum + item;
          if (Math.abs(floatSum) >= Math.abs(item)) compensation += floatSum - t + item;
          else compensation += item - t + floatSum;
          floatSum = t;
          continue;
        }
        if (typeof item === 'bigint' || typeof item === 'boolean') {
          const n = Number(asNumber(item));
          if (Number.isFinite(n)) {
            floatSum += n;
            continue;
          }
        }
        if (compensation && Number.isFinite(compensation)) floatSum += compensation;
        result = floatSum;
        floatSum = null;
        compensation = 0;
      }
      result = binaryOp('+', result, item);
    }
    if (floatSum !== null) {
      if (compensation && Number.isFinite(compensation)) floatSum += compensation;
      return floatSum;
    }
    return result;
  };

  fns.sorted = (args, kwargs) => {
    argCount('sorted', args, 1, 1);
    for (const key of kwargs.keys()) {
      if (key === 'key') throw new UnsupportedFeature('builtin', null, 'sorted(key=...)');
      if (key !== 'reverse') throw pyError('TypeError', `sort() got an unexpected keyword argument '${key}'`);
    }
    const items = collect(args[0], ctx);
    sortItems(items, truthy(kwargs.get('reverse') ?? false));
    return pyList(items);
  };

  fns.isinstance = (args, kwargs) => {
    noKwargs('isinstance', kwargs);
    argCount('isinstance', args, 2, 2);
    const [value, spec] = args;
    const classes = spec !== null && typeof spec === 'object' && spec.kind === 'tuple' ? spec.items : [spec];
    return classes.some((cls) => {
      if (cls === null || typeof cls !== 'object' || cls.kind !== 'type') {
        throw pyError('TypeError', 'isinstance() arg 2 must be a type, a tuple of types, or a union');
      }
      const actual = typeName(value);
      return actual === cls.name || (cls.name === 'int' && actual === 'bool');
    });
  };

  const builtins = new Map<string, Value>();
  for (const name of BUILTIN_NAMES) {
    if (name in types) builtins.set(name, types[name]);
    else if (name in fns) builtins.set(name, { kind: 'builtin', name, call: fns[name] } satisfies PyBuiltin);
  }
  return builtins;
}

/** Yerleşik isim listesinde olup motorun desteklemediği isimler (input, open...) */
export function isKnownBuiltinName(name: string): boolean {
  return (BUILTIN_NAMES as readonly string[]).includes(name);
}

/** Python'un list.sort'u gibi: kararlı, sadece < kullanır; reverse=True kararlılığı korur. */
export function sortItems(items: Value[], reverse: boolean): void {
  if (reverse) items.reverse();
  items.sort((a, b) => (pyLess(b, a) ? 1 : pyLess(a, b) ? -1 : 0));
  if (reverse) items.reverse();
}

// --- metotlar ---

type Method = (self: never, args: Value[], kwargs: Kwargs, ctx: BuiltinContext) => Value;

const LIST_METHODS: Record<string, Method> = {
  append(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.append', kwargs);
    argCount('list.append', args, 1, 1);
    self.items.push(args[0]);
    return null;
  },
  extend(self: { items: Value[] }, args, kwargs, ctx) {
    noKwargs('list.extend', kwargs);
    argCount('list.extend', args, 1, 1);
    self.items.push(...collect(args[0], ctx));
    return null;
  },
  insert(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.insert', kwargs);
    if (args.length !== 2) throw pyError('TypeError', `insert expected 2 arguments, got ${args.length}`);
    const n = BigInt(self.items.length);
    let i = toIndex(args[0]);
    if (i < 0n) i = i + n < 0n ? 0n : i + n;
    if (i > n) i = n;
    self.items.splice(Number(i), 0, args[1]);
    return null;
  },
  pop(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.pop', kwargs);
    argCount('pop', args, 0, 1);
    if (self.items.length === 0) throw pyError('IndexError', 'pop from empty list');
    let i = args.length ? toIndex(args[0]) : -1n;
    if (i < 0n) i += BigInt(self.items.length);
    if (i < 0n || i >= BigInt(self.items.length)) throw pyError('IndexError', 'pop index out of range');
    return self.items.splice(Number(i), 1)[0];
  },
  remove(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.remove', kwargs);
    argCount('list.remove', args, 1, 1);
    const i = self.items.findIndex((x) => pyEquals(x, args[0]));
    if (i < 0) throw pyError('ValueError', 'list.remove(x): x not in list');
    self.items.splice(i, 1);
    return null;
  },
  index(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.index', kwargs);
    argCount('index', args, 1, 3);
    const i = self.items.findIndex((x) => pyEquals(x, args[0]));
    if (i < 0) throw pyError('ValueError', `${repr(args[0])} is not in list`);
    return BigInt(i);
  },
  count(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.count', kwargs);
    argCount('list.count', args, 1, 1);
    return BigInt(self.items.filter((x) => pyEquals(x, args[0])).length);
  },
  sort(self: { items: Value[] }, args, kwargs) {
    if (args.length) throw pyError('TypeError', 'sort() takes no positional arguments');
    for (const key of kwargs.keys()) {
      if (key === 'key') throw new UnsupportedFeature('method', null, 'list.sort(key=...)');
      if (key !== 'reverse') throw pyError('TypeError', `sort() got an unexpected keyword argument '${key}'`);
    }
    sortItems(self.items, truthy(kwargs.get('reverse') ?? false));
    return null;
  },
  reverse(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.reverse', kwargs);
    argCount('list.reverse', args, 0, 0);
    self.items.reverse();
    return null;
  },
  clear(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.clear', kwargs);
    argCount('list.clear', args, 0, 0);
    self.items.length = 0;
    return null;
  },
  copy(self: { items: Value[] }, args, kwargs) {
    noKwargs('list.copy', kwargs);
    argCount('list.copy', args, 0, 0);
    return pyList([...self.items]);
  },
};

const TUPLE_METHODS: Record<string, Method> = {
  index(self: { items: Value[] }, args) {
    argCount('index', args, 1, 3);
    const i = self.items.findIndex((x) => pyEquals(x, args[0]));
    if (i < 0) throw pyError('ValueError', 'tuple.index(x): x not in tuple');
    return BigInt(i);
  },
  count(self: { items: Value[] }, args) {
    argCount('tuple.count', args, 1, 1);
    return BigInt(self.items.filter((x) => pyEquals(x, args[0])).length);
  },
};

function strArg(method: string, v: Value, position = 1): string {
  if (typeof v !== 'string') {
    throw pyError('TypeError', `${method}() argument ${position} must be str, not ${typeName(v)}`);
  }
  return v;
}

function stripChars(self: string, args: Value[], where: 'both' | 'left' | 'right'): string {
  const chars = args[0];
  if (chars === undefined || chars === null) {
    const s = where === 'right' ? self : self.replace(/^[\s\u001c-\u001f\u0085]+/, '');
    return where === 'left' ? s : s.replace(/[\s\u001c-\u001f\u0085]+$/, '');
  }
  const set = new Set(strChars(strArg('strip', chars)));
  const cs = strChars(self);
  let start = 0;
  let end = cs.length;
  if (where !== 'right') while (start < end && set.has(cs[start])) start++;
  if (where !== 'left') while (end > start && set.has(cs[end - 1])) end--;
  return cs.slice(start, end).join('');
}

const STR_METHODS: Record<string, Method> = {
  upper: (self: string, args) => (argCount('str.upper', args, 0, 0), self.toUpperCase()),
  lower: (self: string, args) => (argCount('str.lower', args, 0, 0), self.toLowerCase()),
  capitalize: (self: string, args) => {
    argCount('str.capitalize', args, 0, 0);
    const cs = strChars(self);
    return cs.length ? cs[0].toUpperCase() + cs.slice(1).join('').toLowerCase() : '';
  },
  strip: (self: string, args) => (argCount('strip', args, 0, 1), stripChars(self, args, 'both')),
  lstrip: (self: string, args) => (argCount('lstrip', args, 0, 1), stripChars(self, args, 'left')),
  rstrip: (self: string, args) => (argCount('rstrip', args, 0, 1), stripChars(self, args, 'right')),
  split(self: string, args, kwargs) {
    const sep = kwargs.has('sep') ? kwargs.get('sep')! : args[0] ?? null;
    const maxsplitValue = kwargs.has('maxsplit') ? kwargs.get('maxsplit')! : args[1] ?? -1n;
    const maxsplit = Number(toIndex(maxsplitValue));
    if (sep === null) {
      const parts: string[] = [];
      let rest = self.replace(/^\s+/, '');
      while (rest.length > 0) {
        if (maxsplit >= 0 && parts.length === maxsplit) {
          parts.push(rest.replace(/\s+$/, ''));
          break;
        }
        const m = /\s+/.exec(rest);
        if (!m) {
          parts.push(rest);
          break;
        }
        parts.push(rest.slice(0, m.index));
        rest = rest.slice(m.index + m[0].length);
      }
      return pyList(parts);
    }
    const s = strArg('split', sep);
    if (s === '') throw pyError('ValueError', 'empty separator');
    const pieces = self.split(s);
    if (maxsplit >= 0 && pieces.length > maxsplit + 1) {
      return pyList([...pieces.slice(0, maxsplit), pieces.slice(maxsplit).join(s)]);
    }
    return pyList(pieces);
  },
  join(self: string, args, kwargs, ctx) {
    argCount('str.join', args, 1, 1);
    const items = collect(args[0], ctx);
    items.forEach((item, i) => {
      if (typeof item !== 'string') {
        throw pyError('TypeError', `sequence item ${i}: expected str instance, ${typeName(item)} found`);
      }
    });
    return (items as string[]).join(self);
  },
  replace(self: string, args) {
    argCount('replace', args, 2, 3);
    const old = strArg('replace', args[0], 1);
    const repl = strArg('replace', args[1], 2);
    let count = args[2] === undefined ? -1 : Number(toIndex(args[2]));
    if (count < 0) return self.split(old).join(repl);
    let out = '';
    let rest = self;
    while (count-- > 0) {
      const i = rest.indexOf(old);
      if (i < 0) break;
      out += rest.slice(0, i) + repl;
      rest = rest.slice(i + old.length);
      if (old === '') {
        out += rest.slice(0, 1);
        rest = rest.slice(1);
      }
    }
    return out + rest;
  },
  startswith: (self: string, args) => affix(self, args, 'startswith'),
  endswith: (self: string, args) => affix(self, args, 'endswith'),
  find(self: string, args) {
    argCount('find', args, 1, 3);
    return BigInt(self.indexOf(strArg('find', args[0])));
  },
  index(self: string, args) {
    argCount('index', args, 1, 3);
    const i = self.indexOf(strArg('index', args[0]));
    if (i < 0) throw pyError('ValueError', 'substring not found');
    return BigInt(i);
  },
  count(self: string, args) {
    argCount('count', args, 1, 3);
    const sub = strArg('count', args[0]);
    if (sub === '') return BigInt(strChars(self).length + 1);
    return BigInt(self.split(sub).length - 1);
  },
  isdigit: (self: string, args) => (argCount('str.isdigit', args, 0, 0), /^\p{Nd}+$/u.test(self)),
  isalpha: (self: string, args) => (argCount('str.isalpha', args, 0, 0), /^\p{L}+$/u.test(self)),
  isupper: (self: string) => self !== self.toLowerCase() && self === self.toUpperCase(),
  islower: (self: string) => self !== self.toUpperCase() && self === self.toLowerCase(),
};

function affix(self: string, args: Value[], method: 'startswith' | 'endswith'): boolean {
  argCount(method, args, 1, 3);
  const spec = args[0];
  const options = spec !== null && typeof spec === 'object' && spec.kind === 'tuple' ? spec.items : [spec];
  return options.some((option) => {
    if (typeof option !== 'string') {
      throw pyError('TypeError', `${method} first arg must be str or a tuple of str, not ${typeName(option)}`);
    }
    return method === 'startswith' ? self.startsWith(option) : self.endsWith(option);
  });
}

const METHODS: Record<string, Record<string, Method>> = {
  list: LIST_METHODS,
  tuple: TUPLE_METHODS,
  str: STR_METHODS,
};

/** değer.isim — metot ya da AttributeError */
export function getAttribute(value: Value, name: string, ctx: BuiltinContext): Value {
  const type = typeName(value);
  const method = METHODS[type]?.[name];
  if (method && Object.prototype.hasOwnProperty.call(METHODS[type], name)) {
    return {
      kind: 'method',
      self: value,
      name,
      call: (args, kwargs) => method(value as never, args, kwargs, ctx),
    };
  }
  const dir = (TYPE_DIR as Record<string, readonly string[]>)[type];
  if (dir?.includes(name)) throw new UnsupportedFeature('method', null, `${type}.${name}`);
  let message = `'${type}' object has no attribute '${name}'`;
  const suggestion = dir ? calculateSuggestion(dir, name) : null;
  if (suggestion) message += `. Did you mean: '${suggestion}'?`;
  throw pyError('AttributeError', message);
}
