// Bölümde toplanan şey: her bölge kendi nesnesini toplar (hikâye: docs/tasarim/hikaye.md).
// Oyun kuralı hepsi için aynıdır (collect() toplar); yalnızca haritadaki işareti, adı ve görünüşü değişir.
// Yeni tür eklemek: enum'a bir değer + aşağıdaki iki tabloya birer satır.

using System.Collections.Generic;

namespace MarsKod.Dunya
{
    public enum Collectible { Ice, EnergyCell, PanelPart, CompassPart, CableReel, DronePart, Pebble, DataCard, SaltCrystal, MeteorStone }

    public static class Collectibles
    {
        /// <summary>Bölüm haritasındaki işaretler (panel parçası burada değil: haritada gücü 0 olan panel, "0" yazılır; Level.cs)</summary>
        public static readonly Dictionary<char, Collectible> Marks = new Dictionary<char, Collectible>
        {
            ['B'] = Collectible.Ice,
            ['E'] = Collectible.EnergyCell,
            ['P'] = Collectible.CompassPart,
            ['M'] = Collectible.CableReel,
            ['D'] = Collectible.DronePart,
            ['C'] = Collectible.Pebble,
            ['V'] = Collectible.DataCard,
            ['S'] = Collectible.SaltCrystal,
            ['G'] = Collectible.MeteorStone,
        };

        static readonly Dictionary<Collectible, (string one, string many)> Names = new Dictionary<Collectible, (string, string)>
        {
            [Collectible.Ice] = ("buz", "buzlar"),
            [Collectible.EnergyCell] = ("enerji hücresi", "enerji hücreleri"),
            [Collectible.PanelPart] = ("panel parçası", "panel parçaları"),
            [Collectible.CompassPart] = ("pusula parçası", "pusula parçaları"),
            [Collectible.CableReel] = ("kablo makarası", "kablo makaraları"),
            [Collectible.DronePart] = ("drone parçası", "drone parçaları"),
            [Collectible.Pebble] = ("çakıl", "çakıllar"),
            [Collectible.DataCard] = ("veri kartı", "veri kartları"),
            [Collectible.SaltCrystal] = ("tuz kristali", "tuz kristalleri"),
            [Collectible.MeteorStone] = ("göktaşı", "göktaşları"),
        };

        /// <summary>Tekil Türkçe ad ("3 enerji hücresi daha")</summary>
        public static string Name(Collectible c) => Names[c].one;

        /// <summary>Çoğul Türkçe ad ("enerji hücreleri bitmedi")</summary>
        public static string PluralName(Collectible c) => Names[c].many;
    }
}
