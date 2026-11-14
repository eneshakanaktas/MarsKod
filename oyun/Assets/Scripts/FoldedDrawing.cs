using UnityEngine;

// Katlanmis cizim (Bolge 8, 76): Ece'nin dorde katlanmis kagidi. Krem, ici gorunmez (Perde 4'e sakli); bir kenarindan
// turuncu kalem cizgisinin ucu tasar (istasyon duvar cizimleriyle ayni boya turuncusu). Yerde yatar; localPos zeminin ustu.
// Sirttaki kucuk hali (77+) ayni modeli kullanir: Bolge 8 · 3a Gorev 4.
public static class FoldedDrawing
{
    public const float Width = 0.2f, Depth = 0.15f;   // dorde katlanmis hali (telefonda secilsin diye biraz iri)
    const float Layer = 0.007f;

    static readonly Color Paper = Mats.Hex("#EDE4CC");
    static readonly Color PaperTop = Mats.Hex("#F3ECD8");
    static readonly Color Crease = Mats.Hex("#C9BC9A");

    public static Transform Create(Transform parent, Vector3 localPos, float yawDeg)
    {
        var root = Parts.Empty("KatlanmisCizim", parent);
        root.localPosition = localPos;
        root.localRotation = Quaternion.Euler(0f, yawDeg, 0f);

        // iki kat: alttaki tam, ustteki bir tik kucuk ve hafif kaymis (katlanmis kagidin kalinligi okunsun)
        Parts.Add("Kat-Alt", root, MeshFactory.RoundedBox(new Vector3(Width, Layer, Depth), 0.002f, 1), Mats.Lit(Paper, 0.15f),
            new Vector3(0f, Layer * 0.5f, 0f));
        var top = Parts.Add("Kat-Ust", root, MeshFactory.RoundedBox(new Vector3(Width - 0.012f, Layer, Depth - 0.01f), 0.002f, 1),
            Mats.Lit(PaperTop, 0.15f), new Vector3(0.004f, Layer * 1.5f, -0.003f), outline: false);
        top.localRotation = Quaternion.Euler(0f, 2.5f, 0f);
        // ortadaki kat izi
        Parts.Add("Kat-Izi", root, MeshFactory.RoundedBox(new Vector3(0.004f, 0.002f, Depth - 0.02f), 0.001f, 1), Mats.Lit(Crease, 0.1f),
            new Vector3(0.004f, Layer * 2f + 0.001f, -0.003f), outline: false, castShadow: false);
        // kenardan tasan turuncu cizgi ucu (kagidin icindeki cizimden)
        var stroke = Parts.Add("Cizgi-Ucu", root, MeshFactory.RoundedBox(new Vector3(0.05f, 0.003f, 0.009f), 0.0015f, 1), PaintMarks.PaintMat(0f),
            new Vector3(Width * 0.5f - 0.012f, Layer + 0.0015f, Depth * 0.18f), outline: false, castShadow: false);
        stroke.localRotation = Quaternion.Euler(0f, -24f, 0f);
        return root;
    }
}
