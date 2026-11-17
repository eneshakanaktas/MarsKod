// Rutin şartı (Bölge 6'dan itibaren, fonksiyonlar): bölüm, oyuncunun belli adımları def ile bir rutine koymasını
// isteyebilir. Şart yalnızca dünyaya bakmaz, koda bakar: rutin tanımlı mı, kaç kez çalıştı. Tek kez çalışan şey
// rutin sayılmaz (rutin tekrar kullanmak içindir). Bölüm dosyasında: "rutinler" (adlar), "rutin_sayisi" (adı serbest).
// Bölge 7'den itibaren parametre şartı da olabilir ("parametre_sarti"): rutin bilgi almalı ve en az iki farklı bilgiyle çağrılmalı.
// Bölge 8'den itibaren dönüş şartı da olabilir ("donus_sarti"): rutin cevabını `return` ile vermeli; o zaman bir kez çalışması yeter.

using System;
using System.Collections.Generic;
using System.Linq;
using MarsKod.Motor;

namespace MarsKod.Dunya
{
    /// <summary>Rutin şartı karşılanmadığında oyuncuya gösterilecek açıklama</summary>
    public sealed class RoutineProblem
    {
        public string Title;
        public string Text;
    }

    /// <summary>Zorunlu bir Python yapısı: kodda nasıl aranır ve yoksa oyuncuya ne söylenir</summary>
    public sealed class RequiredConstruct
    {
        public string Name;
        /// <summary>Bir cümlenin kendisi bu yapı mı (iç içe olanlar ayrıca gezilir)</summary>
        public Func<Stmt, bool> Matches;
        public string Title;
        public string Text;
    }

    /// <summary>Zorunlu yapı şartı ("zorunlu_yapilar"): görev bitse bile kodda listedeki yapılardan biri yoksa bölüm geçilmez
    /// (komutları alt alta yazma kestirmesi). Yeni yapı (while, def...) eklemek = aşağıdaki tabloya bir satır.</summary>
    public static class RequiredConstructs
    {
        static readonly RequiredConstruct[] Known =
        {
            new RequiredConstruct
            {
                Name = "for", Matches = s => s is For,
                Title = "Kod bitti, for döngüsü yok",
                Text = "Görev tamam, ama aynı satırları tekrar tekrar yazdın. Bu bölümde `for` döngüsüyle dene.",
            },
            new RequiredConstruct
            {
                Name = "if", Matches = s => s is If,
                Title = "Kod bitti, if yok",
                Text = "Görev tamam, ama robot hiç sormadan ilerledi. Bu bölümde `if` ile önce kontrol et.",
            },
            new RequiredConstruct
            {
                Name = "else", Matches = s => s is If i && i.OrElse != null && i.OrElse.Count > 0,
                Title = "Kod bitti, else yok",
                Text = "Görev tamam, ama robot yalnızca tek yolu biliyor. Bu bölümde `else` ile öteki durumu da yaz.",
            },
            new RequiredConstruct
            {
                Name = "while", Matches = s => s is While,
                Title = "Kod bitti, while yok",
                Text = "Görev tamam, ama robot sormadan, saydığın sayılarla ilerledi. Bu bölümde `while` ile önce sor.",
            },
            new RequiredConstruct
            {
                // degisken: "sayac = 0" ya da "toplam += 1" gibi bir atama (Assign / AugAssign)
                Name = "degisken", Matches = s => s is Assign || s is AugAssign,
                Title = "Kod bitti, değişken yok",
                Text = "Görev tamam, ama sayıyı kendin yazdın. Bu bölümde bir değişken kurup robota saydır: `sayac = 0` ve `sayac += 1`.",
            },
            new RequiredConstruct
            {
                // elif, ayrıştırıcıda "else içinde tek if" olarak durur (Parser.IfStatement)
                Name = "elif", Matches = s => s is If i && i.OrElse != null && i.OrElse.Count == 1 && i.OrElse[0] is If,
                Title = "Kod bitti, elif yok",
                Text = "Görev tamam, ama robot üçüncü yolu bilmeden ilerledi. Bu bölümde `elif` ile ikinci soruyu da yaz.",
            },
        };

        public static IEnumerable<string> Names => Known.Select(c => c.Name);

        public static bool IsKnown(string name) => Known.Any(c => c.Name == name);

        /// <summary>Kodun herhangi bir yerinde (if/for/while/def içleri dahil) geçen bilinen yapıların adları</summary>
        public static HashSet<string> Used(IEnumerable<Stmt> body)
        {
            var used = new HashSet<string>();
            Collect(body, used);
            return used;
        }

        static void Collect(IEnumerable<Stmt> body, HashSet<string> used)
        {
            foreach (var stmt in body)
            {
                foreach (var construct in Known)
                    if (construct.Matches(stmt)) used.Add(construct.Name);
                switch (stmt)
                {
                    case If i: Collect(i.Body, used); Collect(i.OrElse, used); break;
                    case For f: Collect(f.Body, used); Collect(f.OrElse, used); break;
                    case While w: Collect(w.Body, used); Collect(w.OrElse, used); break;
                    case FunctionDef d: Collect(d.Body, used); break;
                }
            }
        }

        /// <summary>İstenen yapılardan kodda olmayan ilkinin açıklaması; hepsi varsa (ya da şart yoksa) null</summary>
        public static RoutineProblem Check(IReadOnlyList<string> required, ISet<string> used)
        {
            foreach (var name in required)
            {
                if (used.Contains(name)) continue;
                var construct = Known.First(c => c.Name == name);
                return new RoutineProblem { Title = construct.Title, Text = construct.Text };
            }
            return null;
        }
    }

    public sealed class RoutineRule
    {
        /// <summary>Bir rutinin sayılması için en az kaç kez çalışması gerekir</summary>
        public const int MinRuns = 2;

        public static readonly RoutineRule None = new RoutineRule(new string[0], 0);

        /// <summary>Adıyla istenen rutinler</summary>
        public readonly IReadOnlyList<string> Names;
        /// <summary>Adı serbest: en az kaç farklı rutin (adlı olanlar dahil)</summary>
        public readonly int MinCount;

        /// <summary>Parametre şartında bir rutinin en az kaç farklı bilgiyle çağrılması gerekir</summary>
        public const int MinDistinctArguments = 2;

        /// <summary>Parametre şartı: şarta giren her rutin parametre almalı ve farklı bilgilerle çağrılmalı</summary>
        public readonly bool NeedsParameters;

        /// <summary>Dönüş şartı: şarta giren her rutin cevabını değer veren bir return ile vermeli</summary>
        public readonly bool NeedsReturn;

        /// <summary>Bir rutinin sayılması için kaç kez çalışması gerekir: değer döndüren rutinin cevabı kullanılır, bir kez yeter</summary>
        public int RequiredRuns => NeedsReturn ? 1 : MinRuns;

        public RoutineRule(IEnumerable<string> names, int minCount, bool needsParameters = false, bool needsReturn = false)
        {
            Names = names.ToList();
            MinCount = minCount;
            NeedsParameters = needsParameters;
            NeedsReturn = needsReturn;
        }

        public bool IsEmpty => Names.Count == 0 && MinCount == 0;

        /// <summary>Kodun çalışma kaydına bakar: rutinler tanımlı mı, kaç kez çalıştı, (istendiyse) bilgi aldı mı. Şart sağlandıysa null.</summary>
        public RoutineProblem Check(RunReport report)
        {
            var defined = report.DefinedRoutines;
            foreach (var name in Names)
            {
                int n = Runs(report, name);
                if (!defined.Contains(name))
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " rutini yok",
                        Text = "Robot işi yaptı ama bu bölümde adımları bir rutine koyman isteniyor. `def " + name + "():` ile yaz, sonra `" + name + "()` diye çağır.",
                    };
                if (n == 0)
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " hiç çalışmadı",
                        Text = "Rutini tanımlamak onu çalıştırmaz. Tanımdan sonra `" + name + "()` yazarak çağır.",
                    };
                if (n < RequiredRuns)
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " yalnızca " + n + " kez çalıştı",
                        Text = "Rutin tekrar kullanmak içindir: aynı işi her gerektiğinde `" + name + "()` yazarak çağır.",
                    };
                var ruleProblem = RuleProblem(report, name);
                if (ruleProblem != null) return ruleProblem;
            }
            var repeated = defined.Where(name => Runs(report, name) >= RequiredRuns).OrderBy(name => name, StringComparer.Ordinal).ToList();
            int used = repeated.Count(name => RuleProblem(report, name) == null);
            if (used < MinCount)
            {
                // tekrar eden rutin var ama bilgi almıyor ya da cevap vermiyor: oyuncuya asıl eksiği söyle
                var ruleProblem = repeated.Select(name => RuleProblem(report, name)).FirstOrDefault(p => p != null);
                if (ruleProblem != null) return ruleProblem;
                return MinCount == 1
                    ? new RoutineProblem
                    {
                        Title = "Kod bitti, rutin yok",
                        Text = "Tekrar eden adımları `def` ile bir rutine koy, adını sen seç ve en az iki kez çağır.",
                    }
                    : new RoutineProblem
                    {
                        Title = "Kod bitti, " + MinCount + " rutin gerekiyor",
                        Text = "Bu bölümde en az " + MinCount + " ayrı rutin isteniyor; her biri en az iki kez çalışmalı. Tekrar eden adım gruplarını bul, her birine bir ad ver.",
                    };
            }
            return null;
        }

        /// <summary>Rutinin parametre ya da dönüş şartındaki eksiği; ikisi de sağlandıysa null</summary>
        RoutineProblem RuleProblem(RunReport report, string name) => ParameterProblem(report, name) ?? ReturnProblem(report, name);

        /// <summary>Dönüş şartı açıksa: rutinin içinde değer veren bir return olmalı (kodun yazısından okunur)</summary>
        RoutineProblem ReturnProblem(RunReport report, string name)
        {
            if (!NeedsReturn || report.ReturningRoutines.Contains(name)) return null;
            return new RoutineProblem
            {
                Title = "Kod bitti, " + name + " cevap vermiyor",
                Text = "`" + name + "` içinde değer veren bir `return` yok: rutin iş yapıyor ama cevabını vermiyor. Cevabı `return` ile ver (örn. `return 3`); `print` ekrana yazar, cevap vermez.",
            };
        }

        /// <summary>Parametre şartı açıksa: rutin en az bir parametre almalı ve en az iki farklı bilgiyle çağrılmalı.</summary>
        RoutineProblem ParameterProblem(RunReport report, string name)
        {
            if (!NeedsParameters) return null;
            if (!report.RoutineParameters.TryGetValue(name, out int count) || count == 0)
                return new RoutineProblem
                {
                    Title = "Kod bitti, " + name + " parametresiz",
                    Text = "`" + name + "` rutininin parametresi yok. Rutine her çağrıda ne yapacağını söylemek için `def " + name + "(yon):` gibi bir bilgi kutusu ekle.",
                };
            var seen = report.RoutineArguments[name];
            if (seen.Count < MinDistinctArguments)
                return new RoutineProblem
                {
                    Title = "Kod bitti, " + name + " hep aynı bilgiyle",
                    Text = "`" + name + "` hep aynı bilgiyle çağrıldı (`" + seen[0] + "`). Parametre, rutine her seferinde farklı bilgi vermek içindir.",
                };
            return null;
        }

        static int Runs(RunReport report, string name) => report.RoutineRuns.TryGetValue(name, out var n) ? n : 0;
    }
}
