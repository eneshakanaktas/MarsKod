// Kelime ayırıcı: oyuncunun kodunu parçalara (token) böler ve girintileri INDENT/DEDENT'e çevirir.
// Hata mesajları CPython 3.12'nin tokenizer'ı ile birebir aynıdır.
//
// Hata olursa o ana kadarki parçalar ve hata birlikte döner: CPython kodu okurken
// daha önceki bir cümle hatası varsa onu önce bildirir; bu sırayı cümle çözücü belirler.

import { PythonException } from './errors';

export type TokenType = 'NAME' | 'NUMBER' | 'STRING' | 'OP' | 'NEWLINE' | 'INDENT' | 'DEDENT' | 'ENDMARKER';

export type NumberKind = 'int' | 'float' | 'imaginary';

export interface Token {
  type: TokenType;
  /** STRING: kaçış dizileri çözülmüş içerik. Diğerleri: kaynaktaki metin. */
  value: string;
  /** 1'den başlar */
  line: number;
  /** 0'dan başlar */
  col: number;
  endLine: number;
  endCol: number;
  /** Sadece STRING: küçük harfli önek ('', 'r', 'f', 'b', 'rb'...) */
  prefix?: string;
  /** Sadece NUMBER */
  numberKind?: NumberKind;
}

export interface TokenizeResult {
  tokens: Token[];
  error: PythonException | null;
}

const OPERATORS = [
  '**=', '//=', '>>=', '<<=', '...',
  '!=', '%=', '&=', '**', '*=', '+=', '-=', '->', '//', '/=', ':=', '<<', '<=', '==', '>=', '>>', '@=', '^=', '|=',
  ...'%&()*+,-./:;<=>@[]^{|}~',
];

const CLOSERS: Record<string, string> = { ')': '(', ']': '[', '}': '{' };

const STRING_PREFIXES = new Set(['r', 'u', 'f', 'b', 'br', 'rb', 'fr', 'rf']);

const ID_START = /[\p{L}\p{Nl}_]/u;
const ID_CONTINUE = /[\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}_]/u;
const NON_PRINTABLE = /[\p{C}\p{Z}]/u;

const D = '[0-9](?:_?[0-9])*';
const EXP = `[eE][+-]?${D}`;
const HEX = /0[xX](?:_?[0-9a-fA-F])+/y;
const OCT = /0[oO](?:_?[0-7])+/y;
const BIN = /0[bB](?:_?[01])+/y;
const FLOAT = new RegExp(`(?:(?:${D})?\\.${D}|${D}\\.)(?:${EXP})?|${D}${EXP}`, 'y');
const INT = new RegExp(D, 'y');

const SIMPLE_ESCAPES: Record<string, string> = {
  '\\': '\\', "'": "'", '"': '"', a: '\x07', b: '\b', f: '\f', n: '\n', r: '\r', t: '\t', v: '\v',
};

function codePointLabel(ch: string): string {
  return 'U+' + ch.codePointAt(0)!.toString(16).toUpperCase().padStart(4, '0');
}

function stickyMatch(re: RegExp, src: string, at: number): string | null {
  re.lastIndex = at;
  const m = re.exec(src);
  return m ? m[0] : null;
}

export function tokenize(source: string): TokenizeResult {
  const src = source.replace(/^﻿/, '').replace(/\r\n?/g, '\n');
  const len = src.length;
  const tokens: Token[] = [];
  const indents = [0];
  // Sekme genişliği 1 sayılarak ölçülen girinti; 8'lik ölçümle çelişirse TabError.
  const altIndents = [0];
  const brackets: { char: string; line: number }[] = [];

  let i = 0;
  let line = 1;
  let lineStart = 0;
  let atLineStart = true;
  let lineHasTokens = false;

  const fail = (pyType: string, message: string, errLine: number): TokenizeResult => ({
    tokens,
    error: new PythonException(pyType, message, errLine),
  });

  const push = (type: TokenType, value: string, startLine: number, startCol: number, extra?: Partial<Token>) => {
    tokens.push({ type, value, line: startLine, col: startCol, endLine: line, endCol: i - lineStart, ...extra });
    if (type !== 'INDENT' && type !== 'DEDENT' && type !== 'NEWLINE') lineHasTokens = true;
  };

  while (true) {
    // Yeni mantıksal satırın başı: girintiyi ölç.
    if (atLineStart) {
      let col = 0;
      let alt = 0;
      while (i < len) {
        const ch = src[i];
        if (ch === ' ') {
          col++;
          alt++;
        } else if (ch === '\t') {
          col = (Math.floor(col / 8) + 1) * 8;
          alt++;
        } else if (ch === '\f') {
          col = alt = 0;
        } else break;
        i++;
      }
      // Boş ya da sadece yorum olan satırlar girintiyi etkilemez.
      if (src[i] === '#') while (i < len && src[i] !== '\n') i++;
      if (src[i] === '\n') {
        i++;
        line++;
        lineStart = i;
        continue;
      }
      if (i >= len) break;

      const top = indents[indents.length - 1];
      const altTop = altIndents[altIndents.length - 1];
      if (col > top) {
        if (alt <= altTop) return fail('TabError', 'inconsistent use of tabs and spaces in indentation', line);
        indents.push(col);
        altIndents.push(alt);
        tokens.push({ type: 'INDENT', value: src.slice(lineStart, i), line, col: 0, endLine: line, endCol: i - lineStart });
      } else if (col < top) {
        while (indents.length > 1 && col < indents[indents.length - 1]) {
          indents.pop();
          altIndents.pop();
          tokens.push({ type: 'DEDENT', value: '', line, col: i - lineStart, endLine: line, endCol: i - lineStart });
        }
        if (col !== indents[indents.length - 1]) {
          return fail('IndentationError', 'unindent does not match any outer indentation level', line);
        }
        if (alt !== altIndents[altIndents.length - 1]) {
          return fail('TabError', 'inconsistent use of tabs and spaces in indentation', line);
        }
      } else if (alt !== altTop) {
        return fail('TabError', 'inconsistent use of tabs and spaces in indentation', line);
      }
      atLineStart = false;
    }

    if (i >= len) break;
    const ch = src[i];
    const startLine = line;
    const startCol = i - lineStart;

    if (ch === ' ' || ch === '\t' || ch === '\f') {
      i++;
      continue;
    }

    if (ch === '#') {
      while (i < len && src[i] !== '\n') i++;
      continue;
    }

    if (ch === '\n') {
      if (brackets.length === 0 && lineHasTokens) {
        tokens.push({ type: 'NEWLINE', value: '\n', line, col: startCol, endLine: line, endCol: startCol + 1 });
      }
      i++;
      line++;
      lineStart = i;
      if (brackets.length === 0) {
        atLineStart = true;
        lineHasTokens = false;
      }
      continue;
    }

    // Satır devamı: \ + satır sonu
    if (ch === '\\') {
      if (src[i + 1] === '\n') {
        i += 2;
        line++;
        lineStart = i;
        continue;
      }
      if (i + 1 >= len) return fail('SyntaxError', 'unexpected EOF while parsing', line);
      return fail('SyntaxError', 'unexpected character after line continuation character', line);
    }

    // Sayı
    if (/[0-9]/.test(ch) || (ch === '.' && /[0-9]/.test(src[i + 1] ?? ''))) {
      const result = readNumber();
      if (result) return result;
      continue;
    }

    // İsim ya da önekli metin (r"...", f"...")
    const cp = String.fromCodePoint(src.codePointAt(i)!);
    if (ID_START.test(cp)) {
      let j = i;
      while (j < len) {
        const c = String.fromCodePoint(src.codePointAt(j)!);
        if (!ID_CONTINUE.test(c)) break;
        j += c.length;
      }
      const word = src.slice(i, j);
      const quote = src[j];
      if ((quote === '"' || quote === "'") && STRING_PREFIXES.has(word.toLowerCase())) {
        i = j;
        const result = readString(word.toLowerCase(), startLine, startCol);
        if (result) return result;
        continue;
      }
      i = j;
      push('NAME', word.normalize('NFKC'), startLine, startCol);
      continue;
    }

    // Metin
    if (ch === '"' || ch === "'") {
      const result = readString('', startLine, startCol);
      if (result) return result;
      continue;
    }

    // İşleç ve parantez
    const op = OPERATORS.find((o) => src.startsWith(o, i));
    if (op) {
      if (op === '(' || op === '[' || op === '{') {
        brackets.push({ char: op, line });
      } else if (op in CLOSERS) {
        const open = brackets.pop();
        if (!open) return fail('SyntaxError', `unmatched '${op}'`, line);
        if (open.char !== CLOSERS[op]) {
          const where = open.line === line ? '' : ` on line ${open.line}`;
          return fail(
            'SyntaxError',
            `closing parenthesis '${op}' does not match opening parenthesis '${open.char}'${where}`,
            line,
          );
        }
      }
      i += op.length;
      push('OP', op, startLine, startCol);
      continue;
    }

    if (NON_PRINTABLE.test(cp)) {
      return fail('SyntaxError', `invalid non-printable character ${codePointLabel(cp)}`, line);
    }
    if (cp.codePointAt(0)! > 127) {
      return fail('SyntaxError', `invalid character '${cp}' (${codePointLabel(cp)})`, line);
    }
    // $ ? ! ` gibi ASCII karakterler: CPython bunları geçersiz parça olarak geçirir,
    // cümle çözücü "invalid syntax" der.
    i += 1;
    push('OP', ch, startLine, startCol);
  }

  if (brackets.length > 0) {
    const open = brackets[brackets.length - 1];
    return fail('SyntaxError', `'${open.char}' was never closed`, open.line);
  }
  // CPython dosya sonu parçalarını son karakterin satırında gösterir.
  const endLine = src.endsWith('\n') ? line - 1 : line;
  const endCol = src.endsWith('\n') ? 0 : i - lineStart;
  const eof = (type: TokenType): Token => ({ type, value: '', line: endLine, col: endCol, endLine, endCol });
  if (lineHasTokens) tokens.push(eof('NEWLINE'));
  while (indents.length > 1) {
    indents.pop();
    tokens.push(eof('DEDENT'));
  }
  tokens.push(eof('ENDMARKER'));
  return { tokens, error: null };

  // --- yardımcılar (i, line gibi durumları paylaşır) ---

  function readNumber(): TokenizeResult | null {
    const startLine = line;
    const startCol = i - lineStart;
    let text: string | null = null;
    let kind: NumberKind = 'int';
    let label = 'decimal';

    const radix = /^0[xXoObB]/.exec(src.slice(i, i + 2));
    if (radix) {
      const letter = radix[0][1].toLowerCase();
      label = letter === 'x' ? 'hexadecimal' : letter === 'o' ? 'octal' : 'binary';
      text = stickyMatch(letter === 'x' ? HEX : letter === 'o' ? OCT : BIN, src, i);
      const after = src[i + (text ? text.length : 2)] ?? '';
      if (letter !== 'x' && /[0-9]/.test(after)) {
        return fail('SyntaxError', `invalid digit '${after}' in ${label} literal`, line);
      }
      if (!text) return fail('SyntaxError', `invalid ${label} literal`, line);
    } else {
      text = stickyMatch(FLOAT, src, i);
      if (text) {
        kind = 'float';
      } else {
        text = stickyMatch(INT, src, i)!;
        if (/^0[0-9_]*$/.test(text) && /[1-9]/.test(text)) {
          return fail(
            'SyntaxError',
            'leading zeros in decimal integer literals are not permitted; use an 0o prefix for octal integers',
            line,
          );
        }
      }
    }

    let end = i + text.length;
    if (!radix && /[jJ]/.test(src[end] ?? '')) {
      end++;
      kind = 'imaginary';
    }
    const next = src.codePointAt(end);
    if (next !== undefined && ID_CONTINUE.test(String.fromCodePoint(next))) {
      return fail('SyntaxError', `invalid ${label} literal`, line);
    }
    const value = src.slice(i, end);
    i = end;
    push('NUMBER', value, startLine, startCol, { numberKind: kind });
    return null;
  }

  function readString(prefix: string, startLine: number, startCol: number): TokenizeResult | null {
    const quote = src[i];
    const triple = src.startsWith(quote.repeat(3), i);
    const closing = triple ? quote.repeat(3) : quote;
    const raw = prefix.includes('r') || prefix.includes('f');
    let value = '';
    i += closing.length;

    while (true) {
      if (i >= len) {
        if (triple) {
          const lastLine = src.endsWith('\n') ? line - 1 : line;
          return fail('SyntaxError', `unterminated triple-quoted string literal (detected at line ${lastLine})`, startLine);
        }
        return fail('SyntaxError', `unterminated string literal (detected at line ${line})`, startLine);
      }
      if (src.startsWith(closing, i)) {
        i += closing.length;
        break;
      }
      const c = src[i];
      if (c === '\n') {
        if (!triple) return fail('SyntaxError', `unterminated string literal (detected at line ${line})`, startLine);
        value += c;
        i++;
        line++;
        lineStart = i;
        continue;
      }
      if (c === '\\' && i + 1 < len) {
        const n = src[i + 1];
        if (n === '\n') {
          // Metin içinde satır devamı: ham metinde korunur, normalde silinir.
          if (raw) value += '\\\n';
          i += 2;
          line++;
          lineStart = i;
          continue;
        }
        if (raw) {
          value += c + n;
          i += 2;
          continue;
        }
        i += 2;
        value += readEscape(n);
        continue;
      }
      value += c;
      i++;
    }

    push('STRING', value, startLine, startCol, { prefix });
    return null;
  }

  // Kaçış dizisi: "\" ve n okunmuş, i ondan sonrasını gösteriyor.
  function readEscape(n: string): string {
    if (n in SIMPLE_ESCAPES) return SIMPLE_ESCAPES[n];
    if (/[0-7]/.test(n)) {
      let digits = n;
      while (digits.length < 3 && /[0-7]/.test(src[i] ?? '')) digits += src[i++];
      return String.fromCodePoint(parseInt(digits, 8));
    }
    const hexLength = n === 'x' ? 2 : n === 'u' ? 4 : n === 'U' ? 8 : 0;
    if (hexLength > 0) {
      const hex = src.slice(i, i + hexLength);
      if (hex.length === hexLength && /^[0-9a-fA-F]+$/.test(hex)) {
        i += hexLength;
        return String.fromCodePoint(parseInt(hex, 16));
      }
    }
    // Bilinmeyen kaçış: Python ters bölüyü olduğu gibi bırakır (sadece uyarı verir).
    return '\\' + n;
  }
}
