using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// "Atla" dugmesinin denetimi (Bolge 8 · 3b, K4; -sahneler turunda): her sahne normal akista (kod cozulunce) baslar, dugme 1 sn sonra
// acilir, ustundeki saniye azalir; dokununca sahne durur, dugme kapanir ve sahnenin son hali kurulur. 51 LandingShots'ta, 76/78/80
// Oyun.IstasyonDenetimi.cs'te denetlenir. Bolge 1 kapanisi 3 sn'den kisa: dugme yok.
public partial class Oyun
{
    IEnumerator SkipChecks(string dir, List<string> problems, bool riverOnly)
    {
        yield return SkipCheck(dir, "63", RiverBedTraces.BirdViewLevel, riverScenes.Skip, problems, () =>
            riverScenes.Phase != "son" ? "evre 'son' degil" : shot.Active ? "kamera birakilmadi" : SparrowAwayFromHome());
        yield return SkipCheck(dir, "68", RiverBedTraces.CrayonLevel, riverScenes.Skip, problems, () =>
            riverScenes.Phase != "son" ? "evre 'son' degil" : !HasCrayon() ? "sirtta kalem yok"
            : RiverBedTraces.Crayon != null && RiverBedTraces.Crayon.gameObject.activeSelf ? "yerdeki kalem gizlenmedi" : shot.Active ? "kamera birakilmadi" : null);
        if (riverOnly) yield break;
        yield return SkipCheck(dir, "55", CanyonTraces.SparrowRepairLevel, canyonScenes.Skip, problems, () =>
            canyonScenes.Phase != "son" ? "evre 'son' degil" : shot.Active ? "kamera birakilmadi"
            : CanyonTraces.ActiveSparrow == null || (CanyonTraces.ActiveSparrow.transform.localPosition - CanyonTraces.SparrowHover).sqrMagnitude > 1e-4f ? "Serce yerinde degil" : null);
        yield return SkipCheck(dir, "60", CanyonTraces.LastLevel, canyonScenes.Skip, problems, () =>
            canyonScenes.Phase != "son" ? "evre 'son' degil" : robot.gameObject.activeSelf ? "robot yarikta (gizli) degil" : !shot.Active ? "kamera yariga bakmiyor" : null);
        yield return SkipCheck(dir, "20", Bolge2SonBolum, SkipClosing, problems, () => FacesWithin(270f));
        yield return SkipCheck(dir, "30", Bolge3SonBolum, SkipClosing, problems, () => FacesWithin(90f));
        yield return SkipCheck(dir, "40", Bolge4SonBolum, SkipClosing, problems, () =>
            !HasRibbon() ? "sirtta kurdele yok" : Traces.Dune == null ? "pusula yok" : FacesWithin(Traces.Dune.YawToAntenna(robot.transform.position)));
        yield return SkipCheck(dir, "50", Bolge5SonBolum, SkipClosing, problems, () =>
            Traces.Antenna == null ? "anten yok" : nameTag != null ? "ad etiketi kaldi" : FacesWithin(DuneFinale.YawTowards(robot.transform.position, Traces.Antenna.Foot)));
    }

    // Bolumu cozer; sahne dugmesi acilinca saniyenin azaldigini olcer, dokunur (skip), son hali endProblem ile denetler (null: dogru)
    IEnumerator SkipCheck(string dir, string label, int number, Action skip, List<string> problems, Func<string> endProblem)
    {
        LoadLevel(levels.FindIndex(l => l.Number == number));
        if (level.Info != null) hud.CloseInfo();
        yield return new WaitForSeconds(0.8f);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        for (float t = 0f; !hud.SkipButtonOpen && running && t < 60f; t += Time.deltaTime) yield return null;
        if (!hud.SkipButtonOpen) { problems.Add(label + " atla: dugme acilmadi"); while (running) yield return null; yield break; }
        string first = hud.SkipSecondsText;
        yield return new WaitForSeconds(1.1f);
        if (hud.SkipButtonOpen && hud.SkipSecondsText == first) problems.Add(label + " atla: kalan saniye azalmadi (" + first + ")");
        Cap(Path.Combine(dir, "atla-" + label + "-dugme.png"));
        yield return null;
        skip();
        yield return null;
        yield return null;
        if (hud.SkipButtonOpen) problems.Add(label + " atla: dugme kapanmadi");
        for (float t = 0f; running && t < 20f; t += Time.deltaTime) yield return null;
        if (running) problems.Add(label + " atla: bolum bitmedi");
        yield return new WaitForSeconds(0.3f);
        var problem = endProblem();
        if (problem != null) problems.Add(label + " atla: " + problem);
        if (!hud.CardVisible) problems.Add(label + " atla: kod karti gizli");
        Cap(Path.Combine(dir, "atla-" + label + "-son.png"));
        yield return null;
    }

    string FacesWithin(float yaw) =>
        Mathf.Abs(Mathf.DeltaAngle(robot.Yaw, yaw)) > 5f ? "robot " + yaw.ToString("F0") + " dereceye donuk degil (" + robot.Yaw.ToString("F0") + ")" : null;

    string SparrowAwayFromHome()
    {
        var sparrow = RiverBedTraces.ActiveSparrow;
        return sparrow != null && (sparrow.transform.localPosition - sparrow.HomePosition).sqrMagnitude > 1e-4f ? "Serce evinde degil" : null;
    }
}
