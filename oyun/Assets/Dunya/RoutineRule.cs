// Rutin şartı (Bölge 6'dan itibaren, fonksiyonlar): bölüm, oyuncunun belli adımları def ile bir rutine koymasını
// isteyebilir. Şart yalnızca dünyaya bakmaz, koda bakar: rutin tanımlı mı, kaç kez çalıştı. Tek kez çalışan şey
// rutin sayılmaz (rutin tekrar kullanmak içindir). Bölüm dosyasında: "rutinler" (adlar), "rutin_sayisi" (adı serbest).
// Bölge 7'den itibaren (parametreli fonksiyonlar) "parametreli_rutinler": bu rutinler ayrıca en az bir parametreyle
// tanımlanmalı ve en az iki farklı değerle çağrılmalı (yoksa oyuncu parametreyi atlayıp iki ayrı rutin yazar).

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

        /// <summary>Parametre şartında bir rutinin sayılması için en az kaç farklı değerle çağrılması gerekir</summary>
        public const int MinDistinctArgs = 2;

        /// <summary>Adıyla istenen rutinler</summary>
        public readonly IReadOnlyList<string> Names;
        /// <summary>Adı serbest: en az kaç farklı rutin (adlı olanlar dahil)</summary>
        public readonly int MinCount;
        /// <summary>Adıyla istenen ve ayrıca parametre alması gereken rutinler (adlı rutin sayılırlar)</summary>
        public readonly IReadOnlyList<string> ParametricNames;

        public RoutineRule(IEnumerable<string> names, int minCount, IEnumerable<string> parametricNames = null)
        {
            Names = names.ToList();
            MinCount = minCount;
            ParametricNames = (parametricNames ?? Enumerable.Empty<string>()).ToList();
        }

        public bool IsEmpty => Names.Count == 0 && MinCount == 0 && ParametricNames.Count == 0;

        /// <summary>defined: oyuncunun def ile tanımladığı rutinler; runs: her rutinin kaç kez çalıştığı;
        /// paramCounts: her rutinin parametre sayısı; args: her rutinin çağrılarındaki değerler (çağrı başına bir metin).
        /// Son ikisi verilmezse parametre şartı sınanmaz. Şart sağlandıysa null.</summary>
        public RoutineProblem Check(ICollection<string> defined, IReadOnlyDictionary<string, int> runs,
            IReadOnlyDictionary<string, int> paramCounts = null, IReadOnlyDictionary<string, List<string>> args = null)
        {
            foreach (var name in Names.Concat(ParametricNames).Distinct())
            {
                int n = Runs(runs, name);
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
            }
            var parametric = CheckParameters(paramCounts, args);
            if (parametric != null) return parametric;
            int used = defined.Count(name => Runs(runs, name) >= MinRuns);
            if (used < MinCount)
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
            return null;
        }

        // Parametreli rutinler: en az bir parametre ve en az iki farklı değerle çağrı
        RoutineProblem CheckParameters(IReadOnlyDictionary<string, int> paramCounts, IReadOnlyDictionary<string, List<string>> args)
        {
            if (paramCounts == null || args == null) return null;
            foreach (var name in ParametricNames)
            {
                if (!paramCounts.TryGetValue(name, out int count) || count == 0)
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " bir sayı almıyor",
                        Text = "Bu bölümde rutinin bir sayı alması isteniyor. `def " + name + "(boy):` diye yaz: parantezin içindeki ad, rutine verilen sayıyı taşır.",
                    };
                if (!args.TryGetValue(name, out var calls) || calls.Distinct().Count() < MinDistinctArgs)
                    return new RoutineProblem
                    {
                        Title = "Kod bitti, " + name + " hep aynı sayıyla çağrıldı",
                        Text = "Rutinin sayı almasının amacı, her seferinde başka bir sayıyla çalışabilmesi. Farklı sayılarla çağır: `" + name + "(3)`, `" + name + "(5)`.",
                    };
            }
            return null;
        }

        static int Runs(IReadOnlyDictionary<string, int> runs, string name) => runs.TryGetValue(name, out var n) ? n : 0;
    }
}
