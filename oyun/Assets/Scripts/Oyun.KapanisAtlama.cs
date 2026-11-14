using System.Collections;
using MarsKod.Dunya;
using UnityEngine;

// Bolge 1-5 kapanislarinin "Atla" dugmesi (Bolge 8 · 3b, K4): sahne 1 sn sonra atlanabilir, atlaninca son hali kurulur (son program
// satiri, robotun yonu, sahnedeki izler). Bolge 1 kapanisi 3 sn'den kisa: dugme yok. Kapanislarin kendisi Oyun.cs'te.
public partial class Oyun
{
    SceneSkip closingSkip;

    // Tahmini uzunluklar (dugmenin ustundeki saniye): adimlarin surelerinin toplami; 50'de kaydin okunmasi dahil
    const float ClosingBolge2Length = 7.5f, ClosingBolge3Length = 9.5f, ClosingBolge4Length = 11f, ClosingBolge5Length = 40f;

    // Bolumun kapanis sahnesi (varsa) atlanabilir haliyle; kapanisi olmayan bolumde hicbir sey
    IEnumerator RegionClosing()
    {
        if (level.Number == Bolge1SonBolum) yield return ClosingSceneBolge1();   // kisa: dugmesiz
        if (level.Number == Bolge2SonBolum) yield return closingSkip.Run(ClosingSceneBolge2(), ClosingBolge2Length, FinishClosingBolge2);
        if (level.Number == Bolge3SonBolum) yield return closingSkip.Run(ClosingSceneBolge3(), ClosingBolge3Length, FinishClosingBolge3);
        if (level.Number == Bolge4SonBolum) yield return closingSkip.Run(ClosingSceneBolge4(), ClosingBolge4Length, FinishClosingBolge4);
        if (level.Number == Bolge5SonBolum) yield return closingSkip.Run(ClosingSceneBolge5(), ClosingBolge5Length, FinishClosingBolge5);
    }

    // Denetim: kosan kapanisi "Atla" ile keser
    void SkipClosing() => closingSkip.Skip();

    // Bolge 2: arac uzakta, robot bati yonune bakar, son satir (sera sulandi)
    void FinishClosingBolge2()
    {
        Sound.Hush();
        Traces.RoverDrivenAway();
        robot.ResetTo(robot.transform.localPosition, 270f);
        hud.SetOutro(Bolge2KapanisSatirlari[2]);
        Traces.SeraYapragiAcildi();
    }

    // Bolge 3: gun dogmus, robot dogudaki gunese bakar, son satir (paneller kendi isiklarini bitirir)
    void FinishClosingBolge3()
    {
        StartCoroutine(RegionLook.ForLevel(level.Number).Sunrise(skyLight, sunLight, 0f));   // sure 0: hemen biter
        robot.ResetTo(robot.transform.localPosition, 90f);
        hud.SetOutro(Bolge3KapanisSatirlari[2]);
    }

    // Bolge 4: ibre antene donuk, kurdele sirtta, robot antene bakar, son satir
    void FinishClosingBolge4()
    {
        var dune = Traces.Dune;
        if (dune == null) return;
        dune.Finished();
        robot.ResetTo(robot.transform.localPosition, dune.YawToAntenna(robot.transform.position));
        robot.SetRibbon(true);
        hud.SetOutro(Bolge4KapanisSatirlari[2]);
    }

    // Bolge 5: anten tam gucte, kayit dinlenmis sayilir (Kayitlar'da), robot antenin dibinde gokyuzune bakar, yeni ad ve baslik, son satir
    void FinishClosingBolge5()
    {
        var antenna = Traces.Antenna;
        if (antenna == null) return;
        Sound.Hush();
        RegionLook.ForLevel(level.Number).Apply(skyLight, sunLight);   // isik titremesinin ortasinda kesildiyse
        antenna.Powered();
        hud.HideRecording();
        MarkHeard(Recordings.ByNumber(1).Number);
        var foot = new Vector3(Pos(level.Target ?? level.Robot).x, TileTop, AreaHalfZ - 0.5f);
        robot.ResetTo(foot, 0f);
        robot.ResetTo(foot, DuneFinale.YawTowards(robot.transform.position, antenna.Foot));
        robot.StartCoroutine(robot.LookUp());
        if (nameTag != null) Destroy(nameTag.gameObject);   // ad etiketi sahnenin sonunda zaten kaybolur
        hud.RewriteChapter(Bolge5Baslik);
        hud.SetOutro(Bolge5SonSatir);
    }
}
