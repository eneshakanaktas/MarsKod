import { Interpreter } from './interpreter';
import { parse } from './parser';
import type { Value } from './values';

import { runPython } from '.';

describe('koruma sınırları', () => {
  it('bitmeyen döngüyü adım sınırında durdurur', () => {
    const result = runPython('x = 0\nwhile True:\n    x += 1\n', { maxSteps: 1000 });
    expect(result.error).toBeNull();
    expect(result.halt).toMatchObject({ kind: 'limit', reason: 'steps' });
    expect([2, 3]).toContain(result.halt!.line);
  });

  it('dev hesapları durdurur', () => {
    expect(runPython('x = 2 ** 100000000').halt).toMatchObject({ kind: 'limit', reason: 'size', line: 1 });
    expect(runPython('x = "a" * 10 ** 12').halt).toMatchObject({ kind: 'limit', reason: 'size', line: 1 });
  });

  it('yerleşik fonksiyonların içindeki uzun yinelemeyi de sayar', () => {
    expect(runPython('print(sum(range(10 ** 9)))', { maxSteps: 1000 }).halt).toMatchObject({ reason: 'steps' });
  });

  it('durmadan önceki çıktıyı korur', () => {
    const result = runPython('print("başla")\nwhile True:\n    pass\n', { maxSteps: 100 });
    expect(result.output).toBe('başla\n');
  });
});

describe('desteklenmeyen özellikler', () => {
  it.each([
    ['x = input()', 'builtin', 'input'],
    ['d = {"a": 1}', 'dict', null],
    ['s = "a".format()', 'method', 'str.format'],
    ['import math', 'import', null],
    ['print("%d" % 5)', 'str-format', null],
  ])('%s', (source, feature, detail) => {
    const result = runPython(source);
    expect(result.error).toBeNull();
    expect(result.halt).toMatchObject({ kind: 'unsupported', feature, line: 1, ...(detail ? { detail } : {}) });
  });

  it('yazım hatası desteklenmeyen özellikten önce gelir', () => {
    expect(runPython('import math\nif x\n').error).toMatchObject({ type: 'SyntaxError', line: 2 });
  });
});

describe('oyunun dışarıdan verdiği komutlar', () => {
  it('move() gibi komutları çağırır ve sonucunu kullanır', () => {
    const moves: string[] = [];
    const externals = new Map<string, Value>([
      ['move', { kind: 'builtin', name: 'move', call: (args) => (moves.push(String(args[0])), null) }],
      ['ice_here', { kind: 'builtin', name: 'ice_here', call: () => moves.length === 2 }],
    ]);
    const interpreter = new Interpreter({ maxSteps: 1000, recursionLimit: 1000, externals });
    const source = 'while not ice_here():\n    move("north")\nprint("buz bulundu")\n';
    for (const _ of interpreter.run(parse(source)));
    expect(moves).toEqual(['north', 'north']);
    expect(interpreter.output).toBe('buz bulundu\n');
  });
});

describe('adım adım çalışma', () => {
  it('her cümleden önce satırını verir', () => {
    const source = 'x = 1\nfor i in range(2):\n    x += i\nprint(x)\n';
    const interpreter = new Interpreter({ maxSteps: 1000, recursionLimit: 1000 });
    const lines: number[] = [];
    for (const step of interpreter.run(parse(source))) lines.push(step.line);
    expect(lines).toEqual([1, 2, 3, 2, 3, 2, 4]);
    expect(interpreter.output).toBe('2\n');
  });

  it('durakta o anki değişkenleri gösterir', () => {
    const interpreter = new Interpreter({ maxSteps: 1000, recursionLimit: 1000 });
    const snapshots: string[] = [];
    for (const step of interpreter.run(parse('enerji = 5\nenerji -= 2\nprint(enerji)\n'))) {
      snapshots.push(String(step.frame.vars.get('enerji') ?? '-'));
    }
    expect(snapshots).toEqual(['-', '5', '3']);
  });
});
