using System.Text.RegularExpressions;
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

    // senaryo-bolge-07.md: Perde 2 biçimi sürer
    [Fact]
    public void Bolge_7_bolumleri_kanyon_dibi_etiketi_ve_damga_tasir()
    {
        for (int n = 61; n <= 70; n++)
        {
            var level = Load("bolum-" + n + ".json");
            Assert.Equal("BÖLGE 7 · KANYON DİBİ", level.Label);
            Assert.StartsWith("Sol 1.0", level.Intro);
            Assert.False(string.IsNullOrEmpty(level.Outro), n + ": bitis yok");
            Assert.True(level.Goal.Length <= 22, n + ": gorev çok uzun (" + level.Goal.Length + " harf)");
        }
        // 64'ün damgası haritadaki çakıl sayısını söyler
        var b64 = Load("bolum-64.json");
        Assert.Equal("Örnek çakıl: " + b64.Ices.Count, b64.Outro);
        // 62: Ece'nin oyundaki ilk "sesi" başlangıç kodunun ilk satırı
        Assert.StartsWith("# serçe yukarıdan bakar, sen yürü :) - E.", Load("bolum-62.json").StartCode.Replace("\r\n", "\n"));
    }

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

    // Perde 2 biçimi (senaryo-bolge-06.md): üst etikette bölge adı, "ÖDEV" yok; giriş ve bitiş kayıt damgası
    [Fact]
    public void Bolge_6_bolumleri_kanyon_etiketi_ve_damga_tasir()
    {
        for (int n = 51; n <= 60; n++)
        {
            var level = Load("bolum-" + n + ".json");
            Assert.Equal("BÖLGE 6 · KANYON GİRİŞİ", level.Label);
            Assert.False(string.IsNullOrEmpty(level.Intro), n + ": giris yok");
            Assert.False(string.IsNullOrEmpty(level.Outro), n + ": bitis yok");
            // büyük başlık telefonda tek satır: "Topla, bildir, git" sığıyor, 28 harf taşıyor
            Assert.True(level.Goal.Length <= 22, n + ": gorev çok uzun (" + level.Goal.Length + " harf)");
        }
    }

    [Theory]
    [MemberData(nameof(Files))]
    public void Tipik_hatalar_gercekten_durur_ya_da_eksik_kalir(string file)
    {
        var level = Load(file);
        Assert.NotEmpty(level.Mistakes);
        foreach (var m in level.Mistakes)
        {
            var report = level.Run(m.Code);
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

    [Fact]
    public void Bolge_1_enerji_hucresi_bolge_2_buz_toplar()
    {
        // Karar (Ragıp, 10-01; docs/tasarim/hikaye.md): Bölüm 1-10 iniş ovası (enerji hücresi), 11-20 kutup buzulu (buz), 21-30 kraterli düzlük (kırık panelin parçası)
        foreach (var f in Files())
        {
            var level = Load((string)f[0]);
            if (level.Ices.Count == 0) continue;
            var expected = Regions.Item(Regions.Of(level.Number));
            Assert.True(expected == level.Item, "Bölüm " + level.Number + ": " + level.Item);
        }
    }

    [Fact]
    public void Her_on_bolum_yeni_bolge()
    {
        Assert.Equal(Region.Plain, Regions.Of(1));
        Assert.Equal(Region.Plain, Regions.Of(10));
        Assert.Equal(Region.PolarIce, Regions.Of(11));
        Assert.Equal(Region.PolarIce, Regions.Of(20));
        Assert.Equal(Region.CraterField, Regions.Of(21));
        Assert.Equal(Region.CraterField, Regions.Of(30));
        Assert.Equal(Collectible.PanelPart, Regions.Item(Region.CraterField));
        Assert.Equal(Collectible.EnergyCell, Regions.Item(Region.Plain));
        Assert.Equal(Collectible.Ice, Regions.Item(Region.PolarIce));
        Assert.Equal(Region.AntennaHill, Regions.Of(50));
        Assert.Equal(Region.Canyon, Regions.Of(51));
        Assert.Equal(Region.Canyon, Regions.Of(60));
        Assert.Equal(Collectible.DronePart, Regions.Item(Region.Canyon));
        Assert.Equal(Collectible.DronePart, Collectibles.Marks['D']);
        Assert.Equal("drone parçası", Collectibles.Name(Collectible.DronePart));
        Assert.Equal(Region.RiverBed, Regions.Of(61));
        Assert.Equal(Region.RiverBed, Regions.Of(70));
        Assert.Equal(Collectible.Pebble, Regions.Item(Region.RiverBed));
        Assert.Equal(Collectible.Pebble, Collectibles.Marks['C']);
        Assert.Equal("çakıl", Collectibles.Name(Collectible.Pebble));
        Assert.Equal(Region.Station, Regions.Of(71));
        Assert.Equal(Region.Station, Regions.Of(80));
        Assert.Equal(Collectible.DataCard, Regions.Item(Region.Station));
        Assert.Equal(Collectible.DataCard, Collectibles.Marks['V']);
        Assert.Equal("veri kartı", Collectibles.Name(Collectible.DataCard));
    }

    // Spec tutarlılık 1: Bölge 8'den itibaren çözüm ve başlangıç kodu yalnızca o bölüme kadar açılmış Python kelimelerini kullanır
    [Theory]
    [MemberData(nameof(Files))]
    public void Cozum_yalnizca_acilmis_kelimeleri_kullanir(string file)
    {
        var level = Load(file);
        if (level.Number < 71) return;
        var all = Directory.GetFiles(Dir, "*.json").Select(f => Level.Parse(File.ReadAllText(f))).ToList();
        var open = new HashSet<string>(Suggestions.OpenWords(all, level.Number));
        foreach (var word in PythonWordsIn(level.Solution + "\n" + level.StartCode))
            Assert.True(open.Contains(word), level.Number + ": \"" + word + "\" henüz açılmadı (python_kelimeleri)");
    }

    // Metinleri ve yorumları çıkarıp kalan adlardan Python kelimesi olanlar
    static IEnumerable<string> PythonWordsIn(string code)
    {
        var bare = Regex.Replace(code, "\"[^\"\\n]*\"|'[^'\\n]*'|#.*", "");
        return Regex.Matches(bare, @"[A-Za-z_]\w*").Select(m => m.Value).Where(Suggestions.IsPythonWord).Distinct();
    }

    // Spec tutarlılık 3: acemi düğmeleri çözümü kurabilmeli. Her çözüm satırı ya başlangıç kodunda ya parcalar'da
    // (sayılar farklı olabilir: oyuncu sayıyı düğmede değiştirir; metin değiştirilemez)
    [Theory]
    [MemberData(nameof(Files))]
    public void Parcalar_cozumu_kurar(string file)
    {
        var level = Load(file);
        if (level.Number < 71) return;
        string Norm(string line) => Regex.Replace(line.Trim(), @"\d+", "#");
        var known = new HashSet<string>(level.StartCode.Split('\n').Concat(level.Pieces).Select(Norm));
        foreach (var line in level.Solution.Split('\n').Where(l => l.Trim().Length > 0))
            Assert.True(known.Contains(Norm(line)), level.Number + ": \"" + line.Trim() + "\" düğmelerle kurulamıyor");
    }

    // ---- dosya biçimi ----

    [Fact]
    public void Enerji_hucresi_E_ile_yazilir_ve_buz_gibi_toplanir()
    {
        var level = Level.Parse(Mini.Replace("\"R . B\"", "\"R . E\""));
        Assert.Equal(Collectible.EnergyCell, level.Item);
        Assert.Equal(new[] { new Cell(2, 0) }, level.Ices);
        Assert.Empty(LevelCheck.Problems(level));
        Assert.Equal(Collectible.Ice, Level.Parse(Mini).Item);
    }

    [Fact]
    public void Ice_here_enerji_hucresi_bolumunde_acilamaz()
    {
        var json = Mini.Replace("\"R . B\"", "\"R . E\"").Replace("[\"move\", \"collect\"]", "[\"move\", \"collect\", \"ice_here\"]");
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(json));
        Assert.Contains("yalnızca buz (B) olan bölümlerde", e.Message);
    }

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
    [InlineData("\"harita\": [\"K . H\", \"R . B\"]", "\"harita\": [\"K . E\", \"R . B\"]", "iki tür toplanacak")]
    public void Yanlis_dosya_anlasilir_mesaj_verir(string from, string to, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace(from, to)));
        Assert.Contains(expected, e.Message);
    }

    [Theory]
    [InlineData("\"baslik\": \"Deneme\",", "\"baslik\": \"Deneme\"", "4. satır", "virgül eksik")]
    [InlineData("\"ipuclari\": [\"bir\"],", "\"ipuclari\": [\"bir\",],", "8. satır", "fazladan virgül")]
    [InlineData("\"gorev\": \"Git\",", "\"gorev\": 'Git',", "4. satır", "çift tırnak")]
    [InlineData("\"gorev\": \"Git\",", "\"gorev\": \"Git,", "4. satır", "Tırnak kapanmamış")]
    public void JSON_yazim_yanlisi_satir_numarasiyla_gelir(string from, string to, string line, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace(from, to)));
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

    // Spec tutarlılık 4 ve 6: 77'nin 24 kişisi 78'in ilk 24 satırı; 78'in son iki satırı senaryodaki gibi, harfi harfine
    [Fact]
    public void Bolum_77_ve_78_listesi_tutarli()
    {
        var people = Load("bolum-77.json").Screen.Rows.Select(r => (string)r.Input).ToList();
        var all = Load("bolum-78.json").Screen.Rows.Select(r => (string)r.Input).ToList();
        Assert.Equal(24, people.Count);
        Assert.Equal(26, all.Count);
        Assert.Equal(people, all.Take(24));
        Assert.All(people, p => Assert.EndsWith(" — kapsül: hazır", p));
        Assert.Equal("BKM-7   — dışarıda — görev: KAPI", all[24]);
        Assert.Equal("KOR     — hizmet dışı — hangar", all[25]);
        var senaryo = File.ReadAllText(Path.Combine(Cases.Root, "docs", "tasarim", "senaryo-perde-2.md"));
        Assert.Contains(all[24], senaryo);
        Assert.Contains(all[25], senaryo);
    }

    // Spec kod çözme bölümleri (Bölge 8): B 72, 77, 78 · C 76, 80; C'de 3 not
    [Fact]
    public void Bolge_8_kod_cozme_bolumleri()
    {
        foreach (var n in new[] { 72, 77, 78 }) Assert.NotNull(Load("bolum-" + n + ".json").Screen);
        foreach (var n in new[] { 76, 80 }) Assert.Equal(3, Load("bolum-" + n + ".json").Notes.Count);
        Assert.Contains("read_note", Load("bolum-76.json").Commands);
        Assert.DoesNotContain("read_note", Load("bolum-75.json").Commands);   // yeni komut yeni Python konusuyla aynı bölümde açılmaz
    }

    // Ekran bulmacası bu bölümlerde ilk kez görünüyor (72) ya da yeni bir yük biniyor (78: uzun, kayan liste): bilgi kartı olmalı
    [Theory]
    [InlineData(72)]
    [InlineData(78)]
    public void Ekran_bulmacasinin_yeni_bolumleri_bilgi_karti_tasir(int number)
    {
        var info = Load("bolum-" + number + ".json").Info;
        Assert.NotNull(info);
        Assert.InRange(info.Paragraphs.Count, 2, 4);
        string text = info.Title + string.Join("", info.Paragraphs);
        foreach (var glyph in new[] { "✓", "✗", "→" }) Assert.DoesNotContain(glyph, text);   // kartın yazı tipinde bu işaretler yok
    }

    // Görev yalnızca report() ile bitebilir (Bölüm 74: toplanacak yok, antene toplam gönderilir)
    [Fact]
    public void Yalniz_rapor_gorevi_olan_bolum_okunur()
    {
        var level = Level.Parse("{\"numara\": 74, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\". . R\", \". . .\"], \"komutlar\": [\"move\", \"report\"], \"rapor\": 3, "
            + "\"konular\": [\"x\"], \"ipuclari\": [\"x\"], \"cozum\": [\"report(3)\"]}");
        Assert.True(level.Run(level.Solution).Complete);
        Assert.False(level.Run("report(4)\n").Complete);
    }

    // "return sayi * 3" tek başına fonksiyon dışında geçersizdir; düğme olarak geçerli sayılmalı
    [Fact]
    public void Return_parcasi_gecerli_sayilir()
    {
        var level = Level.Parse(LevelJson.Screen(", \"parcalar\": [\"def iki_kat(sayi):\", \"return sayi * 3\"]"));
        Assert.Empty(LevelCheck.Problems(level));
    }

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

    // Mini bölüme rutin şartı eklenmiş hâli: cozum rutinsiz olduğu için denetleyici yakalamalı
    [Fact]
    public void Rutin_sarti_okunur_ve_denetlenir()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutinler\": [\"git\"], \"komutlar\""));
        Assert.Equal(new[] { "git" }, level.Routines.Names);
        Assert.Contains(LevelCheck.Problems(level), p => p.Contains("görevi bitirmedi"));
    }

    [Fact]
    public void Rutin_sayisi_okunur()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutin_sayisi\": 2, \"komutlar\""));
        Assert.Equal(2, level.Routines.MinCount);
        Assert.True(Level.Parse(Mini).Routines.IsEmpty);
    }

    [Fact]
    public void Parametre_sarti_okunur()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutinler\": [\"git\"], \"parametre_sarti\": true, \"komutlar\""));
        Assert.True(level.Routines.NeedsParameters);
        Assert.False(Level.Parse(Mini).Routines.NeedsParameters);
    }

    [Fact]
    public void Donus_sarti_okunur()
    {
        var level = Level.Parse(Mini.Replace("\"komutlar\"", "\"rutinler\": [\"git\"], \"donus_sarti\": true, \"komutlar\""));
        Assert.True(level.Routines.NeedsReturn);
        Assert.False(Level.Parse(Mini).Routines.NeedsReturn);
    }

    [Theory]
    [InlineData("\"parametre_sarti\": true, ", "rutin şartıyla birlikte")]
    [InlineData("\"rutinler\": [\"git\"], \"parametre_sarti\": 1, ", "true ya da false")]
    [InlineData("\"donus_sarti\": true, ", "rutin şartıyla birlikte")]
    [InlineData("\"rutinler\": [\"git\"], \"donus_sarti\": 1, ", "true ya da false")]
    public void Bozuk_parametre_sarti_anlasilir_mesaj_verir(string field, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace("\"komutlar\"", field + "\"komutlar\"")));
        Assert.Contains(expected, e.Message);
    }

    [Theory]
    [InlineData("\"rutinler\": [\"sabah turu\"], ", "geçerli bir rutin adı değil")]
    [InlineData("\"rutinler\": [\"move\"], ", "oyun komutunun adı")]
    [InlineData("\"rutinler\": [], ", "boş olamaz")]
    [InlineData("\"rutin_sayisi\": 0, ", "en az 1")]
    public void Bozuk_rutin_alani_anlasilir_mesaj_verir(string field, string expected)
    {
        var e = Assert.Throws<DataFormatError>(() => Level.Parse(Mini.Replace("\"komutlar\"", field + "\"komutlar\"")));
        Assert.Contains(expected, e.Message);
    }
}
