using System;
using System.Collections.Generic;
using UnityEngine;

// Oyun alaninin cevresi: kayalar, kaya kumeleri, iri bloklar, olcum istasyonlari, malzeme sandiklari.
// Arayuz (klavye + ipucu) acilip sahne kucukce cevre genis gorunur; orasi bos kalmasin.
// Kayalar tek parca aga birlestirilir (telefonda yuzlerce kaya = birkac cizim). Hepsi zeminle ayni kuralla arka plana karisir.
public static class Scenery
{
    const int RockShapes = 16;

    public static void Build(Transform parent, Func<float, float, float> height, Vector2 areaHalf, Func<string, Material> farMat)
    {
        var rng = new System.Random(7);
        float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
        var shapes = new Mesh[RockShapes];
        for (int i = 0; i < RockShapes; i++) shapes[i] = MeshFactory.Rock(1f, i);

        var light = new List<CombineInstance>();
        var dark = new List<CombineInstance>();
        void Rock(float x, float z, float size)
        {
            // alanin icine ve hemen kenarina kaya konmaz (bolumun kayalari engeldir)
            if (Mathf.Abs(x) < areaHalf.x + 0.45f && Mathf.Abs(z) < areaHalf.y + 0.45f) return;
            int shape = rng.Next(RockShapes);
            var rot = Quaternion.Euler(R(-10, 10), R(0, 360), R(-10, 10));
            var m = Matrix4x4.TRS(new Vector3(x, height(x, z) + size * 0.12f, z), rot, Vector3.one * size);
            (shape % 3 == 0 ? dark : light).Add(new CombineInstance { mesh = shapes[shape], transform = m });
        }
        float Outside(float x, float z) => Mathf.Max(Mathf.Abs(x) - areaHalf.x, Mathf.Abs(z) - areaHalf.y);

        // 1) Alanin hemen cevresi: uzaklastikca seyrelen kucuk kayalar (normal gorunumde gorunen kisim)
        for (int i = 0; i < 130; i++)
        {
            float x = R(-9f, 9f), z = R(-6f, 8.5f);
            if (R(0f, 1f) < Outside(x, z) * 0.05f) continue;
            Rock(x, z, Mathf.Lerp(0.05f, 0.3f, Mathf.Pow(R(0f, 1f), 2.2f)));
        }
        // 2) Genis cevre: sahne kuculunce gorunen bolge; uzakta biraz daha iri (kucuk fotografta da secilsin)
        for (int i = 0; i < 480; i++)
        {
            float x = R(-24f, 24f), z = R(-14f, 22f);
            if (Mathf.Abs(x) < 9f && z > -6f && z < 8.5f) continue;
            float o = Outside(x, z);
            Rock(x, z, Mathf.Lerp(0.06f, 0.42f, Mathf.Pow(R(0f, 1f), 2.4f)) * (1f + o * 0.03f));
        }
        // 3) Kaya kumeleri: bir merkez etrafinda bir avuc kaya (cokmus bir tepe ya da carpma artigi gibi)
        for (int c = 0; c < 16; c++)
        {
            float cx = R(-20f, 20f), cz = R(-12f, 18f);
            if (Outside(cx, cz) < 3f) continue;
            int n = rng.Next(5, 10);
            for (int k = 0; k < n; k++)
            {
                float a = R(0f, 6.283f), r = Mathf.Sqrt(R(0f, 1f)) * 0.9f;
                Rock(cx + Mathf.Cos(a) * r, cz + Mathf.Sin(a) * r, k == 0 ? R(0.35f, 0.55f) : Mathf.Lerp(0.07f, 0.3f, R(0f, 1f)));
            }
        }
        // 4) Tek tuk iri kaya bloklari
        for (int i = 0; i < 12; i++)
        {
            float x = R(-22f, 22f), z = R(-12f, 18f);
            if (Outside(x, z) < 6f) continue;
            Rock(x, z, R(0.6f, 0.95f));
        }
        Combined("Rocks", parent, light, farMat("#6B4034"));
        Combined("RocksDark", parent, dark, farMat("#553229"));

        BuildProps(parent, height, farMat);
    }

    static void Combined(string name, Transform parent, List<CombineInstance> parts, Material mat)
    {
        var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.CombineMeshes(parts.ToArray(), true, true);
        mesh.RecalculateBounds();
        Parts.Add(name, parent, mesh, mat, Vector3.zero, outline: false);
    }

    // Kolonininkine benzer kucuk isler: isigi yanip sonen olcum istasyonlari, birakilmis malzeme sandiklari.
    // Normal gorunumun disinda dururlar; sahne kuculunce cevreye "burada calisiliyor" havasi verirler.
    static void BuildProps(Transform parent, Func<float, float, float> height, Func<string, Material> farMat)
    {
        var metal = farMat("#6E6A72");
        var crate = farMat("#B9773A");
        var crateDark = farMat("#4A3E3C");

        var stations = new[] { new Vector2(-6.5f, 1.5f), new Vector2(7.2f, -2.5f), new Vector2(-10.5f, 6.5f), new Vector2(11f, 4.5f) };
        for (int i = 0; i < stations.Length; i++)
        {
            var s = stations[i];
            var root = Parts.Empty("Station", parent);
            root.localPosition = new Vector3(s.x, height(s.x, s.y), s.y);
            root.localRotation = Quaternion.Euler(0, i * 67f, 0);
            for (int k = 0; k < 3; k++)
            {
                var leg = Parts.Add("Leg", root, MeshFactory.RoundedCylinder(0.018f, 0.5f, 0.006f, 8), metal, Vector3.zero, outline: false);
                float a = k * 120f * Mathf.Deg2Rad;
                leg.localPosition = new Vector3(Mathf.Cos(a) * 0.1f, 0.24f, Mathf.Sin(a) * 0.1f);
                leg.localRotation = Quaternion.AngleAxis(12f, new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a)));
            }
            Parts.Add("Box", root, MeshFactory.RoundedBox(new Vector3(0.16f, 0.1f, 0.12f), 0.015f), metal, new Vector3(0, 0.5f, 0), outline: false);
            Parts.Add("Panel", root, MeshFactory.RoundedBox(new Vector3(0.22f, 0.012f, 0.14f), 0.004f), crateDark, new Vector3(0, 0.57f, -0.02f), outline: false)
                .localRotation = Quaternion.Euler(-25f, 0, 0);
            var lamp = Parts.Add("Lamp", root, MeshFactory.Sphere(0.03f, 6, 10), null, new Vector3(0, 0.64f, 0.04f), outline: false, castShadow: false);
            Blinker.Attach(lamp, i % 2 == 0 ? Mats.Hex("#FF4A3A") : Mats.Hex("#FFB040"), 0.9f + i * 0.23f, i * 1.7f);
        }

        var crates = new[] { new Vector2(6f, 5.5f), new Vector2(-7.5f, -3.5f), new Vector2(-12.5f, 0.5f) };
        for (int i = 0; i < crates.Length; i++)
        {
            var c = crates[i];
            var root = Parts.Empty("Crates", parent);
            root.localPosition = new Vector3(c.x, height(c.x, c.y), c.y);
            root.localRotation = Quaternion.Euler(0, i * 41f + 15f, 0);
            Parts.Add("Crate", root, MeshFactory.RoundedBox(new Vector3(0.42f, 0.3f, 0.3f), 0.03f), crate, new Vector3(0, 0.15f, 0), outline: false);
            Parts.Add("Crate", root, MeshFactory.RoundedBox(new Vector3(0.3f, 0.24f, 0.26f), 0.03f), crateDark, new Vector3(0.42f, 0.12f, 0.06f), outline: false);
            Parts.Add("Crate", root, MeshFactory.RoundedBox(new Vector3(0.28f, 0.2f, 0.24f), 0.03f), crate, new Vector3(0.05f, 0.4f, 0.02f), outline: false)
                .localRotation = Quaternion.Euler(0, 18f, 0);
        }
    }
}

// Yanip sonen kucuk isik (olcum istasyonlari): her biri kendi hizinda
public class Blinker : MonoBehaviour
{
    Material mat;
    Color color;
    float speed, phase;

    public static void Attach(Transform t, Color color, float speed, float phase)
    {
        var b = t.gameObject.AddComponent<Blinker>();
        b.mat = Mats.Emissive(color, color, 0.5f);
        b.color = color;
        b.speed = speed;
        b.phase = phase;
        t.GetComponent<MeshRenderer>().sharedMaterial = b.mat;
    }

    void Update()
    {
        float on = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Time.time * speed * 3f + phase), 8f);
        mat.SetColor("_EmissionColor", color * (0.15f + 2.2f * on));
    }
}
