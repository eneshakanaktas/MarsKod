using System;
using UnityEngine;

// Bolge 8 · istasyonun hikaye esyalari (alanin disinda, arka duvarin dibinde; zemine basar):
//  76: Ece ile Defne'nin odasi: iki kamp yatagi (kucuk: Ece, renkli battaniye, yastiginin yaninda bez oyuncak; buyuk: Defne,
//      ayak ucunda duzgun katlanmis gri battaniye, yaninda kucuk canta). Boylar insan boyunda: Ece'ninki Kivilcim'in ~2, Defne'ninki ~3 kati.
//  79: atolye: tezgah (ustunde aletler), duvarda alet panosu, yedek parca kasalari + Kor'un kirik bacagi (K4: kimse soylemez;
//      Kor'un ne kadar iri oldugunu ima eder, Kor istasyonda yok).
// wallZ: arka duvarin on yuzunun z'si. Hicbir yazi yok.
public static class StationProps
{
    const float RobotHeight = 0.65f;   // Kivilcim'in boyu (olcek karsilastirmasi)

    static readonly Material Rail = Mats.Lit(Mats.Hex("#5A5E66"), 0.4f);
    static readonly Material Canvas = Mats.Lit(Mats.Hex("#6F7454"), 0.15f);
    static readonly Material Pillow = Mats.Lit(Mats.Hex("#D8D2C2"), 0.15f);
    static readonly Material GreyBlanket = Mats.Lit(Mats.Hex("#7E8288"), 0.12f);
    static readonly Material Bag = Mats.Lit(Mats.Hex("#5A4A3A"), 0.2f);
    static readonly Material Cloth = Mats.Lit(Mats.Hex("#C9A27A"), 0.1f);
    static readonly Color[] Patches = { Mats.Hex("#D9668A"), Mats.Hex("#F0C24E"), Mats.Hex("#5BA3D9"), Mats.Hex("#8FC97A"), Mats.Hex("#E9A15A"), Mats.Hex("#B98AD9") };

    // 76: iki kamp yatagi, duvar boyunca (alanin arkasindaki seritte). Doner: yataklarin koku.
    public static Transform Beds(Transform parent, float wallZ, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Yataklar", parent);
        float z = wallZ - BedsFromWall;
        var ece = CampBed(root, "Yatak-Ece", new Vector3(EceBedX, 0f, z), RobotHeight * 2f, 0.5f, ground);
        PatchworkBlanket(ece, RobotHeight * 2f, 0.5f);
        var toy = Parts.Empty("BezOyuncak", ece);   // yastigin yaninda kucuk bez bir sey (ayi gibi)
        toy.localPosition = new Vector3(-RobotHeight + 0.36f, BedTop + 0.04f, 0.13f);
        toy.localRotation = Quaternion.Euler(0f, 0f, -70f);   // yan yatmis
        Parts.Add("Govde", toy, MeshFactory.Sphere(0.05f, 8, 12), Cloth, Vector3.zero);
        Parts.Add("Bas", toy, MeshFactory.Sphere(0.035f, 8, 12), Cloth, new Vector3(0f, 0.07f, 0f));

        var defne = CampBed(root, "Yatak-Defne", new Vector3(1.05f, 0f, z), RobotHeight * 3f, 0.6f, ground);
        float half = RobotHeight * 1.5f;
        Parts.Add("KatliBattaniye", defne, MeshFactory.RoundedBox(new Vector3(0.42f, 0.09f, 0.46f), 0.02f, 3), GreyBlanket,
            new Vector3(half - 0.3f, BedTop + 0.045f, 0f));
        var bag = Parts.Add("Canta", root, MeshFactory.RoundedBox(new Vector3(0.3f, 0.2f, 0.18f), 0.05f), Bag,
            new Vector3(1.05f + half + 0.28f, ground(1.05f + half + 0.28f, z) + 0.1f, z + 0.05f));
        bag.localRotation = Quaternion.Euler(0f, -18f, 0f);
        return root;
    }

    const float BedTop = 0.3f;
    const float BedsFromWall = 0.38f, EceBedX = -1.5f;

    // 76: Ece'nin (kucuk) yataginin ortasi, battaniyenin ustu (3b sahnesinde Kivilcim buraya doner)
    public static Vector3 EceBedCenter(float wallZ, Func<float, float, float> ground)
    {
        float z = wallZ - BedsFromWall;
        return new Vector3(EceBedX, ground(EceBedX, z) + BedTop, z);
    }

    // Kamp yatagi: iki yan boru, dort ayak, gerili bez, bas ucunda yastik. at: ortasi (y kullanilmaz); length: boyu (x).
    static Transform CampBed(Transform root, string name, Vector3 at, float length, float width, Func<float, float, float> ground)
    {
        var bed = Parts.Empty(name, root);
        bed.localPosition = new Vector3(at.x, ground(at.x, at.z), at.z);
        float hl = length * 0.5f, hw = width * 0.5f;
        foreach (float sz in new[] { -1f, 1f })
            Parts.Add("Boru", bed, MeshFactory.RoundedBox(new Vector3(length, 0.035f, 0.035f), 0.012f, 2), Rail, new Vector3(0f, BedTop - 0.03f, sz * (hw - 0.02f)), outline: false);
        foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
                Parts.Add("Ayak", bed, MeshFactory.RoundedBox(new Vector3(0.03f, BedTop - 0.03f, 0.03f), 0.008f, 2), Rail,
                    new Vector3(sx * (hl - 0.06f), (BedTop - 0.03f) * 0.5f, sz * (hw - 0.02f)), outline: false);
        Parts.Add("Bez", bed, MeshFactory.RoundedBox(new Vector3(length - 0.04f, 0.025f, width - 0.03f), 0.01f, 2), Canvas, new Vector3(0f, BedTop - 0.005f, 0f));
        Parts.Add("Yastik", bed, MeshFactory.RoundedBox(new Vector3(0.26f, 0.07f, width * 0.72f), 0.03f, 3), Pillow, new Vector3(-hl + 0.18f, BedTop + 0.04f, 0f));
        return bed;
    }

    // Ece'nin battaniyesi: renkli yamalar, hafif burusuk (her yama biraz egik ve yuksekligi farkli)
    static void PatchworkBlanket(Transform bed, float length, float width)
    {
        var rng = new System.Random(76);
        int cols = 4, rows = 2;
        float w = (length - 0.42f) / cols, d = (width - 0.06f) / rows;
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows; r++)
            {
                var color = Patches[(c * 2 + r * 3) % Patches.Length];
                var patch = Parts.Add("Yama", bed, MeshFactory.RoundedBox(new Vector3(w + 0.01f, 0.03f, d + 0.01f), 0.01f, 2), Mats.Lit(color, 0.12f),
                    new Vector3(-length * 0.5f + 0.38f + (c + 0.5f) * w, BedTop + 0.025f + 0.012f * (float)rng.NextDouble(), -width * 0.5f + 0.03f + (r + 0.5f) * d),
                    outline: false);
                patch.localRotation = Quaternion.Euler(4f * ((float)rng.NextDouble() - 0.5f), 0f, 5f * ((float)rng.NextDouble() - 0.5f));
            }
    }

    static readonly Material Wood = Mats.Lit(Mats.Hex("#8A6A48"), 0.2f);
    static readonly Material Steel = Mats.Lit(Mats.Hex("#4E5258"), 0.45f);
    static readonly Material ToolDark = Mats.Lit(Mats.Hex("#2A2C30"), 0.4f);
    static readonly Material ToolRed = Mats.Lit(Mats.Hex("#B5452E"), 0.35f);
    static readonly Material Board = Mats.Lit(Mats.Hex("#6A6E74"), 0.25f);

    // 79: tezgah (ustunde aletler), duvarda alet panosu, yedek parca kasalari. wall: arka duvarin on yuzu (yerel y = 0 zemin).
    public static Transform Workshop(Transform parent, Transform wall, float wallZ, Func<float, float, float> ground)
    {
        var root = Parts.Empty("Atolye", parent);
        float bx = -1.7f, bz = wallZ - 0.3f;
        var bench = Parts.Empty("Tezgah", root);
        bench.localPosition = new Vector3(bx, ground(bx, bz), bz);
        const float top = 0.62f, length = 1.5f, depth = 0.46f;
        Parts.Add("Tezgah-Ust", bench, MeshFactory.RoundedBox(new Vector3(length, 0.05f, depth), 0.01f, 2), Wood, new Vector3(0f, top - 0.025f, 0f));
        Parts.Add("Tezgah-Raf", bench, MeshFactory.RoundedBox(new Vector3(length - 0.1f, 0.03f, depth - 0.06f), 0.008f, 2), Steel, new Vector3(0f, 0.16f, 0f), outline: false);
        foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
                Parts.Add("Ayak", bench, MeshFactory.RoundedBox(new Vector3(0.04f, top - 0.05f, 0.04f), 0.01f, 2), Steel,
                    new Vector3(sx * (length * 0.5f - 0.05f), (top - 0.05f) * 0.5f, sz * (depth * 0.5f - 0.04f)), outline: false);
        // aletler: mengene, cekic, anahtar, kucuk parca kutusu
        Parts.Add("Mengene", bench, MeshFactory.RoundedBox(new Vector3(0.14f, 0.1f, 0.12f), 0.015f, 2), ToolRed, new Vector3(-0.55f, top + 0.05f, 0.05f));
        var hammer = Parts.Empty("Cekic", bench);
        hammer.localPosition = new Vector3(-0.15f, top + 0.02f, -0.05f);
        hammer.localRotation = Quaternion.Euler(0f, 30f, 0f);
        Parts.Add("Sap", hammer, MeshFactory.RoundedBox(new Vector3(0.26f, 0.025f, 0.03f), 0.01f, 2), Wood, Vector3.zero, outline: false);
        Parts.Add("Kafa", hammer, MeshFactory.RoundedBox(new Vector3(0.04f, 0.04f, 0.1f), 0.008f, 2), ToolDark, new Vector3(0.13f, 0.01f, 0f), outline: false);
        var wrench = Parts.Add("Anahtar", bench, MeshFactory.RoundedBox(new Vector3(0.22f, 0.012f, 0.03f), 0.006f, 2), Steel, new Vector3(0.25f, top + 0.008f, 0.08f), outline: false);
        wrench.localRotation = Quaternion.Euler(0f, -20f, 0f);
        Parts.Add("ParcaKutusu", bench, MeshFactory.RoundedBox(new Vector3(0.18f, 0.07f, 0.12f), 0.01f, 2), Mats.Lit(StationRoom.ColonyOrange, 0.3f),
            new Vector3(0.55f, top + 0.035f, -0.02f));

        // duvarda alet panosu (cocuk boyundaki acik bantta): koyu aletlerin silueti asili
        var board = Parts.Add("AletPanosu", wall, MeshFactory.RoundedBox(new Vector3(1.3f, 0.36f, 0.02f), 0.01f, 2), Board,
            new Vector3(bx, 0.94f, -0.012f), outline: false, castShadow: false);
        var rng = new System.Random(79);
        for (int k = 0; k < 7; k++)
        {
            float x = -0.54f + k * 0.18f;
            var tool = Parts.Add("Alet", board, MeshFactory.RoundedBox(new Vector3(0.035f, 0.18f + 0.1f * (float)rng.NextDouble(), 0.02f), 0.008f, 2), ToolDark,
                new Vector3(x, -0.02f, -0.02f), outline: false, castShadow: false);
            tool.localRotation = Quaternion.Euler(0f, 0f, 10f * ((float)rng.NextDouble() - 0.5f));
        }

        // yedek parca kasalari + yerde bir disli
        var crates = Parts.Empty("YedekParca", root);
        float cx = 0.2f, cz = wallZ - 0.35f;
        crates.localPosition = new Vector3(cx, ground(cx, cz), cz);
        Obstacles.Crate(crates, Vector3.zero, new Vector3(0.46f, 0.32f, 0.4f), lidOpen: true, tilt: 4f);
        Obstacles.Crate(crates, new Vector3(0.02f, 0.35f, 0.02f), new Vector3(0.32f, 0.22f, 0.3f), lidOpen: false, tilt: -10f);
        var gear = Parts.Add("Disli", root, MeshFactory.RoundedCylinder(0.09f, 0.04f, 0.01f, 10), Steel, new Vector3(-0.5f, ground(-0.5f, cz) + 0.02f, wallZ - 0.5f));
        gear.localRotation = Quaternion.Euler(0f, 15f, 0f);
        return root;
    }

    static readonly Material MinePaint = Mats.Lit(Mats.Hex("#D9A032"), 0.3f);   // sari-turuncu maden boyasi
    static readonly Material Bare = Mats.Lit(Mats.Hex("#7E8084"), 0.5f);        // boyasi soyulmus metal
    static readonly Material Joint = Mats.Lit(Mats.Hex("#3A3C42"), 0.45f);
    static readonly Material Tread = Mats.Lit(Mats.Hex("#1E1F22"), 0.2f);
    static readonly Material Wire = Mats.Lit(Mats.Hex("#B04A2E"), 0.3f);

    // 79 (K4): Kor'un kirik bacagi. Yatik, duvar dibinde, alanin disinda; boyu Kivilcim'in ~1,5 kati (Kivilcim'in bacagi bunun yaninda minik).
    // Uyluk ile baldir arasindaki eklem kirik (araya kablolar sarkar), kapaklar ezik, boyasi yer yer soyulmus; ayak tabani genis, kalin tirtilli.
    // floorPos: bacagin ortasi (y kullanilmaz); yawDeg: duvar boyunca hafif egik.
    public static Transform KorLeg(Transform parent, Vector3 floorPos, float yawDeg, Func<float, float, float> ground)
    {
        const float thick = 0.28f;   // yatik bacagin kalinligi (yerden yuksekligi)
        var leg = Parts.Empty("KorBacak", parent);
        leg.localPosition = new Vector3(floorPos.x, ground(floorPos.x, floorPos.z), floorPos.z);
        leg.localRotation = Quaternion.Euler(0f, yawDeg, 0f);
        leg.localScale = Vector3.one * 0.9f;   // uctan uca ~1,1: yatik oldugu icin yukaridan bakista Kivilcim'dan belirgin iri
        float y = thick * 0.5f;

        // uyluk (solda, kirik ucuyla) - eklem - baldir - ayak (sagda)
        var thigh = Parts.Add("Uyluk", leg, MeshFactory.RoundedBox(new Vector3(0.34f, thick, 0.26f), 0.05f), MinePaint, new Vector3(-0.4f, y, 0f));
        thigh.localRotation = Quaternion.Euler(0f, 14f, 0f);   // kirik eklemden ayrilmis, egik duruyor
        Parts.Add("Uyluk-Kirik", leg, MeshFactory.RoundedBox(new Vector3(0.05f, thick * 0.8f, 0.2f), 0.015f, 2), Bare, new Vector3(-0.58f, y, 0.05f), outline: false);
        Parts.Add("Eklem", leg, MeshFactory.RoundedCylinder(0.1f, 0.22f, 0.02f, 16), Joint, new Vector3(-0.12f, y, 0f)).localRotation = Quaternion.Euler(90f, 0f, 0f);
        // kirik eklemden sarkan kablolar ve piston
        foreach (var (dz, len, ang) in new[] { (-0.05f, 0.16f, 20f), (0.03f, 0.14f, -15f), (0.08f, 0.12f, 35f) })
            Parts.Add("Kablo", leg, MeshFactory.RoundedCylinder(0.01f, len, 0.004f, 6), Wire, new Vector3(-0.24f, y - 0.03f, dz), outline: false)
                .localRotation = Quaternion.Euler(0f, 0f, 90f + ang);
        Parts.Add("Piston", leg, MeshFactory.RoundedCylinder(0.016f, 0.2f, 0.006f, 8), Bare, new Vector3(-0.22f, y + 0.07f, -0.06f), outline: false)
            .localRotation = Quaternion.Euler(0f, 0f, 75f);
        Parts.Add("Baldir", leg, MeshFactory.RoundedBox(new Vector3(0.4f, thick * 0.92f, 0.24f), 0.05f), MinePaint, new Vector3(0.16f, y * 0.92f, 0f));
        // ezik kapaklar: biraz egik, yuzeye oturmus plakalar; soyulmus boya lekeleri
        var dent = Parts.Add("EzikKapak", leg, MeshFactory.RoundedBox(new Vector3(0.16f, 0.02f, 0.14f), 0.01f, 2), MinePaint, new Vector3(0.1f, thick * 0.92f + 0.004f, -0.02f));
        dent.localRotation = Quaternion.Euler(6f, 10f, -9f);
        foreach (var (px, pz, sx, sz) in new[] { (0.24f, 0.05f, 0.1f, 0.07f), (-0.45f, -0.06f, 0.08f, 0.1f), (0.02f, 0.08f, 0.06f, 0.05f) })
            Parts.Add("Soyuk", leg, MeshFactory.RoundedBox(new Vector3(sx, 0.006f, sz), 0.003f, 1), Bare,
                new Vector3(px, (px > -0.2f ? thick * 0.92f : thick) + 0.003f, pz), outline: false, castShadow: false);
        // ayak: genis, kalin; tabani (sag yuz) tirtilli
        Parts.Add("Ayak", leg, MeshFactory.RoundedBox(new Vector3(0.18f, thick * 1.15f, 0.38f), 0.04f), Joint, new Vector3(0.45f, thick * 0.575f, 0.02f));
        for (int k = 0; k < 4; k++)
            Parts.Add("Tirtil", leg, MeshFactory.RoundedBox(new Vector3(0.04f, thick * 1.05f, 0.04f), 0.01f, 2), Tread,
                new Vector3(0.55f, thick * 0.575f, -0.13f + k * 0.087f), outline: false);
        return leg;
    }
}
