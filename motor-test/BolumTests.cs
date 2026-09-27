using MarsKod.Dunya;

namespace MotorTest;

// Bölüm dosyaları (oyun/Assets/Resources/Bolumler) ve bölüm denetleyici.
// Buraya yeni bölüm dosyası eklenince kendiliğinden denetlenir: doğru çözüm görevi bitirmeli, tipik hatalar bitirmemeli.
public class BolumTests
{
    static readonly string Dir = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Bolumler");

    public static IEnumerable<object[]> Files() =>
        Directory.GetFiles(Dir, "*.json").Order(StringComparer.Ordinal).Select(f => new object[] { Path.GetFileName(f) });

    static Level Load(string file) => Level.Parse(File.ReadAllText(Path.Combine(Dir, file)));

    [Theory]
    [MemberData(nameof(Files))]
    public void Bolum_denetimden_gecer(string file)
    {
        var level = Load(file);
        Assert.Empty(LevelCheck.Problems(level));
        // dosya adı numarayla uyumlu: bolum-03.json -> 3
        Assert.Equal("bolum-" + level.Number.ToString("00") + ".json", file);
        // sahne (Oyun.cs) şimdilik yalnızca 6x6 alan çiziyor
        Assert.Equal((6, 6), (level.Cols, level.Rows));
    }

    [Theory]
    [MemberData(nameof(Files))]
    public void Tipik_hatalar_gercekten_durur_ya_da_eksik_kalir(string file)
    {
        var level = Load(file);
        Assert.NotEmpty(level.Mistakes);
        foreach (var m in level.Mistakes)
        {
            var report = ProgramRun.Execute(m.Code, level.CreateWorld());
            Assert.False(report.Complete, m.Explanation);
        }
    }

    [Fact]
    public void Bolumler_1_den_baslayip_bosluksuz_ilerler()
    {
        var numbers = Files().Select(f => Load((string)f[0]).Number).ToList();
        Assert.Equal(Enumerable.Range(1, numbers.Count), numbers);
    }

    [Fact]
    public void Ilk_3_bolumun_komutlari_karara_uygun()
    {
        // Karar (Ragıp, 09-27): 1. yalnızca move; 2. move + collect; 3. for ihtiyaç olarak doğar
        Assert.Equal(new[] { "move" }, Load("bolum-01.json").Commands);
        Assert.Equal(new[] { "move", "collect" }, Load("bolum-02.json").Commands);
        var third = Load("bolum-03.json");
        Assert.Contains("for", third.Solution);
        Assert.DoesNotContain("for", Load("bolum-02.json").Solution);
    }

    // ---- dosya biçimi ----

    const string Mini = """
        {
          "numara": 7,
          "baslik": "Deneme",
          "gorev": "Git",
          "harita": ["K . H", "R . B"],
          "komutlar": ["move", "collect"],
          "konular": [],
          "ipuclari": ["bir"],
          "cozum": ["move(East)", "move(East)", "collect()", "move(North)"]
        }
        """;

    [Fact]
    public void Harita_resim_gibi_okunur_en_ust_satir_kuzey()
    {
        var level = Level.Parse(Mini);
        Assert.Equal((3, 2), (level.Cols, level.Rows));
        Assert.Equal(new Cell(0, 0), level.Robot);
        Assert.Equal(new[] { new Cell(2, 0) }, level.Ices);
        Assert.Equal(new[] { new Cell(0, 1) }, level.Rocks);
        Assert.Equal(new Cell(2, 1), level.Target);
        Assert.Equal("move(East)\nmove(East)\ncollect()\nmove(North)\n", level.Solution);
        Assert.Equal("", level.StartCode);
        Assert.Empty(level.Mistakes);
        Assert.Empty(LevelCheck.Problems(level));
    }

    [Theory]
    [InlineData("\"harita\": [\"K . H\", \"R . B\"]", "\"harita\": [\"K . H\", \"R B\"]", "2. satırında 2 kare var, ilk satırda 3")]
    [InlineData("\"harita\": [\"K . H\", \"R . B\"]", "\"harita\": [\"K . R\", \"R . B\"]", "birden fazla robot")]
    [InlineData("\"harita\": [\"K . H\", \"R . B\"]", "\"harita\": [\"K . H\", \"R . X\"]", "bilinmeyen işaret: 'X'")]
    [InlineData("\"harita\": [\"K . H\", \"R . B\"]", "\"harita\": [\"K . .\", \"R . .\"]", "görev yok")]
    [InlineData("\"harita\": [\"K . H\", \"R . B\"]", "\"harita\": [\"K . H\", \". . B\"]", "robot (R) yok")]
    [InlineData("\"numara\": 7,", "", "\"numara\" alanı eksik")]
    [InlineData("\"numara\": 7", "\"numara\": \"7\"", "tam sayı olmalı")]
    [InlineData("\"komutlar\": [\"move\", \"collect\"]", "\"komutlar\": [\"move\", \"jump\"]", "bilinmeyen komut: \"jump\"")]
    [InlineData("\"komutlar\": [\"move\", \"collect\"]", "\"komutlar\": [\"move\"]", "\"collect\" yok")]
    [InlineData("\"ipuclari\": [\"bir\"]", "\"ipuclari\": []", "en az bir ipucu")]
    [InlineData("\"gorev\": \"Git\"", "\"gorev\": \"Git\", \"zorluk\": 2", "Bilinmeyen alan: \"zorluk\"")]
    public void Yanlis_dosya_anlasilir_mesaj_verir(string from, string to, string expected)
    {
        var e = Assert.Throws<LevelFormatError>(() => Level.Parse(Mini.Replace(from, to)));
        Assert.Contains(expected, e.Message);
    }

    [Theory]
    [InlineData("\"baslik\": \"Deneme\",", "\"baslik\": \"Deneme\"", "4. satır", "virgül eksik")]
    [InlineData("\"ipuclari\": [\"bir\"],", "\"ipuclari\": [\"bir\",],", "8. satır", "fazladan virgül")]
    [InlineData("\"gorev\": \"Git\",", "\"gorev\": 'Git',", "4. satır", "çift tırnak")]
    [InlineData("\"gorev\": \"Git\",", "\"gorev\": \"Git,", "4. satır", "Tırnak kapanmamış")]
    public void JSON_yazim_yanlisi_satir_numarasiyla_gelir(string from, string to, string line, string expected)
    {
        var e = Assert.Throws<LevelFormatError>(() => Level.Parse(Mini.Replace(from, to)));
        Assert.Contains(line, e.Message);
        Assert.Contains(expected, e.Message);
    }

    [Fact]
    public void Json_turleri_ve_kacis_isaretleri()
    {
        var d = (Dictionary<string, object>)Json.Parse("﻿{\"a\": [1, -2.5, true, false, null], \"b\": \"x\\\"y\\n\\u00e7\", \"c\": {}}");
        Assert.Equal(new object[] { 1.0, -2.5, true, false, null }, (List<object>)d["a"]);
        Assert.Equal("x\"y\nç", d["b"]);
        Assert.Empty((Dictionary<string, object>)d["c"]);
    }

    // ---- denetleyici yanlışı yakalar ----

    [Fact]
    public void Denetleyici_bitmeyen_cozumu_yakalar()
    {
        var level = Level.Parse(Mini.Replace(", \"move(North)\"]", "]"));
        Assert.Contains("görevi bitirmedi", Assert.Single(LevelCheck.Problems(level)));
    }

    [Fact]
    public void Denetleyici_duran_cozumu_yakalar()
    {
        var level = Level.Parse(Mini.Replace("[\"move(East)\", \"move(East)\"", "[\"move(North)\", \"move(East)\""));
        Assert.Contains("1. satırda durdu: Önünde kaya var", Assert.Single(LevelCheck.Problems(level)));
    }

    [Fact]
    public void Denetleyici_gorevi_bitiren_tipik_hatayi_ve_baslangic_kodunu_yakalar()
    {
        var level = Level.Parse(Mini.Replace("\"cozum\"",
            "\"baslangic_kodu\": \"move(East)\\nmove(East)\\ncollect()\\nmove(North)\", \"tipik_hatalar\": [{\"kod\": [\"move(East)\", \"move(East)\", \"collect()\", \"move(North)\"], \"aciklama\": \"x\"}], \"cozum\""));
        var problems = LevelCheck.Problems(level);
        Assert.Equal(2, problems.Count);
        Assert.Contains("Başlangıç kodu", problems[0]);
        Assert.Contains("1. tipik hata", problems[1]);
    }

    // ---- kaya, hedef, kilitli komut ----

    [Fact]
    public void Kaya_gecit_vermez_oyun_kuralidir()
    {
        var world = Level.Parse(Mini).CreateWorld();
        var report = ProgramRun.Execute("move(North)\n", world);
        Assert.Equal("Önünde kaya var", report.Rule.Title);
        Assert.Null(report.Error);
        Assert.True(Assert.IsType<Blocked>(Assert.Single(report.Trace[0].Events)).ByRock);
        Assert.Equal(new Cell(0, 0), world.Robot);
    }

    [Fact]
    public void Hedef_varsa_robot_kod_bitince_hedefte_olmali()
    {
        var level = Level.Parse(Mini);
        // buz toplandı ama robot hedefe çıkmadı
        var world = level.CreateWorld();
        Assert.False(ProgramRun.Execute("move(East)\nmove(East)\ncollect()\n", world).Complete);
        Assert.Equal((0, false), (world.IceLeft, world.OnTarget));
        // hedefe gitti ama buzu toplamadı
        world = level.CreateWorld();
        Assert.False(ProgramRun.Execute("move(East)\nmove(East)\nmove(North)\n", world).Complete);
        Assert.Equal((1, true), (world.IceLeft, world.OnTarget));
        // hedeften geçip ayrılmak yetmez
        var target = Level.Parse(Mini.Replace("\"R . B\"", "\"R . .\""));
        Assert.False(ProgramRun.Execute("move(East)\nmove(East)\nmove(North)\nmove(South)\n", target.CreateWorld()).Complete);
    }

    [Fact]
    public void Gorevi_bitirip_sonra_duran_kod_tamam_sayilmaz()
    {
        var world = Load("bolum-03.json").CreateWorld();
        var report = ProgramRun.Execute("for i in range(6):\n    move(East)\n    collect()\n", world);
        Assert.Equal(0, world.IceLeft);
        Assert.Equal("Alanın sınırı", report.Rule.Title);
        Assert.False(report.Complete);
    }

    [Fact]
    public void Acilmamis_komut_oyun_kuralidir()
    {
        var level = Load("bolum-01.json");
        var report = ProgramRun.Execute("move(East)\ncollect()\n", level.CreateWorld());
        Assert.Equal("Bu komut henüz açılmadı", report.Rule.Title);
        Assert.Contains("move()", report.Rule.Text);
        Assert.Equal(2, report.StopLine);
    }
}
