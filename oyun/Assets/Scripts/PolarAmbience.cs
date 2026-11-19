using System;
using UnityEngine;

// Bolge 2 (kutup buzulu) canli cevresi: hafif kar yagar, ruzgar dalgasi gelince yerde kar savrulur, buzun ustunde parilti
// yanip soner. Kutup ekibinin birakip gittigi hava diregi (ruzgar olcer doner, ruzgar gulu doner, flama dalgalanir, tepe
// lambasi yanip soner) ve alanin cevresinde buz kuleleri. Gokteki kutup isigi PolarSky.hlsl'de. Hepsi alanin disinda ya da
// havada; kareler net okunur.
public sealed class PolarAmbience : RegionAmbience
{
    static readonly Color Flake = new Color(0.94f, 0.97f, 1f, 0.85f);
    static readonly Color DriftColor = new Color(0.86f, 0.91f, 0.97f, 0.17f);
    static readonly Color GlintA = Mats.Hex("#E8FBFF"), GlintB = Mats.Hex("#8FE3FF");

    public PolarAmbience(AmbienceSpace space) : base(space)
    {
        Add(new Snowfall(space, 23, new Vector2(-7f, 7f), new Vector2(-5f, 9f), 3.2f, 42f, 0.42f, Flake, new Vector2(0.035f, 0.065f)));
        Add(new GroundDrift(space, 24, DriftColor, 2.5f, 11f));

        // hava diregi: sol arka kosenin yaninda; Bolum 20'de uzaklasan aracin yolunun onunde
        var mast = WeatherMast(space.Root);
        mast.localPosition = new Vector3(-space.AreaHalf.x - 0.4f, 0f, space.AreaHalf.y + 0.6f);
        mast.localRotation = Quaternion.Euler(0f, 18f, 0f);
        GroundPlacement.SitOnGround(mast, space.Height, 0.03f);

        var spires = new[]
        {
            new Vector3(space.AreaHalf.x + 0.65f, 0f, space.AreaHalf.y + 1.0f),   // sag arka kosenin yaninda
            new Vector3(-space.AreaHalf.x - 0.9f, 0f, space.AreaHalf.y + 4.0f),
            new Vector3(space.AreaHalf.x + 2.4f, 0f, space.AreaHalf.y + 5.6f),
            new Vector3(-space.AreaHalf.x - 4.2f, 0f, -0.5f),
            new Vector3(space.AreaHalf.x + 4.0f, 0f, -1.2f),
        };
        for (int i = 0; i < spires.Length; i++)
            IceSpires(space.Root, spires[i], i, far: spires[i].z > 6f);

        // parilti: cevredeki buzun ve kulelerin ustunde
        var onIce = AroundArea(space, new Vector2(-7f, 7f), new Vector2(-4.5f, 8.5f), 0.03f);
        Func<Sprinkler, Vector3> where = rnd =>
        {
            if (!rnd.Chance(0.35f)) return onIce(rnd);
            var s = spires[(int)rnd.Range(0f, spires.Length - 0.001f)];
            return space.OnGround(s.x + rnd.Range(-0.35f, 0.35f), s.z + rnd.Range(-0.3f, 0.3f), rnd.Range(0.1f, 1.1f));
        };
        Add(new Glints(space, 25, where, 14f, GlintA, GlintB, new Vector2(0.07f, 0.13f)));
    }

    // Kutup ekibinin hava diregi: karda yari gomulu ayak, beyaz gozlem kutusu (turuncu serit), kucuk gunes paneli,
    // tepede uc kepceli ruzgar olcer + ruzgar gulu + kirmizi lamba; ortada turuncu flama.
    Transform WeatherMast(Transform parent)
    {
        var root = Parts.Empty("HavaDiregi", parent);
        var metal = Mats.Lit(Mats.Hex("#5C6372"), 0.5f);
        var white = Mats.Lit(Mats.Hex("#E9EEF4"), 0.4f);
        var orange = Mats.Lit(Mats.Hex("#FF7A3D"), 0.3f);

        Parts.Add("KarYigini", root, MeshFactory.RoundedBox(new Vector3(0.42f, 0.12f, 0.36f), 0.06f), Mats.Lit(Mats.Hex("#D7E1EF"), 0.25f),
            new Vector3(0f, 0.03f, 0f), outline: false);
        Parts.Add("Direk", root, MeshFactory.RoundedCylinder(0.024f, 1.12f, 0.008f, 10), metal, new Vector3(0f, 0.56f, 0f));
        Parts.Add("Kutu", root, MeshFactory.RoundedBox(new Vector3(0.2f, 0.16f, 0.12f), 0.02f), white, new Vector3(0f, 0.42f, 0.07f));
        Parts.Add("Serit", root, MeshFactory.RoundedBox(new Vector3(0.205f, 0.03f, 0.125f), 0.008f), orange, new Vector3(0f, 0.46f, 0.07f), outline: false);
        Parts.Add("Panel", root, MeshFactory.RoundedBox(new Vector3(0.22f, 0.012f, 0.15f), 0.004f), Mats.Lit(Mats.Hex("#1E2A44"), 0.8f),
            new Vector3(0f, 0.58f, -0.08f), outline: false).localRotation = Quaternion.Euler(-35f, 0f, 0f);

        // ruzgar olcer: dikey milde uc kol, uclarinda yarim kepceler
        var cups = Parts.Empty("RuzgarOlcer", root);
        cups.localPosition = new Vector3(0f, 1.16f, 0f);
        Parts.Add("Mil", cups, MeshFactory.RoundedCylinder(0.02f, 0.05f, 0.006f, 10), metal, Vector3.zero, outline: false);
        var cupMesh = MeshFactory.Sphere(0.04f, 6, 10);
        for (int k = 0; k < 3; k++)
        {
            var arm = Parts.Empty("Kol", cups);
            arm.localRotation = Quaternion.Euler(0f, k * 120f, 0f);
            Parts.Add("Cubuk", arm, MeshFactory.RoundedBox(new Vector3(0.15f, 0.01f, 0.01f), 0.003f, 2), metal, new Vector3(0.075f, 0.01f, 0f), outline: false);
            Parts.Add("Kepce", arm, cupMesh, white, new Vector3(0.15f, 0.01f, 0.025f), outline: false).localScale = new Vector3(1f, 0.8f, 0.55f);
        }
        Add(new Spinner(cups, Vector3.up, 140f, 520f));

        // ruzgar gulu: kisa kolun ucunda ok ve kuyruk; ruzgarin yonunu arar gibi hafifce saga sola doner
        var vane = Parts.Empty("RuzgarGulu", root);
        vane.localPosition = new Vector3(0f, 0.95f, 0f);
        Parts.Add("Ok", vane, MeshFactory.RoundedBox(new Vector3(0.26f, 0.012f, 0.012f), 0.004f, 2), metal, new Vector3(0.02f, 0f, 0f), outline: false);
        Parts.Add("Kuyruk", vane, MeshFactory.RoundedBox(new Vector3(0.07f, 0.07f, 0.006f), 0.003f, 2), orange, new Vector3(-0.1f, 0.01f, 0f), outline: false);
        Parts.Add("Uc", vane, MeshFactory.RoundedBox(new Vector3(0.035f, 0.035f, 0.012f), 0.004f, 2), metal, new Vector3(0.15f, 0f, 0f), outline: false)
            .localRotation = Quaternion.Euler(0f, 0f, 45f);
        Add(new Seeker(vane));

        Add(new BlinkLamp(root, new Vector3(0f, 1.24f, 0f), 0.028f, Mats.Hex("#FF4A3A"), 2.4f, 0f));
        Add(Pennant.Create(root, new Vector3(0.02f, 0.82f, 0f), new Vector2(0.3f, 0.11f), orange, 4, 0.7f));
        return root;
    }

    // Ruzgar gulu: ruzgarin yonunu arar gibi yavasca saga sola doner
    sealed class Seeker : IAmbientPiece
    {
        readonly Transform vane;
        public Seeker(Transform vane) { this.vane = vane; }

        public void Tick(float time, float dt, float density, float gust) =>
            vane.localRotation = Quaternion.Euler(0f, 90f + 14f * Mathf.Sin(time * 0.7f) + 6f * Mathf.Sin(time * 2.3f), 0f);
    }

    // Buz kuleleri: karin icinden cikan uzun, soluk mavi kristaller (toplanan parlak buzlardan cok daha iri ve isiksiz:
    // karistirilmaz). Uzaktakiler arka plana karisan malzemeyle cizilir.
    void IceSpires(Transform parent, Vector3 at, int seed, bool far)
    {
        var root = Parts.Empty("BuzKulesi", parent);
        root.localPosition = at;
        root.localRotation = Quaternion.Euler(0f, seed * 53f, 0f);
        var mat = far ? Space.FarMat("#B9CDE2") : Mats.Lit(Mats.Hex("#C7DCEF"), 0.85f);
        var shade = far ? Space.FarMat("#93AECB") : Mats.Lit(Mats.Hex("#9DB9D8"), 0.85f);
        var rng = new System.Random(300 + seed);
        float R(float a, float b) => a + (float)rng.NextDouble() * (b - a);
        int count = 3 + seed % 3;
        for (int k = 0; k < count; k++)
        {
            float h = k == 0 ? R(0.85f, 1.15f) : R(0.35f, 0.75f);
            var c = Parts.Add("Kristal", root, MeshFactory.Crystal(R(0.1f, 0.17f), h, R(0.18f, 0.3f)), k % 2 == 0 ? mat : shade,
                k == 0 ? Vector3.zero : new Vector3(R(-0.32f, 0.32f), 0f, R(-0.28f, 0.28f)), outline: !far);
            c.localRotation = Quaternion.Euler(R(-14f, 14f), R(0f, 60f), R(-16f, 16f));
        }
        Parts.Add("KarTaban", root, MeshFactory.RoundedBox(new Vector3(0.7f, 0.12f, 0.55f), 0.06f), far ? Space.FarMat("#D2DDEB") : Mats.Lit(Mats.Hex("#D7E1EF"), 0.25f),
            new Vector3(0f, 0.02f, 0f), outline: false);
        GroundPlacement.SitOnGround(root, Space.Height, 0.06f);
    }
}
