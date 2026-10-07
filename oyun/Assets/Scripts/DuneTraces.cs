using System.Collections;
using UnityEngine;

// Bolge 4 (kum tepeleri, Bolum 31-40) hikaye izleri: docs/tasarim/senaryo-bolge-04.md.
// Hepsi alanin arkasinda (kuzey kenarin otesinde) durur; bulmacayi etkilemez.
// Isaret direkleri 9'dan 1'e geri sayar; biri yolu onceden isaretlemis. Bolum 40'ta yon bulma diregi, son direk + kurdele, devrik anten.
public static class DuneTraces
{
    public const int FirstLevel = 31, CartLevel = 37, StandFirstLevel = 38, LastLevel = 40;

    // Bolge 4 acilisi: kara ekranda iki satir
    public static readonly string[] TransitionLines = { "Yeni bölge: Kum tepeleri", "Hedef: yön bulma direği" };

    // Kumun rengi: zeminin kumuyla ayni ton (Ground.shader, duneSand); direklerin dibi, arabanin ustu, kayalarin dibindeki yigin
    public static readonly Color Sand = Mats.Hex("#B8794C");

    // Bolumun izlerini kurar; finaldeyse kapanis sahnesinin oynatacagi nesneleri dondurur.
    public static DuneFinale Build(int levelNumber, Transform parent, Vector2 areaHalf, Vector3? targetPos)
    {
        if (levelNumber < FirstLevel || levelNumber > LastLevel) return null;
        float back = areaHalf.y + 0.55f;   // alanin arka kenarinin hemen otesi
        switch (levelNumber)
        {
            case 32: Marker(parent, new Vector3(1.4f, 0f, back), 9); break;
            case 33:   // dizi uzaklara (kuzeydoguya) uzaniyor
                Marker(parent, new Vector3(-1.5f, 0f, back), 8);
                Marker(parent, new Vector3(-0.4f, 0f, back + 0.35f), 7);
                Marker(parent, new Vector3(0.7f, 0f, back + 0.7f), 6);
                break;
            case 34: Marker(parent, new Vector3(1.3f, 0f, back), 5, childHand: true); break;
            case 35: Marker(parent, new Vector3(-1.4f, 0f, back), 4); break;
            case 36: Marker(parent, new Vector3(1.7f, 0f, back), 3); break;
            case CartLevel: Marker(parent, new Vector3(-0.2f, 0f, back), 2); Cart(parent, new Vector3(-1.55f, 0f, back + 0.3f)); break;
        }
        if (levelNumber >= StandFirstLevel && levelNumber < LastLevel) CompassStand.Create(parent, new Vector3(-1.4f, 0f, back));
        if (levelNumber != LastLevel) return null;

        // Final: son direk hedefin hemen arkasinda, kurdele ona bagli; anten sag arkada kuma yatmis
        var stand = CompassStand.Create(parent, new Vector3(-1.6f, 0f, back));
        var last = Marker(parent, new Vector3(targetPos.HasValue ? targetPos.Value.x : 0.5f, 0f, back), 1);
        var ribbon = WindRibbon.Create(last, new Vector3(0.015f, 0.38f, 0f));
        var antenna = Antenna(parent, new Vector3(2.35f, 0.0f, back + 0.85f));
        return new DuneFinale(stand, ribbon, antenna);
    }

    // Isaret diregi: kuma yari gomulu, hafif egik direk; tepesinde turuncu levha, ustunde numara.
    // childHand: numara kalip yazisi degil, cocuk eliyle turkuaz boyanmis (Ece; Bolum 34).
    static Transform Marker(Transform parent, Vector3 p, int number, bool childHand = false)
    {
        var root = Parts.Empty("Iz-IsaretDiregi-" + number, parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(-4f, (number % 3 - 1) * 8f, number % 2 == 0 ? 5f : -4f);   // her biri biraz farkli egik
        root.localScale = Vector3.one * 2.4f;

        Parts.Add("Kum", root, MeshFactory.RoundedBox(new Vector3(0.22f, 0.06f, 0.18f), 0.04f), Mats.Lit(Sand, 0.15f), new Vector3(0f, 0.015f, 0f), outline: false);
        Parts.Add("Direk", root, MeshFactory.RoundedCylinder(0.016f, 0.5f, 0.006f, 10), Mats.Lit(Mats.Hex("#4F535C"), 0.4f), new Vector3(0f, 0.25f, 0f));
        // levha geriye yatik: yukaridan bakan kameraya yuzunu doner, numara okunsun
        var sign = Parts.Empty("Levha", root);
        sign.localPosition = new Vector3(0f, 0.5f, -0.012f);
        sign.localRotation = Quaternion.Euler(40f, 0f, 0f);
        Parts.Add("Plaka", sign, MeshFactory.RoundedBox(new Vector3(0.17f, 0.15f, 0.014f), 0.01f), Mats.Lit(Mats.Hex("#E07B3C"), 0.35f), Vector3.zero);
        var paint = childHand ? Mats.Lit(CraterTraces.EceColor, 0.2f) : Mats.Lit(Mats.Hex("#23222A"), 0.2f);
        Digit(sign, new Vector3(0f, 0f, -0.009f), number, paint, childHand);
        return root;
    }

    // Yedi cubuklu rakam (hesap makinesi gibi): a ust, b sag ust, c sag alt, d alt, e sol alt, f sol ust, g orta
    static readonly string[] Segments = { "abcdef", "bc", "abged", "abgcd", "fgbc", "afgcd", "afgedc", "abc", "abcdefg", "abcdfg" };

    static void Digit(Transform root, Vector3 center, int number, Material paint, bool childHand)
    {
        var digit = Parts.Empty("Rakam", root);
        digit.localPosition = center;
        if (childHand)
        {
            digit.localRotation = Quaternion.Euler(0f, 0f, 11f);   // egri bugru, biraz buyuk
            digit.localScale = Vector3.one * 1.12f;
        }
        const float w = 0.065f, h = 0.11f;
        float t = childHand ? 0.022f : 0.017f;
        int k = 0;
        foreach (char s in Segments[number])
        {
            bool horizontal = s == 'a' || s == 'd' || s == 'g';
            float x = s == 'b' || s == 'c' ? w * 0.5f : s == 'e' || s == 'f' ? -w * 0.5f : 0f;
            float y = s == 'a' ? h * 0.5f : s == 'd' ? -h * 0.5f : s == 'g' ? 0f : s == 'b' || s == 'f' ? h * 0.25f : -h * 0.25f;
            var size = horizontal ? new Vector3(w, t, 0.004f) : new Vector3(t, h * 0.5f, 0.004f);
            var stroke = Parts.Add("Cubuk", digit, MeshFactory.RoundedBox(size, 0.003f), paint, new Vector3(x, y, 0f), outline: false, castShadow: false);
            if (childHand) stroke.localRotation = Quaternion.Euler(0f, 0f, (k++ % 2 == 0 ? 7f : -6f));   // cizgiler tam oturmuyor
        }
    }

    // Bolum 37: kuma gomulmus direk arabasi. Kasasi bos (direkler dikilmis), surucu yerinde katli bir battaniye.
    static void Cart(Transform parent, Vector3 p)
    {
        var root = Parts.Empty("Iz-DirekArabasi", parent);
        root.localPosition = p + new Vector3(0f, -0.03f, 0f);
        root.localRotation = Quaternion.Euler(4f, -20f, -5f);   // bir yani kuma batmis
        root.localScale = Vector3.one * 1.3f;

        var body = Mats.Lit(Mats.Hex("#A39886"), 0.3f);
        var dark = Mats.Lit(Mats.Hex("#34313A"), 0.3f);
        var stripe = Mats.Lit(Mats.Hex("#E07B3C"), 0.3f);
        Parts.Add("Sasi", root, MeshFactory.RoundedBox(new Vector3(0.72f, 0.06f, 0.38f), 0.02f), body, new Vector3(0f, 0.13f, 0f));
        Parts.Add("Serit", root, MeshFactory.RoundedBox(new Vector3(0.73f, 0.02f, 0.385f), 0.006f), stripe, new Vector3(0f, 0.11f, 0f), outline: false);
        // kasa: alcak yan duvarlar, ici bos
        foreach (float z in new[] { -0.18f, 0.18f })
            Parts.Add("Kenar", root, MeshFactory.RoundedBox(new Vector3(0.42f, 0.1f, 0.02f), 0.006f), body, new Vector3(0.14f, 0.21f, z));
        Parts.Add("Kenar", root, MeshFactory.RoundedBox(new Vector3(0.02f, 0.1f, 0.38f), 0.006f), body, new Vector3(0.35f, 0.21f, 0f));
        // surucu yeri: koltuk + sirtlik, ustunde katli battaniye (koyu kirmizi, acik seritli)
        Parts.Add("Koltuk", root, MeshFactory.RoundedBox(new Vector3(0.16f, 0.06f, 0.2f), 0.02f), dark, new Vector3(-0.2f, 0.19f, 0f));
        Parts.Add("Sirtlik", root, MeshFactory.RoundedBox(new Vector3(0.03f, 0.14f, 0.2f), 0.012f), dark, new Vector3(-0.07f, 0.26f, 0f));
        var blanket = Parts.Add("Battaniye", root, MeshFactory.RoundedBox(new Vector3(0.13f, 0.045f, 0.15f), 0.018f), Mats.Lit(Mats.Hex("#A8443A"), 0.15f), new Vector3(-0.2f, 0.245f, 0.01f));
        blanket.localRotation = Quaternion.Euler(0f, 12f, 0f);
        Parts.Add("BattaniyeSerit", blanket, MeshFactory.RoundedBox(new Vector3(0.132f, 0.047f, 0.025f), 0.01f), Mats.Lit(Mats.Hex("#D8C9A8"), 0.15f), new Vector3(0f, 0f, 0.04f), outline: false);
        // tekerlekler; arka sol tekerlek kumun altinda
        foreach (var (x, z) in new[] { (-0.24f, -0.2f), (-0.24f, 0.2f), (0.24f, -0.2f) })
            Parts.Add("Teker", root, MeshFactory.RoundedCylinder(0.075f, 0.05f, 0.012f, 16), dark, new Vector3(x, 0.07f, z)).localRotation = Quaternion.Euler(90f, 0f, 0f);
        Parts.Add("Kum", root, MeshFactory.RoundedBox(new Vector3(0.5f, 0.16f, 0.36f), 0.07f), Mats.Lit(Sand, 0.12f), new Vector3(0.28f, 0.05f, 0.2f), outline: false);
    }

    // Bolum 40: karsi tepede yere yatmis dev anten. Kafes govde, ucunda kuma donuk canak, sonmus kirmizi isik.
    static Transform Antenna(Transform parent, Vector3 p)
    {
        var root = Parts.Empty("Iz-Anten", parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(0f, 24f, 6f);
        var metal = Mats.Lit(Mats.Hex("#A9ADB5"), 0.45f);
        const float length = 1.5f;
        foreach (float z in new[] { -0.05f, 0.05f })
            Parts.Add("Kiris", root, MeshFactory.RoundedBox(new Vector3(length, 0.035f, 0.035f), 0.01f), metal, new Vector3(0f, 0.04f, z));
        for (float x = -length * 0.5f + 0.1f; x < length * 0.5f; x += 0.18f)
            Parts.Add("Capraz", root, MeshFactory.RoundedBox(new Vector3(0.02f, 0.02f, 0.12f), 0.006f), metal, new Vector3(x, 0.04f, 0f), outline: false)
                .localRotation = Quaternion.Euler(0f, 35f, 0f);
        var dish = Parts.Empty("Canak", root);
        dish.localPosition = new Vector3(length * 0.5f + 0.12f, 0.16f, 0f);
        dish.localRotation = Quaternion.Euler(0f, 0f, -62f);   // yuzu kuma donuk
        Parts.Add("Kase", dish, MeshFactory.RoundedCylinder(0.3f, 0.05f, 0.02f, 24), Mats.Lit(Mats.Hex("#D8D8D2"), 0.35f), Vector3.zero);
        Parts.Add("Boynuz", dish, MeshFactory.RoundedCylinder(0.025f, 0.2f, 0.008f, 10), metal, new Vector3(0f, -0.12f, 0f), outline: false);
        Parts.Add("Isik", root, MeshFactory.Sphere(0.035f, 8, 12), Mats.Lit(Mats.Hex("#5A1E1A"), 0.6f), new Vector3(-length * 0.5f - 0.03f, 0.05f, 0f), outline: false);
        return root;
    }
}

// Bolum 40 kapanisinin nesneleri: yon bulma diregi, son direkteki kurdele, devrik anten.
public class DuneFinale
{
    readonly CompassStand stand;
    readonly WindRibbon ribbon;
    readonly Transform antenna;

    public DuneFinale(CompassStand stand, WindRibbon ribbon, Transform antenna)
    {
        this.stand = stand;
        this.ribbon = ribbon;
        this.antenna = antenna;
    }

    // Bir noktadan bakis yonu (derece; kuzey 0, dogu 90; robotun Yaw'i ile ayni olcu)
    public static float YawTowards(Vector3 from, Vector3 to) => Mathf.Atan2(to.x - from.x, to.z - from.z) * Mathf.Rad2Deg;

    public float YawToAntenna(Vector3 from) => YawTowards(from, antenna.position);
    public Vector3 RibbonPosition => ribbon.transform.position;

    public IEnumerator PointTheWay() => stand.PointTo(YawToAntenna(stand.transform.position));
    // Ucus kurdelenin kendi uzerinde calisir: bolum yeniden denenirse Restore onu da durdurur.
    public Coroutine StartRibbonFlight(System.Func<Vector3> target) => ribbon.StartCoroutine(ribbon.FlyTo(target));

    public void Restore()
    {
        stand.Restore();
        ribbon.Restore();
    }
}
