using System;
using System.Collections.Generic;
using UnityEngine;

// Bolum 60 yariginin 3B bicimi (saf hesap; ag kurmaz, RiftMeshes kurar). t: yarigin basindan uzaklik (z = startZ + t);
// s: -1 sol duvar, +1 sag duvar. Duvarlar yatay tortu katmanlari: her katman ustundekinden biraz iceride baslar (basamak),
// yuzu kendi duzensizliginde one/arkaya kayar; ince gurultu kaba kaya yuzeyi verir. Katmanlar ortadaki zemine paralel: iki
// duvarda ayni hizadadir. Sag duvara patika rafi oyulmustur: raf kenardan dibe iner; bas tarafta ustu acik (duvar raf boyunca
// geride), derinde sacak altinda (duvar rafin ustunde iceri oyuk).
public class RiftShape
{
    public const float LedgeReach = 5f;          // raf dibe burada varir
    public const float LipOut = 0.2f;            // rafin duvardan disari tasan kismi (dis dudak); raf = oyuk + dudak = 0,5
    public const float LedgeHeadroom = 0.75f;    // raf ile sacak arasi (Kivilcim ~0,55 boyunda)
    public const float LedgeThickness = 0.16f;   // dudagin on yuzunun yuksekligi (altinda karanlik alt yuz)
    public const float BendRockT = 6.2f;         // donemec kayasi (sol duvara yaslanmis iri kaya)
    // dipte patika: kayanin sagindan arkasina, sonra dibin kenarindan asagi (CanyonRift.DropT)
    public static readonly float[] FloorTrail = { 5.5f, 6.0f, 6.5f, 6.9f, 7.3f, 7.7f, 8.1f, 8.6f };
    const float FloorHalfMin = 0.3f;             // dip en az 0,6 genis (robot gecer)

    // Tortu katmanlari, ustten alta (ortadaki zeminden derinlik): kalinlik, ust sinirdaki basamak (iceri kayma), renk.
    // Sira tekrar eder; birkac genis basamakta dusmus kayalar durur (RiftMeshes).
    public const int ToneBody = 0, ToneDark = 1, ToneLight = 2, Tones = 3;
    static readonly (float thickness, float step, int tone)[] LayerCycle =
    {
        (0.30f, 0.00f, ToneBody),    // ust kaya: zemin
        (0.11f, 0.06f, ToneLight),
        (0.40f, 0.03f, ToneBody),
        (0.22f, 0.11f, ToneDark),    // genis basamak
        (0.09f, 0.02f, ToneLight),
        (0.48f, 0.05f, ToneBody),
        (0.26f, 0.04f, ToneDark),
        (0.13f, 0.03f, ToneLight),
        (0.52f, 0.10f, ToneBody),    // genis basamak
        (0.28f, 0.04f, ToneDark),
    };
    const float StepRise = 0.05f;    // basamagin egik ust yuzunun yuksekligi

    readonly float startZ;
    readonly Func<float, float, float> ground;
    readonly List<float> layerTops = new List<float>();
    readonly List<float> layerSteps = new List<float>();
    readonly List<int> layerTones = new List<int>();

    public float PathX { get; }

    public RiftShape(float startZ, float pathX, Func<float, float, float> ground)
    {
        this.startZ = startZ;
        PathX = pathX;
        this.ground = ground;
        float top = 0f;
        for (int i = 0; top < 12f; i++)
        {
            var layer = LayerCycle[i % LayerCycle.Length];
            layerTops.Add(top);
            layerSteps.Add(i == 0 ? 0f : layer.step);
            layerTones.Add(layer.tone);
            top += layer.thickness;
        }
    }

    public float Ground(float x, float z) => ground(x, z);
    public float Z(float t) => startZ + t;
    public float CenterX(float t) => CanyonRift.CenterX(t, PathX);

    // Zemin kesmesinin kenari (Ground.shader riftClip ile ayni) ve oradaki zemin yuksekligi
    public float RimX(float t, float s) => CenterX(t) + s * (CanyonRift.HalfWidth(t) + CanyonRift.RimOffset(t, s));
    public float RimY(float t, float s) => ground(RimX(t, s), Z(t));
    // Katmanlarin olcu yuzeyi (ortadaki zemin) ve dip
    public float LayerTopY(float t) => ground(CenterX(t), Z(t));
    public float FloorY(float t) => LayerTopY(t) - CanyonRift.Depth(t);

    // ---- Katmanlar ----
    public int LayerCount => layerTops.Count;
    public float LayerTop(int k) => layerTops[k];
    public float LayerStep(int k) => layerSteps[k];
    public int ToneAt(float depthBelowTop) => layerTones[LayerAt(depthBelowTop)];

    int LayerAt(float depth)
    {
        int k = 0;
        while (k + 1 < layerTops.Count && layerTops[k + 1] <= depth) k++;
        return k;
    }

    // Katmanlarin ustten toplam iceri kaymasi (basamaklar + hafif geriye yatik duvar)
    float StrataInward(float depth)
    {
        float inward = 0.03f * Mathf.Max(depth, 0f);
        for (int k = 1; k < layerTops.Count && layerTops[k] < depth; k++)
            inward += layerSteps[k] * Mathf.SmoothStep(0f, 1f, (depth - layerTops[k]) / StepRise);
        return inward;
    }

    // Her katmanin yuzu yarik boyunca kendi duzensizliginde one/arkaya kayar; sinirda komsusuna yumusak gecer
    float LayerWobble(float depth, float t, float s)
    {
        int k = LayerAt(depth);
        float w = Wobble(k, t, s);
        float intoLayer = depth - layerTops[k];
        if (k > 0 && intoLayer < StepRise) w = Mathf.Lerp(Wobble(k - 1, t, s), w, Mathf.SmoothStep(0f, 1f, intoLayer / StepRise));
        return w;
    }

    static float Wobble(int k, float t, float s) => 0.05f * (2f * Mathf.PerlinNoise(t * 0.85f + k * 3.7f + s * 11f, k * 1.9f) - 1f);

    // Kaba kaya yuzeyi (iki olcekte gurultu)
    float Roughness(float t, float s, float y) =>
        0.03f * (2f * Mathf.PerlinNoise(Z(t) * 2.9f + s * 7f, y * 3.3f + 5f) - 1f)
      + 0.015f * (2f * Mathf.PerlinNoise(Z(t) * 7.3f + s * 3f, y * 8.1f + 1f) - 1f);

    // Duvarin kenardan ortaya dogru kaymasi (y yuksekliginde); kenarda sifir: duvar zemin kesmesinin tam kenarindan iner
    float Inward(float t, float s, float y)
    {
        float rimY = RimY(t, s), top = LayerTopY(t);
        float strata = StrataInward(top - y) - StrataInward(top - rimY);
        float fromRim = Mathf.SmoothStep(0f, 1f, (rimY - y) / 0.15f);
        float inward = strata + (LayerWobble(top - y, t, s) + Roughness(t, s, y)) * fromRim;
        float hw = CanyonRift.HalfWidth(t);
        return Mathf.Min(inward, hw - Mathf.Min(FloorHalfMin, 0.6f * hw));   // yarigin sivri basinda dip de daralir
    }

    // ---- Sag duvardaki patika rafi ----
    public float LedgeY(float t)
    {
        if (t >= LedgeReach) return FloorY(t);
        float x = Mathf.Clamp01(t / LedgeReach), f = 0.6f * Mathf.SmoothStep(0f, 1f, x) + 0.4f * x;   // basta ve sonda yumusak
        return Mathf.Lerp(RimY(t, 1f), FloorY(t), f);
    }

    // Sag duvarin y'deki oyugu (disari): rafin ustunde sacaga kadar; bas tarafta rafin ustu tamamen acik (RimSetback)
    float Recess(float t, float y)
    {
        float ledge = LedgeY(t);
        float aboveLedge = Mathf.SmoothStep(0f, 1f, (y - ledge + 0.03f) / 0.06f);
        float underEaves = 1f - Mathf.SmoothStep(0f, 1f, (y - ledge - LedgeHeadroom + 0.08f) / 0.08f);
        float open = CanyonRift.RimSetback(t) / CanyonRift.LedgeRecess;
        float fade = 1f - Mathf.SmoothStep(0f, 1f, (t - LedgeReach) / 0.8f);   // dipte oyuk biter
        return CanyonRift.LedgeRecess * aboveLedge * Mathf.Max(underEaves, open) * fade;
    }

    // Duvar yuzunun x'i. Ustte (kenarda) 1,5 cm zeminin altina tasar: zemin kesmesiyle duvar arasinda aralik kalmaz.
    public float WallX(float t, float s, float y)
    {
        float rimX = CenterX(t) + s * (CanyonRift.HalfWidth(t) + CanyonRift.RimJitter(t, s));
        float tuck = 0.015f * (1f - Mathf.SmoothStep(0f, 1f, (RimY(t, s) - y) / 0.05f));
        float recess = s > 0f ? Recess(t, y) : 0f;
        return rimX + s * (tuck + recess - Inward(t, s, y));
    }

    public Vector3 WallPoint(float t, float s, float y) => new Vector3(WallX(t, s, y), y, Z(t));

    // Rafin arka kenari (oyugun dibi), dis dudagi (duvarin rafin altindaki yuzunden LipOut disarida) ve ortasi
    public float LedgeBackX(float t) => WallX(t, 1f, LedgeY(t) + 0.1f) + 0.02f;
    public float LedgeLipX(float t) => WallX(t, 1f, LedgeY(t) - 0.06f) - LipOut;
    public Vector3 LedgeCenter(float t) => new Vector3(0.5f * (LedgeBackX(t) + LedgeLipX(t) + 0.08f), LedgeY(t), Z(t));

    // ---- Dip ----
    public float FloorLeftX(float t) => WallX(t, -1f, FloorY(t));
    public float FloorRightX(float t) => WallX(t, 1f, FloorY(t));

    // Dipteki patika: donemec kayasina kadar sag yanda, sonra kayanin arkasina (sol duvara) gecer
    public Vector3 FloorTrailPoint(float t)
    {
        float behind = Mathf.SmoothStep(0f, 1f, (t - BendRockT - 0.1f) / 0.6f);
        float x = Mathf.Lerp(FloorRightX(t) - 0.3f, FloorLeftX(t) + 0.3f, behind);
        return new Vector3(x, FloorY(t), Z(t));
    }

    public Vector3 BendRockPoint() => new Vector3(FloorLeftX(BendRockT) + 0.2f, FloorY(BendRockT), Z(BendRockT));

    // ---- Golgelenme (gok gorunurlugu, 0..1): derin ve dar yerde kapali, kenarda acik. Isigi degil ortam isigini kisar. ----
    public float Shade(float t, float y)
    {
        float depth = Mathf.Max(RimY(t, -1f), RimY(t, 1f)) - y;
        float width = Mathf.Max(FloorRightX(t) - FloorLeftX(t), 0.3f);
        float open = width / (width + 2f * Mathf.Max(depth, 0f));
        return Mathf.Lerp(0.12f, 1f, open);
    }

    // Oyugun icinde (sacak altinda) ek golge
    public float RecessShade(float t, float y) => 1f - 0.45f * Recess(t, y) / CanyonRift.LedgeRecess * (1f - CanyonRift.RimSetback(t) / CanyonRift.LedgeRecess);
}
