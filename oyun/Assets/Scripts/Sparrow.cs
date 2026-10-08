using UnityEngine;

public enum SparrowState { Incomplete, Dormant, Flying }

// Serce (Bolge 6): kanyon deposunda bulunan drone; adini Ece vermis, govdesinde turkuaz cocuk eliyle "SERÇE".
// Tombul, yuvarlak bir serce govdesi: onde iki kamera "goz" ve kucuk turuncu gaga, arkada kisa kuyruk, altta iki kizak;
// dort capraz kolun ucunda halkali (korumali) pervaneler (Mars'in ince havasinda ucan araclar gibi buyuk pervaneli).
// Kivilcim'in yari boyu. Halleri: eksik (54: bir kol ve pervaneler yok, kabukta delik), tam ama sonuk (55), ucuyor (56-60:
// gozler yanar, pervaneler doner, hafifce suzulup yalpalar). Ileri yonu -Z (kameraya bakar).
public class Sparrow : MonoBehaviour
{
    const float BodyRadius = 0.15f, Scale = 1.3f;
    static readonly Color EyeOn = Mats.Hex("#8FF0FF") * 1.6f;

    Transform body;
    readonly Transform[] blades = new Transform[4];
    GameObject[] missingWhenIncomplete;
    GameObject hole;
    Material eyeMat;
    SparrowState state;
    float phase;

    // Govde (3b'deki ilk ucus sahnesi bunu yukari kaldirir)
    public Transform Body => body;
    // Kizaklarin altindan govde merkezine yukseklik: rafa konurken
    public static float RestHeight => BodyRadius * 0.85f * Scale;

    public static Sparrow Create(Transform parent, Vector3 localPos, SparrowState state)
    {
        var go = new GameObject("Serce");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * Scale;
        var sparrow = go.AddComponent<Sparrow>();
        sparrow.Build();
        sparrow.SetState(state);
        return sparrow;
    }

    void Build()
    {
        phase = (transform.localPosition.x + transform.localPosition.z) * 3.7f;   // goruntu turlari tekrarlanabilir kalsin
        var shell = Mats.Lit(DroneColors.Body, 0.45f);
        var dark = Mats.Lit(DroneColors.Dark, 0.35f);
        var accent = Mats.Lit(DroneColors.Accent, 0.4f);
        eyeMat = Mats.Emissive(Mats.Hex("#20262C"), Color.black, 0.8f);

        body = Parts.Empty("Govde", transform);
        var dome = Parts.Add("Kabuk", body, MeshFactory.Sphere(BodyRadius, 12, 20), shell, Vector3.zero);
        dome.localScale = new Vector3(1f, 0.82f, 1.12f);
        // kuyruk: arkada kisa, hafif yukari kalkik; ucu turuncu
        var tail = Parts.Add("Kuyruk", body, MeshFactory.RoundedBox(new Vector3(0.09f, 0.025f, 0.11f), 0.01f), shell, new Vector3(0f, 0.04f, 0.18f));
        tail.localRotation = Quaternion.Euler(-18f, 0f, 0f);
        Parts.Add("KuyrukUcu", tail, MeshFactory.RoundedBox(new Vector3(0.092f, 0.027f, 0.03f), 0.008f), accent, new Vector3(0f, 0f, 0.045f), outline: false);
        // gaga ve gozler (kameraya bakan yuzde)
        Parts.Add("Gaga", body, MeshFactory.Sphere(0.022f, 6, 10), accent, new Vector3(0f, -0.015f, -0.165f), outline: false);
        foreach (float x in new[] { -0.055f, 0.055f })
        {
            var socket = Parts.Add("Goz", body, MeshFactory.RoundedCylinder(0.034f, 0.03f, 0.008f, 14), dark, new Vector3(x, 0.035f, -0.148f));
            socket.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Parts.Add("Mercek", socket, MeshFactory.Sphere(0.02f, 8, 12), eyeMat, new Vector3(0f, 0.012f, 0f), outline: false);
        }
        // kizaklar
        foreach (float x in new[] { -0.075f, 0.075f })
        {
            float bottom = -BodyRadius * 0.85f;
            Parts.Add("Kizak", body, MeshFactory.RoundedBox(new Vector3(0.02f, 0.016f, 0.24f), 0.007f), dark, new Vector3(x, bottom + 0.008f, 0f));
            Parts.Add("Ayak", body, MeshFactory.RoundedBox(new Vector3(0.014f, 0.07f, 0.014f), 0.005f), dark, new Vector3(x, bottom + 0.045f, 0f), outline: false);
        }
        // dort capraz kol + halkali pervane
        var missing = new System.Collections.Generic.List<GameObject>();
        for (int k = 0; k < 4; k++)
        {
            float a = (45f + 90f * k) * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            var arm = Parts.Add("Kol", body, MeshFactory.RoundedBox(new Vector3(0.2f, 0.024f, 0.03f), 0.01f), dark, dir * 0.17f + Vector3.up * 0.02f);
            arm.localRotation = Quaternion.Euler(0f, -(45f + 90f * k), 0f);
            var rotor = Parts.Empty("Pervane", body);
            rotor.localPosition = dir * 0.27f + Vector3.up * 0.045f;
            blades[k] = DronePart.BuildRotor(rotor, 0.085f, accent);
            // 54'te eksik: on sag kol ve pervanesi (k=3), arka sol pervane (k=1); parcalar 52-54'te toplaniyor
            if (k == 3) missing.Add(arm.gameObject);
            if (k == 3 || k == 1) missing.Add(rotor.gameObject);
        }
        missingWhenIncomplete = missing.ToArray();
        hole = Parts.Add("Delik", body, MeshFactory.Sphere(0.05f, 6, 10), dark, new Vector3(0.11f, 0.05f, 0.04f), outline: false).gameObject;
        hole.transform.localScale = new Vector3(0.5f, 1f, 1.2f);

        // ad: ust-on yuzde, cocuk eliyle hafif egik; yukaridan bakan kamera okur
        var label = WorldText.Create(body, "SERÇE", new Vector3(0f, 0.108f, -0.055f), 0.075f, CraterTraces.EceColor);
        label.transform.localRotation = Quaternion.Euler(52f, 0f, -7f);
    }

    public void SetState(SparrowState s)
    {
        state = s;
        foreach (var go in missingWhenIncomplete) go.SetActive(s != SparrowState.Incomplete);
        hole.SetActive(s == SparrowState.Incomplete);
        eyeMat.SetColor("_EmissionColor", s == SparrowState.Flying ? EyeOn : Color.black);
        body.localPosition = Vector3.zero;
        body.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (state != SparrowState.Flying) return;
        float t = Time.time + phase;
        float spin = 1400f * Time.deltaTime;
        foreach (var b in blades) b.Rotate(0f, spin, 0f, Space.Self);
        body.localPosition = new Vector3(0.01f * Mathf.Sin(t * 0.7f), 0.03f * Mathf.Sin(t * 2.1f), 0f);
        body.localRotation = Quaternion.Euler(3f * Mathf.Sin(t * 1.3f), 0f, 4f * Mathf.Sin(t * 0.9f + 1f));
    }
}
