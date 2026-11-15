using System.Collections;
using UnityEngine;

// Kucuk buz kumesi: 4 kristal + buzlu taban. Toplaninca kucuk bir "pop" ve halka.
public class Ice : MonoBehaviour, IPickup
{
    Transform cluster, ring;
    Material mat, ringMat;
    Color baseEmission;
    float phase;
    bool popping;

    public static Ice Create(Transform parent, Vector3 localPos, int seed)
        => Create(parent, localPos, seed, Mats.Hex("#8FE3FF"), Mats.Hex("#3BB0E0") * 0.55f);

    // Tehlikeli kristal: ayni kume, kirmizi ve daha parlak (toplanmaz, uzerinden gecilir)
    public static Ice CreateHazard(Transform parent, Vector3 localPos, int seed)
        => Create(parent, localPos, seed, Mats.Hex("#FF5A4F"), Mats.Hex("#E02A2A") * 0.8f);

    // Tuz kristali (Bolge 9): sedef govde, altin parilti (renkler: Bolge 9 paleti)
    public static Ice CreateSalt(Transform parent, Vector3 localPos, int seed)
        => Create(parent, localPos, seed, Mats.Hex("#F4F1E8"), Mats.Hex("#E8D24A") * 0.45f);

    // Goktasi (Bolge 10): grafit govde, meteorun zumrut yesili parilti (gecici: kristal kumesi bicimi; asil tas 3a'da)
    public static Ice CreateMeteorStone(Transform parent, Vector3 localPos, int seed)
        => Create(parent, localPos, seed, Mats.Hex("#3A3A40"), Mats.Hex("#2EE86A") * 0.55f);

    static Ice Create(Transform parent, Vector3 localPos, int seed, Color body, Color glow)
    {
        var go = new GameObject("Ice");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 1.5f;
        var ice = go.AddComponent<Ice>();
        ice.Build(seed, body, glow);
        return ice;
    }

    void Build(int seed, Color body, Color glow)
    {
        phase = seed * 1.7f;
        baseEmission = glow;
        mat = Mats.Emissive(body, baseEmission, 0.9f);

        cluster = Parts.Empty("Cluster", transform);
        cluster.localRotation = Quaternion.Euler(0, seed * 47f, 0);

        var specs = new (float r, float body, float tip, Vector3 pos, Vector3 rot)[]
        {
            (0.085f, 0.21f, 0.10f, new Vector3(0f, 0.02f, 0f), new Vector3(0, 0, 0)),
            (0.065f, 0.13f, 0.08f, new Vector3(0.10f, 0.02f, 0.04f), new Vector3(0, 20, -24)),
            (0.055f, 0.09f, 0.06f, new Vector3(-0.09f, 0.02f, -0.05f), new Vector3(0, -10, 22)),
            (0.045f, 0.06f, 0.05f, new Vector3(0.02f, 0.02f, -0.11f), new Vector3(20, 0, 0)),
        };
        foreach (var s in specs)
            Parts.Add("Crystal", cluster, MeshFactory.Crystal(s.r, s.body, s.tip), mat, s.pos).localRotation = Quaternion.Euler(s.rot);

        ring = PickupFx.AddRing(transform, Mats.Hex("#D9F7FF"), out ringMat);
    }

    void Update()
    {
        if (popping) return;
        float t = Time.time + phase;
        cluster.localScale = Vector3.one * (1f + 0.018f * Mathf.Sin(t * 1.8f));
        mat.SetColor("_EmissionColor", baseEmission * (0.8f + 0.4f * (0.5f + 0.5f * Mathf.Sin(t * 1.3f))));
    }

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        popping = true;
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        yield return PickupFx.PopAway(cluster, 1.22f, 0.25f);
    }

    public void Restore()
    {
        StopAllCoroutines();
        popping = false;
        cluster.gameObject.SetActive(true);
        cluster.localScale = Vector3.one;
        cluster.localPosition = Vector3.zero;
        ring.gameObject.SetActive(false);
    }
}
