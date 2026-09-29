using System;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Oyunun kendi kod klavyesi (Orta / Usta). Tasarim: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §5.
// Yukaridan asagiya: oneri satiri (yalnizca Orta) · isaret sirasi ( ) : " = , ⇥ ⇤ … · rakamlar · Turkce Q (3 sira) · bosluk ↵.
// "…" uc harf sirasinin yerine ikinci isaret sayfasini acar (17 isaret tek siraya sigmaz; klavye yuksekligi degismez,
// rakamlar gorunur kalir). Tuslar yalnizca CodeEditor.Edit ile CodeBuffer islemlerini cagirir; klavye metin tutmaz.
// Cizim/doku yok: tuslar UI Toolkit ogeleri, ozel tus simgeleri vektorle cizilir (paket buyumesin).
public class CodeKeyboard : VisualElement
{
    const float KeyH = 104f, Gap = 10f, Side = 12f, SuggestH = 92f, LabelSize = 44f;
    const float DoubleTap = 0.4f;

    static readonly string[][] LetterRows =
    {
        new[] { "q", "w", "e", "r", "t", "y", "u", "ı", "o", "p", "ğ", "ü" },
        new[] { "a", "s", "d", "f", "g", "h", "j", "k", "l", "ş", "i" },
        new[] { "z", "x", "c", "v", "b", "n", "m", "ö", "ç" },
    };
    static readonly string[][] MoreRows =
    {
        new[] { "[", "]", "'", ".", "+", "-", "*", "/", "_" },
        new[] { "//", "%", "#", "==", "!=", "<", ">" },
        new[] { "<=", ">=" },
    };

    readonly CodeEditor editor;
    readonly Font mono;
    readonly Color ink, accent, keyBg, specialBg, pressedBg;
    readonly VisualElement suggestRow, letterPage, morePage, popup;
    readonly Label popupText, moreLabel;
    readonly List<(Label label, string lower)> letterKeys = new List<(Label, string)>();
    readonly List<(VisualElement chip, Label text)> chips = new List<(VisualElement, Label)>();
    readonly List<Suggestion> shown = new List<Suggestion>();
    // Tuslar adlariyla (deneme icin tiklatmak amaciyla): yazdigi isaret ya da ⇧ ⌫ ↵ boşluk ⇥ ⇤ … öneri1..3
    readonly Dictionary<string, VisualElement> keysByName = new Dictionary<string, VisualElement>();
    Icon shiftIcon;
    bool shift, shiftLock, more, showSuggestions = true;
    float lastShiftTap = -10f;
    List<string> openWords = new List<string>();
    string lastWord, lastCode;

    public CodeKeyboard(CodeEditor editor, Font mono, Color ink, Color accent, Color background)
    {
        this.editor = editor; this.mono = mono; this.ink = ink; this.accent = accent;
        keyBg = new Color(1f, 1f, 1f, 0.09f);
        specialBg = new Color(1f, 1f, 1f, 0.035f);
        pressedBg = new Color(1f, 1f, 1f, 0.22f);

        style.backgroundColor = background;
        style.borderTopLeftRadius = 40; style.borderTopRightRadius = 40;
        style.paddingLeft = Side; style.paddingRight = Side;
        style.paddingTop = 14; style.paddingBottom = 18 + Ui.SafeBottom();

        // 1. Oneri satiri (Orta)
        suggestRow = new VisualElement();
        suggestRow.style.flexDirection = FlexDirection.Row;
        suggestRow.style.height = SuggestH;
        suggestRow.style.alignItems = Align.Stretch;
        for (int i = 0; i < Suggestions.Max; i++)
        {
            int idx = i;
            var chip = new VisualElement();
            chip.style.flexGrow = 1; chip.style.flexBasis = 0;
            chip.style.alignItems = Align.Center; chip.style.justifyContent = Justify.Center;
            Ui.Radius(chip, 18);
            if (i > 0)
            {
                chip.style.borderLeftWidth = 2;
                chip.style.borderLeftColor = new Color(1f, 1f, 1f, 0.08f);
            }
            var t = KeyText("", 40f, ink);
            chip.Add(t);
            chip.RegisterCallback<PointerDownEvent>(e =>
            {
                if (idx < shown.Count && !editor.ReadOnly)
                {
                    var s = shown[idx];
                    chip.style.backgroundColor = pressedBg;
                    editor.Edit(b => b.ApplySuggestion(s.Word, s.IsFunction));
                }
                e.StopPropagation();
            });
            chip.RegisterCallback<PointerUpEvent>(_ => chip.style.backgroundColor = Color.clear);
            chip.RegisterCallback<PointerLeaveEvent>(_ => chip.style.backgroundColor = Color.clear);
            chips.Add((chip, t));
            keysByName["öneri" + (i + 1)] = chip;
            suggestRow.Add(chip);
        }
        Add(suggestRow);

        // 2. Isaret sirasi
        var sym = Row();
        foreach (var s in new[] { "(", ")", ":", "\"", "=", "," }) sym.Add(TypeKey(s));
        sym.Add(Named("⇥", IconKey(DrawIndent, () => editor.Edit(b => b.IndentLines()))));
        sym.Add(Named("⇤", IconKey(DrawDedent, () => editor.Edit(b => b.DedentLines()))));
        var moreKey = Named("…", Key(1f, true, () => SetMore(!more), false));
        moreLabel = KeyText("…", 40f, ink);
        moreKey.Add(moreLabel);
        sym.Add(moreKey);
        Add(sym);

        // 3. Rakamlar
        var digits = Row();
        foreach (var d in "1234567890") digits.Add(TypeKey(d.ToString()));
        Add(digits);

        // 4. Turkce Q (ya da ikinci isaret sayfasi): ikisi ayni yerde, biri gorunur
        letterPage = new VisualElement();
        var r1 = Row();
        foreach (var l in LetterRows[0]) r1.Add(LetterKey(l));
        var r2 = Row();
        r2.Add(Spacer(0.5f));
        foreach (var l in LetterRows[1]) r2.Add(LetterKey(l));
        r2.Add(Spacer(0.5f));
        var r3 = Row();
        var shiftKey = Named("⇧", Key(1.5f, true, PressShift, false));
        shiftIcon = new Icon(52, DrawShift);
        shiftKey.Add(shiftIcon);
        r3.Add(shiftKey);
        foreach (var l in LetterRows[2]) r3.Add(LetterKey(l));
        r3.Add(BackspaceKey(1.5f));
        letterPage.Add(r1); letterPage.Add(r2); letterPage.Add(r3);
        Add(letterPage);

        morePage = new VisualElement();
        var m1 = Row();
        foreach (var s in MoreRows[0]) m1.Add(TypeKey(s));
        var m2 = Row();
        m2.Add(Spacer(1f));
        foreach (var s in MoreRows[1]) m2.Add(TypeKey(s));
        m2.Add(Spacer(1f));
        var m3 = Row();
        foreach (var s in MoreRows[2]) m3.Add(TypeKey(s));
        m3.Add(Spacer(5.5f));
        m3.Add(BackspaceKey(1.5f));
        morePage.Add(m1); morePage.Add(m2); morePage.Add(m3);
        morePage.style.display = DisplayStyle.None;
        Add(morePage);

        // 5. Bosluk ve Enter
        var bottom = Row();
        var space = Named("boşluk", Key(7f, false, () => editor.Edit(b => b.Type(" ")), false));
        space.Add(KeyText("boşluk", 32f, new Color(ink.r, ink.g, ink.b, 0.5f)));
        bottom.Add(space);
        var enter = Named("↵", Key(2.4f, true, () => editor.Edit(b => b.Enter()), false));
        enter.style.backgroundColor = accent;
        enter.userData = accent; // basili degilken rengi
        enter.Add(new Icon(56, (p, r) => DrawEnter(p, r, Color.white)));
        bottom.Add(enter);
        Add(bottom);

        // Basinca tusun ustunde buyuk harf balonu
        popup = new VisualElement { pickingMode = PickingMode.Ignore };
        popup.style.position = Position.Absolute;
        popup.style.backgroundColor = new Color(0.27f, 0.25f, 0.32f, 1f);
        Ui.Radius(popup, 22);
        popup.style.alignItems = Align.Center; popup.style.justifyContent = Justify.Center;
        popup.style.display = DisplayStyle.None;
        popupText = KeyText("", 64f, Color.white);
        popup.Add(popupText);
        Add(popup);

        schedule.Execute(UpdateSuggestions).Every(60);
    }

    // Orta: oneri satiri gorunur; Usta: gizli.
    public bool ShowSuggestions
    {
        get => showSuggestions;
        set { showSuggestions = value; suggestRow.style.display = value ? DisplayStyle.Flex : DisplayStyle.None; lastWord = null; }
    }

    // Bu bolume kadar acilmis kelimeler (Suggestions.OpenWords)
    public void SetOpenWords(List<string> words)
    {
        openWords = words ?? new List<string>();
        lastWord = null;
    }

    // Ikinci isaret sayfasi acik mi ("…" tusu)
    public void SetMore(bool on)
    {
        more = on;
        letterPage.style.display = on ? DisplayStyle.None : DisplayStyle.Flex;
        morePage.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
        moreLabel.text = on ? "abc" : "…";
    }

    // Klavye kapaninca: buyuk harf ve isaret sayfasi sifirlanir
    public void ResetState()
    {
        shift = shiftLock = false;
        RefreshLetters();
        SetMore(false);
        HidePopup();
    }

    // Adi verilen tusun ekrandaki yeri (panel koordinati); tus gorunmuyorsa null. Deneme icin.
    public Vector2? KeyCenter(string name)
    {
        if (!keysByName.TryGetValue(name, out var k)) return null;
        for (var e = k; e != null; e = e.parent)
            if (e.resolvedStyle.display == DisplayStyle.None) return null;
        return k.worldBound.center;
    }

    VisualElement Named(string name, VisualElement key)
    {
        if (!keysByName.ContainsKey(name)) keysByName[name] = key;
        return key;
    }

    // ---- Oneriler ----

    void UpdateSuggestions()
    {
        if (!showSuggestions) return;
        string word = editor.Buffer.WordBeforeCaret(), code = editor.Text;
        if (word == lastWord && code == lastCode) return;
        lastWord = word; lastCode = code;
        shown.Clear();
        // yalnizca harfle baslayan yarim kelimeye oneri (sayinin ortasinda oneri cikmasin)
        if (word.Length > 0 && (char.IsLetter(word[0]) || word[0] == '_'))
            shown.AddRange(Suggestions.Suggest(word, code, openWords));
        for (int i = 0; i < chips.Count; i++)
        {
            bool on = i < shown.Count;
            chips[i].text.text = on ? shown[i].Word : "";
            chips[i].chip.pickingMode = on ? PickingMode.Position : PickingMode.Ignore;
        }
    }

    // ---- Tuslar ----

    VisualElement Row()
    {
        var r = new VisualElement();
        r.style.flexDirection = FlexDirection.Row;
        r.style.height = KeyH;
        r.style.marginTop = Gap;
        return r;
    }

    static VisualElement Spacer(float grow)
    {
        var s = new VisualElement { pickingMode = PickingMode.Ignore };
        s.style.flexGrow = grow; s.style.flexBasis = 0;
        return s;
    }

    Label KeyText(string s, float size, Color c)
    {
        var l = new Label(s) { pickingMode = PickingMode.Ignore };
        l.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
        l.style.fontSize = size;
        l.style.color = c;
        l.style.marginLeft = 0; l.style.marginRight = 0; l.style.marginTop = 0; l.style.marginBottom = 0;
        l.style.paddingLeft = 0; l.style.paddingRight = 0; l.style.paddingTop = 0; l.style.paddingBottom = 0;
        l.style.unityTextAlign = TextAnchor.MiddleCenter;
        return l;
    }

    // Tus: basinca hemen calisir (hizli yazmada dokunus kaybolmasin); balon basili tuttukca gorunur.
    VisualElement Key(float grow, bool special, Action press, bool balloon, Func<string> balloonText = null)
    {
        var k = new VisualElement();
        k.style.flexGrow = grow; k.style.flexBasis = 0;
        k.style.marginLeft = Gap * 0.5f; k.style.marginRight = Gap * 0.5f;
        var bg = special ? specialBg : keyBg;
        k.style.backgroundColor = bg;
        k.userData = bg;
        Ui.Radius(k, 16);
        k.style.alignItems = Align.Center; k.style.justifyContent = Justify.Center;
        k.RegisterCallback<PointerDownEvent>(e =>
        {
            e.StopPropagation();
            if (editor.ReadOnly) return;
            k.CapturePointer(e.pointerId);
            k.style.backgroundColor = pressedBg;
            if (balloon) ShowPopup(k, balloonText != null ? balloonText() : "");
            press();
        });
        void Release(int id)
        {
            k.style.backgroundColor = (Color)k.userData;
            if (balloon) HidePopup();
            if (k.HasPointerCapture(id)) k.ReleasePointer(id);
        }
        k.RegisterCallback<PointerUpEvent>(e => Release(e.pointerId));
        k.RegisterCallback<PointerCancelEvent>(e => Release(e.pointerId));
        return k;
    }

    VisualElement TypeKey(string s)
    {
        var k = Key(1f, false, () => editor.Edit(b => b.Type(s)), true, () => s);
        k.Add(KeyText(s, s.Length > 1 ? 38f : LabelSize, ink));
        return Named(s, k);
    }

    VisualElement LetterKey(string lower)
    {
        var label = KeyText(lower, LabelSize, ink);
        var k = Key(1f, false, () => TypeLetter(lower), true, () => Letter(lower));
        k.Add(label);
        letterKeys.Add((label, lower));
        return Named(lower, k);
    }

    VisualElement IconKey(Action<Painter2D, Rect> draw, Action press)
    {
        var k = Key(1f, true, press, false);
        k.Add(new Icon(48, draw));
        return k;
    }

    // ⌫: basili tutunca bir an bekleyip art arda siler
    VisualElement BackspaceKey(float grow)
    {
        IVisualElementScheduledItem repeat = null;
        var k = Key(grow, true, () =>
        {
            editor.Edit(b => b.Backspace());
            repeat?.Pause();
            repeat = schedule.Execute(() => editor.Edit(b => b.Backspace())).StartingIn(420).Every(60);
        }, false);
        k.Add(new Icon(52, DrawBackspace));
        Named("⌫", k);
        k.RegisterCallback<PointerUpEvent>(_ => repeat?.Pause());
        k.RegisterCallback<PointerCancelEvent>(_ => repeat?.Pause());
        k.RegisterCallback<PointerCaptureOutEvent>(_ => repeat?.Pause());
        return k;
    }

    // Turkce buyuk harf: i -> İ, ı -> I
    string Letter(string lower) => shift ? Upper(lower) : lower;

    static string Upper(string s) => s == "i" ? "İ" : s == "ı" ? "I" : s.ToUpperInvariant();

    void TypeLetter(string lower)
    {
        string ch = Letter(lower);
        editor.Edit(b => b.Type(ch));
        if (shift && !shiftLock) { shift = false; RefreshLetters(); }
    }

    // ⇧: bir basis bir sonraki harf buyuk; hizla iki basis kilit; kilitliyken basis kapatir
    void PressShift()
    {
        float now = Time.unscaledTime;
        if (shiftLock) shift = shiftLock = false;
        else if (shift && now - lastShiftTap < DoubleTap) shiftLock = true;
        else shift = !shift;
        lastShiftTap = now;
        RefreshLetters();
    }

    void RefreshLetters()
    {
        foreach (var (label, lower) in letterKeys) label.text = Letter(lower);
        shiftIcon?.MarkDirtyRepaint();
    }

    void ShowPopup(VisualElement key, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var r = key.ChangeCoordinatesTo(this, key.contentRect);
        float w = Mathf.Max(r.width * 1.35f, 110f), h = 132f;
        popup.style.width = w; popup.style.height = h;
        popup.style.left = Mathf.Clamp(r.center.x - w * 0.5f, 0f, Mathf.Max(0f, layout.width - w));
        popup.style.top = r.yMin - h - 10f;
        popupText.text = text;
        popup.style.display = DisplayStyle.Flex;
    }

    void HidePopup() => popup.style.display = DisplayStyle.None;



    // ---- Simgeler ----

    void Stroke(Painter2D p, float s, Color c)
    {
        p.strokeColor = c;
        p.lineWidth = s * 0.075f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
    }

    void DrawShift(Painter2D p, Rect r)
    {
        float s = r.width;
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.5f, s * 0.12f));
        p.LineTo(new Vector2(s * 0.88f, s * 0.52f));
        p.LineTo(new Vector2(s * 0.66f, s * 0.52f));
        p.LineTo(new Vector2(s * 0.66f, s * 0.8f));
        p.LineTo(new Vector2(s * 0.34f, s * 0.8f));
        p.LineTo(new Vector2(s * 0.34f, s * 0.52f));
        p.LineTo(new Vector2(s * 0.12f, s * 0.52f));
        p.ClosePath();
        var c = shift ? accent : ink;
        if (shift) { p.fillColor = c; p.Fill(); }
        Stroke(p, s, c);
        p.Stroke();
        if (shiftLock)
        {
            p.BeginPath();
            p.MoveTo(new Vector2(s * 0.3f, s * 0.95f));
            p.LineTo(new Vector2(s * 0.7f, s * 0.95f));
            p.Stroke();
        }
    }

    void DrawBackspace(Painter2D p, Rect r)
    {
        float s = r.width;
        Stroke(p, s, ink);
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.06f, s * 0.5f));
        p.LineTo(new Vector2(s * 0.3f, s * 0.2f));
        p.LineTo(new Vector2(s * 0.94f, s * 0.2f));
        p.LineTo(new Vector2(s * 0.94f, s * 0.8f));
        p.LineTo(new Vector2(s * 0.3f, s * 0.8f));
        p.ClosePath();
        p.Stroke();
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.5f, s * 0.37f)); p.LineTo(new Vector2(s * 0.74f, s * 0.63f));
        p.MoveTo(new Vector2(s * 0.74f, s * 0.37f)); p.LineTo(new Vector2(s * 0.5f, s * 0.63f));
        p.Stroke();
    }

    void DrawEnter(Painter2D p, Rect r, Color c)
    {
        float s = r.width;
        Stroke(p, s, c);
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.82f, s * 0.2f));
        p.LineTo(new Vector2(s * 0.82f, s * 0.62f));
        p.LineTo(new Vector2(s * 0.2f, s * 0.62f));
        p.MoveTo(new Vector2(s * 0.38f, s * 0.44f));
        p.LineTo(new Vector2(s * 0.2f, s * 0.62f));
        p.LineTo(new Vector2(s * 0.38f, s * 0.8f));
        p.Stroke();
    }

    // ⇥ iceri: ok saga, sagda duvar
    void DrawIndent(Painter2D p, Rect r) => DrawTabArrow(p, r, true);

    // ⇤ disari: ok sola, solda duvar
    void DrawDedent(Painter2D p, Rect r) => DrawTabArrow(p, r, false);

    void DrawTabArrow(Painter2D p, Rect r, bool right)
    {
        float s = r.width;
        float X(float x) => right ? s * x : s * (1f - x);
        Stroke(p, s, ink);
        p.BeginPath();
        p.MoveTo(new Vector2(X(0.12f), s * 0.5f)); p.LineTo(new Vector2(X(0.72f), s * 0.5f));
        p.MoveTo(new Vector2(X(0.52f), s * 0.3f)); p.LineTo(new Vector2(X(0.72f), s * 0.5f)); p.LineTo(new Vector2(X(0.52f), s * 0.7f));
        p.MoveTo(new Vector2(X(0.88f), s * 0.24f)); p.LineTo(new Vector2(X(0.88f), s * 0.76f));
        p.Stroke();
    }
}
