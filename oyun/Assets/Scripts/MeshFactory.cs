using System.Collections.Generic;
using UnityEngine;

// Kodla uretilen oyuncak sekiller: yuvarlak koseli kutu, dondurme (torna) sekilleri, kure, kristal.
// Her sekil dis cizgi icin yumusak normalleri uv3 (TEXCOORD3) kanalina da yazar.
public static class MeshFactory
{
    public static Mesh RoundedBox(Vector3 size, float radius, int k = 5)
    {
        Vector3 h = size * 0.5f;
        float r = Mathf.Min(radius, Mathf.Min(h.x, Mathf.Min(h.y, h.z)));
        Vector3 inner = h - Vector3.one * r;

        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var tris = new List<int>();

        for (int face = 0; face < 6; face++)
        {
            int nAxis = face / 2;
            float sign = face % 2 == 0 ? 1f : -1f;
            int uAxis = (nAxis + 1) % 3;
            int vAxis = (nAxis + 2) % 3;

            float[] cu = AxisCoords(h[uAxis], inner[uAxis], r, k);
            float[] cv = AxisCoords(h[vAxis], inner[vAxis], r, k);

            Vector3 n = Vector3.zero; n[nAxis] = sign;
            Vector3 U = Vector3.zero; U[uAxis] = 1f;
            Vector3 V = Vector3.zero; V[vAxis] = 1f;
            bool flip = Vector3.Dot(Vector3.Cross(U, V), n) < 0f;

            int start = verts.Count;
            for (int j = 0; j < cv.Length; j++)
            for (int i = 0; i < cu.Length; i++)
            {
                Vector3 p = Vector3.zero;
                p[nAxis] = sign * h[nAxis];
                p[uAxis] = cu[i];
                p[vAxis] = cv[j];
                Vector3 c = new Vector3(
                    Mathf.Clamp(p.x, -inner.x, inner.x),
                    Mathf.Clamp(p.y, -inner.y, inner.y),
                    Mathf.Clamp(p.z, -inner.z, inner.z));
                Vector3 d = p - c;
                Vector3 nn = d.sqrMagnitude > 1e-12f ? d.normalized : n;
                verts.Add(c + nn * r);
                norms.Add(nn);
            }

            int w = cu.Length;
            for (int j = 0; j < cv.Length - 1; j++)
            for (int i = 0; i < w - 1; i++)
            {
                int a = start + j * w + i, b = a + 1, c = a + w + 1, d = a + w;
                if (!flip) { tris.Add(a); tris.Add(b); tris.Add(c); tris.Add(a); tris.Add(c); tris.Add(d); }
                else { tris.Add(a); tris.Add(c); tris.Add(b); tris.Add(a); tris.Add(d); tris.Add(c); }
            }
        }
        return Build("RoundedBox", verts, norms, tris);
    }

    static float[] AxisCoords(float h, float inner, float r, int k)
    {
        var l = new List<float>();
        for (int i = k; i >= 1; i--) l.Add(-(inner + r * Mathf.Tan(Mathf.PI * 0.25f * i / k)));
        l.Add(-inner);
        if (inner > 1e-5f) l.Add(inner);
        for (int i = 1; i <= k; i++) l.Add(inner + r * Mathf.Tan(Mathf.PI * 0.25f * i / k));
        return l.ToArray();
    }

    // profile: (yaricap, yukseklik) noktalari, alttan uste. Y ekseni etrafinda dondurulur.
    public static Mesh Lathe(IList<Vector2> profile, int segments, bool flat = false)
    {
        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var tris = new List<int>();
        int pc = profile.Count;

        for (int s = 0; s <= segments; s++)
        {
            float a = Mathf.PI * 2f * s / segments;
            float ca = Mathf.Cos(a), sa = Mathf.Sin(a);
            for (int i = 0; i < pc; i++)
            {
                Vector2 prev = profile[Mathf.Max(i - 1, 0)];
                Vector2 next = profile[Mathf.Min(i + 1, pc - 1)];
                Vector2 t = next - prev;
                Vector2 n2 = new Vector2(t.y, -t.x).normalized;
                verts.Add(new Vector3(profile[i].x * ca, profile[i].y, profile[i].x * sa));
                norms.Add(new Vector3(n2.x * ca, n2.y, n2.x * sa).normalized);
            }
        }

        // Yonu bir orta noktada denetle
        int mid = pc / 2;
        Vector3 U = verts[1 * pc + mid] - verts[mid];
        Vector3 V = verts[Mathf.Min(mid + 1, pc - 1)] - verts[Mathf.Max(mid - 1, 0)];
        bool flip = Vector3.Dot(Vector3.Cross(U, V), norms[mid]) < 0f;

        for (int s = 0; s < segments; s++)
        for (int i = 0; i < pc - 1; i++)
        {
            int a = s * pc + i;          // (s, i)
            int b = (s + 1) * pc + i;    // +U
            int c = (s + 1) * pc + i + 1;
            int d = s * pc + i + 1;      // +V
            if (!flip) { tris.Add(a); tris.Add(b); tris.Add(c); tris.Add(a); tris.Add(c); tris.Add(d); }
            else { tris.Add(a); tris.Add(c); tris.Add(b); tris.Add(a); tris.Add(d); tris.Add(c); }
        }

        var mesh = Build("Lathe", verts, norms, tris);
        return flat ? Flatten(mesh) : mesh;
    }

    public static Mesh Sphere(float r, int rings = 14, int segments = 24)
    {
        var p = new List<Vector2>();
        for (int i = 0; i <= rings; i++)
        {
            float phi = -Mathf.PI * 0.5f + Mathf.PI * i / rings;
            p.Add(new Vector2(Mathf.Cos(phi) * r, Mathf.Sin(phi) * r));
        }
        p[0] = new Vector2(0f, -r);
        p[rings] = new Vector2(0f, r);
        return Lathe(p, segments);
    }

    // Y ekseni boyunca, kenarlari yuvarlatilmis silindir (tekerlek, boyun, anten).
    public static Mesh RoundedCylinder(float radius, float height, float edge, int segments = 28, int k = 5)
    {
        float hh = height * 0.5f;
        edge = Mathf.Min(edge, Mathf.Min(radius, hh));
        var p = new List<Vector2> { new Vector2(0f, -hh) };
        for (int i = 0; i <= k; i++)
        {
            float a = -Mathf.PI * 0.5f + Mathf.PI * 0.5f * i / k;
            p.Add(new Vector2(radius - edge + Mathf.Cos(a) * edge, -hh + edge + Mathf.Sin(a) * edge));
        }
        for (int i = 0; i <= k; i++)
        {
            float a = Mathf.PI * 0.5f * i / k;
            p.Add(new Vector2(radius - edge + Mathf.Cos(a) * edge, hh - edge + Mathf.Sin(a) * edge));
        }
        p.Add(new Vector2(0f, hh));
        return Lathe(p, segments);
    }

    // Alti koseli, sivri uclu buz kristali (duz yuzeyli).
    public static Mesh Crystal(float radius, float body, float tip)
    {
        var p = new List<Vector2>
        {
            new Vector2(0f, 0f),
            new Vector2(radius * 0.82f, 0f),
            new Vector2(radius, body * 0.18f),
            new Vector2(radius, body),
            new Vector2(0f, body + tip),
        };
        return Lathe(p, 6, flat: true);
    }

    // Az koseli (low-poly) kaya: basik, duzensiz, duz yuzeyli.
    public static Mesh Rock(float radius, int seed)
    {
        var p = new List<Vector2>();
        const int rings = 4;
        for (int i = 0; i <= rings; i++)
        {
            float phi = -Mathf.PI * 0.5f + Mathf.PI * i / rings;
            p.Add(new Vector2(Mathf.Cos(phi) * radius, Mathf.Sin(phi) * radius));
        }
        p[0] = new Vector2(0f, -radius);
        p[rings] = new Vector2(0f, radius);
        var smooth = Lathe(p, 7);
        var v = smooth.vertices;
        for (int i = 0; i < v.Length; i++)
        {
            Vector3 k = Round(v[i]);
            float h = Mathf.Abs(Mathf.Sin(k.x * 12.9898f + k.y * 78.233f + k.z * 37.719f + seed * 11.3f) * 43758.5453f) % 1f;
            v[i] = new Vector3(v[i].x * (0.8f + 0.4f * h), v[i].y * (0.55f + 0.25f * h), v[i].z * (0.85f + 0.35f * h));
        }
        smooth.vertices = v;
        smooth.RecalculateNormals();
        smooth.SetUVs(3, new List<Vector3>(smooth.normals));
        return Flatten(smooth);
    }

    // Yukari bakan duz kare (XZ duzlemi), uv 0-1.
    public static Mesh Quad(Vector2 size)
    {
        var m = new Mesh { name = "Quad" };
        float x = size.x * 0.5f, z = size.y * 0.5f;
        m.vertices = new[] { new Vector3(-x, 0, -z), new Vector3(x, 0, -z), new Vector3(-x, 0, z), new Vector3(x, 0, z) };
        m.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        m.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        m.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        m.RecalculateBounds();
        return m;
    }

    static Mesh Build(string name, List<Vector3> v, List<Vector3> n, List<int> t)
    {
        var m = new Mesh { name = name };
        if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.SetVertices(v);
        m.SetNormals(n);
        m.SetUVs(3, n);
        m.SetTriangles(t, 0);
        m.RecalculateBounds();
        return m;
    }

    // Keskin (duz) yuzeyler: her ucgen kendi normalini alir, dis cizgi icin yumusak normal uv3'te kalir.
    static Mesh Flatten(Mesh src)
    {
        var sv = src.vertices; var sn = src.normals; var st = src.triangles;
        // ayni konumdaki noktalarin normallerini ortala (dis cizgi kopmasin)
        var avg = new Dictionary<Vector3, Vector3>();
        for (int i = 0; i < sv.Length; i++)
        {
            Vector3 key = Round(sv[i]);
            avg[key] = avg.TryGetValue(key, out var acc) ? acc + sn[i] : sn[i];
        }
        var v = new List<Vector3>(); var n = new List<Vector3>(); var sm = new List<Vector3>(); var t = new List<int>();
        for (int i = 0; i < st.Length; i += 3)
        {
            Vector3 a = sv[st[i]], b = sv[st[i + 1]], c = sv[st[i + 2]];
            Vector3 fn = Vector3.Cross(b - a, c - a);
            if (fn.sqrMagnitude < 1e-14f) continue;
            fn.Normalize();
            foreach (var p in new[] { a, b, c })
            {
                t.Add(v.Count);
                v.Add(p);
                n.Add(fn);
                sm.Add(avg[Round(p)].normalized);
            }
        }
        var m = new Mesh { name = src.name + "_flat" };
        m.SetVertices(v); m.SetNormals(n); m.SetUVs(3, sm); m.SetTriangles(t, 0);
        m.RecalculateBounds();
        return m;
    }

    static Vector3 Round(Vector3 p) => new Vector3(Mathf.Round(p.x * 10000f), Mathf.Round(p.y * 10000f), Mathf.Round(p.z * 10000f));
}
