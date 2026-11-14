using UnityEngine;

// Istasyonun zemindeki guc kutusu (Bolge 8'de panelin gorunusu; davranisi SolarPanel'de, Bolge 3'teki panelle ayni: olcme, onarma).
// Alcak metal kutu, ustunde menteseli kapak; kapagin on kenarinda 10 kucuk isik cubugu (yanan cubuk = guc / 10). Kivilcim ustune
// cikabilir (yukseklik ~0,1). Catlakta kapagin kosesi kirik kalkmis, kapakta catlak cizgileri, ara sira kivilcim (catlak grubu:
// onarilinca kaybolur, kapak kapali gorunur); cubuklarin kirpismasi SolarPanel'de (flickerWhenCracked).
public static class PowerUnitLook
{
    public const float Height = 0.1f;

    static readonly Material Box = Mats.Lit(Mats.Hex("#4C525A"), 0.4f);
    static readonly Material Lid = Mats.Lit(Mats.Hex("#6E747D"), 0.45f);
    static readonly Material Strip = Mats.Lit(Mats.Hex("#1C1F24"), 0.5f);
    static readonly Material Hinge = Mats.Lit(Mats.Hex("#2E3238"), 0.5f);
    static readonly Material Mark = Mats.Lit(StationRoom.ColonyOrange, 0.3f);
    static readonly Material Crack = Mats.Unlit(Mats.Hex("#E8ECF2"));
    static readonly Color Spark = Mats.Hex("#FFD27A");

    // body: panelin govdesi. lightMats: 10 cubugun malzemesi (burada kurulur). cracks: catlak grubu. glow: onarimda bir an parlayan lens.
    public static void Build(Transform body, Material[] lightMats, Color off, out Transform cracks, out Material glow)
    {
        var size = new Vector3(0.66f, 0.07f, 0.46f);
        Parts.Add("Kutu", body, MeshFactory.RoundedBox(size, 0.02f), Box, new Vector3(0f, size.y * 0.5f, 0f));
        float lidY = size.y + 0.012f;
        Parts.Add("Kapak", body, MeshFactory.RoundedBox(new Vector3(0.62f, 0.024f, 0.42f), 0.008f, 2), Lid, new Vector3(0f, lidY, 0.005f));
        Parts.Add("Mentese", body, MeshFactory.RoundedCylinder(0.012f, 0.5f, 0.004f, 8), Hinge, new Vector3(0f, lidY, 0.215f), outline: false)
            .localRotation = Quaternion.Euler(0f, 0f, 90f);
        float top = lidY + 0.012f;
        // koselerde turuncu isaret
        foreach (float sx in new[] { -1f, 1f })
            Parts.Add("Isaret", body, MeshFactory.RoundedBox(new Vector3(0.07f, 0.004f, 0.07f), 0.002f, 1), Mark,
                new Vector3(sx * 0.26f, top + 0.001f, 0.16f), outline: false, castShadow: false);
        // on kenarda koyu serit + 10 cubuk
        Parts.Add("Serit", body, MeshFactory.RoundedBox(new Vector3(0.56f, 0.004f, 0.1f), 0.002f, 1), Strip,
            new Vector3(0f, top + 0.001f, -0.14f), outline: false, castShadow: false);
        for (int k = 0; k < lightMats.Length; k++)
        {
            lightMats[k] = Mats.Emissive(off, Color.black, 0.6f);
            Parts.Add("PowerLight", body, MeshFactory.RoundedBox(new Vector3(0.036f, 0.008f, 0.07f), 0.003f, 1), lightMats[k],
                new Vector3(-0.24f + k * (0.48f / (lightMats.Length - 1)), top + 0.004f, -0.14f), outline: false, castShadow: false);
        }
        // durum lensi: onarilinca bir an parlar
        glow = Mats.Emissive(Mats.Hex("#5A4A2A"), Mats.Hex("#2A1C08"), 0.7f);
        Parts.Add("Lens", body, MeshFactory.RoundedCylinder(0.025f, 0.01f, 0.003f, 12), glow, new Vector3(0.2f, top + 0.004f, 0.06f), outline: false, castShadow: false);

        // catlak: kapagin sol arka kosesi kirilip kalkmis, kapakta catlaklar, kirik kosede ara sira kivilcim
        cracks = Parts.Empty("Cracks", body);
        cracks.localPosition = new Vector3(-0.12f, top + 0.003f, 0.05f);
        var corner = Parts.Add("KirikKose", cracks, MeshFactory.RoundedBox(new Vector3(0.16f, 0.02f, 0.13f), 0.006f, 2), Lid, new Vector3(-0.1f, 0.03f, 0.08f));
        corner.localRotation = Quaternion.Euler(-24f, 18f, 14f);
        float[] angles = { 10f, 70f, 140f, 200f, 300f };
        float[] lengths = { 0.18f, 0.1f, 0.15f, 0.11f, 0.13f };
        for (int k = 0; k < angles.Length; k++)
        {
            var line = Parts.Add("Crack", cracks, MeshFactory.RoundedBox(new Vector3(lengths[k], 0.004f, 0.01f), 0.002f, 1), Crack,
                Quaternion.Euler(0f, angles[k], 0f) * new Vector3(lengths[k] * 0.5f, 0f, 0f), outline: false, castShadow: false);
            line.localRotation = Quaternion.Euler(0f, angles[k], 0f);
        }
        var spark = Parts.Add("Kivilcim", cracks, MeshFactory.Sphere(0.02f, 6, 10), null, new Vector3(-0.08f, 0.05f, 0.06f), outline: false, castShadow: false);
        Blinker.Attach(spark, Spark, 2.3f, 1.1f);
    }
}
