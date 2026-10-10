using UnityEngine;

// Kanyon deposundaki gorev panosu (Bolum 56): sundurmanin sag on diregine iki kelepceyle asilmis kucuk tebesir panosu,
// hafif egik. Ustteki satirlar tozdan ve silinmekten okunmuyor; okunan tek kelime elle "KAPI" (son gorev, aciklanmaz;
// Bolge 8'de anlam kazanir), sag ust kosede kucuk ve soluk "BKM-7".
public static class TaskBoard
{
    public const string ReadableWord = "KAPI", FaintTag = "BKM-7";
    const float W = 0.8f, H = 0.54f;   // telefonda "KAPI" okunsun

    // postFace: diregin kameraya bakan yuzu (parent'a gore); panonun ortasi buraya gelir, pano -Z'ye bakar
    public static Transform Create(Transform parent, Vector3 postFace)
    {
        var root = Parts.Empty("Iz-GorevPanosu", parent);
        root.localPosition = postFace + new Vector3(0f, 0f, -0.03f);
        root.localRotation = Quaternion.Euler(0f, 0f, -4f);
        var frame = Mats.Lit(Mats.Hex("#4E4C52"), 0.4f);
        var board = Mats.Lit(Mats.Hex("#33403A"), 0.15f);
        var faded = Mats.Lit(Mats.Hex("#56635C"), 0.1f);

        Parts.Add("Cerceve", root, MeshFactory.RoundedBox(new Vector3(W + 0.05f, H + 0.05f, 0.03f), 0.01f), frame, Vector3.zero);
        Parts.Add("Pano", root, MeshFactory.RoundedBox(new Vector3(W, H, 0.02f), 0.006f), board, new Vector3(0f, 0f, -0.008f), outline: false);
        foreach (float y in new[] { H * 0.3f, -H * 0.3f })   // direge saran kelepceler
            Parts.Add("Kelepce", root, MeshFactory.RoundedBox(new Vector3(0.1f, 0.03f, 0.05f), 0.01f), frame, new Vector3(0f, y, 0.025f));

        // okunmayan satirlar: silik tebesir izleri (kisa cizgi obekleri)
        for (int row = 0; row < 3; row++)
        {
            float y = H * 0.5f - 0.07f - row * 0.075f;
            float x = -W * 0.42f;
            for (int k = 0; k < 5; k++)
            {
                float len = 0.04f + 0.07f * Mathf.Abs(Mathf.Sin(row * 3.1f + k * 1.7f));
                if (x + len > W * (row == 0 ? 0.18f : 0.42f)) break;   // ilk satirin sagi BKM-7'ye yer birakir
                Parts.Add("Silik", root, MeshFactory.RoundedBox(new Vector3(len, 0.018f, 0.003f), 0.001f), faded,
                    new Vector3(x + len * 0.5f, y, -0.02f), outline: false, castShadow: false);
                x += len + 0.025f + 0.02f * Mathf.Abs(Mathf.Cos(k + row));
            }
        }
        var chalk = Mats.Emissive(Mats.Hex("#F2EEE2"), Mats.Hex("#E8E4D8") * 0.35f, 0.1f);   // golgede de secilsin
        BrushPaint.Write(root, ReadableWord, new Vector3(0f, -H * 0.22f, -0.021f), 0.19f, chalk, seed: 56, angleDeg: 2f);
        BrushPaint.Write(root, FaintTag, new Vector3(W * 0.31f, H * 0.5f - 0.07f, -0.021f), 0.055f, Mats.Lit(Mats.Hex("#8A948C"), 0.1f), seed: 7);
        return root;
    }
}
