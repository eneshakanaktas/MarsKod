// Kod blokları: ":" ile biten satır (for, if, def...) ve altındaki içeriden satırlar (gövde).
// Kod alanındaki blok çizgisi bunları gösterir: hangi satırların tekrarlanacağı bir bakışta görünsün. Saf mantık.

using System.Collections.Generic;

namespace MarsKod.Dunya
{
    /// <summary>Bir blok: başlık satırı, gövdenin son satırı, başlığın girinti kademesi (hepsi 0'dan).</summary>
    public struct BlockSpan
    {
        public int Header, Last, Level;
    }

    public static class CodeBlocks
    {
        /// <summary>Satır bir blok başlığı mı (":" ile biter, yorum değil).</summary>
        public static bool IsHeader(string line)
        {
            string t = line.Trim();
            return t.Length > 0 && t[0] != '#' && t.EndsWith(":");
        }

        /// <summary>Satırın girinti kademesi (4 boşluk bir kademe).</summary>
        public static int Level(string line) => LeadingSpaces(line) / CodeBuffer.Indent.Length;

        /// <summary>Koddaki bütün bloklar (iç içe olanlar dahil). Gövdesi olmayan başlık blok sayılmaz.
        /// Gövdenin arasındaki boş satırlar gövdeye katılır, sondakiler katılmaz.</summary>
        public static List<BlockSpan> Find(IList<string> lines)
        {
            var spans = new List<BlockSpan>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (!IsHeader(lines[i])) continue;
                int lead = LeadingSpaces(lines[i]), last = i;
                for (int j = i + 1; j < lines.Count; j++)
                {
                    if (lines[j].Trim().Length == 0) continue;
                    if (LeadingSpaces(lines[j]) <= lead) break;
                    last = j;
                }
                if (last > i) spans.Add(new BlockSpan { Header = i, Last = last, Level = lead / CodeBuffer.Indent.Length });
            }
            return spans;
        }

        /// <summary>gap satır arasına level kademesiyle bırakılan satır hangi bloğun gövdesine girer (başlık satırı);
        /// hiçbirine girmiyorsa -1. skip: taşınan satır (hesaba katılmaz).</summary>
        public static int Owner(IList<string> lines, int gap, int level, int skip = -1)
        {
            if (level <= 0) return -1;
            for (int i = System.Math.Min(gap, lines.Count) - 1; i >= 0; i--)
            {
                if (i == skip || lines[i].Trim().Length == 0) continue;
                int l = Level(lines[i]);
                if (l < level) return l == level - 1 && IsHeader(lines[i]) ? i : -1;
            }
            return -1;
        }

        static int LeadingSpaces(string s)
        {
            int i = 0;
            while (i < s.Length && s[i] == ' ') i++;
            return i;
        }
    }
}
