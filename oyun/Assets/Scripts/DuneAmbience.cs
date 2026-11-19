using UnityEngine;

// Bolge 4 (kum tepeleri; oyundaki ilk gunduz) canli cevresi: ruzgar dalgasi gelince kum yerde savrulur, gunes isiginda kum
// zerreleri parlar, ovada iki toz hortumu gezer (Mars'ta toz hortumlari gunduz, en sicak saatte cikar). Isaret direkleri ve
// Ece'nin kurdelesi (DuneTraces) hikayenin kendisi; yeni cizim eklenmedi, kalabalik etmesin.
public sealed class DuneAmbience : RegionAmbience
{
    static readonly Color Sand = new Color(0.9f, 0.7f, 0.5f, 0.22f);
    static readonly Color MoteA = new Color(1f, 0.85f, 0.62f, 0.5f), MoteB = new Color(0.95f, 0.7f, 0.5f, 0.4f);
    static readonly Color Devil = new Color(0.55f, 0.34f, 0.22f, 0.5f);   // kum renkli zeminde secilsin diye koyu

    public DuneAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 41, Sand, 3f, 14f));
        Add(new Motes(space, 42, rnd => new Vector3(rnd.Range(-6f, 6f), rnd.Range(0.05f, 1.4f), rnd.Range(-4f, 8f)), 8f, MoteA, MoteB,
            new Vector2(0.03f, 0.052f), glow: true));
        float back = space.AreaHalf.y;
        // pus alanin arkasinda erken basliyor (z ~ 4,5'ten sonrasi arka plana karisir): hortumlar arka kenara yakin gecer
        Add(new DustDevil(space, 43, Devil, new Vector2(-7f, 7f), new Vector2(back + 1.0f, back + 1.6f), 1.5f, 18f, 7f));
        Add(new DustDevil(space, 44, Devil, new Vector2(-8f, 8f), new Vector2(back + 2.0f, back + 2.6f), 1.2f, 24f, 12f));
    }
}
