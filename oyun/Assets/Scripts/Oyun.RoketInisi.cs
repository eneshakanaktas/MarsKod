using System;
using System.Collections;
using UnityEngine;

// Bolum 51'in roket inisi (spec 3b.4): 51 her yuklendiginde (Enes istedi: ilk acista degil, her giriste) oynar; ekrana dokununca atlanir.
// Sahnenin kendisi CanyonScenes.Landing'te; burada ne zaman oynayacagi, atlatma ve bitis (giris damgasi) var.
public partial class Oyun
{
    Coroutine landing;

    bool LandingDue => !shotsMode && level.Number == CanyonTraces.FirstLevel;

    void StartLanding() => landing = StartCoroutine(PlayLanding());

    IEnumerator PlayLanding()
    {
        hud.ShowSkip(SkipLanding);
        yield return canyonScenes.Landing(level);
        EndLanding();
    }

    void SkipLanding()
    {
        if (landing != null) StopCoroutine(landing);
        canyonScenes.FinishLanding(level);
        EndLanding();
    }

    // Sahne bitti ya da atlandi: dokunma katmani kapanir, bolumun giris damgasi gelir
    void EndLanding()
    {
        landing = null;
        hud.HideSkip();
        hud.ShowIntro(level.Intro);
    }

    // Sahne sirasinda bolum degisirse (LoadLevel): sahne durur, katman kapanir
    void CancelLanding()
    {
        if (landing != null) StopCoroutine(landing);
        landing = null;
        hud.HideSkip();
    }
}
