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
        yield return FlightShots(dir, problems);
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

        RiftCollisions(rift, problems);
        shot.Begin();
        shot.Cut(rift.ViewPoint, Quaternion.LookRotation(rift.ViewTarget - rift.ViewPoint), CanyonRift.ViewFov);
        yield return new WaitForSeconds(1.5f);   // arka plan yeniden cizilsin
        Cap(Path.Combine(dir, "s60-1-asagi.png"));
        yield return new WaitForSeconds(0.4f);

        // Boy olcusu: Kivilcim rafin ortasinda, Serce ustunde; sonra donemecten once ve sonra (sonrakinde gorunmemeli)
        yield return PoseOnTrail(rift, 1, dir, "s60-2a-bas.png");   // yarigin basi: kayalar robota/Serce'ye degmemeli
        yield return PoseOnTrail(rift, 3, dir, "s60-2b-bas.png");
        yield return PoseOnTrail(rift, rift.Trail.Count / 3, dir, "s60-2-raf.png");
        yield return PoseOnTrail(rift, rift.BendIndex - 1, dir, "s60-3-donemec-once.png");
        yield return PoseOnTrail(rift, rift.BendIndex, dir, "s60-4-donemec-sonra.png");
        shot.Release();
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));   // robot ve Serce yerine
        yield return new WaitForSeconds(0.5f);
    }

    // Robot ve Serce yol boyunca (her ~10 cm) yerlestirilir; govdelerinin hicbir parcasi yarigin duvarina, rafina (dudak, alt yuz) ya da
    // kayalarina girmemeli (kaya artik yok). Yarik aglarina gecici MeshCollider eklenir, her parcanin kutusu (kuculterek) sorgulanir. Yalniz ilk
    // birkac ihlal yazilir: hangi nesne, yarigin neresinde (t) ve hangi parca.
    void RiftCollisions(CanyonRift rift, List<string> problems)
    {
        var temporary = new List<MeshCollider>();
        foreach (var mf in rift.GetComponentsInChildren<MeshFilter>(true))
        {
            var c = mf.gameObject.AddComponent<MeshCollider>();
            c.sharedMesh = mf.sharedMesh;
            temporary.Add(c);
        }
        Physics.SyncTransforms();
        var walls = new HashSet<Collider>(temporary);
        var sparrow = CanyonTraces.ActiveSparrow;
        var robotParts = robot.GetComponentsInChildren<Renderer>();
        var sparrowParts = sparrow != null ? sparrow.GetComponentsInChildren<Renderer>() : new Renderer[0];
        int reported = 0;

        string Hit(Renderer[] parts, bool ignoreFloor, Vector3 around)
        {
            foreach (var part in parts)
            {
                if (!part.enabled || !part.gameObject.activeInHierarchy) continue;
                var b = part.bounds;
                if (b.extents.y < 0.03f || part.name == "Thruster") continue;   // ince halka, golge; ayak jetleri robotun yere basma yeri (duvar mesh'inin yatay seridi zemin seviyesinde kalir)
                foreach (var hit in Physics.OverlapBox(b.center, b.extents * 0.8f))
                {
                    if (!walls.Contains(hit)) continue;
                    if (ignoreFloor && (hit.name == "Patika" || hit.name == "Dip")) continue;   // robotun bastigi yer
                    var off = rift.transform.InverseTransformVector(b.center - around);   // parcanin yol noktasina gore yeri (yarik yerelinde)
                    // Duvar parcaya hangi yonden giriyor: dort yatay ve iki dikey yonde ilk duvar mesafesi (cm)
                    var sides = "";
                    foreach (var (label, dir) in new[] { ("+x", Vector3.right), ("-x", Vector3.left), ("+z", Vector3.forward), ("-z", Vector3.back), ("+y", Vector3.up), ("-y", Vector3.down) })
                        if (Physics.Raycast(b.center, dir, out var ray, 1.2f) && walls.Contains(ray.collider)) sides += " " + label + ":" + ray.collider.name.Replace("Duvar-", "D") + "@" + Mathf.RoundToInt(ray.distance * 100f);
                    return hit.name + " <- " + part.name + " (x " + off.x.ToString("+0.00;-0.00") + " y " + off.y.ToString("+0.00;-0.00") + ")" + sides;
                }
            }
            return null;
        }

        for (int i = 0; i < rift.Trail.Count - 1 && reported < 40; i++)
            for (float u = 0f; u < 1f && reported < 40; u += 0.2f)
            {
                var local = Vector3.Lerp(rift.Trail[i], rift.Trail[i + 1], u);
                var ahead = rift.transform.TransformPoint(rift.Trail[i + 1]) - rift.transform.TransformPoint(rift.Trail[i]);
                ahead.y = 0f;
                robot.transform.position = rift.transform.TransformPoint(local);
                if (ahead.sqrMagnitude > 1e-6f) robot.transform.rotation = Quaternion.LookRotation(ahead);
                if (sparrow != null) sparrow.transform.position = rift.transform.TransformPoint(rift.AirBeside(local));
                var robotHit = Hit(robotParts, ignoreFloor: true, around: robot.transform.position);
                var sparrowHit = Hit(sparrowParts, ignoreFloor: false, around: sparrow != null ? sparrow.transform.position : Vector3.zero);
                if (robotHit != null) { problems.Add("yarik: robot t=" + (local.z - rift.Trail[0].z).ToString("0.0") + " icine giriyor: " + robotHit); reported++; }
                if (sparrowHit != null) { problems.Add("yarik: Serce t=" + (local.z - rift.Trail[0].z).ToString("0.0") + " icine giriyor: " + sparrowHit); reported++; }
            }
        // Yarikta kaya yok: hicbir 'Kaya' nesnesi kalmamali
        foreach (var r in rift.GetComponentsInChildren<Renderer>(true))
            if (r.name == "Kaya" || r.name == "Tas") problems.Add("yarik: kaya var (" + r.name + ")");
        foreach (var c in temporary) Destroy(c);
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

    // 55'in sahnesi: dogru cozumu calistirir, evreler gelince kare alir; sonra sahne ortasinda "Tekrar" geri alma denetimi
    IEnumerator FlightShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.SparrowRepairLevel));
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "s55-0-onceki.png"));
        yield return new WaitForSeconds(0.4f);
        yield return SparrowCloseup(dir, "s55-6-yakin.png");
        SetCode(level.Solution);
        OnRun();
        hud.FocusCode(0, 0);   // klavye acik: gercek oyunda kullanici kodu klavyeyle yazar; sahne kamerasi yine dogru olmali
        foreach (var (phase, wait, file) in new[] { ("uyanis", 1f, "s55-1-uyanis.png"), ("kalkis", 1.5f, "s55-2-kalkis.png"), ("kus", 1f, "s55-3-yukaridan.png"), ("son", 0.5f, "s55-4-son.png") })
        {
            yield return WaitForPhase(phase, problems);
            yield return new WaitForSeconds(wait);
            Cap(Path.Combine(dir, file));
            yield return new WaitForSeconds(0.4f);
        }

        // Geri alma: kus evresinde "Tekrar" kamerayi birakmali, Serce rafta sonuk kalmali
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.SparrowRepairLevel));
        yield return new WaitForSeconds(1f);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("kus", problems);
        ResetLevel();
        yield return null;
        if (shot.Active) problems.Add("55 Tekrar: sahne kamerasi birakilmadi");
        if (!robot.gameObject.activeSelf) problems.Add("55 Tekrar: robot gizli kaldi");
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null) problems.Add("55 Tekrar: Serce yok");
        else if ((sparrow.transform.localPosition - sparrow.HomePosition).sqrMagnitude > 1e-4f) problems.Add("55 Tekrar: Serce rafina donmedi");
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "s55-5-tekrar.png"));
    }

    // Serce'nin yakin plani (adi okunuyor mu): oyun kamerasi gibi onden ve yukaridan
    IEnumerator SparrowCloseup(string dir, string file)
    {
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null) yield break;
        shot.Begin();
        shot.Cut(sparrow.transform.position + new Vector3(0f, 1.1f, -1.0f), Quaternion.LookRotation(new Vector3(0f, -1f, 1f)), 25f);
        yield return new WaitForSeconds(1.2f);
        Cap(Path.Combine(dir, file));
        yield return new WaitForSeconds(0.4f);
        shot.Release();
    }

    // Sahnenin evresi gelene kadar bekler (en cok 40 sn)
    IEnumerator WaitForPhase(string phase, List<string> problems)
    {
        for (float t = 0f; canyonScenes.Phase != phase; t += Time.deltaTime)
        {
            if (t > 40f) { problems.Add(phase + " evresi gelmedi (su an: '" + canyonScenes.Phase + "')"); yield break; }
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
