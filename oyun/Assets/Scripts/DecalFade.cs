using UnityEngine;

// Zemine yatik desenin (MarsKod/Decal) gorunurlugu: Show yumusakca belirtir, ShowNow hemen, Hide gizler. Bolge 7'de
// yurudukce beliren izler (TrailView).
public class DecalFade : MonoBehaviour
{
    const float Duration = 0.45f;
    Material mat;
    float t = -1f;   // < 0: belirme yok
    float duration = Duration;

    public void Init(Material m) => mat = m;

    // seconds: belirme suresi (varsayilan hizli; izler kum acilirken yavasca belirsin diye uzun verilir)
    public void Show(float seconds = Duration)
    {
        if (gameObject.activeSelf) return;
        gameObject.SetActive(true);
        t = 0f;
        duration = seconds;
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
        t += Time.deltaTime / duration;
        mat.SetFloat("_A", Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
        if (t >= 1f) t = -1f;
    }
}
