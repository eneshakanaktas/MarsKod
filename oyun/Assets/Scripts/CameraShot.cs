using System.Collections;
using UnityEngine;

// Sahne kamerasi (Bolge 6 sahneleri, spec 3b.1): sahne surerken kamerayi Oyun.FitCamera yerine bu yonetir.
// Gokyuzu resmi (MarsSky.hlsl) ekranda _Horizon/_Picture ile yerlesir; kamera donunce resmin ufku da gercek ufukla
// birlikte kaysin diye her karede yeniden hesaplanir. Begin normal kameranin halini saklar, Release geri verir.
public class CameraShot
{
    static readonly int HorizonId = Shader.PropertyToID("_Horizon");
    static readonly int PictureId = Shader.PropertyToID("_Picture");

    readonly Camera cam;
    Vector3 position;
    Quaternion rotation;
    float fov, shift, horizonOffset, swayTime;

    public bool Active { get; private set; }
    // Yukaridan bakista (55) ucan bir seyden bakiyormus gibi hafif salinim (derece)
    public float Sway { get; set; }

    public CameraShot(Camera cam) { this.cam = cam; }

    // Normal kameranin o anki halini alir: sahne buradan baslar, ufuk farki burada olculur.
    public void Begin()
    {
        if (Active) return;
        position = cam.transform.position;
        rotation = cam.transform.rotation;
        fov = cam.fieldOfView;
        var picture = Shader.GetGlobalVector(PictureId);
        shift = picture.y;
        float screenHorizon = ((2f * Shader.GetGlobalFloat(HorizonId) - 1f) * picture.x + picture.y + 1f) * 0.5f;
        horizonOffset = screenHorizon - GeometricHorizon();
        Sway = 0f;
        Active = true;
    }

    public IEnumerator MoveTo(Vector3 to, Vector3 lookAt, float seconds)
    {
        Vector3 fromPos = position;
        Quaternion fromRot = rotation, toRot = Quaternion.LookRotation(lookAt - to);
        float fromShift = shift, fromOffset = horizonOffset;
        yield return Tween.Run(seconds, t =>
        {
            float e = Tween.InOutCubic(t);
            position = Vector3.Lerp(fromPos, to, e);
            rotation = Quaternion.Slerp(fromRot, toRot, e);
            shift = Mathf.Lerp(fromShift, 0f, e);
            horizonOffset = Mathf.Lerp(fromOffset, 0f, e);   // varista gokyuzunun ufku gercek ufuk olur
        });
    }

    // Ani gecis: kamera yeni yerde, gokyuzunun ufku gercek ufukla bir (offset yok). Yoksa asagi bakista zemin tamamen
    // gokyuzune karisir (normal bakisin sahte ufku ekranin altinda kalir).
    public void Cut(Vector3 to, Quaternion rot, float fieldOfView = 0f)
    {
        position = to; rotation = rot; shift = 0f; horizonOffset = 0f;
        if (fieldOfView > 0f) fov = fieldOfView;   // sahneye ozel gorus acisi (verilmezse normal kameranin acisi)
    }

    // Oyun.LateUpdate her karede cagirir (Active iken FitCamera yerine)
    public void Apply()
    {
        swayTime += Time.deltaTime;
        var sway = Quaternion.Euler(Sway * Mathf.Sin(swayTime * 1.7f), 0f, Sway * 0.7f * Mathf.Sin(swayTime * 1.1f + 1f));
        cam.transform.SetPositionAndRotation(position, rotation * sway);
        cam.fieldOfView = fov;
        cam.ResetProjectionMatrix();
        var p = cam.projectionMatrix;
        p[1, 2] = -shift;
        cam.projectionMatrix = p;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 80f;
        float screenHorizon = GeometricHorizon() + horizonOffset;
        Shader.SetGlobalVector(PictureId, new Vector4(1f, shift, 0f, 0f));
        Shader.SetGlobalFloat(HorizonId, ((2f * screenHorizon - 1f - shift) + 1f) * 0.5f);
    }

    public void Release() { Active = false; Sway = 0f; }

    // Kameranin yuksekliginde, tam onunde cok uzaktaki noktanin ekrandaki yuksekligi: duz zeminin gercek ufku
    float GeometricHorizon()
    {
        var flat = cam.transform.forward; flat.y = 0f;
        if (flat.sqrMagnitude < 1e-4f) flat = Vector3.forward;   // tam asagi bakis (55): ufuk ekranin disinda
        return cam.WorldToViewportPoint(cam.transform.position + flat.normalized * 1000f).y;
    }
}
