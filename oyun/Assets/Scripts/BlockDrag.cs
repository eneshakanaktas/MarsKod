using System;
using UnityEngine;
using UnityEngine.UIElements;

// Acemi surukle-birak (tasarim: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §4).
// Paletten gelen parca ya da koddan kaldirilan satir parmakla tasinir; parmagin ustunde satirin bir kopyasi (hayalet) gider.
// Hayalet kod kartinin ustundeyken CodeEditor satirlar arasinda turuncu cizgiyle birakilacak yeri gosterir. Parmak
// saga/sola kayinca girinti kademe kademe degisir; yalnizca Python'un kabul ettigi kademeler (CodeBuffer.IndentRange).
// Koddan kaldirilan satir kartin disina birakilirsa silinir (hayalette cop isareti). Kod yalnizca CodeEditor.Edit ile degisir.
public class BlockDrag
{
    // Surukleme basladi (Hud: -/+ kutusunu kapatir)
    public event Action Started;

    // Parmakla tasirken hayalet parmagin bu kadar ustunde gider (parmak ortmesin); birakma yeri hayaletin alt kenari
    const float TouchLift = 120f;

    readonly CodeEditor editor;
    readonly VisualElement dropZone, ghost;
    readonly Label ghostText;
    readonly Icon trash;
    readonly Color ghostBg, trashBg;
    string text;
    int fromLine = -1, pointerId = -1, gap = -1, level;
    float lift;
    Vector2 start, last;

    // overlay: ekranin tamamini kaplayan ust katman; dropZone: birakilabilecek alan (kod karti)
    public BlockDrag(VisualElement overlay, VisualElement dropZone, CodeEditor editor, Font mono, Color ink, Color accent, Color danger)
    {
        this.editor = editor; this.dropZone = dropZone;
        ghostBg = new Color(0.2f, 0.19f, 0.25f, 0.96f);
        trashBg = new Color(danger.r * 0.45f, danger.g * 0.3f, danger.b * 0.3f, 0.96f);

        ghost = new VisualElement { pickingMode = PickingMode.Ignore };
        ghost.style.position = Position.Absolute;
        ghost.style.flexDirection = FlexDirection.Row;
        ghost.style.alignItems = Align.Center;
        ghost.style.paddingLeft = 26; ghost.style.paddingRight = 26;
        ghost.style.paddingTop = 16; ghost.style.paddingBottom = 16;
        ghost.style.backgroundColor = ghostBg;
        Ui.Radius(ghost, 22);
        Ui.Border(ghost, 3, accent);
        ghost.style.display = DisplayStyle.None;

        ghostText = new Label { pickingMode = PickingMode.Ignore, enableRichText = true };
        Ui.NoSpacing(ghostText);
        ghostText.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
        ghostText.style.fontSize = 37;
        ghostText.style.color = ink;
        ghostText.style.whiteSpace = WhiteSpace.Pre;
        ghost.Add(ghostText);

        trash = new Icon(52, (p, r) => DrawTrash(p, r, danger));
        trash.style.marginLeft = 20;
        trash.style.display = DisplayStyle.None;
        ghost.Add(trash);
        overlay.Add(ghost);

        // Parmak artik hayalette: tasima olaylari buraya gelir
        ghost.RegisterCallback<PointerMoveEvent>(e => { if (e.pointerId == pointerId) Move(e.position); });
        ghost.RegisterCallback<PointerUpEvent>(e => { if (e.pointerId == pointerId) Drop(e.position); });
        ghost.RegisterCallback<PointerCancelEvent>(e => { if (e.pointerId == pointerId) Finish(); });
        ghost.RegisterCallback<PointerCaptureOutEvent>(_ => Finish());

        // Parmak kartin ust/alt kenarinda dururken de uzun kod kaysin
        ghost.schedule.Execute(() => { if (Active) { editor.EdgeScroll(Target(last)); Move(last); } }).Every(40);
    }

    public bool Active => pointerId >= 0;

    // Paletten bir parca tasinmaya basladi
    public void BeginPiece(string piece, Vector2 at, int pointer) => Begin(piece, -1, at, pointer);

    // Koddaki bir satir tasinmaya basladi
    public void BeginLine(int line, Vector2 at, int pointer)
    {
        Begin(editor.Buffer.Line(line).Trim(), line, at, pointer);
        editor.Lift(line);
    }

    void Begin(string code, int line, Vector2 at, int pointer)
    {
        if (editor.ReadOnly) return;
        text = code; fromLine = line; pointerId = pointer; start = at;
        lift = pointer == PointerId.mousePointerId ? 0f : TouchLift;
        ghostText.text = CodeColors.Line(code);
        ghost.style.display = DisplayStyle.Flex;
        ghost.CapturePointer(pointer);
        Started?.Invoke();
        Move(at);
    }

    // Birakma yeri: parmagin (fareyle imlecin) biraz ustu, hayaletin alt kenari
    Vector2 Target(Vector2 p) => new Vector2(p.x, p.y - lift);

    void Move(Vector2 p)
    {
        last = p;
        var t = Target(p);
        var parent = ghost.parent.layout;
        float w = ghost.layout.width, h = ghost.layout.height;
        if (float.IsNaN(w)) w = 0f;
        if (float.IsNaN(h)) h = 0f;
        ghost.style.left = Mathf.Clamp(t.x - 40f, 0f, Mathf.Max(0f, parent.width - w));
        ghost.style.top = t.y - h;

        bool inside = dropZone.worldBound.Contains(t);
        if (inside)
        {
            gap = editor.GapAt(t);
            int shift = Mathf.RoundToInt((p.x - start.x) / editor.IndentStep);
            level = editor.Buffer.DropIndentLevel(gap, shift, fromLine);
            editor.ShowDrop(gap, level, fromLine);
        }
        else
        {
            gap = -1;
            editor.HideDrop();
        }
        bool deleting = !inside && fromLine >= 0;
        trash.style.display = deleting ? DisplayStyle.Flex : DisplayStyle.None;
        ghost.style.backgroundColor = deleting ? trashBg : ghostBg;
        ghost.style.opacity = !inside && fromLine < 0 ? 0.6f : 1f; // kartin disinda paletten gelen parca birakilmaz
    }

    void Drop(Vector2 p)
    {
        Move(p);
        int g = gap, lv = level, from = fromLine;
        string code = text;
        Finish();
        if (g >= 0)
            editor.Edit(b => { if (from >= 0) b.MoveLine(from, g, lv); else b.DropLine(g, lv, code); });
        else if (from >= 0)
            editor.Edit(b => b.DeleteLine(from));
    }

    void Finish()
    {
        if (!Active) return;
        int id = pointerId;
        pointerId = -1;
        editor.HideDrop();
        editor.Lift(-1);
        ghost.style.display = DisplayStyle.None;
        if (ghost.HasPointerCapture(id)) ghost.ReleasePointer(id);
    }

    // Cop kutusu: kapakli kova
    static void DrawTrash(Painter2D p, Rect r, Color c)
    {
        float s = r.width;
        p.strokeColor = c;
        p.lineWidth = s * 0.08f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.14f, s * 0.26f)); p.LineTo(new Vector2(s * 0.86f, s * 0.26f));
        p.MoveTo(new Vector2(s * 0.38f, s * 0.26f)); p.LineTo(new Vector2(s * 0.42f, s * 0.12f));
        p.LineTo(new Vector2(s * 0.58f, s * 0.12f)); p.LineTo(new Vector2(s * 0.62f, s * 0.26f));
        p.MoveTo(new Vector2(s * 0.24f, s * 0.36f)); p.LineTo(new Vector2(s * 0.3f, s * 0.9f));
        p.LineTo(new Vector2(s * 0.7f, s * 0.9f)); p.LineTo(new Vector2(s * 0.76f, s * 0.36f));
        p.MoveTo(new Vector2(s * 0.43f, s * 0.46f)); p.LineTo(new Vector2(s * 0.44f, s * 0.78f));
        p.MoveTo(new Vector2(s * 0.57f, s * 0.46f)); p.LineTo(new Vector2(s * 0.56f, s * 0.78f));
        p.Stroke();
    }
}
