// Rutin şartı (Bölge 6'dan itibaren, fonksiyonlar): bölüm, oyuncunun belli adımları def ile bir rutine koymasını
// isteyebilir. Şart yalnızca dünyaya bakmaz, koda bakar: rutin tanımlı mı, kaç kez çalıştı. Tek kez çalışan şey
// rutin sayılmaz (rutin tekrar kullanmak içindir). Bölüm dosyasında: "rutinler" (adlar), "rutin_sayisi" (adı serbest).
// Bölge 7'den itibaren parametre şartı da olabilir ("parametre_sarti"): rutin bilgi almalı ve en az iki farklı bilgiyle çağrılmalı.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    /// <summary>Rutin şartı karşılanmadığında oyuncuya gösterilecek açıklama</summary>
    public sealed class RoutineProblem
    {
        public string Title;
        public string Text;
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

        public RoutineRule(IEnumerable<string> names, int minCount, bool needsParameters = false)
        {
            Names = names.ToList();
            MinCount = minCount;
            NeedsParameters = needsParameters;
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
                if (n < MinRuns)
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " yalnızca " + n + " kez çalıştı",
                        Text = "Rutin tekrar kullanmak içindir: aynı işi her gerektiğinde `" + name + "()` yazarak çağır.",
                    };
                var parameterProblem = ParameterProblem(report, name);
                if (parameterProblem != null) return parameterProblem;
            }
            var repeated = defined.Where(name => Runs(report, name) >= MinRuns).OrderBy(name => name, StringComparer.Ordinal).ToList();
            int used = repeated.Count(name => ParameterProblem(report, name) == null);
            if (used < MinCount)
            {
                // tekrar eden rutin var ama bilgi almıyor: oyuncuya asıl eksiği söyle
                var parameterProblem = repeated.Select(name => ParameterProblem(report, name)).FirstOrDefault(p => p != null);
                if (parameterProblem != null) return parameterProblem;
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
