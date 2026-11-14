using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Istasyon terminali (ekran bulmacasi, Bolge 8; docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md, K2): koyu cam, kehribar
// yazi, koyu metal cerceve, cok soluk tarama cizgileri. Satir satir "girdi → cevap ✓/✗". Oyun alaninin alt yarisina yaslanir:
// ustte Kivilcim ve sahnedeki bilgisayarlar gorunur kalir. 26 satira kadar liste kutudan tasmaz, kayar.
// Uzun girdi kisaltilmaz, kutunun genisliginde alt satira kayar.
public class ScreenView : VisualElement
{
    static readonly Color Good = Mats.Hex("#7CF07A");                 // oyunun ortak dili: dogru yesil, yanlis kirmizi
    static readonly Color Amber = Mats.Hex("#FFB347");                // terminal yazisi
    static readonly Color Glass = new Color(0.043f, 0.059f, 0.047f, 0.95f);   // #0B0F0C
    static readonly Color Frame = Mats.Hex("#25282B");
    static readonly Color FrameEdge = Mats.Hex("#6A6F70");
    static readonly Color InverseInk = Mats.Hex("#1A1206");          // True satirinda kehribar zeminin ustundeki yazi
    const float MaxHeightPercent = 55f;   // oyun alaninin en cok bu kadari: ust yarida sahne gorunur
    const long CursorBlinkMs = 530;
    const float FontSize = 34;

    static Texture2D scanlines;

    readonly Font mono;
    readonly Color error;
    readonly Label title, cursor;
    readonly ScrollView list;

    public ScreenView(Font mono, Font bold, Color error)
    {
        this.mono = mono; this.error = error;
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.left = 36; style.right = 36; style.bottom = 20;
        style.maxHeight = Length.Percent(MaxHeightPercent);
        style.flexDirection = FlexDirection.Column;
        // cerceve: ince acik kenar + kalin koyu metal
        style.backgroundColor = Frame;
        Ui.Radius(this, 16);
        Ui.Border(this, 2, FrameEdge);
        style.paddingLeft = 6; style.paddingRight = 6; style.paddingTop = 6; style.paddingBottom = 6;
        style.display = DisplayStyle.None;

        var glass = new VisualElement { pickingMode = PickingMode.Ignore };
        glass.style.flexDirection = FlexDirection.Column;
        glass.style.flexShrink = 1;
        glass.style.overflow = Overflow.Hidden;
        glass.style.backgroundColor = Glass;
        glass.style.paddingLeft = 24; glass.style.paddingRight = 24; glass.style.paddingTop = 18; glass.style.paddingBottom = 18;
        Ui.Radius(glass, 10);
        Add(glass);

        var titleRow = new VisualElement { pickingMode = PickingMode.Ignore };
        titleRow.style.flexDirection = FlexDirection.Row;
        titleRow.style.marginBottom = 12;
        title = Ui.Text("", bold, 30, Amber);
        cursor = Ui.Text("_", bold, 30, Amber);
        titleRow.Add(title);
        titleRow.Add(cursor);
        glass.Add(titleRow);
        schedule.Execute(() => cursor.style.opacity = cursor.style.opacity.value > 0.5f ? 0f : 1f).Every(CursorBlinkMs);

        list = new ScrollView(ScrollViewMode.Vertical) { pickingMode = PickingMode.Position };
        list.style.flexShrink = 1;
        list.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        list.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        glass.Add(list);

        glass.Add(Scanlines());
    }

    // Camin ustunde cok soluk yatay cizgiler (eski ekran): 1x4'luk doku tekrar eder
    static VisualElement Scanlines()
    {
        var lines = new VisualElement { pickingMode = PickingMode.Ignore };
        lines.style.position = Position.Absolute;
        lines.style.left = 0; lines.style.right = 0; lines.style.top = 0; lines.style.bottom = 0;
        lines.style.backgroundImage = new StyleBackground(ScanlineTexture);
        lines.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
        lines.style.backgroundSize = new BackgroundSize(new Length(4), new Length(6));
        return lines;
    }

    static Texture2D ScanlineTexture
    {
        get
        {
            if (scanlines != null) return scanlines;
            scanlines = new Texture2D(1, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            scanlines.SetPixels(new[] { Color.clear, Color.clear, Color.clear, new Color(0f, 0f, 0f, 0.22f) });
            scanlines.Apply();
            return scanlines;
        }
    }

    /// <summary>Kutuyu acar: baslik ve gorunen satirlar (cevaplari henuz yok: "girdi → ?")</summary>
    public void Show(ScreenPuzzle puzzle)
    {
        title.text = "İSTASYON EKRANI · " + puzzle.Function + "(...)";
        Clear();
        foreach (var row in puzzle.VisibleRows) list.Add(MakeRow(row, null));
        style.opacity = 1f;
        style.display = DisplayStyle.Flex;
    }

    /// <summary>80 kapanisi (3b): tek satirlik hatira; 78'de bulunan satirin gorunusunde ama cevabi yok. Kutu yavasca belirir.</summary>
    public void ShowRecall(string line)
    {
        title.text = RecallTitle;
        Clear();
        list.Add(Line(Shown(line), found: true));
        Ui.Transition(this, "opacity", RecallFade);
        style.opacity = 0f;
        style.display = DisplayStyle.Flex;
        schedule.Execute(() => style.opacity = 1f).StartingIn(20);
    }

    public const string RecallTitle = "İSTASYON EKRANI";
    const float RecallFade = 0.3f;

    /// <summary>Kutudaki ilk satirin yazisi (goruntu denetimi)</summary>
    public string FirstRowText => list.childCount > 0 ? list[0].Q<Label>().text : null;
    public string TitleText => title.text;

    /// <summary>Satirlari siler, baslik kalir (kod her calistiginda cevaplar bastan yazilir)</summary>
    public new void Clear() => list.Clear();

    /// <summary>Bir satirin cevabini ekler; en alta kaydirir</summary>
    public void Add(ScreenAnswer answer)
    {
        list.Add(MakeRow(answer.Row, answer));
        // yeni satirin yerlesimi bitince en alta (kaydirma cubugunun gercek en alt degeri) kaydirir
        schedule.Execute(() => list.scrollOffset = new Vector2(0, list.verticalScroller.highValue)).ExecuteLater(30);
    }

    public void Hide() => style.display = DisplayStyle.None;

    /// <summary>Kutudaki satir sayisi (goruntu denetimi)</summary>
    public int Rows => list.childCount;

    VisualElement MakeRow(ScreenRow row, ScreenAnswer answer)
    {
        // cevabi True olan satir ters yazilir: kehribar zemin, koyu yazi (77-78'de Kivilcim'in buldugu kisiler)
        bool found = answer != null && answer.Actual is bool b && b;
        var ink = found ? InverseInk : Amber;
        var line = Line(Shown(row.Input), found);

        var result = Ui.Text("  →  " + (answer == null ? "?" : Shown(answer.Actual)), mono, FontSize, ink);
        result.style.flexShrink = 0;
        line.Add(result);

        if (answer != null)
        {
            var mark = Ui.Text(answer.Correct ? "  ✓" : "  ✗", mono, FontSize, found && answer.Correct ? InverseInk : answer.Correct ? Good : error);
            mark.style.flexShrink = 0;
            line.Add(mark);
            if (!row.Visible)
            {
                var hidden = Ui.Text(" gizli", mono, 24, new Color(ink.r, ink.g, ink.b, 0.5f));
                hidden.style.flexShrink = 0;
                line.Add(hidden);
            }
        }
        return line;
    }

    // Satirin zemini ve girdisi: bulunan (True) satir ters yazilir, kehribar zemin, koyu yazi
    VisualElement Line(string input, bool found)
    {
        var line = new VisualElement { pickingMode = PickingMode.Ignore };
        line.style.flexDirection = FlexDirection.Row;
        line.style.alignItems = Align.FlexStart;
        line.style.paddingTop = 6; line.style.paddingBottom = 6; line.style.paddingLeft = 10; line.style.paddingRight = 10;
        Ui.Radius(line, 6);
        if (found) line.style.backgroundColor = Amber;

        var text = Ui.Text(input, mono, FontSize, found ? InverseInk : Amber);
        text.style.whiteSpace = WhiteSpace.Normal;
        text.style.flexShrink = 1;
        text.style.flexGrow = 1;
        line.Add(text);
        return line;
    }

    // Python'daki gorunusu (metin tirnakli, True/False, None); cevabin turu boylece okunur
    static string Shown(object value) => value == null ? "None" : ProgramRun.ValueText(value);
}
