using System.Collections;
using UnityEngine;

// Enerji hucresi (Bolge 1): ayakta duran pil kapsulu. Koyu metal taban ve baslik, aralarinda ince kafes cubuklari,
// ortada yesil-sari parlayan cekirdek; isik asagidan yukari yavasca dolup bosalir. Basligin ustunde yesil dolum halkasi
// (kamera yukaridan baktigi icin en cok o gorunur) ve kucuk kutup ucu; yerde yesil hale.
// Toplaninca cekirdek dolar ve parlar, kapsul sicrayip kaybolur, yerde yesil halka yayilir.
public class EnergyCell : MonoBehaviour, IPickup
{
    public static readonly Color Glow = Mats.Hex("#C8F25A");
    static readonly Color GlowEmission = Mats.Hex("#8FD63A");

    const float BaseH = 0.045f, CoreH = 0.22f, CapH = 0.035f;
    const float FillMin = 0.35f;   // bostayken cekirdegin dolu kismi

    Transform body, fill, ring, halo;
    Material coreMat, ringMat, haloMat;
    float phase;
    bool popping;

    public static EnergyCell Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("EnergyCell");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 1.5f;
        var cell = go.AddComponent<EnergyCell>();
        cell.Build(seed);
        return cell;
    }

    void Build(int seed)
    {
        phase = seed * 1.3f;
        var metal = Mats.Lit(Mats.Hex("#5A606A"), 0.45f);
        var bright = Mats.Lit(Mats.Hex("#B9BEC6"), 0.55f);
        var tube = Mats.Lit(Mats.Hex("#1F2A1E"), 0.7f);
        coreMat = Mats.Emissive(Glow, GlowEmission, 0.8f);

        body = Parts.Empty("Body", transform);
        body.localRotation = Quaternion.Euler(0, seed * 47f, 0);

        float coreY = BaseH;
        float capY = BaseH + CoreH + CapH * 0.5f;
        float capTop = capY + CapH * 0.5f;
        Parts.Add("Base", body, MeshFactory.RoundedCylinder(0.085f, BaseH, 0.015f), metal, new Vector3(0, BaseH * 0.5f, 0));
        Parts.Add("Cap", body, MeshFactory.RoundedCylinder(0.078f, CapH, 0.012f), metal, new Vector3(0, capY, 0));
        Parts.Add("ChargeRing", body, MeshFactory.RoundedCylinder(0.056f, 0.012f, 0.004f), coreMat, new Vector3(0, capTop, 0), outline: false);
        Parts.Add("Terminal", body, MeshFactory.RoundedCylinder(0.028f, 0.032f, 0.008f), bright, new Vector3(0, capTop + 0.012f, 0));
        // bos cam: koyu; dolu kisim (cekirdek) onun icinden asagidan yukari buyur
        Parts.Add("Tube", body, MeshFactory.RoundedCylinder(0.066f, CoreH, 0.004f), tube, new Vector3(0, coreY + CoreH * 0.5f, 0), outline: false);
        fill = Parts.Empty("Fill", body);
        fill.localPosition = new Vector3(0, coreY, 0);
        Parts.Add("Core", fill, MeshFactory.RoundedCylinder(0.07f, CoreH, 0.01f), coreMat, new Vector3(0, CoreH * 0.5f, 0), outline: false);
        // kafes cubuklari
        for (int k = 0; k < 3; k++)
        {
            float a = k * Mathf.PI * 2f / 3f;
            var pos = new Vector3(Mathf.Cos(a) * 0.076f, coreY + CoreH * 0.5f, Mathf.Sin(a) * 0.072f);
            Parts.Add("Rib", body, MeshFactory.RoundedBox(new Vector3(0.016f, CoreH, 0.016f), 0.006f), bright, pos);
        }

        // yerde yumusak yesil hale (genis, ortasi dolu halka)
        halo = PickupFx.AddRing(transform, Glow, out haloMat);
        haloMat.SetFloat("_R", 0f);
        haloMat.SetFloat("_W", 0.42f);
        halo.gameObject.SetActive(true);

        ring = PickupFx.AddRing(transform, Mats.Hex("#E4FFB0"), out ringMat);
    }

    void Update()
    {
        if (popping) return;
        float t = Time.time + phase;
        float level = 0.5f + 0.5f * Mathf.Sin(t * 1.1f);
        SetFill(Mathf.Lerp(FillMin, 1f, level));
        coreMat.SetColor("_EmissionColor", GlowEmission * (1.1f + 0.7f * level));
        haloMat.SetFloat("_A", 0.22f + 0.12f * level);
    }

    void SetFill(float amount) => fill.localScale = new Vector3(1f, amount, 1f);

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        popping = true;
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        float startFill = fill.localScale.y;
        // once cekirdek tamamen dolar ve beyaza yakin parlar, yerdeki hale soner
        yield return Tween.Run(0.12f, t =>
        {
            SetFill(Mathf.Lerp(startFill, 1f, t));
            coreMat.SetColor("_EmissionColor", Color.Lerp(GlowEmission * 1.8f, Color.white * 2f, t));
            haloMat.SetFloat("_A", Mathf.Lerp(0.3f, 0f, t));
        });
        yield return PickupFx.PopAway(body, 1.18f, 0.3f);
        halo.gameObject.SetActive(false);
    }

    public void Restore()
    {
        StopAllCoroutines();
        popping = false;
        body.gameObject.SetActive(true);
        body.localScale = Vector3.one;
        body.localPosition = Vector3.zero;
        ring.gameObject.SetActive(false);
        halo.gameObject.SetActive(true);
    }
}
