// Dinlenen kayıtlar (Kayıtlar sayfası). Saf mantık; telefona yazma işi Scripts'te (Save/Load metni "kayitlar" anahtarına gider).

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MarsKod.Dunya
{
    public sealed class RecordingLog
    {
        readonly SortedSet<int> heard = new SortedSet<int>();

        public bool Any => heard.Count > 0;

        public bool Heard(int number) => heard.Contains(number);

        /// <summary>Kayıt dinlendi. Katalogda olmayan numara yok sayılır.</summary>
        public void Mark(int number)
        {
            if (Recordings.ByNumber(number) != null) heard.Add(number);
        }

        /// <summary>Dinlenmiş kayıtlar, numara sırasıyla.</summary>
        public IReadOnlyList<Recording> HeardList() => Recordings.All.Where(r => heard.Contains(r.Number)).ToList();

        // ---- kaydetme: "1,2,3" (artan sıra) ----

        public string Save() => string.Join(",", heard);

        /// <summary>Kayıt metnini yükler. Bozuk ya da bilinmeyen parçalar atlanır.</summary>
        public static RecordingLog Load(string saved)
        {
            var log = new RecordingLog();
            if (string.IsNullOrEmpty(saved)) return log;
            foreach (var part in saved.Split(','))
                if (int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int number)) log.Mark(number);
            return log;
        }
    }
}
