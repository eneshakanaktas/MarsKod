using System;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Arayuz: ustte bolum + tek cumle hedef + buz sayaci, altta kod karti ve az dugme.
// Olculer 1080 genislikli telefon ekranina gore (piksel).
public class Hud : MonoBehaviour
{
    public event Action RunPressed, ResetPressed, StarsToggled, MenuPressed;
    // Oyuncu kodu degistirdi (yeni kodun tamami)
    public event Action<string> CodeChanged;

    static readonly Color Accent = Mats.Hex("#E07A5F");
    static readonly Color Ink = Mats.Hex("#E9E4EE");
    static readonly Color CardBg = new Color(0.105f, 0.098f, 0.133f, 0.97f);
    static readonly Color ButtonBg = Mats.Hex("#2A2733");
    static readonly Color HintBg = Mats.Hex("#2E2A38");
    static readonly Color Hairline = new Color(1f, 1f, 1f, 0.07f);
    static readonly Color IceFill = Mats.Hex("#A9E6F5");
    static readonly Color NumColor = Mats.Hex("#4F4A5A");
    static readonly Color HeaderText = Mats.Hex("#F5F1F7");
    static readonly Color ErrorRed = Mats.Hex("#FF6B6B");
    static readonly Color KeyboardBg = new Color(0.075f, 0.07f, 0.095f, 0.98f);

    Font fMed, fSemi, fBold, fMono;
    VisualElement root, header, card, hint, message, runBtn;
    CodeEditor editor;
    CodeKeyboard keyboard;
    BlockPalette palette;
    BlockDrag drag;
    NumberStepper stepper;
    // Su an acik olan yazma paneli (klavye ya da palet), kapaliyken null
    VisualElement shownPanel;
    KeyboardTier tier = KeyboardTier.Orta;
    Label chapter, title, runText, msgTag, msgTitle, msgText, msgOriginal, hintText;
    VisualElement dotsRow;
    Icon starIcon;
    readonly List<Icon> dots = new List<Icon>();
    int collected;
    bool starsOn = true;
    // Bolum bilgisi (SetLevel ile gelir)
    int levelNumber = 1, iceTotal = 3;
    string levelName = "", goal = "3 buz topla";

    public void Build()
    {
        fMed = Resources.Load<Font>("Fonts/Poppins-Medium");
        fSemi = Resources.Load<Font>("Fonts/Poppins-SemiBold");
        fBold = Resources.Load<Font>("Fonts/Poppins-Bold");
        fMono = Resources.Load<Font>("Fonts/JetBrainsMonoNL-Regular");

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

        // Ustten asagi: baslik · bos alan (oyun alani gorunur) · kod karti · yazarken kod klavyesi (Orta/Usta) ya da palet (Acemi).
        // En ustte butun ekrani kaplayan katman: suruklenen satir ve sayinin -/+ kutusu.
        header = BuildHeader();
        card = BuildBottom();
        var free = new VisualElement { pickingMode = PickingMode.Ignore };
        free.style.flexGrow = 1;
        var overlay = new VisualElement { pickingMode = PickingMode.Ignore };
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0; overlay.style.right = 0; overlay.style.top = 0; overlay.style.bottom = 0;

        keyboard = new CodeKeyboard(editor, fMono, Ink, Accent, KeyboardBg);
        keyboard.style.display = DisplayStyle.None;
        drag = new BlockDrag(overlay, card, editor, fMono, Ink, Accent, ErrorRed);
        palette = new BlockPalette(editor, drag, fMono, fMed, Ink, Accent, KeyboardBg);
        palette.style.display = DisplayStyle.None;
        stepper = new NumberStepper(editor, fSemi, Ink, Accent, ButtonBg);
        overlay.Add(stepper);

        editor.LinePicked += (line, at, pointer) => drag.BeginLine(line, at, pointer);
        editor.NumberTapped += (line, col) => { if (line >= 0) stepper.Show(line, col); else stepper.Hide(); };
        drag.Started += stepper.Hide;

        root.Add(header);
        root.Add(free);
        root.Add(card);
        root.Add(keyboard);
        root.Add(palette);
        root.Add(overlay);
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
        var menu = RoundButton(96, glass, new Icon(42, DrawMenu), () => MenuPressed?.Invoke());

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

        dotsRow = new VisualElement { pickingMode = PickingMode.Ignore };
        dotsRow.style.flexDirection = FlexDirection.Row;
        dotsRow.style.marginTop = 16;
        BuildDots();
        center.Add(dotsRow);

        starIcon = new Icon(46, DrawSparkle);
        var starBtn = RoundButton(96, glass, starIcon, () => StarsToggled?.Invoke());

        row.Add(menu);
        row.Add(center);
        row.Add(starBtn);
        h.Add(row);
        return h;
    }

    VisualElement BuildBottom()
    {
        var card = new VisualElement();
        card.style.marginLeft = 28; card.style.marginRight = 28; card.style.marginBottom = 36;
        card.style.backgroundColor = CardBg;
        Ui.Radius(card, 52);
        Ui.Border(card, 2, Hairline);
        card.style.paddingTop = 34; card.style.paddingBottom = 36;
        card.style.paddingLeft = 36; card.style.paddingRight = 36;

        var top = new VisualElement();
        top.style.flexDirection = FlexDirection.Row;
        top.style.justifyContent = Justify.SpaceBetween;
        top.style.paddingLeft = 18; top.style.paddingRight = 8;
        top.Add(Text("kod.py", fMed, 28, new Color(Ink.r, Ink.g, Ink.b, 0.45f)));
        top.Add(Text("Python", fMed, 28, new Color(Ink.r, Ink.g, Ink.b, 0.28f)));
        card.Add(top);

        editor = new CodeEditor(fMono, Ink, NumColor, Accent, ErrorRed);
        editor.style.marginTop = 14;
        editor.Changed += c => CodeChanged?.Invoke(c);
        card.Add(editor);

        var buttons = new VisualElement();
        buttons.style.flexDirection = FlexDirection.Row;
        buttons.style.alignItems = Align.Center;
        buttons.style.marginTop = 28;

        var hintBtn = RoundButton(124, ButtonBg, new Icon(58, DrawBulb), ToggleHint);
        Ui.Border(hintBtn, 2, Hairline);

        runBtn = new VisualElement();
        runBtn.style.flexGrow = 1;
        runBtn.style.height = 124;
        runBtn.style.marginLeft = 22; runBtn.style.marginRight = 22;
        runBtn.style.backgroundColor = Accent;
        Ui.Radius(runBtn, 62);
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
        Ui.Border(resetBtn, 2, Hairline);

        buttons.Add(hintBtn); buttons.Add(runBtn); buttons.Add(resetBtn);
        card.Add(buttons);

        hint = new VisualElement();
        hint.style.position = Position.Absolute;
        hint.style.left = 0; hint.style.right = 0;
        hint.style.bottom = Length.Percent(100);
        hint.style.marginBottom = 20;
        hint.style.backgroundColor = HintBg;
        Ui.Radius(hint, 36);
        Ui.Border(hint, 2, Hairline);
        hint.style.paddingTop = 30; hint.style.paddingBottom = 32;
        hint.style.paddingLeft = 40; hint.style.paddingRight = 40;
        hintText = Text("", fMed, 32, Color.white);
        hintText.enableRichText = true;
        hintText.style.whiteSpace = WhiteSpace.Normal;
        hint.Add(hintText);
        hint.style.display = DisplayStyle.None;
        card.Add(hint);

        // Hata ya da bilgi kutusu: hint ile ayni yerde, kod kartinin ustunde. Dokununca kapanir.
        message = new VisualElement();
        message.style.position = Position.Absolute;
        message.style.left = 0; message.style.right = 0;
        message.style.bottom = Length.Percent(100);
        message.style.marginBottom = 20;
        message.style.backgroundColor = HintBg;
        Ui.Radius(message, 36);
        message.style.paddingTop = 28; message.style.paddingBottom = 32;
        message.style.paddingLeft = 40; message.style.paddingRight = 40;
        msgTag = Text("", fSemi, 24, ErrorRed);
        msgTag.style.letterSpacing = 4;
        msgTitle = Text("", fBold, 40, Color.white);
        msgTitle.style.marginTop = 6;
        msgText = Text("", fMed, 31, new Color(1f, 1f, 1f, 0.88f));
        msgText.style.whiteSpace = WhiteSpace.Normal;
        msgText.style.marginTop = 10;
        msgText.enableRichText = true;
        msgOriginal = Text("", fMono, 26, new Color(1f, 1f, 1f, 0.45f));
        msgOriginal.style.whiteSpace = WhiteSpace.Normal;
        msgOriginal.style.marginTop = 16;
        msgOriginal.enableRichText = false;
        message.Add(msgTag); message.Add(msgTitle); message.Add(msgText); message.Add(msgOriginal);
        message.RegisterCallback<ClickEvent>(_ => HideMessage());
        message.style.display = DisplayStyle.None;
        card.Add(message);

        return card;
    }

    // Karttaki kodu degistirir (bolum degisti vb.); CodeChanged tetiklenmez.
    // Baslangic kodu: butun satirlar "baslangic" turunde. Kayitli kod: turleriyle (tur yoksa null -> Dugme sayilir).
    public void LoadStartCode(string source) => editor.LoadStart(source);
    public void LoadCode(string source, string kinds) => editor.Load(source, kinds);

    public string Code => editor.Text;

    // Bu bolume kadar acilmis kelimeler (oneri satiri icin)
    public void SetOpenWords(List<string> words) => keyboard.SetOpenWords(words);

    // Kod yazma kademesi: Acemi (palet, surukle-birak), Orta (klavye + oneri satiri), Usta (klavye).
    // Secme dugmesi Gorev 8'de; simdilik Orta ya da -kademe baslatma secenegi.
    public KeyboardTier Tier
    {
        get => tier;
        set
        {
            tier = value;
            editor.Blocks = value == KeyboardTier.Acemi;
            keyboard.ShowSuggestions = value == KeyboardTier.Orta;
        }
    }

    // Acemi paletinin parcalari; fresh: bu bolumde yeni olanlar (turuncu kenarli)
    public void SetPieces(IEnumerable<string> pieces, ICollection<string> fresh) => palette.SetPieces(pieces, fresh);

    // Deneme goruntuleri icin: klavyenin ikinci isaret sayfasi
    public void KeyboardMore(bool on) => keyboard.SetMore(on);
    // Satir turlerinin kayit metni (satir basina bir harf)
    public string CodeKinds => editor.Kinds;

    // Buz sayaci: bolumdeki buz kadar nokta (buz yoksa sayac gizli)
    void BuildDots()
    {
        dotsRow.Clear();
        dots.Clear();
        for (int i = 0; i < iceTotal; i++)
        {
            int idx = i;
            var d = new Icon(36, (p, r) => DrawIceDot(p, r, idx < collected));
            d.style.marginLeft = 10; d.style.marginRight = 10;
            Transition(d, "scale", 0.28f, EasingMode.EaseOutBack);
            dots.Add(d);
            dotsRow.Add(d);
        }
        dotsRow.style.display = iceTotal > 0 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // Turkce buyuk harf (i -> İ, ı -> I); telefonun dil ayarina guvenmeden
    static string Upper(string s) => s.Replace('i', 'İ').Replace('ı', 'I').ToUpperInvariant();

    string ChapterText => "BÖLÜM " + levelNumber + (levelName.Length > 0 ? "  ·  " + Upper(levelName) : "");

    // ---- Disaridan cagrilanlar ----

    // Yeni bolum: ust baslik, buz sayaci ve ipucu balonu bu bolume gore kurulur.
    public void SetLevel(int number, string name, string goalText, int ices, string hintRich)
    {
        levelNumber = number;
        levelName = name;
        goal = goalText;
        iceTotal = ices;
        collected = 0;
        BuildDots();
        stepper.Hide();
        editor.StopEditing();
        hintText.text = hintRich;
        hint.style.display = DisplayStyle.None;
        ResetView();
    }

    // Ekranda ust baslik ile alttaki kod karti arasinda kalan bos bant (0 = ekran alti, 1 = ekran ustu).
    // Kamera oyun alanini bu banda sigdirir; kod uzayip kisalinca alan kendiliginden buyur/kuculur.
    public Vector2 FreeBand()
    {
        float h = root.layout.height;
        if (float.IsNaN(h) || h <= 0f || float.IsNaN(card.layout.yMin) || float.IsNaN(header.layout.yMax))
            return new Vector2(0.3f, 0.9f);
        return new Vector2(1f - card.layout.yMin / h, 1f - header.layout.yMax / h);
    }

    // Calisan satiri isaretler (idx 0'dan baslar, -1 = hicbiri). error: satir kirmizi yanar.
    public void SetActiveLine(int idx, bool error = false) => editor.SetActiveLine(idx, error);

    // tag: kutunun ustundeki kucuk etiket ("PYTHON HATASI", "OYUN KURALI"...); original: Python'un kendi (Ingilizce) mesaji, yoksa null.
    public void ShowMessage(string tag, string heading, string text, string original, bool error)
    {
        hint.style.display = DisplayStyle.None;
        msgTag.text = tag;
        msgTag.style.color = error ? ErrorRed : IceFill;
        msgTitle.text = CodeColors.Inline(heading);
        msgText.text = CodeColors.Inline(text);
        msgOriginal.text = original ?? "";
        msgOriginal.style.display = string.IsNullOrEmpty(original) ? DisplayStyle.None : DisplayStyle.Flex;
        Ui.Border(message, 2, error ? new Color(ErrorRed.r, ErrorRed.g, ErrorRed.b, 0.55f) : Hairline);
        message.style.display = DisplayStyle.Flex;
    }

    public void HideMessage() => message.style.display = DisplayStyle.None;

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
        editor.ReadOnly = running; // calisirken kod degistirilemez
        if (running) hint.style.display = DisplayStyle.None;
    }

    // hasNext: sonraki bolum varsa dugme "Sonraki bolum" olur
    public void SetDone(bool hasNext)
    {
        chapter.text = "BÖLÜM " + levelNumber + (iceTotal > 0 ? "  ·  " + collected + "/" + iceTotal : "");
        title.text = "Tamamlandı!";
        title.style.scale = new Scale(new Vector3(1.12f, 1.12f, 1f));
        title.schedule.Execute(() => title.style.scale = new Scale(Vector3.one)).StartingIn(180);
        runText.text = hasNext ? "Sonraki bölüm" : "Tekrar";
        runBtn.style.opacity = 1f;
    }

    public void ResetView()
    {
        chapter.text = ChapterText;
        title.text = goal;
        SetCollected(0);
        SetActiveLine(-1);
        SetRunning(false);
        HideMessage();
    }

    public void SetStars(bool on)
    {
        starsOn = on;
        starIcon.MarkDirtyRepaint();
    }

    void ToggleHint()
    {
        bool show = hint.style.display == DisplayStyle.None;
        if (show) HideMessage();
        hint.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public bool Editing => editor.Editing;

    // Kod alanina odaklanir, from..to secili (esitse yalnizca imlec); deneme goruntuleri icin.
    public void FocusCode(int from, int to) => editor.StartEditing(from, to);

    public void StopEditing() => editor.StopEditing();

    // Kod alaninin ortasinin ekrandaki yeri (sol alt koseden, piksel); deneme icin tiklatmak amaciyla.
    public Vector2 CodeScreenPoint() => PanelToScreen(editor.worldBound.center);

    // Kod klavyesindeki tusun ekrandaki yeri (CodeKeyboard.KeyCenter adlari); tus gorunmuyorsa null. Deneme icin.
    public Vector2? KeyScreenPoint(string name)
    {
        var c = keyboard.KeyCenter(name);
        return c.HasValue ? PanelToScreen(c.Value) : (Vector2?)null;
    }

    // Acemi paletindeki parcanin ekrandaki yeri; palet gorunmuyorsa null. Deneme icin.
    public Vector2? PieceScreenPoint(string piece)
    {
        var c = palette.PieceCenter(piece);
        return c.HasValue ? PanelToScreen(c.Value) : (Vector2?)null;
    }

    // Deneme icin: koddaki harfin (satir, sutun 0'dan), satir arasinin ve -/+ dugmesinin ekrandaki yeri
    public Vector2 CodeCharScreenPoint(int line, int col) => PanelToScreen(editor.CharCenter(line, col));
    public Vector2 CodeGapScreenPoint(int gap, int col) => PanelToScreen(editor.GapPoint(gap, col));
    public Vector2? StepperScreenPoint(int delta)
    {
        var c = stepper.ButtonCenter(delta);
        return c.HasValue ? PanelToScreen(c.Value) : (Vector2?)null;
    }

    Vector2 PanelToScreen(Vector2 c)
    {
        float s = Screen.width / Mathf.Max(1f, root.layout.width);
        return new Vector2(c.x * s, Screen.height - c.y * s);
    }

    void Update()
    {
        if (card == null) return;
        // Yazarken kademeye gore klavye ya da palet acik; kart onun ustune oturur, oyun alani kalan yere kendiliginden sigar
        bool typing = editor.Editing;
        var panelNow = !typing ? null : tier == KeyboardTier.Acemi ? (VisualElement)palette : keyboard;
        if (panelNow != shownPanel)
        {
            if (shownPanel == keyboard) keyboard.ResetState();
            keyboard.style.display = panelNow == keyboard ? DisplayStyle.Flex : DisplayStyle.None;
            palette.style.display = panelNow == palette ? DisplayStyle.Flex : DisplayStyle.None;
            card.style.marginBottom = panelNow != null ? 18 : 36;
            shownPanel = panelNow;
        }
        if (!typing) stepper.Hide();

        // Kod karti en cok ekranin yarisi kadar olsun ve oyun alanina en az ekranin ceyregi kalsin;
        // uzun kod kart icinde kaydirilir
        float rh = root.layout.height, other = card.layout.height - editor.layout.height, top = header.layout.yMax;
        float kb = shownPanel != null && !float.IsNaN(shownPanel.layout.height) ? shownPanel.layout.height : 0f;
        if (!float.IsNaN(rh) && rh > 0f && !float.IsNaN(other) && !float.IsNaN(top))
        {
            float cardMax = Mathf.Min(rh * 0.5f, rh - kb - card.resolvedStyle.marginBottom - top - rh * 0.25f);
            editor.MaxHeight = Mathf.Max(160f, cardMax - other);
        }

        // Kod alaninin disina (oyun alanina) dokununca yazma biter; -/+ kutusunun disina dokununca kutu kapanir
        var p = UnityEngine.InputSystem.Pointer.current;
        if (editor.Editing && p != null && p.press.wasPressedThisFrame && root.panel != null && !drag.Active)
        {
            var sp = p.position.ReadValue();
            var pos = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(sp.x, Screen.height - sp.y));
            var picked = root.panel.Pick(pos);
            if (picked == null) editor.StopEditing();
            else if (stepper.Open && picked != stepper && !stepper.Contains(picked)) stepper.Hide();
        }
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
        Ui.Radius(b, size * 0.5f);
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
