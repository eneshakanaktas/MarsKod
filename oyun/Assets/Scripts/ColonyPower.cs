using System.Collections;
using UnityEngine;

// Koloninin guc lambalari (hikaye: enerji hucreleri toplandikca koloninin isiklari tek tek yanar).
// Ana koloninin onundeki ovada bir sira lamba durur: bolumdeki her hucre icin bir tane, basta sonuk.
// Hucre toplaninca ondan bir isik topu koloniye ucar; vardiginda siradaki lamba parlayarak yanar.
// Lambalari arka plan cizer (MarsSky.hlsl, powerLamps); burasi yalnizca konumlarini ve ne zaman yandiklarini verir.
public class ColonyPower : MonoBehaviour
{
    public const int MaxLamps = 12;   // MarsSky.hlsl: MAX_POWER_LAMPS ile ayni

    // Ana koloninin yeri (MarsSky.hlsl: COLONY_X; platform ufkun 0,047 altinda). Lambalar platformun hemen onunde.
    const float ColonyX = 0.045f;
    const float RowY = -0.051f;                 // ufka gore
    const float RowCenter = -0.085f, RowWidth = 0.23f, MaxSpacing = 0.021f;   // koloniye gore
    const float FlightTime = 0.8f;

    static readonly int LampsId = Shader.PropertyToID("_PowerLamps");
    static readonly int CountId = Shader.PropertyToID("_PowerLampCount");

    // Her lamba: x = fotograftaki yatay konum, y = ufka gore yukseklik, z = yandigi an (sonukse -1)
    readonly Vector4[] lamps = new Vector4[MaxLamps];
    int count, sent;
    Camera cam;
    Material sparkMat;
    Mesh sparkMesh;

    public static ColonyPower Create(Transform parent, Camera cam)
    {
        var power = Parts.Empty("ColonyPower", parent).gameObject.AddComponent<ColonyPower>();
        power.cam = cam;
        power.sparkMat = Mats.Emissive(EnergyCell.Glow, EnergyCell.Glow * 2.2f, 0.9f);
        power.sparkMesh = MeshFactory.Sphere(0.09f, 8, 12);
        power.Begin(0);
        return power;
    }

    // Bolum (yeniden) baslar: lamba sayisi = toplanacak hucre sayisi (0 = lamba yok); hepsi sonuk, yoldaki isiklar silinir
    public void Begin(int lampCount)
    {
        StopAllCoroutines();
        foreach (Transform spark in transform) Destroy(spark.gameObject);
        count = Mathf.Min(lampCount, MaxLamps);
        sent = 0;
        float spacing = count > 1 ? Mathf.Min(MaxSpacing, RowWidth / (count - 1)) : 0f;
        for (int i = 0; i < MaxLamps; i++)
            lamps[i] = new Vector4(ColonyX + RowCenter + (i - (count - 1) * 0.5f) * spacing, RowY, -1f, 0f);
        Apply();
    }

    // Bir hucre toplandi: o noktadan siradaki lambaya isik ucar
    public void Deliver(Vector3 fromWorld)
    {
        if (sent >= count) return;
        StartCoroutine(Fly(fromWorld, sent++));
    }

    IEnumerator Fly(Vector3 from, int lamp)
    {
        var spark = Parts.Add("Spark", transform, sparkMesh, sparkMat, Vector3.zero, outline: false, castShadow: false);
        Vector3 start = from + Vector3.up * 0.4f;
        yield return Tween.Run(FlightTime, t =>
        {
            // hedef her karede yeniden hesaplanir: klavye acilip sahne kuculurse lamba da yer degistirir
            Vector3 end = LampWorld(lamp);
            Vector3 control = Vector3.Lerp(start, end, 0.35f) + Vector3.up * 2.5f;
            float e = Tween.InOutCubic(t);
            spark.position = Bezier(start, control, end, e);
            spark.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(t * 30f)) * (t < 0.8f ? 1f : (1f - t) * 5f);
        });
        Destroy(spark.gameObject);
        lamps[lamp].z = Time.timeSinceLevelLoad;   // cizimdeki _Time.y ile ayni saat
        Apply();
        Sound.Lamp();
    }

    static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
        => Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);

    // Lambanin ekrandaki yerine bakan, sahnenin cok gerisinde bir nokta (arka plan "fotografi" ile ayni olcek ve kayma: MarsPictureUV'nin tersi)
    Vector3 LampWorld(int lamp)
    {
        Vector4 picture = Shader.GetGlobalVector("_Picture");
        float horizon = Shader.GetGlobalFloat("_Horizon");
        float aspect = cam.pixelWidth / (float)cam.pixelHeight;
        var uv = new Vector2(lamps[lamp].x / aspect + 0.5f, horizon + lamps[lamp].y);
        Vector2 n = (uv * 2f - Vector2.one) * Mathf.Max(picture.x, 1e-3f) + new Vector2(0f, picture.y);
        Vector2 screen = (n * 0.5f + Vector2.one * 0.5f) * new Vector2(cam.pixelWidth, cam.pixelHeight);
        var ray = cam.ScreenPointToRay(screen);
        // gercek ufuk ekranin disinda: bu bakis bir yerde zemine degir; top zeminin icine girmesin diye ondan once durur
        float distance = cam.farClipPlane * 0.8f;
        if (new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float ground)) distance = Mathf.Min(distance, ground * 0.85f);
        return ray.GetPoint(distance);
    }

    void Apply()
    {
        Shader.SetGlobalVectorArray(LampsId, lamps);
        Shader.SetGlobalFloat(CountId, count);
    }
}
