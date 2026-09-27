using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarsKod.Dunya;
using MarsKod.Motor;
using UnityEngine;
using UnityEngine.Rendering;

// MarsKod oyun sahnesi (unitytaslak1'den geldi): sahneyi kodla kurar (kamera, isik, arka plan, oyun alani,
// robot, buzlar, arayuz). Kod karttaki Python kodu gercekten calisir: once motorda (Assets/Motor) ve dunya kurallarinda
// (Assets/Dunya) aninda calistirilir, sonra olanlar satir satir animasyonla oynatilir.
public class Oyun : MonoBehaviour
{
    const int Cols = 6, Rows = 6, PathRow = 2;
    const float TileTop = 0f;
    // Taslaktakinden daha dik bakis (40 -> 60): arka siralar ezilmez, alan ekranda buyuk ve net gorunur.
    const float CameraPitch = 60f, CameraFov = 32f;
    // Alanin ustunde ufuk, tepeler ve koloni icin birakilan bant (ekran yuksekliginin orani).
    const float HorizonGap = 0.075f;
    static readonly int[] IceCols = { 2, 3, 4 };
    const float StartYaw = 125f;

    // Bolum 1'in cozumu. Kod yazma alani gelene kadar karttaki kod budur.
    const string Level1Code = "# Buzları topla\nmove(East)\nfor i in range(3):\n    move(East)\n    collect()\n";

    Camera cam;
    Material backdrop;
    Transform world;
    Robot robot;
    readonly List<Ice> ices = new List<Ice>();
    Hud hud;
    string code = Level1Code;
    Coroutine program;
    bool running, done;
    float stars = 1f, starsTarget = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<Oyun>() != null) return;
        new GameObject("Oyun").AddComponent<Oyun>();
    }

    static Vector3 Pos(int c, int r) => new Vector3(c - (Cols - 1) * 0.5f, TileTop, r - (Rows - 1) * 0.5f);
    static Vector3 Pos(Cell c) => Pos(c.Col, c.Row);

    // Bolumun baslangic hali (dunya kurallari icin)
    static World NewWorld() => new World(Cols, Rows, new Cell(0, PathRow), IceCols.Select(c => new Cell(c, PathRow)));

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
        }
        world = new GameObject("World").transform;

        SetupCamera();
        SetupLight();
        BuildBoard();

        robot = Robot.Create(world);
        robot.ResetTo(Pos(0, PathRow), StartYaw);
        for (int i = 0; i < IceCols.Length; i++)
            ices.Add(Ice.Create(world, Pos(IceCols[i], PathRow), i + 1));

        hud = gameObject.AddComponent<Hud>();
        hud.Build(CodeColors.Lines(code));
        hud.RunPressed += OnRun;
        hud.ResetPressed += ResetLevel;
        hud.StarsToggled += () => { starsTarget = starsTarget > 0.5f ? 0f : 1f; hud.SetStars(starsTarget > 0.5f); };

        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-shots") StartCoroutine(Shots(args[i + 1]));
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
        const float x0 = -16f, x1 = 16f, z0 = -10f, z1 = 24f, step = 0.4f;
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

        var ground = Mats.Custom("Ground", "MarsKod/Ground");
        ground.SetVector("_Area", new Vector4(AreaHalfX, AreaHalfZ, 0, 0));
        ground.SetVector("_FogRange", new Vector4(9f, 19f, 0, 0));
        for (int i = 0; i < IceCols.Length; i++)
        {
            var p = Pos(IceCols[i], PathRow);
            ground.SetVector("_Frost" + i, new Vector4(p.x, p.z, 0.45f, 1f));
        }
        var cr = Pos(4, 4);
        ground.SetVector("_Crater", new Vector4(cr.x + 0.08f, cr.z - 0.05f, 0.27f, 0f));
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

        // Kayalar: alanin disina serpistirilmis, uzaklastikca seyrek; alanin icinde birkac cakil ve iri kaya (yol uzerinde degil)
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
        void TopRock(int c, int r, float size, Vector2 off, int seed)
        {
            var p = Pos(c, r);
            var rk = Parts.Add("Rock", board, MeshFactory.Rock(size, 100 + seed), seed % 2 == 0 ? rockMat : rockDark,
                new Vector3(p.x + off.x, size * 0.2f, p.z + off.y), outline: false);
            rk.localRotation = Quaternion.Euler(0, seed * 53f, 0);
        }
        TopRock(5, 4, 0.2f, new Vector2(-0.05f, 0.05f), 0);
        TopRock(5, 4, 0.09f, new Vector2(0.22f, -0.2f), 1);
        TopRock(0, 5, 0.13f, new Vector2(0.1f, 0.12f), 2);
        TopRock(1, 0, 0.06f, new Vector2(-0.2f, 0.1f), 3);
        TopRock(2, 4, 0.05f, new Vector2(0.25f, 0.2f), 5);
        TopRock(4, 0, 0.07f, new Vector2(0.18f, 0.1f), 6);
        TopRock(3, 5, 0.1f, new Vector2(-0.15f, -0.1f), 7);
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

    void FitCamera()
    {
        var band = hud.FreeBand();
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
        if (running) return;
        if (done) ResetLevel();
        program = StartCoroutine(RunProgram());
    }

    void ResetLevel()
    {
        if (program != null) StopCoroutine(program);
        program = null;
        running = false; done = false;
        robot.ResetTo(Pos(0, PathRow), StartYaw);
        foreach (var ice in ices) ice.Restore();
        hud.ResetView();
    }

    // Kodu motorda aninda calistirir, sonra kaydi oynatir.
    IEnumerator RunProgram()
    {
        running = true;
        hud.SetRunning(true);
        hud.HideMessage();
        var report = ProgramRun.Execute(code, NewWorld());

        // Bitmeyen dongude kaydin sadece basi oynatilir; uzun kayitta bos satirlarda beklenmez.
        int count = report.Halt != null && report.Halt.Reason == "steps" ? Mathf.Min(report.Trace.Count, 40) : report.Trace.Count;
        bool pauseOnEmpty = report.Trace.Count <= 300;
        robot.GlanceAtCamera(true);
        for (int i = 0; i < count; i++)
        {
            var entry = report.Trace[i];
            hud.SetActiveLine(entry.Line - 1);
            if (entry.Events.Count == 0)
            {
                yield return pauseOnEmpty ? Tween.Wait(0.28f) : null;
                continue;
            }
            yield return Tween.Wait(0.12f);
            foreach (var e in entry.Events) yield return Play(e);
        }
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
            yield return Tween.Wait(0.15f);
            hud.SetDone();
            done = true;
            yield return robot.Celebrate();
        }
        else
        {
            hud.SetActiveLine(-1);
            int left = IceCols.Length - report.Trace.SelectMany(t => t.Events).OfType<Collected>().Count(c => c.IceIndex >= 0);
            hud.ShowMessage("GÖREV", "Kod bitti, buzlar bitmedi",
                "Kodun sonuna kadar çalıştı ama " + left + " buz daha toplanmayı bekliyor. Robot yalnızca kodda yazanı yapar: eksik adımı bul.",
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

    // Karttaki kodu degistirir (kod yazma alani gelince oradan cagrilacak).
    void SetCode(string source)
    {
        ResetLevel();
        code = source;
        hud.SetCode(CodeColors.Lines(code));
    }

    // ---- Kontrol icin ekran goruntusu: MarsKod.exe -shots <klasor> ----

    static void Cap(string path) => ScreenCapture.CaptureScreenshot(path, 2);

    IEnumerator Shots(string dir)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2.5f);
        Cap(Path.Combine(dir, "1-bekleme.png"));
        yield return new WaitForSeconds(0.3f);
        OnRun();
        yield return new WaitForSeconds(2.62f);
        Cap(Path.Combine(dir, "2-toplama.png"));
        yield return new WaitForSeconds(1.6f);
        Cap(Path.Combine(dir, "3-yuruyus.png"));
        while (running) yield return null;
        yield return new WaitForSeconds(0.4f);
        Cap(Path.Combine(dir, "4-bitti.png"));
        yield return new WaitForSeconds(0.3f);
        starsTarget = 0f; hud.SetStars(false);
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "5-yildizsiz.png"));
        starsTarget = 1f; hud.SetStars(true);

        // Hatali kodlar: Python hatasi, oyun kurali, eksik gorev, yazim hatasi
        var cases = new[]
        {
            ("6-python-hatasi", "move(East)\nfor i in range(3):\n    move(East)\n    colect()\n"),
            ("7-oyun-kurali", "for i in range(9):\n    move(East)\n"),
            ("8-eksik-gorev", "move(East)\nmove(East)\ncollect()\n"),
            ("9-yazim-hatasi", "move(East)\nfor i in range(3)\n    move(East)\n"),
        };
        foreach (var (name, source) in cases)
        {
            SetCode(source);
            yield return new WaitForSeconds(0.6f);
            OnRun();
            while (running) yield return null;
            yield return new WaitForSeconds(0.6f);
            Cap(Path.Combine(dir, name + ".png"));
            yield return new WaitForSeconds(0.3f);
        }
        SetCode(Level1Code);
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
