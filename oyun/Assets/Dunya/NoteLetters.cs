// Notlardaki yön harfleri (Bölge 8'den): koloni Türkçe konuştuğu için Ece ve Defne yönleri Türkçe kısaltmayla
// yazmış: K kuzey, G güney, D doğu, B batı. Komutlar değişmez (move(North)); oyuncu harfi yöne çeviren rutini
// kendisi yazar. V: veri kartı burada (robot yürümez).

using System.Collections.Generic;

namespace MarsKod.Dunya
{
    public static class NoteLetters
    {
        /// <summary>"Veri kartı burada" harfi: yolda adım sayılmaz</summary>
        public const char Collect = 'V';

        public static readonly IReadOnlyDictionary<char, Direction> Directions = new Dictionary<char, Direction>
        {
            ['K'] = Direction.North,
            ['G'] = Direction.South,
            ['D'] = Direction.East,
            ['B'] = Direction.West,
        };

        /// <summary>Değer tek bir yön harfi mi ("K"): oyuncu harfi çevirmeden move'a vermiş</summary>
        public static bool IsDirectionLetter(object value) => value is string s && s.Length == 1 && Directions.ContainsKey(s[0]);

        /// <summary>Notun yürünen adımları (V atlanır); bilinmeyen harf varsa null</summary>
        public static List<Direction> Steps(string note)
        {
            var steps = new List<Direction>(note.Length);
            foreach (char c in note)
            {
                if (c == Collect) continue;
                if (!Directions.TryGetValue(c, out var d)) return null;
                steps.Add(d);
            }
            return steps;
        }
    }
}
