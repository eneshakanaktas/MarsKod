using UnityEngine;
using UnityEngine.UIElements;

// Arayuz ogeleri icin ortak kucuk yardimcilar. Olculer 1080 genislikli telefon ekranina gore (Hud).
public static class Ui
{
    public static void Radius(VisualElement e, float r)
    {
        e.style.borderTopLeftRadius = r; e.style.borderTopRightRadius = r;
        e.style.borderBottomLeftRadius = r; e.style.borderBottomRightRadius = r;
    }

    public static void Border(VisualElement e, float w, Color c)
    {
        e.style.borderTopWidth = w; e.style.borderBottomWidth = w; e.style.borderLeftWidth = w; e.style.borderRightWidth = w;
        e.style.borderTopColor = c; e.style.borderBottomColor = c; e.style.borderLeftColor = c; e.style.borderRightColor = c;
    }

    // Kenar ve ic bosluklari sifirlar (Unity'nin varsayilan bosluklari olculeri bozmasin)
    public static void NoSpacing(VisualElement e)
    {
        e.style.marginLeft = 0; e.style.marginRight = 0; e.style.marginTop = 0; e.style.marginBottom = 0;
        e.style.paddingLeft = 0; e.style.paddingRight = 0; e.style.paddingTop = 0; e.style.paddingBottom = 0;
    }

    // Tek parca yazi (dokunmayi gecirir): yazi tipi, boyut, renk
    public static Label Text(string s, Font font, float size, Color color)
    {
        var l = new Label(s) { pickingMode = PickingMode.Ignore };
        NoSpacing(l);
        l.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(font));
        l.style.fontSize = size;
        l.style.color = color;
        return l;
    }

    // Telefonun alttaki guvenli payi (hareket cubugu vb.), arayuz biriminde
    public static float SafeBottom()
    {
        var sa = Screen.safeArea;
        return Mathf.Max(0f, sa.yMin) * 1080f / Mathf.Max(1, Screen.width);
    }

    // Telefonun ustteki guvenli payi (çentik vb.), arayuz biriminde
    public static float SafeTop()
    {
        var sa = Screen.safeArea;
        return Mathf.Max(0f, Screen.height - sa.yMax) * 1080f / Mathf.Max(1, Screen.width);
    }
}
