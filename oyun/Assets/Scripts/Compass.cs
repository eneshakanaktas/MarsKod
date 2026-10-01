using UnityEngine;
using UnityEngine.UIElements;

// Ekranin kosesinde duran kucuk pusula: North yukari (koloniye dogru), East saga, South asagi, West sola.
// move(North) vb. komutlarin hangi yone gittigini gosterir. Yari saydam; oyun alanini ortmez, dokunmaya tepki vermez.
public class Compass : VisualElement
{
    static readonly Color Disc = new Color(0.08f, 0.07f, 0.11f, 0.45f);
    static readonly Color Ring = new Color(1f, 1f, 1f, 0.14f);
    static readonly Color Arm = new Color(1f, 1f, 1f, 0.5f);
    static readonly Color Accent = Mats.Hex("#E07A5F");

    readonly float size;

    public Compass(float size, Font font)
    {
        this.size = size;
        style.width = size;
        style.height = size;
        pickingMode = PickingMode.Ignore;
        generateVisualContent += ctx => Draw(ctx.painter2D, contentRect);

        AddLetter("N", font, 0f, -1f, Accent);
        AddLetter("E", font, 1f, 0f, new Color(1f, 1f, 1f, 0.6f));
        AddLetter("S", font, 0f, 1f, new Color(1f, 1f, 1f, 0.6f));
        AddLetter("W", font, -1f, 0f, new Color(1f, 1f, 1f, 0.6f));
    }

    // Harfi dairenin icinde, verilen yonde kenara yakin yerlestirir (merkezden yon * 0.36 boy)
    void AddLetter(string text, Font font, float dx, float dy, Color color)
    {
        float fontSize = size * 0.2f;
        var l = Ui.Text(text, font, fontSize, color);
        l.style.position = Position.Absolute;
        l.style.width = fontSize * 1.4f;
        l.style.height = fontSize * 1.4f;
        l.style.unityTextAlign = TextAnchor.MiddleCenter;
        l.style.left = size * 0.5f + dx * size * 0.355f - fontSize * 0.7f;
        l.style.top = size * 0.5f + dy * size * 0.355f - fontSize * 0.7f;
        Add(l);
    }

    static Angle Deg(float d) => new Angle(d, AngleUnit.Degree);

    void Draw(Painter2D p, Rect r)
    {
        Vector2 c = new Vector2(r.width, r.height) * 0.5f;
        p.BeginPath();
        p.Arc(c, r.width * 0.5f, Deg(0), Deg(360));
        p.fillColor = Disc;
        p.Fill();
        p.BeginPath();
        p.Arc(c, r.width * 0.5f - 1f, Deg(0), Deg(360));
        p.strokeColor = Ring;
        p.lineWidth = 2f;
        p.Stroke();

        // dort uclu ince yildiz: kuzey kolu turuncu, digerleri soluk beyaz
        float R = r.width * 0.2f, k = r.width * 0.045f;
        Arm4(p, c, new Vector2(0, -R), new Vector2(k, -k), new Vector2(-k, -k), Accent);
        Arm4(p, c, new Vector2(R, 0), new Vector2(k, -k), new Vector2(k, k), Arm);
        Arm4(p, c, new Vector2(0, R), new Vector2(k, k), new Vector2(-k, k), Arm);
        Arm4(p, c, new Vector2(-R, 0), new Vector2(-k, k), new Vector2(-k, -k), Arm);
    }

    // Merkezden tip noktasina uzanan ince ucgen kol
    static void Arm4(Painter2D p, Vector2 c, Vector2 tip, Vector2 a, Vector2 b, Color color)
    {
        p.BeginPath();
        p.MoveTo(c + tip);
        p.LineTo(c + a);
        p.LineTo(c + b);
        p.ClosePath();
        p.fillColor = color;
        p.Fill();
    }
}
