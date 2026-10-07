using System.Collections;
using UnityEngine;

// Kivilcim: koloninin kucuk bakim robotu (hikaye: docs/tasarim/hikaye.md). Yari insansi: govde, isik yuzlu kafa,
// iki kol; bacak yok, yerden bir karis yukarida suzulur (alcak oldugu icin kaya ve buz blogunun ustunden gecemez).
// Konusmaz; gozlerinin rengi/bicimi, kollari ve antenindeki isikla anlasir. Gogsunde silik bir cocuk cizimi var.
// Gorunus bilerek "isci": koseli, turuncu guvenlik seritli, sirtinda pil cantasi (bilinen robotlara benzemesin).
// Ileri yonu +Z.
public class SparkBot : Robot
{
    const float HoverY = 0.07f;   // govdenin altinin yerden yuksekligi
    // basi hafifce yukari kalkik: kamera yukaridan baktigi icin yuzu (gozleri) gorunsun, oyuncuya bakiyormus gibi
    const float HeadUp = -16f;

    // Kol duruslari: (one kalkma, yana acilma, dirsek bukumu) derece
    static readonly Vector3 ArmRest = new Vector3(8f, 12f, 20f);
    static readonly Vector3 ArmReach = new Vector3(70f, 6f, 10f);
    static readonly Vector3 ArmRaised = new Vector3(150f, 35f, 10f);
    static readonly Vector3 ArmShrug = new Vector3(15f, 40f, 105f);

    static readonly Color EyeCalm = Mats.Hex("#FFE3A0") * 1.6f;     // sicak kivilcim rengi
    static readonly Color EyeHappy = Mats.Hex("#6CF59A") * 1.6f;
    static readonly Color EyeUnsure = Mats.Hex("#FFA040") * 1.6f;

    Transform body, head, eyeL, eyeR, ribbon;
    readonly Transform[] ribbonTails = new Transform[2];
    readonly Transform[] shoulders = new Transform[2], elbows = new Transform[2];
    Material eyeMat, thrusterMat, beaconMat;
    Color eyeColor;
    float headYaw, headYawTarget, nod, tilt, lean, lift, beaconBoost;
    bool posing;   // kutlama/omuz silkme: bos durus hareketleri durur

    protected override float WheelRadius => 1f;   // tekerlegi yok
    protected override Color BeamColor => new Color(0.95f, 0.88f, 0.72f);
    protected override float MoveTilt => 7f;   // suzulurken gidise dogru egilir
    protected override float ModelScale => 2.1f;  // ince, dik govde: karede eski gezgin kadar yer tutsun

    protected override void Build()
    {
        var hull = Mats.Emissive(Mats.Hex("#D8D4CC"), Mats.Hex("#D8D4CC") * 0.07f, 0.3f);   // los sahnede grilesmesin
        var dark = Mats.Lit(Mats.Hex("#34313A"), 0.3f);
        var metal = Mats.Lit(Mats.Hex("#7D7A82"), 0.55f);
        var safety = Mats.Lit(Mats.Hex("#E39A3B"), 0.4f);
        var visor = Mats.Lit(Mats.Hex("#121117"), 0.9f);
        eyeColor = EyeCalm;
        eyeMat = Mats.Emissive(Mats.Hex("#FFF4D8"), eyeColor, 0.6f);
        thrusterMat = Mats.Emissive(Mats.Hex("#BFEFFF"), Mats.Hex("#7FD8FF") * 1.5f, 0.5f);
        beaconMat = Mats.Emissive(Mats.Hex("#FFB547"), Mats.Hex("#FFA733"), 0.6f);

        body = Parts.Empty("Body", rig);
        BuildTorso(hull, dark, metal, safety);
        BuildHead(hull, dark, visor);
        for (int side = 0; side < 2; side++) BuildArm(side, metal, dark, safety);
        SetArms(ArmRest);
    }

    void BuildTorso(Material hull, Material dark, Material metal, Material safety)
    {
        // itici: govdenin altinda koyu halka, altinda mavi-beyaz isik
        Parts.Add("Thruster", body, MeshFactory.RoundedCylinder(0.085f, 0.035f, 0.012f), dark, new Vector3(0, HoverY + 0.02f, 0));
        Parts.Add("ThrusterGlow", body, MeshFactory.RoundedCylinder(0.06f, 0.01f, 0.004f), thrusterMat, new Vector3(0, HoverY, 0), outline: false, castShadow: false);

        Parts.Add("Torso", body, MeshFactory.RoundedBox(new Vector3(0.24f, 0.2f, 0.17f), 0.045f), hull, new Vector3(0, 0.21f, 0));
        Parts.Add("Waist", body, MeshFactory.RoundedBox(new Vector3(0.246f, 0.025f, 0.176f), 0.01f), safety, new Vector3(0, 0.135f, 0), outline: false);

        // gogus plakasi + silik cocuk cizimi
        var plate = Mats.Hex("#C9C4BA");
        var drawing = Mats.Emissive(Color.white, Color.white * 0.07f, 0.2f);
        var tex = ChildDrawing.Create(plate);
        drawing.SetTexture("_BaseMap", tex);
        drawing.SetTexture("_EmissionMap", tex);
        var chest = Parts.Add("Drawing", body, MeshFactory.Quad(new Vector2(0.16f, 0.12f)), drawing, new Vector3(0, 0.215f, 0.0855f), outline: false, castShadow: false);
        // dokunun ustu yukari, sol yani robota bakanin solu
        chest.localRotation = Quaternion.AngleAxis(180f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.right);

        // sirtta pil cantasi: turuncu kapak, uc yesil sarj isigi (robotun arkasi hep belli olsun)
        Parts.Add("Pack", body, MeshFactory.RoundedBox(new Vector3(0.19f, 0.15f, 0.07f), 0.02f), metal, new Vector3(0, 0.22f, -0.115f));
        Parts.Add("PackLid", body, MeshFactory.RoundedBox(new Vector3(0.17f, 0.02f, 0.06f), 0.008f), safety, new Vector3(0, 0.3f, -0.115f), outline: false);
        var led = Mats.Emissive(Mats.Hex("#6CF59A"), Mats.Hex("#6CF59A") * 1.2f, 0.5f);
        for (int k = -1; k <= 1; k++)
            Parts.Add("Led", body, MeshFactory.RoundedBox(new Vector3(0.025f, 0.012f, 0.008f), 0.003f), led, new Vector3(k * 0.04f, 0.25f, -0.152f), outline: false, castShadow: false);

        BuildRibbon();

        Parts.Add("Neck", body, MeshFactory.RoundedCylinder(0.03f, 0.04f, 0.01f, 16), dark, new Vector3(0, 0.325f, 0));
    }

    // Ece'nin kurdelesi (Bolum 40'tan sonra): pil cantasinin ustunde dugum, arkaya sarkan iki uc. Baslangicta gizli.
    static readonly Vector3 RibbonKnot = new Vector3(0.06f, 0.305f, -0.15f);

    void BuildRibbon()
    {
        var cloth = Mats.Lit(CraterTraces.EceColor, 0.25f);
        ribbon = Parts.Empty("Ribbon", body);
        ribbon.localPosition = RibbonKnot;
        Parts.Add("Knot", ribbon, MeshFactory.RoundedBox(new Vector3(0.045f, 0.035f, 0.03f), 0.012f), cloth, Vector3.zero, outline: false, castShadow: false);
        for (int k = 0; k < 2; k++)
        {
            var tail = Parts.Empty("Tail", ribbon);
            tail.localPosition = new Vector3(k == 0 ? -0.012f : 0.012f, 0f, -0.008f);
            float length = k == 0 ? 0.13f : 0.11f;
            Parts.Add("Cloth", tail, MeshFactory.RoundedBox(new Vector3(0.024f, length, 0.005f), 0.002f), cloth, new Vector3(0f, -length * 0.5f, 0f), outline: false, castShadow: false);
            ribbonTails[k] = tail;
        }
        ribbon.gameObject.SetActive(false);
    }

    public override void SetRibbon(bool on) => ribbon.gameObject.SetActive(on);
    public override Vector3 RibbonPoint => transform.TransformPoint(RibbonKnot + Vector3.up * 0.03f);

    void BuildHead(Material hull, Material dark, Material visor)
    {
        head = Parts.Empty("Head", body);
        head.localPosition = new Vector3(0, 0.34f, 0);
        Parts.Add("Skull", head, MeshFactory.RoundedBox(new Vector3(0.2f, 0.12f, 0.15f), 0.04f), hull, new Vector3(0, 0.06f, 0));
        Parts.Add("Visor", head, MeshFactory.RoundedBox(new Vector3(0.16f, 0.07f, 0.012f), 0.012f), visor, new Vector3(0, 0.06f, 0.074f), outline: false);
        var eyeMesh = MeshFactory.RoundedBox(new Vector3(0.044f, 0.03f, 0.008f), 0.01f);
        eyeL = Parts.Add("Eye", head, eyeMesh, eyeMat, new Vector3(-0.034f, 0.064f, 0.081f), outline: false, castShadow: false);
        eyeR = Parts.Add("Eye", head, eyeMesh, eyeMat, new Vector3(0.034f, 0.064f, 0.081f), outline: false, castShadow: false);
        // yanlarda kulak gibi iki koyu baglanti, ustte anten ve turuncu isik
        foreach (float sx in new[] { -1f, 1f })
            Parts.Add("Ear", head, MeshFactory.RoundedCylinder(0.026f, 0.03f, 0.008f, 16), dark, new Vector3(sx * 0.105f, 0.06f, 0), outline: false)
                .localRotation = Quaternion.Euler(0, 0, 90);
        Parts.Add("Antenna", head, MeshFactory.RoundedCylinder(0.006f, 0.09f, 0.002f, 8), dark, new Vector3(0.05f, 0.16f, -0.03f), outline: false);
        Parts.Add("Beacon", head, MeshFactory.Sphere(0.016f, 8, 12), beaconMat, new Vector3(0.05f, 0.21f, -0.03f), outline: false, castShadow: false);
    }

    void BuildArm(int side, Material metal, Material dark, Material safety)
    {
        float sx = side == 0 ? -1f : 1f;
        var shoulder = Parts.Empty("Shoulder", body);
        shoulder.localPosition = new Vector3(sx * 0.14f, 0.28f, 0f);
        Parts.Add("ShoulderJoint", shoulder, MeshFactory.Sphere(0.032f, 8, 12), dark, Vector3.zero);
        Parts.Add("UpperArm", shoulder, MeshFactory.RoundedCylinder(0.022f, 0.09f, 0.01f, 12), metal, new Vector3(0, -0.05f, 0));
        var elbow = Parts.Empty("Elbow", shoulder);
        elbow.localPosition = new Vector3(0, -0.095f, 0);
        Parts.Add("Forearm", elbow, MeshFactory.RoundedCylinder(0.021f, 0.075f, 0.01f, 12), metal, new Vector3(0, -0.04f, 0));
        Parts.Add("Cuff", elbow, MeshFactory.RoundedCylinder(0.024f, 0.015f, 0.005f, 12), safety, new Vector3(0, -0.075f, 0), outline: false);
        Parts.Add("Hand", elbow, MeshFactory.RoundedBox(new Vector3(0.042f, 0.048f, 0.034f), 0.014f), dark, new Vector3(0, -0.105f, 0));
        shoulders[side] = shoulder;
        elbows[side] = elbow;
    }

    // pose: (one kalkma, yana acilma, dirsek bukumu); iki kol ayna gibi
    void SetArms(Vector3 pose)
    {
        for (int side = 0; side < 2; side++)
        {
            float sx = side == 0 ? -1f : 1f;
            shoulders[side].localRotation = Quaternion.Euler(-pose.x, 0, sx * pose.y);
            elbows[side].localRotation = Quaternion.Euler(-pose.z, 0, 0);
        }
    }

    IEnumerator ArmsTo(Vector3 from, Vector3 to, float dur)
        => Tween.Run(dur, t => SetArms(Vector3.Lerp(from, to, Tween.InOutCubic(t))));

    void SetEyes(Color color, float squint)
    {
        eyeColor = color;
        var s = new Vector3(1f, Mathf.Lerp(1f, 0.35f, squint), 1f);
        eyeL.localScale = s;
        eyeR.localScale = s;
    }

    protected override void OnReset()
    {
        posing = false;
        headYaw = headYawTarget = nod = tilt = lean = lift = beaconBoost = 0f;
        SetEyes(EyeCalm, 0f);
        SetArms(ArmRest);
    }

    protected override void Animate(float t)
    {
        // suzulme: yavasca inip kalkar; itici isigi onunla nefes alir
        float bob = 0.018f * Mathf.Sin(t * 2.2f);
        body.localPosition = new Vector3(0, bob + lift, 0);
        body.localRotation = Quaternion.Euler(lean, 0, 0);
        thrusterMat.SetColor("_EmissionColor", Mats.Hex("#7FD8FF") * (1.2f + 0.5f * Mathf.Sin(t * 2.2f + 1.5f)));

        float blink = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(t * 4f), 8f);
        beaconMat.SetColor("_EmissionColor", Mats.Hex("#FFA733") * (0.3f + 1.2f * Mathf.Max(blink, beaconBoost)));

        // kurdelenin uclari suzulmeyle hafifce savrulur (biri digerinden biraz geride)
        for (int k = 0; k < 2; k++)
            ribbonTails[k].localRotation = Quaternion.Euler(25f + 10f * Mathf.Sin(t * 3.1f + k), 0f, (k == 0 ? 18f : -14f) + 8f * Mathf.Sin(t * 2.3f + k * 1.7f));

        // gozler arada bir kirpar
        float wink = Mathf.Repeat(t, 4.3f) < 0.12f && !posing ? 0.15f : 1f;
        eyeMat.SetColor("_EmissionColor", eyeColor * wink);

        headYaw = Mathf.MoveTowards(headYaw, headYawTarget, Time.deltaTime * 120f);
        float look = posing ? 0f : 6f * Mathf.Sin(t * 0.45f);
        head.localRotation = Quaternion.Euler(HeadUp + nod, headYaw + look, tilt);

        // bos dururken kollar suzulmeyle hafifce sallanir
        if (!posing) SetArms(ArmRest + new Vector3(4f * Mathf.Sin(t * 2.2f - 0.8f), 0f, 6f * Mathf.Sin(t * 2.2f - 1.2f)));
    }

    // One egilip iki eliyle yerden alir, gozleri bir an parlar.
    public override IEnumerator Collect()
    {
        posing = true;
        yield return Tween.Run(0.22f, t =>
        {
            float e = Tween.InOutCubic(t);
            lean = 22f * e;
            SetArms(Vector3.Lerp(ArmRest, ArmReach, e));
        });
        eyeMat.SetColor("_EmissionColor", Color.white * 2f);
        yield return Tween.Run(0.22f, t =>
        {
            float e = Tween.InOutCubic(t);
            lean = 22f * (1f - e);
            SetArms(Vector3.Lerp(ArmReach, ArmRest, e));
        });
        lean = 0f;
        posing = false;
    }

    public override void GlanceAtCamera(bool on) => headYawTarget = on ? 32f : 0f;

    // Kameraya (oyuncuya) doner, gozleri yesil gulumser, kollarini kaldirip iki kez zipar, anten isigini iki kez yakar.
    public override IEnumerator Celebrate()
    {
        GlanceAtCamera(false);
        yield return TurnTo(180f, 0.4f);
        posing = true;
        SetEyes(EyeHappy, 0.6f);
        yield return ArmsTo(ArmRest, ArmRaised, 0.3f);
        for (int i = 0; i < 2; i++)
            yield return Tween.Run(0.34f, t =>
            {
                float s = Mathf.Sin(t * Mathf.PI);
                lift = 0.07f * s;
                beaconBoost = s;
            });
        lift = 0f;
        beaconBoost = 0f;
    }

    // Kod hatayla durdu ya da gorev bitmedi: basini yana egip omuz silker ("bu nasil olacak?")
    protected override IEnumerator Puzzled()
    {
        posing = true;
        SetEyes(EyeUnsure, 0.3f);
        yield return Tween.Run(0.3f, t =>
        {
            float e = Tween.InOutCubic(t);
            tilt = 16f * e;
            SetArms(Vector3.Lerp(ArmRest, ArmShrug, e));
        });
        yield return Tween.Wait(0.6f);
        yield return Tween.Run(0.35f, t =>
        {
            float e = Tween.InOutCubic(t);
            tilt = 16f * (1f - e);
            SetArms(Vector3.Lerp(ArmShrug, ArmRest, e));
        });
        tilt = 0f;
        SetEyes(EyeCalm, 0f);
        posing = false;
    }
}
