using UnityEngine;

// Bolum 17: aracin su tanki (docs/tasarim/senaryo-bolge-02.md). Toplanan buz kadar dolar; bulmacayi etkilemez.
// Dolu kisim tabanda baslayip yukari uzanan bir silindir; tankin halkalari sabit.
public class WaterTank : MonoBehaviour
{
    const float Height = 0.42f, Radius = 0.11f;
    Transform water;

    public static WaterTank Create(Transform parent, Vector3 pos)
    {
        var root = Parts.Empty("Iz-SuTanki", parent);
        root.localPosition = pos;
        var tank = root.gameObject.AddComponent<WaterTank>();
        tank.Build(root);
        tank.Restore();
        return tank;
    }

    void Build(Transform root)
    {
        var rim = Mats.Lit(Mats.Hex("#8A909C"), 0.4f);
        var waterMat = Mats.Lit(Mats.Hex("#3F9BD6"), 0.6f);
        Parts.Add("Taban", root, MeshFactory.RoundedCylinder(Radius + 0.025f, 0.03f, 0.01f, 16), rim, new Vector3(0f, 0.015f, 0f));
        Parts.Add("Ust", root, MeshFactory.RoundedCylinder(Radius + 0.025f, 0.03f, 0.01f, 16), rim, new Vector3(0f, Height, 0f));
        water = Parts.Empty("Su", root);
        water.localPosition = new Vector3(0f, 0.03f, 0f);
        Parts.Add("SuGovde", water, MeshFactory.RoundedCylinder(Radius, Height - 0.03f, 0.01f, 16), waterMat, new Vector3(0f, (Height - 0.03f) * 0.5f, 0f), outline: false);
    }

    // fraction: 0 bos, 1 dolu
    public void Fill(float fraction)
    {
        water.localScale = new Vector3(1f, Mathf.Clamp01(fraction), 1f);
    }

    public void Restore() => Fill(0f);
}
