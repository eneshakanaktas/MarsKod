// Değişen alan ("duzenler"): bölüm dosyasındaki 1-3 düzen. Her düzen haritanın boyunda satırlardır ve yalnızca her denemede
// değişen şeyleri taşır: toplanacaklar (bölümün işareti, Bölge 2'de B), T kristal, K engel, U krater, . boş.
// Hiç değişmeyen şeyler (robot, hedef...) "harita"da kalır. Kod her düzende ayrı denenir (Level.RunVersions).
// Kılavuz: docs/tasarim/bolum-dosyasi.md

using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    /// <summary>Bir denemede haritanın üstüne gelen, her denemede değişen nesneler</summary>
    public sealed class Layout
    {
        public readonly List<Cell> Ices = new List<Cell>();
        public readonly List<Cell> Crystals = new List<Cell>();
        public readonly List<Cell> Rocks = new List<Cell>();
        public readonly List<Cell> Craters = new List<Cell>();

        public bool IsEmpty => Ices.Count + Crystals.Count + Rocks.Count + Craters.Count == 0;

        /// <summary>İki düzenin aynı olup olmadığını anlamak için metin</summary>
        public string Key => string.Join("|", new[] { Ices, Crystals, Rocks, Craters }.Select(cells => string.Join(";", cells)));
    }

    /// <summary>"duzenler" alanını okur ve denetler (Level.Parse çağırır)</summary>
    static class LayoutFile
    {
        public const string Field = "duzenler";

        public static void Read(Level level, Dictionary<string, object> d)
        {
            if (!d.ContainsKey(Field)) return;
            if (level.Trails.Count > 0)
                throw new DataFormatError("\"" + Field + "\" ile \"izler\" birlikte yazılmaz.");
            if (!(d[Field] is List<object> layouts) || layouts.Count < 1 || layouts.Count > Level.MaxTrails)
                throw new DataFormatError("\"" + Field + "\" 1-" + Level.MaxTrails + " düzen içeren bir liste olmalı; her düzen haritanın boyunda satırlar, örn. [[\". . U . . .\", ...]].");
            if (level.Notes.Count > 0 && level.Notes.Count != layouts.Count)
                throw new DataFormatError("\"notlar\" ile \"" + Field + "\" aynı sayıda olmalı: her denemede bir not ve bir düzen (şimdi " + level.Notes.Count + " not, " + layouts.Count + " düzen).");
            for (int i = 0; i < layouts.Count; i++)
            {
                string where = "\"" + Field + "\" içindeki " + (i + 1) + ". düzen";
                var layout = ReadOne(level, layouts[i], where);
                if (level.Notes.Count > 0 && level.Notes[i].Route.Cells.Any(c => layout.Craters.Contains(c) || layout.Rocks.Contains(c)))
                    throw new DataFormatError(where + " " + (i + 1) + ". notun yolunun üstüne engel ya da krater koyuyor; yol onlardan geçemez.");
                level.Layouts.Add(layout);
            }
            CheckAcrossLayouts(level);
        }

        static Layout ReadOne(Level level, object value, string where)
        {
            var rows = value is List<object> list && list.All(r => r is string) ? list.Cast<string>().Select(r => r.Replace(" ", "")).ToList() : null;
            if (rows == null || rows.Count != level.Rows || rows.Any(r => r.Length != level.Cols))
                throw new DataFormatError(where + " haritanın boyunda olmalı: " + level.Rows + " satır, her satırda " + level.Cols + " kare.");
            var layout = new Layout();
            for (int i = 0; i < rows.Count; i++)
                for (int col = 0; col < level.Cols; col++)
                {
                    char c = rows[i][col];
                    if (c == Level.EmptyMark) continue;
                    var cell = new Cell(col, level.Rows - 1 - i);
                    if (IsTaken(level, cell))
                        throw new DataFormatError(where + " " + (i + 1) + ". satırda haritada dolu bir kareye bir şey koyuyor; düzendeki nesneler boş kareye gelir.");
                    Place(level, layout, c, cell, where);
                }
            if (layout.IsEmpty) throw new DataFormatError(where + " boş: en az bir nesne yaz.");
            return layout;
        }

        static void Place(Level level, Layout layout, char mark, Cell cell, string where)
        {
            char itemMark = Collectibles.Marks.First(m => m.Value == level.Item).Key;
            if (mark == Level.CrystalMark) layout.Crystals.Add(cell);
            else if (mark == Level.RockMark) layout.Rocks.Add(cell);
            else if (mark == Level.CraterMark)
            {
                if (!level.Commands.Contains("meteor_ahead"))
                    throw new DataFormatError(where + " içinde krater (" + Level.CraterMark + ") var ama \"komutlar\" içinde \"meteor_ahead\" yok; oyuncu kraterin nerede olduğuna bakamaz.");
                layout.Craters.Add(cell);
            }
            else if (Collectibles.Marks.TryGetValue(mark, out var kind) && kind == level.Item)
            {
                if (!level.Commands.Contains("collect"))
                    throw new DataFormatError(where + " içinde toplanacak " + Collectibles.Name(level.Item) + " var ama \"komutlar\" içinde \"collect\" yok; toplanamaz.");
                layout.Ices.Add(cell);
            }
            else
                throw new DataFormatError(where + " içinde yalnızca " + itemMark + " (toplanacak), " + Level.CrystalMark + " (kristal), " + Level.RockMark + " (engel), "
                    + Level.CraterMark + " (krater) ve " + Level.EmptyMark + " (boş) yazılır; '" + mark + "' var.");
        }

        // Haritada zaten bir şey olan, robotun ya da hedefin durduğu kare
        static bool IsTaken(Level level, Cell cell) =>
            cell.Equals(level.Robot) || (level.Target != null && level.Target.Value.Equals(cell)) || level.Rocks.Contains(cell) || level.Ices.Contains(cell)
            || level.Crystals.Contains(cell) || level.Craters.Contains(cell) || level.HotStones.Contains(cell) || level.Panels.ContainsKey(cell) || level.Dust.ContainsKey(cell);

        static void CheckAcrossLayouts(Level level)
        {
            if (level.Layouts.Select(l => l.Key).Distinct().Count() < level.Layouts.Count)
                throw new DataFormatError("\"" + Field + "\" içinde aynı düzen iki kez yazılmış; değişen alanda düzenler farklı olmalı.");
            int total = level.IcesOf(0).Count;
            for (int v = 1; v < level.Layouts.Count; v++)
                if (level.IcesOf(v).Count != total)
                    throw new DataFormatError("\"" + Field + "\" içindeki düzenlerde toplanacak sayısı aynı olmalı (görev yazısı tek): 1. düzende " + total + ", " + (v + 1) + ". düzende " + level.IcesOf(v).Count + ".");
            if (level.Layouts.Any(l => l.Ices.Count > 0) && level.CrystalSizes.Count + level.Reels.Count > 0)
                throw new DataFormatError("\"" + Field + "\" içinde toplanacak nesne yazılırsa boyları/uzunlukları (kristal_boyu, makara_uzunlugu) verilemez; ölçülen nesneler haritada kalır.");
        }
    }
}
