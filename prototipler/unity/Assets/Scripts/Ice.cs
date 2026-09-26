using System.Collections;
using UnityEngine;

// Kucuk buz kumesi: 4 kristal + buzlu taban. Toplaninca kucuk bir "pop" ve halka.
public class Ice : MonoBehaviour
{
    Transform cluster, ring;
    Material mat, ringMat;
    Color baseEmission;
    float phase;
    bool popping;

    public static Ice Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("Ice");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 1.5f;
        var ice = go.AddComponent<Ice>();
        ice.Build(seed);
        return ice;
    }

    void Build(int seed)
    {
        phase = seed * 1.7f;
        baseEmission = Mats.Hex("#3BB0E0") * 0.55f;
        mat = Mats.Emissive(Mats.Hex("#8FE3FF"), baseEmission, 0.9f);

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

        ringMat = Mats.Custom("Ring", "MarsKod/Ring");
        ringMat.SetColor("_Color", Mats.Hex("#D9F7FF"));
        ring = Parts.Add("Ring", transform, MeshFactory.Quad(new Vector2(0.8f, 0.8f)), ringMat, new Vector3(0, 0.008f, 0), outline: false, castShadow: false);
        ring.gameObject.SetActive(false);
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
        StartCoroutine(RingCo());
        yield return Tween.Run(0.1f, t => cluster.localScale = Vector3.one * Mathf.Lerp(1f, 1.22f, Tween.OutCubic(t)));
        yield return Tween.Run(0.22f, t =>
        {
            float e = Tween.InCubic(t);
            cluster.localScale = Vector3.one * Mathf.Lerp(1.22f, 0f, e);
            cluster.localPosition = new Vector3(0, 0.25f * Tween.OutCubic(t), 0);
        });
        cluster.gameObject.SetActive(false);
    }

    IEnumerator RingCo()
    {
        ring.gameObject.SetActive(true);
        yield return Tween.Run(0.5f, t =>
        {
            float e = Tween.OutCubic(t);
            ringMat.SetFloat("_R", Mathf.Lerp(0.2f, 0.95f, e));
            ringMat.SetFloat("_W", Mathf.Lerp(0.05f, 0.09f, e));
            ringMat.SetFloat("_A", 0.9f * (1f - t));
        });
        ring.gameObject.SetActive(false);
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
