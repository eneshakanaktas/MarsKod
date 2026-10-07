using System;
using UnityEngine;
using UnityEngine.UIElements;

// Bolum 50 kapanisi: Defne'nin kaydi (senaryo-bolge-05.md, metin senaryo-perde-1.md). Ekran kararir; ustte dosya adi
// ve ses dalgasi, ortada kaydin satirlari birer birer belirir (eskiler soluklasir). Dalga yalnizca bir satir
// "soylenirken" kipirdar. Gercek ses yok (Ragip'in karari: once yazi); ses eklenirse dalga ona baglanir.
// Ekrana dokununca bir sonraki satir hemen gelir; sahne atlanamaz.
public class RecordingScene : VisualElement
{
    const long FirstLineMs = 1600;
    const long BaseLineMs = 1500, PerCharMs = 48;   // satirin ekranda kalma suresi: okuma hizina gore
    const float SpeakSecondsPerChar = 0.055f;
    const int Bars = 34;

    static readonly Color Ink = new Color(1f, 1f, 1f, 0.92f);
    static readonly Color Faded = new Color(1f, 1f, 1f, 0.42f);
    static readonly Color Wave = Mats.Hex("#7FD8FF");

    readonly Label fileLabel;
    readonly VisualElement wave, linesBox;
    readonly Font font;
    string[] lines = new string[0];
    int shown;
    float speakUntil;
    IVisualElementScheduledItem nextLine, waveTick;

    public bool Finished { get; private set; }

    public RecordingScene(Font font, Font mono)
    {
        this.font = font;
        pickingMode = PickingMode.Position;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
        style.backgroundColor = Color.black;
        style.alignItems = Align.Center;
        style.paddingTop = Ui.SafeTop() + 260;
        style.paddingLeft = 90; style.paddingRight = 90;
        style.display = DisplayStyle.None;
        style.opacity = 0f;
        Ui.Transition(this, "opacity", 0.9f);

        fileLabel = Ui.Text("", mono, 30, new Color(1f, 1f, 1f, 0.5f));
        fileLabel.style.letterSpacing = 3;
        Add(fileLabel);

        wave = new VisualElement { pickingMode = PickingMode.Ignore };
        wave.style.width = 520; wave.style.height = 110;
        wave.style.marginTop = 26; wave.style.marginBottom = 90;
        wave.generateVisualContent += DrawWave;
        Add(wave);

        linesBox = new VisualElement { pickingMode = PickingMode.Ignore };
        linesBox.style.alignSelf = Align.Stretch;
        Add(linesBox);

        RegisterCallback<ClickEvent>(_ => { if (!Finished && shown < lines.Length) ShowLine(); });
    }

    public void Show(string file, string[] texts)
    {
        lines = texts;
        shown = 0;
        Finished = false;
        speakUntil = 0f;
        fileLabel.text = file;
        linesBox.Clear();
        style.display = DisplayStyle.Flex;
        schedule.Execute(() => style.opacity = 1f).StartingIn(20);
        waveTick?.Pause();
        waveTick = schedule.Execute(wave.MarkDirtyRepaint).Every(40);
        Schedule(FirstLineMs);
    }

    // Kararan ekran yavasca acilir, sonra katman kalkar
    public void Hide()
    {
        style.opacity = 0f;
        nextLine?.Pause();
        schedule.Execute(() =>
        {
            style.display = DisplayStyle.None;
            waveTick?.Pause();
        }).StartingIn(950);
    }

    void Schedule(long ms)
    {
        nextLine?.Pause();
        nextLine = schedule.Execute(ShowLine).StartingIn(ms);
    }

    void ShowLine()
    {
        if (shown >= lines.Length) return;
        foreach (var old in linesBox.Children()) ((Label)old).style.color = Faded;
        string text = lines[shown++];
        var label = Ui.Text(text, font, 44, Ink);
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityTextAlign = TextAnchor.UpperCenter;
        label.style.marginBottom = 26;
        label.style.opacity = 0f;
        Ui.Transition(label, "opacity", 0.6f);
        linesBox.Add(label);
        label.schedule.Execute(() => label.style.opacity = 1f).StartingIn(20);
        speakUntil = Time.time + text.Length * SpeakSecondsPerChar;

        long stay = BaseLineMs + PerCharMs * text.Length;
        if (shown < lines.Length) Schedule(stay);
        else
        {
            nextLine?.Pause();
            nextLine = schedule.Execute(() => Finished = true).StartingIn(stay);
        }
    }

    // Ses dalgasi: ortadan iki yana simetrik cubuklar; konusurken boylari titrer, susunca ince bir cizgi kalir
    void DrawWave(MeshGenerationContext ctx)
    {
        var r = wave.contentRect;
        var p = ctx.painter2D;
        float t = Time.time;
        bool speaking = t < speakUntil;
        float gap = r.width / Bars;
        p.lineWidth = gap * 0.5f;
        p.lineCap = LineCap.Round;
        p.strokeColor = new Color(Wave.r, Wave.g, Wave.b, speaking ? 0.95f : 0.45f);
        for (int i = 0; i < Bars; i++)
        {
            float centre = 1f - Mathf.Abs(i - (Bars - 1) * 0.5f) / (Bars * 0.5f);   // ortada yuksek
            float n = Mathf.PerlinNoise(i * 0.37f, t * 6f);
            float h = speaking ? Mathf.Lerp(0.12f, 1f, n) * (0.35f + 0.65f * centre) : 0.05f;
            float x = r.x + gap * (i + 0.5f);
            float half = Mathf.Max(h * r.height * 0.5f, p.lineWidth * 0.5f);
            p.BeginPath();
            p.MoveTo(new Vector2(x, r.center.y - half));
            p.LineTo(new Vector2(x, r.center.y + half));
            p.Stroke();
        }
    }
}
