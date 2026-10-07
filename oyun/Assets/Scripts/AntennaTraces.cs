using UnityEngine;

// Bolge 5 (anten tepesi, Bolum 41-50) hikaye izleri: docs/tasarim/senaryo-bolge-05.md.
// Hepsi alanin arkasinda (kuzey kenarin otesinde, sol arkada) durur; bulmacayi etkilemez.
// Dev anten her bolum biraz daha kalkar (bolum bitince bir sonraki asamaya gecer); 43'ten sonra yakinda,
// dibinde "D. ARAS" yazili alet cantasi (44'ten sonra kapagi acik, icinde cocuk cizimi).
public static class AntennaTraces
{
    public const int FirstLevel = 41, NearLevel = 43, CaseOpenLevel = 44, LockedNoteLevel = 45, LastLevel = 50;

    // Bolge 5 acilisi: kara ekranda iki satir
    public static readonly string[] TransitionLines = { "Yeni bölge: Anten tepesi", "Hedef: anteni kur" };

    // Bolum 45 bitince, ödev listesinde Bolum 50'nin kilitli kartinda "???" yerine
    public const string LockedNote = "KİLİTLİ — gönderen: D. Aras";
    public static bool ShowsLockedNote(int levelNumber, bool level45Solved) => levelNumber == LastLevel && level45Solved;

    // Bolum basinda antenin durusu: onceki bolumlerde kurulanlar (46 ayaklar, 47 canak, 48 kablolar, 49 canak gokyuzune)
    public static AntennaStage StageAtStart(int levelNumber) => StageAfter(levelNumber - 1);

    // Bolum bitince anten bu asamaya gecer (bitis metniyle ayni anda: "Antenin ayaklari dogrultuldu." gibi)
    public static AntennaStage StageAfter(int levelNumber) =>
        levelNumber >= 49 ? AntennaStage.Open
        : levelNumber == 48 ? AntennaStage.Wired
        : levelNumber == 47 ? AntennaStage.Upright
        : levelNumber == 46 ? AntennaStage.Leaning
        : AntennaStage.Fallen;

    // Bolumun izlerini kurar; Bolge 5 disinda null
    public static BigAntenna Build(int levelNumber, Transform parent, Vector2 areaHalf)
    {
        if (levelNumber < FirstLevel || levelNumber > LastLevel) return null;
        // 41-42: uzakta, tepenin ustunde (kucuk gorunur); 43'ten sonra alanin hemen arkasinda, dev.
        // Dik hali ust baslikla cakisir; soldaki dugmelerin altina girmesin diye biraz iceride. Canta sol arka kosede:
        // Bolum 50 sonunda robot o koseye (antenin dibine) gecer.
        bool near = levelNumber >= NearLevel;
        var at = near ? new Vector3(-1.8f, 0f, areaHalf.y + 0.6f)
            : levelNumber == FirstLevel ? new Vector3(-1.5f, 0f, areaHalf.y + 1.45f) : new Vector3(-1.9f, 0f, areaHalf.y + 1.15f);
        float scale = near ? 1.35f : levelNumber == FirstLevel ? 0.75f : 1f;
        var antenna = BigAntenna.Create(parent, at, scale, StageAtStart(levelNumber));
        if (near) ToolCase(parent, new Vector3(-2.9f, 0f, areaHalf.y + 0.4f), open: levelNumber >= CaseOpenLevel);
        return antenna;
    }

    // Alet cantasi: yipranmis metal canta, kapaginda beyaz serit etiket "D. ARAS".
    // Acikken kapak arkaya kalkik; kapagin icine bantli cocuk cizimi (anten ve el sallayan iki kisi) kameraya bakar.
    static void ToolCase(Transform parent, Vector3 p, bool open)
    {
        var root = Parts.Empty("Iz-AletCantasi", parent);
        root.localPosition = p;
        root.localRotation = Quaternion.Euler(0f, 16f, 0f);
        root.localScale = Vector3.one * 1.25f;

        var shell = Mats.Lit(Mats.Hex("#7A3F34"), 0.3f);   // solmus kirmizi
        var dark = Mats.Lit(Mats.Hex("#34313A"), 0.3f);
        Parts.Add("Govde", root, MeshFactory.RoundedBox(new Vector3(0.36f, 0.14f, 0.2f), 0.025f), shell, new Vector3(0f, 0.07f, 0f));
        Parts.Add("Kilit", root, MeshFactory.RoundedBox(new Vector3(0.05f, 0.03f, 0.012f), 0.006f), dark, new Vector3(0f, 0.12f, -0.104f), outline: false);

        // kapak: menteşesi arka ust kenarda
        var lid = Parts.Empty("Kapak", root);
        lid.localPosition = new Vector3(0f, 0.145f, 0.1f);
        lid.localRotation = Quaternion.Euler(open ? 112f : 0f, 0f, 0f);
        Parts.Add("Kapak", lid, MeshFactory.RoundedBox(new Vector3(0.36f, 0.03f, 0.2f), 0.012f), shell, new Vector3(0f, 0f, -0.1f));
        Parts.Add("Sap", lid, MeshFactory.RoundedBox(new Vector3(0.14f, 0.025f, 0.025f), 0.01f), dark, new Vector3(0f, 0.025f, -0.1f), outline: false);

        // etiket: kapagin ust yuzunde (yukaridan bakan kamera okur); kapak acilinca arkada kalir, icindeki cizim gorunur
        var label = Parts.Empty("Etiket", lid);
        label.localPosition = new Vector3(0f, 0.016f, -0.13f);
        Parts.Add("Serit", label, MeshFactory.Quad(new Vector2(0.26f, 0.065f)), Mats.Lit(Mats.Hex("#ECE6D8"), 0.2f), Vector3.zero, outline: false, castShadow: false);
        WorldText.Create(label, "D. ARAS", new Vector3(0f, 0.002f, 0f), 0.05f, Mats.Hex("#2A2730")).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (!open) return;
        // cizim: kapagin ic yuzune bantli kagit; kapak acikken ic yuz kameraya doner
        var paper = Mats.Emissive(Color.white, Color.white * 0.12f, 0.15f);
        var tex = ChildDrawing.CreateAntenna(Mats.Hex("#F4F0E6"));
        paper.SetTexture("_BaseMap", tex);
        paper.SetTexture("_EmissionMap", tex);
        // Quad yukari bakar; ters cevrilince ic yuze bakar, ustu (v) kapagin on kenarina, yani acikken yukariya doner
        var drawing = Parts.Add("Cizim", lid, MeshFactory.Quad(new Vector2(0.3f, 0.18f)), paper, new Vector3(0f, -0.017f, -0.1f), outline: false, castShadow: false);
        drawing.localRotation = Quaternion.Euler(180f, 0f, 0f);
        var tape = Mats.Lit(Mats.Hex("#E8DDB0"), 0.4f);
        foreach (float x in new[] { -0.12f, 0.12f })
            Parts.Add("Bant", lid, MeshFactory.RoundedBox(new Vector3(0.05f, 0.004f, 0.025f), 0.002f), tape, new Vector3(x, -0.019f, -0.185f), outline: false, castShadow: false);
    }
}
