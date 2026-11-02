using System.Collections;
using UnityEngine;

// Nehir cakili (haritada C, Bolge 7): suyun yuvarlattigi bir avuc cakil. Uc-dort basik, duzgun, acik renkli tas (krem, gri,
// soluk pembe) ust uste yigilmis; yuzeyleri hafif parlak (islakken cilalanmis gibi). Golgede secilsin diye en buyugu yavasca
// soluk krem parlar. Toplaninca sicrayip kaybolur, yerde acik renkli halka yayilir.
public class RiverPebble : MonoBehaviour, IPickup
{
    public static readonly Color Cream = Mats.Hex("#EADFCC");   // sayac simgesi de bu renk
    static readonly Color[] Tints = { Cream, Mats.Hex("#C9C4BE"), Mats.Hex("#E3C3B6"), Mats.Hex("#D8CBB8") };
    static readonly Color Glow = Mats.Hex("#FFE9C8");
    const float RingScale = 1.85f;   // diger toplananlarin halkasi kadar

    // Kumenin taslari: yer (x, z), yaricap, basiklik. Ortadaki buyuk, yaninda iki orta, birinin ustunde kucuk.
    static readonly Vector4[] Stones =
    {
        new Vector4(0f, 0f, 0.13f, 0.55f),
        new Vector4(0.15f, 0.08f, 0.09f, 0.6f),
        new Vector4(-0.12f, 0.1f, 0.08f, 0.62f),
        new Vector4(0.06f, -0.13f, 0.07f, 0.65f),
    };

    Transform body, ring;
    Material glowMat, ringMat;
    float phase;
    bool popping;

    public static RiverPebble Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("RiverPebble");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var pebble = go.AddComponent<RiverPebble>();
        pebble.Build(seed);
        return pebble;
    }

    void Build(int seed)
    {
        phase = seed * 1.3f;
        body = Parts.Empty("Body", transform);
        body.localRotation = Quaternion.Euler(0f, seed * 67f, 0f);
        glowMat = Mats.Emissive(Tints[0], Glow * 0.08f, 0.7f);
        for (int k = 0; k < Stones.Length; k++)
        {
            var s = Stones[k];
            float r = s.z, flat = s.w;
            var mat = k == 0 ? glowMat : Mats.Lit(Tints[(k + seed) % Tints.Length], 0.65f);
            float y = r * flat * 0.85f + (k == 3 ? 0.05f : 0f);   // sonuncusu buyugun kenarina yaslanir
            var stone = Parts.Add("Cakil", body, MeshFactory.Cobble(r, seed * 7 + k), mat, new Vector3(s.x, y, s.y));
            stone.localScale = new Vector3(1f, flat, 0.85f + 0.1f * k);
            stone.localRotation = Quaternion.Euler(k == 3 ? 14f : 0f, k * 47f + seed * 13f, 0f);
        }
        ring = PickupFx.AddRing(transform, Mats.Hex("#FFF1DA"), out ringMat);
        ring.localScale = Vector3.one * RingScale;
    }

    void Update()
    {
        if (popping) return;
        float t = Time.time + phase;
        glowMat.SetColor("_EmissionColor", Glow * (0.05f + 0.1f * (0.5f + 0.5f * Mathf.Sin(t * 1.2f))));
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
