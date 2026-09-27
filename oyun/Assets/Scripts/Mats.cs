using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

// Malzemeler: editorde hazirlanan sablonlar (Resources/Materials) kopyalanip renklendirilir.
public static class Mats
{
    static Material outline;

    static Material Template(string name, string shader)
    {
        var m = Resources.Load<Material>("Materials/" + name);
        if (m == null) m = new Material(Shader.Find(shader));
        return m;
    }

    public static Material Lit(Color c, float smooth = 0.35f)
    {
        var m = new Material(Template("Lit", "Universal Render Pipeline/Lit"));
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", 0f);
        return m;
    }

    public static Material Emissive(Color c, Color emission, float smooth = 0.5f)
    {
        var m = new Material(Template("LitEmissive", "Universal Render Pipeline/Lit"));
        m.EnableKeyword("_EMISSION");
        m.SetColor("_BaseColor", c);
        m.SetColor("_EmissionColor", emission);
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", 0f);
        return m;
    }

    public static Material Unlit(Color c)
    {
        var m = new Material(Template("Unlit", "Universal Render Pipeline/Unlit"));
        m.SetColor("_BaseColor", c);
        return m;
    }

    public static Material Custom(string name, string shader) => new Material(Template(name, shader));

    public static Material Outline
    {
        get
        {
            if (outline == null)
            {
                outline = new Material(Template("Outline", "MarsKod/Outline"));
                outline.SetColor("_OutlineColor", Hex("#110E15"));
            }
            return outline;
        }
    }

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}

public static class Parts
{
    public static Transform Empty(string name, Transform parent)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        return t;
    }

    public static Transform Add(string name, Transform parent, Mesh mesh, Material mat, Vector3 localPos,
        bool outline = true, bool castShadow = true)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = outline ? new[] { mat, Mats.Outline } : new[] { mat };
        mr.shadowCastingMode = castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
        return go.transform;
    }
}

public static class Tween
{
    public static IEnumerator Run(float duration, Action<float> step)
    {
        float t = 0f;
        while (t < duration)
        {
            step(t / duration);
            yield return null;
            t += Time.deltaTime;
        }
        step(1f);
    }

    public static IEnumerator Wait(float s)
    {
        float t = 0f;
        while (t < s) { yield return null; t += Time.deltaTime; }
    }

    public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
    public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    public static float InCubic(float t) => t * t * t;
    public static float OutBack(float t) { const float c1 = 1.70158f, c3 = c1 + 1f; return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f); }
}
