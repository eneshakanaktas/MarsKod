using System.Collections;
using UnityEngine;

// Roket yere degince kalkan toz: 12 kati yumak (saydam sis degil) yere yakin disa dogru yayilir, hafifce yukselip alcalir,
// kuculerek kaybolur ve kendini siler. Rengi zemindeki kum rengi.
public class LandingDust : MonoBehaviour
{
    const int PuffCount = 18;
    const float Seconds = 2.5f, StartRadius = 0.3f, EndRadius = 1.1f;

    Transform[] puffs;
    float[] angles, sizes, reaches;   // reaches: her yumagin yayilma orani (halka degil, dolgun bulut olsun)
    float reach;   // yayilma payi: buyuk roket daha genis toz kaldirir

    public static void Play(Transform parent, Vector3 localPos, float reach = 1f)
    {
        var go = new GameObject("Inis-Tozu");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var dust = go.AddComponent<LandingDust>();
        dust.reach = reach;
        dust.Build();
        dust.StartCoroutine(dust.Animate());
    }

    void Build()
    {
        var mat = Mats.Lit(Mats.Hex("#A5694B"), 0.1f);   // zeminin toz tonu: yumaklar zeminden kopuk gorunmesin
        var mesh = MeshFactory.Sphere(0.5f, 8, 12);
        var rnd = new System.Random(3);   // her inista ayni toz: goruntu turlari tekrarlanabilir
        puffs = new Transform[PuffCount];
        angles = new float[PuffCount];
        sizes = new float[PuffCount];
        reaches = new float[PuffCount];
        for (int i = 0; i < PuffCount; i++)
        {
            reaches[i] = 0.3f + 0.7f * (float)rnd.NextDouble();
            puffs[i] = Parts.Add("Yumak", transform, mesh, mat, Vector3.zero, outline: false, castShadow: false);
            angles[i] = (i + (float)rnd.NextDouble() * 0.6f) / PuffCount * Mathf.PI * 2f;
            sizes[i] = 0.3f + (float)rnd.NextDouble() * 0.2f;   // buyuk, ust uste binen yumaklar: tek bir toz bulutu
        }
    }

    IEnumerator Animate()
    {
        yield return Tween.Run(Seconds, t =>
        {
            float radius = Mathf.Lerp(StartRadius, EndRadius, Tween.OutCubic(t)) * reach;
            float rise = 0.15f * Mathf.Sin(t * Mathf.PI);   // yere yakin kalir, hafifce yukselip alcalir
            float size = Mathf.Sin(Mathf.Min(t * 3f, 1f) * Mathf.PI * 0.5f) * (1f - Tween.InCubic(t));   // ilk uc'te buyur, sonra kucul
            for (int i = 0; i < PuffCount; i++)
            {
                float diameter = sizes[i] * 2f * size;
                puffs[i].localPosition = new Vector3(Mathf.Cos(angles[i]) * radius * reaches[i], diameter * 0.35f + rise, Mathf.Sin(angles[i]) * radius * reaches[i]);
                puffs[i].localScale = Vector3.one * diameter;
            }
        });
        Destroy(gameObject);
    }
}
