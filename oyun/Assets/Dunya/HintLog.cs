// İpuçları: bölüm başına kaç ipucunun açıldığı (tasarım belgesi §6; karar 09-30: ilk insan testinde üçü de bedava).
// 1. ipucu her zaman açık sayılır; oyuncu ampule bastıkça 2. ve 3. ipucuna ulaşır; ulaştığı en yüksek sıra saklanır (görünüm her açılışta 1.'den başlar, HintView).
// Saf mantık; telefona yazma işi Scripts'te (Save/Load metni PlayerPrefs'e gider). İleride yıldız ve jeton kuralı buna bakar.

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MarsKod.Dunya
{
    public sealed class HintLog
    {
        readonly Dictionary<int, int> shown = new Dictionary<int, int>();

        /// <summary>Bölümde açılmış ipucu sayısı (en az 1: ilk ipucu bedava ve hep açık).</summary>
        public int Shown(int level) => shown.TryGetValue(level, out int n) && n > 1 ? n : 1;

        /// <summary>Oyuncu bölümde n. ipucuna kadar gitti; bölümde available kadar ipucu var. Kayıtlı en yüksek sayı korunur, yeni açık sayısı döner.</summary>
        public int Reach(int level, int n, int available)
        {
            int best = Shown(level);
            n = n < available ? n : available;
            if (n > best) shown[level] = best = n;
            return best;
        }

        // ---- kaydetme: "3:2;5:3" (bölüm numarası : açılan ipucu sayısı; yalnızca 1'den fazlası yazılır) ----

        public string Save()
        {
            var levels = new List<int>(shown.Keys);
            levels.Sort();
            var sb = new StringBuilder();
            foreach (int level in levels)
            {
                if (sb.Length > 0) sb.Append(';');
                sb.Append(level).Append(':').Append(shown[level]);
            }
            return sb.ToString();
        }

        /// <summary>Kayıt metnini yükler. Bozuk bir metin varsa hiçbir ipucu açılmamış sayılır.</summary>
        public static HintLog Load(string saved)
        {
            var log = new HintLog();
            if (string.IsNullOrEmpty(saved)) return log;
            var parsed = new HintLog();
            foreach (var part in saved.Split(';'))
            {
                var pair = part.Split(':');
                if (pair.Length != 2
                    || !int.TryParse(pair[0], NumberStyles.None, CultureInfo.InvariantCulture, out int level)
                    || !int.TryParse(pair[1], NumberStyles.None, CultureInfo.InvariantCulture, out int n)
                    || n < 2) return log;
                parsed.shown[level] = n;
            }
            return parsed;
        }
    }
}
