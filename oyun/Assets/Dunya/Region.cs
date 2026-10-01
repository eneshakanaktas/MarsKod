// Mars'ın bölgeleri: her 10 bölüm yeni bir bölgede geçer (hikâye: docs/tasarim/hikaye.md, bölge tablosu).
// Bölge, bölümün numarasından bulunur; görünüşü (zemin, gökyüzü, ışık) oyun tarafı bölgeye göre seçer.
// Yeni bölge eklemek: enum'a bir değer + aşağıdaki iki tabloya birer satır (sıra = bölge sırası).

namespace MarsKod.Dunya
{
    public enum Region { Plain, PolarIce }

    public static class Regions
    {
        public const int LevelsPerRegion = 10;

        // Bölge sırasıyla: 1. bölge Bölüm 1-10, 2. bölge Bölüm 11-20...
        static readonly Region[] Order = { Region.Plain, Region.PolarIce };

        /// <summary>Bölümün geçtiği bölge (henüz yazılmamış bölgelerin bölümleri son bölgede kalır)</summary>
        public static Region Of(int levelNumber)
        {
            int index = (levelNumber - 1) / LevelsPerRegion;
            if (index < 0) index = 0;
            if (index >= Order.Length) index = Order.Length - 1;
            return Order[index];
        }

        /// <summary>Bölgede toplanan şey (iniş ovası: enerji hücresi, kutup buzulu: buz)</summary>
        public static Collectible Item(Region r) => r == Region.PolarIce ? Collectible.Ice : Collectible.EnergyCell;
    }
}
