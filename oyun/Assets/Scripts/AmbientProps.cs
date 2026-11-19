using System;
using UnityEngine;

// Canli cevrenin birden fazla bolgede kullanilan hareketli cizimleri (koloninin birakilmis olcum/enerji araclari).
// Her biri koku dondurur (cagiran yerini ve donusunu verir, sonra GroundPlacement.SitOnGround ile oturtur) ve hareketli
// parcalarini add ile canli cevreye kaydeder. Renkler gorsel yonun "insan yapimi" paleti: krem govde, turuncu serit, koyu metal.
public static class AmbientProps
{
    static Material Metal => Mats.Lit(Mats.Hex("#5C6372"), 0.5f);
    static Material Cream => Mats.Lit(Mats.Hex("#E9E3D8"), 0.4f);
    static Material Orange => Mats.Lit(Mats.Hex("#FF7A3D"), 0.3f);

    /// <summary>Ince direk, tepesinde turuncu-beyaz seritli ruzgar corabi ve yanip sonen kirmizi lamba (inis/enerji alanlari)</summary>
    public static Transform WindsockMast(Transform parent, Action<IAmbientPiece> add, float height = 0.95f, float phase = 0f)
    {
        var root = Parts.Empty("RuzgarCorabi", parent);
        var metal = Metal;
        Parts.Add("Taban", root, MeshFactory.RoundedCylinder(0.07f, 0.04f, 0.012f, 12), metal, new Vector3(0f, 0.02f, 0f), outline: false);
        Parts.Add("Direk", root, MeshFactory.RoundedCylinder(0.02f, height, 0.006f, 10), metal, new Vector3(0f, height * 0.5f, 0f));
        Parts.Add("Halka", root, MeshFactory.RoundedCylinder(0.05f, 0.02f, 0.006f, 12), metal, new Vector3(0.0f, height - 0.04f, 0f), outline: false);
        add(Pennant.CreateSock(root, new Vector3(0.02f, height - 0.04f, 0f), 0.36f, 0.05f, Orange, Cream, 5, phase));
        add(new BlinkLamp(root, new Vector3(0f, height + 0.03f, 0f), 0.024f, Mats.Hex("#FF4A3A"), 2.1f, phase));
        return root;
    }

    /// <summary>Kucuk dikey eksenli ruzgar turbini: kisa ayakli govde, ustte iki kivrik kanatli rotor ruzgarla doner, gunduz
    /// enerjisi gosterge lambasi yesil yanip soner</summary>
    public static Transform WindTurbine(Transform parent, Action<IAmbientPiece> add, float phase = 0f)
    {
        var root = Parts.Empty("RuzgarTurbini", parent);
        var metal = Metal;
        var cream = Cream;
        Parts.Add("Kutu", root, MeshFactory.RoundedBox(new Vector3(0.24f, 0.16f, 0.2f), 0.025f), cream, new Vector3(0f, 0.08f, 0f));
        Parts.Add("Serit", root, MeshFactory.RoundedBox(new Vector3(0.245f, 0.03f, 0.205f), 0.008f), Orange, new Vector3(0f, 0.12f, 0f), outline: false);
        Parts.Add("Mil", root, MeshFactory.RoundedCylinder(0.018f, 0.62f, 0.006f, 10), metal, new Vector3(0f, 0.47f, 0f));

        var rotor = Parts.Empty("Rotor", root);
        rotor.localPosition = new Vector3(0f, 0.55f, 0f);
        Parts.Add("UstDisk", rotor, MeshFactory.RoundedCylinder(0.13f, 0.012f, 0.004f, 16), metal, new Vector3(0f, 0.2f, 0f), outline: false);
        Parts.Add("AltDisk", rotor, MeshFactory.RoundedCylinder(0.13f, 0.012f, 0.004f, 16), metal, new Vector3(0f, -0.2f, 0f), outline: false);
        for (int k = 0; k < 2; k++)
        {
            var blade = Parts.Empty("Kanat", rotor);
            blade.localRotation = Quaternion.Euler(0f, k * 180f, 0f);
            // kivrik kanat: uc ince levha yay gibi dizilir
            for (int s = 0; s < 3; s++)
            {
                float a = (s - 1) * 32f * Mathf.Deg2Rad;
                Parts.Add("Levha", blade, MeshFactory.RoundedBox(new Vector3(0.06f, 0.38f, 0.008f), 0.003f, 2), cream,
                    new Vector3(0.06f + Mathf.Sin(a) * 0.05f, 0f, Mathf.Cos(a) * 0.05f - 0.05f), outline: false)
                    .localRotation = Quaternion.Euler(0f, -(s - 1) * 32f, 0f);
            }
        }
        add(new Spinner(rotor, Vector3.up, 90f, 420f));
        add(new BlinkLamp(root, new Vector3(0.1f, 0.12f, -0.101f), 0.018f, Mats.Hex("#5CFF7A"), 1.3f, phase));
        return root;
    }

    /// <summary>Kokun yerini (x, z) verir, ruzgara (doguya) gore hafif cevirir ve zemine oturtur</summary>
    public static Transform Place(Transform root, AmbienceSpace space, float x, float z, float yaw = 0f, float sink = 0.02f)
    {
        root.localPosition = new Vector3(x, 0f, z);
        root.localRotation = Quaternion.Euler(0f, yaw, 0f);
        GroundPlacement.SitOnGround(root, space.Height, sink);
        return root;
    }
}
