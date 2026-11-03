using System;
using System.Collections;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;

// Sahnelerde Kivilcim'in yurumesi (Bolge 6 ve 7): yalnizca bos karelerden (GridPath), noktadan noktaya; Serce'nin eslik etmesi.
// Oyun kurali degil, sahne hareketi. Eskiden CanyonScenes'in icindeydi.
public class SceneWalker
{
    readonly Robot robot;
    readonly Func<Cell, Vector3> cellPos;
    readonly int cols, rows;

    public SceneWalker(Robot robot, Func<Cell, Vector3> cellPos, int cols, int rows)
    {
        this.robot = robot;
        this.cellPos = cellPos;
        this.cols = cols;
        this.rows = rows;
    }

    // Noktadan noktaya: once doner, sonra yurur (hiz birim basina)
    public IEnumerator Walk(IReadOnlyList<Vector3> points, float secondsPerUnit)
    {
        foreach (var p in points)
        {
            var from = robot.transform.localPosition;
            if ((p - from).sqrMagnitude < 1e-4f) continue;
            yield return robot.TurnTo(DuneFinale.YawTowards(from, p), 0.2f);
            yield return robot.MoveTo(p, Vector3.Distance(from, p) * secondsPerUnit);
        }
    }

    // Bos karelerden yolun konumlari; yol yoksa (engeller kapatmis) yalnizca son kare: sahne takilmaz
    public List<Vector3> CellsTo(Level level, Cell from, Cell to, bool itemsTaken)
    {
        var path = GridPath.Find(cols, rows, from, to, GridPath.Obstacles(level, itemsTaken)) ?? new List<Cell> { to };
        return path.ConvertAll(c => cellPos(c));
    }

    // Serce robotun biraz onunde ve ustunde kalir (yol gosterir gibi); Serce'nin kendi coroutine'i olarak calisir
    public IEnumerator Follow(Sparrow sparrow, float lead, float height)
    {
        while (true)
        {
            var target = robot.transform.localPosition + robot.transform.forward * lead + Vector3.up * height;
            sparrow.transform.localPosition = Vector3.Lerp(sparrow.transform.localPosition, target, 1f - Mathf.Exp(-Time.deltaTime * 3f));
            yield return null;
        }
    }

    // Siradaki bos kare (preferCol'a en yakin); yoksa ya da sira alanin disindaysa null
    public Cell? OpenInRow(Level level, int row, int preferCol, bool itemsTaken) =>
        row < 0 || row >= rows ? null : GridPath.OpenInRow(cols, row, preferCol, GridPath.Obstacles(level, itemsTaken));

    // Karenin konumu (bolum kokunun yerel konumu)
    public Vector3 PositionOf(Cell c) => cellPos(c);

    // Arka (kuzey) sira
    public int BackRow => rows - 1;

    // x'e (bolum kokunun yerel konumu) en yakin sutun
    public int ColNear(float x) => Mathf.Clamp(Mathf.RoundToInt(x + (cols - 1) * 0.5f), 0, cols - 1);

    // Yerel konumun karesi (robotun su anki karesi: 68 ve 70'te sahne oradan baslar)
    public Cell CellAt(Vector3 localPos) =>
        new Cell(ColNear(localPos.x), Mathf.Clamp(Mathf.RoundToInt(localPos.z + (rows - 1) * 0.5f), 0, rows - 1));
}
