using System;
using UnityEngine;
using UnityEngine.UIElements;

// Sahnelerin "Atla" dugmesi (Bolge 8 · 3b, K4): kucuk koyu bir dugme, ustunde sahnenin kac saniye daha surecegi ("12 sn").
// Dokununca bir kez onTap cagrilir ve dugme kapanir. Yalniz kendi alanina dokunulur; altindaki sahne ve arayuz serbest.
// Yeri sag ust, pusulanin alti: altta kod karti (78), ekran kutusu (78) ve hatira kutusu (80) var, onlara binmesin.
public class SkipButton : VisualElement
{
    const float Margin = 44f, TopUnderCompass = 340f;

    readonly Label seconds;
    Action onTap;

    public bool Open => style.display == DisplayStyle.Flex;
    public string SecondsText => seconds.text;   // goruntu denetimi

    public SkipButton(Font font, Color ink, Color background)
    {
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.right = Margin;
        style.alignItems = Align.Center;
        style.display = DisplayStyle.None;

        seconds = Ui.Text("", font, 30, new Color(ink.r, ink.g, ink.b, 0.75f));
        seconds.style.marginBottom = 8;
        Add(seconds);

        var button = new VisualElement();
        button.style.paddingLeft = 40; button.style.paddingRight = 40;
        button.style.paddingTop = 18; button.style.paddingBottom = 18;
        button.style.backgroundColor = background;
        Ui.Radius(button, 40);
        button.Add(Ui.Text("Atla", font, 38, ink));
        button.RegisterCallback<ClickEvent>(_ =>
        {
            var tap = onTap;
            Hide();
            Sound.Tap();
            tap?.Invoke();
        });
        Add(button);
    }

    public void Show(Action tap)
    {
        onTap = tap;
        style.top = Ui.SafeTop() + TopUnderCompass;
        style.display = DisplayStyle.Flex;
    }

    public void SetSeconds(int left) => seconds.text = left + " sn";

    public void Hide()
    {
        onTap = null;
        style.display = DisplayStyle.None;
    }
}
