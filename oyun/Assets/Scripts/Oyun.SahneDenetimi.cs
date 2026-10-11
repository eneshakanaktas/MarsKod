using System.Collections;
using System.IO;
using UnityEngine;

// Bolge 6 sahnelerinin goruntu denetimi (-shots <klasor> -sahneler): sahnelerin ara kareleri + SAHNE DENETIMI satiri.
public partial class Oyun
{
    IEnumerator SceneShots(string dir)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2.5f);
        var problems = new System.Collections.Generic.List<string>();
        yield return RiftShots(dir, problems);
        Debug.Log("SAHNE DENETIMI: " + (problems.Count == 0 ? "TAMAM" : "HATA " + string.Join("; ", problems)));
        Application.Quit();
    }

    IEnumerator RiftShots(string dir, System.Collections.Generic.List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "s60-0-normal.png"));
        yield return new WaitForSeconds(0.4f);   // goruntu kare sonunda alinir: sonraki degisiklik ona karismasin
        if (CanyonTraces.Rift == null) { problems.Add("60'ta yarik yok"); yield break; }
        shot.Begin();
        var rift = CanyonTraces.Rift;
        shot.Cut(rift.ViewPoint, Quaternion.LookRotation(rift.ViewTarget - rift.ViewPoint), CanyonRift.ViewFov);
        yield return new WaitForSeconds(1.5f);   // arka plan yeniden cizilsin
        Cap(Path.Combine(dir, "s60-1-asagi.png"));
        yield return new WaitForSeconds(0.4f);   // goruntu kare sonunda alinir
        shot.Release();
    }
}
