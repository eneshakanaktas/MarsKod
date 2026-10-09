using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarsKod.Dunya;
using MarsKod.Motor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// MarsKod oyun sahnesi (unitytaslak1'den geldi): sahneyi kodla kurar (kamera, isik, arka plan, oyun alani,
// robot, buzlar, arayuz). Bolumler dosyadan gelir (Resources/Bolumler/*.json): harita, gorev, ipucu, dogru cozum.
// Kod karttaki Python kodu gercekten calisir: once motorda (Assets/Motor) ve dunya kurallarinda (Assets/Dunya)
// aninda calistirilir, sonra olanlar satir satir animasyonla oynatilir.
public class Oyun : MonoBehaviour
{
    // Sahne simdilik 6x6 alan ciziyor; bolum dosyalari da 6x6 (motor-test bunu denetler).
    const int Cols = 6, Rows = 6;
    const float TileTop = 0f;
    // Taslaktakinden daha dik bakis (40 -> 60): arka siralar ezilmez, alan ekranda buyuk ve net gorunur.
    const float CameraPitch = 60f, CameraFov = 32f;
    // Alanin ustunde ufuk, tepeler ve koloni icin birakilan bant (ekran yuksekliginin orani).
    const float HorizonGap = 0.075f;
    // Ufkun (tepeler, koloni) alanin arka kenarindan yuksekligi; tam boy fotografta ekran yuksekliginin orani.
    // Fotograf kuculurse bu mesafe de onunla birlikte kuculur.
    const float HorizonAboveArea = 0.106f;
    const float StartYaw = 125f;
    // Bolge 1 finali: "Oyuna yapilacaklar" madde 5 (docs/tasarim/senaryo-bolge-01.md). Ses yok, bu yuzden sessiz hali.
    const int Bolge1SonBolum = 10;
    const string Bolge1KapanisSatiri = "Bilinmeyen sinyal algılandı (kutup bölgesi). Bu sinyal ödevin parçası değil.";
    // Bolge 2 finali: docs/tasarim/senaryo-bolge-02.md "Kapanis sahnesi". Aracin ekrani, uzaklasma, sera.
    const int Bolge2SonBolum = PolarTraces.LastLevel;
    static readonly string[] Bolge2KapanisSatirlari =
    {
        "SON KAYIT — ACİL ALIM — 2 KİŞİ — HEDEF: [VERİ BOZUK]",
        "OTOMATİK SÜRÜŞ: KOLONİ",
        "Sera sulandı! Bitki sağlığı: %12.",
    };
    // Bolge 3 finali: docs/tasarim/senaryo-bolge-03.md "Kapanis sahnesi". Gun dogumu, paneller, toz uyarisi.
    const int Bolge3SonBolum = CraterTraces.LastLevel;
    static readonly string[] Bolge3KapanisSatirlari =
    {
        "Gün doğumu. Tarla gücü: %100.",
        "Koloni gündüz enerjisi: %40.",
        "Hava uyarısı: toz. Sonraki ödev: kum tepeleri.",
    };
    // Bolge 4 finali: docs/tasarim/senaryo-bolge-04.md "Kapanis sahnesi". Yon bulma diregi, anten, kurdele.
    const int Bolge4SonBolum = DuneTraces.LastLevel;
    static readonly string[] Bolge4KapanisSatirlari =
    {
        "Yön bulma direği: çalışıyor. İbre bir yapıyı gösteriyor.",
        "Yapı tarandı: anten (devrik).",
        "Sonraki ödev: anten tepesi.",
    };
    // Bolge 5 finali = Perde 1 finali: docs/tasarim/senaryo-bolge-05.md "Kapanis sahnesi". Anten tam guce gecer,
    // Defne'nin kaydi gelir (metin senaryo-perde-1.md), arayuz degisir: robotun adi, baslik, programin son satiri.
    const int Bolge5SonBolum = AntennaTraces.LastLevel;
    const string Bolge5DosyaSatiri = "Ödev 50 tamamlandı! Bir dosya geldi: ogretmene.ses";
    const string Bolge5Dosya = "ogretmene.ses";
    static readonly string[] Bolge5Kayit =
    {
        "Bu bir ödev değil.",
        "Adım Defne Aras. Bu koloninin komutanıyım.",
        "Bunu duyuyorsan, onu yeniden öğretecek kişi sensin.",
        "Bir güneş fırtınası geliyor. Biz yeraltına iniyoruz, hepimiz. Uyuyacağız.",
        "Fırtına robotumuzun belleğini silecek. Ona her şeyi baştan öğretmen gerekecek.",
        "Kızım Ece ona Kıvılcım der. Ona iyi bak.",
        "Yolu işaretledik. Okları takip et.",
        "Seni tanımıyorum. Ama sana güveniyorum. Lütfen… ona öğret.",
    };
    const string Bolge5EskiAd = "BKM-7", Bolge5YeniAd = "KIVILCIM";
    const string Bolge5Baslik = "BÖLGE 5  ·  ANTEN TEPESİ";
    const string Bolge5SonSatir = "Ödev modu kapatıldı.";
    // Perde 2'ye gecis (senaryo-bolge-06.md): finalin son karesinden bu kadar sonra "Devam" -> sinav-50 -> Bolum 51
    const float Bolge5DevamBekleme = 2.5f;
    const string Kayit2Anahtari = "kayit2";   // Bolum 52 basindaki kayit bir kez gosterilir


    Camera cam;
    Material backdrop;
    BackdropCache backdropCache;
    Transform world;
    Robot robot;
    RobotModel robotModel = Robot.DefaultModel;
    readonly List<IPickup> pickups = new List<IPickup>();
    // Calisan gunes panelleri (saglam/catlak; kirik olanlar toplanacak oldugu icin pickups'ta)
    readonly Dictionary<Cell, SolarPanel> panels = new Dictionary<Cell, SolarPanel>();
    readonly Dictionary<Cell, DustCloud> dustClouds = new Dictionary<Cell, DustCloud>();
    // Enerji hucresi bolumlerinde koloninin guc lambalari
    ColonyPower colonyPower;
    // Isiklar: renkleri bolgeye gore (RegionLook)
    Light skyLight, sunLight;
    Target target;
    Transform levelRoot;
    // Bolum 50 kapanisinda robotun ustundeki ad etiketi (yeniden denemede kaldirilir)
    NameTag nameTag;
    Material ground;
    Hud hud;
    // Bolumler (numara sirasiyla) ve oynanan bolum
    readonly List<Level> levels = new List<Level>();
    int levelIndex;
    Level level;
    // Karttaki kod: oyuncunun yazdigi (bolum ilk acildiginda bolumun baslangic kodu). Bolum bolum telefonda saklanir.
    string code = "";
    // Deneme goruntuleri alinirken oyuncunun kayitli kodu kullanilmaz ve ustune yazilmaz
    bool shotsMode;
    // -shots'un genel bolum-bolum taramasinda sinavi atlamak icin (her bes bolumde durup beklemesin);
    // QuizCheck kendi icinde gecici olarak kapatip sinavi gercekten dener.
    bool suppressQuiz;
    Coroutine program;
    bool running, done, complete;
    // Adim adim modu: her satirdan sonra ⏭ basisini bekler (stepRequested: bir satir daha calissin)
    bool stepMode, stepRequested;
    float stars = 1f, starsTarget = 1f;
    // Kademe basina XP; kademe kutusundaki ✓ ve bolum sonu XP (Gorev 9) bunu kullanir.
    Xp xp;
    // Bolum basina kac ipucu acildi (ilk insan testinde uc ipucu da bedava)
    HintLog hints;
    readonly HintView hintView = new HintView();
    // Kod sozlugu (Resources/Sozluk/sozluk.json); dosya okunamazsa bos
    Glossary glossary = new Glossary();
    // Mini sinav (her 5 bolumden sonra, zorunlu): bolum numarasina gore sinav; hangi bolumlerin sinavi gecildigi kalicidir.
    readonly Dictionary<int, Quiz> quizzes = new Dictionary<int, Quiz>();
    readonly HashSet<int> quizzesPassed = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<Oyun>() != null) return;
        new GameObject("Oyun").AddComponent<Oyun>();
    }

    static Vector3 Pos(int c, int r) => new Vector3(c - (Cols - 1) * 0.5f, TileTop, r - (Rows - 1) * 0.5f);
    static Vector3 Pos(Cell c) => Pos(c.Col, c.Row);

    bool HasNext => levelIndex + 1 < levels.Count;

    // Resources/Bolumler icindeki tum bolum dosyalari; bozuk dosya atlanir (sebebi log'a yazilir).
    void LoadLevels()
    {
        foreach (var file in Resources.LoadAll<TextAsset>("Bolumler"))
        {
            try
            {
                var l = Level.Parse(file.text);
                if (l.Cols != Cols || l.Rows != Rows)
                    throw new DataFormatError("Harita " + l.Cols + "x" + l.Rows + "; sahne şimdilik yalnızca " + Cols + "x" + Rows + " çiziyor.");
                levels.Add(l);
            }
            catch (DataFormatError e)
            {
                Debug.LogError("Bölüm dosyası okunamadı: " + file.name + ": " + e.Message);
            }
        }
        levels.Sort((a, b) => a.Number.CompareTo(b.Number));
    }

    // Resources/Sinavlar icindeki tum sinav dosyalari; bozuk dosya atlanir (sebebi log'a yazilir).
    void LoadQuizzes()
    {
        foreach (var file in Resources.LoadAll<TextAsset>("Sinavlar"))
        {
            try
            {
                var q = Quiz.Parse(file.text);
                quizzes[q.AfterLevel] = q;
            }
            catch (DataFormatError e)
            {
                Debug.LogError("Sınav dosyası okunamadı: " + file.name + ": " + e.Message);
            }
        }
        foreach (var s in PlayerPrefs.GetString("sinav", "").Split(','))
            if (int.TryParse(s, out var n)) quizzesPassed.Add(n);
    }

    void MarkQuizPassed(int afterLevel)
    {
        quizzesPassed.Add(afterLevel);
        if (shotsMode) return;
        PlayerPrefs.SetString("sinav", string.Join(",", quizzesPassed));
        PlayerPrefs.Save();
    }

    void LoadGlossary()
    {
        var file = Resources.Load<TextAsset>("Sozluk/sozluk");
        if (file == null) { Debug.LogError("Kod sözlüğü dosyası bulunamadı: Resources/Sozluk/sozluk.json"); return; }
        try
        {
            glossary = Glossary.Parse(file.text);
        }
        catch (DataFormatError e)
        {
            Debug.LogError("Kod sözlüğü okunamadı: " + e.Message);
        }
    }

    // Sozluk kartlari: her sayfa, acildigi bolum ve oynanan bolumde yeni mi (hicbir bolumde acilmayan sayfa gosterilmez)
    List<GlossaryView.Entry> GlossaryEntries()
    {
        var entries = new List<GlossaryView.Entry>();
        foreach (var page in glossary.Pages)
        {
            var opens = Glossary.OpensAt(page, levels);
            if (opens.HasValue)
                entries.Add(new GlossaryView.Entry { Page = page, OpensAt = opens.Value, Fresh = opens.Value == level.Number });
        }
        return entries;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        // arka plan ve zemin ayni gokyuzu/ufuk cizimini kullanir (MarsSky.hlsl)
        Shader.SetGlobalFloat("_Horizon", 0.79f);
        Shader.SetGlobalVector("_Picture", new Vector4(1f, 0f, 0f, 0f));
        Shader.SetGlobalVector("_Sun", new Vector4(0.02f, -0.068f, 0f, 0f));
        Shader.SetGlobalVector("_Focus", new Vector4(0.5f, 0.56f, 0f, 0f));
        Shader.SetGlobalFloat("_StarsOn", 1f);
        GraphicsOptions.Parse();   // cizim deneme secenekleri (-performans, -arkaplan yok ...)
        GraphicsQuality.Init();    // oyuncunun grafik ayari (performans modu)
        var a = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] == "-shots") { shotsMode = suppressQuiz = true; ListenWhileUnfocused(); }
            if (a[i] == "-robot" && i + 1 < a.Length && a[i + 1] == "oyuncak") robotModel = RobotModel.Toy;
            if (a[i] == "-robot" && i + 1 < a.Length && a[i + 1] == "gezgin") robotModel = RobotModel.Rover;
        }
        world = new GameObject("World").transform;

        SetupCamera();
        SetupLight();
        ApplyGraphics();
        GraphicsQuality.Changed += ApplyGraphics;
        BuildBoard();

        robot = Robot.Create(world, robotModel);
        colonyPower = ColonyPower.Create(world, cam);

        hud = gameObject.AddComponent<Hud>();
        hud.Build();
        if (GraphicsOptions.NoHud) hud.HideForMeasurement();
        FrameRateMeter.Create(System.Array.IndexOf(a, "-fps") >= 0);
        hud.RunPressed += OnRun;
        hud.StepPressed += OnStep;
        hud.CodeChanged += OnCodeChanged;
        hud.ResetPressed += ResetLevel;
        // Sol ustteki dugme bolum secme ekranini acar
        hud.MenuPressed += () => hud.ShowLevelSelect(LevelEntries());
        hud.LevelPicked += PickLevel;
        hud.GlossaryPressed += () => hud.ShowGlossary(GlossaryEntries());
        hud.StarsToggled += () => { starsTarget = starsTarget > 0.5f ? 0f : 1f; hud.SetStars(starsTarget > 0.5f); };

        Sound.Init(transform);
        hud.SetSound(Sound.Enabled);
        hud.SoundToggled += () => { Sound.SetEnabled(!Sound.Enabled); hud.SetSound(Sound.Enabled); };

        SetAnimations(!GraphicsOptions.NoAnimations && PlayerPrefs.GetInt(AnimationsKey, 1) != 0);
        hud.AnimationsToggled += () =>
        {
            SetAnimations(!animationsOn);
            PlayerPrefs.SetInt(AnimationsKey, animationsOn ? 1 : 0);
            PlayerPrefs.Save();
        };
        hud.PerformanceToggled += () =>
            GraphicsQuality.Set(GraphicsQuality.Performance ? GraphicsMode.Full : GraphicsMode.Performance);
        hud.SetPerformance(GraphicsQuality.Performance);

        xp = Xp.Load(PlayerPrefs.GetString("xp", ""));
        hud.SetTotalXp(xp.Total);
        hud.TierEarned = t => level != null && xp.Has(level.Number, (LineKind)((int)t + 1));
        hud.TierChanged += SaveTier;

        hints = HintLog.Load(shotsMode ? "" : PlayerPrefs.GetString("ipucu", ""));
        hud.HintPressed += PressHint;

        // Kod yazma kademesi: kayitli tercih kalici (kademe kutusu), -kademe deneme secenegi onune gecer.
        if (!shotsMode && System.Enum.TryParse(PlayerPrefs.GetString("kademe", ""), true, out KeyboardTier savedTier))
            hud.Tier = savedTier;
        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == "-kademe" && System.Enum.TryParse(a[i + 1], true, out KeyboardTier t)) hud.Tier = t;

        LoadLevels();
        LoadGlossary();
        LoadQuizzes();
        int start = 1;
        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == "-bolum") int.TryParse(a[i + 1], out start);
        LoadLevel(Mathf.Clamp(levels.FindIndex(l => l.Number == start), 0, levels.Count - 1));

        bool shotsArg = System.Array.IndexOf(a, "-shots") >= 0;
        bool showOpening = start == 1 && !shotsArg && PlayerPrefs.GetInt("acilis_gorundu", 0) == 0;
        if (System.Array.IndexOf(a, "-acilis") >= 0) showOpening = true; // test icin: her zaman goster
        if (showOpening)
        {
            PlayerPrefs.SetInt("acilis_gorundu", 1);
            PlayerPrefs.Save();
            hud.ShowOpening(() => { });
        }

        bool finale = System.Array.IndexOf(a, "-final") >= 0;   // yalnizca Bolum 50 kapanisinin goruntuleri (~2 dk)
        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == "-shots") { shotsMode = true; LoadLevel(levelIndex); StartCoroutine(finale ? FinaleShots(a[i + 1]) : Shots(a[i + 1])); }
    }

    // Arka plan animasyonlari (ayar, bolum secme ekraninda): kapaliyken gokyuzu duraganlasir, telefon daha az yorulur
    const string AnimationsKey = "animasyon";

    bool animationsOn;   // oyuncunun ayari; Performans modu acikken arka plan yine de duragan kalir

    void SetAnimations(bool on)
    {
        animationsOn = on;
        backdropCache.Animated = on && !GraphicsQuality.Performance;
        hud.SetAnimations(on);
    }

    // Grafik ayari (performans modu) uygulaninca: gunes golgesi, gokyuzu dokusu, arka plan animasyonu
    void ApplyGraphics()
    {
        skyLight.shadows = GraphicsQuality.Shadows;
        backdropCache.Animated = animationsOn && !GraphicsQuality.Performance;
        backdropCache.SetQuality(GraphicsQuality.BackdropScale, GraphicsQuality.BackdropRate);
        if (hud != null) hud.SetPerformance(GraphicsQuality.Performance);
    }

    void OnDestroy() => GraphicsQuality.Changed -= ApplyGraphics;

    void SetupCamera()
    {
        cam = new GameObject("Camera").AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Mats.Hex("#0E0B12");
        cam.fieldOfView = CameraFov;
        cam.allowMSAA = true;

        // Arka plan: kameraya bagli, ekrani dolduran tek kare
        backdrop = Mats.Custom("Backdrop", "MarsKod/Backdrop");
        var mesh = new Mesh { name = "Backdrop" };
        mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up, new Vector3(1, 1, 0) };
        mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);
        var bg = new GameObject("Backdrop");
        bg.transform.SetParent(cam.transform, false);
        bg.transform.localPosition = Vector3.forward * 5f;
        bg.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = bg.AddComponent<MeshRenderer>();
        mr.sharedMaterial = backdrop;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
        // pahali gokyuzu cizimi her karede degil, gerektikce bir dokuya cizilir; ekrandaki kare o dokuyu gosterir
        backdropCache = BackdropCache.Create(backdrop, mesh, GraphicsQuality.BackdropScale, GraphicsOptions.BackdropFlat);
    }

    // Isiklar ve golge; yonleri, renkleri ve ortam isigi bolgeye gore (RegionLook, bolum yuklenince)
    void SetupLight()
    {
        // los gok isigi (robot okunabilsin diye yumusak golge verir)
        skyLight = new GameObject("Sun").AddComponent<Light>();
        skyLight.type = LightType.Directional;
        skyLight.shadowStrength = 0.6f;

        // tepelerin ardindaki gunes: arkadan, alcaktan gelen, golgesiz isik
        sunLight = new GameObject("Dawn").AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.shadows = LightShadows.None;

        RenderSettings.skybox = null;
        RenderSettings.fog = false;
        RegionLook.For(Region.Plain).Apply(skyLight, sunLight);
    }

    // Oyun alani: cevresiyle ayni hizada, Mars yuzeyine gomulu bir bolge (godottaslak2'deki gibi).
    // Zemin tek parca ve ufka kadar uzanir: alan duz, disari hafif inisli cikisli; kayalar ve kraterler uzaklastikca seyrelir,
    // uzakta zemin arka plana (tepeler, koloni) dikissiz karisir. Kare sinirlari zeminde ince, soluk cizgiler.
    const float AreaHalfX = Cols * 0.5f, AreaHalfZ = Rows * 0.5f;
    const float Bend = 0.03f;
    // Zeminin (ve ustundeki kayalarin) arka plana karismaya basladigi ve bitirdigi uzaklik (z)
    static readonly Vector4 GroundFogRange = new Vector4(9f, 19f, 0, 0);

    static float TerrainHeight(float x, float z)
    {
        float dx = Mathf.Max(Mathf.Abs(x) - (AreaHalfX + 0.3f), 0f), dz = Mathf.Max(Mathf.Abs(z) - (AreaHalfZ + 0.3f), 0f);
        float o = Mathf.Sqrt(dx * dx + dz * dz);
        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((o - 0.15f) / 2.5f));
        if (z < 0f) k *= 0.5f; // one dogru (kameraya yakin) tepecikler alcak kalsin, alani ortmesin
        float n = (Mathf.PerlinNoise(x * 0.16f + 3f, z * 0.16f + 7f) - 0.5f) * 0.9f
                + (Mathf.PerlinNoise(x * 0.5f + 11f, z * 0.5f + 2f) - 0.5f) * 0.25f;
        float h = k * n * (1f + o * 0.08f);
        // kucuk bir gezegen yuzeyi gibi uzakta hafifce asagi kivrilir: zemin ufka dogal perspektifle uzanir
        // yanlarda kivrilma bir yerde durur: zemin genis gorunumde (klavye + ipucu) ucurum gibi dusmesin
        float bz = Mathf.Max(z - (AreaHalfZ + 0.8f), 0f), bx = Mathf.Min(Mathf.Max(Mathf.Abs(x) - (AreaHalfX + 1.5f), 0f), 14f);
        return h - Bend * (bz * bz + 0.5f * bx * bx);
    }

    void BuildBoard()
    {
        var board = Parts.Empty("Board", world);

        // Arazi agi: ortada sik, yanlarda seyrek. Klavye + ipucu acilip sahne cok kuculdugunde de zeminin kenari
        // (arkasindaki karanlik) gorunmesin diye yanlara genis uzanir.
        // z0: kod karti uzayip kamera geri cekilince ekranin alti da zemin gorsun (kartin kenarlarinda siyah kalmasin)
        var xs = TerrainAxis(-16f, 16f, 0.4f, 48f, 1.6f);
        var zs = TerrainAxis(-30f, 24f, 0.4f, 0f, 1f);
        int nx = xs.Count, nz = zs.Count;
        var verts = new Vector3[nx * nz];
        for (int j = 0; j < nz; j++)
        for (int i = 0; i < nx; i++)
            verts[j * nx + i] = new Vector3(xs[i], TerrainHeight(xs[i], zs[j]), zs[j]);
        var tris = new List<int>();
        for (int j = 0; j < nz - 1; j++)
        for (int i = 0; i < nx - 1; i++)
        {
            int a = j * nx + i, b = a + 1, c = a + nx + 1, d = a + nx;
            tris.Add(a); tris.Add(d); tris.Add(c); tris.Add(a); tris.Add(c); tris.Add(b);
        }
        var mesh = new Mesh { name = "Terrain", indexFormat = IndexFormat.UInt32 };
        mesh.vertices = verts;
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        ground = Mats.Custom("Ground", "MarsKod/Ground");
        ground.SetVector("_Area", new Vector4(AreaHalfX, AreaHalfZ, 0, 0));
        ground.SetVector("_FogRange", GroundFogRange);
        // alandaki krater kapali: bolumlerde kareler bos ya da dolu net okunmali
        // zemin kendine golge dusurmez (duz zeminde ince golge seritleri olusturuyordu); robot ve kayalarin golgesini alir
        Parts.Add("Terrain", board, mesh, ground, Vector3.zero, outline: false, castShadow: false);

        // Kosedeki isaret direkleri (ustunde kucuk sicak isik)
        var postMat = Mats.Lit(Mats.Hex("#3A3542"), 0.4f);
        var lamp = Mats.Emissive(Mats.Hex("#FFC08A"), Mats.Hex("#FF9A55") * 1.2f, 0.5f);
        var postMesh = MeshFactory.RoundedCylinder(0.03f, 0.3f, 0.01f, 12);
        var lampMesh = MeshFactory.Sphere(0.035f, 8, 12);
        foreach (var (sx, sz) in new[] { (-1f, -1f), (1f, -1f), (-1f, 1f), (1f, 1f) })
        {
            var basePos = new Vector3(sx * (AreaHalfX + 0.06f), 0f, sz * (AreaHalfZ + 0.06f));
            Parts.Add("Post", board, postMesh, postMat, basePos + Vector3.up * 0.15f);
            Parts.Add("Lamp", board, lampMesh, lamp, basePos + Vector3.up * 0.33f, outline: false, castShadow: false);
        }

        // Kayalar, kaya kumeleri, olcum istasyonlari, sandiklar (Scenery.cs). Alanin icindeki kayalar bolumden gelir (engel).
        // Zeminle birlikte arka plana karisirlar: zemin ufukta gokyuzune donustugu yerde havada asili kalmazlar.
        if (!GraphicsOptions.NoScenery) Scenery.Build(board, TerrainHeight, new Vector2(AreaHalfX, AreaHalfZ), FarRockMat);
    }

    // inner0..inner1 arasi fine adimla; disinda outer'a kadar coarse adimla (outer = 0: disari uzanmaz)
    static List<float> TerrainAxis(float inner0, float inner1, float fine, float outer, float coarse)
    {
        var a = new List<float>();
        if (outer > 0f) for (float v = -outer; v < inner0 - 0.01f; v += coarse) a.Add(v);
        int n = Mathf.RoundToInt((inner1 - inner0) / fine);
        for (int i = 0; i <= n; i++) a.Add(inner0 + i * fine);
        if (outer > 0f) for (float v = inner1 + coarse; v <= outer + 0.01f; v += coarse) a.Add(v);
        return a;
    }

    // Suslu kaya malzemesi: zeminle ayni isik ve ayni arka plana karisma (FarRock.shader)
    static Material FarRockMat(string hex)
    {
        var m = Mats.Custom("FarRock", "MarsKod/FarRock");
        m.SetColor("_BaseColor", Mats.Hex(hex));
        m.SetVector("_FogRange", GroundFogRange);
        m.SetVector("_Area", new Vector4(AreaHalfX, AreaHalfZ, 0, 0));
        return m;
    }

    // Bolumu kurar: buzlar, engel kayalari, hedef kare, zemindeki buz izleri, karttaki kod ve ust baslik.
    void LoadLevel(int index)
    {
        if (program != null) StopCoroutine(program);
        program = null;
        running = false; done = false; complete = false;
        stepMode = false; stepRequested = false;
        levelIndex = index;
        level = levels[index];

        if (levelRoot != null) Destroy(levelRoot.gameObject);
        levelRoot = Parts.Empty("Level", world);
        var region = Regions.Of(level.Number);
        RegionLook.For(region).Apply(skyLight, sunLight);
        pickups.Clear();
        for (int i = 0; i < level.Ices.Count; i++)
            pickups.Add(Pickups.Create(level.Item, levelRoot, Pos(level.Ices[i]), i + 1));
        for (int i = 0; i < level.Crystals.Count; i++)
            Ice.CreateHazard(levelRoot, Pos(level.Crystals[i]), 20 + i); // toplanmaz: pickups listesinde degil
        panels.Clear();
        int panelSeed = 0;
        foreach (var p in level.Panels)
            if (p.Value > 0) panels[p.Key] = SolarPanel.Create(levelRoot, Pos(p.Key), p.Value, 40 + panelSeed++);
        dustClouds.Clear();
        int dustSeed = 0;
        foreach (var d in level.Dust)
            dustClouds[d.Key] = DustCloud.Create(levelRoot, Pos(d.Key), 60 + dustSeed++);
        colonyPower.Begin(PowerLampCount);
        int frostCount = level.Item == Collectible.Ice ? level.Ices.Count : 0; // zemindeki buz izi yalnizca buzun altinda
        for (int i = 0; i < 6; i++)
        {
            var p = i < frostCount ? Pos(level.Ices[i]) : Vector3.zero;
            ground.SetVector("_Frost" + i, new Vector4(p.x, p.z, 0.45f, i < frostCount ? 1f : 0f));
        }
        for (int i = 0; i < level.Rocks.Count; i++)
            Obstacles.Create(region, levelRoot, Pos(level.Rocks[i]), i, crate: CanyonTraces.IsDepot(level.Number));
        target = level.Target.HasValue ? Target.Create(levelRoot, Pos(level.Target.Value)) : null;
        Traces.Build(level.Number, levelRoot, new Vector2(AreaHalfX, AreaHalfZ), level.Target.HasValue ? Pos(level.Target.Value) : (Vector3?)null,
            level.Ices.ConvertAll(c => Pos(c)), TerrainHeight);

        ResetRobot();
        string saved = SavedCode(level);
        if (saved != null) hud.LoadCode(saved, SavedKinds(level));
        else hud.LoadStartCode(level.StartCode);
        code = hud.Code;
        hud.SetOpenWords(MarsKod.Dunya.Suggestions.OpenWords(levels, level.Number));
        hud.SetPieces(level.Pieces, Palette.NewPieces(levels, level.Number));
        hud.SetLevel(level.Number, level.Label, level.Title, level.Goal, level.Ices.Count, level.Item, level.Intro);
        var transition = Traces.TransitionLines(level.Number);
        if (!shotsMode && transition != null) hud.ShowTransition(transition, () => { });
        if (!shotsMode && level.Number == CanyonTraces.RecordingLevel && PlayerPrefs.GetInt(Kayit2Anahtari, 0) == 0)
            StartCoroutine(PlayCanyonRecording());
        hintView.Close();
    }

    // Kayit 2 (Bolum 52 basi): ekran kararir, Defne'nin kaydi satir satir; bitince ekran acilir ve bolumun giris damgasi gelir.
    IEnumerator PlayCanyonRecording()
    {
        Sound.Static();
        hud.ShowRecording(CanyonTraces.RecordingFile, CanyonTraces.RecordingLines);
        while (!hud.RecordingFinished) yield return null;
        PlayerPrefs.SetInt(Kayit2Anahtari, 1);
        PlayerPrefs.Save();
        yield return Tween.Wait(1.2f);   // sessizlik
        Sound.Static();
        hud.HideRecording();
        yield return Tween.Wait(1f);
        hud.ShowIntro(level.Intro);
    }

    void LateUpdate()
    {
        FitCamera();
        stars = Mathf.MoveTowards(stars, starsTarget, Time.deltaTime * 2.5f);
        Shader.SetGlobalFloat("_StarsOn", stars);
    }

    // Oyun alanini, arayuzun biraktigi bos banda (baslik ile kod karti arasi) olabildigince buyuk sigdirir.
    // Ekran boyu, centik ya da kod uzunlugu degisse de alan hep en buyuk haliyle ortalanir.
    static readonly Vector3[] Corners = BoardCorners();

    static Vector3[] BoardCorners()
    {
        float x = AreaHalfX + 0.12f, z = AreaHalfZ + 0.12f;
        return new[]
        {
            new Vector3(-x, 0f, -z), new Vector3(x, 0f, -z), new Vector3(-x, 0f, z), new Vector3(x, 0f, z),
            new Vector3(-x, 0.4f, z), new Vector3(x, 0.4f, z), // arka kosedeki direklerin tepesi
            new Vector3(0f, 0.9f, 0f), // robot ve buzlar alanin ortasinda da kesilmesin
        };
    }

    // Kameranin kullandigi bant; arayuz degisince (ipucu balonu acildi vb.) hedefe yumusakca kayar
    Vector2? smoothBand;

    void FitCamera()
    {
        var target = hud.FreeBand();
        smoothBand = smoothBand.HasValue ? Vector2.Lerp(smoothBand.Value, target, 1f - Mathf.Exp(-Time.deltaTime * 12f)) : target;
        var band = smoothBand.Value;
        float bottom = band.x + 0.015f, top = band.y - HorizonGap;
        const float side = 0.035f;
        float needW = 1f - 2f * side, needH = Mathf.Max(0.1f, top - bottom);

        cam.transform.rotation = Quaternion.Euler(CameraPitch, 0f, 0f);
        cam.fieldOfView = CameraFov;
        cam.ResetProjectionMatrix();
        Rect Extent(float d)
        {
            cam.transform.position = -cam.transform.forward * d;
            float x0 = 1f, x1 = 0f, y0 = 1f, y1 = 0f;
            foreach (var c in Corners)
            {
                var v = cam.WorldToViewportPoint(c);
                x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
            }
            return Rect.MinMaxRect(x0, y0, x1, y1);
        }
        // 1) Fotograf: kamera, alan ekranin genisligini dolduracak uzakliga gelir (ikiye bolerek arama).
        //    Bu uzaklik yalnizca ekran genisligine bagli; klavye/ipucu acilinca degismez.
        float lo = 3f, hi = 80f;
        for (int k = 0; k < 24; k++)
        {
            float mid = (lo + hi) * 0.5f;
            if (Extent(mid).width <= needW) hi = mid; else lo = mid;
        }
        float d = hi;
        var photo = Extent(d);

        // 2) Bant alana yetmiyorsa (klavye, ipucu, uzun kod) fotograf butunuyle kuculur: kamera yerinde kalir,
        //    yalnizca gorus acisi genisler. Zemin, kayalar, koloni ve gokyuzu birlikte kuculur; birbirine gore kaymaz.
        float scale = Mathf.Min(1f, needH / photo.height);
        cam.fieldOfView = 2f * Mathf.Atan(Mathf.Tan(CameraFov * 0.5f * Mathf.Deg2Rad) / scale) * Mathf.Rad2Deg;
        cam.ResetProjectionMatrix();
        var ext = Extent(d);
        cam.nearClipPlane = Mathf.Max(0.3f, d - 15f);
        cam.farClipPlane = d + 45f;

        // 3) Fotografi dikeyde kaydir: alan bandin ortasina gelsin (perspektif bozulmaz)
        float shift = 2f * ((bottom + top) * 0.5f - ext.center.y);
        var p = cam.projectionMatrix;
        p[1, 2] = -shift;
        cam.projectionMatrix = p;

        // Arka plan (tepeler, koloni, gokyuzu) ayni fotografin parcasi: ayni olcek ve kayma (MarsSky.hlsl, MarsPictureUV).
        // Ufuk fotografta alanin arka kenarinin biraz ustunde sabittir.
        Shader.SetGlobalVector("_Picture", new Vector4(scale, shift, 0f, 0f));
        Shader.SetGlobalFloat("_Horizon", photo.yMax + HorizonAboveArea);

        var urp = GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        if (urp != null && urp.shadowDistance < d + 8f) urp.shadowDistance = d + 8f;
    }

    // ---- Gosteri programi ----

    void OnRun()
    {
        if (running)
        {
            // adim adim modunda "Devam": kalan satirlar normal hizda
            if (stepMode) { stepMode = false; stepRequested = false; }
            return;
        }
        if (done && complete && HasNext)
        {
            int justFinished = level.Number, nextIndex = levelIndex + 1;
            if (!suppressQuiz && quizzes.TryGetValue(justFinished, out var q) && !quizzesPassed.Contains(justFinished))
            {
                hud.ShowQuiz(q, () => { MarkQuizPassed(justFinished); LoadLevel(nextIndex); });
                return;
            }
            LoadLevel(nextIndex);
            return;
        }
        StartProgram(stepping: false);
    }

    // ⏭: kod calismiyorsa adim adim baslatir (ilk satir hemen calisir); calisiyorsa bir satir daha ilerletir
    // (normal hizda calisirken basilirsa o satirdan sonra durur).
    void OnStep()
    {
        if (running) { stepMode = true; stepRequested = true; return; }
        if (done && complete) return;
        StartProgram(stepping: true);
    }

    void StartProgram(bool stepping)
    {
        if (done) ResetLevel();
        stepMode = stepping;
        stepRequested = stepping;
        program = StartCoroutine(RunProgram());
    }

    // Bolum secme ekraninin satirlari: her bolumun adi, hedefi, kazanilan XP'si (XP > 0 = cozuldu)
    // Kilit kurali: bolum 1 her zaman acik; sonraki bir bolum, bir oncekinin XP'si olmadan (hic cozulmediyse) kilitli.
    // Su an oynanan bolum kurala uymasa bile (orn. -bolum ile dogrudan acildiysa) kilitli gosterilmez.
    // GECICI TEST ANAHTARI (Ragip, 2026-10-03): ekip denemesi icin butun bolumler acik.
    // Play Store'a cikmadan once false yapilmali (ILERLEME.md'de not var).
    const bool TestAllLevelsOpen = true;
    bool LocksOn => forceLocks || (!shotsMode && !TestAllLevelsOpen);   // -shots modunda da kilit yok
    bool forceLocks;   // yalnizca goruntu denetimi: kilitli kartlar gercekte nasil gorunur

    List<LevelSelect.Entry> LevelEntries()
    {
        var entries = new List<LevelSelect.Entry>();
        bool prevSolved = true;
        bool noteFound = xp.LevelTotal(AntennaTraces.LockedNoteLevel) > 0;
        foreach (var l in levels)
        {
            int here = xp.LevelTotal(l.Number);
            entries.Add(new LevelSelect.Entry
            {
                Number = l.Number, Title = l.Title, Goal = l.Goal,
                Xp = here, Current = l == level, Locked = LocksOn && !prevSolved && l != level,
                LockedTitle = AntennaTraces.ShowsLockedNote(l.Number, noteFound) ? AntennaTraces.LockedNote : null,
            });
            prevSolved = here > 0;
        }
        return entries;
    }

    void PickLevel(int number)
    {
        int idx = levels.FindIndex(l => l.Number == number);
        if (idx < 0) return;
        // kilitli bolume doğrudan cagrilsa bile (orn. deneme) girilmez; onceki bolum bitmeli (shotsMode'da kilit yok)
        if (LocksOn && idx > 0 && xp.LevelTotal(levels[idx - 1].Number) == 0 && levels[idx] != level) return;
        LoadLevel(idx);
    }

    // Ampul: 1. -> 2. -> 3. ipucu, bir daha basinca kapanir; tekrar acilinca yine 1.'den baslar.
    // Bolumde en cok kacinci ipucuna gidildigi saklanir (ileride yildiz/jeton kurali buna bakar; deneme kosusunda kaydedilmez).
    void PressHint()
    {
        if (!hud.HintOpen) hintView.Close(); // balon baska bir sebeple kapandiysa (hata kutusu, calistirma) bastan basla
        int total = level.Hints.Count;
        hintView.Press(total);
        if (!hintView.Open) { hud.HideHint(); return; }
        hints.Reach(level.Number, hintView.Current, total);
        hud.ShowHint(level.Hints[hintView.Current - 1], hintView.Current, total);
        if (shotsMode) return;
        PlayerPrefs.SetString("ipucu", hints.Save());
        PlayerPrefs.Save();
    }

    void SaveTier(KeyboardTier t)
    {
        PlayerPrefs.SetString("kademe", t.ToString());
        PlayerPrefs.Save();
    }

    // Enerji hucresi bolumunde her hucre icin kolonide bir lamba; diger bolumlerde lamba yok
    int PowerLampCount => level.Item == Collectible.EnergyCell ? level.Ices.Count : 0;

    // Robot baslangic karesine doner; Bolge 4 finalinden sonraki bolumlerde sirtinda Ece'nin kurdelesi var
    void ResetRobot()
    {
        robot.ResetTo(Pos(level.Robot), StartYaw);
        robot.SetRibbon(level.Number > DuneTraces.LastLevel);
    }

    void ResetLevel()
    {
        if (program != null) StopCoroutine(program);
        program = null;
        running = false; done = false; complete = false;
        stepMode = false; stepRequested = false;
        ResetRobot();
        foreach (var pickup in pickups) pickup.Restore();
        foreach (var panel in panels.Values) panel.Restore();
        foreach (var cloud in dustClouds.Values) cloud.Restore();
        colonyPower.Begin(PowerLampCount);
        Traces.ResetBackdrop(level.Number);
        RegionLook.For(Regions.Of(level.Number)).Apply(skyLight, sunLight);   // Bolge 3 finalindeki gun dogumunu geri alir
        if (nameTag != null) Destroy(nameTag.gameObject);
        if (target != null) target.Restore();
        hud.ResetView();
    }

    // Kodu motorda aninda calistirir, sonra kaydi oynatir.
    IEnumerator RunProgram()
    {
        running = true;
        hud.SetRunning(true);
        hud.HideMessage();
        var world = level.CreateWorld();
        var report = ProgramRun.Execute(code, world, routines: level.Routines);

        // Bitmeyen dongude kaydin sadece basi oynatilir; uzun kayitta bos satirlarda beklenmez.
        int count = report.Halt != null && report.Halt.Reason == "steps" ? Mathf.Min(report.Trace.Count, 40) : report.Trace.Count;
        bool pauseOnEmpty = report.Trace.Count <= 300;
        robot.GlanceAtCamera(true);
        for (int i = 0; i < count; i++)
        {
            if (stepMode)
            {
                if (!stepRequested)
                {
                    hud.SetRunning(true, paused: true);
                    while (stepMode && !stepRequested) yield return null;
                    hud.SetRunning(true);
                }
                stepRequested = false;
            }
            var entry = report.Trace[i];
            hud.SetActiveLine(entry.Line - 1);
            if (entry.Events.Count == 0)
                yield return pauseOnEmpty ? Tween.Wait(0.28f) : null;
            else
            {
                yield return Tween.Wait(0.12f);
                foreach (var e in entry.Events) yield return Play(e);
            }
            // satir bitti: yaninda degiskenlerin yeni hali ("i = 2")
            hud.SetActiveLine(entry.Line - 1, vars: entry.VarsText());
        }
        stepMode = false;
        robot.GlanceAtCamera(false);

        if (report.Stopped)
        {
            hud.SetActiveLine(report.StopLine.HasValue ? report.StopLine.Value - 1 : -1, error: true);
            ShowStop(report);
            robot.ShowPuzzled();
            hud.SetRunning(false);
            done = true;
        }
        else if (report.Complete)
        {
            hud.SetActiveLine(-1);
            if (target != null) target.Reach();
            yield return Tween.Wait(0.15f);
            complete = true;
            var kind = Xp.SolutionKind(hud.Buffer);
            int earned = xp.Award(level.Number, kind);
            string xpLine;
            if (earned > 0)
            {
                if (!shotsMode) { PlayerPrefs.SetString("xp", xp.Save()); PlayerPrefs.Save(); }
                hud.SetTotalXp(xp.Total);
                xpLine = "+" + earned + " XP · " + Xp.Name(kind) + " ile çözdün";
            }
            else xpLine = Xp.Name(kind) + " ile çözdün · bu XP daha önce alındı";
            hud.SetDone(HasNext, xpLine, xp.NextBetterText(level.Number, kind), level.Outro);
            bool perde1Finale = level.Number == Bolge5SonBolum && HasNext;
            if (perde1Finale) hud.HoldNext();
            done = true;
            Sound.Celebrate();
            Traces.LevelDone(level.Number);
            yield return robot.Celebrate();
            if (level.Number == Bolge1SonBolum) yield return ClosingSceneBolge1();
            if (level.Number == Bolge2SonBolum) yield return ClosingSceneBolge2();
            if (level.Number == Bolge3SonBolum) yield return ClosingSceneBolge3();
            if (level.Number == Bolge4SonBolum) yield return ClosingSceneBolge4();
            if (level.Number == Bolge5SonBolum) yield return ClosingSceneBolge5();
            if (perde1Finale)
            {
                yield return Tween.Wait(Bolge5DevamBekleme);   // son kare bir sure yalniz kalir
                hud.ShowContinue();
            }
        }
        else
        {
            hud.SetActiveLine(-1);
            if (world.Complete && report.RoutineProblem != null) // robot isi yapti ama bolumun istedigi rutin yok
                hud.ShowMessage("GÖREV", report.RoutineProblem.Title, report.RoutineProblem.Text, null, error: false);
            else if (world.IceLeft > 0)
                hud.ShowMessage("GÖREV", "Kod bitti, " + Collectibles.PluralName(level.Item) + " bitmedi",
                    "Kodun sonuna kadar çalıştı ama " + world.IceLeft + " " + Collectibles.Name(level.Item) + " daha toplanmayı bekliyor. Robot yalnızca kodda yazanı yapar: eksik adımı bul.",
                    null, error: false);
            else if (world.CrackedLeft > 0)
                hud.ShowMessage("GÖREV", "Kod bitti, çatlak panel kaldı",
                    "Kodun sonuna kadar çalıştı ama " + world.CrackedLeft + " çatlak panel daha onarılmayı bekliyor (turuncu ışıklı olanlar). Robot yalnızca kodda yazanı yapar: hangi paneli atladığını bul.",
                    null, error: false);
            else if (!world.ReportDone)
                hud.ShowMessage("GÖREV", "Kod bitti, rapor gönderilmedi",
                    "Kodun sonuna kadar çalıştı ama anten hâlâ sayıyı bekliyor. Sonunda report() ile istenen sayıyı gönder; sayıyı bir değişkende tut.",
                    null, error: false);
            else
                hud.ShowMessage("GÖREV", "Kod bitti, robot hedefte değil",
                    "Kodun sonuna kadar çalıştı ama robot işaretli kareye varmadı. Kod bittiğinde robot hedef karede durmalı: yolu adım adım say.",
                    null, error: false);
            robot.ShowPuzzled();
            hud.SetRunning(false);
            done = true;
        }
        running = false;
        program = null;
    }

    // Bolge 1 kapanisi: hisirti basar, telsiz diregindeki isik (Traces.RadioMast) yanip sonerken SOS calar;
    // robot guneye doner, program metni ikinci, habersiz satira gecer.
    IEnumerator ClosingSceneBolge1()
    {
        Sound.Static();
        yield return Tween.Wait(0.9f);
        Sound.Sos();
        yield return robot.TurnTo(180f, 0.5f);
        yield return Tween.Wait(0.6f);
        hud.SetOutro(Bolge1KapanisSatiri);
    }

    // Bolge 2 kapanisi: aracin ekrani son kaydi gosterir (hisirtiyla), arac otomatik suruse gecip koloniye uzaklasir,
    // robot arkasindan bakar; en sonda program habersiz ve neseli: sera sulandi.
    IEnumerator ClosingSceneBolge2()
    {
        yield return Tween.Wait(0.6f);
        Sound.Static();
        hud.SetOutro(Bolge2KapanisSatirlari[0]);
        yield return Tween.Wait(2.8f);
        hud.SetOutro(Bolge2KapanisSatirlari[1]);
        yield return robot.TurnTo(270f, 0.6f);   // robot aracin gidecegi yone (bati) doner
        yield return Traces.RoverDriveAway();
        hud.SetOutro(Bolge2KapanisSatirlari[2]);
        Traces.SeraYapragiAcildi();
    }

    // Bolge 3 kapanisi: oyundaki ilk gun dogumu. Isik sicaklasir, panellerin isiklari birer birer parlar,
    // robot dogudaki gunese doner; program habersiz: gunduz enerjisi, sonra toz uyarisi.
    IEnumerator ClosingSceneBolge3()
    {
        yield return Tween.Wait(0.6f);
        int order = 0;
        foreach (var panel in panels.Values) panel.StartCoroutine(FlashAfter(panel, 0.4f + 0.18f * order++));   // panel.Restore durdurur
        yield return RegionLook.For(Regions.Of(level.Number)).Sunrise(skyLight, sunLight, 3f);
        hud.SetOutro(Bolge3KapanisSatirlari[0]);
        yield return robot.TurnTo(90f, 0.8f);   // dogudaki gunese
        yield return Tween.Wait(2.2f);
        hud.SetOutro(Bolge3KapanisSatirlari[1]);
        yield return Tween.Wait(2.8f);
        hud.SetOutro(Bolge3KapanisSatirlari[2]);
    }

    // Bolge 4 kapanisi: toplanan parcalar yon bulma diregine oturur, ibre donup uzaktaki devrik anteni gosterir;
    // robot antene bakar. Sonra ruzgar son direkteki kurdeleyi cozer, robot ona donup uzanir, kurdele sirtina takilir;
    // robot yeniden antene doner (sirti, kurdelesiyle kameraya). Program kurdeleyi fark etmez.
    IEnumerator ClosingSceneBolge4()
    {
        var dune = Traces.Dune;
        if (dune == null) yield break;
        yield return Tween.Wait(0.6f);
        yield return dune.PointTheWay();
        hud.SetOutro(Bolge4KapanisSatirlari[0]);
        float toAntenna = dune.YawToAntenna(robot.transform.position);
        yield return robot.TurnTo(toAntenna, 0.7f);
        yield return Tween.Wait(1.4f);
        hud.SetOutro(Bolge4KapanisSatirlari[1]);
        yield return Tween.Wait(1.8f);

        var flight = dune.StartRibbonFlight(() => robot.RibbonPoint);
        yield return robot.TurnTo(DuneFinale.YawTowards(robot.transform.position, dune.RibbonPosition), 0.45f);
        yield return robot.Collect();
        yield return flight;
        robot.SetRibbon(true);
        Sound.Collect();
        yield return Tween.Wait(0.5f);
        yield return robot.TurnTo(toAntenna, 0.8f);
        yield return Tween.Wait(0.6f);
        hud.SetOutro(Bolge4KapanisSatirlari[2]);
    }

    // Bolge 5 kapanisi (Perde 1 finali, oyunun en guclu ani): anten tam guce gecer, isiklar bir an titrer, program
    // neseyle dosyayi haber verir. Ekran kararir, hisirti, Defne'nin kaydi satir satir. Sessizlik; ekran acilinca robot
    // antenin dibinde, gokyuzune bakiyor. Basinin ustunde "BKM-7" silinip "KIVILCIM" yazilir, ustteki "ÖDEV 50" yerine
    // bolgenin adi gelir, programin son satiri "Ödev modu kapatıldı." Robot anten isigini iki kez yakip sondurur (selam).
    IEnumerator ClosingSceneBolge5()
    {
        var antenna = Traces.Antenna;
        if (antenna == null) yield break;
        yield return Tween.Wait(0.5f);
        yield return robot.TurnTo(DuneFinale.YawTowards(robot.transform.position, antenna.Foot), 0.6f);
        robot.StartCoroutine(robot.LookUp());   // robotun kendi uzerinde: yeniden denemede ResetTo durdurur
        yield return antenna.PowerUp();
        yield return FlickerLights();
        hud.SetOutro(Bolge5DosyaSatiri);
        yield return Tween.Wait(3f);

        Sound.Static();
        hud.ShowRecording(Bolge5Dosya, Bolge5Kayit);
        yield return Tween.Wait(1.2f);   // ekran karardi: robot antenin dibine gecer
        var foot = new Vector3(Pos(level.Target ?? level.Robot).x, TileTop, AreaHalfZ - 0.5f);
        yield return robot.MoveTo(foot, 0.05f);
        yield return robot.TurnTo(DuneFinale.YawTowards(robot.transform.position, antenna.Foot), 0.05f);
        while (!hud.RecordingFinished) yield return null;
        yield return Tween.Wait(1.6f);   // sessizlik
        Sound.Static();
        hud.HideRecording();
        yield return Tween.Wait(2.2f);

        nameTag = NameTag.Create(levelRoot, robot);
        var rename = nameTag.StartCoroutine(nameTag.Rename(Bolge5EskiAd, Bolge5YeniAd));
        yield return Tween.Wait(3.2f);
        hud.RewriteChapter(Bolge5Baslik);
        yield return rename;
        hud.SetOutro(Bolge5SonSatir);
        yield return Tween.Wait(1.6f);
        for (int i = 0; i < 2; i++)
        {
            Sound.Blip();
            antenna.StartCoroutine(antenna.Greet(1));
            yield return robot.Signal(1);
        }
    }

    // Anten tam guce gecerken sahnenin isiklari bir an titrer (guc dalgalanmasi)
    IEnumerator FlickerLights()
    {
        float sky = skyLight.intensity, sun = sunLight.intensity;
        yield return Tween.Run(0.9f, t =>
        {
            float k = Mathf.PerlinNoise(t * 14f, 0.3f) < 0.5f ? 0.45f : 1f;
            skyLight.intensity = sky * k;
            sunLight.intensity = sun * k;
        });
        skyLight.intensity = sky;
        sunLight.intensity = sun;
    }

    static IEnumerator FlashAfter(SolarPanel panel, float delay)
    {
        yield return Tween.Wait(delay);
        yield return panel.Flash();
    }

    static float Yaw(Direction d) => d == Direction.North ? 0f : d == Direction.East ? 90f : d == Direction.South ? 180f : 270f;

    IEnumerator Face(Direction d)
    {
        float yaw = Yaw(d);
        if (Mathf.Abs(Mathf.DeltaAngle(robot.Yaw, yaw)) > 1f) yield return robot.TurnTo(yaw, 0.28f);
    }

    IEnumerator Play(WorldEvent e)
    {
        switch (e)
        {
            case Moved m:
                Sound.Move();
                yield return Face(m.Direction);
                yield return robot.MoveTo(Pos(m.To));
                break;
            case Blocked b:
                Sound.Bump();
                yield return Face(b.Direction);
                yield return robot.Bump();
                break;
            case Collected c:
                StartCoroutine(robot.Collect());
                yield return Tween.Wait(0.12f);
                if (c.IceIndex >= 0)
                {
                    Sound.Collect();
                    pickups[c.IceIndex].Pop();
                    colonyPower.Deliver(world.TransformPoint(Pos(level.Ices[c.IceIndex])));
                    hud.SetCollected(c.Total);
                    Traces.IceCollected(c.Total, level.Ices.Count);
                }
                yield return Tween.Wait(c.IceIndex >= 0 ? 0.4f : 0.3f);
                break;
            case Scanned sc:
                StartCoroutine(ScanPulse.Play(levelRoot, Pos(sc.At), sc.Found));
                yield return Tween.Wait(0.45f);
                break;
            case Measured me:
                var tint = me.Power == 0 ? new Color(0.85f, 0.85f, 0.92f) : World.IsCracked(me.Power) ? Mats.Hex("#FFA53A") : Mats.Hex("#7CF07A");
                StartCoroutine(PowerLabel.Show(levelRoot, Pos(me.At), me.Power, tint));
                if (panels.TryGetValue(me.At, out var measured)) StartCoroutine(measured.Flash());
                yield return Tween.Wait(0.5f);
                break;
            case CableMeasured cm:
                StartCoroutine(PowerLabel.Show(levelRoot, Pos(cm.At), cm.Length + " m", cm.Length == 0 ? new Color(0.85f, 0.85f, 0.92f) : Mats.Hex("#FFB070")));
                yield return Tween.Wait(0.5f);
                break;
            case ReportSent rs:
                if (rs.Correct) Sound.Collect();
                if (rs.Correct && Traces.Antenna != null) Traces.Antenna.Acknowledge();   // antenin durum isigi bir kez yesil
                StartCoroutine(PowerLabel.Show(levelRoot, Pos(rs.At), "RAPOR " + rs.Value, rs.Correct ? Mats.Hex("#7CF07A") : Mats.Hex("#FF6B5A")));
                yield return Tween.Wait(0.7f);
                break;
            case Repaired rp:
                StartCoroutine(robot.Collect());
                Sound.Collect();
                if (panels.TryGetValue(rp.At, out var repaired)) yield return repaired.Repair();
                yield return Tween.Wait(0.15f);
                break;
            case Waited w:
                if (dustClouds.TryGetValue(w.At, out var cloud)) StartCoroutine(cloud.Thin(w.Left, w.Total));
                yield return Tween.Wait(0.45f);
                break;
            case Rejected _:
                Sound.Bump();
                yield return robot.Collect();
                yield return robot.Bump();
                break;
        }
    }

    // Durma sebebini Turkce anlatir; Python hatasinda Python'un kendi mesaji da altta gorunur.
    void ShowStop(RunReport report)
    {
        Sound.Error();
        if (report.Rule != null)
        {
            hud.ShowMessage("OYUN KURALI", report.Rule.Title, report.Rule.Text, null, error: true);
            return;
        }
        var ex = report.Error != null ? Explain.ExplainError(report.Error) : Explain.ExplainHalt(report.Halt);
        string text = ex.Hint != null ? ex.Text + "\n<b>İpucu:</b> " + ex.Hint : ex.Text;
        if (report.Error != null)
            hud.ShowMessage("PYTHON HATASI", ex.Title, text, report.Error.Type + ": " + report.Error.Message, error: true);
        else
            hud.ShowMessage(report.Halt.Kind == "limit" ? "DURDURULDU" : "HENÜZ YOK", ex.Title, text, null, error: true);
    }

    // ---- Oyuncunun kodu ----

    // Kod "kod-<numara>", her satirin nasil yazildigi (satir turleri) "tur-<numara>" altinda saklanir.
    static string SaveKey(Level l) => "kod-" + l.Number;
    static string KindsKey(Level l) => "tur-" + l.Number;

    string SavedCode(Level l) => !shotsMode && PlayerPrefs.HasKey(SaveKey(l)) ? PlayerPrefs.GetString(SaveKey(l)) : null;
    string SavedKinds(Level l) => PlayerPrefs.HasKey(KindsKey(l)) ? PlayerPrefs.GetString(KindsKey(l)) : null;

    // Oyuncu kodu degistirdi: onceki calistirmanin sonucu (robotun yeri, kirmizi satir, hata kutusu) silinir, kod saklanir.
    void OnCodeChanged(string source)
    {
        if (running) return;
        if (done) ResetLevel();
        else hud.HideMessage();
        code = source;
        if (!shotsMode)
        {
            PlayerPrefs.SetString(SaveKey(level), code);
            PlayerPrefs.SetString(KindsKey(level), hud.CodeKinds);
            PlayerPrefs.Save();
        }
    }

    // Karttaki kodu disaridan degistirir (deneme goruntuleri icin; saklanmaz).
    void SetCode(string source)
    {
        ResetLevel();
        hud.LoadCode(source, null);
        code = hud.Code;
    }

    // ---- Bilgisayar klavyesi denetimi: MarsKod.exe -shots <klasor> -klavyedenetimi ----
    // Unity klavye harflerini Windows'un pencereye gonderdigi tus mesajlarindan okur; bu yuzden tuslari disaridan
    // scripts/klavye-denetimi.ps1 gonderir. Oyun koda tiklar, <klasor>/klavye-hazir.txt yazar, betik tuslari gonderip
    // klavye-bitti.txt yazar; oyun sonucu log'a ("KLAVYE DENETIMI:") yazar, klavye.png goruntusunu alir.
    IEnumerator KeyboardCheck(string dir)
    {
        LoadLevel(0);
        SetCode("move(East)");
        yield return new WaitForSeconds(0.3f);
        // Oyuncu gibi once koda fareyle tikla (arayuz klavyeyi en son tiklanan katmana gonderir), sonra imleci sona koy
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null)
        {
            var at = hud.CodeScreenPoint();
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at, buttons = 1 });
            yield return null;
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at, buttons = 0 });
            yield return null;
        }
        hud.FocusCode(code.Length, code.Length);
        yield return null;

        string ready = Path.Combine(dir, "klavye-hazir.txt"), finished = Path.Combine(dir, "klavye-bitti.txt");
        File.Delete(finished);
        File.WriteAllText(ready, "hazir");
        float until = Time.realtimeSinceStartup + 30f;
        while (!File.Exists(finished) && Time.realtimeSinceStartup < until) yield return null;
        yield return new WaitForSeconds(0.6f);

        // Betigin gonderdigi: Enter, "for i in range(2):", Enter (":" sonrasi 4 bosluk iceriden), "move(East))",
        // geri silme (fazla ")"), Home + geri silme (girinti bir kademe geri), Tab (geri iceri), End, Enter,
        // "# çğış {x}", sol ok, "y"
        const string expected = "move(East)\nfor i in range(2):\n    move(East)\n    # çğış {xy}";
        string got = hud.Code;
        Debug.Log("KLAVYE DENETIMI: " + (!File.Exists(finished) ? "betik tuslari gondermedi (30 sn)" : got == expected ? "TAMAM" : "FARKLI -> " + got.Replace("\n", "\n")));
        Cap(Path.Combine(dir, "klavye.png"));
        yield return new WaitForSeconds(0.3f);
        hud.StopEditing();
    }

    // ---- Kod klavyesi denetimi (-shots icinde): ekrandaki tuslara fareyle tiklanir ----
    // Oneri satiri, ⇧ (tek basis, cift basis kilit), Turkce İ, Enter girintisi ve "…" isaret sayfasi denenir.
    // Log'da "KOD KLAVYESI DENETIMI:" satiri; goruntuler klavye-basili.png (tus balonu), klavye-yazildi.png.
    IEnumerator KeyboardTapCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) { Debug.Log("KOD KLAVYESI DENETIMI: fare yok, atlandi"); yield break; }
        LoadLevel(levels.Count - 1);
        SetCode("");
        yield return new WaitForSeconds(0.3f);
        hud.FocusCode(0, 0);
        yield return new WaitForSeconds(0.4f); // klavye acilip yerlessin

        string missing = null;
        IEnumerator Tap(string key, bool hold = false)
        {
            var at = hud.KeyScreenPoint(key);
            if (!at.HasValue) { missing = missing ?? key; yield break; }
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at.Value, buttons = 1 });
            yield return null;
            yield return null;
            if (hold) { Cap(Path.Combine(dir, "klavye-basili.png")); yield return new WaitForSeconds(0.2f); }
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at.Value, buttons = 0 });
            yield return null;
            yield return null;
        }
        IEnumerator Taps(params string[] keys) { foreach (var k in keys) yield return Tap(k); }
        IEnumerator Word(string w) { foreach (char c in w) yield return Tap(c == ' ' ? "boşluk" : c.ToString()); }

        yield return Word("mo");
        yield return new WaitForSeconds(0.15f);           // oneri satiri guncellensin
        yield return Tap("öneri1");                        // move(
        yield return Taps("⇧", "e");                       // E
        yield return Word("ast");
        yield return Taps(")", "↵");
        yield return Word("co");
        yield return new WaitForSeconds(0.15f);
        yield return Taps("öneri1", ")", "↵");             // collect()
        yield return Word("for i in range");
        yield return Tap("(");
        yield return Tap("3", hold: true);                 // basili tus balonu goruntusu
        yield return Taps(")", ":", "↵");                  // ":" sonrasi 4 bosluk iceriden
        yield return Taps("…", "#", "…");                  // isaret sayfasi ve geri
        yield return Taps("⇧", "⇧", "a", "b", "⇧", "c");   // cift basis kilit: AB, sonra c
        yield return Taps("⇧", "i");                        // İ
        yield return Taps("x", "⌫");
        yield return new WaitForSeconds(0.4f);

        const string expected = "move(East)\ncollect()\nfor i in range(3):\n    #ABcİ";
        string got = hud.Code;
        Debug.Log("KOD KLAVYESI DENETIMI: " + (missing != null ? "tus bulunamadi: " + missing : got == expected ? "TAMAM" : "FARKLI -> " + got.Replace("\n", "\\n")));
        Cap(Path.Combine(dir, "klavye-yazildi.png"));
        yield return new WaitForSeconds(0.3f);
        hud.StopEditing();
    }

    // ---- Acemi paleti denetimi (-shots icinde): fareyle surukle-birak ----
    // Bolum 3 yalnizca paletle cozulur: for'u birak, dokunarak satir ekle, satir tasi, sola kaydirip donguden cikar,
    // kartin disina atip sil, sayiyi + ile 5'e cikar, sonra calistir. Log'da "PALET DENETIMI:" satiri;
    // goruntuler acemi-palet.png, acemi-surukle.png (hayalet + turuncu cizgi), acemi-sil.png (cop), acemi-sayi.png (-/+).
    IEnumerator PaletteCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) { Debug.Log("PALET DENETIMI: fare yok, atlandi"); yield break; }
        int idx = levels.FindIndex(l => l.Pieces.Contains("for i in range(3):"));
        if (idx < 0) { Debug.Log("PALET DENETIMI: for parcasi olan bolum yok, atlandi"); yield break; }
        LoadLevel(idx);
        hud.Tier = KeyboardTier.Acemi;
        SetCode("");
        yield return new WaitForSeconds(0.3f);
        hud.FocusCode(0, 0);
        yield return new WaitForSeconds(0.5f); // palet acilip yerlessin
        Cap(Path.Combine(dir, "acemi-palet.png"));
        yield return new WaitForSeconds(0.3f);

        string problem = null;
        void Mouse(Vector2 at, bool down) => UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse,
            new UnityEngine.InputSystem.LowLevel.MouseState { position = at, buttons = (ushort)(down ? 1 : 0) });
        IEnumerator Frames(int n) { for (int k = 0; k < n; k++) yield return null; }
        IEnumerator Click(Vector2? at, string what)
        {
            if (!at.HasValue) { problem = problem ?? what + " bulunamadi"; yield break; }
            Mouse(at.Value, true); yield return Frames(2);
            Mouse(at.Value, false); yield return Frames(2);
        }
        IEnumerator Drag(Vector2? from, Vector2 to, string what, string shot = null)
        {
            if (!from.HasValue) { problem = problem ?? what + " bulunamadi"; yield break; }
            Mouse(from.Value, true); yield return Frames(2);
            for (int k = 1; k <= 10; k++) { Mouse(Vector2.Lerp(from.Value, to, k / 10f), true); yield return Frames(1); }
            yield return Frames(3);
            if (shot != null) { Cap(Path.Combine(dir, shot)); yield return new WaitForSeconds(0.2f); }
            Mouse(to, false); yield return Frames(3);
        }
        void Expect(string step, string want)
        {
            if (problem == null && hud.Code != want) problem = step + ": " + hud.Code.Replace("\n", "\\n");
        }

        yield return Drag(hud.PieceScreenPoint("for i in range(3):"), hud.CodeScreenPoint(), "for parcasi");
        Expect("for birakildi", "for i in range(3):");
        yield return Click(hud.PieceScreenPoint("collect()"), "collect parcasi");
        yield return Click(hud.PieceScreenPoint("move(East)"), "move parcasi");
        Expect("dokunarak eklendi", "for i in range(3):\n    collect()\n    move(East)");
        yield return Drag(hud.CodeCharScreenPoint(2, 6), hud.CodeGapScreenPoint(1, 6), "3. satir", "acemi-surukle.png");
        Expect("satir tasindi", "for i in range(3):\n    move(East)\n    collect()");
        yield return Click(hud.PieceScreenPoint("move(North)"), "move(North) parcasi");
        yield return Drag(hud.CodeCharScreenPoint(3, 6), hud.CodeGapScreenPoint(4, 1), "4. satir"); // 5 harf sola: bir kademe disari
        Expect("donguden cikti", "for i in range(3):\n    move(East)\n    collect()\nmove(North)");
        yield return Drag(hud.CodeCharScreenPoint(3, 2), new Vector2(Screen.width * 0.5f, Screen.height * 0.75f), "4. satir", "acemi-sil.png");
        Expect("satir silindi", "for i in range(3):\n    move(East)\n    collect()");
        yield return Click(hud.CodeCharScreenPoint(0, 15), "sayi");
        yield return new WaitForSeconds(0.2f);
        yield return Click(hud.StepperScreenPoint(+1), "+ dugmesi");
        yield return Click(hud.StepperScreenPoint(+1), "+ dugmesi");
        yield return new WaitForSeconds(0.2f);
        Cap(Path.Combine(dir, "acemi-sayi.png"));
        yield return new WaitForSeconds(0.3f);
        Expect("sayi degisti", "for i in range(5):\n    move(East)\n    collect()");
        // Sil dugmesi: dokununca son satir gider; koddan surukleyip ustune birakinca o satir gider. Ikisinde de
        // paletten ayni satiri yeniden ekleyip kodu eski hâline getirir (asagidaki cozum calissin).
        yield return Click(hud.TrashScreenPoint(), "Sil dugmesi");
        Expect("Sil dokunusu son satiri sildi", "for i in range(5):\n    move(East)");
        yield return Click(hud.PieceScreenPoint("collect()"), "collect parcasi");
        Expect("silinen satir geri eklendi", "for i in range(5):\n    move(East)\n    collect()");
        yield return Drag(hud.CodeCharScreenPoint(2, 6), hud.TrashScreenPoint() ?? Vector2.zero, "3. satir", "acemi-sil-dugmesi.png");
        Expect("satir Sil dugmesine birakildi", "for i in range(5):\n    move(East)");
        yield return Click(hud.PieceScreenPoint("collect()"), "collect parcasi");
        Expect("silinen satir geri eklendi (2)", "for i in range(5):\n    move(East)\n    collect()");
        if (problem == null && hud.CodeKinds != "DDD") problem = "satir turleri " + hud.CodeKinds + " (DDD olmali)";

        hud.StopEditing();
        OnRun();
        while (running) yield return null;
        if (problem == null && !complete) problem = "cozum bolumu bitirmedi";
        Debug.Log("PALET DENETIMI: " + (problem ?? "TAMAM"));
        yield return new WaitForSeconds(0.4f);
        Cap(Path.Combine(dir, "acemi-bitti.png"));
        yield return new WaitForSeconds(0.3f);
    }

    // ---- Sinav denetimi (-shots icinde): sinavli bir bolumu cozup "Sonraki bolum"e basar, sinavi dogru cevaplarla gecer ----
    // Normalde -shots'un bolum-bolum taramasi sinavi atlar (suppressQuiz); burada gecici acilir. Log'da "SINAV DENETIMI:" satiri.
    IEnumerator QuizCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) { Debug.Log("SINAV DENETIMI: fare yok, atlandi"); yield break; }
        int idx = levels.FindIndex(l => quizzes.ContainsKey(l.Number));
        if (idx < 0) { Debug.Log("SINAV DENETIMI: sinavli bolum yok, atlandi"); yield break; }
        int quizLevel = levels[idx].Number;
        quizzesPassed.Remove(quizLevel); // onceki denemeden kalmasin
        suppressQuiz = false;
        LoadLevel(idx);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.5f);
        yield return MouseClick(mouse, hud.RunScreenPoint()); // Calistir
        yield return new WaitForSeconds(0.15f);
        if (!running) yield return MouseClick(mouse, hud.RunScreenPoint()); // ilk tiklama bazen pencere odagini kacirir
        while (running) yield return null;
        string problem = !complete ? "cozum bolumu bitirmedi" : null;
        yield return new WaitForSeconds(0.3f);
        yield return MouseClick(mouse, hud.RunScreenPoint()); // Sonraki bolum -> sinavi acar
        yield return new WaitForSeconds(0.15f);
        if (problem == null && !hud.QuizOpen) yield return MouseClick(mouse, hud.RunScreenPoint());
        yield return new WaitForSeconds(0.3f);

        if (problem == null && !hud.QuizOpen) problem = "sinav acilmadi";
        var quiz = quizzes[quizLevel];
        for (int i = 0; problem == null && i < quiz.Questions.Count; i++)
        {
            Cap(Path.Combine(dir, "sinav-" + (i + 1) + ".png"));
            var at = hud.QuizChoiceScreenPoint(quiz.Questions[i].Correct);
            if (!at.HasValue) { problem = (i + 1) + ". soru gorunmuyor"; break; }
            yield return MouseClick(mouse, at.Value);
            yield return new WaitForSeconds(0.2f);
            var cont = hud.QuizContinueScreenPoint();
            if (!cont.HasValue) { problem = (i + 1) + ". soruda Devam cikmadi"; break; }
            yield return MouseClick(mouse, cont.Value);
            yield return new WaitForSeconds(0.3f);
        }
        if (problem == null && hud.QuizOpen) problem = "sinav kapanmadi";
        if (problem == null && !quizzesPassed.Contains(quizLevel)) problem = "gecilen sinav kaydedilmedi";
        Debug.Log("SINAV DENETIMI: " + (problem ?? "TAMAM"));
        suppressQuiz = true;
        yield return new WaitForSeconds(0.3f);
    }

    // ---- Bolum secme denetimi (-shots icinde): ekran acilir, 2. bolumun satirina fareyle tiklanir ----
    // Bolumler once cozuldugu icin satirlarda ✓ ve XP gorunur. Log'da "BOLUM SECME DENETIMI:" satiri; goruntu bolum-secme.png.
    IEnumerator LevelSelectCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        LoadLevel(0);

        // Kilit gorunumu (gercek ilerlemeden bagimsiz, yalnizca goruntu icin): sahte bos ilerlemeyle bir kez acar.
        // shotsMode'da kilit normalde kapali (bolum-bolum tarama serbest gezsin diye); burada gecici acilir.
        var xpBefore = xp;
        forceLocks = true; xp = new Xp();
        hud.ShowLevelSelect(LevelEntries());
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "bolum-secme-kilitli.png"));
        yield return new WaitForSeconds(0.3f);
        hud.HideLevelSelect();
        forceLocks = false; xp = xpBefore;

        hud.ShowLevelSelect(LevelEntries());
        yield return new WaitForSeconds(0.5f);
        Cap(Path.Combine(dir, "bolum-secme.png"));
        yield return new WaitForSeconds(0.3f);
        var at = hud.LevelRowScreenPoint(2);
        string problem = mouse == null ? "fare yok" : !at.HasValue ? "2. bolumun satiri bulunamadi" : null;
        if (problem == null)
        {
            yield return MouseClick(mouse, at.Value);
            yield return new WaitForSeconds(0.4f);
            if (level.Number != 2) problem = "secilen bolum " + level.Number + " (2 olmali)";
            else if (hud.LevelRowScreenPoint(2).HasValue) problem = "ekran kapanmadi";
        }
        Debug.Log("BOLUM SECME DENETIMI: " + (problem ?? "TAMAM"));
        hud.HideLevelSelect();
    }

    // Denetimler sahte fare olaylari gonderir; girdi sistemi normalde pencere odakta degilken fareyi kapatir.
    // Paket arka planda baslatilinca Windows pencereyi her zaman one almadigi icin denetimler rastgele basarisiz oluyordu.
    static void ListenWhileUnfocused()
    {
        UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
    }

    // Deneme icin: ekrandaki noktaya fareyle bir kez tiklar (bas, iki kare bekle, birak)
    static IEnumerator MouseClick(UnityEngine.InputSystem.Mouse mouse, Vector2 at)
    {
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at, buttons = 1 });
        yield return null; yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at, buttons = 0 });
        yield return null;
    }

    // ---- Adim adim denetimi (-shots icinde): Bolum 9'un cozumunde ⏭'a uc kez, sonra Devam'a fareyle tiklanir ----
    // Uc adimda for, move, collect calisir: robot bir kare ilerler (ve yalnizca bir kare), satirin yaninda "i = 0" yazar ve kod bekler;
    // Devam kalanini bitirir. Log'da "ADIM ADIM DENETIMI:" satiri; goruntu adim-adim.png.
    const int StepCheckLevel = 9;
    IEnumerator StepCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) { Debug.Log("ADIM ADIM DENETIMI: fare yok, atlandi"); yield break; }
        // cozumu for / move / collect ile baslayan sabit bir bolum (son bolum olmaz: Bolge 6'dan beri cozumler def ile baslar)
        LoadLevel(levels.FindIndex(l => l.Number == StepCheckLevel));
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.4f);
        for (int k = 0; k < 3; k++)
        {
            yield return MouseClick(mouse, hud.StepScreenPoint());
            yield return new WaitForSeconds(1.2f); // satirin animasyonu bitsin
        }
        Cap(Path.Combine(dir, "adim-adim.png"));
        yield return new WaitForSeconds(0.3f);
        var start = level.Robot;
        float oneCell = Vector3.Distance(Pos(start), Pos(new Cell(start.Col + 1, start.Row))); // hangi yone gittigi onemli degil
        string problem = !running || !stepMode ? "kod adim adim beklemiyor"
            : Mathf.Abs(Vector3.Distance(robot.transform.localPosition, Pos(start)) - oneCell) > 0.05f ? "robot tam bir kare ilerlemedi"
            : null;
        yield return MouseClick(mouse, hud.RunScreenPoint()); // Devam
        while (running) yield return null;
        if (problem == null && !complete) problem = "Devam sonrasi bolum bitmedi";
        Debug.Log("ADIM ADIM DENETIMI: " + (problem ?? "TAMAM"));
        yield return new WaitForSeconds(0.3f);
    }

    // ---- Sozluk denetimi (-shots icinde): son bolumde kitap dugmesine, sonra "range" kisayoluna fareyle tiklanir ----
    // Sozluk acilmali ve range karti (en sondaki) tamamen gorunur olmali. Log'da "SOZLUK DENETIMI:" satiri;
    // goruntuler sozluk.png (acilis, for/range YENI), sozluk-range.png (kisayoldan sonra).
    IEnumerator GlossaryCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) { Debug.Log("SOZLUK DENETIMI: fare yok, atlandi"); yield break; }
        LoadLevel(levels.Count - 1);
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "sozluk-dugmesi.png"));
        yield return new WaitForSeconds(0.3f);
        yield return MouseClick(mouse, hud.GlossaryButtonScreenPoint());
        yield return new WaitForSeconds(0.5f);
        Cap(Path.Combine(dir, "sozluk.png"));
        yield return new WaitForSeconds(0.3f);
        string problem = !hud.GlossaryOpen ? "kitap dugmesi sozlugu acmadi" : null;
        if (problem == null)
        {
            var at = hud.GlossaryShortcutScreenPoint("range");
            if (!at.HasValue) problem = "range kisayolu bulunamadi";
            else
            {
                yield return MouseClick(mouse, at.Value);
                yield return new WaitForSeconds(0.5f);
                Cap(Path.Combine(dir, "sozluk-range.png"));
                yield return new WaitForSeconds(0.3f);
                if (!hud.GlossaryCardInView("range")) problem = "kisayol range kartini tam gostermedi";
            }
        }
        Debug.Log("SOZLUK DENETIMI: " + (problem ?? "TAMAM"));
        hud.HideGlossary();
    }

    // ---- Ipucu denetimi (-shots icinde): uc ipucu olan son bolumde ampule fareyle basilir ----
    // 1. -> 2. -> 3. ipucu, bir daha basinca kapanir; tekrar acilinca 1.'den baslar ve kayitta en yuksek sira (3) durur; × balonu kapatir.
    // (Sinav bolumlerinde tek ipucu var; onlar atlanir.) Log'da "IPUCU DENETIMI:" satiri; goruntuler ipucu-1.png ve ipucu-3.png.
    IEnumerator HintCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        int idx = levels.FindLastIndex(l => l.Hints.Count >= 3);
        if (idx < 0) { Debug.Log("IPUCU DENETIMI: uc ipucu olan bolum yok, atlandi"); yield break; }
        LoadLevel(idx);
        yield return new WaitForSeconds(0.3f);
        string problem = mouse == null ? "fare yok" : null;
        int total = level.Hints.Count;
        for (int k = 1; k <= total + 1 && problem == null; k++)
        {
            yield return MouseClick(mouse, hud.HintButtonScreenPoint());
            yield return new WaitForSeconds(0.4f);
            if (k > total) { if (hud.HintOpen) problem = "son basista balon kapanmadi"; continue; }
            if (!hud.HintOpen || hintView.Current != k) problem = k + ". basista " + hintView.Current + ". ipucu acik (" + k + " olmali)";
            if (k == 1) Cap(Path.Combine(dir, "ipucu-1.png"));
            if (k == total) Cap(Path.Combine(dir, "ipucu-3.png"));
        }
        if (problem == null && hints.Shown(level.Number) != total) problem = "kayitli en yuksek ipucu " + hints.Shown(level.Number) + " (" + total + " olmali)";
        if (problem == null)
        {
            yield return MouseClick(mouse, hud.HintButtonScreenPoint());
            yield return new WaitForSeconds(0.3f);
            if (!hud.HintOpen || hintView.Current != 1) problem = "yeniden acilinca 1. ipucundan baslamadi";
        }
        if (problem == null)
        {
            var x = hud.HintCloseScreenPoint();
            if (!x.HasValue) problem = "× dugmesi bulunamadi";
            else
            {
                yield return MouseClick(mouse, x.Value);
                yield return new WaitForSeconds(0.3f);
                if (hud.HintOpen) problem = "× balonu kapatmadi";
            }
        }
        if (problem == null && hints.Shown(level.Number) != total) problem = "yeniden acma kaydi dusurdu";
        Debug.Log("IPUCU DENETIMI: " + (problem ?? "TAMAM"));
        hud.HideHint();
    }

    // ---- Kontrol icin ekran goruntusu: MarsKod.exe -shots <klasor> ----

    static void Cap(string path) => ScreenCapture.CaptureScreenshot(path, 2);

    // Bolum 50 kapanisi (Perde 1 finali) goruntuleri: dogru cozum calisir, kapanis bitene kadar her 2 saniyede bir
    // goruntu; once de Bolum 45 bitmisken odev listesindeki kilitli Bolum 50 karti.
    IEnumerator FinaleShots(string dir)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2f);

        // Bolum 52 basindaki Kayit 2 (oyunda bir kez gelir; burada dogrudan gosterilir)
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.RecordingLevel));
        int kayit2Before = PlayerPrefs.GetInt(Kayit2Anahtari, 0);
        StartCoroutine(PlayCanyonRecording());
        yield return new WaitForSeconds(9f);
        Cap(Path.Combine(dir, "kayit-2.png"));
        while (!hud.RecordingFinished) yield return null;
        yield return new WaitForSeconds(2.6f);
        Cap(Path.Combine(dir, "bolum-52-giris.png"));
        yield return new WaitForSeconds(0.4f);   // goruntu kare sonunda alinir: sonraki ekran onu ortmesin
        PlayerPrefs.SetInt(Kayit2Anahtari, kayit2Before);   // deneme, oyuncunun ilk dinleyisini engellemesin

        var xpBefore = xp;
        forceLocks = true; xp = new Xp();
        for (int n = 1; n <= 48; n++) xp.Award(n, LineKind.Dugme);
        LoadLevel(levels.FindIndex(l => l.Number == 49));
        hud.ShowLevelSelect(LevelEntries());
        yield return new WaitForSeconds(0.6f);
        Cap(Path.Combine(dir, "final-0-kilitli-kart.png"));
        yield return new WaitForSeconds(0.3f);
        hud.HideLevelSelect();
        forceLocks = false; xp = xpBefore;

        LoadLevel(levels.FindIndex(l => l.Number == AntennaTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.5f);
        OnRun();
        int shot = 1;
        while (running)
        {
            Cap(Path.Combine(dir, "final-" + shot++.ToString("00") + ".png"));
            yield return new WaitForSeconds(2f);
        }
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "final-son.png"));   // "Devam" dugmesi gorunur olmali
        yield return new WaitForSeconds(0.4f);

        // Perde 2'ye gecis: Devam -> sinav-50 ("KIVILCIM · KENDİNİ DENETLİYOR")
        suppressQuiz = false;
        quizzesPassed.Remove(level.Number);
        OnRun();
        yield return new WaitForSeconds(0.8f);
        Cap(Path.Combine(dir, "final-sinav.png"));
        Debug.Log("FINAL DENETIMI: " + (hud.QuizOpen ? "TAMAM" : "sinav acilmadi"));
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }

    IEnumerator Shots(string dir)
    {
        Directory.CreateDirectory(dir);
        int firstLevel = levelIndex;   // -bolum N verildiyse bolum goruntuleri N'den baslar (tumu uzun suruyor)
        yield return new WaitForSeconds(2.5f);

        // Acilis sahnesi (gorsel kontrol icin; oyunun kendisi bunu yalnizca ilk acilista, bu bayraklar olmadan gosterir)
        bool openingDone = false;
        hud.ShowOpening(() => openingDone = true);
        yield return new WaitForSeconds(0.5f);
        Cap(Path.Combine(dir, "acilis-1.png"));
        yield return new WaitForSeconds(1.5f);
        Cap(Path.Combine(dir, "acilis-2.png"));
        while (!openingDone) yield return null; // dokunmadan dogal akisin sonunu bekler

        // Her bolum: bekleme, yolun ortasi, bitis (dogru cozumle)
        for (int i = firstLevel; i < levels.Count; i++)
        {
            string b = "b" + levels[i].Number + "-";
            LoadLevel(i);
            yield return new WaitForSeconds(0.8f);
            Cap(Path.Combine(dir, b + "0-baslangic-kodu.png"));
            yield return new WaitForSeconds(0.3f);
            SetCode(levels[i].Solution);
            yield return new WaitForSeconds(0.5f);
            Cap(Path.Combine(dir, b + "1-bekleme.png"));
            yield return new WaitForSeconds(0.3f);
            OnRun();
            yield return new WaitForSeconds(2.6f);
            Cap(Path.Combine(dir, b + "2-yolda.png"));
            while (running) yield return null;
            yield return new WaitForSeconds(0.4f);
            Cap(Path.Combine(dir, b + "3-bitti.png"));
            yield return new WaitForSeconds(0.3f);
        }
        // Kirmizi kristal: kor kor toplayan baslangic kodu robotu durdurur (OYUN KURALI kutusu)
        int crystalLevel = levels.FindIndex(l => l.Crystals.Count > 0);
        if (crystalLevel >= 0)
        {
            LoadLevel(crystalLevel);
            yield return new WaitForSeconds(0.5f);
            OnRun();
            while (running) yield return null;
            yield return new WaitForSeconds(0.6f);
            Cap(Path.Combine(dir, "kristal-durdu.png"));
            yield return new WaitForSeconds(0.3f);
        }
        yield return LevelSelectCheck(dir);
        yield return HintCheck(dir);
        yield return StepCheck(dir);
        yield return GlossaryCheck(dir);

        // Kod yazarken: imlec kodun sonunda
        LoadLevel(levels.Count - 1);
        yield return new WaitForSeconds(0.3f);
        hud.FocusCode(code.Length, code.Length);
        yield return new WaitForSeconds(1.1f); // imlec yanip soner: saniyenin ilk yarisinda gorunur
        Cap(Path.Combine(dir, "yaziyor.png"));
        yield return new WaitForSeconds(0.3f);
        int east = code.IndexOf("East", System.StringComparison.Ordinal);
        hud.FocusCode(east, east + 4); // "East" secili: secim yaziyla hizali mi
        yield return new WaitForSeconds(0.5f);
        Cap(Path.Combine(dir, "yaziyor-secim.png"));
        yield return new WaitForSeconds(0.3f);
        // Uzun kod: kod karti yarim ekrani gecmesin, imlec (son satir, uzun satirin sonu) gorunsun diye kendiliginden kaysin
        var longCode = new System.Text.StringBuilder("# Uzun bir kod: kart kaydirilabilir olmali, alan cok kuculmemeli\n");
        for (int k = 0; k < 12; k++) longCode.Append(k % 3 == 2 ? "collect()\n" : "move(East)\n");
        longCode.Append("move(East)  # bu satir cok uzun, kartin disina tasmamali; yatay kaydirilir");
        SetCode(longCode.ToString());
        yield return new WaitForSeconds(0.4f);
        hud.FocusCode(code.Length, code.Length);
        yield return new WaitForSeconds(1.1f);
        Cap(Path.Combine(dir, "uzun-kod.png"));
        yield return new WaitForSeconds(0.3f);
        hud.StopEditing();

        // Kademe kutusu: kod kartinin sag ustundeki dugmeye dokununca acilan liste (Gorev 8)
        LoadLevel(0);
        yield return new WaitForSeconds(0.3f);
        hud.ShowTierMenu(true);
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "kademe-kutusu.png"));
        yield return new WaitForSeconds(0.2f);
        hud.ShowTierMenu(false);
        yield return new WaitForSeconds(0.2f);
        hud.ShowTierMenu(true, true);
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "kademe-kisayol.png"));
        yield return new WaitForSeconds(0.2f);
        hud.ShowTierMenu(false);

        // Kod klavyesi: Orta (oneri satiri "mo" icin move), ikinci isaret sayfasi, Usta (oneri satiri yok)
        LoadLevel(levels.Count - 1);
        SetCode("for i in range(5):\n    mo");
        yield return new WaitForSeconds(0.3f);
        hud.FocusCode(code.Length, code.Length);
        yield return new WaitForSeconds(0.6f);
        Cap(Path.Combine(dir, "klavye-orta.png"));
        yield return new WaitForSeconds(0.3f);
        hud.KeyboardMore(true);
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "klavye-isaretler.png"));
        yield return new WaitForSeconds(0.3f);
        hud.KeyboardMore(false);
        var tierBefore = hud.Tier;
        hud.Tier = KeyboardTier.Usta;
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "klavye-usta.png"));
        yield return new WaitForSeconds(0.3f);
        // Klavye + ipucu: sahne en cok kuculdugunde zeminin kenari gorunmemeli, cevre bos durmamali
        PressHint();
        yield return new WaitForSeconds(0.8f);
        Cap(Path.Combine(dir, "klavye-ipucu.png"));
        yield return new WaitForSeconds(0.2f);
        hud.HideHint();
        int threeHints = levels.FindLastIndex(l => l.Hints.Count >= 3);
        if (threeHints >= 0)
        {
            LoadLevel(threeHints);
            hud.FocusCode(code.Length, code.Length);
            PressHint(); PressHint(); // 2. ipucu (bolum 4'te en uzunu)
            yield return new WaitForSeconds(0.8f);
            Cap(Path.Combine(dir, "klavye-ipucu2.png"));
            yield return new WaitForSeconds(0.2f);
            PressHint();
            yield return new WaitForSeconds(0.8f);
            Cap(Path.Combine(dir, "klavye-ipucu3.png"));
            yield return new WaitForSeconds(0.2f);
            hud.HideHint();
            LoadLevel(levels.Count - 1);
            SetCode("for i in range(5):\n    mo");
            hud.FocusCode(code.Length, code.Length);
            yield return new WaitForSeconds(0.3f);
        }
        hud.Tier = KeyboardTier.Orta;
        hud.StopEditing();
        yield return KeyboardTapCheck(dir);
        yield return PaletteCheck(dir);
        yield return QuizCheck(dir);
        hud.Tier = tierBefore;

        if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-klavyedenetimi") >= 0) yield return KeyboardCheck(dir);
        LoadLevel(levels.Count - 1);
        yield return new WaitForSeconds(0.3f);
        starsTarget = 0f; hud.SetStars(false);
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "yildizsiz.png"));
        starsTarget = 1f; hud.SetStars(true);

        // Hatali kodlar (bolum numarasi, ad, kod): Python hatasi, oyun kurallari, eksik gorev, yazim hatasi
        var cases = new[]
        {
            (3, "h1-python-hatasi", "for i in range(5):\n    move(East)\n    colect()\n"),
            (3, "h2-alan-siniri", "for i in range(9):\n    move(East)\n"),
            (3, "h3-eksik-buz", "move(East)\ncollect()\nmove(East)\n"),
            (3, "h4-yazim-hatasi", "for i in range(5)\n    move(East)\n"),
            (2, "h5-kaya", "move(East)\nmove(East)\ncollect()\nmove(East)\n"),
            (1, "h6-kilitli-komut", "move(East)\ncollect()\n"),
            (1, "h7-hedefte-degil", "move(East)\nmove(East)\nmove(East)\n"),
            (3, "h8-govde-girintisiz", "for i in range(5):\nmove(East)\ncollect()\n"),
            (3, "h9-sebepsiz-iceride", "move(East)\n    collect()\n"),
        };
        foreach (var (number, name, source) in cases)
        {
            int idx = levels.FindIndex(l => l.Number == number);
            if (idx < 0) continue;
            LoadLevel(idx);
            SetCode(source);
            yield return new WaitForSeconds(0.6f);
            OnRun();
            while (running) yield return null;
            yield return new WaitForSeconds(0.6f);
            Cap(Path.Combine(dir, name + ".png"));
            yield return new WaitForSeconds(0.3f);
        }
        LoadLevel(0);
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
