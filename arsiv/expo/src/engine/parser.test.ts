import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

import type * as A from './ast';
import { PythonException, UnsupportedFeature } from './errors';
import { parse } from './parser';

/** Ağacı okunabilir kısa bir metne çevirir (testleri kısa tutmak için). */
function show(e: A.Expr): string {
  switch (e.kind) {
    case 'Name':
      return e.id;
    case 'Constant':
      return e.value.type === 'str' ? JSON.stringify(e.value.value) : e.value.type === 'none' ? 'None' : String(e.value.value);
    case 'BinOp':
      return `(${show(e.left)} ${e.op} ${show(e.right)})`;
    case 'UnaryOp':
      return `(${e.op} ${show(e.operand)})`;
    case 'BoolOp':
      return `(${e.values.map(show).join(` ${e.op} `)})`;
    case 'Compare':
      return `(${show(e.left)} ${e.ops.map((op, i) => `${op} ${show(e.comparators[i])}`).join(' ')})`;
    case 'Call':
      return `${show(e.func)}(${[...e.args.map(show), ...e.keywords.map((k) => `${k.name}=${show(k.value)}`)].join(', ')})`;
    case 'Attribute':
      return `${show(e.value)}.${e.attr}`;
    case 'Subscript':
      return `${show(e.value)}[${show(e.index)}]`;
    case 'Slice':
      return [e.lower, e.upper, ...(e.step ? [e.step] : [])].map((x) => (x ? show(x) : '')).join(':');
    case 'List':
      return `[${e.elts.map(show).join(', ')}]`;
    case 'Tuple':
      return `tuple(${e.elts.map(show).join(', ')})`;
    case 'IfExp':
      return `(${show(e.body)} if ${show(e.test)} else ${show(e.orelse)})`;
    case 'Dict':
      return `{${e.keys.map((k, i) => `${show(k)}: ${show(e.values[i])}`).join(', ')}}`;
    case 'Set':
      return `set{${e.elts.map(show).join(', ')}}`;
  }
}

const expr = (source: string) => {
  const stmt = parse(source).body[0];
  if (stmt.kind !== 'Expr') throw new Error('ifade bekleniyordu');
  return show(stmt.value);
};

const errorOf = (source: string) => {
  try {
    parse(source);
  } catch (e) {
    if (e instanceof PythonException) return { type: e.pyType, message: e.pyMessage, line: e.line };
    if (e instanceof UnsupportedFeature) return { unsupported: e.feature, line: e.line };
    throw e;
  }
  return null;
};

describe('parse: işlem önceliği', () => {
  it.each([
    ['1 + 2 * 3', '(1 + (2 * 3))'],
    ['(1 + 2) * 3', '((1 + 2) * 3)'],
    ['-2 ** 2', '(- (2 ** 2))'],
    ['2 ** 3 ** 2', '(2 ** (3 ** 2))'],
    ['10 - 3 - 2', '((10 - 3) - 2)'],
    ['not a and b or c', '(((not a) and b) or c)'],
    ['1 < x <= 10', '(1 < x <= 10)'],
    ['x not in liste', '(x not in liste)'],
    ['x is not None', '(x is not None)'],
    ['a if k else b', '(a if k else b)'],
    ['f(1, x=2)', 'f(1, x=2)'],
    ['robot.move()', 'robot.move()'],
    ['a[1:3]', 'a[1:3]'],
    ['a[::2]', 'a[::2]'],
    ['[1, 2, 3][0]', '[1, 2, 3][0]'],
    ['"a" "b"', '"ab"'],
    ['{"buz": 1}', '{"buz": 1}'],
    ['{1, 2}', 'set{1, 2}'],
    ['()', 'tuple()'],
    ['(1,)', 'tuple(1)'],
  ])('%s', (source, expected) => {
    expect(expr(source)).toBe(expected);
  });

  it('büyük tam sayıları kaybetmeden okur', () => {
    const stmt = parse('12345678901234567890123').body[0] as A.StmtOf<'Expr'>;
    expect(stmt.value).toMatchObject({ kind: 'Constant', value: { type: 'int', value: 12345678901234567890123n } });
  });
});

describe('parse: cümleler', () => {
  it('atamaları ayırır', () => {
    const [a, b, c] = parse('x = 1\na = b = 2\nx += 1').body;
    expect(a).toMatchObject({ kind: 'Assign', targets: [{ id: 'x' }] });
    expect(b).toMatchObject({ kind: 'Assign', targets: [{ id: 'a' }, { id: 'b' }] });
    expect(c).toMatchObject({ kind: 'AugAssign', op: '+', target: { id: 'x' } });
  });

  it('if/elif/else zincirini kurar', () => {
    const [s] = parse('if a:\n    x = 1\nelif b:\n    x = 2\nelse:\n    x = 3\n').body;
    expect(s).toMatchObject({
      kind: 'If',
      line: 1,
      orelse: [{ kind: 'If', line: 3, orelse: [{ kind: 'Assign', line: 6 }] }],
    });
  });

  it('for, while ve def yapılarını kurar', () => {
    const body = parse('def f(a, b=2):\n    for i in range(a):\n        while i:\n            i -= 1\n    return b\n').body;
    expect(body[0]).toMatchObject({
      kind: 'FunctionDef',
      name: 'f',
      params: [{ name: 'a', default: null }, { name: 'b' }],
      body: [{ kind: 'For', target: { id: 'i' }, body: [{ kind: 'While' }] }, { kind: 'Return' }],
    });
  });

  it('tek satırlık blokları kabul eder', () => {
    expect(parse('if x: y = 1; z = 2\n').body[0]).toMatchObject({ kind: 'If', body: [{ kind: 'Assign' }, { kind: 'Assign' }] });
  });
});

describe('parse: desteklenmeyen özellikler', () => {
  it.each([
    ['import math', 'import'],
    ['class Robot:\n    pass', 'class'],
    ['x = [i for i in range(3)]', 'comprehension'],
    ['print(f"{x}")', 'f-string'],
    ['f = lambda x: x', 'lambda'],
    ['try:\n    pass\nexcept:\n    pass', 'try'],
  ])('%s → %s', (source, feature) => {
    expect(errorOf(source)).toMatchObject({ unsupported: feature });
  });
});

// Gerçek Python örnekleriyle karşılaştırma
const CASES_DIR = join(__dirname, '..', '..', 'tests', 'python-cases');
const SYNTAX_ERRORS = ['SyntaxError', 'IndentationError', 'TabError'];

describe('parse = gerçek Python', () => {
  for (const group of readdirSync(CASES_DIR)) {
    for (const file of readdirSync(join(CASES_DIR, group)).filter((f) => f.endsWith('.py'))) {
      const name = file.replace(/\.py$/, '');
      const source = readFileSync(join(CASES_DIR, group, file), 'utf8');
      const ref = JSON.parse(readFileSync(join(CASES_DIR, group, `${name}.json`), 'utf8'));
      const expected = ref.error && SYNTAX_ERRORS.includes(ref.error.type) ? ref.error : null;

      it(`${group}/${name}`, () => {
        expect(errorOf(source)).toEqual(expected);
      });
    }
  }
});
