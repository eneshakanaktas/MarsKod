using System.Collections;
using UnityEngine;

// Toz bulutu (Bolge 4): karenin cevresinde donen kum renkli, kenar cizgili kabarik topaklar (kucuk bir toz hortumu)
// ve yerde yumusak bir toz hali. Robot ortada gorunur kalir. Robot icindeyken yuruyemez (World.DustAt);
// her wait() ile topaklar kuculur, son beklemede dagilir. Seffaf degil (telefonda ucuz); yalnizca yerdeki hale seffaf.
public class DustCloud : MonoBehaviour
{
    const int PuffCount = 10;
    const float HazeAlpha = 0.55f;
    // Dagilmamis bulut en az bu kadar buyuk kalir (son beklemelerde de "hala toz var" belli olsun)
    const float MinDensity = 0.4f;
    static readonly Color SandLow = Mats.Hex("#C99467");
    static readonly Color SandHigh = Mats.Hex("#F3D5AA");

    Transform spinner, haze;
    Transform[] puffs;
    Vector3[] home;
    float[] size;
    Material hazeMat;
    float density = 1f;
    float phase;

    public static DustCloud Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("DustCloud");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var cloud = go.AddComponent<DustCloud>();
        cloud.Build(seed);
        return cloud;
    }

    void Build(int seed)
    {
        phase = seed * 2.1f;
        var rng = new System.Random(seed * 7919 + 13);
        var sphere = MeshFactory.Sphere(1f, 8, 12);
        spinner = Parts.Empty("Spinner", transform);
        puffs = new Transform[PuffCount];
        home = new Vector3[PuffCount];
        size = new float[PuffCount];
        for (int i = 0; i < PuffCount; i++)
        {
            float angle = i * Mathf.PI * 2f / PuffCount + (float)rng.NextDouble() * 0.4f;
            float height = (float)rng.NextDouble();
            float radius = 0.24f + 0.08f * (float)rng.NextDouble() + 0.06f * height; // yukari dogru acilir
            home[i] = new Vector3(Mathf.Cos(angle) * radius, 0.08f + 0.5f * height, Mathf.Sin(angle) * radius);
            size[i] = Mathf.Lerp(0.15f, 0.09f, height) * (0.85f + 0.3f * (float)rng.NextDouble());
            var mat = Mats.Lit(Color.Lerp(SandLow, SandHigh, height), 0.1f);
            puffs[i] = Parts.Add("Puff", spinner, sphere, mat, home[i], castShadow: false);
        }

        // yerde yumusak toz hali (Ring ciziminin yaricapi 0 hali: ortasi dolu, kenara dogru solar)
        hazeMat = Mats.Custom("Ring", "MarsKod/Ring");
        hazeMat.SetColor("_Color", SandHigh);
        hazeMat.SetFloat("_R", 0f);
        hazeMat.SetFloat("_W", 0.55f);
        hazeMat.SetFloat("_A", HazeAlpha);
        haze = Parts.Add("Haze", transform, MeshFactory.Quad(Vector2.one * 1.1f), hazeMat, new Vector3(0f, 0.012f, 0f), outline: false, castShadow: false);
    }

    void Update()
    {
        float t = Time.time + phase;
        spinner.localRotation = Quaternion.Euler(0f, t * 35f, 0f);
        float grow = density;
        for (int i = 0; i < PuffCount; i++)
        {
            float breathe = 1f + 0.08f * Mathf.Sin(t * 2.1f + i * 1.3f);
            puffs[i].localPosition = home[i] + new Vector3(0f, 0.02f * Mathf.Sin(t * 1.7f + i), 0f);
            puffs[i].localScale = Vector3.one * size[i] * grow * breathe;
        }
        hazeMat.SetFloat("_A", HazeAlpha * density);
    }

    // Robot bir tur bekledi: bulut kalan bekleme oranina gore kuculur; kalan 0 ise tamamen dagilir.
    public IEnumerator Thin(int left, int total)
    {
        float from = density;
        float to = left <= 0 ? 0f : Mathf.Lerp(MinDensity, 1f, (float)left / total);
        yield return Tween.Run(0.4f, t => density = Mathf.Lerp(from, to, Tween.OutCubic(t)));
        if (left <= 0) gameObject.SetActive(false);
    }

    public void Restore()
    {
        StopAllCoroutines();
        density = 1f;
        gameObject.SetActive(true);
    }
}
