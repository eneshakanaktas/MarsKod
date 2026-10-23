using System;
using System.Collections;
using UnityEngine;

// Sahne kamerasi (Bolge 6 sahneleri, spec 3b.1): sahne surerken kamerayi Oyun.FitCamera yerine bu yonetir.
// Gokyuzu resmi (MarsSky.hlsl) ekranda _Horizon/_Picture ile yerlesir. Zemin uzakta asagi kivrildigi icin (kucuk gezegen)
// gercek ufuk zeminin ekrandaki ust siniridir; resmin ufku her karede bu sinira gore yeniden hesaplanir, kamera donunce
// gokyuzu de zeminle birlikte kayar. Begin normal kameranin halini saklar; Return oraya yumusakca doner, Release aninda.
public class CameraShot
{
    static readonly int HorizonId = Shader.PropertyToID("_Horizon");
    static readonly int PictureId = Shader.PropertyToID("_Picture");
    static readonly int SceneCameraId = Shader.PropertyToID("_SceneCamera");

    // Kameranin bir hali: yeri, yonu, gorus acisi, gokyuzu resminin yerlesimi (olcek, dikey kayma, ufuk farki) ve ne kadar
    // "sahne kamerasi" oldugu (0 normal kamera, 1 sahne; gokyuzu ve zemin buna gore yumusakca degisir, _SceneCamera)
    struct Pose
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Fov, Shift, PictureScale, HorizonOffset, Scene;

        public static Pose Lerp(Pose a, Pose b, float e) => new Pose
        {
            Position = Vector3.Lerp(a.Position, b.Position, e),
            Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, e),
            Fov = Mathf.Lerp(a.Fov, b.Fov, e),
            Shift = Mathf.Lerp(a.Shift, b.Shift, e),
            PictureScale = Mathf.Lerp(a.PictureScale, b.PictureScale, e),
            HorizonOffset = Mathf.Lerp(a.HorizonOffset, b.HorizonOffset, e),
            Scene = Mathf.Lerp(a.Scene, b.Scene, e),
        };
    }

    readonly Camera cam;
    readonly Func<float, float, float> groundHeight;
    Pose pose, normal;   // normal: Begin aninda normal kameranin hali (Return buraya doner)
    float swayTime;

    public bool Active { get; private set; }
    // Yukaridan bakista (55) ucan bir seyden bakiyormus gibi hafif salinim (derece)
    public float Sway { get; set; }

    // groundHeight: zemin yuksekligi (x, z); gercek ufuk (zeminin ekrandaki ust siniri) bundan olculur
    public CameraShot(Camera cam, Func<float, float, float> groundHeight)
    {
        this.cam = cam;
        this.groundHeight = groundHeight;
    }

    // Normal kameranin o anki halini alir: sahne buradan baslar. Gokyuzu resmi ekranda oldugu gibi kalir (ilk karede sicrama
    // yok): olcegi ve kaymasi aynen saklanir, resmin ufkunun gercek ufka uzakligi da fark olarak saklanir.
    public void Begin()
    {
        if (Active) return;
        var picture = Shader.GetGlobalVector(PictureId);
        normal = new Pose
        {
            Position = cam.transform.position,
            Rotation = cam.transform.rotation,
            Fov = cam.fieldOfView,
            Shift = picture.y,
            PictureScale = picture.x,
        };
        normal.HorizonOffset = ScreenY(Shader.GetGlobalFloat(HorizonId), normal) - GroundHorizon();
        pose = normal;
        Sway = 0f;
        Active = true;
    }

    // Kamera yumusakca yeni yere gider, lookAt'e bakar. fieldOfView > 0 ise gorus acisi da oraya gecer. Varista gokyuzu resmi
    // tam olcekte, kaymasiz ve ufku gercek ufukta (zeminin bittigi yerde) durur.
    public IEnumerator MoveTo(Vector3 to, Vector3 lookAt, float seconds, float fieldOfView = 0f)
    {
        yield return Blend(Target(to, LookRotation(to, lookAt), fieldOfView), seconds);
    }

    // Ani gecis (55'teki kirpisma): MoveTo'nun varis hali hemen
    public void Cut(Vector3 to, Quaternion rotation, float fieldOfView = 0f) => pose = Target(to, rotation, fieldOfView);

    // Normal kameraya yumusakca doner (Begin'deki hal), sonra birakir: FitCamera ayni yerden devam eder, sicrama olmaz
    public IEnumerator Return(float seconds)
    {
        yield return Blend(normal, seconds);
        Release();
    }

    // Aninda birakir ("Tekrar", bolum degisimi, 55'teki kirpisma donusu)
    public void Release()
    {
        Active = false;
        Sway = 0f;
        Shader.SetGlobalFloat(SceneCameraId, 0f);
    }

    // Oyun.LateUpdate her karede cagirir (Active iken FitCamera yerine)
    public void Apply()
    {
        swayTime += Time.deltaTime;
        var sway = Quaternion.Euler(Sway * Mathf.Sin(swayTime * 1.7f), 0f, Sway * 0.7f * Mathf.Sin(swayTime * 1.1f + 1f));
        cam.transform.SetPositionAndRotation(pose.Position, pose.Rotation * sway);
        cam.fieldOfView = pose.Fov;
        cam.ResetProjectionMatrix();
        var p = cam.projectionMatrix;
        p[1, 2] = -pose.Shift;
        cam.projectionMatrix = p;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 80f;
        float screenHorizon = GroundHorizon() + pose.HorizonOffset;
        Shader.SetGlobalVector(PictureId, new Vector4(pose.PictureScale, pose.Shift, 0f, 0f));
        Shader.SetGlobalFloat(HorizonId, PictureY(screenHorizon, pose));
        Shader.SetGlobalFloat(SceneCameraId, pose.Scene);
    }

    Pose Target(Vector3 to, Quaternion rotation, float fieldOfView) => new Pose
    {
        Position = to,
        Rotation = rotation,
        Fov = fieldOfView > 0f ? fieldOfView : pose.Fov,
        Shift = 0f,
        PictureScale = 1f,
        HorizonOffset = 0f,
        Scene = 1f,
    };

    IEnumerator Blend(Pose target, float seconds)
    {
        var from = pose;
        yield return Tween.Run(seconds, t => pose = Pose.Lerp(from, target, Tween.InOutCubic(t)));
    }

    // Kameradan lookAt'e bakis; ikisi ayni noktaysa yon degismez
    Quaternion LookRotation(Vector3 from, Vector3 lookAt)
    {
        var dir = lookAt - from;
        return dir.sqrMagnitude < 1e-8f ? pose.Rotation : Quaternion.LookRotation(dir);
    }

    // Gokyuzu resminin y'si (MarsPictureUV) <-> ekran y'si (0 alt, 1 ust): resim ekranda olcek kadar buyur, kayma kadar kayar
    static float ScreenY(float pictureY, Pose p) => ((2f * pictureY - 1f) * p.PictureScale + p.Shift + 1f) * 0.5f;
    static float PictureY(float screenY, Pose p) => ((2f * screenY - 1f - p.Shift) / Mathf.Max(p.PictureScale, 1e-3f) + 1f) * 0.5f;

    // Gercek ufuk: kameranin onunde, ekranin yukarisina dogru uzanan zemin cizgisinin ekrandaki en yuksek noktasi. Zemin uzakta
    // asagi kivrildigi icin bu, duz bir zeminin ufkundan asagidadir; zemin tam burada biter, otesi gokyuzu resmidir.
    float GroundHorizon()
    {
        var t = cam.transform;
        var ahead = t.forward + t.up; ahead.y = 0f;   // ekranin ustune dogru yer (tam asagi bakista da tanimli)
        if (ahead.sqrMagnitude < 1e-6f) ahead = Vector3.forward;
        ahead.Normalize();
        float best = float.NegativeInfinity;
        for (float d = 0.2f; d < 45f; d += 0.1f + d * 0.04f)
        {
            float x = t.position.x + ahead.x * d, z = t.position.z + ahead.z * d;
            var v = cam.WorldToViewportPoint(new Vector3(x, groundHeight(x, z), z));
            if (v.z > 0f && v.y > best) best = v.y;
        }
        return float.IsNegativeInfinity(best) ? 1.5f : best;   // zemin hic onde degilse ufuk ekranin ustunde kalir
    }
}
