using System.Collections;
using UnityEngine;

// Mavi boya kalemi (haritada Q, Bolge 7 · Bolum 68): Ece'nin yan kanyonda dusurdugu kalem. Alti koseli mavi govde, ortasinda
// acik mavi kagit bant, acik ahsap uc ve mavi kursun. Kuma yatmis durur; toplaninca sicrayip kaybolur, yerde mavi halka yayilir.
public class BluePencil : MonoBehaviour, IPickup
{
    public static readonly Color Blue = Mats.Hex("#2F6BD8");   // boya da ayni mavi (PaintTiles)
    static readonly Color Wood = Mats.Hex("#E6C394");
    static readonly Color Label = Mats.Hex("#A9C4F5");
    const float Radius = 0.04f, BodyLength = 0.4f, WoodLength = 0.09f, LeadLength = 0.035f;
    const float Scale = 1.35f, RingScale = 1.85f;   // halka diger toplananlarinki kadar

    Transform body, ring;
    Material ringMat;

    public static BluePencil Create(Transform parent, Vector3 localPos, int seed)
    {
        var go = new GameObject("BluePencil");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * Scale;
        var pencil = go.AddComponent<BluePencil>();
        pencil.Build(seed);
        return pencil;
    }

    void Build(int seed)
    {
        body = Parts.Empty("Body", transform);
        var lying = Parts.Empty("Yatan", body);
        lying.localPosition = new Vector3(0f, Radius * 0.87f, 0f);   // alti koseli govdenin yan yuzu kuma basar
        lying.localRotation = Quaternion.Euler(0f, 130f + seed * 11f, 90f);   // ekranda capraz: uzunlugu okunur
        BuildShape(lying);
        ring = PickupFx.AddRing(transform, Mats.Hex("#A9C6FF"), out ringMat);
        ring.localScale = Vector3.one * (RingScale / Scale);
    }

    // Kalemin yalnizca gorunusu; uzunlugu Y ekseni boyunca (ortasi 0, ucu +Y)
    public static void BuildShape(Transform parent)
    {
        var blue = Mats.Lit(Blue, 0.3f);
        float half = BodyLength * 0.5f;
        Parts.Add("Govde", parent, MeshFactory.RoundedCylinder(Radius, BodyLength, 0.004f, 6), blue, Vector3.zero);
        Parts.Add("Bant", parent, MeshFactory.RoundedCylinder(Radius * 1.03f, BodyLength * 0.5f, 0.002f, 6), Mats.Lit(Label, 0.2f),
                  new Vector3(0f, -half * 0.15f, 0f), outline: false);
        var wood = new[] { new Vector2(0f, 0f), new Vector2(Radius * 0.95f, 0f), new Vector2(Radius * 0.32f, WoodLength), new Vector2(0f, WoodLength) };
        Parts.Add("Ahsap", parent, MeshFactory.Lathe(wood, 12, flat: true), Mats.Lit(Wood, 0.15f), new Vector3(0f, half, 0f));
        var lead = new[] { new Vector2(0f, 0f), new Vector2(Radius * 0.32f, 0f), new Vector2(0f, LeadLength) };
        Parts.Add("Kursun", parent, MeshFactory.Lathe(lead, 12, flat: true), blue, new Vector3(0f, half + WoodLength, 0f), outline: false);
    }

    public void Pop() => StartCoroutine(PopCo());

    IEnumerator PopCo()
    {
        StartCoroutine(PickupFx.RingPulse(ring, ringMat));
        yield return PickupFx.PopAway(body, 1.25f, 0.25f);
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
