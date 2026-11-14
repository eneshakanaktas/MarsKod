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
        yield return DrawingTakeCheck(dir, problems);
        yield return BackDrawingCheck(dir, problems);
        yield return RevealCheck(dir, problems);
        yield return WorkshopCheck(dir, problems);
        yield return MapCheck(dir, problems);
        yield return ClosingCheck(dir, problems);
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

    // 76: her not surumunde kart son harfe kadar ilerler; sahneden sonra (3b) yerdeki cizim gizli, sirttaki gorunur; "Tekrar" ile kalkar.
    // Cizimin yerde belirmesi (son karede, yere basar) DrawingTakeCheck'te, sahne baslamadan once olculur.
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
            if (drawing.gameObject.activeSelf) problems.Add(label + "sahneden sonra cizim yerde kaldi");
            if (!BackDrawingShown()) problems.Add(label + "sahneden sonra sirtta cizim yok");
            if (stationScenes.Phase != "son") problems.Add(label + "sahne bitmedi ('" + stationScenes.Phase + "')");
        }
        if (versions.Count != level.Notes.Count) problems.Add("76: " + level.Notes.Count + " notun " + versions.Count + "'i denendi");
        ResetLevel();
        if (noteEnd != null && noteEnd.Drawing != null) problems.Add("76 Tekrar: katlanmis cizim kalkmadi");
        if (hud.NoteLetter != -1) problems.Add("76 Tekrar: kart kapanmadi");
    }

    static Vector3 Flat(Vector3 p) => new Vector3(p.x, 0f, p.z);

    // 76 (3b, K3): uc not surumunun her bitis karesinden sahne baslatilir; ilkinden kareler. Cizim once son karede ve yere basar;
    // elde iken robotun govdesine girmez; yatak aninda robot Ece'nin yatagina doner (+-25 derece); sonda yerdeki gizli, sirttaki gorunur.
    // Sonra atlama (son hal) ve normal akista "acma" aninda "Tekrar".
    IEnumerator DrawingTakeCheck(string dir, List<string> problems)
    {
        var shots = new[] { ("bakis", 1.3f, "s76-1-bakis.png"), ("acma", 0.5f, "s76-2-acma.png"), ("yatak", 0.4f, "s76-3-yatak.png"),
            ("sirt", 0.3f, "s76-4-sirt.png"), ("son", 0.5f, "s76-5-son.png") };
        yield return LoadStationLevel(StationTraces.FoldedLevel);
        int versions = level.NoteRoutes.Count;
        for (int v = 0; v < versions; v++)
        {
            if (v > 0) yield return LoadStationLevel(StationTraces.FoldedLevel);
            string label = "76 not " + (v + 1) + ": ";
            yield return DrawingAtNoteEnd(v, label, problems);
            var drawing = noteEnd.Drawing;
            if (drawing == null) continue;
            if (v == 0) { Cap(Path.Combine(dir, "s76-cizim.png")); yield return new WaitForSeconds(0.2f); }
            StartCoroutine(stationScenes.TakeDrawing(level, noteEnd));
            bool inBody = false, bedChecked = false;
            void Measure()
            {
                var phase = stationScenes.Phase;
                if ((phase == "acma" || phase == "yatak") && PaperInTorso(drawing)) inBody = true;
                if (phase == "sirt" && !bedChecked) { bedChecked = true; CheckFacesBed(label, problems); }
            }
            float t = 0f;
            foreach (var (phase, wait, file) in v == 0 ? shots : new[] { ("son", 0f, "") })
            {
                for (; stationScenes.Phase != phase && t < 40f; t += Time.deltaTime) { Measure(); yield return null; }
                if (stationScenes.Phase != phase) { problems.Add(label + phase + " evresi gelmedi ('" + stationScenes.Phase + "')"); break; }
                for (float w = 0f; w < wait; w += Time.deltaTime) { Measure(); yield return null; }
                if (file != "") Cap(Path.Combine(dir, file));
                yield return null;
            }
            if (inBody) problems.Add(label + "kagit elde iken govdenin icine girdi");
            if (!bedChecked) problems.Add(label + "yataga donus olculmedi");
            if (drawing.gameObject.activeSelf) problems.Add(label + "sonda yerdeki cizim gizlenmedi");
            if (!BackDrawingShown()) problems.Add(label + "sonda sirtta cizim yok");
            if (shot.Active) problems.Add(label + "sonda sahne kamerasi birakilmadi");
        }

        // "Atla" (K4): normal akista "acma" aninda dugmeye dokunulur, sahne durur, son hal kurulur
        yield return LoadStationLevel(StationTraces.FoldedLevel);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        yield return WaitForPhase("acma", problems, () => stationScenes.Phase);
        yield return new WaitForSeconds(0.5f);
        if (!hud.SkipButtonOpen) problems.Add("76 atlama: Atla dugmesi acik degil");
        stationScenes.Skip();
        yield return null;
        yield return null;
        if (noteEnd.Drawing == null || noteEnd.Drawing.gameObject.activeSelf) problems.Add("76 atlama: yerdeki cizim gizlenmedi");
        if (!BackDrawingShown()) problems.Add("76 atlama: sirtta cizim yok");
        if (shot.Active || !hud.CardVisible) problems.Add("76 atlama: kamera ya da kart geri gelmedi");
        if (stationScenes.Phase != "son") problems.Add("76 atlama: evre 'son' degil");

        // normal akista "acma" aninda "Tekrar"
        yield return LoadStationLevel(StationTraces.FoldedLevel);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        yield return WaitForPhase("acma", problems, () => stationScenes.Phase);
        yield return new WaitForSeconds(0.5f);
        ResetLevel();
        yield return null;
        CheckStationRestored("76", problems);
        if (noteEnd.Drawing != null) problems.Add("76 Tekrar: cizim kalkmadi");
        if (BackDrawingShown()) problems.Add("76 Tekrar: sirtta cizim kaldi");
        foreach (var t in robot.GetComponentsInChildren<Transform>(true))
            if (t.name == "KatlanmisCizim" && t.parent != null && t.parent.name == "Hand") problems.Add("76 Tekrar: elde kagit kaldi");
    }

    // Robot not surumu v'nin son karesinde, kutlamadan sonraki gibi kameraya donuk; cizim belirir, son karede ve yere basar
    IEnumerator DrawingAtNoteEnd(int v, string label, List<string> problems)
    {
        var cell = Pos(level.NoteRoutes[v].End);
        robot.ResetTo(cell, 180f);
        noteEnd.Arrived(cell);
        yield return new WaitForSeconds(1.2f);
        var drawing = noteEnd.Drawing;
        if (drawing == null) { problems.Add(label + "katlanmis cizim belirmedi"); yield break; }
        var b = MeshBounds(drawing, levelRoot);
        if (Mathf.Abs(b.center.x - cell.x) > 0.5f || Mathf.Abs(b.center.z - cell.z) > 0.5f) problems.Add(label + "cizim son karede degil");
        if (Mathf.Abs(b.min.y - TerrainHeight(b.center.x, b.center.z)) > GroundTolerance) problems.Add(label + "cizim yere basmiyor");
    }

    // Elde tutulan kagit robotun govdesinin (Torso) icine giriyor mu: govdenin kendi yerelinde (degmek serbest)
    bool PaperInTorso(Transform paper)
    {
        Transform torso = null;
        foreach (var t in robot.GetComponentsInChildren<Transform>(false)) if (t.name == "Torso") torso = t;
        if (torso == null) return false;
        var body = MeshBounds(torso, torso);
        body.Expand(-0.008f);
        return body.Intersects(MeshBounds(paper, torso));
    }

    void CheckFacesBed(string label, List<string> problems)
    {
        var bed = levelRoot.TransformPoint(StationTraces.EceBed.Value);
        float want = DuneFinale.YawTowards(robot.transform.position, bed);
        float off = Mathf.Abs(Mathf.DeltaAngle(robot.Yaw, want));
        if (off > 25f) problems.Add(label + "yatak aninda yataga donuk degil (" + off.ToString("F0") + " derece)");
    }

    // Sahne ortasinda "Tekrar": kamera serbest, kart gorunur, kirpisma ve atlama kapali, robot baslangicta ve normal durusta
    void CheckStationRestored(string label, List<string> problems)
    {
        string p = label + " Tekrar: ";
        if (shot.Active) problems.Add(p + "sahne kamerasi birakilmadi");
        if (!hud.CardVisible) problems.Add(p + "kod karti gizli kaldi");
        if (hud.SkipButtonOpen) problems.Add(p + "Atla dugmesi acik kaldi");
        if (!robot.gameObject.activeSelf) problems.Add(p + "robot gizli kaldi");
        if ((robot.transform.localPosition - Pos(level.Robot)).sqrMagnitude > 1e-4f) problems.Add(p + "robot baslangic karesinde degil");
        if (stationScenes.Phase != "") problems.Add(p + "sahne durmadi ('" + stationScenes.Phase + "')");
        if (robot is SparkBot spark && (spark.Posing || spark.EyeColor != SparkBot.CalmEyes)) problems.Add(p + "robot normal durusa donmedi");
    }

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

    // 78 (3b, K2): dogru cozumde akis BKM-7 satirinda durur (anlama aninda 25 satir, KOR yok); sonda 26 satir, terminal bir kez turuncu,
    // robot kutlama yapmadi (ekrana donuk), bolum tamam. Yanlis kod (hep True): sahne oynamaz, terminal kirpisir, bolum bitmez.
    // Sahne ortasinda "Tekrar", klavye acikken bitis (kamera sicramaz) ve 77'de sahne yok.
    IEnumerator RevealCheck(string dir, List<string> problems)
    {
        yield return LoadStationLevel(StationTraces.ListEndLevel);
        var screens = StationTraces.Screens;
        if (screens == null) { problems.Add("78: duvar terminali yok"); yield break; }
        int total = level.Screen.Rows.Count;
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        foreach (var (phase, wait, file) in new[] { ("durus", 0.6f, "s78-1-durus.png"), ("anlama", 1.0f, "s78-2-anlama.png"), ("son", 0.5f, "s78-3-son.png") })
        {
            yield return WaitForPhase(phase, problems, () => stationScenes.Phase);
            yield return new WaitForSeconds(wait);
            if (phase == "anlama" && hud.ScreenRows != total - 1) problems.Add("78 anlama: ekranda " + hud.ScreenRows + " satir (" + (total - 1) + " olmali)");
            Cap(Path.Combine(dir, file));
            yield return null;
        }
        while (running) yield return null;
        if (hud.ScreenRows != total) problems.Add("78 son: ekranda " + hud.ScreenRows + " satir");
        if (screens.Highlights != 1) problems.Add("78: terminal " + screens.Highlights + " kez turuncu oldu");
        if (!complete) problems.Add("78: bolum tamamlanmadi");
        if (Mathf.Abs(Mathf.DeltaAngle(robot.Yaw, ScreenYaw)) > 5f) problems.Add("78: robot ekrana donuk degil (kutlama yapti?)");

        // "Atla" (K4): anlama aninda dokunulur; kalan satirlar duser, sessiz bitis
        yield return LoadStationLevel(StationTraces.ListEndLevel);
        screens = StationTraces.Screens;
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        yield return WaitForPhase("anlama", problems, () => stationScenes.Phase);
        yield return new WaitForSeconds(0.3f);
        if (!hud.SkipButtonOpen) problems.Add("78 atlama: Atla dugmesi acik degil");
        Cap(Path.Combine(dir, "s78-atla-dugme.png"));
        yield return null;
        stationScenes.Skip();
        while (running) yield return null;
        if (stationScenes.Phase != "son") problems.Add("78 atlama: evre 'son' degil (" + stationScenes.Phase + ")");
        if (hud.ScreenRows != total) problems.Add("78 atlama: ekranda " + hud.ScreenRows + " satir");
        if (!complete) problems.Add("78 atlama: bolum tamamlanmadi");
        if (shot.Active) problems.Add("78 atlama: kamera birakilmadi");
        if (Mathf.Abs(Mathf.DeltaAngle(robot.Yaw, ScreenYaw)) > 5f) problems.Add("78 atlama: robot ekrana donuk degil");

        // yanlis kod: her satira True
        yield return LoadStationLevel(StationTraces.ListEndLevel);
        screens = StationTraces.Screens;
        SetCode("def " + level.Screen.Function + "(satir):\n    return True");
        yield return new WaitForSeconds(0.3f);
        OnRun();
        bool played = false;
        while (running) { played |= stationScenes.Phase != ""; yield return null; }
        if (played) problems.Add("78 yanlis kod: sahne oynadi");
        if (complete) problems.Add("78 yanlis kod: bolum tamamlandi");
        if (screens.Highlights == 0) problems.Add("78 yanlis kod: terminal turuncu kirpismadi");

        // sahne ortasinda "Tekrar"
        yield return LoadStationLevel(StationTraces.ListEndLevel);
        screens = StationTraces.Screens;
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        yield return WaitForPhase("anlama", problems, () => stationScenes.Phase);
        yield return new WaitForSeconds(0.8f);
        ResetLevel();
        yield return null;
        CheckStationRestored("78", problems);
        if (screens.Highlights != 0) problems.Add("78 Tekrar: terminal bosta degil");

        // klavye acikken bitis: sahne kamerasi kucultulmus gorunumden sicramadan baslar (k0 ile k1 ayni olmali)
        yield return LoadStationLevel(StationTraces.ListEndLevel);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        yield return new WaitForSeconds(0.5f);
        hud.FocusCode(0, 0);
        while (hud.ScreenRows < total - 3 && stationScenes.Phase == "") yield return null;
        Cap(Path.Combine(dir, "s78-k0-klavye.png"));
        yield return WaitForPhase("durus", problems, () => stationScenes.Phase);
        Cap(Path.Combine(dir, "s78-k1-begin.png"));
        while (running) yield return null;

        // 77: sahne yok, kutlama var
        yield return LoadStationLevel(StationTraces.ListLevel);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        bool played77 = false;
        while (running) { played77 |= stationScenes.Phase != ""; yield return null; }
        if (played77) problems.Add("77: 78'in sahnesi oynadi");
        if (Mathf.Abs(Mathf.DeltaAngle(robot.Yaw, 180f)) > 5f) problems.Add("77: robot kutlama yapmadi");
    }

    // 80 (3b, K1): uc not surumunun hepsinden kapanis sonuna varilir; ilkinden kareler. Koridorda 5 cizim, 80'in haritasi gizli, sirtta
    // uc esya; satirda kutudaki yazi bolum-78'deki BKM-7 satiriyla ayni; sonda koridor yok, harita gorunur, bas kalkik, kart gorunur.
    // Sonra "Atla" dugmesi (1 sn sonra, saniye azalir; cizimde dokununca son hal), "satir" ve "cizim" anlarinda "Tekrar".
    IEnumerator ClosingCheck(string dir, List<string> problems)
    {
        string kapi = KapiLineOf78();
        if (kapi != StationTraces.KapiLine) problems.Add("80: hatira satiri bolum-78'deki satirla ayni degil ('" + kapi + "')");
        yield return LoadStationLevel(StationTraces.MapLevel);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        var shots = new[] { ("harita", 2.2f, "s80-1-harita.png"), ("cizim", 1f, "s80-2-cizim.png"), ("satir", 1.2f, "s80-3-satir.png"),
            ("bas", 1.5f, "s80-4-bas.png"), ("son", 0.5f, "s80-5-son.png") };
        for (int run = 0; run < level.Notes.Count; run++)
        {
            if (run > 0) ResetLevel();
            string label = "80 kosu " + (run + 1) + ": ";
            OnRun();
            foreach (var (phase, wait, file) in run == 0 ? shots : new[] { ("cizim", 0.5f, ""), ("satir", 0.5f, ""), ("son", 0.5f, "") })
            {
                yield return WaitForPhase(phase, problems, () => stationScenes.Phase, 60f);
                if (stationScenes.Phase != phase) break;
                yield return new WaitForSeconds(wait);
                if (phase == "cizim") CheckCorridor(label, problems);
                if (phase == "satir") CheckRecall(label, kapi, problems);
                if (phase == "son") CheckClosingEnd(label, problems);
                if (file != "") Cap(Path.Combine(dir, file));
                yield return null;
            }
            while (running) yield return null;
        }

        // Atla: dugme 1 sn sonra, saniye azalir; cizimde dokununca son hal
        yield return LoadStationLevel(StationTraces.MapLevel);
        SetCode(level.Solution);
        yield return new WaitForSeconds(0.3f);
        OnRun();
        yield return WaitForPhase("harita", problems, () => stationScenes.Phase, 60f);
        yield return new WaitForSeconds(0.5f);
        if (hud.SkipButtonOpen) problems.Add("80 atlama: dugme 1 sn dolmadan acildi");
        yield return new WaitForSeconds(0.9f);
        string first = hud.SkipSecondsText;
        if (!hud.SkipButtonOpen) problems.Add("80 atlama: dugme acilmadi");
        Cap(Path.Combine(dir, "s80-atla-dugme.png"));
        yield return new WaitForSeconds(1.1f);
        if (hud.SkipSecondsText == first) problems.Add("80 atlama: kalan saniye azalmadi (" + first + ")");
        yield return WaitForPhase("cizim", problems, () => stationScenes.Phase, 60f);
        stationScenes.Skip();
        yield return null;
        yield return null;
        if (stationScenes.Phase != "son") problems.Add("80 atlama: evre 'son' degil (" + stationScenes.Phase + ")");
        if (hud.SkipButtonOpen) problems.Add("80 atlama: dugme kapanmadi");
        CheckClosingEnd("80 atlama: ", problems);
        if (CorridorRoot() != null) problems.Add("80 atlama: koridor silinmedi");
        while (running) yield return null;
        Cap(Path.Combine(dir, "s80-atlama-son.png"));
        yield return null;

        // sahne ortasinda "Tekrar": satirda ve cizimde (koridor acikken)
        foreach (var phase in new[] { "satir", "cizim" })
        {
            yield return LoadStationLevel(StationTraces.MapLevel);
            SetCode(level.Solution);
            yield return new WaitForSeconds(0.3f);
            OnRun();
            yield return WaitForPhase(phase, problems, () => stationScenes.Phase, 60f);
            yield return new WaitForSeconds(0.5f);
            ResetLevel();
            yield return null;
            CheckStationRestored("80 (" + phase + ")", problems);
            if (CorridorRoot() != null) problems.Add("80 Tekrar (" + phase + "): koridor silinmedi");
            if (StationTraces.WallMap == null || !StationTraces.WallMap.gameObject.activeInHierarchy) problems.Add("80 Tekrar (" + phase + "): harita gorunmuyor");
            if (hud.ScreenOpen) problems.Add("80 Tekrar (" + phase + "): hatira kutusu acik");
        }
    }

    // bolum-78.json'da True donen (BKM-7) satirin girdisi
    string KapiLineOf78()
    {
        var list = levels.Find(l => l.Number == StationTraces.ListEndLevel);
        foreach (var row in list.Screen.Rows)
            if (row.Expected is bool yes && yes) return row.Input as string;
        return null;
    }

    Transform CorridorRoot() => world.Find("Koridor-Hatirasi");

    void CheckCorridor(string label, List<string> problems)
    {
        var corridor = CorridorRoot();
        if (corridor == null) { problems.Add(label + "koridor kurulmadi"); return; }
        int drawings = 0;
        foreach (var t in corridor.GetComponentsInChildren<Transform>(false))
            if (t.name.StartsWith("Cizim-") && t.name.Length > 6 && char.IsDigit(t.name[6])) drawings++;
        if (drawings != 5) problems.Add(label + "koridorda " + drawings + " cizim");
        if (StationTraces.WallMap != null && StationTraces.WallMap.gameObject.activeInHierarchy) problems.Add(label + "koridorda 80'in haritasi gorunuyor");
        if (!BackDrawingShown() || !HasCrayon() || !HasRibbon()) problems.Add(label + "koridorda sirtta uc esya yok");
        var p = robot.transform.localPosition;
        if (Mathf.Abs(p.x) > AreaHalfX || Mathf.Abs(p.z) > AreaHalfZ) problems.Add(label + "koridorda robot alanin disinda " + p.ToString("F2"));
    }

    void CheckRecall(string label, string kapi, List<string> problems)
    {
        if (!hud.ScreenOpen) { problems.Add(label + "hatira kutusu acilmadi"); return; }
        string row = hud.ScreenFirstRow;
        if (kapi == null || row == null || !row.Contains(kapi)) problems.Add(label + "hatira satiri '" + row + "'");
        if (hud.ScreenTitle != ScreenView.RecallTitle) problems.Add(label + "kutu basligi '" + hud.ScreenTitle + "'");
    }

    void CheckClosingEnd(string label, List<string> problems)
    {
        if (StationTraces.WallMap == null || !StationTraces.WallMap.gameObject.activeInHierarchy) problems.Add(label + "sonda harita gorunmuyor");
        if (hud.ScreenOpen) problems.Add(label + "sonda hatira kutusu acik");
        if (!hud.CardVisible) problems.Add(label + "sonda kart gizli");
        if (robot is SparkBot spark && spark.Nod > -15f) problems.Add(label + "sonda bas kalkik degil (" + spark.Nod.ToString("F0") + ")");
        if (Mathf.Abs(Mathf.DeltaAngle(robot.Yaw, 0f)) > 5f) problems.Add(label + "sonda robot haritaya donuk degil");
    }

    bool HasRibbon()
    {
        foreach (var t in robot.GetComponentsInChildren<Transform>(false)) if (t.name == "Ribbon") return true;
        return false;
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
