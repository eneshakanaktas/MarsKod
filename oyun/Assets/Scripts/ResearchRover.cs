using System.Collections;
using UnityEngine;

// Kutup ekibinin arastirma araci KT-2 (docs/tasarim/senaryo-bolge-02.md, Bolum 14-20).
// Alanin arkasinda durur, bulmacayi etkilemez. Kara saplanmis hali yardim isigini yakar (uc kisa yanip sonme);
// Bolum 19 sonunda dogrulur, farlari yanar; Bolum 20 kapanisinda koloniye dogru uzaklasir.
public class ResearchRover : MonoBehaviour
{
    // Kara saplanmis hali: burnu kara gomulu, arkasi havada, biraz da yana yatik (kameradan bakinca belli olsun)
    const float NoseDownAngle = 24f, RollAngle = 10f;
    const float DoorOpenAngle = 75f;    // kapi menteseden disari (guneye, kameraya) acik

    Transform tiltPivot, door;
    Material beaconMat, headlightMat;
    Color beaconColor, headlightColor;
    bool beaconOn, upright, doorOpen;
    Vector3 homePos;

    public static ResearchRover Create(Transform parent, Vector3 pos, bool upright, bool doorOpen)
    {
        var root = Parts.Empty("Iz-Arac-KT2", parent);
        root.localPosition = pos;
        var rover = root.gameObject.AddComponent<ResearchRover>();
        rover.Build();
        rover.homePos = pos;
        rover.upright = upright;
        rover.doorOpen = doorOpen;
        rover.Restore();
        return rover;
    }

    void Build()
    {
        var shell = Mats.Lit(Mats.Hex("#D9D4C8"), 0.35f);
        var stripe = Mats.Lit(Mats.Hex("#E07B3C"), 0.3f);
        var dark = Mats.Lit(Mats.Hex("#25262E"), 0.2f);
        var glass = Mats.Lit(Mats.Hex("#1B2A3A"), 0.8f);
        var tire = Mats.Lit(Mats.Hex("#1E1E22"), 0.1f);
        var snow = Mats.Lit(Mats.Hex("#C9D3E6"), 0.25f);
        beaconColor = Mats.Hex("#FF4A3A");
        headlightColor = Mats.Hex("#FFF2C8");
        beaconMat = Mats.Emissive(beaconColor, beaconColor, 0.5f);
        headlightMat = Mats.Emissive(headlightColor, Color.black, 0.6f);

        // Devrilme ekseni aracin burnunun alt kenari: arac burnunun uzerine kapanir
        tiltPivot = Parts.Empty("Devrilme", transform);
        tiltPivot.localPosition = new Vector3(-0.5f, 0f, 0f);
        var body = Parts.Empty("Govde", tiltPivot);
        body.localPosition = new Vector3(0.5f, 0f, 0f);

        Parts.Add("Kasa", body, MeshFactory.RoundedBox(new Vector3(1.0f, 0.26f, 0.5f), 0.05f), shell, new Vector3(0f, 0.25f, 0f));
        Parts.Add("Serit", body, MeshFactory.RoundedBox(new Vector3(1.01f, 0.05f, 0.51f), 0.02f), stripe, new Vector3(0f, 0.22f, 0f), outline: false);
        Parts.Add("Kabin", body, MeshFactory.RoundedBox(new Vector3(0.42f, 0.22f, 0.46f), 0.06f), shell, new Vector3(-0.24f, 0.48f, 0f));
        Parts.Add("Cam", body, MeshFactory.RoundedBox(new Vector3(0.03f, 0.13f, 0.38f), 0.015f), glass, new Vector3(-0.455f, 0.5f, 0f), outline: false);
        // Kapinin ardindaki karanlik ic (kapi kapaliyken gorunmez)
        Parts.Add("Ic", body, MeshFactory.RoundedBox(new Vector3(0.26f, 0.17f, 0.01f), 0.01f), dark, new Vector3(-0.24f, 0.48f, -0.231f), outline: false);
        var hinge = Parts.Empty("Mentese", body);
        hinge.localPosition = new Vector3(-0.39f, 0.48f, -0.24f);
        door = hinge;
        Parts.Add("Kapi", hinge, MeshFactory.RoundedBox(new Vector3(0.28f, 0.19f, 0.025f), 0.012f), shell, new Vector3(0.15f, 0f, 0f));

        foreach (float x in new[] { -0.32f, 0.32f })
            foreach (float z in new[] { -0.27f, 0.27f })
            {
                var wheel = Parts.Add("Teker", body, MeshFactory.RoundedCylinder(0.12f, 0.09f, 0.02f, 16), tire, new Vector3(x, 0.12f, z));
                wheel.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

        Parts.Add("Direk", body, MeshFactory.RoundedCylinder(0.012f, 0.3f, 0.004f, 8), dark, new Vector3(0.36f, 0.53f, 0.12f), outline: false);
        Parts.Add("YardimIsigi", body, MeshFactory.Sphere(0.05f, 8, 12), beaconMat, new Vector3(0.36f, 0.71f, 0.12f), outline: false, castShadow: false);
        foreach (float z in new[] { -0.16f, 0.16f })
            Parts.Add("Far", body, MeshFactory.Sphere(0.035f, 8, 12), headlightMat, new Vector3(-0.5f, 0.27f, z), outline: false, castShadow: false);

        // Aracin burnunun saplandigi kar yigini (arac dogrulunca yerinde kalir)
        Parts.Add("Kar", transform, MeshFactory.RoundedBox(new Vector3(0.45f, 0.2f, 0.75f), 0.15f), snow, new Vector3(-0.6f, 0.03f, 0f), outline: false);
    }

    // Bolum basi ya da yeniden deneme: bolumun baslangic hali
    public void Restore()
    {
        StopAllCoroutines();
        transform.localPosition = homePos;
        SetPose(upright ? 0f : 1f);
        door.localRotation = Quaternion.Euler(0f, doorOpen ? DoorOpenAngle : 0f, 0f);
    }

    // Bolum 19 sonu: arac yavasca dogrulur, farlar yanar, yardim isigi soner (artik gerek yok)
    public void StandUp()
    {
        if (upright) return;
        StartCoroutine(Tween.Run(1.4f, k => SetPose(1f - Tween.InOutCubic(k))));
    }

    // Bolum 20 kapanisi: koloniye dogru (bati) uzaklasir
    public IEnumerator DriveAway()
    {
        Vector3 from = transform.localPosition, to = from + new Vector3(-6f, 0f, 0.6f);
        yield return Tween.Run(3.2f, k => transform.localPosition = Vector3.Lerp(from, to, Tween.InCubic(k)));
    }

    // tilt: 1 kara saplanmis (isik yanip soner, farlar sonuk), 0 dik (farlar yanik)
    void SetPose(float tilt)
    {
        tiltPivot.localRotation = Quaternion.Euler(RollAngle * tilt, 0f, NoseDownAngle * tilt);
        beaconOn = tilt > 0.5f;
        headlightMat.SetColor("_EmissionColor", headlightColor * (2.4f * (1f - tilt)));
    }

    void Update()
    {
        // arka plandaki uzak aracla ayni desen: uc kisa yanip sonme, sonra bekleme
        float phase = Mathf.Repeat(Time.time, 3f);
        bool on = beaconOn && phase < 1.2f && Mathf.Repeat(phase / 0.4f, 1f) < 0.5f;
        beaconMat.SetColor("_EmissionColor", beaconColor * (on ? 2.6f : 0.12f));
    }
}
