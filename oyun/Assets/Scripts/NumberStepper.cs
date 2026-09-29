using UnityEngine;
using UnityEngine.UIElements;

// Acemi (tasarim: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §4): koddaki sayiya dokununca ustunde
// - / + cikar (range(3) -> range(5)). Basili tutunca art arda degistirir. Satirin turu degismez (CodeBuffer.ChangeNumber).
public class NumberStepper : VisualElement
{
    const float W = 280f, H = 116f;

    readonly CodeEditor editor;
    readonly VisualElement minus, plus;
    int line = -1, col;

    public NumberStepper(CodeEditor editor, Font font, Color ink, Color accent, Color background)
    {
        this.editor = editor;
        style.position = Position.Absolute;
        style.width = W; style.height = H;
        style.flexDirection = FlexDirection.Row;
        style.backgroundColor = background;
        Ui.Radius(this, H * 0.5f);
        Ui.Border(this, 3, accent);
        style.display = DisplayStyle.None;
        minus = StepButton("−", -1, font, ink);
        Add(minus);
        var divider = new VisualElement { pickingMode = PickingMode.Ignore };
        divider.style.width = 2;
        divider.style.marginTop = 24; divider.style.marginBottom = 24;
        divider.style.backgroundColor = new Color(1f, 1f, 1f, 0.12f);
        Add(divider);
        plus = StepButton("+", +1, font, ink);
        Add(plus);
    }

    public bool Open => line >= 0;

    // Dugmenin panel koordinati (deneme icin); kutu kapaliysa null
    public Vector2? ButtonCenter(int delta) => Open ? (delta > 0 ? plus : minus).worldBound.center : (Vector2?)null;

    // col: sayinin basladigi sutun
    public void Show(int line, int col)
    {
        this.line = line;
        this.col = col;
        if (!Place()) Hide();
    }

    public void Hide()
    {
        if (!Open) return;
        line = -1;
        style.display = DisplayStyle.None;
        editor.ClearNumberMark();
    }

    // Kutuyu sayinin ustune (yer yoksa altina) koyar; sayi artik yoksa false
    bool Place()
    {
        var r = editor.MarkNumber(line, col);
        if (!r.HasValue) return false;
        var area = parent.layout;
        float top = r.Value.yMin - H - 18f;
        if (top < 0f) top = r.Value.yMax + 18f;
        style.left = Mathf.Clamp(r.Value.center.x - W * 0.5f, 12f, Mathf.Max(12f, area.width - W - 12f));
        style.top = top;
        style.display = DisplayStyle.Flex;
        return true;
    }

    void Step(int delta)
    {
        if (!Open) return;
        editor.Edit(b => b.ChangeNumber(line, col, delta));
        if (!Place()) Hide();
    }

    VisualElement StepButton(string sign, int delta, Font font, Color ink)
    {
        var b = new VisualElement();
        b.style.flexGrow = 1;
        b.style.alignItems = Align.Center; b.style.justifyContent = Justify.Center;
        Ui.Radius(b, H * 0.5f);
        var t = new Label(sign) { pickingMode = PickingMode.Ignore };
        Ui.NoSpacing(t);
        t.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(font));
        t.style.fontSize = 60;
        t.style.color = ink;
        b.Add(t);

        IVisualElementScheduledItem repeat = null;
        void Release(int id)
        {
            repeat?.Pause();
            b.style.backgroundColor = Color.clear;
            if (b.HasPointerCapture(id)) b.ReleasePointer(id);
        }
        b.RegisterCallback<PointerDownEvent>(e =>
        {
            e.StopPropagation();
            b.CapturePointer(e.pointerId);
            b.style.backgroundColor = new Color(1f, 1f, 1f, 0.14f);
            Step(delta);
            repeat?.Pause();
            repeat = schedule.Execute(() => Step(delta)).StartingIn(450).Every(110);
        });
        b.RegisterCallback<PointerUpEvent>(e => Release(e.pointerId));
        b.RegisterCallback<PointerCancelEvent>(e => Release(e.pointerId));
        return b;
    }
}
