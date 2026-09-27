// "Did you mean: 'enerji'?" önerileri. CPython 3.12 Python/suggestions.c ile birebir aynı algoritma.

using System;
using System.Collections.Generic;
using System.Text;

namespace MarsKod.Motor
{
    public static class Suggestions
    {
        const int MaxCandidateItems = 750;
        const int MaxStringSize = 40;
        const int MoveCost = 2;
        const int CaseCost = 1;

        // CPython karakterleri UTF-8 baytları olarak karşılaştırır.
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, false);

        static int SubstitutionCost(int a, int b)
        {
            if ((a & 31) != (b & 31)) return MoveCost;
            if (a == b) return 0;
            if (a >= 65 && a <= 90) a += 32;
            if (b >= 65 && b <= 90) b += 32;
            return a == b ? CaseCost : MoveCost;
        }

        static int Levenshtein(byte[] aFull, byte[] bFull, int maxCost)
        {
            int aStart = 0, bStart = 0, aEnd = aFull.Length, bEnd = bFull.Length;
            // Ortak baş ve sonları at
            while (aStart < aEnd && bStart < bEnd && aFull[aStart] == bFull[bStart])
            {
                aStart++;
                bStart++;
            }
            while (aStart < aEnd && bStart < bEnd && aFull[aEnd - 1] == bFull[bEnd - 1])
            {
                aEnd--;
                bEnd--;
            }
            var a = new ArraySegment<byte>(aFull, aStart, aEnd - aStart);
            var b = new ArraySegment<byte>(bFull, bStart, bEnd - bStart);
            if (a.Count == 0 || b.Count == 0) return (a.Count + b.Count) * MoveCost;
            if (a.Count > MaxStringSize || b.Count > MaxStringSize) return maxCost + 1;
            if (b.Count < a.Count)
            {
                var swap = a;
                a = b;
                b = swap;
            }
            if ((b.Count - a.Count) * MoveCost > maxCost) return maxCost + 1;

            var buffer = new int[a.Count];
            int tmp = MoveCost;
            for (int i = 0; i < a.Count; i++)
            {
                buffer[i] = tmp;
                tmp += MoveCost;
            }
            int result = 0;
            for (int bIndex = 0; bIndex < b.Count; bIndex++)
            {
                int code = b.Array[b.Offset + bIndex];
                int distance = result = bIndex * MoveCost;
                int minimum = int.MaxValue;
                for (int index = 0; index < a.Count; index++)
                {
                    int substitute = distance + SubstitutionCost(code, a.Array[a.Offset + index]);
                    distance = buffer[index];
                    int insertDelete = Math.Min(result, distance) + MoveCost;
                    result = Math.Min(insertDelete, substitute);
                    buffer[index] = result;
                    if (result < minimum) minimum = result;
                }
                if (minimum > maxCost) return maxCost + 1;
            }
            return result;
        }

        /// <summary>Adaylar arasından en yakın ismi bulur (CPython calculate_suggestions); yoksa null.</summary>
        public static string CalculateSuggestion(IList<string> candidates, string name)
        {
            if (candidates.Count >= MaxCandidateItems) return null;
            byte[] nameBytes = Utf8.GetBytes(name);
            string best = null;
            int bestDistance = int.MaxValue;
            foreach (var item in candidates)
            {
                if (item == name) continue;
                byte[] itemBytes = Utf8.GetBytes(item);
                int maxDistance = Math.Min((nameBytes.Length + itemBytes.Length + 3) * MoveCost / 6, bestDistance - 1);
                int distance = Levenshtein(nameBytes, itemBytes, maxDistance);
                if (distance > maxDistance) continue;
                if (best == null || distance < bestDistance)
                {
                    best = item;
                    bestDistance = distance;
                }
            }
            return best;
        }

        /// <summary>
        /// NameError mesajı. Aday sırası CPython'daki gibi: önce fonksiyonun yerel isimleri,
        /// bulunamazsa globaller, sonra yerleşikler. Standart kütüphane modülüyse import ipucu eklenir.
        /// </summary>
        public static string NameErrorMessage(string name, IEnumerable<IList<string>> scopes)
        {
            string message = "name '" + name + "' is not defined";
            string suggestion = null;
            foreach (var scope in scopes)
            {
                suggestion = CalculateSuggestion(scope, name);
                if (suggestion != null) break;
            }
            if (suggestion != null) message += ". Did you mean: '" + suggestion + "'?";
            if (Array.IndexOf(PythonNames.StdlibModuleNames, name) >= 0)
                message += suggestion != null ? " Or did you forget to import '" + name + "'?" : ". Did you forget to import '" + name + "'?";
            return message;
        }
    }
}
