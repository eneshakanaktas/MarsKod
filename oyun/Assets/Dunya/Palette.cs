// Acemi paleti: bölümün hazır satırları (bölüm dosyasındaki "parcalar").
// Tasarım: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §4. Saf mantık.

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    public static class Palette
    {
        /// <summary>Bu bölümde ilk kez görülen parçalar (düğmesi turuncu kenarlı gösterilir). İlk bölümde her şey yeni
        /// olduğundan hiçbiri işaretlenmez (hepsi turuncu olunca "yeni" bir şey anlatmaz).</summary>
        public static HashSet<string> NewPieces(IEnumerable<Level> levels, int number)
        {
            var all = levels.ToList();
            var current = all.FirstOrDefault(l => l.Number == number);
            var earlier = all.Where(l => l.Number < number).ToList();
            if (current == null || earlier.Count == 0) return new HashSet<string>();
            var seen = new HashSet<string>(earlier.SelectMany(l => l.Pieces));
            return new HashSet<string>(current.Pieces.Where(p => !seen.Contains(p)));
        }
    }
}
