using UnityEngine;

// Kanyon deposunun metal rafi (Bolge 6). Dikmeler kare sinirlarinda (karelerin ortasi bos: robot rafa girip "uzanir");
// alt goz yere yakin ve karenin tamamini kaplar (drone parcalari orada durur), ust goz yalnizca karenin arka yarisinda ve
// yuksekte (robotun basina degmez). Ustunde birkac kutu.
public static class StorageShelf
{
    const float Depth = 0.86f, TopY = 0.72f;

    // center: rafin ortasi (yerde, karelerin ortasi hizasinda); length: kare sayisi kadar birim;
    // stocked: alt gozde de kutular var (depodaki dekor raf; 53'teki rafin alt gozunde drone parcalari durur)
    public static Transform Create(Transform parent, Vector3 center, float length, bool stocked)
    {
        var root = Parts.Empty("Iz-Raf", parent);
        root.localPosition = center;
        var metal = Mats.Lit(Mats.Hex("#5E636C"), 0.4f);
        var board = Mats.Lit(Mats.Hex("#565B63"), 0.35f);
        var box = Mats.Lit(Mats.Hex("#A89A72"), 0.2f);

        // dikmeler: her kare sinirinda onde ve arkada
        int posts = Mathf.RoundToInt(length) + 1;
        for (int i = 0; i < posts; i++)
        {
            float x = -length * 0.5f + i;
            foreach (float z in new[] { -Depth * 0.5f, Depth * 0.5f })
            {
                float h = z < 0f ? 0.32f : TopY + 0.05f;   // ondekiler alcak (kareleri ortmesin)
                Parts.Add("Dikme", root, MeshFactory.RoundedBox(new Vector3(0.035f, h, 0.035f), 0.008f), metal, new Vector3(x, h * 0.5f, z));
            }
        }
        // alt goz: karenin tamami; ust goz: arka yari
        Parts.Add("AltGoz", root, MeshFactory.RoundedBox(new Vector3(length, 0.03f, Depth), 0.01f), board, new Vector3(0f, 0.03f, 0f), castShadow: false);
        Parts.Add("UstGoz", root, MeshFactory.RoundedBox(new Vector3(length, 0.03f, Depth * 0.42f), 0.01f), board, new Vector3(0f, TopY, Depth * 0.29f));
        // on kenar seridi (rafin onu okunsun) ve arkada capraz destekler
        Parts.Add("OnSerit", root, MeshFactory.RoundedBox(new Vector3(length, 0.05f, 0.02f), 0.008f), metal, new Vector3(0f, 0.32f, -Depth * 0.5f), outline: false);
        Parts.Add("ArkaSerit", root, MeshFactory.RoundedBox(new Vector3(length, 0.04f, 0.02f), 0.008f), metal, new Vector3(0f, 0.4f, Depth * 0.5f), outline: false);
        // ust gozde kutular
        for (int i = 0; i < Mathf.RoundToInt(length); i++)
        {
            if (i % 2 == 1) continue;
            var b = Parts.Add("Kutu", root, MeshFactory.RoundedBox(new Vector3(0.3f, 0.2f, 0.26f), 0.015f), box,
                new Vector3(-length * 0.5f + i + 0.5f + 0.1f * Mathf.Sin(i), TopY + 0.115f, Depth * 0.3f));
            b.localRotation = Quaternion.Euler(0f, 8f * Mathf.Sin(i * 2.3f), 0f);
        }
        if (stocked)
            for (int i = 0; i < Mathf.RoundToInt(length); i++)
            {
                var b = Parts.Add("Kutu", root, MeshFactory.RoundedBox(new Vector3(0.38f, 0.26f, 0.34f), 0.015f), box,
                    new Vector3(-length * 0.5f + i + 0.5f - 0.08f * Mathf.Cos(i), 0.175f, 0.05f));
                b.localRotation = Quaternion.Euler(0f, -6f + 9f * i, 0f);
            }
        return root;
    }

    // Ust gozun yuzeyinin (ortasinin) yerel konumu: Serce oraya konur
    public static Vector3 TopCenter => new Vector3(0f, TopY + 0.015f, Depth * 0.29f);
}
