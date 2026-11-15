using System.Collections.Generic;
using UnityEngine;

// Duz boyali sekiller (arma, kagittaki cizim): yerel XY duzleminde dis hat ve ondan yelpaze orgu. Orgu -Z'ye bakar.
public static class FlatShapes
{
    // Bes koseli yildiz: tepeden baslar, saat yonunde (-Z'den bakana gore on yuz)
    public static List<Vector2> Star(Vector2 center, float outer, float inner)
    {
        var rim = new List<Vector2>();
        for (int k = 0; k < 10; k++)
        {
            float a = Mathf.PI * 0.5f - k * Mathf.PI / 5f;
            rim.Add(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (k % 2 == 0 ? outer : inner));
        }
        return rim;
    }

    // Dis hattin ortasindan yelpaze orgu; rim saat yonunde olmali
    public static Mesh Fan(string name, List<Vector2> rim)
    {
        var center = Vector2.zero;
        foreach (var p in rim) center += p;
        center /= rim.Count;
        var verts = new List<Vector3> { center };
        foreach (var p in rim) verts.Add(p);
        var tris = new List<int>();
        for (int k = 0; k < rim.Count; k++) tris.AddRange(new[] { 0, 1 + k, 1 + (k + 1) % rim.Count });
        var normals = new Vector3[verts.Count];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.back;
        var mesh = new Mesh { name = name };
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
