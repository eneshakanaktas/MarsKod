using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Ipucu balonu (kod kartinin ustunde, ampul dugmesiyle acilir). Acilmis ipuclari alt alta gorunur:
// 1. yon gosterir, 2. konuyu anlatir, 3. kodun bir kismini verir (bolum dosyasi "ipuclari").
// Daha acilmamis ipucu varsa altta "Bir ipucu daha" dugmesi; kac ipucunun acik oldugu Oyun.cs'ten gelir (HintLog).
public class HintPanel : VisualElement
{
    public event Action MorePressed;

    readonly Font fMed, fSemi;
    readonly Color accent;
    VisualElement more; // "Bir ipucu daha" dugmesi (hepsi aciksa balonda yok)

    public HintPanel(Font fMed, Font fSemi, Color accent, Color background, Color hairline)
    {
        this.fMed = fMed; this.fSemi = fSemi; this.accent = accent;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0;
        style.bottom = Length.Percent(100);
        style.marginBottom = 20;
        style.backgroundColor = background;
        Ui.Radius(this, 36);
        Ui.Border(this, 2, hairline);
        style.paddingTop = 28; style.paddingBottom = 32;
        style.paddingLeft = 40; style.paddingRight = 40;
        style.display = DisplayStyle.None;
    }

    public bool Open => style.display == DisplayStyle.Flex;

    public void Show() => style.display = DisplayStyle.Flex;

    public void Hide() => style.display = DisplayStyle.None;

    // hints: bolumun butun ipuclari; shown: kacinin acik oldugu (en az 1)
    public void SetHints(IReadOnlyList<string> hints, int shown)
    {
        Clear();
        int count = Mathf.Min(shown, hints.Count);
        for (int i = 0; i < count; i++)
        {
            var tag = Ui.Text("İPUCU " + (i + 1) + "/" + hints.Count, fSemi, 22, new Color(1f, 1f, 1f, 0.45f));
            tag.style.letterSpacing = 3;
            if (i > 0) tag.style.marginTop = 26;
            Add(tag);
            var body = Ui.Text(CodeColors.Inline(hints[i]), fMed, 32, Color.white); // `kod` kod renginde
            body.enableRichText = true;
            body.style.whiteSpace = WhiteSpace.Normal;
            body.style.marginTop = 6;
            Add(body);
        }
        if (count < hints.Count) Add(MoreButton());
    }

    // "Bir ipucu daha" dugmesinin ortasi (panel koordinati); balon kapaliysa ya da dugme yoksa null. Deneme icin.
    public Vector2? MoreCenter()
    {
        if (!Open || more == null || more.parent != this) return null;
        return more.worldBound.center;
    }

    VisualElement MoreButton()
    {
        var b = more = new VisualElement();
        b.style.alignSelf = Align.FlexStart;
        b.style.marginTop = 26;
        b.style.paddingTop = 16; b.style.paddingBottom = 16;
        b.style.paddingLeft = 34; b.style.paddingRight = 34;
        Ui.Radius(b, 40);
        Ui.Border(b, 2, new Color(accent.r, accent.g, accent.b, 0.7f));
        b.Add(Ui.Text("Bir ipucu daha", fSemi, 28, accent));
        b.RegisterCallback<ClickEvent>(_ => MorePressed?.Invoke());
        return b;
    }
}
