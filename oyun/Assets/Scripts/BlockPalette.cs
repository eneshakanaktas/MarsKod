using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Acemi paleti (tasarim: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §4): bolumun hazir satirlari
// ("parcalar") dugme olarak. Dugme tutulup koda suruklenir (BlockDrag); kisa dokunus satiri kodun sonuna ekler.
// Bu bolumde ilk kez gorulen parcanin dugmesi turuncu kenarli. Palet metin tutmaz; kod yalnizca CodeEditor.Edit ile degisir.
public class BlockPalette : VisualElement
{
    const float DragSlop = 14f, CodeSize = 36f;

    readonly CodeEditor editor;
    readonly BlockDrag drag;
    readonly Font mono;
    readonly Color ink, accent, chipBg, pressedBg, hairline;
    readonly VisualElement chipsBox;
    // Dugmeler parcalariyla (deneme icin tiklatmak amaciyla)
    readonly Dictionary<string, VisualElement> chipsByPiece = new Dictionary<string, VisualElement>();

    public BlockPalette(CodeEditor editor, BlockDrag drag, Font mono, Font captionFont, Color ink, Color accent, Color background)
    {
        this.editor = editor; this.drag = drag; this.mono = mono; this.ink = ink; this.accent = accent;
        chipBg = new Color(1f, 1f, 1f, 0.09f);
        pressedBg = new Color(1f, 1f, 1f, 0.22f);
        hairline = new Color(1f, 1f, 1f, 0.08f);

        style.backgroundColor = background;
        style.borderTopLeftRadius = 40; style.borderTopRightRadius = 40;
        style.paddingLeft = 24; style.paddingRight = 24;
        style.paddingTop = 22; style.paddingBottom = 30 + Ui.SafeBottom();

        var caption = new Label("Sürükle: istediğin satıra  ·  Dokun: sona ekle") { pickingMode = PickingMode.Ignore };
        Ui.NoSpacing(caption);
        caption.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(captionFont));
        caption.style.fontSize = 28;
        caption.style.color = new Color(ink.r, ink.g, ink.b, 0.45f);
        caption.style.unityTextAlign = TextAnchor.MiddleCenter;
        caption.style.marginBottom = 12;
        Add(caption);

        chipsBox = new VisualElement();
        chipsBox.style.flexDirection = FlexDirection.Row;
        chipsBox.style.flexWrap = Wrap.Wrap;
        chipsBox.style.justifyContent = Justify.Center;
        Add(chipsBox);
    }

    // Bolumun parcalari; fresh: bu bolumde yeni olanlar (turuncu kenarli)
    public void SetPieces(IEnumerable<string> pieces, ICollection<string> fresh)
    {
        chipsBox.Clear();
        chipsByPiece.Clear();
        foreach (var piece in pieces)
        {
            var chip = Chip(piece, fresh != null && fresh.Contains(piece));
            chipsByPiece[piece] = chip;
            chipsBox.Add(chip);
        }
    }

    // Parcanin dugmesinin ekrandaki yeri (panel koordinati); palet gorunmuyorsa null. Deneme icin.
    public Vector2? PieceCenter(string piece)
    {
        if (resolvedStyle.display == DisplayStyle.None || !chipsByPiece.TryGetValue(piece, out var chip)) return null;
        return chip.worldBound.center;
    }

    VisualElement Chip(string piece, bool fresh)
    {
        var chip = new VisualElement();
        chip.style.marginLeft = 8; chip.style.marginRight = 8; chip.style.marginTop = 8; chip.style.marginBottom = 8;
        chip.style.paddingLeft = 28; chip.style.paddingRight = 28; chip.style.paddingTop = 22; chip.style.paddingBottom = 22;
        chip.style.backgroundColor = chipBg;
        Ui.Radius(chip, 22);
        Ui.Border(chip, 3, fresh ? accent : hairline);

        var text = new Label(CodeColors.Line(piece)) { pickingMode = PickingMode.Ignore, enableRichText = true };
        Ui.NoSpacing(text);
        text.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(mono));
        text.style.fontSize = CodeSize;
        text.style.color = ink;
        text.style.whiteSpace = WhiteSpace.Pre;
        chip.Add(text);

        // Basinca bekler: parmak kayarsa surukleme baslar, kaymadan kalkarsa sona eklenir
        int pointer = -1;
        Vector2 down = default;
        void Release()
        {
            chip.style.backgroundColor = chipBg;
            if (pointer >= 0 && chip.HasPointerCapture(pointer)) chip.ReleasePointer(pointer);
            pointer = -1;
        }
        chip.RegisterCallback<PointerDownEvent>(e =>
        {
            e.StopPropagation();
            if (editor.ReadOnly || drag.Active) return;
            pointer = e.pointerId;
            down = e.position;
            chip.CapturePointer(pointer);
            chip.style.backgroundColor = pressedBg;
        });
        chip.RegisterCallback<PointerMoveEvent>(e =>
        {
            if (e.pointerId != pointer || ((Vector2)e.position - down).magnitude <= DragSlop) return;
            int id = pointer;
            Release();
            drag.BeginPiece(piece, e.position, id);
        });
        chip.RegisterCallback<PointerUpEvent>(e =>
        {
            if (e.pointerId != pointer) return;
            Release();
            Append(piece);
        });
        chip.RegisterCallback<PointerCancelEvent>(_ => Release());
        return chip;
    }

    // Kisa dokunus: satir kodun sonuna, varsayilan girintiyle (ustteki satir ":" ile bitiyorsa iceride)
    void Append(string piece)
        => editor.Edit(b => b.DropLine(b.LineCount, b.DefaultIndentLevel(b.LineCount), piece));
}
