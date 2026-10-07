using System.Collections;
using UnityEngine;

// Pusula parcasi (Bolge 4): firtinada dagilmis yon bulma direginin parcasi. Kuma egik dusmus pirinc kadran,
// ustunde kirmizi-beyaz ibre (yavasca sallanir, kirmizi ucu hafif parlar) ve yaninda kirik bir direk cubugu.
// Toplaninca sicrayip kaybolur, yerde sicak renkli halka yayilir.
public class CompassPart : MonoBehaviour, IPickup
{
    static readonly Color NeedleGlow = Mats.Hex("#FF5A3C");

    Transform body, needle, ring;
    Material needleMat, ringMat;
    float phase;
    bool popping;

    public static CompassPart Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("CompassPart");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 1.85f;
        var part = go.AddComponent<CompassPart>();
        part.Build(seed);
        return part;
    }

    void Build(int seed)
    {
        phase = seed * 1.9f;
        var brass = Mats.Lit(Mats.Hex("#C9A25A"), 0.6f);
        var face = Mats.Lit(Mats.Hex("#2B2A30"), 0.5f);
        var white = Mats.Lit(Mats.Hex("#E9E6DF"), 0.4f);
        var rod = Mats.Lit(Mats.Hex("#6B6F78"), 0.45f);
        needleMat = Mats.Emissive(Mats.Hex("#E8473B"), NeedleGlow * 0.5f, 0.5f);

        body = Parts.Empty("Body", transform);
        body.localRotation = Quaternion.Euler(0f, seed * 47f, 0f);

        // kadran kuma egik dusmus: bir kenari gomulu
        var dial = Parts.Empty("Dial", body);
        dial.localPosition = new Vector3(0f, 0.03f, 0f);
        dial.localRotation = Quaternion.Euler(14f, 0f, 0f);
        Parts.Add("Rim", dial, MeshFactory.RoundedCylinder(0.11f, 0.03f, 0.01f), brass, Vector3.zero);
        Parts.Add("Face", dial, MeshFactory.RoundedCylinder(0.088f, 0.034f, 0.006f), face, Vector3.zero, outline: false);
        needle = Parts.Empty("Needle", dial);
        needle.localPosition = new Vector3(0f, 0.022f, 0f);
        Parts.Add("North", needle, MeshFactory.RoundedBox(new Vector3(0.026f, 0.012f, 0.075f), 0.005f), needleMat, new Vector3(0f, 0f, 0.037f), outline: false);
        Parts.Add("South", needle, MeshFactory.RoundedBox(new Vector3(0.026f, 0.012f, 0.075f), 0.005f), white, new Vector3(0f, 0f, -0.037f), outline: false);
        Parts.Add("Pin", needle, MeshFactory.Sphere(0.014f, 6, 10), brass, new Vector3(0f, 0.008f, 0f), outline: false);

        // diregin kirik cubugu, kadranin yaninda kuma yatmis
        var bar = Parts.Add("Rod", body, MeshFactory.RoundedBox(new Vector3(0.028f, 0.028f, 0.2f), 0.008f), rod, new Vector3(0.13f, 0.016f, -0.02f));
        bar.localRotation = Quaternion.Euler(0f, 28f, 6f);

        ring = PickupFx.AddRing(transform, Mats.Hex("#FFD49A"), out ringMat);
    }

    void Update()
    {
        if (popping) return;
        float t = Time.time + phase;
        needle.localRotation = Quaternion.Euler(0f, 18f * Mathf.Sin(t * 0.9f) + 6f * Mathf.Sin(t * 2.3f), 0f);
        needleMat.SetColor("_EmissionColor", NeedleGlow * (0.35f + 0.3f * (0.5f + 0.5f * Mathf.Sin(t * 1.6f))));
    }

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        popping = true;
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        yield return PickupFx.PopAway(body, 1.22f, 0.25f);
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
