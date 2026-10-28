using System.Collections;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;

// Bolge 7 cizimi (bolge-7-design.md §3): boyanacak kareler zeminde silik turuncu, kesik kenarli cerceve; kalem inikken
// gecilen kare yumusakca belirir. 61-67'de kuma cizilmis koyu oluk, mavi kalem alindiktan sonra (68 ve sonrasi) mavi boya.
// Yalnizca gosterir: hangi karenin boyandigi dunya kurallarindan (PaintBoard, Painted olayi) gelir.
public class PaintTiles : MonoBehaviour
{
    public const int BluePencilLevel = 68;   // bu bolumde kalem toplanir; sonraki bolumler hep mavi

    const float FrameHalf = 0.42f, DashWidth = 0.045f, DashesPerSide = 3, DashFill = 0.6f;
    const float MarkSize = 0.8f, RimSize = 0.88f, AppearSeconds = 0.28f;
    static readonly Color FrameColor = Mats.Hex("#C98552");
    static readonly Color GrooveColor = Mats.Hex("#3B1D12");
    static readonly Color RimColor = Mats.Hex("#D9A27A");   // oluk kenarina itilmis acik kum: kare kuma kazinmis gibi okunur

    readonly Dictionary<Cell, Transform> frames = new Dictionary<Cell, Transform>();
    readonly List<Transform> marks = new List<Transform>();
    Material frameMat, grooveMat, rimMat, blueMat;
    Mesh markMesh, rimMesh;
    System.Func<Cell, Vector3> pos;
    bool blueFromStart, blue;

    public static PaintTiles Create(Transform parent, IEnumerable<Cell> wanted, System.Func<Cell, Vector3> pos, int levelNumber)
    {
        var go = new GameObject("PaintTiles");
        go.transform.SetParent(parent, false);
        var p = go.AddComponent<PaintTiles>();
        p.Build(wanted, pos, levelNumber > BluePencilLevel);
        return p;
    }

    void Build(IEnumerable<Cell> wanted, System.Func<Cell, Vector3> pos, bool blueAlready)
    {
        this.pos = pos;
        blueFromStart = blue = blueAlready;
        frameMat = Mats.Unlit(FrameColor);
        grooveMat = Mats.Lit(GrooveColor, 0.05f);
        rimMat = Mats.Lit(RimColor, 0.05f);
        blueMat = Mats.Lit(BluePencil.Blue, 0.25f);
        markMesh = MeshFactory.Quad(new Vector2(MarkSize, MarkSize));
        rimMesh = MeshFactory.Quad(new Vector2(RimSize, RimSize));
        var dashMesh = MeshFactory.Quad(new Vector2(FrameHalf * 2f / DashesPerSide * DashFill, DashWidth));
        foreach (var c in wanted) frames[c] = Frame(c, dashMesh);
    }

    // Kesik kenarli kare: her kenarda esit aralikli kisa cizgiler (koseler bos kalir, kare "cizilecek" gibi okunur)
    Transform Frame(Cell c, Mesh dash)
    {
        var root = Parts.Empty("Boyanacak", transform);
        root.localPosition = pos(c) + new Vector3(0f, 0.007f, 0f);
        float step = FrameHalf * 2f / DashesPerSide;
        for (int side = 0; side < 4; side++)
        {
            var edge = Parts.Empty("Kenar", root);
            edge.localRotation = Quaternion.Euler(0f, side * 90f, 0f);
            for (int i = 0; i < DashesPerSide; i++)
            {
                float along = -FrameHalf + step * (i + 0.5f);
                Parts.Add("Cizgi", edge, dash, frameMat, new Vector3(along, 0f, FrameHalf), outline: false, castShadow: false);
            }
        }
        return root;
    }

    // Mavi kalem toplandi: bundan sonra boyanan kareler mavi
    public void UseBlue() => blue = true;

    public void Paint(Cell c)
    {
        var mark = Parts.Empty(blue ? "Mavi boya" : "Oluk", transform);
        mark.localPosition = pos(c) + new Vector3(0f, 0.009f + marks.Count * 0.0002f, 0f);
        if (!blue) Parts.Add("Kenar", mark, rimMesh, rimMat, Vector3.zero, outline: false, castShadow: false);
        Parts.Add("Ic", mark, markMesh, blue ? blueMat : grooveMat, new Vector3(0f, 0.001f, 0f), outline: false, castShadow: false);
        mark.localScale = Vector3.zero;
        marks.Add(mark);
        StartCoroutine(Appear(mark, c));
    }

    IEnumerator Appear(Transform mark, Cell c)
    {
        yield return Tween.Run(AppearSeconds, k => mark.localScale = Vector3.one * Mathf.Lerp(0.3f, 1f, Tween.OutCubic(k)));
        if (frames.TryGetValue(c, out var frame)) frame.gameObject.SetActive(false);
    }

    // "Tekrar": boyalar silinir, cerceveler geri gelir
    public void Restore()
    {
        StopAllCoroutines();
        foreach (var m in marks) Destroy(m.gameObject);
        marks.Clear();
        foreach (var f in frames.Values) f.gameObject.SetActive(true);
        blue = blueFromStart;
    }
}
