// Hataların Türkçe açıklaması ve türü (oyuncu profili hata türlerini sayar).
// Metinlerin kendisi explanations-tr.ts dosyasındadır; kod bilmeyen ekip üyeleri de düzenleyebilir.

import type { UnsupportedFeatureId } from './errors';
import type { Halt, PythonError } from './index';
import { FEATURE_NAMES, HALT_TEXTS, RULES, type Rule } from './explanations-tr';

/** Hata türleri: oyuncu profili ve ders önerileri bunlara göre çalışır. */
export type ErrorCategory =
  | 'yazim'
  | 'girinti'
  | 'kosul'
  | 'atama'
  | 'isim'
  | 'tur'
  | 'donusum'
  | 'indeks'
  | 'liste'
  | 'fonksiyon'
  | 'dongu'
  | 'sifira-bolme'
  | 'ozellik'
  | 'desteklenmeyen'
  | 'sinir';

/** Anlatım tonu. İleride ayarlara 'eglenceli' eklenecek (bkz. yapım planı, Aşama 7). */
export type Tone = 'standart';

export interface Explanation {
  category: ErrorCategory;
  /** Kısa başlık, örn. "Tanımsız isim" */
  title: string;
  /** Ne oldu, neden oldu */
  text: string;
  /** Ne yapılabilir; yoksa null */
  hint: string | null;
  /** Bu mesaj için özel metin yoksa true (genel açıklama verildi) */
  generic: boolean;
}

/** Şablonlardaki {1:tur} gibi süzgeçler */
const FILTERS: Record<string, (value: string) => string> = {
  tur: (t) => TYPE_NAMES[t.replace(/ object$/, '')] ?? t,
  ifade: (e) => EXPR_NAMES[e] ?? e,
  kapanis: (c) => ({ '(': ')', '[': ']', '{': '}' })[c] ?? c,
  ad: (name) => name.split('.').pop()!,
  ve: (list) => list.replace(/,? and /, ' ve '),
};

const TYPE_NAMES: Record<string, string> = {
  int: 'tam sayı (int)',
  float: 'ondalıklı sayı (float)',
  str: 'metin (str)',
  string: 'metin (str)',
  bool: 'doğru/yanlış değeri (bool)',
  NoneType: 'None (hiçbir şey)',
  list: 'liste',
  tuple: 'demet (tuple)',
  range: 'range',
  function: 'fonksiyon',
  builtin_function_or_method: 'yerleşik fonksiyon',
  type: 'tür',
};

const EXPR_NAMES: Record<string, string> = {
  literal: 'sabit bir değer (sayı ya da metin)',
  expression: 'bir işlem',
  'function call': 'bir fonksiyon çağrısı',
  comparison: 'bir karşılaştırma',
  attribute: 'bir özellik',
  subscript: 'bir eleman',
  tuple: 'bir demet',
  list: 'bir liste',
  'conditional expression': 'bir koşullu ifade',
  'dict literal': 'bir sözlük',
  'set display': 'bir küme',
};

export interface MatchContext {
  /** "Did you mean: 'x'?" önerisi */
  suggestion: string | null;
  /** "Did you forget to import 'x'?" ipucu */
  importName: string | null;
}

function fill(template: string, groups: string[]): string {
  return template.replace(/\{(\d)(?::(\w+))?\}/g, (_, index: string, filter?: string) => {
    const value = groups[Number(index)] ?? '';
    return filter ? FILTERS[filter](value) : value;
  });
}

function render(part: Rule['text'] | Rule['hint'], groups: string[], ctx: MatchContext): string | null {
  if (part === undefined || part === null) return null;
  const template = typeof part === 'function' ? part(groups, ctx) : part;
  return template === null ? null : fill(template, groups);
}

export function explainError(error: PythonError, tone: Tone = 'standart'): Explanation {
  void tone; // şimdilik tek ton
  const suggestion = /\. Did you mean: '(.+?)'\?/.exec(error.message)?.[1] ?? null;
  const importName = /did you forget to import '(.+?)'\??$/i.exec(error.message)?.[1] ?? null;
  const base = error.message
    .replace(/\. Did you mean: '.+?'\?/, '')
    .replace(/(\.| Or) did you forget to import '.+?'\??$/i, '');
  const ctx: MatchContext = { suggestion, importName };

  for (const rule of RULES) {
    if (!rule.types.includes(error.type)) continue;
    const m = rule.pattern.exec(base);
    if (!m) continue;
    const groups = [...m] as string[];
    return {
      category: rule.category,
      title: fill(rule.title, groups),
      text: render(rule.text, groups, ctx)!,
      hint: render(rule.hint, groups, ctx),
      generic: rule.generic ?? false,
    };
  }
  throw new Error(`Açıklama bulunamadı: ${error.type}`); // RULES sonunda her tür için genel kural var
}

export function explainHalt(halt: Halt, tone: Tone = 'standart'): Explanation {
  void tone;
  if (halt.kind === 'limit') return { ...HALT_TEXTS[halt.reason], hint: HALT_TEXTS[halt.reason].hint, generic: false };
  return {
    category: 'desteklenmeyen',
    title: 'Bu oyunda henüz yok',
    text: `Bu kod gerçek Python'da çalışır, ama ${featureName(halt.feature, halt.detail)} bu oyunda henüz desteklenmiyor.`,
    hint: FEATURE_NAMES[halt.feature].hint ?? 'Aynı işi, oyunun bildiği komutlarla yapmayı dene.',
    generic: false,
  };
}

function featureName(feature: UnsupportedFeatureId, detail: string | null): string {
  return FEATURE_NAMES[feature].name.replace('{detail}', detail ?? '');
}
