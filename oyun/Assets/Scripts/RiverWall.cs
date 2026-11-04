using System;
using UnityEngine;

// Bolge 7'de alana yakin kaya duvar parcasi (65-66 yan kanyonun agzi, 67-70 arka duvar). Kodla cizilmis, su asindirmis tortu
// kayasi: ust uste ince yatay katmanlar (acik/koyu), altta suyun oydugu yatay girinti, katmanlar duvar boyunca hafif one-arkaya
// kayar; ust kenar kirik bloklar halinde. Sabah gunesi dibe inmemis: govde golgede (serin kahve-mor); yalniz yuksek duvarin
// (sunlit) en ust ince seridi ve tepesi turuncu aydinlik (RiverSky.hlsl ile ayni fikir), alcak kayalar tamamen golgede.
// Her kolon zeminin gercek yuksekligine oturur.
public static class RiverWall
{
    static readonly Material Body = Mats.Lit(Mats.Hex("#7A6466"), 0.1f);
    static readonly Material Band = Mats.Lit(Mats.Hex("#88736C"), 0.1f);     // acik tortu tabakasi
    static readonly Material Hollow = Mats.Lit(Mats.Hex("#594A4E"), 0.08f);   // suyun oydugu girinti: derin golge
    static readonly Material Sunlit = Mats.Emissive(Mats.Hex("#A8785C"), Mats.Hex("#2E160A"), 0.1f);

    enum Layer { Body, Band, Hollow, Sunlit }

    // Katmanlar alttan uste: yukseklik payi, one/arkaya kayma (+ arkaya) ve gorunus. Ikinci koyu katman suyun oydugu girinti.
    static readonly float[] LayerShare = { 0.16f, 0.08f, 0.12f, 0.22f, 0.1f, 0.2f, 0.12f };
    static readonly float[] LayerSetback = { 0f, 0.03f, 0.11f, 0.02f, 0.05f, 0.04f, 0.06f };
    static readonly Layer[] LayerLook = { Layer.Body, Layer.Band, Layer.Hollow, Layer.Body, Layer.Band, Layer.Body, Layer.Sunlit };

    // Duvara resim/yazi konacak bolge (Bolum 70 duvar resmi): duvar yuzu orada one cikmaz (el ve kalem katmana girmesin) ve
    // yazinin oldugu aralikta tepe en az MinTop yukseklikte kalir (engebeli ust kenar yazinin altina dusup yaziyi tasin disina cikarmasin).
    // x'ler duvarin yerel x'inde, MinTop kokun y'sinde (on yuzun y'siyle ayni); sinirlarda GuardFade kadar yumusak gecis.
    public readonly struct Guard
    {
        public readonly float FlatFrom, FlatTo, TallFrom, TallTo, MinTop;
        public Guard(float flatFrom, float flatTo, float tallFrom, float tallTo, float minTop)
        {
            FlatFrom = flatFrom; FlatTo = flatTo; TallFrom = tallFrom; TallTo = tallTo; MinTop = minTop;
        }
    }

    const float GuardFade = 0.35f;     // korumali bolgenin kenarinda yumusak gecis boyu

    const float Depth = 0.24f;         // duvarin tepesinin derinligi (yukaridan bakan kamera tepeyi ince serit gorur)
    const float SegmentLength = 0.16f; // ust kenarin kiriklari ve zemine oturma bu aralikla
    const float BlockLength = 0.7f;    // ust kenardaki kirik bloklarin boyu

    // baseCenter: on yuzun ortasinin zemindeki yeri (y kullanilmaz); length: boyu; height: yuksekligi; yawDeg: donusu
    // (0: on yuz kameraya, -Z'ye bakar); sunlit: ust serit gunes alir (yuksek duvar). Doner: on yuzun duzlemi (yerel -Z kameraya
    // bakar, y = 0 zeminin ortadaki yuksekligi); el izleri ve duvar resmi buraya konur.
    public static Transform Build(Transform parent, Vector3 baseCenter, float length, float height, float yawDeg, bool sunlit, int seed,
        Func<float, float, float> ground, Guard? guard = null)
    {
        var root = Parts.Empty("Iz-KayaDuvar", parent);
        var rotation = Quaternion.Euler(0f, yawDeg, 0f);
        float baseY = ground(baseCenter.x, baseCenter.z);
        root.localPosition = new Vector3(baseCenter.x, baseY, baseCenter.z);
        root.localRotation = rotation;

        // yerel x'teki kolonun zemin yuksekligi (koke gore); biraz gomulur: dibinde bosluk kalmasin
        float Floor(float lx)
        {
            var w = baseCenter + rotation * new Vector3(lx, 0f, 0f);
            return ground(w.x, w.z) - baseY - 0.06f;
        }

        // katmanin one/arkaya kaymasi; korumali bolgede hicbir katman duvar yuzunden one cikmaz
        float SetbackAt(float x, int layer)
        {
            float s = Setback(x, layer, seed);
            return guard.HasValue ? Mathf.Lerp(s, Mathf.Max(s, 0f), GuardWeight(x, guard.Value.FlatFrom, guard.Value.FlatTo)) : s;
        }

        var meshes = new FlatMesh[4];
        for (int i = 0; i < meshes.Length; i++) meshes[i] = new FlatMesh();
        Layer topLook = sunlit ? Layer.Sunlit : Layer.Body;
        int columns = Mathf.Max(2, Mathf.CeilToInt(length / SegmentLength));
        for (int c = 0; c < columns; c++)
        {
            float x0 = -length * 0.5f + length * c / columns, x1 = -length * 0.5f + length * (c + 1) / columns;
            float f0 = Floor(x0), f1 = Floor(x1);
            float t0 = Taper(x0, length), t1 = Taper(x1, length);
            float y0 = 0f, y1 = 0f;   // katmanin alt kenarinin zemin ustu yuksekligi (iki ucta)
            for (int k = 0; k < LayerShare.Length; k++)
            {
                bool top = k == LayerShare.Length - 1;
                float h0 = height * LayerShare[k] * Wobble(x0, k, seed), h1 = height * LayerShare[k] * Wobble(x1, k, seed);
                if (top) { h0 += TopProfile(x0, seed) * height; h1 += TopProfile(x1, seed) * height; }
                h0 *= t0; h1 *= t1;
                if (top && guard.HasValue)   // tepe, yazinin altinda kalmasin: katmanin ustu en az MinTop'a ulasir
                {
                    var g = guard.Value;
                    h0 = Mathf.Max(h0, GuardWeight(x0, g.TallFrom, g.TallTo) * (g.MinTop - f0 - y0));
                    h1 = Mathf.Max(h1, GuardWeight(x1, g.TallFrom, g.TallTo) * (g.MinTop - f1 - y1));
                }
                float z0 = SetbackAt(x0, k), z1 = SetbackAt(x1, k);
                var a = new Vector3(x0, f0 + y0, z0);
                var b = new Vector3(x1, f1 + y1, z1);
                var cTop = new Vector3(x1, f1 + y1 + h1, z1);
                var dTop = new Vector3(x0, f0 + y0 + h0, z0);
                meshes[(int)(top ? topLook : LayerLook[k])].Quad(a, b, cTop, dTop, Vector3.back);
                // katmanin ustu: bir ustteki katman geride duruyorsa basamak gorunur (en ustte duvarin tepesi, gunes alir)
                float n0 = top ? Depth : SetbackAt(x0, k + 1), n1 = top ? Depth : SetbackAt(x1, k + 1);
                meshes[(int)(top ? topLook : LayerLook[k])].Quad(dTop, cTop, new Vector3(x1, cTop.y, Mathf.Max(n1, z1)),
                    new Vector3(x0, dTop.y, Mathf.Max(n0, z0)), Vector3.up);
                y0 += h0; y1 += h1;
            }
            if (c == 0) EndCap(meshes[(int)Layer.Body], x0, f0, y0, -1f);
            if (c == columns - 1) EndCap(meshes[(int)Layer.Body], x1, f1, y1, 1f);
        }
        Parts.Add("Govde", root, meshes[(int)Layer.Body].ToMesh("RiverWall"), Body, Vector3.zero, outline: false);
        Parts.Add("Tabaka", root, meshes[(int)Layer.Band].ToMesh("RiverWallBand"), Band, Vector3.zero, outline: false);
        Parts.Add("Oyuk", root, meshes[(int)Layer.Hollow].ToMesh("RiverWallHollow"), Hollow, Vector3.zero, outline: false);
        if (sunlit) Parts.Add("Gunesli", root, meshes[(int)Layer.Sunlit].ToMesh("RiverWallLit"), Sunlit, Vector3.zero, outline: false);

        var face = Parts.Empty("Yuzey", root);
        face.localPosition = new Vector3(0f, 0f, -0.01f);
        return face;
    }

    // Korumali aralikta 1, disinda GuardFade boyunca yumusakca 0
    static float GuardWeight(float x, float from, float to) =>
        Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1f - Mathf.Max(from - x, x - to, 0f) / GuardFade));

    // duvarin yan ucu: on yuzden arkaya kapatir
    static void EndCap(FlatMesh mesh, float x, float floor, float top, float side)
    {
        var outward = new Vector3(side, 0f, 0f);
        mesh.Quad(new Vector3(x, floor, 0f), new Vector3(x, floor, Depth), new Vector3(x, floor + top, Depth), new Vector3(x, floor + top, 0f), outward);
    }

    // iki uca dogru duvar yere iner (kesik kutu ucu gibi durmasin: asinmis kaya cikintisi)
    static float Taper(float x, float length)
    {
        float span = Mathf.Min(0.7f, length * 0.25f);
        return Mathf.Lerp(0.2f, 1f, Mathf.SmoothStep(0f, 1f, (length * 0.5f - Mathf.Abs(x)) / span));
    }

    // katmanin one/arkaya kaymasi: duvar boyunca yumusak dalga (her katman kendi dalgasiyla)
    static float Setback(float x, int layer, int seed) =>
        LayerSetback[layer] + 0.06f * (Mathf.PerlinNoise(x * 1.1f + seed * 0.37f, layer * 1.9f) - 0.5f)
        + 0.1f * (Mathf.PerlinNoise(x * 0.45f + seed * 0.53f, 7.3f) - 0.5f);   // genis cikinti ve girintiler: butun katmanlar birlikte

    // katman kalinligi duvar boyunca degisir (dogal tortu)
    static float Wobble(float x, int layer, int seed) =>
        1f + 0.35f * (Mathf.PerlinNoise(x * 0.9f + layer * 3.7f, seed * 0.21f) - 0.5f);

    // ust kenar (yukseklige oranla): genis dalga + kirik bloklar (her blok kendi yuksekliginde, aralarda kisa gecis)
    static float TopProfile(float x, int seed)
    {
        float u = x / BlockLength + seed * 0.13f;
        float i = Mathf.Floor(u), f = u - i;
        float block = Mathf.Lerp(Hash(i, seed), Hash(i + 1f, seed), Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.82f, 1f, f)));
        return 0.2f * (Mathf.PerlinNoise(x * 0.6f, seed * 0.11f) - 0.5f) + 0.14f * (block - 0.5f);
    }

    static float Hash(float i, int seed) => Mathf.Abs(Mathf.Sin(i * 12.9898f + seed * 78.233f) * 43758.5453f) % 1f;
}
