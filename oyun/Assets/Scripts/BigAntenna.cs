using System.Collections;
using UnityEngine;

// Anten tepesindeki dev anten (Bolge 5, senaryo-bolge-05.md). Bolum bolum ayaga kalkar; durusu asamadan gelir.
// Dibinde kurulum bilgisayari ve onun durum isigi: report() dogru sayiyi gonderince bir kez yesil yanar,
// Bolum 50'de anten tam guce gecince yesil kalir. Govdede uc lamba: kablolar baglaninca sirayla yanip soner (guc yok),
// tam guce gecince hep yanar. Bulmacayi etkilemez; alanin arkasinda durur.
public enum AntennaStage { Fallen, Leaning, Upright, Wired, Open, Powered }

public class BigAntenna : MonoBehaviour
{
    const float MastHeight = 1.6f;
    static readonly Color StatusIdle = Mats.Hex("#5A1E1A");
    static readonly Color StatusGreen = Mats.Hex("#59F08A");
    static readonly Color LampColor = Mats.Hex("#FFB547");

    // Asamaya gore direk egimi (z ekseni, derece; eksi = +x'e devrik) ve canagin durusu
    static float MastTilt(AntennaStage s) => s == AntennaStage.Fallen ? -84f : s == AntennaStage.Leaning ? -38f : 0f;
    static Vector3 DishAngles(AntennaStage s) =>
        s == AntennaStage.Fallen ? new Vector3(0f, -75f, 12f)      // yuzu kuma donuk
        : s == AntennaStage.Leaning ? new Vector3(0f, -60f, 0f)    // hala yan
        : s >= AntennaStage.Open ? new Vector3(118f, 0f, 0f)       // gokyuzune (ufkun ustune, Dunya'ya)
        : Vector3.zero;                                            // yerine oturmus, kameraya bakar

    Transform mast, dish, cable;
    Material statusMat, feedMat;
    readonly Material[] lampMats = new Material[3];
    AntennaStage startStage, stage;
    float ackUntil;      // durum isiginin yesil yanacagi son an (bir kez yanis)
    float signal;        // 0-1: selam yanip sonmesi (isik bir an parlar)
    int litLamps;        // tam gucte yanan lamba sayisi (alttan uste birer birer yanar)

    public static BigAntenna Create(Transform parent, Vector3 localPos, float scale, AntennaStage stage)
    {
        var go = new GameObject("Iz-Anten");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(0f, 14f, 0f);   // hafif yan: kafes ve canak derinlikli gorunsun
        go.transform.localScale = Vector3.one * scale;
        var antenna = go.AddComponent<BigAntenna>();
        antenna.Build();
        antenna.startStage = stage;
        antenna.Apply(stage);
        return antenna;
    }

    public AntennaStage Stage => stage;
    // Dibi (dunya konumu): robot finalde buraya bakar
    public Vector3 Foot => transform.position;

    void Build()
    {
        var metal = Mats.Lit(Mats.Hex("#A9ADB5"), 0.45f);
        var dark = Mats.Lit(Mats.Hex("#3A3D44"), 0.35f);
        var concrete = Mats.Lit(Mats.Hex("#8C8279"), 0.15f);

        Parts.Add("Kaide", transform, MeshFactory.RoundedBox(new Vector3(0.5f, 0.08f, 0.5f), 0.02f), concrete, new Vector3(0f, 0.04f, 0f));
        BuildConsole(dark);

        mast = Parts.Empty("Direk", transform);
        mast.localPosition = new Vector3(0f, 0.08f, 0f);
        // kafes: iki dikme, aralarinda capraz cubuklar; altta iki egik ayak
        foreach (float x in new[] { -0.07f, 0.07f })
            Parts.Add("Dikme", mast, MeshFactory.RoundedBox(new Vector3(0.035f, MastHeight, 0.035f), 0.01f), metal, new Vector3(x, MastHeight * 0.5f, 0f));
        for (float y = 0.12f; y < MastHeight - 0.05f; y += 0.17f)
            Parts.Add("Capraz", mast, MeshFactory.RoundedBox(new Vector3(0.16f, 0.018f, 0.018f), 0.005f), metal, new Vector3(0f, y, 0f), outline: false)
                .localRotation = Quaternion.Euler(0f, 0f, 35f);
        foreach (float side in new[] { -1f, 1f })
        {
            var leg = Parts.Add("Ayak", mast, MeshFactory.RoundedBox(new Vector3(0.03f, 0.62f, 0.03f), 0.01f), metal, new Vector3(side * 0.2f, 0.27f, 0f));
            leg.localRotation = Quaternion.Euler(0f, 0f, side * 24f);
        }
        for (int k = 0; k < lampMats.Length; k++)
        {
            lampMats[k] = Mats.Emissive(LampColor, Color.black, 0.5f);
            Parts.Add("Lamba", mast, MeshFactory.Sphere(0.035f, 8, 12), lampMats[k], new Vector3(0f, 0.45f + k * 0.42f, -0.04f), outline: false, castShadow: false);
        }

        // canak: direk ucunda; yuzu (ici) -z'ye, kameraya bakar
        dish = Parts.Empty("Canak", mast);
        dish.localPosition = new Vector3(0f, MastHeight + 0.05f, 0f);
        Parts.Add("Mafsal", dish, MeshFactory.Sphere(0.06f, 8, 12), dark, Vector3.zero);
        var bowl = Parts.Empty("Kase", dish);
        bowl.localPosition = new Vector3(0f, 0.12f, -0.06f);
        bowl.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        // ince kabuk: alt yuz disa, ust yuz (ici) yukari; bowl'un +y'si -z'ye (kameraya) doner
        var shell = new[]
        {
            new Vector2(0f, -0.03f), new Vector2(0.2f, -0.01f), new Vector2(0.38f, 0.05f), new Vector2(0.45f, 0.1f),
            new Vector2(0.44f, 0.12f), new Vector2(0.38f, 0.08f), new Vector2(0.2f, 0.02f), new Vector2(0f, 0f),
        };
        Parts.Add("Kase", bowl, MeshFactory.Lathe(shell, 28), Mats.Lit(Mats.Hex("#E2E0D8"), 0.35f), Vector3.zero);
        // kirik kenar: kasenin bir yaninda eksik parca yerine koyu bir yama (Bolum 42: "kirik kenari seciliyor")
        Parts.Add("Yama", bowl, MeshFactory.RoundedBox(new Vector3(0.12f, 0.03f, 0.1f), 0.01f), dark, new Vector3(0.36f, 0.1f, 0.05f), outline: false);
        var feedAt = new Vector3(0f, 0.4f, 0f);
        foreach (float a in new[] { 0f, 120f, 240f })
            Rod("Kol", bowl, Quaternion.Euler(0f, a, 0f) * new Vector3(0.42f, 0.12f, 0f), feedAt, 0.008f, metal);
        feedMat = Mats.Emissive(Mats.Hex("#5A5E66"), Color.black, 0.5f);
        Parts.Add("Alici", bowl, MeshFactory.RoundedCylinder(0.035f, 0.07f, 0.01f, 12), feedMat, new Vector3(0f, 0.4f, 0f), outline: false);

        BuildCable();
    }

    // Kurulum bilgisayari: kaidenin onunde kucuk bir kasa, ustunde durum isigi, onunde sonuk ekran.
    void BuildConsole(Material dark)
    {
        var box = Parts.Empty("KurulumBilgisayari", transform);
        box.localPosition = new Vector3(-0.42f, 0f, -0.2f);
        box.localRotation = Quaternion.Euler(0f, -20f, 0f);
        Parts.Add("Kasa", box, MeshFactory.RoundedBox(new Vector3(0.22f, 0.24f, 0.14f), 0.02f), Mats.Lit(Mats.Hex("#C9C2B0"), 0.3f), new Vector3(0f, 0.12f, 0f));
        Parts.Add("Serit", box, MeshFactory.RoundedBox(new Vector3(0.225f, 0.03f, 0.145f), 0.008f), Mats.Lit(Mats.Hex("#E07B3C"), 0.3f), new Vector3(0f, 0.05f, 0f), outline: false);
        Parts.Add("Ekran", box, MeshFactory.RoundedBox(new Vector3(0.14f, 0.08f, 0.01f), 0.01f), dark, new Vector3(0f, 0.16f, -0.072f), outline: false);
        statusMat = Mats.Emissive(StatusIdle, Color.black, 0.6f);
        Parts.Add("DurumIsigi", box, MeshFactory.Sphere(0.045f, 8, 12), statusMat, new Vector3(0f, 0.28f, 0f), outline: false, castShadow: false);
    }

    // Ana kablo: kaideden alanin kenarina (onde, sagda) kuma serilmis kalin kablo; kablolar baglaninca gorunur.
    // Ucu alanin kenarinda biter (anten 1,35 boyla alanin 0,6 gerisinde; daha uzun olursa karelerin ustune tasar).
    void BuildCable()
    {
        cable = Parts.Empty("AnaKablo", transform);
        var mat = Mats.Lit(Mats.Hex("#C9652A"), 0.3f);
        var points = new[] { new Vector3(0.12f, 0.02f, -0.15f), new Vector3(0.35f, 0.02f, -0.27f), new Vector3(0.56f, 0.02f, -0.3f), new Vector3(0.82f, 0.02f, -0.36f) };
        for (int k = 0; k + 1 < points.Length; k++)
            Rod("Parca", cable, points[k], points[k + 1], 0.03f, mat, overlap: 0.05f);
    }

    // a'dan b'ye uzanan yuvarlak cubuk (overlap: parcalar birlesim yerinde bosluk birakmasin)
    static void Rod(string name, Transform parent, Vector3 a, Vector3 b, float radius, Material mat, float overlap = 0f)
    {
        var rod = Parts.Add(name, parent, MeshFactory.RoundedCylinder(radius, Vector3.Distance(a, b) + overlap, radius * 0.4f, 10), mat, (a + b) * 0.5f, outline: false);
        rod.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
    }

    // Asamanin durusunu hemen kurar (bolum basi, yeniden deneme)
    void Apply(AntennaStage s)
    {
        stage = s;
        mast.localRotation = Quaternion.Euler(0f, 0f, MastTilt(s));
        dish.localRotation = Quaternion.Euler(DishAngles(s));
        cable.gameObject.SetActive(s >= AntennaStage.Wired);
        litLamps = s == AntennaStage.Powered ? lampMats.Length : 0;
    }

    // Bolum bitti: anten o bolumun asamasina kalkar (zaten oradaysa bir sey olmaz)
    public void Advance(AntennaStage s)
    {
        if (s != stage) StartCoroutine(RiseTo(s));
    }

    // Bir sonraki asamaya yavasca gecer
    IEnumerator RiseTo(AntennaStage s, float dur = 1.6f)
    {
        float fromTilt = MastTilt(stage), toTilt = MastTilt(s);
        var fromDish = dish.localRotation;
        var toDish = Quaternion.Euler(DishAngles(s));
        stage = s;
        if (s >= AntennaStage.Wired) cable.gameObject.SetActive(true);
        yield return Tween.Run(dur, t =>
        {
            float e = Tween.InOutCubic(t);
            mast.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(fromTilt, toTilt, e));
            dish.localRotation = Quaternion.Slerp(fromDish, toDish, e);
        });
    }

    // report() dogru sayiyi gonderdi: durum isigi bir kez yesil yanar
    public void Acknowledge() => ackUntil = Time.time + 1.4f;

    // Bolum 50: tam guc. Lambalar alttan uste birer birer yanar, alici parlar, durum isigi yesil kalir.
    public IEnumerator PowerUp()
    {
        stage = AntennaStage.Powered;
        litLamps = 0;
        for (int k = 0; k < lampMats.Length; k++)
        {
            litLamps = k + 1;
            Sound.Lamp();
            yield return Tween.Wait(0.28f);
        }
        yield return Tween.Run(0.6f, t => feedMat.SetColor("_EmissionColor", StatusGreen * 1.4f * t));
    }

    // Selam: isik iki kez parlayip soner (Kivilcim'in sinyaliyle ayni anda)
    public IEnumerator Greet(int times)
    {
        for (int i = 0; i < times; i++)
        {
            yield return Tween.Run(0.34f, t => signal = Mathf.Sin(t * Mathf.PI));
            signal = 0f;
            yield return Tween.Wait(0.18f);
        }
    }

    public void Restore()
    {
        StopAllCoroutines();
        ackUntil = 0f;
        signal = 0f;
        feedMat.SetColor("_EmissionColor", Color.black);
        Apply(startStage);
    }

    void Update()
    {
        bool powered = stage == AntennaStage.Powered;
        // durum isigi: tam gucte yesil; dogru raporda bir an yesil; yoksa sonuk kirmizi. Selamda bir an beyaza parlar.
        Color status = powered || Time.time < ackUntil ? StatusGreen * (1.6f + 1.4f * signal) : StatusIdle * 0.6f;
        statusMat.SetColor("_EmissionColor", status);
        statusMat.SetColor("_BaseColor", powered || Time.time < ackUntil ? StatusGreen : StatusIdle);

        for (int k = 0; k < lampMats.Length; k++)
        {
            float on;
            if (powered) on = k < litLamps ? 1f + signal : 0f;
            else if (stage >= AntennaStage.Wired) on = Mathf.Repeat(Time.time * 1.6f - k * 0.33f, 1f) < 0.33f ? 0.8f : 0f;   // sirayla: guc yok
            else on = 0f;
            lampMats[k].SetColor("_EmissionColor", LampColor * 1.6f * on);
        }
    }
}
