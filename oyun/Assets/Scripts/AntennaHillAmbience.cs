using UnityEngine;

// Bolge 5 (anten tepesi; gunduz, ruzgarli yukseklik) canli cevresi: tepede ruzgar daha sert: kum yerde sik savrulur, havada
// gunes isiginda zerreler parlar. Sag arka kosede tepenin ruzgar corabi (hep dalgalanir). Buyuk anten ve kablolar
// (AntennaTraces, BigAntenna) hikayenin kendisi: anten 50'de guce gecene kadar sessiz kalir, canli cevre ona dokunmaz.
public sealed class AntennaHillAmbience : RegionAmbience
{
    static readonly Color Dust = new Color(0.82f, 0.6f, 0.44f, 0.13f);
    static readonly Color MoteA = new Color(1f, 0.85f, 0.65f, 0.5f), MoteB = new Color(0.95f, 0.72f, 0.55f, 0.4f);

    public AntennaHillAmbience(AmbienceSpace space) : base(space)
    {
        Add(new GroundDrift(space, 51, Dust, 4f, 16f));
        Add(new Motes(space, 52, rnd => new Vector3(rnd.Range(-6f, 6f), rnd.Range(0.1f, 1.6f), rnd.Range(-4f, 8f)), 7f, MoteA, MoteB,
            new Vector2(0.028f, 0.05f), glow: true));
        AmbientProps.Place(AmbientProps.WindsockMast(space.Root, p => Add(p), 1.05f, 1.3f), space, space.AreaHalf.x + 0.45f, space.AreaHalf.y + 0.55f);
    }
}
