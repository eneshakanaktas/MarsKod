using UnityEngine;

// Hedefi haritada yazmayan bolumlerde (Bolge 7 izi, Bolge 8 notu) yolun sonuna varilinca beliren hedef isareti:
// belirir ve hemen "varildi" der. Tekrar / yeniden denemede kaldirilir.
public sealed class ArrivalMark
{
    readonly Transform root;
    Target target;

    public ArrivalMark(Transform root) { this.root = root; }

    public void Show(Vector3 localPos)
    {
        if (target != null) return;
        target = Target.Create(root, localPos);
        target.Reach();
    }

    public void Clear()
    {
        if (target != null) Object.Destroy(target.gameObject);
        target = null;
    }
}
