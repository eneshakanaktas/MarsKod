using System.Collections.Generic;

namespace MarsKod.Dunya
{
    /// <summary>
    /// Sahnelerde (Bölge 6: 51 roketin inişi, 60 kanyona iniş) robotun yalnızca boş karelerden yürüdüğü yol.
    /// Oyun kuralı değildir: bölümün çözümünü etkilemez, yalnızca sahnenin yürüyüşünü hesaplar.
    /// </summary>
    public static class GridPath
    {
        static readonly Direction[] Order = { Direction.North, Direction.East, Direction.South, Direction.West };   // her koşuda aynı sonuç

        /// <summary>En kısa yol (from ve to dahil); engelin üstünden geçmez. Yol yoksa ya da to engelliyse null.</summary>
        public static List<Cell> Find(int cols, int rows, Cell from, Cell to, ICollection<Cell> blocked)
        {
            if (blocked.Contains(to)) return null;
            var cameFrom = new Dictionary<Cell, Cell>();
            var queue = new Queue<Cell>();
            queue.Enqueue(from);
            cameFrom[from] = from;
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (cell.Equals(to)) return Trace(cameFrom, from, to);
                foreach (var dir in Order)
                {
                    var next = cell.Step(dir);
                    if (next.Col < 0 || next.Row < 0 || next.Col >= cols || next.Row >= rows) continue;
                    if (blocked.Contains(next) || cameFrom.ContainsKey(next)) continue;
                    cameFrom[next] = cell;
                    queue.Enqueue(next);
                }
            }
            return null;
        }

        /// <summary>Satırda preferCol'a en yakın boş kare (eşitlikte soldaki); satır doluysa null.</summary>
        public static Cell? OpenInRow(int cols, int row, int preferCol, ICollection<Cell> blocked)
        {
            for (int distance = 0; distance < cols; distance++)
                foreach (int col in new[] { preferCol - distance, preferCol + distance })
                    if (col >= 0 && col < cols && !blocked.Contains(new Cell(col, row))) return new Cell(col, row);
            return null;
        }

        /// <summary>
        /// Robotun üstünden geçemeyeceği kareler: kaya, panel, kristal, makara. itemsTaken false ise toplanacaklar da.
        /// Toz bulutu katı değildir (oyun kuralında yalnızca yolu görünmez kılar): sahne yürüyüşü içinden geçer.
        /// Yoksa 51'de robotun başlangıcı (kaya + toz arasında) ve 60'ın üst yarısı kapalı kalır.
        /// </summary>
        public static HashSet<Cell> Obstacles(Level level, bool itemsTaken)
        {
            var blocked = new HashSet<Cell>(level.Rocks);
            blocked.UnionWith(level.Panels.Keys);
            blocked.UnionWith(level.Crystals);
            blocked.UnionWith(level.Reels.Keys);
            if (!itemsTaken) blocked.UnionWith(level.Ices);
            return blocked;
        }

        static List<Cell> Trace(Dictionary<Cell, Cell> cameFrom, Cell from, Cell to)
        {
            var path = new List<Cell> { to };
            for (var cell = to; !cell.Equals(from); cell = cameFrom[cell]) path.Add(cameFrom[cell]);
            path.Reverse();
            return path;
        }
    }
}
