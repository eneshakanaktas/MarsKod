using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarsKod.Dunya;
using MarsKod.Motor;
using UnityEngine;
using UnityEngine.Rendering;

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
    const float StartYaw = 125f;


    Camera cam;
    Material backdrop;
    Transform world;
    Robot robot;
    readonly List<Ice> ices = new List<Ice>();
    Target target;
    Transform levelRoot;
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
    Coroutine program;
    bool running, done, complete;
    // Adim adim modu: her satirdan sonra ⏭ basisini bekler (stepRequested: bir satir daha calissin)
    bool stepMode, stepRequested;
    float stars = 1f, starsTarget = 1f;
    // Kademe basina XP; kademe kutusundaki ✓ ve bolum sonu XP (Gorev 9) bunu kullanir.
    Xp xp;
    // Bolum basina kac ipucu acildi (ilk insan testinde uc ipucu da bedava)
    HintLog hints;
    // Kod sozlugu (Resources/Sozluk/sozluk.json); dosya okunamazsa bos
    Glossary glossary = new Glossary();

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
        Shader.SetGlobalVector("_Sun", new Vector4(0.02f, -0.068f, 0f, 0f));
        Shader.SetGlobalVector("_Focus", new Vector4(0.5f, 0.56f, 0f, 0f));
        Shader.SetGlobalFloat("_StarsOn", 1f);
        // Deneme secenekleri (telefonda: adb ... -e unity "-kalite 1 -cizgisiz")
        var a = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] == "-kalite" && i + 1 < a.Length) QualitySettings.SetQualityLevel(int.Parse(a[i + 1]), true);
            if (a[i] == "-cizgisiz") Parts.NoOutline = true;
            if (a[i] == "-shots") shotsMode = true;
        }
        world = new GameObject("World").transform;

        SetupCamera();
        SetupLight();
        BuildBoard();

        robot = Robot.Create(world);

        hud = gameObject.AddComponent<Hud>();
        hud.Build();
        hud.RunPressed += OnRun;
        hud.StepPressed += OnStep;
        hud.CodeChanged += OnCodeChanged;
        hud.ResetPressed += ResetLevel;
        // Sol ustteki dugme bolum secme ekranini acar
        hud.MenuPressed += () => hud.ShowLevelSelect(LevelEntries());
        hud.LevelPicked += PickLevel;
        hud.GlossaryPressed += () => hud.ShowGlossary(GlossaryEntries());
        hud.StarsToggled += () => { starsTarget = starsTarget > 0.5f ? 0f : 1f; hud.SetStars(starsTarget > 0.5f); };

        xp = Xp.Load(PlayerPrefs.GetString("xp", ""));
        hud.SetTotalXp(xp.Total);
        hud.TierEarned = t => level != null && xp.Has(level.Number, (LineKind)((int)t + 1));
        hud.TierChanged += SaveTier;

        hints = HintLog.Load(shotsMode ? "" : PlayerPrefs.GetString("ipucu", ""));
        hud.MoreHintPressed += RevealHint;

        // Kod yazma kademesi: kayitli tercih kalici (kademe kutusu), -kademe deneme secenegi onune gecer.
        if (!shotsMode && System.Enum.TryParse(PlayerPrefs.GetString("kademe", ""), true, out KeyboardTier savedTier))
            hud.Tier = savedTier;
        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == "-kademe" && System.Enum.TryParse(a[i + 1], true, out KeyboardTier t)) hud.Tier = t;

        LoadLevels();
        LoadGlossary();
        int start = 1;
        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == "-bolum") int.TryParse(a[i + 1], out start);
        LoadLevel(Mathf.Clamp(levels.FindIndex(l => l.Number == start), 0, levels.Count - 1));

        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == "-shots") { shotsMode = true; LoadLevel(levelIndex); StartCoroutine(Shots(a[i + 1])); }
    }

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
    }

    void SetupLight()
    {
        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        // safak oncesi: gunes yok; soguk, los bir gok isigi (robot okunabilsin diye yumusak golge verir)
        sun.color = Mats.Hex("#B9B8D8");
        sun.intensity = 0.55f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.6f;
        sun.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

        // Tepelerin ardindan dogmak uzere olan gunes: arkadan, alcaktan gelen sicak, golgesiz isik
        var dawn = new GameObject("Dawn").AddComponent<Light>();
        dawn.type = LightType.Directional;
        dawn.color = Mats.Hex("#FF9E66");
        dawn.intensity = 0.5f;
        dawn.shadows = LightShadows.None;
        dawn.transform.rotation = Quaternion.Euler(14f, 168f, 0f);
        // zemin cizimi (Ground.shader) safak isigini buradan okur
        Shader.SetGlobalVector("_DawnDir", dawn.transform.forward);
        Shader.SetGlobalVector("_DawnColor", dawn.color.linear * dawn.intensity);

        // Yumusak ortam isigi: ustten serin gokyuzu, yanlardan sicak
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Mats.Hex("#5E5B7C");
        RenderSettings.ambientEquatorColor = Mats.Hex("#4C4352");
        RenderSettings.ambientGroundColor = Mats.Hex("#241D25");
        var sh = new SphericalHarmonicsL2();
        sh.AddAmbientLight(Mats.Hex("#524C60").linear);
        sh.AddDirectionalLight(Vector3.up, Mats.Hex("#4A4A6A").linear, 0.9f);
        RenderSettings.ambientProbe = sh;
        RenderSettings.skybox = null;
        RenderSettings.fog = false;
    }

    // Oyun alani: cevresiyle ayni hizada, Mars yuzeyine gomulu bir bolge (godottaslak2'deki gibi).
    // Zemin tek parca ve ufka kadar uzanir: alan duz, disari hafif inisli cikisli; kayalar ve kraterler uzaklastikca seyrelir,
    // uzakta zemin arka plana (tepeler, koloni) dikissiz karisir. Kare sinirlari zeminde ince, soluk cizgiler.
    const float AreaHalfX = Cols * 0.5f, AreaHalfZ = Rows * 0.5f;
    const float Bend = 0.03f;

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
        float bz = Mathf.Max(z - (AreaHalfZ + 0.8f), 0f), bx = Mathf.Max(Mathf.Abs(x) - (AreaHalfX + 1.5f), 0f);
        return h - Bend * (bz * bz + 0.5f * bx * bx);
    }

    void BuildBoard()
    {
        var board = Parts.Empty("Board", world);
        var rng = new System.Random(7);
        float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        // Arazi agi
        // z0: kod karti uzayip kamera geri cekilince ekranin alti da zemin gorsun (kartin kenarlarinda siyah kalmasin)
        const float x0 = -16f, x1 = 16f, z0 = -30f, z1 = 24f, step = 0.4f;
        int nx = Mathf.RoundToInt((x1 - x0) / step) + 1, nz = Mathf.RoundToInt((z1 - z0) / step) + 1;
        var verts = new Vector3[nx * nz];
        for (int j = 0; j < nz; j++)
        for (int i = 0; i < nx; i++)
        {
            float x = x0 + i * step, z = z0 + j * step;
            verts[j * nx + i] = new Vector3(x, TerrainHeight(x, z), z);
        }
        var tris = new List<int>();
        for (int j = 0; j < nz - 1; j++)
        for (int i = 0; i < nx - 1; i++)
        {
            int a = j * nx + i, b = a + 1, c = a + nx + 1, d = a + nx;
            tris.Add(a); tris.Add(d); tris.Add(c); tris.Add(a); tris.Add(c); tris.Add(b);
        }
        var mesh = new Mesh { name = "Terrain" };
        mesh.vertices = verts;
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        ground = Mats.Custom("Ground", "MarsKod/Ground");
        ground.SetVector("_Area", new Vector4(AreaHalfX, AreaHalfZ, 0, 0));
        ground.SetVector("_FogRange", new Vector4(9f, 19f, 0, 0));
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

        // Kayalar: alanin disina serpistirilmis, uzaklastikca seyrek. Alanin icindeki kayalar bolumden gelir (engel).
        var rockMat = Mats.Lit(Mats.Hex("#6B4034"), 0.15f);
        var rockDark = Mats.Lit(Mats.Hex("#553229"), 0.12f);
        for (int i = 0; i < 130; i++)
        {
            float x = R(-9f, 9f), z = R(-6f, 8.5f);
            if (Mathf.Abs(x) < AreaHalfX + 0.45f && Mathf.Abs(z) < AreaHalfZ + 0.45f) continue;
            float dist = Mathf.Max(Mathf.Abs(x) - AreaHalfX, Mathf.Abs(z) - AreaHalfZ);
            if (R(0f, 1f) < dist * 0.05f) continue; // uzakta daha seyrek
            float size = Mathf.Lerp(0.05f, 0.3f, Mathf.Pow(R(0f, 1f), 2.2f));
            var rk = Parts.Add("Rock", board, MeshFactory.Rock(size, i), i % 3 == 0 ? rockDark : rockMat,
                new Vector3(x, TerrainHeight(x, z) + size * 0.12f, z), outline: false);
            rk.localRotation = Quaternion.Euler(R(-10, 10), R(0, 360), R(-10, 10));
        }
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
        ices.Clear();
        for (int i = 0; i < level.Ices.Count; i++)
            ices.Add(Ice.Create(levelRoot, Pos(level.Ices[i]), i + 1));
        for (int i = 0; i < 6; i++)
        {
            var p = i < level.Ices.Count ? Pos(level.Ices[i]) : Vector3.zero;
            ground.SetVector("_Frost" + i, new Vector4(p.x, p.z, 0.45f, i < level.Ices.Count ? 1f : 0f));
        }
        var rockMat = Mats.Lit(Mats.Hex("#7A4A3B"), 0.15f);
        var rockDark = Mats.Lit(Mats.Hex("#5E3A2F"), 0.12f);
        for (int i = 0; i < level.Rocks.Count; i++)
        {
            // engel kaya: kareyi dolduran iri bir kaya + yaninda kucuk bir parca (kod bilmeyen de "buradan gecilmez" diye okusun)
            var p = Pos(level.Rocks[i]);
            var big = Parts.Add("Obstacle", levelRoot, MeshFactory.Rock(0.34f, 200 + i), rockMat, p + new Vector3(0f, 0.05f, 0f));
            big.localRotation = Quaternion.Euler(0, i * 71f + 20f, 0);
            var small = Parts.Add("Obstacle", levelRoot, MeshFactory.Rock(0.13f, 300 + i), rockDark, p + new Vector3(0.28f, 0.02f, -0.22f));
            small.localRotation = Quaternion.Euler(0, i * 37f, 0);
        }
        target = level.Target.HasValue ? Target.Create(levelRoot, Pos(level.Target.Value)) : null;

        robot.ResetTo(Pos(level.Robot), StartYaw);
        string saved = SavedCode(level);
        if (saved != null) hud.LoadCode(saved, SavedKinds(level));
        else hud.LoadStartCode(level.StartCode);
        code = hud.Code;
        hud.SetOpenWords(MarsKod.Dunya.Suggestions.OpenWords(levels, level.Number));
        hud.SetPieces(level.Pieces, Palette.NewPieces(levels, level.Number));
        hud.SetLevel(level.Number, level.Title, level.Goal, level.Ices.Count);
        hud.SetHints(level.Hints, hints.Shown(level.Number));
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
        // Alan banda tam oturana kadar kamerayi yaklastir/uzaklastir (ikiye bolerek arama)
        float lo = 3f, hi = 80f;
        for (int k = 0; k < 24; k++)
        {
            float mid = (lo + hi) * 0.5f;
            var e = Extent(mid);
            if (e.width <= needW && e.height <= needH) hi = mid; else lo = mid;
        }
        float d = hi;
        var ext = Extent(d);
        cam.nearClipPlane = Mathf.Max(0.3f, d - 10f);
        cam.farClipPlane = d + 45f;

        // Goruntuyu dikeyde kaydir: alan bandin ortasina gelsin (perspektif bozulmaz)
        var p = cam.projectionMatrix;
        p[1, 2] = -2f * ((bottom + top) * 0.5f - ext.center.y);
        cam.projectionMatrix = p;

        // Tepeler ve koloni zeminin gercekten bittigi cizgiye otursun: kivrik zeminin ekrandaki en ust noktasini bul
        float horizon = 0f;
        for (float z = AreaHalfZ; z < 24f; z += 0.25f)
        {
            var v = cam.WorldToViewportPoint(new Vector3(0f, TerrainHeight(0f, z), z));
            if (v.z > 0f) horizon = Mathf.Max(horizon, v.y);
        }
        Shader.SetGlobalFloat("_Horizon", Mathf.Clamp(horizon + 0.05f, top + 0.02f, 0.9f));

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
            LoadLevel(levelIndex + 1);
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
    List<LevelSelect.Entry> LevelEntries() => levels.Select(l => new LevelSelect.Entry
    {
        Number = l.Number, Title = l.Title, Goal = l.Goal,
        Xp = xp.LevelTotal(l.Number), Current = l == level,
    }).ToList();

    void PickLevel(int number)
    {
        int idx = levels.FindIndex(l => l.Number == number);
        if (idx >= 0) LoadLevel(idx);
    }

    // "Bir ipucu daha": siradaki ipucu acilir ve bolum tekrar acilinca da gorunur (deneme kosusunda kaydedilmez)
    void RevealHint()
    {
        hud.SetHints(level.Hints, hints.Reveal(level.Number, level.Hints.Count));
        if (shotsMode) return;
        PlayerPrefs.SetString("ipucu", hints.Save());
        PlayerPrefs.Save();
    }

    void SaveTier(KeyboardTier t)
    {
        PlayerPrefs.SetString("kademe", t.ToString());
        PlayerPrefs.Save();
    }

    void ResetLevel()
    {
        if (program != null) StopCoroutine(program);
        program = null;
        running = false; done = false; complete = false;
        stepMode = false; stepRequested = false;
        robot.ResetTo(Pos(level.Robot), StartYaw);
        foreach (var ice in ices) ice.Restore();
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
        var report = ProgramRun.Execute(code, world);

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
            hud.SetDone(HasNext, xpLine, xp.NextBetterText(level.Number, kind));
            done = true;
            yield return robot.Celebrate();
        }
        else
        {
            hud.SetActiveLine(-1);
            if (world.IceLeft > 0)
                hud.ShowMessage("GÖREV", "Kod bitti, buzlar bitmedi",
                    "Kodun sonuna kadar çalıştı ama " + world.IceLeft + " buz daha toplanmayı bekliyor. Robot yalnızca kodda yazanı yapar: eksik adımı bul.",
                    null, error: false);
            else
                hud.ShowMessage("GÖREV", "Kod bitti, robot hedefte değil",
                    "Kodun sonuna kadar çalıştı ama robot işaretli kareye varmadı. Kod bittiğinde robot hedef karede durmalı: yolu adım adım say.",
                    null, error: false);
            hud.SetRunning(false);
            done = true;
        }
        running = false;
        program = null;
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
                yield return Face(m.Direction);
                yield return robot.MoveTo(Pos(m.To));
                break;
            case Blocked b:
                yield return Face(b.Direction);
                yield return robot.Bump();
                break;
            case Collected c:
                StartCoroutine(robot.Collect());
                yield return Tween.Wait(0.12f);
                if (c.IceIndex >= 0)
                {
                    ices[c.IceIndex].Pop();
                    hud.SetCollected(c.Total);
                }
                yield return Tween.Wait(c.IceIndex >= 0 ? 0.4f : 0.3f);
                break;
        }
    }

    // Durma sebebini Turkce anlatir; Python hatasinda Python'un kendi mesaji da altta gorunur.
    void ShowStop(RunReport report)
    {
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

    // ---- Bolum secme denetimi (-shots icinde): ekran acilir, 2. bolumun satirina fareyle tiklanir ----
    // Bolumler once cozuldugu icin satirlarda ✓ ve XP gorunur. Log'da "BOLUM SECME DENETIMI:" satiri; goruntu bolum-secme.png.
    IEnumerator LevelSelectCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        LoadLevel(0);
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

    // Deneme icin: ekrandaki noktaya fareyle bir kez tiklar (bas, iki kare bekle, birak)
    static IEnumerator MouseClick(UnityEngine.InputSystem.Mouse mouse, Vector2 at)
    {
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at, buttons = 1 });
        yield return null; yield return null;
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { position = at, buttons = 0 });
        yield return null;
    }

    // ---- Adim adim denetimi (-shots icinde): son bolumun cozumunde ⏭'a uc kez, sonra Devam'a fareyle tiklanir ----
    // Uc adimda for, move, collect calisir: robot bir kare ilerler (ve yalnizca bir kare), satirin yaninda "i = 0" yazar ve kod bekler;
    // Devam kalanini bitirir. Log'da "ADIM ADIM DENETIMI:" satiri; goruntu adim-adim.png.
    IEnumerator StepCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) { Debug.Log("ADIM ADIM DENETIMI: fare yok, atlandi"); yield break; }
        LoadLevel(levels.Count - 1);
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
        var oneStep = Pos(new Cell(start.Col + 1, start.Row));
        string problem = !running || !stepMode ? "kod adim adim beklemiyor"
            : Vector3.Distance(robot.transform.localPosition, oneStep) > 0.05f ? "robot tam bir kare ilerlemedi"
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

    // ---- Ipucu denetimi (-shots icinde): son bolumde balon acilir, "Bir ipucu daha"ya iki kez fareyle tiklanir ----
    // Log'da "IPUCU DENETIMI:" satiri; goruntuler ipucu-1.png (yalnizca ilk ipucu), ipucu-3.png (ucu de acik).
    IEnumerator HintCheck(string dir)
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        LoadLevel(levels.Count - 1);
        hud.ShowHint(true);
        yield return new WaitForSeconds(0.4f);
        Cap(Path.Combine(dir, "ipucu-1.png"));
        yield return new WaitForSeconds(0.3f);
        string problem = mouse == null ? "fare yok" : null;
        for (int k = 0; k < 2 && problem == null; k++)
        {
            var at = hud.MoreHintScreenPoint();
            if (!at.HasValue) { problem = "'Bir ipucu daha' dugmesi bulunamadi (" + (k + 1) + ". tiklama)"; break; }
            yield return MouseClick(mouse, at.Value);
            yield return new WaitForSeconds(0.3f);
        }
        int want = Mathf.Min(3, level.Hints.Count);
        if (problem == null && hints.Shown(level.Number) != want) problem = "acik ipucu " + hints.Shown(level.Number) + " (" + want + " olmali)";
        if (problem == null && hud.MoreHintScreenPoint().HasValue) problem = "hepsi acikken dugme hala gorunuyor";
        Debug.Log("IPUCU DENETIMI: " + (problem ?? "TAMAM"));
        Cap(Path.Combine(dir, "ipucu-3.png"));
        yield return new WaitForSeconds(0.3f);
        hud.ShowHint(false);
    }

    // ---- Kontrol icin ekran goruntusu: MarsKod.exe -shots <klasor> ----

    static void Cap(string path) => ScreenCapture.CaptureScreenshot(path, 2);

    IEnumerator Shots(string dir)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2.5f);
        // Her bolum: bekleme, yolun ortasi, bitis (dogru cozumle)
        for (int i = 0; i < levels.Count; i++)
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
        hud.Tier = KeyboardTier.Orta;
        hud.StopEditing();
        yield return KeyboardTapCheck(dir);
        yield return PaletteCheck(dir);
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
