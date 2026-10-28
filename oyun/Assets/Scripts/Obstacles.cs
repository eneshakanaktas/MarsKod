using MarsKod.Dunya;
using UnityEngine;

// Bolumdeki engel (haritada K): oyun kurali her bolgede ayni (gecilmez), gorunusu bolgeye gore.
// Kod bilmeyen de "buradan gecilmez" diye okusun: kareyi dolduran iri bir parca + yaninda kucuk bir parca.
// Bolum icindeki yere gore: kanyon deposunun icinde (Bolum 53-56) kaya yerine sandik; dar yan kanyonda (67-70) duvardan
// kopmus kanyon kayasi, nehir yataginda suyun yuvarlattigi taslar.
public static class Obstacles
{
    public static void Create(Region region, Transform parent, Vector3 pos, int index, int levelNumber)
    {
        switch (region)
        {
            case Region.PolarIce: IceBlock(parent, pos, index); break;
            case Region.CraterField: Rock(parent, pos, index, "#5C4C50", "#463A3F"); break;   // koyu bazalt
            case Region.Dunes:
            case Region.AntennaHill: Rock(parent, pos, index, "#8A5440", "#6E4232"); SandDrift(parent, pos, index); break;
            case Region.Canyon:
                if (CanyonTraces.IsDepot(levelNumber)) Crates(parent, pos, index); else LayeredRock(parent, pos, index);
                break;
            case Region.Riverbed:
                if (RiverbedTraces.IsSideCanyon(levelNumber)) LayeredRock(parent, pos, index); else RiverStones(parent, pos, index);
                break;
            default: Rock(parent, pos, index, "#7A4A3B", "#5E3A2F"); break;
        }
    }

    // Nehir yatagi: eski suyun yuvarlattigi taslar. Iri, yassi bir tas; ustunde ya da yaninda daha kucuk biri; yanda bir cakil.
    // Diplerinde su cekilirken biriken acik renkli kurumus camur halkasi.
    static void RiverStones(Transform parent, Vector3 p, int i)
    {
        var root = Parts.Empty("Obstacle", parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(0f, i * 67f + 15f, 0f);
        var silt = Parts.Add("Camur", root, MeshFactory.Sphere(0.5f, 10, 18), Mats.Lit(Mats.Hex("#A57A60"), 0.08f), new Vector3(0f, -0.005f, 0f), outline: false);
        silt.localScale = new Vector3(0.92f, 0.035f, 0.84f);
        Stone(root, new Vector3(0f, 0.17f, 0f), new Vector3(0.74f, 0.42f, 0.64f), "#7A5244");                 // iri, yassi
        bool stacked = i % 2 == 0;
        Stone(root, stacked ? new Vector3(0.05f, 0.43f, 0.02f) : new Vector3(0.34f, 0.11f, -0.24f),
              new Vector3(0.38f, 0.24f, 0.32f), "#9A6A55");                                                  // kucuk: ustte ya da yanda
        Stone(root, new Vector3(-0.34f, 0.05f, -0.2f), new Vector3(0.15f, 0.1f, 0.13f), "#6A463A");    // cakil
    }

    // Yuvarlak tas: basik kure (size: tam en, boy, derinlik)
    static void Stone(Transform parent, Vector3 centre, Vector3 size, string color)
    {
        var s = Parts.Add("Tas", parent, MeshFactory.Sphere(0.5f, 12, 20), Mats.Lit(Mats.Hex(color), 0.18f), centre);
        s.localScale = size;
    }

    static void Rock(Transform parent, Vector3 p, int i, string bigColor, string smallColor)
    {
        var big = Parts.Add("Obstacle", parent, MeshFactory.Rock(0.34f, 200 + i), Mats.Lit(Mats.Hex(bigColor), 0.15f), p + new Vector3(0f, 0.05f, 0f));
        big.localRotation = Quaternion.Euler(0, i * 71f + 20f, 0);
        var small = Parts.Add("Obstacle", parent, MeshFactory.Rock(0.13f, 300 + i), Mats.Lit(Mats.Hex(smallColor), 0.12f), p + new Vector3(0.28f, 0.02f, -0.22f));
        small.localRotation = Quaternion.Euler(0, i * 37f, 0);
    }

    // Kanyon: duvarlardan kopmus tortu kayasi. Ust uste kaymis yassi katmanlar (biri acik renkli bant), yaninda kucuk bir parca.
    static void LayeredRock(Transform parent, Vector3 p, int i)
    {
        string[] colors = { "#8E5A44", "#A8705A", "#7A4A38" };   // alttan uste: koyu, acik bant, koyu
        float y = 0.02f;
        for (int k = 0; k < 3; k++)
        {
            float r = 0.36f - k * 0.06f;
            var slab = Parts.Add("Obstacle", parent, MeshFactory.Rock(r, 220 + i * 3 + k), Mats.Lit(Mats.Hex(colors[k]), 0.12f),
                p + new Vector3(0.04f * Mathf.Sin(i + k * 2.1f), y, 0.04f * Mathf.Cos(i * 1.7f + k)));
            slab.localScale = new Vector3(1f, 0.5f, 1f);
            slab.localRotation = Quaternion.Euler(0f, i * 71f + k * 40f, 0f);
            y += r * 0.62f;
        }
        var chip = Parts.Add("Obstacle", parent, MeshFactory.Rock(0.13f, 300 + i), Mats.Lit(Mats.Hex("#7A4A38"), 0.12f), p + new Vector3(0.3f, 0.02f, -0.22f));
        chip.localScale = new Vector3(1f, 0.6f, 1f);
        chip.localRotation = Quaternion.Euler(0, i * 37f, 0);
    }

    // Kanyon deposu: kareyi dolduran iri sandik + ustunde (ya da yaninda) kucuk sandik. Soluk haki govde ve kapak, koyu metal kose seritleri,
    // on yuzde soluk sari serit; bazilarinin kapagi acik. Hafif egik duruslar: firtinada itilip kakilmis.
    static void Crates(Transform parent, Vector3 p, int i)
    {
        var root = Parts.Empty("Obstacle", parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(0f, (i % 2 == 0 ? 1f : -1f) * (4f + 3f * i), 0f);
        Crate(root, new Vector3(-0.03f, 0f, 0.02f), new Vector3(0.56f, 0.42f, 0.48f), lidOpen: i % 3 == 1, tilt: 0f);
        bool onTop = i % 2 == 0;
        Crate(root, onTop ? new Vector3(0.04f, 0.42f, 0.04f) : new Vector3(0.3f, 0f, -0.24f), new Vector3(0.3f, 0.26f, 0.28f),
            lidOpen: false, tilt: onTop ? 9f : -6f);
    }

    static void Crate(Transform parent, Vector3 basePos, Vector3 size, bool lidOpen, float tilt)
    {
        var body = Mats.Lit(Mats.Hex("#A89A72"), 0.2f);
        var metal = Mats.Lit(Mats.Hex("#4E4C52"), 0.4f);
        var stripe = Mats.Lit(Mats.Hex("#C9A24A"), 0.2f);
        var crate = Parts.Empty("Sandik", parent);
        crate.localPosition = basePos;
        crate.localRotation = Quaternion.Euler(0f, tilt, 0f);
        Parts.Add("Govde", crate, MeshFactory.RoundedBox(size, 0.02f), body, new Vector3(0f, size.y * 0.5f, 0f));
        // dikey kose seritleri
        for (int k = 0; k < 4; k++)
        {
            float sx = (k % 2 == 0 ? -1f : 1f) * (size.x * 0.5f - 0.012f), sz = (k < 2 ? -1f : 1f) * (size.z * 0.5f - 0.012f);
            Parts.Add("Kose", crate, MeshFactory.RoundedBox(new Vector3(0.04f, size.y + 0.006f, 0.04f), 0.008f), metal,
                new Vector3(sx, size.y * 0.5f, sz), outline: false);
        }
        // on yuzde (kameraya bakan, -z) soluk sari serit
        Parts.Add("Serit", crate, MeshFactory.RoundedBox(new Vector3(size.x * 0.6f, size.y * 0.16f, 0.006f), 0.002f), stripe,
            new Vector3(0f, size.y * 0.55f, -size.z * 0.5f - 0.003f), outline: false, castShadow: false);
        // kapak: kapali ya da arkaya dogru acilmis
        var lid = Parts.Add("Kapak", crate, MeshFactory.RoundedBox(new Vector3(size.x + 0.02f, 0.03f, size.z + 0.02f), 0.01f), body,
            lidOpen ? new Vector3(0f, size.y + 0.09f, size.z * 0.5f + 0.02f) : new Vector3(0f, size.y + 0.012f, 0f));
        if (lidOpen) lid.localRotation = Quaternion.Euler(-40f, 0f, 0f);
    }

    // Kum tepelerinde ruzgar (batidan) kumu kayanin dibine yigar: kayanin bati yaninda yassi, yuvarlak bir kum yigini
    static void SandDrift(Transform parent, Vector3 p, int i)
    {
        var drift = Parts.Add("Obstacle-Kum", parent, MeshFactory.Sphere(0.5f, 10, 16), Mats.Lit(DuneTraces.Sand, 0.1f),
            p + new Vector3(-0.22f, -0.01f, 0.05f * Mathf.Sin(i * 1.3f)), outline: false);
        drift.localScale = new Vector3(0.5f, 0.16f, 0.58f);
    }

    // Koseli buz blogu: soluk mavi, ustunde kar; yaninda kucuk bir blok. Parlamaz (toplanan parlak buz kristalleriyle karismasin).
    static void IceBlock(Transform parent, Vector3 p, int i)
    {
        var ice = Mats.Lit(Mats.Hex("#9FBAD6"), 0.75f);
        var snow = Mats.Lit(Mats.Hex("#E4ECF5"), 0.3f);
        var block = Parts.Empty("Obstacle", parent);
        block.localPosition = p;
        block.localRotation = Quaternion.Euler(4f * Mathf.Sin(i * 1.7f), i * 71f + 20f, 5f * Mathf.Cos(i * 2.3f));
        var size = new Vector3(0.56f, 0.44f, 0.5f);
        Parts.Add("Ice", block, MeshFactory.RoundedBox(size, 0.06f), ice, new Vector3(0f, size.y * 0.5f, 0f));
        Parts.Add("Snow", block, MeshFactory.RoundedBox(new Vector3(size.x - 0.04f, 0.06f, size.z - 0.04f), 0.03f), snow, new Vector3(0f, size.y + 0.005f, 0f), outline: false);

        var chip = Parts.Add("Obstacle", parent, MeshFactory.RoundedBox(new Vector3(0.2f, 0.15f, 0.17f), 0.035f), ice, p + new Vector3(0.3f, 0.075f, -0.24f));
        chip.localRotation = Quaternion.Euler(8f, i * 37f, -6f);
    }
}
