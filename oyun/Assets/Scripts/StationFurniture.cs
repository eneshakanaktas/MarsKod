using UnityEngine;

// Bolge 8 · istasyonun ic esyasi (72-80). Haritadaki engel (K) iceride bunlardan biri olur; sirayla: dolap (kapagi aralik),
// masa + devrilmis sandalye, raf (ustunde kutular), kasa yigini. Her biri tek karenin icinde (kenarlara 0,05 pay), yere basar;
// insan esyasi oldugu icin Kivilcim'dan buyuk (boyu Kivilcim'in ~1-1,6 kati). "Buradan gecilmez" okunsun: kareyi dolduran iri parca.
public static class StationFurniture
{
    const float Half = 0.45f;   // karenin yari boyu - pay

    static readonly Material Steel = Mats.Lit(Mats.Hex("#6C7A78"), 0.35f);       // dolap: soluk yesilimsi gri
    static readonly Material SteelDark = Mats.Lit(Mats.Hex("#3E4648"), 0.35f);
    static readonly Material Desk = Mats.Lit(Mats.Hex("#8E949C"), 0.3f);
    static readonly Material Leg = Mats.Lit(Mats.Hex("#45474E"), 0.4f);
    static readonly Material Seat = Mats.Lit(Mats.Hex("#2F4A5E"), 0.2f);         // sandalyenin minderi: koyu mavi
    static readonly Material Shelf = Mats.Lit(Mats.Hex("#5C636E"), 0.35f);
    static readonly Material Box = Mats.Lit(Mats.Hex("#A89A72"), 0.2f);
    static readonly Material BoxLight = Mats.Lit(Mats.Hex("#C8BCA0"), 0.2f);
    static readonly Material Paper = Mats.Lit(Mats.Hex("#D8D2C2"), 0.2f);
    static readonly Material Handle = Mats.Lit(Mats.Hex("#C9A24A"), 0.4f);

    enum Kind { Cabinet, DeskAndChair, ShelfUnit, CrateStack }

    public static void Obstacle(Transform parent, Vector3 pos, int index)
    {
        var root = Parts.Empty("Obstacle", parent);
        root.localPosition = pos;
        root.localRotation = Quaternion.Euler(0f, (index % 2 == 0 ? 1f : -1f) * (3f + 2f * (index % 3)), 0f);
        switch ((Kind)(index % 4))
        {
            case Kind.Cabinet: Cabinet(root); break;
            case Kind.DeskAndChair: DeskAndChair(root); break;
            case Kind.ShelfUnit: ShelfUnit(root); break;
            default: CrateStack(root, index); break;
        }
    }

    // Uzun metal dolap; kapaklardan biri aralik (icinde koyu bosluk), yaninda yere dusmus kucuk kutu
    static void Cabinet(Transform root)
    {
        var size = new Vector3(0.6f, 0.92f, 0.42f);
        float back = 0.02f;   // dolap karenin biraz arkasinda; ondeki kapak acilinca kareden tasmaz
        Parts.Add("Dolap", root, MeshFactory.RoundedBox(size, 0.02f), Steel, new Vector3(0f, size.y * 0.5f, back));
        Parts.Add("Dolap-Ici", root, MeshFactory.RoundedBox(new Vector3(0.27f, 0.84f, 0.01f), 0.004f, 1), SteelDark,
            new Vector3(0.14f, size.y * 0.5f, back - size.z * 0.5f - 0.003f), outline: false, castShadow: false);
        Parts.Add("Dolap-Derz", root, MeshFactory.RoundedBox(new Vector3(0.012f, 0.86f, 0.01f), 0.004f, 1), SteelDark,
            new Vector3(0f, size.y * 0.5f, back - size.z * 0.5f - 0.004f), outline: false, castShadow: false);
        Parts.Add("Kol", root, MeshFactory.RoundedBox(new Vector3(0.018f, 0.1f, 0.02f), 0.006f, 2), Handle,
            new Vector3(-0.04f, size.y * 0.55f, back - size.z * 0.5f - 0.012f), outline: false, castShadow: false);
        // sag kapak aralik: menteseden (sag kenar) one dogru acilmis
        var hinge = Parts.Empty("Kapak", root);
        hinge.localPosition = new Vector3(0.29f, 0f, back - size.z * 0.5f);
        hinge.localRotation = Quaternion.Euler(0f, -38f, 0f);
        Parts.Add("Kapak", hinge, MeshFactory.RoundedBox(new Vector3(0.28f, 0.86f, 0.02f), 0.006f, 2), Steel,
            new Vector3(-0.14f, size.y * 0.5f, -0.01f));
        var box = Parts.Add("Kutu", root, MeshFactory.RoundedBox(new Vector3(0.16f, 0.1f, 0.13f), 0.01f, 2), Box, new Vector3(-0.32f, 0.05f, -0.3f));
        box.localRotation = Quaternion.Euler(0f, 25f, 0f);
    }

    // Masa (ustunde kagitlar) + yanina devrilmis sandalye
    static void DeskAndChair(Transform root)
    {
        const float top = 0.66f;
        var desk = Parts.Empty("Masa", root);
        desk.localPosition = new Vector3(0f, 0f, 0.1f);
        Parts.Add("Masa-Ust", desk, MeshFactory.RoundedBox(new Vector3(0.82f, 0.04f, 0.5f), 0.01f, 2), Desk, new Vector3(0f, top - 0.02f, 0f));
        foreach (var (sx, sz) in new[] { (-1f, -1f), (1f, -1f), (-1f, 1f), (1f, 1f) })
            Parts.Add("Masa-Ayak", desk, MeshFactory.RoundedBox(new Vector3(0.035f, top - 0.04f, 0.035f), 0.008f, 2), Leg,
                new Vector3(sx * 0.37f, (top - 0.04f) * 0.5f, sz * 0.21f), outline: false);
        Parts.Add("Cekmece", desk, MeshFactory.RoundedBox(new Vector3(0.26f, 0.2f, 0.44f), 0.01f, 2), Desk, new Vector3(0.24f, top - 0.14f, 0f));
        for (int k = 0; k < 2; k++)
        {
            var sheet = Parts.Add("Kagit", desk, MeshFactory.RoundedBox(new Vector3(0.14f, 0.004f, 0.19f), 0.002f, 1), Paper,
                new Vector3(-0.15f + k * 0.08f, top + 0.003f + k * 0.004f, -0.03f + k * 0.04f), outline: false, castShadow: false);
            sheet.localRotation = Quaternion.Euler(0f, k * 23f - 8f, 0f);
        }
        // sandalye: masanin onunde yan yatmis (yerel yukari -> sola); oturagin eni yukarida kalir, ayaklari saga bakar
        var chair = Parts.Empty("Sandalye", root);
        chair.localPosition = new Vector3(0.2f, 0.16f, -0.28f);
        chair.localRotation = Quaternion.Euler(0f, 20f, 90f);
        chair.localScale = Vector3.one * 0.85f;
        Parts.Add("Oturak", chair, MeshFactory.RoundedBox(new Vector3(0.3f, 0.04f, 0.3f), 0.012f, 2), Seat, new Vector3(0f, 0.17f, 0f));
        Parts.Add("Arkalik", chair, MeshFactory.RoundedBox(new Vector3(0.3f, 0.3f, 0.035f), 0.012f, 2), Seat, new Vector3(0f, 0.34f, 0.14f));
        foreach (var (sx, sz) in new[] { (-1f, -1f), (1f, -1f), (-1f, 1f), (1f, 1f) })
            Parts.Add("Ayak", chair, MeshFactory.RoundedBox(new Vector3(0.022f, 0.16f, 0.022f), 0.006f, 2), Leg,
                new Vector3(sx * 0.12f, 0.08f, sz * 0.12f), outline: false);
    }

    // Acik raf: uc kat, ustunde kutular (biri yere dusmus)
    static void ShelfUnit(Transform root)
    {
        const float h = 0.95f, w = 0.84f, d = 0.36f;
        foreach (float sx in new[] { -1f, 1f })
            Parts.Add("Raf-Dikme", root, MeshFactory.RoundedBox(new Vector3(0.04f, h, d), 0.008f, 2), Shelf, new Vector3(sx * (w * 0.5f - 0.02f), h * 0.5f, 0.05f));
        float[] levels = { 0.06f, 0.4f, 0.74f };
        foreach (float y in levels)
            Parts.Add("Raf-Kat", root, MeshFactory.RoundedBox(new Vector3(w - 0.04f, 0.03f, d), 0.006f, 2), Shelf, new Vector3(0f, y, 0.05f));
        Parts.Add("Raf-Ust", root, MeshFactory.RoundedBox(new Vector3(w, 0.03f, d), 0.006f, 2), Shelf, new Vector3(0f, h - 0.015f, 0.05f));
        // kutular: katlarda farkli boylar
        var boxes = new[]
        {
            (new Vector3(-0.22f, 0.075f, 0.05f), new Vector3(0.26f, 0.24f, 0.26f), Box),
            (new Vector3(0.16f, 0.075f, 0.05f), new Vector3(0.3f, 0.2f, 0.24f), BoxLight),
            (new Vector3(-0.18f, 0.415f, 0.05f), new Vector3(0.22f, 0.18f, 0.22f), BoxLight),
            (new Vector3(0.2f, 0.755f, 0.05f), new Vector3(0.24f, 0.14f, 0.22f), Box),
        };
        foreach (var (at, size, mat) in boxes)
            Parts.Add("Kutu", root, MeshFactory.RoundedBox(size, 0.01f, 2), mat, at + new Vector3(0f, size.y * 0.5f, 0f));
        var fallen = Parts.Add("Kutu", root, MeshFactory.RoundedBox(new Vector3(0.18f, 0.12f, 0.16f), 0.01f, 2), Box, new Vector3(0.28f, 0.06f, -0.3f));
        fallen.localRotation = Quaternion.Euler(0f, -20f, 0f);
    }

    // Kasa yigini: alta iki kasa, uste bir kasa (Bolge 6'nin sandigi)
    static void CrateStack(Transform root, int index)
    {
        Obstacles.Crate(root, new Vector3(-0.17f, 0f, 0.06f), new Vector3(0.44f, 0.36f, 0.5f), lidOpen: false, tilt: 3f);
        Obstacles.Crate(root, new Vector3(0.24f, 0f, -0.05f), new Vector3(0.36f, 0.32f, 0.4f), lidOpen: index % 2 == 1, tilt: -7f);
        Obstacles.Crate(root, new Vector3(-0.08f, 0.39f, 0.06f), new Vector3(0.38f, 0.3f, 0.36f), lidOpen: false, tilt: 8f);
    }
}
