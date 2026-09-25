import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

import { tokenize, type Token } from './tokenizer';

const brief = (tokens: Token[]) => tokens.map((t) => (t.type === 'OP' || t.type === 'NAME' ? t.value : t.type));

describe('tokenize', () => {
  it('basit satırı parçalara böler', () => {
    const { tokens, error } = tokenize('x = 3 + 4\n');
    expect(error).toBeNull();
    expect(brief(tokens)).toEqual(['x', '=', 'NUMBER', '+', 'NUMBER', 'NEWLINE', 'ENDMARKER']);
  });

  it('girintiyi INDENT/DEDENT olarak verir', () => {
    const { tokens } = tokenize('if x:\n    y\n    if z:\n        w\nv\n');
    expect(brief(tokens)).toEqual([
      'if', 'x', ':', 'NEWLINE',
      'INDENT', 'y', 'NEWLINE',
      'if', 'z', ':', 'NEWLINE',
      'INDENT', 'w', 'NEWLINE',
      'DEDENT', 'DEDENT', 'v', 'NEWLINE', 'ENDMARKER',
    ]);
  });

  it('dosya sonunda açık blokları kapatır', () => {
    const { tokens } = tokenize('for i in r:\n    print(i)');
    expect(brief(tokens).slice(-4)).toEqual([')', 'NEWLINE', 'DEDENT', 'ENDMARKER']);
  });

  it('boş satırlar ve yorumlar girintiyi bozmaz', () => {
    const { tokens, error } = tokenize('if x:\n\n    # yorum\n  \n    y\n');
    expect(error).toBeNull();
    expect(brief(tokens)).toEqual(['if', 'x', ':', 'NEWLINE', 'INDENT', 'y', 'NEWLINE', 'DEDENT', 'ENDMARKER']);
  });

  it('parantez içindeki satır sonlarını yok sayar', () => {
    const { tokens } = tokenize('x = (1,\n     2)\n');
    expect(brief(tokens)).toEqual(['x', '=', '(', 'NUMBER', ',', 'NUMBER', ')', 'NEWLINE', 'ENDMARKER']);
  });

  it('metinlerdeki kaçış dizilerini çözer', () => {
    const { tokens } = tokenize(String.raw`"a\tb\n" 'c\'d' r"\n" "\x41\u00e7"`);
    expect(tokens.filter((t) => t.type === 'STRING').map((t) => [t.prefix, t.value])).toEqual([
      ['', 'a\tb\n'],
      ['', "c'd"],
      ['r', '\\n'],
      ['', 'Aç'],
    ]);
  });

  it('sayı türlerini ayırır', () => {
    const { tokens } = tokenize('1 1_000 0x1F 1.5 .5 2. 1e3 3j');
    expect(tokens.filter((t) => t.type === 'NUMBER').map((t) => [t.value, t.numberKind])).toEqual([
      ['1', 'int'],
      ['1_000', 'int'],
      ['0x1F', 'int'],
      ['1.5', 'float'],
      ['.5', 'float'],
      ['2.', 'float'],
      ['1e3', 'float'],
      ['3j', 'imaginary'],
    ]);
  });

  it('Türkçe harfli isimleri kabul eder', () => {
    const { tokens, error } = tokenize('çalışan_robot = 1');
    expect(error).toBeNull();
    expect(tokens[0]).toMatchObject({ type: 'NAME', value: 'çalışan_robot' });
  });

  it('konum bilgisini verir', () => {
    const { tokens } = tokenize('x = 1\n  \nyaz("a")');
    expect(tokens.find((t) => t.value === 'yaz')).toMatchObject({ line: 3, col: 0, endCol: 3 });
    expect(tokens.find((t) => t.type === 'STRING')).toMatchObject({ line: 3, col: 4, endCol: 7 });
  });
});

// Gerçek Python örnekleriyle karşılaştırma
const CASES_DIR = join(__dirname, '..', '..', 'tests', 'python-cases');

// Bu örneklerde hata kelime ayırma aşamasında oluşur; mesaj gerçek Python'la aynı olmalı.
const TOKENIZER_ERROR_CASES = [
  'bastaki-sifir',
  'fazla-parantez',
  'gecersiz-sayi',
  'girinti-uyusmuyor',
  'gorunmez-bosluk',
  'ic-ice-kapanmamis',
  'kivrik-tirnak',
  'parantez-kapanmamis',
  'parantez-uyusmuyor',
  'parantez-uyusmuyor-coklu-satir',
  'sekme-bosluk-karisik',
  'sonra-kapanmamis-parantez',
  'tirnak-kapanmamis',
  'uclu-tirnak-kapanmamis',
];

describe('tokenize = gerçek Python', () => {
  for (const group of readdirSync(CASES_DIR)) {
    for (const file of readdirSync(join(CASES_DIR, group)).filter((f) => f.endsWith('.py'))) {
      const name = file.replace(/\.py$/, '');
      const source = readFileSync(join(CASES_DIR, group, file), 'utf8');
      const ref = JSON.parse(readFileSync(join(CASES_DIR, group, `${name}.json`), 'utf8'));

      if (TOKENIZER_ERROR_CASES.includes(name)) {
        it(`${group}/${name}: aynı hatayı verir`, () => {
          const { error } = tokenize(source);
          expect(error && { type: error.pyType, message: error.pyMessage, line: error.line }).toEqual(ref.error);
        });
      } else if (!ref.error || !['SyntaxError', 'IndentationError', 'TabError'].includes(ref.error.type)) {
        // Yazım hatası olmayan örneklerde kelime aşamasında da hata çıkmamalı.
        // (Cümle aşaması hatalarının son hâli parser testlerinde denetlenir.)
        it(`${group}/${name}: kelime aşamasında hata yok`, () => {
          expect(tokenize(source).error).toBeNull();
        });
      }
    }
  }
});
