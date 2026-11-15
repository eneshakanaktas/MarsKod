using UnityEngine;

// Katlanmis cizim (Bolge 8, 76): Ece'nin dorde katlanmis kagidi. Krem, ici gorunmez (Perde 4'e sakli); bir kenarindan
// turuncu kalem cizgisinin ucu tasar (istasyon duvar cizimleriyle ayni boya turuncusu). Yerde yatar; localPos zeminin ustu.
// Sirttaki kucuk hali (77+) ayni modeli kullanir: Bolge 8 · 3a Gorev 4.
// Ust kat (ve kat izi) uzun arka kenarindan menteseli (3b, 76'da Kivilcim acmaya yeltenir): SetOpen kaldirir. Acilinca icinde
// turuncu cizginin ucu biraz daha uzun ve on kosede kucuk mavi bir yildiz gorunur (Perde 4 finali: "gokyuzunde onu izleyen kucuk,
// mavi bir yildiz"; kapaliyken ust katin icinde kalir, gorunmez). Ust katin alti krem.
public static class FoldedDrawing
{
    public const float Width = 0.2f, Depth = 0.15f;   // dorde katlanmis hali (telefonda secilsin diye biraz iri)
    const float Layer = 0.007f;
    const string HingeName = "Kat-Ust-Mentese";
    const float FullOpenDegrees = 110f;

    static readonly Color Paper = Mats.Hex("#EDE4CC");
    static readonly Color PaperTop = Mats.Hex("#F3ECD8");
    static readonly Color Crease = Mats.Hex("#C9BC9A");
    static readonly Color StarBlue = Mats.Hex("#5B8FE0");   // Ece'nin mavi yildizi (turkuaz kalem degil)

    public static Transform Create(Transform parent, Vector3 localPos, float yawDeg)
    {
        var root = Parts.Empty("KatlanmisCizim", parent);
        root.localPosition = localPos;
        root.localRotation = Quaternion.Euler(0f, yawDeg, 0f);

        // iki kat: alttaki tam, ustteki bir tik kucuk ve hafif kaymis (katlanmis kagidin kalinligi okunsun)
        Parts.Add("Kat-Alt", root, MeshFactory.RoundedBox(new Vector3(Width, Layer, Depth), 0.002f, 1), Mats.Lit(Paper, 0.15f),
            new Vector3(0f, Layer * 0.5f, 0f));
        // menteşe: alt katin ustunde, arka uzun kenarda; ust kat ve kat izi onun altinda (kapaliyken gorunus aynen eskisi)
        var hinge = Parts.Empty(HingeName, root);
        hinge.localPosition = new Vector3(0f, Layer, -Depth * 0.5f);
        var top = Parts.Add("Kat-Ust", hinge, MeshFactory.RoundedBox(new Vector3(Width - 0.012f, Layer, Depth - 0.01f), 0.002f, 1),
            Mats.Lit(PaperTop, 0.15f), new Vector3(0.004f, Layer * 0.5f, Depth * 0.5f - 0.003f), outline: false);
        top.localRotation = Quaternion.Euler(0f, 2.5f, 0f);
        // ortadaki kat izi
        Parts.Add("Kat-Izi", hinge, MeshFactory.RoundedBox(new Vector3(0.004f, 0.002f, Depth - 0.02f), 0.001f, 1), Mats.Lit(Crease, 0.1f),
            new Vector3(0.004f, Layer + 0.001f, Depth * 0.5f - 0.003f), outline: false, castShadow: false);
        // kenardan tasan turuncu cizgi ucu (kagidin icindeki cizimden)
        var stroke = Parts.Add("Cizgi-Ucu", root, MeshFactory.RoundedBox(new Vector3(0.05f, 0.003f, 0.009f), 0.0015f, 1), PaintMarks.PaintMat(0f),
            new Vector3(Width * 0.5f - 0.012f, Layer + 0.0015f, Depth * 0.18f), outline: false, castShadow: false);
        stroke.localRotation = Quaternion.Euler(0f, -24f, 0f);
        AddHiddenStar(root);
        return root;
    }

    // Alt katin ustunde, on kosede (mentese arkada: aralaninca ilk gorunen yer) kucuk mavi yildiz; duz boya, yukari bakar
    static void AddHiddenStar(Transform root)
    {
        var mesh = FlatShapes.Fan("Cizim-Yildiz", FlatShapes.Star(Vector2.zero, 0.014f, 0.006f));
        var star = Parts.Add("Cizim-Yildiz", root, mesh, Mats.Unlit(StarBlue),
            new Vector3(-Width * 0.18f, Layer + 0.0005f, Depth * 0.28f), outline: false, castShadow: false);
        star.localRotation = Quaternion.Euler(90f, 0f, 0f);   // orgu -Z'ye bakar; 90 derece ile yukari
    }

    // Ust kati acar: 0 katli, 1 tam acik (3b'de en fazla ~0,35). paper: Create'in dondurdugu kok.
    public static void SetOpen(Transform paper, float amount)
    {
        var hinge = paper.Find(HingeName);
        if (hinge != null) hinge.localRotation = Quaternion.Euler(-FullOpenDegrees * Mathf.Clamp01(amount), 0f, 0f);
    }
}
