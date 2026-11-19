using UnityEngine;

// Bolge 3 (kraterli duzluk, gunes paneli tarlasi; safak cok yakin) canli cevresi: safagin pembe isiginda parlayan toz zerreleri,
// yerde savrulan toz, uzakta kucuk bir toz hortumu, gokte kayan yildiz (CraterSky.hlsl). Panel tarlasinin birakilmis enerji
// araclari: ruzgar corabi (sol arka kose) ve kucuk ruzgar turbini (yesil lambasi "hala calisiyor" der). Defne'nin kulubesi
// ve firtina izleri (CraterTraces) alanin arkasinin ortasinda; yeni cizimler onlarin yanlarinda.
public sealed class CraterAmbience : RegionAmbience
{
    static readonly Color Dust = new Color(0.62f, 0.4f, 0.36f, 0.10f);
    static readonly Color MoteA = new Color(1f, 0.72f, 0.62f, 0.55f), MoteB = new Color(0.86f, 0.7f, 0.95f, 0.45f);
    static readonly Color Devil = new Color(0.4f, 0.26f, 0.24f, 0.42f);

    public CraterAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 31, Dust, 1.5f, 6f));
        Add(new Motes(space, 32, rnd => new Vector3(rnd.Range(-6f, 6f), rnd.Range(0.2f, 1.8f), rnd.Range(-4f, 8f)), 7f, MoteA, MoteB,
            new Vector2(0.03f, 0.052f), glow: true));
        // yalniz sol yarida: kulubenin arkasindan gecerse bacadan cikan duman gibi okunur ("kulubede biri var" yanlis ipucu)
        Add(new DustDevil(space, 33, Devil, new Vector2(-7.5f, -1.2f), new Vector2(space.AreaHalf.y + 1.4f, space.AreaHalf.y + 2.0f), 1.0f, 16f, 14f));

        AmbientProps.Place(AmbientProps.WindsockMast(space.Root, p => Add(p), 0.95f, 0.4f), space, -space.AreaHalf.x - 0.45f, space.AreaHalf.y + 0.55f);
        AmbientProps.Place(AmbientProps.WindTurbine(space.Root, p => Add(p), 1.1f), space, space.AreaHalf.x + 0.5f, space.AreaHalf.y + 0.6f, -20f);
    }
}
