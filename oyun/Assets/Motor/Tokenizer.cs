// Kelime ayırıcı: oyuncunun kodunu parçalara (token) böler ve girintileri INDENT/DEDENT'e çevirir.
// Hata mesajları CPython 3.12'nin tokenizer'ı ile birebir aynıdır.
//
// Hata olursa o ana kadarki parçalar ve hata birlikte döner: CPython kodu okurken
// daha önceki bir cümle hatası varsa onu önce bildirir; bu sırayı cümle çözücü belirler.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MarsKod.Motor
{
    public enum TokenType { NAME, NUMBER, STRING, OP, NEWLINE, INDENT, DEDENT, ENDMARKER }

    public enum NumberKind { Int, Float, Imaginary }

    public sealed class Token
    {
        public TokenType Type;
        /// <summary>STRING: kaçış dizileri çözülmüş içerik. Diğerleri: kaynaktaki metin.</summary>
        public string Value;
        /// <summary>1'den başlar</summary>
        public int Line;
        /// <summary>0'dan başlar</summary>
        public int Col;
        public int EndLine;
        public int EndCol;
        /// <summary>Sadece STRING: küçük harfli önek ("", "r", "f", "b", "rb"...)</summary>
        public string Prefix;
        /// <summary>Sadece NUMBER</summary>
        public NumberKind NumberKind;

        public override string ToString() => Type + " " + Value;
    }

    public sealed class TokenizeResult
    {
        public List<Token> Tokens;
        public PythonException Error;
    }

    public sealed class Tokenizer
    {
        static readonly string[] Operators =
        {
            "**=", "//=", ">>=", "<<=", "...",
            "!=", "%=", "&=", "**", "*=", "+=", "-=", "->", "//", "/=", ":=", "<<", "<=", "==", ">=", ">>", "@=", "^=", "|=",
            "%", "&", "(", ")", "*", "+", ",", "-", ".", "/", ":", ";", "<", "=", ">", "@", "[", "]", "^", "{", "|", "}", "~",
        };

        static readonly Dictionary<string, string> Closers = new Dictionary<string, string> { [")"] = "(", ["]"] = "[", ["}"] = "{" };

        static readonly HashSet<string> StringPrefixes = new HashSet<string> { "r", "u", "f", "b", "br", "rb", "fr", "rf" };

        const string D = "[0-9](?:_?[0-9])*";
        const string Exp = "[eE][+-]?" + D;
        static readonly Regex Hex = new Regex(@"\G0[xX](?:_?[0-9a-fA-F])+", RegexOptions.CultureInvariant);
        static readonly Regex Oct = new Regex(@"\G0[oO](?:_?[0-7])+", RegexOptions.CultureInvariant);
        static readonly Regex Bin = new Regex(@"\G0[bB](?:_?[01])+", RegexOptions.CultureInvariant);
        static readonly Regex FloatRe = new Regex(@"\G(?:(?:(?:" + D + @")?\." + D + "|" + D + @"\.)(?:" + Exp + ")?|" + D + Exp + ")", RegexOptions.CultureInvariant);
        static readonly Regex IntRe = new Regex(@"\G" + D, RegexOptions.CultureInvariant);
        static readonly Regex LeadingZeros = new Regex("^0[0-9_]*$", RegexOptions.CultureInvariant);

        public static TokenizeResult Tokenize(string source) => new Tokenizer(source).Run();

        // --- durum ---
        readonly string src;
        readonly int len;
        readonly List<Token> tokens = new List<Token>();
        readonly List<int> indents = new List<int> { 0 };
        // Sekme genişliği 1 sayılarak ölçülen girinti; 8'lik ölçümle çelişirse TabError.
        readonly List<int> altIndents = new List<int> { 0 };
        readonly List<(string ch, int line)> brackets = new List<(string, int)>();
        int i;
        int line = 1;
        int lineStart;
        bool atLineStart = true;
        bool lineHasTokens;

        Tokenizer(string source)
        {
            if (source.Length > 0 && source[0] == '\uFEFF') source = source.Substring(1);
            src = source.Replace("\r\n", "\n").Replace('\r', '\n');
            len = src.Length;
        }

        char At(int k) => k >= 0 && k < len ? src[k] : '\0';

        static bool IsDigit(char c) => c >= '0' && c <= '9';

        TokenizeResult Fail(string pyType, string message, int errLine) =>
            new TokenizeResult { Tokens = tokens, Error = new PythonException(pyType, message, errLine) };

        void Push(TokenType type, string value, int startLine, int startCol, string prefix = null, NumberKind kind = NumberKind.Int)
        {
            tokens.Add(new Token
            {
                Type = type, Value = value, Line = startLine, Col = startCol, EndLine = line, EndCol = i - lineStart,
                Prefix = prefix, NumberKind = kind,
            });
            if (type != TokenType.INDENT && type != TokenType.DEDENT && type != TokenType.NEWLINE) lineHasTokens = true;
        }

        void PushAt(TokenType type, string value, int l, int col, int endL, int endCol) =>
            tokens.Add(new Token { Type = type, Value = value, Line = l, Col = col, EndLine = endL, EndCol = endCol });

        static string CodePointLabel(int cp) => "U+" + cp.ToString("X4");

        static bool IsIdStart(string s, int k)
        {
            if (s[k] == '_') return true;
            switch (Values.Category(s, k))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsIdContinue(string s, int k)
        {
            if (IsIdStart(s, k)) return true;
            switch (Values.Category(s, k))
            {
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>k'deki kod noktasının UTF-16 uzunluğu (1 ya da 2)</summary>
        int CharLen(int k) => char.IsHighSurrogate(src[k]) && k + 1 < len && char.IsLowSurrogate(src[k + 1]) ? 2 : 1;

        bool StartsWith(string s, int at) => at + s.Length <= len && string.CompareOrdinal(src, at, s, 0, s.Length) == 0;

        TokenizeResult Run()
        {
            while (true)
            {
                // Yeni mantıksal satırın başı: girintiyi ölç.
                if (atLineStart)
                {
                    int col = 0;
                    int alt = 0;
                    while (i < len)
                    {
                        char ch0 = src[i];
                        if (ch0 == ' ')
                        {
                            col++;
                            alt++;
                        }
                        else if (ch0 == '\t')
                        {
                            col = (col / 8 + 1) * 8;
                            alt++;
                        }
                        else if (ch0 == '\f')
                        {
                            col = alt = 0;
                        }
                        else break;
                        i++;
                    }
                    // Boş ya da sadece yorum olan satırlar girintiyi etkilemez.
                    if (At(i) == '#') while (i < len && src[i] != '\n') i++;
                    if (i < len && src[i] == '\n')
                    {
                        i++;
                        line++;
                        lineStart = i;
                        continue;
                    }
                    if (i >= len) break;

                    int top = indents[indents.Count - 1];
                    int altTop = altIndents[altIndents.Count - 1];
                    if (col > top)
                    {
                        if (alt <= altTop) return Fail("TabError", "inconsistent use of tabs and spaces in indentation", line);
                        indents.Add(col);
                        altIndents.Add(alt);
                        PushAt(TokenType.INDENT, src.Substring(lineStart, i - lineStart), line, 0, line, i - lineStart);
                    }
                    else if (col < top)
                    {
                        while (indents.Count > 1 && col < indents[indents.Count - 1])
                        {
                            indents.RemoveAt(indents.Count - 1);
                            altIndents.RemoveAt(altIndents.Count - 1);
                            PushAt(TokenType.DEDENT, "", line, i - lineStart, line, i - lineStart);
                        }
                        if (col != indents[indents.Count - 1])
                            return Fail("IndentationError", "unindent does not match any outer indentation level", line);
                        if (alt != altIndents[altIndents.Count - 1])
                            return Fail("TabError", "inconsistent use of tabs and spaces in indentation", line);
                    }
                    else if (alt != altTop)
                    {
                        return Fail("TabError", "inconsistent use of tabs and spaces in indentation", line);
                    }
                    atLineStart = false;
                }

                if (i >= len) break;
                char ch = src[i];
                int startLine = line;
                int startCol = i - lineStart;

                if (ch == ' ' || ch == '\t' || ch == '\f')
                {
                    i++;
                    continue;
                }

                if (ch == '#')
                {
                    while (i < len && src[i] != '\n') i++;
                    continue;
                }

                if (ch == '\n')
                {
                    if (brackets.Count == 0 && lineHasTokens)
                        PushAt(TokenType.NEWLINE, "\n", line, startCol, line, startCol + 1);
                    i++;
                    line++;
                    lineStart = i;
                    if (brackets.Count == 0)
                    {
                        atLineStart = true;
                        lineHasTokens = false;
                    }
                    continue;
                }

                // Satır devamı: \ + satır sonu
                if (ch == '\\')
                {
                    if (At(i + 1) == '\n')
                    {
                        i += 2;
                        line++;
                        lineStart = i;
                        continue;
                    }
                    if (i + 1 >= len) return Fail("SyntaxError", "unexpected EOF while parsing", line);
                    return Fail("SyntaxError", "unexpected character after line continuation character", line);
                }

                // Sayı
                if (IsDigit(ch) || (ch == '.' && IsDigit(At(i + 1))))
                {
                    var result = ReadNumber();
                    if (result != null) return result;
                    continue;
                }

                // İsim ya da önekli metin (r"...", f"...")
                if (IsIdStart(src, i))
                {
                    int j = i;
                    while (j < len && IsIdContinue(src, j)) j += CharLen(j);
                    string word = src.Substring(i, j - i);
                    char quote = At(j);
                    if ((quote == '"' || quote == '\'') && StringPrefixes.Contains(word.ToLowerInvariant()))
                    {
                        i = j;
                        var result = ReadString(word.ToLowerInvariant(), startLine, startCol);
                        if (result != null) return result;
                        continue;
                    }
                    i = j;
                    Push(TokenType.NAME, NormalizeName(word), startLine, startCol);
                    continue;
                }

                // Metin
                if (ch == '"' || ch == '\'')
                {
                    var result = ReadString("", startLine, startCol);
                    if (result != null) return result;
                    continue;
                }

                // İşleç ve parantez
                string op = null;
                foreach (var o in Operators)
                {
                    if (StartsWith(o, i))
                    {
                        op = o;
                        break;
                    }
                }
                if (op != null)
                {
                    if (op == "(" || op == "[" || op == "{")
                    {
                        brackets.Add((op, line));
                    }
                    else if (Closers.TryGetValue(op, out string opener))
                    {
                        if (brackets.Count == 0) return Fail("SyntaxError", "unmatched '" + op + "'", line);
                        var open = brackets[brackets.Count - 1];
                        brackets.RemoveAt(brackets.Count - 1);
                        if (open.ch != opener)
                        {
                            string where = open.line == line ? "" : " on line " + open.line;
                            return Fail(
                                "SyntaxError",
                                "closing parenthesis '" + op + "' does not match opening parenthesis '" + open.ch + "'" + where,
                                line);
                        }
                    }
                    i += op.Length;
                    Push(TokenType.OP, op, startLine, startCol);
                    continue;
                }

                int cp = Values.CodePointAt(src, i);
                if (Values.IsNonPrintable(src, i))
                    return Fail("SyntaxError", "invalid non-printable character " + CodePointLabel(cp), line);
                if (cp > 127)
                    return Fail("SyntaxError", "invalid character '" + Values.FromCodePoint(cp) + "' (" + CodePointLabel(cp) + ")", line);
                // $ ? ! ` gibi ASCII karakterler: CPython bunları geçersiz parça olarak geçirir,
                // cümle çözücü "invalid syntax" der.
                i += 1;
                Push(TokenType.OP, ch.ToString(), startLine, startCol);
            }

            if (brackets.Count > 0)
            {
                var open = brackets[brackets.Count - 1];
                return Fail("SyntaxError", "'" + open.ch + "' was never closed", open.line);
            }
            // CPython dosya sonu parçalarını son karakterin satırında gösterir.
            bool endsWithNewline = len > 0 && src[len - 1] == '\n';
            int endLine = endsWithNewline ? line - 1 : line;
            int endCol = endsWithNewline ? 0 : i - lineStart;
            if (lineHasTokens) PushAt(TokenType.NEWLINE, "", endLine, endCol, endLine, endCol);
            while (indents.Count > 1)
            {
                indents.RemoveAt(indents.Count - 1);
                PushAt(TokenType.DEDENT, "", endLine, endCol, endLine, endCol);
            }
            PushAt(TokenType.ENDMARKER, "", endLine, endCol, endLine, endCol);
            return new TokenizeResult { Tokens = tokens, Error = null };
        }

        /// <summary>Python isimleri NFKC biçimine çevirir (sadece ASCII dışı harf varsa gerekir).</summary>
        static string NormalizeName(string word)
        {
            foreach (char c in word)
            {
                if (c > 127) return word.Normalize(NormalizationForm.FormKC);
            }
            return word;
        }

        // --- yardımcılar ---

        static string StickyMatch(Regex re, string s, int at)
        {
            var m = re.Match(s, at);
            return m.Success ? m.Value : null;
        }

        TokenizeResult ReadNumber()
        {
            int startLine = line;
            int startCol = i - lineStart;
            string text;
            var kind = NumberKind.Int;
            string label = "decimal";

            bool radix = At(i) == '0' && "xXoObB".IndexOf(At(i + 1)) >= 0 && At(i + 1) != '\0';
            if (radix)
            {
                char letter = char.ToLowerInvariant(At(i + 1));
                label = letter == 'x' ? "hexadecimal" : letter == 'o' ? "octal" : "binary";
                text = StickyMatch(letter == 'x' ? Hex : letter == 'o' ? Oct : Bin, src, i);
                char after = At(i + (text != null ? text.Length : 2));
                if (letter != 'x' && IsDigit(after))
                    return Fail("SyntaxError", "invalid digit '" + after + "' in " + label + " literal", line);
                if (text == null) return Fail("SyntaxError", "invalid " + label + " literal", line);
            }
            else
            {
                text = StickyMatch(FloatRe, src, i);
                if (text != null)
                {
                    kind = NumberKind.Float;
                }
                else
                {
                    text = StickyMatch(IntRe, src, i);
                    if (LeadingZeros.IsMatch(text) && text.IndexOfAny("123456789".ToCharArray()) >= 0)
                    {
                        return Fail(
                            "SyntaxError",
                            "leading zeros in decimal integer literals are not permitted; use an 0o prefix for octal integers",
                            line);
                    }
                }
            }

            int end = i + text.Length;
            if (!radix && (At(end) == 'j' || At(end) == 'J'))
            {
                end++;
                kind = NumberKind.Imaginary;
            }
            if (end < len && IsIdContinue(src, end)) return Fail("SyntaxError", "invalid " + label + " literal", line);
            string value = src.Substring(i, end - i);
            i = end;
            Push(TokenType.NUMBER, value, startLine, startCol, null, kind);
            return null;
        }

        TokenizeResult ReadString(string prefix, int startLine, int startCol)
        {
            char quote = src[i];
            bool triple = StartsWith(new string(quote, 3), i);
            string closing = triple ? new string(quote, 3) : quote.ToString();
            bool raw = prefix.Contains("r") || prefix.Contains("f");
            var value = new StringBuilder();
            i += closing.Length;

            while (true)
            {
                if (i >= len)
                {
                    if (triple)
                    {
                        int lastLine = len > 0 && src[len - 1] == '\n' ? line - 1 : line;
                        return Fail("SyntaxError", "unterminated triple-quoted string literal (detected at line " + lastLine + ")", startLine);
                    }
                    return Fail("SyntaxError", "unterminated string literal (detected at line " + line + ")", startLine);
                }
                if (StartsWith(closing, i))
                {
                    i += closing.Length;
                    break;
                }
                char c = src[i];
                if (c == '\n')
                {
                    if (!triple) return Fail("SyntaxError", "unterminated string literal (detected at line " + line + ")", startLine);
                    value.Append(c);
                    i++;
                    line++;
                    lineStart = i;
                    continue;
                }
                if (c == '\\' && i + 1 < len)
                {
                    char n = src[i + 1];
                    if (n == '\n')
                    {
                        // Metin içinde satır devamı: ham metinde korunur, normalde silinir.
                        if (raw) value.Append("\\\n");
                        i += 2;
                        line++;
                        lineStart = i;
                        continue;
                    }
                    if (raw)
                    {
                        value.Append(c).Append(n);
                        i += 2;
                        continue;
                    }
                    i += 2;
                    value.Append(ReadEscape(n));
                    continue;
                }
                value.Append(c);
                i++;
            }

            Push(TokenType.STRING, value.ToString(), startLine, startCol, prefix);
            return null;
        }

        static bool IsOctal(char c) => c >= '0' && c <= '7';

        static bool IsHex(char c) => IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

        // Kaçış dizisi: "\" ve n okunmuş, i ondan sonrasını gösteriyor.
        string ReadEscape(char n)
        {
            switch (n)
            {
                case '\\': return "\\";
                case '\'': return "'";
                case '"': return "\"";
                case 'a': return "\x07";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
            }
            if (IsOctal(n))
            {
                string digits = n.ToString();
                while (digits.Length < 3 && IsOctal(At(i))) digits += src[i++];
                return Values.FromCodePoint(Convert.ToInt32(digits, 8));
            }
            int hexLength = n == 'x' ? 2 : n == 'u' ? 4 : n == 'U' ? 8 : 0;
            if (hexLength > 0 && i + hexLength <= len)
            {
                string hex = src.Substring(i, hexLength);
                bool ok = true;
                foreach (char h in hex) ok &= IsHex(h);
                if (ok)
                {
                    long cp = Convert.ToInt64(hex, 16);
                    if (cp <= 0x10FFFF)
                    {
                        i += hexLength;
                        return Values.FromCodePoint((int)cp);
                    }
                }
            }
            // Bilinmeyen kaçış: Python ters bölüyü olduğu gibi bırakır (sadece uyarı verir).
            return "\\" + n;
        }
    }
}
