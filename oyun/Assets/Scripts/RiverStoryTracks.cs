using System;
using System.Collections.Generic;
using UnityEngine;

// Goc izi sayisi (63): sayi denetimi ve 3b'nin kus bakisi okur. Butun diziler tek agda oldugu icin sayi bu bilesende durur.
public class MigrationTrack : MonoBehaviour
{
    public int Big, Small;
}

// Bolge 7'nin hikaye izleri (spec 3a.3, senaryo-bolge-07.md "Izler ve gizli ayrintilar"); oynanan iz degil (o TrailView'de),
// alanin icinde ve disinda zemine islenmis sus izleri. Soylenmez, yalniz gorunur:
//  63-64 goc izi: koloni (23 yetiskin + Ece) nehrin ic kiyisi boyunca gelir, kivrimi kisaltip alanin ustunden capraz gecer, kiyiya doner.
//  65 kucuk iz ayriliyor: alanin arkasindan gecen birkac yetiskin izinden tek kucuk iz sag arkadaki yan kanyonun agzina sapar.
//  66 gidis ve donus: kucuk + kosan buyuk iz yan kanyona gider; tek buyuk iz kisa, derin adimlarla geri doner (bir sey tasiyor).
//  67 ziplama: yan kanyonun dibinde duz taslar; kucuk iz tastan tasa ziplar. 67-69 donus izi duvar dibinden ve alanin solundan gecer.
public static class RiverStoryTracks
{
    public const int MigrationBig = 23, MigrationSmall = 1;   // 63 damgasi "Izler: 23 buyuk, 1 kucuk" (koloni 24 kisi)
    static readonly int[] Families = { 5, 5, 4, 5, 4 };       // goc izinde aile aile gevsek gruplar (toplam MigrationBig)

    const float PersonGap = 0.1f, FamilyGap = 0.22f;          // goc izinde yan yana yuruyenler arasi, aileler arasi
    const float InAreaDepth = 0.5f;                           // alanin icinde izler soluk: kareler okunsun
    const float BankInset = 1.2f;                             // goc izi nehrin ortasindan ic kiyiya bu kadar iceride

    // Bolumun hikaye izlerini kurar. Doner: 63-64'te goc izi, digerlerinde null.
    public static Transform Build(int levelNumber, Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        switch (levelNumber)
        {
            case 63:
            case 64:
                return Migration(parent, areaHalf, ground);
            case 65: SplittingTrack(parent, areaHalf, ground); break;
            case 66: OutAndBack(parent, areaHalf, ground); break;
            case 67:
                HoppingStones(parent, areaHalf, ground);
                ReturnTrack(parent, areaHalf, ground);
                break;
            case 68:
            case 69:
                ReturnTrack(parent, areaHalf, ground);
                break;
        }
        return null;
    }

    // ---- 63-64: goc izi ----

    static Transform Migration(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        var spine = Resample(MigrationSpine(areaHalf), 0.4f);
        float width = MigrationBig * PersonGap + (Families.Length - 1) * FamilyGap;
        var prints = new List<Footprints.Print>();
        float lateral = -width * 0.5f;
        int person = 0;
        for (int f = 0; f < Families.Length; f++)
        {
            float phase = f * 1.7f;
            float familyStart = lateral;
            for (int k = 0; k < Families[f]; k++, person++)
            {
                float side = lateral;
                var path = Offset(spine, s => side + 0.15f * Mathf.Sin(s * 0.35f + phase));
                prints.AddRange(Footprints.Prints(path, Walker.Big, Gait.Walk, 0f, 700 + person));
                lateral += PersonGap;
            }
            if (f == 1)   // Ece, ailesinin kenarinda bir yetiskinin hemen yaninda (el ele)
            {
                var path = Offset(spine, s => familyStart - 0.1f + 0.15f * Mathf.Sin(s * 0.35f + phase));
                prints.AddRange(Footprints.Prints(path, Walker.Small, Gait.Walk, 0f, 760));
            }
            lateral += FamilyGap - PersonGap;
        }
        var track = Build("GocIzi", parent, prints, areaHalf, ground);
        var count = track.gameObject.AddComponent<MigrationTrack>();
        count.Big = person;
        count.Small = MigrationSmall;
        return track;
    }

    // Goc izinin orta cizgisi: batida nehrin ic kiyisi boyunca (kameraya yakin yandan) gelir, alanin ustunden capraz
    // gecer (sol on taraftan sag arkaya), doguda yine ic kiyiyi izler. Her iki yanda alanin disinda en az ~8 birim surer (3b kus bakisi).
    static List<Vector3> MigrationSpine(Vector2 areaHalf)
    {
        var points = new List<Vector3>();
        for (float x = -10.5f; x <= -9f; x += 0.25f) points.Add(InnerBank(x, areaHalf));
        for (float x = 5.5f; x <= 9.5f; x += 0.25f) points.Add(InnerBank(x, areaHalf));
        return points;
    }

    // Nehrin orta cizgisi (z). IKIZ: Ground.shader riverMeander (ayni sabitler; biri degisirse oteki de).
    static float RiverZ(float x, Vector2 areaHalf)
    {
        float bendX = Mathf.Max(Mathf.Abs(x) - (areaHalf.x + 1.5f), 0f);
        return areaHalf.y + 1.15f + 0.4f * Mathf.Sin(x * 0.5f + 0.7f) - 0.32f * bendX * bendX;
    }

    // Nehrin ic kiyisi: orta cizgiden alana dogru BankInset kadar iceride
    static Vector3 InnerBank(float x, Vector2 areaHalf)
    {
        var c = new Vector3(x, 0f, RiverZ(x, areaHalf));
        var tangent = new Vector3(0.02f, 0f, RiverZ(x + 0.01f, areaHalf) - RiverZ(x - 0.01f, areaHalf)).normalized;
        var normal = new Vector3(-tangent.z, 0f, tangent.x);
        if (Vector3.Dot(normal, -c) < 0f) normal = -normal;
        return c + normal * BankInset;
    }

    // ---- 65: kucuk iz ayriliyor ----

    // Alanin arkasindan batidan doguya gecen dort yetiskin izi; ortalarda kucuk iz ayrilip sag arkadaki yan kanyonun agzina sapar.
    static void SplittingTrack(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        float z = areaHalf.y + 0.55f;
        var band = Resample(new List<Vector3> { new Vector3(-12f, 0f, z + 0.3f), new Vector3(0f, 0f, z), new Vector3(12f, 0f, z - 0.4f) }, 0.4f);
        var prints = new List<Footprints.Print>();
        for (int k = 0; k < 4; k++)
        {
            float side = (k - 1.5f) * 0.13f;
            prints.AddRange(Footprints.Prints(Offset(band, s => side), Walker.Big, Gait.Walk, 0f, 650 + k));
        }
        // kucuk iz: yetiskinlerle gelir, sonra yan kanyonun agzina (iki kaya parcasinin arasi) doner
        var small = new List<Vector3> { new Vector3(-12f, 0f, z + 0.5f), new Vector3(0.6f, 0f, z + 0.25f), new Vector3(1.9f, 0f, z + 0.7f),
            new Vector3(2.5f, 0f, areaHalf.y + 2.4f) };
        prints.AddRange(Footprints.Prints(Resample(small, 0.3f), Walker.Small, Gait.Walk, 0f, 655));
        Build("AyrilanIz", parent, prints, areaHalf, ground);
    }

    // ---- 66: gidis ve donus ----

    // Alanin sag yanindan yan kanyona: gidiste kucuk + kosan buyuk yan yana; donuste tek buyuk iz, kisa ve derin adimlarla.
    static void OutAndBack(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        float x = areaHalf.x + 0.6f;
        var toCanyon = Resample(new List<Vector3> { new Vector3(x - 0.2f, 0f, -10f), new Vector3(x, 0f, areaHalf.y - 0.4f),
            new Vector3(2.6f, 0f, areaHalf.y + 1.6f), new Vector3(2.4f, 0f, areaHalf.y + 3f) }, 0.3f);
        var prints = new List<Footprints.Print>();
        prints.AddRange(Footprints.Prints(Offset(toCanyon, s => -0.12f), Walker.Small, Gait.Walk, 0f, 661));
        prints.AddRange(Footprints.Prints(Offset(toCanyon, s => 0.12f), Walker.Big, Gait.Run, 0f, 662));
        var back = new List<Vector3>(toCanyon);
        back.Reverse();
        prints.AddRange(Footprints.Prints(Offset(back, s => 0.5f), Walker.Big, Gait.Carry, 0f, 663));
        Build("GidisDonus", parent, prints, areaHalf, ground);
    }

    // ---- 67: ziplama ----

    // Alanin sag yaninda, yan kanyonun dibinde dort duz tas; kucuk iz her tasin onunde ve arkasinda iki ayak yan yana (ziplamis).
    static void HoppingStones(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        var stoneMat = Mats.Lit(Mats.Hex("#9A8A80"), 0.4f);
        var prints = new List<Footprints.Print>();
        var forward = Vector3.forward;
        float x = areaHalf.x + 0.4f;   // alanin hemen yani: genis gorunum olmadan da ekranda
        for (int k = 0; k < 4; k++)
        {
            var at = new Vector3(x + 0.05f * Mathf.Sin(k * 2.3f), 0f, -2.4f + k * 1.1f);
            var stone = Parts.Add("DuzTas", parent, MeshFactory.Cobble(0.2f, 670 + k), stoneMat, new Vector3(at.x, ground(at.x, at.z) + 0.02f, at.z));
            stone.localScale = new Vector3(1.1f, 0.22f, 0.9f);
            stone.localRotation = Quaternion.Euler(0f, k * 41f, 0f);
            prints.AddRange(Footprints.HopPair(at - forward * 0.36f, forward, Walker.Small, 680 + k));
            prints.AddRange(Footprints.HopPair(at + forward * 0.36f, forward, Walker.Small, 690 + k));
        }
        Build("ZiplamaIzi", parent, prints, areaHalf, ground);
    }

    // ---- 67-69: donus izi ----

    // Tek buyuk iz, kisa ve derin adimlar: arka duvarin dibinden batiya, sonra alanin sol yanindan kameraya dogru (68'de kalem yaninda).
    static void ReturnTrack(Transform parent, Vector2 areaHalf, Func<float, float, float> ground)
    {
        float wallFoot = areaHalf.y + 0.45f, left = -(areaHalf.x + 0.5f);
        var path = Resample(new List<Vector3> { new Vector3(9f, 0f, wallFoot), new Vector3(left + 0.4f, 0f, wallFoot),
            new Vector3(left, 0f, wallFoot - 0.6f), new Vector3(left - 0.1f, 0f, -10f) }, 0.3f);
        Build("DonusIzi", parent, Footprints.Prints(path, Walker.Big, Gait.Carry, 0f, 666), areaHalf, ground);
    }

    // ---- yardimcilar ----

    // Izleri tek agda kurar; alanin icine dusenler soluk (kareler okunsun)
    static Transform Build(string name, Transform parent, List<Footprints.Print> prints, Vector2 areaHalf, Func<float, float, float> ground)
    {
        for (int i = 0; i < prints.Count; i++)
        {
            var p = prints[i].Pos;
            if (Mathf.Abs(p.x) < areaHalf.x && Mathf.Abs(p.z) < areaHalf.y) prints[i] = prints[i].WithDepth(prints[i].Depth * InAreaDepth);
        }
        return Parts.Add(name, parent, Footprints.BuildMesh(prints, ground), Footprints.Material(), Vector3.zero, outline: false, castShadow: false);
    }

    // Yolu esit araliklarla yeniden ornekler (yana kaydirma yumusak olsun)
    static List<Vector3> Resample(List<Vector3> points, float spacing)
    {
        var result = new List<Vector3> { points[0] };
        for (int i = 1; i < points.Count; i++)
        {
            var a = points[i - 1];
            var b = points[i];
            int n = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a, b) / spacing));
            for (int k = 1; k <= n; k++) result.Add(Vector3.Lerp(a, b, (float)k / n));
        }
        return result;
    }

    // Yolu yana kaydirir: lateral(s) yolun s uzakligindaki kayma (+ saga)
    static List<Vector3> Offset(List<Vector3> path, Func<float, float> lateral)
    {
        var result = new List<Vector3>(path.Count);
        float s = 0f;
        for (int i = 0; i < path.Count; i++)
        {
            if (i > 0) s += Vector3.Distance(path[i - 1], path[i]);
            var tangent = path[Mathf.Min(i + 1, path.Count - 1)] - path[Mathf.Max(i - 1, 0)];
            tangent.y = 0f;
            tangent.Normalize();
            var right = new Vector3(tangent.z, 0f, -tangent.x);
            result.Add(path[i] + right * lateral(s));
        }
        return result;
    }
}
