# Bölge 7 Hikâyesi (Bölüm 61-70) Uygulama Planı — 2. parça

> **Yürütme:** Görevleri sırayla yap, adımlar onay kutusu (`- [ ]`) biçiminde. Yardımcı ajan (subagent) yok; işi aynı sohbet yapar (Enes'in kararı, ILERLEME.md).
> **Model · efor:** Sonnet 5.5 · medium (~2-3 sa). Görev 4 (Kayıtlar sayfası, arayüz) takılırsa high.
> **Onay:** Hikâye metinleri ve hikâye görüntüleri Ragıp'ın alanı. Görev 3 ve Görev 4'ün sonunda Ragıp'a göster; onaylamadan sonraki göreve geçme.

**Goal:** Bölge 7'nin hikâye katmanını oyuna koymak: 61-70'in etiket/giriş/bitiş damgaları, Bölge 7 açılış satırları, Bölüm 64 başında **Kayıt 3**, menüde **"Kayıtlar"** sayfası (dinlenmiş kayıtları yeniden dinleme) ve hikâye belgeleri (`senaryo-bolge-07.md` yeni; `senaryo-perde-2.md`, `hikaye-kitabi.md` güncel).

**Architecture:** Kayıt metinleri bugün iki ayrı yerde (`Oyun.Bolge5Kayit`, `CanyonTraces.RecordingLines`); Kayıtlar sayfası da onlara ihtiyaç duyacağı için **tek bir kataloğa** taşınır: saf C# `Recordings` (`oyun/Assets/Dunya/`, `dotnet test` ile sınanır). Hangi kaydın dinlendiğini saf C# `RecordingLog` tutar (`HintLog` gibi: `Save/Load` metni, telefona yazma işi `Oyun.cs`'te, anahtar `kayitlar`). Arayüz (`RecordsView`, `LevelSelect` satırı) yalnızca gösterir; listeyi `Oyun.cs` verir. Kayıt çalma tek bir yardımcıda (`Oyun.PlayRecording`) toplanır: Bölüm 52, Bölüm 64 ve sayfadan yeniden dinleme bunu kullanır. Bölüm 50 kapanışı kendi akışını korur (kayıt sürerken robotu taşıyor), yalnızca metni katalogdan alır.

**Tech Stack:** Unity 6 (C# 9, .NET Standard 2.1), xUnit (`motor-test/`, `dotnet test`; başlangıçta 1456 test).

**Spec:** `docs/superpowers/specs/2026-11-02-bolge-7-design.md` (bölüm "2. Hikâye"; karar 6, 7, 9).

## Global Constraints

- `oyun/Assets/Dunya/` saf C#: `UnityEngine` yok, C# 9, .NET Standard 2.1 (motor-test derlerken yakalar).
- Perde 2 biçimi (`senaryo-bolge-06.md`): robotlar konuşmaz; giriş/bitiş **cümle değil kayıt damgası**; büyük başlıktaki görev en çok 22 harf (61-70'te zaten 17-21, değişmez).
- Ses kaydı yalnızca Defne'nin; öğrenciye ("sen") konuşur. `scout()` bir yön döndürür, konuşma değildir.
- Ece'nin rengi **turkuaz**; "mavi" yalnızca Dünya'nın yıldızına (Bölge 10) kalır. Belgelerde Bölge 7 için "mavi kalem/mavi boya/mavi ışık" kalmaz.
- Dinlenmemiş kayıt Kayıtlar sayfasında görünmez (sürpriz bozulmaz); hiç kayıt dinlenmemişse menüde "Kayıtlar" satırı da yok.
- Görünüş işleri (çakıl, iz, kalem, duvar yazısı, 63/68/70 sahneleri) **bu planda yok**: 3a/3b. 69'un "El izi: 7 küçük, 1 büyük" damgası 3a'daki çizimle aynı sayıda olmalı (3a planına not).
- Yeni `.cs` dosyasının `.meta`'sı Unity paketi üretilince oluşur, git'e onunla girer.
- **Commit kuralı (CLAUDE.md, ekip döngüsü):** görev başına commit yok; oturum kapanışında ILERLEME.md ile birlikte tek commit + push.
- Kod kalitesi (CLAUDE.md): tek iş yapan sınıf/fonksiyon, tekrar yok, okunur adlar, gereksiz katman yok.

## Plan sırasında verilen kararlar (spec'e ek)

1. **Kayıt 3'ün dosya adı `izler.ses`** (Kayıt 1 `ogretmene.ses`, Kayıt 2 `rutinler.ses`: konuya göre ad). Ragıp başka ad isterse yalnızca katalogdaki satır değişir.
2. **Kayıt 3 dört satır** (ekranda satır satır):
   `Ece yine önden koştu.` / `Bir anlığına onu kaybettim.` / `Sonra buldum; bir duvarın önünde, elinde boya kalemi.` / `Ona kızamadım.`
3. **Kayıt 3 Bölüm 64 ilk kez açılınca, bir kez** (Kayıt 2 ile aynı kural: bölüm kurulur → kayıt → ekran açılır → giriş damgası). "Tekrar" ve sonraki girişler yeniden oynatmaz; yeniden dinlemek Kayıtlar sayfasından.
4. **Bölge 7 açılışı** (61 açılırken kara ekranda, Bölge 6'daki gibi): `Yeni bölge: Kanyon dibi` / `Hedef: göç izleri`. Spec'te yoktu; Bölge 2-6'nın hepsinde var.
5. **Kayıt metinleri tek yerde:** `Recordings` kataloğu (saf C#). Kayıt 1 ve 2'nin metinleri `Oyun.cs` ve `CanyonTraces.cs`'ten oraya **taşınır** (kopyalanmaz). Belgeler (`senaryo-perde-1.md`, `senaryo-bolge-06.md`) metnin kaynağı olarak kalır; test katalogdaki Kayıt 3'ü spec'teki metinle karşılaştırır.
6. **"Kayıtlar"ın yeri:** menü düğmesi (≡) Bölümler ekranını açar; ayar satırlarının altında, bölüm listesinin üstünde **"Kayıtlar" satırı** (not: "Dinlediğin kayıtlar: 3"). Dokununca Kayıtlar sayfası açılır. Yeni yuvarlak düğme eklenmez (telefonda yer dar).
7. **Kayıtlar sayfası satırı:** `Kayıt 2 · Bölge 6` (üstte), altında dosya adı ve ilk dört kelime: `rutinler.ses — Ona ezber değil, beceri…`. Numara sırasıyla. Dokununca sayfa kapanır, kayıt ekranı oynar (aynı `RecordingScene`: dokununca sonraki satır), bitince Kayıtlar sayfası yeniden açılır.
8. **Ne zaman "dinlendi" sayılır:** kayıt ekranı son satırını bitirince (`RecordingFinished`). Bölüm 50 kapanışında Kayıt 1 de böyle işaretlenir.
9. **Eski kayıtlı oyuncular:** `kayit2` anahtarı 1 ise Kayıt 2 dinlenmiş sayılır; Bölüm 50 bitirilmişse (`xp.LevelTotal(50) > 0`) Kayıt 1 dinlenmiş sayılır. `kayit2` anahtarı artık yazılmaz (okunur, silinmez).
10. **62'nin başlangıç kodundaki Ece notu** (`# serçe yukarıdan bakar, sen yürü :) - E.`) 1. parçada konmuştu; burada yalnızca testle kilitlenir.

## Review Focus

- `Recordings` + `RecordingLog` saf C# (UnityEngine yok); `Load` bozuk/bilinmeyen numaraları yok sayar → Görev 1 testi.
- Kayıt 1/2 metinleri taşındıktan sonra `Oyun.cs` ve `CanyonTraces.cs`'te kayıt metni **kalmamalı** (tekrar yok) → Görev 2.
- `-shots` modunda kayıt dinlenmiş sayılmamalı ve oyuncunun kaydı değişmemeli (`hints` gibi boş yüklenir, yazılmaz) → Görev 2.
- `-final` denetimi bugün `kayit2` anahtarını saklayıp geri yazıyor; artık `RecordingLog` saklanıp geri konmalı → Görev 2.
- Kayıtlar sayfasından açılan kayıt sırasında Bölümler ekranı ve sayfa kapalı; bitince sayfa yeniden açık → Görev 5 denetimi.
- Hiç kayıt dinlenmemişken Bölümler ekranında "Kayıtlar" satırı yok → Görev 5 denetimi.

---

### Görev 1: Kayıt kataloğu + dinlenen kayıtlar (saf C#)

**Files:**
- Create: `oyun/Assets/Dunya/Recordings.cs` (`Recording` + `Recordings`)
- Create: `oyun/Assets/Dunya/RecordingLog.cs`
- Test: `motor-test/RecordingTests.cs` (yeni)

**Interfaces:**
- Produces:
  - `public sealed class Recording { public readonly int Number, Region, Level; public readonly bool AtLevelStart; public readonly string File; public readonly IReadOnlyList<string> Lines; }` — `Level`: oynadığı bölüm; `AtLevelStart`: bölüm başında mı (52, 64) yoksa kapanışta mı (50).
  - `public static class Recordings { IReadOnlyList<Recording> All; Recording ByNumber(int n) /* yoksa null */; Recording AtStartOf(int level) /* yoksa null */; string Preview(Recording r, int words = 4); }`
  - `public sealed class RecordingLog { bool Heard(int n); void Mark(int n); IReadOnlyList<Recording> HeardList(); bool Any; string Save(); static RecordingLog Load(string text); }` — `Save` biçimi `"1,2,3"` (artan sıra).

- [ ] **Step 1: Failing test yaz** — `motor-test/RecordingTests.cs`:

```csharp
using System.Linq;
using MarsKod.Dunya;
using Xunit;

public class RecordingTests
{
    [Fact]
    public void Katalog_sirali_ve_dolu()
    {
        Assert.Equal(new[] { 1, 2, 3 }, Recordings.All.Select(r => r.Number));
        Assert.Equal(Recordings.All.Count, Recordings.All.Select(r => r.File).Distinct().Count());
        foreach (var r in Recordings.All)
        {
            Assert.NotEmpty(r.Lines);
            Assert.All(r.Lines, l => Assert.False(string.IsNullOrWhiteSpace(l)));
            Assert.EndsWith(".ses", r.File);
        }
    }

    // Spec 2.2'deki metin (hikaye-kitabi.md "Kayıtlar" satırı 3 ile aynı olmalı)
    [Fact]
    public void Kayit_3_uzun_metin()
    {
        var r = Recordings.ByNumber(3);
        Assert.Equal("izler.ses", r.File);
        Assert.Equal((7, 64, true), (r.Region, r.Level, r.AtLevelStart));
        Assert.Equal("Ece yine önden koştu. Bir anlığına onu kaybettim. Sonra buldum; bir duvarın önünde, elinde boya kalemi. Ona kızamadım.",
                     string.Join(" ", r.Lines));
    }

    [Fact]
    public void Bolum_basi_kayitlari()
    {
        Assert.Equal(2, Recordings.AtStartOf(52).Number);
        Assert.Equal(3, Recordings.AtStartOf(64).Number);
        Assert.Null(Recordings.AtStartOf(50));   // Kayıt 1 kapanışta, başta değil
        Assert.Null(Recordings.AtStartOf(61));
        Assert.Null(Recordings.ByNumber(9));
    }

    [Fact]
    public void Onizleme_ilk_kelimeler()
    {
        Assert.Equal("Ona ezber değil, beceri…", Recordings.Preview(Recordings.ByNumber(2)));
        Assert.Equal("Bu bir ödev…", Recordings.Preview(Recordings.ByNumber(1), 3));
        Assert.Equal("Ece yine önden koştu.", Recordings.Preview(Recordings.ByNumber(3)));   // tam dört kelime: üç nokta yok
    }

    [Fact]
    public void Dinlenenler_sirayla_listelenir()
    {
        var log = new RecordingLog();
        Assert.False(log.Any);
        Assert.Empty(log.HeardList());
        log.Mark(3); log.Mark(1); log.Mark(3);
        Assert.True(log.Heard(1));
        Assert.False(log.Heard(2));
        Assert.Equal(new[] { 1, 3 }, log.HeardList().Select(r => r.Number));
        Assert.Equal("1,3", log.Save());
    }

    [Fact]
    public void Kayit_metni_geri_okunur_bozuk_parca_atlanir()
    {
        Assert.Equal("1,3", RecordingLog.Load("3,1").Save());
        Assert.Equal("2", RecordingLog.Load("x,2,,99").Save());   // 99 katalogda yok
        Assert.False(RecordingLog.Load("").Any);
        Assert.False(RecordingLog.Load(null).Any);
    }
}
```

- [ ] **Step 2: Çalıştır, başarısız olduğunu gör** — `cd motor-test && dotnet test --filter RecordingTests` → derleme hatası (sınıflar yok).

- [ ] **Step 3: `Recordings.cs`** — dosya başı yorumu: "Defne'nin kayıtları (hikaye-kitabi.md "Kayıtlar"): metinlerin oyundaki tek yeri. Bölüm 50 kapanışı, bölüm başları (52, 64) ve Kayıtlar sayfası buradan okur. Yeni kayıt = listeye bir satır." `All` listesi:
  - `1, Region 5, Level 50, AtLevelStart false, "ogretmene.ses"`, satırlar: `Oyun.Bolge5Kayit`'taki 8 satır **aynen** (Görev 2'de oradan silinir).
  - `2, Region 6, Level 52, true, "rutinler.ses"`, satırlar: `CanyonTraces.RecordingLines`'taki 4 satır aynen.
  - `3, Region 7, Level 64, true, "izler.ses"`, satırlar: karar 2.
  - `Preview`: ilk satırın ilk `words` kelimesi (boşlukla bölünür); ilk satır daha uzunsa sonuna `…` (U+2026) eklenir.

- [ ] **Step 4: `RecordingLog.cs`** — `HintLog` biçiminde (dosya başı yorumu: "Dinlenen kayıtlar. Saf mantık; telefona yazma işi Scripts'te (`kayitlar` anahtarı)."). İçte `SortedSet<int>`; `Mark` katalogda olmayan numarayı yok sayar; `HeardList` = `Recordings.All` içinden dinlenenler; `Load` virgülle böler, `int.TryParse(..., CultureInfo.InvariantCulture)` + katalogda var mı.

- [ ] **Step 5: Testleri çalıştır** — `cd motor-test && dotnet test` → hepsi geçer.

---

### Görev 2: Oyunu kataloğa bağla + Kayıt 3 + Bölge 7 açılışı

**Files:**
- Modify: `oyun/Assets/Scripts/Oyun.cs`
- Modify: `oyun/Assets/Scripts/CanyonTraces.cs` (kayıt sabitleri kalkar)
- Create: `oyun/Assets/Scripts/RiverBedTraces.cs`
- Modify: `oyun/Assets/Scripts/Traces.cs` (`TransitionLines`)

- [ ] **Step 1: `RiverBedTraces.cs`** — `CanyonTraces`'in başı gibi küçük bir sınıf; 3a görünüşü buraya eklenecek:

```csharp
// Bolge 7 · Kanyon dibi, eski nehir yatagi (Bolum 61-70, parametreli fonksiyonlar): docs/tasarim/senaryo-bolge-07.md.
// Simdilik yalnizca bolge acilisi; gorunus (izler, kalem, duvar yazisi) tasarim belgesi 3a/3b'de buraya eklenir.
public static class RiverBedTraces
{
    public const int FirstLevel = 61, LastLevel = 70;

    // Bolge 7 acilisi: kara ekranda iki satir
    public static readonly string[] TransitionLines = { "Yeni bölge: Kanyon dibi", "Hedef: göç izleri" };
}
```

- [ ] **Step 2: `Traces.TransitionLines`** — zincire `levelNumber == RiverBedTraces.FirstLevel ? RiverBedTraces.TransitionLines :` ekle (CanyonTraces satırının altına). Dosya başı yorumundaki bölge listesine "Bolge 7'ninkiler RiverBedTraces'te" ekle.

- [ ] **Step 3: `CanyonTraces.cs`** — `RecordingLevel`, `RecordingFile`, `RecordingLines` ve "Kayit 2" yorumunu sil; dosya başı yorumundaki "Bolum 52 basindaki Kayit 2" ifadesini kaldır (Kayıt 2 artık `Recordings`'te). `grep -rn "CanyonTraces.Recording" oyun/Assets/Scripts` → kalan her kullanımı Step 5-7'de değiştir.

- [ ] **Step 4: Dinlenen kayıtlar alanı (`Oyun.cs`)** — `hints`'in yüklendiği yerin yanına:

```csharp
// Dinlenen kayitlar (Kayitlar sayfasi). Deneme goruntulerinde oyuncunun kaydi okunmaz/yazilmaz.
recordings = RecordingLog.Load(shotsMode ? "" : PlayerPrefs.GetString(RecordingsKey, ""));
if (!shotsMode) MarkOlderRecordings();
```

  `const string RecordingsKey = "kayitlar";`, `RecordingLog recordings;`. `MarkOlderRecordings()` (karar 9): eski `kayit2` anahtarı 1 ise `Mark(2)`; `xp.LevelTotal(Bolge5SonBolum) > 0` ise `Mark(1)`. `Kayit2Anahtari` sabitini yalnızca bu okumada kullan (yorum: "eski kayit; artik yazilmaz"). `xp` bu satırdan **önce** yüklenmiş olmalı (sırayı kontrol et).
  `void MarkHeard(int n)`: `recordings.Mark(n)`; `shotsMode` değilse `PlayerPrefs.SetString(RecordingsKey, recordings.Save()); PlayerPrefs.Save();`.

- [ ] **Step 5: Ortak kayıt çalma** — `PlayCanyonRecording` yerine:

```csharp
// Defne'nin kaydi: ekran kararir, satir satir; bitince dinlendi sayilir, sessizlik, ekran acilir.
// Bolum basi kayitlari (52, 64) ve Kayitlar sayfasi kullanir; Bolum 50 kapanisi kendi akisinda (robotu tasir).
IEnumerator PlayRecording(Recording r)
{
    Sound.Static();
    hud.ShowRecording(r.File, r.Lines);
    while (!hud.RecordingFinished) yield return null;
    MarkHeard(r.Number);
    yield return Tween.Wait(1.2f);   // sessizlik
    Sound.Static();
    hud.HideRecording();
    yield return Tween.Wait(1f);
}

// Bolum basindaki kayit (52, 64): ilk giriste bir kez; bitince bolumun giris damgasi gelir.
IEnumerator PlayLevelStartRecording(Recording r)
{
    yield return PlayRecording(r);
    hud.ShowIntro(level.Intro);
}
```

  `hud.ShowRecording`'in parametresi `string[]` ise `IReadOnlyList<string>` yap ya da çağrıda `r.Lines.ToArray()` (Hud'da tek satır değişiklik daha temiz: `RecordingScene.Show` de `IReadOnlyList<string>` alır). `LoadLevel`'daki satır:

```csharp
var startRecording = Recordings.AtStartOf(level.Number);
if (!shotsMode && startRecording != null && !recordings.Heard(startRecording.Number))
    StartCoroutine(PlayLevelStartRecording(startRecording));
```

- [ ] **Step 6: Bölüm 50 kapanışı** — `Bolge5Dosya` ve `Bolge5Kayit` sabitlerini sil; `ClosingSceneBolge5`'te `var rec = Recordings.ByNumber(1);` → `hud.ShowRecording(rec.File, rec.Lines)`; `while (!hud.RecordingFinished)` döngüsünden hemen sonra `MarkHeard(rec.Number);`. `Bolge5DosyaSatiri` ("…Bir dosya geldi: ogretmene.ses") metni kalır ama dosya adını katalogdan kur: `"Ödev 50 tamamlandı! Bir dosya geldi: " + Recordings.ByNumber(1).File` (tek yer).

- [ ] **Step 7: `-final` denetimi** (`Oyun.cs` ~1560, "Bolum 52 basindaki Kayit 2"): `CanyonTraces.RecordingLevel` → `Recordings.ByNumber(2).Level`; `PlayCanyonRecording()` → `PlayLevelStartRecording(Recordings.ByNumber(2))`; `kayit2Before` yerine `string recordingsBefore = recordings.Save();` … sonra `recordings = RecordingLog.Load(recordingsBefore);` (shotsMode'da zaten yazılmaz; yorum: "deneme, oyuncunun ilk dinleyisini engellemesin" kalsın).

- [ ] **Step 8: Denetim** — `cd motor-test && dotnet test` (dünya). `grep -n "Kayit\|kayit\|Recording" oyun/Assets/Scripts/Oyun.cs oyun/Assets/Scripts/CanyonTraces.cs` → kayıt metni (Defne'nin cümleleri) yalnızca `Recordings.cs`'te. Unity derlemesi Görev 6'da.

---

### Görev 3: Bölüm 61-70 damgaları + test

**Files:**
- Modify: `oyun/Assets/Resources/Bolumler/bolum-61.json` … `bolum-70.json` (`etiket`, `giris`, `bitis`; `baslik`'ın altına, 51-60'taki sırayla)
- Test: `motor-test/BolumTests.cs`

- [ ] **Step 1: Failing test** — `Bolge_6_bolumleri_kanyon_etiketi_ve_damga_tasir`'ın altına:

```csharp
// senaryo-bolge-07.md: Perde 2 biçimi sürer
[Fact]
public void Bolge_7_bolumleri_kanyon_dibi_etiketi_ve_damga_tasir()
{
    for (int n = 61; n <= 70; n++)
    {
        var level = Load("bolum-" + n + ".json");
        Assert.Equal("BÖLGE 7 · KANYON DİBİ", level.Label);
        Assert.StartsWith("Sol 1.0", level.Intro);
        Assert.False(string.IsNullOrEmpty(level.Outro), n + ": bitis yok");
        Assert.True(level.Goal.Length <= 22, n + ": gorev çok uzun (" + level.Goal.Length + " harf)");
    }
    // 64'ün damgası haritadaki çakıl sayısını söyler
    var b64 = Load("bolum-64.json");
    Assert.Equal("Örnek çakıl: " + b64.Ices.Count, b64.Outro);
    // 62: Ece'nin oyundaki ilk "sesi" başlangıç kodunun ilk satırı
    Assert.StartsWith("# serçe yukarıdan bakar, sen yürü :) - E.\n", Load("bolum-62.json").StartCode);
}
```

  (`Ices` toplanacak nesnelerin listesi, adı eski. `StartCode` satırları `\n` ile birleşmiş tek metin; birleştirme `\r\n` ise testi ona göre düzelt.)

- [ ] **Step 2: Bölüm dosyaları** — her birine `"etiket": "BÖLGE 7 · KANYON DİBİ"` ve aşağıdaki damgalar (spec 2.1; 64'teki çakıl sayısı haritadan: 2):

| Dosya | `giris` | `bitis` |
|---|---|---|
| 61 | `Sol 1.088 · Kanyon dibi` | `Zemin: kurumuş çamur · Eski su izi` |
| 62 | `Sol 1.088 · Serçe yukarıda` | `Yol ayrımı: 1 · Serçe gördü` |
| 63 | `Sol 1.089 · Kumda izler` | `İzler: 23 büyük, 1 küçük` |
| 64 | `Sol 1.089 · Kayıt 3` | `Örnek çakıl: 2` |
| 65 | `Sol 1.090 · İzler ayrılıyor` | `Küçük iz: yan kanyona` |
| 66 | `Sol 1.090 · İkinci iz` | `Adım aralığı: 1,6 m` |
| 67 | `Sol 1.091 · Yan kanyon` | `Kanyon genişliği: 4 m` |
| 68 | `Sol 1.091 · İz: tek kişi, derin` | `Bulundu: boya kalemi` |
| 69 | `Sol 1.092 · Duvarda el izleri` | `El izi: 7 küçük, 1 büyük` |
| 70 | `Sol 1.092 · İzin sonu` | `Sonraki: araştırma istasyonu` |

  Not: 61'in bitişinde spec "eski su izi" küçük harfle; damgalarda `·`'dan sonra büyük harf kullanıldığı için (`Drone parçası: 3/9 · Rutin kaydedildi`) "Eski su izi" yazıldı.

- [ ] **Step 3: Testleri çalıştır** — `cd motor-test && dotnet test` → hepsi geçer.

- [ ] **Step 4: Ragıp'a göster** — tabloyu ve karar 1-4'ü (Kayıt 3 dosya adı/satırları, açılış satırları) Ragıp'a sade dille özetle; "değiştirmek istediğin damga var mı?" diye sor. Onaysız Görev 4'e geçme.

---

### Görev 4: Kayıtlar sayfası

**Files:**
- Create: `oyun/Assets/Scripts/RecordsView.cs`
- Modify: `oyun/Assets/Scripts/LevelSelect.cs` (Kayıtlar satırı)
- Modify: `oyun/Assets/Scripts/Hud.cs` (sayfa + olaylar)
- Modify: `oyun/Assets/Scripts/RecordingScene.cs` (`IReadOnlyList<string>`; Görev 2'de yapılmadıysa)
- Modify: `oyun/Assets/Scripts/Oyun.cs` (liste + yeniden dinleme)

**Interfaces:**
- `public class RecordsView : FullScreenPanel` — `RecordsView(Font fMed, Font fSemi, Font fBold, Font fMono, Color ink, Color accent, Color buttonBg, Color hairline)` (başlık `"Kayıtlar"`), `public event Action<int> Picked`, `public void Show(IReadOnlyList<Entry> entries)`, `public struct Entry { public int Number, Region; public string File, Preview; }`, `public Vector2? RowCenter(int number)` (deneme için, `LevelSelect.RowCenter` gibi).
- `LevelSelect.Show(IReadOnlyList<Entry> entries, int recordCount)`; `public event Action RecordsPressed`; `public Vector2? RecordsRowCenter()` (deneme için; satır yoksa null).
- `Hud`: `public event Action RecordsPressed; public event Action<int> RecordPicked; public void ShowLevelSelect(IReadOnlyList<LevelSelect.Entry> entries, int recordCount); public void ShowRecords(IReadOnlyList<RecordsView.Entry> entries); public void HideRecords(); public bool RecordsOpen;` + deneme için `RecordsRowScreenPoint()`, `RecordRowScreenPoint(int number)` (`PanelToScreen` ile, var olan `LevelRowScreenPoint` gibi).

- [ ] **Step 1: `RecordsView.cs`** — `GlossaryView`/`LevelSelect` biçiminde; dosya başı yorumu: "Kayıtlar sayfası (Bölümler ekranındaki "Kayıtlar" satırından): dinlenmiş Defne kayıtları, numara sırasıyla. Yalnızca gösterir ve seçileni haber verir; liste Oyun.cs'ten gelir (dinlenmemiş kayıt listede yok)." Satır: `LevelSelect`'in bölüm satırıyla aynı ölçü/kenarlık; üstte `Kayıt {Number} · Bölge {Region}` (`fSemi`, 34), altta `{File} — {Preview}` (`fMed`, 26, soluk mürekkep; dosya adı `fMono` olabilir ama tek satırda iki yazı tipi gerekmiyorsa `fMed` yeterli). Sağda küçük ses dalgası simgesi gerekmez (ölçülü ol). Satıra dokununca `Sound.Tap(); Picked?.Invoke(Number)`.

- [ ] **Step 2: `LevelSelect`** — `Show`'a `int recordCount` parametresi; ayar satırlarından sonra, `recordCount > 0` ise bir **gezinme satırı**: başlık `Kayıtlar`, not `Dinlediğin kayıtlar: {recordCount}`, sağda `›` (anahtar yok). `SwitchRow.Build`'deki satır kabuğunu (kenarlık, yuvarlaklık, iki satır yazı) tekrar yazma: ortak bir `static VisualElement RowShell(string title, string note, ...)` yardımcısına al, `SwitchRow` ve Kayıtlar satırı onu kullansın. Dokununca `RecordsPressed?.Invoke()`. Dosya başı yorumuna "ayarların altında, kayıt dinlenmişse Kayıtlar satırı" ekle.

- [ ] **Step 3: `Hud`** — `recordsView` alanı, `glossary` gibi kurulur ve `top`'a eklenir (sıra: `levelSelect`'in üstünde görünsün; `RecordingScene`'in **altında** kalmalı, yoksa kayıt sayfanın arkasında oynar — kurulum sırasını kontrol et). `levelSelect.RecordsPressed += () => RecordsPressed?.Invoke();`, `recordsView.Picked += n => RecordPicked?.Invoke(n);`. `ShowRecords` önce `CloseTransient()`. `ShowLevelSelect` imzası `recordCount` alır (çağıran 4 yer: `Oyun.cs` 256, 1401, 1408, 1576 civarı → hepsine `recordings.HeardList().Count`; bunun için `Oyun`'da küçük `void ShowMenu() => hud.ShowLevelSelect(LevelEntries(), recordings.HeardList().Count);` yaz ve dört yeri ona bağla).

- [ ] **Step 4: `Oyun.cs`** — Start'ta olay bağları:

```csharp
hud.RecordsPressed += () => { hud.HideLevelSelect(); hud.ShowRecords(RecordEntries()); };
hud.RecordPicked += n => StartCoroutine(ReplayRecording(Recordings.ByNumber(n)));
```

```csharp
List<RecordsView.Entry> RecordEntries() =>
    recordings.HeardList().Select(r => new RecordsView.Entry { Number = r.Number, Region = r.Region, File = r.File, Preview = Recordings.Preview(r) }).ToList();

// Kayitlar sayfasindan yeniden dinleme: sayfa kapanir, kayit oynar, bitince sayfa yeniden acilir. Bolumun durumu degismez.
IEnumerator ReplayRecording(Recording r)
{
    hud.HideRecords();
    yield return PlayRecording(r);
    hud.ShowRecords(RecordEntries());
}
```

  (`Oyun.cs`'te `System.Linq` zaten var.) Kayıt sürerken oyuncu kod çalıştıramaz: `RecordingScene` tüm ekranı kaplar ve dokunuşu alır (değişiklik gerekmez; Step 6'da elle dene).

- [ ] **Step 5: Paket + görüntü** — `oyun/Build/` sil → `Unity.exe -batchmode -quit -projectPath oyun -executeMethod OyunBuild.BuildWindows -logFile build.log` → `error CS` yok; `RecordsView.cs.meta`, `RiverBedTraces.cs.meta` oluştu. Unity süreçlerini kapat.

- [ ] **Step 6: Elle deneme** (paket, `-shots` olmadan): menü → Bölümler ekranında "Kayıtlar" satırı (bu bilgisayarda Bölüm 50 bitirilmişse görünür; görünmüyorsa 52'ye girip Kayıt 2'yi dinle) → sayfa → satıra dokun → kayıt oynar → bitince sayfa geri gelir → kapat (×) → oyun kaldığı yerde. 64'e ilk girişte Kayıt 3 oynar, sonra giriş damgası; 64'e yeniden girince oynamaz; Kayıtlar sayfasında artık Kayıt 3 de var. 61'e girince kara ekranda "Yeni bölge: Kanyon dibi / Hedef: göç izleri".

- [ ] **Step 7: Ragıp'a göster** — Bölümler ekranı (Kayıtlar satırı), Kayıtlar sayfası ve Kayıt 3 ekranının görüntüleri (Görev 5'in denetimi `kayitlar.png`, `kayit-3.png` üretir; önce onu yapmak istersen Görev 5 Step 1-2'yi öne al). Onaysız Görev 6'ya geçme.

---

### Görev 5: Kayıtlar denetimi (sekizinci arayüz denetimi)

**Files:**
- Modify: `oyun/Assets/Scripts/Oyun.cs` (`RecordsCheck`, `LevelSelectCheck`'in yanında; arayüz denetimleri listesine `GlossaryCheck`'ten sonra)

- [ ] **Step 1: `RecordsCheck(string dir)`** — `LevelSelectCheck` biçiminde fareyle (`mouse == null` → "fare yok, atlandi"). Log satırı `KAYITLAR DENETIMI:`. Akış:
  1. `string before = recordings.Save();` → `recordings = new RecordingLog();` → `ShowMenu()` → "Kayıtlar" satırı **yok** olmalı (`hud.RecordsRowScreenPoint() == null`), değilse problem "kayit dinlenmeden Kayitlar satiri var".
  2. `recordings.Mark(1); recordings.Mark(2); recordings.Mark(3);` → menüyü yeniden aç → satıra tıkla → `hud.RecordsOpen` olmalı → `kayitlar.png`.
  3. Kayıt 3'ün satırına tıkla → kayıt ekranı açılmalı, `hud.RecordsOpen` false → ~2 sn sonra `kayit-3.png` → `RecordingFinished` bekle (en çok 40 sn; dolarsa problem) → 2,5 sn sonra `hud.RecordsOpen` true olmalı.
  4. Sayfayı kapat; `recordings = RecordingLog.Load(before);`.
  Yorum: "Denetim, oyuncunun dinlediklerini degistirmez (shotsMode'da zaten yazilmaz)."

- [ ] **Step 2: Paket + çalıştır** — paketi yeniden üret; arayüz denetimlerini çalıştır (pencere **ön planda** olmalı, ILERLEME.md'deki denetim dersi) → `KAYITLAR DENETIMI: TAMAM` + öteki yedi TAMAM. Görüntüler `docs/tasarim/bolge-7-hikaye-2026-11-02/` (`kayitlar.png`, `kayit-3.png`, `bolum-secme.png`).

---

### Görev 6: Belgeler

**Files:**
- Create: `docs/tasarim/senaryo-bolge-07.md`
- Modify: `docs/tasarim/senaryo-perde-2.md`, `docs/tasarim/hikaye-kitabi.md`, `docs/tasarim/senaryo-bolge-06.md` (yalnızca "Kayıtlar" satırı)
- Kontrol: `grep -rn "mavi" docs/tasarim/*.md` → Bölge 7'nin kalemi/boyası/ışığı için "mavi" kalmamalı (Bölge 10'un mavi yıldızı ve Serçe'nin "açık mavi" gözleri doğru, dokunma).

- [ ] **Step 1: `senaryo-bolge-07.md`** — `senaryo-bolge-06.md`'nin bölüm düzeniyle:
  - Başlık `# Senaryo — Bölge 7: Kanyon dibi, eski nehir yatağı (Bölüm 61-70)`; alıntı satırı: "Taslak 1 — 2026-11-02 (Ragıp + Claude; Ragıp onayladı). Dayandığı belgeler: tasarım belgesi (`docs/superpowers/specs/2026-11-02-bolge-7-design.md` bölüm 2), `senaryo-perde-2.md` (Bölge 7), `hikaye-kitabi.md`. Bulmacalar 2026-11-02'de yapıldı; görünüş 3a/3b'de."
  - **Bu bölgenin görevi:** parametreli rutinler ("nereye", "kaç kez"); Serçe yukarıdan görür (`scout()` yön döndürür, konuşmaz), Kıvılcım aşağıda yürür; göç izi → küçük iz ayrılır → büyük iz peşinden → dönüş izi → kalem → el izleri → "KIVILCIM BENİ BUL". Oyuncu ne öğrenir: Ece göçte gruptan ayrıldı, Defne peşinden gitti, Ece duvara robota mesaj bıraktı (senaryo-perde-2 "Gizemin parçaları").
  - **Bölüm 60 → 61 geçişi:** 60'ın kanyona iniş sahnesi → `sinav-60` → 61 açılırken kara ekranda iki satır (karar 4).
  - **Kayıt 3 (Bölüm 64 başı):** dosya `izler.ses`, ne zaman (karar 3), dört satır (karar 2), neden uzun (kalemi ve duvarı önceden sezdirir; çelişki dönüş iziyle çözülür: kalem Defne Ece'yi taşıyarak dönerken düştü — spec karar 7).
  - **Bölüm bölüm:** Görev 3'teki tablo + "Olan" sütunu (spec 2.1) + "Görünüşte (3. parça)" sütunu (spec 3a.3).
  - **İzler ve gizli ayrıntılar:** spec 2.3 tablosu aynen + "Ragıp'a/Hamza'ya not (dizi): dönüş izi, el izleri ve pasta sözsüz anlatım; videoda da yazısız çalışır."
  - **Kayıtlar sayfası:** karar 6-9, sade dille.
  - **Çelişki kontrolü** (✔ listesi, spec 2.5): ses kaydı yalnızca Defne; robotlar konuşmaz (damga, Serçe'nin ışığı, `scout()`); Kıvılcım uçmaz, Serçe uçar; kimse ölmez; kayıt "sen"e konuşur; Ece'nin rengi turkuaz; sol 861 = Ece'nin 9. doğum günü = göç günü; "KIVILCIM BENİ BUL" tek okunur kelime kuralının bilinçli tek istisnası (karar 8).
  - **Oyuna konanlar (2026-11-02, [yapan kişinin adı]):** `bolum-61..70.json` damgaları, `Recordings`/`RecordingLog`, Kayıt 3, `RiverBedTraces` açılış satırları, Kayıtlar sayfası + `KAYITLAR DENETIMI`.

- [ ] **Step 2: `senaryo-perde-2.md`** — Bölge 7 bölümü: "mavi boya kalemi" → "turkuaz boya kalemi (ucu aşınmış; dönüş izinin yanında, Bölüm 68)"; "mavi boyayla" → "turkuaz boyayla"; kapanışta "mavi bir ışıkla" → "turkuaz bir ışıkla"; **Olay**'a "Serçe yukarıdan izi görür, `scout()` ile yönü söyler (konuşma değil); dönüş izi: iki çift büyük iz, dönüşte daha derin ve yanında küçük iz yok (Defne Ece'yi taşıdı; söylenmez)" ekle; bölüm listesinin altına "Ayrıntı: `senaryo-bolge-07.md`". "Yeni motif: Kıvılcım'ın çantası" tablosunda 7: "Turkuaz bir boya kalemi". "Hikâye kitabına işlenecekler"in altına "(Bölge 7'ninkiler 2026-11-02'de işlendi)".

- [ ] **Step 3: `hikaye-kitabi.md`**
  - **Kayıtlar** girişi: "uzunu 'Kayıtlar' sayfasında" → "Dinlenen kayıtlar Bölge 7'den itibaren menüdeki **Kayıtlar** sayfasında yeniden dinlenir (dinlenmemiş olan görünmez)." Tablo satırı 3: `Bölge 7 (oyunda: Bölüm 64 başı, izler.ses)` | `Ayak izleri: "Ece yine önden koştu. Bir anlığına onu kaybettim. Sonra buldum; bir duvarın önünde, elinde boya kalemi. Ona kızamadım."`
  - **Motifler:** Kıvılcım'ın çantası satırında "boya kalemi (7)" → "turkuaz boya kalemi (7, Bölüm 68; ucu aşınmış)". "Ece" yazısı (turkuaz) satırının karşılığına "Bölge 7: kalem, duvarda çocuk boyunda el izleri, robot çizimi + 'KIVILCIM BENİ BUL'; o anda Kıvılcım'ın gözleri turkuaz yanar". D.A. notları satırına ek: "Ece'nin notu: Bölüm 62 `# serçe yukarıdan bakar, sen yürü :) - E.` (annesi 'D.A.', kızı 'E.'; Ece'nin oyundaki ilk sesi)". Sol 861 satırına: "Bölüm 70 duvarında dokuz çizgili pasta (kimse hatırlatmaz)". Yeni satır: **Dönüş izi** | Bölüm 66 (gidiş aralıklı, koşarak) | 66-69: dönüş daha derin, yanında küçük iz yok → Defne Ece'yi taşıdı; kalem dönüşte düştü.
  - **Gerçekte ne oldu** (zaman çizelgesi): göç günü (sol 861) satırının altına: "Göçte Ece gruptan ayrılıp yan kanyona koştu, duvara Kıvılcım'a mesaj boyadı ('KIVILCIM BENİ BUL'); Defne peşinden gidip onu buldu, kucağında taşıyarak döndü; turkuaz kalem dönüşte düştü." (Var olan satırların biçimine uy; çelişki varsa Ragıp'a sor.)
  - **Serçe** kartı: "Bölge 7'den: yukarıdan izi görür; `scout()` izin yönünü döndürür (konuşma değil). 'Serçe yukarıda güneşte, Kıvılcım aşağıda gölgede.'"
  - **Çelişki kontrol listesi:** iki madde ekle: "Ece'nin rengi turkuaz; 'mavi' yalnızca Dünya'nın yıldızı (Bölge 10)." ve "Bir sahnede en fazla tek okunur kelime; tek istisna Bölüm 70'teki duvar yazısı."

- [ ] **Step 4: `senaryo-bolge-06.md`** — "Perde 2 biçimi" tablosunda Kayıtlar satırı: "… Bir kez gösterilir; 'Kayıtlar' sayfası Bölge 7'de." → "… Bölüm başında bir kez gösterilir; yeniden dinlemek menüdeki 'Kayıtlar' sayfasından (Bölge 7'de yapıldı)." Kayıt 2 bölümündeki "Telefonda bir kez (`kayit2` kaydı)" → "Telefonda bir kez (dinlenen kayıtlar: `kayitlar`; eski `kayit2` anahtarı okunur)."

- [ ] **Step 5: Okuma** — dört belgeyi baştan sona bir kez oku: sayılar (sol, 23+1=24 kişi, 7 küçük + 1 büyük el izi, 2 çakıl) belgeler arasında aynı mı; "mavi" grep'i temiz mi.

---

### Görev 7: Son denetim

- [ ] **Step 1:** `cd motor-test && dotnet test` → hepsi geçer; sayıyı not et (1456 + yeni testler).
- [ ] **Step 2:** Windows paketi hatasız (Görev 5'te üretildiyse ve sonra kod değişmediyse yeniden üretme). Unity süreçlerini kapat.
- [ ] **Step 3:** `-bolum 61 -shots <klasör>` → 61-70'in her biri "Tamamlandı" ile bitiyor; bitiş karelerinde yeni damgalar görünüyor (61 ve 70'in bitiş karesini `docs/tasarim/bolge-7-hikaye-2026-11-02/`'ye koy).
- [ ] **Step 4:** `-bolum 50 -final -shots <klasör>` → `FINAL DENETIMI: TAMAM` (Kayıt 1 ve Kayıt 2 artık katalogdan; görüntülerde metin önceki gibi). Klavye denetimi → TAMAM. Sekiz arayüz denetimi → TAMAM (pencere ön planda).
- [ ] **Step 5:** Bölge 6'ya sızma yok: 52'nin girişinde kayıt (elle, `-shots` olmadan, temiz kayıtla: PlayerPrefs'te `kayitlar`'ı silmek için oyunu kendi kaydınla değil, Unity'de *Edit > Clear All PlayerPrefs* ile ya da elle denediğini not ederek) → Kayıt 2 bir kez oynar.
- [ ] **Step 6:** ILERLEME.md'ye (yapan kişinin adıyla) özet: ne yapıldı, `dotnet test` sayısı, denetimler, görüntü klasörü. **Açık sorular:** Kayıt 3'ün dosya adı (`izler.ses`, karar 1); Bölge 7 açılış satırları (karar 4); Kayıtlar sayfasından açılan kayıt atlanamaz (Kayıt 1-2 gibi; "kapat" düğmesi istenir mi?); 70'in başlığı (1. parçadan kalan). **Sıradaki:** 3a görünüş (Opus 5.5 · high, yeni sohbet, önce yapım planı; 69'un el izi sayısı 7 küçük + 1 büyük, 63'ün izleri 23 büyük + 1 küçük ile eşleşmeli). Oturum kapanışında tek commit + push (CLAUDE.md ekip döngüsü).

## Kapsam dışı

Görünüş ve sahneler (3a/3b: çakıl, izler, Serçe ışığı, kalem, el izleri, duvar yazısı, 63/68/70 sahneleri, Kıvılcım'ın sırtındaki kalem), Kayıt 0 ve sonraki kayıtlar, kayıtlara gerçek ses, telefon testleri (ILERLEME.md listesi), `TestAllLevelsOpen`/`TestSolutionsFilled` (Play Store öncesi).
