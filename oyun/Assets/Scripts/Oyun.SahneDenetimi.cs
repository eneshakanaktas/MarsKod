using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Bolge 6 sahnelerinin goruntu denetimi (-shots <klasor> -sahneler): sahnelerin ara kareleri + SAHNE DENETIMI satiri.
public partial class Oyun
{
    IEnumerator SceneShots(string dir)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2.5f);
        var problems = new List<string>();
        yield return RiftShots(dir, problems);
        yield return DescentShots(dir, problems);
        yield return ShotKeyboardCheck(dir);
        Debug.Log("SAHNE DENETIMI: " + (problems.Count == 0 ? "TAMAM" : "HATA " + string.Join("; ", problems)));
        Application.Quit();
    }

    // 60: normal bakis, yariga bakis, Kivilcim ve Serce raftayken (boy olcusu), dibin kenarindan inmeden once ve sonra
    IEnumerator RiftShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "s60-0-normal.png"));
        yield return new WaitForSeconds(0.4f);   // goruntu kare sonunda alinir: sonraki degisiklik ona karismasin
        var rift = CanyonTraces.Rift;
        if (rift == null) { problems.Add("60'ta yarik yok"); yield break; }
        if (rift.BendIndex <= 1 || rift.BendIndex >= rift.Trail.Count) problems.Add("donemec noktasi patikada degil");

        shot.Begin();
        shot.Cut(rift.ViewPoint, Quaternion.LookRotation(rift.ViewTarget - rift.ViewPoint), CanyonRift.ViewFov);
        yield return new WaitForSeconds(1.5f);   // arka plan yeniden cizilsin
        Cap(Path.Combine(dir, "s60-1-asagi.png"));
        yield return new WaitForSeconds(0.4f);

        // Boy olcusu: Kivilcim rafin ortasinda, Serce ustunde; sonra donemecten once ve sonra (sonrakinde gorunmemeli)
        yield return PoseOnTrail(rift, rift.Trail.Count / 3, dir, "s60-2-raf.png");
        yield return PoseOnTrail(rift, rift.BendIndex - 1, dir, "s60-3-donemec-once.png");
        yield return PoseOnTrail(rift, rift.BendIndex, dir, "s60-4-donemec-sonra.png");
        shot.Release();
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));   // robot ve Serce yerine
        yield return new WaitForSeconds(0.5f);
    }

    // 60'in kapanis sahnesi: dogru cozumu calistirir, evreler gelince kare alir; sonra sahne ortasinda "Tekrar" geri alma denetimi
    IEnumerator DescentShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        SetCode(level.Solution);
        OnRun();
        yield return new WaitForSeconds(0.5f);
        hud.FocusCode(0, 0);   // klavye acik: sahne basinda kapanmali (kareler klavyesiz olmali)
        foreach (var (phase, wait, file) in new[] { ("yol", 0f, "s60-5-yol.png"), ("bakis", 1.5f, "s60-6-bakis.png"), ("inis", 1.5f, "s60-7-inis.png"), ("son", 1f, "s60-8-son.png") })
        {
            yield return WaitForPhase(phase, problems);
            yield return new WaitForSeconds(wait);
            Cap(Path.Combine(dir, file));
            yield return new WaitForSeconds(0.4f);
        }

        // Geri alma: sahne ortasinda "Tekrar" her seyi yerine koymali
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("yol", problems);
        yield return WaitForPhase("bakis", problems);
        yield return WaitForPhase("inis", problems);
        ResetLevel();
        yield return null;
        if (shot.Active) problems.Add("Tekrar: sahne kamerasi birakilmadi");
        if (!robot.gameObject.activeSelf) problems.Add("Tekrar: robot gizli kaldi");
        if ((robot.transform.localPosition - Pos(level.Robot)).sqrMagnitude > 1e-4f) problems.Add("Tekrar: robot baslangic karesinde degil");
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null || !sparrow.gameObject.activeSelf) problems.Add("Tekrar: Serce yok ya da gizli");
        else if ((sparrow.transform.localPosition - sparrow.HomePosition).sqrMagnitude > 1e-4f) problems.Add("Tekrar: Serce evine donmedi");
        yield return new WaitForSeconds(0.1f);   // 0,5 sn sonra denetim ortaminda (sahnesiz bolumlerde de) toz topu buyuk gorunuyor: bkz. ILERLEME.md
        Cap(Path.Combine(dir, "s60-9-tekrar.png"));
    }

    // Sahnenin evresi gelene kadar bekler (en cok 40 sn)
    IEnumerator WaitForPhase(string phase, List<string> problems)
    {
        for (float t = 0f; canyonScenes.Phase != phase; t += Time.deltaTime)
        {
            if (t > 40f) { problems.Add("60: " + phase + " evresi gelmedi (su an: '"+canyonScenes.Phase+"')"); yield break; }
            yield return null;
        }
    }

    IEnumerator PoseOnTrail(CanyonRift rift, int index, string dir, string file)
    {
        var at = rift.transform.TransformPoint(rift.Trail[index]);
        var next = rift.transform.TransformPoint(rift.Trail[Mathf.Min(index + 1, rift.Trail.Count - 1)]);
        var ahead = next - at; ahead.y = 0f;
        robot.transform.position = at;
        if (ahead.sqrMagnitude > 1e-6f) robot.transform.rotation = Quaternion.LookRotation(ahead);
        if (CanyonTraces.ActiveSparrow != null) CanyonTraces.ActiveSparrow.transform.position = at + Vector3.up * 1.1f;
        yield return new WaitForSeconds(0.6f);
        Cap(Path.Combine(dir, file));
        yield return new WaitForSeconds(0.4f);
    }

    // Klavye acikken (fotograf kucultulmus) sahne kamerasi: Begin'de gokyuzu sicramamali, donuste ilk hale gelmeli
    IEnumerator ShotKeyboardCheck(string dir)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        hud.FocusCode(0, 0);
        yield return new WaitForSeconds(1.2f);
        Cap(Path.Combine(dir, "s60-k0-klavye.png"));
        yield return new WaitForSeconds(0.4f);
        shot.Begin();
        yield return null;
        Cap(Path.Combine(dir, "s60-k1-begin.png"));   // k0 ile ayni olmali
        yield return new WaitForSeconds(0.4f);
        var rift = CanyonTraces.Rift;
        yield return shot.MoveTo(rift.ViewPoint, rift.ViewTarget, 1.2f, CanyonRift.ViewFov);
        Cap(Path.Combine(dir, "s60-k2-klavye-yarik.png"));
        yield return new WaitForSeconds(0.4f);
        yield return shot.Return(1.2f);
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "s60-k3-klavye-donus.png"));   // k0 ile ayni olmali
        yield return new WaitForSeconds(0.4f);
        hud.StopEditing();
    }
}
