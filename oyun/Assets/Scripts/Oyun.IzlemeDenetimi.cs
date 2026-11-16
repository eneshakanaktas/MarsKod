using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Izleme denetimi: MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -izle <ilk> <son>
// Pencere acikken bolumleri OTOMATIK ve YAVAS dener, ekip uyesi izler; sonuclar ayni anda log'a yazilir ("IZLEME:" satirlari, en sonda
// "IZLEME DENETIMI:"). Her bolumde: bilgi karti varsa kendiliginden acildigi, "Anladim" ve i dugmesi; sonra bolumun her tipik hatasi
// (bolum bitmemeli, oyuncuya hata mesaji gosterilmeli) ve cozum (bolum bitmeli). Ust kenardaki serit (AuditBanner) hangi denemede
// oldugumuzu ve sonucu gosterir. -shots gibi dogrudan ic cagrilarla calisir (fare/klavye taklidi yok); sinav atlanir, sahneler
// "Atla" yoluyla gecilir. Tur bitince pencere kapanmaz, ozet seritte kalir.
public partial class Oyun
{
    // Bekleme sureleri (saniye): izleyen kisi okuyabilsin diye yavas
    const float WatchStartWait = 2.5f, WatchLevelWait = 0.8f, WatchCodeWait = 1.5f, WatchMessageHold = 2.5f, WatchDoneHold = 2f;
    const float WatchInfoHold = 3f, WatchBubbleHold = 1.5f, WatchResultHold = 0.8f;
    // Takilma korumasi: bir deneme / kapanis sahnesi bu kadar surmezse HATA yazilip devam edilir
    const float WatchRunTimeout = 120f, WatchSceneTimeout = 60f;

    // Tur boyunca sayilanlar: her deneme ve bilgi karti kontrolu bir "deneme"dir
    sealed class WatchTally
    {
        public int Total, Errors;
        public readonly List<int> BadLevels = new List<int>();

        public void Record(int levelNumber, bool ok)
        {
            Total++;
            if (ok) return;
            Errors++;
            if (!BadLevels.Contains(levelNumber)) BadLevels.Add(levelNumber);
        }
    }

    // -izle <ilk> <son>: bolum numaralari (ikisi de dahil); tek sayi = yalniz o bolum; sayi yoksa hepsi
    static bool TryReadWatchRange(string[] args, out int first, out int last)
    {
        first = 1; last = int.MaxValue;
        int at = System.Array.IndexOf(args, "-izle");
        if (at < 0) return false;
        if (at + 1 < args.Length && int.TryParse(args[at + 1], out int from))
        {
            first = last = from;
            if (at + 2 < args.Length && int.TryParse(args[at + 2], out int to)) last = to;
        }
        return true;
    }

    IEnumerator WatchAudit(int first, int last)
    {
        yield return new WaitForSeconds(WatchStartWait);
        var tally = new WatchTally();
        hud.ShowAudit("DENETİM başlıyor · Bölüm " + first + (last == int.MaxValue ? " ve sonrası" : "-" + last));
        yield return new WaitForSeconds(1.2f);
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].Number < first || levels[i].Number > last) continue;
            yield return WatchLevel(i, tally);
        }

        bool ok = tally.Total > 0 && tally.Errors == 0;
        string line = tally.Total == 0 ? "IZLEME DENETIMI: HATA (0/0) bu aralikta bolum yok"
            : ok ? "IZLEME DENETIMI: TAMAM (" + tally.Total + " deneme)"
            : "IZLEME DENETIMI: HATA (" + tally.Errors + "/" + tally.Total + ") bolumler: " + string.Join(", ", tally.BadLevels);
        Debug.Log(line);
        string summary = "DENETİM bitti · " + tally.Total + " deneme · " + tally.Errors + " hata";
        if (tally.BadLevels.Count > 0) summary += "\nHatalı bölümler: " + string.Join(", ", tally.BadLevels);
        hud.ShowAudit(summary);   // pencere kapanmaz: ozet ekranda kalir
        hud.FinishAudit(ok);
    }

    IEnumerator WatchLevel(int index, WatchTally tally)
    {
        LoadLevel(index);
        yield return new WaitForSeconds(WatchLevelWait);
        var current = level;
        if (current.Info != null) yield return WatchInfoCard(tally);
        int total = current.Mistakes.Count + 1;
        for (int k = 0; k < current.Mistakes.Count; k++)
            yield return WatchAttempt(tally, k + 1, total, current.Mistakes[k].Code, expectPass: false);
        yield return WatchAttempt(tally, total, total, current.Solution, expectPass: true);
    }

    // Bolume girince bilgi karti kendiliginden acilmali; "Anladim"dan sonra i dugmesi gorunmeli
    IEnumerator WatchInfoCard(WatchTally tally)
    {
        int number = level.Number;
        hud.ShowAudit("DENETİM · Bölüm " + number + " · bilgi kartı");
        bool opened = hud.InfoCardOpen;
        yield return new WaitForSeconds(WatchInfoHold);
        hud.CloseInfo();   // "Anladım"ın yaptığı iş
        yield return new WaitForSeconds(0.4f);
        bool button = hud.InfoButtonVisible;
        yield return new WaitForSeconds(WatchBubbleHold);   // "buradan tekrar açabilirsin" balonu
        hud.HideInfoBubble();

        bool ok = opened && button;
        Debug.Log("IZLEME: bolum=" + number + " bilgi_karti=" + (opened ? "acildi" : "acilmadi") + " i_dugmesi=" + (button ? "var" : "yok") + " " + (ok ? "TAMAM" : "HATA"));
        tally.Record(number, ok);
        hud.FinishAudit(ok);
        yield return new WaitForSeconds(WatchResultHold);
    }

    // Tek deneme: kodu yaz, bekle, Calistir, bitince beklenenle karsilastir, mesaji ekranda tut
    IEnumerator WatchAttempt(WatchTally tally, int number, int total, string source, bool expectPass)
    {
        int levelNumber = level.Number;
        hud.ShowAudit("DENETİM · Bölüm " + levelNumber + " · " + number + "/" + total + " · " + (expectPass ? "çözüm" : "tipik hata")
            + " · beklenen: " + (expectPass ? "geçer" : "geçmez"));
        SetCode(source);
        yield return new WaitForSeconds(WatchCodeWait);   // yazilan kod okunsun
        OnRun();   // "Çalıştır"ın yaptığı iş
        for (float t = 0f; !done && t < WatchRunTimeout; t += Time.deltaTime)
        {
            if (hud.SkipButtonOpen) SkipRunningScenes();   // kosu sirasindaki sahneler (78) en kisa yoldan
            yield return null;
        }

        bool finished = done;
        bool passed = finished && complete;
        string message = hud.MessageOpen ? WatchPlainText(hud.MessageSummary) : "";
        string problem = !finished ? "zaman asimi"
            : passed != expectPass ? (passed ? "bolum gecti, gecmemeliydi" : "bolum gecmedi, gecmeliydi")
            : !passed && message.Length == 0 ? "hata mesaji gorunmedi"
            : null;
        bool ok = problem == null;
        Debug.Log("IZLEME: bolum=" + levelNumber + " deneme=" + number + " tur=" + (expectPass ? "cozum" : "tipik")
            + " beklenen=" + (expectPass ? "gecer" : "gecmez") + " sonuc=" + (!finished ? "zamanasimi" : passed ? "gecer" : "gecmez")
            + " " + (ok ? "TAMAM" : "HATA") + (ok ? "" : " neden=\"" + problem + "\"") + " mesaj=\"" + message + "\"");
        tally.Record(levelNumber, ok);
        hud.FinishAudit(ok);

        // oyuncunun gordugu ekran: hata/uyari mesaji ya da "Tamamlandı"
        yield return new WaitForSeconds(passed ? WatchDoneHold : WatchMessageHold);
        if (passed)   // kapanis/hikaye sahneleri en kisa yoldan
            for (float t = 0f; running && t < WatchSceneTimeout; t += Time.deltaTime)
            {
                if (hud.SkipButtonOpen) SkipRunningScenes();
                yield return null;
            }
    }

    // Sahnenin "Atla" dugmesinin yaptigi is (butun sahne yoneticileri; calisan yoksa zararsiz)
    void SkipRunningScenes()
    {
        SkipClosing();
        canyonScenes.Skip();
        riverScenes.Skip();
        stationScenes.Skip();
    }

    // Log icin: renk etiketleri atilir, satir sonlari bosluk olur, tirnak isareti kesme olur
    static string WatchPlainText(string rich) =>
        Regex.Replace(Regex.Replace(rich, "<[^>]+>", ""), "\\s+", " ").Trim().Replace('"', '\'');
}
