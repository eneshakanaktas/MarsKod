using System.Collections;
using UnityEngine;

// Roketin bicimi: Slim ince uzun (52, kucuk); Lander kalin gövdeli inis araci (51): Kivilcim kapidan sigsin diye gövde ve kapi buyuk.
public class RocketLook
{
    public float Girth = 1f;                                   // govde kalinligi carpani
    public float DoorWidth = 0.15f, DoorHeight = 0.27f, DoorBottom = 0.845f;
    public float BandY = 1.32f;                                // turuncu bandin yuksekligi (kapinin ustunde kalsin)

    public static readonly RocketLook Slim = new RocketLook();
    public static readonly RocketLook Lander = new RocketLook { Girth = 1.9f, DoorWidth = 0.5f, DoorHeight = 0.85f, DoorBottom = 0.45f, BandY = 1.5f };
}

// Koloninin kucuk roketi (Bolge 6): Kivilcim'i otomatik rotayla kanyona getirdi ("Hazirlayan: D. Aras").
// Burun konisi + silindir + motor etegi, turuncu bant, uc egik ayak. Kameraya bakan yuzde kapi (acikken koyu bosluk + yana acilmis
// kapak), kapidan yere inen merdiven, altinda yanik iz, motorun altinda alev. 51'in inis sahnesi (CanyonScenes.Landing) bunu oynatir.
public class ColonyRocket : MonoBehaviour
{
    const float LadderTilt = 22f;          // merdivenin dikeyden egimi
    // kapak disa ve saga dogru genis acilir (-z kameraya bakan yuz): kapinin onu bos kalir, Kivilcim'in icinden gecmez
    const float HatchOpenAngle = -150f;
    // Kivilcim gövdenin ve merdivenin tamamen disinda kalsin diye esigin ve merdiven dibinin onundeki pay (robotun sirt cantasi dahil)
    const float StepOut = 0.26f, LadderClearance = 0.3f;
    const float FlameLength = 1f;

    Transform hatch, ladder, flame, burn;
    Vector3 home;                          // Create'teki yer (parent'in yerel uzayinda)
    RocketLook look;
    float hullRadius, ladderLength, flightHeight, flameStrength;

    public static ColonyRocket Create(Transform parent, Vector3 localPos, float scale, RocketLook look = null)
    {
        var root = Parts.Empty("Iz-Roket", parent);
        root.localPosition = localPos;
        root.localScale = Vector3.one * scale;
        var rocket = root.gameObject.AddComponent<ColonyRocket>();
        rocket.home = localPos;
        rocket.look = look ?? RocketLook.Slim;
        rocket.Build();
        rocket.SetLanded();
        return rocket;
    }

    public Vector3 HomePosition => home;   // yerde durdugu yer (SetLanded'in varacagi yer)

    // Kapinin onu ve merdivenin dibinin onu: parent'in yerel uzayinda (Kivilcim bunlar arasinda suzulur).
    // Ikisi de gövdenin ve merdivenin disinda: Kivilcim hicbir parcanin icinden gecmez.
    public Vector3 DoorPoint => ToParent(new Vector3(0f, look.DoorBottom, -(hullRadius + StepOut)));
    public Vector3 LadderFoot
    {
        get
        {
            var p = ToParent(ladder.localPosition + ladder.localRotation * new Vector3(0f, -ladderLength, 0f) + Vector3.back * LadderClearance);
            return new Vector3(p.x, home.y, p.z);
        }
    }

    // Yerinde: kapak acik, merdiven acik, yanik iz gorunur, alev yok
    public void SetLanded()
    {
        transform.localPosition = home;
        SetDoorOpen(1f);
        burn.gameObject.SetActive(true);
        flameStrength = 0f;
        flame.gameObject.SetActive(false);
    }

    // Havada: kapak kapali, merdiven ve yanik yok, alev yaniyor
    public void SetInFlight(float height)
    {
        flightHeight = height;
        transform.localPosition = home + Vector3.up * height;
        SetDoorOpen(0f);
        burn.gameObject.SetActive(false);
        flameStrength = 1f;
        flame.gameObject.SetActive(true);
    }

    // Yumusak inis: yavaslayarak yere oturur; son aninda alev kisilir, degince hafifce seker, yanik iz belirir
    public IEnumerator Land(float seconds)
    {
        float from = flightHeight;
        yield return Tween.Run(seconds, t =>
        {
            transform.localPosition = home + Vector3.up * from * (1f - Tween.OutCubic(t));
            flameStrength = t < 0.85f ? 1f : Mathf.Clamp01((1f - t) / 0.15f);
        });
        yield return Tween.Run(0.14f, t => transform.localPosition = home + Vector3.down * 0.04f * Mathf.Sin(t * Mathf.PI));
        transform.localPosition = home;
        flame.gameObject.SetActive(false);
        burn.gameObject.SetActive(true);
    }

    // Kapak acilir, ardindan merdiven ustten asagi uzar
    public IEnumerator OpenDoor()
    {
        yield return Tween.Run(0.5f, t => hatch.localRotation = Quaternion.Euler(0f, HatchOpenAngle * Tween.OutCubic(t), 0f));
        ladder.gameObject.SetActive(true);
        yield return Tween.Run(0.5f, t => ladder.localScale = new Vector3(1f, Tween.OutCubic(t), 1f));
    }

    void SetDoorOpen(float amount)
    {
        hatch.localRotation = Quaternion.Euler(0f, HatchOpenAngle * amount, 0f);
        ladder.localScale = new Vector3(1f, amount, 1f);
        ladder.gameObject.SetActive(amount > 0f);
    }

    Vector3 ToParent(Vector3 rocketLocal) => transform.parent.InverseTransformPoint(transform.TransformPoint(rocketLocal));

    void Update()
    {
        if (!flame.gameObject.activeSelf) return;
        float flicker = 1f + 0.08f * Mathf.Sin(Time.time * 38f) + 0.05f * Mathf.Sin(Time.time * 23f);
        flame.localScale = new Vector3(look.Girth, flameStrength * flicker, look.Girth);
    }

    void Build()
    {
        float g = look.Girth;
        hullRadius = 0.2f * g;
        var root = transform;
        var hull = Mats.Lit(Mats.Hex("#D9D4CA"), 0.45f);
        var dark = Mats.Lit(Mats.Hex("#34313A"), 0.35f);
        var accent = Mats.Lit(Mats.Hex("#E8743B"), 0.4f);

        var profile = new[]
        {
            new Vector2(0.11f * g, 0.28f), new Vector2(0.17f * g, 0.4f), new Vector2(0.2f * g, 0.48f), new Vector2(0.2f * g, 1.55f),
            new Vector2(0.17f * g, 1.8f), new Vector2(0.1f * g, 2.02f), new Vector2(0.03f * g, 2.16f), new Vector2(0f, 2.19f),
        };
        Parts.Add("Govde", root, MeshFactory.Lathe(profile, 28), hull, Vector3.zero);
        Parts.Add("Motor", root, MeshFactory.RoundedCylinder(0.13f * g, 0.12f, 0.02f, 20), dark, new Vector3(0f, 0.3f, 0f));
        Parts.Add("Bant", root, MeshFactory.RoundedCylinder(0.206f * g, 0.09f, 0.01f, 28), accent, new Vector3(0f, look.BandY, 0f), outline: false);
        Parts.Add("Pencere", root, MeshFactory.RoundedCylinder(0.04f * g, 0.02f, 0.006f, 14), dark, new Vector3(0f, 1.72f + (g - 1f) * 0.05f, -0.172f * g), outline: false)
            .localRotation = Quaternion.Euler(80f, 0f, 0f);

        BuildLegs(dark);
        BuildDoor(hull, dark);
        BuildFlame();

        // yanik iz: kenari yumusak gorunsun diye iki kat; inis aninda belirir
        burn = Parts.Empty("Yanik", root);
        Parts.Add("Yanik", burn, MeshFactory.RoundedCylinder(0.75f, 0.004f, 0.002f, 28), Mats.Lit(Mats.Hex("#4A2A20"), 0.05f), new Vector3(0f, 0.002f, 0f), outline: false, castShadow: false);
        Parts.Add("Yanik", burn, MeshFactory.RoundedCylinder(0.48f, 0.004f, 0.002f, 28), Mats.Lit(Mats.Hex("#2A1C18"), 0.05f), new Vector3(0f, 0.004f, 0f), outline: false, castShadow: false);
    }

    // uc ayak: govdeden yere egik; arkada bir, onde iki (kapinin onu bos kalsin)
    void BuildLegs(Material dark)
    {
        float g = look.Girth;
        float spread = 0.46f + 0.2f * (g - 1f);
        float thick = 0.04f * Mathf.Sqrt(g);
        foreach (float deg in new[] { 90f, 210f, 330f })
        {
            float a = deg * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            var top = dir * (0.17f * g) + Vector3.up * 0.65f;
            var foot = dir * spread;
            var leg = Parts.Add("Ayak", transform, MeshFactory.RoundedBox(new Vector3(thick, (top - foot).magnitude, thick), 0.012f), dark, (top + foot) * 0.5f);
            leg.localRotation = Quaternion.FromToRotation(Vector3.up, top - foot);
            Parts.Add("Taban", transform, MeshFactory.RoundedCylinder(0.06f * Mathf.Sqrt(g), 0.025f, 0.008f, 14), dark, foot + Vector3.up * 0.012f, outline: false);
        }
    }

    // kapi: kameraya bakan yuzde koyu bosluk; kapak menteseyle sag kenarda (kapaliyken kapiyi orter); merdiven kapinin esiginden yere
    void BuildDoor(Material hull, Material dark)
    {
        float w = look.DoorWidth, h = look.DoorHeight, bottom = look.DoorBottom;
        float faceZ = -hullRadius;
        // derin kutu: kalin govdede yuvarlak yuzeyin kapi kenarlarindan bosluk birakmasini kapatir
        Parts.Add("Kapi", transform, MeshFactory.RoundedBox(new Vector3(w, h, hullRadius * 0.6f), 0.01f), dark, new Vector3(0f, bottom + h * 0.5f, faceZ + hullRadius * 0.3f - 0.012f), outline: false);

        hatch = Parts.Empty("Kapak", transform);
        hatch.localPosition = new Vector3(w * 0.5f, bottom + h * 0.5f, faceZ - 0.02f);
        Parts.Add("Levha", hatch, MeshFactory.RoundedBox(new Vector3(w, h, 0.015f), 0.008f), hull, new Vector3(-w * 0.5f, 0f, 0f));

        // merdiven: esikten disa dogru egik; ustte (esik) sabit, asagi dogru uzar (OpenDoor ust ucundan acar)
        ladderLength = bottom / Mathf.Cos(LadderTilt * Mathf.Deg2Rad);
        ladder = Parts.Empty("Merdiven", transform);
        ladder.localPosition = new Vector3(0f, bottom, faceZ - 0.03f);
        ladder.localRotation = Quaternion.Euler(LadderTilt, 0f, 0f);
        float rail = w * 0.33f;
        foreach (float x in new[] { -rail, rail })
            Parts.Add("Ray", ladder, MeshFactory.RoundedBox(new Vector3(0.014f, ladderLength, 0.014f), 0.005f), dark, new Vector3(x, -ladderLength * 0.5f, 0f), outline: false);
        const int steps = 5;
        for (int k = 0; k < steps; k++)
            Parts.Add("Basamak", ladder, MeshFactory.RoundedBox(new Vector3(w * 0.67f, 0.012f, 0.014f), 0.004f), dark, new Vector3(0f, -ladderLength * (k + 0.5f) / steps, 0f), outline: false);
    }

    // alev: motorun altinda asagi bakan koni (ic ve dis); isik almaz, golge dusurmez
    void BuildFlame()
    {
        flame = Parts.Empty("Alev", transform);
        flame.localPosition = new Vector3(0f, 0.3f, 0f);
        var outer = new[] { new Vector2(0f, -FlameLength), new Vector2(0.05f, -0.6f * FlameLength), new Vector2(0.11f, -0.2f * FlameLength), new Vector2(0.12f, 0f) };
        var inner = new[] { new Vector2(0f, -0.65f * FlameLength), new Vector2(0.03f, -0.4f * FlameLength), new Vector2(0.07f, -0.1f * FlameLength), new Vector2(0.08f, 0f) };
        Parts.Add("AlevDis", flame, MeshFactory.Lathe(outer, 16), Mats.Unlit(Mats.Hex("#FFB25A")), Vector3.zero, outline: false, castShadow: false);
        Parts.Add("AlevIc", flame, MeshFactory.Lathe(inner, 16), Mats.Unlit(Mats.Hex("#FFF1C9")), Vector3.zero, outline: false, castShadow: false);
    }
}
