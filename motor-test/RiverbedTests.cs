using MarsKod.Dunya;

namespace MotorTest;

// Bölge 7 (Bölüm 61-70, nehir yatağı): bölüm dosyasındaki çizim işaretleri ve alanları, bölge ve toplanan nesne.
public class RiverbedTests
{
    // Robot (0,1); boyanacak kareler (1,0) ve (2,0). Çözüm: aşağı, sağa, kalemi indir (kareyi boyar), sağa (kareyi boyar).
    const string Mini = """
        {
          "numara": 7,
          "baslik": "Deneme",
          "gorev": "Çiz",
          "harita": ["R . .", ". O O"],
          "komutlar": ["move", "pen_down", "pen_up"],
          "konular": [],
          "ipuclari": ["bir"],
          "cozum": ["move(South)", "move(East)", "pen_down()", "move(East)"]
        }
        """;

    [Fact]
    public void O_haritada_boyanacak_kare_olarak_okunur()
    {
        var level = Level.Parse(Mini);
        Assert.Equal(new[] { new Cell(1, 0), new Cell(2, 0) }, level.Paint);
        var world = level.CreateWorld();
        Assert.True(world.Paint.WantedAt(new Cell(1, 0)));
        Assert.False(world.Paint.WantedAt(new Cell(0, 0)));
        Assert.Empty(LevelCheck.Problems(level));
    }

    [Fact]
    public void Yalnizca_boyanacak_kare_olan_harita_gorev_sayilir()
    {
        // toplanacak, panel ve hedef yok; çizim tek başına görevdir (hata vermemeli)
        Assert.Empty(Level.Parse(Mini).Ices);
        Assert.Null(Level.Parse(Mini).Target);
    }

    [Fact]
    public void Cizim_olan_bolumde_kalem_komutlari_acik_olmali()
    {
        var json = Mini.Replace("[\"move\", \"pen_down\", \"pen_up\"]", "[\"move\", \"pen_down\"]");
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(json));
        Assert.Contains("pen_down", e.Message);
        Assert.Contains("pen_up", e.Message);
    }

    [Fact]
    public void Cizimsiz_bolumde_kalem_komutu_acilamaz()
    {
        var json = Mini.Replace("\"R . .\", \". O O\"", "\"R . H\", \". . .\"");
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(json));
        Assert.Contains("boyanacak kare", e.Message);
    }

    [Fact]
    public void Q_mavi_kalemdir_ve_toplanir()
    {
        var json = Mini.Replace("\"R . .\", \". O O\"", "\"R . Q\", \". . .\"")
            .Replace("[\"move\", \"pen_down\", \"pen_up\"]", "[\"move\", \"collect\"]")
            .Replace("[\"move(South)\", \"move(East)\", \"pen_down()\", \"move(East)\"]", "[\"move(East)\", \"move(East)\", \"collect()\"]");
        var level = Level.Parse(json);
        Assert.Equal(Collectible.BluePencil, Collectibles.Marks['Q']);
        Assert.Equal(Collectible.BluePencil, level.Item);
        Assert.Equal("mavi boya kalemi", Collectibles.Name(Collectible.BluePencil));
        Assert.Equal("mavi boya kalemleri", Collectibles.PluralName(Collectible.BluePencil));
        Assert.Empty(LevelCheck.Problems(level));
    }

    [Fact]
    public void Parametreli_rutinler_okunur_ve_adi_denetlenir()
    {
        var level = Level.Parse(Mini.Replace("\"konular\": [],", "\"konular\": [], \"parametreli_rutinler\": [\"ok\", \"cizgi\"],"));
        Assert.Equal(new[] { "ok", "cizgi" }, level.Routines.ParametricNames);
        Assert.False(level.Routines.IsEmpty);
        foreach (var bad in new[] { "def", "move", "2tur", "iki kelime" })
        {
            var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace("\"konular\": [],", "\"konular\": [], \"parametreli_rutinler\": [\"" + bad + "\"],")));
            Assert.Contains("parametreli_rutinler", e.Message);
        }
    }

    [Fact]
    public void Parametreli_rutinler_bos_liste_olamaz()
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace("\"konular\": [],", "\"konular\": [], \"parametreli_rutinler\": [],")));
        Assert.Contains("boş olamaz", e.Message);
    }

    [Fact]
    public void Parametre_sarti_bolum_denetimine_girer()
    {
        // ok rutini yazılmazsa doğru çözüm bile görevi bitirmez: denetleyici bunu yakalar
        var level = Level.Parse(Mini.Replace("\"konular\": [],", "\"konular\": [], \"parametreli_rutinler\": [\"ok\"],"));
        Assert.Contains(LevelCheck.Problems(level), p => p.Contains("görevi bitirmedi"));
    }

    [Fact]
    public void Bolge_7_nehir_yatagidir_ve_mavi_kalem_toplanir()
    {
        Assert.Equal(Region.Canyon, Regions.Of(60));
        Assert.Equal(Region.Riverbed, Regions.Of(61));
        Assert.Equal(Region.Riverbed, Regions.Of(70));
        Assert.Equal(Collectible.BluePencil, Regions.Item(Region.Riverbed));
    }

    // ---- Bölüm 61-70 (dosyalar yazıldıkça denetlenir) ----

    static readonly string Dir = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Bolumler");

    static Level Load(int n) => Level.Parse(File.ReadAllText(Path.Combine(Dir, "bolum-" + n + ".json")));

    public static IEnumerable<object[]> Numbers() =>
        Enumerable.Range(61, 10).Where(n => File.Exists(Path.Combine(Dir, "bolum-" + n + ".json"))).Select(n => new object[] { n });

    // Perde 2 biçimi: etikette bölge adı, giriş ve bitiş kayıt damgası, büyük başlık telefonda tek satır
    [Theory]
    [MemberData(nameof(Numbers))]
    public void Bolge_7_bolumu_perde_2_bicimindedir(int n)
    {
        var level = Load(n);
        Assert.Equal("BÖLGE 7 · NEHİR YATAĞI", level.Label);
        Assert.False(string.IsNullOrEmpty(level.Intro), n + ": giris yok");
        Assert.False(string.IsNullOrEmpty(level.Outro), n + ": bitis yok");
        Assert.True(level.Goal.Length <= 22, n + ": gorev çok uzun (" + level.Goal.Length + " harf)");
    }

    // Bölge 7'nin konusu parametre: her bölüm parametreli bir rutin ister
    [Theory]
    [MemberData(nameof(Numbers))]
    public void Bolge_7_bolumu_parametreli_rutin_ister(int n)
    {
        Assert.NotEmpty(Load(n).Routines.ParametricNames);
    }

    // Her bölümde parametresiz çözüm (rutinsiz kopyala-yapıştır, parametresiz tanım ya da hep aynı sayı) bulunmalı ve görevi
    // tam da rutin şartında bitirmemeli (kod durmadan çalışıp "Kod bitti, ... rutini/sayı" mesajı vermeli)
    [Theory]
    [MemberData(nameof(Numbers))]
    public void Bolge_7_bolumunde_parametre_sartina_takilan_hata_ornegi_var(int n)
    {
        var level = Load(n);
        Assert.Contains(level.Mistakes, m => level.Run(m.Code).RoutineProblem != null);
    }

    [Theory]
    [MemberData(nameof(Numbers))]
    public void Bolge_7_bolumu_cizimliyse_kalem_acik_degilse_kapali(int n)
    {
        var level = Load(n);
        Assert.Equal(level.Paint.Count > 0, level.Commands.Contains("pen_down"));
        Assert.Equal(level.Paint.Count > 0, level.Commands.Contains("pen_up"));
    }

    [Theory]
    [MemberData(nameof(Numbers))]
    public void Bolge_7_cozumu_en_az_iki_farkli_degerle_cagirir(int n)
    {
        var level = Load(n);
        var report = level.Run(level.Solution);
        Assert.True(report.Complete, n + ": doğru çözüm görevi bitirmiyor");
        foreach (var name in level.Routines.ParametricNames)
            Assert.True(report.RoutineArgs[name].Distinct().Count() >= 2, n + ": " + name + " en az iki farklı değerle çağrılmıyor");
    }
}
