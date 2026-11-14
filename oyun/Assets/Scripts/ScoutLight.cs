using UnityEngine;

// scout() ani (Bolge 7): Serce yukarida gunesin icinde; asagidaki golgeye yansittigi sicak isik yol ayriminin karesinden secilen
// kola dogru uzanan yumusak, kenari daginik bir leke olarak yere duser. ~0,15 sn'de belirir, ~0,35 sn durur, ~0,3 sn'de soner;
// sonra kendini siler (bolum degisse de kendiliginden biter). Renk gunes rengi: turkuaz yalniz Ece'nin (K1).
public class ScoutLight : MonoBehaviour
{
    public static readonly Color Sun = Mats.Hex("#FFE3A8");
    const float In = 0.15f, Hold = 0.35f, Out = 0.3f;
    const float Strength = 1.2f;

    Material mat;
    float age;

    // cellCenter: yol ayriminin karesi; towards: secilen koldaki komsu kare (ikisi de parent'in yerel konumu)
    public static ScoutLight Show(Transform parent, Vector3 cellCenter, Vector3 towards)
    {
        var dir = towards - cellCenter;
        dir.y = 0f;
        float reach = dir.magnitude;
        var mat = LightPool.Additive(Sun);
        // leke karenin ortasindan kolun komsu karesine kadar uzanir: merkezi ikisinin arasinda, boyu ~1,7 kare, eni ~0,95 kare
        var at = cellCenter + dir * 0.45f + Vector3.up * 0.012f;
        var quad = Parts.Add("SerceIsigi", parent, MeshFactory.Quad(new Vector2(0.95f, reach * 1.7f)), mat, at, outline: false, castShadow: false);
        quad.localRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        var light = quad.gameObject.AddComponent<ScoutLight>();
        light.mat = mat;
        return light;
    }

    void Update()
    {
        age += Time.deltaTime;
        float a = age < In ? age / In : age < In + Hold ? 1f : 1f - (age - In - Hold) / Out;
        mat.SetFloat("_A", Strength * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(a)));
        if (age >= In + Hold + Out) Destroy(gameObject);
    }
}
