# Kod Klavyesi — Parça 1 Yapım Planı

> Tarih: 2026-09-28 · Yazan: Ragıp (Claude ile) · Dayandığı belge: `docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md` (§2–§6, §8–§9)
> Parça 1 = oyunun kendi klavyesi (Acemi palet + Orta/Usta klavye) + satır takibi + bölüm başına XP. Parça 2 (seviye sistemi) bu planda yok; ama ona ters düşen bir şey yapılmaz (belge §7).

## Ana fikir

1. **Önce risk:** telefon klavyesinin hiç açılmadığını sanal telefonda kanıtla (Görev 0). Sonuca göre `CodeEditor` yolu seçilir.
2. **Sonra saf mantık** (`oyun/Assets/Dunya/`, UnityEngine yok, `dotnet test`): kod tamponu (yazı + imleç + satır türleri), XP, öneriler, bölüm dosyası alanları. Arayüzden önce bitip testlenir; arayüz yalnızca bunları çağırır.
3. **En son arayüz** (`oyun/Assets/Scripts/`): kod alanı, klavye, palet, klavye tuşu. Her görevden sonra Windows paketi + `-shots` görüntüsü; büyük adımlarda sanal telefon.

Kural: her görev kendi başına derlenir ve testler yeşil kalır. Görev bitince bu planda işaretlenir, ILERLEME.md'ye kısa not düşülür.

---

## Görev 0 — Risk: telefon klavyesi açılmasın (sanal telefon) ✅ (2026-09-28, Ragıp — yol B seçildi)

**Sonuç:** B çalışıyor. Sanal telefonda 9 dokunma/sürükleme (koda, tuşlara, boş alana, alttaki arayüze): telefon klavyesi hiç açılmadı (`TouchScreenKeyboard.visible` ve Android `mInputShown` ikisi de hep kapalı). Dokunma imleci doğru harfe koyuyor (satır sonu, parantez içi), sürükleme seçim yapıyor, geçici tuşlar yazıyor. Notlar: (1) `⌫` `↵` gibi işaretler kod yazı tipinde yok → tuş simgeleri çizilecek (Painter2D, `Hud`'daki simgeler gibi). (2) `MeasureTextSize` telefonda bir kez "No ICU data" uyarısı yazıyor, zararsız. (3) Bilgisayarda fiziksel klavye otomatik denenmedi (tuş gönderme başka pencereye kaçtı); Görev 5'te elle denenecek. Deneme: `Scripts/KlavyeDeneme.cs`, `-klavyedeneme`.

**Önerilen yol (B): `TextField`'sız kendi metin alanı.** Kod yazı tipi eş genişlikli olduğu için dokunulan yer → (satır, sütun) hesabı basit: `sütun = round(x / harfGenişliği)`, `satır = floor(y / satırAralığı)`. Yazı kutusu olmayınca Unity `TouchScreenKeyboard` açmaz.

Deneme (geçici, `CodeEditor`'a dokunmadan ayrı bir deneme dosyasıyla; `-klavyedeneme` başlatma seçeneği):
- [ ] Renkli kod `Label`'ı + kendi imlecimiz; dokununca imleç o harfin önüne gider.
- [ ] Ekranda 5-6 tuşluk geçici bir klavye (`m o v e ( ⌫ ↵`) koda yazar.
- [ ] Sanal telefonda: koda, tuşlara, oyun alanına defalarca dokun → **telefon klavyesi hiç açılmamalı**; imleç doğru harfe gitmeli; basılı tutup sürükleyince seçim olmalı.
- [ ] Bilgisayarda: fiziksel klavye (odaklanabilir öğe + `KeyDownEvent`) yazabiliyor mu.

**Karar:** B çalışırsa Görev 5'te `CodeEditor` B ile yeniden yazılır. Çalışmazsa yedek yol (A): salt-okunur `TextField` + kendi imleç/seçim mantığı; onu da aynı şekilde dene. Deneme kodu Görev 5'te silinir.

---

## Görev 1 — Kod tamponu: yazma işlemleri (Dunya, saf) ✅ (2026-09-28, Ragıp — `CodeBuffer.cs`, `CodeBufferTests` 44 test)

Yeni `CodeBuffer` (Dunya): kod metni + imleç + seçim + satır türleri tek yerde. Arayüz yalnızca bunu çağırır, kendisi metin değiştirmez.
- İşlemler: `Type(char)`, `Backspace()`, `Enter()`, `Indent()` (⇥), `Dedent()` (⇤), `ApplySuggestion(kelime, fonksiyonMu)`, `DropLine(satırNo, girinti, metin)` (palet), `MoveLine(eski, yeni, girinti)`, `DeleteLine(n)`, `ChangeNumber(satır, sütun, +1/-1)`, `SetCaret`, `Select(a, b)`.
- `CodeTyping` kuralları doğrudan burada uygulanır (`:` sonrası ↵ içeriden, girintide ⌫ bir kademe). Bugünkü "önce/sonra farkı" tahmini gereksizleşir; `CodeTyping` fiziksel klavye/eski yol için kalır ya da `CodeBuffer`'a taşınıp silinir (Görev 5'te karar).
- [ ] `motor-test/Dunya/CodeBufferTests.cs`: her işlem; bugünkü `KodYazmaTests` durumlarının hepsi yeni yolla da geçmeli.

## Görev 2 — Satır türleri (Dunya, saf) — belge §6 ✅ büyük kısmı (2026-09-28, Ragıp — `CodeBuffer` içinde, `LineTagsTests` 21 test; türler: Baslangic < Dugme < Oneri < Elle. Kalan: "başlangıç kodu sayılsın mı" ayarı Görev 3 `Xp`'de)

`LineKind { Dugme, Oneri, Elle }` (sıralama Dugme < Oneri < Elle). `CodeBuffer` her işlemde satır türlerini günceller (ayrı `LineTags` sınıfı olarak; tampon onu çağırır).
- [ ] §6 tablosunun her satırı bir test (`LineTagsTests`): palet satırı Dugme; başlangıç kodu Dugme; yeni yazılan satır Elle, öneriye dokununca Oneri; var olan satırda değişiklik en düşüğü korur; sayı −/+ değiştirmez; silinip yeniden yazılan satır yeni tür alır; bölünen satırın iki parçası eski türü alır; birleşen satırlar en düşüğü alır; boş ve yalnızca yorum satırı hesaba katılmaz.
- [ ] **Açık soru 1 için tek ayar:** `StartCodeCounts` (başlangıç satırları çözüm türünü etkilesin mi). Varsayılan belgeye göre: etkiler (Dugme).
- [ ] Kaydetme biçimi: satır türleri kısa bir metne çevrilir/geri okunur (ör. `"DDEO"`); kod ile türlerin satır sayısı uyuşmazsa hepsi en düşüğe (Dugme) düşer (güvenli taraf).

## Görev 3 — XP (Dunya, saf) — belge §6, §7 ✅ (2026-09-29, Enes — `Xp.cs`, `XpTests` 20 test; `SolutionKind(tampon, startCodeCounts=true)`, `Award`, `NextBetter`/`NextBetterText`, `Save`/`Load`, `Total`; Baslangic = Acemi sayılır)

`Xp` sınıfı:
- [ ] `SolutionKind(tampon)` = sayılan satırların en düşük türü.
- [ ] Bölüm başına alınan kademeler kümesi + `Award(bölüm, tür)` → verilen XP (Dugme 30, Oneri 50, Elle 100; ikinci kez 0).
- [ ] `NextBetter(bölüm, tür)` → "Usta ile çözersen +100 XP daha" metni için sıradaki alınmamış zor kademe.
- [ ] Kaydetme biçimi saf metin (ör. `"3:DO;5:E"`); telefona yazma işi Scripts'te (`PlayerPrefs`). Miktarlar tek yerde (Parça 2'de seviyeye göre değişecek).
- [ ] `XpTests`: 30+50+100=180 sınırı, tekrar XP yok, bozuk kayıt metni sıfırdan başlar.

## Görev 4 — Öneriler + bölüm dosyası alanları (Dunya, saf) — belge §4, §5 ✅ (2026-09-29, Enes — `Level.Pieces`/`PythonWords`, `LevelCheck` parça denetimi, `Suggestions.cs` (`Suggest`, `OpenWords`), `SuggestTests` 20 test; bolum-03.json güncellendi, 1., 2. bölüm parçaları komutlardan türer)

- [ ] `Level`: `"parcalar"` (satır listesi) ve `"python_kelimeleri"` alanları. `parcalar` yoksa `komutlar`'dan türetilir (`move` → dört yön, `collect` → `collect()`). Açık Python kelimeleri = bu ve önceki bölümlerin listelerinin birleşimi.
- [ ] Denetleyici: her parça tek başına geçerli Python satırı mı (`:` ile bitenler hariç motorla sözdizimi denetimi), yalnızca açık komutları mı kullanıyor, `python_kelimeleri` motorun tanıdığı kelimeler mi.
- [ ] `Suggest(yarımKelime, kod, açıkKelimeler)` → en çok 3 öneri: oyun komutları, yönler, açık Python kelimeleri, koddaki isimler (atama/`for` değişkeni/`def`). Fonksiyonsa `(` eklenecek bilgisi. `SuggestTests`.
- [ ] `bolum-01..03.json`'a yeni alanlar (Bölüm 3: `range(3)` ile, belge §4 örneği); `docs/tasarim/bolum-dosyasi.md` kılavuzu güncellenir.

---

## Görev 5 — Kod alanı: kendi metin alanımız (Scripts) ✅ (2026-09-29, Enes — telefonda dokunma denemesi Görev 6'yla birlikte)

`CodeEditor.cs` yeniden: Görev 0'da seçilen yol; metin `CodeBuffer`'da.
- [x] Dokunma → imleç; basılı tut (350 ms) + sürükle → seçim (turuncu); hemen sürükle → kaydır; kalın turuncu yanıp sönen imleç. Farede tıkla/sürükle = seçim, Shift+tıkla seçimi uzatır, tekerlek kaydırır. Dışarıdan değiştirme tek yol: `CodeEditor.Edit(op)` (Görev 6-7 klavye/palet bunu çağıracak).
- [x] Satır türleri kodla birlikte kaydedilir (`kod-<numara>` yanında `tur-<numara>`); eski kayıtlarda tür yoksa Dugme. Başlangıç kodu `LoadStart` ile (Baslangic).
- [x] Bilgisayarda fiziksel klavye çalışır (harf, Türkçe harfler, AltGr işaretleri, ⌫, Delete, ↵, Tab / Shift+Tab, oklar, Home/End, Ctrl+A). Denetim: `scripts/klavye-denetimi.ps1` → `KLAVYE DENETIMI: TAMAM`. Kopyala/yapıştır yok (gerekirse sonra).
- [x] Uzun satır yatay kayar, çok satırlı kod kart içinde dikey kayar; kod kartı en çok yarım ekran (`Hud.Update` → `editor.MaxHeight`). Çalışan satır görünmüyorsa kod oraya kayar. `-shots` görüntüsü `uzun-kod.png`.
- [x] `Hud.Update`'teki `TouchScreenKeyboard.area` hesabı kalktı (palet/klavye yüksekliği Görev 6-7'de eklenecek).
- Ek: kart uzayınca kamera geri çekiliyor, ekranın altı zeminin bittiği yerin ötesini gösterip siyah kalıyordu → zemin kameraya doğru uzatıldı (`z0 = -30`).
- Silindi: `CodeTyping.cs`, `KodYazmaTests.cs` (durumları `CodeBufferTests`'te), `KlavyeDeneme.cs` + `-klavyedeneme`.
- Not: Görev 6'ya kadar telefonda yazılamaz (telefon klavyesi artık açılmıyor, oyunun klavyesi henüz yok). Uzun satırda yatay kayınca kısa satırlar görünmez olur (normal editör davranışı); insan testinde kafa karıştırırsa düşünülür.

## Görev 6 — Kod klavyesi (Orta/Usta) — belge §5 ✅ (2026-09-29, Enes — telefonda parmakla deneme Görev 9'da)

`CodeKeyboard.cs` (UI Toolkit öğeleri, doku yok):
- [x] Sıralar: (Orta) öneri satırı · işaret sırası `( ) : " = ,` ⇥ ⇤ `…` · rakamlar · Türkçe Q üç sıra · boşluk ↵. **Değişiklik:** 17 işaret tek sıraya sığmadığı için `…` rakam sırası yerine **üç harf sırasını** ikinci işaret sayfasına çevirir (telefonlardaki "?123" gibi; rakamlar görünür kalır, yükseklik değişmez). Sayfaya `_` eklendi (değişken adları için).
- [x] ⇧ tek basış bir harf, çift basış kilit (altında çizgi); `i`→`İ`, `ı`→`I`. Basınca tuş üstünde büyük harf balonu; ⌫ basılı tutunca art arda siler. Tuş basınca hemen yazar (hızlı yazmada dokunuş kaybolmaz).
- [x] Kod çalışırken klavye kapanır (kod salt okunur olur). Oyun alanına dokununca kapanır, kod kartına dokununca açılır. Kod kartı en çok yarım ekran ve oyun alanına en az ekranın çeyreği kalır.
- [x] Görüntü: `-shots` → `klavye-orta.png`, `klavye-isaretler.png`, `klavye-usta.png`, `klavye-basili.png` (balon), `klavye-yazildi.png`; 450×975 ve 450×800 (kısa ekran) denendi. Otomatik denetim: `-shots` içinde tuşlara fareyle tıklanır → log'da `KOD KLAVYESI DENETIMI: TAMAM`.
- Ek: kod yazı tipi JetBrains Mono, `==` `!=` `<=` `>=` işaretlerini tek şekle birleştiriyordu (`≠`, `≤`…; öğrenen için yanıltıcı) → birleştirmesiz resmî sürüm **JetBrains Mono NL** (`Fonts/JetBrainsMonoNL-Regular.ttf`, OFL lisansı `OFL-JetBrainsMono.txt`; +94 KB).
- Parantez (Enes'in isteği): öneriye dokununca `move()` gelir, imleç içeride; `)` sağda varsa `)` yazmak üstünden geçer; boş `()` içinde ⌫ ikisini siler; elle `(` kendiliğinden kapanmaz (`CodeBuffer`, 4 yeni test; toplam 1142).
- Orta/Usta seçimi Görev 8'de; şimdilik klavye Orta (öneri satırı açık). `Hud.ShowSuggestions` ile değişir.
- Açık soru 3 (12 tuşluk sıra): tuşlar ~80×104 (1080 genişliğe göre), Gboard'un Türkçe Q'suyla aynı sayı; gerçek telefonda parmakla denenmeli.

## Görev 7 — Acemi paleti: sürükle-bırak — belge §4

`BlockPalette.cs`:
- [ ] Bölümün parçaları düğme olarak; yeni açılan komut turuncu kenarlı.
- [ ] Tut-sürükle: kod kartında satırlar arasında ince turuncu çizgi + girinti gösterimi (varsayılan: üstteki `:` ile bitiyorsa içeride); parmak sağa/sola → girinti kademe kademe.
- [ ] Koddaki satırı tutup taşıma; kart dışına bırakınca silme (çöp işareti).
- [ ] Sayıya dokununca −/+ (tür değişmez). Acemi'de imleç yok, harf yazılmaz.
- [ ] Sanal telefonda Bölüm 3'ü yalnızca sürükle-bırakla çöz; girinti zor gelirse açık soru 2'deki ⇥/⇤ yedeği.

## Görev 8 — Klavye tuşu + kademe seçimi — belge §3, K6, K7

- [ ] Alt sıra: 💡 · ⌨ · ▶ Çalıştır · ↻. ⌨ → küçük kutu: Hazır düğmeler (Acemi) · Klavye + öneri (Orta) · Klavye (Usta); seçili işaretli, o bölümde alınan kademelerin yanında ✓.
- [ ] Seçim kalıcı, **tek yerde** tutulur (`KeyboardTier` ayarı; Parça 2'de "sistemin önerdiği kademe" buraya bağlanacak).

## Görev 9 — XP ekranda + kapanış

- [ ] Bölüm bitince: "+50 XP · Orta ile çözdün" ve varsa "Usta ile çözersen +100 XP daha". Toplam XP üst başlıkta küçük sayı.
- [ ] `-shots` yeni ekranları çeker (palet, klavye Orta, klavye Usta, bölüm sonu XP).
- [ ] Sanal telefonda belge §9 senaryoları: Bölüm 3'ü Acemi / Orta / Usta ile çöz → 30/50/100; aynı kademe tekrar → XP yok; kapat-aç → satır türleri ve XP duruyor.
- [ ] APK üret → `paketler/marskod-oyun.apk`; boyut önceki 31 MB civarında kalmalı.
- [ ] Yapım planı Aşama 5 işaretlenir, ILERLEME.md, gelişim sayfası (taslak kuralı: ekran görüntüsü + video).

---

## Sohbet bölme önerisi
- Sohbet 1: Görev 0–4 (risk denemesi + saf mantık, çoğu `dotnet test`).
- Sohbet 2: Görev 5–6 (kod alanı + klavye).
- Sohbet 3: Görev 7–9 (palet, klavye tuşu, XP, telefon denemesi).

## Açık sorular (belge §10'dan, bu planda nasıl ele alındığı)
1. Başlangıç kodu satırları → Görev 2'de tek ayar; varsayılan "etkiler". Enes/Ragıp karar verince yalnızca ayar değişir.
2. Sürükle-bırakta girinti → Görev 7'de telefonda denenir; zorsa ⇥/⇤ yedeği.
3. Türkçe Q tuş boyutu → Görev 6'da dar telefon boyutunda görüntüyle bakılır; küçükse `ğ ü ş i` uzun basma.
