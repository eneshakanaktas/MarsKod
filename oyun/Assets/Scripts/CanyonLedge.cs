using System;
using UnityEngine;

// Kanyondaki alcak kaya yapilari (Bolge 6): 58'de alanin arkasindaki dogal kaya basamagi, 60'ta zeminin bittigi kenar
// (asagi inen patikanin agzi). Ikisi de ufku kesmeyecek kadar alcak; her parca zeminin gercek yuksekligine oturur.
public static class CanyonLedge
{
    static readonly Material Rock = Mats.Lit(Mats.Hex("#7D4A34"), 0.12f);
    static readonly Material RockTop = Mats.Lit(Mats.Hex("#94593C"), 0.12f);
    static readonly Material Strata = Mats.Lit(Mats.Hex("#5E3424"), 0.1f);
    static readonly Material PostMat = Mats.Lit(Mats.Hex("#2E2624"), 0.3f);

    const float StepDepth = 0.6f;
    static readonly float[] BlockShares = { 0.3f, 0.22f, 0.28f, 0.2f };   // basamagin bloklari: boyun payi (soldan saga)

    // Bolum 58. z: basamagin on yuzu; width: boyu; height: yuksekligi. Dogal tortu kayasi: bloklar farkli boyda ve derinlikte,
    // hafif donuk, koseleri asinmis; ustlerinde basik kaya parcalari (kutu/tugla sirasi gibi gorunmesin).
    // Doner: on yuz (boya buraya, -Z'ye bakar; y=0 zemin).
    public static Transform Step(Transform parent, float z, float width, float height, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Iz-KayaBasamagi", parent);
        float x = -width * 0.5f;
        for (int i = 0; i < BlockShares.Length; i++)
        {
            float w = width * BlockShares[i], h = height * (1f + 0.25f * Mathf.Sin(i * 2.3f + 0.5f));
            float d = StepDepth * (1f + 0.25f * Mathf.Cos(i * 1.7f)), back = 0.06f * Mathf.Sin(i * 3.1f);
            float cx = x + w * 0.5f, y = ground(cx, z + d * 0.5f);
            var block = Parts.Add("Blok", root, MeshFactory.RoundedBox(new Vector3(w + 0.08f, h, d), 0.12f), Rock,
                new Vector3(cx, y + h * 0.5f, z + back + d * 0.5f));
            block.localRotation = Quaternion.Euler(0f, 2f * Mathf.Sin(i * 2.9f), 2f * Mathf.Cos(i * 1.3f));   // boya yuzeyi blogun onunde kalsin: donus kucuk
            // ustte basik, duzensiz kaya parcalari
            for (int k = 0; k < 2; k++)
            {
                float rx = cx + w * (k == 0 ? -0.22f : 0.18f) + 0.05f * Mathf.Sin(i + k);
                var top = Parts.Add("UstKaya", root, MeshFactory.Rock(0.22f + 0.06f * Mathf.Sin(i * 1.9f + k), 590 + i * 2 + k), RockTop,
                    new Vector3(rx, y + h - 0.02f, z + back + d * (0.45f + 0.15f * k)));
                top.localScale = new Vector3(1.6f, 0.32f, 1.1f);
            }
            // on yuzde iki ince koyu tortu bandi (hafif egik)
            foreach (float t in new[] { 0.33f, 0.68f })
            {
                var band = Parts.Add("Tortu", root, MeshFactory.RoundedBox(new Vector3(w * 0.9f, 0.022f, 0.01f), 0.004f), Strata,
                    new Vector3(cx, y + h * t, z + back - 0.006f), outline: false, castShadow: false);
                band.localRotation = Quaternion.Euler(0f, 0f, 1.5f * Mathf.Sin(i * 2f + t * 7f));
            }
            x += w;
        }
        var face = Parts.Empty("Yuzey", root);
        face.localPosition = new Vector3(0f, ground(0f, z), z - 0.08f);
        return face;
    }

    // Bolum 60. z: kenarin cizgisi; pathX: patikanin kenari kestigi yer. Patikanin iki yaninda isaret diregi (yaninda kaya yok: Enes,
    // yarigin cevresinde kaya gorunumu istemiyor). Doner: patikanin basindaki zemin yuzeyi (yere yatik: yerel -Z yukari, +Y kanyona
    // dogru; ok buraya).
    public static Transform Edge(Transform parent, float z, float pathX, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Iz-KanyonKenari", parent);
        foreach (float side in new[] { -1f, 1f })
            MarkerPost(root, new Vector3(pathX + side * PathMouth, 0f, z - 0.15f), ground);   // yarigin basinin hemen onunde

        var surface = Parts.Empty("PatikaBasi", root);
        float sz = z - 0.6f;
        surface.localPosition = new Vector3(pathX, ground(pathX, sz) + 0.005f, sz);
        surface.localRotation = Quaternion.Euler(90f, 0f, 0f);
        return surface;
    }

    const float PathMouth = 0.42f;   // patika agzinin yari genisligi (direkler bunun kenarinda)

    // Patikanin iki yanindaki isaret diregi: telefonda secilecek boyda; iki turuncu bant arasinda beyaz yansitici, tepede
    // her isikta net amber fener (yola "kapi" gibi bakar)
    static readonly Material ReflectorMat = Mats.Lit(Mats.Hex("#E8E2D4"), 0.4f);
    static readonly Material LampMat = Mats.Unlit(Mats.Hex("#FFB547"));

    static void MarkerPost(Transform root, Vector3 pos, Func<float, float, float> ground)
    {
        const float H = 0.95f;
        float y = ground(pos.x, pos.z);
        Parts.Add("Direk", root, MeshFactory.RoundedCylinder(0.065f, H, 0.014f, 12), PostMat, new Vector3(pos.x, y + H * 0.5f, pos.z));
        foreach (var (top, height, mat) in new[] { (H - 0.1f, 0.12f, PaintMarks.PaintMat(0f)), (H - 0.26f, 0.07f, ReflectorMat), (H - 0.42f, 0.12f, PaintMarks.PaintMat(0f)) })
            Parts.Add("Bant", root, MeshFactory.RoundedCylinder(0.072f, height, 0.01f, 12), mat, new Vector3(pos.x, y + top, pos.z), outline: false);
        Parts.Add("Fener", root, MeshFactory.Sphere(0.07f, 8, 12), LampMat, new Vector3(pos.x, y + H + 0.04f, pos.z), outline: false);
    }
}
