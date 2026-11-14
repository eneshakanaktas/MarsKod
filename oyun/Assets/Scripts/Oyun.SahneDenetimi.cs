using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Bolge 6 ve Bolge 7 sahnelerinin, Bolge 7 izlerinin goruntu denetimi (-shots <klasor> -sahneler): ara kareler + SAHNE DENETIMI satiri.
// Tam turda sonunda Bolge 8 denetimi de calisir (Oyun.IstasyonDenetimi.cs; yalniz o: -bolge8).
public partial class Oyun
{
    // riverOnly (-bolge7): yalnizca Bolge 7 (sahne uzerinde calisirken hizli tur). Bolge 6 denetimleri 3b'den once calisir: yarik cakisma
    // denetimi robotun bostaki kipirtisinin (kol, kurdele, govde) o anki haliyle olcer ve yarikta pay birkac santim; once calisan denetimler
    // degisince kipirtinin baska haline denk gelip hata verebiliyor (ILERLEME.md, 2026-11-03).
    IEnumerator SceneShots(string dir, bool riverOnly)
    {
        Directory.CreateDirectory(dir);
        yield return new WaitForSeconds(2.5f);
        var problems = new List<string>();
        yield return ScoutShots(dir, problems);
        yield return CrayonCloseup(dir, problems);
        yield return RiverBedChecks(dir, problems);
        if (!riverOnly)
        {
            yield return RiftShots(dir, problems);
            yield return DescentShots(dir, problems);
            yield return FlightShots(dir, problems);
            yield return LandingShots(dir, problems);
            yield return ShotKeyboardCheck(dir);
        }
        yield return BirdViewShots(dir, problems);
        yield return CrayonShots(dir, problems);
        yield return FinaleShots70(dir, problems);
        yield return SkipChecks(dir, problems, riverOnly);   // "Atla" dugmesi (Oyun.AtlaDenetimi.cs)
        if (!riverOnly) yield return LandingEveryEntryCheck(problems);
        if (!riverOnly) yield return StationChecks(dir, problems);   // Bolge 8 (Oyun.IstasyonDenetimi.cs)
        Debug.Log("SAHNE DENETIMI" + (riverOnly ? " (yalniz Bolge 7)" : "") + ": " + (problems.Count == 0 ? "TAMAM" : "HATA " + string.Join("; ", problems)));
        Application.Quit();
    }

    // Bolge 7: 62'de scout() ani (Serce yol ayriminda, isik yerde) ve yol bitince izler; 65'te yalniz kucuk iz
    IEnumerator ScoutShots(string dir, List<string> problems)
    {
        foreach (int number in new[] { 62, 65 })
        {
            LoadLevel(levels.FindIndex(l => l.Number == number));
            yield return new WaitForSeconds(0.8f);
            if (trailView == null) { problems.Add(number + "'de iz yok"); continue; }
            SetCode(level.Solution);
            yield return new WaitForSeconds(0.3f);
            OnRun();
            float waited = 0f;
            while (!trailView.Scouting && running && waited < 5f) { waited += Time.deltaTime; yield return null; }
            if (!trailView.Scouting) problems.Add(number + "'de scout() ani gelmedi");
            yield return new WaitForSeconds(0.12f);
            Cap(Path.Combine(dir, "s" + number + "-scout.png"));
            // izler robotun degil, izin sahiplerinin: kum acilirken belirir. Bir sonraki yeni iz belirdiginde (kum bulutu ve belirmekte
            // olan izler) kare al
            int seen = trailView.Uncovered;
            for (float w = 0f; trailView.Uncovered == seen && running && w < 8f; w += Time.deltaTime) yield return null;
            yield return new WaitForSeconds(0.25f);
            Cap(Path.Combine(dir, "s" + number + "-kum.png"));
            while (running) yield return null;
            yield return new WaitForSeconds(0.6f);
            Cap(Path.Combine(dir, "s" + number + "-izler.png"));
            yield return new WaitForSeconds(0.3f);
        }
    }

    // Bolge 7 sayi denetimi: sahnede gorunen sayilar bitis damgasiyla eslesmeli (63 goc izi, 64 cakil, 69 el izleri);
    // 68'de yerde kalem, 70'te duvar resmi ve Kivilcim'in elinin bos yeri duvarin onunde.
    IEnumerator RiverBedChecks(string dir, List<string> problems)
    {
        yield return LoadRiverLevel(63);
        var migration = RiverBedTraces.Migration != null ? RiverBedTraces.Migration.GetComponent<MigrationTrack>() : null;
        if (migration == null) problems.Add("63'te goc izi yok");
        else Expect(problems, 63, "İzler: " + migration.Big + " büyük, " + migration.Small + " küçük");
        Cap(Path.Combine(dir, "s63-izler.png"));
        yield return new WaitForSeconds(0.4f);

        yield return LoadRiverLevel(64);
        Expect(problems, 64, "Örnek çakıl: " + level.Ices.Count);
        if (level.Item != MarsKod.Dunya.Collectible.Pebble) problems.Add("64'te toplanan cakil degil");

        yield return LoadRiverLevel(RiverBedTraces.CrayonLevel);
        if (RiverBedTraces.Crayon == null) problems.Add("68'de yerde kalem yok");

        yield return LoadRiverLevel(RiverBedTraces.HandsLevel);
        int small = CountNamed("ElIzi-Kucuk"), big = CountNamed("ElIzi-Buyuk");
        if (small != RiverBedTraces.HandsSmall || big != RiverBedTraces.HandsBig) problems.Add("69'da el izi " + small + " kucuk, " + big + " buyuk");
        Expect(problems, RiverBedTraces.HandsLevel, "El izi: " + small + " küçük, " + big + " büyük");
        Cap(Path.Combine(dir, "s69-eller.png"));
        yield return new WaitForSeconds(0.4f);

        yield return LoadRiverLevel(RiverBedTraces.MuralLevel);
        var mural = RiverBedTraces.Mural;
        if (mural == null || RiverBedTraces.BackWallFace == null) problems.Add("70'te duvar resmi yok");
        else if (mural.Value.RobotHand.z >= levelRoot.InverseTransformPoint(RiverBedTraces.BackWallFace.position).z)
            problems.Add("70'te robot elinin yeri duvarin arkasinda");
        Cap(Path.Combine(dir, "s70-duvar.png"));
        yield return new WaitForSeconds(0.4f);
    }

    // 63'un kus bakisi: dogru cozum, klavye acik bitis (sahne basinda kapanmali); evreler gelince kare. Sonra kus aninda "Tekrar".
    IEnumerator BirdViewShots(string dir, List<string> problems)
    {
        yield return LoadRiverLevel(RiverBedTraces.BirdViewLevel);
        SetCode(level.Solution);
        OnRun();
        yield return new WaitForSeconds(0.5f);
        hud.FocusCode(0, 0);
        foreach (var (phase, wait, file) in new[] { ("kalkis", 0.5f, "s63-1-kalkis.png"), ("kus", 1.2f, "s63-2-kus.png"), ("son", 0.5f, "s63-3-son.png") })
        {
            yield return WaitForPhase(phase, problems, RiverPhase);
            yield return new WaitForSeconds(wait);
            Cap(Path.Combine(dir, file));
            yield return new WaitForSeconds(0.4f);
        }

        yield return LoadRiverLevel(RiverBedTraces.BirdViewLevel);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("kus", problems, RiverPhase);
        ResetLevel();
        yield return null;
        CheckRiverRestored("63", problems);
    }

    // 68'in kalem sahnesi: her iz sonundan (yol her kosuda baska yerde biter) ayri baslatilir; ilkinden kareler. Yuruyuste robot kaya
    // karesine girmemeli; sonunda yerdeki kalem gizli, sirttaki gorunur. Sonra normal akistan bir kez ve alis aninda "Tekrar".
    IEnumerator CrayonShots(string dir, List<string> problems)
    {
        yield return LoadRiverLevel(RiverBedTraces.CrayonLevel);
        for (int i = 0; i < level.Trails.Count; i++)
        {
            yield return LoadRiverLevel(RiverBedTraces.CrayonLevel);
            var blocked = MarsKod.Dunya.GridPath.Obstacles(level, itemsTaken: true);
            var walker = new SceneWalker(robot, c => Pos(c), Cols, Rows);
            robot.ResetTo(Pos(level.Trails[i].End), 0f);
            trailView?.RevealAll();   // gercekte kod kosarken belirmis olurdu: sahne kamerasinda da gorunmeli
            StartCoroutine(riverScenes.TakeCrayon(level));
            var shots = i == 0 ? new[] { ("yol", 0.6f, "s68-1-yol.png"), ("alis", 1.5f, "s68-2-alis.png"), ("alis", 0.15f, "s68-2b-omuz.png"), ("sirt", 0.3f, "s68-3-sirt.png"), ("son", 0.5f, "s68-4-son.png") }
                : new[] { ("son", 0f, "") };
            float t = 0f;
            bool onRock = false;
            foreach (var (phase, wait, file) in shots)
            {
                for (; riverScenes.Phase != phase && t < 40f; t += Time.deltaTime)
                {
                    onRock |= blocked.Contains(walker.CellAt(robot.transform.localPosition));
                    yield return null;
                }
                if (riverScenes.Phase != phase) { problems.Add("68 iz " + (i + 1) + ": " + phase + " evresi gelmedi"); break; }
                if (file == "") continue;
                yield return new WaitForSeconds(wait);
                Cap(Path.Combine(dir, file));
                yield return new WaitForSeconds(0.4f);
            }
            if (onRock) problems.Add("68 iz " + (i + 1) + ": robot yuruyuste kaya karesine girdi");
            if (RiverBedTraces.Crayon == null || RiverBedTraces.Crayon.gameObject.activeSelf) problems.Add("68 iz " + (i + 1) + ": yerdeki kalem gizlenmedi");
            if (!HasCrayon()) problems.Add("68 iz " + (i + 1) + ": sirtta kalem yok");
        }

        yield return LoadRiverLevel(RiverBedTraces.CrayonLevel);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("son", problems, RiverPhase);

        yield return LoadRiverLevel(RiverBedTraces.CrayonLevel);
        var crayon = RiverBedTraces.Crayon;
        var home = crayon.localPosition;
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("alis", problems, RiverPhase);
        yield return new WaitForSeconds(1.2f);   // kalem elde
        ResetLevel();
        yield return null;
        CheckRiverRestored("68", problems);
        if (!crayon.gameObject.activeSelf || crayon.parent != levelRoot || (crayon.localPosition - home).sqrMagnitude > 1e-6f) problems.Add("68 Tekrar: kalem yerine donmedi");
        if (HasCrayon()) problems.Add("68 Tekrar: sirtta kalem kaldi");
    }

    // 70'in finali: her iz sonundan ayri baslatilir; ilkinden kareler. Sahne boyunca robotun hicbir parcasi duvara, kosedeki direge ya da kaya
    // karesine girmemeli; cizim basinda avuc duvardaki bos yerde, eller aninda Kivilcim'in eli duvarda cizili; sonda gozler turkuaz. Sonra atlama, sahne ortasinda "Tekrar" ve normal akis.
    IEnumerator FinaleShots70(string dir, List<string> problems)
    {
        yield return LoadRiverLevel(RiverBedTraces.MuralLevel);
        int trails = level.Trails.Count;
        for (int i = 0; i < trails; i++)
        {
            yield return LoadRiverLevel(RiverBedTraces.MuralLevel);
            var blocked = MarsKod.Dunya.GridPath.Obstacles(level, itemsTaken: true);
            var walker = new SceneWalker(robot, c => Pos(c), Cols, Rows);
            var solids = TemporaryColliders(levelRoot, "Iz-KayaDuvar", world, "Post");
            robot.ResetTo(Pos(level.Trails[i].End), 0f);
            trailView?.RevealAll();   // gercekte kod kosarken belirmis olurdu: sahne kamerasinda da gorunmeli
            StartCoroutine(riverScenes.Finale(level));
            var shots = i == 0
                ? new[] { ("yol", 0.8f, "s70-1-yol.png"), ("okuma", 0.5f, "s70-2-duvar.png"), ("sessizlik", 1f, "s70-3-sessizlik.png"),
                    ("cizim", 0f, "s70-4a-el.png"), ("cizim", 2.6f, "s70-4b-cizim.png"), ("eller", 0.5f, "s70-4c-eller.png"),
                    ("donus", 0.8f, "s70-5-donus.png"), ("cicekler", 1.8f, "s70-6-cicekler.png"), ("gozler", 1.5f, "s70-7-gozler.png"), ("son", 0.5f, "s70-8-son.png") }
                : new[] { ("cizim", 0f, ""), ("eller", 0f, ""), ("son", 0f, "") };
            string label = "70 iz " + (i + 1) + ": ";
            float t = 0f;
            bool onRock = false;
            string hit = null;
            foreach (var (phase, wait, file) in shots)
            {
                for (; riverScenes.Phase != phase && t < 60f; t += Time.deltaTime)
                {
                    onRock |= blocked.Contains(walker.CellAt(robot.transform.localPosition));
                    hit ??= PartInside(robot.GetComponentsInChildren<Renderer>(), solids, riverScenes.Phase);
                    yield return null;
                }
                if (riverScenes.Phase != phase) { problems.Add(label + phase + " evresi gelmedi"); break; }
                if (phase == "cizim" && file != "s70-4b-cizim.png") CheckPalm(label, problems);
                if (phase == "eller" && !riverScenes.OwnHandDrawn) problems.Add(label + "Kivilcim'in eli duvarda cizilmedi");
                yield return new WaitForSeconds(wait);
                if (file != "") Cap(Path.Combine(dir, file));
                yield return new WaitForSeconds(file != "" ? 0.4f : 0f);
            }
            hit ??= PartInside(robot.GetComponentsInChildren<Renderer>(), solids, riverScenes.Phase);
            if (onRock) problems.Add(label + "robot yuruyuste kaya karesine girdi");
            if (hit != null) problems.Add(label + "robot icine giriyor: " + hit);
            if (!EceEyesOn()) problems.Add(label + "sonda gozler turkuaz degil");
            foreach (var c in solids) Destroy(c);
        }
        yield return FinaleSkipAndUndo(dir, problems);
    }

    // Cizim basinda avuc duvardaki bos yerde mi (kol hedefe kurulur: suzulurken de el yerinde kalir)
    void CheckPalm(string label, List<string> problems)
    {
        if (!(robot is SparkBot spark)) return;
        var palm = spark.LeftPalm;
        var spot = levelRoot.TransformPoint(RiverBedTraces.Mural.Value.RobotHand);
        if ((palm - spot).magnitude > 0.03f) problems.Add(label + "avuc " + palm.ToString("F2") + ", duvardaki bos yer " + spot.ToString("F2"));
    }

    bool EceEyesOn() =>
        robot is SparkBot spark && ((Vector4)(spark.EyeColor - CraterTraces.EceColor)).magnitude < 0.05f;

    // 70 normal akistan (kutlama -> sahne -> son), atlama ("Atla" dugmesi ilk izleyisten itibaren; dokununca son kare) ve cicekler
    // aninda "Tekrar".
    IEnumerator FinaleSkipAndUndo(string dir, List<string> problems)
    {
        yield return LoadRiverLevel(RiverBedTraces.MuralLevel);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("yol", problems, RiverPhase);
        yield return WaitForPhase("son", problems, RiverPhase, limit: 90f);   // final ~36-45 sn surer: 40 sn sinir yuklu turda dar kaliyordu
        yield return new WaitForSeconds(0.5f);
        Cap(Path.Combine(dir, "s70-9-normal-son.png"));
        yield return new WaitForSeconds(0.4f);
        if (!EceEyesOn()) problems.Add("70 normal akis: sonda gozler turkuaz degil");

        yield return SkipCheck(dir, "70", RiverBedTraces.MuralLevel, riverScenes.Skip, problems, () =>
            riverScenes.Phase != "son" ? "evre 'son' degil (" + riverScenes.Phase + ")"
            : !EceEyesOn() ? "gozler turkuaz degil"
            : !riverScenes.OwnHandDrawn ? "Kivilcim'in eli duvarda cizilmedi" : null);

        yield return LoadRiverLevel(RiverBedTraces.MuralLevel);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("yol", problems, RiverPhase);   // once kod calisir (her bekleme en fazla 40 sn)
        yield return WaitForPhase("eller", problems, RiverPhase);
        yield return WaitForPhase("cicekler", problems, RiverPhase);
        ResetLevel();
        yield return null;
        CheckRiverRestored("70", problems);
        if (EceEyesOn()) problems.Add("70 Tekrar: gozler turkuaz kaldi");
        if (CountNamed(OwnHandPrint.Name) != 0) problems.Add("70 Tekrar: Kivilcim'in duvardaki eli silinmedi");
        Sound.Tap();   // sessizlikten sonra ses yine calisiyor (hata vermemeli)
        yield return new WaitForSeconds(1f);
        if (riverScenes.Phase != "") problems.Add("70 Tekrar: sahne durmadi (" + riverScenes.Phase + ")");
    }

    // Adi verilen nesnelerin agaclarindaki her aga gecici MeshCollider (cakisma denetimi icin); denetim bitince silinir
    List<MeshCollider> TemporaryColliders(Transform root, string name, Transform otherRoot, string otherName)
    {
        var made = new List<MeshCollider>();
        foreach (var (under, wanted) in new[] { (root, name), (otherRoot, otherName) })
            foreach (var t in under.GetComponentsInChildren<Transform>())
            {
                if (t.name != wanted) continue;
                foreach (var mf in t.GetComponentsInChildren<MeshFilter>())
                {
                    var c = mf.gameObject.AddComponent<MeshCollider>();
                    c.sharedMesh = mf.sharedMesh;
                    made.Add(c);
                }
            }
        Physics.SyncTransforms();
        return made;
    }

    // Parcalardan biri (kutusu biraz kucultulerek) katilardan birine giriyorsa "kati <- parca"; yoksa null. Ince parcalar (golge, itici) sayilmaz.
    static string PartInside(Renderer[] parts, List<MeshCollider> solids, string phase = "")
    {
        Physics.SyncTransforms();
        foreach (var part in parts)
        {
            if (!part.enabled || !part.gameObject.activeInHierarchy) continue;
            var b = part.bounds;
            if (b.extents.y < 0.03f || part.name == "Thruster") continue;
            foreach (var other in Physics.OverlapBox(b.center, b.extents * 0.8f))
                if (other is MeshCollider mc && solids.Contains(mc))
                    return "evre " + phase + ": " + other.name + " <- " + part.name + " (el/parca yeri " + b.center.ToString("0.00") + ", duvar " + other.bounds.center.ToString("0.00") + ")";
        }
        return null;
    }

    // Bolge 7 sahnesinin ortasinda "Tekrar": kamera serbest, kart gorunur, kirpisma/atlama kapali, robot baslangicta ve gorunur, Serce evinde
    void CheckRiverRestored(string label, List<string> problems)
    {
        string p = label + " Tekrar: ";
        if (shot.Active) problems.Add(p + "sahne kamerasi birakilmadi");
        if (!hud.CardVisible) problems.Add(p + "kod karti gizli kaldi");
        if (hud.SkipButtonOpen) problems.Add(p + "Atla dugmesi acik kaldi");
        if (!robot.gameObject.activeSelf) problems.Add(p + "robot gizli kaldi");
        if ((robot.transform.localPosition - Pos(level.Robot)).sqrMagnitude > 1e-4f) problems.Add(p + "robot baslangic karesinde degil");
        var sparrow = RiverBedTraces.ActiveSparrow;
        if (sparrow == null || !sparrow.gameObject.activeSelf) problems.Add(p + "Serce yok ya da gizli");
        else if ((sparrow.transform.localPosition - sparrow.HomePosition).sqrMagnitude > 1e-4f) problems.Add(p + "Serce evine donmedi");
    }

    IEnumerator LoadRiverLevel(int number)
    {
        LoadLevel(levels.FindIndex(l => l.Number == number));
        yield return new WaitForSeconds(0.8f);
    }

    void Expect(List<string> problems, int number, string outro)
    {
        if (level.Outro != outro) problems.Add(number + " bitis damgasi '" + level.Outro + "', sahne '" + outro + "'");
    }

    int CountNamed(string name)
    {
        int n = 0;
        foreach (var t in levelRoot.GetComponentsInChildren<Transform>(true))
            if (t.name == name) n++;
        return n;
    }

    // 69: Kivilcim'in sirtinda Ece'nin kalemi (kalici karakter gorunusu); arkadan ve yukaridan yakin plan
    IEnumerator CrayonCloseup(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == RiverBedTraces.HandsLevel));
        yield return new WaitForSeconds(0.6f);
        robot.transform.rotation = Quaternion.identity;   // kuzeye bakar: sirti kameraya
        var look = robot.RibbonPoint;
        var from = look + new Vector3(0.9f, 1.9f, -3.4f);
        shot.Begin();
        shot.Cut(from, Quaternion.LookRotation(look - from), 24f);
        yield return new WaitForSeconds(1.2f);
        Cap(Path.Combine(dir, "s69-sirt-kalem.png"));
        yield return new WaitForSeconds(0.4f);
        shot.Release();
        if (!HasCrayon()) problems.Add("69'da sirtta kalem yok");
    }

    bool HasCrayon()
    {
        foreach (var t in robot.GetComponentsInChildren<Transform>(false))
            if (t.name == "Kalem") return true;
        return false;
    }

    // 60: normal bakis, yariga bakis, Kivilcim ve Serce raftayken (boy olcusu), dibin kenarindan inmeden once ve sonra
    IEnumerator RiftShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "s60-0-normal.png"));
        yield return new WaitForSeconds(0.4f);   // goruntu kare sonunda alinir: sonraki degisiklik ona karismasin
        var rift = CanyonTraces.Rift;
        if (rift == null) { problems.Add("60'ta yarik yok"); yield break; }
        if (rift.BendIndex <= 1 || rift.BendIndex >= rift.Trail.Count) problems.Add("donemec noktasi patikada degil");

        RiftCollisions(rift, problems);
        shot.Begin();
        shot.Cut(rift.ViewPoint, Quaternion.LookRotation(rift.ViewTarget - rift.ViewPoint), CanyonRift.ViewFov);
        yield return new WaitForSeconds(1.5f);   // arka plan yeniden cizilsin
        Cap(Path.Combine(dir, "s60-1-asagi.png"));
        yield return new WaitForSeconds(0.4f);

        // Boy olcusu: Kivilcim rafin ortasinda, Serce ustunde; sonra donemecten once ve sonra (sonrakinde gorunmemeli)
        yield return PoseOnTrail(rift, 1, dir, "s60-2a-bas.png");   // yarigin basi: kayalar robota/Serce'ye degmemeli
        yield return PoseOnTrail(rift, 3, dir, "s60-2b-bas.png");
        yield return PoseOnTrail(rift, rift.Trail.Count / 3, dir, "s60-2-raf.png");
        yield return PoseOnTrail(rift, rift.BendIndex - 1, dir, "s60-3-donemec-once.png");
        yield return PoseOnTrail(rift, rift.BendIndex, dir, "s60-4-donemec-sonra.png");
        shot.Release();
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));   // robot ve Serce yerine
        yield return new WaitForSeconds(0.5f);
    }

    // Robot ve Serce yol boyunca (her ~10 cm) yerlestirilir; govdelerinin hicbir parcasi yarigin duvarina, rafina (dudak, alt yuz) ya da
    // kayalarina girmemeli (kaya artik yok). Yarik aglarina gecici MeshCollider eklenir, her parcanin kutusu (kuculterek) sorgulanir. Yalniz ilk
    // birkac ihlal yazilir: hangi nesne, yarigin neresinde (t) ve hangi parca.
    void RiftCollisions(CanyonRift rift, List<string> problems)
    {
        var temporary = new List<MeshCollider>();
        foreach (var mf in rift.GetComponentsInChildren<MeshFilter>(true))
        {
            var c = mf.gameObject.AddComponent<MeshCollider>();
            c.sharedMesh = mf.sharedMesh;
            temporary.Add(c);
        }
        Physics.SyncTransforms();
        var walls = new HashSet<Collider>(temporary);
        var sparrow = CanyonTraces.ActiveSparrow;
        var robotParts = robot.GetComponentsInChildren<Renderer>();
        var sparrowParts = sparrow != null ? sparrow.GetComponentsInChildren<Renderer>() : new Renderer[0];
        int reported = 0;

        string Hit(Renderer[] parts, bool ignoreFloor, Vector3 around)
        {
            foreach (var part in parts)
            {
                if (!part.enabled || !part.gameObject.activeInHierarchy) continue;
                var b = part.bounds;
                if (b.extents.y < 0.03f || part.name == "Thruster") continue;   // ince halka, golge; ayak jetleri robotun yere basma yeri (duvar mesh'inin yatay seridi zemin seviyesinde kalir)
                foreach (var hit in Physics.OverlapBox(b.center, b.extents * 0.8f))
                {
                    if (!walls.Contains(hit)) continue;
                    if (ignoreFloor && (hit.name == "Patika" || hit.name == "Dip")) continue;   // robotun bastigi yer
                    var off = rift.transform.InverseTransformVector(b.center - around);   // parcanin yol noktasina gore yeri (yarik yerelinde)
                    // Duvar parcaya hangi yonden giriyor: dort yatay ve iki dikey yonde ilk duvar mesafesi (cm)
                    var sides = "";
                    foreach (var (label, dir) in new[] { ("+x", Vector3.right), ("-x", Vector3.left), ("+z", Vector3.forward), ("-z", Vector3.back), ("+y", Vector3.up), ("-y", Vector3.down) })
                        if (Physics.Raycast(b.center, dir, out var ray, 1.2f) && walls.Contains(ray.collider)) sides += " " + label + ":" + ray.collider.name.Replace("Duvar-", "D") + "@" + Mathf.RoundToInt(ray.distance * 100f);
                    return hit.name + " <- " + part.name + " (x " + off.x.ToString("+0.00;-0.00") + " y " + off.y.ToString("+0.00;-0.00") + ")" + sides;
                }
            }
            return null;
        }

        for (int i = 0; i < rift.Trail.Count - 1 && reported < 40; i++)
            for (float u = 0f; u < 1f && reported < 40; u += 0.2f)
            {
                var local = Vector3.Lerp(rift.Trail[i], rift.Trail[i + 1], u);
                var ahead = rift.transform.TransformPoint(rift.Trail[i + 1]) - rift.transform.TransformPoint(rift.Trail[i]);
                ahead.y = 0f;
                robot.transform.position = rift.transform.TransformPoint(local);
                if (ahead.sqrMagnitude > 1e-6f) robot.transform.rotation = Quaternion.LookRotation(ahead);
                if (sparrow != null) sparrow.transform.position = rift.transform.TransformPoint(rift.AirBeside(local));
                var robotHit = Hit(robotParts, ignoreFloor: true, around: robot.transform.position);
                var sparrowHit = Hit(sparrowParts, ignoreFloor: false, around: sparrow != null ? sparrow.transform.position : Vector3.zero);
                if (robotHit != null) { problems.Add("yarik: robot t=" + (local.z - rift.Trail[0].z).ToString("0.0") + " icine giriyor: " + robotHit); reported++; }
                if (sparrowHit != null) { problems.Add("yarik: Serce t=" + (local.z - rift.Trail[0].z).ToString("0.0") + " icine giriyor: " + sparrowHit); reported++; }
            }
        // Yarikta kaya yok: hicbir 'Kaya' nesnesi kalmamali
        foreach (var r in rift.GetComponentsInChildren<Renderer>(true))
            if (r.name == "Kaya" || r.name == "Tas") problems.Add("yarik: kaya var (" + r.name + ")");
        foreach (var c in temporary) Destroy(c);
    }

    // 60'in kapanis sahnesi: dogru cozumu calistirir, evreler gelince kare alir; sonra sahne ortasinda "Tekrar" geri alma denetimi
    IEnumerator DescentShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        SetCode(level.Solution);
        OnRun();
        yield return new WaitForSeconds(0.5f);
        hud.FocusCode(0, 0);   // klavye acik: sahne basinda kapanmali (kareler klavyesiz olmali)
        foreach (var (phase, wait, file) in new[] { ("yol", 0f, "s60-5-yol.png"), ("bakis", 1.5f, "s60-6-bakis.png"), ("inis", 1.5f, "s60-7-inis.png"), ("son", 1f, "s60-8-son.png") })
        {
            yield return WaitForPhase(phase, problems);
            yield return new WaitForSeconds(wait);
            Cap(Path.Combine(dir, file));
            yield return new WaitForSeconds(0.4f);
        }

        // Geri alma: sahne ortasinda "Tekrar" her seyi yerine koymali
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        yield return new WaitForSeconds(1f);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("yol", problems);
        yield return WaitForPhase("bakis", problems);
        yield return WaitForPhase("inis", problems);
        ResetLevel();
        yield return null;
        if (shot.Active) problems.Add("Tekrar: sahne kamerasi birakilmadi");
        if (!robot.gameObject.activeSelf) problems.Add("Tekrar: robot gizli kaldi");
        if ((robot.transform.localPosition - Pos(level.Robot)).sqrMagnitude > 1e-4f) problems.Add("Tekrar: robot baslangic karesinde degil");
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null || !sparrow.gameObject.activeSelf) problems.Add("Tekrar: Serce yok ya da gizli");
        else if ((sparrow.transform.localPosition - sparrow.HomePosition).sqrMagnitude > 1e-4f) problems.Add("Tekrar: Serce evine donmedi");
        yield return new WaitForSeconds(0.1f);   // 0,5 sn sonra denetim ortaminda (sahnesiz bolumlerde de) toz topu buyuk gorunuyor: bkz. ILERLEME.md
        Cap(Path.Combine(dir, "s60-9-tekrar.png"));
    }

    // 55'in sahnesi: dogru cozumu calistirir, evreler gelince kare alir; sonra sahne ortasinda "Tekrar" geri alma denetimi
    IEnumerator FlightShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.SparrowRepairLevel));
        yield return new WaitForSeconds(1f);
        Cap(Path.Combine(dir, "s55-0-onceki.png"));
        yield return new WaitForSeconds(0.4f);
        yield return SparrowCloseup(dir, "s55-6-yakin.png");
        SetCode(level.Solution);
        OnRun();
        hud.FocusCode(0, 0);   // klavye acik: gercek oyunda kullanici kodu klavyeyle yazar; sahne kamerasi yine dogru olmali
        foreach (var (phase, wait, file) in new[] { ("uyanis", 1f, "s55-1-uyanis.png"), ("kalkis", 1.5f, "s55-2-kalkis.png"), ("kus", 1f, "s55-3-yukaridan.png"), ("son", 0.5f, "s55-4-son.png") })
        {
            yield return WaitForPhase(phase, problems);
            yield return new WaitForSeconds(wait);
            Cap(Path.Combine(dir, file));
            yield return new WaitForSeconds(0.4f);
        }

        // Geri alma: kus evresinde "Tekrar" kamerayi birakmali, Serce rafta sonuk kalmali
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.SparrowRepairLevel));
        yield return new WaitForSeconds(1f);
        SetCode(level.Solution);
        OnRun();
        yield return WaitForPhase("kus", problems);
        ResetLevel();
        yield return null;
        if (shot.Active) problems.Add("55 Tekrar: sahne kamerasi birakilmadi");
        if (!robot.gameObject.activeSelf) problems.Add("55 Tekrar: robot gizli kaldi");
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null) problems.Add("55 Tekrar: Serce yok");
        else if ((sparrow.transform.localPosition - sparrow.HomePosition).sqrMagnitude > 1e-4f) problems.Add("55 Tekrar: Serce rafina donmedi");
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "s55-5-tekrar.png"));
    }

    // 51'in roket inisi (denetimde bolum yuklenirken oynamaz: sahne elle baslatilir): evreler gelince kare alir;
    // sonra ucus sirasinda dokunma (atlatma) denetimi: her sey son haline gelmeli
    IEnumerator LandingShots(string dir, List<string> problems)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.FirstLevel));
        yield return new WaitForSeconds(1f);
        canyonScenes.PrepareLanding();
        StartLanding();
        foreach (var (phase, wait, file) in new[] { ("ucus", 0.8f, "s51-1-ucus.png"), ("ucus", 1.2f, "s51-2-alcalma.png"), ("indi", 0.3f, "s51-3-indi.png"),
                     ("kapi", 0.5f, "s51-4-kapi.png"), ("son", 0.3f, "s51-5-son.png") })
        {
            yield return WaitForPhase(phase, problems);
            yield return new WaitForSeconds(wait);
            Cap(Path.Combine(dir, file));
            yield return new WaitForSeconds(0.4f);
        }
        if (hud.SkipButtonOpen || landing != null) problems.Add("51: sahne bitti ama Atla dugmesi/sahne acik kaldi");

        // Atlatma: ucus evresinde (1 sn sonra) "Atla"
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.FirstLevel));
        yield return new WaitForSeconds(1f);
        canyonScenes.PrepareLanding();
        StartLanding();
        yield return WaitForPhase("ucus", problems);
        yield return new WaitForSeconds(1.3f);
        if (!hud.SkipButtonOpen) problems.Add("51 atlatma: Atla dugmesi acik degil");
        canyonScenes.Skip();
        yield return null;
        yield return null;
        var rocket = CanyonTraces.ActiveRocket;
        if (rocket == null) problems.Add("51 atlatma: roket yok");
        else if ((rocket.transform.localPosition - rocket.HomePosition).sqrMagnitude > 1e-4f) problems.Add("51 atlatma: roket yerine oturmadi");
        if (!robot.gameObject.activeSelf) problems.Add("51 atlatma: robot gizli kaldi");
        if ((robot.transform.localPosition - Pos(level.Robot)).sqrMagnitude > 1e-4f) problems.Add("51 atlatma: robot baslangic karesinde degil");
        if (canyonScenes.Phase != "son") problems.Add("51 atlatma: evre 'son' degil (" + canyonScenes.Phase + ")");
        if (hud.SkipButtonOpen || landing != null) problems.Add("51 atlatma: Atla dugmesi kapanmadi ya da sahne bitmedi");
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "s51-6-atlatma.png"));
        yield return new WaitForSeconds(0.4f);
    }

    // 51'in roket inisi bolume her girisde oynamali (oyun gibi, denetim modu kapali): bolum iki kez yuklenir, ikisinde de roket
    // havada beklemeli (gecis yazilarindan sonra iner). Son kontrol: arkasindan kare alinmaz, uygulama kapanir.
    IEnumerator LandingEveryEntryCheck(List<string> problems)
    {
        shotsMode = false;
        for (int entry = 1; entry <= 2; entry++)
        {
            LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.FirstLevel));
            var rocket = CanyonTraces.ActiveRocket;
            if (rocket == null || rocket.transform.localPosition.y < rocket.HomePosition.y + 1f)
                problems.Add("51 her giriste: " + entry + ". yuklemede roket havada beklemiyor");
            if (robot.gameObject.activeSelf) problems.Add("51 her giriste: " + entry + ". yuklemede robot gizli degil");
            yield return new WaitForSeconds(0.3f);
        }
        shotsMode = true;
    }

    // Serce'nin yakin plani (adi okunuyor mu): oyun kamerasi gibi onden ve yukaridan
    IEnumerator SparrowCloseup(string dir, string file)
    {
        var sparrow = CanyonTraces.ActiveSparrow;
        if (sparrow == null) yield break;
        shot.Begin();
        shot.Cut(sparrow.transform.position + new Vector3(0f, 1.1f, -1.0f), Quaternion.LookRotation(new Vector3(0f, -1f, 1f)), 25f);
        yield return new WaitForSeconds(1.2f);
        Cap(Path.Combine(dir, file));
        yield return new WaitForSeconds(0.4f);
        shot.Release();
    }

    // Sahnenin evresi gelene kadar bekler (en cok 40 sn). current: hangi sahnenin evresi (verilmezse Bolge 6)
    IEnumerator WaitForPhase(string phase, List<string> problems, Func<string> current = null, float limit = 40f)
    {
        current ??= () => canyonScenes.Phase;
        for (float t = 0f; current() != phase; t += Time.deltaTime)
        {
            if (t > limit) { problems.Add(phase + " evresi gelmedi (su an: '" + current() + "')"); yield break; }
            yield return null;
        }
    }

    string RiverPhase() => riverScenes.Phase;

    IEnumerator PoseOnTrail(CanyonRift rift, int index, string dir, string file)
    {
        var at = rift.transform.TransformPoint(rift.Trail[index]);
        var next = rift.transform.TransformPoint(rift.Trail[Mathf.Min(index + 1, rift.Trail.Count - 1)]);
        var ahead = next - at; ahead.y = 0f;
        robot.transform.position = at;
        if (ahead.sqrMagnitude > 1e-6f) robot.transform.rotation = Quaternion.LookRotation(ahead);
        if (CanyonTraces.ActiveSparrow != null) CanyonTraces.ActiveSparrow.transform.position = at + Vector3.up * 1.1f;
        yield return new WaitForSeconds(0.6f);
        Cap(Path.Combine(dir, file));
        yield return new WaitForSeconds(0.4f);
    }

    // Klavye acikken (fotograf kucultulmus) sahne kamerasi: Begin'de gokyuzu sicramamali, donuste ilk hale gelmeli
    IEnumerator ShotKeyboardCheck(string dir)
    {
        LoadLevel(levels.FindIndex(l => l.Number == CanyonTraces.LastLevel));
        hud.FocusCode(0, 0);
        yield return new WaitForSeconds(1.2f);
        Cap(Path.Combine(dir, "s60-k0-klavye.png"));
        yield return new WaitForSeconds(0.4f);
        shot.Begin();
        yield return null;
        Cap(Path.Combine(dir, "s60-k1-begin.png"));   // k0 ile ayni olmali
        yield return new WaitForSeconds(0.4f);
        var rift = CanyonTraces.Rift;
        yield return shot.MoveTo(rift.ViewPoint, rift.ViewTarget, 1.2f, CanyonRift.ViewFov);
        Cap(Path.Combine(dir, "s60-k2-klavye-yarik.png"));
        yield return new WaitForSeconds(0.4f);
        yield return shot.Return(1.2f);
        yield return new WaitForSeconds(0.3f);
        Cap(Path.Combine(dir, "s60-k3-klavye-donus.png"));   // k0 ile ayni olmali
        yield return new WaitForSeconds(0.4f);
        hud.StopEditing();
    }
}
