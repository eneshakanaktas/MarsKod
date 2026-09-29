using System;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Kod yazma kademesi (Acemi/Orta/Usta) secimi (tasarim: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §3, K6, K7).
// Kod kartinin sag ustunde, eskiden islevsiz olan "Python" yazisinin yerinde bir dugme; dokununca kucuk bir kutu acilir.
// Yer karari (Ragip, 09-29, Redmi 9C denemesi): dar telefonda alt sira (ipucu/klavye/Calistir/bastan al) sikismasin,
// kod alaniyla ilgili ayar kodun yaninda dursun diye Calistir'in yani yerine burasi secildi.
public class TierMenu : VisualElement
{
    static readonly (KeyboardTier tier, string kisa, string ad)[] Options =
    {
        (KeyboardTier.Acemi, "Acemi", "Hazır düğmeler"),
        (KeyboardTier.Orta, "Orta", "Klavye + öneri"),
        (KeyboardTier.Usta, "Usta", "Klavye"),
    };
    const float BoxW = 460f;

    // Bu bolumde bu kademeden XP alindi mi (kutu her acilista sorulur)
    public Func<KeyboardTier, bool> Earned;
    public event Action<KeyboardTier> Picked;

    readonly Label pillText;
    readonly VisualElement box;
    readonly Color accent, ink;
    readonly Dictionary<KeyboardTier, (VisualElement row, Label ad, Icon check)> rows
        = new Dictionary<KeyboardTier, (VisualElement, Label, Icon)>();
    KeyboardTier tier = KeyboardTier.Orta;

    public TierMenu(VisualElement overlay, Font fMed, Font fSemi, Color ink, Color accent, Color background, Color hairline)
    {
        this.ink = ink; this.accent = accent;
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        style.paddingLeft = 18; style.paddingRight = 18;
        style.paddingTop = 6; style.paddingBottom = 6;
        style.backgroundColor = new Color(1f, 1f, 1f, 0.06f);
        Ui.Radius(this, 22);

        pillText = new Label { pickingMode = PickingMode.Ignore };
        Ui.NoSpacing(pillText);
        pillText.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(fMed));
        pillText.style.fontSize = 26;
        pillText.style.color = new Color(ink.r, ink.g, ink.b, 0.6f);
        pillText.style.marginRight = 8;
        Add(pillText);
        var caret = new Icon(18, (p, r) => DrawCaret(p, r, ink));
        Add(caret);
        RegisterCallback<ClickEvent>(_ => { if (Open) Hide(); else Show(); });

        box = new VisualElement();
        box.style.position = Position.Absolute;
        box.style.width = BoxW;
        box.style.backgroundColor = background;
        Ui.Radius(box, 28);
        Ui.Border(box, 2, hairline);
        box.style.paddingTop = 12; box.style.paddingBottom = 12;
        box.style.display = DisplayStyle.None;
        foreach (var o in Options) box.Add(Row(o.tier, o.ad, fMed, fSemi));
        overlay.Add(box);

        SetTier(tier);
    }

    public bool Open => box.style.display == DisplayStyle.Flex;

    // picked, disariya tiklandiginda kapatmak icin: bu kutunun (dugme ya da acilir liste) parcasi mi.
    public bool Owns(VisualElement picked) => this == picked || Contains(picked) || box == picked || box.Contains(picked);

    public void SetTier(KeyboardTier t)
    {
        tier = t;
        pillText.text = Options[(int)t].kisa;
        Refresh();
    }

    static void DrawCaret(Painter2D p, Rect r, Color c)
    {
        float w = r.width, h = r.height;
        p.strokeColor = new Color(c.r, c.g, c.b, 0.6f);
        p.lineWidth = w * 0.16f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.2f, h * 0.38f));
        p.LineTo(new Vector2(w * 0.5f, h * 0.66f));
        p.LineTo(new Vector2(w * 0.8f, h * 0.38f));
        p.Stroke();
    }

    static void DrawCheck(Painter2D p, Rect r, Color c)
    {
        float w = r.width, h = r.height;
        p.strokeColor = c;
        p.lineWidth = w * 0.13f;
        p.lineCap = LineCap.Round;
        p.lineJoin = LineJoin.Round;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.14f, h * 0.52f));
        p.LineTo(new Vector2(w * 0.4f, h * 0.76f));
        p.LineTo(new Vector2(w * 0.86f, h * 0.26f));
        p.Stroke();
    }

    public void Show()
    {
        Refresh();
        if (!Place()) Hide();
    }

    public void Hide() => box.style.display = DisplayStyle.None;

    VisualElement Row(KeyboardTier t, string ad, Font fMed, Font fSemi)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.paddingLeft = 30; row.style.paddingRight = 26;
        row.style.paddingTop = 20; row.style.paddingBottom = 20;

        var adLabel = new Label(ad) { pickingMode = PickingMode.Ignore };
        Ui.NoSpacing(adLabel);
        adLabel.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(fMed));
        adLabel.style.fontSize = 32;
        adLabel.style.flexGrow = 1;
        row.Add(adLabel);

        var check = new Icon(30, (p, r) => DrawCheck(p, r, accent));
        row.Add(check);

        rows[t] = (row, adLabel, check);
        row.RegisterCallback<ClickEvent>(e => { e.StopPropagation(); Picked?.Invoke(t); Hide(); });
        return row;
    }

    void Refresh()
    {
        foreach (var (t, parts) in rows)
        {
            bool sel = t == tier;
            parts.row.style.backgroundColor = sel ? new Color(accent.r, accent.g, accent.b, 0.16f) : Color.clear;
            parts.ad.style.color = sel ? ink : new Color(ink.r, ink.g, ink.b, 0.72f);
            parts.check.style.display = (Earned?.Invoke(t) ?? false) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // Kutuyu dugmenin altina, sag kenari dugmeyle hizali koyar (ekranin disina tasmasin diye sola kayabilir).
    bool Place()
    {
        var r = worldBound;
        var area = box.parent.layout;
        if (float.IsNaN(area.width) || float.IsNaN(r.xMax)) return false;
        box.style.left = Mathf.Clamp(r.xMax - BoxW, 12f, Mathf.Max(12f, area.width - BoxW - 12f));
        box.style.top = r.yMax + 14f;
        box.style.display = DisplayStyle.Flex;
        return true;
    }
}
