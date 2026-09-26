using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Arayuz: ustte bolum + tek cumle hedef + buz sayaci, altta kod karti ve az dugme.
// Olculer 1080 genislikli telefon ekranina gore (piksel).
public class Hud : MonoBehaviour
{
    public event Action RunPressed, ResetPressed, StarsToggled;

    static readonly Color Accent = Mats.Hex("#E07A5F");
    static readonly Color Ink = Mats.Hex("#E9E4EE");
    static readonly Color CardBg = new Color(0.105f, 0.098f, 0.133f, 0.97f);
    static readonly Color ButtonBg = Mats.Hex("#2A2733");
    static readonly Color HintBg = Mats.Hex("#2E2A38");
    static readonly Color Hairline = new Color(1f, 1f, 1f, 0.07f);
    static readonly Color IceFill = Mats.Hex("#A9E6F5");
    static readonly Color NumColor = Mats.Hex("#4F4A5A");
    static readonly Color HeaderText = Mats.Hex("#F5F1F7");

    Font fMed, fSemi, fBold, fMono;
    VisualElement root, hint, runBtn;
    Label chapter, title, runText;
    Icon starIcon;
    readonly List<(VisualElement row, VisualElement bar, Label num)> lines = new List<(VisualElement, VisualElement, Label)>();
    readonly List<Icon> dots = new List<Icon>();
    int collected;
    bool starsOn = true;

    public void Build(string[] code)
    {
        fMed = Resources.Load<Font>("Fonts/Poppins-Medium");
        fSemi = Resources.Load<Font>("Fonts/Poppins-SemiBold");
        fBold = Resources.Load<Font>("Fonts/Poppins-Bold");
        fMono = Resources.Load<Font>("Fonts/JetBrainsMono-Regular");

        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/MarsTheme");
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1080, 2340);
        ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = (float)Screen.width / Screen.height > 0.6f ? 1f : 0f;

        var doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = ps;
        root = doc.rootVisualElement;
        root.pickingMode = PickingMode.Ignore;
        root.style.position = Position.Absolute;
        root.style.left = 0; root.style.right = 0; root.style.top = 0; root.style.bottom = 0;
        root.style.justifyContent = Justify.SpaceBetween;

        root.Add(BuildHeader());
        root.Add(BuildBottom(code));
    }

    float SafeTop()
    {
        var sa = Screen.safeArea;
        float inset = Screen.height - sa.yMax;
        return Mathf.Max(0f, inset) * 1080f / Mathf.Max(1, Screen.width);
    }

    VisualElement BuildHeader()
    {
        var h = new VisualElement { pickingMode = PickingMode.Ignore };
        h.style.paddingTop = SafeTop() + 40;
        h.style.paddingLeft = 44; h.style.paddingRight = 44;

        var row = new VisualElement { pickingMode = PickingMode.Ignore };
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;
        row.style.alignItems = Align.FlexStart;

        var glass = new Color(1f, 1f, 1f, 0.12f);
        var menu = RoundButton(96, glass, new Icon(42, DrawMenu), null);

        var center = new VisualElement { pickingMode = PickingMode.Ignore };
        center.style.flexGrow = 1;
        center.style.alignItems = Align.Center;
        center.style.paddingTop = 2;
        chapter = Text("BÖLÜM 1", fSemi, 30, new Color(1f, 1f, 1f, 0.62f));
        chapter.style.letterSpacing = 6;
        title = Text("3 buz topla", fBold, 68, HeaderText);
        title.style.marginTop = -4;
        Transition(title, "scale", 0.35f, EasingMode.EaseOutBack);
        center.Add(chapter);
        center.Add(title);

        var dotsRow = new VisualElement { pickingMode = PickingMode.Ignore };
        dotsRow.style.flexDirection = FlexDirection.Row;
        dotsRow.style.marginTop = 16;
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            var d = new Icon(36, (p, r) => DrawIceDot(p, r, idx < collected));
            d.style.marginLeft = 10; d.style.marginRight = 10;
            Transition(d, "scale", 0.28f, EasingMode.EaseOutBack);
            dots.Add(d);
            dotsRow.Add(d);
        }
        center.Add(dotsRow);

        starIcon = new Icon(46, DrawSparkle);
        var starBtn = RoundButton(96, glass, starIcon, () => StarsToggled?.Invoke());

        row.Add(menu);
        row.Add(center);
        row.Add(starBtn);
        h.Add(row);
        return h;
    }

    VisualElement BuildBottom(string[] code)
    {
        var card = new VisualElement();
        card.style.marginLeft = 28; card.style.marginRight = 28; card.style.marginBottom = 36;
        card.style.backgroundColor = CardBg;
        Radius(card, 52);
        Border(card, 2, Hairline);
        card.style.paddingTop = 34; card.style.paddingBottom = 36;
        card.style.paddingLeft = 36; card.style.paddingRight = 36;

        var top = new VisualElement();
        top.style.flexDirection = FlexDirection.Row;
        top.style.justifyContent = Justify.SpaceBetween;
        top.style.paddingLeft = 18; top.style.paddingRight = 8;
        top.Add(Text("kod.py", fMed, 28, new Color(Ink.r, Ink.g, Ink.b, 0.45f)));
        top.Add(Text("Python", fMed, 28, new Color(Ink.r, Ink.g, Ink.b, 0.28f)));
        card.Add(top);

        var codeBox = new VisualElement();
        codeBox.style.marginTop = 14;
        for (int i = 0; i < code.Length; i++)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.height = 66;
            row.style.paddingLeft = 18;
            Radius(row, 16);
            Transition(row, "background-color", 0.18f, EasingMode.EaseOut);

            var bar = new VisualElement();
            bar.style.position = Position.Absolute;
            bar.style.left = 0; bar.style.top = 14; bar.style.bottom = 14; bar.style.width = 6;
            bar.style.backgroundColor = Accent;
            Radius(bar, 3);
            bar.style.opacity = 0;
            Transition(bar, "opacity", 0.18f, EasingMode.EaseOut);

            var num = Text((i + 1).ToString(), fMono, 30, NumColor);
            num.style.width = 50;
            var txt = Text(code[i], fMono, 37, Ink);
            txt.enableRichText = true;
            txt.style.whiteSpace = WhiteSpace.NoWrap;

            row.Add(bar); row.Add(num); row.Add(txt);
            codeBox.Add(row);
            lines.Add((row, bar, num));
        }
        card.Add(codeBox);

        var buttons = new VisualElement();
        buttons.style.flexDirection = FlexDirection.Row;
        buttons.style.alignItems = Align.Center;
        buttons.style.marginTop = 28;

        var hintBtn = RoundButton(124, ButtonBg, new Icon(58, DrawBulb), ToggleHint);
        Border(hintBtn, 2, Hairline);

        runBtn = new VisualElement();
        runBtn.style.flexGrow = 1;
        runBtn.style.height = 124;
        runBtn.style.marginLeft = 22; runBtn.style.marginRight = 22;
        runBtn.style.backgroundColor = Accent;
        Radius(runBtn, 62);
        runBtn.style.flexDirection = FlexDirection.Row;
        runBtn.style.alignItems = Align.Center;
        runBtn.style.justifyContent = Justify.Center;
        runBtn.Add(new Icon(38, DrawPlay));
        runText = Text("Çalıştır", fSemi, 42, Color.white);
        runText.style.marginLeft = 16;
        runBtn.Add(runText);
        Pressable(runBtn, () => RunPressed?.Invoke());
        Transition(runBtn, "opacity", 0.2f, EasingMode.EaseOut);

        var resetBtn = RoundButton(124, ButtonBg, new Icon(56, DrawReset), () => ResetPressed?.Invoke());
        Border(resetBtn, 2, Hairline);

        buttons.Add(hintBtn); buttons.Add(runBtn); buttons.Add(resetBtn);
        card.Add(buttons);

        hint = new VisualElement();
        hint.style.position = Position.Absolute;
        hint.style.left = 0; hint.style.right = 0;
        hint.style.bottom = Length.Percent(100);
        hint.style.marginBottom = 20;
        hint.style.backgroundColor = HintBg;
        Radius(hint, 36);
        Border(hint, 2, Hairline);
        hint.style.paddingTop = 30; hint.style.paddingBottom = 32;
        hint.style.paddingLeft = 40; hint.style.paddingRight = 40;
        var ht = Text("<b>move(East)</b> robotu doğuya bir kare ilerletir.\n<b>for</b> altındaki girintili satırları 3 kez tekrarlar.", fMed, 32, Color.white);
        ht.enableRichText = true;
        ht.style.whiteSpace = WhiteSpace.Normal;
        hint.Add(ht);
        hint.style.display = DisplayStyle.None;
        card.Add(hint);

        return card;
    }

    // ---- Disaridan cagrilanlar ----

    public void SetActiveLine(int idx)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            bool a = i == idx;
            lines[i].row.style.backgroundColor = a ? new Color(1f, 1f, 1f, 0.07f) : new Color(1f, 1f, 1f, 0f);
            lines[i].bar.style.opacity = a ? 1f : 0f;
            lines[i].num.style.color = a ? Accent : NumColor;
        }
    }

    public void SetCollected(int n)
    {
        int before = collected;
        collected = n;
        for (int i = 0; i < dots.Count; i++)
        {
            var d = dots[i];
            d.MarkDirtyRepaint();
            if (i >= before && i < n)
            {
                d.style.scale = new Scale(new Vector3(1.45f, 1.45f, 1f));
                d.schedule.Execute(() => d.style.scale = new Scale(Vector3.one)).StartingIn(140);
            }
        }
    }

    public void SetRunning(bool running)
    {
        runText.text = running ? "Çalışıyor…" : "Çalıştır";
        runBtn.style.opacity = running ? 0.6f : 1f;
        if (running) hint.style.display = DisplayStyle.None;
    }

    public void SetDone()
    {
        chapter.text = "BÖLÜM 1  ·  3/3";
        title.text = "Tamamlandı!";
        title.style.scale = new Scale(new Vector3(1.12f, 1.12f, 1f));
        title.schedule.Execute(() => title.style.scale = new Scale(Vector3.one)).StartingIn(180);
        runText.text = "Tekrar";
        runBtn.style.opacity = 1f;
    }

    public void ResetView()
    {
        chapter.text = "BÖLÜM 1";
        title.text = "3 buz topla";
        SetCollected(0);
        SetActiveLine(-1);
        SetRunning(false);
    }

    public void SetStars(bool on)
    {
        starsOn = on;
        starIcon.MarkDirtyRepaint();
    }

    void ToggleHint()
    {
        bool show = hint.style.display == DisplayStyle.None;
        hint.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ---- Yardimcilar ----

    Label Text(string s, Font f, float size, Color c)
    {
        var l = new Label(s) { pickingMode = PickingMode.Ignore };
        l.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(f));
        l.style.fontSize = size;
        l.style.color = c;
        l.style.marginLeft = 0; l.style.marginRight = 0; l.style.marginTop = 0; l.style.marginBottom = 0;
        l.style.paddingLeft = 0; l.style.paddingRight = 0; l.style.paddingTop = 0; l.style.paddingBottom = 0;
        return l;
    }

    VisualElement RoundButton(float size, Color bg, Icon icon, Action onClick)
    {
        var b = new VisualElement();
        b.style.width = size; b.style.height = size;
        Radius(b, size * 0.5f);
        b.style.backgroundColor = bg;
        b.style.alignItems = Align.Center;
        b.style.justifyContent = Justify.Center;
        b.Add(icon);
        Pressable(b, onClick);
        return b;
    }

    static void Pressable(VisualElement b, Action onClick)
    {
        Transition(b, "scale", 0.12f, EasingMode.EaseOut);
        b.RegisterCallback<PointerDownEvent>(_ => b.style.scale = new Scale(new Vector3(0.94f, 0.94f, 1f)));
        b.RegisterCallback<PointerUpEvent>(_ => b.style.scale = new Scale(Vector3.one));
        b.RegisterCallback<PointerLeaveEvent>(_ => b.style.scale = new Scale(Vector3.one));
        if (onClick != null) b.RegisterCallback<ClickEvent>(_ => onClick());
    }

    static void Transition(VisualElement e, string prop, float seconds, EasingMode mode)
    {
        e.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName(prop) };
        e.style.transitionDuration = new List<TimeValue> { new TimeValue(seconds, TimeUnit.Second) };
        e.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(mode) };
    }

    static void Radius(VisualElement e, float r)
    {
        e.style.borderTopLeftRadius = r; e.style.borderTopRightRadius = r;
        e.style.borderBottomLeftRadius = r; e.style.borderBottomRightRadius = r;
    }

    static void Border(VisualElement e, float w, Color c)
    {
        e.style.borderTopWidth = w; e.style.borderBottomWidth = w; e.style.borderLeftWidth = w; e.style.borderRightWidth = w;
        e.style.borderTopColor = c; e.style.borderBottomColor = c; e.style.borderLeftColor = c; e.style.borderRightColor = c;
    }

    // ---- Simgeler (vektorle cizilir) ----

    static Angle Deg(float d) => new Angle(d, AngleUnit.Degree);

    static void DrawMenu(Painter2D p, Rect r)
    {
        p.fillColor = new Color(1f, 1f, 1f, 0.92f);
        for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
        {
            p.BeginPath();
            p.Arc(new Vector2(r.width * (0.16f + 0.34f * i), r.height * (0.16f + 0.34f * j)), r.width * 0.1f, Deg(0), Deg(360));
            p.Fill();
        }
    }

    void DrawSparkle(Painter2D p, Rect r)
    {
        Vector2 c = r.center - r.position;
        float R = r.width * 0.46f, k = r.width * 0.07f;
        p.BeginPath();
        p.MoveTo(c + new Vector2(0, -R));
        p.QuadraticCurveTo(c + new Vector2(k, -k), c + new Vector2(R, 0));
        p.QuadraticCurveTo(c + new Vector2(k, k), c + new Vector2(0, R));
        p.QuadraticCurveTo(c + new Vector2(-k, k), c + new Vector2(-R, 0));
        p.QuadraticCurveTo(c + new Vector2(-k, -k), c + new Vector2(0, -R));
        p.ClosePath();
        if (starsOn) { p.fillColor = new Color(1f, 0.97f, 0.9f, 0.95f); p.Fill(); }
        else { p.strokeColor = new Color(1f, 1f, 1f, 0.55f); p.lineWidth = 3f; p.lineJoin = LineJoin.Round; p.Stroke(); }
    }

    static void DrawIceDot(Painter2D p, Rect r, bool filled)
    {
        float w = r.width, h = r.height;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.5f, h * 0.04f));
        p.LineTo(new Vector2(w * 0.84f, h * 0.38f));
        p.LineTo(new Vector2(w * 0.5f, h * 0.96f));
        p.LineTo(new Vector2(w * 0.16f, h * 0.38f));
        p.ClosePath();
        p.lineJoin = LineJoin.Round;
        if (filled) { p.fillColor = IceFill; p.Fill(); p.strokeColor = IceFill; p.lineWidth = 3f; p.Stroke(); }
        else { p.strokeColor = new Color(1f, 1f, 1f, 0.5f); p.lineWidth = 3f; p.Stroke(); }
    }

    static void DrawBulb(Painter2D p, Rect r)
    {
        float s = r.width;
        p.strokeColor = Ink;
        p.lineWidth = s * 0.075f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
        p.BeginPath();
        p.Arc(new Vector2(s * 0.5f, s * 0.4f), s * 0.25f, Deg(135), Deg(45));
        p.LineTo(new Vector2(s * 0.6f, s * 0.68f));
        p.LineTo(new Vector2(s * 0.4f, s * 0.68f));
        p.ClosePath();
        p.Stroke();
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.42f, s * 0.84f));
        p.LineTo(new Vector2(s * 0.58f, s * 0.84f));
        p.Stroke();
    }

    static void DrawReset(Painter2D p, Rect r)
    {
        float s = r.width;
        Vector2 c = new Vector2(s * 0.5f, s * 0.53f);
        float R = s * 0.3f;
        p.strokeColor = Ink;
        p.lineWidth = s * 0.075f;
        p.lineCap = LineCap.Round;
        p.BeginPath();
        p.Arc(c, R, Deg(-60), Deg(200));
        p.Stroke();
        // ok ucu, yayin basinda (sag ust)
        float a = -60f * Mathf.Deg2Rad;
        Vector2 tip = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * R;
        Vector2 dir = new Vector2(Mathf.Sin(a), -Mathf.Cos(a)); // saat yonunun tersi
        Vector2 perp = new Vector2(-dir.y, dir.x);
        p.fillColor = Ink;
        p.BeginPath();
        p.MoveTo(tip + dir * s * 0.13f);
        p.LineTo(tip - dir * s * 0.03f + perp * s * 0.12f);
        p.LineTo(tip - dir * s * 0.03f - perp * s * 0.12f);
        p.ClosePath();
        p.Fill();
    }

    static void DrawPlay(Painter2D p, Rect r)
    {
        float s = r.width;
        p.fillColor = Color.white;
        p.strokeColor = Color.white;
        p.lineWidth = s * 0.12f;
        p.lineJoin = LineJoin.Round;
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.22f, s * 0.12f));
        p.LineTo(new Vector2(s * 0.86f, s * 0.5f));
        p.LineTo(new Vector2(s * 0.22f, s * 0.88f));
        p.ClosePath();
        p.Fill();
        p.Stroke();
    }
}

// Vektorle cizilen kucuk simge.
public class Icon : VisualElement
{
    readonly Action<Painter2D, Rect> draw;

    public Icon(float size, Action<Painter2D, Rect> draw)
    {
        this.draw = draw;
        style.width = size;
        style.height = size;
        pickingMode = PickingMode.Ignore;
        generateVisualContent += ctx => this.draw(ctx.painter2D, contentRect);
    }
}
