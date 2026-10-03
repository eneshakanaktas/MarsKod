using System.Collections.Generic;
using UnityEngine;

// Kisa oyun sesleri, kodla uretilir (ses dosyasi yok, paket buyumez): dugme, move, toplama, engele carpma,
// hata, bolum sonu kutlamasi, kolonide lamba yanmasi, acilis bipi, kapanis hisirtisi ve SOS.
// Robotun "dili" henuz yok (hikaye belirleyecek); bu sesler sade kalir.
// Ayarlar: Enabled, PlayerPrefs("ses") ile kalici; Hud'daki hoparlor dugmesiyle acilip kapanir.
public static class Sound
{
    public static bool Enabled { get; private set; } = true;

    const int SampleRate = 22050;
    const float SosToneSeconds = 0.12f;
    const float SosDashSeconds = 0.36f;
    const float SosGapSeconds = 0.1f;
    const float SosLetterGapSeconds = 0.3f;
    static AudioSource source;
    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static void Init(Transform parent)
    {
        Enabled = PlayerPrefs.GetInt("ses", 1) != 0;
        var go = new GameObject("Ses");
        go.transform.SetParent(parent, false);
        source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    public static void SetEnabled(bool on)
    {
        Enabled = on;
        PlayerPrefs.SetInt("ses", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    static void Play(AudioClip clip, float volume)
    {
        if (!Enabled || source == null || clip == null) return;
        source.PlayOneShot(clip, volume);
    }

    static AudioClip Get(string key, System.Func<AudioClip> make)
    {
        if (!cache.TryGetValue(key, out var clip)) cache[key] = clip = make();
        return clip;
    }

    // ---- efektler ----
    public static void Tap() => Play(Get("tap", () => Beep(980f, 0.035f)), 0.4f);
    public static void Move() => Play(Get("move", () => Beep(240f, 0.05f)), 0.4f);
    public static void Collect() => Play(Get("collect", () => Chime(new[] { 660f, 880f }, 0.065f)), 0.6f);
    public static void Bump() => Play(Get("bump", () => Buzz(130f, 0.1f)), 0.55f);
    public static void Error() => Play(Get("error", () => Chime(new[] { 440f, 310f }, 0.09f, square: true)), 0.55f);
    public static void Celebrate() => Play(Get("celebrate", () => Chime(new[] { 523f, 659f, 784f, 1046f }, 0.1f)), 0.7f);
    public static void Lamp() => Play(Get("lamp", () => Beep(1300f, 0.05f)), 0.35f);
    public static void Blip() => Play(Get("blip", () => Beep(520f, 0.12f)), 0.35f);
    public static void Static() => Play(Get("static", () => Hiss(1.2f)), 0.5f);
    public static void Sos() => Play(Get("sos", () => Morse("... --- ...", 880f)), 0.5f);

    // ---- uretim ----

    static AudioClip MakeClip(float[] samples, string name)
    {
        var clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Kisa, tikliksiz bir zarf: hizli giris, sonuna kadar duz azalan.
    static float Envelope(int i, int n, int attack)
    {
        float a = attack <= 0 ? 1f : Mathf.Clamp01(i / (float)attack);
        float d = Mathf.Clamp01(1f - i / (float)n);
        return a * d;
    }

    static int SampleCount(float seconds) => Mathf.Max(1, Mathf.RoundToInt(seconds * SampleRate));

    // Tek sinus tonu (zarfli); diger seslerin ortak parcasi.
    static float[] Tone(float freq, float seconds, float amplitude)
    {
        int n = SampleCount(seconds);
        int attack = Mathf.Min(n / 6, SampleRate / 200);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SampleRate;
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * Envelope(i, n, attack) * amplitude;
        }
        return data;
    }

    static AudioClip Beep(float freq, float seconds) => MakeClip(Tone(freq, seconds, 0.6f), "beep");

    // Kare dalga + hafif gurultu: carpma/hata icin "pat" hissi.
    static AudioClip Buzz(float freq, float seconds)
    {
        int n = SampleCount(seconds);
        int attack = Mathf.Min(n / 10, SampleRate / 300);
        var rnd = new System.Random(7);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)SampleRate;
            float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
            float noise = (float)(rnd.NextDouble() * 2f - 1f) * 0.3f;
            data[i] = (square * 0.7f + noise) * Envelope(i, n, attack) * 0.6f;
        }
        return MakeClip(data, "buzz");
    }

    // Art arda kisa notalar (toplama, kutlama, hata icin dusen/cikan ezgi).
    static AudioClip Chime(float[] freqs, float noteSeconds, bool square = false)
    {
        int noteN = SampleCount(noteSeconds);
        int attack = Mathf.Min(noteN / 6, SampleRate / 200);
        var data = new float[noteN * freqs.Length];
        for (int k = 0; k < freqs.Length; k++)
        {
            float freq = freqs[k];
            for (int i = 0; i < noteN; i++)
            {
                float t = i / (float)SampleRate;
                float s = square ? Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) : Mathf.Sin(2f * Mathf.PI * freq * t);
                data[k * noteN + i] = s * Envelope(i, noteN, attack) * 0.55f;
            }
        }
        return MakeClip(data, "chime");
    }

    // Morse benzeri dizi: '.' kisa bip, '-' uzun bip, ' ' harf arasi sessizlik.
    static AudioClip Morse(string pattern, float freq)
    {
        var data = new List<float>();
        foreach (char symbol in pattern)
        {
            if (symbol == ' ') data.AddRange(new float[SampleCount(SosLetterGapSeconds)]);
            else
            {
                float seconds = symbol == '-' ? SosDashSeconds : SosToneSeconds;
                data.AddRange(Tone(freq, seconds, 0.6f));
                data.AddRange(new float[SampleCount(SosGapSeconds)]);
            }
        }
        return MakeClip(data.ToArray(), "morse");
    }

    // Hisirti: yumusatilmis gurultu, sonra soner. Kapanis sahnesinde telsizin gelmesinden once calar.
    static AudioClip Hiss(float seconds)
    {
        int n = SampleCount(seconds);
        var rnd = new System.Random(11);
        var data = new float[n];
        float smooth = 0f;
        for (int i = 0; i < n; i++)
        {
            float noise = (float)(rnd.NextDouble() * 2f - 1f);
            smooth += 0.25f * (noise - smooth);
            data[i] = Mathf.Clamp(smooth * 2.5f, -1f, 1f) * Envelope(i, n, SampleRate / 10);
        }
        return MakeClip(data, "hiss");
    }
}
