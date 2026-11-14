using System;
using System.Collections;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;

// Bolge 7'nin sahneleri (spec 3b): 63 kus bakisi, 68 kalem, 70 final. Akis burada; nesneler (Serce, robot, kalem) yalnizca komut
// alir. Kamera sahne surerken CameraShot'ta; Restore her seyi geri verir. Kivilcim ucmaz: yalnizca bos karelerden yurur (SceneWalker).
public class RiverBedScenes
{
    readonly Robot robot;
    readonly CameraShot shot;
    readonly Hud hud;
    readonly SceneWalker walker;
    readonly Vector2 areaHalf;
    readonly SceneSkip skip;   // K4: sahneler "Atla" dugmesiyle atlanir

    // Tahmini uzunluklar (Atla dugmesinin saniyesi). 68 ve 70'te yuruyus yola gore degisir: ortalama yol alindi.
    const float BirdViewLength = 6.5f, TakeCrayonLength = 9f, FinaleLength = 40f;

    public IEnumerator BirdViewWithSkip(Level level) => skip.Run(BirdView(level), BirdViewLength, FinishBirdView);
    public IEnumerator TakeCrayonWithSkip(Level level) => skip.Run(TakeCrayon(level), TakeCrayonLength, FinishTakeCrayon);
    public IEnumerator FinaleWithSkip(Level level) => skip.Run(Finale(level), FinaleLength, FinishFinale);

    // "Atla" dugmesi (denetim de cagirir)
    public void Skip() => skip.Skip();
    public string Phase { get; private set; } = "";   // goruntu denetimi hangi anda oldugunu buradan okur

    public RiverBedScenes(MonoBehaviour host, Robot robot, CameraShot shot, Hud hud, SceneWalker walker, Vector2 areaHalf)
    {
        skip = new SceneSkip(host, hud);
        this.robot = robot;
        this.shot = shot;
        this.hud = hud;
        this.walker = walker;
        this.areaHalf = areaHalf;
    }

    // 63 kus bakisi (K1): Serce yukselir, bir an onun gozunden tam yukaridan; alan ortada-altta, ustte nehir kivrimi, iki yanda
    // goc izi ekranin kenarina uzanir, Kivilcim hedefte minik. Kuzey ekranin ustunde (normal bakisla ayni yon).
    const float SparrowRise = 0.7f;   // kalkista Serce bu kadar yukselir (ust yazilara girmez)
    // Kameranin yerden yuksekligi ve alanin yanlarindaki pay (nehir kivrimi alanin iki yaninda asagi doner). Zemin uzakta asagi kivrildigi
    // icin alcak ve genis acili kamera ekranin ustunde ufku gorur: yuksek ve dar aci (goruntude ayarlandi).
    const float BirdRiverHeight = 24f, BirdRiverMargin = 4f;
    const float BirdAreaOnScreen = 0.47f;                        // alanin ortasi ekranda alttan bu kadar yukarida

    public IEnumerator BirdView(Level level)
    {
        var sparrow = RiverBedTraces.ActiveSparrow;
        if (sparrow == null) yield break;
        hud.StopEditing();
        Phase = "kalkis";
        robot.StartCoroutine(robot.LookUp());
        yield return sparrow.FlyTo(sparrow.HomePosition + Vector3.up * SparrowRise, 0.9f);
        Phase = "kus";
        shot.Begin();            // once kamera sabitlenir, sonra kart kapanir (Bolge 6 gibi)
        hud.SetCardVisible(false);
        hud.Blink(0.3f);
        yield return Tween.Wait(0.15f);
        var (eye, fov) = BirdEye();
        shot.Cut(eye, Quaternion.Euler(90f, 0f, 0f), fov);
        shot.Sway = 1.2f;
        yield return Tween.Wait(3f);
        hud.Blink(0.3f);
        yield return Tween.Wait(0.15f);
        hud.SetCardVisible(true);   // kart kamera birakilmadan once (CanyonScenes.FirstFlight'taki neden)
        yield return Tween.Wait(0.2f);
        shot.Release();
        Phase = "inis";
        yield return sparrow.FlyTo(sparrow.HomePosition, 1.2f);
        Phase = "son";
    }

    // Kus bakisinin yeri ve gorus acisi: genislik alan + pay; dikeyde alanin ortasi ekranda BirdAreaOnScreen'de
    (Vector3 eye, float fov) BirdEye()
    {
        float fov = CameraShot.FovToFit(areaHalf.x + BirdRiverMargin, BirdRiverHeight);
        float halfHeight = BirdRiverHeight * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float z = (0.5f - BirdAreaOnScreen) * 2f * halfHeight;
        return (new Vector3(0f, BirdRiverHeight, z), fov);
    }

    // 68 kalem (K2): Kivilcim bos karelerden sol kenara, kalemin yanina yurur, egilip alir, omzunun ustunden sirtina takar; takarken
    // kamera kisa bir yakin cekimle arkasina gelir. Yol her kosuda baska yerde biter: sahne robotun o anki karesinden baslar.
    const float CrayonReach = 0.48f;   // robot kalemden bu kadar uzakta durur: egilince sag eli kalemin ustunde (goruntude ayarlandi)
    static readonly Vector3 CrayonShotOffset = new Vector3(2.2f, 2.4f, -1.6f);   // yakin cekim: robotun sag-arka caprazi, yukaridan (sag el alir, sirt da gorunur)
    const float CrayonShotHalfWidth = 0.85f;   // yakin cekimde yatayda gorunen (robot egilince eli ve kalem sigsin)

    public IEnumerator TakeCrayon(Level level)
    {
        var crayon = RiverBedTraces.Crayon;
        if (crayon == null) yield break;
        var sparrow = RiverBedTraces.ActiveSparrow;
        hud.StopEditing();
        Phase = "yol";
        yield return robot.Calm();
        var walk = walker.CellsTo(level, walker.CellAt(robot.transform.localPosition), EdgeCellNear(level, crayon.localPosition.z), itemsTaken: true);
        var toArea = Vector3.right;   // kalem alanin solunda: robot onun alan tarafinda durur
        walk.Add(new Vector3(crayon.localPosition.x, walk[walk.Count - 1].y, crayon.localPosition.z) + toArea * CrayonReach);
        if (sparrow != null) sparrow.StartCoroutine(walker.Follow(sparrow, 0.4f, 1.4f));
        yield return walker.Walk(walk, 0.42f);
        yield return robot.TurnTo(DuneFinale.YawTowards(robot.transform.localPosition, crayon.localPosition), 0.3f);
        if (sparrow != null)
        {
            sparrow.StopAllCoroutines();   // eslik biter; yakin cekimde kameranin onune girmesin diye yerine doner
            sparrow.StartCoroutine(sparrow.FlyTo(sparrow.HomePosition, 1.5f));
        }

        Phase = "alis";
        shot.Begin();
        hud.SetCardVisible(false);
        var look = Vector3.Lerp(robot.RibbonPoint, crayon.position, 0.35f);
        yield return shot.MoveTo(look + robot.transform.rotation * CrayonShotOffset, look, 0.9f,
            CameraShot.FovToFit(CrayonShotHalfWidth, CrayonShotOffset.magnitude));
        yield return robot.TakeToBack(crayon, BackSlot.Crayon);
        Phase = "sirt";
        yield return Tween.Wait(0.8f);
        hud.SetCardVisible(true);   // kart kamera donmeden once (CanyonScenes.FirstFlight'taki neden)
        yield return Tween.Wait(0.2f);
        yield return shot.Return(1f);
        Phase = "son";
    }

    // Sol sutunda (alanin kalem tarafi) satiri z'ye en yakin bos kare; hepsi doluysa robotun bulundugu kare
    Cell EdgeCellNear(Level level, float z)
    {
        int want = walker.CellAt(new Vector3(0f, 0f, z)).Row;
        for (int d = 0; d < 6; d++)
            foreach (int row in new[] { want - d, want + d })
            {
                var open = walker.OpenInRow(level, row, 0, itemsTaken: true);
                if (open.HasValue && open.Value.Col == 0) return open.Value;
            }
        return walker.CellAt(robot.transform.localPosition);
    }

    // 70 final "KIVILCIM BENI BUL" (spec 3b.3): ruzgar yukselirken Kivilcim bos karelerden duvara, el izlerinin yanindaki bos yerin onune
    // yurur; kamera duvara doner, yazi okunur; ruzgar bir anda kesilir (K6), sessizlik; Kivilcim sol elini bos yere koyar, sirtindaki
    // kalemle elinin cevresini cizer (K3, Ragip 2026-11-03: iz kalir), geri cekilir: uc el yan yana. Sonra cicegin sagina gecip sirtini
    // duvara verir: gogsundeki cicek ile duvardaki cicek ayni karede (K4); gozleri turkuaza doner (K5: yalniz 70 sonunda), Serce duvarin
    // ustunde gunese cikar; son kare kalir (K8).
    // Duvar kareleri hafif yukaridan: dik ekranda gorus acisi genis, alcak kamerada yakindaki kayalar on planda dev gorunur
    const float WallShotHalfWidth = 2.1f, WallShotDistance = 4f, WallShotRise = 2.2f;     // yazi + robot cizimi
    const float PanShotHalfWidth = 1.3f, PanShotDistance = 3f, PanShotRise = 1.8f;        // cicek, pasta, eller, Kivilcim
    // Iki eli ancak gogsunun onunde bulusur: Kivilcim bos yerin tam onunde durur. Son adimi yandan atar (onden gelse sol kolu kosedeki
    // direge degerdi).
    // (0,4'te sag eli alanin sagindaki yan kanat duvarina giriyordu: denetimde el x=4,23, kanat yuzu ~4,24)
    static readonly Vector3 LastStepFrom = new Vector3(0.2f, 0f, 0f);
    // Kivilcim elini cizerken: soldan, duvara yakin (onden bakinca govdesi elleri kapatir); kalemli el, duvardaki el, yandaki el izleri
    static readonly Vector3 DrawShotLook = new Vector3(-0.05f, 0.05f, -0.15f), DrawShotFrom = new Vector3(-1.2f, 0.45f, -0.55f);
    const float DrawShotHalfWidth = 0.5f, TraceSeconds = 3f;
    // Cizdikten sonra robot geri ve saga cekilir: govdesi uc eli kapatmaz. Yana adim kisa: 0,45'te sag eli alanin sagindaki yan
    // kanat duvarina giriyordu (denetim: Govde <- Hand, el x=4,23, kanat yuzu ~4,24)
    static readonly Vector3 StepBack = new Vector3(0.25f, 0f, -0.55f);
    // Uc el ve Kivilcim: onden-soldan (Kivilcim sagda, govdesi el izlerini kapatmaz)
    static readonly Vector3 HandsShotFromHands = new Vector3(-0.8f, 0.75f, -2.1f);
    const float HandsShotHalfWidth = 1.0f;
    // Iki cicek: Kivilcim duvardaki cicegin saginda (solunda dursa son karede yaziyi kapatir), sirti duvarda, cicek sag omzunun ustunde;
    // donerken eli duvardaki boyaya degmez. Kamera alcak ve yakin onden.
    const float FlowerSide = 0.5f, BackToWall = 0.46f;
    static readonly Vector3 FlowersShotOffset = new Vector3(0.15f, 0.25f, -1.35f);
    const float FlowersShotHalfWidth = 0.6f;
    // Son kare: duvar resmi ustte, Kivilcim onde (pastanin onunde); kod karti geri gelince ekranin alt yarisi kartin (goruntude ayarlandi)
    const float FinalShotX = 0.5f, FinalShotHalfWidth = 2.4f;
    static readonly Vector3 FinalEyeFromWall = new Vector3(0f, 2.5f, -4.8f), FinalLookFromWall = new Vector3(0f, 0.3f, -1.6f);

    public IEnumerator Finale(Level level)
    {
        if (!RiverBedTraces.Mural.HasValue) yield break;
        var mural = RiverBedTraces.Mural.Value;
        var sparrow = RiverBedTraces.ActiveSparrow;
        hud.StopEditing();
        Sound.Wind();
        Phase = "yol";
        yield return robot.Calm();
        if (sparrow != null) sparrow.StartCoroutine(walker.Follow(sparrow, 0.4f, 1.4f));
        var stand = StandFor(mural.RobotHand);
        yield return WalkViaBackRow(level, stand + LastStepFrom);
        yield return robot.TurnTo(0f, 0.4f);
        yield return robot.MoveTo(stand, 0.6f);
        if (sparrow != null)
        {
            sparrow.StopAllCoroutines();
            sparrow.StartCoroutine(sparrow.FlyTo(sparrow.HomePosition, 1.5f));
        }

        Phase = "duvar";
        shot.Begin();
        hud.SetCardVisible(false);
        var textCenter = Vector3.Lerp(mural.Drawing, mural.TextCenter, 0.65f);
        yield return WallShot(textCenter, WallShotHalfWidth, WallShotDistance, WallShotRise, 3f);
        Phase = "okuma";
        yield return Tween.Wait(2.5f);

        Phase = "sessizlik";
        Sound.Hush();
        yield return WallShot(Vector3.Lerp(mural.Flower, mural.RobotHand, 0.5f), PanShotHalfWidth, PanShotDistance, PanShotRise, 2f);

        Phase = "el";
        robot.StartCoroutine(robot.PalmOnWall(robot.transform.parent.TransformPoint(mural.RobotHand)));
        var drawing = mural.RobotHand + DrawShotLook;
        yield return shot.MoveTo(drawing + DrawShotFrom, drawing, 1.5f, CameraShot.FovToFit(DrawShotHalfWidth, DrawShotFrom.magnitude));

        Phase = "cizim";   // K3: Kivilcim kendi elini Ece'nin ona biraktigi bos yere cizer
        ownHand?.Clear();
        ownHand = new OwnHandPrint(RiverBedTraces.BackWallFace);
        yield return robot.TraceHand(ownHand.Path(), ownHand.DrawTo, TraceSeconds);
        yield return robot.PalmOnWall(null);

        Phase = "geri";    // bir adim geri ve yana: uc el yan yana gorunur
        var hands = Vector3.Lerp(mural.SmallHand, mural.RobotHand, 0.6f);
        robot.StartCoroutine(robot.MoveTo(robot.transform.localPosition + StepBack, 0.8f));
        yield return shot.MoveTo(hands + HandsShotFromHands, hands, 1.5f, CameraShot.FovToFit(HandsShotHalfWidth, HandsShotFromHands.magnitude));
        Phase = "eller";
        yield return Tween.Wait(2f);

        Phase = "donus";
        yield return WalkViaBackRow(level, FlowerStand(mural));
        yield return robot.TurnTo(180f, 0.5f);   // kameraya doner, sirti duvarda

        Phase = "cicekler";
        var flowers = Vector3.Lerp(robot.ChestPoint, mural.Flower, 0.5f);
        yield return shot.MoveTo(flowers + FlowersShotOffset, flowers, 1.5f, CameraShot.FovToFit(FlowersShotHalfWidth, FlowersShotOffset.magnitude));
        yield return Tween.Wait(2.5f);

        Phase = "gozler";
        robot.StartCoroutine(robot.EceEyes(1.2f));
        if (sparrow != null) sparrow.StartCoroutine(sparrow.FlyTo(RiverBedTraces.MuralPerch, 2f));
        var (eye, look, fov) = FinalShot(mural);
        yield return shot.MoveTo(eye, look, 2.5f, fov);
        hud.SetCardVisible(true);   // kamera birakilmaz: son kare kalir; "Tekrar" ya da bolum degisimi Restore ile birakir
        Phase = "son";
    }

    // Robotun duvara bakarken (kuzey) avucu tam spot'a degsin diye duracagi yer
    Vector3 StandFor(Vector3 spot)
    {
        var reach = robot.PalmReach;
        return new Vector3(spot.x - reach.x, robot.transform.localPosition.y, spot.z - reach.z);
    }

    // Duvardaki cicegin saginda, sirti duvara donuk duracagi yer
    Vector3 FlowerStand(WallMural.Spots mural) =>
        new Vector3(mural.Flower.x + FlowerSide, robot.transform.localPosition.y, mural.Flower.z - BackToWall);

    (Vector3 eye, Vector3 look, float fov) FinalShot(WallMural.Spots mural)
    {
        var wall = new Vector3(FinalShotX, 0f, mural.TextCenter.z);
        Vector3 eye = wall + FinalEyeFromWall, look = wall + FinalLookFromWall;
        return (eye, look, CameraShot.FovToFit(FinalShotHalfWidth, Vector3.Distance(eye, look)));
    }

    // Arka sira uzerinden yurur: alanin disindaysa once arka siranin hizasina, sonra bos karelerden to'nun x'ine en yakin arka kareye,
    // oradan yana (gerekirse alanin disina) ve kuzeye, to'ya. Alan ile duvar arasindaki seritte yana yurumez: serit dar, kosedeki direk
    // yolu kapatir (robot yandan direge degerdi).
    IEnumerator WalkViaBackRow(Level level, Vector3 to)
    {
        var here = robot.transform.localPosition;
        var back = walker.OpenInRow(level, walker.BackRow, walker.ColNear(to.x), itemsTaken: true) ?? walker.CellAt(here);
        float backZ = walker.PositionOf(back).z;
        var walk = new List<Vector3>();
        bool outside = Mathf.Abs(here.x) > areaHalf.x || here.z > areaHalf.y;
        if (outside) walk.Add(new Vector3(here.x, here.y, backZ));
        walk.AddRange(walker.CellsTo(level, walker.CellAt(outside ? walk[0] : here), back, itemsTaken: true));
        walk.Add(new Vector3(to.x, here.y, backZ));
        walk.Add(to);
        yield return walker.Walk(walk, 0.45f);
    }

    // Kamera duvarin onunde, center'a bakar; yatayda halfWidth sigar. rise: kameranin center'dan yuksekligi
    IEnumerator WallShot(Vector3 center, float halfWidth, float distance, float rise, float seconds)
    {
        var eye = center + new Vector3(0f, rise, -distance);
        yield return shot.MoveTo(eye, center, seconds, CameraShot.FovToFit(halfWidth, Vector3.Distance(eye, center)));
    }

    OwnHandPrint ownHand;   // Kivilcim'in duvara cizdigi eli; "Tekrar" siler

    // Denetim: Kivilcim'in eli duvarda tam cizildi mi
    public bool OwnHandDrawn => ownHand != null && ownHand.Complete;


    // 63'un son hali hemen: Serce yerinde, kamera normal, kart gorunur, kirpisma yok
    void FinishBirdView() => BackToNormal();

    // Atlanan sahnenin ortak sonu: Serce evinde, kirpisma yok, kart gorunur, kamera normal
    void BackToNormal()
    {
        var sparrow = RiverBedTraces.ActiveSparrow;
        if (sparrow != null)
        {
            sparrow.StopAllCoroutines();
            sparrow.transform.localPosition = sparrow.HomePosition;
        }
        hud.ClearBlink();
        hud.SetCardVisible(true);
        shot.Release();
        Phase = "son";
    }

    // 68'in son hali hemen: kalem sirtta (yerdeki gizli), robot durdugu yerde sakin, Serce evinde, kamera normal
    void FinishTakeCrayon()
    {
        RiverBedTraces.ReturnCrayon();   // elde kaldiysa yerine; sonra gizlenir
        if (RiverBedTraces.Crayon != null) RiverBedTraces.Crayon.gameObject.SetActive(false);
        robot.ResetTo(robot.transform.localPosition, robot.Yaw);
        robot.SetCrayon(true);
        BackToNormal();
    }

    // Son kare hemen: Kivilcim ciceklerin onunde kameraya donuk, gozler turkuaz, Serce duvarin ustunde, kamera son kadrajda, ses kesik
    void FinishFinale()
    {
        Sound.Hush();
        if (!RiverBedTraces.Mural.HasValue) return;
        var mural = RiverBedTraces.Mural.Value;
        robot.ResetTo(FlowerStand(mural), 180f);   // robotun yarim kalan hareketleri durur
        robot.StartCoroutine(robot.EceEyes(0f));
        ownHand ??= new OwnHandPrint(RiverBedTraces.BackWallFace);
        ownHand.DrawAll();
        var sparrow = RiverBedTraces.ActiveSparrow;
        if (sparrow != null)
        {
            sparrow.StopAllCoroutines();
            sparrow.transform.localPosition = RiverBedTraces.MuralPerch;
        }
        shot.Begin();
        var (eye, look, fov) = FinalShot(mural);
        shot.Cut(eye, Quaternion.LookRotation(look - eye), fov);
        hud.SetCardVisible(true);
        Phase = "son";
    }

    // "Tekrar" ve bolum degisimi: sahnenin degistirdigi ne varsa geri verir (Serce'yi RiverBedTraces.ResetBackdrop evine, gozleri
    // Oyun.ResetRobot -> Robot.ResetTo eski rengine dondurur)
    public void Restore()
    {
        skip.Stop();
        ownHand?.Clear();
        ownHand = null;
        RiverBedTraces.ReturnCrayon();   // 68: elde ya da sirtta kalmis kalem yere (sirttaki kalemi Oyun.ResetRobot kaldirir)
        robot.gameObject.SetActive(true);
        hud.SetCardVisible(true);
        hud.ClearBlink();
        hud.HideSkip();
        shot.Release();
        Phase = "";
    }
}
