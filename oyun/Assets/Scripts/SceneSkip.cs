using System;
using System.Collections;
using UnityEngine;

// Atlanabilir sahne (Bolge 8 · 3b, K4; butun bolgelerin sahneleri): sahne baslayinca 1 sn sonra kucuk "Atla" dugmesi, ustunde kalan
// saniye (sahnenin bildirdigi uzunluktan gecen sure; en az 1). Ilk izleyisten itibaren atlanir. Dokunulursa sahne durur, finish son hali
// kurar (hikayeyi izlemeyen de izleri gorur). 3 sn'den kisa sahnede dugme yok.
public class SceneSkip
{
    const float ButtonDelay = 1f, MinLength = 3f;

    readonly MonoBehaviour host;
    readonly Hud hud;
    Coroutine playing;
    bool done, skipRequested;

    public SceneSkip(MonoBehaviour host, Hud hud)
    {
        this.host = host;
        this.hud = hud;
    }

    public IEnumerator Run(IEnumerator scene, float length, Action finish)
    {
        skipRequested = false;
        done = false;
        bool button = length >= MinLength, buttonShown = false;
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
        playing = null;
        hud.HideSkip();
    }

    // "Atla" dugmesi (denetim de cagirir)
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
