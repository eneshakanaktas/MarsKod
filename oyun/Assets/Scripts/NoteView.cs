using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Notun kagit karti (Bolge 8, 76 ve 80; docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md, K3). Dunyada yazi yok kurali:
// harfler arayuzde, kagit renginde bir kartta. Kivilcim yurudukce o an okudugu harf turuncu, alti cizili; okunmuslar soluk,
// okunmayanlar normal. Dongunun harf harf ilerledigi gorunur. Tek satir, ortada; basligin altindaki koyu bantta (duvarin ust
// kismi) durur: arka duvardaki yataklari (76) ve haritayi (80) kapatmaz. Bolum bitince bitis yazilari oraya gelir: Oyun karti kapatir.
public class NoteView : VisualElement
{
    static readonly Color Paper = Mats.Hex("#EFE6CF");
    static readonly Color PaperEdge = Mats.Hex("#C9BC9A");
    static readonly Color InkColor = Mats.Hex("#2B2622");
    static readonly Color Current = Mats.Hex("#C4511A");     // okunan harf: koyu turuncu
    static readonly Color Underline = Mats.Hex("#E07A2E");
    const float ReadAlpha = 0.32f;
    const float Tilt = -2f;
    const float LetterSize = 52f, SmallLetterSize = 46f;
    const float AboveArea = -116f;   // oyun alaninin ustunden yukari: basligin altindaki bos bant (ust dugme sutununun yaninda)
    const int SmallFrom = 10;   // bu kadar ve daha cok harfte harfler kuculur: kart tasmaz

    readonly Font font;
    readonly VisualElement letters;
    readonly List<Label> cells = new List<Label>();
    int current = -1;

    public NoteView(Font font)
    {
        this.font = font;
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.top = AboveArea; style.left = 0; style.right = 0;
        style.alignItems = Align.Center;
        style.display = DisplayStyle.None;

        var card = new VisualElement { pickingMode = PickingMode.Ignore };
        card.style.backgroundColor = Paper;
        card.style.rotate = new Rotate(new Angle(Tilt, AngleUnit.Degree));
        card.style.paddingLeft = 26; card.style.paddingRight = 26; card.style.paddingTop = 8; card.style.paddingBottom = 12;
        // kose kivrimi: alt kenar biraz kalin ve koyu (kagidin altina golge dusmus gibi)
        card.style.borderBottomWidth = 5; card.style.borderRightWidth = 3;
        card.style.borderBottomColor = PaperEdge; card.style.borderRightColor = PaperEdge;
        card.style.borderTopLeftRadius = 4; card.style.borderTopRightRadius = 10;
        card.style.borderBottomLeftRadius = 10; card.style.borderBottomRightRadius = 4;
        Add(card);

        letters = new VisualElement { pickingMode = PickingMode.Ignore };
        letters.style.flexDirection = FlexDirection.Row;
        card.Add(letters);
    }

    // Su an vurgulu harf (denetim icin); not yoksa -1
    public int CurrentLetter => current;

    /// <summary>Karti acar: harfler sirayla, hicbiri vurgulu degil (her okunan notta bastan)</summary>
    public void Show(string note)
    {
        letters.Clear();
        cells.Clear();
        current = -1;
        float size = note.Length >= SmallFrom ? SmallLetterSize : LetterSize;
        foreach (char c in note)
        {
            var cell = Ui.Text(c.ToString(), font, size, InkColor);
            cell.style.marginLeft = 6; cell.style.marginRight = 6;
            cell.style.paddingLeft = 4; cell.style.paddingRight = 4;
            cell.style.borderBottomWidth = 6;
            cell.style.borderBottomColor = Color.clear;
            cells.Add(cell);
            letters.Add(cell);
        }
        style.display = DisplayStyle.Flex;
    }

    /// <summary>Siradaki harfi vurgular, oncekini soluk yapar. Son harften sonra yerinde kalir (bozuk kod hata vermez).</summary>
    public void Advance()
    {
        if (cells.Count == 0 || current >= cells.Count - 1) return;
        if (current >= 0) Mark(cells[current], InkColor, ReadAlpha, Color.clear);
        current++;
        Mark(cells[current], Current, 1f, Underline);
    }

    public void Hide()
    {
        style.display = DisplayStyle.None;
        letters.Clear();
        cells.Clear();
        current = -1;
    }

    static void Mark(Label cell, Color color, float alpha, Color line)
    {
        cell.style.color = new Color(color.r, color.g, color.b, alpha);
        cell.style.borderBottomColor = line;
    }
}
