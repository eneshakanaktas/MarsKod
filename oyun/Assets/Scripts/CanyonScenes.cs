using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsKod.Dunya;
using UnityEngine;

// Bolge 6'nin sahneleri (spec 3b): 60 kanyona inis (simdi), 55 Serce'nin ilk ucusu, 51 roketin inisi. Akis burada; nesneler
// (yarik, Serce, roket) yalnizca komut alir. Kamera sahne surerken CameraShot'ta; Restore her seyi geri verir.
// Kivilcim ucmaz: yalnizca bos karelerden (GridPath) ve patikadan yurur.
public class CanyonScenes
{
    readonly Robot robot;
    readonly CameraShot shot;
    readonly Hud hud;                         // 55 ve 51'in ekran gecisleri
    readonly Func<Cell, Vector3> cellPos;
    readonly int cols, rows;
    readonly float startYaw;                  // Oyun.StartYaw: robotun baslangic yonu (51'de kapidan cikinca)
    const float BirdViewAbove = 4.5f, BirdMargin = 1.2f, SparrowOnScreen = 0.55f;   // 55 kus bakisi: Serce'nin ustunde ne kadar yukarida, alanin yanlarinda pay
    public string Phase { get; private set; } = "";   // goruntu denetimi hangi anda oldugunu buradan okur

    public CanyonScenes(Robot robot, CameraShot shot, Hud hud, Func<Cell, Vector3> cellPos, int cols, int rows, float startYaw)
    {
        this.robot = robot;
        this.shot = shot;
        this.hud = hud;
        this.cellPos = cellPos;
        this.cols = cols;
        this.rows = rows;
        this.startYaw = startYaw;
    }

    // 60: hedeften bos karelerle arka siraya, patikadan direklerin arasina, yariga inis; kamera yariga bakar
    public IEnumerator Descent(Level level)
    {
        var rift = CanyonTraces.Rift;
        var sparrow = CanyonTraces.ActiveSparrow;
        if (rift == null) yield break;
        hud.StopEditing();   // klavye kapanir; ekran duzeni kalkis sirasinda oturur
        Phase = "kalkis";
        if (sparrow != null) yield return sparrow.FlyTo(robot.transform.localPosition + Vector3.up * 1.2f, 1f);
        else yield return Tween.Wait(1f);
        // Kamera simdiki gorunumde sabitlenir, SONRA kod karti kapanir: kart kapaninca normal kamera ekrani yeniden sigdirip
        // alani yakinlastirmasin. Sahne boyunca kart yok, ekranin tamami sahnenin.
        shot.Begin();
        hud.SetCardVisible(false);
        Phase = "yol";
        var walk = CellsTo(level, level.Target ?? level.Robot, ExitCell(level), itemsTaken: true).ConvertAll(c => cellPos(c));
        walk.Add(rift.Trail[0]);
        if (sparrow != null) sparrow.StartCoroutine(Follow(sparrow, 0.5f));
        yield return Walk(walk, 0.45f);
        Phase = "bakis";
        yield return shot.MoveTo(rift.ViewPoint, rift.ViewTarget, 3f, CanyonRift.ViewFov);
        Phase = "inis";
        if (sparrow != null)
        {
            sparrow.StopAllCoroutines();   // zemindeki "Follow" biter; yarikta acik havadan eslik eder
            sparrow.StartCoroutine(FollowBeside(sparrow, rift));
        }
        yield return robot.Glide(rift.Trail.Take(rift.BendIndex + 1).ToList(), 0.5f);
        robot.gameObject.SetActive(false);
        if (sparrow != null)
        {
            sparrow.StopAllCoroutines();
            yield return sparrow.FlyTo(rift.AirBeside(rift.Trail[rift.BendIndex]), 1.2f);
            sparrow.gameObject.SetActive(false);
        }
        yield return Tween.Wait(0.8f);
        hud.SetCardVisible(true);   // son kare kalir; "Tekrar" yeniden gorunur
        Phase = "son";
    }

    // 55: Serce uyanir, cati boslugundan yukselir; bir an Serce'nin gozunden tam yukaridan bakis; sonra yerine ucar
    public IEnumerator FirstFlight(Level level)
    {
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null) yield break;
        hud.StopEditing();
        Phase = "uyanis";
        yield return robot.TurnTo(DuneFinale.YawTowards(robot.transform.position, sparrow.transform.position), 0.5f);
        yield return sparrow.WakeUp();
        Phase = "kalkis";
        robot.StartCoroutine(robot.LookUp());
        var above = sparrow.transform.localPosition + Vector3.up * 3.2f;
        yield return sparrow.TakeOff(above, 3f);
        Phase = "kus";
        shot.Begin();            // once kamera sabitlenir, sonra kart kapanir (60'taki gibi)
        hud.SetCardVisible(false);
        hud.Blink(0.3f);
        yield return Tween.Wait(0.15f);
        var (eye, fov) = BirdView(sparrow);
        shot.Cut(eye, Quaternion.Euler(90f, 0f, 0f), fov);
        shot.Sway = 1.5f;
        yield return Tween.Wait(2.5f);
        hud.Blink(0.3f);
        yield return Tween.Wait(0.15f);
        // Kart kamera birakilmadan once geri gelir: normal kamera alani kartin ustundeki bosluga sigdirir; kart gizliyken birakilirsa
        // bosluk sifir sanilir ve kamera asiri uzaklasir. Yerlesim oturana kadar bekler (ekran bu sirada kararik).
        hud.SetCardVisible(true);
        yield return Tween.Wait(0.2f);
        shot.Release();
        Phase = "inis";
        yield return sparrow.FlyTo(CanyonTraces.SparrowHover, 1.6f);
        Phase = "son";
    }

    // 55 kus bakisi: alanin tamami (kareler, Kivilcim) ve sundurma kadrajda. Gorus acisi ekran oranindan hesaplanir (kod karti ya da
    // klavyenin durumuna bagli degil): telefonda da bilgisayar penceresinde de ayni sey gorunur.
    (Vector3 eye, float fov) BirdView(Sparrow sparrow)
    {
        var first = cellPos(new Cell(0, 0));
        var last = cellPos(new Cell(cols - 1, rows - 1));
        float halfWidth = Mathf.Abs(last.x - first.x) * 0.5f + BirdMargin;
        float height = sparrow.transform.position.y + BirdViewAbove;
        float aspect = (float)Screen.width / Mathf.Max(Screen.height, 1);
        float fov = Mathf.Clamp(2f * Mathf.Atan(halfWidth / (height * aspect)) * Mathf.Rad2Deg, 30f, 100f);
        // Serce ekranin ust ceyreginde, baslik yazilarinin altinda gorunsun (ekranda ust kenardan SparrowOnScreen kadar asagida)
        float halfHeightAtSparrow = (height - sparrow.transform.position.y) * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float z = sparrow.transform.position.z - SparrowOnScreen * halfHeightAtSparrow;
        return (new Vector3((first.x + last.x) * 0.5f, height, z), fov);
    }

    const float LandingHeight = 6f;   // 51: roketin gokyuzunde belirdigi yukseklik (ekranin ustunden girer)

    // 51 her yuklendiginde: bolum yuklenir yuklenmez (gecis yazilari sirasinda) roket havada, robot icinde; ekranda hicbir sey "ani belirmez"
    public void PrepareLanding()
    {
        var rocket = CanyonTraces.ActiveRocket;
        if (rocket == null) return;
        robot.gameObject.SetActive(false);
        rocket.SetInFlight(LandingHeight);
    }

    // 51 (her giriste): roket kanyona iner, kapi acilir, Kivilcim merdivenden inip bos karelerden baslangica gider.
    public IEnumerator Landing(Level level)
    {
        var rocket = CanyonTraces.ActiveRocket;
        if (rocket == null) yield break;
        Phase = "ucus";
        PrepareLanding();
        Sound.Rocket();
        yield return rocket.Land(4f);
        LandingDust.Play(rocket.transform.parent, rocket.HomePosition, CanyonTraces.LanderScale);
        Phase = "indi";
        yield return Tween.Wait(0.6f);
        yield return rocket.OpenDoor();
        Phase = "kapi";
        robot.ResetTo(rocket.DoorPoint, 180f);
        robot.gameObject.SetActive(true);
        yield return robot.MoveTo(rocket.LadderFoot, 0.8f);
        var entry = GridPath.OpenInRow(cols, rows - 1, 0, GridPath.Obstacles(level, itemsTaken: false)) ?? level.Robot;
        yield return Walk(CellsTo(level, entry, level.Robot, itemsTaken: false).ConvertAll(c => cellPos(c)), 0.4f);
        yield return robot.TurnTo(startYaw, 0.3f);
        Phase = "son";
    }

    // Dokununca atlanir: her sey son haline (roket yerde, kapi acik, robot baslangicta)
    public void FinishLanding(Level level)
    {
        CanyonTraces.ActiveRocket?.SetLanded();
        robot.gameObject.SetActive(true);
        robot.ResetTo(cellPos(level.Robot), startYaw);
        Phase = "son";
    }

    // "Tekrar" ve bolum degisimi: sahnenin degistirdigi ne varsa geri verir (Serce'yi CanyonTraces.ResetBackdrop toplar)
    public void Restore()
    {
        robot.gameObject.SetActive(true);
        hud.SetCardVisible(true);
        hud.ClearBlink();
        shot.Release();
        Phase = "";
    }

    // Serce robotun biraz onunde ve ustunde kalir (yol gosterir gibi)
    IEnumerator Follow(Sparrow sparrow, float lead)
    {
        while (true)
        {
            var target = robot.transform.localPosition + robot.transform.forward * lead + Vector3.up * 1.1f;
            sparrow.transform.localPosition = Vector3.Lerp(sparrow.transform.localPosition, target, 1f - Mathf.Exp(-Time.deltaTime * 3f));
            yield return null;
        }
    }

    // Yarikta Serce robotun yaninda, acik havada kalir (rafin sacaginin ve duvarin icine girmez)
    IEnumerator FollowBeside(Sparrow sparrow, CanyonRift rift)
    {
        while (true)
        {
            var target = rift.AirBeside(robot.transform.localPosition + robot.transform.forward * 0.3f);
            sparrow.transform.localPosition = Vector3.Lerp(sparrow.transform.localPosition, target, 1f - Mathf.Exp(-Time.deltaTime * 4f));
            yield return null;
        }
    }

    // Noktadan noktaya: once doner, sonra yurur (hiz birim basina)
    IEnumerator Walk(IReadOnlyList<Vector3> points, float secondsPerUnit)
    {
        foreach (var p in points)
        {
            var from = robot.transform.localPosition;
            if ((p - from).sqrMagnitude < 1e-4f) continue;
            yield return robot.TurnTo(DuneFinale.YawTowards(from, p), 0.2f);
            yield return robot.MoveTo(p, Vector3.Distance(from, p) * secondsPerUnit);
        }
    }

    // Bos karelerden yol; yoksa (engeller kapatmis) robot dogrudan son kareye konur: sahne takilmaz
    List<Cell> CellsTo(Level level, Cell from, Cell to, bool itemsTaken)
    {
        var path = GridPath.Find(cols, rows, from, to, GridPath.Obstacles(level, itemsTaken));
        return path ?? new List<Cell> { to };
    }

    Cell ExitCell(Level level) =>
        GridPath.OpenInRow(cols, rows - 1, ColNear(CanyonTraces.DescentPathX), GridPath.Obstacles(level, itemsTaken: true)) ?? level.Target ?? level.Robot;

    int ColNear(float x) => Mathf.Clamp(Mathf.RoundToInt(x + (cols - 1) * 0.5f), 0, cols - 1);
}
