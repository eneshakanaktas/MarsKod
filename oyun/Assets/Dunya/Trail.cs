// Kareden kareye yön listesi: Bölge 7'de Serçe'nin gördüğü göç izi (scout() izin sonraki yönünü söyler; "izler"),
// Bölge 8'de notun yolu (Level.NoteRoutes; "notlar"). Hedef yolun son karesidir.

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    public sealed class Trail
    {
        public readonly IReadOnlyList<Direction> Steps;
        /// <summary>İzin kareleri: Cells[0] başlangıç, Cells[i] i. adımdan sonra varılan kare</summary>
        public readonly IReadOnlyList<Cell> Cells;

        public Trail(Cell start, IEnumerable<Direction> steps)
        {
            Steps = steps.ToList();
            var cells = new List<Cell> { start };
            foreach (var step in Steps) cells.Add(cells[cells.Count - 1].Step(step));
            Cells = cells;
        }

        public Cell End => Cells[Cells.Count - 1];

        /// <summary>Karenin izdeki sırası (0 başlangıç); izde değilse -1</summary>
        public int IndexOf(Cell c)
        {
            for (int i = 0; i < Cells.Count; i++)
                if (Cells[i].Equals(c)) return i;
            return -1;
        }

        /// <summary>İz aynı kareden iki kez geçiyor mu (o zaman scout() o karede iki yön arasında kalırdı)</summary>
        public bool CrossesItself => Cells.Distinct().Count() != Cells.Count;
    }
}
