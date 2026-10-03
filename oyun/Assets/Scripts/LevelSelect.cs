using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Bolum secme ekrani: butun ekrani kaplayan koyu katman, ustte ayar satiri (arka plan animasyonlari), altinda bolumler
// alt alta sira sira. Yalnizca gosterir ve secileni/degisen ayari haber verir; hangi bolumun cozuldugu, kac XP getirdigi, kilitli olup olmadigi
// Oyun.cs'ten gelir (Entry). Kilit kurali: bir onceki bolum bitirilmeden sonraki acilmaz (Oyun.LevelEntries).
public class LevelSelect : FullScreenPanel
{
    public struct Entry
    {
        public int Number;
        public string Title, Goal;
        public int Xp;       // bu bolumden kazanilan toplam XP
        public bool Current; // su an oynanan bolum
        public bool Locked;  // onceki bolum bitirilmedigi icin henuz girilemez
    }

    public event Action<int> Picked; // secilen bolumun numarasi
    public event Action AnimationsToggled;

    readonly Font fMed, fSemi, fBold;
    readonly Color ink, accent;
    bool animationsOn = true;
    VisualElement animSwitch, animKnob;

    public LevelSelect(Font fMed, Font fSemi, Font fBold, Color ink, Color accent, Color buttonBg, Color hairline)
        : base("Bölümler", fBold, ink, buttonBg, hairline)
    {
        this.fMed = fMed; this.fSemi = fSemi; this.fBold = fBold;
        this.ink = ink; this.accent = accent;
    }

    public void Show(IReadOnlyList<Entry> entries)
    {
        Content.Clear();
        Content.Add(AnimationsRow());
        foreach (var e in entries) Content.Add(Row(e));
        ShowPanel();
    }

    public void SetAnimations(bool on)
    {
        animationsOn = on;
        PaintSwitch();
    }

    // Ayar: arka plandaki suregiden hareketler (yildizlar, dronlar, yanip sonen isiklar). Kapaliyken telefon daha az yorulur.
    VisualElement AnimationsRow()
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.marginBottom = 34;
        row.style.paddingTop = 22; row.style.paddingBottom = 22;
        row.style.paddingLeft = 36; row.style.paddingRight = 32;
        Ui.Radius(row, 36);
        Ui.Border(row, 2, new Color(1f, 1f, 1f, 0.07f));

        var texts = new VisualElement();
        texts.style.flexGrow = 1;
        texts.style.flexShrink = 1;
        texts.Add(Ui.Text("Arka plan animasyonları", fSemi, 34, ink));
        var note = Ui.Text("Kapalıyken telefon daha az yorulur", fMed, 26, new Color(ink.r, ink.g, ink.b, 0.55f));
        note.style.marginTop = 4;
        note.style.whiteSpace = WhiteSpace.Normal;
        texts.Add(note);
        row.Add(texts);

        animSwitch = new VisualElement();
        animSwitch.style.width = 120; animSwitch.style.height = 68;
        animSwitch.style.flexShrink = 0;
        animSwitch.style.marginLeft = 24;
        Ui.Radius(animSwitch, 34);
        animSwitch.style.justifyContent = Justify.Center;
        animKnob = new VisualElement();
        animKnob.style.width = 52; animKnob.style.height = 52;
        Ui.Radius(animKnob, 26);
        animKnob.style.backgroundColor = Color.white;
        animSwitch.Add(animKnob);
        row.Add(animSwitch);
        PaintSwitch();

        row.RegisterCallback<ClickEvent>(_ => { Sound.Tap(); AnimationsToggled?.Invoke(); });
        return row;
    }

    void PaintSwitch()
    {
        if (animSwitch == null) return;
        animSwitch.style.backgroundColor = animationsOn ? accent : new Color(1f, 1f, 1f, 0.16f);
        animKnob.style.marginLeft = animationsOn ? 60 : 8;
    }

    // Bolum satirinin ortasi (panel koordinati); ekran kapaliysa ya da satir yoksa null. Deneme icin.
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
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.marginBottom = 22;
        row.style.paddingTop = 30; row.style.paddingBottom = 30;
        row.style.paddingLeft = 32; row.style.paddingRight = 36;
        row.style.backgroundColor = e.Current ? new Color(accent.r, accent.g, accent.b, 0.14f) : new Color(1f, 1f, 1f, 0.05f);
        Ui.Radius(row, 44);
        Ui.Border(row, 2, e.Current ? new Color(accent.r, accent.g, accent.b, 0.55f) : new Color(1f, 1f, 1f, 0.07f));
        if (e.Locked) row.style.opacity = 0.4f;

        var badge = new VisualElement();
        badge.style.width = 96; badge.style.height = 96;
        Ui.Radius(badge, 48);
        badge.style.backgroundColor = e.Xp > 0 ? accent : new Color(1f, 1f, 1f, 0.1f);
        badge.style.alignItems = Align.Center;
        badge.style.justifyContent = Justify.Center;
        badge.style.marginRight = 30;
        if (e.Locked) badge.Add(new Icon(40, DrawLock));
        else if (e.Xp > 0) badge.Add(new Icon(46, (p, r) => TierMenu.DrawCheck(p, r, Color.white)));
        else badge.Add(Ui.Text(e.Number.ToString(), fBold, 44, ink));
        row.Add(badge);

        var texts = new VisualElement();
        texts.style.flexGrow = 1;
        texts.style.flexShrink = 1;
        texts.Add(Ui.Text("BÖLÜM " + e.Number, fSemi, 24, new Color(ink.r, ink.g, ink.b, 0.5f)));
        var title = Ui.Text(e.Locked ? "???" : e.Title, fSemi, 40, ink);
        title.style.whiteSpace = WhiteSpace.Normal;
        texts.Add(title);
        if (!e.Locked)
        {
            var goal = Ui.Text(e.Goal, fMed, 28, new Color(ink.r, ink.g, ink.b, 0.6f));
            goal.style.whiteSpace = WhiteSpace.Normal;
            goal.style.marginTop = 4;
            texts.Add(goal);
        }
        row.Add(texts);

        if (e.Xp > 0) row.Add(Ui.Text(e.Xp + " XP", fSemi, 32, accent));

        int number = e.Number;
        row.userData = number;
        if (!e.Locked) row.RegisterCallback<ClickEvent>(_ => { Hide(); Picked?.Invoke(number); });
        return row;
    }

    // Kilit: yuvarlak gövde + üstte kavisli kilit kolu.
    static void DrawLock(Painter2D p, Rect r)
    {
        float w = r.width, h = r.height;
        var c = new Color(1f, 1f, 1f, 0.55f);
        p.strokeColor = c;
        p.lineWidth = w * 0.11f;
        p.lineCap = LineCap.Round;
        p.BeginPath();
        p.Arc(new Vector2(w * 0.5f, h * 0.42f), w * 0.2f, new Angle(200, AngleUnit.Degree), new Angle(-20, AngleUnit.Degree));
        p.Stroke();
        p.fillColor = c;
        p.BeginPath();
        p.MoveTo(new Vector2(w * 0.24f, h * 0.42f));
        p.LineTo(new Vector2(w * 0.76f, h * 0.42f));
        p.LineTo(new Vector2(w * 0.76f, h * 0.84f));
        p.LineTo(new Vector2(w * 0.24f, h * 0.84f));
        p.ClosePath();
        p.Fill();
    }
}
