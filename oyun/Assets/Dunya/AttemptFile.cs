// Deneme başına yazılabilen ölçüler (A2, A3): oyuncunun göremediği, ölçerek bulması gereken sayılar.
//   "toz_suresi"     toz bulutunun (Z) kaç beklemede dağılacağı
//   "kristal_boyu"   tuz kristalinin (S) boyu
//   "makara_uzunlugu" kablo makarasının (M) uzunluğu (yalnız düz biçim)
//   "rapor"          antenin beklediği sayı
// Düz biçim her denemede aynıdır: [3, 5] / 27. Deneme başına biçim her denemede başkadır: [[3, 5], [5, 2]] / [27, 24].
// Kod her denemede ayrı dünyada denenir (Level.RunVersions); ölçmeden yazılan (ezberlenen) sayı bir denemede tutmaz.
// Tasarım: docs/superpowers/specs/2026-11-19-degisen-toz-suresi-design.md · Kılavuz: docs/tasarim/bolum-dosyasi.md

using System.Collections.Generic;
using System.Linq;
using static MarsKod.Dunya.DataFields;

namespace MarsKod.Dunya
{
    static class AttemptFile
    {
        public const string DustField = "toz_suresi", CrystalField = "kristal_boyu", ReelField = "makara_uzunlugu", ReportField = "rapor";

        /// <summary>Haritadaki nesnelere (okuma sırasıyla) her deneme için listedeki sayıyı bağlar. Alan yoksa nesne de olmamalı.
        /// name/mark/unit hata mesajı için: "toz bulutu", "Z", "bekleme sayısı".</summary>
        public static List<Dictionary<Cell, int>> ReadCellValues(Dictionary<string, object> d, string field, List<Cell> cells, string name, string mark, string unit)
        {
            var attempts = d.ContainsKey(field) ? Attempts(d[field], field) : new List<List<int>> { new List<int>() };
            var result = new List<Dictionary<Cell, int>>();
            for (int a = 0; a < attempts.Count; a++)
            {
                string where = attempts.Count == 1 ? "\"" + field + "\"" : "\"" + field + "\" içindeki " + (a + 1) + ". deneme";
                var values = attempts[a];
                if (values.Count != cells.Count)
                    throw new DataFormatError("Haritada " + cells.Count + " " + name + " (" + mark + ") var ama " + where + " listesinde " + values.Count
                        + " sayı var. Her biri için bir " + unit + " yaz, haritadaki sırayla: üst satırdan başla, soldan sağa.");
                var bound = new Dictionary<Cell, int>();
                for (int i = 0; i < cells.Count; i++)
                {
                    if (values[i] < 1) throw new DataFormatError(where + " içindeki sayılar 1 ya da daha büyük olmalı (" + (i + 1) + ". sayı " + values[i] + ").");
                    bound[cells[i]] = values[i];
                }
                result.Add(bound);
            }
            return result;
        }

        /// <summary>"rapor": tek sayı (her denemede aynı) ya da deneme başına sayı listesi</summary>
        public static List<int> ReadReports(Dictionary<string, object> d)
        {
            if (d[ReportField] is List<object> list)
            {
                var reports = IntListOf(list, "\"" + ReportField + "\"");
                CheckAttemptCount(reports.Count, ReportField, "[27, 24]", "27");
                return reports;
            }
            return new List<int> { Int(d, ReportField) };
        }

        // Düz liste tek deneme; iç içe liste deneme başına (2-3 deneme, ikisi aynı olamaz)
        static List<List<int>> Attempts(object value, string field)
        {
            if (!(value is List<object> list && list.Count > 0 && list.All(x => x is List<object>)))
                return new List<List<int>> { IntListOf(value, "\"" + field + "\"") };
            CheckAttemptCount(list.Count, field, "[[3, 5], [5, 2]]", "[3, 5]");
            var attempts = list.Select((x, i) => IntListOf(x, "\"" + field + "\" içindeki " + (i + 1) + ". deneme")).ToList();
            if (attempts.Select(a => string.Join(",", a)).Distinct().Count() < attempts.Count)
                throw new DataFormatError("\"" + field + "\" içinde iki deneme aynı; her denemede en az bir sayı farklı olmalı.");
            return attempts;
        }

        static void CheckAttemptCount(int count, string field, string perAttemptExample, string singleExample)
        {
            if (count < 2 || count > Level.MaxTrails)
                throw new DataFormatError("\"" + field + "\" deneme başına yazılırsa 2-" + Level.MaxTrails + " deneme olmalı, örn. " + perAttemptExample
                    + "; tek denemeyse düz yaz: " + singleExample + ".");
        }

        /// <summary>Her denemede değişen şeylerin (iz, not, düzen, toz, kristal boyu, rapor) deneme sayıları eşit olmalı;
        /// kristal boyu değişiyorsa ve rapor bekleniyorsa rapor da deneme başına yazılmalı (cevap boylara bağlı)</summary>
        public static void CheckCounts(Level level)
        {
            var counts = new Dictionary<string, int>
            {
                ["izler"] = level.Trails.Count, ["notlar"] = level.Notes.Count, [LayoutFile.Field] = level.Layouts.Count,
                [DustField] = level.DustVersions.Count, [CrystalField] = level.CrystalSizeVersions.Count, [ReportField] = level.ReportVersions.Count,
            }.Where(kv => kv.Value > 1).ToList();
            if (counts.Select(kv => kv.Value).Distinct().Count() > 1)
                throw new DataFormatError("Her denemede değişen alanların deneme sayıları eşit olmalı: "
                    + string.Join(", ", counts.Select(kv => "\"" + kv.Key + "\" " + kv.Value)) + ".");
            bool perNoteReport = level.Notes.Count > 0 && level.Notes.All(n => n.Report != null);
            if (level.CrystalSizeVersions.Count > 1 && level.ExpectedReport != null && level.ReportVersions.Count < 2 && !perNoteReport)
                throw new DataFormatError("\"" + CrystalField + "\" her denemede değişiyor ama \"" + ReportField + "\" tek sayı: cevap boylara bağlıysa her deneme için yaz, örn. [27, 24].");
        }
    }
}
