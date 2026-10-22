using UnityEngine;
using UnityEngine.UIElements;

// Ekran kirpismasi (Bolge 6 sahneleri): tam ekran kara katman yarim surede kararir, yarim surede acilir.
// Kameranin bir yerden baska yere "kesilmesini" (55'te Serce'nin gozunden bakis) yumusatir. Dokunmayi engellemez.
public class ScreenBlink : VisualElement
{
    const long FrameMs = 16;
    IVisualElementScheduledItem running;

    public ScreenBlink()
    {
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
        style.backgroundColor = Color.black;
        style.opacity = 0f;
    }

    // Kirpisma bir kez oynar; yenisi gelirse eskisi kesilir
    public void Play(float seconds)
    {
        running?.Pause();
        float start = Time.unscaledTime;
        running = schedule.Execute(() =>
        {
            float t = Mathf.Clamp01((Time.unscaledTime - start) / Mathf.Max(seconds, 0.01f));
            style.opacity = 1f - Mathf.Abs(2f * t - 1f);
            if (t >= 1f) running.Pause();
        }).Every(FrameMs);
    }

    // Katmani hemen kaldirir ("Tekrar", bolum degisimi)
    public void Clear()
    {
        running?.Pause();
        style.opacity = 0f;
    }
}
