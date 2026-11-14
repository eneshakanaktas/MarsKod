using System;
using UnityEngine;

// Bolge 8 · 71: kanyonun ucunda arastirma istasyonunun dis cephesi (alanin arkasinda). Prefabrik, koseleri yuvarlak, alcak moduller:
// ortada kapi modulu, iki yaninda birer modul. Krem govde kumla asinmis, koloni turuncusu seritler. Kapi tek kanatli surgulu, yari
// acik: icerisi karanlik (72'de iceri girilir). Kapinin ustunde armanin yeri bos (Gorev 4: ColonyCrest). Iki yanda kanyon duvarina
// dayanan alcak kayalar, kapinin onunde yere birakilmis iki kasa (alan disi, sus). Ust yazilara (ekranin ust ~%20'si) uzanmaz.
public static class StationFacade
{
    // IKIZ: Ground.shader STATION_FACADE_GAP (beton onluk cepheye kadar uzanir)
    public const float Gap = 1.0f;   // alanin arka kenari -> cephenin on yuzu

    // Yukseklikler: kamera yukaridan dik bakar; cephe ust yazilara ve sayaclara uzanmasin (goruntude ayarlandi)
    const float DoorModuleWidth = 1.5f, DoorModuleHeight = 0.95f;
    const float SideModuleWidth = 2.1f, SideModuleHeight = 0.72f;
    const float Depth = 1.2f, Sink = 0.12f;
    const float DoorWidth = 0.6f, DoorHeight = 0.7f;

    static readonly Material Hull = Mats.Lit(Mats.Hex("#CDC2AE"), 0.25f);
    static readonly Material HullWorn = Mats.Lit(Mats.Hex("#B39E86"), 0.15f);   // alt kenar: kum yemis
    static readonly Material Stripe = Mats.Lit(StationRoom.ColonyOrange, 0.25f);
    static readonly Material Frame = Mats.Lit(Mats.Hex("#4A4C54"), 0.4f);
    static readonly Material Leaf = Mats.Lit(Mats.Hex("#8C9098"), 0.45f);
    static readonly Material Dark = Mats.Unlit(Mats.Hex("#0C0B0E"));
    static readonly Material Glass = Mats.Lit(Mats.Hex("#1C2028"), 0.8f);
    static readonly Material Sand = Mats.Lit(Mats.Hex("#9A7258"), 0.08f);

    // Doner: armanin konacagi yer (kapinin ustu, cephenin on yuzu; yerel -Z kameraya bakar)
    public static Transform Build(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Istasyon-Cephe", parent);
        float frontZ = areaHalf.y + Gap;
        float baseY = ground(0f, frontZ) - Sink;
        root.localPosition = new Vector3(0f, baseY, frontZ);

        Module(root, 0f, DoorModuleWidth, DoorModuleHeight + Sink, stripeAt: DoorModuleHeight - 0.08f);
        foreach (float side in new[] { -1f, 1f })
        {
            float x = side * (DoorModuleWidth + SideModuleWidth) * 0.5f;
            Module(root, x, SideModuleWidth, SideModuleHeight + Sink, stripeAt: 0.46f);
            foreach (float px in new[] { -0.45f, 0.45f })   // iki kucuk yuvarlak lomboz
            {
                var port = Parts.Add("Lomboz", root, MeshFactory.RoundedCylinder(0.085f, 0.03f, 0.01f, 16), Glass,
                    new Vector3(x + px, Sink + 0.28f, -0.012f), outline: false);
                port.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
        Door(root);

        var crest = Parts.Empty("Arma-Yeri", root);
        crest.localPosition = new Vector3(0f, Sink + DoorHeight + 0.08f, -0.01f);

        SandAlongBase(parent, areaHalf, frontZ, ground);
        SideRocks(parent, areaHalf, frontZ, ground);
        // kapinin onunde birakilmis iki kasa (alanin disinda, betonun ustunde)
        var crates = Parts.Empty("Iz-Kasalar", parent);
        float cz = areaHalf.y + 0.45f;
        crates.localPosition = new Vector3(0.95f, ground(0.95f, cz), cz);
        crates.localRotation = Quaternion.Euler(0f, -12f, 0f);
        Obstacles.Crate(crates, Vector3.zero, new Vector3(0.36f, 0.26f, 0.3f), lidOpen: false, tilt: 0f);
        Obstacles.Crate(crates, new Vector3(0.38f, 0f, 0.1f), new Vector3(0.26f, 0.2f, 0.24f), lidOpen: true, tilt: 18f);
        return crest;
    }

    // Tek modul: yuvarlak koseli kutu, on yuzu z = 0'da; govdenin alti kum yemis bant, ustte turuncu serit
    static void Module(Transform root, float x, float width, float height, float stripeAt)
    {
        Parts.Add("Modul", root, MeshFactory.RoundedBox(new Vector3(width, height, Depth), 0.12f), Hull, new Vector3(x, height * 0.5f, Depth * 0.5f));
        Parts.Add("Modul-Alt", root, MeshFactory.RoundedBox(new Vector3(width + 0.01f, Sink + 0.14f, 0.04f), 0.02f, 2), HullWorn,
            new Vector3(x, (Sink + 0.14f) * 0.5f, -0.005f), outline: false, castShadow: false);
        Parts.Add("Serit", root, MeshFactory.RoundedBox(new Vector3(width - 0.2f, 0.07f, 0.02f), 0.01f, 2), Stripe,
            new Vector3(x, Sink + stripeAt, -0.01f), outline: false, castShadow: false);
    }

    // Surgulu kapi: koyu kasa, karanlik aciklik, kanat saga kaymis (yarisi acik)
    static void Door(Transform root)
    {
        float y = Sink + DoorHeight * 0.5f;
        Parts.Add("Kapi-Kasa", root, MeshFactory.RoundedBox(new Vector3(DoorWidth + 0.12f, DoorHeight + 0.08f, 0.05f), 0.02f, 2), Frame,
            new Vector3(0f, y + 0.02f, -0.02f), outline: false);
        Parts.Add("Kapi-Ici", root, MeshFactory.RoundedBox(new Vector3(DoorWidth, DoorHeight, 0.02f), 0.005f, 1), Dark,
            new Vector3(0f, y, -0.05f), outline: false, castShadow: false);
        Parts.Add("Kapi-Kanat", root, MeshFactory.RoundedBox(new Vector3(DoorWidth * 0.98f, DoorHeight, 0.035f), 0.01f, 2), Leaf,
            new Vector3(DoorWidth * 0.55f, y, -0.075f));
        Parts.Add("Kapi-Kol", root, MeshFactory.RoundedBox(new Vector3(0.03f, 0.14f, 0.02f), 0.008f, 2), Frame,
            new Vector3(DoorWidth * 0.55f - DoorWidth * 0.4f, y, -0.1f), outline: false, castShadow: false);
    }

    // Cephenin dibine savrulmus kum (alcak, yassi yiginlar)
    static void SandAlongBase(Transform parent, Vector2 areaHalf, float frontZ, Func<float, float, float> ground)
    {
        float[] xs = { -2.4f, -1.1f, 1.2f, 2.5f };
        for (int i = 0; i < xs.Length; i++)
        {
            float x = xs[i], z = frontZ - 0.08f;
            var heap = Parts.Add("Iz-Kum", parent, MeshFactory.Sphere(0.5f, 8, 14), Sand, new Vector3(x, ground(x, z) - 0.02f, z), outline: false);
            heap.localScale = new Vector3(0.8f + 0.2f * (i % 2), 0.12f, 0.36f);
        }
    }

    // Iki yanda kanyon duvarina dayanan alcak, katmanli kayalar (cephenin iki ucunu kapatir)
    static void SideRocks(Transform parent, Vector2 areaHalf, float frontZ, Func<float, float, float> ground)
    {
        string[] colors = { "#8E5A44", "#A8705A", "#7A4A38" };
        foreach (float side in new[] { -1f, 1f })
            for (int k = 0; k < 3; k++)
            {
                float x = side * (areaHalf.x + 0.6f + k * 0.55f), z = frontZ - 0.1f + k * 0.35f;
                var rock = Parts.Add("Iz-Kaya", parent, MeshFactory.Rock(0.42f - k * 0.04f, 710 + k + (side > 0 ? 5 : 0)),
                    Mats.Lit(Mats.Hex(colors[k]), 0.12f), new Vector3(x, ground(x, z) + 0.05f, z));
                rock.localScale = new Vector3(1.2f, 0.75f + 0.25f * k, 1f);
                rock.localRotation = Quaternion.Euler(0f, k * 47f + side * 20f, 0f);
            }
    }
}
