using System.Collections;
using System.IO;
using UnityEngine;

// Canli cevre denetimi (Ragip, 2026-11-19; docs/tasarim/canli-cevre.md). Hareket tek ekran goruntusunde gorunmez: bu mod her
// bolumu acip oyun kamerasindan kare serisi ceker (once bos sahne, sonra dogru cozum calisirken: adim tozu, toplama parıltısı,
// bitis kutlamasi). Kareler videoya/temas sayfasina cevrilip bakilir. Bekleme sirasindaki kare hizi log'a yazilir.
// MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -canli <ilk> <son> <mutlak klasor> [-adim <n>]
// -adim: kac bolumde bir (ornegin 1 100 -adim 10 -> 1, 11, 21 ... 91). Log'da "CANLI CEVRE: bolum=... fps=..." ve sonda
// "CANLI CEVRE DENETIMI: TAMAM (N bolum)". Oyuncu kaydina yazmaz (shotsMode).
public partial class Oyun
{
    const float LiveFrameGap = 0.25f, LiveOpeningGap = 0.12f, LiveIdleSeconds = 5f, LiveAfterSeconds = 2.5f;
    bool liveAuditing;   // -canli: denetim modunda da bolum girisi (LevelArrival) oynar

    IEnumerator LiveAudit(int first, int last, int every, string dir)
    {
        shotsMode = suppressQuiz = true;
        liveAuditing = true;   // bolum girisi gosterisi de cekilsin
        ListenWhileUnfocused();
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2f);
        int count = 0;
        for (int i = 0; i < levels.Count; i++)
        {
            int n = levels[i].Number;
            if (n < first || n > last || (n - first) % every != 0) continue;
            string b = Path.Combine(dir, "c" + n.ToString("000") + "-");
            LoadLevel(i);
            yield return null;
            hud.CloseInfo();   // bilgi karti hemen kapanir: bolum girisi gosterisi bastan cekilsin
            hud.HideInfoBubble();

            int frame = 0, ticks = 0;
            float started = Time.realtimeSinceStartup;
            for (float t = 0f; t < LiveIdleSeconds; )
            {
                ScreenCapture.CaptureScreenshot(b + (frame++).ToString("000") + ".png", 1);
                float gap = t < 3.5f ? LiveOpeningGap : LiveFrameGap;   // giris gosterisi sirasinda daha sik
                t += gap;
                float until = Time.time + gap;
                while (Time.time < until) { ticks++; yield return null; }
            }
            float fps = ticks / Mathf.Max(0.01f, Time.realtimeSinceStartup - started);

            SetCode(levels[i].Solution);
            yield return new WaitForSeconds(0.3f);
            OnRun();
            float after = -1f;
            while (after < 0f || Time.time < after)
            {
                if (!running && after < 0f) after = Time.time + LiveAfterSeconds;
                ScreenCapture.CaptureScreenshot(b + (frame++).ToString("000") + ".png", 1);
                yield return new WaitForSeconds(LiveFrameGap);
                if (frame > 400) break;   // takilan bir cozum sonsuza dek cekmesin
            }
            Debug.Log("CANLI CEVRE: bolum=" + n + " kare=" + frame + " fps=" + fps.ToString("0"));
            count++;
        }
        Debug.Log("CANLI CEVRE DENETIMI: TAMAM (" + count + " bolum)");
        yield return new WaitForSeconds(0.3f);
        Application.Quit();
    }

    // -canli <ilk> <son> <klasor> [-adim <n>]
    void StartLiveAuditIfAsked(string[] a)
    {
        int every = 1;
        for (int i = 0; i < a.Length - 1; i++)
            if (a[i] == "-adim") int.TryParse(a[i + 1], out every);
        for (int i = 0; i < a.Length - 3; i++)
            if (a[i] == "-canli" && int.TryParse(a[i + 1], out int first) && int.TryParse(a[i + 2], out int last))
                StartCoroutine(LiveAudit(first, last, Mathf.Max(1, every), a[i + 3]));
    }
}
