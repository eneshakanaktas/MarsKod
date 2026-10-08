using UnityEngine;

// Kanyonun yakindaki kaya yuzleri (Bolge 6): alanin hemen arkasinda kanyon duvarinin dibi (58'de boya oklar burada) ve
// kanyon kenari (60: kanyonun icine inen kayalik yol). Duvarlar tortu kayasi: kat kat bantlar, ustu ve dibi duzensiz kayalarla.
public static class CanyonCliff
{
    static readonly string[] Bands = { "#7A4A38", "#A8705A", "#8E5A44", "#B07A60", "#7E4E3C" };   // alttan uste
    // Bantlarin en one cikaninin on yuzu (boya buraya; bantlar bundan 0-7 cm geride)
    public const float FaceZ = 0.03f;

    // Duvarin on yuzu yerel z = FaceZ'de, -Z'ye (kameraya) bakar; uzerine cizilecekler (oklar) donen koke eklenir.
    public static Transform Wall(Transform parent, float z, float width, float height)
    {
        var root = Parts.Empty("Iz-KanyonDuvari", parent);
        root.localPosition = new Vector3(0f, 0f, z);
        // kat kat bantlar: her bant biraz one ya da geriye kaymis, kalinliklari farkli
        float y = 0f;
        for (int k = 0; k < Bands.Length; k++)
        {
            float h = height / Bands.Length * (0.75f + 0.5f * Mathf.Abs(Mathf.Sin(k * 1.9f)));
            if (k == Bands.Length - 1) h = Mathf.Max(height - y, 0.1f);
            float inset = FaceZ + 0.07f * Mathf.Abs(Mathf.Sin(k * 2.7f));   // her bant biraz geride
            Parts.Add("Bant", root, MeshFactory.RoundedBox(new Vector3(width - 0.1f * k, h, 0.6f), 0.05f), Mats.Lit(Mats.Hex(Bands[k]), 0.12f),
                new Vector3(0.04f * Mathf.Sin(k), y + h * 0.5f, 0.3f + inset), outline: false);
            y += h;
        }
        // ust kenar, yuze gomulu cikintilar ve dip: duzensiz kayalar (kutu ya da tahta gibi gorunmesin)
        for (int i = 0; i < 7; i++)
        {
            float x = -width * 0.5f + width * (i + 0.5f) / 7f;
            var top = Parts.Add("Kaya", root, MeshFactory.Rock(0.45f + 0.12f * Mathf.Sin(i * 2.1f), 500 + i), Mats.Lit(Mats.Hex(Bands[i % Bands.Length]), 0.12f),
                new Vector3(x, height - 0.05f + 0.12f * Mathf.Sin(i * 1.3f), 0.45f));
            top.localScale = new Vector3(1.2f, 0.9f, 1f);
            if (i % 2 == 0)
            {
                var ledge = Parts.Add("Kaya", root, MeshFactory.Rock(0.22f, 560 + i), Mats.Lit(Mats.Hex(Bands[(i + 2) % Bands.Length]), 0.12f),
                    new Vector3(x + 0.35f, height * (0.3f + 0.25f * Mathf.Abs(Mathf.Sin(i))), 0.12f));
                ledge.localScale = new Vector3(1.4f, 0.5f, 0.7f);
            }
            var bottom = Parts.Add("Kaya", root, MeshFactory.Rock(0.18f + 0.06f * Mathf.Cos(i * 1.3f), 520 + i), Mats.Lit(Mats.Hex("#8E5A44"), 0.12f),
                new Vector3(x + 0.2f, 0.04f, -0.08f));
            bottom.localScale = new Vector3(1f, 0.6f, 1f);
        }
        return root;
    }

    // Bolum 60: kanyonun kenari. Kenar boyunca iri kayalar, aralarinda (pathX) asagi inen yolun ilk taslari. pos: kenarin ortasi; yaw: kenarin donusu (yerel +Z kanyona, alandan disari dogru);
    // length: kenarin boyu; pathX: yolun kenar boyunca yeri. Ok koymak icin yolun yanindaki kaya yuzeyini doner.
    public static Transform Rim(Transform parent, Vector3 pos, float yaw, float length, float pathX)
    {
        var root = Parts.Empty("Iz-KanyonKenari", parent);
        root.localPosition = pos;
        root.localRotation = Quaternion.Euler(0f, yaw, 0f);
        // kenar kayalari: yolun iki yaninda
        int count = Mathf.RoundToInt(length) + 2;
        for (int i = 0; i < count; i++)
        {
            float x = -length * 0.5f - 0.5f + i * (length + 1f) / (count - 1);
            if (Mathf.Abs(x - pathX) < 0.55f) continue;
            var rock = Parts.Add("Kaya", root, MeshFactory.Rock(0.3f + 0.1f * Mathf.Sin(i * 1.7f), 540 + i), Mats.Lit(Mats.Hex(Bands[i % Bands.Length]), 0.12f),
                new Vector3(x, 0.08f, 0.25f + 0.1f * Mathf.Cos(i)));
            rock.localScale = new Vector3(1.2f, 0.7f, 1f);
        }
        // yol: kenar kayalarinin arasindan kanyona inen yolun ilk yassi taslari (inisin kendisi 3b'deki final sahnesinde;
        // kenarin otesi arka plandaki kanyonla birlesir)
        for (int k = 0; k < 2; k++)
        {
            var slab = Parts.Add("Yol", root, MeshFactory.RoundedBox(new Vector3(0.7f - 0.08f * k, 0.03f, 0.4f), 0.015f),
                Mats.Lit(Color.Lerp(Mats.Hex("#9A6450"), Mats.Hex("#6A4038"), k), 0.1f),
                new Vector3(pathX + 0.06f * Mathf.Sin(k * 2f), 0.015f, 0.35f + 0.42f * k), outline: false, castShadow: false);
            slab.localRotation = Quaternion.Euler(0f, 8f * Mathf.Sin(k * 1.7f), 0f);
        }
        // ok icin yuzey: yolun solundaki kayanin onunde, kameraya donuk
        var surface = Parts.Empty("Yuzey", parent);
        surface.position = root.TransformPoint(new Vector3(pathX - 0.75f, 0.32f, 0.05f));
        return surface;
    }
}
