using MarsKod.Dunya;
using static MotorTest.CodeBufferTests;

namespace MotorTest;

// Acemi paleti: surukle-birakta girinti secenekleri ve yeni parcalar (tasarim belgesi §4).
public class PaletteTests
{
    // ---- girinti ----

    [Fact]
    public void Iki_nokta_sonrasi_varsayilan_iceri_ama_sola_kaydirilabilir()
    {
        // Oyuncu hatali girintiyi de secebilir: Python hatasi ve aciklamasi ogretir (karar 09-29)
        var b = Buf("|for i in range(3):\nmove(East)");
        Assert.Equal(1, b.DefaultIndentLevel(1));
        Assert.Equal(0, b.DropIndentLevel(1, -1));
        Assert.Equal(1, b.DropIndentLevel(1, +2)); // ustteki satirin bir kademe icerisinden fazla olmaz
    }

    [Fact]
    public void Blok_icinde_kalir_sola_kayinca_disina_cikar_saga_bir_kademe_gider()
    {
        var b = Buf("|for i in range(3):\n    move(East)");
        Assert.Equal(1, b.DropIndentLevel(2, 0));  // varsayilan: ustteki satirla ayni
        Assert.Equal(0, b.DropIndentLevel(2, -1)); // bir kademe sola: dongunun disi
        Assert.Equal(0, b.DropIndentLevel(2, -5));
        Assert.Equal(2, b.DropIndentLevel(2, +1)); // hatali ama secilebilir (unexpected indent)
        Assert.Equal(2, b.DropIndentLevel(2, +4));
    }

    [Fact]
    public void En_ustte_ve_bos_kodda_girinti_yok()
    {
        Assert.Equal(0, Buf("|move(East)").DropIndentLevel(0, 3));
        Assert.Equal(0, Buf("|").DropIndentLevel(1, 3));
    }

    [Fact]
    public void Tasinan_satir_kendi_girintisini_belirlemez()
    {
        // for satirini asagi tasirken, hemen ustundeki "kendisi" hesaba katilmaz
        var b = Buf("|move(East)\nfor i in range(3):\ncollect()");
        Assert.Equal(0, b.DefaultIndentLevel(2, movingLine: 1));
        Assert.Equal(1, b.DefaultIndentLevel(2));
    }

    [Fact]
    public void Surukleyerek_dongu_kurulur()
    {
        // Bolum 3'u yalnizca paletle cozmek: for birak, altina iki satir birak, sayiyi 5'e cikar
        var b = Buf("|");
        b.DropLine(0, b.DropIndentLevel(0, 0), "for i in range(3):");
        b.DropLine(1, b.DropIndentLevel(1, 0), "move(East)");
        b.DropLine(2, b.DropIndentLevel(2, 0), "collect()");
        b.ChangeNumber(0, 15, +2);
        Assert.Equal("for i in range(5):\n    move(East)\n    collect()", b.Text);
        Assert.Equal("DDD", b.SaveKinds());
    }

    [Fact]
    public void Tasinan_satir_sola_kaydirilinca_dongu_disina_cikar()
    {
        var b = Buf("|for i in range(3):\n    collect()\n    move(East)");
        int level = b.DropIndentLevel(3, -1, movingLine: 1);
        b.MoveLine(1, 3, level);
        Assert.Equal("for i in range(3):\n    move(East)\ncollect()", b.Text);
    }

    // ---- bloklar (blok cizgisi) ----

    static List<string> Lines(string code) => code.Split('\n').ToList();

    [Fact]
    public void Blok_govdesi_ic_ice_bloklar_ve_aradaki_bos_satir()
    {
        var spans = CodeBlocks.Find(Lines("for i in range(2):\n    move(East)\n\n    for j in range(3):\n        collect()\nmove(North)\n"));
        Assert.Equal(2, spans.Count);
        Assert.Equal((0, 4, 0), (spans[0].Header, spans[0].Last, spans[0].Level));
        Assert.Equal((3, 4, 1), (spans[1].Header, spans[1].Last, spans[1].Level));
    }

    [Fact]
    public void Govdesiz_baslik_ve_yorum_blok_degil()
    {
        Assert.Empty(CodeBlocks.Find(Lines("for i in range(3):\nmove(East)")));
        Assert.Empty(CodeBlocks.Find(Lines("# not:\n    x = 1")));
    }

    [Fact]
    public void Birakilan_satirin_girdigi_blok()
    {
        var lines = Lines("for i in range(3):\n    move(East)\nmove(North)");
        Assert.Equal(0, CodeBlocks.Owner(lines, 1, 1));   // for'un hemen alti, iceride
        Assert.Equal(0, CodeBlocks.Owner(lines, 2, 1));   // gövdenin sonu, iceride
        Assert.Equal(-1, CodeBlocks.Owner(lines, 2, 0));  // disarida
        Assert.Equal(-1, CodeBlocks.Owner(lines, 3, 1));  // move(North)'tan sonra iceride: hicbir blok (hata)
        Assert.Equal(-1, CodeBlocks.Owner(lines, 2, 2));  // fazla iceride: hicbir blok (hata)
        Assert.Equal(0, CodeBlocks.Owner(lines, 2, 1, skip: 1)); // tasinan satir atlanir
    }

    // ---- yeni parcalar ----

    static string Mini(string number, string pieces) => @"{
  ""numara"": " + number + @", ""baslik"": ""x"", ""gorev"": ""y"",
  ""harita"": [""R H . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . ."", "". . . . . .""],
  ""komutlar"": [""move"", ""collect""], ""parcalar"": " + pieces + @",
  ""python_kelimeleri"": [""for"", ""in"", ""range""],
  ""konular"": [""a""], ""ipuclari"": [""i""],
  ""cozum"": [""move(East)""]
}";

    static readonly List<Level> Levels = new()
    {
        Level.Parse(Mini("1", "[\"move(East)\", \"move(West)\"]")),
        Level.Parse(Mini("2", "[\"move(East)\", \"collect()\"]")),
        Level.Parse(Mini("3", "[\"move(East)\", \"collect()\", \"for i in range(3):\"]")),
    };

    [Fact]
    public void Yalnizca_bu_bolumde_ilk_kez_gorulen_parca_yeni()
    {
        Assert.Equal(new[] { "collect()" }, Palette.NewPieces(Levels, 2));
        Assert.Equal(new[] { "for i in range(3):" }, Palette.NewPieces(Levels, 3));
    }

    [Fact]
    public void Ilk_bolumde_ve_bilinmeyen_bolumde_yeni_isareti_yok()
    {
        Assert.Empty(Palette.NewPieces(Levels, 1));
        Assert.Empty(Palette.NewPieces(Levels, 9));
    }
}
