using UnityEngine;

// Unity'nin parcacik sistemi (kar, kivilcim, toz) icin ortak kurulum. Taneler elle (Emit) atilir: sistem kendi kendine
// tane uretmez, yalniz atilanlari yasatir. Tanenin gorunusu MarsKod/Particle (yumusak yuvarlak nokta, resim dosyasi yok).
// Bitince Finish: yeni tane atilmaz, kalanlar omrunu doldurunca nesne kendini siler.
public static class ParticleFx
{
    const float AlphaBlend = 10f, AddBlend = 1f;   // UnityEngine.Rendering.BlendMode: OneMinusSrcAlpha / One

    /// <summary>core: tanenin dolu ortasi (0 = sis gibi yumusak, 0.6 = belirgin tane); gravity: taneyi asagi ceken pay (kivilcim icin);
    /// glow: tane isik gibi parlar (ustuste binen taneler daha da parlar; parilti, kivilcim)</summary>
    public static ParticleSystem Create(string name, Transform parent, int maxParticles, float core, float gravity = 0f, bool glow = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startSpeed = 0f;
        main.gravityModifier = gravity;
        main.stopAction = ParticleSystemStopAction.Destroy;
        var emission = ps.emission;
        emission.enabled = false;
        var shape = ps.shape;
        shape.enabled = false;

        // dogarken belirir, olurken solar (tane birden yok olmasin)
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;

        var mat = Mats.Custom("Particle", "MarsKod/Particle");
        mat.SetFloat("_Core", core);
        mat.SetFloat("_DstBlend", glow ? AddBlend : AlphaBlend);
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortMode = ParticleSystemSortMode.Distance;

        ps.Play();
        return ps;
    }

    /// <summary>Taneler havada yavasca salinir (kar tanesi, toz zerresi): Unity'nin gurultu modulu.
    /// strength: salinimin hizi (birim/sn), frequency: ne kadar sik yon degistirir</summary>
    public static void Sway(ParticleSystem ps, float strength, float frequency)
    {
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = strength;
        noise.frequency = frequency;
        noise.scrollSpeed = 0.2f;
        noise.damping = false;
        noise.quality = ParticleSystemNoiseQuality.Low;
    }

    /// <summary>Tane omru boyunca buyur/kuculur (0..1 arasi olcek egrisi; ornegin toz bulutu genisler)</summary>
    public static void SizeOverLife(ParticleSystem ps, float from, float to)
    {
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, from, 1f, to));
    }

    /// <summary>Tek tane atar</summary>
    public static void Emit(ParticleSystem ps, Vector3 position, Vector3 velocity, float size, float life, Color color, float rotation = 0f)
    {
        var p = new ParticleSystem.EmitParams
        {
            position = position, velocity = velocity, startSize = size, startLifetime = life, startColor = color, rotation = rotation,
        };
        ps.Emit(p, 1);
    }

    /// <summary>Yeni tane atilmaz; kalanlar solup bitince nesne kendini siler</summary>
    public static void Finish(ParticleSystem ps)
    {
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
