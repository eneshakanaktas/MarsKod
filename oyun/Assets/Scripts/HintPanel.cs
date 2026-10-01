using System;
using UnityEngine;
using UnityEngine.UIElements;

// Ipucu balonu: ust basligin ortasinda (sol ve sagdaki dugmelerin arasinda) acilir, baslikla yer degistirir;
// bu yuzden oyun alani hic kuculmez. Bir seferde tek ipucu gorunur (1. yon gosterir, 2. konuyu anlatir, 3. kodun bir kismini verir).
// Hangi ipucunun gorunecegini Oyun.cs belirler (ampul dongusu, HintView); balon yalnizca gosterir.
// Sag ustteki × her an kapatir.
public class HintPanel : VisualElement
{
    public event Action ClosePressed;

    readonly Label tag, body, note;
    VisualElement closeButton;

    // top: balonun ust kenari (baslikta dugmelerle ayni hizada); sideInset: soldaki/sagdaki dugmelerin genisligi + bosluk
    public HintPanel(Font fMed, Font fSemi, Color background, Color hairline, float top, float sideInset)
    {
        pickingMode = PickingMode.Position;
        style.position = Position.Absolute;
        style.left = sideInset; style.right = sideInset;
        style.top = top;
        style.backgroundColor = background;
        Ui.Radius(this, 36);
        Ui.Border(this, 2, hairline);
        style.paddingTop = 20; style.paddingBottom = 22;
        style.paddingLeft = 36; style.paddingRight = 24;
        style.display = DisplayStyle.None;

        var head = new VisualElement { pickingMode = PickingMode.Ignore };
        head.style.flexDirection = FlexDirection.Row;
        head.style.alignItems = Align.Center;
        head.style.justifyContent = Justify.SpaceBetween;
        tag = Ui.Text("", fSemi, 22, new Color(1f, 1f, 1f, 0.45f));
        tag.style.letterSpacing = 3;
        head.Add(tag);
        head.Add(CloseButton());
        Add(head);

        body = Ui.Text("", fMed, 30, Color.white);
        body.enableRichText = true;
        body.style.whiteSpace = WhiteSpace.Normal;
        body.style.marginRight = 12;
        Add(body);

        note = Ui.Text("", fMed, 22, new Color(1f, 1f, 1f, 0.4f));
        note.style.marginTop = 10;
        Add(note);
    }

    public bool Open => style.display == DisplayStyle.Flex;

    public void Hide() => style.display = DisplayStyle.None;

    // n. ipucu (1'den baslar), toplam total ipucu var. Metindeki `kod` kod renginde gorunur.
    public void Show(string text, int n, int total)
    {
        tag.text = "İPUCU " + n + "/" + total;
        body.text = CodeColors.Inline(text);
        note.text = n < total ? "Ampule bas: " + (n + 1) + ". ipucu" : "Ampule bas: kapat";
        style.display = DisplayStyle.Flex;
    }

    // × dugmesinin ortasi (balon yerlesim koordinati); balon kapaliysa null. Deneme icin.
    public Vector2? CloseCenter() => Open ? closeButton.worldBound.center : (Vector2?)null;

    // Dokunma alani kucuk simgeden genis (parmakla kolay basilsin)
    VisualElement CloseButton()
    {
        var b = closeButton = new VisualElement();
        b.style.width = 64; b.style.height = 64;
        b.style.alignItems = Align.Center;
        b.style.justifyContent = Justify.Center;
        b.Add(new Icon(30, FullScreenPanel.DrawClose));
        b.RegisterCallback<ClickEvent>(_ => ClosePressed?.Invoke());
        return b;
    }
}
