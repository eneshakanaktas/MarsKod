# MarsKod — Kod Klavyesi ve XP (Tasarım Belgesi)

> Tarih: 2026-09-28 · Kararları veren: Enes (Claude ile beyin fırtınası) · Yapacak: Ragıp
> Durum: **Enes onayladı (2026-09-28).** Sıradaki: bu belgeden Parça 1 için yapım planı (writing-plans), sonra yapım.
> Ana tasarım belgesi: `2026-09-25-marskod-design.md` (§2 "Kod yazma" bu belgeye yönlendiriyor; §5.3 "Kod ekranı" ve §6 "Oyuncu analizi" yerine bu belge geçerli).
> Anlatım sayfası (bu belgeyle aynı kararlar): https://claude.ai/artifact/5ohWDmnNGsjqudA5tuYJ4F · yerel kopya `docs/tasarim/klavye/index.html`

## 1. Özet (kod bilmeyenler için)

Oyuncu kodunu üç yoldan biriyle yazar:

| Kademe | Nasıl yazılır | XP (şimdilik) |
|---|---|---|
| **Acemi** | Hazır komut düğmelerini sürükleyip koda bırakır. Harf yazmaz. | 30 |
| **Orta** | Oyunun kendi klavyesiyle yazar; yazarken öneri satırı yardım eder (`mo` → `move(`). | 50 |
| **Usta** | Aynı klavye, öneri satırı yok. Her şeyi kendisi yazar. | 100 |

- Telefonun kendi klavyesi **hiç açılmaz**; klavyeyi oyun çizer (Türkçe Q düzeni + Python işaretleri).
- Oyuncu Çalıştır'ın yanındaki **klavye tuşuyla** istediği kademeyi seçer. Seçim sonraki bölümlerde de kalır; istediği an değiştirebilir (Acemi'ye dönmek dahil).
- Oyun, kodun **her satırının nasıl yazıldığını** aklında tutar. Tek bir satır bile düğmeyle eklendiyse çözüm "Acemi" sayılır.
- Her bölümde **her kademe bir kez** XP verir: aynı bölümü önce Acemi, sonra Orta, sonra Usta ile çözen 30 + 50 + 100 = 180 XP alır. Sonrasında o bölüm XP vermez.

İş iki parçaya bölündü (karar: Enes):
- **Parça 1 — Klavye (şimdi, Ragıp):** bu belgenin §2–§6'sı.
- **Parça 2 — Seviye sistemi (sonra):** §7. Parça 1 yapılırken §7'ye ters düşen bir şey yapılmamalı.

## 2. Kararlar (2026-09-28, Enes)

| # | Konu | Karar |
|---|---|---|
| K1 | Klavye türü | Oyunun kendi klavyesi. Telefon klavyesi açılmaz. Sebep: işaretler ana ekranda, büyük harf/kıvrık tırnak/otomatik düzeltme sorunları yok, her telefonda (ileride iPhone ve Steam'de) aynı. |
| K2 | Acemi etkileşimi | **Sürükle-bırak.** Düğme tutulup kodda istenen yere bırakılır. |
| K3 | Acemi düğmeleri | **Tam satır:** `move(East)`, `collect()`, `for i in range(3):`. Koddaki sayıya dokununca artır/azalt çıkar. |
| K4 | Orta ile Usta farkı | **Yalnızca öneri satırı.** İşaret tuşları ve otomatik girinti ikisinde de var. |
| K5 | İşaretler | Hepsi var: `( ) [ ] : " ' , . = == != < > <= >= + - * / // % #` ve girinti tuşları (⇥ içeri, ⇤ dışarı). **Sık olanlar önde:** `( ) : " = ,` ⇥ ⇤ hep görünür; kalanlar tek dokunuşla açılan ikinci işaret sırasında. |
| K6 | Klavye tuşu yeri | **Çalıştır'ın yanında.** Alt sıra: ipucu · klavye · Çalıştır · baştan al. |
| K7 | Seçim kalıcı mı | **Kalıcı.** Seçilen kademe sonraki bölümlerde de açılır; oyuncu her an her kademeye (Acemi dahil) geçebilir. |
| K8 | Öneri satırı içeriği | **Yalnızca açılmış olanlar:** oyuncunun açtığı oyun komutları, yönler, öğrendiği Python kelimeleri ve kendi kodunda tanımladığı isimler. |
| K9 | Harf düzeni | **Türkçe Q** (ğ ü ş i ö ç ı ayrı tuşlar). |
| K10 | Karışık çözüm | **En kolay yol sayılır.** Bir satır düğmeyle → Acemi; düğme yok ama bir satırda öneriye dokunulmuş → Orta; hepsi harf harf → Usta. |
| K11 | XP tekrarı | Her bölümde her kademe **bir kez** XP verir (toplam en çok 180). Aynı kademeyle ikinci çözüm XP vermez. |
| K12 | Sıra | Önce klavye (Parça 1), sonra seviye sistemi (Parça 2). Yapım planındaki Aşama 5, Aşama 3'ün kalanından (adım adım modu, bölüm seçme) **önce** yapılıyor. |

## 3. Ekran düzeni

```
┌──────────────────────────────┐
│  Bölüm başlığı               │
│  Mars alanı (küçülür)        │
├──────────────────────────────┤
│  kod.py              Python  │  ← kod kartı
│  1  for i in range(5):       │
│  2      move(East)           │
├──────────────────────────────┤
│  💡   ⌨   ▶ Çalıştır    ↻    │  ← alt sıra (klavye tuşu yeni)
├──────────────────────────────┤
│  [Acemi: düğme paleti]       │  ← kademeye göre biri görünür
│  ya da                       │
│  [Orta/Usta: kod klavyesi]   │
└──────────────────────────────┘
```

- Palet ya da klavye açıkken oyun alanı kalan yere küçülür (bugünkü "klavye açılınca kart kayar" davranışı gibi). Oyun alanına dokununca palet/klavye kapanır; kod kartına dokununca açılır.
- Kod çalışırken palet/klavye kilitlenir (bugünkü `ReadOnly` gibi).
- **Klavye tuşu (⌨):** dokununca küçük bir seçim kutusu açılır: *Hazır düğmeler (Acemi) · Klavye + öneri (Orta) · Klavye (Usta)*. Seçili olan işaretli. Her seçeneğin yanında o bölümde o kademenin XP'si alındı mı (✓) görünür.

## 4. Acemi: düğme paleti

- **Düğmeler nereden gelir:** Bölüm dosyasına yeni bir alan: `"parcalar"` (satır listesi). Örnek Bölüm 3:
  `"parcalar": ["move(North)", "move(East)", "move(South)", "move(West)", "collect()", "for i in range(3):"]`
  Alan yoksa `komutlar`'dan türetilir (`move` → dört yön, `collect` → `collect()`). Sayılar bilerek çözümdekinden farklı yazılır (`range(3)`), oyuncu değiştirir.
- **Sürükle-bırak:**
  - Düğme tutulup kod kartına sürüklenir; satırlar arasında bırakılacak yeri gösteren ince turuncu çizgi çıkar.
  - **Girinti:** Çizgi, bırakılacak satırın girintisini de gösterir. Varsayılan: hemen üstteki satır `:` ile bitiyorsa bir kademe içeride, değilse üstteki satırla aynı. Parmak sağa/sola kaydırılınca girinti kademe kademe değişir (4 boşluk adım).
  - Koddaki bir satır tutulup başka yere sürüklenebilir (sıra değiştirme); kod kartının dışına bırakılırsa silinir (sürüklerken çöp kutusu işareti görünür).
- **Sayı değiştirme:** Koddaki bir sayıya dokununca üstünde − / + çıkar (`range(3)` → `range(5)`). Satırın türü değişmez (düğme kalır).
- Acemi'de kod kartında harf yazılamaz; imleç yoktur.
- Yeni açılan komutun düğmesi turuncu kenarlı olur (bölümdeki ilk kez görülen `parcalar`).

## 5. Orta / Usta: kod klavyesi

**Düzen (yukarıdan aşağıya):**
1. **Öneri satırı** (yalnızca Orta): en çok 3 öneri.
2. **İşaret sırası:** `(` `)` `:` `"` `=` `,` ⇥ ⇤ `…` — `…` ikinci işaret sayfasını açar/kapatır: `[` `]` `'` `.` `==` `!=` `<` `>` `<=` `>=` `+` `-` `*` `/` `//` `%` `#` `_`. **(09-29, yapımda değişti)** 17 işaret tek sıraya sığmadığı için sayfa rakam sırasının değil, **üç harf sırasının** yerine geçer (telefonlardaki "?123" gibi); rakamlar görünür kalır, klavye yüksekliği değişmez. `_` değişken adları için eklendi.
3. **Rakamlar:** `1 2 3 4 5 6 7 8 9 0`
4. **Türkçe Q:** `q w e r t y u ı o p ğ ü` / `a s d f g h j k l ş i` / `⇧ z x c v b n m ö ç ⌫` / `boşluk ↵`

**Davranış:**
- ⇧: bir sonraki harf büyük; iki kez basınca kilit. Türkçe kuralı: `ı`→`I`, `i`→`İ`. (Python kodunda `I`/`İ` nadir; `North`, `East` gibi yönler için öneri satırı büyük harfi kendisi koyar.)
- Tuşa basınca tuşun üstünde büyütülmüş harf balonu (telefon klavyelerindeki gibi). ⌫ basılı tutulunca art arda siler.
- Bugünkü yazma kolaylıkları aynen kalır (`CodeTyping`): `:` sonrası ↵ bir kademe içeriden başlar; girintide ⌫ bir kademe geri gider. ⇥ / ⇤ imlecin satırının girintisini bir kademe artırır/azaltır.
- İmleç: kod kartında dokunulan yere gider; basılı tutup sürükleyince seçim yapılır. Kopyala/yapıştır yok.
- Bilgisayarda (ileride Steam) fiziksel klavye de çalışır.
- `"` tuşu düz tırnak koyar (kıvrık tırnak sorunu biter).

**Öneri satırı (Orta):**
- İmlecin solundaki yarım kelimeyle (en az 1 harf) başlayan açık kelimeler gösterilir: oyun komutları (`move`, `collect`...), yönler (`North`...), öğrenilmiş Python kelimeleri (`for`, `in`, `range`...), koddaki isimler (değişkenler).
- "Öğrenilmiş Python kelimeleri" bölüm dosyasındaki yeni alandan gelir: `"python_kelimeleri": ["for", "in", "range"]`. Bir bölümde açılan kelime sonraki bölümlerde de açık kalır (önceki bölümlerin listeleri birleşir).
- Öneriye dokununca kelime tamamlanır; fonksiyonsa `()` eklenir, imleç parantezin içine gelir (`move(|)`). **(09-29, Enes)** Elle yazılan `(` kendiliğinden kapanmaz (Usta parantezi kapatmayı öğrensin). İmlecin sağında `)` varken `)` yazmak ikinci parantez eklemez, üstünden geçer; boş `()` içinde ⌫ ikisini birden siler.

## 6. Satır takibi ve XP (Parça 1'de yapılacak kısım)

**Satır türleri:** `Dugme` (Acemi), `Oneri` (Orta), `Elle` (Usta). Sıralama: Dugme < Oneri < Elle.

| Olay | Satırın türü |
|---|---|
| Paletten bırakılan satır | Dugme |
| Bölümün başlangıç kodundaki satırlar | Dugme (oyuncu yazmadı) — bkz. Açık soru 1 |
| Klavyeyle yazılan yeni satır | Elle; o satırda bir öneriye dokunulursa Oneri |
| Var olan satırda klavyeyle değişiklik | En düşük olan kalır (Dugme satırı yazıyla düzeltilse de Dugme kalır; Elle satırda öneri kullanılırsa Oneri'ye düşer) |
| Sayıya − / + ile dokunma | Değişmez |
| Satır tamamen silinip yeniden yazılırsa | Yeni satır: Elle/Oneri |
| Satır bölünürse (ortada ↵) | İki parça da eski türü alır |
| İki satır birleşirse (satır başında ⌫) | İkisinin en düşüğü |
| Boş satır ve yalnızca yorum (`#`) olan satır | Hesaba katılmaz |

- Satır türleri kodla birlikte telefonda saklanır (bugünkü `kod-<numara>` kaydının yanında), oyun kapanıp açılınca kaybolmaz.
- **Çözüm türü** = bölüm başarıyla bitince koddaki satırların en düşük türü (K10).
- **XP:** Çözüm türünün XP'si o bölümde daha önce alınmadıysa verilir ve kaydedilir (bölüm başına alınan türler kümesi). Miktarlar Parça 1'de sabit: Dugme 30, Oneri 50, Elle 100.
- **Ekranda (Parça 1):** Bölüm sonu kutusunda "+50 XP · Orta ile çözdün" ve alınmamış daha zor kademe varsa "Usta ile çözersen +100 XP daha". Toplam XP üst başlıkta küçük bir sayı. Seviye çubuğu ve kutlama Parça 2'de.

## 7. Parça 2 — Seviye sistemi (sonra yapılacak; kararlar şimdiden)

- **XP ve seviye:** XP birikir, seviye yükselir. **Jeton ayrıdır:** jeton harcanır (ipucu), XP harcanmaz.
- **XP miktarı oyuncunun seviyesine göre değişir** (30/50/100 başlangıç değerleri). Formül Parça 2'de belirlenecek.
- **Gizli beceri puanı** (ana belge §6): komut/konu başına; hata, süre, ipucu ve deneme sayısından. Oyuncu sınava sokulmaz.
- **Klavye kendiliğinden dönüşür (seçenek C):** Kararı gizli beceri puanı verir, komut komut (oyuncu `move`'da ustaysa `move` düğmesi kalkar, `for` zayıfsa düğmesi kalır). Dönüşüm anı görünür bir seviye atlaması gibi kutlanır ("Seviye 6! Artık `move`'u kendin yazıyorsun").
- **Alt sınır (seçenek B, yumuşatılmış):** Sistem bir düğmeyi kaldırınca oyuncu onu kendisi geri açabilir, ama o düğmeyle kurulan çözüm **daha az XP** verir (miktar Parça 2'de). Oyuncu o komutta zorlanırsa (puan düşerse) düğme kendiliğinden geri gelir ve XP yine tam olur. Hiçbir yol XP'siz kalmaz.
- Parça 1 için anlamı: satır türleri ve bölüm başına alınan kademeler şimdiden kaydedilmeli; kademe seçimi (K7) ileride "sistemin önerdiği kademe" ile birlikte çalışabilecek şekilde tek yerde tutulmalı.

## 8. Teknik notlar (Claude / Ragıp için)

- **Saf mantık `oyun/Assets/Dunya/`'ya (UnityEngine yok, `dotnet test` ile test):**
  - `LineTags` — kod değişince (önce/sonra metni + değişikliğin kaynağı: palet / klavye / öneri / sayı) satır türlerini günceller. §6 tablosunun her satırı bir test.
  - `Xp` — satır türlerinden çözüm türü, bölüm başına alınan kademeler, verilecek XP.
  - `Suggest` — yarım kelime + açık kelimeler → öneriler.
  - `Level` — `parcalar` ve `python_kelimeleri` alanları (yoksa türetme); bölüm denetleyici yeni alanları da denetler.
- **Arayüz `oyun/Assets/Scripts/`:** `CodeKeyboard.cs` (tuşlar, işaret sıraları), `BlockPalette.cs` (sürükle-bırak), `KeyboardMenu` (klavye tuşu kutusu), `CodeEditor.cs` (telefon klavyesi yerine kendi girişimiz; imleç/seçim; satır türlerini tutar), `Hud.cs` (alt sıraya ⌨).
- **İlk iş (risk):** Unity `TextField`'a dokununca telefon klavyesi açılıyor. Kendi klavyemizde bunun hiç açılmaması gerekir. Önce telefonda (sanal telefon) dene: salt-okunur yazı kutusu + kendi imleç/seçim mantığı mı, yoksa `TextField`'sız tamamen kendi metin alanımız mı. Karar ondan sonra.
- `CodeTyping` kolaylıkları kendi klavyemizle doğrudan uygulanır (artık telefon klavyesinin kopyasıyla uğraşmak gerekmez; `Show()`'daki "önce klavyeye yeni yazı" düzeltmesi gereksizleşir).
- Boyut: klavye çizim/doku eklemeden, UI Toolkit öğeleriyle yapılır (paket büyümesin).

## 9. Test

- `motor-test/`: `LineTagsTests`, `XpTests`, `SuggestTests`; `BolumTests` yeni alanları kapsar.
- Sanal telefonda: Acemi ile Bölüm 3'ü sürükle-bırakla çöz (girinti dahil); Orta ile öneri kullanarak; Usta ile harf harf. Her birinde doğru XP; aynı kademeyle tekrar çözünce XP yok; oyunu kapat-aç, satır türleri ve XP duruyor.
- Klavye tuşlarının parmakla rahat basılabildiğini gerçek telefon boyutunda kontrol et (Türkçe Q'da satırda 12 tuş var).

## 10. Açık sorular

1. **Başlangıç kodu satırları:** Şimdilik "Dugme" sayılıyor (oyuncu yazmadı). Sonuç: Usta XP'si için oyuncunun başlangıç satırlarını silip kendisi yazması gerekir. Alternatif: başlangıç satırları çözüm türünü etkilemesin. Enes/Ragıp karar verecek; Parça 1'de tek bir ayarla değiştirilebilir yapılmalı.
2. **Sürükle-bırakta girinti** (§4) telefonda denenince zor gelirse: bırakılan satırın solunda ⇥/⇤ küçük düğmeleri alternatifi.
3. **Türkçe Q'da tuş boyutu:** 12 tuşluk satır dar telefonlarda küçük kalırsa `ğ ü` / `ş i` için uzun basma alternatifi.
4. Parça 2 formülleri: seviyeye göre XP, "kendin geri açtığın düğme" XP'si, seviye eşikleri.
