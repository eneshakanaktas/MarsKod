// Öneri satırı (Orta kademe): imlecin solundaki yarım kelimeye göre en çok 3 öneri.
// Tasarım: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §5. Saf mantık.
// Kelimeler: oyun komutları, yönler, bölümlerde açılmış Python kelimeleri, koddaki isimler (atama / for / def).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MarsKod.Motor;

namespace MarsKod.Dunya
{
    public sealed class Suggestion
    {
        public string Word;
        /// <summary>Fonksiyonsa seçilince "(" de eklenir.</summary>
        public bool IsFunction;
    }

    public static class Suggestions
    {
        public const int Max = 3;

        /// <summary>Python'un ayrılmış kelimeleri (bölüm dosyasındaki "python_kelimeleri" bunlardan ya da yerleşik isimlerden olabilir).</summary>
        public static readonly string[] PythonKeywords =
        {
            "False", "None", "True", "and", "as", "assert", "async", "await", "break", "class", "continue", "def", "del", "elif",
            "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal", "not", "or",
            "pass", "raise", "return", "try", "while", "with", "yield",
        };

        /// <summary>Motorun tanıdığı bir Python kelimesi mi (ayrılmış kelime ya da yerleşik isim).</summary>
        public static bool IsPythonWord(string word)
            => Array.IndexOf(PythonKeywords, word) >= 0 || Array.IndexOf(PythonNames.BuiltinNames, word) >= 0;

        /// <summary>Bölüm numarasına kadar (o dahil) açılmış kelimeler: komutlar, yönler ve Python kelimeleri; önceki bölümlerinkiler birleşir.</summary>
        public static List<string> OpenWords(IEnumerable<Level> levels, int upToNumber)
        {
            var words = new List<string>();
            var open = levels.Where(l => l.Number <= upToNumber).OrderBy(l => l.Number).ToList();
            foreach (var l in open) words.AddRange(l.Commands);
            if (open.Any(l => l.Commands.Contains("move"))) words.AddRange(Enum.GetNames(typeof(Direction)));
            foreach (var l in open) words.AddRange(l.PythonWords);
            return words.Distinct().ToList();
        }

        /// <summary>partial: imlecin solundaki yarım kelime (en az 1 harf); code: yazılmış kod; openWords: OpenWords sonucu.</summary>
        public static List<Suggestion> Suggest(string partial, string code, IEnumerable<string> openWords)
        {
            var result = new List<Suggestion>();
            if (string.IsNullOrEmpty(partial)) return result;
            var commands = new HashSet<string>(World.AllCommands);
            var defined = DefinedNames(code ?? "");
            foreach (var w in openWords.Concat(defined.Names))
            {
                if (result.Count >= Max) break;
                if (!w.StartsWith(partial, StringComparison.OrdinalIgnoreCase)) continue;
                if (result.Any(s => s.Word == w)) continue;
                bool fn = commands.Contains(w) || defined.Functions.Contains(w) || IsBuiltinFunction(w);
                if (w == partial && !fn) continue; // zaten tamamlanmış
                result.Add(new Suggestion { Word = w, IsFunction = fn });
            }
            return result;
        }

        static bool IsBuiltinFunction(string w)
            => Array.IndexOf(PythonNames.BuiltinNames, w) >= 0 && char.IsLower(w[0]) && !w.StartsWith("_");

        static readonly Regex DefRe = new Regex(@"^\s*def\s+([A-Za-z_]\w*)", RegexOptions.Multiline);
        static readonly Regex AssignRe = new Regex(@"^\s*([A-Za-z_]\w*)\s*(?:[-+*/%]|//)?=(?!=)", RegexOptions.Multiline);
        static readonly Regex ForRe = new Regex(@"^\s*for\s+([A-Za-z_]\w*(?:\s*,\s*[A-Za-z_]\w*)*)\s+in\b", RegexOptions.Multiline);

        static (List<string> Names, HashSet<string> Functions) DefinedNames(string code)
        {
            var names = new List<string>();
            var functions = new HashSet<string>();
            foreach (Match m in DefRe.Matches(code))
            {
                names.Add(m.Groups[1].Value);
                functions.Add(m.Groups[1].Value);
            }
            foreach (Match m in AssignRe.Matches(code)) names.Add(m.Groups[1].Value);
            foreach (Match m in ForRe.Matches(code))
                foreach (var n in m.Groups[1].Value.Split(',')) names.Add(n.Trim());
            return (names.Distinct().ToList(), functions);
        }
    }
}
