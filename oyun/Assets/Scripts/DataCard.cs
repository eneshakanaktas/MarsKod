using System.Collections;
using UnityEngine;

// Veri karti (haritada V, Bolge 8): kalin, avuc ici boyunda bilgi kartusu, yere yatik. Koyu gri govde, ustunde koloni turuncusu
// etiket ve ince temas seridi, kosesinde yavasca yanip sonen kucuk yesil isik (los odada da secilsin diye altina soluk yesil leke).
// Toplaninca sicrayip kaybolur, yerde acik renkli halka yayilir.
public class DataCard : MonoBehaviour, IPickup
{
    public static readonly Color Label = StationRoom.ColonyOrange;   // sayac simgesi de bu renk
    static readonly Color Led = Mats.Hex("#5CFF8A");
    static readonly Vector3 Size = new Vector3(0.28f, 0.045f, 0.19f);   // telefonda secilsin diye avuc icinden biraz buyuk
    const float RingScale = 1.85f;

    Transform body, ring;
    Material ledMat, ringMat, poolMat;
    float phase;
    bool popping;

    public static IPickup Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("DataCard");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var card = go.AddComponent<DataCard>();
        card.Build(seed);
        return card;
    }

    void Build(int seed)
    {
        phase = seed * 1.7f;
        body = Parts.Empty("Body", transform);
        var tilt = Parts.Empty("Egik", body);
        tilt.localPosition = new Vector3(0f, Size.y * 0.5f + 0.01f, 0f);
        tilt.localRotation = Quaternion.Euler(-6f + 3f * (seed % 3), seed * 41f % 50f - 25f, 4f);   // yere yatik, hafif egri dusmus

        Parts.Add("Govde", tilt, MeshFactory.RoundedBox(Size, 0.012f), Mats.Lit(Mats.Hex("#3A3F47"), 0.5f), Vector3.zero);
        Parts.Add("Etiket", tilt, MeshFactory.RoundedBox(new Vector3(Size.x * 0.55f, 0.004f, Size.z * 0.62f), 0.002f, 1),
            Mats.Lit(Label, 0.3f), new Vector3(-0.025f, Size.y * 0.5f + 0.001f, 0f), outline: false, castShadow: false);
        Parts.Add("Temas", tilt, MeshFactory.RoundedBox(new Vector3(0.024f, 0.004f, Size.z * 0.7f), 0.002f, 1),
            Mats.Lit(Mats.Hex("#C9A24A"), 0.7f), new Vector3(Size.x * 0.5f - 0.03f, Size.y * 0.5f + 0.001f, 0f), outline: false, castShadow: false);
        ledMat = Mats.Emissive(Led, Led, 0.6f);
        Parts.Add("Isik", tilt, MeshFactory.Sphere(0.013f, 6, 10), ledMat, new Vector3(Size.x * 0.5f - 0.03f, Size.y * 0.5f + 0.006f, Size.z * 0.5f - 0.028f),
            outline: false, castShadow: false);
        poolMat = LightPool.Create(transform, Vector3.zero, new Vector2(0.5f, 0.42f), Led, 0.18f, "Kart-Isigi");

        ring = PickupFx.AddRing(transform, Mats.Hex("#E9FFE9"), out ringMat);
        ring.localScale = Vector3.one * RingScale;
    }

    void Update()
    {
        if (popping) return;
        float on = 0.5f + 0.5f * Mathf.Sin((Time.time + phase) * 2.2f);
        ledMat.SetColor("_EmissionColor", Led * (0.4f + 1.8f * on * on));
        poolMat.SetFloat("_A", 0.1f + 0.12f * on);
    }

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        popping = true;
        poolMat.SetFloat("_A", 0f);
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        yield return PickupFx.PopAway(body, 1.25f, 0.25f);
    }

    public void Restore()
    {
        StopAllCoroutines();
        popping = false;
        body.gameObject.SetActive(true);
        body.localScale = Vector3.one;
        body.localPosition = Vector3.zero;
        ring.gameObject.SetActive(false);
    }
}
