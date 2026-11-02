using UnityEngine;

// Zemine yatik desenin (MarsKod/Decal) gorunurlugu: Show yumusakca belirtir, ShowNow hemen, Hide gizler. Bolge 7'de
// yurudukce beliren izler (TrailView).
public class DecalFade : MonoBehaviour
{
    const float Duration = 0.45f;
    Material mat;
    float t = -1f;   // < 0: belirme yok

    public void Init(Material m) => mat = m;

    public void Show()
    {
        if (gameObject.activeSelf) return;
        gameObject.SetActive(true);
        t = 0f;
        mat.SetFloat("_A", 0f);
    }

    public void ShowNow()
    {
        gameObject.SetActive(true);
        t = -1f;
        mat.SetFloat("_A", 1f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        t = -1f;
    }

    void Update()
    {
        if (t < 0f) return;
        t += Time.deltaTime / Duration;
        mat.SetFloat("_A", Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
        if (t >= 1f) t = -1f;
    }
}
