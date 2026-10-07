using UnityEngine;

// Sahnedeki (3B) yazi: olcum etiketi, alet cantasinin etiketi, robotun ad etiketi.
// height: bir satirin dunyadaki yuksekligi (birim). Yazi kendi -z yuzune bakar (dondurmek cagirana kalir).
public static class WorldText
{
    const int FontSize = 96;
    static Font font;

    public static TextMesh Create(Transform parent, string text, Vector3 localPos, float height, Color color)
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject("Yazi");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var mesh = go.AddComponent<TextMesh>();
        mesh.font = font;
        mesh.text = text;
        mesh.fontSize = FontSize;
        mesh.characterSize = height * 10f / FontSize;
        mesh.fontStyle = FontStyle.Bold;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = color;
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = font.material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return mesh;
    }
}
