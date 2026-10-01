using MarsKod.Dunya;
using UnityEngine;

// Bolumdeki engel (haritada K): oyun kurali her bolgede ayni (gecilmez), gorunusu bolgeye gore.
// Kod bilmeyen de "buradan gecilmez" diye okusun: kareyi dolduran iri bir parca + yaninda kucuk bir parca.
public static class Obstacles
{
    public static void Create(Region region, Transform parent, Vector3 pos, int index)
    {
        if (region == Region.PolarIce) IceBlock(parent, pos, index);
        else Rock(parent, pos, index);
    }

    static void Rock(Transform parent, Vector3 p, int i)
    {
        var big = Parts.Add("Obstacle", parent, MeshFactory.Rock(0.34f, 200 + i), Mats.Lit(Mats.Hex("#7A4A3B"), 0.15f), p + new Vector3(0f, 0.05f, 0f));
        big.localRotation = Quaternion.Euler(0, i * 71f + 20f, 0);
        var small = Parts.Add("Obstacle", parent, MeshFactory.Rock(0.13f, 300 + i), Mats.Lit(Mats.Hex("#5E3A2F"), 0.12f), p + new Vector3(0.28f, 0.02f, -0.22f));
        small.localRotation = Quaternion.Euler(0, i * 37f, 0);
    }

    // Koseli buz blogu: soluk mavi, ustunde kar; yaninda kucuk bir blok. Parlamaz (toplanan parlak buz kristalleriyle karismasin).
    static void IceBlock(Transform parent, Vector3 p, int i)
    {
        var ice = Mats.Lit(Mats.Hex("#9FBAD6"), 0.75f);
        var snow = Mats.Lit(Mats.Hex("#E4ECF5"), 0.3f);
        var block = Parts.Empty("Obstacle", parent);
        block.localPosition = p;
        block.localRotation = Quaternion.Euler(4f * Mathf.Sin(i * 1.7f), i * 71f + 20f, 5f * Mathf.Cos(i * 2.3f));
        var size = new Vector3(0.56f, 0.44f, 0.5f);
        Parts.Add("Ice", block, MeshFactory.RoundedBox(size, 0.06f), ice, new Vector3(0f, size.y * 0.5f, 0f));
        Parts.Add("Snow", block, MeshFactory.RoundedBox(new Vector3(size.x - 0.04f, 0.06f, size.z - 0.04f), 0.03f), snow, new Vector3(0f, size.y + 0.005f, 0f), outline: false);

        var chip = Parts.Add("Obstacle", parent, MeshFactory.RoundedBox(new Vector3(0.2f, 0.15f, 0.17f), 0.035f), ice, p + new Vector3(0.3f, 0.075f, -0.24f));
        chip.localRotation = Quaternion.Euler(8f, i * 37f, -6f);
    }
}
