// Derleyici: yapı ağacını (AST) basit komutlar listesine çevirir (CPython'un bytecode'u gibi).
//
// Neden: Çalıştırıcı bu listeyi tek bir döngüde işler. JavaScript yığını büyümez; böylece
// iç içe 1000 fonksiyon çağrısı (Python'un sınırı) telefonda da çalışır ve adım adım mod kolaylaşır.
// Her komut, hatası çıkarsa bildirilecek satırı taşır (CPython'daki gibi komutun kendi satırı).

import type * as A from './ast';
import type { UnsupportedFeatureId } from './errors';
import type { Value } from './values';

export type Instr = { line: number } & (
  | { op: 'STEP' }
  | { op: 'CONST'; value: Value }
  | { op: 'LOAD'; name: string }
  | { op: 'STORE'; name: string }
  | { op: 'LOAD_ATTR'; name: string }
  | { op: 'STORE_ATTR'; name: string }
  | { op: 'BINARY'; binop: A.BinOperator }
  | { op: 'INPLACE'; binop: A.BinOperator }
  | { op: 'UNARY'; unop: A.UnaryOperator }
  | { op: 'COMPARE'; cmp: A.CompareOperator }
  | { op: 'BUILD'; kind: 'list' | 'tuple'; count: number }
  | { op: 'BUILD_SLICE' }
  | { op: 'SUBSCR' }
  | { op: 'STORE_SUBSCR' }
  | { op: 'UNPACK'; count: number }
  | { op: 'CALL'; argc: number; kwnames: string[] }
  | { op: 'POP' }
  | { op: 'DUP' }
  | { op: 'DUP2' }
  | { op: 'ROT2' }
  | { op: 'ROT3' }
  | { op: 'JUMP'; target: number }
  | { op: 'JUMP_IF_FALSE'; target: number }
  | { op: 'JUMP_IF_FALSE_OR_POP'; target: number }
  | { op: 'JUMP_IF_TRUE_OR_POP'; target: number }
  | { op: 'GET_ITER' }
  | { op: 'FOR_ITER'; target: number }
  | { op: 'MAKE_FUNCTION'; def: A.StmtOf<'FunctionDef'> }
  | { op: 'RETURN' }
  | { op: 'UNSUPPORTED'; feature: UnsupportedFeatureId; detail?: string }
);

export interface Code {
  instrs: Instr[];
}

type Distribute<T> = T extends unknown ? Omit<T, 'line'> : never;
type InstrBody = Distribute<Instr>;

interface Loop {
  continueTarget: number;
  breakJumps: number[];
  /** for döngüsünde yığında yineleyici durur; break öncesi atılmalı */
  isFor: boolean;
}

class Compiler {
  readonly instrs: Instr[] = [];
  private readonly loops: Loop[] = [];

  emit(body: InstrBody, line: number): number {
    this.instrs.push({ ...body, line } as Instr);
    return this.instrs.length - 1;
  }

  get here(): number {
    return this.instrs.length;
  }

  patch(index: number, target: number): void {
    (this.instrs[index] as { target: number }).target = target;
  }

  // --- cümleler ---

  body(stmts: A.Stmt[]): void {
    for (const s of stmts) this.stmt(s);
  }

  stmt(s: A.Stmt): void {
    this.emit({ op: 'STEP' }, s.line);
    switch (s.kind) {
      case 'Expr':
        this.expr(s.value);
        this.emit({ op: 'POP' }, s.line);
        return;

      case 'Assign':
        this.expr(s.value);
        s.targets.forEach((target, i) => {
          if (i < s.targets.length - 1) this.emit({ op: 'DUP' }, s.line);
          this.store(target);
        });
        return;

      case 'AugAssign':
        this.augAssign(s);
        return;

      case 'If': {
        this.expr(s.test);
        const toElse = this.emit({ op: 'JUMP_IF_FALSE', target: -1 }, s.line);
        this.body(s.body);
        if (s.orelse.length === 0) {
          this.patch(toElse, this.here);
          return;
        }
        const toEnd = this.emit({ op: 'JUMP', target: -1 }, s.line);
        this.patch(toElse, this.here);
        this.body(s.orelse);
        this.patch(toEnd, this.here);
        return;
      }

      case 'While': {
        const toTest = this.emit({ op: 'JUMP', target: -1 }, s.line);
        const cont = this.emit({ op: 'STEP' }, s.line); // her yeni turda başlık satırı
        this.patch(toTest, this.here);
        this.expr(s.test);
        const toElse = this.emit({ op: 'JUMP_IF_FALSE', target: -1 }, s.line);
        const loop: Loop = { continueTarget: cont, breakJumps: [], isFor: false };
        this.loops.push(loop);
        this.body(s.body);
        this.loops.pop();
        this.emit({ op: 'JUMP', target: cont }, s.line);
        this.patch(toElse, this.here);
        this.body(s.orelse);
        loop.breakJumps.forEach((j) => this.patch(j, this.here));
        return;
      }

      case 'For': {
        this.expr(s.iter);
        this.emit({ op: 'GET_ITER' }, s.iter.line);
        const toNext = this.emit({ op: 'JUMP', target: -1 }, s.line);
        const cont = this.emit({ op: 'STEP' }, s.line);
        this.patch(toNext, this.here);
        const forIter = this.emit({ op: 'FOR_ITER', target: -1 }, s.line);
        this.store(s.target);
        const loop: Loop = { continueTarget: cont, breakJumps: [], isFor: true };
        this.loops.push(loop);
        this.body(s.body);
        this.loops.pop();
        this.emit({ op: 'JUMP', target: cont }, s.line);
        this.patch(forIter, this.here);
        this.body(s.orelse);
        loop.breakJumps.forEach((j) => this.patch(j, this.here));
        return;
      }

      case 'Break': {
        const loop = this.loops[this.loops.length - 1];
        if (loop.isFor) this.emit({ op: 'POP' }, s.line);
        loop.breakJumps.push(this.emit({ op: 'JUMP', target: -1 }, s.line));
        return;
      }

      case 'Continue':
        this.emit({ op: 'JUMP', target: this.loops[this.loops.length - 1].continueTarget }, s.line);
        return;

      case 'FunctionDef':
        for (const p of s.params) if (p.default) this.expr(p.default);
        this.emit({ op: 'MAKE_FUNCTION', def: s }, s.line);
        this.emit({ op: 'STORE', name: s.name }, s.line);
        return;

      case 'Return':
        if (s.value) this.expr(s.value);
        else this.emit({ op: 'CONST', value: null }, s.line);
        this.emit({ op: 'RETURN' }, s.line);
        return;

      case 'Pass':
      case 'Global':
      case 'Nonlocal':
        return;
    }
  }

  private augAssign(s: A.StmtOf<'AugAssign'>): void {
    const t = s.target;
    if (t.kind === 'Name') {
      this.emit({ op: 'LOAD', name: t.id }, t.line);
      this.expr(s.value);
      this.emit({ op: 'INPLACE', binop: s.op }, s.line);
      this.emit({ op: 'STORE', name: t.id }, s.line);
    } else if (t.kind === 'Subscript') {
      this.expr(t.value);
      this.index(t.index);
      this.emit({ op: 'DUP2' }, t.line);
      this.emit({ op: 'SUBSCR' }, t.line);
      this.expr(s.value);
      this.emit({ op: 'INPLACE', binop: s.op }, s.line);
      this.emit({ op: 'ROT3' }, s.line);
      this.emit({ op: 'STORE_SUBSCR' }, t.line);
    } else if (t.kind === 'Attribute') {
      this.expr(t.value);
      this.emit({ op: 'DUP' }, t.line);
      this.emit({ op: 'LOAD_ATTR', name: t.attr }, t.line);
      this.expr(s.value);
      this.emit({ op: 'INPLACE', binop: s.op }, s.line);
      this.emit({ op: 'ROT2' }, s.line);
      this.emit({ op: 'STORE_ATTR', name: t.attr }, t.line);
    }
  }

  /** Yığının tepesindeki değeri hedefe yazar. */
  private store(target: A.Expr): void {
    switch (target.kind) {
      case 'Name':
        this.emit({ op: 'STORE', name: target.id }, target.line);
        return;
      case 'Tuple':
      case 'List':
        this.emit({ op: 'UNPACK', count: target.elts.length }, target.line);
        target.elts.forEach((elt) => this.store(elt));
        return;
      case 'Subscript':
        this.expr(target.value);
        this.index(target.index);
        this.emit({ op: 'STORE_SUBSCR' }, target.line);
        return;
      case 'Attribute':
        this.expr(target.value);
        this.emit({ op: 'STORE_ATTR', name: target.attr }, target.line);
        return;
    }
    throw new Error(`atanamaz hedef: ${target.kind}`);
  }

  // --- ifadeler ---

  expr(e: A.Expr): void {
    switch (e.kind) {
      case 'Constant': {
        const c = e.value;
        this.emit({ op: 'CONST', value: c.type === 'none' ? null : c.value }, e.line);
        return;
      }
      case 'Name':
        this.emit({ op: 'LOAD', name: e.id }, e.line);
        return;
      case 'BinOp':
        this.expr(e.left);
        this.expr(e.right);
        this.emit({ op: 'BINARY', binop: e.op }, e.line);
        return;
      case 'UnaryOp':
        this.expr(e.operand);
        this.emit({ op: 'UNARY', unop: e.op }, e.line);
        return;
      case 'BoolOp': {
        const jumps: number[] = [];
        e.values.forEach((value, i) => {
          this.expr(value);
          if (i < e.values.length - 1) {
            const op = e.op === 'and' ? 'JUMP_IF_FALSE_OR_POP' : 'JUMP_IF_TRUE_OR_POP';
            jumps.push(this.emit({ op, target: -1 }, e.line));
          }
        });
        jumps.forEach((j) => this.patch(j, this.here));
        return;
      }
      case 'Compare':
        this.compare(e);
        return;
      case 'IfExp': {
        this.expr(e.test);
        const toElse = this.emit({ op: 'JUMP_IF_FALSE', target: -1 }, e.line);
        this.expr(e.body);
        const toEnd = this.emit({ op: 'JUMP', target: -1 }, e.line);
        this.patch(toElse, this.here);
        this.expr(e.orelse);
        this.patch(toEnd, this.here);
        return;
      }
      case 'Call':
        this.expr(e.func);
        e.args.forEach((arg) => this.expr(arg));
        e.keywords.forEach((k) => this.expr(k.value));
        this.emit({ op: 'CALL', argc: e.args.length, kwnames: e.keywords.map((k) => k.name) }, e.line);
        return;
      case 'Attribute':
        this.expr(e.value);
        this.emit({ op: 'LOAD_ATTR', name: e.attr }, e.line);
        return;
      case 'Subscript':
        this.expr(e.value);
        this.index(e.index);
        this.emit({ op: 'SUBSCR' }, e.line);
        return;
      case 'List':
      case 'Tuple':
        e.elts.forEach((elt) => this.expr(elt));
        this.emit({ op: 'BUILD', kind: e.kind === 'List' ? 'list' : 'tuple', count: e.elts.length }, e.line);
        return;
      case 'Dict':
        this.emit({ op: 'UNSUPPORTED', feature: 'dict' }, e.line);
        return;
      case 'Set':
        this.emit({ op: 'UNSUPPORTED', feature: 'set' }, e.line);
        return;
      case 'Slice':
        this.emit({ op: 'UNSUPPORTED', feature: 'method', detail: 'slice' }, e.line);
        return;
    }
  }

  private index(index: A.Expr): void {
    if (index.kind === 'Slice') {
      for (const part of [index.lower, index.upper, index.step]) {
        if (part) this.expr(part);
        else this.emit({ op: 'CONST', value: null }, index.line);
      }
      this.emit({ op: 'BUILD_SLICE' }, index.line);
      return;
    }
    if (index.kind === 'Tuple' && index.elts.some((x) => x.kind === 'Slice')) {
      this.emit({ op: 'UNSUPPORTED', feature: 'method', detail: 'multi-slice' }, index.line);
      return;
    }
    this.expr(index);
  }

  /** a < b < c: CPython gibi ara değer bir kez hesaplanır, ilk yanlışta durulur. */
  private compare(e: A.ExprOf<'Compare'>): void {
    this.expr(e.left);
    if (e.ops.length === 1) {
      this.expr(e.comparators[0]);
      this.emit({ op: 'COMPARE', cmp: e.ops[0] }, e.line);
      return;
    }
    const cleanups: number[] = [];
    e.ops.forEach((op, i) => {
      this.expr(e.comparators[i]);
      if (i < e.ops.length - 1) {
        this.emit({ op: 'DUP' }, e.line);
        this.emit({ op: 'ROT3' }, e.line);
        this.emit({ op: 'COMPARE', cmp: op }, e.line);
        cleanups.push(this.emit({ op: 'JUMP_IF_FALSE_OR_POP', target: -1 }, e.line));
      } else {
        this.emit({ op: 'COMPARE', cmp: op }, e.line);
      }
    });
    const toEnd = this.emit({ op: 'JUMP', target: -1 }, e.line);
    cleanups.forEach((j) => this.patch(j, this.here));
    this.emit({ op: 'ROT2' }, e.line);
    this.emit({ op: 'POP' }, e.line);
    this.patch(toEnd, this.here);
  }
}

export function compileModule(module: A.Module): Code {
  const c = new Compiler();
  c.body(module.body);
  c.emit({ op: 'CONST', value: null }, 0);
  c.emit({ op: 'RETURN' }, 0);
  return { instrs: c.instrs };
}

const functionCache = new WeakMap<A.StmtOf<'FunctionDef'>, Code>();

export function compileFunction(def: A.StmtOf<'FunctionDef'>): Code {
  const cached = functionCache.get(def);
  if (cached) return cached;
  const c = new Compiler();
  c.body(def.body);
  const endLine = def.body.length ? def.body[def.body.length - 1].line : def.line;
  c.emit({ op: 'CONST', value: null }, endLine);
  c.emit({ op: 'RETURN' }, endLine);
  const code = { instrs: c.instrs };
  functionCache.set(def, code);
  return code;
}
