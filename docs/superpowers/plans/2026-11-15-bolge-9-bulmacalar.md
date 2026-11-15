# Bölge 9 Bulmacaları + Hikâye (Bölüm 81-90) — yapılanların kaydı

> **2026-11-15, Claude (Opus 5.5) — Ragıp "durmadan çalış, kararları sen ver, sonra söyle" dedi.** Plan ayrı yazılmadı; iş aynı oturumda yapıldı. Bu belge kararların ve yapılanların kaydıdır (Bölge 8 planının biçiminde, kısa).
> **Spec:** `docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md` (Bölge 9). **Senaryo:** `docs/tasarim/senaryo-bolge-09.md`.

## Yapılanlar

1. **Tuz kristali `S`** (`Collectible.SaltCrystal`, "tuz kristali") ve **tuz gölü bölgesi** (`Region.SaltLake`, 81-90).
2. **`crystal_size()`** (yeni komut, 82'de açılır): robotun karesindeki toplanmamış tuz kristalinin boyu (cm); yoksa, toplandıysa, kırmızı kristalse 0. Boylar bölüm dosyasında `kristal_boyu` (okuma sırasıyla; yalnız `crystal_size` açıksa zorunlu). Kablo makarasıyla ortak okuyucu: `Level.ReadSizes`, `World.SizeHere`.
3. **Liste notları** (`Note.cs`): `notlar` öğesi artık metin (Bölge 8), metin listesi (`["KK", "D"]`: yol parçaların harfleri art arda) ya da `{"yazi", "yol", "rapor"}` (yazı yol değilse: 84 okları, 87 haritası). `read_note()` liste notunda her okuyuşta yeni bir Python listesi verir. Nota özel `rapor`: değişen notta her notta anten başka sayı bekler (87). `Level.NoteRoutes` kalktı, yol `Note.Route`'ta.
4. **Sahne (geçici, derlendi, Unity'de görülmedi):** liste notu kartta her öğe ayrı satır (`NoteView.Show(lines, stepByLetter)`); yazısı yol olmayan notta harf vurgusu yok; `crystal_size` ölçümü karenin üstünde "N cm" (`Oyun.ShowMeasure`, makara ile ortak). Kristal şimdilik buz görünüşünde, göl iniş ovası ışığında (3a'da).
5. **Bölüm 81-90**, **`sinav-85`**, **`sinav-90`**, sözlükte **Liste (list)**, **`crystal_size`**, **`max`** sayfaları; `for` sayfasına iç içe döngü.
6. **Hikâye:** etiket "BÖLGE 9 · TUZ GÖLÜ", damgalar (sol 1.098-1.102), **Kayıt 5** (`oklar.ses`, 83 başı), bölge geçişi ("Yeni bölge: Tuz gölü / Hedef: okların gösterdiği dağ", `SaltLakeTraces`).
7. **Testler:** `TuzGoluTests.cs` (kristal ölçümü, liste/nesne notları, nota özel rapor, hata mesajları, sondaki virgül, `not in`), Bölge 9 damga testi (bitiş damgasındaki sayılar bulmacayla aynı), Kayıt 5 testi. 1772 test geçti.

## Plan sırasında verilen kararlar (spec'e ek / düzeltme)

1. **84'te `oklar[0]` + `len`:** not ok listesi verir; tuzla çevrili ilk ok (`oklar[0]`) yön, okların sayısı (`len(oklar)`) adım. Okları tek tek yürümek (`for ok in oklar`) yanlış yere götürür. Hikâyede Defne'nin yöntemi; kimse açıklamaz, ipucu söyler.
2. **85'te `in`:** "kaç farklı boy var" (`if boy not in farkli: farkli.append(boy)`, `report(len(farkli))`). `in` olmadan çözülmez (7 kristal, 4 farklı boy). Defne'nin listesi başlangıç kodunda; sondaki virgül bilgi kartında ("hata değil").
3. **86 ve 88 tarama:** satırın başına dönüş için ikinci iç döngü (`for adim in range(4): move(West)`). Yılan gibi gidiş (yön değişkeni) öğretilmedi, kullanılmadı.
4. **88'de `if` zorunlu:** taranan alanda kırmızı kristaller var; her karede `collect()` kırmızıda robotu durdurur. `crystal_size()` kırmızıda 0.
5. **87'de robot yürümez:** not gölün haritası (`T` = tuz kristali), oyuncu iç içe döngüyle sayar, `report`. Notların yolu boş (`"yol": ""`), farkı rapor sayısında.
6. **90:** bütün notlar aynı başla (`KKK`, `DD`) kristallerin üstünden geçer; sonra ayrılır. İkinci notun yolu kırmızı kristalden geçer: `if crystal_size() > 0` olmadan robot durur.
7. **Yön rutini (`yon`) 84, 89, 90'da başlangıç kodunda hazır** (ders o değil); rutin şartı ve dönüş şartı sürer.
8. **`max` 83'te açılır** (sözlük sayfası). `append` bir metot, kelime listesine girmez; Liste sayfası anlatır.
9. **Bilgi kartı:** 82 (yeni komut), 84 (not artık liste), 85 (yarım liste, sondaki virgül). 86'da (iç içe döngü) kart yok: ipuçları yeter.

## Kalanlar (sonraki parçalar)

- **3a görünüş:** bembeyaz göl, gökyüzünü yansıtan zemin (yansıma nesneyle aynı yerde), tuz kristali modeli + sayaç simgesi, tuzla çevrili oklar (84), ufukta volkan (88'de belirginleşir), tuza gömülü oyuncak astronot (86, sırtta), kırık tuz kabuğunda izler, bölgenin ışığı (gündüzden 90'da gün batımına), bölge engeli (tuz kümesi).
- **3b sahneler:** 86 astronotun alınışı, 90 kapanış (göl mora, laciverte; ilk yıldızlar; volkanın üstünde parıltı). "Atla" düğmesi kuralı.
- Unity'de paket + görüntü denetimi (bu oturumda Unity Hub kurulu/girişli değildi).
