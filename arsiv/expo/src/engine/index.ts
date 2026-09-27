// Mini-Python motorunun dışa açılan yüzü. Oyundan habersizdir.

import { ExecutionLimit, PythonException, UnsupportedFeature, type UnsupportedFeatureId } from './errors';
import { Interpreter } from './interpreter';
import { parse } from './parser';

export { explainError, explainHalt, type ErrorCategory, type Explanation, type Tone } from './explain';

export interface PythonError {
  /** Gerçek Python'daki hata türü, örn. "NameError" */
  type: string;
  /** Gerçek Python'un İngilizce mesajıyla birebir aynı metin */
  message: string;
  /** Hatanın olduğu satır (1'den başlar); bilinmiyorsa null */
  line: number | null;
}

/** Python hatası olmadan durma: desteklenmeyen özellik ya da oyunun koruma sınırı. */
export type Halt =
  | { kind: 'unsupported'; feature: UnsupportedFeatureId; detail: string | null; line: number | null }
  | { kind: 'limit'; reason: 'steps' | 'size'; line: number | null };

export interface RunResult {
  /** print ile ekrana yazılan her şey */
  output: string;
  error: PythonError | null;
  halt: Halt | null;
}

export interface RunOptions {
  /** Varsayılan 200 000 adım */
  maxSteps?: number;
}

const DEFAULTS = { maxSteps: 200_000, recursionLimit: 1000 };

export function runPython(source: string, options: RunOptions = {}): RunResult {
  let interpreter: Interpreter | null = null;
  try {
    const module = parse(source);
    interpreter = new Interpreter({ ...DEFAULTS, ...options });
    const execution = interpreter.run(module);
    while (!execution.next().done);
    return { output: interpreter.output, error: null, halt: null };
  } catch (e) {
    const output = interpreter?.output ?? '';
    if (e instanceof PythonException) {
      return { output, error: { type: e.pyType, message: e.pyMessage, line: e.line }, halt: null };
    }
    if (e instanceof UnsupportedFeature) {
      return { output, error: null, halt: { kind: 'unsupported', feature: e.feature, detail: e.detail ?? null, line: e.line } };
    }
    if (e instanceof ExecutionLimit) {
      return { output, error: null, halt: { kind: 'limit', reason: e.reason, line: e.line } };
    }
    throw e;
  }
}
