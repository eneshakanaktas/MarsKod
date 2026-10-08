using System.Collections;
using UnityEngine;

// Serce'nin (drone, Bolge 6) renkleri: krem govde, turuncu ayrintilar, koyu kollar. Parcalar ve Serce ayni renkleri kullanir.
public static class DroneColors
{
    public static readonly Color Body = Mats.Hex("#E8E1D2");
    public static readonly Color Accent = Mats.Hex("#E8743B");
    public static readonly Color Dark = Mats.Hex("#34313A");
    public static readonly Color Prop = Mats.Hex("#4A4750");
}

public enum DronePiece { Propeller, Arm, Shell }

// Drone parcasi (haritada D, Bolge 6): firtinada dagilmis Serce'nin parcasi. Uc cesit sirayla: halkali pervane,
// ucunda motorlu kol, govde kabugu parcasi. Kuma egik yatar, turuncu ayrintisi yavasca parlar.
// Toplaninca sicrayip kaybolur, yerde sicak renkli halka yayilir.
public class DronePart : MonoBehaviour, IPickup
{
    static readonly Color Glow = Mats.Hex("#FF7A3C");
    const float Scale = 2.6f, RingScale = 1.85f;   // halka diger toplananlarinki kadar (pusula parcasi 1.85)

    Transform body, ring;
    Material accentMat, ringMat;
    float phase;
    bool popping;

    public static DronePart Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("DronePart");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * Scale;
        var part = go.AddComponent<DronePart>();
        part.Build(seed);
        return part;
    }

    void Build(int seed)
    {
        phase = seed * 1.7f;
        accentMat = Mats.Emissive(DroneColors.Accent, Glow * 0.3f, 0.5f);
        body = Parts.Empty("Body", transform);
        body.localRotation = Quaternion.Euler(0f, seed * 53f, 0f);
        var piece = BuildPiece(body, (DronePiece)((seed - 1) % 3), accentMat);
        piece.localRotation = Quaternion.Euler(10f, 0f, -8f);   // kuma egik dusmus
        ring = PickupFx.AddRing(transform, Mats.Hex("#FFD2A8"), out ringMat);
        ring.localScale = Vector3.one * (RingScale / Scale);
    }

    // Parcanin yalnizca gorunusu (yerdeki kirintilar da kullanir). accent: turuncu ayrintinin malzemesi (parlayan ya da duz).
    public static Transform BuildPiece(Transform parent, DronePiece piece, Material accent)
    {
        var root = Parts.Empty("Parca-" + piece, parent);
        var shell = Mats.Lit(DroneColors.Body, 0.45f);
        var dark = Mats.Lit(DroneColors.Dark, 0.35f);
        switch (piece)
        {
            case DronePiece.Propeller:
                root.localPosition = new Vector3(0f, 0.03f, 0f);
                BuildRotor(root, 0.1f, accent);
                break;
            case DronePiece.Arm:
                Parts.Add("Kol", root, MeshFactory.RoundedBox(new Vector3(0.22f, 0.03f, 0.04f), 0.01f), shell, new Vector3(0f, 0.02f, 0f));
                Parts.Add("Motor", root, MeshFactory.RoundedCylinder(0.032f, 0.05f, 0.008f, 14), dark, new Vector3(0.11f, 0.035f, 0f));
                Parts.Add("Bant", root, MeshFactory.RoundedCylinder(0.034f, 0.014f, 0.004f, 14), accent, new Vector3(0.11f, 0.045f, 0f), outline: false);
                // kopuk ucta iki kablo
                for (int k = 0; k < 2; k++)
                {
                    var wire = Parts.Add("Kablo", root, MeshFactory.RoundedCylinder(0.004f, 0.06f, 0.002f, 6), Mats.Lit(k == 0 ? DroneColors.Accent : DroneColors.Body, 0.3f),
                        new Vector3(-0.13f, 0.02f, (k - 0.5f) * 0.014f), outline: false);
                    wire.localRotation = Quaternion.Euler(0f, k * 30f - 15f, 80f);
                }
                break;
            default:
                var dome = Parts.Add("Kabuk", root, MeshFactory.Sphere(0.1f, 8, 16), shell, new Vector3(0f, 0.005f, 0f));
                dome.localScale = new Vector3(1f, 0.5f, 1.15f);
                Parts.Add("Serit", root, MeshFactory.RoundedBox(new Vector3(0.03f, 0.012f, 0.2f), 0.005f), accent, new Vector3(0.03f, 0.05f, 0f), outline: false);
                break;
        }
        return root;
    }

    // Halkali (korumali) pervane: krem halka koruyucu, ortada gobek, iki pal. Donen kisim (pallar) dondurulur.
    // Serce de ayni pervaneyi kullanir.
    public static Transform BuildRotor(Transform parent, float radius, Material accent)
    {
        var guardMat = Mats.Lit(DroneColors.Body, 0.45f);
        var guard = new[]
        {
            new Vector2(radius * 0.9f, -0.012f), new Vector2(radius, -0.012f), new Vector2(radius, 0.012f),
            new Vector2(radius * 0.9f, 0.012f), new Vector2(radius * 0.9f, -0.012f),
        };
        Parts.Add("Halka", parent, MeshFactory.Lathe(guard, 24, flat: true), guardMat, Vector3.zero);
        Parts.Add("Gobek", parent, MeshFactory.RoundedCylinder(radius * 0.18f, 0.03f, 0.006f, 12), accent, Vector3.zero, outline: false);
        var blades = Parts.Empty("Pallar", parent);
        var bladeMat = Mats.Lit(DroneColors.Prop, 0.4f);
        for (int k = 0; k < 2; k++)
        {
            var blade = Parts.Add("Pal", blades, MeshFactory.RoundedBox(new Vector3(radius * 1.6f, 0.006f, radius * 0.22f), 0.003f),
                bladeMat, new Vector3(0f, 0.008f, 0f), outline: false);
            blade.localRotation = Quaternion.Euler(k == 0 ? 6f : -6f, k * 90f + 20f, 0f);
        }
        return blades;
    }

    void Update()
    {
        if (popping) return;
        float t = Time.time + phase;
        accentMat.SetColor("_EmissionColor", Glow * (0.2f + 0.3f * (0.5f + 0.5f * Mathf.Sin(t * 1.4f))));
    }

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        popping = true;
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        yield return PickupFx.PopAway(body, 1.22f, 0.25f);
    }

    public void Restore()
    {
        StopAllCoroutines();
        popping = false;
        body.gameObject.SetActive(true);
        body.localScale = Vector3.one;
        body.localPosition = Vector3.zero;
        ring.gameObject.SetActive(false);
    }
}
