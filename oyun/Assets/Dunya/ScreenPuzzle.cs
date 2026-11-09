// Ekran bulmacası (Bölge 8'den itibaren, tür B): robot yürümez. Oyuncu adı belli bir fonksiyon yazar; oyun onu her
// satırın girdisiyle çağırır, istasyon ekranına `girdi → cevap → ✓/✗` yazar. Görünen satırlar görev kartında baştan
// görünür, gizliler yalnızca çalıştırınca: görünenleri ezberleyen kod gizlide takılır. Bölüm dosyasında: "ekran".

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static MarsKod.Dunya.DataFields;

namespace MarsKod.Dunya
{
    /// <summary>Ekranın bir satırı: fonksiyona verilen girdi ve beklenen cevap (motorun türleriyle: string, BigInteger, bool)</summary>
    public sealed class ScreenRow
    {
        public object Input;
        public object Expected;
        /// <summary>Görev kartında baştan görünür mü (değilse gizli: yalnızca çalıştırınca)</summary>
        public bool Visible;
    }

    /// <summary>Fonksiyonun bir satıra verdiği cevap</summary>
    public sealed class ScreenAnswer
    {
        public ScreenRow Row;
        /// <summary>Fonksiyonun döndürdüğü değer (return yoksa null = None)</summary>
        public object Actual;
        public bool Correct;
    }

    public sealed class ScreenPuzzle
    {
        /// <summary>Ekran çağrısında kullanılan gizli adlar (oyuncunun değişkenleriyle karışmasın diye __ ile; kayıtta görünmez)</summary>
        public const string InputName = "__girdi__", AnswerName = "__cevap__";
        public const int MinRows = 3;

        /// <summary>Ekranın çağırdığı fonksiyonun adı</summary>
        public readonly string Function;
        /// <summary>Satırlar, ekranda göründüğü sırayla</summary>
        public readonly IReadOnlyList<ScreenRow> Rows;

        public IEnumerable<ScreenRow> VisibleRows => Rows.Where(r => r.Visible);

        ScreenPuzzle(string function, List<ScreenRow> rows)
        {
            Function = function;
            Rows = rows;
        }

        // ---- dosyadan okuma ----

        /// <summary>Bölüm dosyasındaki "ekran" alanının değerini okur; yanlışta DataFormatError verir.</summary>
        public static ScreenPuzzle Parse(object value)
        {
            if (!(value is Dictionary<string, object> d))
                throw new DataFormatError("\"ekran\" bir nesne olmalı: { \"fonksiyon\": \"...\", \"satirlar\": [...] }.");
            CheckKeys(d, new[] { "fonksiyon", "satirlar" }, "\"ekran\"");
            string function = Text(d, "fonksiyon");
            var nameProblem = Level.RoutineNameProblem(function, "\"ekran\" içindeki fonksiyon adı \"" + function + "\"", "fonksiyon");
            if (nameProblem != null) throw new DataFormatError(nameProblem);
            if (!(Field(d, "satirlar") is List<object> list))
                throw new DataFormatError("\"ekran\" içindeki \"satirlar\" bir liste olmalı: [{\"girdi\": 3, \"cevap\": 6, \"gorunur\": true}, ...].");
            var rows = list.Select((item, i) => ReadRow(item, i + 1)).ToList();
            if (rows.Count < MinRows)
                throw new DataFormatError("\"ekran\" en az " + MinRows + " satır içermeli (şimdi " + rows.Count + "): örnekler ve en az bir gizli satır.");
            if (!rows.Any(r => r.Visible))
                throw new DataFormatError("\"ekran\" içinde en az bir satır görünür olmalı (\"gorunur\": true): oyuncu ne beklendiğini görsün.");
            if (rows.All(r => r.Visible))
                throw new DataFormatError("\"ekran\" içinde en az bir satır gizli olmalı: yalnızca örnekleri ezberleyen kod takılsın.");
            if (rows.All(r => SameAnswer(rows[0].Expected, r.Expected)))
                throw new DataFormatError("\"ekran\" içindeki cevapların hepsi aynı: tek bir cevabı ezberleyen kod (return True gibi) geçerdi.");
            return new ScreenPuzzle(function, rows);
        }

        static ScreenRow ReadRow(object item, int number)
        {
            string where = "\"ekran\" içindeki " + number + ". satır";
            if (!(item is Dictionary<string, object> d))
                throw new DataFormatError(where + " bir nesne olmalı: {\"girdi\": 3, \"cevap\": 6}.");
            CheckKeys(d, new[] { "girdi", "cevap", "gorunur" }, where);
            return new ScreenRow
            {
                Input = ReadValue(Field(d, "girdi"), where + " (girdi)"),
                Expected = ReadValue(Field(d, "cevap"), where + " (cevap)"),
                Visible = d.ContainsKey("gorunur") && Flag(d, "gorunur"),
            };
        }

        // JSON değeri motorun türüne: metin aynen, true/false bool, tam sayı BigInteger
        static object ReadValue(object value, string where)
        {
            switch (value)
            {
                case string s: return s;
                case bool b: return b;
                case double n when n == Math.Floor(n) && Math.Abs(n) < 1e15: return new BigInteger(n);
                case double _: throw new DataFormatError(where + " tam sayı olmalı (ondalıklı sayı ekranda kullanılmaz).");
                default: throw new DataFormatError(where + " bir metin (\"Aras\"), tam sayı ya da true/false olmalı.");
            }
        }

        // ---- cevapları değerlendirme ----

        /// <summary>Cevap aynı mı: türüyle birlikte (True ile 1, "6" ile 6 aynı değildir)</summary>
        public static bool SameAnswer(object expected, object actual)
        {
            if (expected == null || actual == null) return expected == null && actual == null;
            if (expected is bool e) return actual is bool a && a == e;
            if (expected is BigInteger n) return actual is BigInteger m && m == n;
            if (expected is string s) return actual is string t && string.Equals(s, t, StringComparison.Ordinal);
            return false;
        }

        /// <summary>Kodda ekranın sorduğu adla bir fonksiyon yok</summary>
        public RoutineProblem MissingFunction() => new RoutineProblem
        {
            Title = "Ekran: `" + Function + "` fonksiyonu yok",
            Text = "Ekran her satır için `" + Function + "(...)` diye soruyor ama kodda bu adla bir fonksiyon yok. `def " + Function + "(...):` ile yaz; ad harfi harfine aynı olmalı.",
        };

        /// <summary>İlk yanlış cevabın açıklaması; hepsi doğruysa null</summary>
        public RoutineProblem ProblemOf(IReadOnlyList<ScreenAnswer> answers)
        {
            var wrong = answers.FirstOrDefault(a => !a.Correct);
            if (wrong == null) return null;
            string input = ProgramRun.ValueText(wrong.Row.Input);
            string expected = ProgramRun.ValueText(wrong.Row.Expected);
            string start = (wrong.Row.Visible ? "" : "Bu satır görev kartında yoktu: kod yalnız örneklere göre yazılmış olabilir. ")
                + "`" + Function + "(" + input + ")` için `" + expected + "` bekleniyordu, ";
            return new RoutineProblem { Title = "Ekran: " + input + " için yanlış cevap", Text = start + WhyWrong(wrong.Row.Expected, wrong.Actual) };
        }

        // Yanlış cevabın sebebi: hiç cevap yok (None), türü farklı ya da değeri farklı
        static string WhyWrong(object expected, object actual)
        {
            if (actual == null)
                return "`None` geldi: fonksiyon cevabını `return` ile vermeli. `print` ekrana yazar ama cevap vermez; `return` yazılmayan yol da `None` döndürür.";
            string got = "`" + ProgramRun.ValueText(actual) + "` geldi";
            if (expected.GetType() == actual.GetType()) return got + ".";
            if (expected is string || actual is string)
                return got + ": biri metin, öteki değil. Tırnaklı `\"6\"` metindir, tırnaksız `6` sayıdır, `True` tırnaksız yazılır.";
            return got + ": türü farklı. `True` / `False` doğru-yanlış cevabıdır, sayı değil.";
        }
    }
}
