using System;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;

// Bolge 10 (gece ovasi) tehlikeleri, GECICI gorunus (asil hali 3a'da): meteorun yeni dustugu krater (U, girilmez) ve dumani
// tuten kizgin goktasi (X, toplanmaz). Kraterler her denemede baska yerde olabilir ("duzenler"): Show o denemenin kraterlerini kurar.
public sealed class MeteorMarks
{
    static readonly Color Rim = Mats.Hex("#2A2030");
    // Renkler Bolge 10 paletinden (docs/superpowers/plans/2026-11-15-bolge-9-10-gorunus.md): kor akkor-fusya (turuncu Defne'nin
    // oklarina ayrili), tas grafit
    static readonly Color Ember = Mats.Hex("#FF3FA4");
    static readonly Color StoneDark = Mats.Hex("#2B2B2E");

    readonly Transform root;
    readonly Func<Cell, Vector3> pos;

    public MeteorMarks(Transform parent, Func<Cell, Vector3> pos)
    {
        root = Parts.Empty("Kraterler", parent);
        this.pos = pos;
    }

    /// <summary>Bu denemenin kraterlerini kurar (oncekiler silinir)</summary>
    public void Show(IEnumerable<Cell> craters)
    {
        for (int i = root.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(root.GetChild(i).gameObject);
        int seed = 0;
        foreach (var cell in craters) Crater(root, pos(cell), seed++);
    }

    // Krater: yere yassi koyu bir halka (kenari kalkik toprak) + ortasinda kor gibi parlayan cukur
    static void Crater(Transform parent, Vector3 p, int seed)
    {
        var crater = Parts.Empty("Krater", parent);
        crater.localPosition = p;
        var rim = Parts.Add("Krater-Kenar", crater, MeshFactory.Rock(0.38f, 700 + seed), Mats.Lit(Rim, 0.1f), new Vector3(0f, -0.03f, 0f));
        rim.localScale = new Vector3(1f, 0.2f, 1f);
        Parts.Add("Krater-Dip", crater, MeshFactory.RoundedCylinder(0.24f, 0.02f, 0.008f), Mats.Lit(Mats.Hex("#140A18"), 0.05f), new Vector3(0f, 0.035f, 0f),
            outline: false, castShadow: false);
        Parts.Add("Krater-Kor", crater, MeshFactory.RoundedCylinder(0.11f, 0.02f, 0.008f), Mats.Emissive(Ember * 0.25f, Ember * 0.75f), new Vector3(0f, 0.045f, 0f),
            outline: false, castShadow: false);
    }

    /// <summary>Kizgin goktasi: koyu tas, catlaklarindan akkor-fusya isik (haritada X, her denemede ayni)</summary>
    public static void HotStone(Transform parent, Vector3 p, int seed)
    {
        var stone = Parts.Empty("KizginTas", parent);
        stone.localPosition = p;
        Parts.Add("KizginTas-Govde", stone, MeshFactory.Rock(0.16f, 800 + seed), Mats.Lit(StoneDark, 0.2f), new Vector3(0f, 0.06f, 0f));
        Parts.Add("KizginTas-Kor", stone, MeshFactory.Rock(0.1f, 900 + seed), Mats.Emissive(Ember * 0.5f, Ember * 1.4f), new Vector3(0.04f, 0.1f, 0.02f),
            outline: false, castShadow: false);
    }
}
