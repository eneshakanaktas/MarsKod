using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MarsKod.Dunya;
using UnityEngine;

// Degisen alan gecis denetimi (A1, docs/superpowers/specs/2026-11-19-degisen-alan-gezme-design.md).
// MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -duzenler <ilk> <son> <mutlak klasor>
// Aralikta "duzenler" olan her bolumde: gosterge ("TİPİ 1/n" / "METEOR 1/n") gorunuyor mu; gecisten sonra degisen kareler parliyor mu; ▶ ile her duzene gecis (ara kareler + son hal),
// sona gelince basa donus; gecis yarida Tekrar'a basilinca sahne temiz kaliyor mu; dogru cozum iki kez calisir
// (ikincisinde oynatilan duzen degisir: kosu basindaki gecis cekilir) ve iki kosu da bolumu gecmeli.
// Log'da her sorun "DUZEN: bolum=… …", sonda "DUZEN DENETIMI: TAMAM" ya da "HATA (k sorun)".
public partial class Oyun
{
    static readonly float[] GustFrames = { 0.15f, 0.4f, 0.65f, 0.9f, 1.15f };   // ▶'dan sonra cekilen anlar (sn)

    IEnumerator LayoutAudit(int first, int last, string dir)
    {
        shotsMode = suppressQuiz = true;
        ListenWhileUnfocused();
        Directory.CreateDirectory(dir);
        animationsOn = true;   // gecisler oynasin (oyuncunun ayarina yazilmaz)
        yield return new WaitForSeconds(2f);
        int problems = 0, checkedLevels = 0;
        for (int i = 0; i < levels.Count; i++)
        {
            var lv = levels[i];
            if (lv.Number < first || lv.Number > last || lv.Layouts.Count < 2) continue;
            string b = Path.Combine(dir, "d" + lv.Number.ToString("000") + "-");
            int count = lv.Layouts.Count;
            LoadLevel(i);
            yield return new WaitForSeconds(0.8f);
            hud.CloseInfo();
            hud.HideInfoBubble();
            yield return new WaitForSeconds(0.5f);
            problems += Expect(lv.Number, hud.LayoutSwitcherVisible && hud.LayoutText == LayoutName + " 1/" + count, "gosterge yok ya da yanlis: '" + hud.LayoutText + "'");
            yield return CapStill(b + "0-alan1.png");

            // ▶ ile her duzen, sonda basa donus
            for (int step = 1; step <= count; step++)
            {
                int expected = step % count;
                StepLayout(+1);
                yield return CaptureGust(b + step + "-gecis-");
                while (layoutStep != null) yield return null;
                int glowing = LayoutDiff.ChangedCells(level, (expected - 1 + count) % count, expected).Count;
                problems += Expect(lv.Number, LastGlowCount() == glowing, "parlayan kare " + LastGlowCount() + ", beklenen " + glowing);
                yield return new WaitForSeconds(0.3f);
                yield return CapStill(b + step + "-alan" + (expected + 1) + ".png");
                problems += Expect(lv.Number, builtVersion == expected && hud.LayoutText == LayoutName + " " + (expected + 1) + "/" + count,
                    "▶ sonrasi duzen " + builtVersion + ", gosterge '" + hud.LayoutText + "', beklenen " + expected);
                problems += ExpectCleanScene(lv.Number, expected);
            }

            // gecis yarida keserse (Tekrar) sahne temiz kalmali
            StepLayout(+1);
            yield return new WaitForSeconds(0.5f);
            ResetLevel();
            yield return null;
            problems += Expect(lv.Number, layoutStep == null && layoutGust == null, "Tekrar gecisi durdurmadi");
            problems += ExpectCleanScene(lv.Number, builtVersion);

            // dogru cozum iki kez: ikinci kosu baska duzende oynar, basinda gecis olur
            for (int run = 1; run <= 2; run++)
            {
                SetCode(lv.Solution);
                yield return new WaitForSeconds(0.3f);
                OnRun();
                if (run == 2) yield return CaptureGust(b + "kosu2-gecis-");
                while (running) yield return null;
                yield return new WaitForSeconds(0.6f);
                yield return CapStill(b + "kosu" + run + "-bitti.png");
                problems += Expect(lv.Number, complete, run + ". kosu bolumu gecmedi (duzen " + builtVersion + ")");
                problems += Expect(lv.Number, hud.LayoutText == LayoutName + " " + (builtVersion + 1) + "/" + count, run + ". kosuda gosterge '" + hud.LayoutText + "'");
            }
            checkedLevels++;
        }
        Debug.Log("DUZEN DENETIMI: " + (problems == 0 ? "TAMAM (" + checkedLevels + " bolum)" : "HATA (" + problems + " sorun, " + checkedLevels + " bolum)"));
        yield return new WaitForSeconds(0.3f);
        Application.Quit();
    }

    // Kare, cekildigi karenin sonunda yazilir: hemen ardindan sahne degisirse resim yeni hali gosterir. Cek ve bir an bekle.
    static IEnumerator CapStill(string path)
    {
        Cap(path);
        yield return new WaitForSeconds(0.15f);
    }

    IEnumerator CaptureGust(string prefix)
    {
        float waited = 0f;
        for (int f = 0; f < GustFrames.Length; f++)
        {
            yield return new WaitForSeconds(GustFrames[f] - waited);
            waited = GustFrames[f];
            Cap(prefix + (f + 1) + ".png");
        }
    }

    // Gecisten sonra: topak/eski duzen kalmamis; bu duzenin her dolu karesinde gorunen bir nesne var, bos karede nesne kalmamis.
    // Bir nesne birkac parcadan olabilir (kaya = buyuk + kucuk tas): parcalar karesinin yakininda sayilir.
    int ExpectCleanScene(int number, int version)
    {
        const float Near = 0.45f;
        int problems = Expect(number, levelRoot.Find("Duzen-Gecisi") == null, "gecis topaklari silinmedi");
        var cells = level.IcesOf(version).Concat(level.CrystalsOf(version)).Concat(level.RocksOf(version)).Concat(level.CratersOf(version))
            .Select(c => Pos(c)).ToList();
        var parts = new List<Transform>();
        if (layoutRoot != null) foreach (Transform t in layoutRoot) parts.Add(t);
        if (meteorMarks.Current != null) foreach (Transform t in meteorMarks.Current) parts.Add(t);
        bool Shown(Transform t) => t.gameObject.activeInHierarchy && t.localScale.sqrMagnitude > 0.01f;
        bool Close(Vector3 a, Vector3 b) => Mathf.Abs(a.x - b.x) < Near && Mathf.Abs(a.z - b.z) < Near;
        foreach (var cell in cells)
            problems += Expect(number, parts.Any(t => Shown(t) && Close(t.localPosition, cell)), "dolu karede (" + cell.x + ", " + cell.z + ") gorunen nesne yok");
        foreach (var t in parts)
            problems += Expect(number, cells.Any(c => Close(t.localPosition, c)), t.name + " bos karede kalmis (" + t.localPosition.x + ", " + t.localPosition.z + ")");
        return problems;
    }

    // En son kurulan "degisen kareler" parlamasinda kac kare var (yoksa -1)
    int LastGlowCount()
    {
        for (int i = levelRoot.childCount - 1; i >= 0; i--)
            if (levelRoot.GetChild(i).name == "Degisen-Kareler") return levelRoot.GetChild(i).childCount;
        return -1;
    }

    static int Expect(int number, bool ok, string message)
    {
        if (!ok) Debug.Log("DUZEN: bolum=" + number + " " + message);
        return ok ? 0 : 1;
    }
}
