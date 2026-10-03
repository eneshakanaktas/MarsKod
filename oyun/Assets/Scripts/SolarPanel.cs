using System.Collections;
using MarsKod.Dunya;
using UnityEngine;

// Gunes paneli (Bolge 3): kisa ayak uzerinde, guneye (kameraya) egik koyu mavi cam; camda hucre cizgileri.
// Camin on kenarinda 10 kucuk guc isigi: yanan isik sayisi = guc / 10 (gucu 30 olan panelde 3 isik), sagslamda yesil, catlakta turuncu.
// Catlak panelin caminda beyaz catlak cizgileri; onarilinca catlaklar kaybolur, isiklar birer birer yesil dolar.
// Kirik panel (gucu 0) toplanacak parcadir (IPickup): yere devrilmis, cami kararmis, yaninda kirik cam parcalari, altinda turuncu hale.
public class SolarPanel : MonoBehaviour, IPickup
{
    const int Lights = 10;
    static readonly Color Healthy = Mats.Hex("#7CF07A");
    static readonly Color Weak = Mats.Hex("#FFA53A");
    static readonly Color Off = Mats.Hex("#1B2027");
    static readonly Color BrokenGlow = Mats.Hex("#FFB25E");

    int startPower, power;
    Transform body, cracks, ring, halo;
    Material ringMat, haloMat, glassMat;
    readonly Material[] lightMats = new Material[Lights];
    bool popping;

    public static SolarPanel Create(Transform parent, Vector3 localPos, int power, int seed)
    {
        var go = new GameObject(power == 0 ? "BrokenPanel" : "SolarPanel");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var panel = go.AddComponent<SolarPanel>();
        panel.startPower = panel.power = power;
        panel.Build(seed);
        return panel;
    }

    bool Broken => startPower == 0;

    void Build(int seed)
    {
        var metal = Mats.Lit(Mats.Hex("#6B7079"), 0.4f);
        var frameMat = Mats.Lit(Mats.Hex("#3A3F48"), 0.45f);
        var lineMat = Mats.Unlit(Mats.Hex("#5F7FA8"));
        glassMat = Broken ? Mats.Lit(Mats.Hex("#1A2232"), 0.5f) : Mats.Emissive(Mats.Hex("#21407A"), Mats.Hex("#0E2148"), 0.85f);

        body = Parts.Empty("Body", transform);
        body.localRotation = Quaternion.Euler(0f, Broken ? seed * 53f % 60f - 30f : 0f, 0f);

        // ayak ve egik tabla (kirik panelde ayak yikik, tabla yere devrik)
        var tilt = Parts.Empty("Tilt", body);
        if (Broken)
        {
            tilt.localPosition = new Vector3(0f, 0.06f, 0.02f);
            tilt.localRotation = Quaternion.Euler(-12f, 0f, 14f);
            Parts.Add("Post", body, MeshFactory.RoundedBox(new Vector3(0.05f, 0.05f, 0.24f), 0.012f), metal, new Vector3(0.24f, 0.03f, 0.2f))
                .localRotation = Quaternion.Euler(0f, 35f, 0f);
        }
        else
        {
            Parts.Add("Post", body, MeshFactory.RoundedBox(new Vector3(0.06f, 0.2f, 0.06f), 0.015f), metal, new Vector3(0f, 0.1f, 0.06f));
            tilt.localPosition = new Vector3(0f, 0.22f, 0f);
            tilt.localRotation = Quaternion.Euler(-28f, 0f, 0f); // on kenar (guney) asagida: cam kameraya bakar
        }

        var size = new Vector3(0.74f, 0.035f, 0.5f);
        Parts.Add("Frame", tilt, MeshFactory.RoundedBox(size, 0.015f), frameMat, Vector3.zero);
        float top = size.y * 0.5f + 0.004f;
        Parts.Add("Glass", tilt, MeshFactory.RoundedBox(new Vector3(0.68f, 0.012f, 0.4f), 0.004f), glassMat, new Vector3(0f, top, 0.025f), outline: false);
        // hucre cizgileri: 3 dik + 1 yatay
        for (int k = 1; k <= 3; k++)
            Parts.Add("CellLine", tilt, MeshFactory.RoundedBox(new Vector3(0.008f, 0.004f, 0.4f), 0.002f), lineMat,
                new Vector3(-0.34f + k * 0.17f, top + 0.008f, 0.025f), outline: false, castShadow: false);
        Parts.Add("CellLine", tilt, MeshFactory.RoundedBox(new Vector3(0.68f, 0.004f, 0.008f), 0.002f), lineMat,
            new Vector3(0f, top + 0.008f, 0.025f), outline: false, castShadow: false);

        // guc isiklari: camin on (guney) kenarinda, cercevenin ustunde
        for (int k = 0; k < Lights; k++)
        {
            lightMats[k] = Mats.Emissive(Off, Color.black, 0.6f);
            Parts.Add("PowerLight", tilt, MeshFactory.RoundedBox(new Vector3(0.05f, 0.012f, 0.03f), 0.004f), lightMats[k],
                new Vector3(-0.3f + k * (0.6f / (Lights - 1)), top + 0.002f, -0.215f), outline: false, castShadow: false);
        }

        // catlaklar: camin ustunde bir noktadan dagilan beyaz cizgiler (catlak ve kirik panelde)
        cracks = Parts.Empty("Cracks", tilt);
        cracks.localPosition = new Vector3(0.08f, top + 0.011f, 0.06f);
        var crackMat = Mats.Unlit(Broken ? Mats.Hex("#C9D3E0") : Mats.Hex("#F2F6FF"));
        float[] angles = { 15f, 80f, 150f, 215f, 290f };
        float[] lengths = { 0.2f, 0.13f, 0.18f, 0.12f, 0.16f };
        for (int k = 0; k < angles.Length; k++)
        {
            var crack = Parts.Add("Crack", cracks, MeshFactory.RoundedBox(new Vector3(lengths[k], 0.004f, 0.012f), 0.002f), crackMat,
                Quaternion.Euler(0f, angles[k], 0f) * new Vector3(lengths[k] * 0.5f, 0f, 0f), outline: false, castShadow: false);
            crack.localRotation = Quaternion.Euler(0f, angles[k], 0f);
        }

        if (Broken)
        {
            // yere sacilmis cam parcalari + altta turuncu hale (toplanacak oldugu belli olsun)
            var shard = Mats.Lit(Mats.Hex("#2B3A55"), 0.8f);
            Parts.Add("Shard", transform, MeshFactory.RoundedBox(new Vector3(0.12f, 0.012f, 0.08f), 0.004f), shard, new Vector3(-0.3f, 0.01f, -0.22f))
                .localRotation = Quaternion.Euler(0f, 30f, 0f);
            Parts.Add("Shard", transform, MeshFactory.RoundedBox(new Vector3(0.08f, 0.012f, 0.06f), 0.004f), shard, new Vector3(-0.18f, 0.01f, -0.32f))
                .localRotation = Quaternion.Euler(0f, -20f, 0f);
            halo = PickupFx.AddRing(transform, BrokenGlow, out haloMat);
            haloMat.SetFloat("_R", 0f);
            haloMat.SetFloat("_W", 0.46f);
            haloMat.SetFloat("_A", 0.28f);
            halo.gameObject.SetActive(true);
            ring = PickupFx.AddRing(transform, BrokenGlow, out ringMat);
        }
        ShowPower(power);
    }

    void Update()
    {
        if (popping || !Broken) return;
        haloMat.SetFloat("_A", 0.2f + 0.1f * (0.5f + 0.5f * Mathf.Sin(Time.time * 2f)));
    }

    void ShowPower(int p)
    {
        int lit = p / PowerPerLight;
        var on = World.IsCracked(p) ? Weak : Healthy;
        for (int k = 0; k < Lights; k++)
        {
            lightMats[k].SetColor("_BaseColor", k < lit ? on : Off);
            lightMats[k].SetColor("_EmissionColor", k < lit ? on * 1.6f : Color.black);
        }
        cracks.gameObject.SetActive(p < World.CrackedBelow);
    }

    const int PowerPerLight = World.RepairedPower / Lights;

    /// <summary>panel_power() ile olculdu: isiklar bir an parlar.</summary>
    public IEnumerator Flash()
    {
        int lit = power / PowerPerLight;
        var on = World.IsCracked(power) ? Weak : Healthy;
        yield return Tween.Run(0.4f, t =>
        {
            float boost = 1.6f + 2.4f * Mathf.Sin(t * Mathf.PI);
            for (int k = 0; k < lit; k++) lightMats[k].SetColor("_EmissionColor", on * boost);
        });
    }

    /// <summary>Catlak panel onarildi: catlaklar kuculup kaybolur, isiklar birer birer yesil dolar, cam bir an parlar.</summary>
    public IEnumerator Repair()
    {
        int from = power / PowerPerLight;
        power = World.RepairedPower;
        var glass = glassMat.GetColor("_EmissionColor");
        yield return Tween.Run(0.25f, t => cracks.localScale = Vector3.one * (1f - t));
        cracks.gameObject.SetActive(false);
        cracks.localScale = Vector3.one;
        for (int k = 0; k < Lights; k++)
        {
            lightMats[k].SetColor("_BaseColor", Healthy);
            lightMats[k].SetColor("_EmissionColor", Healthy * 1.6f);
            if (k >= from) yield return Tween.Wait(0.035f);
        }
        yield return Tween.Run(0.3f, t => glassMat.SetColor("_EmissionColor", Color.Lerp(glass * 4f, glass, t)));
    }

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        popping = true;
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        halo.gameObject.SetActive(false);
        yield return PickupFx.PopAway(body, 1.15f, 0.25f);
        foreach (Transform child in transform)
            if (child.name == "Shard") child.gameObject.SetActive(false);
    }

    public void Restore()
    {
        StopAllCoroutines();
        popping = false;
        power = startPower;
        body.gameObject.SetActive(true);
        body.localScale = Vector3.one;
        body.localPosition = Vector3.zero;
        cracks.localScale = Vector3.one;
        foreach (Transform child in transform)
            if (child.name == "Shard") child.gameObject.SetActive(true);
        if (Broken)
        {
            ring.gameObject.SetActive(false);
            halo.gameObject.SetActive(true);
        }
        ShowPower(power);
    }
}
