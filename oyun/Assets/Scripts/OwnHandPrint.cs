using System.Collections.Generic;
using UnityEngine;

// Bolum 70 finali: Kivilcim'in duvardaki bos yere (Ece'nin onun icin biraktigi yer) kalemle cizdigi kendi eli. Kalemin ucu geldikce
// parca parca belirir; Clear siler ("Tekrar"). Duvar bolumle birlikte silinince cizim de gider.
public class OwnHandPrint
{
    public const string Name = "ElIzi-Robot";
    const float TipGap = 0.01f;   // kalemin ucu boyanin bu kadar onunde durur (duvarin cikintilarina girmesin)

    readonly Transform face;
    readonly Vector2[] outline = WallMural.RobotHandOutline();
    Transform drawn;
    int drawnTo;

    public OwnHandPrint(Transform wallFace) { face = wallFace; }

    // Kalemin ucunun gidecegi yol (dunya konumlari)
    public List<Vector3> Path()
    {
        var path = new List<Vector3>();
        foreach (var p in outline) path.Add(face.TransformPoint(new Vector3(p.x, p.y, CrayonMarks.Lift - TipGap)));
        return path;
    }

    public bool Complete => drawn != null && drawnTo == outline.Length - 1;

    // Kalemin ucu index'inci noktaya vardi: oraya kadar olan parca cizilir
    public void DrawTo(int index)
    {
        if (drawn == null)
        {
            drawn = new GameObject(Name).transform;
            drawn.SetParent(face, false);
            drawnTo = 0;
        }
        if (index <= drawnTo) return;
        var part = new Vector2[index - drawnTo + 1];
        System.Array.Copy(outline, drawnTo, part, 0, part.Length);
        HandTrace.DrawPart(drawn, part, 7000 + index);
        drawnTo = index;
    }

    public void DrawAll() => DrawTo(outline.Length - 1);

    public void Clear()
    {
        if (drawn != null) Object.Destroy(drawn.gameObject);
        drawn = null;
        drawnTo = 0;
    }
}
