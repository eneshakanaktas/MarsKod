using System.Collections;
using UnityEngine;

// Hedef kare: zeminde sicak renkli sabit halka + disa dogru atan ikinci halka, ustunde suzulen kucuk isaret tasi.
// Robot hedefe varip gorev bitince isaret kucuk bir "pop" ile kaybolur.
public class Target : MonoBehaviour
{
    static readonly Color Warm = Mats.Hex("#FFB36B");

    Transform marker;
    Material markMat, pulseMat;
    bool reached;

    public static Target Create(Transform parent, Vector3 localPos)
    {
        var go = new GameObject("Target");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var t = go.AddComponent<Target>();
        t.Build();
        return t;
    }

    void Build()
    {
        var quad = MeshFactory.Quad(new Vector2(1f, 1f));
        var ringMat = Mats.Custom("TargetRing", "MarsKod/Ring");
        ringMat.SetColor("_Color", Warm);
        ringMat.SetFloat("_R", 0.78f);
        ringMat.SetFloat("_W", 0.06f);
        ringMat.SetFloat("_A", 0.95f);
        Parts.Add("Ring", transform, quad, ringMat, new Vector3(0, 0.006f, 0), outline: false, castShadow: false);

        pulseMat = Mats.Custom("TargetPulse", "MarsKod/Ring");
        pulseMat.SetColor("_Color", Warm);
        Parts.Add("Pulse", transform, quad, pulseMat, new Vector3(0, 0.008f, 0), outline: false, castShadow: false);

        // isiktan bagimsiz duz renk: los safakta da parlak turuncu okunur (hologram isaret gibi)
        markMat = Mats.Unlit(Warm);
        marker = Parts.Empty("Marker", transform);
        // iki kristal ucu uca: yere bakan bir elmas
        var gem = MeshFactory.Crystal(0.12f, 0.03f, 0.17f);
        Parts.Add("Top", marker, gem, markMat, Vector3.zero, outline: false);
        Parts.Add("Bottom", marker, gem, markMat, Vector3.zero, outline: false).localRotation = Quaternion.Euler(180f, 0f, 0f);
    }

    void Update()
    {
        float t = Time.time;
        float p = (t * 0.6f) % 1f;
        pulseMat.SetFloat("_R", Mathf.Lerp(0.3f, 0.95f, Tween.OutCubic(p)));
        pulseMat.SetFloat("_W", 0.05f);
        pulseMat.SetFloat("_A", reached ? 0f : 0.7f * (1f - p));
        if (reached) return;
        marker.localPosition = new Vector3(0f, 0.7f + 0.05f * Mathf.Sin(t * 2.2f), 0f);
        marker.localRotation = Quaternion.Euler(0f, t * 50f, 0f);
        markMat.SetColor("_BaseColor", Color.Lerp(Warm, Mats.Hex("#FFE2B8"), 0.5f + 0.5f * Mathf.Sin(t * 3f)));
    }

    public void Reach() => StartCoroutine(ReachCo());

    IEnumerator ReachCo()
    {
        reached = true;
        yield return Tween.Run(0.1f, k => marker.localScale = Vector3.one * Mathf.Lerp(1f, 1.35f, Tween.OutCubic(k)));
        yield return Tween.Run(0.25f, k => marker.localScale = Vector3.one * Mathf.Lerp(1.35f, 0f, Tween.InCubic(k)));
        marker.gameObject.SetActive(false);
    }

    public void Restore()
    {
        StopAllCoroutines();
        reached = false;
        marker.gameObject.SetActive(true);
        marker.localScale = Vector3.one;
    }
}
