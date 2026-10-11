using System.Collections.Generic;
using UnityEngine;

// Duz yuzeyli (her ucgenin kendi normali, alcak poligon kaya gorunumu) basit ag kurucu. Her koseye bir golgelenme degeri
// (0 kapali, 1 acik gok) yazilabilir: koseli rengin kirmizisinda durur, FarRock bunu ortam isigina uygular (_VertexAO).
public class FlatMesh
{
    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<Vector3> normals = new List<Vector3>();
    readonly List<Color> colors = new List<Color>();
    readonly List<int> triangles = new List<int>();

    public bool IsEmpty => triangles.Count == 0;

    // a-b-c-d dortgeni; outward: yuzun bakmasi gereken yon (sarmal buna gore duzeltilir); ao: dort kosenin golgelenmesi
    public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward, Vector4 ao)
    {
        Triangle(a, b, c, outward, ao.x, ao.y, ao.z);
        Triangle(a, c, d, outward, ao.x, ao.z, ao.w);
    }

    public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward, float ao = 1f) =>
        Quad(a, b, c, d, outward, new Vector4(ao, ao, ao, ao));

    public void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 outward, float aoA, float aoB, float aoC)
    {
        var normal = Vector3.Cross(b - a, c - a);
        if (normal.sqrMagnitude < 1e-12f) return;
        if (Vector3.Dot(normal, outward) < 0f)
        {
            (b, c) = (c, b);
            (aoB, aoC) = (aoC, aoB);
            normal = -normal;
        }
        normal.Normalize();
        int i = vertices.Count;
        vertices.Add(a); vertices.Add(b); vertices.Add(c);
        normals.Add(normal); normals.Add(normal); normals.Add(normal);
        colors.Add(Shade(aoA)); colors.Add(Shade(aoB)); colors.Add(Shade(aoC));
        triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
    }

    static Color Shade(float ao) => new Color(ao, ao, ao, 1f);

    public Mesh ToMesh(string name)
    {
        var m = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        m.SetVertices(vertices);
        m.SetNormals(normals);
        m.SetColors(colors);
        m.SetTriangles(triangles, 0);
        m.RecalculateBounds();
        return m;
    }

    // Hazir bir aga (ornegin MeshFactory.Rock) tek bir golgelenme degeri yazar: derindeki kaya duvarlar gibi kararsin
    public static Mesh WithShade(Mesh mesh, float ao)
    {
        var c = new Color[mesh.vertexCount];
        for (int i = 0; i < c.Length; i++) c[i] = Shade(ao);
        mesh.colors = c;
        return mesh;
    }
}
