using System.Collections;
using UnityEngine;

// Ece'nin sac kurdelesi (Bolge 4, docs/tasarim/senaryo-bolge-04.md): son isaret diregine bagli, ruzgarda dalgalanir.
// Bolum 40 kapanisinda ruzgar onu cozer, havada savrulup robota gelir. Yeniden denemede diregine geri baglanir.
public class WindRibbon : MonoBehaviour
{
    Transform home, tailA, tailB;
    Vector3 homePos;
    bool flying;

    public static WindRibbon Create(Transform pole, Vector3 localPos)
    {
        var go = new GameObject("Iz-Kurdele");
        go.transform.SetParent(pole, false);
        go.transform.localPosition = localPos;
        var ribbon = go.AddComponent<WindRibbon>();
        ribbon.home = pole;
        ribbon.homePos = localPos;
        ribbon.Build();
        return ribbon;
    }

    // Dugum + ruzgar yonune (dogu) savrulan iki uzun uc. Uclar kok noktalarindan doner.
    void Build()
    {
        var cloth = Mats.Lit(CraterTraces.EceColor, 0.25f);
        Parts.Add("Dugum", transform, MeshFactory.RoundedBox(new Vector3(0.06f, 0.05f, 0.06f), 0.018f), cloth, Vector3.zero, outline: false);
        tailA = Tail(cloth, 0.26f, 0.016f);
        tailB = Tail(cloth, 0.2f, -0.016f);
    }

    Transform Tail(Material cloth, float length, float y)
    {
        var root = Parts.Empty("Uc", transform);
        root.localPosition = new Vector3(0.02f, y, 0f);
        Parts.Add("Kumas", root, MeshFactory.RoundedBox(new Vector3(length, 0.038f, 0.006f), 0.003f), cloth, new Vector3(length * 0.5f, 0f, 0f), outline: false, castShadow: false);
        return root;
    }

    void Update()
    {
        float t = Time.time;
        float speed = flying ? 14f : 6f;
        tailA.localRotation = Quaternion.Euler(0f, 25f * Mathf.Sin(t * speed), -12f + 10f * Mathf.Sin(t * speed * 0.7f));
        tailB.localRotation = Quaternion.Euler(0f, 25f * Mathf.Sin(t * speed + 1.3f), -28f + 10f * Mathf.Sin(t * speed * 0.8f + 0.5f));
    }

    // Ruzgar cozer: once yukari savrulur, sonra kavis cizerek hedefe (robotun sirti) iner ve kaybolur.
    public IEnumerator FlyTo(System.Func<Vector3> target, float dur = 1.3f)
    {
        flying = true;
        transform.SetParent(home.parent, true);
        Vector3 from = transform.position;
        yield return Tween.Run(dur, t =>
        {
            Vector3 to = target();
            Vector3 peak = Vector3.Lerp(from, to, 0.4f) + Vector3.up * 0.9f;
            float e = Tween.InOutCubic(t);
            transform.position = Vector3.Lerp(Vector3.Lerp(from, peak, e), Vector3.Lerp(peak, to, e), e);
            transform.rotation = Quaternion.Euler(30f * Mathf.Sin(t * 9f), 400f * t, 20f * Mathf.Cos(t * 7f));
        });
        gameObject.SetActive(false);
    }

    public void Restore()
    {
        StopAllCoroutines();
        flying = false;
        gameObject.SetActive(true);
        transform.SetParent(home, false);
        transform.localPosition = homePos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
