using System.Collections;
using UnityEngine;

// Bolge 8 · 73 bitince onarilan guc kutulariyla tavan lambalarinin yanmasi (docs/superpowers/plans/2026-11-14-bolge-8-gorunus-3a.md, K1):
// lambalar soldan saga sirayla kirpisarak yanar (~1,2 sn), isik lostan aydinliga gecer, sonunda acil isik soner.
// Odanin kokunde durur: bolum degisince oda ile silinir; "Tekrar" (Room.SetLights) yarim kalan yanmayi durdurur.
public class StationLamps : MonoBehaviour
{
    const float LampDelay = 0.25f, FlickerStep = 0.05f, BlendSeconds = 1.2f;
    static readonly float[] Flicker = { 0.7f, 0f, 0.45f, 0.05f, 1f };

    StationRoom.Room room;

    public static StationLamps Attach(Transform roomRoot, StationRoom.Room room)
    {
        var lamps = roomRoot.gameObject.AddComponent<StationLamps>();
        lamps.room = room;
        return lamps;
    }

    public void TurnOn(Light sky, Light sun, RegionLook from, RegionLook to)
    {
        StopAllCoroutines();
        StartCoroutine(from.BlendTo(to, sky, sun, BlendSeconds));
        for (int i = 0; i < room.Lamps.Count; i++) StartCoroutine(FlickerOn(i, i * LampDelay));
        StartCoroutine(EmergencyOff(BlendSeconds));
    }

    public void Stop() => StopAllCoroutines();

    IEnumerator FlickerOn(int lamp, float delay)
    {
        yield return Tween.Wait(delay);
        foreach (float level in Flicker)
        {
            room.SetLamp(lamp, level);
            yield return Tween.Wait(FlickerStep);
        }
    }

    IEnumerator EmergencyOff(float delay)
    {
        yield return Tween.Wait(delay);
        room.SetEmergency(false);
    }
}
