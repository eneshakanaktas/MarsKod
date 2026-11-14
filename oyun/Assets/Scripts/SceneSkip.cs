using System;
using System.Collections;
using UnityEngine;

// Atlanabilir sahne (Bolge 8 · 3b, K4): sahne baslayinca 1 sn sonra altta kucuk "Atla" dugmesi, ustunde kalan saniye (sahnenin
// bildirdigi uzunluktan gecen sure; en az 1). Dokunulursa sahne durur, finish son hali kurar (izi izlemeyen de gorur). 3 sn'den kisa
// sahnede dugme yok. Eskiden RiverBedScenes'in icindeydi (70).
// seenKey verilirse eski kural (70): ilk tam izleyiste atlanamaz, sonra tam ekran dokunma katmani. Gorev 5'te 70 de dugmeye gecer.
public class SceneSkip
{
    const float ButtonDelay = 1f, MinLength = 3f;

    readonly MonoBehaviour host;
    readonly Hud hud;
    readonly Func<bool> saveProgress;   // ilk izleyis kaydedilir mi (denetim modunda hayir)
    Coroutine playing;
    bool done, skipRequested;

    public SceneSkip(MonoBehaviour host, Hud hud, Func<bool> saveProgress)
    {
        this.host = host;
        this.hud = hud;
        this.saveProgress = saveProgress;
    }

    public IEnumerator Run(IEnumerator scene, float length, Action finish, string seenKey = null)
    {
        skipRequested = false;
        done = false;
        bool layer = seenKey != null && PlayerPrefs.GetInt(seenKey, 0) == 1;
        if (layer) hud.ShowSkip(Skip);
        bool button = seenKey == null && length >= MinLength, buttonShown = false;
        float start = Time.time;
        playing = host.StartCoroutine(Play(scene));
        while (!done && !skipRequested)
        {
            float passed = Time.time - start;
            if (button && passed >= ButtonDelay)
            {
                if (!buttonShown) { hud.ShowSkipButton(Skip); buttonShown = true; }
                hud.SetSkipSeconds(Mathf.Max(1, Mathf.CeilToInt(length - passed)));
            }
            yield return null;
        }
        if (!done)
        {
            host.StopCoroutine(playing);
            finish();
        }
        else if (seenKey != null && saveProgress())
        {
            PlayerPrefs.SetInt(seenKey, 1);
            PlayerPrefs.Save();
        }
        playing = null;
        hud.HideSkip();
    }

    // "Atla" dugmesi ya da dokunma katmani (denetim de cagirir)
    public void Skip() => skipRequested = true;

    // "Tekrar" ve bolum degisimi: kosan sahne durur (son hal kurulmaz; Restore'lar geri alir)
    public void Stop()
    {
        if (playing != null) host.StopCoroutine(playing);
        playing = null;
        hud.HideSkip();
    }

    IEnumerator Play(IEnumerator scene)
    {
        yield return scene;
        done = true;
    }
}
