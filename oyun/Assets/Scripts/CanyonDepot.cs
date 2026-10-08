using UnityEngine;

// Kanyon deposu (Bolge 6). Iceride (53-56): alanin arkasinda ve iki yaninda alcak, yipranmis oluklu metal duvarlar; on taraf
// (kamera) acik. Koselerde dikmeler ve yalnizca kenarlar boyunca yirtik cati iskeleti (alanin ustunu kapatmaz); kirislerden
// sarkan branda parcalari. Firtina catiyi yirtmis: gunes ve toz iceri giriyor (55'te yirtik en genis: panelleri catidan dusmus).
// Arka duvar boyunca kisa raf ve kutular. Disaridan (57): alanin arkasinda, catisi yirtik alcak bir yapi.
public static class CanyonDepot
{
    const float WallH = 0.8f, PostH = 1.5f, Gap = 0.5f, BackGap = 1.05f;   // duvarlar alanin kenarindan Gap (arkada BackGap) uzakta

    // Arka duvarin on yuzu (oklar, gorev panosu buraya)
    public static float BackWallZ(Vector2 areaHalf) => areaHalf.y + BackGap - 0.03f;
    public static float WallHeight => WallH;

    // Malzemeler bir kez uretilir, her bolumde paylasilir (depo ~70 parca: parca basina yeni malzeme bellegi sisirirdi)
    static readonly Material Sheet = Mats.Lit(Mats.Hex("#6E7480"), 0.35f);
    static readonly Material Rib = Mats.Lit(Mats.Hex("#5A606B"), 0.35f);
    static readonly Material Rust = Mats.Lit(Mats.Hex("#8A5A40"), 0.15f);
    static readonly Material Beam = Mats.Lit(Mats.Hex("#4E4C52"), 0.4f);
    static readonly Material Tarp = Mats.Lit(Mats.Hex("#9A8C68"), 0.1f);

    // Doner: arka duvardaki rafin ust yuzeyinin konumu (parent'a gore; Serce 54-55'te oraya konur)
    public static Vector3 Inside(Transform parent, Vector2 areaHalf, bool wideTear)
    {
        var root = Parts.Empty("Iz-Depo", parent);
        float bx = areaHalf.x + Gap, bz = areaHalf.y + BackGap, front = -areaHalf.y + 0.3f;

        // duvarlar: arka + iki yan (on acik)
        Wall(root, new Vector3(0f, 0f, bz), bx * 2f, 0f, 1);
        Wall(root, new Vector3(-bx, 0f, (bz + front) * 0.5f), bz - front, 90f, 2);
        Wall(root, new Vector3(bx, 0f, (bz + front) * 0.5f), bz - front, -90f, 3);

        // dikmeler ve kenar kirisleri
        foreach (var p in new[] { new Vector2(-bx, bz), new Vector2(bx, bz), new Vector2(-bx, front), new Vector2(bx, front) })
            Parts.Add("Dikme", root, MeshFactory.RoundedBox(new Vector3(0.1f, PostH, 0.1f), 0.02f), Beam, new Vector3(p.x, PostH * 0.5f, p.y));
        Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(bx * 2f, 0.08f, 0.1f), 0.02f), Beam, new Vector3(0f, PostH, bz));
        Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(0.1f, 0.08f, bz - front), 0.02f), Beam, new Vector3(-bx, PostH, (bz + front) * 0.5f));
        if (!wideTear)
            Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(0.1f, 0.08f, bz - front), 0.02f), Beam, new Vector3(bx, PostH, (bz + front) * 0.5f));
        else
        {
            // sag kiris kopmus: bir ucu dikmede, obur ucu yere sarkmis
            var broken = Parts.Add("KirikKiris", root, MeshFactory.RoundedBox(new Vector3(0.1f, 0.08f, 1.9f), 0.02f), Beam, new Vector3(bx + 0.05f, 0.95f, bz - 0.8f));
            broken.localRotation = Quaternion.Euler(-32f, 0f, 4f);
        }
        // cati kaplamasindan kalanlar: arka kirise asili, yirtik branda; yanlarda sarkan seritler
        for (int i = 0; i < (wideTear ? 2 : 4); i++)
        {
            float x = -bx + 0.7f + i * (bx * 2f - 1.4f) / 3f;
            var flap = Parts.Add("Branda", root, MeshFactory.RoundedBox(new Vector3(0.7f, 0.5f + 0.15f * Mathf.Sin(i * 2f), 0.02f), 0.01f), Tarp,
                new Vector3(x, PostH - 0.3f, bz - 0.08f), outline: false);
            flap.localRotation = Quaternion.Euler(-12f + 6f * Mathf.Sin(i), 0f, 5f * Mathf.Cos(i * 1.7f));
        }
        foreach (float sx in new[] { -1f, 1f })
        {
            if (wideTear && sx > 0f) continue;
            var side = Parts.Add("Branda", root, MeshFactory.RoundedBox(new Vector3(0.02f, 0.45f, 0.9f), 0.01f), Tarp,
                new Vector3(sx * (bx - 0.06f), PostH - 0.28f, 0.6f), outline: false);
            side.localRotation = Quaternion.Euler(0f, 0f, sx * 8f);
        }

        // arka duvar boyunca kisa raf (sol tarafta; sagda pusula/yazi yok, solda dugmeler var: ortadan biraz sola)
        var shelf = StorageShelf.Create(root, new Vector3(-0.9f, 0f, bz - 0.48f), 2f, stocked: true);
        return root.localPosition + shelf.localPosition + StorageShelf.TopCenter;
    }

    // Bolum 57: deponun arka avlusu; depo alanin arkasinda, disaridan gorunur
    public static void Outside(Transform parent, Vector3 localPos)
    {
        var root = Parts.Empty("Iz-Depo", parent);
        root.localPosition = localPos;
        const float w = 4.2f, d = 2.4f;
        Wall(root, new Vector3(0f, 0f, -d * 0.5f), w, 0f, 4);
        Wall(root, new Vector3(-w * 0.5f, 0f, 0f), d, 90f, 5);
        Wall(root, new Vector3(w * 0.5f, 0f, 0f), d, -90f, 6);
        foreach (var p in new[] { new Vector2(-w * 0.5f, -d * 0.5f), new Vector2(w * 0.5f, -d * 0.5f) })
            Parts.Add("Dikme", root, MeshFactory.RoundedBox(new Vector3(0.1f, PostH, 0.1f), 0.02f), Beam, new Vector3(p.x, PostH * 0.5f, p.y));
        Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(w, 0.08f, 0.1f), 0.02f), Beam, new Vector3(0f, PostH, -d * 0.5f));
        // cati: yirtik; yalnizca iki kirik levha kalmis (biri kirise yaslanmis), yirtiktan branda sarkar
        var sheet = Parts.Add("CatiParcasi", root, MeshFactory.RoundedBox(new Vector3(1.1f, 0.03f, 0.7f), 0.01f), Sheet, new Vector3(-w * 0.32f, PostH - 0.25f, -d * 0.5f + 0.3f));
        sheet.localRotation = Quaternion.Euler(-48f, 10f, 6f);
        var sheet2 = Parts.Add("CatiParcasi", root, MeshFactory.RoundedBox(new Vector3(0.8f, 0.03f, 0.6f), 0.01f), Sheet, new Vector3(w * 0.3f, 0.35f, -d * 0.5f - 0.35f));
        sheet2.localRotation = Quaternion.Euler(-70f, -15f, 0f);
        var flap = Parts.Add("Branda", root, MeshFactory.RoundedBox(new Vector3(0.9f, 0.6f, 0.02f), 0.01f), Tarp, new Vector3(0.6f, PostH - 0.35f, -d * 0.5f - 0.05f), outline: false);
        flap.localRotation = Quaternion.Euler(-8f, 0f, 6f);
    }

    // Oluklu metal duvar parcasi: pos tabanin ortasi, yaw donusu (0: duvar x boyunca uzanir). seed: pas ve kirik levha yeri.
    static void Wall(Transform parent, Vector3 pos, float length, float yaw, int seed)
    {
        var wall = Parts.Empty("Duvar", parent);
        wall.localPosition = pos;
        wall.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Parts.Add("Levha", wall, MeshFactory.RoundedBox(new Vector3(length, WallH, 0.05f), 0.015f), Sheet, new Vector3(0f, WallH * 0.5f, 0f));
        // oluklar: yarim birimde bir ince dikey serit
        int ribs = Mathf.FloorToInt(length / 0.5f);
        for (int i = 1; i < ribs; i++)
            Parts.Add("Oluk", wall, MeshFactory.RoundedBox(new Vector3(0.03f, WallH - 0.04f, 0.07f), 0.01f), Rib,
                new Vector3(-length * 0.5f + i * length / ribs, WallH * 0.5f, 0f), outline: false, castShadow: false);
        // pas lekeleri (iki yuzde de)
        for (int i = 0; i < 3; i++)
        {
            float x = -length * 0.5f + length * Mathf.Repeat(0.23f + 0.31f * i + 0.17f * seed, 1f);
            foreach (float z in new[] { -0.03f, 0.03f })
                Parts.Add("Pas", wall, MeshFactory.RoundedBox(new Vector3(0.3f + 0.1f * i, 0.18f + 0.06f * i, 0.004f), 0.002f), Rust,
                    new Vector3(x, 0.15f + 0.12f * i, z), outline: false, castShadow: false);
        }
        // bir levha egilmis/kirik: ust kosesi disari dogru acilmis
        var bent = Parts.Add("KirikLevha", wall, MeshFactory.RoundedBox(new Vector3(0.45f, 0.4f, 0.03f), 0.01f), Sheet,
            new Vector3(-length * 0.5f + length * Mathf.Repeat(0.61f + 0.29f * seed, 1f), WallH - 0.15f, 0.05f));
        bent.localRotation = Quaternion.Euler(-24f, 0f, 9f);
    }
}
