// Bölümde toplanan şey: her bölge kendi nesnesini toplar (hikâye: docs/tasarim/hikaye.md).
// Oyun kuralı hepsi için aynıdır (collect() toplar); yalnızca haritadaki işareti, adı ve görünüşü değişir.
// Yeni tür eklemek: enum'a bir değer + aşağıdaki iki tabloya birer satır.

using System.Collections.Generic;

namespace MarsKod.Dunya
{
    public enum Collectible { Ice, EnergyCell, PanelPart, CompassPart, CableReel }

    public static class Collectibles
    {
        /// <summary>Bölüm haritasındaki işaretler (panel parçası burada değil: haritada gücü 0 olan panel, "0" yazılır; Level.cs)</summary>
        public static readonly Dictionary<char, Collectible> Marks = new Dictionary<char, Collectible>
        {
            ['B'] = Collectible.Ice,
            ['E'] = Collectible.EnergyCell,
            ['P'] = Collectible.CompassPart,
            ['M'] = Collectible.CableReel,
        };

        static readonly Dictionary<Collectible, (string one, string many)> Names = new Dictionary<Collectible, (string, string)>
        {
            [Collectible.Ice] = ("buz", "buzlar"),
            [Collectible.EnergyCell] = ("enerji hücresi", "enerji hücreleri"),
            [Collectible.PanelPart] = ("panel parçası", "panel parçaları"),
            [Collectible.CompassPart] = ("pusula parçası", "pusula parçaları"),
            [Collectible.CableReel] = ("kablo makarası", "kablo makaraları"),
        };

        /// <summary>Tekil Türkçe ad ("3 enerji hücresi daha")</summary>
        public static string Name(Collectible c) => Names[c].one;

        /// <summary>Çoğul Türkçe ad ("enerji hücreleri bitmedi")</summary>
        public static string PluralName(Collectible c) => Names[c].many;
    }
}
