using UnityEngine;

// Bolge 3 (kraterli duzluk, Bolum 21-30) hikaye izleri: docs/tasarim/senaryo-bolge-03.md.
// Hepsi alanin arkasinda (kuzey kenarin otesinde) durur; bulmacayi etkilemez.
// Bakim kulubesi 23-24'te uzakta, 25'ten sonra yakinda; her bolum ona bir ayrinti ekler.
public static class CraterTraces
{
    public const int FirstLevel = 21, ShedFarLevel = 23, ShedNearLevel = 25, CalendarLevel = 27, ToyLevel = 28, LastLevel = 30;

    // Bolge 3 acilisi: kara ekranda iki satir
    public static readonly string[] TransitionLines = { "Yeni bölge: Kraterli düzlük", "Hedef: güneş enerjisi" };

    // Ece'nin boyasi; Bolge 4'teki turkuaz direk numarasi ve kurdele de bu renk (senaryo-bolge-04.md)
    public static readonly Color EceColor = Mats.Hex("#2FC4B8");

    public static void Build(int levelNumber, Transform parent, Vector2 areaHalf)
    {
        float back = areaHalf.y + 0.55f;
        switch (levelNumber)
        {
            case 21: Traces.StormDebris(parent, new Vector3(-1.5f, 0f, back + 0.2f)); CableReel(parent, new Vector3(1.2f, 0f, back)); break;
            case 22: Traces.StormDebris(parent, new Vector3(-1.5f, 0f, back + 0.2f)); PartsCrate(parent, new Vector3(1.0f, 0f, back)); break;
        }
        if (levelNumber >= ShedNearLevel && levelNumber <= LastLevel)
            Shed(parent, new Vector3(1.1f, 0f, areaHalf.y + 0.9f), 1.5f, levelNumber);
        else if (levelNumber >= ShedFarLevel && levelNumber < ShedNearLevel)
            Shed(parent, new Vector3(2.0f, 0f, areaHalf.y + 1.25f), 0.7f, levelNumber);   // zeminin bittigi yerden once kalmali
    }

    // Bolum 21: firtinanin yuvarladigi kablo makarasi (yan yatmis)
    static void CableReel(Transform parent, Vector3 p)
    {
        var reel = Parts.Empty("Iz-Makara", parent);
        reel.localPosition = p + new Vector3(0f, 0.13f, 0f);
        reel.localRotation = Quaternion.Euler(0f, 30f, 90f);
        var wood = Mats.Lit(Mats.Hex("#8A6A4A"), 0.2f);
        foreach (float y in new[] { -0.09f, 0.09f })
            Parts.Add("Kenar", reel, MeshFactory.RoundedCylinder(0.13f, 0.02f, 0.006f, 16), wood, new Vector3(0f, y, 0f));
        Parts.Add("Kablo", reel, MeshFactory.RoundedCylinder(0.09f, 0.16f, 0.02f, 16), Mats.Lit(Mats.Hex("#232323"), 0.25f), Vector3.zero, outline: false);
    }

    // Bolum 22: toplanan parcalar icin acik yedek parca sandigi
    static void PartsCrate(Transform parent, Vector3 p)
    {
        var crate = Parts.Empty("Iz-Sandik", parent);
        crate.localPosition = p;
        crate.localRotation = Quaternion.Euler(0f, -15f, 0f);
        Parts.Add("Govde", crate, MeshFactory.RoundedBox(new Vector3(0.4f, 0.2f, 0.26f), 0.02f), Mats.Lit(Mats.Hex("#5C6470"), 0.3f), new Vector3(0f, 0.1f, 0f));
        Parts.Add("Serit", crate, MeshFactory.RoundedBox(new Vector3(0.41f, 0.04f, 0.27f), 0.01f), Mats.Lit(Mats.Hex("#E07B3C"), 0.3f), new Vector3(0f, 0.14f, 0f), outline: false);
        var lid = Parts.Add("Kapak", crate, MeshFactory.RoundedBox(new Vector3(0.4f, 0.02f, 0.26f), 0.01f), Mats.Lit(Mats.Hex("#4A515C"), 0.3f), new Vector3(0f, 0.3f, 0.2f));
        lid.localRotation = Quaternion.Euler(-70f, 0f, 0f);   // arkaya acilmis
    }

    // Bakim kulubesi; on yuzu (kapi) guneye, kameraya bakar. Ayrintilar bolume gore eklenir.
    static void Shed(Transform parent, Vector3 p, float scale, int levelNumber)
    {
        var root = Parts.Empty("Iz-Kulube", parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(0f, -12f, 0f);
        root.localScale = Vector3.one * scale;

        var wall = Mats.Lit(Mats.Hex("#B8B2A6"), 0.3f);
        var frame = Mats.Lit(Mats.Hex("#6E675C"), 0.25f);
        const float front = -0.35f;   // on duvarin yuzu (z)
        Parts.Add("Duvar", root, MeshFactory.RoundedBox(new Vector3(0.9f, 0.62f, 0.7f), 0.03f), wall, new Vector3(0f, 0.31f, 0f));
        var roof = Parts.Add("Cati", root, MeshFactory.RoundedBox(new Vector3(1.02f, 0.05f, 0.84f), 0.02f), Mats.Lit(Mats.Hex("#7A6E66"), 0.3f), new Vector3(0f, 0.66f, 0f));
        roof.localRotation = Quaternion.Euler(-8f, 0f, 0f);   // one dogru egik
        // Kapinin ardindaki karanlik ic (kapi kapaliyken gorunmez)
        Parts.Add("Ic", root, MeshFactory.RoundedBox(new Vector3(0.3f, 0.46f, 0.01f), 0.005f), Mats.Lit(Mats.Hex("#141218"), 0.1f), new Vector3(0.12f, 0.23f, front - 0.002f), outline: false);
        Parts.Add("Pervaz", root, MeshFactory.RoundedBox(new Vector3(0.035f, 0.5f, 0.02f), 0.006f), frame, new Vector3(0.29f, 0.25f, front - 0.01f), outline: false);
        Parts.Add("Pervaz", root, MeshFactory.RoundedBox(new Vector3(0.035f, 0.5f, 0.02f), 0.006f), frame, new Vector3(-0.05f, 0.25f, front - 0.01f), outline: false);

        bool near = levelNumber >= ShedNearLevel;
        if (near) HeightMarks(root, new Vector3(0.29f, 0f, front - 0.021f));
        if (levelNumber >= CalendarLevel) Calendar(root, new Vector3(0.12f, 0.3f, front - 0.011f));
        Door(root, new Vector3(-0.03f, 0.23f, front - 0.015f), near, open: levelNumber >= CalendarLevel);
        if (levelNumber >= ToyLevel) ToyShelf(root, new Vector3(-0.24f, 0.3f, front));
    }

    // Kapi: menteşe sol kenarda. Bolum 25'ten itibaren ustunde cocuk eliyle "ECE"; 27'den sonra aralik.
    static void Door(Transform root, Vector3 hingePos, bool withName, bool open)
    {
        var hinge = Parts.Empty("Mentese", root);
        hinge.localPosition = hingePos;
        hinge.localRotation = Quaternion.Euler(0f, open ? 55f : 0f, 0f);   // disari (kameraya) acilir
        Parts.Add("Kapi", hinge, MeshFactory.RoundedBox(new Vector3(0.3f, 0.46f, 0.025f), 0.01f), Mats.Lit(Mats.Hex("#8E8676"), 0.3f), new Vector3(0.15f, 0f, 0f));
        if (withName) EceName(hinge, new Vector3(0.15f, 0.08f, -0.014f));
    }

    // "ECE": buyuk, egri bugru harfler (cocuk eli). Her harf cubuklardan; harfler hafif egik ve farkli boyda.
    static void EceName(Transform door, Vector3 center)
    {
        var paint = Mats.Lit(EceColor, 0.2f);
        var letters = new[] { ('E', -0.085f, 6f, 1.0f), ('C', 0f, -5f, 0.9f), ('E', 0.085f, 9f, 1.08f) };
        foreach (var (ch, x, tilt, size) in letters)
        {
            var letter = Parts.Empty("Harf-" + ch, door);
            letter.localPosition = center + new Vector3(x, 0f, 0f);
            letter.localRotation = Quaternion.Euler(0f, 0f, tilt);
            letter.localScale = Vector3.one * size;
            const float h = 0.11f, w = 0.065f, t = 0.02f;
            Stroke(letter, paint, new Vector3(-w * 0.5f, 0f, 0f), new Vector3(t, h, t));        // dik cubuk
            Stroke(letter, paint, new Vector3(0f, h * 0.5f, 0f), new Vector3(w, t, t));          // ust
            Stroke(letter, paint, new Vector3(0f, -h * 0.5f, 0f), new Vector3(w, t, t));         // alt
            if (ch == 'E') Stroke(letter, paint, new Vector3(-0.005f, 0f, 0f), new Vector3(w * 0.8f, t, t));   // orta
        }
    }

    static void Stroke(Transform parent, Material mat, Vector3 pos, Vector3 size) =>
        Parts.Add("Cubuk", parent, MeshFactory.RoundedBox(size, 0.006f), mat, pos, outline: false, castShadow: false);

    // Kapi pervazinda uc boy cizgisi (ECE 7, 8, 9); her biri bir oncekinden biraz yuksekte
    static void HeightMarks(Transform root, Vector3 framePos)
    {
        var pencil = Mats.Lit(Mats.Hex("#26221E"), 0.1f);
        foreach (float y in new[] { 0.25f, 0.29f, 0.33f })
            Parts.Add("Boy-Cizgisi", root, MeshFactory.RoundedBox(new Vector3(0.075f, 0.012f, 0.006f), 0.003f), pencil, framePos + new Vector3(0f, y, 0f), outline: false, castShadow: false);
    }

    // Bolum 27: kapi aralaninca arka duvarda gorunen takvim; satirlar gri, son gunlerin ustu kirmizi
    static void Calendar(Transform root, Vector3 p)
    {
        var sheet = Parts.Add("Takvim", root, MeshFactory.RoundedBox(new Vector3(0.13f, 0.15f, 0.004f), 0.004f), Mats.Lit(Mats.Hex("#E8E4DA"), 0.2f), p, outline: false, castShadow: false);
        var row = Mats.Lit(Mats.Hex("#9A968E"), 0.1f);
        var red = Mats.Lit(Mats.Hex("#D23A30"), 0.2f);
        for (int r = 0; r < 4; r++)
            Parts.Add("Satir", sheet, MeshFactory.RoundedBox(new Vector3(0.1f, 0.008f, 0.003f), 0.002f), row, new Vector3(0f, 0.04f - r * 0.03f, -0.003f), outline: false, castShadow: false);
        // isaretli son gunler ilk iki satirda; sonrasi bos
        for (int k = 0; k < 6; k++)
            Parts.Add("Isaret", sheet, MeshFactory.RoundedBox(new Vector3(0.012f, 0.012f, 0.003f), 0.002f), red,
                new Vector3(-0.04f + (k % 4) * 0.025f, 0.045f - (k / 4) * 0.03f, -0.005f), outline: false, castShadow: false);
    }

    // Bolum 28: kapinin solunda raf; ustunde el yapimi oyuncak robot (bir kolu kopmus, rafta yaninda duruyor)
    static void ToyShelf(Transform root, Vector3 wallPos)
    {
        var shelf = Parts.Add("Raf", root, MeshFactory.RoundedBox(new Vector3(0.24f, 0.02f, 0.09f), 0.006f), Mats.Lit(Mats.Hex("#8A6A4A"), 0.2f), wallPos + new Vector3(0f, 0f, -0.045f));
        var toy = Parts.Empty("Oyuncak-Robot", shelf);
        toy.localPosition = new Vector3(-0.03f, 0.01f, 0f);
        toy.localRotation = Quaternion.Euler(0f, 15f, 0f);
        var tin = Mats.Lit(Mats.Hex("#C9C2B0"), 0.35f);
        Parts.Add("Govde", toy, MeshFactory.RoundedBox(new Vector3(0.05f, 0.06f, 0.04f), 0.008f), tin, new Vector3(0f, 0.03f, 0f));
        Parts.Add("Bas", toy, MeshFactory.RoundedBox(new Vector3(0.042f, 0.035f, 0.035f), 0.008f), tin, new Vector3(0f, 0.08f, 0f));
        var eye = Mats.Lit(Mats.Hex("#FFC23A"), 0.4f);
        foreach (float x in new[] { -0.009f, 0.009f })
            Parts.Add("Goz", toy, MeshFactory.Sphere(0.0055f, 6, 8), eye, new Vector3(x, 0.083f, -0.018f), outline: false, castShadow: false);
        // gogsunde Kivilcim'inkine benzer kucuk turuncu cizim
        Parts.Add("Cizim", toy, MeshFactory.RoundedBox(new Vector3(0.022f, 0.022f, 0.003f), 0.004f), Mats.Lit(Mats.Hex("#E07B3C"), 0.3f), new Vector3(0f, 0.033f, -0.021f), outline: false, castShadow: false);
        Parts.Add("Kol", toy, MeshFactory.RoundedBox(new Vector3(0.012f, 0.04f, 0.012f), 0.004f), tin, new Vector3(0.033f, 0.035f, 0f));
        var lostArm = Parts.Add("Kopuk-Kol", shelf, MeshFactory.RoundedBox(new Vector3(0.012f, 0.04f, 0.012f), 0.004f), tin, new Vector3(0.07f, 0.017f, 0f));
        lostArm.localRotation = Quaternion.Euler(0f, 30f, 90f);   // rafta yatiyor
    }
}
