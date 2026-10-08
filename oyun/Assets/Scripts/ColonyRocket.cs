using UnityEngine;

// Koloninin kucuk roketi (Bolge 6): Kivilcim'i otomatik rotayla kanyona getirdi ("Hazirlayan: D. Aras").
// Ince uzun govde (burun konisi + silindir + motor etegi), turuncu bant, uc egik ayak. Kameraya bakan yuzde kapisi acik,
// kapidan yere inen ince merdiven; altinda yanik iz. Boyu ~2,2 birim (olcek 1). 3b'deki inis sahnesi koku hareket ettirir.
public static class ColonyRocket
{
    public static Transform Create(Transform parent, Vector3 localPos, float scale)
    {
        var root = Parts.Empty("Iz-Roket", parent);
        root.localPosition = localPos;
        root.localScale = Vector3.one * scale;

        var hull = Mats.Lit(Mats.Hex("#D9D4CA"), 0.45f);
        var dark = Mats.Lit(Mats.Hex("#34313A"), 0.35f);
        var accent = Mats.Lit(Mats.Hex("#E8743B"), 0.4f);
        var soot = Mats.Lit(Mats.Hex("#2A1C18"), 0.05f);

        var profile = new[]
        {
            new Vector2(0.11f, 0.28f), new Vector2(0.17f, 0.4f), new Vector2(0.2f, 0.48f), new Vector2(0.2f, 1.55f),
            new Vector2(0.17f, 1.8f), new Vector2(0.1f, 2.02f), new Vector2(0.03f, 2.16f), new Vector2(0f, 2.19f),
        };
        Parts.Add("Govde", root, MeshFactory.Lathe(profile, 28), hull, Vector3.zero);
        Parts.Add("Motor", root, MeshFactory.RoundedCylinder(0.13f, 0.12f, 0.02f, 20), dark, new Vector3(0f, 0.3f, 0f));
        Parts.Add("Bant", root, MeshFactory.RoundedCylinder(0.206f, 0.09f, 0.01f, 28), accent, new Vector3(0f, 1.32f, 0f), outline: false);
        Parts.Add("Pencere", root, MeshFactory.RoundedCylinder(0.04f, 0.02f, 0.006f, 14), dark, new Vector3(0f, 1.72f, -0.172f), outline: false)
            .localRotation = Quaternion.Euler(80f, 0f, 0f);

        // uc ayak: govdeden yere egik; arkada bir, onde iki (kapinin onu bos kalsin)
        foreach (float deg in new[] { 90f, 210f, 330f })
        {
            float a = deg * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            var top = dir * 0.17f + Vector3.up * 0.65f;
            var foot = dir * 0.46f;
            var leg = Parts.Add("Ayak", root, MeshFactory.RoundedBox(new Vector3(0.04f, (top - foot).magnitude, 0.04f), 0.012f), dark, (top + foot) * 0.5f);
            leg.localRotation = Quaternion.FromToRotation(Vector3.up, top - foot);
            Parts.Add("Taban", root, MeshFactory.RoundedCylinder(0.06f, 0.025f, 0.008f, 14), dark, foot + Vector3.up * 0.012f, outline: false);
        }

        // kapi: kameraya bakan yuzde koyu acik kapi + yana acilmis kapak
        Parts.Add("Kapi", root, MeshFactory.RoundedBox(new Vector3(0.15f, 0.27f, 0.03f), 0.01f), dark, new Vector3(0f, 0.98f, -0.192f), outline: false);
        var hatch = Parts.Empty("Kapak", root);
        hatch.localPosition = new Vector3(0.075f, 0.98f, -0.2f);
        hatch.localRotation = Quaternion.Euler(0f, 70f, 0f);
        Parts.Add("Levha", hatch, MeshFactory.RoundedBox(new Vector3(0.15f, 0.27f, 0.015f), 0.008f), hull, new Vector3(0.075f, 0f, 0f));

        // merdiven: kapinin altindan yere, one dogru egik
        var ladder = Parts.Empty("Merdiven", root);
        ladder.localPosition = new Vector3(0f, 0.42f, -0.36f);
        ladder.localRotation = Quaternion.Euler(-22f, 0f, 0f);
        foreach (float x in new[] { -0.05f, 0.05f })
            Parts.Add("Ray", ladder, MeshFactory.RoundedBox(new Vector3(0.014f, 0.9f, 0.014f), 0.005f), dark, new Vector3(x, 0f, 0f), outline: false);
        for (int k = 0; k < 5; k++)
            Parts.Add("Basamak", ladder, MeshFactory.RoundedBox(new Vector3(0.1f, 0.012f, 0.014f), 0.004f), dark, new Vector3(0f, -0.36f + k * 0.18f, 0f), outline: false);

        // yanik iz: kenari yumusak gorunsun diye iki kat
        Parts.Add("Yanik", root, MeshFactory.RoundedCylinder(0.75f, 0.004f, 0.002f, 28), Mats.Lit(Mats.Hex("#4A2A20"), 0.05f), new Vector3(0f, 0.002f, 0f), outline: false, castShadow: false);
        Parts.Add("Yanik", root, MeshFactory.RoundedCylinder(0.48f, 0.004f, 0.002f, 28), soot, new Vector3(0f, 0.004f, 0f), outline: false, castShadow: false);
        return root;
    }
}
