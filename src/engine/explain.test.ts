import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

import type { UnsupportedFeatureId } from './errors';
import { explainError, explainHalt } from './explain';
import { FEATURE_NAMES } from './explanations-tr';

const CASES_DIR = join(__dirname, '..', '..', 'tests', 'python-cases');

const allTexts = (e: { title: string; text: string; hint: string | null }) => [e.title, e.text, e.hint ?? ''].join(' ');

describe('her bilinen hatanın Türkçe açıklaması var', () => {
  const seen = new Set<string>();
  for (const group of readdirSync(CASES_DIR)) {
    for (const file of readdirSync(join(CASES_DIR, group)).filter((f) => f.endsWith('.json'))) {
      const { error } = JSON.parse(readFileSync(join(CASES_DIR, group, file), 'utf8'));
      if (!error || seen.has(error.message)) continue;
      seen.add(error.message);
      it(`${error.type}: ${error.message}`, () => {
        const explanation = explainError(error);
        expect(explanation.generic).toBe(false);
        expect(allTexts(explanation)).not.toMatch(/\{\d|undefined|null/);
      });
    }
  }
});

describe('açıklama ayrıntıları', () => {
  it('"Did you mean" önerisini ipucuna çevirir', () => {
    const e = explainError({ type: 'NameError', message: "name 'enerj' is not defined. Did you mean: 'enerji'?", line: 2 });
    expect(e).toMatchObject({ category: 'isim', title: 'Tanımsız isim', hint: 'Belki `enerji` yazmak istedin?' });
    expect(e.text).toContain('`enerj`');
  });

  it('unutulmuş modülü açıklar', () => {
    const e = explainError({
      type: 'NameError',
      message: "name 'random' is not defined. Did you forget to import 'random'?",
      line: 1,
    });
    expect(e.hint).toContain('import random');
  });

  it('türleri Türkçeleştirir', () => {
    const e = explainError({ type: 'TypeError', message: "unsupported operand type(s) for +: 'int' and 'str'", line: 1 });
    expect(e.text).toBe('tam sayı (int) ile metin (str) arasında `+` işlemi yapılamaz.');
  });

  it('eksik argümanları Türkçe bağlaçla sıralar', () => {
    const e = explainError({
      type: 'TypeError',
      message: "dis.<locals>.ic() missing 3 required positional arguments: 'a', 'b', and 'c'",
      line: 4,
    });
    expect(e.text).toBe("`ic()` fonksiyonu 3 değer daha bekliyor: 'a', 'b' ve 'c'.");
  });

  it('kıvrık tırnağı genel geçersiz karakterden ayırır', () => {
    expect(explainError({ type: 'SyntaxError', message: "invalid character '“' (U+201C)", line: 1 }).title).toBe('Kıvrık tırnak');
    expect(explainError({ type: 'SyntaxError', message: "invalid character '€' (U+20AC)", line: 1 }).title).toBe('Geçersiz karakter');
  });

  it('koşuldaki tek eşittiri "koşul" türüne ayırır', () => {
    const e = explainError({ type: 'SyntaxError', message: "invalid syntax. Maybe you meant '==' or ':=' instead of '='?", line: 2 });
    expect(e.category).toBe('kosul');
  });

  it('bilinmeyen mesajda türüne göre genel açıklama verir', () => {
    expect(explainError({ type: 'SyntaxError', message: 'çok garip bir hata', line: 1 })).toMatchObject({ generic: true, category: 'yazim' });
  });
});

describe('durma açıklamaları', () => {
  it('bitmeyen döngü', () => {
    expect(explainHalt({ kind: 'limit', reason: 'steps', line: 3 })).toMatchObject({ category: 'dongu', title: 'Kod durmuyor' });
  });

  it.each(Object.keys(FEATURE_NAMES) as UnsupportedFeatureId[])('desteklenmeyen: %s', (feature) => {
    const e = explainHalt({ kind: 'unsupported', feature, detail: 'input', line: 1 });
    expect(e.category).toBe('desteklenmeyen');
    expect(allTexts(e)).not.toMatch(/\{detail\}|undefined/);
  });
});
