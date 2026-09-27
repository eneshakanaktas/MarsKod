// Hata açıklamalarının Türkçe metinleri (ton: standart — sade ve sıcak).
//
// DÜZENLEME REHBERİ (kod bilmeyenler için):
// - Sadece tırnak içindeki metinleri değiştirin.
// - {1}, {2} ... Python mesajından alınan parçalardır (isim, tür, satır no). Yerlerini koruyun.
// - {1:tur} gibi yazımlar parçayı Türkçeleştirir (int → "tam sayı (int)").
// - `ters tırnak` içindeki kısımlar oyunda kod gibi görünür.
// - Kurallar yukarıdan aşağı denenir; ilk uyan kullanılır. Her türün en altında genel kural vardır.
// Değişiklikten sonra `npm test` çalıştırın: her bilinen hatanın metni olduğu otomatik denetlenir.

import type { UnsupportedFeatureId } from './errors';
import type { ErrorCategory, MatchContext } from './explain';

export interface Rule {
  /** Python hata türleri, örn. ['SyntaxError'] */
  types: string[];
  /** Python'un İngilizce mesajına uyan kalıp ("Did you mean" kısmı önceden ayrılır) */
  pattern: RegExp;
  category: ErrorCategory;
  title: string;
  text: string | ((g: string[], ctx: MatchContext) => string);
  hint?: string | null | ((g: string[], ctx: MatchContext) => string | null);
  /** Genel (yedek) kural mı */
  generic?: boolean;
}

const SYNTAX = ['SyntaxError'];
const INDENT = ['IndentationError', 'TabError'];

const TIP_NONE =
  'Değer `None` (hiçbir şey). Çoğu zaman `return` yazılmamış bir fonksiyondan ya da sonuç vermeyen bir komuttan (örneğin `liste.sort()`) gelir.';

export const RULES: Rule[] = [
  // --- girinti ---
  {
    types: INDENT,
    pattern: /^expected an indented block after '(\w+)' statement on line (\d+)$/,
    category: 'girinti',
    title: 'Girinti eksik',
    text: '{2}. satırdaki `{1}` iki nokta (:) ile bitiyor. Python, ondan sonraki satırların içeride, yani girintili olmasını bekler.',
    hint: 'Bu satırın başına 4 boşluk ekle.',
  },
  {
    types: INDENT,
    pattern: /^expected an indented block after function definition on line (\d+)$/,
    category: 'girinti',
    title: 'Girinti eksik',
    text: '{1}. satırdaki fonksiyon tanımı (`def`) iki nokta (:) ile bitiyor. Fonksiyonun içindeki satırlar girintili olmalı.',
    hint: 'Bu satırın başına 4 boşluk ekle.',
  },
  {
    types: INDENT,
    pattern: /^unexpected indent$/,
    category: 'girinti',
    title: 'Beklenmeyen girinti',
    text: 'Bu satır sebepsiz yere içeri kaymış. Python\'da bir satır sadece bir bloğun (`if`, `for`, `def`...) içindeyse girintili olur.',
    hint: 'Satırın başındaki boşlukları sil ya da üstteki satırla aynı hizaya getir.',
  },
  {
    types: INDENT,
    pattern: /^unindent does not match any outer indentation level$/,
    category: 'girinti',
    title: 'Girinti hizası tutmuyor',
    text: 'Bu satırın girintisi, üstteki blokların hiçbirinin hizasına uymuyor.',
    hint: 'Aynı bloktaki satırların başında aynı sayıda boşluk olmalı (genelde 4).',
  },
  {
    types: INDENT,
    pattern: /^inconsistent use of tabs and spaces in indentation$/,
    category: 'girinti',
    title: 'Sekme ve boşluk karışmış',
    text: 'Girintide hem sekme (Tab) hem boşluk kullanılmış. Ekranda aynı görünseler de Python ikisini farklı sayar.',
    hint: 'Girintileri sadece boşlukla yap.',
  },
  {
    types: INDENT,
    pattern: /^unexpected unindent$/,
    category: 'girinti',
    title: 'Beklenmeyen girinti',
    text: 'Bu satırın girintisi beklenenden erken bitmiş.',
    hint: 'Satırı, ait olduğu bloğun hizasına getir.',
  },
  {
    types: INDENT,
    pattern: /^/,
    category: 'girinti',
    title: 'Girinti hatası',
    text: 'Satırların içeri kayma (girinti) düzeninde bir sorun var.',
    hint: 'Aynı bloktaki satırlar aynı hizada olmalı; bloklar iki noktadan (:) sonra 4 boşluk içeride başlar.',
    generic: true,
  },

  // --- koşulda tek eşittir ---
  {
    types: SYNTAX,
    pattern: /^invalid syntax\. Maybe you meant '==' or ':=' instead of '='\?$/,
    category: 'kosul',
    title: 'Karşılaştırmada tek eşittir',
    text: 'Tek eşittir (`=`) bir kutuya değer koymak içindir. İki şeyin eşit olup olmadığını sormak için iki eşittir (`==`) kullanılır.',
    hint: '`=` yerine `==` yaz.',
  },
  {
    types: SYNTAX,
    pattern: /^expression cannot contain assignment, perhaps you meant "=="\?$/,
    category: 'kosul',
    title: 'Parantez içinde tek eşittir',
    text: 'Parantezin içinde tek eşittir (`=`) kullanılmış. Karşılaştırma için iki eşittir (`==`) gerekir.',
    hint: '`=` yerine `==` yaz.',
  },

  // --- atama ---
  {
    types: SYNTAX,
    pattern: /^cannot assign to (.+) here\. Maybe you meant '==' instead of '='\?$/,
    category: 'atama',
    title: 'Buraya değer verilemez',
    text: 'Eşittirin (`=`) solunda bir değişken adı olmalı; burada {1:ifade} var.',
    hint: 'Karşılaştırmak istediysen `==` kullan. Değer vermek istediysen solda sadece değişken adı olsun: `x = 5`',
  },
  {
    types: SYNTAX,
    pattern: /^cannot assign to (True|False|None)$/,
    category: 'atama',
    title: 'Özel kelimeye değer verilemez',
    text: '`{1}` Python\'un özel bir kelimesi; ona değer verilemez.',
    hint: 'Değişkenine başka bir isim ver.',
  },
  {
    types: SYNTAX,
    pattern: /^cannot assign to (.+)$/,
    category: 'atama',
    title: 'Buraya değer verilemez',
    text: 'Eşittirin (`=`) solunda {1:ifade} var. Değer sadece bir değişkene (ya da listenin bir elemanına) verilebilir.',
    hint: 'Solda bir değişken adı olsun, örneğin: `x = 5`',
  },
  {
    types: SYNTAX,
    pattern: /^'(.+)' is an illegal expression for augmented assignment$/,
    category: 'atama',
    title: '`+=` burada kullanılamaz',
    text: '`+=`, `-=` gibi işlemler sadece bir değişkene uygulanabilir; burada {1:ifade} var.',
    hint: 'Solda bir değişken adı olsun, örneğin: `sayac += 1`',
  },

  // --- yazım ---
  {
    types: SYNTAX,
    pattern: /^expected ':'$/,
    category: 'yazim',
    title: 'İki nokta eksik',
    text: 'Bu satır bir blok başlatıyor (`if`, `for`, `while`, `def`...) ama sonunda iki nokta (`:`) yok.',
    hint: 'Satırın sonuna `:` ekle.',
  },
  {
    types: SYNTAX,
    pattern: /^expected '\('$/,
    category: 'yazim',
    title: 'Parantez eksik',
    text: 'Fonksiyon tanımında isimden hemen sonra parantez gelmeli.',
    hint: 'Örnek: `def selamla():`',
  },
  {
    types: SYNTAX,
    pattern: /^'(.)' was never closed$/,
    category: 'yazim',
    title: 'Kapanmamış parantez',
    text: 'Bu satırda açılan `{1}` hiç kapanmamış.',
    hint: 'Kapatan işareti (`{1:kapanis}`) ekle.',
  },
  {
    types: SYNTAX,
    pattern: /^unmatched '(.)'$/,
    category: 'yazim',
    title: 'Fazladan kapanış',
    text: '`{1}` işareti var ama onunla eşleşen bir açılış yok.',
    hint: 'Fazla olan kapanışı sil ya da eksik açılışı ekle.',
  },
  {
    types: SYNTAX,
    pattern: /^closing parenthesis '(.)' does not match opening parenthesis '(.)'/,
    category: 'yazim',
    title: 'Parantezler uyuşmuyor',
    text: '`{2}` ile açılan şey `{1}` ile kapatılmış. Açılış ve kapanış aynı türde olmalı: `( )`, `[ ]`, `{ }`.',
    hint: 'Kapanışı `{2:kapanis}` yap.',
  },
  {
    types: SYNTAX,
    pattern: /^unterminated string literal/,
    category: 'yazim',
    title: 'Kapanmamış tırnak',
    text: 'Bir metin tırnakla başlamış ama aynı satırda kapanmamış.',
    hint: 'Metnin sonuna, başladığın tırnağın aynısını ekle.',
  },
  {
    types: SYNTAX,
    pattern: /^unterminated triple-quoted string literal/,
    category: 'yazim',
    title: 'Kapanmamış üçlü tırnak',
    text: '`"""` ile başlayan uzun metin hiç kapanmamış.',
    hint: 'Metnin sonuna `"""` ekle.',
  },
  {
    types: SYNTAX,
    pattern: /^invalid character '([“”‘’])'/,
    category: 'yazim',
    title: 'Kıvrık tırnak',
    text: '`{1}` düz tırnak değil, kıvrık tırnak. Telefon klavyeleri bazen düz tırnağı kendiliğinden buna çevirir; Python ise sadece düz tırnağı tanır.',
    hint: 'Düz tırnak kullan: `"` ya da `\'`',
  },
  {
    types: SYNTAX,
    pattern: /^invalid character '(.+)'/,
    category: 'yazim',
    title: 'Geçersiz karakter',
    text: '`{1}` işareti Python kodunda kullanılamaz.',
    hint: 'Bu karakteri sil ya da değiştir.',
  },
  {
    types: SYNTAX,
    pattern: /^invalid non-printable character/,
    category: 'yazim',
    title: 'Görünmez karakter',
    text: 'Bu satırda gözle görünmeyen ama Python\'un tanımadığı bir karakter var. Çoğu zaman kopyala-yapıştır ya da klavye yüzünden normal boşluk yerine özel bir boşluk girer.',
    hint: 'Satırdaki boşlukları silip yeniden yaz.',
  },
  {
    types: SYNTAX,
    pattern: /^invalid (decimal|hexadecimal|octal|binary) literal$|^invalid digit/,
    category: 'yazim',
    title: 'Geçersiz sayı',
    text: 'Bir sayının hemen yanına harf yazılmış (örneğin `3kaya`). Python bunu ne sayı ne de isim olarak okuyabilir.',
    hint: 'Sayıyla ismin arasına bir işlem koy (`3 * kaya`). Değişken isimleri rakamla başlayamaz.',
  },
  {
    types: SYNTAX,
    pattern: /^leading zeros in decimal integer literals/,
    category: 'yazim',
    title: 'Başta sıfır',
    text: 'Python\'da tam sayılar 0 ile başlayamaz (`007` gibi).',
    hint: 'Baştaki sıfırları sil: `7`',
  },
  {
    types: SYNTAX,
    pattern: /^Missing parentheses in call to '(\w+)'/,
    category: 'yazim',
    title: '`{1}` için parantez gerekli',
    text: 'Python 3\'te `{1}` bir fonksiyondur; yazdırılacak şey parantez içine alınmalı.',
    hint: 'Örnek: `{1}("merhaba")`',
  },
  {
    types: SYNTAX,
    pattern: /^invalid syntax\. Perhaps you forgot a comma\?$/,
    category: 'yazim',
    title: 'Virgül eksik',
    text: 'Parantezin içinde yan yana iki şey var ama aralarında virgül yok.',
    hint: 'Aralarına virgül (`,`) koy.',
  },
  {
    types: SYNTAX,
    pattern: /^expected 'else' after 'if' expression$/,
    category: 'yazim',
    title: '`else` eksik',
    text: 'Tek satırlık koşulda (`a if koşul else b`) `else` kısmı eksik.',
    hint: 'Sonuna `else` ve bir değer ekle.',
  },

  // --- fonksiyon ve döngü kuralları (derleme sırasında) ---
  {
    types: SYNTAX,
    pattern: /^'return' outside function$/,
    category: 'fonksiyon',
    title: '`return` fonksiyon dışında',
    text: '`return` sadece bir fonksiyonun (`def`) içinde kullanılabilir.',
    hint: '`return` satırını bir fonksiyonun içine al ya da sil.',
  },
  {
    types: SYNTAX,
    pattern: /^'break' outside loop$/,
    category: 'dongu',
    title: '`break` döngü dışında',
    text: '`break` bir döngüyü (`for`, `while`) erken bitirmek içindir; burada ortada döngü yok.',
    hint: '`break` satırını bir döngünün içine al.',
  },
  {
    types: SYNTAX,
    pattern: /^'continue' not properly in loop$/,
    category: 'dongu',
    title: '`continue` döngü dışında',
    text: '`continue` döngünün bir sonraki turuna geçmek içindir; burada ortada döngü yok.',
    hint: '`continue` satırını bir döngünün içine al.',
  },
  {
    types: SYNTAX,
    pattern: /^duplicate argument '(.+)' in function definition$/,
    category: 'fonksiyon',
    title: 'Aynı parametre iki kez',
    text: 'Fonksiyonun parametrelerinde `{1}` iki kez yazılmış.',
    hint: 'Parametrelere farklı isimler ver.',
  },
  {
    types: SYNTAX,
    pattern: /^parameter without a default follows parameter with a default$/,
    category: 'fonksiyon',
    title: 'Parametre sırası',
    text: 'Varsayılan değeri olan bir parametreden (`b=1` gibi) sonra, varsayılanı olmayan bir parametre gelemez.',
    hint: 'Varsayılanı olmayanları başa al: `def f(a, b=1):`',
  },
  {
    types: SYNTAX,
    pattern: /^positional argument follows keyword argument$/,
    category: 'fonksiyon',
    title: 'Argüman sırası',
    text: 'İsimle verilen bir argümandan (`sep="-"` gibi) sonra, isimsiz bir argüman gelemez.',
    hint: 'İsimsiz argümanları başa al.',
  },
  {
    types: SYNTAX,
    pattern: /^keyword argument repeated: (.+)$/,
    category: 'fonksiyon',
    title: 'Argüman iki kez verilmiş',
    text: '`{1}=` argümanı aynı çağrıda iki kez yazılmış.',
    hint: 'Birini sil.',
  },
  {
    types: SYNTAX,
    pattern: /^invalid syntax$/,
    category: 'yazim',
    title: 'Yazım hatası',
    text: 'Python bu satırı anlayamadı. Genellikle bir işaret eksik ya da fazladır, ya da bir kelime yanlış yerde kullanılmıştır.',
    hint: 'Satırı dikkatle oku: parantezler, tırnaklar, iki noktalar ve virgüller yerinde mi?',
  },
  {
    types: SYNTAX,
    pattern: /^/,
    category: 'yazim',
    title: 'Yazım hatası',
    text: 'Python bu satırı anlayamadı. Genellikle bir işaret eksik ya da fazladır, ya da bir kelime yanlış yerde kullanılmıştır.',
    hint: 'Satırı dikkatle oku: parantezler, tırnaklar, iki noktalar ve virgüller yerinde mi?',
    generic: true,
  },

  // --- isimler ---
  {
    types: ['NameError'],
    pattern: /^name '(.+)' is not defined$/,
    category: 'isim',
    title: 'Tanımsız isim',
    text: '`{1}` adında bir değişken ya da fonksiyon yok. Ya henüz değer verilmedi ya da adı farklı yazıldı.',
    hint: (_, ctx) => {
      if (ctx.suggestion) return `Belki \`${ctx.suggestion}\` yazmak istedin?`;
      if (ctx.importName) return `\`${ctx.importName}\` Python'un hazır modüllerinden birinin adı. Gerçek Python'da önce \`import ${ctx.importName}\` gerekir; bu oyunda modüller henüz yok.`;
      return 'Adı doğru yazdığından ve bu satırdan önce ona değer verdiğinden emin ol.';
    },
  },
  {
    types: ['UnboundLocalError'],
    pattern: /^cannot access local variable '(.+)' where it is not associated with a value$/,
    category: 'isim',
    title: 'Değer verilmeden kullanıldı',
    text: '`{1}` bu fonksiyonun içinde daha sonra değer alıyor; bu yüzden Python onu fonksiyona ait sayar. Ama bu satırda henüz değeri yok.',
    hint: 'Dışarıdaki `{1}` değişkenini kullanmak istiyorsan fonksiyonun başına `global {1}` yaz; değilse önce ona değer ver.',
  },
  {
    types: ['NameError'],
    pattern: /^cannot access free variable '(.+)'/,
    category: 'isim',
    title: 'Değer verilmeden kullanıldı',
    text: '`{1}` dıştaki fonksiyonda tanımlı ama bu satır çalıştığında henüz değeri yok.',
    hint: '`{1}` değişkenine, onu kullanan satırdan önce değer ver.',
  },
  {
    types: ['NameError', 'UnboundLocalError'],
    pattern: /^/,
    category: 'isim',
    title: 'İsim hatası',
    text: 'Kullanılan bir isim (değişken ya da fonksiyon) bulunamadı.',
    hint: 'İsmin doğru yazıldığından ve önceden değer aldığından emin ol.',
    generic: true,
  },

  // --- türler ---
  {
    types: ['TypeError'],
    pattern: /^can only concatenate str \(not "(.+)"\) to str$/,
    category: 'tur',
    title: 'Metne sayı eklenemez',
    text: '`+` ile bir metnin yanına sadece başka bir metin eklenebilir; burada {1:tur} var.',
    hint: 'Sayıyı metne çevir: `str(enerji)`. Ya da `print` içinde virgül kullan: `print("Enerji:", enerji)`',
  },
  {
    types: ['TypeError'],
    pattern: /^can only concatenate (list|tuple) \(not "(.+)"\) to (?:list|tuple)$/,
    category: 'tur',
    title: 'Bu ikisi birleştirilemez',
    text: '`+` ile bir {1:tur} yanına sadece başka bir {1:tur} eklenebilir; burada {2:tur} var.',
    hint: (g) => (g[1] === 'list' ? 'Tek bir eleman eklemek için `liste.append(x)` kullan.' : null),
  },
  {
    types: ['TypeError'],
    pattern: /^unsupported operand type\(s\) for (.+): '(.+)' and '(.+)'$/,
    category: 'tur',
    title: 'Bu işlem bu türlerle yapılamaz',
    text: '{2:tur} ile {3:tur} arasında `{1}` işlemi yapılamaz.',
    hint: (g) => {
      if (g[2] === 'NoneType' || g[3] === 'NoneType') return TIP_NONE;
      if (g[2] === 'str' || g[3] === 'str') return 'Metni sayıya çevirmek için `int()`, sayıyı metne çevirmek için `str()` kullan.';
      return null;
    },
  },
  {
    types: ['TypeError'],
    pattern: /^can't multiply sequence by non-int of type '(.+)'$/,
    category: 'tur',
    title: 'Tam sayı ile çarpılmalı',
    text: 'Bir metin ya da liste sadece tam sayı ile çarpılabilir (tekrarlamak için); burada {1:tur} var.',
    hint: 'Örnek: `"ab" * 3` sonucu `"ababab"` olur. Ondalıklı sayıyı `int()` ile tam sayıya çevir.',
  },
  {
    types: ['TypeError'],
    pattern: /^'(.+)' not supported between instances of '(.+)' and '(.+)'$/,
    category: 'tur',
    title: 'Karşılaştırılamaz',
    text: '{2:tur} ile {3:tur}, `{1}` ile karşılaştırılamaz.',
    hint: 'İkisini aynı türe çevir; örneğin `int("5") < 7`',
  },
  {
    types: ['TypeError'],
    pattern: /^bad operand type for unary (.): '(.+)'$/,
    category: 'tur',
    title: 'Bu işaret burada kullanılamaz',
    text: '{2:tur} önüne `{1}` işareti konamaz.',
    hint: null,
  },
  {
    types: ['TypeError'],
    pattern: /^'(.+)' object is not callable$/,
    category: 'tur',
    title: 'Bu bir fonksiyon değil',
    text: '{1:tur} bir fonksiyon değil; parantezle çağrılamaz.',
    hint: 'Bir değişkene fonksiyonla aynı ismi vermiş olabilir misin? Ya da sonuna yanlışlıkla `()` koymuş olabilirsin.',
  },
  {
    types: ['TypeError'],
    pattern: /^'(.+)' object is not iterable$/,
    category: 'dongu',
    title: 'Bunun üzerinde dönülemez',
    text: '`for` döngüsü {1:tur} üzerinde dönemez; liste, metin ya da `range` gerekir.',
    hint: 'Bir sayı kadar dönmek için `range` kullan: `for i in range(5):`',
  },
  {
    types: ['TypeError'],
    pattern: /^'(.+)' object is not subscriptable$/,
    category: 'indeks',
    title: 'Köşeli parantez kullanılamaz',
    text: '{1:tur} içinden köşeli parantezle (`[ ]`) eleman alınamaz.',
    hint: (g) => (g[1] === 'NoneType' ? TIP_NONE : 'Köşeli parantez liste ve metinlerde kullanılır.'),
  },
  {
    types: ['TypeError'],
    pattern: /^'(.+)' object does not support item assignment$/,
    category: 'tur',
    title: 'Elemanı değiştirilemez',
    text: '{1:tur} içindeki bir eleman sonradan değiştirilemez.',
    hint: (g) => (g[1] === 'str' ? 'Yeni bir metin oluştur, örneğin: `s = "X" + s[1:]`' : null),
  },
  {
    types: ['TypeError'],
    pattern: /^(list|string|tuple) indices must be integers(?: or slices)?, not '?(\w+)'?$/,
    category: 'indeks',
    title: 'Sıra numarası tam sayı olmalı',
    text: 'Köşeli parantezin içine bir sıra numarası (tam sayı) yazılmalı; burada {2:tur} var.',
    hint: 'Örnek: `liste[0]`. Ondalıklı sayıyı `int()` ile çevir.',
  },
  {
    types: ['TypeError'],
    pattern: /^object of type '(.+)' has no len\(\)$/,
    category: 'tur',
    title: 'Uzunluğu ölçülemez',
    text: '`len()` metin, liste gibi şeylerin uzunluğunu ölçer; {1:tur} için kullanılamaz.',
    hint: null,
  },
  {
    types: ['TypeError'],
    pattern: /^'(.+)' object cannot be interpreted as an integer$/,
    category: 'tur',
    title: 'Tam sayı gerekiyor',
    text: 'Burada tam sayı gerekiyor ama {1:tur} verilmiş.',
    hint: '`int()` ile tam sayıya çevir.',
  },
  {
    types: ['TypeError'],
    pattern: /^cannot unpack non-iterable (.+) object$/,
    category: 'atama',
    title: 'Değerler dağıtılamaz',
    text: 'Birden çok değişkene aynı anda değer vermek için sağda bir liste ya da demet olmalı; burada {1:tur} var.',
    hint: null,
  },
  {
    types: ['TypeError'],
    pattern: /^(.+)\(\) missing (\d+) required positional arguments?: (.+)$/,
    category: 'fonksiyon',
    title: 'Eksik değer',
    text: '`{1:ad}()` fonksiyonu {2} değer daha bekliyor: {3:ve}.',
    hint: 'Çağırırken parantezin içine eksik değerleri de yaz.',
  },
  {
    types: ['TypeError'],
    pattern: /^(.+)\(\) takes (\d+) positional arguments? but (\d+) (?:was|were) given$/,
    category: 'fonksiyon',
    title: 'Fazla değer',
    text: '`{1:ad}()` fonksiyonu {2} değer alıyor ama {3} değer verilmiş.',
    hint: 'Fazla değerleri sil ya da fonksiyona yeni bir parametre ekle.',
  },
  {
    types: ['TypeError'],
    pattern: /^(.+)\(\) takes from (\d+) to (\d+) positional arguments but (\d+) were given$/,
    category: 'fonksiyon',
    title: 'Fazla değer',
    text: '`{1:ad}()` fonksiyonu en az {2}, en fazla {3} değer alıyor ama {4} değer verilmiş.',
    hint: 'Fazla değerleri sil.',
  },
  {
    types: ['TypeError'],
    pattern: /^(.+)\(\) got an unexpected keyword argument '(.+)'$/,
    category: 'fonksiyon',
    title: 'Böyle bir parametre yok',
    text: '`{1:ad}()` fonksiyonunun `{2}` adında bir parametresi yok.',
    hint: 'Parametre adını, fonksiyonun tanımındaki (`def`) adla aynı yaz.',
  },
  {
    types: ['TypeError'],
    pattern: /^(.+)\(\) got multiple values for argument '(.+)'$/,
    category: 'fonksiyon',
    title: 'Aynı parametreye iki değer',
    text: '`{2}` parametresine iki kez değer verilmiş: bir kez sırayla, bir kez adıyla.',
    hint: 'İkisinden birini sil.',
  },
  {
    types: ['TypeError'],
    pattern: /^(sep|end) must be None or a string, not (.+)$/,
    category: 'tur',
    title: '`{1}` metin olmalı',
    text: '`print`\'in `{1}` ayarı bir metin olmalı; burada {2:tur} var.',
    hint: 'Örnek: `print(1, 2, {1}="-")`',
  },
  {
    types: ['TypeError'],
    pattern: /^/,
    category: 'tur',
    title: 'Tür uyuşmazlığı',
    text: 'Bu işlem, verilen değerin türüyle (sayı, metin, liste...) yapılamıyor.',
    hint: 'Değerlerin türlerini kontrol et; gerekirse `int()`, `str()` ile çevir.',
    generic: true,
  },

  // --- değerler ---
  {
    types: ['ValueError'],
    pattern: /^invalid literal for int\(\) with base \d+: (.+)$/,
    category: 'donusum',
    title: 'Tam sayıya çevrilemez',
    text: '{1} metni bir tam sayıya çevrilemiyor.',
    hint: (g) =>
      g[1].includes('.')
        ? 'Ondalıklı sayılar için önce `float()` kullan, gerekirse sonra `int()`.'
        : '`int()` sadece rakamlardan oluşan metinleri çevirebilir.',
  },
  {
    types: ['ValueError'],
    pattern: /^could not convert string to float: (.+)$/,
    category: 'donusum',
    title: 'Sayıya çevrilemez',
    text: '{1} metni bir sayıya çevrilemiyor.',
    hint: 'Metin sadece bir sayı içermeli, örneğin `"2.5"`.',
  },
  {
    types: ['ValueError'],
    pattern: /^list\.remove\(x\): x not in list$/,
    category: 'liste',
    title: 'Silinecek eleman yok',
    text: '`remove()` ile silmek istediğin değer listede yok.',
    hint: 'Önce kontrol et: `if x in liste:`',
  },
  {
    types: ['ValueError'],
    pattern: /^(.+) is not in list$/,
    category: 'liste',
    title: 'Listede yok',
    text: '{1} listede bulunmuyor, bu yüzden sırası (`index`) bulunamadı.',
    hint: 'Önce kontrol et: `if x in liste:`',
  },
  {
    types: ['ValueError'],
    pattern: /^(min|max)\(\) iterable argument is empty$/,
    category: 'liste',
    title: 'Liste boş',
    text: '`{1}()` boş bir listede sonuç bulamaz.',
    hint: 'Önce listenin boş olmadığını kontrol et: `if liste:`',
  },
  {
    types: ['ValueError'],
    pattern: /^not enough values to unpack \(expected (\d+), got (\d+)\)$/,
    category: 'atama',
    title: 'Değerler az',
    text: 'Solda {1} değişken var ama sağda sadece {2} değer var.',
    hint: 'İki taraftaki sayılar eşit olmalı.',
  },
  {
    types: ['ValueError'],
    pattern: /^too many values to unpack \(expected (\d+)\)$/,
    category: 'atama',
    title: 'Değerler fazla',
    text: 'Solda {1} değişken var ama sağda daha fazla değer var.',
    hint: 'İki taraftaki sayılar eşit olmalı.',
  },
  {
    types: ['ValueError'],
    pattern: /^range\(\) arg 3 must not be zero$/,
    category: 'dongu',
    title: '`range` adımı sıfır olamaz',
    text: '`range`\'in üçüncü sayısı (adım) 0 olursa sayılar hiç ilerlemez.',
    hint: 'Adımı 1 ya da başka bir sayı yap; geriye saymak için eksi sayı kullan: `range(10, 0, -1)`',
  },
  {
    types: ['ValueError'],
    pattern: /^/,
    category: 'donusum',
    title: 'Uygunsuz değer',
    text: 'Değerin türü doğru ama kendisi bu işlem için uygun değil.',
    hint: null,
    generic: true,
  },

  // --- sıra numaraları ---
  {
    types: ['IndexError'],
    pattern: /^(list|string|tuple|range object) index out of range$/,
    category: 'indeks',
    title: 'Sıra numarası dışarıda',
    text: 'Bu {1:tur} içinde bu sıra numarasında bir eleman yok. Unutma: sayma 0\'dan başlar; 3 elemanlı bir listede son sıra numarası 2\'dir.',
    hint: 'Uzunluğa `len()` ile bakabilirsin.',
  },
  {
    types: ['IndexError'],
    pattern: /^list assignment index out of range$/,
    category: 'indeks',
    title: 'Sıra numarası dışarıda',
    text: 'Listede olmayan bir sıra numarasına değer verilemez.',
    hint: 'Listenin sonuna eklemek için `liste.append(x)` kullan.',
  },
  {
    types: ['IndexError'],
    pattern: /^pop from empty list$/,
    category: 'liste',
    title: 'Liste boş',
    text: 'Boş bir listeden `pop()` ile eleman alınamaz.',
    hint: 'Önce listenin boş olmadığını kontrol et: `if liste:`',
  },
  {
    types: ['IndexError'],
    pattern: /^pop index out of range$/,
    category: 'liste',
    title: 'Sıra numarası dışarıda',
    text: '`pop()` ile istenen sıra numarasında bir eleman yok.',
    hint: 'Sayma 0\'dan başlar; son eleman için sıra numarası vermeden `pop()` yaz.',
  },
  {
    types: ['IndexError'],
    pattern: /^/,
    category: 'indeks',
    title: 'Sıra numarası dışarıda',
    text: 'İstenen sıra numarasında bir eleman yok.',
    hint: 'Sayma 0\'dan başlar; uzunluğa `len()` ile bakabilirsin.',
    generic: true,
  },

  // --- özellik/komut ---
  {
    types: ['AttributeError'],
    pattern: /^'(.+)' object has no attribute '(.+)'$/,
    category: 'ozellik',
    title: 'Böyle bir komut yok',
    text: '{1:tur} için `{2}` diye bir komut ya da özellik yok.',
    hint: (g, ctx) => {
      if (ctx.suggestion) return `Belki \`${ctx.suggestion}\` yazmak istedin?`;
      if (g[1] === 'NoneType') return TIP_NONE;
      return null;
    },
  },
  {
    types: ['AttributeError'],
    pattern: /^/,
    category: 'ozellik',
    title: 'Özellik hatası',
    text: 'Bu değerin böyle bir özelliği yok ya da değiştirilemez.',
    hint: null,
    generic: true,
  },

  // --- diğer ---
  {
    types: ['ZeroDivisionError'],
    pattern: /^0\.0 cannot be raised to a negative power$/,
    category: 'sifira-bolme',
    title: 'Sıfıra bölme',
    text: '0\'ın eksi kuvveti alınamaz; bu da sıfıra bölmek demektir.',
    hint: null,
  },
  {
    types: ['ZeroDivisionError'],
    pattern: /^/,
    category: 'sifira-bolme',
    title: 'Sıfıra bölme',
    text: 'Bir sayı sıfıra bölünemez.',
    hint: 'Bölmeden önce kontrol et: `if bolen != 0:`',
  },
  {
    types: ['RecursionError'],
    pattern: /^/,
    category: 'fonksiyon',
    title: 'Fonksiyon kendini durmadan çağırıyor',
    text: 'Bir fonksiyon kendini çağırıyor ama hiç durmuyor. Python 1000 iç içe çağrıdan sonra durdurur.',
    hint: 'Fonksiyonda, kendini çağırmayı bitiren bir koşul (`if ...: return`) olduğundan emin ol.',
  },
  {
    types: ['OverflowError'],
    pattern: /^/,
    category: 'tur',
    title: 'Sayı çok büyük',
    text: 'Sonuç, ondalıklı sayıların sığabileceği sınırdan büyük.',
    hint: null,
  },
];

export const HALT_TEXTS: Record<'steps' | 'size', { category: ErrorCategory; title: string; text: string; hint: string }> = {
  steps: {
    category: 'dongu',
    title: 'Kod durmuyor',
    text: 'Kodun çok uzun süre çalıştı ve durduruldu. Büyük ihtimalle bitmeyen bir döngü var: `while` koşulu hiç yanlış olmuyor olabilir.',
    hint: 'Döngünün içinde, koşulu sonunda yanlış yapacak bir satır olduğundan emin ol.',
  },
  size: {
    category: 'sinir',
    title: 'Değer çok büyük',
    text: 'Kod, telefonun belleğine sığmayacak kadar büyük bir sayı ya da metin üretmeye çalıştı.',
    hint: 'Sayıları ve tekrar sayılarını küçült.',
  },
};

export const FEATURE_NAMES: Record<UnsupportedFeatureId, { name: string; hint?: string }> = {
  annotation: { name: 'tür ipuçları (`x: int`)', hint: 'Tür ipucunu silebilirsin; kod onsuz da çalışır.' },
  assert: { name: '`assert`' },
  async: { name: 'eşzamansız kod (`async`)' },
  await: { name: '`await`' },
  builtin: { name: '`{detail}` fonksiyonu' },
  bytes: { name: 'bayt metinleri (`b"..."`)' },
  class: { name: 'sınıflar (`class`)' },
  complex: { name: 'karmaşık sayılar' },
  comprehension: {
    name: 'tek satırda liste üretme (`[x for x in ...]`)',
    hint: 'Aynı işi bir `for` döngüsü ve `append()` ile yapabilirsin.',
  },
  decorator: { name: 'dekoratörler (`@`)' },
  del: { name: 'silme (`del`)', hint: 'Listeden silmek için `liste.remove(x)` ya da `liste.pop()` kullan.' },
  dict: { name: 'sözlükler (`{"anahtar": değer}`)' },
  ellipsis: { name: 'üç nokta (`...`)', hint: 'Boş bir blok için `pass` yaz.' },
  'f-string': { name: 'f-metinleri (`f"..."`)', hint: 'Şimdilik birleştirme kullan: `"Enerji: " + str(enerji)`' },
  import: { name: 'modül ekleme (`import`)' },
  lambda: { name: '`lambda` fonksiyonları', hint: 'Aynı işi `def` ile bir fonksiyon yazarak yapabilirsin.' },
  method: { name: '`{detail}` komutu' },
  raise: { name: 'hata fırlatma (`raise`)' },
  set: { name: 'kümeler (`{1, 2}`)' },
  'slice-assign': { name: 'dilime değer verme (`liste[1:3] = ...`)' },
  'star-args': { name: 'yıldızlı argümanlar (`*liste`)' },
  'star-params': { name: 'yıldızlı parametreler (`*args`)' },
  'str-format': { name: '`%` ile metin biçimlendirme', hint: 'Şimdilik birleştirme kullan: `"Enerji: " + str(enerji)`' },
  try: { name: 'hata yakalama (`try`/`except`)' },
  walrus: { name: '`:=` işleci' },
  with: { name: '`with` bloğu' },
  yield: { name: '`yield`' },
};
