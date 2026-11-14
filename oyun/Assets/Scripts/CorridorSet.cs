using System;
using System.Collections.Generic;
using UnityEngine;

// Bolum 80 kapanisinin hatira kesmesi (Bolge 8 · 3b, K1): kirpismanin en karanlik aninda 80'in odasi ve alan esyalari (bolumun koku)
// gizlenir, ayni yerde 75'in koridoru kurulur (yerlesimi StationTraces.ShowCorridor secer). Hide koridoru siler, 80'i geri acar.
// Onceden kurulmaz: 80 acilirken telefonu yormasin. Robot bolumun kokunun disinda (gizlenmez).
public sealed class CorridorSet
{
    readonly Transform root, level;

    // Cizimlerin ortalari (bolumun yerel konumu); sonuncusu robot ve kiz el ele
    public IReadOnlyList<Vector3> Drawings { get; }
    public Vector3 LastDrawing => Drawings[Drawings.Count - 1];

    CorridorSet(Transform root, Transform level, IReadOnlyList<Vector3> drawings)
    {
        this.root = root;
        this.level = level;
        Drawings = drawings;
    }

    // build: koridoru verilen kokun altina kurar, cizimlerin ortalarini dondurur
    public static CorridorSet Show(Transform levelRoot, Func<Transform, IReadOnlyList<Vector3>> build)
    {
        levelRoot.gameObject.SetActive(false);
        var root = Parts.Empty("Koridor-Hatirasi", levelRoot.parent);
        root.localPosition = levelRoot.localPosition;
        root.localRotation = levelRoot.localRotation;
        return new CorridorSet(root, levelRoot, build(root));
    }

    public bool Shown { get; private set; } = true;

    public void Hide()
    {
        if (!Shown) return;
        Shown = false;
        if (root != null) UnityEngine.Object.Destroy(root.gameObject);
        if (level != null) level.gameObject.SetActive(true);
    }
}
