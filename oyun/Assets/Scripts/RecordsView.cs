using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Kayitlar sayfasi (Bolumler ekranindaki "Kayitlar" satirindan): dinlenmis Defne kayitlari, numara siralamasiyla.
// Yalnizca gosterir ve secileni haber verir; liste Oyun.cs'ten gelir (dinlenmemis kayit listede yok).
public class RecordsView : FullScreenPanel
{
    public struct Entry
    {
        public int Number, Region;
        public string File, Preview;
    }

    public event Action<int> Picked; // secilen kaydin numarasi

    readonly Font fMed, fSemi;
    readonly Color ink;

    public RecordsView(Font fMed, Font fSemi, Font fBold, Color ink, Color buttonBg, Color hairline)
        : base("Kayıtlar", fBold, ink, buttonBg, hairline)
    {
        this.fMed = fMed; this.fSemi = fSemi;
        this.ink = ink;
    }

    public void Show(IReadOnlyList<Entry> entries)
    {
        Content.Clear();
        foreach (var e in entries) Content.Add(Row(e));
        ShowPanel();
    }

    // Kayit satirinin ortasi (panel koordinati); sayfa kapaliysa ya da kayit yoksa null. Deneme icin.
    public Vector2? RowCenter(int number)
    {
        if (!Open) return null;
        foreach (var row in Content.Children())
            if (row.userData is int n && n == number) return row.worldBound.center;
        return null;
    }

    VisualElement Row(Entry e)
    {
        var row = new VisualElement();
        row.style.marginBottom = 22;
        row.style.paddingTop = 28; row.style.paddingBottom = 28;
        row.style.paddingLeft = 36; row.style.paddingRight = 36;
        row.style.backgroundColor = new Color(1f, 1f, 1f, 0.05f);
        Ui.Radius(row, 44);
        Ui.Border(row, 2, new Color(1f, 1f, 1f, 0.07f));

        row.Add(Ui.Text("KAYIT " + e.Number + "  ·  BÖLGE " + e.Region, fSemi, 26, new Color(ink.r, ink.g, ink.b, 0.5f)));
        var preview = Ui.Text(e.File + " — " + e.Preview, fMed, 34, ink);
        preview.style.whiteSpace = WhiteSpace.Normal;
        preview.style.marginTop = 6;
        row.Add(preview);

        int number = e.Number;
        row.userData = number;
        row.RegisterCallback<ClickEvent>(_ => { Sound.Tap(); Picked?.Invoke(number); });
        return row;
    }
}
