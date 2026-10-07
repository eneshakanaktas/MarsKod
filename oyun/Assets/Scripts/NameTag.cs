using System.Collections;
using UnityEngine;

// Bolum 50 kapanisi: robotun basinin ustunde eski adi ("BKM-7") belirir, harf harf silinir, yerine yeni adi
// ("KIVILCIM") harf harf yazilir, bir sure kalir ve soner. Ad yalnizca bu anda gorunur (senaryo-bolge-05.md, acik soru 1).
public class NameTag : MonoBehaviour
{
    static readonly Color Ink = new Color(1f, 0.96f, 0.86f);

    TextMesh text;
    Robot robot;

    public static NameTag Create(Transform parent, Robot robot)
    {
        var tag = new GameObject("AdEtiketi").AddComponent<NameTag>();
        tag.transform.SetParent(parent, false);
        tag.robot = robot;
        tag.text = WorldText.Create(tag.transform, "", Vector3.zero, 0.42f, Ink);
        tag.Follow();
        return tag;
    }

    void LateUpdate() => Follow();

    void Follow()
    {
        transform.position = robot.HeadTop + Vector3.up * 0.45f;
        var cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }

    public IEnumerator Rename(string oldName, string newName)
    {
        yield return Fade(0f, 1f, 0.5f, oldName);
        yield return Tween.Wait(1.1f);
        for (int n = oldName.Length - 1; n >= 0; n--)
        {
            text.text = oldName.Substring(0, n);
            yield return Tween.Wait(0.13f);
        }
        yield return Tween.Wait(0.4f);
        for (int n = 1; n <= newName.Length; n++)
        {
            text.text = newName.Substring(0, n);
            Sound.Tap();
            yield return Tween.Wait(0.17f);
        }
        yield return Tween.Wait(2.4f);
        yield return Fade(1f, 0f, 0.8f, newName);
        Destroy(gameObject);
    }

    IEnumerator Fade(float from, float to, float dur, string label)
    {
        text.text = label;
        yield return Tween.Run(dur, t =>
        {
            var c = Ink;
            c.a = Mathf.Lerp(from, to, t);
            text.color = c;
        });
    }
}
