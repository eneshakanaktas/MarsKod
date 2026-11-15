using System.Numerics;
using MarsKod.Dunya;
using MarsKod.Motor;

namespace MotorTest;

// Tuz gölü (Bölge 9, Bölüm 81-90, listeler + iç içe döngü): tuz kristali S, boyunu crystal_size() ölçer ("kristal_boyu");
// notlar liste de olabilir (["KK", "D"]: yol parçaları) ya da yazısı ve yolu ayrı yazılır ({"yazi", "yol", "rapor"}).
public class TuzGoluTests
{
    const string Yon = "def yon(harf):\n    if harf == \"K\":\n        return North\n    elif harf == \"G\":\n        return South\n    elif harf == \"D\":\n        return East\n    elif harf == \"B\":\n        return West\n";

    // Robot (0,0); doğuda (1,0) 5 cm, (2,0) 9 cm kristal, (3,0) kırmızı kristal
    static Level Row(string extra = "", string commands = "\"move\", \"collect\", \"crystal_size\"") => Level.Parse(
        "{\"numara\": 82, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \"R S S T . .\"],"
        + " \"kristal_boyu\": [5, 9], \"komutlar\": [" + commands + "]" + extra + ", \"konular\": [], \"ipuclari\": [\"x\"], \"cozum\": [\"move(East)\"]}");

    static Level Notes(string notes, string commands = "\"move\", \"read_note\"", string extra = "") => Level.Parse(
        "{\"numara\": 84, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". K . . . .\", \"R . . . . .\"],"
        + " \"notlar\": " + notes + ", \"komutlar\": [" + commands + "]" + extra + ", \"konular\": [], \"ipuclari\": [\"x\"], \"cozum\": [\"move(East)\"]}");

    [Fact]
    public void Tuz_kristali_S_ile_yazilir_bolge_9_tuz_golu()
    {
        Assert.Equal(Collectible.SaltCrystal, Collectibles.Marks['S']);
        Assert.Equal("tuz kristali", Collectibles.Name(Collectible.SaltCrystal));
        Assert.Equal(Region.SaltLake, Regions.Of(81));
        Assert.Equal(Region.SaltLake, Regions.Of(90));
        Assert.Equal(Collectible.SaltCrystal, Regions.Item(Region.SaltLake));
        var level = Row();
        Assert.Equal(Collectible.SaltCrystal, level.Item);
        Assert.Equal(9, level.CrystalSizes[new Cell(2, 0)]);
    }

    [Fact]
    public void Kristal_boyu_olculur_toplaninca_ve_kirmizida_sifir()
    {
        var world = Row().CreateWorld();
        world.Move(Direction.East);
        Assert.Equal(5, world.CrystalSize());
        world.Collect();
        Assert.Equal(0, world.CrystalSize());
        world.Move(Direction.East);
        world.Move(Direction.East);
        Assert.Equal(0, world.CrystalSize());   // kırmızı kristal tuz kristali değil
        Assert.Equal(new[] { 5, 0, 0 }, world.Events.OfType<CrystalMeasured>().Select(e => e.Size));
    }

    [Fact]
    public void Kristal_boyu_listesi_kristal_sayisina_uymali()
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(
            "{\"numara\": 82, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\"R S S\"], \"kristal_boyu\": [5],"
            + " \"komutlar\": [\"move\", \"collect\"], \"konular\": [], \"ipuclari\": [\"x\"], \"cozum\": [\"move(East)\"]}"));
        Assert.Contains("2 tuz kristali (S)", e.Message);
        Assert.Contains("\"kristal_boyu\"", e.Message);
    }

    [Fact]
    public void Olcup_listeye_ekleyip_en_buyugunu_bildirmek()
    {
        var level = Row(", \"rapor\": 9", "\"move\", \"collect\", \"crystal_size\", \"report\"");
        var code = "boylar = []\nfor i in range(2):\n    move(East)\n    boylar.append(crystal_size())\n    collect()\nreport(max(boylar))\n";
        Assert.True(level.Run(code).Complete);
        // önce toplanırsa boylar [0, 0] olur
        var late = "boylar = []\nfor i in range(2):\n    move(East)\n    collect()\n    boylar.append(crystal_size())\nreport(max(boylar))\n";
        Assert.Equal("Anten bu sayıyı beklemiyordu", level.Run(late).Rule.Title);
    }

    [Fact]
    public void Liste_notu_python_listesi_verir_yolu_parcalarin_harfleri()
    {
        var level = Notes("[[\"KK\", \"D\"], [\"DD\", \"KKK\"]]");
        Assert.Equal("['KK', 'D']", level.Notes[0].Text);
        Assert.Equal(new Cell(1, 2), level.Notes[0].Route.End);
        Assert.Equal(new Cell(2, 3), level.Notes[1].Route.End);
        var nested = Yon + "for parca in read_note():\n    for harf in parca:\n        move(yon(harf))\n";
        Assert.All(level.RunVersions(nested), r => Assert.True(r.Report.Complete));
        // parçayı harf sanmak: yon("KK") None döndürür
        var flat = Yon + "for harf in read_note():\n    move(yon(harf))\n";
        Assert.Equal("Yön yerine None geldi", level.Run(flat).Rule.Title);
    }

    [Fact]
    public void Liste_notu_her_okuyusta_yeni_liste()
    {
        var level = Notes("[[\"KK\", \"D\"]]");
        var code = "a = read_note()\na.append(\"G\")\nb = read_note()\nprint(len(b))\nfor parca in b:\n    for harf in parca:\n        if harf == \"K\":\n            move(North)\n        else:\n            move(East)\n";
        var report = level.Run(code);
        Assert.True(report.Complete);
        Assert.Equal("2\n", report.Output);   // ikinci okuyuş eklenen "G"yi görmez
    }

    [Fact]
    public void Nesne_notu_yazisi_ve_yolu_ayri()
    {
        // 84: ilk ok (oklar[0]) yön, okların sayısı adım sayısı
        var level = Notes("[{\"yazi\": [\"D\", \"K\", \"K\"], \"yol\": \"DDD\"}, {\"yazi\": [\"K\", \"B\", \"D\", \"G\"], \"yol\": \"KKKK\"}]",
            extra: ", \"rutinler\": [\"yon\"], \"donus_sarti\": true");
        Assert.Equal(new Cell(3, 0), level.Notes[0].Route.End);
        Assert.Equal(new Cell(0, 4), level.Notes[1].Route.End);
        var code = Yon + "oklar = read_note()\nfor i in range(len(oklar)):\n    move(yon(oklar[0]))\n";
        Assert.All(level.RunVersions(code), r => Assert.True(r.Report.Complete));
        var second = Yon + "oklar = read_note()\nfor i in range(len(oklar)):\n    move(yon(oklar[1]))\n";
        Assert.False(level.Run(second).Complete);
    }

    [Fact]
    public void Notta_rapor_her_notta_antenin_bekledigi_sayi()
    {
        var level = Notes("[{\"yazi\": [\"T.T\", \".T.\"], \"yol\": \"\", \"rapor\": 3}, {\"yazi\": [\"TT.\", \"...\"], \"yol\": \"\", \"rapor\": 2}]",
            "\"read_note\", \"report\"");
        Assert.Equal(new BigInteger(3), level.CreateWorld(0).ExpectedReport);
        Assert.Equal(new BigInteger(2), level.CreateWorld(1).ExpectedReport);
        var count = "sayac = 0\nfor satir in read_note():\n    for kare in satir:\n        if kare == \"T\":\n            sayac += 1\nreport(sayac)\n";
        Assert.All(level.RunVersions(count), r => Assert.True(r.Report.Complete));
        Assert.False(level.Run("report(3)\n").Complete);   // ezber: ikinci notta yanlış
    }

    [Theory]
    [InlineData("[[\"KK\", \"\"]]", "", "boş olmayan metinlerden")]
    [InlineData("[[\"KX\"]]", "", "bilinmeyen harf içeriyor: \"X\"")]
    [InlineData("[{\"yazi\": \"T\"}]", "", "\"yol\"")]
    [InlineData("[{\"yazi\": \"T\", \"yol\": \"\", \"renk\": 1}]", "", "bilinmeyen alan: \"renk\"")]
    [InlineData("[{\"yazi\": \"A\", \"yol\": \"\", \"rapor\": 1}, {\"yazi\": \"B\", \"yol\": \"\", \"rapor\": 1}]", ", \"report\"", "aynı yere çıkıyor")]
    [InlineData("[{\"yazi\": \"A\", \"yol\": \"\", \"rapor\": 1}, {\"yazi\": \"B\", \"yol\": \"D\"}]", ", \"report\"", "hepsi yazmalı")]
    [InlineData("[{\"yazi\": \"A\", \"yol\": \"\", \"rapor\": 1}]", "", "\"report\" yok")]
    [InlineData("[5]", "", "metin listesi")]
    public void Yanlis_not_anlasilir_mesaj_verir(string notes, string moreCommands, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Notes(notes, "\"move\", \"read_note\"" + moreCommands));
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Sondaki_virgul_ve_not_in_calisir()
    {
        var level = Row(", \"rapor\": 2", "\"move\", \"collect\", \"crystal_size\", \"report\"");
        var code = "yanimiza_al = [\"su\", \"ilac\", \"battaniye\", \"Ece'nin defteri\", ]\nfarkli = []\nfor i in range(2):\n    move(East)\n"
            + "    boy = crystal_size()\n    collect()\n    if boy not in farkli:\n        farkli.append(boy)\nreport(len(farkli))\n";
        Assert.True(level.Run(code).Complete);
    }
}
