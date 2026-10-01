using System.Collections;
using UnityEngine;

// Koloni icin calisan is gezgini: alti tekerlek + eklemli bacaklar (rocker-bogie), far cubugu, kamera diregi,
// toplama kolu, gunes paneli, buz haznesi, turuncu tepe lambasi. Ileri yonu +Z.
public class ColonyRover : Robot
{
    const float WheelR = 0.08f;
    const float ArmUpper = 0.12f, ArmFore = 0.11f;
    // kolun duruslari (derece; arti = asagi): toplanmis ve yere uzanmis
    static readonly Vector2 ArmStowed = new Vector2(-55f, 120f);
    static readonly Vector2 ArmReach = new Vector2(35f, 50f);

    Transform head, shoulder, elbow;
    Material beacon, lensGlow;
    Color beaconColor, beaconAmber, beaconGreen, lensColor;
    float nod, headYaw, headYawTarget;
    bool celebrating;

    protected override float WheelRadius => WheelR;
    protected override Color BeamColor => new Color(0.95f, 0.88f, 0.72f);   // sicak beyaz farlar
    protected override float MoveTilt => 2.5f;   // agir is araci: oyuncak kadar sallanmaz
    protected override float ModelScale => 1.55f;  // alcak arac: karede oyuncak robot kadar yer tutsun

    protected override void Build()
    {
        var hull = Mats.Emissive(Mats.Hex("#D8D4CC"), Mats.Hex("#D8D4CC") * 0.07f, 0.3f); // loş sahnede grilesmesin
        var dark = Mats.Lit(Mats.Hex("#34313A"), 0.3f);
        var metal = Mats.Lit(Mats.Hex("#7D7A82"), 0.55f);
        var rubber = Mats.Lit(Mats.Hex("#1E1C22"), 0.15f);
        var safety = Mats.Lit(Mats.Hex("#E39A3B"), 0.4f);
        var solar = Mats.Lit(Mats.Hex("#1F2A40"), 0.85f);
        var lens = Mats.Lit(Mats.Hex("#0E0D12"), 0.9f);
        var lamp = Mats.Unlit(Mats.Hex("#FFF1D6"));
        var ice = Mats.Emissive(Mats.Hex("#6FC9DA"), Mats.Hex("#6FC9DA") * 0.6f, 0.7f);
        beaconAmber = Mats.Hex("#FFA733") * 1.4f;
        beaconGreen = Mats.Hex("#5BF08A") * 1.4f;
        beaconColor = beaconAmber;
        beacon = Mats.Emissive(Mats.Hex("#FFB547"), beaconAmber, 0.6f);
        lensColor = Mats.Hex("#7FE3F0");
        lensGlow = Mats.Emissive(Mats.Hex("#1A3A40"), lensColor * 0.5f, 0.9f);

        BuildWheels(rubber, metal, dark);
        BuildHull(hull, dark, safety, lamp, solar, metal, ice);
        BuildMast(hull, dark, metal, lens);
        BuildArm(metal, dark, safety);
    }

    void BuildWheels(Material rubber, Material metal, Material dark)
    {
        var tire = MeshFactory.RoundedCylinder(WheelR, 0.065f, 0.018f);
        var hub = MeshFactory.RoundedCylinder(0.034f, 0.02f, 0.006f);
        foreach (float sx in new[] { -1f, 1f })
        {
            foreach (float z in new[] { 0.18f, 0f, -0.18f })
            {
                var pivot = Parts.Empty("Wheel", rig);
                pivot.localPosition = new Vector3(sx * 0.25f, WheelR, z);
                Parts.Add("Tire", pivot, tire, rubber, Vector3.zero).localRotation = Quaternion.Euler(0, 0, 90);
                Parts.Add("Hub", pivot, hub, metal, new Vector3(sx * 0.034f, 0, 0), outline: false).localRotation = Quaternion.Euler(0, 0, 90);
                wheels.Add(pivot);
            }
            // eklemli bacaklar: govdeye bagli kol (on tekerlek + arka kol), arka kol orta ve arka tekerlegi tasir
            float x = sx * 0.205f;
            var body = new Vector3(x, 0.20f, 0.03f);
            var bogie = new Vector3(x, 0.135f, -0.09f);
            Bar("Rocker", rig, body, new Vector3(x, WheelR, 0.18f), 0.026f, dark);
            Bar("Rocker", rig, body, bogie, 0.026f, dark);
            Bar("Bogie", rig, bogie, new Vector3(x, WheelR, 0f), 0.022f, dark);
            Bar("Bogie", rig, bogie, new Vector3(x, WheelR, -0.18f), 0.022f, dark);
            Parts.Add("Joint", rig, MeshFactory.RoundedCylinder(0.022f, 0.03f, 0.006f, 16), metal, body, outline: false).localRotation = Quaternion.Euler(0, 0, 90);
            Parts.Add("Joint", rig, MeshFactory.RoundedCylinder(0.018f, 0.03f, 0.006f, 16), metal, bogie, outline: false).localRotation = Quaternion.Euler(0, 0, 90);
        }
    }

    void BuildHull(Material hull, Material dark, Material safety, Material lamp, Material solar, Material metal, Material ice)
    {
        Parts.Add("Belly", rig, MeshFactory.RoundedBox(new Vector3(0.34f, 0.05f, 0.46f), 0.015f), dark, new Vector3(0, 0.175f, 0));
        Parts.Add("Hull", rig, MeshFactory.RoundedBox(new Vector3(0.36f, 0.08f, 0.48f), 0.02f), hull, new Vector3(0, 0.235f, 0));
        // yanlarda is guvenligi seridi, onde tampon
        Parts.Add("Stripe", rig, MeshFactory.RoundedBox(new Vector3(0.372f, 0.018f, 0.30f), 0.006f), safety, new Vector3(0, 0.215f, -0.02f), outline: false);
        Parts.Add("Bumper", rig, MeshFactory.RoundedBox(new Vector3(0.30f, 0.035f, 0.03f), 0.01f), dark, new Vector3(0, 0.19f, 0.25f));

        // far cubugu: zemine dusen isik (Ground.shader) buradan geliyormus gibi
        Parts.Add("LightBar", rig, MeshFactory.RoundedBox(new Vector3(0.26f, 0.035f, 0.035f), 0.01f), dark, new Vector3(0, 0.29f, 0.215f));
        var lampMesh = MeshFactory.RoundedBox(new Vector3(0.06f, 0.022f, 0.01f), 0.005f);
        foreach (float sx in new[] { -1f, 1f })
            Parts.Add("Headlamp", rig, lampMesh, lamp, new Vector3(sx * 0.07f, 0.29f, 0.234f), outline: false, castShadow: false);

        // gunes paneli
        Parts.Add("PanelFrame", rig, MeshFactory.RoundedBox(new Vector3(0.34f, 0.012f, 0.20f), 0.004f), metal, new Vector3(0, 0.281f, -0.02f));
        Parts.Add("Panel", rig, MeshFactory.RoundedBox(new Vector3(0.32f, 0.008f, 0.18f), 0.002f), solar, new Vector3(0, 0.289f, -0.02f), outline: false);

        // buz haznesi (icindeki buz hafifce parlar)
        Parts.Add("Hopper", rig, MeshFactory.RoundedBox(new Vector3(0.26f, 0.06f, 0.10f), 0.012f), metal, new Vector3(0, 0.305f, -0.19f));
        Parts.Add("Ice", rig, MeshFactory.RoundedBox(new Vector3(0.22f, 0.01f, 0.075f), 0.004f), ice, new Vector3(0, 0.334f, -0.19f), outline: false, castShadow: false);

        // turuncu tepe lambasi + anten
        Parts.Add("BeaconPost", rig, MeshFactory.RoundedCylinder(0.01f, 0.03f, 0.003f, 12), dark, new Vector3(0.105f, 0.35f, -0.215f));
        Parts.Add("Beacon", rig, MeshFactory.Sphere(0.02f, 8, 14), beacon, new Vector3(0.105f, 0.372f, -0.215f), outline: false, castShadow: false);
        Parts.Add("Whip", rig, MeshFactory.RoundedCylinder(0.005f, 0.17f, 0.002f, 8), dark, new Vector3(-0.115f, 0.42f, -0.215f), outline: false);
    }

    void BuildMast(Material hull, Material dark, Material metal, Material lens)
    {
        var basePos = new Vector3(-0.11f, 0.285f, 0.13f);
        Parts.Add("MastBase", rig, MeshFactory.RoundedCylinder(0.028f, 0.03f, 0.008f, 16), dark, basePos);
        Parts.Add("Mast", rig, MeshFactory.RoundedCylinder(0.013f, 0.20f, 0.004f, 12), metal, basePos + new Vector3(0, 0.11f, 0));

        head = Parts.Empty("Head", rig);
        head.localPosition = basePos + new Vector3(0, 0.215f, 0);
        Parts.Add("Camera", head, MeshFactory.RoundedBox(new Vector3(0.14f, 0.06f, 0.07f), 0.014f), hull, new Vector3(0, 0.03f, 0));
        Parts.Add("CameraBand", head, MeshFactory.RoundedBox(new Vector3(0.146f, 0.012f, 0.076f), 0.004f), dark, new Vector3(0, 0.012f, 0), outline: false);
        var lensMesh = MeshFactory.RoundedCylinder(0.016f, 0.02f, 0.004f, 16);
        var glintMesh = MeshFactory.Sphere(0.0075f, 6, 10);
        foreach (float sx in new[] { -1f, 1f })
        {
            Parts.Add("Lens", head, lensMesh, lens, new Vector3(sx * 0.034f, 0.032f, 0.037f), outline: false).localRotation = Quaternion.Euler(90, 0, 0);
            Parts.Add("LensGlow", head, glintMesh, lensGlow, new Vector3(sx * 0.034f, 0.032f, 0.045f), outline: false, castShadow: false);
        }
    }

    void BuildArm(Material metal, Material dark, Material safety)
    {
        shoulder = Parts.Empty("Shoulder", rig);
        shoulder.localPosition = new Vector3(0.11f, 0.28f, 0.20f);
        Parts.Add("ShoulderJoint", shoulder, MeshFactory.RoundedCylinder(0.02f, 0.035f, 0.006f, 16), dark, Vector3.zero, outline: false).localRotation = Quaternion.Euler(0, 0, 90);
        Bar("UpperArm", shoulder, Vector3.zero, new Vector3(0, 0, ArmUpper), 0.02f, metal);

        elbow = Parts.Empty("Elbow", shoulder);
        elbow.localPosition = new Vector3(0, 0, ArmUpper);
        Parts.Add("ElbowJoint", elbow, MeshFactory.RoundedCylinder(0.016f, 0.03f, 0.005f, 16), dark, Vector3.zero, outline: false).localRotation = Quaternion.Euler(0, 0, 90);
        Bar("Forearm", elbow, Vector3.zero, new Vector3(0, 0, ArmFore), 0.017f, metal);
        Parts.Add("Scoop", elbow, MeshFactory.RoundedBox(new Vector3(0.05f, 0.03f, 0.035f), 0.008f), dark, new Vector3(0, 0, ArmFore + 0.012f));
        Parts.Add("ScoopLip", elbow, MeshFactory.RoundedBox(new Vector3(0.054f, 0.008f, 0.012f), 0.003f), safety, new Vector3(0, -0.012f, ArmFore + 0.03f), outline: false);
        SetArm(ArmStowed);
    }

    // a'dan b'ye uzanan cubuk
    static void Bar(string name, Transform parent, Vector3 a, Vector3 b, float thick, Material mat)
    {
        var t = Parts.Add(name, parent, MeshFactory.RoundedBox(new Vector3(thick, thick, (b - a).magnitude), thick * 0.3f), mat, (a + b) * 0.5f);
        t.localRotation = Quaternion.LookRotation(b - a);
    }

    void SetArm(Vector2 pose)
    {
        shoulder.localRotation = Quaternion.Euler(pose.x, 0, 0);
        elbow.localRotation = Quaternion.Euler(pose.y, 0, 0);
    }

    protected override void OnReset()
    {
        nod = 0f; headYaw = headYawTarget = 0f;
        head.localRotation = Quaternion.identity;
        celebrating = false;
        beaconColor = beaconAmber;
        SetArm(ArmStowed);
    }

    protected override void Animate(float t)
    {
        // tepe lambasi doner gibi kisa kisa parlar; kamera mercegi yavasca nefes alir
        float flash = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(t * 6.5f), 6f);
        beacon.SetColor("_EmissionColor", beaconColor * (0.25f + 0.9f * flash));
        lensGlow.SetColor("_EmissionColor", lensColor * (0.4f + 0.15f * Mathf.Sin(t * 1.7f)));

        headYaw = Mathf.MoveTowards(headYaw, headYawTarget, Time.deltaTime * 120f);
        // bos dururken kamera etrafi yavasca tarar
        float scan = celebrating ? 0f : 4f * Mathf.Sin(t * 0.5f);
        head.localRotation = Quaternion.Euler(nod, headYaw + scan, 0f);
    }

    // Kolu one-asagi uzatip buzu alir, sonra toplar.
    public override IEnumerator Collect()
    {
        yield return Tween.Run(0.45f, t => SetArm(Vector2.Lerp(ArmStowed, ArmReach, Mathf.Sin(t * Mathf.PI))));
        SetArm(ArmStowed);
    }

    public override void GlanceAtCamera(bool on) => headYawTarget = on ? 32f : 0f;

    // Kameraya doner, tepe lambasi yesile doner, kamerasiyla iki kez "evet" der, kolunu kaldirir.
    public override IEnumerator Celebrate()
    {
        GlanceAtCamera(false);
        yield return TurnTo(180f, 0.4f);
        celebrating = true;
        beaconColor = beaconGreen;
        var raised = new Vector2(-80f, 40f);
        yield return Tween.Run(0.3f, t => SetArm(Vector2.Lerp(ArmStowed, raised, Tween.InOutCubic(t))));
        for (int i = 0; i < 2; i++)
            yield return Tween.Run(0.32f, t => nod = 14f * Mathf.Sin(t * Mathf.PI));
        nod = 0f;
    }
}
