using System.Collections;
using UnityEngine;

// panel_power() olcumu: robotun durdugu karenin ustunde, olculen sayi (kodun gordugu deger) kisa sure yukselip soner.
// Renk: 0 gri (kirik ya da panel yok), catlak turuncu, saglam yesil. Yazi her zaman kameraya bakar.
public static class PowerLabel
{
    public static IEnumerator Show(Transform parent, Vector3 localPos, int power, Color color) => Show(parent, localPos, power.ToString(), color);

    public static IEnumerator Show(Transform parent, Vector3 localPos, string label, Color color)
    {
        var text = WorldText.Create(parent, label, localPos, 0.528f, color);
        var go = text.gameObject;

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
