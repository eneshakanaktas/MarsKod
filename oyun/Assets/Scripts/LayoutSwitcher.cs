using System;
using UnityEngine;
using UnityEngine.UIElements;

// Degisen alanda ("duzenler") basligin altindaki gosterge: "◀  TİPİ 1/2  ▶", altinda "Kodun her ihtimalde çalışmalı"
// (A1, docs/superpowers/specs/2026-11-19-degisen-alan-gezme-design.md). Ad bolgeye gore gelir (TİPİ, METEOR).
// Oklar oyuncunun oteki duzenleri kod yazmadan once sahnede gezmesi icindir; kod calisirken sonuk kalir ve basilmaz,
// yazi o an oynatilan duzeni gosterir. Duzeni olmayan (ya da tek duzenli) bolumde gorunmez.
public class LayoutSwitcher : VisualElement
{
    const float ArrowSize = 84f;

    /// <summary>Oyuncu bir oka basti: -1 onceki duzen, +1 sonraki duzen</summary>
    public event Action<int> Stepped;

    const string Caption = "Kodun her ihtimalde çalışmalı";

    readonly Label label, caption;
    readonly VisualElement previous, next;
    string title = "";   // TİPİ, METEOR
    int count;
    bool locked;

    public bool Visible => style.display == DisplayStyle.Flex;
    public string Text => label.text;   // goruntu denetimi

    public LayoutSwitcher(Font font, Color ink, Color glass)
    {
        pickingMode = PickingMode.Ignore;
        style.alignItems = Align.Center;
        style.marginTop = 14;
        style.display = DisplayStyle.None;

        var row = new VisualElement { pickingMode = PickingMode.Ignore };
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        previous = Arrow(glass, ink, -1);
        label = Ui.Text("", font, 30, new Color(ink.r, ink.g, ink.b, 0.85f));
        label.style.letterSpacing = 5;
        label.style.marginLeft = 26; label.style.marginRight = 26;
        next = Arrow(glass, ink, +1);
        row.Add(previous);
        row.Add(label);
        row.Add(next);
        Add(row);

        caption = Ui.Text(Caption, font, 24, new Color(ink.r, ink.g, ink.b, 0.55f));
        caption.style.marginTop = 8;
        Add(caption);
    }

    /// <summary>Bolum acilirken: kac duzen var (2'den azsa gosterge gizlenir) ve gostergenin adi (TİPİ, METEOR)</summary>
    public void Setup(int layoutCount, string layoutName)
    {
        count = layoutCount;
        title = layoutName;
        style.display = count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        Show(0);
    }

    /// <summary>Sahnedeki duzen (0'dan baslar)</summary>
    public void Show(int index) => label.text = title + " " + (index + 1) + "/" + count;

    /// <summary>Kod calisirken ya da gecis oynarken oklar basilmaz</summary>
    public void SetLocked(bool on)
    {
        locked = on;
        previous.style.opacity = next.style.opacity = on ? 0.3f : 1f;
    }

    VisualElement Arrow(Color glass, Color ink, int direction)
    {
        var b = new VisualElement();
        b.style.width = ArrowSize; b.style.height = ArrowSize;
        Ui.Radius(b, ArrowSize * 0.5f);
        b.style.backgroundColor = glass;
        b.style.alignItems = Align.Center;
        b.style.justifyContent = Justify.Center;
        b.Add(new Icon(36, (p, r) => DrawChevron(p, r, ink, direction)));
        Ui.Transition(b, "scale", 0.12f, EasingMode.EaseOut);
        b.RegisterCallback<PointerDownEvent>(_ => { if (!locked) b.style.scale = new Scale(new Vector3(0.92f, 0.92f, 1f)); });
        b.RegisterCallback<PointerUpEvent>(_ => b.style.scale = new Scale(Vector3.one));
        b.RegisterCallback<PointerLeaveEvent>(_ => b.style.scale = new Scale(Vector3.one));
        b.RegisterCallback<ClickEvent>(_ =>
        {
            if (locked) return;
            Sound.Tap();
            Stepped?.Invoke(direction);
        });
        return b;
    }

    static void DrawChevron(Painter2D p, Rect r, Color ink, int direction)
    {
        float cx = r.center.x, cy = r.center.y, w = r.width * 0.22f, h = r.height * 0.34f;
        p.strokeColor = ink;
        p.lineWidth = 5f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
        p.BeginPath();
        // ucu okun yonune bakar: -1 "<", +1 ">"
        p.MoveTo(new Vector2(cx - w * direction, cy - h));
        p.LineTo(new Vector2(cx + w * direction, cy));
        p.LineTo(new Vector2(cx - w * direction, cy + h));
        p.Stroke();
    }
}
