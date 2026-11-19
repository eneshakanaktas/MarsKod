using System;
using UnityEngine;

// Ruzgarda dalgalanan zincir: ucgen flama (direk bayragi) ya da cizgili ruzgar corabi. Birbirine bagli birkac parca; her parca
// bir oncekinin ucundan doner, dalga direkten uca dogru ilerler. Zincir +x yonune uzanir (ruzgar yonu = ebeveynin donusu).
// Canli cevre (AmbientLife) her karede Tick ile oynatir; animasyon kapaliyken yerinde durur.
public sealed class Pennant : IAmbientPiece
{
    readonly Transform[] hinges;
    readonly float phase, sag;

    Pennant(Transform[] hinges, float phase, float sag)
    {
        this.hinges = hinges;
        this.phase = phase;
        this.sag = sag;
    }

    /// <summary>Ucgen flama. size: boy (x) ve direkteki yukseklik (y); flama uca dogru sivrilir</summary>
    public static Pennant Create(Transform parent, Vector3 localPos, Vector2 size, Material mat, int segments = 4, float phase = 0f)
    {
        float step = size.x / segments;
        var hinges = Chain(parent, localPos, segments, step, (hinge, k) =>
        {
            float h = size.y * (1f - (k + 0.5f) / segments * 0.85f);   // ucgen: uca dogru alcalir
            Parts.Add("Kumas", hinge, MeshFactory.RoundedBox(new Vector3(step * 1.04f, h, 0.008f), 0.003f, 2), mat,
                new Vector3(step * 0.5f, 0f, 0f), outline: false);
        });
        return new Pennant(hinges, phase, 6f);
    }

    /// <summary>Ruzgar corabi: uca dogru daralan, iki renk seritli hortum. Ruzgar azken asagi sarkar</summary>
    public static Pennant CreateSock(Transform parent, Vector3 localPos, float length, float radius, Material a, Material b, int segments = 5, float phase = 0f)
    {
        float step = length / segments;
        var hinges = Chain(parent, localPos, segments, step, (hinge, k) =>
        {
            float r = radius * (1f - k / (float)segments * 0.45f);
            var piece = Parts.Add("Serit", hinge, MeshFactory.RoundedCylinder(r, step * 1.02f, r * 0.15f, 12), k % 2 == 0 ? a : b,
                new Vector3(step * 0.5f, 0f, 0f), outline: false);
            piece.localRotation = Quaternion.Euler(0f, 0f, 90f);   // silindir boyuna yatar
        });
        return new Pennant(hinges, phase, 22f);
    }

    static Transform[] Chain(Transform parent, Vector3 localPos, int segments, float step, Action<Transform, int> addSegment)
    {
        var hinges = new Transform[segments];
        var at = parent;
        for (int k = 0; k < segments; k++)
        {
            var hinge = Parts.Empty("Zincir-" + k, at);
            hinge.localPosition = k == 0 ? localPos : new Vector3(step, 0f, 0f);
            addSegment(hinge, k);
            hinges[k] = hinge;
            at = hinge;
        }
        return hinges;
    }

    /// <summary>gust: 0 durgun hava (az oynar, uc sarkar), 1 sert ruzgar (hizli ve genis dalga)</summary>
    public void Tick(float time, float dt, float density, float gust)
    {
        float speed = 5f + 6f * gust, amp = 9f + 16f * gust;
        float droop = (1f - gust) * sag;
        for (int k = 0; k < hinges.Length; k++)
        {
            float yaw = Mathf.Sin(time * speed - k * 0.95f + phase) * amp * (0.5f + 0.25f * k);
            hinges[k].localRotation = Quaternion.Euler(0f, yaw, -droop * (k == 0 ? 1f : 0.5f));
        }
    }
}
