using UnityEngine;

// Zemine yatik sahte isik lekesi: yumusak, kenari daginik, zemini aydinlatir (Decal.shader ekleme). Telefonda gercek nokta isik
// pahali oldugu icin lamba ve ekranlarin yere dusen isigi boyle verilir. Serce'nin isigi (ScoutLight) da ayni dokuyu kullanir.
public static class LightPool
{
    static Texture2D blob;

    // localPos: lekenin ortasi (zeminin hemen ustu); size: en ve boy; strength: 0 sonuk. Doner: malzeme (_A ile soner/yanar).
    public static Material Create(Transform parent, Vector3 localPos, Vector2 size, Color color, float strength, string name = "IsikLekesi")
    {
        var mat = Additive(color);
        mat.SetFloat("_A", strength);
        Parts.Add(name, parent, MeshFactory.Quad(size), mat, localPos + Vector3.up * 0.012f, outline: false, castShadow: false);
        return mat;
    }

    // Ekleyici leke malzemesi (zemini aydinlatir)
    public static Material Additive(Color color)
    {
        var mat = Mats.Custom("Decal", "MarsKod/Decal");
        mat.SetTexture("_MainTex", Blob);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Mode", 1f);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
        mat.SetFloat("_A", 0f);
        return mat;
    }

    // Yumusak leke dokusu: ortasi dolu, kenari daginik (gauss)
    public static Texture2D Blob
    {
        get
        {
            if (blob != null) return blob;
            const int n = 64;
            blob = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "IsikLekesi" };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                    float a = Mathf.Exp(-2.4f * (u * u + v * v)) * Mathf.Clamp01(1f - Mathf.Max(Mathf.Abs(u), Mathf.Abs(v)) * 1.02f);
                    px[y * n + x] = new Color(1f, 1f, 1f, a);
                }
            blob.SetPixels(px);
            blob.Apply();
            return blob;
        }
    }
}
