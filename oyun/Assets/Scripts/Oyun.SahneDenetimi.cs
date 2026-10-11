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
        yield return ShotKeyboardCheck(dir);
        Debug.Log("SAHNE DENETIMI: " + (problems.Count == 0 ? "TAMAM" : "HATA " + string.Join("; ", problems)));
        Application.Quit();
    }

    // 60: normal bakis, yariga bakis, Kivilcim ve Serce raftayken (boy olcusu), donemec kayasindan once ve sonra
    IEnumerator RiftShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "s60-0-normal.png"));
        LogRiftOnScreen("normal");
        yield return new WaitForSeconds(0.4f);   // goruntu kare sonunda alinir: sonraki degisiklik ona karismasin
        var rift = CanyonTraces.Rift;
        if (rift == null) { problems.Add("60'ta yarik yok"); yield break; }
        if (rift.BendIndex <= 1 || rift.BendIndex >= rift.Trail.Count) problems.Add("donemec noktasi patikada degil");

        // GECICI: kamera adaylari (geri, yukseklik, egim, gorus acisi)
        var candidates = new[] { (2.5f, 3.0f, 40f, 50f), (2.0f, 3.5f, 42f, 50f), (3.0f, 3.5f, 40f, 45f), (2.0f, 2.6f, 36f, 55f), (1.6f, 2.2f, 35f, 55f) };
        shot.Begin();
        for (int i = 0; i < candidates.Length; i++)
        {
            var (back, height, pitch, fov) = candidates[i];
            rift.SetView(back, height, pitch);
            shot.Cut(rift.ViewPoint, Quaternion.LookRotation(rift.ViewTarget - rift.ViewPoint), fov);
            yield return new WaitForSeconds(1.2f);
            Cap(Path.Combine(dir, "s60-aday-" + i + ".png"));
            LogRiftOnScreen("aday " + i);
            yield return new WaitForSeconds(0.4f);
        }
        rift.SetView();

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

    // GECICI: yarigin orta cizgisinin ve kenarlarinin gokyuzu resmindeki yeri (x: ortadan, en-boy olcekli; d: ufkun alti)
    void LogRiftOnScreen(string label)
    {
        var rift = CanyonTraces.Rift;
        var picture = Shader.GetGlobalVector("_Picture");
        float horizon = Shader.GetGlobalFloat("_Horizon"), aspect = (float)Screen.width / Screen.height;
        var sb = new System.Text.StringBuilder("YARIK EKRAN " + label + " H=" + horizon.ToString("F3") + ":");
        for (float t = 0f; t <= 12f; t += 1f)
        {
            float cx = CanyonRift.CenterX(t, CanyonTraces.DescentPathX), z = AreaHalfZ + CanyonTraces.DescentGap + t, hw = CanyonRift.HalfWidth(t);
            Vector2 P(float x)
            {
                var v = cam.WorldToViewportPoint(new Vector3(x, TerrainHeight(x, z), z));
                float px = (v.x * 2f - 1f) / picture.x * 0.5f + 0.5f, py = (v.y * 2f - 1f - picture.y) / picture.x * 0.5f + 0.5f;
                return new Vector2((px - 0.5f) * aspect, horizon - py);
            }
            Vector2 c = P(cx), l = P(cx - hw), r = P(cx + hw);
            sb.Append(" t" + t + "(" + c.x.ToString("F3") + "," + c.y.ToString("F3") + " w" + (r.x - l.x).ToString("F3") + ")");
        }
        Debug.Log(sb.ToString());
    }
}
