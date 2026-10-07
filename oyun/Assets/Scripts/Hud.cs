using System;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Arayuz: ustte bolum + tek cumle hedef + buz sayaci, altta kod karti ve az dugme.
// Olculer 1080 genislikli telefon ekranina gore (piksel).
public class Hud : MonoBehaviour
{
    public event Action RunPressed, ResetPressed, StarsToggled, MenuPressed, SoundToggled, AnimationsToggled, PerformanceToggled;
    // Calistir'in sag ucundaki ⏭: kodu bir satir ilerletir (adim adim modu)
    public event Action StepPressed;
    // Sol ustteki kitap: kod sozlugu
    public event Action GlossaryPressed;
    // Oyuncu kodu degistirdi (yeni kodun tamami)
    public event Action<string> CodeChanged;
    // Oyuncu kademe kutusundan yeni bir kademe secti (kalici tutulmasi icin; Tier zaten degisti)
    public event Action<KeyboardTier> TierChanged;
    // Oyuncu bolum secme ekranindan bir bolum secti (bolum numarasi)
    public event Action<int> LevelPicked;
    // Oyuncu ampule basti (hangi ipucunun gorunecegine Oyun.cs karar verir)
    public event Action HintPressed;

    static readonly Color Accent = Mats.Hex("#E07A5F");
    static readonly Color Ink = Mats.Hex("#E9E4EE");
    static readonly Color CardBg = new Color(0.105f, 0.098f, 0.133f, 0.97f);
    static readonly Color ButtonBg = Mats.Hex("#2A2733");
    static readonly Color HintBg = Mats.Hex("#2E2A38");
    static readonly Color Hairline = new Color(1f, 1f, 1f, 0.07f);
    static readonly Color IceFill = Mats.Hex("#A9E6F5");
    static readonly Color CellFill = Mats.Hex("#C8F25A");
    static readonly Color PanelFill = Mats.Hex("#FFB25E");
    static readonly Color CompassFill = Mats.Hex("#E2B65C");
    static readonly Color ReelFill = Mats.Hex("#E8742E");
    static readonly Color NumColor = Mats.Hex("#4F4A5A");
    static readonly Color HeaderText = Mats.Hex("#F5F1F7");
    static readonly Color ErrorRed = Mats.Hex("#FF6B6B");
    static readonly Color KeyboardBg = new Color(0.075f, 0.07f, 0.095f, 0.98f);

    Font fMed, fSemi, fBold, fMono;
    VisualElement root, header, card, message, runBtn, stepBtn, glossaryBtn, hintBtn, overlay;
    GlossaryView glossary;
    HintPanel hint;
    CodeEditor editor;
    CodeKeyboard keyboard;
    BlockPalette palette;
    BlockDrag drag;
    NumberStepper stepper;
    TierMenu tierMenu;
    LevelSelect levelSelect;
    OpeningScene opening;
    RecordingScene recording;
    QuizView quiz;
    // Su an acik olan yazma paneli (klavye ya da palet), kapaliyken null
    VisualElement shownPanel;
    KeyboardTier tier = KeyboardTier.Orta;
    Label chapter, title, runText, msgTag, msgTitle, msgText, msgOriginal, xpTotalLabel, xpDoneLabel, xpNextLabel, outroLabel, introText;
    VisualElement dotsRow;
    IVisualElementScheduledItem chapterRewrite;   // RewriteChapter suruyorsa (ResetView durdurur)
    Icon starIcon, soundIcon;
    bool soundOn = true;
    readonly List<Icon> dots = new List<Icon>();
    int collected;
    bool starsOn = true;
    // Bolum bilgisi (SetLevel ile gelir)
    int levelNumber = 1, iceTotal = 3;
    Collectible item = Collectible.Ice;
    string levelName = "", goal = "3 buz topla", levelLabel = "BÖLÜM";

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
        // En ustte butun ekrani kaplayan katman: suruklenen satir, sayinin -/+ kutusu ve kademe kutusu (hepsi kartin ustunde gorunmeli).
        header = BuildHeader();
        var free = new VisualElement { pickingMode = PickingMode.Ignore };
        free.style.flexGrow = 1;
        overlay = new VisualElement { pickingMode = PickingMode.Ignore };
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0; overlay.style.right = 0; overlay.style.top = 0; overlay.style.bottom = 0;

        card = BuildBottom();

        keyboard = new CodeKeyboard(editor, fMono, Ink, Accent, KeyboardBg);
        keyboard.style.display = DisplayStyle.None;
        drag = new BlockDrag(overlay, card, editor, fMono, Ink, Accent, ErrorRed);
        palette = new BlockPalette(editor, drag, fMono, fMed, Ink, Accent, ErrorRed, KeyboardBg);
        drag.AttachPalette(palette);
        palette.style.display = DisplayStyle.None;
        stepper = new NumberStepper(editor, fSemi, Ink, Accent, ButtonBg);
        overlay.Add(stepper);

        editor.LinePicked += (line, at, pointer) => drag.BeginLine(line, at, pointer);
        editor.NumberTapped += (line, col) => { if (line >= 0) stepper.Show(line, col); else stepper.Hide(); };
        drag.Started += stepper.Hide;

        // Bolum secme ekrani en ustte: butun ekrani kaplar (sol ustteki dugmeyle acilir)
        levelSelect = new LevelSelect(fMed, fSemi, fBold, Ink, Accent, ButtonBg, Hairline);
        levelSelect.Picked += n => LevelPicked?.Invoke(n);
        levelSelect.AnimationsToggled += () => AnimationsToggled?.Invoke();
        levelSelect.PerformanceToggled += () => PerformanceToggled?.Invoke();
        overlay.Add(levelSelect);
        glossary = new GlossaryView(fMed, fSemi, fBold, fMono, Ink, Accent, ButtonBg, Hairline);
        overlay.Add(glossary);
        opening = new OpeningScene(fBold);
        overlay.Add(opening);
        recording = new RecordingScene(fMed, fMono);
        overlay.Add(recording);
        quiz = new QuizView(fMed, fSemi, fBold, Ink, Accent, ButtonBg, Hairline, ErrorRed, CellFill);
        overlay.Add(quiz);

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
        // Sol sutun: bolumler, altinda kod sozlugu (her an acilabilir)
        var menuCol = new VisualElement { pickingMode = PickingMode.Ignore };
        menuCol.Add(RoundButton(96, glass, new Icon(42, DrawMenu), () => MenuPressed?.Invoke()));
        glossaryBtn = RoundButton(96, glass, new Icon(46, DrawBook), () => GlossaryPressed?.Invoke());
        glossaryBtn.style.marginTop = 20;
        menuCol.Add(glossaryBtn);
        soundIcon = new Icon(44, DrawSpeaker);
        var soundBtn = RoundButton(96, glass, soundIcon, () => SoundToggled?.Invoke());
        soundBtn.style.marginTop = 20;
        menuCol.Add(soundBtn);

        var center = new VisualElement { pickingMode = PickingMode.Ignore };
        center.style.flexGrow = 1;
        center.style.alignItems = Align.Center;
        center.style.paddingTop = 2;
        chapter = Text("BÖLÜM 1", fSemi, 30, new Color(1f, 1f, 1f, 0.62f));
        chapter.style.letterSpacing = 6;
        title = Text("3 buz topla", fBold, 68, HeaderText);
        title.style.marginTop = -4;
        Ui.Transition(title, "scale", 0.35f, EasingMode.EaseOutBack);
        center.Add(chapter);
        center.Add(title);

        introText = Text("", fMed, 30, new Color(1f, 1f, 1f, 0.78f));
        introText.style.marginTop = 10;
        introText.style.maxWidth = 620;
        introText.style.whiteSpace = WhiteSpace.Normal;
        introText.style.unityTextAlign = TextAnchor.UpperCenter;
        introText.style.display = DisplayStyle.None;
        Ui.Transition(introText, "opacity", 0.5f, EasingMode.EaseOutSine);
        center.Add(introText);

        outroLabel = Text("", fMed, 30, new Color(1f, 1f, 1f, 0.78f));
        outroLabel.style.marginTop = 10;
        outroLabel.style.maxWidth = 620;
        outroLabel.style.whiteSpace = WhiteSpace.Normal;
        outroLabel.style.unityTextAlign = TextAnchor.UpperCenter;
        outroLabel.style.display = DisplayStyle.None;
        center.Add(outroLabel);

        dotsRow = new VisualElement { pickingMode = PickingMode.Ignore };
        dotsRow.style.flexDirection = FlexDirection.Row;
        dotsRow.style.marginTop = 16;
        BuildDots();
        center.Add(dotsRow);

        xpDoneLabel = Text("", fSemi, 32, HeaderText);
        xpDoneLabel.style.marginTop = 14;
        xpDoneLabel.style.display = DisplayStyle.None;
        center.Add(xpDoneLabel);

        xpNextLabel = Text("", fMed, 27, new Color(1f, 1f, 1f, 0.55f));
        xpNextLabel.style.marginTop = 6;
        xpNextLabel.style.display = DisplayStyle.None;
        center.Add(xpNextLabel);

        starIcon = new Icon(46, DrawSparkle);
        var starBtn = RoundButton(96, glass, starIcon, () => StarsToggled?.Invoke());
        var starCol = new VisualElement { pickingMode = PickingMode.Ignore };
        starCol.style.alignItems = Align.Center;
        xpTotalLabel = Text("0 XP", fSemi, 24, new Color(1f, 1f, 1f, 0.5f));
        xpTotalLabel.style.marginTop = 10;
        starCol.Add(starBtn);
        starCol.Add(xpTotalLabel);
        // pusula: XP'nin altinda, yerlesimi etkilemez (baslik uzayip oyun alani kuculmesin)
        var compass = new Compass(92, fSemi);
        compass.style.position = Position.Absolute;
        compass.style.left = 2;
        compass.style.top = 96 + 10 + 40 + 14;
        starCol.Add(compass);

        row.Add(menuCol);
        row.Add(center);
        row.Add(starCol);
        h.Add(row);

        // Ipucu balonu: baslikla ayni yerde, soldaki ve sagdaki dugmelerin arasinda (yerlesimi etkilemez, oyun alani kuculmez)
        hint = new HintPanel(fMed, fSemi, HintBg, Hairline, SafeTop() + 32, 44 + 96 + 20);
        hint.ClosePressed += hint.Hide;
        h.Add(hint);
        return h;
    }

    // "Program giris" metnini basligin altinda kisa sure gosterir, sonra kendiliginden kaybolur.
    void ShowIntro(string text)
    {
        introText.style.display = DisplayStyle.None;
        if (string.IsNullOrEmpty(text)) return;
        introText.text = text;
        introText.style.opacity = 1f;
        introText.style.display = DisplayStyle.Flex;
        introText.schedule.Execute(() => introText.style.opacity = 0f).StartingIn(2600);
        introText.schedule.Execute(() => introText.style.display = DisplayStyle.None).StartingIn(3100);
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
        tierMenu = new TierMenu(overlay, fMed, fSemi, Ink, Accent, KeyboardBg, Hairline);
        tierMenu.Picked += t => { Tier = t; TierChanged?.Invoke(t); };
        top.Add(tierMenu);
        card.Add(top);

        editor = new CodeEditor(fMono, Ink, NumColor, Accent, ErrorRed);
        editor.style.marginTop = 14;
        editor.Changed += c => CodeChanged?.Invoke(c);
        card.Add(editor);

        var buttons = new VisualElement();
        buttons.style.flexDirection = FlexDirection.Row;
        buttons.style.alignItems = Align.Center;
        buttons.style.marginTop = 28;

        hintBtn = RoundButton(124, ButtonBg, new Icon(58, DrawBulb), () => HintPressed?.Invoke());
        Ui.Border(hintBtn, 2, Hairline);

        runBtn = new VisualElement();
        runBtn.style.flexGrow = 1;
        runBtn.style.height = 124;
        runBtn.style.marginLeft = 22; runBtn.style.marginRight = 22;
        runBtn.style.backgroundColor = Accent;
        Ui.Radius(runBtn, 62);
        runBtn.style.flexDirection = FlexDirection.Row;
        runBtn.style.alignItems = Align.Stretch;
        // Sol: Calistir (hepsi normal hizda). Sag uc: ⏭ adim (her basista bir satir); ayri dugme yok, alt sira sikismasin.
        var runMain = new VisualElement();
        runMain.style.flexGrow = 1;
        runMain.style.flexDirection = FlexDirection.Row;
        runMain.style.alignItems = Align.Center;
        runMain.style.justifyContent = Justify.Center;
        runMain.Add(new Icon(38, DrawPlay));
        runText = Text("Çalıştır", fSemi, 42, Color.white);
        runText.style.marginLeft = 16;
        runMain.Add(runText);
        runMain.RegisterCallback<ClickEvent>(_ => { Sound.Tap(); RunPressed?.Invoke(); });
        runBtn.Add(runMain);
        stepBtn = new VisualElement();
        stepBtn.style.width = 116;
        stepBtn.style.alignItems = Align.Center;
        stepBtn.style.justifyContent = Justify.Center;
        stepBtn.style.borderLeftWidth = 3;
        stepBtn.style.borderLeftColor = new Color(1f, 1f, 1f, 0.35f);
        stepBtn.style.marginTop = 26; stepBtn.style.marginBottom = 26;
        stepBtn.Add(new Icon(40, DrawStep));
        stepBtn.RegisterCallback<ClickEvent>(_ => { Sound.Tap(); StepPressed?.Invoke(); });
        runBtn.Add(stepBtn);
        Pressable(runBtn, null);
        Ui.Transition(runBtn, "opacity", 0.2f, EasingMode.EaseOut);

        var resetBtn = RoundButton(124, ButtonBg, new Icon(56, DrawReset), () => ResetPressed?.Invoke());
        Ui.Border(resetBtn, 2, Hairline);

        // Kademe kutusunun ikinci kisayolu; yeri tasarim belgesi K6: ipucu · klavye · Calistir · bastan al
        var tierBtn = RoundButton(124, ButtonBg, new Icon(62, DrawKeyboard), null);
        Ui.Border(tierBtn, 2, Hairline);
        tierBtn.style.marginLeft = 22;
        tierMenu.SetShortcut(tierBtn);

        buttons.Add(hintBtn); buttons.Add(tierBtn); buttons.Add(runBtn); buttons.Add(resetBtn);
        card.Add(buttons);

        // Hata ya da bilgi kutusu: kod kartinin ustunde. Dokununca kapanir.
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
    // Kart sag ustundeki kademe kutusundan (TierMenu) ya da -kademe baslatma secenegiyle degisir; kalicilik Oyun.cs'te.
    public KeyboardTier Tier
    {
        get => tier;
        set
        {
            tier = value;
            editor.Blocks = value == KeyboardTier.Acemi;
            keyboard.ShowSuggestions = value == KeyboardTier.Orta;
            tierMenu.SetTier(value);
        }
    }

    // Bolumde bu kademeden XP alinip alinmadigini sorar (kademe kutusundaki ✓ icin); Oyun.cs kurar.
    public Func<KeyboardTier, bool> TierEarned { set => tierMenu.Earned = value; }

    // Oyuncunun kodu + imlec + secim + satir turleri (Xp.SolutionKind icin)
    public CodeBuffer Buffer => editor.Buffer;

    // Acemi paletinin parcalari; fresh: bu bolumde yeni olanlar (turuncu kenarli)
    public void SetPieces(IEnumerable<string> pieces, ICollection<string> fresh) => palette.SetPieces(pieces, fresh);

    // Deneme goruntuleri icin: klavyenin ikinci isaret sayfasi
    public void KeyboardMore(bool on) => keyboard.SetMore(on);
    // Deneme goruntuleri icin: kademe kutusunu acar/kapatir
    public void ShowTierMenu(bool on, bool fromShortcut = false)
    {
        if (!on) tierMenu.Hide();
        else if (fromShortcut) tierMenu.ShowFromShortcut();
        else tierMenu.Show();
    }
    // Satir turlerinin kayit metni (satir basina bir harf)
    public string CodeKinds => editor.Kinds;

    // Toplama sayaci: bolumdeki buz/hucre kadar simge (yoksa sayac gizli); simge toplanan ture gore
    void BuildDots()
    {
        dotsRow.Clear();
        dots.Clear();
        for (int i = 0; i < iceTotal; i++)
        {
            int idx = i;
            var d = item == Collectible.EnergyCell ? new Icon(36, (p, r) => DrawCellDot(p, r, idx < collected))
                : item == Collectible.PanelPart ? new Icon(36, (p, r) => DrawPanelDot(p, r, idx < collected))
                : item == Collectible.CompassPart ? new Icon(36, (p, r) => DrawCompassDot(p, r, idx < collected))
                : item == Collectible.CableReel ? new Icon(36, (p, r) => DrawReelDot(p, r, idx < collected))
                : new Icon(36, (p, r) => DrawIceDot(p, r, idx < collected));
            d.style.marginLeft = 10; d.style.marginRight = 10;
            Ui.Transition(d, "scale", 0.28f, EasingMode.EaseOutBack);
            dots.Add(d);
            dotsRow.Add(d);
        }
        dotsRow.style.display = iceTotal > 0 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // Turkce buyuk harf (i -> İ, ı -> I); telefonun dil ayarina guvenmeden
    static string Upper(string s) => s.Replace('i', 'İ').Replace('ı', 'I').ToUpperInvariant();

    string ChapterText => levelLabel + " " + levelNumber + (levelName.Length > 0 ? "  ·  " + Upper(levelName) : "");

    // ---- Disaridan cagrilanlar ----

    // Yeni bolum: ust baslik ve toplama sayaci bu bolume gore kurulur, ipucu balonu kapanir.
    // intro: "program metni" (giris); bosa boslukla kisa sure gorunur, bosta hic gorunmez.
    public void SetLevel(int number, string label, string name, string goalText, int ices, Collectible kind, string intro = "")
    {
        item = kind;
        levelNumber = number;
        levelLabel = label;
        levelName = name;
        goal = goalText;
        iceTotal = ices;
        collected = 0;
        BuildDots();
        stepper.Hide();
        tierMenu.Hide();
        xpDoneLabel.style.display = DisplayStyle.None;
        xpNextLabel.style.display = DisplayStyle.None;
        outroLabel.style.display = DisplayStyle.None;
        editor.StopEditing();
        hint.Hide();
        ResetView();
        ShowIntro(intro);
    }

    // Ekranda ust baslik ile alttaki kod karti arasinda kalan bos bant (0 = ekran alti, 1 = ekran ustu).
    // Kamera oyun alanini bu banda sigdirir; kod uzayip kisalinca alan kendiliginden buyur/kuculur.
    // Kartin ustundeki hata kutusu aciksa bant onun ustunde biter, ipucu balonu aciksa bant onun altinda baslar
    // (ikisi de alani kucultur ama ortmez; uzun ipucu + klavye birlikte acikken alan asiri sikismasin diye).
    public Vector2 FreeBand()
    {
        float h = root.layout.height;
        if (float.IsNaN(h) || h <= 0f || float.IsNaN(card.layout.yMin) || float.IsNaN(header.layout.yMax))
            return new Vector2(0.3f, 0.9f);
        float bottom = card.layout.yMin;
        if (message.style.display == DisplayStyle.Flex) bottom = Mathf.Min(bottom, TopInRoot(message));
        float top = header.layout.yMax;
        if (hint.Open) top = Mathf.Max(top, BottomInRoot(hint));
        return new Vector2(1f - bottom / h, 1f - top / h);
    }

    // Ogenin ust kenari, root icinde (henuz yerlesmediyse cok buyuk sayi: hesaba katilmaz)
    float TopInRoot(VisualElement e)
    {
        float y = e.worldBound.yMin - root.worldBound.yMin;
        return float.IsNaN(y) ? float.MaxValue : y;
    }

    // Ogenin alt kenari, root icinde (henuz yerlesmediyse cok kucuk sayi: hesaba katilmaz)
    float BottomInRoot(VisualElement e)
    {
        float y = e.worldBound.yMax - root.worldBound.yMin;
        return float.IsNaN(y) ? float.MinValue : y;
    }

    // Calisan satiri isaretler (idx 0'dan baslar, -1 = hicbiri). error: satir kirmizi yanar.
    // vars: satirin sagindaki degiskenler ("i = 2"), yoksa null.
    public void SetActiveLine(int idx, bool error = false, string vars = null) => editor.SetActiveLine(idx, error, vars);

    // tag: kutunun ustundeki kucuk etiket ("PYTHON HATASI", "OYUN KURALI"...); original: Python'un kendi (Ingilizce) mesaji, yoksa null.
    public void ShowMessage(string tag, string heading, string text, string original, bool error)
    {
        hint.Hide();
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

    // paused: adim adim modunda bir sonraki basisi bekliyor (Calistir "Devam" olur, kalanini normal hizda calistirir)
    public void SetRunning(bool running, bool paused = false)
    {
        runText.text = !running ? "Çalıştır" : paused ? "Devam" : "Çalışıyor…";
        runBtn.style.opacity = running && !paused ? 0.6f : 1f;
        stepBtn.style.display = DisplayStyle.Flex;
        editor.ReadOnly = running; // calisirken kod degistirilemez
        if (running) hint.Hide();
    }

    // hasNext: sonraki bolum varsa dugme "Sonraki bolum" olur.
    // xpLine: "+50 XP · Orta ile çözdün" gibi (Xp mantığı Oyun.cs'te kurulur, burada yalnızca gösterilir).
    // nextBetter: "Usta ile çözersen +100 XP daha" ya da bos (alinacak daha zor kademe kalmadiysa).
    // outro: "program metni" (bitis); bos olabilir.
    public void SetDone(bool hasNext, string xpLine, string nextBetter, string outro = "")
    {
        chapter.text = levelLabel + " " + levelNumber + (iceTotal > 0 ? "  ·  " + collected + "/" + iceTotal : "");
        title.text = "Tamamlandı!";
        title.style.scale = new Scale(new Vector3(1.12f, 1.12f, 1f));
        title.schedule.Execute(() => title.style.scale = new Scale(Vector3.one)).StartingIn(180);
        runText.text = hasNext ? "Sonraki bölüm" : "Tekrar";
        runBtn.style.opacity = 1f;
        stepBtn.style.display = DisplayStyle.None; // bolum bitti: adimlanacak bir sey yok
        outroLabel.text = outro;
        outroLabel.style.display = string.IsNullOrEmpty(outro) ? DisplayStyle.None : DisplayStyle.Flex;
        xpDoneLabel.text = xpLine;
        xpDoneLabel.style.display = string.IsNullOrEmpty(xpLine) ? DisplayStyle.None : DisplayStyle.Flex;
        xpNextLabel.text = nextBetter;
        xpNextLabel.style.display = string.IsNullOrEmpty(nextBetter) ? DisplayStyle.None : DisplayStyle.Flex;
    }

    // Bolum secme ekranini acar; satirlar (cozuldu mu, kac XP) Oyun.cs'ten gelir. Acikken yazma ve acilir kutular kapanir.
    public void ShowLevelSelect(IReadOnlyList<LevelSelect.Entry> entries)
    {
        CloseTransient();
        levelSelect.Show(entries);
    }

    // Kod sozlugunu acar; sayfalar ve hangi bolumde acildiklari Oyun.cs'ten gelir.
    public void ShowGlossary(IReadOnlyList<GlossaryView.Entry> entries)
    {
        CloseTransient();
        glossary.Show(entries);
    }

    public void HideGlossary() => glossary.Hide();

    // Tam ekran bir katman acilmadan once: yazma ve acilir kutular kapanir
    void CloseTransient()
    {
        editor.StopEditing();
        stepper.Hide();
        tierMenu.Hide();
        hint.Hide();
    }

    public void HideLevelSelect() => levelSelect.Hide();

    // Acilis sahnesi (Bolum 1 oncesi, bir kez); atlanabilir, bitince onDone cagrilir.
    public void ShowOpening(Action onDone) => opening.Show(onDone);
    public void ShowTransition(string[] texts, Action onDone) => opening.Show(texts, -1, onDone);

    // Bolum 50 kapanisi: kararan ekranda kaydin satirlari (RecordingScene). Bitince RecordingFinished; HideRecording ekrani acar.
    public void ShowRecording(string file, string[] lines)
    {
        CloseTransient();
        recording.Show(file, lines);
    }
    public bool RecordingFinished => recording.Finished;
    public void HideRecording() => recording.Hide();

    // Bolum 50 kapanisi, arayuz degisir: ustteki "ÖDEV 50" harf harf silinir, yerine yeni yazi harf harf yazilir;
    // XP satirlari kalkar (o an oyun degil, hikaye konusur). Bolum yeniden kurulunca (ResetView) eski haline doner.
    public void RewriteChapter(string text)
    {
        xpDoneLabel.style.display = DisplayStyle.None;
        xpNextLabel.style.display = DisplayStyle.None;
        string from = chapter.text;
        int step = 0, erase = from.Length;
        chapterRewrite?.Pause();
        chapterRewrite = chapter.schedule.Execute(() =>
        {
            step++;
            chapter.text = step <= erase ? from.Substring(0, erase - step) : text.Substring(0, Mathf.Min(step - erase, text.Length));
        }).Every(70).Until(() => step >= erase + text.Length);
    }

    // Mini sinav (her 5 bolumden sonra, zorunlu); tum sorular dogru cevaplaninca onPassed cagrilir.
    public void ShowQuiz(Quiz q, Action onPassed) => quiz.Show(q, onPassed);
    public bool QuizOpen => quiz.Open;
    public Vector2? QuizChoiceScreenPoint(int i) => quiz.ChoiceCenter(i) is Vector2 c ? PanelToScreen(c) : (Vector2?)null;
    public Vector2? QuizContinueScreenPoint() => quiz.ContinueCenter() is Vector2 c ? PanelToScreen(c) : (Vector2?)null;

    // Ust basliktaki toplam XP sayaci; Oyun.cs bolum yuklendiginde ve XP kazanildiginda cagirir.
    public void SetTotalXp(int total) => xpTotalLabel.text = total + " XP";

    // Bolum sonu ekranindaki program metnini degistirir (kapanis sahnesi gibi sonradan gelen bir satir icin).
    public void SetOutro(string text)
    {
        outroLabel.text = text;
        outroLabel.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void ResetView()
    {
        chapterRewrite?.Pause();
        chapter.text = ChapterText;
        title.text = goal;
        xpDoneLabel.style.display = DisplayStyle.None;
        xpNextLabel.style.display = DisplayStyle.None;
        outroLabel.style.display = DisplayStyle.None;
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

    public void SetSound(bool on)
    {
        soundOn = on;
        soundIcon.MarkDirtyRepaint();
    }

    public void SetAnimations(bool on) => levelSelect.SetAnimations(on);
    public void SetPerformance(bool on) => levelSelect.SetPerformance(on);

    // Olcum icin (-arayuz yok): arayuz cizilmez ama yerlesimi durur, sahnenin kamera hesabi degismez
    public void HideForMeasurement() => root.visible = false;

    public bool HintOpen => hint.Open;

    // Ipucu balonunu gosterir: n. ipucu (1'den), toplam total (acilinca hata kutusu kapanir)
    public void ShowHint(string text, int n, int total)
    {
        HideMessage();
        hint.Show(text, n, total);
    }

    public void HideHint() => hint.Hide();

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

    // Acemi paletindeki Sil dugmesinin ekrandaki yeri; palet gorunmuyorsa null. Deneme icin.
    public Vector2? TrashScreenPoint()
    {
        var c = palette.TrashCenter();
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

    // Sozluk: kitap dugmesi, kisayol ve kartin yeri; sozluk acik mi. Deneme icin.
    public Vector2 GlossaryButtonScreenPoint() => PanelToScreen(glossaryBtn.worldBound.center);
    public bool GlossaryOpen => glossary.Open;
    public Vector2? GlossaryShortcutScreenPoint(string title)
    {
        var c = glossary.ShortcutCenter(title);
        return c.HasValue ? PanelToScreen(c.Value) : (Vector2?)null;
    }
    public bool GlossaryCardInView(string title) => glossary.CardInView(title);

    // Calistir ve ⏭ bolmelerinin ekrandaki yeri. Deneme icin.
    public Vector2 RunScreenPoint() => PanelToScreen(runText.parent.worldBound.center);
    public Vector2 StepScreenPoint() => PanelToScreen(stepBtn.worldBound.center);

    // Ampul dugmesinin ve (balon aciksa) balondaki × dugmesinin ekrandaki yeri. Deneme icin.
    public Vector2 HintButtonScreenPoint() => PanelToScreen(hintBtn.worldBound.center);
    public Vector2? HintCloseScreenPoint()
    {
        var c = hint.CloseCenter();
        return c.HasValue ? PanelToScreen(c.Value) : (Vector2?)null;
    }

    // Bolum secme ekranindaki satirin ekrandaki yeri; ekran kapaliysa null. Deneme icin.
    public Vector2? LevelRowScreenPoint(int number)
    {
        var c = levelSelect.RowCenter(number);
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
        // Kademe kutusu acikken disina dokununca kapanir (yazarken olup olmamasindan bagimsiz)
        if (tierMenu.Open && p != null && p.press.wasPressedThisFrame && root.panel != null)
        {
            var sp2 = p.position.ReadValue();
            var pos2 = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(sp2.x, Screen.height - sp2.y));
            var picked2 = root.panel.Pick(pos2);
            if (picked2 == null || !tierMenu.Owns(picked2)) tierMenu.Hide();
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
        Pressable(b, onClick == null ? (Action)null : () => { Sound.Tap(); onClick(); });
        return b;
    }

    static void Pressable(VisualElement b, Action onClick)
    {
        Ui.Transition(b, "scale", 0.12f, EasingMode.EaseOut);
        b.RegisterCallback<PointerDownEvent>(_ => b.style.scale = new Scale(new Vector3(0.94f, 0.94f, 1f)));
        b.RegisterCallback<PointerUpEvent>(_ => b.style.scale = new Scale(Vector3.one));
        b.RegisterCallback<PointerLeaveEvent>(_ => b.style.scale = new Scale(Vector3.one));
        if (onClick != null) b.RegisterCallback<ClickEvent>(_ => onClick());
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

    // Hoparlor: govde + huni; aciksa iki ses dalgasi, kapaliysa carpi isareti.
    void DrawSpeaker(Painter2D p, Rect r)
    {
        float w = r.width, h = r.height;
        var color = new Color(1f, 1f, 1f, soundOn ? 0.95f : 0.55f);
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.14f, h * 0.38f));
        p.LineTo(new Vector2(w * 0.34f, h * 0.38f));
        p.LineTo(new Vector2(w * 0.56f, h * 0.18f));
        p.LineTo(new Vector2(w * 0.56f, h * 0.82f));
        p.LineTo(new Vector2(w * 0.34f, h * 0.62f));
        p.LineTo(new Vector2(w * 0.14f, h * 0.62f));
        p.ClosePath();
        p.fillColor = color;
        p.Fill();
        if (soundOn)
        {
            p.lineCap = LineCap.Round;
            p.lineWidth = 3f;
            p.strokeColor = color;
            p.BeginPath();
            p.Arc(new Vector2(w * 0.56f, h * 0.5f), w * 0.16f, Deg(-40), Deg(40));
            p.Stroke();
            p.BeginPath();
            p.Arc(new Vector2(w * 0.56f, h * 0.5f), w * 0.3f, Deg(-40), Deg(40));
            p.Stroke();
        }
        else
        {
            p.lineCap = LineCap.Round;
            p.lineWidth = 3.5f;
            p.strokeColor = color;
            p.BeginPath();
            p.MoveTo(new Vector2(w * 0.64f, h * 0.3f));
            p.LineTo(new Vector2(w * 0.92f, h * 0.7f));
            p.MoveTo(new Vector2(w * 0.92f, h * 0.3f));
            p.LineTo(new Vector2(w * 0.64f, h * 0.7f));
            p.Stroke();
        }
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

    // Pil: dik, kosesi yuvarlak govde + ustte kucuk kutup ucu
    static void DrawCellDot(Painter2D p, Rect r, bool filled)
    {
        float w = r.width, h = r.height;
        var color = filled ? CellFill : new Color(1f, 1f, 1f, 0.5f);
        p.lineJoin = LineJoin.Round;
        p.lineWidth = 3f;
        p.strokeColor = color;
        RoundedRect(p, new Rect(w * 0.27f, h * 0.18f, w * 0.46f, h * 0.78f), w * 0.08f);
        if (filled) { p.fillColor = color; p.Fill(); }
        p.Stroke();
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.40f, h * 0.17f));
        p.LineTo(new Vector2(w * 0.40f, h * 0.06f));
        p.LineTo(new Vector2(w * 0.60f, h * 0.06f));
        p.LineTo(new Vector2(w * 0.60f, h * 0.17f));
        p.fillColor = color;
        p.Fill();
        p.Stroke();
    }

    // Panel parcasi: egik dortgen cam + ortasindan gecen hucre cizgisi
    static void DrawPanelDot(Painter2D p, Rect r, bool filled)
    {
        float w = r.width, h = r.height;
        var color = filled ? PanelFill : new Color(1f, 1f, 1f, 0.5f);
        p.lineJoin = LineJoin.Round;
        p.lineWidth = 3f;
        p.strokeColor = color;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.24f, h * 0.22f));
        p.LineTo(new Vector2(w * 0.86f, h * 0.22f));
        p.LineTo(new Vector2(w * 0.76f, h * 0.78f));
        p.LineTo(new Vector2(w * 0.14f, h * 0.78f));
        p.ClosePath();
        if (filled) { p.fillColor = color; p.Fill(); }
        p.Stroke();
        p.strokeColor = filled ? new Color(0.13f, 0.25f, 0.48f) : color;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.55f, h * 0.22f));
        p.LineTo(new Vector2(w * 0.45f, h * 0.78f));
        p.Stroke();
    }

    // Pusula parcasi: yuvarlak kadran + ortasindan gecen ince ibre (dolu iken kadran pirinc, ibre koyu)
    static void DrawCompassDot(Painter2D p, Rect r, bool filled)
    {
        float w = r.width, h = r.height;
        var color = filled ? CompassFill : new Color(1f, 1f, 1f, 0.5f);
        p.lineJoin = LineJoin.Round;
        p.lineWidth = 3f;
        p.strokeColor = color;
        p.BeginPath();
        p.Arc(new Vector2(w * 0.5f, h * 0.5f), w * 0.36f, Deg(0), Deg(360));
        if (filled) { p.fillColor = color; p.Fill(); }
        p.Stroke();
        p.fillColor = filled ? new Color(0.17f, 0.16f, 0.19f) : color;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.5f, h * 0.2f));
        p.LineTo(new Vector2(w * 0.6f, h * 0.5f));
        p.LineTo(new Vector2(w * 0.5f, h * 0.8f));
        p.LineTo(new Vector2(w * 0.4f, h * 0.5f));
        p.ClosePath();
        p.Fill();
    }

    // Kablo makarasi: yandan gorunen makara (iki kenar cizgisi arasinda sarili kablo)
    static void DrawReelDot(Painter2D p, Rect r, bool filled)
    {
        float w = r.width, h = r.height;
        var color = filled ? ReelFill : new Color(1f, 1f, 1f, 0.5f);
        p.lineCap = LineCap.Round;
        p.lineWidth = 3f;
        p.strokeColor = color;
        var drum = new Rect(w * 0.3f, h * 0.3f, w * 0.4f, h * 0.4f);
        RoundedRect(p, drum, w * 0.05f);
        if (filled) { p.fillColor = color; p.Fill(); }
        p.Stroke();
        p.strokeColor = filled ? new Color(0.78f, 0.8f, 0.84f) : color;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.24f, h * 0.18f)); p.LineTo(new Vector2(w * 0.24f, h * 0.82f));
        p.MoveTo(new Vector2(w * 0.76f, h * 0.18f)); p.LineTo(new Vector2(w * 0.76f, h * 0.82f));
        p.Stroke();
    }

    // Kosesi c yaricapla yuvarlanmis dikdortgen yolu (cizmez; ardindan Fill/Stroke)
    static void RoundedRect(Painter2D p, Rect b, float c)
    {
        float x0 = b.xMin, x1 = b.xMax, y0 = b.yMin, y1 = b.yMax;
        p.BeginPath();
        p.MoveTo(new Vector2(x0 + c, y0));
        p.LineTo(new Vector2(x1 - c, y0));
        p.ArcTo(new Vector2(x1, y0), new Vector2(x1, y0 + c), c);
        p.LineTo(new Vector2(x1, y1 - c));
        p.ArcTo(new Vector2(x1, y1), new Vector2(x1 - c, y1), c);
        p.LineTo(new Vector2(x0 + c, y1));
        p.ArcTo(new Vector2(x0, y1), new Vector2(x0, y1 - c), c);
        p.LineTo(new Vector2(x0, y0 + c));
        p.ArcTo(new Vector2(x0, y0), new Vector2(x0 + c, y0), c);
        p.ClosePath();
    }

    // Acik kitap: iki sayfa, ortada sirt
    static void DrawBook(Painter2D p, Rect r)
    {
        float w = r.width, h = r.height;
        p.strokeColor = new Color(1f, 1f, 1f, 0.92f);
        p.lineWidth = w * 0.08f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
        // iki sayfa: sirttan disa dogru hafif yukselen dortgenler
        foreach (float side in new[] { -1f, 1f })
        {
            float outer = 0.5f + side * 0.42f;
            p.BeginPath();
            p.MoveTo(new Vector2(w * 0.5f, h * 0.3f));
            p.LineTo(new Vector2(w * outer, h * 0.2f));
            p.LineTo(new Vector2(w * outer, h * 0.72f));
            p.LineTo(new Vector2(w * 0.5f, h * 0.82f));
            p.ClosePath();
            p.Stroke();
        }
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

    static void DrawKeyboard(Painter2D p, Rect r)
    {
        float w = r.width, h = r.height;
        p.strokeColor = Ink;
        p.lineWidth = w * 0.075f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
        // govde
        RoundedRect(p, new Rect(w * 0.06f, h * 0.24f, w * 0.88f, h * 0.52f), w * 0.1f);
        p.Stroke();
        // tuslar: iki sira nokta + bosluk cubugu
        p.fillColor = Ink;
        for (int i = 0; i < 4; i++)
        {
            float x = w * (0.24f + 0.173f * i);
            foreach (float y in new[] { 0.41f, 0.54f })
            {
                p.BeginPath();
                p.Arc(new Vector2(x, h * y), w * 0.036f, Deg(0), Deg(360));
                p.Fill();
            }
        }
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.3f, h * 0.65f));
        p.LineTo(new Vector2(w * 0.7f, h * 0.65f));
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

    // ⏭: ucgen + dik cizgi (bir satir ilerle)
    static void DrawStep(Painter2D p, Rect r)
    {
        float s = r.width;
        p.fillColor = Color.white;
        p.strokeColor = Color.white;
        p.lineJoin = LineJoin.Round;
        p.lineCap = LineCap.Round;
        p.lineWidth = s * 0.1f;
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.14f, s * 0.16f));
        p.LineTo(new Vector2(s * 0.64f, s * 0.5f));
        p.LineTo(new Vector2(s * 0.14f, s * 0.84f));
        p.ClosePath();
        p.Fill();
        p.Stroke();
        p.lineWidth = s * 0.13f;
        p.BeginPath();
        p.MoveTo(new Vector2(s * 0.84f, s * 0.16f));
        p.LineTo(new Vector2(s * 0.84f, s * 0.84f));
        p.Stroke();
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
