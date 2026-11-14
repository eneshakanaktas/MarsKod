using System;
using System.Collections.Generic;
using UnityEngine;

// Bolge 8 · Istasyonun ic mekani (72-80): tam kapali oda, gokyuzu gorunmez (docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md, K1).
// Kamera yukaridan diktir: dik duvarin yalniz alt ~2 birimi ekranin gorunen kismina duser, ustu baslik ve dugmelerin arkasinda kalir.
// Bu yuzden duvar bantlidir:
//   0-1,2  acik gri-krem panel (cocuk boyu): cizimler, harita, bilgisayarlar buraya konur (BackWallFace).
//   1,2-1,35 metal korkuluk + ince koloni turuncusu serit.
//   1,35-2,1 koyu gri-mavi bant: duvar lambalari, havalandirma izgaralari.
//   2,1 ustu koyu, sade (ust yazilarin arkasi): yalniz yatay derzler; tavan kirisleri burada one uzanir.
// Duvarlar cok yuksek ve yanlara uzun: klavye + ipucu acikken kamera geri cekilir, yine her yer oda (gokyuzu acilmaz).
// Lambalar parlayan malzeme + zemine yatik sahte isik lekesi (LightPool); gercek nokta isik en fazla bir (yanikken, hafif modda yok).
// Duvarlar golge dusurmez (yukseklikleri yuzunden alani karartirlardi).
public static class StationRoom
{
    // IKIZ: Ground.shader STATION_BACK_GAP / STATION_SIDE_GAP (zemindeki toz duvar dibinde kalinlasir)
    public const float BackGap = 0.8f;    // alanin arka kenari -> arka duvarin on yuzu
    public const float SideGap = 0.45f;   // alanin yan kenari -> yan duvarin ic yuzu

    public const float PanelTop = 1.2f;   // acik bandin ustu (cizimler, harita bunun altinda kalir)
    const float RailTop = 1.35f, LampBandTop = 2.1f, LampY = 1.66f;
    const float WallHeight = 60f, Thickness = 0.15f, FrontReach = 30f;   // kameradan yuksek: genis gorunumde de duvarin ustu gorunmez
    const float BeamY = 4.6f, BeamReach = 1.8f;

    static readonly float[] LampX = { -2.2f, 0f, 2.2f };
    const float EmergencyX = -1.1f;

    static readonly Color LampOff = Mats.Hex("#3A3D44");
    static readonly Color LampOn = Mats.Hex("#F2F5F8");
    static readonly Color LampGlow = Mats.Hex("#DCE6F2");
    static readonly Color PoolColor = Mats.Hex("#FFF1DC");
    static readonly Color EmergencyRed = Mats.Hex("#FF4A2E");
    public static readonly Color ColonyOrange = Mats.Hex("#D2692F");   // koloninin seritleri (cephe, korkuluk, kasa etiketi)

    const float PoolOn = 0.3f, EmergencyPoolOn = 0.22f;

    // Kurulan oda: arka duvarin on yuzu (yerel -Z kameraya bakar, y = 0 zemin) ve lambalar (73 bitince yanar: Gorev 3).
    public sealed class Room
    {
        public Transform BackWallFace { get; }
        public IReadOnlyList<Renderer> Lamps => lamps;
        readonly List<Renderer> lamps;
        readonly List<Material> pools;
        readonly Renderer emergency;
        readonly Material emergencyPool;
        readonly Light point;
        readonly StationLamps switcher;

        internal Room(Transform face, List<Renderer> lamps, List<Material> pools, Renderer emergency, Material emergencyPool, Light point)
        {
            BackWallFace = face;
            switcher = StationLamps.Attach(face.parent, this);
            this.lamps = lamps;
            this.pools = pools;
            this.emergency = emergency;
            this.emergencyPool = emergencyPool;
            this.point = point;
        }

        // 73 bitince: lambalar sirayla kirpisarak yanar, isik lostan aydinliga gecer, acil isik soner (StationLamps)
        public void TurnOn(Light sky, Light sun, RegionLook from, RegionLook to) => switcher.TurnOn(sky, sun, from, to);

        // Tavan lambalari yanik (aydinlik) ya da sonuk (yalniz kirmizi-turuncu acil isik); yarim kalan yanma durur
        public void SetLights(bool on)
        {
            switcher.Stop();
            for (int i = 0; i < lamps.Count; i++) SetLamp(i, on ? 1f : 0f);
            SetEmergency(!on);
        }

        // Acil isik yanik mi (lambalar sonukken); denetim icin
        public bool EmergencyOn { get; private set; }

        // Kirmizi-turuncu acil isik ve gercek nokta isik (yalniz lambalar yanikken)
        internal void SetEmergency(bool on)
        {
            EmergencyOn = on;
            emergency.sharedMaterial.SetColor("_EmissionColor", on ? EmergencyRed * 2.2f : Color.black);
            emergency.sharedMaterial.SetColor("_BaseColor", on ? EmergencyRed : LampOff);
            emergencyPool.SetFloat("_A", on ? EmergencyPoolOn : 0f);
            if (point != null) point.enabled = !on;
        }

        // Tek lambanin parlakligi (0 sonuk, 1 yanik); kirpisarak yanma icin
        public void SetLamp(int index, float level)
        {
            var mat = lamps[index].sharedMaterial;   // her lensin kendi malzemesi var
            mat.SetColor("_BaseColor", Color.Lerp(LampOff, LampOn, level));
            mat.SetColor("_EmissionColor", LampGlow * (1.6f * level));
            pools[index].SetFloat("_A", PoolOn * level);
        }
    }

    // backClutter: arka duvar dibine terk edilmislik (kum, kagit, kablo) konsun mu; esyali bolumlerde (yatak, tezgah) false: cakismasin
    public static Room Build(Transform parent, Vector2 areaHalf, bool lightsOn, bool backClutter, int seed, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Istasyon-Oda", parent);
        float backZ = areaHalf.y + BackGap, sideX = areaHalf.x + SideGap;

        BackWall(root, sideX, backZ);
        foreach (float side in new[] { -1f, 1f }) SideWall(root, side, sideX, backZ);
        Beams(root, sideX, backZ);

        var lamps = new List<Renderer>();
        var pools = new List<Material>();
        foreach (float x in LampX)
        {
            lamps.Add(WallLamp(root, new Vector3(x, LampY, backZ), new Vector3(0.62f, 0.13f, 0.08f)));
            pools.Add(LightPool.Create(root, new Vector3(x, ground(x, backZ - 0.5f), backZ - 0.5f), new Vector2(1.7f, 1.0f), PoolColor, 0f, "Lamba-Isigi"));
        }
        var emergency = WallLamp(root, new Vector3(EmergencyX, LampY + 0.02f, backZ), new Vector3(0.16f, 0.1f, 0.07f));
        var emergencyPool = LightPool.Create(root, new Vector3(EmergencyX, ground(EmergencyX, backZ - 0.35f), backZ - 0.35f), new Vector2(1.0f, 0.6f),
            EmergencyRed, 0f, "Acil-Isik");

        Light point = null;
        if (!GraphicsQuality.Performance)
        {
            point = new GameObject("Tavan-Isigi").AddComponent<Light>();
            point.transform.SetParent(root, false);
            point.transform.localPosition = new Vector3(0f, 1.9f, backZ - 1.0f);
            point.type = LightType.Point;
            point.range = 6f;
            point.intensity = 1.3f;
            point.color = LampGlow;
            point.shadows = LightShadows.None;
        }

        Neglect(root, areaHalf, sideX, backZ, backClutter, seed, ground);

        var face = Parts.Empty("Yuzey", root);
        face.localPosition = new Vector3(0f, 0f, backZ - 0.012f);
        var room = new Room(face, lamps, pools, emergency, emergencyPool, point);
        room.SetLights(lightsOn);
        return room;
    }

    static readonly Material Panel = Mats.Lit(Mats.Hex("#BDB5A6"), 0.2f);      // acik gri-krem
    static readonly Material PanelSeam = Mats.Lit(Mats.Hex("#8E877C"), 0.2f);
    static readonly Material Base = Mats.Lit(Mats.Hex("#34363C"), 0.3f);       // supurgelik
    static readonly Material Rail = Mats.Lit(Mats.Hex("#5A5E66"), 0.45f);
    static readonly Material Stripe = Mats.Lit(ColonyOrange, 0.25f);
    static readonly Material LampBand = Mats.Lit(Mats.Hex("#454B57"), 0.3f);
    static readonly Material Upper = Mats.Lit(Mats.Hex("#2A2D35"), 0.2f);      // ust: koyu, sade
    static readonly Material Joint = Mats.Lit(Mats.Hex("#22252C"), 0.2f);
    static readonly Material BeamMat = Mats.Lit(Mats.Hex("#202228"), 0.3f);
    static readonly Material Housing = Mats.Lit(Mats.Hex("#26282E"), 0.4f);

    // Duvarin bantlari: bir duz yuzey (on yuzu z = 0'da, -Z'ye bakar) boyunca, x: -half..half. depthOut: one cikma.
    static void Bands(Transform wall, float half)
    {
        float width = half * 2f;
        Slab(wall, "Supurgelik", Base, width, -0.1f, 0.08f, 0.02f);
        Slab(wall, "Panel", Panel, width, 0.08f, PanelTop, 0.01f);
        // panel derzleri: genis araliklarla (cizim alanini bolmesin)
        for (float x = -half + 1.7f; x < half - 0.3f; x += 1.7f)
            Parts.Add("Derz", wall, MeshFactory.RoundedBox(new Vector3(0.018f, PanelTop - 0.1f, 0.01f), 0.004f), PanelSeam,
                new Vector3(x, (PanelTop + 0.1f) * 0.5f, -0.012f), outline: false, castShadow: false);
        Slab(wall, "Korkuluk", Rail, width, PanelTop, RailTop, 0.03f);
        Slab(wall, "Serit", Stripe, width, PanelTop + 0.05f, PanelTop + 0.09f, 0.035f);
        Slab(wall, "LambaBandi", LampBand, width, RailTop, LampBandTop, 0f);
        Slab(wall, "Ust", Upper, width, LampBandTop, WallHeight, -0.01f);
        for (float y = 2.9f; y < 9f; y += 1.1f)
            Slab(wall, "Ust-Derz", Joint, width, y, y + 0.03f, -0.005f);
    }

    // Duvar boyunca yatay serit: y0..y1, on yuzu z = -depthOut (one cikma), arkaya Thickness kalin
    static void Slab(Transform wall, string name, Material mat, float width, float y0, float y1, float depthOut)
    {
        float h = y1 - y0;
        Parts.Add(name, wall, MeshFactory.RoundedBox(new Vector3(width, h, Thickness), 0.005f, 1), mat,
            new Vector3(0f, y0 + h * 0.5f, Thickness * 0.5f - depthOut), outline: false, castShadow: false);
    }

    static void BackWall(Transform root, float sideX, float backZ)
    {
        var wall = Parts.Empty("ArkaDuvar", root);
        wall.localPosition = new Vector3(0f, 0f, backZ);
        Bands(wall, sideX);
        // havalandirma izgaralari (lamba bandinda, lambalarin arasinda)
        foreach (float x in new[] { 1.1f, -3.0f })
            Vent(wall, new Vector3(x, LampY, 0f));
    }

    static void SideWall(Transform root, float side, float sideX, float backZ)
    {
        float length = backZ + FrontReach;
        var wall = Parts.Empty("YanDuvar", root);
        wall.localPosition = new Vector3(side * sideX, 0f, backZ - length * 0.5f);
        wall.localRotation = Quaternion.Euler(0f, side * 90f, 0f);   // on yuzu (-Z) odanin ortasina bakar
        Bands(wall, length * 0.5f);
    }

    // Arka duvarin ustunden one uzanan kirisler + onlari baglayan enine kiris (ust yazilarin arkasinda kalir: koyu, sade)
    static void Beams(Transform root, float sideX, float backZ)
    {
        foreach (float x in LampX)
            Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(0.24f, 0.3f, BeamReach), 0.02f, 2), BeamMat,
                new Vector3(x, BeamY, backZ - BeamReach * 0.5f), outline: false, castShadow: false);
        Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(sideX * 2f, 0.28f, 0.24f), 0.02f, 2), BeamMat,
            new Vector3(0f, BeamY, backZ - BeamReach), outline: false, castShadow: false);
    }

    // Duvar lambasi: koyu govde, onde lens (kendi malzemesi: tek tek yanar). at: duvarin on yuzunde orta.
    static Renderer WallLamp(Transform root, Vector3 at, Vector3 size)
    {
        Parts.Add("Lamba-Govde", root, MeshFactory.RoundedBox(size + new Vector3(0.05f, 0.05f, 0f), 0.02f, 2), Housing,
            at + new Vector3(0f, 0f, -size.z * 0.5f), outline: false, castShadow: false);
        var lens = Parts.Add("Lamba", root, MeshFactory.RoundedBox(new Vector3(size.x, size.y, 0.02f), 0.015f, 2),
            Mats.Emissive(LampOff, Color.black, 0.6f), at + new Vector3(0f, 0f, -size.z - 0.01f), outline: false, castShadow: false);
        return lens.GetComponent<Renderer>();
    }

    static void Vent(Transform wall, Vector3 at)
    {
        Parts.Add("Izgara", wall, MeshFactory.RoundedBox(new Vector3(0.42f, 0.26f, 0.03f), 0.01f, 2), Housing, at + new Vector3(0f, 0f, -0.015f),
            outline: false, castShadow: false);
        for (int k = 0; k < 4; k++)
            Parts.Add("Izgara-Cubuk", wall, MeshFactory.RoundedBox(new Vector3(0.38f, 0.025f, 0.02f), 0.006f, 1), Rail,
                at + new Vector3(0f, -0.09f + k * 0.06f, -0.035f), outline: false, castShadow: false);
    }

    static readonly Material Drift = Mats.Lit(Mats.Hex("#76665A"), 0.05f);   // zemindeki tozla ayni renk ailesi
    static readonly Material Paper = Mats.Lit(Mats.Hex("#D8D2C2"), 0.2f);
    static readonly Material Cable = Mats.Lit(Mats.Hex("#1E1E22"), 0.3f);

    // Terk edilmislik (alanin disinda, zemine basar): duvar diplerinde kum birikintisi, yere dusmus kagitlar, bir kablo
    static void Neglect(Transform root, Vector2 areaHalf, float sideX, float backZ, bool backClutter, int seed, Func<float, float, float> ground)
    {
        var rng = new System.Random(seed);
        float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        foreach (float side in new[] { -1f, 1f })
            Heap(root, new Vector3(side * (sideX - 0.06f), 0f, backZ - R(0.6f, 1.2f)), new Vector3(0.2f, 0.05f, R(1.0f, 1.5f)), ground);
        // kagitlar: yanlardaki dar seritte; arka seritte de (esya yoksa)
        var spots = new List<Vector3> { new Vector3(-(areaHalf.x + 0.25f), 0f, R(-1.5f, 1.5f)) };
        if (backClutter)
        {
            foreach (float x in new[] { -2.6f, -0.4f, 1.7f })
                Heap(root, new Vector3(x + R(-0.3f, 0.3f), 0f, backZ - 0.06f), new Vector3(R(0.8f, 1.2f), R(0.04f, 0.06f), 0.22f), ground);
            spots.Add(new Vector3(R(-2.4f, -1.4f), 0f, R(areaHalf.y + 0.25f, backZ - 0.3f)));
            spots.Add(new Vector3(R(0.4f, 1.2f), 0f, R(areaHalf.y + 0.25f, backZ - 0.3f)));
            spots.Add(new Vector3(R(2.0f, 2.7f), 0f, R(areaHalf.y + 0.25f, backZ - 0.3f)));
        }
        foreach (var p in spots)
        {
            var sheet = Parts.Add("Kagit", root, MeshFactory.RoundedBox(new Vector3(0.13f, 0.004f, 0.18f), 0.002f, 1), Paper,
                new Vector3(p.x, ground(p.x, p.z) + 0.003f, p.z), outline: false);
            sheet.localRotation = Quaternion.Euler(R(-3f, 3f), R(0f, 360f), R(-3f, 3f));
        }

        if (!backClutter) return;
        // kablo: duvar dibinde yerde, bir ucu duvardan kopmus
        float cx = 0.9f, cz = backZ - 0.28f;
        var cable = Parts.Add("Kablo", root, MeshFactory.RoundedCylinder(0.016f, 1.1f, 0.006f, 8), Cable,
            new Vector3(cx, ground(cx, cz) + 0.016f, cz), outline: false);
        cable.localRotation = Quaternion.Euler(0f, 14f, 90f);
    }

    static void Heap(Transform root, Vector3 at, Vector3 size, Func<float, float, float> ground)
    {
        var heap = Parts.Add("Kum", root, MeshFactory.Sphere(0.5f, 8, 14), Drift, new Vector3(at.x, ground(at.x, at.z) - size.y * 0.15f, at.z), outline: false);
        heap.localScale = new Vector3(size.x, size.y * 2f, size.z);
    }
}
