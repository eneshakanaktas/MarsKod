// XP: bölüm başına, çözümün kademesine göre bir kez verilir (tasarım: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §6).
// Saf mantık; telefona yazma işi Scripts'te (Save/Load metni PlayerPrefs'e gider).
// Miktarlar yalnızca burada: Parça 2'de seviyeye göre değişecek.

using System;
using System.Collections.Generic;
using System.Text;

namespace MarsKod.Dunya
{
    public sealed class Xp
    {
        // Kademeler: Dugme = Acemi, Oneri = Orta, Elle = Usta
        const string TierLetters = "DOE";
        static readonly LineKind[] Tiers = { LineKind.Dugme, LineKind.Oneri, LineKind.Elle };

        readonly Dictionary<int, HashSet<LineKind>> taken = new Dictionary<int, HashSet<LineKind>>();

        /// <summary>Kademenin XP'si. Baslangic (oyuncu yazmadı) en kolay kademe sayılır.</summary>
        public static int Amount(LineKind kind)
        {
            switch (Tier(kind))
            {
                case LineKind.Elle: return 100;
                case LineKind.Oneri: return 50;
                default: return 30;
            }
        }

        /// <summary>Oyuncuya gösterilen ad.</summary>
        public static string Name(LineKind kind)
        {
            switch (Tier(kind))
            {
                case LineKind.Elle: return "Usta";
                case LineKind.Oneri: return "Orta";
                default: return "Acemi";
            }
        }

        static LineKind Tier(LineKind kind) => kind == LineKind.Baslangic ? LineKind.Dugme : kind;

        /// <summary>Çözümün türü = sayılan satırların en düşüğü. startCodeCounts kapalıysa başlangıç satırları hesaba katılmaz.
        /// Hiç sayılan satır yoksa (boş kod ya da yalnızca başlangıç kodu) en kolay kademe.</summary>
        public static LineKind SolutionKind(CodeBuffer code, bool startCodeCounts = true)
        {
            LineKind lowest = LineKind.Elle;
            bool any = false;
            for (int i = 0; i < code.LineCount; i++)
            {
                if (!code.Counts(i)) continue;
                var k = code.Kind(i);
                if (k == LineKind.Baslangic && !startCodeCounts) continue;
                k = Tier(k);
                if (!any || k < lowest) lowest = k;
                any = true;
            }
            return any ? lowest : LineKind.Dugme;
        }

        /// <summary>Bölümde bu kademeden XP alındı mı.</summary>
        public bool Has(int level, LineKind kind)
            => taken.TryGetValue(level, out var set) && set.Contains(Tier(kind));

        /// <summary>Çözüm bitince çağrılır: verilen XP'yi döndürür (bu bölümde bu kademe daha önce alındıysa 0).</summary>
        public int Award(int level, LineKind kind)
        {
            kind = Tier(kind);
            if (Has(level, kind)) return 0;
            if (!taken.TryGetValue(level, out var set)) taken[level] = set = new HashSet<LineKind>();
            set.Add(kind);
            return Amount(kind);
        }

        /// <summary>Bu kademeden daha zor, bölümde henüz alınmamış en yakın kademe; yoksa null.</summary>
        public LineKind? NextBetter(int level, LineKind kind)
        {
            kind = Tier(kind);
            foreach (var t in Tiers)
                if (t > kind && !Has(level, t)) return t;
            return null;
        }

        /// <summary>"Usta ile çözersen +100 XP daha" ya da (alınacak kademe kalmadıysa) boş metin.</summary>
        public string NextBetterText(int level, LineKind kind)
        {
            var next = NextBetter(level, kind);
            return next == null ? "" : Name(next.Value) + " ile çözersen +" + Amount(next.Value) + " XP daha";
        }

        /// <summary>Bu bölümden kazanılan XP (bölüm seçme ekranı için); hiç çözülmediyse 0.</summary>
        public int LevelTotal(int level)
        {
            int sum = 0;
            if (taken.TryGetValue(level, out var set))
                foreach (var k in set) sum += Amount(k);
            return sum;
        }

        /// <summary>Toplam XP.</summary>
        public int Total
        {
            get
            {
                int sum = 0;
                foreach (int level in taken.Keys) sum += LevelTotal(level);
                return sum;
            }
        }

        // ---- kaydetme: "3:DO;5:E" (bölüm numarası : alınan kademelerin harfleri) ----

        public string Save()
        {
            var levels = new List<int>(taken.Keys);
            levels.Sort();
            var sb = new StringBuilder();
            foreach (int level in levels)
            {
                if (taken[level].Count == 0) continue;
                if (sb.Length > 0) sb.Append(';');
                sb.Append(level).Append(':');
                foreach (var t in Tiers)
                    if (taken[level].Contains(t)) sb.Append(TierLetters[Array.IndexOf(Tiers, t)]);
            }
            return sb.ToString();
        }

        /// <summary>Kayıt metnini yükler. Bozuk ya da tanınmayan bir metin varsa hiçbir şey alınmamış sayılır (sıfırdan başlar).</summary>
        public static Xp Load(string saved)
        {
            var xp = new Xp();
            if (string.IsNullOrEmpty(saved)) return xp;
            var parsed = new Xp();
            foreach (var part in saved.Split(';'))
            {
                int colon = part.IndexOf(':');
                if (colon <= 0 || colon == part.Length - 1) return xp;
                if (!int.TryParse(part.Substring(0, colon), System.Globalization.NumberStyles.None,
                        System.Globalization.CultureInfo.InvariantCulture, out int level)) return xp;
                for (int i = colon + 1; i < part.Length; i++)
                {
                    int t = TierLetters.IndexOf(part[i]);
                    if (t < 0) return xp;
                    parsed.Award(level, Tiers[t]);
                }
            }
            return parsed;
        }
    }
}
