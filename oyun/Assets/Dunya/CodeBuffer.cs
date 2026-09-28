// Kod tamponu: oyuncunun kodu + imleç + seçim + her satırın nasıl yazıldığı (satır türü).
// Oyunun kendi klavyesi, Acemi paleti ve bilgisayar klavyesi kodu yalnızca buradaki işlemlerle değiştirir.
// Yazma kolaylıkları: ":" ile biten satırdan sonra Enter bir kademe (4 boşluk) içeriden başlar; girintide geri silme
// bir kademe geri gider; ⇥ / ⇤ satırın girintisini bir kademe değiştirir.
//
// Satır türleri (tasarım: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §6):
// - Paletten bırakılan satır Dugme; başlangıç kodundaki satırlar Baslangic.
// - Klavyeyle yazılan yeni satır Elle; o satırda öneriye dokunulursa Oneri.
// - Var olan satır değişince en düşük tür kalır (düzeltmek türü yükseltmez).
// - Bölünen satırın iki parçası eski türü alır; birleşen satırlar en düşüğü alır.
// - Boş (yalnızca boşluk) satır "yeni" sayılır: satır tamamen silinip yeniden yazılırsa yeni tür alır.

using System;
using System.Collections.Generic;
using System.Text;

namespace MarsKod.Dunya
{
    /// <summary>Satırın nasıl yazıldığı. Sıra önemli: küçük olan daha kolay yol (çözüm en düşüğe göre sayılır).</summary>
    public enum LineKind
    {
        /// <summary>Bölümün başlangıç kodundan geldi (oyuncu yazmadı)</summary>
        Baslangic = 0,
        /// <summary>Acemi paletinden bırakıldı</summary>
        Dugme = 1,
        /// <summary>Klavyeyle yazıldı, öneri satırı kullanıldı</summary>
        Oneri = 2,
        /// <summary>Klavyeyle harf harf yazıldı</summary>
        Elle = 3,
    }

    public sealed class CodeBuffer
    {
        public const string Indent = "    ";
        const string KindLetters = "BDOE"; // kayıt biçimi: satır başına bir harf

        readonly List<string> lines = new List<string> { "" };
        readonly List<LineKind> kinds = new List<LineKind> { LineKind.Elle };

        /// <summary>İmlecin yeri (koddaki harf sırası)</summary>
        public int Caret { get; private set; }
        /// <summary>Seçimin öbür ucu; Caret'e eşitse seçim yok</summary>
        public int Anchor { get; private set; }

        public string Text => string.Join("\n", lines);
        public int LineCount => lines.Count;
        public string Line(int i) => lines[i];
        public LineKind Kind(int i) => kinds[i];
        public bool HasSelection => Caret != Anchor;
        public int SelectionStart => Math.Min(Caret, Anchor);
        public int SelectionEnd => Math.Max(Caret, Anchor);

        // ---- yükleme / kaydetme ----

        /// <summary>Bölümün başlangıç kodunu yükler: bütün satırlar Baslangic.</summary>
        public void LoadStart(string code) => Load(code, null, LineKind.Baslangic);

        /// <summary>Kaydedilmiş kodu türleriyle yükler. Türler eksik ya da kodla uyuşmuyorsa bütün satırlar Dugme sayılır
        /// (en kolay yol; oyuncu hak etmediği XP'yi almasın).</summary>
        public void Load(string code, string savedKinds) => Load(code, savedKinds, LineKind.Dugme);

        void Load(string code, string savedKinds, LineKind fallback)
        {
            code = (code ?? "").Replace("\r", "").Replace("\t", Indent).TrimEnd('\n');
            lines.Clear();
            kinds.Clear();
            lines.AddRange(code.Split('\n'));
            bool ok = savedKinds != null && savedKinds.Length == lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                int k = ok ? KindLetters.IndexOf(savedKinds[i]) : -1;
                kinds.Add(k >= 0 ? (LineKind)k : fallback);
            }
            NormalizeBlank(0, lines.Count - 1);
            Caret = Anchor = 0;
        }

        /// <summary>Satır türlerinin kayıt metni (satır başına bir harf: B, D, O, E).</summary>
        public string SaveKinds()
        {
            var sb = new StringBuilder(kinds.Count);
            foreach (var k in kinds) sb.Append(KindLetters[(int)k]);
            return sb.ToString();
        }

        /// <summary>Satır çözüm türüne katılır mı: boş ve yalnızca yorum olan satırlar katılmaz.</summary>
        public bool Counts(int i)
        {
            string t = lines[i].Trim();
            return t.Length > 0 && t[0] != '#';
        }

        // ---- imleç ----

        public void SetCaret(int index) => Caret = Anchor = Clamp(index);

        public void Select(int anchor, int caret)
        {
            Anchor = Clamp(anchor);
            Caret = Clamp(caret);
        }

        /// <summary>İmleci sola/sağa taşır (extend: seçimi genişlet). Seçim varken genişletmeden basılırsa seçimin o ucuna gider.</summary>
        public void MoveHorizontal(int delta, bool extend = false)
        {
            if (HasSelection && !extend) { SetCaret(delta < 0 ? SelectionStart : SelectionEnd); return; }
            Caret = Clamp(Caret + delta);
            if (!extend) Anchor = Caret;
        }

        /// <summary>İmleci bir satır yukarı/aşağı taşır (sütun korunur, satır kısaysa sonuna gider).</summary>
        public void MoveVertical(int delta, bool extend = false)
        {
            var (l, c) = Position(Caret);
            int nl = Math.Max(0, Math.Min(lines.Count - 1, l + delta));
            Caret = Index(nl, Math.Min(c, lines[nl].Length));
            if (!extend) Anchor = Caret;
        }

        /// <summary>Satır başına (girintinin sonuna) ya da satır sonuna gider.</summary>
        public void MoveToLineEdge(bool end, bool extend = false)
        {
            var (l, _) = Position(Caret);
            Caret = Index(l, end ? lines[l].Length : LeadingSpaces(lines[l]));
            if (!extend) Anchor = Caret;
        }

        /// <summary>Koddaki sıra → (satır, sütun), ikisi de 0'dan.</summary>
        public (int line, int col) Position(int index)
        {
            index = Clamp(index);
            int l = 0;
            while (index > lines[l].Length) { index -= lines[l].Length + 1; l++; }
            return (l, index);
        }

        /// <summary>(satır, sütun) → koddaki sıra; sütun satır uzunluğuna kısılır.</summary>
        public int Index(int line, int col)
        {
            line = Math.Max(0, Math.Min(lines.Count - 1, line));
            int idx = 0;
            for (int i = 0; i < line; i++) idx += lines[i].Length + 1;
            return idx + Math.Max(0, Math.Min(col, lines[line].Length));
        }

        // ---- klavye ----

        /// <summary>Klavyeden yazı (tek harf ya da işaret; satır sonu içermez). Seçim varsa onun yerine yazılır.</summary>
        public void Type(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            if (s.IndexOf('\n') >= 0) throw new ArgumentException("Satır sonu için Enter() kullanılır.");
            s = s.Replace("\t", Indent);
            DeleteSelection();
            Replace(Caret, Caret, s);
        }

        /// <summary>Enter: yeni satır, girinti üstteki satırdan (":" ile bitiyorsa bir kademe içeride).
        /// Satırın ortasında basılırsa alta inen kısmın baştaki boşlukları atılır.</summary>
        public void Enter()
        {
            DeleteSelection();
            var (l, c) = Position(Caret);
            string left = lines[l].Substring(0, c);
            string ind = left.Substring(0, LeadingSpaces(left));
            if (left.TrimEnd().EndsWith(":")) ind += Indent;
            int spaces = LeadingSpaces(lines[l].Substring(c));
            Replace(Caret, Caret + spaces, "\n" + ind);
        }

        /// <summary>Geri silme. Satır başından girintinin içindeyken bir kademe (4'ün katına) geri gider;
        /// satırın en başında üstteki satırla birleştirir.</summary>
        public void Backspace()
        {
            if (HasSelection) { DeleteSelection(); return; }
            if (Caret == 0) return;
            var (l, c) = Position(Caret);
            if (c > 0 && LeadingSpaces(lines[l]) >= c)
            {
                int target = (c - 1) / Indent.Length * Indent.Length;
                Replace(Caret - (c - target), Caret, "");
            }
            else Replace(Caret - 1, Caret, "");
        }

        /// <summary>⇥: imlecin satırının (seçim varsa seçili satırların) girintisini bir kademe artırır.</summary>
        public void IndentLines() => ShiftLines(true);

        /// <summary>⇤: girintiyi bir kademe azaltır (4'ün bir alt katına).</summary>
        public void DedentLines() => ShiftLines(false);

        /// <summary>Öneriye dokunuldu: imlecin solundaki yarım kelime öneriyle tamamlanır; fonksiyonsa "(" de eklenir.
        /// Satırın türü en çok Oneri olur.</summary>
        public void ApplySuggestion(string word, bool isFunction)
        {
            if (HasSelection) SetCaret(Caret);
            var (l, c) = Position(Caret);
            string line = lines[l];
            int s = c;
            while (s > 0 && IsWordChar(line[s - 1])) s--;
            int e = c;
            while (e < line.Length && IsWordChar(line[e])) e++; // kelimenin sağında kalan parça da değişir
            bool parenThere = e < line.Length && line[e] == '(';
            Replace(Index(l, s), Index(l, e), word + (isFunction && !parenThere ? "(" : ""));
            if (isFunction && parenThere) SetCaret(Caret + 1);
            kinds[l] = Min(kinds[l], LineKind.Oneri);
        }

        /// <summary>İmlecin solundaki yarım kelime (öneriler için).</summary>
        public string WordBeforeCaret()
        {
            var (l, c) = Position(Caret);
            int s = c;
            while (s > 0 && IsWordChar(lines[l][s - 1])) s--;
            return lines[l].Substring(s, c - s);
        }

        // ---- Acemi paleti ----

        /// <summary>Bırakılacak yerin varsayılan girinti kademesi: üstteki (boş olmayan) satır ":" ile bitiyorsa
        /// bir kademe içeride, değilse onunla aynı. gap: satır arası (0 = en üst, LineCount = en alt).</summary>
        public int DefaultIndentLevel(int gap)
        {
            for (int i = Math.Min(gap, lines.Count) - 1; i >= 0; i--)
            {
                if (lines[i].Trim().Length == 0) continue;
                int level = LeadingSpaces(lines[i]) / Indent.Length;
                return lines[i].TrimEnd().EndsWith(":") ? level + 1 : level;
            }
            return 0;
        }

        /// <summary>Paletten bir satır bırakır (türü Dugme). Kod bomboşsa onun yerine geçer. Dönen: satırın numarası.</summary>
        public int DropLine(int gap, int indentLevel, string code)
        {
            string text = Spaces(indentLevel) + code.Trim();
            int at;
            if (lines.Count == 1 && lines[0].Trim().Length == 0)
            {
                lines[0] = text;
                kinds[0] = LineKind.Dugme;
                at = 0;
            }
            else
            {
                at = Math.Max(0, Math.Min(lines.Count, gap));
                lines.Insert(at, text);
                kinds.Insert(at, LineKind.Dugme);
            }
            SetCaret(Index(at, text.Length));
            return at;
        }

        /// <summary>Bir satırı başka bir satır arasına taşır (türü onunla gider), girintisi yeniden verilir.
        /// gap taşımadan önceki satır aralarına göredir. Dönen: satırın yeni numarası.</summary>
        public int MoveLine(int from, int gap, int indentLevel)
        {
            string text = Spaces(indentLevel) + lines[from].TrimStart(' ');
            var kind = kinds[from];
            lines.RemoveAt(from);
            kinds.RemoveAt(from);
            int at = gap > from ? gap - 1 : gap;
            at = Math.Max(0, Math.Min(lines.Count, at));
            lines.Insert(at, text);
            kinds.Insert(at, kind);
            NormalizeBlank(at, at);
            SetCaret(Index(at, text.Length));
            return at;
        }

        /// <summary>Bir satırı siler (kod kartının dışına bırakıldı).</summary>
        public void DeleteLine(int i)
        {
            lines.RemoveAt(i);
            kinds.RemoveAt(i);
            if (lines.Count == 0) { lines.Add(""); kinds.Add(LineKind.Elle); }
            SetCaret(Index(Math.Min(i, lines.Count - 1), 0));
        }

        /// <summary>(satır, sütun)'daki ya da hemen solundaki tam sayıyı bulur.</summary>
        public bool FindNumber(int line, int col, out int start, out int length)
        {
            start = length = 0;
            if (line < 0 || line >= lines.Count) return false;
            string s = lines[line];
            int p = col < s.Length && char.IsDigit(s[col]) ? col : col > 0 && col - 1 < s.Length && char.IsDigit(s[col - 1]) ? col - 1 : -1;
            if (p < 0) return false;
            int a = p, b = p + 1;
            while (a > 0 && char.IsDigit(s[a - 1])) a--;
            while (b < s.Length && char.IsDigit(s[b])) b++;
            if (a > 0 && IsWordChar(s[a - 1])) return false; // isimin parçası (x2 gibi), sayı değil
            start = a;
            length = b - a;
            return true;
        }

        /// <summary>Sayıyı −/+ ile değiştirir (0 ile 99 arası). Satırın türü değişmez.</summary>
        public bool ChangeNumber(int line, int col, int delta)
        {
            if (!FindNumber(line, col, out int start, out int length)) return false;
            string s = lines[line];
            if (!int.TryParse(s.Substring(start, length), out int n)) return false;
            int m = Math.Max(0, Math.Min(99, n + delta));
            if (m == n) return false;
            lines[line] = s.Substring(0, start) + m + s.Substring(start + length);
            SetCaret(Caret); // imleç satır dışına taşmasın
            return true;
        }

        // ---- iç işler ----

        void DeleteSelection()
        {
            if (HasSelection) Replace(SelectionStart, SelectionEnd, "");
        }

        // a..b arasını ins ile değiştirir. Etkilenen satırlar birleşip yeniden bölünür; hepsi eski satırların en düşük
        // türünü alır. Boş kalan satırlar "yeni" (Elle) olur.
        void Replace(int a, int b, string ins)
        {
            var (la, ca) = Position(a);
            var (lb, cb) = Position(b);
            var kind = kinds[la];
            for (int i = la + 1; i <= lb; i++) kind = Min(kind, kinds[i]);
            var parts = (lines[la].Substring(0, ca) + ins + lines[lb].Substring(cb)).Split('\n');
            lines.RemoveRange(la, lb - la + 1);
            kinds.RemoveRange(la, lb - la + 1);
            for (int i = 0; i < parts.Length; i++)
            {
                lines.Insert(la + i, parts[i]);
                kinds.Insert(la + i, kind);
            }
            NormalizeBlank(la, la + parts.Length - 1);
            Caret = Anchor = a + ins.Length;
        }

        void ShiftLines(bool indent)
        {
            var (l1, c1) = Position(SelectionStart);
            var (l2, _) = Position(SelectionEnd);
            if (l2 > l1 && Position(SelectionEnd).col == 0) l2--; // seçim bir sonraki satırın başında bitiyorsa o satır dahil değil
            var (cl, cc) = Position(Caret);
            var (al, ac) = Position(Anchor);
            for (int l = l1; l <= l2; l++)
            {
                int lead = LeadingSpaces(lines[l]);
                int delta = indent ? Indent.Length : -(lead == 0 ? 0 : lead - (lead - 1) / Indent.Length * Indent.Length);
                if (delta == 0) continue;
                lines[l] = delta > 0 ? Indent + lines[l] : lines[l].Substring(-delta);
                if (cl == l) cc = Math.Max(0, cc + delta);
                if (al == l) ac = Math.Max(0, ac + delta);
            }
            Caret = Index(cl, cc);
            Anchor = Index(al, ac);
        }

        void NormalizeBlank(int from, int to)
        {
            for (int i = from; i <= to && i < lines.Count; i++)
                if (lines[i].Trim().Length == 0) kinds[i] = LineKind.Elle;
        }

        int Clamp(int index)
        {
            int len = 0;
            foreach (var l in lines) len += l.Length + 1;
            return Math.Max(0, Math.Min(len - 1, index));
        }

        static LineKind Min(LineKind a, LineKind b) => a < b ? a : b;

        static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

        static int LeadingSpaces(string s)
        {
            int i = 0;
            while (i < s.Length && s[i] == ' ') i++;
            return i;
        }

        static string Spaces(int level) => new string(' ', Math.Max(0, level) * Indent.Length);
    }
}
