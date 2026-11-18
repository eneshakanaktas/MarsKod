using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Gorsel denetim (Sezon 1 gorsel mantik hatalari, Enes 2026-11-18; yontem: docs/tasarim/gorsel-denetim/YONTEM.md).
// MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -incele <ilk> <son> <mutlak klasor> [-yalniz-olcum]
// Her bolumde once otomatik yerlesim olcumu (PlacementAudit; log'da "YERLESIM:" satirlari, sonda "YERLESIM DENETIMI:").
// Her bolum iki anda (baslangic, dogru cozum bittikten sonra) cekilir: oyun kamerasi, klavye acik genis gorunum, sonra
// arayuz ve gokyuzu resmi gizlenerek bes ayri acidan (iki yan, alcak on, arka, ust) ve uzak bir genel bakis.
// Oyun kamerasi yalniz tek acidan bakar; havada duran, zemine gomulen, duvardan tasan seyler ancak yandan gorunur.
// Not: gokyuzu/tepeler/koloni kameraya bagli duz bir resimdir (MarsSky.hlsl); yalniz oyun kamerasinda anlamlidir.
public partial class Oyun
{
    bool inspecting;   // gorsel denetim kamerayi kendisi yerlestiriyor: FitCamera calismaz

    struct AuditView
    {
        public string Name;
        public Vector3 Eye, Target;
        public float Fov;
        public AuditView(string name, Vector3 eye, Vector3 target, float fov) { Name = name; Eye = eye; Target = target; Fov = fov; }
    }

    static AuditView[] AuditViews()
    {
        float x = AreaHalfX, z = AreaHalfZ;
        var center = new Vector3(0f, 0.3f, 0f);
        return new[]
        {
            new AuditView("3-sol", new Vector3(-(x + 5f), 1.2f, 0f), center, 50f),
            new AuditView("4-sag", new Vector3(x + 5f, 1.2f, 0f), center, 50f),
            new AuditView("5-on-alcak", new Vector3(0f, 0.7f, -(z + 5f)), new Vector3(0f, 0.6f, z), 60f),
            new AuditView("6-arka", new Vector3(0f, 2.5f, z + 6f), center, 55f),
            new AuditView("7-ust", new Vector3(0f, 22f, 0f), Vector3.zero, 40f),
            new AuditView("8-genel", new Vector3(-(x + 10f), 8f, -(z + 8f)), new Vector3(0f, 0f, z + 4f), 60f),
        };
    }

    IEnumerator VisualAudit(int first, int last, string dir)
    {
        shotsMode = suppressQuiz = true;
        ListenWhileUnfocused();
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2f);
        // -yalniz-olcum: goruntu cekmez, yalniz yerlesim olcer (100 bolum birkac dakikada)
        bool measureOnly = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-yalniz-olcum") >= 0;
        int count = 0;
        var flagged = new List<int>();
        int problems = 0;
        for (int i = 0; i < levels.Count; i++)
        {
            int n = levels[i].Number;
            if (n < first || n > last) continue;
            string b = "b" + n.ToString("000") + "-";
            LoadLevel(i);
            yield return new WaitForSeconds(0.8f);
            hud.CloseInfo();
            hud.HideInfoBubble();
            yield return new WaitForSeconds(0.5f);
            var startFindings = LogPlacement(n, "baslangic", flagged);
            problems += startFindings.Count;
            if (!measureOnly) yield return AuditAllViews(dir, b + "a-baslangic-", CloseUps(startFindings));

            SetCode(levels[i].Solution);
            yield return new WaitForSeconds(0.4f);
            OnRun();
            yield return new WaitForSeconds(2.6f);
            if (!measureOnly) Cap(Path.Combine(dir, b + "b-yolda-1-oyun.png"));
            while (running) yield return null;
            yield return new WaitForSeconds(0.8f);
            var endFindings = LogPlacement(n, "bitti", flagged);
            problems += endFindings.Count;
            if (!measureOnly) yield return AuditAllViews(dir, b + "c-bitti-", CloseUps(endFindings));
            count++;
        }
        Debug.Log("GORSEL DENETIM: Bolum " + first + "-" + last + ", " + count + " bolum cekildi");
        Debug.Log("YERLESIM DENETIMI: " + (problems == 0 ? "TAMAM" : problems + " sorun, bolumler: " + string.Join(", ", flagged)));
        yield return new WaitForSeconds(0.3f);
        Application.Quit();
    }

    // Otomatik yerlesim olcumu (PlacementAudit): bulgular log'a "YERLESIM:" satirlariyla
    List<PlacementAudit.Finding> LogPlacement(int levelNumber, string moment, List<int> flagged)
    {
        var findings = PlacementAudit.Check(levelRoot, world, TerrainHeight, GroundFogRange.x);
        foreach (var f in findings) Debug.Log("YERLESIM: bolum=" + levelNumber + " an=" + moment + " " + f.Text);
        if (findings.Count > 0 && !flagged.Contains(levelNumber)) flagged.Add(levelNumber);
        return findings;
    }

    // Yakin cekim: olcumun isaretledigi nesneler ve her zaman hikaye izleri ("Iz-": elle yerlestirilen sus nesneleri, hataya en acik yer).
    // Yer hizasindan, iki yandan; sabit acilarin karesine girmeyebilirler.
    const float CloseUpDistance = 1.3f, CloseUpHeight = 0.25f;
    const string StoryTracePrefix = "Iz-";

    IEnumerable<AuditView> CloseUps(List<PlacementAudit.Finding> findings)
    {
        var targets = new List<(Transform item, Vector3 at)>();
        foreach (var f in findings) targets.Add((f.Item, world.TransformPoint(f.At)));
        foreach (Transform child in levelRoot)
            if (child.name.StartsWith(StoryTracePrefix) && TryCenter(child, out var c)) targets.Add((child, c));
        var seen = new HashSet<string>();   // ayni adli parcalar (alti tekerlek seridi) icin bir yakin cekim yeter
        foreach (var (item, at) in targets)
        {
            if (!seen.Add(item.name)) continue;
            // uzun nesnede (ufka giden iz) ortasi uzakta ve kivrilan zeminin altinda kalir: alana yakin kisma, zemin hizasina bakilir
            var local = world.InverseTransformPoint(at);
            local.z = Mathf.Min(local.z, AreaHalfZ);
            local.y = Mathf.Max(local.y, TerrainHeight(local.x, local.z) + 0.1f);
            var target = world.TransformPoint(local);
            string name = "9-yakin" + seen.Count;
            yield return new AuditView(name + "a-" + item.name, target + new Vector3(-CloseUpDistance, CloseUpHeight, -0.4f), target, 45f);
            yield return new AuditView(name + "b-" + item.name, target + new Vector3(0.4f, CloseUpHeight, -CloseUpDistance), target, 45f);
        }
    }

    static bool TryCenter(Transform item, out Vector3 center)
    {
        var renderers = item.GetComponentsInChildren<Renderer>();
        center = Vector3.zero;
        if (renderers.Length == 0) return false;
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        center = b.center;
        return true;
    }

    IEnumerator AuditAllViews(string dir, string prefix, IEnumerable<AuditView> closeUps)
    {
        Cap(Path.Combine(dir, prefix + "1-oyun.png"));
        yield return new WaitForSeconds(0.3f);

        // klavye acik: arayuz buyur, kamera genisler, alanin cevresi gorunur (76'daki duvar disi gibi sorunlar burada)
        hud.FocusCode(code.Length, code.Length);
        yield return new WaitForSeconds(0.8f);
        Cap(Path.Combine(dir, prefix + "2-genis.png"));
        yield return new WaitForSeconds(0.3f);
        hud.StopEditing();
        yield return new WaitForSeconds(0.5f);

        var backdropRenderer = cam.transform.Find("Backdrop").GetComponent<Renderer>();
        var clear = cam.clearFlags;
        inspecting = true;
        hud.SetAuditHidden(true);
        backdropRenderer.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor;
        var views = new List<AuditView>(AuditViews());
        views.AddRange(closeUps);
        foreach (var view in views)
        {
            cam.ResetProjectionMatrix();
            cam.fieldOfView = view.Fov;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 300f;
            var look = (view.Target - view.Eye).normalized;
            var up = Mathf.Abs(look.y) > 0.99f ? Vector3.forward : Vector3.up;   // tam tepeden bakista kuzey ekranin ustu
            cam.transform.SetPositionAndRotation(view.Eye, Quaternion.LookRotation(look, up));
            yield return new WaitForSeconds(0.25f);
            Cap(Path.Combine(dir, prefix + view.Name + ".png"));
            yield return new WaitForSeconds(0.15f);
        }
        backdropRenderer.enabled = true;
        cam.clearFlags = clear;
        hud.SetAuditHidden(false);
        inspecting = false;
        yield return null;
    }
}
