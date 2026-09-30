using System;
using System.Collections.Generic;
using System.Text;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Kod yazma alani: kendi metin alanimiz. Yazi kutusu (TextField) yok, bu yuzden telefon klavyesi hic acilmaz.
// Kod, imlec, secim ve satir turleri CodeBuffer'da; burasi yalnizca gosterir, dokunma ve klavyeyi CodeBuffer islemlerine cevirir.
// Yazi tipi es genislikli: dokunulan yer = (satir, sutun).
// Telefonda: dokun -> imlec; basili tut + surukle -> secim; hemen surukle -> kaydir.
// Bilgisayarda: fareyle surukle -> secim, tekerlek -> kaydir; fiziksel klavye yazar (harf, geri silme, Enter, Tab, oklar).
// Uzun satir saga-sola, cok satirli kod yukari-asagi kaydirilir; alan en cok MaxHeight kadar buyur.
// Acemi (Blocks): imlec yok, harf yazilmaz; basili tut (farede surukle) -> satir tasinmak uzere kalkar (LinePicked),
// dokun -> sayiya dokunulduysa NumberTapped. Surukleme isini BlockDrag yapar; burasi birakma yerini gosterir.
public class CodeEditor : VisualElement
{
    // Oyuncu kodu degistirdi (yeni kodun tamami)
    public event Action<string> Changed;
    // Acemi: satir tasinmak uzere tutuldu (satir, parmagin yeri, parmak numarasi)
    public event Action<int, Vector2, int> LinePicked;
    // Acemi: koda dokunuldu; sayiya dokunulduysa (satir, sayinin basladigi sutun), degilse (-1, -1)
    public event Action<int, int> NumberTapped;

    const float FontSize = 37f, NumSize = 30f, Gutter = 68f;
    // Satirlar arasi ek bosluk: satirlar parmakla secilebilecek kadar ferah olsun
    const float Spacing = 34f;
    const long LongPressMs = 350;
    const float DragSlop = 18f;

    enum Drag { None, Pending, Scroll, Select }

    readonly Color ink, numColor, accent, errorRed;
    readonly Font mono;
    readonly CodeBuffer buffer = new CodeBuffer();
    readonly VisualElement rowsLayer, textClip, textLayer, selLayer, caret, dropMark, numberMark, guideLayer;
    readonly Label colored, probe;
    // Calisan satirin sagindaki soluk yazi: satir bitince degiskenler ("i = 2"), adim adim modu icin
    readonly Label varsNote;
    readonly List<(VisualElement row, VisualElement bar, Label num)> rows = new List<(VisualElement, VisualElement, Label)>();
    int activeLine = -1;
    bool activeError;
    string activeVars = "";
    float pitch = FontSize * 1.32f + Spacing, lineHeight = FontSize * 1.32f, charW = FontSize * 0.6f;
    float scrollX, scrollY, maxHeight = float.MaxValue;
    bool editing, readOnly, pitchMeasured, blocks;
    // Acemi: tasinmak uzere kaldirilan satir (soluk gorunur), yoksa -1
    int lifted = -1;
    // Blok cizgileri (":" ile biten satirin govdesi) ve surukleme sirasinda satirin girecegi blok (baslik satiri, yoksa -1)
    List<BlockSpan> spans = new List<BlockSpan>();
    int dropOwner = -1, dropGap = -1;
    int caretSeen = -1;
    float blinkStart;
    // dokunma / fare
    Drag drag;
    bool dragMouse;
    int dragPointer = -1;
    Vector2 downPos, lastPos;
    IVisualElementScheduledItem longPress;

    public CodeEditor(Font mono, Color ink, Color numColor, Color accent, Color errorRed)
    {
        this.mono = mono; this.ink = ink; this.numColor = numColor; this.accent = accent; this.errorRed = errorRed;
        focusable = true; // bilgisayar klavyesi icin (yazi kutusu degil)
        style.overflow = Overflow.Hidden;

        // En altta: satir vurgulari ve satir numaralari (yalnizca yukari-asagi kayar)
        rowsLayer = new VisualElement { pickingMode = PickingMode.Ignore };
        rowsLayer.style.position = Position.Absolute;
        rowsLayer.style.left = 0; rowsLayer.style.right = 0; rowsLayer.style.top = 0; rowsLayer.style.bottom = 0;
        Add(rowsLayer);

        // Yazi: numaralarin sagindaki pencere; icindeki katman iki yone kayar
        textClip = new VisualElement { pickingMode = PickingMode.Ignore };
        textClip.style.position = Position.Absolute;
        textClip.style.left = Gutter; textClip.style.right = 0; textClip.style.top = 0; textClip.style.bottom = 0;
        textClip.style.overflow = Overflow.Hidden;
        Add(textClip);

        textLayer = new VisualElement { pickingMode = PickingMode.Ignore };
        textLayer.style.position = Position.Absolute;
        textLayer.style.left = 0; textLayer.style.top = Spacing * 0.5f;
        textClip.Add(textLayer);

        selLayer = new VisualElement { pickingMode = PickingMode.Ignore };
        selLayer.style.position = Position.Absolute;
        selLayer.style.left = 0; selLayer.style.top = 0;
        textLayer.Add(selLayer);

        colored = new Label { pickingMode = PickingMode.Ignore, enableRichText = true };
        TextStyle(colored, ink);
        textLayer.Add(colored);

        // Tek satirin yuksekligini ve harf genisligini olcmek icin gorunmez ornek (10 harf)
        probe = new Label("0000000000") { pickingMode = PickingMode.Ignore };
        TextStyle(probe, Color.clear);
        probe.style.position = Position.Absolute;
        probe.style.visibility = Visibility.Hidden;
        textLayer.Add(probe);

        varsNote = new Label { pickingMode = PickingMode.Ignore, enableRichText = false };
        TextStyle(varsNote, new Color(ink.r, ink.g, ink.b, 0.5f));
        varsNote.style.fontSize = NumSize;
        varsNote.style.position = Position.Absolute;
        varsNote.style.display = DisplayStyle.None;
        textLayer.Add(varsNote);

        // Imlec: kalin, turuncu, yanip sonen bir cizgi
        caret = new VisualElement { pickingMode = PickingMode.Ignore };
        caret.style.position = Position.Absolute;
        caret.style.width = 4;
        caret.style.backgroundColor = accent;
        Ui.Radius(caret, 2);
        caret.style.display = DisplayStyle.None;
        textLayer.Add(caret);
        schedule.Execute(UpdateCaret).Every(33);

        // Acemi: birakilacak yer (satirlar arasinda ince turuncu cizgi; basindaki nokta girintiyi gosterir)
        dropMark = new VisualElement { pickingMode = PickingMode.Ignore };
        dropMark.style.position = Position.Absolute;
        dropMark.style.right = -4000; // yatay kaydirmada da sag kenara uzansin
        dropMark.style.height = 6;
        dropMark.style.backgroundColor = accent;
        Ui.Radius(dropMark, 3);
        var dot = new VisualElement { pickingMode = PickingMode.Ignore };
        dot.style.position = Position.Absolute;
        dot.style.left = -9; dot.style.top = -7; dot.style.width = 20; dot.style.height = 20;
        dot.style.backgroundColor = accent;
        Ui.Radius(dot, 10);
        dropMark.Add(dot);
        dropMark.style.display = DisplayStyle.None;
        textLayer.Add(dropMark);

        // Acemi: -/+ ile degistirilen sayinin cercevesi
        numberMark = new VisualElement { pickingMode = PickingMode.Ignore };
        numberMark.style.position = Position.Absolute;
        Ui.Border(numberMark, 3, accent);
        Ui.Radius(numberMark, 8);
        numberMark.style.display = DisplayStyle.None;
        textLayer.Insert(0, numberMark);

        // Blok cizgileri: her blogun govdesinin solunda ince dikey cizgi (hangi satirlar tekrarlanacak, bir bakista)
        guideLayer = new VisualElement { pickingMode = PickingMode.Ignore };
        guideLayer.style.position = Position.Absolute;
        guideLayer.style.left = 0; guideLayer.style.top = 0;
        textLayer.Insert(0, guideLayer);

        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
        RegisterCallback<PointerCancelEvent>(e => EndDrag(e.pointerId));
        RegisterCallback<PointerCaptureOutEvent>(_ => EndDrag(dragPointer));
        RegisterCallback<WheelEvent>(e =>
        {
            float step = pitch * 0.5f;
            ScrollBy(Mathf.Clamp(e.delta.x * step, -pitch * 3, pitch * 3), Mathf.Clamp(e.delta.y * step, -pitch * 3, pitch * 3));
            e.StopPropagation();
        });
        RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        // Tab ve oklar odagi baska yere tasimasin
        RegisterCallback<NavigationMoveEvent>(e => e.StopImmediatePropagation(), TrickleDown.TrickleDown);

        colored.RegisterCallback<GeometryChangedEvent>(_ => { MeasurePitch(); Layout(); });
        probe.RegisterCallback<GeometryChangedEvent>(_ => Layout());
        RegisterCallback<GeometryChangedEvent>(_ => Layout());
        Repaint();
    }

    public string Text => buffer.Text;

    // Satir turlerinin kayit metni (satir basina bir harf; CodeBuffer.SaveKinds)
    public string Kinds => buffer.SaveKinds();

    // Kod tamponu (XP, oneriler vb. okumak icin). Degistirmek icin Edit kullanilir.
    public CodeBuffer Buffer => buffer;

    // Oyuncu su an yaziyor mu (koda dokundu, henuz disari dokunmadi)
    public bool Editing => editing && !readOnly;

    public bool ReadOnly
    {
        get => readOnly;
        set { readOnly = value; if (value) StopEditing(); }
    }

    // Acemi kademesi: imlec ve secim yok, harf yazilmaz; satirlar surukle-birakla tasinir.
    public bool Blocks
    {
        get => blocks;
        set
        {
            if (blocks == value) return;
            blocks = value;
            buffer.SetCaret(buffer.Caret);
            Repaint();
        }
    }

    // Kod alaninin en cok buyuyebilecegi yukseklik (fazlasi kaydirilir)
    public float MaxHeight
    {
        get => maxHeight;
        set
        {
            if (Mathf.Abs(value - maxHeight) <= 0.5f) return;
            maxHeight = value;
            Layout();
            if (Editing) EnsureCaretVisible(); // klavye acilip alan kuculunce imlec gorunur kalsin
        }
    }

    // Yazmaya baslar; from..to arasi secili olur (ikisi esitse yalnizca imlec).
    public void StartEditing(int from, int to)
    {
        if (readOnly) return;
        editing = true;
        Focus();
        buffer.Select(from, to);
        Repaint();
        EnsureCaretVisible();
    }

    // Yazma biter: imlec ve secim gizlenir.
    public void StopEditing()
    {
        editing = false;
        buffer.SetCaret(buffer.Caret);
        Blur();
        Repaint();
    }

    // Bolumun baslangic kodunu yukler (butun satirlar Baslangic). Changed tetiklenmez.
    public void LoadStart(string code)
    {
        buffer.LoadStart(code);
        scrollX = scrollY = 0;
        Repaint();
    }

    // Kaydedilmis kodu turleriyle yukler (tur yoksa Dugme sayilir). Changed tetiklenmez.
    public void Load(string code, string kinds)
    {
        buffer.Load(code, kinds);
        scrollX = scrollY = 0;
        Repaint();
    }

    // Kodu bir CodeBuffer islemiyle degistirir (klavye, palet, bilgisayar klavyesi hepsi bunu cagirir).
    public void Edit(Action<CodeBuffer> op)
    {
        if (readOnly) return;
        if (editing) Focus(); // ekrandaki tusa basmak odagi almasin (bilgisayar klavyesi de yazmaya devam etsin)
        string before = buffer.Text;
        op(buffer);
        Repaint();
        EnsureCaretVisible();
        if (buffer.Text != before) Changed?.Invoke(buffer.Text);
    }

    // Calisan satiri isaretler (0'dan baslar, -1 = hicbiri). error: satir kirmizi yanar. Satir gorunmuyorsa oraya kayar.
    // vars: satirin saginda soluk yazi (degiskenler, "i = 2"); bos ya da null ise yazi yok.
    public void SetActiveLine(int idx, bool error = false, string vars = null)
    {
        activeLine = idx;
        activeError = error;
        activeVars = vars ?? "";
        PlaceVarsNote();
        var color = error ? errorRed : accent;
        for (int i = 0; i < rows.Count; i++)
        {
            bool a = i == idx;
            rows[i].row.style.backgroundColor = a ? (error ? new Color(errorRed.r, errorRed.g, errorRed.b, 0.14f) : new Color(1f, 1f, 1f, 0.07f)) : new Color(1f, 1f, 1f, 0f);
            rows[i].bar.style.backgroundColor = color;
            rows[i].bar.style.opacity = a ? 1f : 0f;
            rows[i].num.style.color = a ? color : numColor;
        }
        if (idx >= 0 && idx < rows.Count) EnsureLineVisible(idx);
    }

    // Degisken yazisini calisan satirin sonuna, iki harf bosluk birakarak koyar
    void PlaceVarsNote()
    {
        bool show = activeVars.Length > 0 && activeLine >= 0 && activeLine < buffer.LineCount;
        varsNote.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        if (!show) return;
        varsNote.text = activeVars;
        varsNote.style.left = (buffer.Line(activeLine).Length + 2) * charW;
        varsNote.style.top = activeLine * pitch + (lineHeight - NumSize * 1.32f) * 0.5f;
    }

    // ---- Acemi: surukle-birak ve -/+ icin ----

    // Bir girinti kademesinin (4 bosluk) ekrandaki genisligi
    public float IndentStep => charW * CodeBuffer.Indent.Length;

    // Noktaya en yakin satir arasi (0 = en ust, LineCount = en alt)
    public int GapAt(Vector2 world)
    {
        var p = textLayer.WorldToLocal(world);
        return Mathf.Clamp(Mathf.RoundToInt((p.y + (pitch - lineHeight) * 0.5f) / pitch), 0, buffer.LineCount);
    }

    // Birakilacak yeri gosterir: gap satir arasinda, level kademe iceriden baslayan turuncu cizgi. Satir bir blogun
    // govdesine girecekse o blogun cizgisi turuncu olup birakilacak yere uzar. movingLine: tasinan satir (yoksa -1).
    public void ShowDrop(int gap, int level, int movingLine)
    {
        dropOwner = CodeBlocks.Owner(Lines(), gap, level, movingLine);
        dropGap = gap;
        LayoutGuides();
        float y = gap * pitch - (pitch - lineHeight) * 0.5f;
        y = Mathf.Clamp(y, -Spacing * 0.5f + 8f, ContentHeight - Spacing * 0.5f - 8f); // en ustte/altta kesilmesin
        dropMark.style.left = level * IndentStep;
        dropMark.style.top = y - 3f;
        dropMark.style.display = DisplayStyle.Flex;
    }

    public void HideDrop()
    {
        dropMark.style.display = DisplayStyle.None;
        if (dropOwner < 0 && dropGap < 0) return;
        dropOwner = dropGap = -1;
        LayoutGuides();
    }

    List<string> Lines()
    {
        var list = new List<string>(buffer.LineCount);
        for (int i = 0; i < buffer.LineCount; i++) list.Add(buffer.Line(i));
        return list;
    }

    // Tasinmak uzere kaldirilan satir soluk gorunur (-1: hicbiri)
    public void Lift(int line)
    {
        if (lifted == line) return;
        lifted = line;
        Repaint();
    }

    // Surukleyen parmak kod alaninin ust/alt kenarindaysa kodu o yone kaydirir (uzun kodda gorunmeyen yere birakmak icin)
    public void EdgeScroll(Vector2 world)
    {
        var r = worldBound;
        float zone = pitch * 0.7f;
        if (world.x < r.xMin || world.x > r.xMax) return;
        if (world.y < r.yMin + zone && world.y > r.yMin - zone * 2f) ScrollBy(0f, -pitch * 0.2f);
        else if (world.y > r.yMax - zone && world.y < r.yMax + zone * 2f) ScrollBy(0f, pitch * 0.2f);
    }

    // (line, col)'daki sayinin cevresine cerceve koyar; sayinin ekrandaki yerini dondurur. Sayi yoksa cerceve kalkar, null.
    public Rect? MarkNumber(int line, int col)
    {
        if (!buffer.FindNumber(line, col, out int start, out int length))
        {
            ClearNumberMark();
            return null;
        }
        var r = new Rect(start * charW - 8f, line * pitch - 6f, length * charW + 16f, lineHeight + 12f);
        numberMark.style.left = r.x; numberMark.style.top = r.y;
        numberMark.style.width = r.width; numberMark.style.height = r.height;
        numberMark.style.display = DisplayStyle.Flex;
        return textLayer.LocalToWorld(r);
    }

    public void ClearNumberMark() => numberMark.style.display = DisplayStyle.None;

    // Deneme icin: (satir, sutun)'daki harfin ortasi ve gap satir arasinin (sutun hizasinda) panel koordinati
    public Vector2 CharCenter(int line, int col) => textLayer.LocalToWorld(new Vector2((col + 0.5f) * charW, line * pitch + lineHeight * 0.5f));
    public Vector2 GapPoint(int gap, int col) => textLayer.LocalToWorld(new Vector2((col + 0.5f) * charW, gap * pitch - (pitch - lineHeight) * 0.5f));

    // ---- Dokunma / fare ----

    void OnPointerDown(PointerDownEvent e)
    {
        bool mouse = e.pointerType == UnityEngine.UIElements.PointerType.mouse;
        if (mouse && e.button != 0) return;
        if (!readOnly) { editing = true; Focus(); }
        dragPointer = e.pointerId;
        dragMouse = mouse;
        downPos = lastPos = e.position;
        this.CapturePointer(e.pointerId);
        if (blocks)
        {
            // Acemi: kisa dokunus sayiya bakar; parmakla basili tutmak ya da fareyle surukleme satiri kaldirir
            drag = Drag.Pending;
            longPress?.Pause();
            if (!mouse) longPress = schedule.Execute(() => { if (drag == Drag.Pending) PickLine(downPos); }).StartingIn(LongPressMs);
        }
        else if (mouse)
        {
            // fare: tiklanan yere imlec, surukleyince secim (Shift ile secimi uzatir)
            drag = Drag.Select;
            if (!readOnly)
            {
                int i = Hit(e.position);
                if (e.shiftKey) buffer.Select(buffer.Anchor, i); else buffer.SetCaret(i);
                Repaint();
            }
        }
        else
        {
            // parmak: kisa dokunus imlec, basili tutmak secim, hemen kaydirmak kaydirma
            drag = Drag.Pending;
            longPress?.Pause();
            longPress = schedule.Execute(StartTouchSelect).StartingIn(LongPressMs);
        }
        e.StopPropagation();
    }

    void StartTouchSelect()
    {
        if (drag != Drag.Pending || readOnly) return;
        drag = Drag.Select;
        buffer.SetCaret(Hit(downPos));
        blinkStart = Time.unscaledTime;
        Repaint();
    }

    void OnPointerMove(PointerMoveEvent e)
    {
        if (e.pointerId != dragPointer || drag == Drag.None) return;
        Vector2 p = e.position;
        if (drag == Drag.Pending && (p - downPos).magnitude > DragSlop)
        {
            longPress?.Pause();
            if (blocks && dragMouse) { PickLine(downPos); return; }
            drag = Drag.Scroll;
            if (blocks) NumberTapped?.Invoke(-1, -1); // -/+ kutusu kayan koddan ayri kalmasin
        }
        if (drag == Drag.Scroll) ScrollBy(lastPos.x - p.x, lastPos.y - p.y);
        else if (drag == Drag.Select && !readOnly)
        {
            buffer.Select(buffer.Anchor, Hit(p));
            Repaint();
            EnsureCaretVisible();
        }
        lastPos = p;
    }

    void OnPointerUp(PointerUpEvent e)
    {
        if (e.pointerId != dragPointer) return;
        if (drag == Drag.Pending && !readOnly)
        {
            if (blocks) TapNumber(e.position);
            else
            {
                buffer.SetCaret(Hit(e.position));
                Repaint();
            }
        }
        EndDrag(e.pointerId);
    }

    // Acemi: dokunulan yerde sayi varsa haber verir (ustunde -/+ acilsin)
    void TapNumber(Vector2 world)
    {
        var (l, c) = buffer.Position(Hit(world));
        if (buffer.FindNumber(l, c, out int start, out _)) NumberTapped?.Invoke(l, start);
        else NumberTapped?.Invoke(-1, -1);
    }

    // Acemi: parmagin altindaki satir tasinmak uzere kalkar (BlockDrag parmagi devralir)
    void PickLine(Vector2 world)
    {
        drag = Drag.None;
        int line = LineAt(world);
        if (readOnly || line < 0) return;
        NumberTapped?.Invoke(-1, -1);
        LinePicked?.Invoke(line, world, dragPointer);
    }

    // Noktanin ustundeki satir; satirlarin disindaysa ya da kod bossa -1
    int LineAt(Vector2 world)
    {
        if (buffer.LineCount == 1 && buffer.Line(0).Trim().Length == 0) return -1;
        int line = RowAt(textLayer.WorldToLocal(world));
        return line >= 0 && line < buffer.LineCount ? line : -1;
    }

    void EndDrag(int pointerId)
    {
        longPress?.Pause();
        drag = Drag.None;
        if (pointerId >= 0 && this.HasPointerCapture(pointerId)) this.ReleasePointer(pointerId);
        dragPointer = -1;
    }

    // Ekrandaki nokta -> koddaki yer
    int Hit(Vector2 world)
    {
        var p = textLayer.WorldToLocal(world);
        int line = Mathf.Clamp(RowAt(p), 0, buffer.LineCount - 1);
        int col = Mathf.Max(0, Mathf.RoundToInt(p.x / charW));
        return buffer.Index(line, col);
    }

    // Yazi katmanindaki noktanin satir sirasi (satirin ust/alt boslugu dahil; kodun disinda da sayi verir)
    int RowAt(Vector2 local) => Mathf.FloorToInt((local.y + (pitch - lineHeight) * 0.5f) / pitch);

    // ---- Bilgisayar klavyesi ----

    void OnKeyDown(KeyDownEvent e)
    {
        if (!Editing || blocks) return; // Acemi'de harf yazilmaz
        bool shift = e.shiftKey;
        bool ctrl = (e.ctrlKey || e.commandKey) && !e.altKey; // AltGr (Ctrl+Alt) ile yazilan { [ @ gibi isaretler yazi sayilir
        bool handled = true;
        switch (e.keyCode)
        {
            case KeyCode.Backspace: Edit(b => b.Backspace()); break;
            case KeyCode.Delete: Edit(DeleteForward); break;
            case KeyCode.Return:
            case KeyCode.KeypadEnter: Edit(b => b.Enter()); break;
            case KeyCode.Tab:
                Edit(b =>
                {
                    if (shift) b.DedentLines();
                    else if (b.HasSelection) b.IndentLines();
                    else b.Type(CodeBuffer.Indent);
                });
                break;
            case KeyCode.LeftArrow: Move(b => b.MoveHorizontal(-1, shift)); break;
            case KeyCode.RightArrow: Move(b => b.MoveHorizontal(1, shift)); break;
            case KeyCode.UpArrow: Move(b => b.MoveVertical(-1, shift)); break;
            case KeyCode.DownArrow: Move(b => b.MoveVertical(1, shift)); break;
            case KeyCode.Home: Move(b => b.MoveToLineEdge(false, shift)); break;
            case KeyCode.End: Move(b => b.MoveToLineEdge(true, shift)); break;
            case KeyCode.A when ctrl: Move(b => b.Select(0, b.Text.Length)); break;
            default: handled = false; break;
        }
        // Harfler ve isaretler (satir sonu, Tab gibi kontrol karakterleri yukarida tus olarak islenir)
        if (!handled && !ctrl && e.character >= ' ' && e.character != (char)127)
        {
            char ch = e.character;
            Edit(b => b.Type(ch.ToString()));
            handled = true;
        }
        if (handled) e.StopImmediatePropagation();
    }

    static void DeleteForward(CodeBuffer b)
    {
        if (!b.HasSelection)
        {
            if (b.Caret >= b.Text.Length) return;
            b.Select(b.Caret + 1, b.Caret);
        }
        b.Backspace(); // secim varken yalnizca secimi siler
    }

    void Move(Action<CodeBuffer> op)
    {
        op(buffer);
        Repaint();
        EnsureCaretVisible();
    }

    // ---- Kaydirma ----

    float ViewWidth => float.IsNaN(layout.width) ? 0f : Mathf.Max(0f, layout.width - Gutter);
    float ContentHeight => (buffer.LineCount - 1) * pitch + lineHeight + Spacing;
    float ViewHeight => Mathf.Min(ContentHeight, maxHeight);

    void ScrollBy(float dx, float dy)
    {
        scrollX += dx;
        scrollY += dy;
        ApplyScroll();
    }

    void ApplyScroll()
    {
        int longest = 0;
        for (int i = 0; i < buffer.LineCount; i++) longest = Mathf.Max(longest, buffer.Line(i).Length);
        float maxX = ViewWidth > 0f ? Mathf.Max(0f, (longest + 2) * charW - ViewWidth) : 0f;
        float maxY = Mathf.Max(0f, ContentHeight - ViewHeight);
        scrollX = Mathf.Clamp(scrollX, 0f, maxX);
        scrollY = Mathf.Clamp(scrollY, 0f, maxY);
        rowsLayer.style.translate = new Translate(0, -scrollY);
        textLayer.style.translate = new Translate(-scrollX, -scrollY);
    }

    // Imlec pencerenin icinde kalsin (kenardan 2 harf pay)
    void EnsureCaretVisible()
    {
        var (l, c) = buffer.Position(buffer.Caret);
        float w = ViewWidth;
        if (w > 0f)
        {
            float x = c * charW, m = Mathf.Min(charW * 2f, w * 0.3f);
            if (x < scrollX + m) scrollX = x - m;
            else if (x > scrollX + w - m) scrollX = x - w + m;
        }
        EnsureLineVisible(l);
    }

    void EnsureLineVisible(int line)
    {
        float top = line * pitch, bottom = top + pitch, h = ViewHeight;
        if (top < scrollY) scrollY = top;
        else if (bottom > scrollY + h) scrollY = bottom - h;
        ApplyScroll();
    }

    // ---- Gorunum ----

    // Imlec yazarken 0.5 sn'de bir yanip soner; secim varken ya da yazmiyorken gizli.
    void UpdateCaret()
    {
        bool show = Editing && !blocks && !buffer.HasSelection;
        if (show)
        {
            if (buffer.Caret != caretSeen) { caretSeen = buffer.Caret; blinkStart = Time.unscaledTime; }
            show = (Time.unscaledTime - blinkStart) % 1f < 0.55f;
        }
        else caretSeen = -1;
        caret.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void Repaint()
    {
        int n = buffer.LineCount;
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            if (i > 0) sb.Append('\n');
            // bos satir da yer kaplasin
            string line = buffer.Line(i);
            sb.Append(line.Length == 0 ? " " : i == lifted ? CodeColors.Faded(line) : CodeColors.Line(line));
        }
        colored.text = sb.ToString();
        spans = CodeBlocks.Find(Lines());

        while (rows.Count < n) AddRow();
        while (rows.Count > n)
        {
            rows[rows.Count - 1].row.RemoveFromHierarchy();
            rows[rows.Count - 1].num.RemoveFromHierarchy();
            rows.RemoveAt(rows.Count - 1);
        }
        for (int i = 0; i < rows.Count; i++) rows[i].num.text = (i + 1).ToString();
        if (activeLine >= rows.Count) activeLine = -1;
        SetActiveLine(activeLine, activeError, activeVars);
        Layout();
    }

    void AddRow()
    {
        var row = new VisualElement { pickingMode = PickingMode.Ignore };
        row.style.position = Position.Absolute;
        row.style.left = 0; row.style.right = 0;
        Ui.Radius(row, 16);
        Transition(row, "background-color");

        var bar = new VisualElement { pickingMode = PickingMode.Ignore };
        bar.style.position = Position.Absolute;
        bar.style.left = 0; bar.style.top = 14; bar.style.bottom = 14; bar.style.width = 6;
        bar.style.backgroundColor = accent;
        Ui.Radius(bar, 3);
        bar.style.opacity = 0;
        Transition(bar, "opacity");
        row.Add(bar);

        var num = new Label { pickingMode = PickingMode.Ignore };
        Ui.NoSpacing(num);
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

    // Satir araligi (satir yuksekligi + paragraf boslugu) yazinin gercek yuksekliginden olculur. Yalnizca yazi yerlestikten
    // sonra (colored'un GeometryChanged'i) cagrilir; o an gosterilen satir sayisi colored.text'ten sayilir.
    void MeasurePitch()
    {
        float lh = probe.layout.height, h = colored.layout.height;
        int n = 1;
        foreach (char ch in colored.text) if (ch == '\n') n++;
        if (n > 1 && !float.IsNaN(lh) && lh > 1f && !float.IsNaN(h) && h > lh)
        {
            pitch = (h - lh) / (n - 1);
            pitchMeasured = true;
        }
    }

    // Olculeri yazinin gercek satir araligindan alir; satirlari, imleci, secimi yerlestirir; yuksekligi ve kaydirmayi ayarlar.
    void Layout()
    {
        float lh = probe.layout.height, pw = probe.layout.width;
        if (!float.IsNaN(lh) && lh > 1f)
        {
            lineHeight = lh;
            if (!pitchMeasured) pitch = lh + Spacing;
        }
        if (!float.IsNaN(pw) && pw > 1f) charW = pw / 10f;

        float top0 = Spacing * 0.5f;
        for (int i = 0; i < rows.Count; i++)
        {
            float y = top0 + i * pitch;
            rows[i].row.style.top = y - (pitch - lineHeight) * 0.5f;
            rows[i].row.style.height = pitch;
            rows[i].num.style.top = y;
            rows[i].num.style.height = lineHeight;
        }
        style.height = ViewHeight;

        LayoutGuides();
        PlaceVarsNote(); // harf genisligi olculunce yeri degisir

        // imlec
        var (cl, cc) = buffer.Position(buffer.Caret);
        caret.style.left = Mathf.Max(0f, cc * charW - 2f);
        caret.style.top = cl * pitch - 2f;
        caret.style.height = lineHeight + 4f;

        // secim: satir satir turuncu seritler
        selLayer.Clear();
        if (Editing && !blocks && buffer.HasSelection)
        {
            var (la, ca) = buffer.Position(buffer.SelectionStart);
            var (lb, cb) = buffer.Position(buffer.SelectionEnd);
            for (int li = la; li <= lb; li++)
            {
                int from = li == la ? ca : 0, to = li == lb ? cb : buffer.Line(li).Length + 1;
                var r = new VisualElement { pickingMode = PickingMode.Ignore };
                r.style.position = Position.Absolute;
                r.style.left = from * charW; r.style.width = Mathf.Max(0, to - from) * charW;
                r.style.top = li * pitch - (pitch - lineHeight) * 0.25f;
                r.style.height = lineHeight + (pitch - lineHeight) * 0.5f;
                r.style.backgroundColor = new Color(accent.r, accent.g, accent.b, 0.35f);
                Ui.Radius(r, 6);
                selLayer.Add(r);
            }
        }
        ApplyScroll();
    }

    // Blok cizgileri: baslik satirinin ilk harfinin altindan govdenin son satirina. Surukleme sirasinda satirin girecegi
    // blogun cizgisi turuncu ve birakilacak yere kadar uzun (govdesi henuz bos olan blokta da gorunur).
    void LayoutGuides()
    {
        guideLayer.Clear();
        bool ownerDrawn = false;
        foreach (var b in spans)
        {
            bool owner = b.Header == dropOwner;
            ownerDrawn |= owner;
            AddGuide(b.Header, b.Last, b.Level, owner);
        }
        if (dropOwner >= 0 && !ownerDrawn && dropOwner < buffer.LineCount)
            AddGuide(dropOwner, dropOwner, CodeBlocks.Level(buffer.Line(dropOwner)), true);
    }

    void AddGuide(int header, int last, int level, bool owner)
    {
        float top = header * pitch + lineHeight + 2f, bottom = last * pitch + lineHeight;
        if (owner) bottom = Mathf.Max(bottom, dropGap * pitch - (pitch - lineHeight) * 0.5f);
        if (bottom <= top) return;
        var g = new VisualElement { pickingMode = PickingMode.Ignore };
        g.style.position = Position.Absolute;
        g.style.left = (level * CodeBuffer.Indent.Length + 0.5f) * charW - (owner ? 2.5f : 1.5f);
        g.style.width = owner ? 5f : 3f;
        g.style.top = top;
        g.style.height = bottom - top;
        g.style.backgroundColor = owner ? accent : new Color(1f, 1f, 1f, 0.14f);
        Ui.Radius(g, 2);
        guideLayer.Add(g);
    }

    void TextStyle(TextElement t, Color c)
    {
        Ui.NoSpacing(t);
        t.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
        t.style.fontSize = FontSize;
        t.style.color = c;
        t.style.whiteSpace = WhiteSpace.Pre; // girinti bosluklari korunsun
        t.style.unityParagraphSpacing = Spacing;
        t.style.unityTextAlign = TextAnchor.UpperLeft;
    }



    static void Transition(VisualElement e, string prop)
    {
        e.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName(prop) };
        e.style.transitionDuration = new List<TimeValue> { new TimeValue(0.18f, TimeUnit.Second) };
        e.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.EaseOut) };
    }
}
