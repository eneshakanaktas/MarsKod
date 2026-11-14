using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Bolge 8 · 3a goruntu ve tutarlilik denetimi (-shots <klasor> -sahneler -bolge8; docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md,
// Gorev 5): sahnedeki sayilar bitis damgalariyla eslesir (veri karti, bilgisayar, cizim, 24 cop adam), alan disindaki esyalar zemine basar
// ve oynanan karelere girmez, engel esyasi kendi karesinde, cizimler ve harita duvar yuzunun bandinda. Calistirinca olanlar ve "Tekrar"
// (bilgisayarlar soner, lambalar losa doner, kart kapanir, katlanmis cizim kalkar). Ara kareler s71-...png; sonunda SAHNE DENETIMI satiri.
public partial class Oyun
{
    const float GroundTolerance = 0.02f;   // alt sinir zemin yuksekliginin +-2 cm'inde
    const float WallBandBottom = 0.08f;     // StationRoom: supurgeligin ustu (acik panel bandi buradan PanelTop'a)

    // Alan disinda zemine basan istasyon esyalari (adlariyla)
    static readonly HashSet<string> StationFloorProps = new HashSet<string> { "Yatak-Ece", "Yatak-Defne", "Canta", "Tezgah", "YedekParca", "KorBacak", "Iz-Kasalar" };

    IEnumerator StationSceneShots(string dir)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2.5f);
        var problems = new List<string>();
        yield return StationChecks(dir, problems);
        Debug.Log("SAHNE DENETIMI (yalniz Bolge 8): " + (problems.Count == 0 ? "TAMAM" : "HATA " + string.Join("; ", problems)));
        Application.Quit();
    }

    IEnumerator StationChecks(string dir, List<string> problems)
    {
        for (int n = StationTraces.FirstLevel; n <= StationTraces.LastLevel; n++)
        {
            yield return LoadStationLevel(n);
            CheckStationFloor(n, problems);
        }
        yield return DoorCheck(dir, problems);
        yield return ScreensCheck(dir, problems);
        yield return DrawingsCheck(dir, problems, StationTraces.DrawingsFirstLevel, 3);
        yield return LightsCheck(problems);
        yield return DrawingsCheck(dir, problems, StationTraces.DrawingsSecondLevel, 5);
        yield return NoteCheck(dir, problems);
        yield return BackDrawingCheck(dir, problems);
        yield return HighlightCheck(problems);
        yield return WorkshopCheck(dir, problems);
        yield return MapCheck(dir, problems);
    }

    IEnumerator LoadStationLevel(int number)
    {
        LoadLevel(levels.FindIndex(l => l.Number == number));
        if (level.Info != null) hud.CloseInfo();
        yield return new WaitForSeconds(0.8f);
        if (level.Info != null) hud.HideInfoBubble();
    }

    // Dogru cozumu calistirir, bitene kadar bekler
    IEnumerator RunStationSolution()
    {
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        while (running) yield return null;
        yield return new WaitForSeconds(0.4f);
    }

    // Alan disindaki esyalar (ve ekranli bolumun masalari) zemine basar, oynanan karelere girmez; engel esyasi kendi karesinde
    void CheckStationFloor(int n, List<string> problems)
    {
        foreach (var t in levelRoot.GetComponentsInChildren<Transform>(false))
        {
            bool desk = t.name == "Masa" && t.parent != null && t.parent.name == "Bilgisayarlar";
            bool outside = StationFloorProps.Contains(t.name) || desk;
            bool obstacle = t.name == "Obstacle" && StationTraces.IsIndoor(n);   // 71'in kayalari zemine gomulu: denetlenmez
            if (!outside && !obstacle) continue;
            var b = MeshBounds(t, levelRoot);
            float g = TerrainHeight(b.center.x, b.center.z);
            if (Mathf.Abs(b.min.y - g) > GroundTolerance)
                problems.Add(n + ": " + t.name + " yere basmiyor (alt " + b.min.y.ToString("F3") + ", zemin " + g.ToString("F3") + ", " + b.center.ToString("F2") + ")");
            if (outside && OverlapsArea(b)) problems.Add(n + ": " + t.name + " oynanan alana giriyor (" + b.center.ToString("F2") + ")");
            if (obstacle && !InOneCell(b)) problems.Add(n + ": engel esyasi karesinden tasiyor (" + b.center.ToString("F2") + ")");
        }
    }

    // 71: iki veri karti; arma kapi modulunun catisinin icinde ve ustunde
    IEnumerator DoorCheck(string dir, List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.DoorLevel);
        if (level.Item != MarsKod.Dunya.Collectible.DataCard || pickups.Count != 2) problems.Add("71: veri karti " + pickups.Count);
        ExpectPart(problems, 71, "Veri kartı: " + pickups.Count);
        var crest = StationTraces.Crest;
        var facade = crest != null ? crest.parent.parent : null;   // arma -> arma yeri -> cephe
        Transform doorModule = null;
        if (facade != null)
            foreach (Transform c in facade)
                if (c.name == "Modul" && Mathf.Abs(c.localPosition.x) < 0.01f) doorModule = c;
        if (crest == null || doorModule == null) problems.Add("71: arma ya da kapi modulu yok");
        else
        {
            var a = MeshBounds(crest, facade);
            var m = MeshBounds(doorModule, facade);
            bool inside = a.min.x >= m.min.x && a.max.x <= m.max.x && a.min.z >= m.min.z && a.max.z <= m.max.z;
            if (!inside || Mathf.Abs(a.min.y - m.max.y) > GroundTolerance) problems.Add("71: arma catinin disinda (" + a.center.ToString("F2") + ")");
        }
        Cap(Path.Combine(dir, "s71-kapi.png"));
        yield return new WaitForSeconds(0.3f);
    }

    // 72: dogru cozumde 6 bilgisayar uyanir; "Tekrar" ile hepsi soner
    IEnumerator ScreensCheck(string dir, List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.ScreensLevel);
        var screens = StationTraces.Screens;
        if (screens == null) { problems.Add("72: bilgisayar yok"); yield break; }
        if (screens.AwakeCount != 0) problems.Add("72: basta uyanik bilgisayar " + screens.AwakeCount);
        yield return RunStationSolution();
        if (screens.AwakeCount != 6) problems.Add("72: uyanan bilgisayar " + screens.AwakeCount);
        ExpectPart(problems, 72, "Uyanan bilgisayar: " + screens.AwakeCount);
        Cap(Path.Combine(dir, "s72-ekranlar.png"));
        yield return new WaitForSeconds(0.3f);
        ResetLevel();
        if (screens.AwakeCount != 0) problems.Add("72 Tekrar: bilgisayarlar sonmedi (" + screens.AwakeCount + ")");
    }

    // 73 / 75: duvardaki cizim sayisi, 24 cop adam, cizimler duvar bandinin icinde
    IEnumerator DrawingsCheck(string dir, List<string> problems, int number, int expected)
    {
        yield return LoadStationLevel(number);
        var face = StationTraces.BackWallFace;
        if (face == null) { problems.Add(number + ": arka duvar yok"); yield break; }
        int drawings = 0;
        bool people = false, route = false;
        foreach (Transform c in face)
        {
            if (!c.name.StartsWith("Cizim-")) continue;
            if (c.name.Length > 6 && char.IsDigit(c.name[6])) drawings++;
            if (c.name.Contains("(" + WallDrawings.DescentPeople + " kisi)")) people = true;
            if (c.name == "Cizim-Rota") route = true;
            CheckOnWallBand(number + ": " + c.name, c, face, problems);
        }
        if (drawings != expected) problems.Add(number + ": duvarda " + drawings + " cizim");
        ExpectPart(problems, number, "Çizim: " + drawings);
        if (!people) problems.Add(number + ": inen insanlar " + WallDrawings.DescentPeople + " degil");
        if (number == StationTraces.DrawingsSecondLevel && !route) problems.Add(number + ": rota yazisi yok");
        if (StationTraces.Drawings == null || StationTraces.Drawings.Count != expected) problems.Add(number + ": cizim yerleri kayitli degil");
        Cap(Path.Combine(dir, "s" + number + "-cizimler.png"));
        yield return new WaitForSeconds(0.3f);
    }

    // 73: cozumden sonra lambalar yanik, acil isik sonuk; "Tekrar" ile losa doner
    IEnumerator LightsCheck(List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.LightsOnLevel);
        var room = StationTraces.Room;
        if (room == null) { problems.Add("73: oda yok"); yield break; }
        if (!room.EmergencyOn || LampGlow(room) > 0.1f) problems.Add("73: basta lambalar yanik");
        yield return RunStationSolution();
        yield return new WaitForSeconds(1.5f);
        if (room.EmergencyOn || LampGlow(room) < 1f) problems.Add("73: cozumden sonra lambalar yanmadi");
        ResetLevel();
        if (!room.EmergencyOn || LampGlow(room) > 0.1f) problems.Add("73 Tekrar: lambalar losa donmedi");
    }

    static float LampGlow(StationRoom.Room room)
    {
        float min = float.MaxValue;
        foreach (var lamp in room.Lamps) min = Mathf.Min(min, lamp.sharedMaterial.GetColor("_EmissionColor").maxColorComponent);
        return min;
    }

    // 76: her not surumunde kart son harfe kadar ilerler, katlanmis cizim notun son karesinde belirir ve yere basar; "Tekrar" ile kalkar
    IEnumerator NoteCheck(string dir, List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.FoldedLevel);
        if (BackDrawingShown()) problems.Add("76: sirtta katlanmis cizim var (bulunmadan once)");
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        var versions = new HashSet<int>();
        for (int run = 0; run < level.Notes.Count; run++)
        {
            if (run > 0) ResetLevel();   // "Tekrar": bitince Calistir dugmesi "Sonraki bolum" olur
            OnRun();
            int lastLetter = -1;
            bool captured = run > 0;
            while (running)
            {
                lastLetter = Mathf.Max(lastLetter, hud.NoteLetter);
                if (!captured && hud.NoteLetter == 0)
                {
                    captured = true;
                    Cap(Path.Combine(dir, "s76-not.png"));   // kod calisirken ilk adim: kart acik, ilk harf vurgulu
                }
                yield return null;
            }
            yield return new WaitForSeconds(1.0f);
            int v = level.NoteRoutes.FindIndex(r => (Flat(Pos(r.End)) - Flat(robot.transform.localPosition)).sqrMagnitude < 0.01f);
            if (v < 0) { problems.Add("76 kosu " + (run + 1) + ": robot notun sonunda degil"); continue; }
            versions.Add(v);
            string label = "76 not " + (v + 1) + ": ";
            if (lastLetter != level.Notes[v].Length - 1) problems.Add(label + "kart vurgusu " + lastLetter + ". harfte bitti");
            var drawing = noteEnd != null ? noteEnd.Drawing : null;
            if (drawing == null) { problems.Add(label + "katlanmis cizim belirmedi"); continue; }
            var b = MeshBounds(drawing, levelRoot);
            var cell = Pos(level.NoteRoutes[v].End);
            if (Mathf.Abs(b.center.x - cell.x) > 0.5f || Mathf.Abs(b.center.z - cell.z) > 0.5f) problems.Add(label + "cizim son karede degil");
            if (Mathf.Abs(b.min.y - TerrainHeight(b.center.x, b.center.z)) > GroundTolerance) problems.Add(label + "cizim yere basmiyor");
            if (run == 0)
            {
                Cap(Path.Combine(dir, "s76-cizim.png"));
                yield return new WaitForSeconds(0.3f);
            }
        }
        if (versions.Count != level.Notes.Count) problems.Add("76: " + level.Notes.Count + " notun " + versions.Count + "'i denendi");
        ResetLevel();
        if (noteEnd != null && noteEnd.Drawing != null) problems.Add("76 Tekrar: katlanmis cizim kalkmadi");
        if (hud.NoteLetter != -1) problems.Add("76 Tekrar: kart kapanmadi");
    }

    static Vector3 Flat(Vector3 p) => new Vector3(p.x, 0f, p.z);

    // 77: sirtta katlanmis cizim (kalici karakter gorunusu); arkadan ve yukaridan yakin plan
    IEnumerator BackDrawingCheck(string dir, List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.ListLevel);
        if (!BackDrawingShown()) problems.Add("77: sirtta katlanmis cizim yok");
        robot.transform.rotation = Quaternion.identity;   // kuzeye bakar: sirti kameraya
        var look = robot.RibbonPoint;
        var from = look + new Vector3(0.9f, 1.9f, -3.4f);
        shot.Begin();
        shot.Cut(from, Quaternion.LookRotation(look - from), 24f);
        yield return new WaitForSeconds(1.2f);
        Cap(Path.Combine(dir, "s77-sirt-cizim.png"));
        yield return new WaitForSeconds(0.4f);
        shot.Release();
    }

    bool BackDrawingShown()
    {
        foreach (var t in robot.GetComponentsInChildren<Transform>(false))
            if (t.name == "KatlanmisCizim-Sirt") return true;
        return false;
    }

    // 78: True (BKM-7) satirinda terminal bir kez turuncuya doner
    IEnumerator HighlightCheck(List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.ListEndLevel);
        var screens = StationTraces.Screens;
        if (screens == null) { problems.Add("78: duvar terminali yok"); yield break; }
        yield return RunStationSolution();
        if (screens.Highlights != 1) problems.Add("78: terminal " + screens.Highlights + " kez turuncu oldu");
    }

    // 79: Kor'un bacagi var (yere basma ve alan disi CheckStationFloor'da)
    IEnumerator WorkshopCheck(string dir, List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.WorkshopLevel);
        if (StationTraces.KorLeg == null || CountNamed("KorBacak") != 1) problems.Add("79: Kor'un bacagi " + CountNamed("KorBacak"));
        Cap(Path.Combine(dir, "s79-atolye.png"));
        yield return new WaitForSeconds(0.3f);
    }

    // 80: harita arka duvarin yuzunde, bandin icinde
    IEnumerator MapCheck(string dir, List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.MapLevel);
        var map = StationTraces.WallMap;
        var face = StationTraces.BackWallFace;
        if (map == null || face == null || map.parent != face) problems.Add("80: harita duvar yuzunde degil");
        else CheckOnWallBand("80: harita", map, face, problems);
        Cap(Path.Combine(dir, "s80-harita.png"));
        yield return new WaitForSeconds(0.3f);
    }

    // Duvar yuzunun acik bandinda mi (yanlarda yan duvarlarin icinde, dikeyde supurgelik ile korkuluk arasinda)
    void CheckOnWallBand(string label, Transform item, Transform face, List<string> problems)
    {
        var b = MeshBounds(item, face);
        float side = AreaHalfX + StationRoom.SideGap;
        if (b.min.x < -side || b.max.x > side || b.min.y < WallBandBottom || b.max.y > StationRoom.PanelTop)
            problems.Add(label + " duvar bandindan tasiyor (x " + b.min.x.ToString("F2") + ".." + b.max.x.ToString("F2") + ", y " + b.min.y.ToString("F2") + ".." + b.max.y.ToString("F2") + ")");
    }

    void ExpectPart(List<string> problems, int number, string part)
    {
        if (level.Outro == null || !level.Outro.Contains(part)) problems.Add(number + " bitis damgasi '" + level.Outro + "', sahnede '" + part + "'");
    }

    // Oynanan alanla (kareler) kesisiyor mu (kenara degmek serbest)
    static bool OverlapsArea(Bounds b)
    {
        const float eps = 0.01f;
        return b.min.x < AreaHalfX - eps && b.max.x > -AreaHalfX + eps && b.min.z < AreaHalfZ - eps && b.max.z > -AreaHalfZ + eps;
    }

    // Tek bir karenin icinde mi (kare ortalari yarim sayilarda: Pos)
    static bool InOneCell(Bounds b)
    {
        float cx = Mathf.Floor(b.center.x) + 0.5f, cz = Mathf.Floor(b.center.z) + 0.5f;
        const float half = 0.5f + 0.001f;
        return b.min.x >= cx - half && b.max.x <= cx + half && b.min.z >= cz - half && b.max.z <= cz + half;
    }

    // Seklin koselerinden (tam) sinir kutusu, space'in yerel olcusunde (Renderer.bounds donuk parcalarda sisip zemin altina iner)
    static Bounds MeshBounds(Transform item, Transform space)
    {
        bool any = false;
        var b = new Bounds();
        foreach (var mf in item.GetComponentsInChildren<MeshFilter>(false))
        {
            if (mf.sharedMesh == null) continue;
            var m = mf.transform.localToWorldMatrix;
            foreach (var v in mf.sharedMesh.vertices)
            {
                var p = space.InverseTransformPoint(m.MultiplyPoint3x4(v));
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; }
                else b.Encapsulate(p);
            }
        }
        return b;
    }
}
