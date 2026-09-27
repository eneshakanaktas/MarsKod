// Motorun içinde fırlatılan hatalar.

/** Gerçek Python hatası. Dışarıya PythonError olarak çıkar. */
export class PythonException extends Error {
  constructor(
    /** Gerçek Python'daki hata türü, örn. "SyntaxError" */
    public readonly pyType: string,
    /** Gerçek Python'un İngilizce mesajıyla birebir aynı metin */
    public readonly pyMessage: string,
    /**
     * Hatanın olduğu satır (1'den başlar). Yerleşik fonksiyonlar satırı bilmez (null bırakır);
     * çalıştırıcı, hatanın çıktığı ifadenin satırını sonradan yazar.
     */
    public line: number | null,
  ) {
    super(`${pyType}: ${pyMessage}`);
    this.name = 'PythonException';
  }
}

/** Gerçek Python'da olan ama bu oyunun motorunun (henüz) desteklemediği özellikler. */
export type UnsupportedFeatureId =
  | 'annotation'
  | 'assert'
  | 'async'
  | 'await'
  | 'builtin'
  | 'bytes'
  | 'class'
  | 'complex'
  | 'comprehension'
  | 'decorator'
  | 'del'
  | 'dict'
  | 'ellipsis'
  | 'f-string'
  | 'import'
  | 'lambda'
  | 'method'
  | 'raise'
  | 'set'
  | 'slice-assign'
  | 'star-args'
  | 'star-params'
  | 'str-format'
  | 'try'
  | 'walrus'
  | 'with'
  | 'yield';

/** "Gerçek Python'da var ama bu oyunda henüz yok" durumu. Python hatası değildir. */
export class UnsupportedFeature extends Error {
  constructor(
    public readonly feature: UnsupportedFeatureId,
    public line: number | null,
    /** Örn. feature 'builtin' için 'input', 'method' için 'str.format' */
    public readonly detail?: string,
  ) {
    super(`Desteklenmeyen özellik: ${feature}${detail ? ` (${detail})` : ''}`);
    this.name = 'UnsupportedFeature';
  }
}

/** Oyunun koruma sınırı aşıldı (bitmeyen döngü, aşırı büyük değer). Python hatası değildir. */
export class ExecutionLimit extends Error {
  constructor(
    public readonly reason: 'steps' | 'size',
    public line: number | null,
  ) {
    super(reason === 'steps' ? 'Adım sınırı aşıldı' : 'Değer çok büyük');
    this.name = 'ExecutionLimit';
  }
}
