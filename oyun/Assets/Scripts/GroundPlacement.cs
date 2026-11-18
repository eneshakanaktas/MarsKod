using System;
using System.Collections.Generic;
using UnityEngine;

// Zemine yerlestirme (gorsel denetim, 2026-11-18; docs/tasarim/gorsel-denetim/YONTEM.md).
// Zemin alanin disinda inisli cikisli ve arkada asagi kivrilir (Oyun.TerrainHeight); uzakta da kameraya bagli gokyuzu resmine karisir.
// Zemine konan bir sey sabit yukseklikle konursa zemin kivrildiginda havada kalir, ufukta daglarin/gokyuzunun ustune cizilir.
// Kural: yerdeki nesne SitOnGround ile oturtulur; yere serili iz/serit Ribbon ile zemini izler ve uzaga gidiyorsa
// zeminle ayni karisma kuralini kullanan malzemeyle (Oyun.FarRockMat) cizilir.
public static class GroundPlacement
{
    // Nesneyi, zemine en yakin noktasi zemine degecek sekilde indirir ya da kaldirir. sink: zemine hafif gomulme (dogal durus).
    // Cevre kutusu (bounds) degil gercek noktalar olculur: egik nesnenin kutusu govdesinden asagi tasar, kutu yere degip govde havada kalir.
    public static void SitOnGround(Transform root, Func<float, float, float> height, float sink = 0f)
    {
        var parent = root.parent;
        float lowestGap = float.MaxValue;
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            foreach (var v in mf.sharedMesh.vertices)
            {
                var w = mf.transform.TransformPoint(v);
                var p = parent != null ? parent.InverseTransformPoint(w) : w;
                lowestGap = Mathf.Min(lowestGap, p.y - height(p.x, p.z));
            }
        }
        if (lowestGap == float.MaxValue) return;
        root.localPosition -= Vector3.up * (lowestGap + sink);
    }

    // Zemini izleyen ince serit (tekerlek izi, patika): path (x, z) noktalari boyunca, genislik width, zeminden lift yukarida.
    public static Mesh Ribbon(IReadOnlyList<Vector2> path, float width, Func<float, float, float> height, float lift)
    {
        int n = path.Count;
        var verts = new Vector3[n * 2];
        var uv = new Vector2[n * 2];
        float along = 0f;
        for (int i = 0; i < n; i++)
        {
            var dir = (path[Mathf.Min(i + 1, n - 1)] - path[Mathf.Max(i - 1, 0)]).normalized;
            var side = new Vector2(dir.y, -dir.x) * (width * 0.5f);
            if (i > 0) along += Vector2.Distance(path[i - 1], path[i]);
            for (int k = 0; k < 2; k++)
            {
                var q = path[i] + (k == 0 ? -side : side);
                verts[i * 2 + k] = new Vector3(q.x, height(q.x, q.y) + lift, q.y);
                uv[i * 2 + k] = new Vector2(k, along);
            }
        }
        var tris = new int[(n - 1) * 6];
        for (int i = 0; i < n - 1; i++)
        {
            int a = i * 2, t = i * 6;
            tris[t] = a; tris[t + 1] = a + 2; tris[t + 2] = a + 1;
            tris[t + 3] = a + 1; tris[t + 4] = a + 2; tris[t + 5] = a + 3;
        }
        var mesh = new Mesh { name = "GroundRibbon", vertices = verts, uv = uv, triangles = tris };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
