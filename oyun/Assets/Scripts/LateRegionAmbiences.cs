using UnityEngine;

// Bolge 6-10 canli cevreleri (docs/tasarim/canli-cevre.md). Her biri ortak parcalardan (AmbientPieces) kurulur; bolgenin
// hikaye nesneleri (Serce, istasyonun lambalari, kizgin goktaslari...) kendi dosyalarinda kalir, burasi onlara dokunmaz.

// Bolge 6 (kanyon girisi, gun batimi): alcak gunes isiginda parlayan sicak zerreler, kanyon tabaninda savrulan ince toz
public sealed class CanyonAmbience : RegionAmbience
{
    static readonly Color Dust = new Color(0.72f, 0.45f, 0.34f, 0.10f);
    static readonly Color MoteA = new Color(1f, 0.74f, 0.45f, 0.55f), MoteB = new Color(1f, 0.6f, 0.4f, 0.45f);

    public CanyonAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 61, Dust, 1.5f, 6f));
        Add(new Motes(space, 62, rnd => new Vector3(rnd.Range(-6f, 6f), rnd.Range(0.2f, 2f), rnd.Range(-4f, 8f)), 9f, MoteA, MoteB,
            new Vector2(0.03f, 0.055f), glow: true));
    }
}

// Bolge 7 (kanyon dibi, eski nehir yatagi; sabah, alan golgede): yere yakin yavas sabah sisi, duvarlarin ustunden gelen
// gunes isiginda parlayan zerreler
public sealed class RiverBedAmbience : RegionAmbience
{
    static readonly Color Mist = new Color(0.86f, 0.84f, 0.86f, 0.12f);
    static readonly Color MoteA = new Color(1f, 0.92f, 0.75f, 0.5f), MoteB = new Color(0.85f, 0.95f, 1f, 0.4f);

    public RiverBedAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 71, Mist, 4f, 3f));
        Add(new Motes(space, 72, rnd => new Vector3(rnd.Range(-6f, 6f), rnd.Range(0.3f, 2f), rnd.Range(-4f, 8f)), 7f, MoteA, MoteB,
            new Vector2(0.028f, 0.05f), glow: true));
    }
}

// Bolge 8 (terk edilmis arastirma istasyonu): icerde tavan lambalarinin isiginda yavasca suzulen toz; disarida (istasyonun
// onu) ovadaki gibi ince toz. Lambalar ve ekranlar hikayenin parcasi (StationTraces), dokunulmaz.
public sealed class StationAmbience : RegionAmbience
{
    static readonly Color IndoorA = new Color(1f, 0.93f, 0.8f, 0.4f), IndoorB = new Color(0.85f, 0.9f, 1f, 0.3f);
    static readonly Color Dust = new Color(0.66f, 0.5f, 0.45f, 0.09f);

    public StationAmbience(AmbienceSpace space, bool indoor) : base(space)
    {
        if (indoor)
        {
            // yalniz odanin icinde (duvarlarin disina tasmaz)
            Add(new Motes(space, 81, rnd => new Vector3(rnd.Range(-space.AreaHalf.x + 0.2f, space.AreaHalf.x - 0.2f), rnd.Range(0.25f, 1.8f),
                rnd.Range(-space.AreaHalf.y + 0.2f, space.AreaHalf.y - 0.2f)), 5f, IndoorA, IndoorB, new Vector2(0.024f, 0.042f), glow: true));
            return;
        }
        Add(new GroundDrift(space, 82, Dust, 1.5f, 6f));
        Add(new Motes(space, 83, rnd => new Vector3(rnd.Range(-6f, 6f), rnd.Range(0.15f, 1.6f), rnd.Range(-4f, 8f)), 5f, IndoorA, IndoorB,
            new Vector2(0.026f, 0.045f), glow: false));
    }
}

// Bolge 9 (tuz golu; bembeyaz, gokyuzunu yansitan kurumus gol): tuz kabugunda gunes parıltısı yanip soner, ruzgar dalgasiyla
// beyaz tuz tozu savrulur
public sealed class SaltLakeAmbience : RegionAmbience
{
    static readonly Color Salt = new Color(0.82f, 0.76f, 0.62f, 0.16f);   // bembeyaz zeminde secilsin diye biraz koyu
    static readonly Color GlintA = Mats.Hex("#E8B83A"), GlintB = Mats.Hex("#FFE27A");   // altin: tuz kristallerinin parıltısıyla ayni

    public SaltLakeAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 91, Salt, 2.5f, 10f));
        // yalniz yakin zeminde: gol uzakta (z > ~5) gokyuzunu yansitan resme karisir, orada parilti havada gorunur
        Add(new Glints(space, 92, AroundArea(space, new Vector2(-6f, 6f), new Vector2(-4.5f, 5f), 0.03f), 20f, GlintA, GlintB,
            new Vector2(0.12f, 0.19f), glow: false));
    }
}

// Bolge 10 (gece ovasi, meteor yagmuru): gokte sik kayan yildiz (NightSky.hlsl); alanin cevresinde onceki dususlerden
// kalmis, icin icin yanan kucuk kor taslari ve onlardan yukselen kivilcimlar. Kor taslari soluk: bolumun kizgin goktaslariyla
// (MeteorMarks.HotStone, alanin icinde, tehlike) karismasin diye kucuk ve alanin disinda.
public sealed class NightPlainAmbience : RegionAmbience
{
    static readonly Color Dust = new Color(0.32f, 0.28f, 0.36f, 0.12f);
    static readonly Color EmberA = Mats.Hex("#FF9A3D"), EmberB = Mats.Hex("#FFD27A");

    public NightPlainAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 101, Dust, 1.5f, 6f));
        var coals = new[]
        {
            new Vector2(-space.AreaHalf.x - 0.6f, space.AreaHalf.y + 0.9f),
            new Vector2(space.AreaHalf.x + 0.7f, space.AreaHalf.y + 1.6f),
            new Vector2(-space.AreaHalf.x - 1.8f, space.AreaHalf.y + 3.4f),
            new Vector2(space.AreaHalf.x + 2.2f, space.AreaHalf.y + 4.2f),
            new Vector2(-space.AreaHalf.x - 1.2f, -space.AreaHalf.y - 0.4f),
        };
        for (int i = 0; i < coals.Length; i++) Add(new Coal(space, coals[i], i));
        Add(new Embers(space, 102, rnd =>
        {
            var c = coals[(int)rnd.Range(0f, coals.Length - 0.001f)];
            return space.OnGround(c.x + rnd.Range(-0.08f, 0.08f), c.y + rnd.Range(-0.08f, 0.08f), 0.06f);
        }, 6f, EmberA, EmberB));
    }

    // Icin icin yanan kucuk kor tasi: koyu, catlaklarindan turuncu isik nefes alir gibi parlayip soner
    sealed class Coal : IAmbientPiece
    {
        readonly Material glow;
        readonly float phase;

        public Coal(AmbienceSpace space, Vector2 at, int seed)
        {
            phase = seed * 1.9f;
            var root = Parts.Empty("KorTasi", space.Root);
            root.localPosition = new Vector3(at.x, 0f, at.y);
            glow = Mats.Emissive(Mats.Hex("#2A2224"), Mats.Hex("#FF6A2A") * 0.8f, 0.3f);
            Parts.Add("Tas", root, MeshFactory.Rock(0.09f + seed % 2 * 0.03f, 20 + seed), glow, Vector3.zero, outline: false);
            Parts.Add("Tas", root, MeshFactory.Rock(0.05f, 40 + seed), glow, new Vector3(0.12f, 0f, 0.04f), outline: false);
            GroundPlacement.SitOnGround(root, space.Height, 0.02f);
        }

        public void Tick(float time, float dt, float density, float gust)
        {
            float breath = 0.55f + 0.45f * Mathf.Sin(time * 1.3f + phase) + 0.25f * gust;
            glow.SetColor("_EmissionColor", Mats.Hex("#FF6A2A") * breath);
        }
    }
}
