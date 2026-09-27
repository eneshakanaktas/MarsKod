using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tombul oyuncak gezgin. Ileri yonu +Z. Olculer bir kare = 1 birim.
public class Robot : MonoBehaviour
{
    const float WheelR = 0.105f;

    Transform rig, upper, head, eyeL, eyeR;
    readonly List<Transform> wheels = new List<Transform>();
    Material ball;
    Color ballEmission;
    Vector3 eyeScale, eyePosL, eyePosR;
    float wheelAngle, nextBlink = 2f, blinkT = -1f;
    float nod, headYaw, headYawTarget;
    bool happy;

    public float Yaw { get; private set; }

    public static Robot Create(Transform parent)
    {
        var go = new GameObject("Robot");
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one * 1.4f;
        var r = go.AddComponent<Robot>();
        r.Build();
        return r;
    }

    void Build()
    {
        var white = Mats.Emissive(Mats.Hex("#F4EFE8"), Mats.Hex("#F4EFE8") * 0.16f, 0.5f); // loş sahnede grilesmesin
        var dark = Mats.Lit(Mats.Hex("#2E2A36"), 0.3f);
        var orange = Mats.Lit(Mats.Hex("#E07A5F"), 0.4f);
        var face = Mats.Lit(Mats.Hex("#16141C"), 0.85f);
        var eye = Mats.Unlit(Mats.Hex("#A4ECF4"));
        ballEmission = Mats.Hex("#F07A5A") * 0.8f;
        ball = Mats.Emissive(Mats.Hex("#F0907A"), ballEmission, 0.5f);

        rig = Parts.Empty("Rig", transform);

        var tire = MeshFactory.RoundedCylinder(WheelR, 0.085f, 0.03f);
        var hub = MeshFactory.RoundedCylinder(0.042f, 0.02f, 0.008f);
        foreach (var (x, z) in new[] { (-0.265f, 0.15f), (0.265f, 0.15f), (-0.265f, -0.15f), (0.265f, -0.15f) })
        {
            var pivot = Parts.Empty("Wheel", rig);
            pivot.localPosition = new Vector3(x, WheelR, z);
            Parts.Add("Tire", pivot, tire, dark, Vector3.zero).localRotation = Quaternion.Euler(0, 0, 90);
            Parts.Add("Hub", pivot, hub, orange, new Vector3(Mathf.Sign(x) * 0.046f, 0, 0), outline: false).localRotation = Quaternion.Euler(0, 0, 90);
            wheels.Add(pivot);
        }

        Parts.Add("Chassis", rig, MeshFactory.RoundedBox(new Vector3(0.44f, 0.1f, 0.42f), 0.04f), dark, new Vector3(0, 0.16f, 0));

        upper = Parts.Empty("Upper", rig);
        Parts.Add("Body", upper, MeshFactory.RoundedBox(new Vector3(0.46f, 0.25f, 0.42f), 0.1f), white, new Vector3(0, 0.31f, 0));
        Parts.Add("Stripe", upper, MeshFactory.RoundedBox(new Vector3(0.472f, 0.045f, 0.432f), 0.02f), orange, new Vector3(0, 0.255f, 0), outline: false);
        Parts.Add("Neck", upper, MeshFactory.RoundedCylinder(0.045f, 0.08f, 0.015f), dark, new Vector3(0, 0.46f, 0));

        head = Parts.Empty("Head", upper);
        head.localPosition = new Vector3(0, 0.47f, 0);
        Parts.Add("Skull", head, MeshFactory.RoundedBox(new Vector3(0.36f, 0.23f, 0.27f), 0.09f), white, new Vector3(0, 0.13f, 0.02f));
        float faceZ = 0.02f + 0.135f - 0.006f;
        Parts.Add("Face", head, MeshFactory.RoundedBox(new Vector3(0.28f, 0.15f, 0.03f), 0.05f), face, new Vector3(0, 0.13f, faceZ), outline: false);
        var eyeMesh = MeshFactory.RoundedBox(new Vector3(0.05f, 0.075f, 0.02f), 0.024f);
        eyePosL = new Vector3(-0.06f, 0.135f, faceZ + 0.012f);
        eyePosR = new Vector3(0.06f, 0.135f, faceZ + 0.012f);
        eyeL = Parts.Add("EyeL", head, eyeMesh, eye, eyePosL, outline: false, castShadow: false);
        eyeR = Parts.Add("EyeR", head, eyeMesh, eye, eyePosR, outline: false, castShadow: false);
        eyeScale = Vector3.one;

        Parts.Add("Antenna", head, MeshFactory.RoundedCylinder(0.012f, 0.13f, 0.005f, 12), dark, new Vector3(0.1f, 0.3f, -0.02f));
        Parts.Add("Ball", head, MeshFactory.Sphere(0.032f, 10, 16), ball, new Vector3(0.1f, 0.38f, -0.02f));
    }

    public void ResetTo(Vector3 localPos, float yaw)
    {
        transform.localPosition = localPos;
        SetYaw(yaw);
        rig.localPosition = Vector3.zero;
        rig.localRotation = Quaternion.identity;
        rig.localScale = Vector3.one;
        nod = 0f; headYaw = headYawTarget = 0f;
        head.localRotation = Quaternion.identity;
        happy = false;
        SetEyes(1f, 0f);
    }

    void SetYaw(float y) { Yaw = y; transform.localRotation = Quaternion.Euler(0, y, 0); }

    void SetEyes(float open, float lift)
    {
        var s = new Vector3(eyeScale.x, eyeScale.y * open, eyeScale.z);
        eyeL.localScale = s; eyeR.localScale = s;
        eyeL.localPosition = eyePosL + Vector3.up * lift;
        eyeR.localPosition = eyePosR + Vector3.up * lift;
    }

    void Update()
    {
        // zemin cizimi robotun onune dusen isigi buradan okur
        Shader.SetGlobalVector("_RobotPos", transform.position);
        // isik farlar gibi govdeye bagli: bas donse de her zaman gidis yonune vurur
        Shader.SetGlobalVector("_RobotFwd", transform.forward);

        float t = Time.time;
        upper.localPosition = new Vector3(0, 0.006f * Mathf.Sin(t * 2.2f), 0);
        ball.SetColor("_EmissionColor", ballEmission * (0.7f + 0.3f * Mathf.Sin(t * 2.6f)));
        headYaw = Mathf.MoveTowards(headYaw, headYawTarget, Time.deltaTime * 120f);
        head.localRotation = Quaternion.Euler(nod, headYaw, 0f);

        if (happy) return;
        if (blinkT < 0f && t > nextBlink) blinkT = 0f;
        if (blinkT >= 0f)
        {
            blinkT += Time.deltaTime;
            float k = blinkT < 0.07f ? 1f - blinkT / 0.07f : Mathf.Clamp01((blinkT - 0.07f) / 0.09f);
            SetEyes(Mathf.Max(0.1f, k), 0f);
            if (blinkT > 0.16f) { blinkT = -1f; nextBlink = t + Random.Range(2.2f, 4.8f); SetEyes(1f, 0f); }
        }
    }

    public IEnumerator TurnTo(float yaw, float dur = 0.3f)
    {
        float from = Yaw;
        yield return Tween.Run(dur, t => SetYaw(Mathf.LerpAngle(from, yaw, Tween.InOutCubic(t))));
    }

    public IEnumerator MoveTo(Vector3 target, float dur = 0.55f)
    {
        Vector3 from = transform.localPosition;
        float dist = Vector3.Distance(from, target);
        float startAngle = wheelAngle;
        yield return Tween.Run(dur, t =>
        {
            float e = Tween.InOutCubic(t);
            transform.localPosition = Vector3.Lerp(from, target, e);
            // kalkista hafif geriye, durusta hafif one egilme
            rig.localRotation = Quaternion.Euler(-4.5f * Mathf.Sin(t * Mathf.PI * 2f), 0, 0);
            float a = startAngle + dist * e / WheelR * Mathf.Rad2Deg;
            foreach (var w in wheels) w.localRotation = Quaternion.Euler(a, 0, 0);
        });
        wheelAngle = startAngle + dist / WheelR * Mathf.Rad2Deg;
        rig.localRotation = Quaternion.identity;
    }

    // Ilerleyemedi (alanin siniri): biraz one atilir, geri sekip sallanir.
    public IEnumerator Bump()
    {
        Vector3 from = transform.localPosition;
        Vector3 fwd = transform.localRotation * Vector3.forward;
        yield return Tween.Run(0.16f, t => transform.localPosition = from + fwd * 0.16f * Tween.InOutCubic(t));
        yield return Tween.Run(0.36f, t =>
        {
            transform.localPosition = from + fwd * 0.16f * (1f - Tween.InOutCubic(t));
            rig.localRotation = Quaternion.Euler(0, 0, 7f * Mathf.Sin(t * Mathf.PI * 4f) * (1f - t));
        });
        transform.localPosition = from;
        rig.localRotation = Quaternion.identity;
    }

    public IEnumerator Collect()
    {
        yield return Tween.Run(0.32f, t => nod = 16f * Mathf.Sin(t * Mathf.PI));
        nod = 0f;
    }

    // Surerken basini kameraya dogru hafifce cevirir (yuzu gorunsun).
    public void GlanceAtCamera(bool on) => headYawTarget = on ? 32f : 0f;

    public IEnumerator Celebrate()
    {
        GlanceAtCamera(false);
        yield return TurnTo(180f, 0.4f);
        happy = true;
        yield return Tween.Run(0.12f, t => SetEyes(Mathf.Lerp(1f, 0.42f, t), Mathf.Lerp(0f, 0.012f, t)));
        for (int i = 0; i < 2; i++)
        {
            yield return Tween.Run(0.34f, t => rig.localPosition = new Vector3(0, 4f * t * (1f - t) * 0.17f, 0));
            yield return Tween.Run(0.16f, t =>
            {
                float s = Mathf.Sin(t * Mathf.PI);
                rig.localScale = new Vector3(1f + 0.06f * s, 1f - 0.1f * s, 1f + 0.06f * s);
            });
        }
        rig.localScale = Vector3.one;
    }
}
