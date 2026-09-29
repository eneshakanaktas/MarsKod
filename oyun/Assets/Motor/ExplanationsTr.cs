// Hata açıklamalarının Türkçe metinleri (ton: standart — sade ve sıcak).
//
// DÜZENLEME REHBERİ (kod bilmeyenler için):
// - Sadece tırnak içindeki metinleri (Title, Text, Hint) değiştirin.
// - Metnin içinde çift tırnak (") gerekirse önüne ters bölü koyun: \"
// - {1}, {2} ... Python mesajından alınan parçalardır (isim, tür, satır no). Yerlerini koruyun.
// - {1:tur} gibi yazımlar parçayı Türkçeleştirir (int → "tam sayı (int)").
// - `ters tırnak` içindeki kısımlar oyunda kod gibi görünür.
// - Kurallar yukarıdan aşağı denenir; ilk uyan kullanılır. Her türün en altında genel kural vardır.
// Değişiklikten sonra motor-test klasöründe `dotnet test` çalıştırın: her bilinen hatanın metni
// olduğu otomatik denetlenir.

using System.Collections.Generic;

namespace MarsKod.Motor
{
    public sealed class HaltText
    {
        public ErrorCategory Category;
        public string Title;
        public string Text;
        public string Hint;
    }

    public sealed class FeatureText
    {
        public string Name;
        public string Hint;
    }

    public static class ExplanationsTr
    {
        static readonly string[] Syntax = { "SyntaxError" };
        static readonly string[] Indent = { "IndentationError", "TabError" };
        static readonly string[] NameErr = { "NameError" };
        static readonly string[] TypeErr = { "TypeError" };
        static readonly string[] ValueErr = { "ValueError" };
        static readonly string[] IndexErr = { "IndexError" };
        static readonly string[] AttrErr = { "AttributeError" };

        const string TipNone =
            "Değer `None` (hiçbir şey). Çoğu zaman `return` yazılmamış bir fonksiyondan ya da sonuç vermeyen bir komuttan (örneğin `liste.sort()`) gelir.";

        public static readonly List<Rule> Rules = new List<Rule>
        {
            // --- girinti ---
            // Yeni baslayan icin: "girinti", "blok" gibi kelimeler yerine ne oldugunu ve neyi degistirecegini anlat.
            // Ipucu hem suruklemeye (Acemi) hem yazmaya (Orta/Usta) uysun: "sağa kaydır / içeri al".
            new Rule
            {
                Types = Indent,
                Pattern = @"^expected an indented block after '(for|while)' statement on line (\d+)$",
                Category = ErrorCategory.Girinti,
                Title = "`{1}` neyi tekrarlayacağını bilmiyor",
                Text = "{2}. satırdaki `{1}` bir işi tekrar tekrar yaptırır. Neyi tekrarlayacağını, hemen altındaki biraz sağa kaymış (içeri alınmış) satırlardan anlar. Şu an altında içeride hiç satır yok.",
                Hint = "Tekrarlanmasını istediğin satırları `{1}` satırının altına koy ve biraz sağa kaydır. İçeri aldığın satırların solunda ince bir çizgi belirir: çizginin yanındakiler tekrarlanır.",
            },
            new Rule
            {
                Types = Indent,
                Pattern = @"^expected an indented block after '(\w+)' statement on line (\d+)$",
                Category = ErrorCategory.Girinti,
                Title = "`{1}` satırının altı boş",
                Text = "{2}. satırdaki `{1}` iki nokta (:) ile bitiyor. İki nokta \"şimdi ne yapılacağını söyleyeceğim\" demektir: Python, bu satıra ait olanları hemen altında, biraz sağa kaymış (içeri alınmış) satırlarda arar. Şu an orada hiç satır yok.",
                Hint = "`{1}` satırına ait olan satırı hemen altına koy ve biraz sağa kaydır.",
            },
            new Rule
            {
                Types = Indent,
                Pattern = @"^expected an indented block after function definition on line (\d+)$",
                Category = ErrorCategory.Girinti,
                Title = "Girinti eksik",
                Text = "{1}. satırdaki fonksiyon tanımı (`def`) iki nokta (:) ile bitiyor. Fonksiyonun içindeki satırlar girintili olmalı.",
                Hint = "Bu satırın başına 4 boşluk ekle.",
            },
            new Rule
            {
                Types = Indent,
                Pattern = @"^unexpected indent$",
                Category = ErrorCategory.Girinti,
                Title = "Bu satır sebepsiz yere içeride",
                Text = "Bir satır yalnızca iki nokta (:) ile biten bir satırın (`for`, `if`...) altındaysa sağa kayar (içeri alınır); ona ait olduğunu böyle gösterir. Bu satırın üstünde öyle bir satır yok, Python neden içeride olduğunu anlayamıyor.",
                Hint = "Satırı sola, üstündeki satırla aynı hizaya kaydır.",
            },
            new Rule
            {
                Types = Indent,
                Pattern = @"^unindent does not match any outer indentation level$",
                Category = ErrorCategory.Girinti,
                Title = "Girinti hizası tutmuyor",
                Text = "Bu satırın girintisi, üstteki blokların hiçbirinin hizasına uymuyor.",
                Hint = "Aynı bloktaki satırların başında aynı sayıda boşluk olmalı (genelde 4).",
            },
            new Rule
            {
                Types = Indent,
                Pattern = @"^inconsistent use of tabs and spaces in indentation$",
                Category = ErrorCategory.Girinti,
                Title = "Sekme ve boşluk karışmış",
                Text = "Girintide hem sekme (Tab) hem boşluk kullanılmış. Ekranda aynı görünseler de Python ikisini farklı sayar.",
                Hint = "Girintileri sadece boşlukla yap.",
            },
            new Rule
            {
                Types = Indent,
                Pattern = @"^unexpected unindent$",
                Category = ErrorCategory.Girinti,
                Title = "Beklenmeyen girinti",
                Text = "Bu satırın girintisi beklenenden erken bitmiş.",
                Hint = "Satırı, ait olduğu bloğun hizasına getir.",
            },
            new Rule
            {
                Types = Indent,
                Pattern = @"^",
                Category = ErrorCategory.Girinti,
                Title = "Girinti hatası",
                Text = "Satırların içeri kayma (girinti) düzeninde bir sorun var.",
                Hint = "Aynı bloktaki satırlar aynı hizada olmalı; bloklar iki noktadan (:) sonra 4 boşluk içeride başlar.",
                Generic = true,
            },

            // --- koşulda tek eşittir ---
            new Rule
            {
                Types = Syntax,
                Pattern = @"^invalid syntax\. Maybe you meant '==' or ':=' instead of '='\?$",
                Category = ErrorCategory.Kosul,
                Title = "Karşılaştırmada tek eşittir",
                Text = "Tek eşittir (`=`) bir kutuya değer koymak içindir. İki şeyin eşit olup olmadığını sormak için iki eşittir (`==`) kullanılır.",
                Hint = "`=` yerine `==` yaz.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^expression cannot contain assignment, perhaps you meant ""==""\?$",
                Category = ErrorCategory.Kosul,
                Title = "Parantez içinde tek eşittir",
                Text = "Parantezin içinde tek eşittir (`=`) kullanılmış. Karşılaştırma için iki eşittir (`==`) gerekir.",
                Hint = "`=` yerine `==` yaz.",
            },

            // --- atama ---
            new Rule
            {
                Types = Syntax,
                Pattern = @"^cannot assign to (.+) here\. Maybe you meant '==' instead of '='\?$",
                Category = ErrorCategory.Atama,
                Title = "Buraya değer verilemez",
                Text = "Eşittirin (`=`) solunda bir değişken adı olmalı; burada {1:ifade} var.",
                Hint = "Karşılaştırmak istediysen `==` kullan. Değer vermek istediysen solda sadece değişken adı olsun: `x = 5`",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^cannot assign to (True|False|None)$",
                Category = ErrorCategory.Atama,
                Title = "Özel kelimeye değer verilemez",
                Text = "`{1}` Python'un özel bir kelimesi; ona değer verilemez.",
                Hint = "Değişkenine başka bir isim ver.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^cannot assign to (.+)$",
                Category = ErrorCategory.Atama,
                Title = "Buraya değer verilemez",
                Text = "Eşittirin (`=`) solunda {1:ifade} var. Değer sadece bir değişkene (ya da listenin bir elemanına) verilebilir.",
                Hint = "Solda bir değişken adı olsun, örneğin: `x = 5`",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^'(.+)' is an illegal expression for augmented assignment$",
                Category = ErrorCategory.Atama,
                Title = "`+=` burada kullanılamaz",
                Text = "`+=`, `-=` gibi işlemler sadece bir değişkene uygulanabilir; burada {1:ifade} var.",
                Hint = "Solda bir değişken adı olsun, örneğin: `sayac += 1`",
            },

            // --- yazım ---
            new Rule
            {
                Types = Syntax,
                Pattern = @"^expected ':'$",
                Category = ErrorCategory.Yazim,
                Title = "İki nokta eksik",
                Text = "Bu satır bir blok başlatıyor (`if`, `for`, `while`, `def`...) ama sonunda iki nokta (`:`) yok.",
                Hint = "Satırın sonuna `:` ekle.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^expected '\('$",
                Category = ErrorCategory.Yazim,
                Title = "Parantez eksik",
                Text = "Fonksiyon tanımında isimden hemen sonra parantez gelmeli.",
                Hint = "Örnek: `def selamla():`",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^'(.)' was never closed$",
                Category = ErrorCategory.Yazim,
                Title = "Kapanmamış parantez",
                Text = "Bu satırda açılan `{1}` hiç kapanmamış.",
                Hint = "Kapatan işareti (`{1:kapanis}`) ekle.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^unmatched '(.)'$",
                Category = ErrorCategory.Yazim,
                Title = "Fazladan kapanış",
                Text = "`{1}` işareti var ama onunla eşleşen bir açılış yok.",
                Hint = "Fazla olan kapanışı sil ya da eksik açılışı ekle.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^closing parenthesis '(.)' does not match opening parenthesis '(.)'",
                Category = ErrorCategory.Yazim,
                Title = "Parantezler uyuşmuyor",
                Text = "`{2}` ile açılan şey `{1}` ile kapatılmış. Açılış ve kapanış aynı türde olmalı: `( )`, `[ ]`, `{ }`.",
                Hint = "Kapanışı `{2:kapanis}` yap.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^unterminated string literal",
                Category = ErrorCategory.Yazim,
                Title = "Kapanmamış tırnak",
                Text = "Bir metin tırnakla başlamış ama aynı satırda kapanmamış.",
                Hint = "Metnin sonuna, başladığın tırnağın aynısını ekle.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^unterminated triple-quoted string literal",
                Category = ErrorCategory.Yazim,
                Title = "Kapanmamış üçlü tırnak",
                Text = "`\"\"\"` ile başlayan uzun metin hiç kapanmamış.",
                Hint = "Metnin sonuna `\"\"\"` ekle.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^invalid character '([“”‘’])'",
                Category = ErrorCategory.Yazim,
                Title = "Kıvrık tırnak",
                Text = "`{1}` düz tırnak değil, kıvrık tırnak. Telefon klavyeleri bazen düz tırnağı kendiliğinden buna çevirir; Python ise sadece düz tırnağı tanır.",
                Hint = "Düz tırnak kullan: `\"` ya da `'`",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^invalid character '(.+)'",
                Category = ErrorCategory.Yazim,
                Title = "Geçersiz karakter",
                Text = "`{1}` işareti Python kodunda kullanılamaz.",
                Hint = "Bu karakteri sil ya da değiştir.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^invalid non-printable character",
                Category = ErrorCategory.Yazim,
                Title = "Görünmez karakter",
                Text = "Bu satırda gözle görünmeyen ama Python'un tanımadığı bir karakter var. Çoğu zaman kopyala-yapıştır ya da klavye yüzünden normal boşluk yerine özel bir boşluk girer.",
                Hint = "Satırdaki boşlukları silip yeniden yaz.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^invalid (decimal|hexadecimal|octal|binary) literal$|^invalid digit",
                Category = ErrorCategory.Yazim,
                Title = "Geçersiz sayı",
                Text = "Bir sayının hemen yanına harf yazılmış (örneğin `3kaya`). Python bunu ne sayı ne de isim olarak okuyabilir.",
                Hint = "Sayıyla ismin arasına bir işlem koy (`3 * kaya`). Değişken isimleri rakamla başlayamaz.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^leading zeros in decimal integer literals",
                Category = ErrorCategory.Yazim,
                Title = "Başta sıfır",
                Text = "Python'da tam sayılar 0 ile başlayamaz (`007` gibi).",
                Hint = "Baştaki sıfırları sil: `7`",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^Missing parentheses in call to '(\w+)'",
                Category = ErrorCategory.Yazim,
                Title = "`{1}` için parantez gerekli",
                Text = "Python 3'te `{1}` bir fonksiyondur; yazdırılacak şey parantez içine alınmalı.",
                Hint = "Örnek: `{1}(\"merhaba\")`",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^invalid syntax\. Perhaps you forgot a comma\?$",
                Category = ErrorCategory.Yazim,
                Title = "Virgül eksik",
                Text = "Parantezin içinde yan yana iki şey var ama aralarında virgül yok.",
                Hint = "Aralarına virgül (`,`) koy.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^expected 'else' after 'if' expression$",
                Category = ErrorCategory.Yazim,
                Title = "`else` eksik",
                Text = "Tek satırlık koşulda (`a if koşul else b`) `else` kısmı eksik.",
                Hint = "Sonuna `else` ve bir değer ekle.",
            },

            // --- fonksiyon ve döngü kuralları (derleme sırasında) ---
            new Rule
            {
                Types = Syntax,
                Pattern = @"^'return' outside function$",
                Category = ErrorCategory.Fonksiyon,
                Title = "`return` fonksiyon dışında",
                Text = "`return` sadece bir fonksiyonun (`def`) içinde kullanılabilir.",
                Hint = "`return` satırını bir fonksiyonun içine al ya da sil.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^'break' outside loop$",
                Category = ErrorCategory.Dongu,
                Title = "`break` döngü dışında",
                Text = "`break` bir döngüyü (`for`, `while`) erken bitirmek içindir; burada ortada döngü yok.",
                Hint = "`break` satırını bir döngünün içine al.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^'continue' not properly in loop$",
                Category = ErrorCategory.Dongu,
                Title = "`continue` döngü dışında",
                Text = "`continue` döngünün bir sonraki turuna geçmek içindir; burada ortada döngü yok.",
                Hint = "`continue` satırını bir döngünün içine al.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^duplicate argument '(.+)' in function definition$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Aynı parametre iki kez",
                Text = "Fonksiyonun parametrelerinde `{1}` iki kez yazılmış.",
                Hint = "Parametrelere farklı isimler ver.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^parameter without a default follows parameter with a default$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Parametre sırası",
                Text = "Varsayılan değeri olan bir parametreden (`b=1` gibi) sonra, varsayılanı olmayan bir parametre gelemez.",
                Hint = "Varsayılanı olmayanları başa al: `def f(a, b=1):`",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^positional argument follows keyword argument$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Argüman sırası",
                Text = "İsimle verilen bir argümandan (`sep=\"-\"` gibi) sonra, isimsiz bir argüman gelemez.",
                Hint = "İsimsiz argümanları başa al.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^keyword argument repeated: (.+)$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Argüman iki kez verilmiş",
                Text = "`{1}=` argümanı aynı çağrıda iki kez yazılmış.",
                Hint = "Birini sil.",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^invalid syntax$",
                Category = ErrorCategory.Yazim,
                Title = "Yazım hatası",
                Text = "Python bu satırı anlayamadı. Genellikle bir işaret eksik ya da fazladır, ya da bir kelime yanlış yerde kullanılmıştır.",
                Hint = "Satırı dikkatle oku: parantezler, tırnaklar, iki noktalar ve virgüller yerinde mi?",
            },
            new Rule
            {
                Types = Syntax,
                Pattern = @"^",
                Category = ErrorCategory.Yazim,
                Title = "Yazım hatası",
                Text = "Python bu satırı anlayamadı. Genellikle bir işaret eksik ya da fazladır, ya da bir kelime yanlış yerde kullanılmıştır.",
                Hint = "Satırı dikkatle oku: parantezler, tırnaklar, iki noktalar ve virgüller yerinde mi?",
                Generic = true,
            },

            // --- isimler ---
            new Rule
            {
                Types = NameErr,
                Pattern = @"^name '(.+)' is not defined$",
                Category = ErrorCategory.Isim,
                Title = "Tanımsız isim",
                Text = "`{1}` adında bir değişken ya da fonksiyon yok. Ya henüz değer verilmedi ya da adı farklı yazıldı.",
                HintFn = (g, ctx) =>
                {
                    if (ctx.Suggestion != null) return "Belki `" + ctx.Suggestion + "` yazmak istedin?";
                    if (ctx.ImportName != null)
                        return "`" + ctx.ImportName + "` Python'un hazır modüllerinden birinin adı. Gerçek Python'da önce `import "
                            + ctx.ImportName + "` gerekir; bu oyunda modüller henüz yok.";
                    return "Adı doğru yazdığından ve bu satırdan önce ona değer verdiğinden emin ol.";
                },
            },
            new Rule
            {
                Types = new[] { "UnboundLocalError" },
                Pattern = @"^cannot access local variable '(.+)' where it is not associated with a value$",
                Category = ErrorCategory.Isim,
                Title = "Değer verilmeden kullanıldı",
                Text = "`{1}` bu fonksiyonun içinde daha sonra değer alıyor; bu yüzden Python onu fonksiyona ait sayar. Ama bu satırda henüz değeri yok.",
                Hint = "Dışarıdaki `{1}` değişkenini kullanmak istiyorsan fonksiyonun başına `global {1}` yaz; değilse önce ona değer ver.",
            },
            new Rule
            {
                Types = NameErr,
                Pattern = @"^cannot access free variable '(.+)'",
                Category = ErrorCategory.Isim,
                Title = "Değer verilmeden kullanıldı",
                Text = "`{1}` dıştaki fonksiyonda tanımlı ama bu satır çalıştığında henüz değeri yok.",
                Hint = "`{1}` değişkenine, onu kullanan satırdan önce değer ver.",
            },
            new Rule
            {
                Types = new[] { "NameError", "UnboundLocalError" },
                Pattern = @"^",
                Category = ErrorCategory.Isim,
                Title = "İsim hatası",
                Text = "Kullanılan bir isim (değişken ya da fonksiyon) bulunamadı.",
                Hint = "İsmin doğru yazıldığından ve önceden değer aldığından emin ol.",
                Generic = true,
            },

            // --- türler ---
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^can only concatenate str \(not ""(.+)""\) to str$",
                Category = ErrorCategory.Tur,
                Title = "Metne sayı eklenemez",
                Text = "`+` ile bir metnin yanına sadece başka bir metin eklenebilir; burada {1:tur} var.",
                Hint = "Sayıyı metne çevir: `str(enerji)`. Ya da `print` içinde virgül kullan: `print(\"Enerji:\", enerji)`",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^can only concatenate (list|tuple) \(not ""(.+)""\) to (?:list|tuple)$",
                Category = ErrorCategory.Tur,
                Title = "Bu ikisi birleştirilemez",
                Text = "`+` ile bir {1:tur} yanına sadece başka bir {1:tur} eklenebilir; burada {2:tur} var.",
                HintFn = (g, ctx) => g[1] == "list" ? "Tek bir eleman eklemek için `liste.append(x)` kullan." : null,
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^unsupported operand type\(s\) for (.+): '(.+)' and '(.+)'$",
                Category = ErrorCategory.Tur,
                Title = "Bu işlem bu türlerle yapılamaz",
                Text = "{2:tur} ile {3:tur} arasında `{1}` işlemi yapılamaz.",
                HintFn = (g, ctx) =>
                {
                    if (g[2] == "NoneType" || g[3] == "NoneType") return TipNone;
                    if (g[2] == "str" || g[3] == "str") return "Metni sayıya çevirmek için `int()`, sayıyı metne çevirmek için `str()` kullan.";
                    return null;
                },
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^can't multiply sequence by non-int of type '(.+)'$",
                Category = ErrorCategory.Tur,
                Title = "Tam sayı ile çarpılmalı",
                Text = "Bir metin ya da liste sadece tam sayı ile çarpılabilir (tekrarlamak için); burada {1:tur} var.",
                Hint = "Örnek: `\"ab\" * 3` sonucu `\"ababab\"` olur. Ondalıklı sayıyı `int()` ile tam sayıya çevir.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^'(.+)' not supported between instances of '(.+)' and '(.+)'$",
                Category = ErrorCategory.Tur,
                Title = "Karşılaştırılamaz",
                Text = "{2:tur} ile {3:tur}, `{1}` ile karşılaştırılamaz.",
                Hint = "İkisini aynı türe çevir; örneğin `int(\"5\") < 7`",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^bad operand type for unary (.): '(.+)'$",
                Category = ErrorCategory.Tur,
                Title = "Bu işaret burada kullanılamaz",
                Text = "{2:tur} önüne `{1}` işareti konamaz.",
                Hint = null,
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^'(.+)' object is not callable$",
                Category = ErrorCategory.Tur,
                Title = "Bu bir fonksiyon değil",
                Text = "{1:tur} bir fonksiyon değil; parantezle çağrılamaz.",
                Hint = "Bir değişkene fonksiyonla aynı ismi vermiş olabilir misin? Ya da sonuna yanlışlıkla `()` koymuş olabilirsin.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^'(.+)' object is not iterable$",
                Category = ErrorCategory.Dongu,
                Title = "Bunun üzerinde dönülemez",
                Text = "`for` döngüsü {1:tur} üzerinde dönemez; liste, metin ya da `range` gerekir.",
                Hint = "Bir sayı kadar dönmek için `range` kullan: `for i in range(5):`",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^'(.+)' object is not subscriptable$",
                Category = ErrorCategory.Indeks,
                Title = "Köşeli parantez kullanılamaz",
                Text = "{1:tur} içinden köşeli parantezle (`[ ]`) eleman alınamaz.",
                HintFn = (g, ctx) => g[1] == "NoneType" ? TipNone : "Köşeli parantez liste ve metinlerde kullanılır.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^'(.+)' object does not support item assignment$",
                Category = ErrorCategory.Tur,
                Title = "Elemanı değiştirilemez",
                Text = "{1:tur} içindeki bir eleman sonradan değiştirilemez.",
                HintFn = (g, ctx) => g[1] == "str" ? "Yeni bir metin oluştur, örneğin: `s = \"X\" + s[1:]`" : null,
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^(list|string|tuple) indices must be integers(?: or slices)?, not '?(\w+)'?$",
                Category = ErrorCategory.Indeks,
                Title = "Sıra numarası tam sayı olmalı",
                Text = "Köşeli parantezin içine bir sıra numarası (tam sayı) yazılmalı; burada {2:tur} var.",
                Hint = "Örnek: `liste[0]`. Ondalıklı sayıyı `int()` ile çevir.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^object of type '(.+)' has no len\(\)$",
                Category = ErrorCategory.Tur,
                Title = "Uzunluğu ölçülemez",
                Text = "`len()` metin, liste gibi şeylerin uzunluğunu ölçer; {1:tur} için kullanılamaz.",
                Hint = null,
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^'(.+)' object cannot be interpreted as an integer$",
                Category = ErrorCategory.Tur,
                Title = "Tam sayı gerekiyor",
                Text = "Burada tam sayı gerekiyor ama {1:tur} verilmiş.",
                Hint = "`int()` ile tam sayıya çevir.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^cannot unpack non-iterable (.+) object$",
                Category = ErrorCategory.Atama,
                Title = "Değerler dağıtılamaz",
                Text = "Birden çok değişkene aynı anda değer vermek için sağda bir liste ya da demet olmalı; burada {1:tur} var.",
                Hint = null,
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^(.+)\(\) missing (\d+) required positional arguments?: (.+)$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Eksik değer",
                Text = "`{1:ad}()` fonksiyonu {2} değer daha bekliyor: {3:ve}.",
                Hint = "Çağırırken parantezin içine eksik değerleri de yaz.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^(.+)\(\) takes (\d+) positional arguments? but (\d+) (?:was|were) given$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Fazla değer",
                Text = "`{1:ad}()` fonksiyonu {2} değer alıyor ama {3} değer verilmiş.",
                Hint = "Fazla değerleri sil ya da fonksiyona yeni bir parametre ekle.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^(.+)\(\) takes from (\d+) to (\d+) positional arguments but (\d+) were given$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Fazla değer",
                Text = "`{1:ad}()` fonksiyonu en az {2}, en fazla {3} değer alıyor ama {4} değer verilmiş.",
                Hint = "Fazla değerleri sil.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^(.+)\(\) got an unexpected keyword argument '(.+)'$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Böyle bir parametre yok",
                Text = "`{1:ad}()` fonksiyonunun `{2}` adında bir parametresi yok.",
                Hint = "Parametre adını, fonksiyonun tanımındaki (`def`) adla aynı yaz.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^(.+)\(\) got multiple values for argument '(.+)'$",
                Category = ErrorCategory.Fonksiyon,
                Title = "Aynı parametreye iki değer",
                Text = "`{2}` parametresine iki kez değer verilmiş: bir kez sırayla, bir kez adıyla.",
                Hint = "İkisinden birini sil.",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^(sep|end) must be None or a string, not (.+)$",
                Category = ErrorCategory.Tur,
                Title = "`{1}` metin olmalı",
                Text = "`print`'in `{1}` ayarı bir metin olmalı; burada {2:tur} var.",
                Hint = "Örnek: `print(1, 2, {1}=\"-\")`",
            },
            new Rule
            {
                Types = TypeErr,
                Pattern = @"^",
                Category = ErrorCategory.Tur,
                Title = "Tür uyuşmazlığı",
                Text = "Bu işlem, verilen değerin türüyle (sayı, metin, liste...) yapılamıyor.",
                Hint = "Değerlerin türlerini kontrol et; gerekirse `int()`, `str()` ile çevir.",
                Generic = true,
            },

            // --- değerler ---
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^invalid literal for int\(\) with base \d+: (.+)$",
                Category = ErrorCategory.Donusum,
                Title = "Tam sayıya çevrilemez",
                Text = "{1} metni bir tam sayıya çevrilemiyor.",
                HintFn = (g, ctx) => g[1].Contains(".")
                    ? "Ondalıklı sayılar için önce `float()` kullan, gerekirse sonra `int()`."
                    : "`int()` sadece rakamlardan oluşan metinleri çevirebilir.",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^could not convert string to float: (.+)$",
                Category = ErrorCategory.Donusum,
                Title = "Sayıya çevrilemez",
                Text = "{1} metni bir sayıya çevrilemiyor.",
                Hint = "Metin sadece bir sayı içermeli, örneğin `\"2.5\"`.",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^list\.remove\(x\): x not in list$",
                Category = ErrorCategory.Liste,
                Title = "Silinecek eleman yok",
                Text = "`remove()` ile silmek istediğin değer listede yok.",
                Hint = "Önce kontrol et: `if x in liste:`",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^(.+) is not in list$",
                Category = ErrorCategory.Liste,
                Title = "Listede yok",
                Text = "{1} listede bulunmuyor, bu yüzden sırası (`index`) bulunamadı.",
                Hint = "Önce kontrol et: `if x in liste:`",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^(min|max)\(\) iterable argument is empty$",
                Category = ErrorCategory.Liste,
                Title = "Liste boş",
                Text = "`{1}()` boş bir listede sonuç bulamaz.",
                Hint = "Önce listenin boş olmadığını kontrol et: `if liste:`",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^not enough values to unpack \(expected (\d+), got (\d+)\)$",
                Category = ErrorCategory.Atama,
                Title = "Değerler az",
                Text = "Solda {1} değişken var ama sağda sadece {2} değer var.",
                Hint = "İki taraftaki sayılar eşit olmalı.",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^too many values to unpack \(expected (\d+)\)$",
                Category = ErrorCategory.Atama,
                Title = "Değerler fazla",
                Text = "Solda {1} değişken var ama sağda daha fazla değer var.",
                Hint = "İki taraftaki sayılar eşit olmalı.",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^range\(\) arg 3 must not be zero$",
                Category = ErrorCategory.Dongu,
                Title = "`range` adımı sıfır olamaz",
                Text = "`range`'in üçüncü sayısı (adım) 0 olursa sayılar hiç ilerlemez.",
                Hint = "Adımı 1 ya da başka bir sayı yap; geriye saymak için eksi sayı kullan: `range(10, 0, -1)`",
            },
            new Rule
            {
                Types = ValueErr,
                Pattern = @"^",
                Category = ErrorCategory.Donusum,
                Title = "Uygunsuz değer",
                Text = "Değerin türü doğru ama kendisi bu işlem için uygun değil.",
                Hint = null,
                Generic = true,
            },

            // --- sıra numaraları ---
            new Rule
            {
                Types = IndexErr,
                Pattern = @"^(list|string|tuple|range object) index out of range$",
                Category = ErrorCategory.Indeks,
                Title = "Sıra numarası dışarıda",
                Text = "Bu {1:tur} içinde bu sıra numarasında bir eleman yok. Unutma: sayma 0'dan başlar; 3 elemanlı bir listede son sıra numarası 2'dir.",
                Hint = "Uzunluğa `len()` ile bakabilirsin.",
            },
            new Rule
            {
                Types = IndexErr,
                Pattern = @"^list assignment index out of range$",
                Category = ErrorCategory.Indeks,
                Title = "Sıra numarası dışarıda",
                Text = "Listede olmayan bir sıra numarasına değer verilemez.",
                Hint = "Listenin sonuna eklemek için `liste.append(x)` kullan.",
            },
            new Rule
            {
                Types = IndexErr,
                Pattern = @"^pop from empty list$",
                Category = ErrorCategory.Liste,
                Title = "Liste boş",
                Text = "Boş bir listeden `pop()` ile eleman alınamaz.",
                Hint = "Önce listenin boş olmadığını kontrol et: `if liste:`",
            },
            new Rule
            {
                Types = IndexErr,
                Pattern = @"^pop index out of range$",
                Category = ErrorCategory.Liste,
                Title = "Sıra numarası dışarıda",
                Text = "`pop()` ile istenen sıra numarasında bir eleman yok.",
                Hint = "Sayma 0'dan başlar; son eleman için sıra numarası vermeden `pop()` yaz.",
            },
            new Rule
            {
                Types = IndexErr,
                Pattern = @"^",
                Category = ErrorCategory.Indeks,
                Title = "Sıra numarası dışarıda",
                Text = "İstenen sıra numarasında bir eleman yok.",
                Hint = "Sayma 0'dan başlar; uzunluğa `len()` ile bakabilirsin.",
                Generic = true,
            },

            // --- özellik/komut ---
            new Rule
            {
                Types = AttrErr,
                Pattern = @"^'(.+)' object has no attribute '(.+)'$",
                Category = ErrorCategory.Ozellik,
                Title = "Böyle bir komut yok",
                Text = "{1:tur} için `{2}` diye bir komut ya da özellik yok.",
                HintFn = (g, ctx) =>
                {
                    if (ctx.Suggestion != null) return "Belki `" + ctx.Suggestion + "` yazmak istedin?";
                    if (g[1] == "NoneType") return TipNone;
                    return null;
                },
            },
            new Rule
            {
                Types = AttrErr,
                Pattern = @"^",
                Category = ErrorCategory.Ozellik,
                Title = "Özellik hatası",
                Text = "Bu değerin böyle bir özelliği yok ya da değiştirilemez.",
                Hint = null,
                Generic = true,
            },

            // --- diğer ---
            new Rule
            {
                Types = new[] { "ZeroDivisionError" },
                Pattern = @"^0\.0 cannot be raised to a negative power$",
                Category = ErrorCategory.SifiraBolme,
                Title = "Sıfıra bölme",
                Text = "0'ın eksi kuvveti alınamaz; bu da sıfıra bölmek demektir.",
                Hint = null,
            },
            new Rule
            {
                Types = new[] { "ZeroDivisionError" },
                Pattern = @"^",
                Category = ErrorCategory.SifiraBolme,
                Title = "Sıfıra bölme",
                Text = "Bir sayı sıfıra bölünemez.",
                Hint = "Bölmeden önce kontrol et: `if bolen != 0:`",
            },
            new Rule
            {
                Types = new[] { "RecursionError" },
                Pattern = @"^",
                Category = ErrorCategory.Fonksiyon,
                Title = "Fonksiyon kendini durmadan çağırıyor",
                Text = "Bir fonksiyon kendini çağırıyor ama hiç durmuyor. Python 1000 iç içe çağrıdan sonra durdurur.",
                Hint = "Fonksiyonda, kendini çağırmayı bitiren bir koşul (`if ...: return`) olduğundan emin ol.",
            },
            new Rule
            {
                Types = new[] { "OverflowError" },
                Pattern = @"^",
                Category = ErrorCategory.Tur,
                Title = "Sayı çok büyük",
                Text = "Sonuç, ondalıklı sayıların sığabileceği sınırdan büyük.",
                Hint = null,
            },
        };

        public static readonly Dictionary<string, HaltText> HaltTexts = new Dictionary<string, HaltText>
        {
            ["steps"] = new HaltText
            {
                Category = ErrorCategory.Dongu,
                Title = "Kod durmuyor",
                Text = "Kodun çok uzun süre çalıştı ve durduruldu. Büyük ihtimalle bitmeyen bir döngü var: `while` koşulu hiç yanlış olmuyor olabilir.",
                Hint = "Döngünün içinde, koşulu sonunda yanlış yapacak bir satır olduğundan emin ol.",
            },
            ["size"] = new HaltText
            {
                Category = ErrorCategory.Sinir,
                Title = "Değer çok büyük",
                Text = "Kod, telefonun belleğine sığmayacak kadar büyük bir sayı ya da metin üretmeye çalıştı.",
                Hint = "Sayıları ve tekrar sayılarını küçült.",
            },
        };

        /// <summary>Desteklenmeyen özelliklerin Türkçe adları; {detail} yerine ayrıntı (örn. "input") yazılır.</summary>
        public static readonly Dictionary<string, FeatureText> FeatureNames = new Dictionary<string, FeatureText>
        {
            [Feature.Annotation] = new FeatureText { Name = "tür ipuçları (`x: int`)", Hint = "Tür ipucunu silebilirsin; kod onsuz da çalışır." },
            [Feature.Assert] = new FeatureText { Name = "`assert`" },
            [Feature.Async] = new FeatureText { Name = "eşzamansız kod (`async`)" },
            [Feature.Await] = new FeatureText { Name = "`await`" },
            [Feature.Builtin] = new FeatureText { Name = "`{detail}` fonksiyonu" },
            [Feature.Bytes] = new FeatureText { Name = "bayt metinleri (`b\"...\"`)" },
            [Feature.Class] = new FeatureText { Name = "sınıflar (`class`)" },
            [Feature.Complex] = new FeatureText { Name = "karmaşık sayılar" },
            [Feature.Comprehension] = new FeatureText
            {
                Name = "tek satırda liste üretme (`[x for x in ...]`)",
                Hint = "Aynı işi bir `for` döngüsü ve `append()` ile yapabilirsin.",
            },
            [Feature.Decorator] = new FeatureText { Name = "dekoratörler (`@`)" },
            [Feature.Del] = new FeatureText { Name = "silme (`del`)", Hint = "Listeden silmek için `liste.remove(x)` ya da `liste.pop()` kullan." },
            [Feature.Dict] = new FeatureText { Name = "sözlükler (`{\"anahtar\": değer}`)" },
            [Feature.Ellipsis] = new FeatureText { Name = "üç nokta (`...`)", Hint = "Boş bir blok için `pass` yaz." },
            [Feature.FString] = new FeatureText { Name = "f-metinleri (`f\"...\"`)", Hint = "Şimdilik birleştirme kullan: `\"Enerji: \" + str(enerji)`" },
            [Feature.Import] = new FeatureText { Name = "modül ekleme (`import`)" },
            [Feature.Lambda] = new FeatureText { Name = "`lambda` fonksiyonları", Hint = "Aynı işi `def` ile bir fonksiyon yazarak yapabilirsin." },
            [Feature.Method] = new FeatureText { Name = "`{detail}` komutu" },
            [Feature.Raise] = new FeatureText { Name = "hata fırlatma (`raise`)" },
            [Feature.Set] = new FeatureText { Name = "kümeler (`{1, 2}`)" },
            [Feature.SliceAssign] = new FeatureText { Name = "dilime değer verme (`liste[1:3] = ...`)" },
            [Feature.StarArgs] = new FeatureText { Name = "yıldızlı argümanlar (`*liste`)" },
            [Feature.StarParams] = new FeatureText { Name = "yıldızlı parametreler (`*args`)" },
            [Feature.StrFormat] = new FeatureText { Name = "`%` ile metin biçimlendirme", Hint = "Şimdilik birleştirme kullan: `\"Enerji: \" + str(enerji)`" },
            [Feature.Try] = new FeatureText { Name = "hata yakalama (`try`/`except`)" },
            [Feature.Walrus] = new FeatureText { Name = "`:=` işleci" },
            [Feature.With] = new FeatureText { Name = "`with` bloğu" },
            [Feature.Yield] = new FeatureText { Name = "`yield`" },
        };
    }
}
