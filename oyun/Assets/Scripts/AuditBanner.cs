using UnityEngine;
using UnityEngine.UIElements;

// Izleme denetiminin seridi (MarsKod.exe -izle): ekranin ust kenarinda kucuk, yari saydam bir hap; deneme bilgisini ve bitince
// sonucu ("→ TAMAM" yesil, "→ HATA" kirmizi) gosterir. Dokunmayi engellemez; yalnizca denetim modunda gorunur.
// Sonuc oku ("→") Poppins'te yok: sonuc yazisi JetBrains Mono ile.
public class AuditBanner : VisualElement
{
    static readonly Color Good = Mats.Hex("#7CF07A"), Bad = Mats.Hex("#FF6B6B");

    readonly Label text, result;

    public AuditBanner(Font textFont, Font resultFont)
    {
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0;
        style.top = Ui.SafeTop() + 4;
        style.alignItems = Align.Center;
        style.display = DisplayStyle.None;

        var pill = new VisualElement { pickingMode = PickingMode.Ignore };
        pill.style.flexDirection = FlexDirection.Row;
        pill.style.alignItems = Align.Center;
        pill.style.maxWidth = 1000;
        pill.style.paddingLeft = 24; pill.style.paddingRight = 24;
        pill.style.paddingTop = 6; pill.style.paddingBottom = 6;
        pill.style.backgroundColor = new Color(0.04f, 0.04f, 0.06f, 0.82f);
        Ui.Radius(pill, 26);

        text = Ui.Text("", textFont, 26, new Color(1f, 1f, 1f, 0.95f));
        text.style.whiteSpace = WhiteSpace.Normal;
        text.style.flexShrink = 1;
        pill.Add(text);

        result = Ui.Text("", resultFont, 26, Good);
        result.style.marginLeft = 14;
        result.style.flexShrink = 0;
        pill.Add(result);
        Add(pill);
    }

    public bool Open => style.display == DisplayStyle.Flex;

    /// <summary>Yeni deneme: bilgi yazilir, sonuc silinir</summary>
    public void Show(string info)
    {
        text.text = info;
        result.text = "";
        style.display = DisplayStyle.Flex;
    }

    /// <summary>Deneme bitti: yanina yesil "→ TAMAM" ya da kirmizi "→ HATA"</summary>
    public void Finish(bool ok)
    {
        result.text = ok ? "→ TAMAM" : "→ HATA";
        result.style.color = ok ? Good : Bad;
    }

    public void Hide() => style.display = DisplayStyle.None;
}
