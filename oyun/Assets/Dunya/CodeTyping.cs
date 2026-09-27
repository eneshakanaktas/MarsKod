// Kod yazma kolayliklari (telefon klavyesinde Tab yok, girintiyi elle saymak zor):
// - ":" ile biten satirdan sonra Enter: yeni satir bir kademe (4 bosluk) iceriden baslar; digerlerinde onceki satirin girintisiyle.
// - Girintideki bir bosluk silinince girinti bir kademe geri gider (4'un katina).
// - Tab karakteri 4 bosluga cevrilir.
// Yazi kutusu metni degistirdikten sonra (once/sonra farkina bakarak) uygulanir; bu yuzden hem bilgisayar klavyesiyle
// hem telefon klavyesiyle ayni calisir.

using System;

namespace MarsKod.Dunya
{
    public static class CodeTyping
    {
        public const string Indent = "    ";

        /// <summary>before: degisiklikten onceki kod, after: sonraki; caret: imlecin yeri (after icinde).
        /// Donen: duzeltilmis kod (degisiklik yoksa after'in kendisi); caret duzeltilmis koda gore guncellenir.</summary>
        public static string Apply(string before, string after, ref int caret)
        {
            before = before ?? "";
            after = (after ?? "").Replace("\r", "");
            caret = Math.Max(0, Math.Min(caret, after.Length));

            if (after.IndexOf('\t') >= 0)
            {
                int tabs = 0;
                for (int i = 0; i < caret; i++) if (after[i] == '\t') tabs++;
                caret += tabs * (Indent.Length - 1);
                return after.Replace("\t", Indent);
            }

            if (after.Length == before.Length + 1)
            {
                // eklenen karakterin yeri: imlecin hemen solu (ayni karakter art arda gelse de dogru yer)
                int p = caret > 0 && string.CompareOrdinal(after.Remove(caret - 1, 1), before) == 0 ? caret - 1 : Prefix(before, after);
                // tek karakter yazilinca imlec hep onun hemen sagindadir (telefonda imlec bilgisi gec gelebiliyor)
                caret = p + 1;
                if (after[p] == '\n')
                {
                    int ls = LineStart(after, p);
                    string line = after.Substring(ls, p - ls);
                    string ind = LeadingSpaces(line);
                    if (line.TrimEnd().EndsWith(":")) ind += Indent;
                    // Enter satirin ortasinda basildiysa, alta inen kismin kendi bosluklari atilir
                    int rest = p + 1, spaces = 0;
                    while (rest + spaces < after.Length && after[rest + spaces] == ' ') spaces++;
                    if (ind.Length > 0 || spaces > 0)
                    {
                        after = after.Substring(0, rest) + ind + after.Substring(rest + spaces);
                        caret = rest + ind.Length;
                    }
                }
                return after;
            }

            if (after.Length == before.Length - 1)
            {
                // silinen karakterin yeri: geri silmede imlecin oldugu yer
                int p = caret < before.Length && string.CompareOrdinal(before.Remove(caret, 1), after) == 0 ? caret : Prefix(after, before);
                int ls = LineStart(before, p);
                caret = p; // geri silmede imlec silinen karakterin yerinde kalir
                if (before[p] == ' ' && IsSpaces(before, ls, p + 1))
                {
                    int col = p + 1 - ls; // silmeden onceki girinti (imlece kadar)
                    int target = (col - 1) / Indent.Length * Indent.Length;
                    int extra = col - 1 - target;
                    if (extra > 0)
                    {
                        after = after.Remove(p - extra, extra);
                        caret = p - extra;
                    }
                }
            }
            return after;
        }

        static int Prefix(string a, string b)
        {
            int n = Math.Min(a.Length, b.Length), i = 0;
            while (i < n && a[i] == b[i]) i++;
            return i;
        }

        static int LineStart(string s, int p) => p <= 0 ? 0 : s.LastIndexOf('\n', p - 1) + 1;

        static string LeadingSpaces(string line)
        {
            int i = 0;
            while (i < line.Length && line[i] == ' ') i++;
            return line.Substring(0, i);
        }

        static bool IsSpaces(string s, int from, int to)
        {
            for (int i = from; i < to; i++) if (s[i] != ' ') return false;
            return true;
        }
    }
}
