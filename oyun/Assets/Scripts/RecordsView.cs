using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// "Kayıtlar" sayfasi: Defne'nin ses kayitlari sirayla. Dinlenmis kayda dokununca yeniden calar; dinlenmemisler kilitli.
// Yalnizca gosterir ve secileni haber verir; hangisinin dinlendigi Oyun.cs'ten gelir (Entry).
public class RecordsView : FullScreenPanel
{
    public struct Entry
    {
        public int Number;
        public string File, FirstLine;
        public bool Heard;
    }

    public event Action<int> Picked;   // calinacak kaydin numarasi

    readonly Font fMed, fSemi, fBold;
    readonly Color ink, accent;

    public RecordsView(Font fMed, Font fSemi, Font fBold, Color ink, Color accent, Color buttonBg, Color hairline)
        : base("Kayıtlar", fBold, ink, buttonBg, hairline)
    {
        this.fMed = fMed; this.fSemi = fSemi; this.fBold = fBold;
        this.ink = ink; this.accent = accent;
    }

    public void Show(IReadOnlyList<Entry> entries)
    {
        Content.Clear();
        foreach (var e in entries) Content.Add(Row(e));
        ShowPanel();
    }

    VisualElement Row(Entry e)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.marginBottom = 22;
        row.style.paddingTop = 30; row.style.paddingBottom = 30;
        row.style.paddingLeft = 32; row.style.paddingRight = 36;
        row.style.backgroundColor = new Color(1f, 1f, 1f, 0.05f);
        Ui.Radius(row, 44);
        Ui.Border(row, 2, new Color(1f, 1f, 1f, 0.07f));
        if (!e.Heard) row.style.opacity = 0.4f;

        var badge = new VisualElement();
        badge.style.width = 96; badge.style.height = 96;
        Ui.Radius(badge, 48);
        badge.style.backgroundColor = e.Heard ? accent : new Color(1f, 1f, 1f, 0.1f);
        badge.style.alignItems = Align.Center;
        badge.style.justifyContent = Justify.Center;
        badge.style.marginRight = 30;
        badge.Add(Ui.Text(e.Number.ToString(), fBold, 44, ink));
        row.Add(badge);

        var texts = new VisualElement();
        texts.style.flexGrow = 1;
        texts.style.flexShrink = 1;
        texts.Add(Ui.Text("KAYIT " + e.Number, fSemi, 24, new Color(ink.r, ink.g, ink.b, 0.5f)));
        texts.Add(Ui.Text(e.Heard ? e.File : "???", fSemi, 40, ink));
        if (e.Heard)
        {
            var first = Ui.Text("“" + e.FirstLine + "”", fMed, 28, new Color(ink.r, ink.g, ink.b, 0.6f));
            first.style.whiteSpace = WhiteSpace.Normal;
            first.style.marginTop = 4;
            texts.Add(first);
        }
        row.Add(texts);

        int number = e.Number;
        if (e.Heard) row.RegisterCallback<ClickEvent>(_ => { Sound.Tap(); Picked?.Invoke(number); });
        return row;
    }
}
