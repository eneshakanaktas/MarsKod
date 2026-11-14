using System;
using System.Collections;
using UnityEngine;

// Bolum 51'in roket inisi (spec 3b.4): 51 her yuklendiginde (Enes istedi: ilk acista degil, her giriste) oynar; "Atla" dugmesiyle atlanir.
// Sahnenin kendisi CanyonScenes.Landing'te (atlama LandingWithSkip); burada ne zaman oynayacagi ve bitis (giris damgasi) var.
public partial class Oyun
{
    Coroutine landing;

    bool LandingDue => !shotsMode && level.Number == CanyonTraces.FirstLevel;

    void StartLanding() => landing = StartCoroutine(PlayLanding());

    IEnumerator PlayLanding()
    {
        yield return canyonScenes.LandingWithSkip(level);
        landing = null;   // sahne bitti ya da atlandi: bolumun giris damgasi gelir
        hud.ShowIntro(level.Intro);
    }

    // Sahne sirasinda bolum degisirse (LoadLevel): sahne durur (dugmeyi CanyonScenes.Restore kapatir)
    void CancelLanding()
    {
        if (landing != null) StopCoroutine(landing);
        landing = null;
    }
}
