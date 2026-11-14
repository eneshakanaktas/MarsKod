using System;
using System.Collections;
using MarsKod.Dunya;
using UnityEngine;

// Bolge 8 sahneleri: 76 cizim, 78 aciklama ani, 80 kapanis (docs/superpowers/plans/2026-11-14-bolge-8-sahneler-3b.md).
// Akis burada; nesneler (robot, kagit, terminal) yalnizca komut alir. Kamera sahne surerken CameraShot'ta; Restore her seyi geri verir.
// Kivilcim ucmaz: 76'da yerinde doner, yurumez.
public class StationScenes
{
    readonly Robot robot;
    readonly CameraShot shot;
    readonly Hud hud;
    readonly Vector2 areaHalf;
    readonly Func<float, float, float> ground;   // zemin yuksekligi (80'in koridoru)
    readonly SceneSkip skip;                     // K4: her sahne "Atla" dugmesiyle atlanir
    NoteEnd drawingEnd;                          // 76: cizimin bulundugu notun sonu ("Tekrar" cizimi oradan kaldirir)
    CorridorSet corridor;                        // 80: koridor hatirasi kuruluyken
    Vector3 closingStand;                        // 80: robotun haritanin onundeki yeri (atlama son hali buraya koyar)

    public string Phase { get; private set; } = "";   // goruntu denetimi hangi anda oldugunu buradan okur

    public StationScenes(MonoBehaviour host, Robot robot, CameraShot shot, Hud hud, Vector2 areaHalf, Func<float, float, float> ground)
    {
        this.robot = robot;
        this.shot = shot;
        this.hud = hud;
        this.areaHalf = areaHalf;
        this.ground = ground;
        skip = new SceneSkip(host, hud);
    }

    // Sahnelerin tahmini uzunluklari (Atla dugmesinin ustundeki saniye): adimlarin surelerinin toplami
    const float TakeDrawingLength = 8.5f, RevealLength = 7f, ClosingLength = 15f;

    // Atlanabilir halleri (K4): dugmeye dokunulursa sahne durur, son hal kurulur. Oyun bunlari cagirir.
    public IEnumerator TakeDrawingWithSkip(Level level, NoteEnd end) => skip.Run(TakeDrawing(level, end), TakeDrawingLength, FinishTakeDrawing);
    public IEnumerator RevealWithSkip(int row) => skip.Run(Reveal(row), RevealLength, FinishReveal);
    public IEnumerator ClosingWithSkip(Level level, Transform levelRoot) => skip.Run(Closing(level, levelRoot), ClosingLength, FinishClosing);

    // "Atla" dugmesi (denetim de cagirir)
    public void Skip() => skip.Skip();

    // 76 (K3): Kivilcim ayaginin dibindeki katlanmis cizime doner, egilip alir, gogsunun onunde acmaya yeltenir (ust katin kosesi
    // kalkar), durur; Ece'nin yatagina doner, bakar, kagidi geri katlar ve omzunun ustunden sirtina koyar (~6 sn). Kimse soylemez:
    // "Acilmadi" damgasi anlam kazanir, cizimi Ece'ye goturuyor. Yol her kosuda baska yerde biter: sahne robotun o anki yerinden baslar.
    // Kamera once onden-yandan-yukaridan (Kivilcim, elindeki kagit, arkada yataklar), sonra arkasindan yataga dogru (sirti kamerada).
    // bakilan yerden: guneyden, alanin ortasina dogru yandan, yukaridan (alcakta alandaki masalar on plana giriyordu)
    static readonly Vector3 DrawingShotFrom = new Vector3(0.9f, 2.1f, -2.0f);
    const float DrawingShotHalfWidth = 0.8f, DrawingShotLookAt = 0.4f;         // robotla kagit arasinda kagida 0,4 yakin bakar
    const float ChestHeight = 0.45f;                                            // robotun gogsunun yerden yuksekligi (bakis noktasi)
    const float BedShotBack = 1.5f, BedShotRise = 1.6f, BedShotHalfWidth = 1.2f, BedShotLookAt = 0.35f;
    const float RoomMargin = 0.3f;                                              // kamera duvarlara bu kadar yaklasir
    const float PeekAmount = 0.35f, PeekHold = 0.6f;                            // ust kat ne kadar kalkar, ne kadar acik durur

    public IEnumerator TakeDrawing(Level level, NoteEnd end)
    {
        var paper = end != null ? end.Drawing : null;
        if (paper == null || !StationTraces.EceBed.HasValue) yield break;
        drawingEnd = end;
        var place = robot.transform.parent;
        var bed = place.TransformPoint(StationTraces.EceBed.Value);
        hud.StopEditing();
        Phase = "bakis";
        yield return robot.Calm();
        shot.Begin();            // once kamera sabitlenir, sonra kart kapanir (Bolge 6-7 gibi)
        hud.SetCardVisible(false);
        robot.StartCoroutine(robot.TurnTo(DuneFinale.YawTowards(robot.transform.position, paper.position), 0.5f));
        var chest = robot.transform.position + Vector3.up * ChestHeight;
        var look = Vector3.Lerp(chest, paper.position, DrawingShotLookAt);
        var eye = InsideRoom(look + TowardCenter(DrawingShotFrom));
        yield return shot.MoveTo(eye, look, 0.9f, CameraShot.FovToFit(DrawingShotHalfWidth, Vector3.Distance(eye, look)));

        Phase = "alis";
        yield return robot.PickUp(paper);
        Phase = "acma";
        yield return robot.TryUnfold(paper, PeekAmount, PeekHold);

        Phase = "yatak";   // kagit hala yari acik: Ece'nin yatagina doner, bakar, sonra katlar
        float bedYaw = DuneFinale.YawTowards(robot.transform.position, bed);
        robot.StartCoroutine(robot.TurnTo(bedYaw, 0.6f));
        var (bedEye, bedLook, bedFov) = BedShot(bed, bedYaw);
        yield return shot.MoveTo(bedEye, bedLook, 0.9f, bedFov);
        yield return Tween.Wait(0.6f);
        yield return robot.Refold(0.4f);

        Phase = "sirt";
        yield return robot.StowOnBack(paper, BackSlot.FoldedDrawing);
        yield return Tween.Wait(0.8f);
        hud.SetCardVisible(true);   // kart kamera donmeden once (CanyonScenes.FirstFlight'taki neden)
        yield return Tween.Wait(0.2f);
        yield return shot.Return(1f);
        Phase = "son";
    }

    // 76'nin son hali hemen (atlama, Gorev 3 baglar): cizim sirtta, yerdeki gizli, mentese kapali, kamera normal, kart gorunur
    void FinishTakeDrawing()
    {
        var paper = drawingEnd != null ? drawingEnd.Drawing : null;
        if (paper != null)
        {
            paper.gameObject.SetActive(false);
            paper.SetParent(drawingEnd.transform, true);
        }
        robot.ResetTo(robot.transform.localPosition, robot.Yaw);   // yarim kalan hareket durur, durus duzelir
        robot.SetFoldedDrawing(true);
        shot.Release();
        hud.SetCardVisible(true);
        Phase = "son";
    }

    // 78 (K2): satirlar duserken BKM-7 satirinda akis durur; terminal turuncu kalir, kamera yavasca Kivilcim'in yan profiline yaklasir.
    // Kivilcim bir an donar, basini gogsundeki cizime eger, sonra yine ekrana bakar; akis devam eder, bolum sevinc dansi olmadan biter.
    // Kod karti gizlenmez: ekran kutusu satiri gosteriyor; Kivilcim ve terminal kutunun ustunde (ekranin ust yarisi) kalir.
    // Kamera robotun yerelinde (ileri: ekrana, sag: dogu): solundan, bas hizasinin biraz ustunden (yan profil; turuncu ekran
    // kadrajin solunda). Yatayda yuzun biraz onune bakar; bas ekranda dikeyde RevealHeadOnScreen'de durur (alt yari ekran kutusunun).
    static readonly Vector3 RevealEyeFromHead = new Vector3(-1.6f, 0.25f, 0.65f);   // biraz onunden: gozler ve egilen bas secilsin
    const float RevealAimAhead = 0.45f, RevealHalfWidth = 1.0f, RevealHeadOnScreen = 0.68f;
    const float HeadHeight = 0.85f;   // robotun basinin ortasinin yerden yuksekligi

    // Sahne yalniz 78'de ve butun cozum dogruysa oynar: o zaman True donen satir BKM-7'dir. Yoksa -1.
    public static int RevealRow(Level level, RunReport report)
    {
        if (level.Number != StationTraces.ListEndLevel || !report.Complete) return -1;
        return report.ScreenAnswers.FindIndex(a => a.Actual is bool found && found);
    }

    // 78: sevinc dansi ve kutlama sesi yok (yumusak tini)
    public static bool QuietEnding(int levelNumber) => levelNumber == StationTraces.ListEndLevel;

    public IEnumerator Reveal(int row)
    {
        var screens = StationTraces.Screens;
        if (screens == null) yield break;
        hud.StopEditing();
        Phase = "durus";
        screens.HoldHighlight(true);   // bolum sonuna kadar turuncu kalir
        shot.Begin();
        var (eye, look, fov) = RevealShot();
        yield return shot.MoveTo(eye, look, 2.2f, fov);
        Phase = "anlama";
        yield return robot.Realize();
        Phase = "devam";
        yield return Tween.Wait(0.4f);
        yield return shot.Return(1.2f);
        Phase = "son";
    }

    // 78'in son hali hemen (atlama, Gorev 3 baglar): robot yine ekrana bakar, kamera normal; kalan satirlari Oyun'un akisi dusurur
    void FinishReveal()
    {
        robot.ResetTo(robot.transform.localPosition, robot.Yaw);
        shot.Release();
        Phase = "son";
    }

    (Vector3 eye, Vector3 look, float fov) RevealShot()
    {
        var head = robot.transform.position + Vector3.up * HeadHeight;
        var frame = robot.transform.rotation;
        var eye = InsideRoom(head + frame * RevealEyeFromHead);
        var aim = head + frame * (Vector3.forward * RevealAimAhead);
        float fov = CameraShot.FovToFit(RevealHalfWidth, Vector3.Distance(eye, aim));
        return (eye, LookPlacing(eye, aim, fov, RevealHeadOnScreen), fov);
    }

    // Kameranin bakacagi nokta: subject ekranda dikeyde screenY'de (0 alt, 1 ust) dursun diye bakis subject'ten o kadar asagi egilir
    static Vector3 LookPlacing(Vector3 eye, Vector3 subject, float fov, float screenY)
    {
        var dir = (subject - eye).normalized;
        float pitch = Mathf.Atan((2f * screenY - 1f) * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
        var right = Vector3.Cross(Vector3.up, dir).normalized;
        return eye + Quaternion.AngleAxis(pitch, right) * dir;
    }

    // 80 (K1): haritanin onunde (arkadan) -> kirpisma -> koridorda son cizimin (robot ve kiz el ele) onu, terminal kutusunda
    // "BKM-7 — disarida — gorev: KAPI" hatirasi -> kirpisma -> harita, Kivilcim basini haritadaki turuncu oklara kaldirir (~14 sn).
    // Son kare kalir (kamera birakilmaz); "Tekrar" ya da bolum degisimi Restore ile birakir.
    const float BlinkSeconds = 0.3f;
    // Harita kareleri: robot ve hedef (harita / oklar) ayni kadrajda (PairShot; robot alanin herhangi bir sutununda olabilir). Son karede kod karti ekranin alt yarisini kaplar:
    // kamera yuksekten bakar, robot haritanin hemen altinda gorunur.
    const float MapRise = 1.3f, MapBack = 2.2f, MapSubjectOnScreen = 0.55f;
    // Son kare: kod karti ekranin alt yarisini, bitis yazisi ustunu kaplar; arada kalan bantta robot ve oklar sigsin diye genis ve uzak
    const float FinalRise = 1.5f, FinalBack = 3.2f, FinalSubjectOnScreen = 0.68f, FinalHalfWidth = 1.9f, FinalTowardTarget = 0.4f;
    const float MapHalfWidth = 1.2f, MapTowardTarget = 0.5f;
    // Koridor: yan-arkadan; cizim buyuk ve ustte, Kivilcim'in basi ve sirti sag altta onde
    static readonly Vector3 CorridorEyeFromRobot = new Vector3(-1.1f, 0.95f, -1.6f);
    const float CorridorHalfWidth = 1.0f, CorridorLookAt = 0.85f, CorridorSubjectOnScreen = 0.58f, CorridorCloser = 0.08f;

    public IEnumerator Closing(Level level, Transform levelRoot)
    {
        if (StationTraces.WallMap == null || !StationTraces.MapArrowsEnd.HasValue) yield break;
        var mapCenter = StationTraces.WallMap.position;
        var arrows = StationTraces.MapArrowsEnd.Value;
        hud.StopEditing();
        Phase = "harita";
        closingStand = robot.transform.localPosition;
        yield return robot.Calm();
        shot.Begin();
        hud.SetCardVisible(false);
        robot.StartCoroutine(robot.TurnTo(0f, 0.6f));
        var map = PairShot(closingStand, mapCenter, MapRise, MapBack, MapHalfWidth, MapTowardTarget, MapSubjectOnScreen);
        yield return shot.MoveTo(map.eye, map.look, 1.5f, map.fov);
        yield return Tween.Wait(1.2f);

        Phase = "gecis";   // hatiraya: en karanlik anda koridor kurulur, robot son cizimin onune konur
        hud.Blink(BlinkSeconds);
        yield return Tween.Wait(BlinkSeconds * 0.5f);
        corridor = StationTraces.ShowCorridor(levelRoot, areaHalf, ground);
        var drawingStand = CorridorStand(corridor.LastDrawing, closingStand.y);
        robot.ResetTo(drawingStand, 0f);   // sirttaki esyalar kalir (ResetTo onlara dokunmaz)
        var drawing = robot.transform.parent.TransformPoint(corridor.LastDrawing);
        var wall = FacingShot(drawingStand, CorridorEyeFromRobot, drawing, CorridorLookAt, CorridorHalfWidth, CorridorSubjectOnScreen);
        shot.Cut(wall.eye, Quaternion.LookRotation(wall.look - wall.eye), wall.fov);
        yield return Tween.Wait(BlinkSeconds * 0.5f);

        Phase = "cizim";
        yield return shot.MoveTo(Vector3.Lerp(wall.eye, wall.look, CorridorCloser), wall.look, 2.5f, wall.fov);
        Phase = "satir";
        hud.ShowRecall(StationTraces.KapiLine);
        yield return Tween.Wait(2.5f);

        Phase = "donus";   // hatiradan donus: koridor silinir, robot haritanin onune
        hud.HideScreen();
        hud.Blink(BlinkSeconds);
        yield return Tween.Wait(BlinkSeconds * 0.5f);
        corridor.Hide();
        corridor = null;
        robot.ResetTo(closingStand, 0f);
        shot.Cut(map.eye, Quaternion.LookRotation(map.look - map.eye), map.fov);
        yield return Tween.Wait(BlinkSeconds * 0.5f);

        Phase = "bas";
        robot.StartCoroutine(robot.RaiseHeadTo(arrows, 1.2f));
        var final = PairShot(closingStand, arrows, FinalRise, FinalBack, FinalHalfWidth, FinalTowardTarget, FinalSubjectOnScreen);
        yield return shot.MoveTo(final.eye, final.look, 2.5f, final.fov);
        hud.SetCardVisible(true);   // kamera birakilmaz: son kare kalir
        Phase = "son";
    }

    // 80'in son hali hemen (atlama): koridor yok, kutu kapali, robot haritaya donuk ve basi kalkik, kamera son kadrajda, kart gorunur
    void FinishClosing()
    {
        corridor?.Hide();
        corridor = null;
        hud.HideScreen();
        hud.ClearBlink();
        if (StationTraces.WallMap == null || !StationTraces.MapArrowsEnd.HasValue) return;
        if (Phase == "") closingStand = robot.transform.localPosition;   // sahne baslamadan atlandi
        robot.ResetTo(closingStand, 0f);
        var arrows = StationTraces.MapArrowsEnd.Value;
        robot.StartCoroutine(robot.RaiseHeadTo(arrows, 0f));
        var final = PairShot(closingStand, arrows, FinalRise, FinalBack, FinalHalfWidth, FinalTowardTarget, FinalSubjectOnScreen);
        shot.Begin();
        shot.Cut(final.eye, Quaternion.LookRotation(final.look - final.eye), final.fov);
        hud.SetCardVisible(true);
        Phase = "son";
    }

    // Koridorda: alanin ust sirasinda, son cizimin x'ine en yakin sutun (kare ortalari yarim sayilarda)
    Vector3 CorridorStand(Vector3 drawing, float y)
    {
        float x = Mathf.Clamp(Mathf.Floor(drawing.x) + 0.5f, -areaHalf.x + 0.5f, areaHalf.x - 0.5f);
        return new Vector3(x, y, areaHalf.y - 0.5f);
    }

    // Robot ile duvardaki hedef ayni kadrajda (dik ekran dar: yan yana sigmazlar): kamera robotun omzunun arkasinda, hedeften robota
    // uzanan cizginin uzantisinda (guneye dogru egik), 'back' geride ve 'rise' yukarida; robot onde, hedef arkada. Robotun basi ile hedefin
    // arasinda 'toward' kadar hedefe yakin nokta ekranda dikeyde subjectOnScreen'de durur; yatayda halfWidth sigar.
    (Vector3 eye, Vector3 look, float fov) PairShot(Vector3 stand, Vector3 target, float rise, float back, float halfWidth, float toward,
        float subjectOnScreen)
    {
        var robotAt = robot.transform.parent.TransformPoint(stand);
        var head = robotAt + Vector3.up * HeadHeight;
        var away = robotAt - target;
        away.y = 0f;
        away = (away.normalized + Vector3.back).normalized;
        var eye = InsideRoom(robotAt + away * back + Vector3.up * rise);
        var subject = Vector3.Lerp(head, target, toward);
        float fov = CameraShot.FovToFit(halfWidth, Vector3.Distance(eye, subject));
        return (eye, LookPlacing(eye, subject, fov, subjectOnScreen), fov);
    }

    // Kuzeye (duvara) donuk robotun arkasindan bakan kadraj: kamera robotun yerine gore eyeFromRobot'ta (oda icinde); robotun basi ile
    // hedef arasinda lookAt kadar hedefe yakin nokta ekranda dikeyde subjectOnScreen'de durur. stand: robotun duracagi yer (yerel).
    (Vector3 eye, Vector3 look, float fov) FacingShot(Vector3 stand, Vector3 eyeFromRobot, Vector3 target, float lookAt, float halfWidth,
        float subjectOnScreen)
    {
        var robotAt = robot.transform.parent.TransformPoint(stand);
        var head = robotAt + Vector3.up * HeadHeight;
        var eye = InsideRoom(robotAt + eyeFromRobot);
        var subject = Vector3.Lerp(head, target, lookAt);
        float fov = CameraShot.FovToFit(halfWidth, Vector3.Distance(eye, subject));
        return (eye, LookPlacing(eye, subject, fov, subjectOnScreen), fov);
    }

    // Arkasindan yataga dogru: robotun sirti onde, yatak otede
    (Vector3 eye, Vector3 look, float fov) BedShot(Vector3 bed, float yaw)
    {
        var toward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        var back = robot.transform.position + Vector3.up * ChestHeight;
        var eye = InsideRoom(back - toward * BedShotBack + Vector3.up * BedShotRise);
        var look = Vector3.Lerp(back, bed, BedShotLookAt);
        return (eye, look, CameraShot.FovToFit(BedShotHalfWidth, Vector3.Distance(eye, look)));
    }

    // Yan pay alanin ortasina dogru: robot dogudaysa kamera batidan bakar (yan duvara girmesin)
    Vector3 TowardCenter(Vector3 offset) =>
        new Vector3(robot.transform.localPosition.x > 0f ? -offset.x : offset.x, offset.y, offset.z);

    // Kamera odanin icinde kalir: yan duvarlar ve arka duvar (on taraf acik)
    Vector3 InsideRoom(Vector3 world)
    {
        var place = robot.transform.parent;
        var p = place.InverseTransformPoint(world);
        float side = areaHalf.x + StationRoom.SideGap - RoomMargin, back = areaHalf.y + StationRoom.BackGap - RoomMargin;
        p.x = Mathf.Clamp(p.x, -side, side);
        p.z = Mathf.Min(p.z, back);
        return place.TransformPoint(p);
    }

    // "Tekrar" ve bolum degisimi: sahnenin degistirdigi ne varsa geri verir (robotun durusunu ve sirtini Oyun.ResetRobot duzeltir)
    public void Restore()
    {
        skip.Stop();
        drawingEnd?.Restore();   // 76: elde ya da sirta giderken kalan cizim kalkar
        drawingEnd = null;
        corridor?.Hide();        // 80: koridor silinir, 80'in odasi geri gelir
        corridor = null;
        hud.HideScreen();        // 80'in hatira kutusu (ekranli bolumlerde Oyun sonra yine acar)
        robot.gameObject.SetActive(true);
        hud.SetCardVisible(true);
        hud.ClearBlink();
        hud.HideSkip();
        shot.Release();
        Phase = "";
    }
}
