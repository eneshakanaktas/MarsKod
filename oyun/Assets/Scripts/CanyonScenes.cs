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
        hud.StopEditing();   // klavye acik kalirsa kanyona giris gorunmez (kamera ve ekran duzeni bu sirada oturur)
        Phase = "kalkis";
        if (sparrow != null) yield return sparrow.FlyTo(robot.transform.localPosition + Vector3.up * 1.2f, 1f);
        Phase = "yol";
        var walk = CellsTo(level, level.Target ?? level.Robot, ExitCell(level), itemsTaken: true).ConvertAll(c => cellPos(c));
        walk.Add(rift.Trail[0]);
        if (sparrow != null) sparrow.StartCoroutine(Follow(sparrow, 0.5f));
        yield return Walk(walk, 0.45f);
        Phase = "bakis";
        shot.Begin();
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
        Phase = "son";
    }

    // "Tekrar" ve bolum degisimi: sahnenin degistirdigi ne varsa geri verir (Serce'yi CanyonTraces.ResetBackdrop toplar)
    public void Restore()
    {
        robot.gameObject.SetActive(true);
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
