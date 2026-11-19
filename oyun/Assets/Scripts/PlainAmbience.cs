using UnityEngine;

// Bolge 1 (inis ovasi, safaktan hemen once) canli cevresi: havada ince kizil toz zerreleri suzulur, ruzgar dalgasi gelince
// yerde toz savrulur, alanin arkasindaki ovada ara sira bir toz hortumu gezer (Mars'in bilinen toz hortumlari). Gokte ara sira
// kayan yildiz (MarsSky.hlsl, PlainBackdrop). Koloni karanlik kalir (hikaye: koloni bos).
public sealed class PlainAmbience : RegionAmbience
{
    static readonly Color Dust = new Color(0.72f, 0.42f, 0.32f, 0.10f);
    static readonly Color MoteA = new Color(0.86f, 0.58f, 0.45f, 0.45f), MoteB = new Color(0.95f, 0.75f, 0.6f, 0.35f);
    static readonly Color Devil = new Color(0.45f, 0.26f, 0.2f, 0.45f);   // ovanin pusundan koyu: yerden kalktigi secilsin

    public PlainAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 11, Dust, 1.5f, 7f));
        Add(new Motes(space, 12, rnd => new Vector3(rnd.Range(-6f, 6f), rnd.Range(0.15f, 1.6f), rnd.Range(-4f, 8f)), 6f, MoteA, MoteB,
            new Vector2(0.03f, 0.055f), glow: false));
        // toz hortumu alanin arkasinda, arka kenara yakin: daha uzakta dibi pusa karisir, tepesi gokyuzunde asili kalir
        Add(new DustDevil(space, 13, Devil, new Vector2(-7f, 7f), new Vector2(space.AreaHalf.y + 1.2f, space.AreaHalf.y + 2.0f), 1.3f, 24f, 12f));
    }
}
