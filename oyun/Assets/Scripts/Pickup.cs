using System.Collections;
using MarsKod.Dunya;
using UnityEngine;

// Sahnedeki toplanabilir nesne (buz, enerji hucresi...): toplaninca kendi efektini oynatir, bolum yeniden baslayinca geri gelir.
public interface IPickup
{
    void Pop();
    void Restore();
}

// Bolumun toplanan turune gore dogru nesneyi kurar. Yeni tur = yeni sinif + buraya bir satir.
public static class Pickups
{
    public static IPickup Create(Collectible kind, Transform parent, Vector3 localPos, int seed)
    {
        switch (kind)
        {
            case Collectible.EnergyCell: return EnergyCell.Create(parent, localPos, seed);
            case Collectible.PanelPart: return SolarPanel.Create(parent, localPos, 0, seed);
            default: return Ice.Create(parent, localPos, seed);
        }
    }
}

// Toplanabilirlerin ortak efektleri: yerde genisleyen halka, kisa sicrayip kuculerek kaybolma.
public static class PickupFx
{
    // Yerde duran, isik halkasi icin kare (Ring cizimi). Baslangicta gizli.
    public static Transform AddRing(Transform parent, Color color, out Material mat)
    {
        mat = Mats.Custom("Ring", "MarsKod/Ring");
        mat.SetColor("_Color", color);
        var ring = Parts.Add("Ring", parent, MeshFactory.Quad(new Vector2(0.8f, 0.8f)), mat, new Vector3(0, 0.008f, 0), outline: false, castShadow: false);
        ring.gameObject.SetActive(false);
        return ring;
    }

    // Halka icten disa yayilip soner
    public static IEnumerator RingPulse(Transform ring, Material mat)
    {
        ring.gameObject.SetActive(true);
        yield return Tween.Run(0.5f, t =>
        {
            float e = Tween.OutCubic(t);
            mat.SetFloat("_R", Mathf.Lerp(0.2f, 0.95f, e));
            mat.SetFloat("_W", Mathf.Lerp(0.05f, 0.09f, e));
            mat.SetFloat("_A", 0.9f * (1f - t));
        });
        ring.gameObject.SetActive(false);
    }

    // Nesne bir an buyur, sonra yukari kalkarken kuculup kaybolur
    public static IEnumerator PopAway(Transform body, float grow, float rise)
    {
        yield return Tween.Run(0.1f, t => body.localScale = Vector3.one * Mathf.Lerp(1f, grow, Tween.OutCubic(t)));
        yield return Tween.Run(0.22f, t =>
        {
            body.localScale = Vector3.one * Mathf.Lerp(grow, 0f, Tween.InCubic(t));
            body.localPosition = new Vector3(0, rise * Tween.OutCubic(t), 0);
        });
        body.gameObject.SetActive(false);
    }
}
