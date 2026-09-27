// Çalıştırıcı: derleyicinin ürettiği komutları tek bir döngüde işler.
//
// Her cümleden önce durak (Step) verir: adım adım modunda oyun burada bekler, normal
// çalıştırmada duraklar hemen geçilir. Fonksiyon çağrıları JavaScript yığınını kullanmaz;
// çağrı çerçeveleri kendi listemizde tutulur (Python'un 1000 derinlik sınırı telefonda da geçerli).

import type * as A from './ast';
import { createBuiltins, getAttribute, isKnownBuiltinName, type BuiltinContext } from './builtins';
import { compileFunction, compileModule, type Code } from './compiler';
import { ExecutionLimit, PythonException, UnsupportedFeature } from './errors';
import {
  binaryOp,
  compare,
  getItem,
  inplaceOp,
  iterate,
  setItem,
  unaryOp,
  type SliceValue,
} from './operators';
import { BUILTIN_NAMES, MODULE_GLOBAL_NAMES, TYPE_DIR } from './python-names';
import { nameErrorMessage } from './suggestions';
import { pyError, pyList, pyTuple, truthy, typeName, type Kwargs, type PyFunction, type Value } from './values';

/** Değişkenlerin yaşadığı yer: modül ya da bir fonksiyon çağrısı */
export class Frame {
  readonly vars = new Map<string, Value>();
  constructor(
    readonly fn: PyFunction | null,
    /** Tanımlandığı (dıştaki) fonksiyonun çerçevesi */
    readonly parent: Frame | null,
  ) {}
}

export interface Step {
  line: number;
  frame: Frame;
}

export interface InterpreterOptions {
  /** Bu kadar adımdan sonra durdurulur (bitmeyen döngüye karşı) */
  maxSteps: number;
  /** CPython'daki gibi en fazla iç içe çağrı (modül dahil) */
  recursionLimit: number;
  /** Oyunun verdiği komutlar (move, collect...) */
  externals?: Map<string, Value>;
}

/** Çalışmakta olan bir kod parçası: komutlar, sıradaki komut, ara değerler yığını */
interface Activation {
  code: Code;
  pc: number;
  stack: unknown[];
  frame: Frame;
}

/** Satırı olmayan hataya, çıktığı komutun satırını yazar. */
function fillLine(e: unknown, line: number): void {
  if ((e instanceof PythonException || e instanceof UnsupportedFeature || e instanceof ExecutionLimit) && e.line === null) {
    e.line = line;
  }
}

export class Interpreter {
  output = '';
  private steps = 0;
  private currentLine: number | null = null;
  readonly globals = new Frame(null, null);
  private readonly builtins: Map<string, Value>;
  private readonly builtinNames: string[];
  private readonly ctx: BuiltinContext;

  constructor(private readonly options: InterpreterOptions) {
    for (const name of MODULE_GLOBAL_NAMES) this.globals.vars.set(name, name === '__name__' ? '__main__' : null);
    this.ctx = {
      write: (text) => {
        this.output += text;
      },
      tick: () => this.tick(),
    };
    this.builtins = createBuiltins(this.ctx);
    for (const [name, value] of options.externals ?? []) this.builtins.set(name, value);
    this.builtinNames = [...BUILTIN_NAMES, ...(options.externals?.keys() ?? [])];
  }

  private tick(): void {
    if (++this.steps > this.options.maxSteps) throw new ExecutionLimit('steps', this.currentLine);
  }

  *run(module: A.Module): Generator<Step, void, void> {
    const calls: Activation[] = [{ code: compileModule(module), pc: 0, stack: [], frame: this.globals }];
    let line = 0;
    try {
      while (true) {
        const act = calls[calls.length - 1];
        const stack = act.stack;
        const ins = act.code.instrs[act.pc++];
        line = ins.line;

        switch (ins.op) {
          case 'STEP':
            this.currentLine = ins.line;
            this.tick();
            yield { line: ins.line, frame: act.frame };
            break;

          case 'CONST':
            stack.push(ins.value);
            break;
          case 'LOAD':
            stack.push(this.load(ins.name, act.frame));
            break;
          case 'STORE':
            this.store(ins.name, stack.pop() as Value, act.frame);
            break;
          case 'LOAD_ATTR':
            stack.push(getAttribute(stack.pop() as Value, ins.name, this.ctx));
            break;
          case 'STORE_ATTR':
            throw attributeAssignError(stack.pop() as Value, ins.name);

          case 'BINARY': {
            const right = stack.pop() as Value;
            stack.push(binaryOp(ins.binop, stack.pop() as Value, right));
            break;
          }
          case 'INPLACE': {
            const right = stack.pop() as Value;
            stack.push(inplaceOp(ins.binop, stack.pop() as Value, right));
            break;
          }
          case 'UNARY':
            stack.push(unaryOp(ins.unop, stack.pop() as Value));
            break;
          case 'COMPARE': {
            const right = stack.pop() as Value;
            stack.push(compare(ins.cmp, stack.pop() as Value, right));
            break;
          }

          case 'BUILD': {
            const items = stack.splice(stack.length - ins.count) as Value[];
            stack.push(ins.kind === 'list' ? pyList(items) : pyTuple(items));
            break;
          }
          case 'BUILD_SLICE': {
            const [lower, upper, step] = stack.splice(stack.length - 3) as Value[];
            stack.push({ lower, upper, step } satisfies SliceValue);
            break;
          }
          case 'SUBSCR': {
            const index = stack.pop() as Value | SliceValue;
            stack.push(getItem(stack.pop() as Value, index));
            break;
          }
          case 'STORE_SUBSCR': {
            const index = stack.pop() as Value | SliceValue;
            const container = stack.pop() as Value;
            setItem(container, index, stack.pop() as Value);
            break;
          }
          case 'UNPACK': {
            const items = unpack(stack.pop() as Value, ins.count);
            for (let i = items.length - 1; i >= 0; i--) stack.push(items[i]);
            break;
          }

          case 'POP':
            stack.pop();
            break;
          case 'DUP':
            stack.push(stack[stack.length - 1]);
            break;
          case 'DUP2':
            stack.push(stack[stack.length - 2], stack[stack.length - 1]);
            break;
          case 'ROT2': {
            const top = stack.pop();
            stack.splice(stack.length - 1, 0, top);
            break;
          }
          case 'ROT3': {
            const top = stack.pop();
            stack.splice(stack.length - 2, 0, top);
            break;
          }

          case 'JUMP':
            act.pc = ins.target;
            break;
          case 'JUMP_IF_FALSE':
            if (!truthy(stack.pop() as Value)) act.pc = ins.target;
            break;
          case 'JUMP_IF_FALSE_OR_POP':
            if (!truthy(stack[stack.length - 1] as Value)) act.pc = ins.target;
            else stack.pop();
            break;
          case 'JUMP_IF_TRUE_OR_POP':
            if (truthy(stack[stack.length - 1] as Value)) act.pc = ins.target;
            else stack.pop();
            break;

          case 'GET_ITER':
            stack.push(getIterator(stack.pop() as Value));
            break;
          case 'FOR_ITER': {
            const next = (stack[stack.length - 1] as Iterator<Value>).next();
            if (next.done) {
              stack.pop();
              act.pc = ins.target;
            } else {
              stack.push(next.value);
            }
            break;
          }

          case 'MAKE_FUNCTION': {
            const def = ins.def;
            const withDefaults = def.params.filter((p) => p.default);
            const values = stack.splice(stack.length - withDefaults.length) as Value[];
            const defaults = new Map(withDefaults.map((p, i) => [p.name, values[i]]));
            const scope = scopeOf(def);
            const outer = act.frame.fn;
            stack.push({
              kind: 'function',
              def,
              qualname: outer ? `${outer.qualname}.<locals>.${def.name}` : def.name,
              defaults,
              closure: outer ? act.frame : null,
              localNames: scope.localNames,
              globalNames: scope.globalNames,
              nonlocalNames: scope.nonlocalNames,
            } satisfies PyFunction);
            break;
          }

          case 'CALL': {
            const kwvalues = stack.splice(stack.length - ins.kwnames.length) as Value[];
            const args = stack.splice(stack.length - ins.argc) as Value[];
            const func = stack.pop() as Value;
            const kwargs: Kwargs = new Map(ins.kwnames.map((name, i) => [name, kwvalues[i]]));
            if (func !== null && typeof func === 'object' && func.kind === 'function') {
              const frame = bindArguments(func, args, kwargs);
              if (calls.length >= this.options.recursionLimit) {
                throw pyError('RecursionError', 'maximum recursion depth exceeded');
              }
              calls.push({ code: compileFunction(func.def), pc: 0, stack: [], frame });
            } else {
              stack.push(this.callNative(func, args, kwargs));
            }
            break;
          }

          case 'RETURN': {
            const value = stack.pop();
            calls.pop();
            if (calls.length === 0) return;
            calls[calls.length - 1].stack.push(value);
            break;
          }

          case 'UNSUPPORTED':
            throw new UnsupportedFeature(ins.feature, ins.line, ins.detail);
        }
      }
    } catch (e) {
      fillLine(e, line);
      throw e;
    }
  }

  private callNative(func: Value, args: Value[], kwargs: Kwargs): Value {
    if (func !== null && typeof func === 'object') {
      if (func.kind === 'builtin' || func.kind === 'method') {
        this.tick();
        return func.call(args, kwargs);
      }
      if (func.kind === 'type') {
        if (!func.call) throw pyError('TypeError', `cannot create '${func.name}' instances`);
        this.tick();
        return func.call(args, kwargs);
      }
    }
    throw pyError('TypeError', `'${typeName(func)}' object is not callable`);
  }

  // --- isimler ---

  private load(name: string, frame: Frame): Value {
    const fn = frame.fn;
    if (fn && !fn.globalNames.has(name)) {
      if (scopeOf(fn.def).localSet.has(name)) {
        if (frame.vars.has(name)) return frame.vars.get(name)!;
        throw pyError('UnboundLocalError', `cannot access local variable '${name}' where it is not associated with a value`);
      }
      for (let p = frame.parent; p?.fn; p = p.parent) {
        if (p.fn.globalNames.has(name)) break;
        if (scopeOf(p.fn.def).localSet.has(name)) {
          if (p.vars.has(name)) return p.vars.get(name)!;
          throw pyError(
            'NameError',
            `cannot access free variable '${name}' where it is not associated with a value in enclosing scope`,
          );
        }
      }
    }
    if (this.globals.vars.has(name)) return this.globals.vars.get(name)!;
    if (this.builtins.has(name)) return this.builtins.get(name)!;
    if (isKnownBuiltinName(name)) throw new UnsupportedFeature('builtin', null, name);
    throw pyError(
      'NameError',
      nameErrorMessage(name, [fn ? fn.localNames : [], [...this.globals.vars.keys()], this.builtinNames]),
    );
  }

  private store(name: string, value: Value, frame: Frame): void {
    const fn = frame.fn;
    if (!fn || fn.globalNames.has(name)) {
      this.globals.vars.set(name, value);
      return;
    }
    if (fn.nonlocalNames.has(name)) {
      for (let p = frame.parent; p?.fn; p = p.parent) {
        if (scopeOf(p.fn.def).localSet.has(name)) {
          p.vars.set(name, value);
          return;
        }
      }
    }
    frame.vars.set(name, value);
  }
}

// --- çağrı ---

/** Argümanları parametrelere bağlar; hata mesajları CPython ile aynı. */
function bindArguments(fn: PyFunction, args: Value[], kwargs: Kwargs): Frame {
  const params = fn.def.params;
  const name = fn.qualname;
  if (args.length > params.length) {
    const required = params.filter((p) => !p.default).length;
    const given = `${args.length} ${args.length === 1 ? 'was' : 'were'} given`;
    if (required === params.length) {
      const s = params.length === 1 ? '' : 's';
      throw pyError('TypeError', `${name}() takes ${params.length} positional argument${s} but ${given}`);
    }
    throw pyError('TypeError', `${name}() takes from ${required} to ${params.length} positional arguments but ${given}`);
  }

  const frame = new Frame(fn, fn.closure as Frame | null);
  args.forEach((value, i) => frame.vars.set(params[i].name, value));
  for (const [key, value] of kwargs) {
    if (!params.some((p) => p.name === key)) {
      throw pyError('TypeError', `${name}() got an unexpected keyword argument '${key}'`);
    }
    if (frame.vars.has(key)) throw pyError('TypeError', `${name}() got multiple values for argument '${key}'`);
    frame.vars.set(key, value);
  }
  const missing = params.filter((p) => !frame.vars.has(p.name) && !fn.defaults.has(p.name)).map((p) => `'${p.name}'`);
  if (missing.length > 0) {
    const list =
      missing.length === 1
        ? missing[0]
        : missing.length === 2
          ? `${missing[0]} and ${missing[1]}`
          : `${missing.slice(0, -1).join(', ')}, and ${missing[missing.length - 1]}`;
    const s = missing.length === 1 ? '' : 's';
    throw pyError('TypeError', `${name}() missing ${missing.length} required positional argument${s}: ${list}`);
  }
  for (const [key, value] of fn.defaults) if (!frame.vars.has(key)) frame.vars.set(key, value);
  return frame;
}

// --- yardımcılar ---

function getIterator(v: Value): Iterator<Value> {
  const iterable =
    typeof v === 'string' || (v !== null && typeof v === 'object' && (v.kind === 'list' || v.kind === 'tuple' || v.kind === 'range'));
  if (!iterable) throw pyError('TypeError', `'${typeName(v)}' object is not iterable`);
  return iterate(v);
}

function unpack(value: Value, expected: number): Value[] {
  let items: Value[];
  try {
    items = [...iterate(value)];
  } catch (e) {
    if (e instanceof PythonException && e.pyType === 'TypeError') {
      throw pyError('TypeError', `cannot unpack non-iterable ${typeName(value)} object`);
    }
    throw e;
  }
  if (items.length > expected) throw pyError('ValueError', `too many values to unpack (expected ${expected})`);
  if (items.length < expected) {
    throw pyError('ValueError', `not enough values to unpack (expected ${expected}, got ${items.length})`);
  }
  return items;
}

function attributeAssignError(obj: Value, attr: string): PythonException {
  const type = typeName(obj);
  const dir = (TYPE_DIR as Record<string, readonly string[]>)[type];
  if (dir?.includes(attr)) return pyError('AttributeError', `'${type}' object attribute '${attr}' is read-only`);
  return pyError('AttributeError', `'${type}' object has no attribute '${attr}'`);
}

// --- kapsam (hangi isim yerel, hangisi global) ---

interface ScopeInfo {
  localNames: string[];
  localSet: Set<string>;
  globalNames: Set<string>;
  nonlocalNames: Set<string>;
}

const scopeCache = new WeakMap<A.StmtOf<'FunctionDef'>, ScopeInfo>();

/**
 * Fonksiyonun yerel isimleri: parametreler + gövdede atanan isimler (global/nonlocal hariç).
 * Sıra CPython'un co_varnames sırasına yakındır: parametreler, sonra ilk geçtikleri sıra.
 * ("Did you mean" önerisinde eşit yakınlıktaki isimlerden hangisinin seçileceğini bu sıra belirler.)
 */
function scopeOf(def: A.StmtOf<'FunctionDef'>): ScopeInfo {
  const cached = scopeCache.get(def);
  if (cached) return cached;

  const bound = new Set<string>();
  const globalNames = new Set<string>();
  const nonlocalNames = new Set<string>();

  const bindTarget = (t: A.Expr) => {
    if (t.kind === 'Name') bound.add(t.id);
    else if (t.kind === 'Tuple' || t.kind === 'List') t.elts.forEach(bindTarget);
  };
  const collectBindings = (body: A.Stmt[]) => {
    for (const s of body) {
      switch (s.kind) {
        case 'Assign':
          s.targets.forEach(bindTarget);
          break;
        case 'AugAssign':
          bindTarget(s.target);
          break;
        case 'For':
          bindTarget(s.target);
          collectBindings(s.body);
          collectBindings(s.orelse);
          break;
        case 'If':
        case 'While':
          collectBindings(s.body);
          collectBindings(s.orelse);
          break;
        case 'FunctionDef':
          bound.add(s.name);
          break;
        case 'Global':
          s.names.forEach((n) => globalNames.add(n));
          break;
        case 'Nonlocal':
          s.names.forEach((n) => nonlocalNames.add(n));
          break;
      }
    }
  };
  collectBindings(def.body);

  const params = def.params.map((p) => p.name);
  const isLocal = (n: string) => params.includes(n) || (bound.has(n) && !globalNames.has(n) && !nonlocalNames.has(n));
  const order: string[] = [...params];
  const note = (n: string) => {
    if (!order.includes(n) && isLocal(n)) order.push(n);
  };
  const visitExpr = (e: A.Expr) => {
    if (e.kind === 'Name') note(e.id);
    forEachChild(e, visitExpr);
  };
  const visitBody = (body: A.Stmt[]) => {
    for (const s of body) {
      switch (s.kind) {
        case 'Expr':
          visitExpr(s.value);
          break;
        case 'Assign':
          visitExpr(s.value);
          s.targets.forEach(visitExpr);
          break;
        case 'AugAssign':
          visitExpr(s.target);
          visitExpr(s.value);
          break;
        case 'If':
        case 'While':
          visitExpr(s.test);
          visitBody(s.body);
          visitBody(s.orelse);
          break;
        case 'For':
          visitExpr(s.iter);
          visitExpr(s.target);
          visitBody(s.body);
          visitBody(s.orelse);
          break;
        case 'FunctionDef':
          s.params.forEach((p) => p.default && visitExpr(p.default));
          note(s.name);
          break;
        case 'Return':
          if (s.value) visitExpr(s.value);
          break;
      }
    }
  };
  visitBody(def.body);

  const info: ScopeInfo = { localNames: order, localSet: new Set(order), globalNames, nonlocalNames };
  scopeCache.set(def, info);
  return info;
}

function forEachChild(e: A.Expr, fn: (child: A.Expr) => void): void {
  switch (e.kind) {
    case 'BinOp':
      fn(e.left);
      fn(e.right);
      break;
    case 'UnaryOp':
      fn(e.operand);
      break;
    case 'BoolOp':
      e.values.forEach(fn);
      break;
    case 'Compare':
      fn(e.left);
      e.comparators.forEach(fn);
      break;
    case 'IfExp':
      fn(e.test);
      fn(e.body);
      fn(e.orelse);
      break;
    case 'Call':
      fn(e.func);
      e.args.forEach(fn);
      e.keywords.forEach((k) => fn(k.value));
      break;
    case 'Attribute':
      fn(e.value);
      break;
    case 'Subscript':
      fn(e.value);
      fn(e.index);
      break;
    case 'Slice':
      [e.lower, e.upper, e.step].forEach((x) => x && fn(x));
      break;
    case 'List':
    case 'Tuple':
    case 'Set':
      e.elts.forEach(fn);
      break;
    case 'Dict':
      e.keys.forEach(fn);
      e.values.forEach(fn);
      break;
  }
}
