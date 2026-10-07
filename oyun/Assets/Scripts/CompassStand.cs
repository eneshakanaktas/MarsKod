using System.Collections;
using UnityEngine;

// Yon bulma diregi (Bolge 4, docs/tasarim/senaryo-bolge-04.md): firtinada kadrani kopmus, kuma egik saplanmis ayak.
// Bolum 40 kapanisinda toplanan parcalar ustune oturur: kadran belirir, ibre firil firil doner, yavaslar
// ve verilen yonde (antende) durur. Yeniden denemede kadransiz haline doner.
public class CompassStand : MonoBehaviour
{
    static readonly Color NeedleGlow = Mats.Hex("#FF5A3C");

    Transform dial, needle;
    Material needleMat;

    public static CompassStand Create(Transform parent, Vector3 localPos)
    {
        var go = new GameObject("Iz-YonDiregi");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 1.5f;   // uzaktan da secilsin
        var stand = go.AddComponent<CompassStand>();
        stand.Build();
        stand.Restore();
        return stand;
    }

    void Build()
    {
        var brass = Mats.Lit(Mats.Hex("#C9A25A"), 0.6f);
        var rod = Mats.Lit(Mats.Hex("#6B6F78"), 0.45f);
        var sand = Mats.Lit(DuneTraces.Sand, 0.15f);

        Parts.Add("Kum", transform, MeshFactory.RoundedBox(new Vector3(0.3f, 0.07f, 0.24f), 0.05f), sand, new Vector3(0f, 0.02f, 0f), outline: false);
        Parts.Add("Ayak", transform, MeshFactory.RoundedCylinder(0.024f, 0.42f, 0.008f, 10), rod, new Vector3(0f, 0.21f, 0f));
        // tepede kadranin oturdugu pirinc yuva; yanindan kopmus kisa bir parca sarkiyor
        Parts.Add("Yuva", transform, MeshFactory.RoundedCylinder(0.05f, 0.025f, 0.008f, 14), brass, new Vector3(0f, 0.43f, 0f));
        var broken = Parts.Add("Kirik", transform, MeshFactory.RoundedBox(new Vector3(0.016f, 0.07f, 0.016f), 0.005f), rod, new Vector3(0.05f, 0.4f, 0f));
        broken.localRotation = Quaternion.Euler(0f, 0f, 35f);

        // kadran: kameraya dogru hafif egik, ibre kuzey ucu kirmizi (CompassPart ile ayni renkler)
        var face = Mats.Lit(Mats.Hex("#2B2A30"), 0.5f);
        var white = Mats.Lit(Mats.Hex("#E9E6DF"), 0.4f);
        needleMat = Mats.Emissive(Mats.Hex("#E8473B"), NeedleGlow * 0.5f, 0.5f);
        dial = Parts.Empty("Kadran", transform);
        dial.localPosition = new Vector3(0f, 0.46f, 0f);
        Parts.Add("Cerceve", dial, MeshFactory.RoundedCylinder(0.11f, 0.03f, 0.01f), brass, Vector3.zero);
        Parts.Add("Yuz", dial, MeshFactory.RoundedCylinder(0.088f, 0.034f, 0.006f), face, Vector3.zero, outline: false);
        needle = Parts.Empty("Ibre", dial);
        needle.localPosition = new Vector3(0f, 0.022f, 0f);
        Parts.Add("Kuzey", needle, MeshFactory.RoundedBox(new Vector3(0.026f, 0.012f, 0.085f), 0.005f), needleMat, new Vector3(0f, 0f, 0.042f), outline: false);
        Parts.Add("Guney", needle, MeshFactory.RoundedBox(new Vector3(0.026f, 0.012f, 0.085f), 0.005f), white, new Vector3(0f, 0f, -0.042f), outline: false);
        Parts.Add("Pim", needle, MeshFactory.Sphere(0.014f, 6, 10), brass, new Vector3(0f, 0.008f, 0f), outline: false);
    }

    // Kapanis: kadran yerine oturur, ibre donup dunya yonu "yaw" (derece, kuzey = 0) uzerinde durur.
    public IEnumerator PointTo(float yaw)
    {
        dial.gameObject.SetActive(true);
        Sound.Lamp();
        yield return Tween.Run(0.4f, t => dial.localScale = Vector3.one * Tween.OutBack(t));
        float from = needle.localEulerAngles.y;
        float spin = yaw + 360f * 3f;   // uc tam tur, sonra yavaslayip durur
        yield return Tween.Run(2.2f, t =>
        {
            needle.localRotation = Quaternion.Euler(0f, Mathf.Lerp(from, spin, Tween.OutCubic(t)), 0f);
            needleMat.SetColor("_EmissionColor", NeedleGlow * (0.5f + 1.2f * t));
        });
        Sound.Lamp();
    }

    public void Restore()
    {
        StopAllCoroutines();
        dial.gameObject.SetActive(false);
        dial.localScale = Vector3.one;
        needle.localRotation = Quaternion.Euler(0f, 140f, 0f);
        needleMat.SetColor("_EmissionColor", NeedleGlow * 0.5f);
    }
}
