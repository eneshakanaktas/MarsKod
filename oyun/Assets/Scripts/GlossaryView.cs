using System.Collections.Generic;
using System.Linq;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Kod sozlugu ekrani (sol ustteki kitap dugmesi): ustte sayfa kisayollari, altinda her kelime icin bir kart
// (ne ise yarar + renkli ornek kod). Kisayola dokununca o karta kayar. Oynanan bolumde yeni olanlar "YENI" isaretli.
// Sayfalar ve hangi bolumde acildiklari Oyun.cs'ten gelir (Entry); burasi yalnizca gosterir.
public class GlossaryView : FullScreenPanel
{
    public struct Entry
    {
        public GlossaryPage Page;
        public int OpensAt; // sayfanin acildigi bolum
        public bool Fresh;  // oynanan bolumde yeni
    }

    readonly Font fMed, fSemi, fMono;
    readonly Color ink, accent;
    readonly Dictionary<string, VisualElement> cards = new Dictionary<string, VisualElement>();

    public GlossaryView(Font fMed, Font fSemi, Font fBold, Font fMono, Color ink, Color accent, Color buttonBg, Color hairline)
        : base("Kod sözlüğü", fBold, ink, buttonBg, hairline)
    {
        this.fMed = fMed; this.fSemi = fSemi; this.fMono = fMono;
        this.ink = ink; this.accent = accent;
    }

    public void Show(IReadOnlyList<Entry> entries)
    {
        Content.Clear();
        cards.Clear();
        Content.Add(Shortcuts(entries));
        foreach (var e in entries)
        {
            var card = Card(e);
            cards[e.Page.Title] = card;
            Content.Add(card);
        }
        ShowPanel();
    }

    // Kisayolun ortasi (panel koordinati); ekran kapaliysa ya da sayfa yoksa null. Deneme icin.
    public Vector2? ShortcutCenter(string title)
    {
        if (!Open || Content.childCount == 0) return null;
        var chip = Content[0].Children().FirstOrDefault(c => (c.userData as string) == title);
        return chip?.worldBound.center;
    }

    // Kart kayan alanin icinde tamamen gorunuyor mu; deneme icin (kisayol karta kaydirdi mi).
    public bool CardInView(string title)
    {
        if (!cards.TryGetValue(title, out var c)) return false;
        Rect view = Scroll.contentViewport.worldBound, card = c.worldBound;
        return card.yMin >= view.yMin - 1f && card.yMax <= view.yMax + 1f;
    }

    VisualElement Shortcuts(IReadOnlyList<Entry> entries)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.flexWrap = Wrap.Wrap;
        row.style.marginBottom = 18;
        foreach (var e in entries)
        {
            var chip = new VisualElement { userData = e.Page.Title };
            chip.style.paddingLeft = 28; chip.style.paddingRight = 28;
            chip.style.paddingTop = 14; chip.style.paddingBottom = 14;
            chip.style.marginRight = 16; chip.style.marginBottom = 16;
            chip.style.backgroundColor = new Color(1f, 1f, 1f, 0.07f);
            Ui.Radius(chip, 34);
            if (e.Fresh) Ui.Border(chip, 2, accent);
            chip.Add(Ui.Text(e.Page.Title, fMono, 32, ink));
            string title = e.Page.Title;
            // kart ekranin ustune hizalanir (sondaki kartlar hizalanamaz; liste en alta kadar kayar, kart yine tam gorunur)
            chip.RegisterCallback<ClickEvent>(_ => Scroll.verticalScroller.value = cards[title].layout.y);
            row.Add(chip);
        }
        return row;
    }

    VisualElement Card(Entry e)
    {
        var card = new VisualElement();
        card.style.marginBottom = 22;
        card.style.paddingTop = 30; card.style.paddingBottom = 34;
        card.style.paddingLeft = 36; card.style.paddingRight = 36;
        card.style.backgroundColor = new Color(1f, 1f, 1f, 0.05f);
        Ui.Radius(card, 40);
        Ui.Border(card, 2, e.Fresh ? new Color(accent.r, accent.g, accent.b, 0.55f) : new Color(1f, 1f, 1f, 0.07f));

        var top = new VisualElement();
        top.style.flexDirection = FlexDirection.Row;
        top.style.alignItems = Align.Center;
        var title = Ui.Text(e.Page.Title, fMono, 46, accent);
        title.style.flexGrow = 1;
        top.Add(title);
        if (e.Fresh) top.Add(Tag("YENİ", accent, Color.white));
        top.Add(Tag("BÖLÜM " + e.OpensAt, new Color(1f, 1f, 1f, 0.08f), new Color(ink.r, ink.g, ink.b, 0.6f)));
        card.Add(top);

        var text = Ui.Text(e.Page.Text, fMed, 30, new Color(ink.r, ink.g, ink.b, 0.88f));
        text.style.whiteSpace = WhiteSpace.Normal;
        text.style.marginTop = 14;
        card.Add(text);

        var code = new VisualElement();
        code.style.marginTop = 22;
        code.style.paddingTop = 20; code.style.paddingBottom = 20;
        code.style.paddingLeft = 26; code.style.paddingRight = 26;
        code.style.backgroundColor = new Color(0f, 0f, 0f, 0.3f);
        Ui.Radius(code, 24);
        foreach (var line in e.Page.Example.TrimEnd('\n').Split('\n'))
        {
            var l = Ui.Text(CodeColors.Line(line), fMono, 29, ink);
            l.enableRichText = true;
            l.style.whiteSpace = WhiteSpace.Pre; // girinti bosluklari korunsun
            l.style.marginTop = 4; l.style.marginBottom = 4;
            code.Add(l);
        }
        card.Add(code);
        return card;
    }

    Label Tag(string s, Color bg, Color fg)
    {
        var t = Ui.Text(s, fSemi, 22, fg);
        t.style.letterSpacing = 2;
        t.style.marginLeft = 12;
        t.style.paddingLeft = 16; t.style.paddingRight = 16;
        t.style.paddingTop = 6; t.style.paddingBottom = 6;
        t.style.backgroundColor = bg;
        Ui.Radius(t, 20);
        return t;
    }
}
