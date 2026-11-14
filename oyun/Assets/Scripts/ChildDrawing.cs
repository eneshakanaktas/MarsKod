using UnityEngine;

// Ece'nin cizimleri, mum boya gibi, yer yer silinmis cizgilerle kodla kucuk bir dokuya cizilir:
// - Robotun gogsundeki silik cizim (anlami Bolum 200'de cikar): bir cicek ve yaninda el ele bir kiz ile kucuk bir robot.
//   Cicek (70'teki duvar resmi) ve kiz + robot (75'teki duvar cizimi) asagidaki ortak olculerden cizilir.
// - Alet cantasinin kapagina bantli cizim (Bolum 44, senaryo-bolge-05.md): anten ve el sallayan iki kisi, biri buyuk biri kucuk.
public static class ChildDrawing
{
    const int W = 128, H = 96;

    static readonly Color Pink = new Color(0.93f, 0.45f, 0.58f);
    static readonly Color Green = new Color(0.36f, 0.66f, 0.36f);
    static readonly Color Yellow = new Color(0.98f, 0.80f, 0.25f);
    static readonly Color Blue = new Color(0.33f, 0.52f, 0.85f);
    static readonly Color Brown = new Color(0.45f, 0.33f, 0.28f);

    // Ece'nin cicegi, dokunun pikseliyle. 70'teki duvar resmi (WallMural) ayni cicegi bu olculerden cizer.
    public const float FlowerX = 30f, FlowerY = 60f, StemBottom = 12f, StemTop = 52f;
    public const int Petals = 5;
    public const float PetalDistance = 9f, PetalRadius = 5.5f, CenterRadius = 4f;
    public static readonly Vector4[] Leaves = { new Vector4(30f, 28f, 20f, 36f), new Vector4(30f, 34f, 40f, 40f) };   // (x0, y0, x1, y1)
    public static float PetalAngle(int k) => k * Mathf.PI * 2f / Petals + 0.3f;

    // El ele kiz ve kucuk robot, dokunun pikseliyle. 75'teki duvar cizimi (WallDrawings) ayni olculerden cizer.
    // Cizgiler (x0, y0, x1, y1), daireler (x, y, r), dortgenler (x0, y0, x1, y1); sirasi dokudaki cizim sirasi (degismesin).
    public static readonly Vector3 GirlHead = new Vector3(72f, 70f, 7f);
    public static readonly Vector4[] GirlSkirt = { new Vector4(72f, 63f, 63f, 38f), new Vector4(72f, 63f, 81f, 38f), new Vector4(63f, 38f, 81f, 38f) };
    // bacaklar, kiza dogru uzanmayan kol, iki orgu
    public static readonly Vector4[] GirlLimbs = { new Vector4(68f, 38f, 68f, 22f), new Vector4(76f, 38f, 76f, 22f), new Vector4(66f, 55f, 58f, 46f),
        new Vector4(65f, 74f, 61f, 64f), new Vector4(79f, 74f, 83f, 64f) };
    public static readonly Vector4 RobotHead = new Vector4(98f, 54f, 112f, 66f), RobotBody = new Vector4(96f, 30f, 114f, 52f);
    public static readonly Vector4 RobotAntenna = new Vector4(105f, 66f, 105f, 74f);
    public static readonly Vector3 AntennaTip = new Vector3(105f, 76f, 2f);
    public static readonly Vector2[] RobotEyes = { new Vector2(102f, 60f), new Vector2(108f, 60f) };
    public static readonly Vector4[] RobotLegs = { new Vector4(98f, 22f, 98f, 30f), new Vector4(112f, 22f, 112f, 30f) };
    public static readonly Vector4 HoldingHands = new Vector4(78f, 55f, 96f, 46f);   // kizin eli robotun elinde

    // background: gogus plakasinin rengi (cizim onun ustune solgun karisir)
    public static Texture2D Create(Color background)
    {
        var rng = new System.Random(5);
        var px = Paper(background, rng);

        // cicek (solda): sap, iki yaprak, bes tac yaprak, sari orta
        Line(px, rng, FlowerX, StemBottom, FlowerX, StemTop, Green);
        foreach (var leaf in Leaves) Line(px, rng, leaf.x, leaf.y, leaf.z, leaf.w, Green);
        for (int k = 0; k < Petals; k++)
        {
            float a = PetalAngle(k);
            Circle(px, rng, FlowerX + Mathf.Cos(a) * PetalDistance, FlowerY + Mathf.Sin(a) * PetalDistance, PetalRadius, Pink);
        }
        Circle(px, rng, FlowerX, FlowerY, CenterRadius, Yellow);

        // kiz (ortada): bas, uc gen etek, kollar, bacaklar, iki orgu
        Circle(px, rng, GirlHead, Brown);
        foreach (var l in GirlSkirt) Line(px, rng, l, Pink);
        foreach (var l in GirlLimbs) Line(px, rng, l, Brown);

        // kucuk robot (sagda): kare bas, kare govde, anten, kiza uzanan kol (el ele)
        Rect(px, rng, RobotHead, Blue);
        Rect(px, rng, RobotBody, Blue);
        Line(px, rng, RobotAntenna, Blue); Circle(px, rng, AntennaTip, Yellow);
        foreach (var e in RobotEyes) Dot(px, rng, e.x, e.y, Blue);
        foreach (var l in RobotLegs) Line(px, rng, l, Blue);
        Line(px, rng, HoldingHands, Brown);

        return ToTexture(px);
    }

    // Alet cantasinin kapagindaki cizim: solda anten (cubuk, iki ayak, yukari bakan canak), sagda el ele iki copten adam;
    // buyuk olan (anne) bir elini sallar, kucuk olan (kiz, Ece'nin turkuazi) obur elini. Kosede gunes.
    public static Texture2D CreateAntenna(Color paper)
    {
        var rng = new System.Random(11);
        var px = Paper(paper, rng);
        var teal = CraterTraces.EceColor;

        Line(px, rng, 4, 10, 124, 12, Green);   // yer
        // anten
        Line(px, rng, 28, 12, 28, 62, Blue);
        Line(px, rng, 28, 36, 16, 12, Blue); Line(px, rng, 28, 36, 40, 12, Blue);
        for (int k = 0; k <= 8; k++)
        {
            float a = Mathf.PI * (1.05f + k * 0.9f / 8f);   // asagi kavisli canak
            Dot(px, rng, 28 + Mathf.Cos(a) * 14f, 74 + Mathf.Sin(a) * 9f, Blue);
        }
        Line(px, rng, 14, 74, 42, 74, Blue);
        Line(px, rng, 28, 66, 28, 80, Blue); Circle(px, rng, 28, 82, 2f, Yellow);

        // anne: bas, govde, bacaklar; sol kol sallanir, sag kol kizin elinde
        Circle(px, rng, 70, 66, 7f, Brown);
        Line(px, rng, 70, 59, 70, 34, Brown);
        Line(px, rng, 70, 34, 62, 12, Brown); Line(px, rng, 70, 34, 78, 12, Brown);
        Line(px, rng, 70, 52, 57, 68, Brown); Line(px, rng, 57, 68, 54, 74, Brown);
        Line(px, rng, 70, 50, 86, 40, Brown);

        // kiz: kucuk bas, iki orgu, ucgen etek; sol kol annenin elinde, sag kol sallanir
        Circle(px, rng, 96, 46, 5.5f, teal);
        Line(px, rng, 91, 47, 88, 38, teal); Line(px, rng, 101, 47, 104, 38, teal);
        Line(px, rng, 96, 40, 90, 24, teal); Line(px, rng, 96, 40, 102, 24, teal); Line(px, rng, 90, 24, 102, 24, teal);
        Line(px, rng, 93, 24, 93, 12, teal); Line(px, rng, 99, 24, 99, 12, teal);
        Line(px, rng, 94, 34, 86, 40, teal);
        Line(px, rng, 98, 34, 110, 48, teal); Line(px, rng, 110, 48, 112, 54, teal);

        Circle(px, rng, 114, 84, 6f, Yellow);
        return ToTexture(px);
    }

    // Kagit: zeminin rengi, hafif dokulu
    static Color[] Paper(Color background, System.Random rng)
    {
        var px = new Color[W * H];
        for (int i = 0; i < px.Length; i++) px[i] = background * (0.97f + 0.03f * (float)rng.NextDouble());
        return px;
    }

    static Texture2D ToTexture(Color[] px)
    {
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // Mum boya izi: kalin, kenarlari yumusak, yer yer bosluklu, zemine solgun karisan nokta
    static void Dot(Color[] px, System.Random rng, float cx, float cy, Color c)
    {
        const float r = 1.8f;
        for (int y = (int)(cy - r - 1); y <= (int)(cy + r + 1); y++)
        for (int x = (int)(cx - r - 1); x <= (int)(cx + r + 1); x++)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) continue;
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            float a = Mathf.Clamp01(r - d + 0.5f);
            if (rng.NextDouble() < 0.3) a *= 0.3f;   // silinmis yerler
            int i = y * W + x;
            px[i] = Color.Lerp(px[i], c, a * 0.62f);
        }
    }

    static void Line(Color[] px, System.Random rng, float x0, float y0, float x1, float y1, Color c)
    {
        int steps = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)));
        for (int s = 0; s <= steps; s++)
        {
            float t = steps == 0 ? 0f : s / (float)steps;
            Dot(px, rng, Mathf.Lerp(x0, x1, t), Mathf.Lerp(y0, y1, t), c);
        }
    }

    static void Circle(Color[] px, System.Random rng, float cx, float cy, float r, Color c)
    {
        int steps = Mathf.CeilToInt(r * 7f);
        for (int s = 0; s < steps; s++)
        {
            float a = s * Mathf.PI * 2f / steps;
            Dot(px, rng, cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r, c);
        }
    }

    static void Line(Color[] px, System.Random rng, Vector4 l, Color c) => Line(px, rng, l.x, l.y, l.z, l.w, c);
    static void Circle(Color[] px, System.Random rng, Vector3 circle, Color c) => Circle(px, rng, circle.x, circle.y, circle.z, c);
    static void Rect(Color[] px, System.Random rng, Vector4 r, Color c) => Rect(px, rng, r.x, r.y, r.z, r.w, c);

    static void Rect(Color[] px, System.Random rng, float x0, float y0, float x1, float y1, Color c)
    {
        Line(px, rng, x0, y0, x1, y0, c); Line(px, rng, x1, y0, x1, y1, c);
        Line(px, rng, x1, y1, x0, y1, c); Line(px, rng, x0, y1, x0, y0, c);
    }
}
