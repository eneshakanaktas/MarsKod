using System.Collections.Generic;
using UnityEngine;

// Bolum 60 yariginin aglari (bicim RiftShape'te): katmanli iki duvar, bas tarafi kapatan yuz, dip, sag duvara oyulmus patika
// rafi, kenar kayalari, basamaklarda ve dipte dusmus kayalar, donemec kayasi. Hepsi gercek gunesin golgesini dusurur ve alir;
// derinde ortam isigi kisilir (koselere yazilan gok gorunurlugu). Malzeme FarRock: zeminle ayni isik ve ufukta ayni karisma.
public static class RiftMeshes
{
    // Katman renkleri (RiftShape.Tone*): uzak mesalarin govdesi, koyu tabaka, acik tabaka
    static readonly string[] ToneColors = { "#7A4732", "#583120", "#A06A4C" };
    const string FloorColor = "#4E2D20", PathColor = "#B07A58", LipColor = "#6E402C", UndersideColor = "#3A2219";

    // trail: robotun ve Serce'nin gececegi yol (yerel); kayalar bunun uzerine konmaz
    public static void Build(Transform root, RiftShape shape, IReadOnlyList<Vector3> trail)
    {
        var tones = new FlatMesh[RiftShape.Tones];
        for (int i = 0; i < tones.Length; i++) tones[i] = new FlatMesh();
        var floor = new FlatMesh();
        var path = new FlatMesh();
        var lip = new FlatMesh();
        var underside = new FlatMesh();

        var columns = Columns();
        var rows = Rows(shape);
        var left = Grid(shape, columns, rows, -1f);
        var right = Grid(shape, columns, rows, 1f);
        Walls(shape, columns, rows, left, -1f, tones);
        Walls(shape, columns, rows, right, 1f, tones);
        HeadCap(shape, rows, left, right, tones);
        Floor(shape, columns, floor);
        Ledge(shape, path, lip, underside);

        for (int i = 0; i < tones.Length; i++) Add(root, "Duvar-" + i, tones[i], ToneColors[i]);
        Add(root, "Dip", floor, FloorColor);
        Add(root, "Patika", path, PathColor);
        Add(root, "PatikaDudak", lip, LipColor);
        Add(root, "PatikaAlti", underside, UndersideColor);
        Rocks(root, shape, trail);
    }

    // Yarik boyunca kesitler: gorunen kisimda sik, ufkun otesinde seyrek
    static List<float> Columns()
    {
        var t = new List<float>();
        for (float v = 0f; v < 8f - 1e-3f; v += 0.125f) t.Add(v);
        for (float v = 8f; v <= CanyonRift.Length + 1e-3f; v += 0.5f) t.Add(v);
        return t;
    }

    // Duvar satirlari: ortadaki zeminden derinlik. Her katman sinirinda ve basamagin ust yuzunun bitiminde bir satir (katman
    // cizgileri yarik boyunca duz kalir), aralarda ~9 cm'de bir. Kenarin ustu ve dibin alti kesitte kenara/dibe yapisir.
    static List<float> Rows(RiftShape shape)
    {
        var rows = new List<float> { -0.4f, -0.25f, -0.12f };
        for (int k = 0; k < shape.LayerCount - 1; k++)
        {
            float top = shape.LayerTop(k), next = shape.LayerTop(k + 1);
            float from = k == 0 ? top : top + 0.05f;
            if (k > 0) rows.Add(top);
            int parts = Mathf.Max(1, Mathf.CeilToInt((next - from) / 0.09f));
            for (int i = 0; i < parts; i++) rows.Add(from + (next - from) * i / parts);
        }
        return rows;
    }

    struct WallVertex { public Vector3 Position; public float Depth, Shade; }

    static WallVertex[,] Grid(RiftShape shape, List<float> columns, List<float> rows, float s)
    {
        var grid = new WallVertex[columns.Count, rows.Count];
        for (int c = 0; c < columns.Count; c++)
        {
            float t = columns[c], top = shape.LayerTopY(t), rim = shape.RimY(t, s), floor = shape.FloorY(t);
            for (int r = 0; r < rows.Count; r++)
            {
                float y = Mathf.Clamp(top - rows[r], floor, rim);
                float shade = shape.Shade(t, y) * (s > 0f ? shape.RecessShade(t, y) : 1f);
                grid[c, r] = new WallVertex { Position = shape.WallPoint(t, s, y), Depth = top - y, Shade = shade };
            }
        }
        return grid;
    }

    static void Walls(RiftShape shape, List<float> columns, List<float> rows, WallVertex[,] g, float s, FlatMesh[] tones)
    {
        var inward = new Vector3(-s, 0f, 0f);
        for (int c = 0; c + 1 < columns.Count; c++)
            for (int r = 0; r + 1 < rows.Count; r++)
            {
                WallVertex a = g[c, r], b = g[c, r + 1], d = g[c + 1, r], e = g[c + 1, r + 1];
                int tone = shape.ToneAt(0.25f * (a.Depth + b.Depth + d.Depth + e.Depth));
                tones[tone].Quad(a.Position, b.Position, e.Position, d.Position, inward, new Vector4(a.Shade, b.Shade, e.Shade, d.Shade));
            }
    }

    // Yarigin basi (t = 0): iki duvarin ilk kesitleri arasinda, yariga bakan kaya yuzu
    static void HeadCap(RiftShape shape, List<float> rows, WallVertex[,] left, WallVertex[,] right, FlatMesh[] tones)
    {
        for (int r = 0; r + 1 < rows.Count; r++)
        {
            WallVertex a = left[0, r], b = left[0, r + 1], c = right[0, r + 1], d = right[0, r];
            int tone = shape.ToneAt(0.5f * (a.Depth + b.Depth));
            tones[tone].Quad(a.Position, b.Position, c.Position, d.Position, Vector3.forward, new Vector4(a.Shade, b.Shade, c.Shade, d.Shade));
        }
    }

    // Dip: iki duvarin dibi arasinda, ortasi hafif inisli cikisli (kenarlarda tam duvarin dibinde)
    static void Floor(RiftShape shape, List<float> columns, FlatMesh floor)
    {
        const int Across = 4;
        Vector3 Point(float t, int i)
        {
            float u = (float)i / Across, x = Mathf.Lerp(shape.FloorLeftX(t), shape.FloorRightX(t), u), y = shape.FloorY(t);
            float bump = 0.05f * Mathf.Sin(u * Mathf.PI) * (Mathf.PerlinNoise(x * 3.1f + 2f, t * 2.3f) - 0.5f);
            return new Vector3(x, y + bump, shape.Z(t));
        }
        for (int c = 0; c + 1 < columns.Count; c++)
        {
            float t0 = columns[c], t1 = columns[c + 1], s0 = shape.Shade(t0, shape.FloorY(t0)), s1 = shape.Shade(t1, shape.FloorY(t1));
            for (int i = 0; i < Across; i++)
                floor.Quad(Point(t0, i), Point(t0, i + 1), Point(t1, i + 1), Point(t1, i), Vector3.up, new Vector4(s0, s0, s1, s1));
        }
    }

    // Patika rafi: ust yuz (arkada oyugun dibine girer), dis dudakta alcak kaya kenari, on yuz ve duvara donen karanlik alt yuz
    static void Ledge(RiftShape shape, FlatMesh path, FlatMesh lip, FlatMesh underside)
    {
        const float Dt = 0.125f;
        for (float t = 0f; t < RiftShape.LedgeReach - 1e-3f; t += Dt)
        {
            var p0 = LedgeSection(shape, t);
            var p1 = LedgeSection(shape, Mathf.Min(t + Dt, RiftShape.LedgeReach));
            float a0 = shape.Shade(t, p0[0].y) * 0.8f, a1 = shape.Shade(t + Dt, p1[0].y) * 0.8f;
            path.Quad(p0[0], p0[1], p1[1], p1[0], Vector3.up, new Vector4(a0, a0, a1, a1));
            lip.Quad(p0[1], p0[2], p1[2], p1[1], Vector3.up + Vector3.left, a0);
            lip.Quad(p0[2], p0[3], p1[3], p1[2], Vector3.up, a0);
            lip.Quad(p0[3], p0[4], p1[4], p1[3], Vector3.left, a0);
            underside.Quad(p0[4], p0[5], p1[5], p1[4], Vector3.down + Vector3.left * 0.3f, a0 * 0.6f);
        }
    }

    // Rafin kesiti (arkadan one): arka, dudagin ici, dudagin tepesi, dudagin disi, on yuzun alti, alt yuzun duvara degdigi yer
    static Vector3[] LedgeSection(RiftShape shape, float t)
    {
        float y = shape.LedgeY(t), z = shape.Z(t), back = shape.LedgeBackX(t), lipX = shape.LedgeLipX(t);
        float lipHeight = 0.035f + 0.03f * Mathf.PerlinNoise(t * 3.7f, 4.2f);
        float bottom = y - RiftShape.LedgeThickness, wallBottom = bottom - 0.12f;
        return new[]
        {
            new Vector3(back, y, z),
            new Vector3(lipX + 0.09f, y, z),
            new Vector3(lipX + 0.035f, y + lipHeight, z),
            new Vector3(lipX, y + lipHeight * 0.5f, z),
            new Vector3(lipX - 0.01f, bottom, z),
            new Vector3(shape.WallX(t, 1f, wallBottom) + 0.02f, wallBottom, z),
        };
    }

    // Kenar kayalari, basamaklarda ve dipte dusmus kayalar, donemec kayasi. Her biri yuzeyine oturur (bir kismi gomulu).
    // Kaya (merkez pos, yaricap radius) patikaya robotun/Serce'nin gecebilecegi kadar yakin mi
    static bool OnTrail(IReadOnlyList<Vector3> trail, Vector3 pos, float radius)
    {
        const float Clearance = 0.45f, Height = 1.4f;
        for (int i = 1; i < trail.Count; i++)
            for (float u = 0f; u <= 1f; u += 0.25f)
            {
                var p = Vector3.Lerp(trail[i - 1], trail[i], u);
                if (Mathf.Abs(pos.y - p.y) < Height + radius && (new Vector2(pos.x - p.x, pos.z - p.z)).magnitude < radius + Clearance) return true;
            }
        return false;
    }

    static void Rocks(Transform root, RiftShape shape, IReadOnlyList<Vector3> trail)
    {
        int seed = 900;
        // kenarda, zeminde: kirik kenarin parcalari
        foreach (float s in new[] { -1f, 1f })
            for (float t = 0.8f; t < 7.5f; t += 1.3f + 0.4f * Mathf.Sin(t * 3f + s))
            {
                float size = 0.07f + 0.06f * Mathf.PerlinNoise(t * 2f, s + 3f);
                if (s > 0f && t < RiftShape.LedgeReach) continue;   // patikanin agzi acik kalsin
                float x = shape.RimX(t, s) + s * size * 0.9f, z = shape.Z(t);
                var at = new Vector3(x, shape.Ground(x, z) + size * 0.3f, z);
                if (!OnTrail(trail, at, size)) Rock(root, shape, at, size, seed++, 1f);
            }
        // genis basamaklarda: duvardan kopup basamakta kalmis kucuk kayalar
        for (int k = 1; k < shape.LayerCount; k++)
        {
            if (shape.LayerStep(k) < 0.09f) continue;
            for (int i = 0; i < 2; i++)
            {
                float s = (k + i) % 2 == 0 ? -1f : 1f, t = 1.5f + 2.3f * i + 0.7f * k;
                float y = shape.LayerTopY(t) - shape.LayerTop(k);
                if (t > 7f || y < shape.FloorY(t) + 0.3f) continue;
                if (s > 0f && Mathf.Abs(y - shape.LedgeY(t)) < RiftShape.LedgeHeadroom + 0.2f) continue;
                float x = 0.5f * (shape.WallX(t, s, y + 0.06f) + shape.WallX(t, s, y - 0.06f)), size = 0.06f;
                var at = new Vector3(x, y + size * 0.25f, shape.Z(t));
                if (!OnTrail(trail, at, size)) Rock(root, shape, at, size, seed++, 0.8f);
            }
        }
        // dipte: duvarlarin dibine dokulmus moloz (patikadan uzakta)
        for (float t = 1.2f; t < 7.8f; t += 0.55f + 0.3f * Mathf.Sin(t * 5f))
        {
            float s = Mathf.Sin(t * 4.1f) > 0f ? -1f : 1f;
            if (s > 0f && t > RiftShape.LedgeReach - 0.5f) s = -1f;   // dipteki patika sag yanda
            float size = 0.07f + 0.08f * Mathf.PerlinNoise(t * 1.7f, 8f);
            float x = (s < 0f ? shape.FloorLeftX(t) : shape.FloorRightX(t)) - s * size * 0.8f;
            var at = new Vector3(x, shape.FloorY(t) + size * 0.2f, shape.Z(t));
            if (!OnTrail(trail, at, size)) Rock(root, shape, at, size, seed++, 0.9f);
        }
        // donemec kayasi: sol duvara yaslanmis iri, uzun kaya; patika arkasina gecer
        if (!OnTrail(trail, shape.BendRockPoint() + Vector3.up * 0.35f, 0.3f))
        {
            var bend = Rock(root, shape, shape.BendRockPoint() + Vector3.up * 0.35f, 0.42f, 701, 1f);
            bend.localScale = new Vector3(0.6f, 1.9f, 1.2f);
            bend.localRotation = Quaternion.Euler(4f, 25f, -6f);
        }
    }

    static Transform Rock(Transform root, RiftShape shape, Vector3 pos, float size, int seed, float tone)
    {
        float shade = shape.Shade(pos.z - shape.Z(0f), pos.y);
        var mesh = FlatMesh.WithShade(MeshFactory.Rock(size, seed), shade);
        var rock = Parts.Add("Kaya", root, mesh, RiftMat(tone >= 1f ? "#5E3424" : "#6A3C2A"), pos, outline: false);
        rock.localRotation = Quaternion.Euler(0f, seed * 47f, 0f);
        return rock;
    }

    static void Add(Transform root, string name, FlatMesh mesh, string hex)
    {
        if (mesh.IsEmpty) return;
        var part = Parts.Add(name, root, mesh.ToMesh(name), RiftMat(hex), Vector3.zero, outline: false);
        // Duvarlar tek yuzlu: gunese arkasini donen sol duvarin da golge dusurmesi icin iki yuzlu golge
        part.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    }

    // FarRock: zeminle ayni isik. Yarik ufka kadar gorunur kalir, ancak ufkun hemen altinda gokyuzune karisir (_FadeBand; daha
    // genis bantta yarik ufka varmadan silinip duz zemine donuyordu); derinde ortam isigi koselerdeki gok gorunurluguyle kisilir
    // (_VertexAO); alcak safak isigi da gunesin golgesinde kalir (_DawnShadow); golge tam koyu (_DeepShadow).
    static Material RiftMat(string hex)
    {
        var m = Oyun.FarRockMat(hex);
        m.SetVector("_FadeBand", new Vector4(0.012f, 0f, 0f, 0f));
        m.SetVector("_FogRange", new Vector4(60f, 80f, 0f, 0f));   // uzaklik karismasi yok: yalnizca ufukta
        m.SetFloat("_VertexAO", 1f);
        m.SetFloat("_DawnShadow", 1f);
        m.SetFloat("_DeepShadow", 1f);
        m.SetFloat("_SceneOnly", 1f);   // normal bakista yok: yerini zemindeki catlak alir (Ground.shader riftCrack)
        return m;
    }
}
