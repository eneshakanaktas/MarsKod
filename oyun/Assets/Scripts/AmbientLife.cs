using System;
using MarsKod.Dunya;
using UnityEngine;

// Bolgenin canli cevresi (Ragip, 2026-11-19; docs/tasarim/canli-cevre.md): kar yagar, toz hortumu gezer, kayan yildiz gecer,
// ruzgar olcer doner... Bulmacayi etkilemez, yalniz gorunus. Her bolge kendi sinifini yazar (IAmbience); burasi bolge degisince
// eskisini siler, yenisini kurar ve her karede calistirir.
// - Oyuncunun "Animasyonlar" ayari kapaliysa hicbir sey kipirdamaz, yeni tane atilmaz (duran nesneler yerinde kalir).
// - Performans modunda tane sayisi yariya iner (Density).
// Yeni bolge: IAmbience'i uygulayan bir sinif + Ambiences.Create'e bir satir.
public interface IAmbience
{
    /// <summary>Ayni bolgede bolum degisti (bolume ozel ayar: ornegin firtinanin siddeti, hikaye anina gore isik)</summary>
    void SetLevel(int levelNumber);

    /// <summary>Her kare. time: oyun saati (sn), dt: kare suresi, density: tane yogunlugu (1 tam, performans modunda daha az)</summary>
    void Tick(float time, float dt, float density);
}

// Canli cevrenin kuruldugu yer ve zeminin bilgileri (bolge siniflarina verilir)
public sealed class AmbienceSpace
{
    public Transform Root;                         // bolgenin butun nesneleri bunun altinda (bolge degisince birlikte silinir)
    public Vector2 AreaHalf;                       // oyun alaninin yari genisligi (x) ve yari derinligi (z)
    public Func<float, float, float> Height;       // zemin yuksekligi (x, z)
    public Func<string, Material> FarMat;          // uzakta arka plana karisan malzeme (Oyun.FarRockMat)

    /// <summary>Oyun alaninin icinde mi (kenar payi margin kadar genisletilmis)</summary>
    public bool InArea(float x, float z, float margin = 0f) => Mathf.Abs(x) < AreaHalf.x + margin && Mathf.Abs(z) < AreaHalf.y + margin;

    /// <summary>Zeminde bir nokta (y = zemin yuksekligi + lift)</summary>
    public Vector3 OnGround(float x, float z, float lift = 0f) => new Vector3(x, Height(x, z) + lift, z);
}

// Bolum basina tane sayaci + tohumlu rastgele: "saniyede N tane" diye atilir; ayni tohumla her seferinde ayni dizi
public sealed class Sprinkler
{
    readonly System.Random rnd;
    float due;

    public Sprinkler(int seed) { rnd = new System.Random(seed); }

    /// <summary>Bu karede atilacak tane sayisi</summary>
    public int Due(float perSecond, float dt)
    {
        due += perSecond * dt;
        int n = (int)due;
        due -= n;
        return n;
    }

    public float Range(float min, float max) => min + (max - min) * (float)rnd.NextDouble();
    public bool Chance(float p) => rnd.NextDouble() < p;
    public Color Mix(Color a, Color b) => Color.Lerp(a, b, (float)rnd.NextDouble());
}

public sealed class AmbientLife : MonoBehaviour
{
    const float PerformanceDensity = 0.45f;

    AmbienceSpace space;
    Transform world;
    IAmbience current;
    Region? region;
    bool indoor;

    /// <summary>Oyuncunun animasyon ayari: kapaliyken canli cevre durur</summary>
    public bool Animated { get; set; } = true;

    public static AmbientLife Create(Transform world, Vector2 areaHalf, Func<float, float, float> height, Func<string, Material> farMat)
    {
        var life = new GameObject("CanliCevre").AddComponent<AmbientLife>();
        life.world = world;
        life.space = new AmbienceSpace { AreaHalf = areaHalf, Height = height, FarMat = farMat };
        return life;
    }

    /// <summary>Bolum yuklenince: bolge degistiyse eski cevre silinir, yenisi kurulur. indoor: istasyonun ici (dis manzara yok)</summary>
    public void Show(Region newRegion, int levelNumber, bool newIndoor)
    {
        if (region != newRegion || indoor != newIndoor)
        {
            if (space.Root != null) Destroy(space.Root.gameObject);
            space.Root = Parts.Empty("CanliCevre-" + newRegion + (newIndoor ? "-ic" : ""), world);
            region = newRegion;
            indoor = newIndoor;
            current = Ambiences.Create(newRegion, newIndoor, space);
        }
        current?.SetLevel(levelNumber);
    }

    void Update()
    {
        if (current == null || !Animated) return;
        current.Tick(Time.time, Time.deltaTime, GraphicsQuality.Performance ? PerformanceDensity : 1f);
    }
}

// Ortak ruzgar: ara sira gelip giden ruzgar dalgalari (0 = durgun, 1 = en sert an). Ayni bolgedeki her sey (flama, ruzgar olcer,
// savrulan kar/kum) ayni ruzgara uyar; boylece sahne "birlikte" kipirdar.
public static class Wind
{
    public static float Gust(float time)
    {
        float slow = 0.5f + 0.5f * Mathf.Sin(time * 0.31f);
        float quick = 0.5f + 0.5f * Mathf.Sin(time * 0.83f + 1.3f);
        return Mathf.Clamp01(Mathf.Pow(slow, 2.2f) * 0.8f + quick * slow * 0.35f);
    }
}

// Bolgeye gore canli cevre sinifi
public static class Ambiences
{
    public static IAmbience Create(Region region, bool indoor, AmbienceSpace space)
    {
        switch (region)
        {
            case Region.Plain: return new PlainAmbience(space);
            case Region.PolarIce: return new PolarAmbience(space);
            case Region.CraterField: return new CraterAmbience(space);
            case Region.Dunes: return new DuneAmbience(space);
            case Region.AntennaHill: return new AntennaHillAmbience(space);
            case Region.Canyon: return new CanyonAmbience(space);
            case Region.RiverBed: return new RiverBedAmbience(space);
            case Region.Station: return new StationAmbience(space, indoor);
            case Region.SaltLake: return new SaltLakeAmbience(space);
            case Region.NightPlain: return new NightPlainAmbience(space);
            default: return null;
        }
    }
}
