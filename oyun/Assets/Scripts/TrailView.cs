using System;
using System.Collections;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;

// Oynanan izde kimlerin izi var ve nasil yurumusler (RiverBedTraces.LookFor)
public readonly struct TrailLook
{
    public readonly bool Small, Big;
    public readonly Gait SmallGait, BigGait;
    public TrailLook(bool small, bool big, Gait smallGait, Gait bigGait)
    {
        Small = small; Big = big; SmallGait = smallGait; BigGait = bigGait;
    }
}

// Bolge 7 iz bolumlerinde oynarken gorunenler (spec 3a.2). Izin yalniz basi (alanin onunden baslangic karesine gelen izler)
// gorunur; gerisi kumla ortulu. Kivilcim izin bir karesine bastikca oraya giden izler belirir. scout() aninda Serce yol
// ayriminin ustune suzulur, kolu isikla gosterir. Izin sonuna varilinca hedef belirir. Dunya kurallarini bilmez: Oyun ona
// kare konumlarini ve olaylari verir.
public class TrailView
{
    const float SmallSide = -0.11f, BigSide = 0.1f;   // el ele: kucuk solda, buyuk sagda
    const float LeadIn = 0.55f;                       // izin baslangic karesine alanin onunden (kameraya yakin yandan) gelen kismi
    const float ScoutHeight = 1.1f, ScoutAside = 0.8f, GlideTime = 0.3f, LightWait = 0.35f, ReturnTime = 0.8f;
    static readonly Color LabelColor = ScoutLight.Sun;   // K1-K2: yon yazisi kucuk, gunes renginde
    const float LabelSize = 0.3f;

    readonly Transform root;
    readonly Func<Cell, Vector3> pos;
    readonly Func<float, float, float> ground;
    readonly Sparrow sparrow;
    readonly TrailLook look;
    readonly MonoBehaviour runner;

    Transform prints;                     // bu izin butun izleri (her biri bir grup)
    readonly List<DecalFade> groups = new List<DecalFade>();   // groups[i]: izin i. karesine varan izler; 0 = alanin onunden gelen bas
    Trail trail;
    Target arrival;

    // Serce yol ayriminin ustunde, isik yerde (denetim karesi icin)
    public bool Scouting { get; private set; }

    public TrailView(Transform levelRoot, Func<Cell, Vector3> pos, Func<float, float, float> ground, Sparrow sparrow, TrailLook look,
        MonoBehaviour runner, Trail first)
    {
        root = levelRoot;
        this.pos = pos;
        this.ground = ground;
        this.sparrow = sparrow;
        this.look = look;
        this.runner = runner;
        Begin(first);
    }

    // Her Calistir'da: onceki izleri siler, bu izin izlerini gizli kurar; yalniz bas gorunur
    public void Begin(Trail shown)
    {
        if (prints != null) UnityEngine.Object.Destroy(prints.gameObject);
        groups.Clear();
        trail = shown;
        prints = Parts.Empty("OynananIz", root);
        var path = Path(shown);
        var byCell = new List<Footprints.Print>[shown.Cells.Count];
        for (int i = 0; i < byCell.Length; i++) byCell[i] = new List<Footprints.Print>();
        if (look.Small) Sort(Footprints.Prints(path, Walker.Small, look.SmallGait, look.Big ? SmallSide : 0f, 620 + shown.Steps.Count), byCell);
        if (look.Big) Sort(Footprints.Prints(path, Walker.Big, look.BigGait, BigSide, 640 + shown.Steps.Count), byCell);
        for (int i = 0; i < byCell.Length; i++)
        {
            var mat = Footprints.Material(0f);
            var piece = Parts.Add("Iz-" + i, prints, Footprints.BuildMesh(byCell[i], ground), mat, Vector3.zero, outline: false, castShadow: false);
            var fade = piece.gameObject.AddComponent<DecalFade>();
            fade.Init(mat);
            groups.Add(fade);
        }
        HideAll();
        Restore();
    }

    // Kivilcim bu kareye basti: izde ise oraya giden izler yumusakca belirir
    public void Stepped(Cell at)
    {
        int i = trail.IndexOf(at);
        if (i > 0) groups[i].Show();
    }

    // Serce yol ayriminin ustune suzulur, kolu isikla gosterir; donusu arkada baslar (beklenmez). Pes pese scout()'ta
    // bulundugu yerden siradakine gider.
    public IEnumerator Scouted(Cell at, Direction d)
    {
        var cell = pos(at);
        sparrow.StopAllCoroutines();
        yield return sparrow.FlyTo(cell + ScoutOffset(cell, d), GlideTime);
        ScoutLight.Show(root, cell, pos(at.Step(d)));
        runner.StartCoroutine(PowerLabel.Show(root, cell, d.ToString(), LabelColor, LabelSize));
        Scouting = true;
        yield return Tween.Wait(LightWait);
        Scouting = false;
        sparrow.StartCoroutine(sparrow.FlyTo(sparrow.HomePosition, ReturnTime));
    }

    // Serce'nin yol ayrimindaki yeri: ne Kivilcim'in ne de isikli kolun ustunu kapatir (kamera onden, yukaridan bakar).
    // Kol dogu/batiya giderse karenin arkasinda; kuzey/guneye giderse alanin ortasina dogru yaninda.
    static Vector3 ScoutOffset(Vector3 cell, Direction d)
    {
        bool sideways = d == Direction.East || d == Direction.West;
        var aside = sideways ? new Vector3(0f, 0f, ScoutAside) : new Vector3(cell.x > 0f ? -ScoutAside : ScoutAside, 0f, 0.2f);
        return aside + Vector3.up * ScoutHeight;
    }

    // Izin sonu: hedef belirir ve hemen "varildi" der
    public void Arrived()
    {
        if (arrival != null) return;
        arrival = Target.Create(root, pos(trail.End));
        arrival.Reach();
    }

    // Tekrar / yeniden deneme: izler yine gizli (bas gorunur), hedef yok, Serce yerinde
    public void Restore()
    {
        HideAll();
        groups[0].ShowNow();
        if (arrival != null) UnityEngine.Object.Destroy(arrival.gameObject);
        arrival = null;
        sparrow.ResetHome();
    }

    void HideAll()
    {
        foreach (var g in groups) g.Hide();
    }

    // Izin gectigi yol: alanin onunden baslangic karesine, sonra kare ortalarindan
    List<Vector3> Path(Trail t)
    {
        var path = new List<Vector3>();
        var start = pos(t.Cells[0]);
        path.Add(start - Vector3.forward * LeadIn);
        foreach (var c in t.Cells) path.Add(pos(c));
        return path;
    }

    // Her izi yoldaki uzakligina gore bir gruba koyar: baslangic karesine kadar olanlar 0, (i-1). kareden i. kareye giderken birakilanlar i
    void Sort(List<Footprints.Print> all, List<Footprints.Print>[] byCell)
    {
        foreach (var p in all)
        {
            int i = p.Distance <= LeadIn ? 0 : Mathf.Min(byCell.Length - 1, 1 + (int)((p.Distance - LeadIn) / CellSize));
            byCell[i].Add(p);
        }
    }

    const float CellSize = 1f;   // kareler arasi (Oyun.Pos)
}
