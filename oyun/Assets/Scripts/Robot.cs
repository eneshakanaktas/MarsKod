using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RobotModel { Spark, Rover, Toy }

// Oyuncunun robotu: hareket ve ortak davranis burada, gorunum alt siniflarda (SparkBot, ColonyRover, ToyRobot).
// Ileri yonu +Z. Olculer bir kare = 1 birim.
public abstract class Robot : MonoBehaviour
{
    // Kullanilan robot: Kivilcim (SparkBot). Digerlerine donmek icin burayi degistirmek yeter
    // (deneme: -robot gezgin = Enes'in koloni gezgini, -robot oyuncak = ilk robot).
    public const RobotModel DefaultModel = RobotModel.Spark;

    protected Transform rig;
    protected readonly List<Transform> wheels = new List<Transform>();
    float wheelAngle;

    public float Yaw { get; private set; }

    protected abstract float WheelRadius { get; }
    // kalkista geriye, durusta one egilme (derece)
    protected virtual float MoveTilt => 4.5f;
    // modelin boyu (bir kare = 1 birim)
    protected virtual float ModelScale => 1.4f;
    // onundeki zemine dusen isigin rengi (Ground.shader)
    protected abstract Color BeamColor { get; }

    public static Robot Create(Transform parent, RobotModel model)
    {
        var go = new GameObject("Robot");
        go.transform.SetParent(parent, false);
        Robot r = model == RobotModel.Toy ? go.AddComponent<ToyRobot>()
            : model == RobotModel.Rover ? go.AddComponent<ColonyRover>()
            : (Robot)go.AddComponent<SparkBot>();
        go.transform.localScale = Vector3.one * r.ModelScale;
        r.rig = Parts.Empty("Rig", go.transform);
        r.Build();
        return r;
    }

    protected abstract void Build();
    protected virtual void OnReset() { }
    protected virtual void Animate(float time) { }

    public void ResetTo(Vector3 localPos, float yaw)
    {
        StopAllCoroutines();   // yarim kalan tepki (ShowPuzzled) bolum yeniden baslayinca biter
        transform.localPosition = localPos;
        SetYaw(yaw);
        rig.localPosition = Vector3.zero;
        rig.localRotation = Quaternion.identity;
        rig.localScale = Vector3.one;
        OnReset();
    }

    void SetYaw(float y) { Yaw = y; transform.localRotation = Quaternion.Euler(0, y, 0); }

    void Update()
    {
        // zemin cizimi robotun onune dusen isigi buradan okur
        Shader.SetGlobalVector("_RobotPos", transform.position);
        // isik farlar gibi govdeye bagli: bas donse de her zaman gidis yonune vurur
        Shader.SetGlobalVector("_RobotFwd", transform.forward);
        Shader.SetGlobalVector("_RobotBeam", BeamColor);
        Animate(Time.time);
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
            rig.localRotation = Quaternion.Euler(-MoveTilt * Mathf.Sin(t * Mathf.PI * 2f), 0, 0);
            float a = startAngle + dist * e / WheelRadius * Mathf.Rad2Deg;
            foreach (var w in wheels) w.localRotation = Quaternion.Euler(a, 0, 0);
        });
        wheelAngle = startAngle + dist / WheelRadius * Mathf.Rad2Deg;
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

    public abstract IEnumerator Collect();
    // Surerken basini (kamerasini) kameraya dogru hafifce cevirir.
    public abstract void GlanceAtCamera(bool on);
    public abstract IEnumerator Celebrate();

    // Kod hatayla durdu ya da gorev bitmedi: kisa bir saskinlik tepkisi (gorunum isterse; varsayilan yok)
    protected virtual IEnumerator Puzzled() { yield break; }

    // Tepkiyi kendi basina oynatir (oyunun akisini bekletmez)
    public void ShowPuzzled()
    {
        StopAllCoroutines();
        StartCoroutine(Puzzled());
    }
}
