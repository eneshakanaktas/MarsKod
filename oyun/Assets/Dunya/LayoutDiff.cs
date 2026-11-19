// Değişen alanda iki düzen arasında içeriği değişen kareler (A1: tipi/meteor geçişinden sonra bu kareler kısa süre parlar).
// Bir karede bir şey belirdiyse, kaybolduysa ya da türü değiştiyse (buz ↔ kristal gibi) o kare değişmiştir.

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    public static class LayoutDiff
    {
        public static List<Cell> ChangedCells(Level level, int from, int to)
        {
            var before = Contents(level, from);
            var after = Contents(level, to);
            return before.Keys.Union(after.Keys)
                .Where(cell => !before.TryGetValue(cell, out char a) || !after.TryGetValue(cell, out char b) || a != b)
                .OrderBy(cell => cell.Row).ThenBy(cell => cell.Col)
                .ToList();
        }

        // Bu düzende her dolu karede ne var (işaretler bölüm dosyasındakiyle aynı)
        static Dictionary<Cell, char> Contents(Level level, int version)
        {
            var contents = new Dictionary<Cell, char>();
            foreach (var cell in level.IcesOf(version)) contents[cell] = 'B';
            foreach (var cell in level.CrystalsOf(version)) contents[cell] = Level.CrystalMark;
            foreach (var cell in level.RocksOf(version)) contents[cell] = Level.RockMark;
            foreach (var cell in level.CratersOf(version)) contents[cell] = Level.CraterMark;
            return contents;
        }
    }
}
