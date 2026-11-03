using System.Collections.Generic;
using UnityEngine;

public enum Hand { Small, Big, Robot }

// Duvara konmus elin kalemle cizilmis cevresi (Bolge 7, K3/K4): cocuklar elini duvara koyup etrafini cizer. Kucuk = Ece,
// buyuk = Ece'nin annesinin eli (Ece cizmis), robot = 3b'de Kivilcim'in eli (uc kalin parmak). Bilek tarafi acik kalir.
public static class HandTrace
{
    public const float SmallSize = 0.19f, BigSize = 0.28f, RobotSize = 0.22f;   // elin boyu (yuzeyin biriminde); kamera duvari yukaridan, yatik gorur
    // (dikeyde ~yarisi kalir): gercek boyundan biraz buyuk ki telefonda tek tek sayilabilsin
    const float Thickness = 0.014f;

    // wallFace: duvarin on yuzu (RiverWall.Build); at: elin bilek ortasi (yuzeyin XY'si); angleDeg: parmaklarin yukaridan sapmasi
    public static Transform Draw(Transform wallFace, Vector2 at, Hand hand, float angleDeg, int seed)
    {
        string name = hand == Hand.Small ? "ElIzi-Kucuk" : hand == Hand.Big ? "ElIzi-Buyuk" : "ElIzi-Robot";
        return CrayonMarks.Draw(wallFace, name, new[] { Outline(at, hand, angleDeg) }, seed, Thickness);
    }

    // Cizginin bir parcasi (Bolum 70: Kivilcim kendi elini cizdikce belirir); ayni kalinlikta
    public static Transform DrawPart(Transform surface, Vector2[] part, int seed) =>
        CrayonMarks.Draw(surface, "Parca", new[] { part }, seed, Thickness);

    // Elin cevresi yuzeyin biriminde (baska cizimlerle tek agda birlesmek icin)
    public static Vector2[] Outline(Vector2 at, Hand hand, float angleDeg)
    {
        var shape = hand == Hand.Robot ? RobotShape() : HumanShape();
        float size = hand == Hand.Small ? SmallSize : hand == Hand.Big ? BigSize : RobotSize;
        var rotation = Quaternion.Euler(0f, 0f, -angleDeg);
        var result = new Vector2[shape.Count];
        for (int i = 0; i < shape.Count; i++)
            result[i] = at + (Vector2)(rotation * (Vector3)((shape[i] - new Vector2(0.5f, 0f)) * size));
        return result;
    }

    // Insan eli (birim kutu, bilek altta ortada, parmaklar yukari): bilegin solundan baslar, basparmak, dort parmak, bilegin sagi
    static List<Vector2> HumanShape()
    {
        var p = new List<Vector2> { new Vector2(0.28f, 0f), new Vector2(0.22f, 0.3f), new Vector2(0.12f, 0.42f) };
        // basparmak: sola-yukari egik
        p.Add(new Vector2(0.02f, 0.56f)); p.Add(new Vector2(0.03f, 0.64f)); p.Add(new Vector2(0.1f, 0.63f)); p.Add(new Vector2(0.22f, 0.52f));
        // dort parmak: isaret, orta, yuzuk, serce (soldan saga), aralarinda dip
        float[] centers = { 0.3f, 0.45f, 0.6f, 0.74f }, lengths = { 0.4f, 0.46f, 0.42f, 0.31f };
        const float baseY = 0.58f, half = 0.06f;
        for (int f = 0; f < centers.Length; f++)
        {
            float c = centers[f], top = baseY + lengths[f];
            p.Add(new Vector2(c - half, baseY + 0.02f));
            p.Add(new Vector2(c - half, top - half));
            for (int k = 1; k < 4; k++)   // yuvarlak parmak ucu
            {
                float a = Mathf.PI - k * Mathf.PI / 4f;
                p.Add(new Vector2(c + Mathf.Cos(a) * half, top - half + Mathf.Sin(a) * half));
            }
            p.Add(new Vector2(c + half, top - half));
            p.Add(new Vector2(c + half, baseY + (f == centers.Length - 1 ? -0.04f : 0.01f)));
        }
        p.Add(new Vector2(0.82f, 0.4f)); p.Add(new Vector2(0.76f, 0.2f)); p.Add(new Vector2(0.7f, 0f));
        return p;
    }

    // Kivilcim'in eli: yuvarlak koseli avuc, uc kisa kalin parmak
    static List<Vector2> RobotShape()
    {
        var p = new List<Vector2> { new Vector2(0.2f, 0f), new Vector2(0.15f, 0.5f) };
        float[] centers = { 0.3f, 0.5f, 0.7f };
        const float baseY = 0.55f, half = 0.08f, length = 0.3f;
        foreach (float c in centers)
        {
            p.Add(new Vector2(c - half, baseY));
            p.Add(new Vector2(c - half, baseY + length));
            p.Add(new Vector2(c + half, baseY + length));
            p.Add(new Vector2(c + half, baseY));
        }
        p.Add(new Vector2(0.85f, 0.5f)); p.Add(new Vector2(0.8f, 0f));
        return p;
    }
}
