using System.Collections;
using UnityEngine;

// Kablo makarasi (Bolge 5): anteni yeniden kurmak icin dagilmis makaralar. Yan yatmis gri iki tekerlek (kenar),
// aralarina sarili turuncu kablo, ucundan kuma sarkan kisa bir kablo parcasi. Uzunlugu disaridan gorunmez
// (cable_length() olcer). Toplaninca sicrayip kaybolur, yerde turuncu halka yayilir.
public class CableReel : MonoBehaviour, IPickup
{
    Transform body, ring;
    Material ringMat;

    public static CableReel Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("CableReel");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 1.6f;
        var reel = go.AddComponent<CableReel>();
        reel.Build(seed);
        return reel;
    }

    void Build(int seed)
    {
        var metal = Mats.Lit(Mats.Hex("#9AA0A8"), 0.55f);
        var hub = Mats.Lit(Mats.Hex("#3A3D44"), 0.4f);
        var cable = Mats.Lit(Mats.Hex("#E8742E"), 0.3f);

        body = Parts.Empty("Body", transform);
        body.localRotation = Quaternion.Euler(0f, seed * 53f, 0f);

        // makara yan yatmis: ekseni yatay (X), kenar tekerlekleri kuma degiyor
        const float r = 0.13f;
        var spool = Parts.Empty("Spool", body);
        spool.localPosition = new Vector3(0f, r, 0f);
        spool.localRotation = Quaternion.Euler(0f, 0f, 90f);
        Parts.Add("FlangeL", spool, MeshFactory.RoundedCylinder(r, 0.022f, 0.006f), metal, new Vector3(0f, -0.085f, 0f));
        Parts.Add("FlangeR", spool, MeshFactory.RoundedCylinder(r, 0.022f, 0.006f), metal, new Vector3(0f, 0.085f, 0f));
        Parts.Add("Cable", spool, MeshFactory.RoundedCylinder(r * 0.78f, 0.15f, 0.02f), cable, Vector3.zero);
        Parts.Add("Hub", spool, MeshFactory.RoundedCylinder(0.03f, 0.2f, 0.006f), hub, Vector3.zero, outline: false);

        // makaradan cozulup kuma sarkan kablo ucu
        var loose = Parts.Add("Loose", body, MeshFactory.RoundedBox(new Vector3(0.022f, 0.018f, 0.16f), 0.008f), cable, new Vector3(0.02f, 0.01f, -0.16f));
        loose.localRotation = Quaternion.Euler(0f, 22f, 0f);

        ring = PickupFx.AddRing(transform, Mats.Hex("#FFB070"), out ringMat);
    }

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        yield return PickupFx.PopAway(body, 1.2f, 0.25f);
    }

    public void Restore()
    {
        StopAllCoroutines();
        body.gameObject.SetActive(true);
        body.localScale = Vector3.one;
        body.localPosition = Vector3.zero;
        ring.gameObject.SetActive(false);
    }
}
