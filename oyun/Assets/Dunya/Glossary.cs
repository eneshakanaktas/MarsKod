// Kod sözlüğü: oyuncunun istediği an bakabildiği sayfalar (komut / Python kelimesi: ne işe yarar + küçük örnek).
// Tasarım notu: docs/tasarim/kod-sozlugu.md. Metinler Resources/Sozluk/sozluk.json; biçim docs/tasarim/sozluk-dosyasi.md.
// Bir sayfanın hangi bölümde açıldığı dosyaya yazılmaz, bölüm dosyalarından türetilir (tek yerde dursun).

using System.Collections.Generic;
using System.Linq;
using MarsKod.Motor;
using static MarsKod.Dunya.DataFields;

namespace MarsKod.Dunya
{
    public sealed class GlossaryPage
    {
        /// <summary>Sayfa başlığı ("for", "Yönler")</summary>
        public string Title;
        /// <summary>Sayfanın anlattığı kelimeler (bölüm dosyalarındaki komut / Python kelimesi adlarıyla)</summary>
        public List<string> Words = new List<string>();
        /// <summary>Ne işe yarar (bir iki cümle, sade)</summary>
        public string Text;
        /// <summary>Küçük örnek kod</summary>
        public string Example;
    }

    public sealed class Glossary
    {
        public readonly List<GlossaryPage> Pages = new List<GlossaryPage>();

        static readonly string[] FileKeys = { "sayfalar" };
        static readonly string[] PageKeys = { "baslik", "kelimeler", "aciklama", "ornek" };

        /// <summary>JSON metninden sözlüğü okur; yanlışta DataFormatError verir.</summary>
        public static Glossary Parse(string json)
        {
            var d = ParseObject(json, FileKeys);
            if (!(Field(d, "sayfalar") is List<object> list) || list.Count == 0)
                throw new DataFormatError("\"sayfalar\" en az bir sayfası olan bir liste olmalı; her sayfa { \"baslik\", \"kelimeler\", \"aciklama\", \"ornek\" }.");
            var glossary = new Glossary();
            for (int i = 0; i < list.Count; i++)
            {
                string where = (i + 1) + ". sayfa";
                if (!(list[i] is Dictionary<string, object> p)) throw new DataFormatError(where + " { ... } biçiminde olmalı.");
                CheckKeys(p, PageKeys, where);
                try
                {
                    var page = new GlossaryPage
                    {
                        Title = Text(p, "baslik"),
                        Words = TextList(p, "kelimeler"),
                        Text = Text(p, "aciklama"),
                        Example = Code(p, "ornek"),
                    };
                    if (page.Words.Count == 0) throw new DataFormatError("\"kelimeler\" boş olamaz.");
                    glossary.Pages.Add(page);
                }
                catch (DataFormatError e)
                {
                    throw new DataFormatError(where + ": " + e.Message);
                }
            }
            return glossary;
        }

        /// <summary>Sayfanın açıldığı ilk bölüm: kelimelerinden biri o bölümde açılıyor; hiçbir bölümde yoksa null.</summary>
        public static int? OpensAt(GlossaryPage page, IEnumerable<Level> levels)
        {
            var sorted = levels.OrderBy(l => l.Number).ToList();
            foreach (var l in sorted)
                if (Suggestions.OpenWords(sorted, l.Number).Intersect(page.Words).Any()) return l.Number;
            return null;
        }
    }

    /// <summary>Sözlük denetleyici: bölümlerde açılan her kelimenin sayfası var mı, sayfalar bölümlerde geçiyor mu,
    /// örnekler geçerli Python mı.</summary>
    public static class GlossaryCheck
    {
        public static List<string> Problems(Glossary glossary, IList<Level> levels)
        {
            var problems = new List<string>();
            int last = levels.Count == 0 ? 0 : levels.Max(l => l.Number);
            var covered = new HashSet<string>(glossary.Pages.SelectMany(p => p.Words));
            foreach (var w in Suggestions.OpenWords(levels, last))
                if (!covered.Contains(w))
                    problems.Add("\"" + w + "\" bölümlerde açılıyor ama sözlükte sayfası yok.");
            foreach (var page in glossary.Pages)
            {
                if (Glossary.OpensAt(page, levels) == null)
                    problems.Add("\"" + page.Title + "\" sayfasının kelimeleri hiçbir bölümde açılmıyor.");
                try
                {
                    Parser.Parse(page.Example);
                }
                catch (System.Exception e) when (e is PythonException || e is UnsupportedFeature)
                {
                    problems.Add("\"" + page.Title + "\" sayfasının örneği geçerli bir Python kodu değil.");
                }
            }
            return problems;
        }
    }
}
