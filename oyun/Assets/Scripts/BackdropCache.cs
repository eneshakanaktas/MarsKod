using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Arka plan (gokyuzu, tepeler, koloni) cok pahali bir cizim: her karede ekranin her pikseli icin yeniden hesaplanirsa orta
// seviye telefonun ekran karti yetisemiyor (Oppo A74'te oyun siyah ekranda kaldi). Bu yuzden bir dokuya cizilip saklanir
// (Backdrop.shader "Bake"); ekrandaki arka plan ve ufka karisan zemin bu dokuyu okur (MarsSky.hlsl, MarsBackdropCached).
// Doku yalnizca gerektiginde yenilenir:
//  - cizimin dayandigi degerlerden biri degisince (kamera fotografi, bolge, yildizlar, izler, lambalar) ve ardindan kisa bir
//    sure (lamba parlamasi, sera isiginin titremesi bu surede oynar);
//  - animasyonlar aciksa saniyede animatedRate kez (yildiz parlamasi, dronlar, yanip sonen isiklar, toz).
// Animasyonlar kapaliyken suregiden hareketler durur, arka plan neredeyse hic cizilmez.
[DefaultExecutionOrder(1000)]   // Oyun.LateUpdate kamerayi ve degerleri ayarladiktan sonra
public class BackdropCache : MonoBehaviour
{
    const float SettleSeconds = 2.5f;

    static readonly int TexId = Shader.PropertyToID("_BackdropTex");
    static readonly int ScreenId = Shader.PropertyToID("_SkyScreen");
    static readonly int SkyTimeId = Shader.PropertyToID("_SkyTime");
    static readonly int NowId = Shader.PropertyToID("_SkyNow");

    Material material;
    int bakePass;
    Mesh quad;
    float scale;
    bool flat;
    Color flatColor;
    RenderTexture target;
    CommandBuffer cmd;
    readonly BackdropInputs inputs = new BackdropInputs();
    float skyTime, settleUntil, nextAnimatedBake;
    float animatedRate = 20f;   // animasyon acikken saniyede kac kez yeniden cizilir (GraphicsQuality.BackdropRate)
    bool animated = true;

    // scale: dokunun ekrana gore boyu (0,5 = yari genislik, yari yukseklik). flat: arka plani hic cizmez, duz renk (deneme).
    public static BackdropCache Create(Material backdrop, Mesh quad, float scale, bool flat)
    {
        var cache = new GameObject("BackdropCache").AddComponent<BackdropCache>();
        cache.material = backdrop;
        cache.bakePass = backdrop.FindPass("Bake");
        cache.quad = quad;
        cache.scale = Mathf.Clamp(scale, 0.1f, 1f);
        cache.flat = flat;
        cache.flatColor = new Color(0.176f, 0.110f, 0.106f);   // ufuk dibindeki ovanin rengi (MarsSky.hlsl, MarsDustHaze)
        cache.cmd = new CommandBuffer { name = "Arka plan" };
        return cache;
    }

    public bool Animated
    {
        get => animated;
        set
        {
            animated = value;
            settleUntil = 0f;   // acilinca hemen yenilensin, kapaninca son hali kalsin
            nextAnimatedBake = 0f;
        }
    }

    // Grafik ayari degisince: dokunun boyu (ekrana gore) ve animasyonda saniyede kac kez yenilenecegi
    public void SetQuality(float newScale, float rate)
    {
        scale = Mathf.Clamp(newScale, 0.1f, 1f);
        animatedRate = rate;
        settleUntil = 0f;   // EnsureTarget yeni boyu gorur, doku hemen yeniden cizilir
    }

    void LateUpdate()
    {
        bool resized = EnsureTarget();
        float now = Time.timeSinceLevelLoad;
        if (animated) skyTime = now;

        bool changed = inputs.Changed() || resized;
        if (changed) settleUntil = now + SettleSeconds;
        bool due = changed || now < settleUntil || (animated && now >= nextAnimatedBake);
        if (!due) return;
        nextAnimatedBake = now + 1f / animatedRate;
        Bake(now);
    }

    // Ekran boyu degistiyse dokuyu yeniden kurar; kurduysa true
    bool EnsureTarget()
    {
        int w = Mathf.Max(1, Mathf.RoundToInt(Screen.width * scale));
        int h = Mathf.Max(1, Mathf.RoundToInt(Screen.height * scale));
        if (target != null && target.width == w && target.height == h) return false;
        if (target != null)
        {
            target.Release();
            Destroy(target);
        }
        target = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        {
            name = "Arka plan",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
        };
        target.Create();
        Shader.SetGlobalTexture(TexId, target);
        return true;
    }

    void Bake(float now)
    {
        cmd.Clear();
        cmd.SetRenderTarget(target);
        if (flat)
        {
            cmd.ClearRenderTarget(false, true, flatColor);
        }
        else
        {
            cmd.SetGlobalVector(ScreenId, new Vector4(target.width, target.height, 0f, 0f));
            cmd.SetGlobalFloat(SkyTimeId, skyTime);
            cmd.SetGlobalFloat(NowId, now);
            cmd.DrawMesh(quad, Matrix4x4.identity, material, 0, bakePass);
        }
        Graphics.ExecuteCommandBuffer(cmd);
    }

    void OnDestroy()
    {
        if (target != null)
        {
            target.Release();
            Destroy(target);
        }
        cmd?.Release();
    }

    // Arka plan cizimin C#'tan okudugu degerler (MarsSky.hlsl, PolarSky.hlsl, CraterSky.hlsl, CanyonSky.hlsl). Yeni bir deger eklenirse buraya da eklenir.
    class BackdropInputs
    {
        static readonly int[] FloatIds =
        {
            Shader.PropertyToID("_StarsOn"), Shader.PropertyToID("_Region"), Shader.PropertyToID("_Horizon"),
            Shader.PropertyToID("_TraceDoor"), Shader.PropertyToID("_TraceGreenhouse"), Shader.PropertyToID("_TraceLeaf"),
            Shader.PropertyToID("_PolarRoverHere"), Shader.PropertyToID("_PowerLampCount"), Shader.PropertyToID("_Sunrise"),
        };
        static readonly int[] VectorIds =
        {
            Shader.PropertyToID("_Picture"), Shader.PropertyToID("_Focus"), Shader.PropertyToID("_Sun"),
            Shader.PropertyToID("_CanyonDescent"),
        };
        static readonly int LampsId = Shader.PropertyToID("_PowerLamps");

        readonly float[] floats = new float[FloatIds.Length];
        readonly Vector4[] vectors = new Vector4[VectorIds.Length];
        readonly List<Vector4> lamps = new List<Vector4>();
        readonly List<Vector4> lampsNow = new List<Vector4>();
        bool first = true;

        // Son bakistan beri bir deger degisti mi (ilk cagrida her zaman evet)
        public bool Changed()
        {
            bool changed = first;
            first = false;
            for (int i = 0; i < FloatIds.Length; i++)
            {
                float v = Shader.GetGlobalFloat(FloatIds[i]);
                if (v != floats[i]) { floats[i] = v; changed = true; }
            }
            for (int i = 0; i < VectorIds.Length; i++)
            {
                Vector4 v = Shader.GetGlobalVector(VectorIds[i]);
                if (v != vectors[i]) { vectors[i] = v; changed = true; }
            }
            Shader.GetGlobalVectorArray(LampsId, lampsNow);
            if (!SameLamps())
            {
                lamps.Clear();
                lamps.AddRange(lampsNow);
                changed = true;
            }
            return changed;
        }

        bool SameLamps()
        {
            if (lamps.Count != lampsNow.Count) return false;
            for (int i = 0; i < lamps.Count; i++)
                if (lamps[i] != lampsNow[i]) return false;
            return true;
        }
    }
}
