using System.Collections;
using UnityEngine;

// Robotun ice_here() taramasi: robotun altinda genisleyen kisa bir halka (buz bulduysa buz mavisi, bulamadiysa soluk beyaz).
public static class ScanPulse
{
    public static IEnumerator Play(Transform parent, Vector3 localPos, bool found)
    {
        var mat = Mats.Custom("Ring", "MarsKod/Ring");
        mat.SetColor("_Color", found ? Mats.Hex("#8FE3FF") : new Color(0.85f, 0.85f, 0.92f));
        var ring = Parts.Add("ScanRing", parent, MeshFactory.Quad(new Vector2(0.9f, 0.9f)), mat, localPos + new Vector3(0f, 0.012f, 0f), outline: false, castShadow: false);
        yield return Tween.Run(0.45f, t =>
        {
            mat.SetFloat("_R", Mathf.Lerp(0.1f, 0.42f, Tween.OutCubic(t)));
            mat.SetFloat("_W", 0.05f);
            mat.SetFloat("_A", 0.8f * (1f - t));
        });
        Object.Destroy(ring.gameObject);
        Object.Destroy(mat);
    }
}
