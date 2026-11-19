using MarsKod.Dunya;

namespace MotorTest;

// Değişen alanda iki düzen arasında içeriği değişen kareler (LayoutDiff): geçişten sonra parlayacak kareler.
public class LayoutDiffTests
{
    [Fact]
    public void Bolum_15_te_buz_ile_kristalin_yer_degistirdigi_dort_kare_degisir()
    {
        var level = DegisenAlanTests.Load(15);
        // Düzen 1 (yukarıdan aşağı): kristal, buz, buz, kristal, buz; düzen 2: buz, buz, kristal, buz, kristal. 4. satır ikisinde de buz.
        var changed = LayoutDiff.ChangedCells(level, 0, 1);
        Assert.Equal(new[] { new Cell(0, 1), new Cell(0, 2), new Cell(0, 3), new Cell(0, 5) }, changed);
    }

    [Fact]
    public void Ayni_duzende_degisen_kare_yok()
    {
        var level = DegisenAlanTests.Load(15);
        Assert.Empty(LayoutDiff.ChangedCells(level, 1, 1));
    }

    [Fact]
    public void Iki_yonde_de_ayni_kareler()
    {
        var level = DegisenAlanTests.Load(20);
        Assert.Equal(LayoutDiff.ChangedCells(level, 0, 2), LayoutDiff.ChangedCells(level, 2, 0));
    }

    [Fact]
    public void Bolum_91_de_yalniz_yeri_degisen_kraterler_sayilir()
    {
        var level = DegisenAlanTests.Load(91);
        // İki düzende ortak kraterler: (5,4), (5,3), (4,2); farklı olanlar: düzen 1'de (3,1), (3,0); düzen 2'de (4,1), (1,0)
        var changed = LayoutDiff.ChangedCells(level, 0, 1);
        Assert.Equal(new[] { new Cell(1, 0), new Cell(3, 0), new Cell(3, 1), new Cell(4, 1) }, changed);
    }
}
