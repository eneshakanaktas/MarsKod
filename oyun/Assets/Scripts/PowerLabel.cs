using System.Collections;
using UnityEngine;

// panel_power() olcumu: robotun durdugu karenin ustunde, olculen sayi (kodun gordugu deger) kisa sure yukselip soner.
// Renk: 0 gri (kirik ya da panel yok), catlak turuncu, saglam yesil. Yazi her zaman kameraya bakar.
public static class PowerLabel
{
    static Font font;

    public static IEnumerator Show(Transform parent, Vector3 localPos, int power, Color color) => Show(parent, localPos, power.ToString(), color);

    public static IEnumerator Show(Transform parent, Vector3 localPos, string label, Color color)
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject("PowerLabel");
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMesh>();
        text.font = font;
        text.text = label;
        text.fontSize = 96;
        text.characterSize = 0.055f;
        text.fontStyle = FontStyle.Bold;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = font.material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var cam = Camera.main;
        yield return Tween.Run(1.1f, t =>
        {
            go.transform.localPosition = localPos + new Vector3(0f, 0.75f + 0.25f * Tween.OutCubic(t), 0f);
            if (cam != null) go.transform.rotation = cam.transform.rotation;
            go.transform.localScale = Vector3.one * (t < 0.15f ? Tween.OutBack(t / 0.15f) : 1f);
            var c = color;
            c.a = t < 0.7f ? 1f : 1f - (t - 0.7f) / 0.3f;
            text.color = c;
        });
        Object.Destroy(go);
    }
}
