using System;
using UnityEngine;
using UnityEngine.UIElements;

// Butun ekrani kaplayan koyu katman: ustte baslik + X, altinda kayan icerik (bolum secme, kod sozlugu).
// Alttaki oyuna dokunus gecmez. Icerigi alt sinif Content'e ekler.
public class FullScreenPanel : VisualElement
{
    public event Action Closed;

    protected readonly ScrollView Scroll;
    protected VisualElement Content => Scroll.contentContainer;

    public FullScreenPanel(string heading, Font fBold, Color ink, Color buttonBg, Color hairline)
    {
        style.position = Position.Absolute;
        style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
        // tam koyu: arkadaki baslik ve kod karti icinden gorunmesin
        style.backgroundColor = new Color(0.055f, 0.05f, 0.07f, 1f);
        style.display = DisplayStyle.None;
        RegisterCallback<PointerDownEvent>(e => e.StopPropagation());

        var top = new VisualElement();
        top.style.flexDirection = FlexDirection.Row;
        top.style.alignItems = Align.Center;
        top.style.justifyContent = Justify.SpaceBetween;
        top.style.paddingLeft = 52; top.style.paddingRight = 44;
        top.style.paddingTop = Ui.SafeTop() + 44; top.style.paddingBottom = 24;
        top.Add(Ui.Text(heading, fBold, 64, ink));
        top.Add(CloseButton(buttonBg, hairline));
        Add(top);

        Scroll = new ScrollView(ScrollViewMode.Vertical);
        Scroll.style.flexGrow = 1;
        Scroll.style.paddingLeft = 36; Scroll.style.paddingRight = 36;
        // telefonda parmakla kaydirilir; Unity'nin gri kaydirma cubugu gerekmez
        Scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        Scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        Content.style.paddingBottom = 60 + Ui.SafeBottom();
        Add(Scroll);
    }

    public bool Open => style.display == DisplayStyle.Flex;

    protected void ShowPanel()
    {
        style.display = DisplayStyle.Flex;
        Scroll.scrollOffset = Vector2.zero;
    }

    public void Hide() => style.display = DisplayStyle.None;

    VisualElement CloseButton(Color bg, Color hairline)
    {
        var b = new VisualElement();
        b.style.width = 96; b.style.height = 96;
        Ui.Radius(b, 48);
        b.style.backgroundColor = bg;
        Ui.Border(b, 2, hairline);
        b.style.alignItems = Align.Center;
        b.style.justifyContent = Justify.Center;
        b.Add(new Icon(40, DrawClose));
        b.RegisterCallback<ClickEvent>(_ => { Hide(); Closed?.Invoke(); });
        return b;
    }

    public static void DrawClose(Painter2D p, Rect r)
    {
        float w = r.width, h = r.height;
        p.strokeColor = new Color(0.91f, 0.89f, 0.93f);
        p.lineWidth = w * 0.11f;
        p.lineCap = LineCap.Round;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.18f, h * 0.18f)); p.LineTo(new Vector2(w * 0.82f, h * 0.82f));
        p.MoveTo(new Vector2(w * 0.82f, h * 0.18f)); p.LineTo(new Vector2(w * 0.18f, h * 0.82f));
        p.Stroke();
    }
}
