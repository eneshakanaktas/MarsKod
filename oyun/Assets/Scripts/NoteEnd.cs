using System;
using System.Collections;
using UnityEngine;

// Notlu bolumlerde (Bolge 8, 76 ve 80) notun yolunun sonu: haritada hedef yazilmaz, Kivilcim varinca hedef isareti belirir.
// 76'da ayrica o karede, Kivilcim'in ayaginin dibinde (kameraya bakan yaninda) yerde katlanmis cizim kumdan acilir gibi yavasca
// belirir (Bolge 7'de izin sonunda hedefin belirmesi gibi). Notun sonu hep yataklarin onu: cizim yatagin yaninda bulunur. Karenin
// yataga bakan (kuzey) kenarinda Kivilcim'in arkasinda kalip gorunmuyordu. Cizim 3a'da yerinde kalir; alinma sahnesi 3b'de.
// Dunya kurallarini bilmez: Oyun yolun son karesinin konumunu verir.
public class NoteEnd : MonoBehaviour
{
    static readonly Vector3 AtFeet = new Vector3(0.24f, 0f, -0.34f);   // karenin ortasindan one-saga: robotun govdesinin onunde
    const float RiseSeconds = 0.9f, SinkDepth = 0.02f;
    const float DustReach = 1.2f, DustScale = 0.2f, DustSeconds = 0.8f;

    ArrivalMark arrival;
    Func<float, float, float> ground;
    bool foldedDrawing;

    // 76'da yerde beliren katlanmis cizim (3b sahnesi alir); belirmeden once ve oteki bolumlerde null
    public Transform Drawing { get; private set; }

    public static NoteEnd Create(Transform levelRoot, Func<float, float, float> ground, bool foldedDrawing)
    {
        var end = Parts.Empty("NotunSonu", levelRoot).gameObject.AddComponent<NoteEnd>();
        end.arrival = new ArrivalMark(levelRoot);
        end.ground = ground;
        end.foldedDrawing = foldedDrawing;
        return end;
    }

    // Kivilcim notun son karesine vardi (cellCenter: karenin ortasi, alanin yerel konumu)
    public void Arrived(Vector3 cellCenter)
    {
        arrival.Show(cellCenter);
        if (!foldedDrawing || Drawing != null) return;
        float x = cellCenter.x + AtFeet.x, z = cellCenter.z + AtFeet.z;
        var at = new Vector3(x, ground(x, z), z);
        Drawing = FoldedDrawing.Create(transform, at, -12f);
        LandingDust.Play(transform, at, DustReach, DustScale, DustSeconds);
        StartCoroutine(Rise(Drawing, at));
    }

    // Kumun altindan yavasca yuzeye cikar ve acilir gibi buyur
    static IEnumerator Rise(Transform drawing, Vector3 at)
    {
        yield return Tween.Run(RiseSeconds, t =>
        {
            float e = Tween.OutCubic(t);
            drawing.localPosition = at + Vector3.down * (SinkDepth * (1f - e));
            drawing.localScale = Vector3.one * Mathf.Lerp(0.55f, 1f, e);
        });
    }

    // Tekrar / yeniden deneme: hedef ve cizim kalkar
    public void Restore()
    {
        StopAllCoroutines();
        arrival.Clear();
        if (Drawing != null) Destroy(Drawing.gameObject);
        Drawing = null;
    }
}
