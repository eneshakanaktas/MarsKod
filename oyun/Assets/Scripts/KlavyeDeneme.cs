using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

// GECICI DENEME (kod klavyesi plani, Gorev 0): TextField'siz kendi metin alanimiz telefon klavyesini hic acmiyor mu?
// Baslatma secenegi -klavyedeneme ile oyunun ustunde acilir. Gorev 5'te silinecek.
// Kod alani: renkli Label + kendi imlecimiz + secim. Yazi tipi es genislikli: dokunulan yer = (satir, sutun).
// Altta birkac tusluk gecici klavye. Bilgisayarda fiziksel klavye de yazar.
// Log: "KLAVYEDENEME:" satirlari (telefonda adb logcat).
public class KlavyeDeneme : MonoBehaviour
{
    const float FontSize = 37f;
    string text = "move(East)\ncollect()\nfor i in range(3):\n    move(East)";
    int caret, anchor; // secim: anchor..caret
    VisualElement area, caretEl, selLayer;
    Label code, status;
    Font mono;
    float charW, lineH;
    bool dragging, lastVisible;
    int taps;

    public static void Open()
    {
        new GameObject("KlavyeDeneme").AddComponent<KlavyeDeneme>();
    }

    void Start()
    {
        mono = Resources.Load<Font>("Fonts/JetBrainsMono-Regular");
        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/MarsTheme");
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1080, 2340);
        ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = 0f;
        ps.sortingOrder = 100;
        var doc = gameObject.AddComponent<UIDocument>();
        doc.panelSettings = ps;
        var root = doc.rootVisualElement;
        root.style.position = Position.Absolute;
        root.style.left = 0; root.style.right = 0; root.style.top = 0; root.style.bottom = 0;
        root.style.backgroundColor = new Color(0.08f, 0.07f, 0.1f, 0.96f);
        root.style.paddingTop = 140; root.style.paddingLeft = 40; root.style.paddingRight = 40;

        status = new Label();
        status.style.color = Color.white; status.style.fontSize = 30;
        status.style.whiteSpace = WhiteSpace.Normal;
        root.Add(status);

        // Kod alani: odaklanabilir (fiziksel klavye icin) ama yazi kutusu degil
        area = new VisualElement { focusable = true };
        area.style.marginTop = 30;
        area.style.backgroundColor = new Color(0.16f, 0.15f, 0.2f);
        area.style.paddingLeft = 30; area.style.paddingTop = 30; area.style.paddingBottom = 30; area.style.paddingRight = 30;
        area.style.minHeight = 520;
        root.Add(area);

        var inner = new VisualElement { pickingMode = PickingMode.Ignore };
        area.Add(inner);
        selLayer = new VisualElement { pickingMode = PickingMode.Ignore };
        selLayer.style.position = Position.Absolute;
        selLayer.style.left = 0; selLayer.style.top = 0; selLayer.style.right = 0; selLayer.style.bottom = 0;
        inner.Add(selLayer);
        code = new Label { pickingMode = PickingMode.Ignore, enableRichText = true };
        code.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
        code.style.fontSize = FontSize;
        code.style.color = Color.white;
        code.style.whiteSpace = WhiteSpace.Pre;
        code.style.marginLeft = 0; code.style.paddingLeft = 0; code.style.marginTop = 0; code.style.paddingTop = 0;
        inner.Add(code);
        caretEl = new VisualElement { pickingMode = PickingMode.Ignore };
        caretEl.style.position = Position.Absolute;
        caretEl.style.width = 4;
        caretEl.style.backgroundColor = new Color(1f, 0.55f, 0.2f);
        inner.Add(caretEl);

        area.RegisterCallback<PointerDownEvent>(e =>
        {
            taps++;
            area.Focus();
            caret = anchor = Hit(e.position);
            dragging = true;
            area.CapturePointer(e.pointerId);
            Log("dokunma " + taps + " -> imlec " + caret);
            Redraw();
        });
        area.RegisterCallback<PointerMoveEvent>(e =>
        {
            if (!dragging) return;
            caret = Hit(e.position);
            Redraw();
        });
        area.RegisterCallback<PointerUpEvent>(e => { dragging = false; area.ReleasePointer(e.pointerId); });
        area.RegisterCallback<KeyDownEvent>(e =>
        {
            if (e.keyCode == KeyCode.Backspace) Backspace();
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) Type("\n");
            else if (e.keyCode == KeyCode.LeftArrow) { caret = anchor = Mathf.Max(0, caret - 1); Redraw(); }
            else if (e.keyCode == KeyCode.RightArrow) { caret = anchor = Mathf.Min(text.Length, caret + 1); Redraw(); }
            else if (e.character >= ' ') Type(e.character.ToString());
            else return;
            e.StopPropagation();
        });

        // Gecici klavye
        var keys = new VisualElement();
        keys.style.flexDirection = FlexDirection.Row; keys.style.flexWrap = Wrap.Wrap;
        keys.style.marginTop = 40;
        foreach (var k in new[] { "m", "o", "v", "e", "(", ")", ":", "ğ", "ı", "⌫", "↵" })
        {
            var b = new Label(k) { focusable = false };
            b.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
            b.style.fontSize = 44; b.style.color = Color.white;
            b.style.width = 150; b.style.height = 130;
            b.style.marginRight = 16; b.style.marginBottom = 16;
            b.style.unityTextAlign = TextAnchor.MiddleCenter;
            b.style.backgroundColor = new Color(0.25f, 0.23f, 0.3f);
            string key = k;
            b.RegisterCallback<PointerDownEvent>(e =>
            {
                if (key == "⌫") Backspace(); else Type(key == "↵" ? "\n" : key);
                Log("tus " + key);
                e.StopPropagation();
            });
            keys.Add(b);
        }
        root.Add(keys);

        code.RegisterCallback<GeometryChangedEvent>(_ => Redraw());
        Redraw();
        Log("acildi, sistem klavyesi destekli=" + TouchScreenKeyboard.isSupported);
    }

    void Update()
    {
        bool v = TouchScreenKeyboard.visible;
        if (v != lastVisible) { lastVisible = v; Log("SISTEM KLAVYESI " + (v ? "ACILDI" : "kapandi")); }
        if (status != null)
            status.text = "Sistem klavyesi: " + (v ? "<color=#ff5050>AÇIK</color>" : "<color=#60ff90>kapalı</color>")
                + "   dokunma: " + taps + "   imleç: " + caret + (caret != anchor ? "  seçim: " + Mathf.Min(caret, anchor) + ".." + Mathf.Max(caret, anchor) : "")
                + "\nodak: " + (area.focusController?.focusedElement == area ? "kod alanı" : "yok");
    }

    void Type(string s)
    {
        int a = Mathf.Min(caret, anchor), b = Mathf.Max(caret, anchor);
        text = text.Substring(0, a) + s + text.Substring(b);
        caret = anchor = a + s.Length;
        Redraw();
    }

    void Backspace()
    {
        int a = Mathf.Min(caret, anchor), b = Mathf.Max(caret, anchor);
        if (a == b) { if (a == 0) return; a--; }
        text = text.Substring(0, a) + text.Substring(b);
        caret = anchor = a;
        Redraw();
    }

    void Measure()
    {
        var size = code.MeasureTextSize("0000000000", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
        charW = size.x / 10f;
        var one = code.MeasureTextSize("0", 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
        int n = text.Split('\n').Length;
        float h = code.layout.height;
        lineH = !float.IsNaN(h) && h > 1f ? h / n : one.y;
    }

    // Ekrandaki nokta -> metindeki yer (satir, sutun hesabiyla)
    int Hit(Vector2 world)
    {
        Measure();
        var p = code.WorldToLocal(world);
        var lines = text.Split('\n');
        int line = Mathf.Clamp(Mathf.FloorToInt(p.y / lineH), 0, lines.Length - 1);
        int col = Mathf.Clamp(Mathf.RoundToInt(p.x / charW), 0, lines[line].Length);
        int idx = 0;
        for (int i = 0; i < line; i++) idx += lines[i].Length + 1;
        return idx + col;
    }

    (int line, int col) Pos(int idx)
    {
        int line = 0, ls = 0;
        for (int i = 0; i < idx; i++) if (text[i] == '\n') { line++; ls = i + 1; }
        return (line, idx - ls);
    }

    void Redraw()
    {
        var lines = text.Split('\n');
        var sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) sb.Append('\n');
            sb.Append(lines[i].Length == 0 ? " " : CodeColors.Line(lines[i]));
        }
        code.text = sb.ToString();
        Measure();
        var (l, c) = Pos(caret);
        caretEl.style.left = c * charW - 2; caretEl.style.top = l * lineH; caretEl.style.height = lineH;
        caretEl.style.display = caret == anchor ? DisplayStyle.Flex : DisplayStyle.None;

        selLayer.Clear();
        if (caret != anchor)
        {
            int a = Mathf.Min(caret, anchor), b = Mathf.Max(caret, anchor);
            var (la, ca) = Pos(a); var (lb, cb) = Pos(b);
            for (int li = la; li <= lb; li++)
            {
                int from = li == la ? ca : 0, to = li == lb ? cb : lines[li].Length + 1;
                var r = new VisualElement { pickingMode = PickingMode.Ignore };
                r.style.position = Position.Absolute;
                r.style.left = from * charW; r.style.width = Mathf.Max(0, to - from) * charW;
                r.style.top = li * lineH; r.style.height = lineH;
                r.style.backgroundColor = new Color(1f, 0.55f, 0.2f, 0.35f);
                selLayer.Add(r);
            }
        }
    }

    static void Log(string s) => Debug.Log("KLAVYEDENEME: " + s);
}
