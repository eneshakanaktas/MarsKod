using System;
using System.Collections.Generic;
using System.Text;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Kod yazma alani: renkli kod (Label) + ustunde ayni yazi tipinde gorunmez bir yazi kutusu (TextField).
// Oyuncu yazi kutusuna yazar (telefonda normal klavye); imlec ve secim yazi kutusundan, renkler alttaki Label'dan gorunur.
// Kolaylik: ":" ile biten satirdan sonra Enter 4 bosluk iceriden baslar; girintideki bosluk silinince girinti bir kademe geri gider;
// Tab 4 bosluk ekler.
public class CodeEditor : VisualElement
{
    public event Action<string> Changed;

    const float FontSize = 37f, NumSize = 30f, Gutter = 68f;
    // Satirlar arasi ek bosluk: satirlar parmakla secilebilecek kadar ferah olsun (onceki sabit satir yuksekligi 66'ydi)
    const float Spacing = 34f;
    
    readonly Color ink, numColor, accent, errorRed;
    readonly Font mono;
    readonly VisualElement rowsLayer, textLayer;
    readonly Label colored, probe;
    readonly TextField field;
    readonly VisualElement caret;
    TextElement fieldText;
    int caretSeen = -1;
    float blinkStart;
    readonly List<(VisualElement row, VisualElement bar, Label num)> rows = new List<(VisualElement, VisualElement, Label)>();
    string text = "";
    int activeLine = -1;
    bool activeError;
    float pitch = FontSize * 1.32f + Spacing, lineHeight = FontSize * 1.32f;

    public CodeEditor(Font mono, Color ink, Color numColor, Color accent, Color errorRed)
    {
        this.mono = mono; this.ink = ink; this.numColor = numColor; this.accent = accent; this.errorRed = errorRed;
        style.paddingTop = Spacing * 0.5f; style.paddingBottom = Spacing * 0.5f;
        style.overflow = Overflow.Hidden;

        // En altta: satir vurgulari ve satir numaralari (konumlari olculen satir araligina gore)
        rowsLayer = new VisualElement { pickingMode = PickingMode.Ignore };
        rowsLayer.style.position = Position.Absolute;
        rowsLayer.style.left = 0; rowsLayer.style.right = 0; rowsLayer.style.top = 0; rowsLayer.style.bottom = 0;
        Add(rowsLayer);

        textLayer = new VisualElement();
        textLayer.style.marginLeft = Gutter;
        Add(textLayer);

        colored = new Label { pickingMode = PickingMode.Ignore, enableRichText = true };
        TextStyle(colored, ink);
        textLayer.Add(colored);

        // Tek satirin yuksekligini olcmek icin gorunmez ornek
        probe = new Label("0") { pickingMode = PickingMode.Ignore };
        TextStyle(probe, Color.clear);
        probe.style.position = Position.Absolute;
        probe.style.visibility = Visibility.Hidden;
        textLayer.Add(probe);

        field = new TextField { multiline = true };
        field.style.position = Position.Absolute;
        field.style.left = 0; field.style.right = 0; field.style.top = 0; field.style.bottom = 0;
        field.textEdition.hideMobileInput = true;   // telefonda klavyenin ustunde ayri kutu acilmasin, burada yazilsin
        field.textEdition.autoCorrection = false;
        // Varsayilan klavye: Unity Android'de ASCIICapable'i "cumle basi buyuk harf" ile aciyor (move -> Move); Default acmiyor
        field.textEdition.keyboardType = TouchScreenKeyboardType.Default;
        // Dokununca kodun tamami secilmesin (telefonda ilk harf butun kodu silerdi)
        field.textSelection.selectAllOnFocus = false;
        field.textSelection.selectAllOnMouseUp = false;
        // imlec ve secim renkleri tema dosyasinda (Resources/UI/MarsTheme.tss)
        field.textSelection.doubleClickSelectsWord = true;
        textLayer.Add(field);

        // Kendi imlecimiz: Unity'ninki telefonda cok ince; kalin, turuncu, yanip sonen bir cizgi
        caret = new VisualElement { pickingMode = PickingMode.Ignore };
        caret.style.position = Position.Absolute;
        caret.style.width = 4;
        caret.style.backgroundColor = accent;
        Radius(caret, 2);
        caret.style.display = DisplayStyle.None;
        textLayer.Add(caret);
        schedule.Execute(UpdateCaret).Every(33);
        // Yazi kutusunun kendi cercevesi/boslugu olmasin; yazisi gorunmez ama alttaki renkli yaziyla birebir ust uste gelsin
        field.Query<VisualElement>().ForEach(e =>
        {
            Zero(e);
            e.style.backgroundColor = Color.clear;
            e.style.borderTopWidth = 0; e.style.borderBottomWidth = 0; e.style.borderLeftWidth = 0; e.style.borderRightWidth = 0;
            if (e is TextElement t && !(e is Label)) TextStyle(t, Color.clear);
            else if (e is Label l) l.style.display = DisplayStyle.None;
        });
        field.style.minWidth = 0; field.style.minHeight = 0;

        field.RegisterValueChangedCallback(OnValueChanged);
        // Tab: odak baska yere gecmesin, 4 bosluk eklensin
        field.RegisterCallback<KeyDownEvent>(e =>
        {
            if (e.keyCode != KeyCode.Tab && e.character != '\t') return;
            if (e.keyCode == KeyCode.Tab && !field.isReadOnly) Insert(CodeTyping.Indent);
            e.StopImmediatePropagation();
        }, TrickleDown.TrickleDown);
        field.RegisterCallback<NavigationMoveEvent>(e =>
        {
            if (e.direction == NavigationMoveEvent.Direction.Next || e.direction == NavigationMoveEvent.Direction.Previous)
                e.StopImmediatePropagation();
        }, TrickleDown.TrickleDown);

        colored.RegisterCallback<GeometryChangedEvent>(_ => Layout());
        probe.RegisterCallback<GeometryChangedEvent>(_ => Layout());
        SetText("");
    }

    public string Text => text;

    // Oyuncu su an yaziyor mu (yazi kutusu odakta)
    public bool Editing => field.focusController != null && field.focusController.focusedElement is VisualElement f && (f == field || field.Contains(f));

    public bool ReadOnly
    {
        get => field.isReadOnly;
        set { field.isReadOnly = value; if (value) Blur(); }
    }

    public void Blur() => field.Blur();

    // Yaziya odaklanir; from..to arasi secili olur (ikisi esitse yalnizca imlec).
    public void Focus(int from, int to)
    {
        field.Focus();
        from = Mathf.Clamp(from, 0, text.Length); to = Mathf.Clamp(to, 0, text.Length);
        field.textSelection.SelectRange(to, from);
    }

    // Kodu disaridan degistirir (bolum degisti vb.); Changed olayi tetiklenmez.
    public void SetText(string source)
    {
        text = (source ?? "").Replace("\r", "").Replace("\t", CodeTyping.Indent).TrimEnd('\n');
        field.SetValueWithoutNotify(text);
        Repaint();
    }

    // Calisan satiri isaretler (0'dan baslar, -1 = hicbiri). error: satir kirmizi yanar.
    public void SetActiveLine(int idx, bool error = false)
    {
        activeLine = idx;
        activeError = error;
        var color = error ? errorRed : accent;
        for (int i = 0; i < rows.Count; i++)
        {
            bool a = i == idx;
            rows[i].row.style.backgroundColor = a ? (error ? new Color(errorRed.r, errorRed.g, errorRed.b, 0.14f) : new Color(1f, 1f, 1f, 0.07f)) : new Color(1f, 1f, 1f, 0f);
            rows[i].bar.style.backgroundColor = color;
            rows[i].bar.style.opacity = a ? 1f : 0f;
            rows[i].num.style.color = a ? color : numColor;
        }
    }

    // ---- Yazma ----

    void OnValueChanged(ChangeEvent<string> e)
    {
        // Telefonda yazi kutusu once yaziyi, sonra imleci gunceller: imlecin yeni yerini klavyenin kendisinden oku
        var keyboard = field.textEdition.touchScreenKeyboard;
        int caret = keyboard != null && keyboard.active && keyboard.canGetSelection ? keyboard.selection.start : field.textSelection.cursorIndex;
        string raw = e.newValue ?? "";
        string fixedText = CodeTyping.Apply(text, raw, ref caret);
        text = fixedText;
        if (fixedText != raw) Show(caret);
        Repaint();
        Changed?.Invoke(text);
    }


    // Duzeltilmis kodu yazi kutusuna koyar, imleci yerlestirir. Telefon klavyesi kendi kopyasini tutar: once ona yeni yazi
    // verilmeli; yoksa imlec eski (kisa) yazinin disinda kalir, hata verir ve klavye duzeltmeyi hemen geri ezer.
    void Show(int caret)
    {
        var kb = field.textEdition.touchScreenKeyboard;
        if (kb != null && kb.active) kb.text = text;
        field.SetValueWithoutNotify(text);
        caret = Mathf.Clamp(caret, 0, text.Length);
        field.textSelection.SelectRange(caret, caret);
    }

    void Insert(string s)
    {
        int a = Mathf.Min(field.textSelection.cursorIndex, field.textSelection.selectIndex), b = Mathf.Max(field.textSelection.cursorIndex, field.textSelection.selectIndex);
        a = Mathf.Clamp(a, 0, text.Length); b = Mathf.Clamp(b, a, text.Length);
        text = text.Substring(0, a) + s + text.Substring(b);
        Show(a + s.Length);
        Repaint();
        Changed?.Invoke(text);
    }

    // ---- Gorunum ----

    // Imleci yazi kutusunun imlec yerine tasir; yazarken 0.5 sn'de bir yanip soner, secim varken gizlenir.
    void UpdateCaret()
    {
        var sel = field.textSelection;
        bool show = Editing && !field.isReadOnly && sel.cursorIndex == sel.selectIndex;
        if (show)
        {
            if (fieldText == null) fieldText = field.Query<TextElement>().Where(t => !(t is Label)).First();
            int idx = Mathf.Clamp(sel.cursorIndex, 0, text.Length);
            if (idx != caretSeen) { caretSeen = idx; blinkStart = Time.unscaledTime; }
            int line = 0;
            for (int i = 0; i < idx; i++) if (text[i] == '\n') line++;
            float x = fieldText != null ? textLayer.WorldToLocal(fieldText.LocalToWorld(sel.cursorPosition)).x : 0f;
            caret.style.left = Mathf.Max(0f, x - 2f);
            caret.style.top = line * pitch - 2f;
            caret.style.height = lineHeight + 4f;
            show = (Time.unscaledTime - blinkStart) % 1f < 0.55f;
        }
        else caretSeen = -1;
        caret.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void Repaint()
    {
        var lines = text.Split('\n');
        var sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) sb.Append('\n');
            // bos satir da yer kaplasin (yazi kutusundaki satirla ayni yukseklikte)
            sb.Append(lines[i].Length == 0 ? " " : CodeColors.Line(lines[i]));
        }
        colored.text = sb.ToString();

        while (rows.Count < lines.Length) AddRow();
        while (rows.Count > lines.Length)
        {
            rows[rows.Count - 1].row.RemoveFromHierarchy();
            rows[rows.Count - 1].num.RemoveFromHierarchy();
            rows.RemoveAt(rows.Count - 1);
        }
        for (int i = 0; i < rows.Count; i++) rows[i].num.text = (i + 1).ToString();
        if (activeLine >= rows.Count) activeLine = -1;
        SetActiveLine(activeLine, activeError);
        Layout();
    }

    void AddRow()
    {
        var row = new VisualElement { pickingMode = PickingMode.Ignore };
        row.style.position = Position.Absolute;
        row.style.left = 0; row.style.right = 0;
        Radius(row, 16);
        Transition(row, "background-color");

        var bar = new VisualElement { pickingMode = PickingMode.Ignore };
        bar.style.position = Position.Absolute;
        bar.style.left = 0; bar.style.top = 14; bar.style.bottom = 14; bar.style.width = 6;
        bar.style.backgroundColor = accent;
        Radius(bar, 3);
        bar.style.opacity = 0;
        Transition(bar, "opacity");
        row.Add(bar);

        var num = new Label { pickingMode = PickingMode.Ignore };
        Zero(num);
        num.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
        num.style.fontSize = NumSize;
        num.style.color = numColor;
        num.style.position = Position.Absolute;
        num.style.left = 18;
        num.style.unityTextAlign = TextAnchor.MiddleLeft;

        rowsLayer.Add(row);
        rowsLayer.Add(num);
        rows.Add((row, bar, num));
    }

    // Satir vurgularini ve numaralari, yazinin gercek satir araligina gore yerlestirir.
    void Layout()
    {
        float lh = probe.layout.height, h = colored.layout.height;
        if (!float.IsNaN(lh) && lh > 1f)
        {
            lineHeight = lh;
            int n = rows.Count;
            pitch = n > 1 && !float.IsNaN(h) && h > lh ? (h - lh) / (n - 1) : lh + Spacing;
        }
        float top0 = Spacing * 0.5f;
        for (int i = 0; i < rows.Count; i++)
        {
            float y = top0 + i * pitch;
            rows[i].row.style.top = y - (pitch - lineHeight) * 0.5f;
            rows[i].row.style.height = pitch;
            rows[i].num.style.top = y;
            rows[i].num.style.height = lineHeight;
        }
        style.minHeight = pitch + Spacing;
    }

    void TextStyle(TextElement t, Color c)
    {
        Zero(t);
        t.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
        t.style.fontSize = FontSize;
        t.style.color = c;
        t.style.whiteSpace = WhiteSpace.Pre; // girinti bosluklari korunsun
        t.style.unityParagraphSpacing = Spacing;
        t.style.unityTextAlign = TextAnchor.UpperLeft;
    }

    static void Zero(VisualElement e)
    {
        e.style.marginLeft = 0; e.style.marginRight = 0; e.style.marginTop = 0; e.style.marginBottom = 0;
        e.style.paddingLeft = 0; e.style.paddingRight = 0; e.style.paddingTop = 0; e.style.paddingBottom = 0;
    }

    static void Radius(VisualElement e, float r)
    {
        e.style.borderTopLeftRadius = r; e.style.borderTopRightRadius = r;
        e.style.borderBottomLeftRadius = r; e.style.borderBottomRightRadius = r;
    }

    static void Transition(VisualElement e, string prop)
    {
        e.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName(prop) };
        e.style.transitionDuration = new List<TimeValue> { new TimeValue(0.18f, TimeUnit.Second) };
        e.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.EaseOut) };
    }
}
