using System;
using System.Collections.Generic;
using UnityEngine;

// Otomatik yerlesim olcumu (gorsel denetim, 2026-11-18; docs/tasarim/gorsel-denetim/YONTEM.md).
// Bolumun nesnelerini (bolum kokunun cocuklari; engeller ve toplanacaklar ayri ayri) zemine gore olcer:
//  - havada: nesnenin bir bolgesinde en alt noktasi zeminden FloatTolerance'tan yuksek (zemin kivrilinca kalkan duz iz gibi)
//  - gomulu: noktalarinin yarisindan cogu zeminin altinda
//  - uzakta karismiyor: arka plana karisma baslangicindan (z) oteye uzaniyor ama zeminle ayni karisma kuralini kullanmiyor
//    (ufukta daglarin, gokyuzunun ustune cizilir)
// Bilerek havada duran nesneler Airborne ile etiketlenir, sayilmaz. Sonuc yalniz gozle bakilacak yerleri gosterir; hukmu goruntu verir.
public static class PlacementAudit
{
    // Bir bulgu: hangi nesne, nerede (dunya kokune gore), ne sorun
    public readonly struct Finding
    {
        public readonly Transform Item;
        public readonly Vector3 At;
        public readonly string Text;
        public Finding(Transform item, Vector3 at, string text) { Item = item; At = at; Text = text; }
    }

    const float FloatTolerance = 0.03f;   // m: bundan yuksekte duran bolge "havada"
    const float BuriedShare = 0.5f;       // noktalarin bu kadarindan fazlasi zeminin altindaysa "gomulu"
    const float CellSize = 1f;            // nesne bu buyuklukte bolgelere bolunup her bolge ayri olculur (uzun iz, kablo)
    const int MaxPointsPerMesh = 400;     // buyuk agda her noktaya bakilmaz
    static readonly string[] FadingShaders = { "MarsKod/FarRock", "MarsKod/Ground" };

    public static List<Finding> Check(Transform levelRoot, Transform world, Func<float, float, float> height, float fadeStartZ)
    {
        var findings = new List<Finding>();
        foreach (var item in Items(levelRoot)) CheckItem(item, world, height, fadeStartZ, findings);
        return findings;
    }

    // Olculecek nesneler: bolum kokunun cocuklari; "Duzen" grubunun (engeller, buz, hucre) cocuklari ayri ayri
    static IEnumerable<Transform> Items(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.name == "Duzen") { foreach (Transform part in child) yield return part; }
            else yield return child;
        }
    }

    static void CheckItem(Transform item, Transform world, Func<float, float, float> height, float fadeStartZ, List<Finding> findings)
    {
        var lowestInCell = new Dictionary<Vector2Int, float>();
        float lowest = float.MaxValue;
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
        int points = 0, below = 0;
        bool farWithoutFade = false;
        Vector3 sum = Vector3.zero;
        foreach (var mf in item.GetComponentsInChildren<MeshFilter>())
        {
            var r = mf.GetComponent<Renderer>();
            if (r == null || !r.enabled || !mf.gameObject.activeInHierarchy || mf.GetComponentInParent<Airborne>() != null) continue;
            var mesh = mf.sharedMesh;
            if (mesh == null || !mesh.isReadable) continue;
            bool fades = Fades(r);
            var verts = mesh.vertices;
            int step = Mathf.Max(1, verts.Length / MaxPointsPerMesh);
            for (int i = 0; i < verts.Length; i += step)
            {
                var p = world.InverseTransformPoint(mf.transform.TransformPoint(verts[i]));
                float gap = p.y - height(p.x, p.z);
                var cell = new Vector2Int(Mathf.FloorToInt(p.x / CellSize), Mathf.FloorToInt(p.z / CellSize));
                lowestInCell[cell] = lowestInCell.TryGetValue(cell, out float low) ? Mathf.Min(low, gap) : gap;
                lowest = Mathf.Min(lowest, gap);
                min = Vector2.Min(min, new Vector2(p.x, p.z));
                max = Vector2.Max(max, new Vector2(p.x, p.z));
                points++;
                if (gap < -0.005f) below++;
                if (p.z > fadeStartZ && !fades) farWithoutFade = true;
                sum += p;
            }
        }
        if (points == 0) return;

        var at = sum / points;
        string where = $"{item.name} ({at.x:0.0}, {at.z:0.0})";
        // kucuk nesne butun olarak olculur (kavisli kabugu bir bolgede yukari kalkar); buyuk nesne (iz, kablo) bolge bolge
        bool small = max.x - min.x < CellSize && max.y - min.y < CellSize;
        int floatingCells = 0;
        float worst = 0f;
        if (small) { if (lowest > FloatTolerance) { floatingCells = 1; worst = lowest; } }
        else
            foreach (var low in lowestInCell.Values)
                if (low > FloatTolerance) { floatingCells++; worst = Mathf.Max(worst, low); }
        if (floatingCells > 0) findings.Add(new Finding(item, at, $"{where}: havada ({floatingCells} bolgede, en cok {worst:0.00} m)"));
        if (below > points * BuriedShare) findings.Add(new Finding(item, at, $"{where}: gomulu (noktalarin %{100 * below / points}'i zeminin altinda)"));
        if (farWithoutFade) findings.Add(new Finding(item, at, $"{where}: uzaga uzaniyor ama arka plana karismiyor (ufukta daglarin/gokyuzunun ustune cizilir)"));
    }

    static bool Fades(Renderer r)
    {
        var mat = r.sharedMaterial;
        return mat != null && Array.IndexOf(FadingShaders, mat.shader.name) >= 0;
    }
}
