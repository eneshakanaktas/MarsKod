using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bolge 8 · Ekran bulmacasinda arka duvardaki bilgisayarlar (docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md, K2).
//   72: arka duvarin onunde yan yana 6 masa, her birinde eski, kalin kasali bir bilgisayar; ekranlar sonuk koyu cam.
//       Her dogru cevapta sirasi gelen bilgisayar (soldan saga) kirpisarak kehribar yanar; yanlista bir an kirmizi kirpisir.
//   77-78: arka duvarda tek, genis duvar terminali (hep acik); her cevapta kisa kirpisir. 78'de True satiri gelince bir an turuncu.
// Sahnedeki ekranlarda okunur yazi yok (dunyada bilgisayar yazisi yok): kehribar soyut satir cizgileri kayar.
// Dunya kurallarini bilmez: Oyun cevabin sirasini ve dogrulugunu verir. Alanin disinda kalir, masalar zemine basar.
public class StationScreens : MonoBehaviour
{
    // masa ve bilgisayar (duvar yuzunun yerel olcusunde: y yukari, -z kameraya)
    const float DeskTop = 0.62f, DeskWidth = 0.86f, DeskDepth = 0.46f, DeskZ = -0.32f, DeskSpacing = 1f;
    static readonly Vector3 CaseSize = new Vector3(0.44f, 0.38f, 0.36f);
    static readonly Vector2 MonitorGlass = new Vector2(0.34f, 0.26f);
    // duvar terminali (77-78)
    const float TerminalWidth = 3f, TerminalHeight = 0.9f, TerminalBottom = 0.2f, TerminalDepth = 0.1f;

    static readonly Color Amber = Mats.Hex("#FFB347");
    static readonly Color Alarm = Mats.Hex("#FF4A3A");
    static readonly Color Found = Mats.Hex("#FF7A2E");   // 78: disarida kalan bulundu
    static readonly Color DeadGlass = Mats.Hex("#141A17");
    static readonly Color LitGlass = Mats.Hex("#3A2A12");
    const float GlowOn = 1.9f, TerminalIdle = 0.75f, PoolOn = 0.22f;
    const float ScrollSpeed = 0.18f;

    static readonly Material CaseMat = Mats.Lit(Mats.Hex("#A39C8C"), 0.25f);    // eski, sararmis plastik
    static readonly Material BezelMat = Mats.Lit(Mats.Hex("#5C5850"), 0.3f);
    static readonly Material DeskMat = Mats.Lit(Mats.Hex("#6E7178"), 0.35f);
    static readonly Material DeskLegMat = Mats.Lit(Mats.Hex("#3E4148"), 0.4f);
    static readonly Material KeysMat = Mats.Lit(Mats.Hex("#47474C"), 0.3f);
    static readonly Material FrameMat = Mats.Lit(Mats.Hex("#2E3236"), 0.45f);
    static Texture2D lineTexture;

    sealed class Monitor
    {
        public Material Glass;
        public Material Pool;   // masanin onune dusen kehribar leke (yalniz 72)
        public float Glow;      // 0 sonuk, 1 tam yanik
        public bool On;
    }

    readonly List<Monitor> screens = new List<Monitor>();
    bool terminal, highlightTrue;

    // Uyanik (ekrani yanan) bilgisayar sayisi; denetim icin. Terminal hep acik: 1.
    public int AwakeCount { get; private set; }
    // 78: True satirinda terminalin turuncuya donme sayisi (denetim icin)
    public int Highlights { get; private set; }

    // wallFace: StationRoom'un arka duvar yuzu. count: 72'de 6 masa, 77-78'de 1 (duvar terminali). highlightTrue: True satirinda
    // terminal turuncuya doner (78). ground: zemin yuksekligi (alanin yerel x, z).
    public static StationScreens Create(Transform wallFace, int count, bool highlightTrue, int seed, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Bilgisayarlar", wallFace);
        var s = root.gameObject.AddComponent<StationScreens>();
        s.terminal = count == 1;
        s.highlightTrue = highlightTrue;
        if (s.terminal) s.BuildTerminal();
        else
        {
            var rng = new System.Random(seed);
            for (int i = 0; i < count; i++)
                s.BuildDesk(wallFace, (i - (count - 1) * 0.5f) * DeskSpacing, (float)(rng.NextDouble() * 8.0 - 4.0), ground);
        }
        s.Restore();
        return s;
    }

    // Ekranin bir satirina cevap geldi (index: satirin sirasi). 72'de dogruysa siradaki bilgisayar uyanir; terminal kirpisir.
    public void Answer(int index, bool correct, bool returnedTrue)
    {
        if (terminal)
        {
            StartCoroutine(returnedTrue && highlightTrue ? Highlight() : Blink(screens[0], correct));
            return;
        }
        if (index >= screens.Count) return;
        StartCoroutine(correct ? WakeUp(screens[index]) : Blink(screens[index], false));
    }

    // Hepsi bolumun basindaki haline: 72'de hepsi sonuk, terminal bosta acik
    public void Restore()
    {
        StopAllCoroutines();
        AwakeCount = 0;
        Highlights = 0;
        foreach (var s in screens)
        {
            s.On = terminal;
            SetGlow(s, terminal ? TerminalIdle : 0f, Amber);
            if (terminal) AwakeCount++;
        }
    }

    void Update()
    {
        // uyanik ekranlarda soyut satirlar yukari kayar
        float offset = Time.time * ScrollSpeed;
        foreach (var s in screens)
            if (s.On) s.Glass.SetTextureOffset("_BaseMap", new Vector2(0f, -offset));
    }

    // Kirpisarak yanar: bir-iki kez yanip soner, sonra kehribar kalir; masanin onune isik dusur
    IEnumerator WakeUp(Monitor s)
    {
        if (s.On) yield break;
        s.On = true;
        AwakeCount++;
        foreach (float g in new[] { 0.6f, 0f, 0.9f, 0.15f, 1f })
        {
            SetGlow(s, g, Amber);
            yield return Tween.Wait(0.06f);
        }
    }

    // Kisa kirpisma: dogruysa kehribar, yanlista bir an kirmizi; sonra eski haline
    IEnumerator Blink(Monitor s, bool correct)
    {
        float rest = s.On ? (terminal ? TerminalIdle : 1f) : 0f;
        SetGlow(s, correct ? 1.3f : 1.1f, correct ? Amber : Alarm);
        yield return Tween.Wait(correct ? 0.07f : 0.18f);
        SetGlow(s, rest, Amber);
    }

    // 78: disarida kalan bulundu; terminal bir an turuncuya doner (3b aciklama aninin isareti)
    IEnumerator Highlight()
    {
        Highlights++;
        var s = screens[0];
        yield return Tween.Run(0.7f, t => SetGlow(s, Mathf.Lerp(1.6f, TerminalIdle, t * t), Color.Lerp(Found, Amber, t * t)));
    }

    void SetGlow(Monitor s, float glow, Color color)
    {
        s.Glow = glow;
        float lit = Mathf.Clamp01(glow);
        s.Glass.SetColor("_BaseColor", Color.Lerp(DeadGlass, LitGlass, lit));
        s.Glass.SetColor("_EmissionColor", color * (GlowOn * glow));
        if (s.Pool != null) s.Pool.SetFloat("_A", PoolOn * lit);
    }

    // ---- kurulum ----

    void BuildDesk(Transform face, float x, float yawDeg, Func<float, float, float> ground)
    {
        float floor = ground(face.localPosition.x + x, face.localPosition.z + DeskZ) - face.localPosition.y;
        var desk = Parts.Empty("Masa", transform);
        desk.localPosition = new Vector3(x, floor, DeskZ);

        Parts.Add("Masa-Ust", desk, MeshFactory.RoundedBox(new Vector3(DeskWidth, 0.04f, DeskDepth), 0.012f, 2), DeskMat,
            new Vector3(0f, DeskTop - 0.02f, 0f));
        foreach (float side in new[] { -1f, 1f })
            Parts.Add("Masa-Ayak", desk, MeshFactory.RoundedBox(new Vector3(0.04f, DeskTop - 0.04f, DeskDepth - 0.06f), 0.008f, 1), DeskLegMat,
                new Vector3(side * (DeskWidth * 0.5f - 0.05f), (DeskTop - 0.04f) * 0.5f, 0f));

        var computer = Parts.Empty("Bilgisayar", desk);
        computer.localPosition = new Vector3(0f, DeskTop, 0.04f);
        computer.localRotation = Quaternion.Euler(0f, yawDeg, 0f);
        Parts.Add("Kasa", computer, MeshFactory.RoundedBox(CaseSize, 0.04f, 3), CaseMat, new Vector3(0f, CaseSize.y * 0.5f + 0.02f, 0f));
        Parts.Add("Ayak", computer, MeshFactory.RoundedBox(new Vector3(0.2f, 0.02f, 0.16f), 0.006f, 1), BezelMat, new Vector3(0f, 0.01f, 0f));
        float glassY = CaseSize.y * 0.5f + 0.03f, front = -CaseSize.z * 0.5f;
        Parts.Add("Cerceve", computer, MeshFactory.RoundedBox(new Vector3(MonitorGlass.x + 0.04f, MonitorGlass.y + 0.04f, 0.012f), 0.01f, 2), BezelMat,
            new Vector3(0f, glassY, front - 0.004f), outline: false, castShadow: false);
        var glass = GlassMaterial(Vector2.one);
        GlassQuad(computer, MonitorGlass, new Vector3(0f, glassY, front - 0.012f), glass);
        Parts.Add("Klavye", desk, MeshFactory.RoundedBox(new Vector3(0.36f, 0.02f, 0.11f), 0.006f, 1), KeysMat,
            new Vector3(0f, DeskTop + 0.01f, -DeskDepth * 0.5f + 0.08f), castShadow: false);

        float poolZ = DeskZ - DeskDepth * 0.5f - 0.25f;
        var pool = LightPool.Create(transform, new Vector3(x, ground(face.localPosition.x + x, face.localPosition.z + poolZ) - face.localPosition.y, poolZ),
            new Vector2(0.9f, 0.6f), Amber, 0f, "Ekran-Isigi");
        screens.Add(new Monitor { Glass = glass, Pool = pool });
    }

    void BuildTerminal()
    {
        var unit = Parts.Empty("Duvar-Terminali", transform);
        float midY = TerminalBottom + TerminalHeight * 0.5f;
        Parts.Add("Cerceve", unit, MeshFactory.RoundedBox(new Vector3(TerminalWidth + 0.16f, TerminalHeight + 0.16f, TerminalDepth), 0.03f, 2), FrameMat,
            new Vector3(0f, midY, -TerminalDepth * 0.5f), castShadow: false);
        // altinda dar bir konsol rafi (dugmeler soyut: kucuk koyu kutular)
        Parts.Add("Konsol", unit, MeshFactory.RoundedBox(new Vector3(TerminalWidth * 0.7f, 0.05f, 0.18f), 0.012f, 2), FrameMat,
            new Vector3(0f, TerminalBottom - 0.06f, -0.09f), castShadow: false);
        var glass = GlassMaterial(new Vector2(3f, 1f));
        GlassQuad(unit, new Vector2(TerminalWidth, TerminalHeight), new Vector3(0f, midY, -TerminalDepth - 0.004f), glass);
        screens.Add(new Monitor { Glass = glass });
    }

    // Ekran cami: dik duran duz yuzey (kameraya bakar), uzerinde kayan satir dokusu
    static void GlassQuad(Transform parent, Vector2 size, Vector3 at, Material mat)
    {
        var quad = Parts.Add("Ekran", parent, MeshFactory.Quad(size), mat, at, outline: false, castShadow: false);
        quad.localRotation = Quaternion.Euler(-90f, 0f, 0f);   // yukari bakan kare -> -Z'ye (kameraya) bakar
    }

    static Material GlassMaterial(Vector2 tiling)
    {
        var mat = Mats.Emissive(DeadGlass, Color.black, 0.85f);
        mat.SetTexture("_BaseMap", LineTexture);
        mat.SetTexture("_EmissionMap", LineTexture);
        mat.SetTextureScale("_BaseMap", tiling);
        return mat;
    }

    // Soyut satirlar: koyu zeminde farkli uzunlukta yatay kehribar cizgiler (yazi degil)
    static Texture2D LineTexture
    {
        get
        {
            if (lineTexture != null) return lineTexture;
            const int size = 32;
            lineTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            var rng = new System.Random(72);
            var px = new Color[size * size];
            var dim = new Color(0.18f, 0.18f, 0.18f, 1f);
            for (int i = 0; i < px.Length; i++) px[i] = dim;
            for (int y = 1; y < size; y += 4)
            {
                int start = 3 + rng.Next(0, 3), length = 6 + rng.Next(0, 20);
                for (int x = start; x < Mathf.Min(size - 2, start + length); x++) px[y * size + x] = Color.white;
            }
            lineTexture.SetPixels(px);
            lineTexture.Apply();
            return lineTexture;
        }
    }
}
