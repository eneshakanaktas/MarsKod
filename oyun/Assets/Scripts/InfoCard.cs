using System;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Bilgi karti: bolumde yeni bir sey ilk kez goruldugunde ortada acilan kisa aciklama (bolum dosyasinda "bilgi").
// Arkadaki oyuna dokunus gecmez; yalniz "Anladım" kapatir. Kart, sol ustteki i dugmesiyle istenince tekrar acilir.
// Hud kurar ve ne zaman acilacagina karar verir; kart yalnizca gosterir.
public class InfoCard : VisualElement
{
    public event Action Closed;

    readonly Label title;
    readonly VisualElement paragraphs;
    readonly Font fMed;
    readonly Color ink;

    public InfoCard(Font fMed, Font fSemi, Font fBold, Color ink, Color accent, Color hairline)
    {
        this.fMed = fMed;
        this.ink = ink;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
        style.backgroundColor = new Color(0f, 0f, 0f, 0.66f);
        style.alignItems = Align.Center;
        style.justifyContent = Justify.Center;
        style.paddingLeft = 48; style.paddingRight = 48;
        style.display = DisplayStyle.None;
        RegisterCallback<PointerDownEvent>(e => e.StopPropagation());

        var panel = new VisualElement { pickingMode = PickingMode.Ignore };
        panel.style.width = Length.Percent(100);
        panel.style.maxWidth = 960;
        panel.style.maxHeight = Length.Percent(82);
        panel.style.backgroundColor = new Color(0.105f, 0.098f, 0.135f, 1f);
        Ui.Radius(panel, 44);
        Ui.Border(panel, 2, hairline);
        panel.style.paddingLeft = 48; panel.style.paddingRight = 48;
        panel.style.paddingTop = 40; panel.style.paddingBottom = 40;
        Add(panel);

        var tag = Ui.Text("BİLGİ", fSemi, 26, accent);
        tag.style.letterSpacing = 5;
        panel.Add(tag);

        title = Ui.Text("", fBold, 48, ink);
        title.style.whiteSpace = WhiteSpace.Normal;
        title.style.marginTop = 10;
        panel.Add(title);

        // uzun metin kutudan tasmasin: kayar
        var scroll = new ScrollView(ScrollViewMode.Vertical) { pickingMode = PickingMode.Position };
        scroll.style.flexShrink = 1;
        scroll.style.marginTop = 6;
        scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        paragraphs = scroll.contentContainer;
        panel.Add(scroll);

        panel.Add(BuildButton(fBold, accent));
    }

    public bool Open => style.display == DisplayStyle.Flex;

    public void Show(LevelInfo info)
    {
        title.text = info.Title;
        paragraphs.Clear();
        foreach (var text in info.Paragraphs)
        {
            var p = Ui.Text(CodeColors.Inline(text), fMed, 32, new Color(ink.r, ink.g, ink.b, 0.88f));
            p.enableRichText = true;
            p.style.whiteSpace = WhiteSpace.Normal;
            p.style.marginTop = 20;
            paragraphs.Add(p);
        }
        style.display = DisplayStyle.Flex;
    }

    public void Hide() => style.display = DisplayStyle.None;

    VisualElement BuildButton(Font fBold, Color accent)
    {
        var b = new VisualElement();
        b.style.marginTop = 36;
        b.style.height = 112;
        b.style.backgroundColor = accent;
        Ui.Radius(b, 56);
        b.style.alignItems = Align.Center;
        b.style.justifyContent = Justify.Center;
        b.Add(Ui.Text("Anladım", fBold, 42, Color.white));
        Ui.Transition(b, "scale", 0.12f, EasingMode.EaseOut);
        b.RegisterCallback<PointerDownEvent>(_ => b.style.scale = new Scale(new Vector3(0.96f, 0.96f, 1f)));
        b.RegisterCallback<PointerUpEvent>(_ => b.style.scale = new Scale(Vector3.one));
        b.RegisterCallback<PointerLeaveEvent>(_ => b.style.scale = new Scale(Vector3.one));
        b.RegisterCallback<ClickEvent>(_ => { Sound.Tap(); Closed?.Invoke(); });
        return b;
    }
}
