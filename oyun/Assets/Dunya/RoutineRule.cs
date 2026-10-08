// Rutin şartı (Bölge 6'dan itibaren, fonksiyonlar): bölüm, oyuncunun belli adımları def ile bir rutine koymasını
// isteyebilir. Şart yalnızca dünyaya bakmaz, koda bakar: rutin tanımlı mı, kaç kez çalıştı. Tek kez çalışan şey
// rutin sayılmaz (rutin tekrar kullanmak içindir). Bölüm dosyasında: "rutinler" (adlar), "rutin_sayisi" (adı serbest).

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

        public RoutineRule(IEnumerable<string> names, int minCount)
        {
            Names = names.ToList();
            MinCount = minCount;
        }

        public bool IsEmpty => Names.Count == 0 && MinCount == 0;

        /// <summary>defined: oyuncunun def ile tanımladığı rutinler; runs: her rutinin kaç kez çalıştığı.
        /// Şart sağlandıysa null.</summary>
        public RoutineProblem Check(ICollection<string> defined, IReadOnlyDictionary<string, int> runs)
        {
            foreach (var name in Names)
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

        static int Runs(IReadOnlyDictionary<string, int> runs, string name) => runs.TryGetValue(name, out var n) ? n : 0;
    }
}
