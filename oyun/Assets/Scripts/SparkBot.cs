using System;
using System.Collections;
using System.Collections.Generic;
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
    static readonly Vector3 ArmPickup = new Vector3(55f, 4f, 8f);           // egilince eli yere degsin (govde PickupLean kadar one egik)
    static readonly Vector3 ArmOverShoulder = new Vector3(165f, 10f, 70f);  // sag kol omzun ustunden sirta (goruntude ayarlandi)
    static readonly Vector3 ArmHoldNote = new Vector3(62f, 14f, 78f);       // sag el gogsun onunde, kagit yuze donuk (Bolge 8 notu)
    const float NoteNod = 14f;                                              // notu okurken basi kagida egilir
    // 76 katlanmis cizim (goruntude ayarlandi): kagit gogsun onunde (govdenin yerelinde), ustu basa donuk; iki el yan kenarlarindan tutar
    // (kollar hedefe uzanir: ReachArm). Kollar kisa: iki el ancak gogsun onunde bulusur.
    static readonly Vector3 HeldPaperAt = new Vector3(0f, 0.29f, 0.17f);
    static readonly Quaternion HeldPaperTurn = Quaternion.Euler(-25f, 0f, 0f);
    const float HeldPaperSize = 1f;     // elde dunya olcegi (yerdekiyle ayni)
    const float GripInset = 0.8f;       // eller kagidin yan kenarlarinda (yarim genisligin 0,8'i)
    const float PaperNod = 32f;         // basi kagida egilir (yuz kagida bakar)
    const float PickupLean = 34f;

    // Bolum 70: avuc duvara, kalemin ucu duvardaki cizgiye. Kollar kisa (iki el ancak gogsun onunde bulusur): duvardaki avuc govdenin
    // tam onunde, kollar hedefe gore kurulur (ReachArm). Olculer govdenin biriminde.
    const float UpperArm = 0.095f, PalmFromElbow = 0.129f;   // omuz-dirsek, dirsek-avuc (elin ucu)
    const float PalmForward = 0.14f;                          // duvardaki avuc omuz hizasinin bu kadar onunde
    static readonly Vector3 HeldCrayonAt = new Vector3(0f, -0.06f, 0f);   // sag eldeki kalemin ortasi (yumruktan one cikar)
    const float HeldCrayonLength = 0.105f, CrayonTipFromElbow = 0.105f + 0.06f + 0.044f;

    static readonly Color EyeCalm = Mats.Hex("#FFE3A0") * 1.6f;     // sicak kivilcim rengi
    static readonly Color EyeHappy = Mats.Hex("#6CF59A") * 1.6f;
    static readonly Color EyeUnsure = Mats.Hex("#FFA040") * 1.6f;
    static readonly Color EyeEce = CraterTraces.EceColor;            // 70 sonu: Ece'nin turkuazi (1,6 kat parlaklikta beyaza doyuyordu)

    Transform body, head, eyeL, eyeR, ribbon, crayon, heldCrayon, heldNote, foldedDrawing, backPaper;
    Transform carriedHome;   // elde tasinan nesnenin (68 kalem, 76 cizim) yerdeki ebeveyni
    Transform heldPaper;     // 76: elde tutulan katlanmis cizim (TryUnfold - StowOnBack arasi)
    bool gripPaper;          // iki el heldPaper'in kenarlarina uzanir (Animate her karede hedefi kagittan alir)
    float openAmount;        // elde tutulan kagidin ne kadar acik oldugu
    readonly Vector3[] armPose = { ArmRest, ArmRest };   // her kolun su anki durusu (SetArm)
    readonly Transform[] ribbonTails = new Transform[2];
    readonly Transform[] shoulders = new Transform[2], elbows = new Transform[2], hands = new Transform[2];
    // Hedefe uzanan kollar (Bolum 70): Animate her karede kurar, suzulurken govde inip kalksa da el duvarda yerinde kalir
    readonly Vector3[] reachAt = new Vector3[2];                    // hedef (dunya konumu)
    readonly float[] reachWeight = new float[2];                    // 0 reachFrom durusu, 1 tam hedefte
    readonly Vector3[] reachFrom = { ArmRest, ArmRest };
    Material eyeMat, thrusterMat, beaconMat;
    Color eyeColor;
    Vector3 arms = ArmRest;   // kollarin su anki durusu (SetArms)
    float headYaw, headYawTarget, nod, tilt, lean, lift, beaconBoost, beaconDim;
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
        var chest = Parts.Add("Drawing", body, MeshFactory.Quad(new Vector2(0.16f, 0.12f)), drawing, ChestAt, outline: false, castShadow: false);
        // dokunun ustu yukari, sol yani robota bakanin solu
        chest.localRotation = Quaternion.AngleAxis(180f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.right);

        // sirtta pil cantasi: turuncu kapak, uc yesil sarj isigi (robotun arkasi hep belli olsun)
        Parts.Add("Pack", body, MeshFactory.RoundedBox(new Vector3(0.19f, 0.15f, 0.07f), 0.02f), metal, new Vector3(0, 0.22f, -0.115f));
        Parts.Add("PackLid", body, MeshFactory.RoundedBox(new Vector3(0.17f, 0.02f, 0.06f), 0.008f), safety, new Vector3(0, 0.3f, -0.115f), outline: false);
        var led = Mats.Emissive(Mats.Hex("#6CF59A"), Mats.Hex("#6CF59A") * 1.2f, 0.5f);
        for (int k = -1; k <= 1; k++)
            Parts.Add("Led", body, MeshFactory.RoundedBox(new Vector3(0.025f, 0.012f, 0.008f), 0.003f), led, new Vector3(k * 0.04f, 0.25f, -0.152f), outline: false, castShadow: false);

        BuildRibbon();
        BuildCrayon();
        BuildFoldedDrawing(dark);

        Parts.Add("Neck", body, MeshFactory.RoundedCylinder(0.03f, 0.04f, 0.01f, 16), dark, new Vector3(0, 0.325f, 0));
    }

    static readonly Vector3 ChestAt = new Vector3(0, 0.215f, 0.0855f);   // gogusteki cizimin yeri

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

    // Ece'nin kalemi (Bolum 68'de bulunur): pil cantasinin arkasinda, kurdele dugumunun yaninda capraz. Baslangicta gizli.
    void BuildCrayon()
    {
        crayon = Crayon.Create(body, new Vector3(-0.035f, 0.225f, -0.168f), 0.105f);
        crayon.localRotation = Quaternion.Euler(0f, 0f, 32f);
        crayon.gameObject.SetActive(false);
    }

    public override void SetCrayon(bool on) => crayon.gameObject.SetActive(on);

    // Ece'nin katlanmis cizimi (Bolum 76'da bulunur): pil cantasinin sirtinda, kalemin ustunde, cantanin ust kenarindaki ince kayisin altina
    // sikistirilmis; ust kismi kapagin arkasindan tasar (kamera yukaridan-arkadan bakar: cantanin alt kisminda kalemin arkasinda
    // gorunmuyordu). Yerdeki ile ayni model (FoldedDrawing), kucultulmus. Baslangicta gizli. Canta sirasi: kurdele, kalem, cizim.
    void BuildFoldedDrawing(Material strap)
    {
        foldedDrawing = Parts.Empty("KatlanmisCizim-Sirt", body);
        foldedDrawing.localPosition = new Vector3(-0.05f, 0.298f, -0.154f);
        backPaper = FoldedDrawing.Create(foldedDrawing, Vector3.zero, 0f);
        backPaper.localRotation = Quaternion.Euler(-90f, 0f, -10f);   // kagidin yuzu arkaya bakar, hafif egik
        backPaper.localScale = Vector3.one * 0.36f;
        Parts.Add("Kayis", foldedDrawing, MeshFactory.RoundedBox(new Vector3(0.2f, 0.012f, 0.008f), 0.003f, 1), strap,
            new Vector3(0.05f, -0.012f, -0.008f), outline: false, castShadow: false);
        foldedDrawing.gameObject.SetActive(false);
    }

    public override void SetFoldedDrawing(bool on) => foldedDrawing.gameObject.SetActive(on);
    public override Vector3 RibbonPoint => transform.TransformPoint(RibbonKnot + Vector3.up * 0.03f);
    public override Vector3 ChestPoint => transform.TransformPoint(ChestAt);

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
        hands[side] = Parts.Add("Hand", elbow, MeshFactory.RoundedBox(new Vector3(0.042f, 0.048f, 0.034f), 0.014f), dark, new Vector3(0, -0.105f, 0));
        shoulders[side] = shoulder;
        elbows[side] = elbow;
        if (side == 1) { BuildHeldCrayon(); BuildHeldNote(); }
    }

    // Sag eldeki kucuk kagit (Bolge 8, notu okurken): krem, yazisiz; baslangicta gizli
    void BuildHeldNote()
    {
        heldNote = Parts.Add("HeldNote", hands[1], MeshFactory.RoundedBox(new Vector3(0.075f, 0.004f, 0.055f), 0.002f, 1),
            Mats.Lit(Mats.Hex("#EFE6CF"), 0.15f), new Vector3(0f, -0.03f, 0.03f), outline: false);
        heldNote.localRotation = Quaternion.Euler(-20f, 0f, 0f);
        heldNote.gameObject.SetActive(false);
    }

    // Sag eldeki kalem (Bolum 70'te Kivilcim kendi elini cizerken): kolun uzantisi gibi one bakar. Baslangicta gizli.
    void BuildHeldCrayon()
    {
        heldCrayon = Crayon.Create(hands[1], HeldCrayonAt, HeldCrayonLength);
        heldCrayon.localRotation = Quaternion.Euler(0f, 0f, -90f);   // kalemin ucu (+X) elin ucuna (-Y)
        heldCrayon.gameObject.SetActive(false);
    }

    // pose: (one kalkma, yana acilma, dirsek bukumu); iki kol ayna gibi
    void SetArms(Vector3 pose)
    {
        arms = pose;
        for (int side = 0; side < 2; side++) SetArm(side, pose);
    }

    // Tek kol (side 0 sol, 1 sag)
    void SetArm(int side, Vector3 pose)
    {
        armPose[side] = pose;
        shoulders[side].localRotation = ShoulderPose(side, pose);
        elbows[side].localRotation = ElbowPose(pose.z);
    }

    static Quaternion ShoulderPose(int side, Vector3 pose) => Quaternion.Euler(-pose.x, 0, (side == 0 ? -1f : 1f) * pose.y);
    static Quaternion ElbowPose(float bend) => Quaternion.Euler(-bend, 0, 0);

    // Kolu, ucu (sol: avuc, sag: eldeki kalemin ucu) reachAt'e degecek sekilde kurar: iki parcali kol, dirsek disari-asagi bukulur;
    // ulasamazsa kol o yone uzanir. reachWeight kadar reachFrom durusundan hedefe.
    void ReachArm(int side)
    {
        float sx = side == 0 ? -1f : 1f;
        float upper = UpperArm, lower = side == 1 && heldCrayon.gameObject.activeSelf ? CrayonTipFromElbow : PalmFromElbow;
        Vector3 from = shoulders[side].localPosition, to = body.InverseTransformPoint(reachAt[side]);
        float d = Mathf.Clamp(Vector3.Distance(from, to), Mathf.Abs(upper - lower) + 1e-3f, upper + lower - 1e-3f);
        var dir = (to - from).normalized;
        var elbowSide = Vector3.ProjectOnPlane(new Vector3(sx, -1f, -0.5f), dir).normalized;
        float cos = (upper * upper + d * d - lower * lower) / (2f * upper * d);
        var elbowAt = from + dir * (upper * cos) + elbowSide * (upper * Mathf.Sqrt(Mathf.Max(0f, 1f - cos * cos)));
        var upperDir = (elbowAt - from) / upper;
        var lowerDir = (from + dir * d - elbowAt).normalized;
        // omuz: kolun yerel -Y'si ust kola, dirsek kolun yerel +Z'sine bukulur
        var reached = Quaternion.LookRotation(Vector3.ProjectOnPlane(lowerDir, upperDir).normalized, -upperDir);
        float w = reachWeight[side];
        shoulders[side].localRotation = Quaternion.Slerp(ShoulderPose(side, reachFrom[side]), reached, w);
        elbows[side].localRotation = Quaternion.Slerp(ElbowPose(reachFrom[side].z), ElbowPose(Vector3.Angle(upperDir, lowerDir)), w);
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
        headYaw = headYawTarget = nod = tilt = lean = lift = beaconBoost = beaconDim = 0f;
        SetEyes(EyeCalm, 0f);
        for (int side = 0; side < 2; side++) { reachWeight[side] = 0f; reachFrom[side] = ArmRest; }
        SetArms(ArmRest);
        heldNote.gameObject.SetActive(false);
        carriedHome = null;   // elde kalan nesneyi sahnesi geri alir (RiverBedTraces.ReturnCrayon, NoteEnd.Restore)
        heldPaper = null;
        gripPaper = false;
        openAmount = 0f;
        if (heldCrayon.gameObject.activeSelf)   // elinin cevresini cizerken durduruldu: kalem sirta
        {
            heldCrayon.gameObject.SetActive(false);
            SetCrayon(true);
        }
    }

    protected override void Animate(float t)
    {
        // suzulme: yavasca inip kalkar; itici isigi onunla nefes alir
        float bob = 0.018f * Mathf.Sin(t * 2.2f);
        body.localPosition = new Vector3(0, bob + lift, 0);
        body.localRotation = Quaternion.Euler(lean, 0, 0);
        thrusterMat.SetColor("_EmissionColor", Mats.Hex("#7FD8FF") * (1.2f + 0.5f * Mathf.Sin(t * 2.2f + 1.5f)));

        float blink = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(t * 4f), 8f);
        beaconMat.SetColor("_EmissionColor", Mats.Hex("#FFA733") * ((0.3f + 1.2f * Mathf.Max(blink, beaconBoost)) * (1f - beaconDim)));

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
        if (gripPaper)
            for (int side = 0; side < 2; side++)
                reachAt[side] = heldPaper.TransformPoint(new Vector3((side == 0 ? -0.5f : 0.5f) * FoldedDrawing.Width * GripInset, 0f, 0f));
        for (int side = 0; side < 2; side++)
            if (reachWeight[side] > 0f) ReachArm(side);
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

    // Gozlerin su anki rengi, normal rengi ve ozel durusta mi (goruntu denetimi)
    public Color EyeColor => eyeColor;
    public static Color CalmEyes => EyeCalm;
    public bool Posing => posing;

    public override IEnumerator Calm()
    {
        GlanceAtCamera(false);
        var from = arms;
        SetEyes(EyeCalm, 0f);
        yield return Tween.Run(0.4f, t =>
        {
            float e = Tween.InOutCubic(t);
            SetArms(Vector3.Lerp(from, ArmRest, e));
            lift = 0f;
            beaconBoost = 0f;
        });
        posing = false;
    }

    public override IEnumerator EceEyes(float seconds)
    {
        var from = eyeColor;
        if (seconds > 0f) yield return Tween.Run(seconds, t => eyeColor = Color.Lerp(from, EyeEce, Tween.InOutCubic(t)));
        SetEyes(EyeEce, 0f);
    }

    // Bolum 70: sol elini yavasca duvara, gogsunun onundeki bos yere koyar, basini hafifce ona eger; spot null ise kolu indirir
    public override IEnumerator PalmOnWall(Vector3? spot)
    {
        posing = true;
        GlanceAtCamera(false);
        if (spot.HasValue) reachAt[0] = spot.Value;
        float from = reachWeight[0], to = spot.HasValue ? 1f : 0f;
        yield return Tween.Run(0.7f, t =>
        {
            reachWeight[0] = Mathf.Lerp(from, to, Tween.InOutCubic(t));
            if (reachWeight[1] == 0f) SetArm(1, ArmRest);
            tilt = 7f * reachWeight[0];
            nod = 12f * reachWeight[0];   // eline bakar
        });
        if (spot.HasValue) yield break;
        SetArm(0, ArmRest);
        posing = false;
    }

    // Duvardaki avucun robota gore yeri (dunya olcusunde): govdenin tam onu
    public override Vector3 PalmReach => new Vector3(0f, 0f, PalmForward * ModelScale);

    // Sol avucun su anki yeri (dunya konumu; goruntu denetimi)
    public Vector3 LeftPalm => hands[0].TransformPoint(new Vector3(0f, -0.024f, 0f));

    // Bolum 70: kalemi sag eliyle omzunun ustunden sirtindan alir, ucunu duvara koyup outline boyunca (duvardaki elinin cevresi) gezdirir,
    // kalemi yine sirtina takar. Sol eli duvarda kalir (PalmOnWall(null) indirir).
    public override IEnumerator TraceHand(IReadOnlyList<Vector3> outline, Action<int> reached, float seconds)
    {
        posing = true;
        yield return Tween.Run(0.5f, t => SetArm(1, Vector3.Lerp(ArmRest, ArmOverShoulder, Tween.InOutCubic(t))));
        SetCrayon(false);
        heldCrayon.gameObject.SetActive(true);
        reachFrom[1] = ArmOverShoulder;
        reachAt[1] = outline[0];
        yield return Tween.Run(0.7f, t => reachWeight[1] = Tween.InOutCubic(t));
        reached(0);

        var along = new float[outline.Count];   // her noktaya kadar cizginin boyu
        for (int i = 1; i < outline.Count; i++) along[i] = along[i - 1] + Vector3.Distance(outline[i - 1], outline[i]);
        int next = 1;
        yield return Tween.Run(seconds, t =>
        {
            float s = Mathf.SmoothStep(0f, 1f, t) * along[along.Length - 1];
            while (next < outline.Count - 1 && along[next] <= s) reached(next++);
            float k = Mathf.InverseLerp(along[next - 1], along[next], s);
            reachAt[1] = Vector3.Lerp(outline[next - 1], outline[next], k);
        });
        reached(outline.Count - 1);

        yield return Tween.Run(0.6f, t => reachWeight[1] = 1f - Tween.InOutCubic(t));
        SetArm(1, ArmOverShoulder);
        heldCrayon.gameObject.SetActive(false);
        SetCrayon(true);
        reachFrom[1] = ArmRest;
        yield return Tween.Run(0.3f, t => SetArm(1, Vector3.Lerp(ArmOverShoulder, ArmRest, Tween.InOutCubic(t))));
    }

    // Bolge 8 notu: sag kol kalkar, elde kucuk krem kagit, bas kagida egilir; indirince kagit elde kalmaz (yuruyusu bozmasin)
    public override IEnumerator HoldNote(bool up)
    {
        posing = true;
        if (up) heldNote.gameObject.SetActive(true);
        var from = up ? ArmRest : ArmHoldNote;
        var to = up ? ArmHoldNote : ArmRest;
        float nodFrom = nod, nodTo = up ? NoteNod : 0f;
        yield return Tween.Run(0.3f, t =>
        {
            float e = Tween.InOutCubic(t);
            SetArm(1, Vector3.Lerp(from, to, e));
            nod = Mathf.Lerp(nodFrom, nodTo, e);
        });
        if (up) yield break;
        heldNote.gameObject.SetActive(false);
        arms = ArmRest;
        posing = false;
    }

    // 68 kalem, 76 cizim: one egilir, sag eliyle yerdeki nesneyi alir (egik kalir). Nesnenin eski ebeveyni saklanir: StowOnBack
    // sonunda nesneyi gizleyip oraya birakir ("Tekrar" onu yine yere koyabilsin).
    public override IEnumerator PickUp(Transform item)
    {
        posing = true;
        GlanceAtCamera(false);
        carriedHome = item.parent;
        yield return Tween.Run(0.4f, t =>
        {
            float e = Tween.InOutCubic(t);
            lean = PickupLean * e;
            nod = 20f * e;   // basi nesneye bakar
            SetArms(Vector3.Lerp(ArmRest, ArmPickup, e));
        });
        // nesne ele gelir (son birkac santim), sonra ele baglanir
        var hand = hands[1];
        Vector3 fromPos = item.position;
        Quaternion fromRot = item.rotation;
        yield return Tween.Run(0.15f, t => item.SetPositionAndRotation(Vector3.Lerp(fromPos, hand.position, t), fromRot));
        item.SetParent(hand, true);
    }

    // Dogrulurken kolunu omzunun ustunden gecirir, nesne elden sirttaki yerine kayar (kagit sirttaki kucuk boyuna iner), kol iner.
    public override IEnumerator StowOnBack(Transform item, BackSlot slot)
    {
        posing = true;
        float leanFrom = lean, nodFrom = nod;
        Vector3 leftFrom = armPose[0], rightFrom = armPose[1];
        bool gripped = gripPaper;   // 76: iki el kagidin kenarinda; kagit sag ele gecer, kollar uzanmadan durusa doner
        if (gripped)
        {
            gripPaper = false;
            item.SetParent(hands[1], true);
            reachFrom[0] = ArmRest;
            reachFrom[1] = ArmOverShoulder;
        }
        yield return Tween.Run(0.6f, t =>
        {
            float e = Tween.InOutCubic(t);
            lean = leanFrom * (1f - e);
            nod = nodFrom * (1f - e);
            if (gripped) reachWeight[0] = reachWeight[1] = 1f - e;
            else
            {
                SetArm(0, Vector3.Lerp(leftFrom, ArmRest, e));
                SetArm(1, Vector3.Lerp(rightFrom, ArmOverShoulder, e));
            }
        });
        if (gripped)
        {
            reachWeight[0] = reachWeight[1] = 0f;
            reachFrom[0] = reachFrom[1] = ArmRest;
            SetArm(0, ArmRest);
            SetArm(1, ArmOverShoulder);
        }
        var spot = slot == BackSlot.Crayon ? crayon : backPaper;
        // kalemin yerdeki ve sirttaki boyu ayni; kagit sirtta kucuk
        float sizeFrom = item.lossyScale.x, sizeTo = slot == BackSlot.Crayon ? sizeFrom : backPaper.lossyScale.x;
        Vector3 fromPos = item.position;
        Quaternion fromRot = item.rotation;
        yield return Tween.Run(0.25f, t =>
        {
            float e = Tween.InOutCubic(t);
            item.SetPositionAndRotation(Vector3.Lerp(fromPos, spot.position, e), Quaternion.Slerp(fromRot, spot.rotation, e));
            item.localScale = Vector3.one * (Mathf.Lerp(sizeFrom, sizeTo, e) / item.parent.lossyScale.x);
        });
        item.gameObject.SetActive(false);
        item.SetParent(carriedHome, true);
        carriedHome = null;
        heldPaper = null;
        ShowOnBack(slot);
        yield return Tween.Run(0.3f, t => SetArm(1, Vector3.Lerp(ArmOverShoulder, ArmRest, Tween.InOutCubic(t))));
        lean = 0f;
        nod = 0f;
        arms = ArmRest;
        posing = false;
    }

    // 76: dogrulur, kagidi gogsunun onune getirir, iki eli kenarlarinda; basi kagida egilir; ust katin kosesini kaldirir ve durur.
    public override IEnumerator TryUnfold(Transform paper, float amount, float hold)
    {
        posing = true;
        heldPaper = paper;
        paper.SetParent(body, true);   // suzulurken govdeyle birlikte iner kalkar; eller onu izler
        float leanFrom = lean, nodFrom = nod;
        for (int side = 0; side < 2; side++) reachFrom[side] = armPose[side];
        gripPaper = true;
        Vector3 posFrom = paper.localPosition;
        Quaternion rotFrom = paper.localRotation;
        float sizeFrom = paper.localScale.x, sizeTo = HeldPaperSize / body.lossyScale.x;
        yield return Tween.Run(0.5f, t =>
        {
            float e = Tween.InOutCubic(t);
            lean = leanFrom * (1f - e);
            nod = Mathf.Lerp(nodFrom, PaperNod, e);
            reachWeight[0] = reachWeight[1] = e;
            paper.localPosition = Vector3.Lerp(posFrom, HeldPaperAt, e);
            paper.localRotation = Quaternion.Slerp(rotFrom, HeldPaperTurn, e);
            paper.localScale = Vector3.one * Mathf.Lerp(sizeFrom, sizeTo, e);
        });
        yield return Tween.Run(0.5f, t => FoldedDrawing.SetOpen(paper, amount * Tween.InOutCubic(t)));
        openAmount = amount;
        yield return Tween.Wait(hold);
    }

    public override IEnumerator Refold(float seconds)
    {
        if (heldPaper == null) yield break;
        var paper = heldPaper;
        float from = openAmount;
        yield return Tween.Run(seconds, t => FoldedDrawing.SetOpen(paper, from * (1f - Tween.InOutCubic(t))));
        openAmount = 0f;
    }

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

    public override Vector3 HeadTop => head.position + Vector3.up * 0.25f * transform.lossyScale.y;

    // Kutlamadan sonra kollar iner, gozler sakinlesir, basi yavasca gokyuzune kalkar ve orada kalir.
    public override IEnumerator LookUp()
    {
        GlanceAtCamera(false);
        posing = true;
        SetEyes(EyeCalm, 0f);
        var from = arms;
        yield return Tween.Run(1.4f, t =>
        {
            float e = Tween.InOutCubic(t);
            SetArms(Vector3.Lerp(from, ArmRest, e));
            nod = -30f * e;
        });
    }

    // 78 aciklama ani: donar, gozler sonuklesip kisilir, anten isigi soner; basini gogsundeki cizime eger, durur, yavasca ekrana kaldirir.
    const float RealizeNod = 45f, RealizeEyes = 0.6f, RealizeSquint = 0.35f;

    public override IEnumerator Realize()
    {
        posing = true;
        GlanceAtCamera(false);
        var dim = EyeCalm * RealizeEyes;
        yield return Tween.Run(0.3f, t =>
        {
            float e = Tween.InOutCubic(t);
            SetEyes(Color.Lerp(EyeCalm, dim, e), RealizeSquint * e);
            beaconDim = e;
        });
        yield return Tween.Wait(0.3f);
        yield return Tween.Run(0.6f, t => nod = RealizeNod * Tween.InOutCubic(t));
        yield return Tween.Wait(1.2f);
        yield return Tween.Run(0.8f, t =>
        {
            float e = Tween.InOutCubic(t);
            nod = RealizeNod * (1f - e);
            SetEyes(Color.Lerp(dim, EyeCalm, e), RealizeSquint * (1f - e));
            beaconDim = 1f - e;
        });
        nod = 0f;
        beaconDim = 0f;
        SetEyes(EyeCalm, 0f);
        posing = false;
    }

    // 80 kapanisi: kollar iner, gozler sakin; bas noktaya kalkar (yukari en cok 45 derece), yuksekse govde biraz geriye yaslanir.
    // Harita cocuk boyunda (oklar bas hizasinin biraz altinda): bas yine de belirgin kalkar, "okun otesine" bakar
    const float RaiseMaxUp = 45f, RaiseMinUp = 22f, RaiseMaxLean = 8f, RaiseMaxTurn = 30f;
    public float Nod => nod;   // goruntu denetimi (eksi: yukari bakiyor)

    public override IEnumerator RaiseHeadTo(Vector3 worldPoint, float seconds)
    {
        GlanceAtCamera(false);
        posing = true;
        SetEyes(EyeCalm, 0f);
        var local = body.InverseTransformPoint(worldPoint) - head.localPosition;
        float flat = new Vector2(local.x, local.z).magnitude;
        float up = Mathf.Atan2(local.y, Mathf.Max(flat, 1e-3f)) * Mathf.Rad2Deg;          // noktanin yukseklik acisi
        float nodTo = Mathf.Clamp(-up - HeadUp, -RaiseMaxUp, -RaiseMinUp);                // bas zaten HeadUp kadar kalkik
        float leanTo = -Mathf.Clamp(up - RaiseMaxUp, 0f, RaiseMaxLean);
        float turnTo = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -RaiseMaxTurn, RaiseMaxTurn);
        var armsFrom = arms;
        float nodFrom = nod, leanFrom = lean;
        headYawTarget = turnTo;
        if (seconds <= 0f) headYaw = turnTo;
        else yield return Tween.Run(seconds, t =>
        {
            float e = Tween.InOutCubic(t);
            SetArms(Vector3.Lerp(armsFrom, ArmRest, e));
            nod = Mathf.Lerp(nodFrom, nodTo, e);
            lean = Mathf.Lerp(leanFrom, leanTo, e);
        });
        SetArms(ArmRest);
        nod = nodTo;
        lean = leanTo;
    }

    // Basindaki anten isigi art arda parlayip soner (selam).
    public override IEnumerator Signal(int times)
    {
        for (int i = 0; i < times; i++)
        {
            yield return Tween.Run(0.34f, t => beaconBoost = 1.6f * Mathf.Sin(t * Mathf.PI));
            beaconBoost = 0f;
            yield return Tween.Wait(0.18f);
        }
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
