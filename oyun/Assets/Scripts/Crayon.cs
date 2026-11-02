using UnityEngine;

// Ece'nin turkuaz boya kalemi (Bolge 7, Bolum 68'de yerde; 69'dan sonra Kivilcim'in sirtinda). Kisa (cok kullanilmis), altigen
// govde, ortasinda soluk kagit sargi, ucu cok asinmis: sivri degil, yuvarlak ve kut. Kok noktasi kalemin ortasi; kalem yerel X
// boyunca yatar (uc +X'te). Yerde ve sirtta ayni model, yalniz boyu farkli.
public static class Crayon
{
    static readonly Color Wrap = Mats.Hex("#CFE6DF");   // solmus kagit sargi

    public static Transform Create(Transform parent, Vector3 localPos, float length)
    {
        var root = Parts.Empty("Kalem", parent);
        root.localPosition = localPos;
        var axis = Parts.Empty("Eksen", root);
        axis.localRotation = Quaternion.Euler(0f, 0f, -90f);   // cizimler Y boyunca: Y -> +X
        float r = length * 0.12f, body = length * 0.82f;
        var wax = Mats.Lit(CraterTraces.EceColor, 0.35f);
        Parts.Add("Govde", axis, MeshFactory.RoundedCylinder(r, body, r * 0.15f, 6), wax, new Vector3(0f, -length * 0.5f + body * 0.5f, 0f));
        Parts.Add("Sargi", axis, MeshFactory.RoundedCylinder(r * 1.06f, body * 0.5f, r * 0.08f, 6), Mats.Lit(Wrap, 0.2f),
            new Vector3(0f, -length * 0.5f + body * 0.45f, 0f), outline: false);
        var tip = Parts.Add("Uc", axis, MeshFactory.Sphere(r * 0.95f, 6, 10), wax, new Vector3(0f, -length * 0.5f + body, 0f));
        tip.localScale = new Vector3(1f, 0.85f, 1f);   // asinmis: kut, yuvarlak
        return root;
    }
}
