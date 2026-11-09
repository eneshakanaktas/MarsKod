using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Istasyon ekrani (ekran bulmacasi, Bolge 8): oyun alaninin ustunde sade bir kutu; satir satir "girdi → cevap ✓/✗".
// Gecici gorunus: asil istasyon ekrani Bolge 8 · 3a'da. 26 satira kadar liste kutudan tasmaz, kayar.
// Uzun girdi kisaltilmaz, kutunun genisliginde alt satira kayar.
public class ScreenView : VisualElement
{
    static readonly Color Good = Mats.Hex("#7CF07A");

    readonly Font mono;
    readonly Color ink, accent, error, hairline;
    readonly Label title;
    readonly ScrollView list;
    ScreenPuzzle puzzle;

    public ScreenView(Font mono, Font bold, Color ink, Color accent, Color error, Color hairline)
    {
        this.mono = mono; this.ink = ink; this.accent = accent; this.error = error; this.hairline = hairline;
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.left = 36; style.right = 36; style.top = 24;
        style.maxHeight = Length.Percent(90);
        style.flexDirection = FlexDirection.Column;
        style.paddingLeft = 28; style.paddingRight = 28; style.paddingTop = 22; style.paddingBottom = 22;
        style.backgroundColor = new Color(0.055f, 0.05f, 0.07f, 0.92f);
        Ui.Radius(this, 22);
        Ui.Border(this, 2, hairline);
        style.display = DisplayStyle.None;

        title = Ui.Text("", bold, 30, accent);
        title.style.marginBottom = 14;
        Add(title);

        list = new ScrollView(ScrollViewMode.Vertical) { pickingMode = PickingMode.Position };
        list.style.flexShrink = 1;
        list.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        list.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        Add(list);
    }

    /// <summary>Kutuyu acar: baslik ve gorunen satirlar (cevaplari henuz yok: "girdi → ?")</summary>
    public void Show(ScreenPuzzle puzzle)
    {
        this.puzzle = puzzle;
        title.text = "İSTASYON EKRANI · " + puzzle.Function + "(...)";
        Clear();
        foreach (var row in puzzle.VisibleRows) list.Add(MakeRow(row, null));
        style.display = DisplayStyle.Flex;
    }

    /// <summary>Satirlari siler, baslik kalir (kod her calistiginda cevaplar bastan yazilir)</summary>
    public void Clear() => list.Clear();

    /// <summary>Bir satirin cevabini ekler; en alta kaydirir</summary>
    public void Add(ScreenAnswer answer)
    {
        list.Add(MakeRow(answer.Row, answer));
        // yeni satirin yerlesimi bitince en alta (kaydirma cubugunun gercek en alt degeri) kaydirir
        schedule.Execute(() => list.scrollOffset = new Vector2(0, list.verticalScroller.highValue)).ExecuteLater(30);
    }

    public void Hide()
    {
        puzzle = null;
        style.display = DisplayStyle.None;
    }

    VisualElement MakeRow(ScreenRow row, ScreenAnswer answer)
    {
        var line = new VisualElement { pickingMode = PickingMode.Ignore };
        line.style.flexDirection = FlexDirection.Row;
        line.style.alignItems = Align.FlexStart;
        line.style.paddingTop = 6; line.style.paddingBottom = 6; line.style.paddingLeft = 10; line.style.paddingRight = 10;
        Ui.Radius(line, 10);
        // cevabi True olan satir vurgulu (77-78'de Kivilcim'in buldugu kisiler)
        if (answer != null && answer.Actual is bool b && b) line.style.backgroundColor = new Color(accent.r, accent.g, accent.b, 0.28f);

        var input = Ui.Text(Shown(row.Input), mono, 34, ink);
        input.style.whiteSpace = WhiteSpace.Normal;
        input.style.flexShrink = 1;
        input.style.flexGrow = 1;
        line.Add(input);

        var result = Ui.Text("  →  " + (answer == null ? "?" : Shown(answer.Actual)), mono, 34, ink);
        result.style.flexShrink = 0;
        line.Add(result);

        if (answer != null)
        {
            var mark = Ui.Text(answer.Correct ? "  ✓" : "  ✗", mono, 34, answer.Correct ? Good : error);
            mark.style.flexShrink = 0;
            line.Add(mark);
            if (!row.Visible)
            {
                var hidden = Ui.Text(" gizli", mono, 24, new Color(ink.r, ink.g, ink.b, 0.45f));
                hidden.style.flexShrink = 0;
                line.Add(hidden);
            }
        }
        return line;
    }

    // Python'daki gorunusu (metin tirnakli, True/False, None); cevabin turu boylece okunur
    static string Shown(object value) => value == null ? "None" : ProgramRun.ValueText(value);
}
