using UnityEngine;

// Kanyon deposundaki gorev panosu (Bolum 56): arka duvarin ustune dikilmis tebesir panosu. Ustteki satirlar tozdan ve
// silinmekten okunmuyor; okunan tek satir en altta: "BKM-7 ··· KAPI" (son gorev, aciklanmaz; Bolge 8'de anlam kazanir).
public static class TaskBoard
{
    public const string ReadableLine = "BKM-7  ·······  KAPI";

    // pos: panonun alt kenarinin ortasi (parent'a gore); pano -Z'ye (kameraya) bakar
    public static Transform Create(Transform parent, Vector3 pos)
    {
        var root = Parts.Empty("Iz-GorevPanosu", parent);
        root.localPosition = pos;
        const float w = 2.5f, h = 1.05f;
        var frame = Mats.Lit(Mats.Hex("#4E4C52"), 0.4f);
        var board = Mats.Lit(Mats.Hex("#33403A"), 0.15f);
        var chalk = Mats.Hex("#E8E4D8");
        var faded = Mats.Lit(Mats.Hex("#56635C"), 0.1f);

        foreach (float x in new[] { -w * 0.42f, w * 0.42f })
            Parts.Add("Ayak", root, MeshFactory.RoundedBox(new Vector3(0.07f, 0.5f, 0.07f), 0.015f), frame, new Vector3(x, -0.25f, 0.02f));
        Parts.Add("Cerceve", root, MeshFactory.RoundedBox(new Vector3(w + 0.1f, h + 0.1f, 0.05f), 0.02f), frame, new Vector3(0f, h * 0.5f, 0.02f));
        Parts.Add("Pano", root, MeshFactory.RoundedBox(new Vector3(w, h, 0.03f), 0.01f), board, new Vector3(0f, h * 0.5f, -0.005f), outline: false);

        // okunmayan satirlar: silik tebesir izleri (kisa cizgi obekleri)
        for (int row = 0; row < 4; row++)
        {
            float y = h - 0.16f - row * 0.19f;
            float x = -w * 0.44f;
            for (int k = 0; k < 6; k++)
            {
                float len = 0.12f + 0.22f * Mathf.Abs(Mathf.Sin(row * 3.1f + k * 1.7f));
                if (x + len > w * 0.44f) break;
                Parts.Add("Silik", root, MeshFactory.RoundedBox(new Vector3(len, 0.055f, 0.004f), 0.002f), faded,
                    new Vector3(x + len * 0.5f, y, -0.023f), outline: false, castShadow: false);
                x += len + 0.07f + 0.05f * Mathf.Abs(Mathf.Cos(k + row));
            }
        }
        // okunan satir
        WorldText.Create(root, ReadableLine, new Vector3(0f, 0.17f, -0.025f), 0.2f, chalk);
        return root;
    }
}
