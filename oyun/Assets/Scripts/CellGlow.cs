using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Duzen gecisinden sonra icerigi degisen kareler kisa sure parlar (A1): her karede yerde iki kez yanip sonen bir isik halkasi.
// Oyuncu "su kareler degisti" diye hemen gorur. Kendi nesnesi uzerinde calisir ve bitince kendini siler (bolum degisirse
// bolumle birlikte silinir).
public class CellGlow : MonoBehaviour
{
    const float Seconds = 1.6f, Pulses = 2f;

    readonly List<Material> mats = new List<Material>();

    public static void Play(Transform parent, IEnumerable<Vector3> cells, Color color)
    {
        var go = new GameObject("Degisen-Kareler");
        go.transform.SetParent(parent, false);
        var glow = go.AddComponent<CellGlow>();
        var quad = MeshFactory.Quad(new Vector2(0.95f, 0.95f));
        foreach (var p in cells)
        {
            var mat = Mats.Custom("Ring", "MarsKod/Ring");
            mat.SetColor("_Color", color);
            mat.SetFloat("_R", 0.78f);
            mat.SetFloat("_W", 0.14f);
            mat.SetFloat("_A", 0f);
            Parts.Add("Parilti", go.transform, quad, mat, p + new Vector3(0f, 0.012f, 0f), outline: false, castShadow: false);
            glow.mats.Add(mat);
        }
        glow.StartCoroutine(glow.Animate());
    }

    IEnumerator Animate()
    {
        yield return Tween.Run(Seconds, t =>
        {
            float a = Mathf.Sin(t * Pulses * Mathf.PI);
            a = 0.9f * a * a;   // iki yumusak parlama, aralarinda soner
            foreach (var m in mats) m.SetFloat("_A", a);
        });
        Destroy(gameObject);
    }
}
