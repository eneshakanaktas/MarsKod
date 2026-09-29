using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

// Kod kartindaki renklendirme: duz Python satirini renkli (zengin metin) satira cevirir.
public static class CodeColors
{
    const string Comment = "#6E6879", Keyword = "#EE9A7C", Call = "#7CC6D4", Constant = "#B9A3E3", Number = "#E6C07E", Text = "#B5D39A";
    const string Lifted = "#E9E4EE40";

    static readonly HashSet<string> Keywords = new HashSet<string>
    {
        "and", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "finally", "for", "from",
        "global", "if", "import", "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield",
    };

    static readonly HashSet<string> Constants = new HashSet<string> { "True", "False", "None", "North", "East", "South", "West" };

    static readonly Regex Part = new Regex(
        @"(?<c>#.*$)|(?<s>""(?:[^""\\]|\\.)*""?|'(?:[^'\\]|\\.)*'?)|(?<n>\b\d+(?:\.\d+)?\b)|(?<w>[^\W\d]\w*)(?<call>\s*\()?|(?<o>.)",
        RegexOptions.CultureInvariant);

    // Tek satiri renklendirir (kod yazma alani her satiri ayri cagirir).
    public static string Line(string line)
    {
        var sb = new StringBuilder();
        foreach (Match m in Part.Matches(line))
        {
            if (m.Groups["c"].Success) Paint(sb, m.Value, Comment);
            else if (m.Groups["s"].Success) Paint(sb, m.Value, Text);
            else if (m.Groups["n"].Success) Paint(sb, m.Value, Number);
            else if (m.Groups["w"].Success)
            {
                string w = m.Groups["w"].Value;
                if (Keywords.Contains(w)) Paint(sb, w, Keyword);
                else if (Constants.Contains(w)) Paint(sb, w, Constant);
                else if (m.Groups["call"].Success) Paint(sb, w, Call);
                else sb.Append(Escape(w));
                if (m.Groups["call"].Success) sb.Append(Escape(m.Groups["call"].Value));
            }
            else sb.Append(Escape(m.Value));
        }
        return sb.ToString();
    }

    static readonly Regex InlineCode = new Regex("`([^`]+)`", RegexOptions.CultureInvariant);

    // Aciklama metnindeki `kod` parcalari: ters tirnaklar kalkar, kod kod rengiyle gorunur (yeni baslayan isaretlere takilmasin)
    public static string Inline(string text)
        => string.IsNullOrEmpty(text) ? text : InlineCode.Replace(text, m => "<color=" + Call + ">" + Escape(m.Groups[1].Value) + "</color>");

    // Tasinmak uzere kaldirilmis satir: tek renk, soluk
    public static string Faded(string line)
    {
        var sb = new StringBuilder();
        Paint(sb, line, Lifted);
        return sb.ToString();
    }

    static void Paint(StringBuilder sb, string s, string color) => sb.Append("<color=").Append(color).Append('>').Append(Escape(s)).Append("</color>");

    // "<" zengin metinde etiket sanilmasin
    static string Escape(string s) => s.Replace("<", "<noparse><</noparse>");
}
