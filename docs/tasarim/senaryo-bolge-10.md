# Senaryo — Bölge 10: Gece ovası, meteor yağmuru (Bölüm 91-100) · Sezon 1 finali

> **Taslak 1 — 2026-11-15 (Claude; Ragıp "durmadan çalış, kararları sen ver, sonra söyle" dedi).** Ragıp bakınca değişebilir.
> Dayandığı belgeler: tasarım belgesi (`docs/superpowers/specs/2026-11-09-bolge-8-10-ders-bulmaca-design.md`, Bölge 10), `senaryo-perde-2.md` (Bölge 10 ve "Bölüm 100 — Perde 2 finali"), `hikaye-kitabi.md`.
> Bulmacalar ve hikâye damgaları aynı gün yapıldı (kayıt `docs/superpowers/plans/2026-11-15-bolge-10-bulmacalar.md`). Görünüş (gece, meteorlar, kraterler, mavi nokta, kapının silueti) ve **Bölüm 100 final sahnesi** (iki uyarı, sinyal göstergesi, ışığın bir kez yanması) henüz yok: 3a/3b.

## Bu bölgenin görevi

- Ders: `not` (hatırlatma, 92), `and` (93), `or` + `True/False` döndüren rutin (94); 95-100 yeni Python yok, pekiştirme; 99 **şifre ödevinin hikâyedeki hâli** (ekran bulmacası).
- Hikâye: gece ovası → ilk meteorlar → göktaşları → Kayıt 6 (taslaklarda 5b: mavi yıldız) → Kıvılcım mavi noktaya bakar → dumanlı kızgın taşlar → yağmur yoğunlaşır → volkanın eteğinde kapının silueti → sessizlik → sığınaktan sinyal → **Bölüm 100: iki saat birden** (Perde 2 / Sezon 1 finali).
- Oyuncu ne öğrenir: gökyüzündeki mavi nokta **Dünya**; Ece her gece ona bakardı. Sığınak yaşıyor ama kapsüllerin zamanı sınırlı ve bağlantı kopacak (Bölüm 100 uyarıları).

## Bölüm 90 → 91 geçişi

90'ın finali → `sinav-90` → 91 açılırken kara ekranda: **"Yeni bölge: Gece ovası" / "Hedef: volkanın eteği"** (`NightPlainTraces.TransitionLines`).

## Kayıt 6 (Bölüm 93 başı; taslaklarda "Kayıt 5b")

Dosya adı `yildiz.ses`. Oyunda Kayıtlar sayfasında "KAYIT 6 · BÖLGE 10" yazar (oyuncunun duyduğu altıncı kayıt; Bölüm 100'deki uyarılar kayıt değil).

> Ece her gece o mavi yıldızı arardı.
> 'Orada bizi duyan biri var mı?' derdi.
> Ben de 'Belki bir gün' derdim.

Yeri: 91-92'de gece, meteorlar; 93'te gökyüzünde mavi nokta ilk kez fark edilir (bitiş damgası "Gökyüzünde: mavi bir nokta"), 94'te Kıvılcım ona uzun süre bakar (3b sahnesi). **Gizli iz:** Ece'nin sorusu oyuncuya "Dünya'da bizi duyan var mı" diye gelir; babasının Dünya'da olduğunu bilen (dizi, Sezon 2) ikinci kez anlar (`hikaye-kitabi.md`, Kaan Aras). Oyunda hiç söylenmez.

## Bölüm bölüm

Sol iki bölümde bir artar (Bölge 9 sol 1.102'de bitti). Etiket **"BÖLGE 10 · GECE OVASI"**.

| Bölüm | Giriş damgası | Bitiş damgası | Olan · ders |
|---|---|---|---|
| 91 Gece ovası | Sol 1.103 · Gece ovası | İlk meteorlar · Kraterler sıcak | `meteor_ahead(yon)` açılır (bilgi kartı). Doğuda krater varsa kuzeye, yoksa doğuya: `if/else`. 2 meteor düzeni. |
| 92 Göktaşları | Sol 1.103 · Göktaşları | Toplanan göktaşı: 3 | `not` hatırlatma (Bölüm 32'den beri açık): `if not meteor_ahead(North):`. Göktaşları toplanır. |
| 93 Kayıt 6 | Sol 1.104 · Kayıt 6 | Gökyüzünde: mavi bir nokta | Bölüm başında Kayıt 6. `and`: krater yok **ve** kaya yok. |
| 94 Mavi nokta | Sol 1.104 · Mavi nokta | Kıvılcım bakıyor · Uzun süre | `or` + `def tehlikeli(yon): return meteor_ahead(yon) or rock_ahead(yon)`. Başlangıç kodunda yarım rutin. |
| 95 Dumanlı krater | Sol 1.105 · Dumanlı krater | Kızgın taş: 3 · Dokunulmadı | `smoke_here()` açılır (bilgi kartı; yeni Python yok): kızgın taş toplanmaz. Yol başlangıç kodunda hazır. |
| 96 Yağmur yoğunlaşır | Sol 1.105 · Yağmur yoğunlaşır | Göktaşı: 2 · Kızgın: 2 | `tehlikeli` + `smoke_here` birlikte, 3 meteor düzeni. |
| 97 Kapının silueti | Sol 1.106 · Kapının silueti | Volkanın eteğinde: bir kapı | Okuyan robot: not yol parçaları listesi, iç içe döngü, `yon`, kızgın taşa dokunmamak. 3 not. |
| 98 Sessizlik | Sol 1.106 · Sessizlik | Yağmur dindi | Büyük, sakin bölüm, tek ipucu: `satir_tara` rutini + iç içe döngü + `not smoke_here()`. |
| 99 Sığınaktan sinyal | Sol 1.107 · Sığınaktan sinyal | Gerçek sinyal: 3 · Mesaj açılıyor | **Ekran bulmacası (ANTEN):** `gercek_mi(kod)`: en az 8 karakter **ve** en az bir rakam **ve** gürültü listesinde değil. 9 kod; 3'ü gerçek. |
| 100 İki saat | Sol 1.107 · İki saat | Anten ışığı: bir kez · Söz | Son yürüyüş, orta zorluk: `tehlikeli` ile antene var, göktaşlarını topla. 2 düzen. Sonra final sahnesi (3b). |

Sınavlar: `sinav-95` (`and/or/not` doğruluk tablosu, `not` ile `!=`), **`sinav-100` = Sezon 1 büyük sınavı** (9 soru: `range`, sayaç, `while`, `return`, `len`, `liste[2]`, iç içe döngü, şifre koşulu, `or`).

## 99'un kodları (gizli iz)

Görünen: `DEFNE861` (gerçek), `KKKKKKKK`, `4821`. Gizli: `00000000`, `SIGINAK1` (gerçek), `BKM7`, `12345678`, `ARAS-ECE`, `KAPI8610` (gerçek). Gerçek üç kodun ikisinde **861** var (göç günü, kapı kodu; Perde 3'te sığınak kapısının kodu). `BKM7` ve `ARAS-ECE` gürültü ama tanıdık: antene dolan, kolonideki eski kayıtların parçaları. Kimse açıklamaz.

## İzler ve gizli ayrıntılar

| Bölüm | İz |
|---|---|
| 91-96 | Kraterler her denemede başka yerde: meteor yağmuru gerçek ve sürüyor (spec "fizik mantıklı": krater önceden varsa meteor oraya yeni düşmüş olamaz; düzenler birbirinden bağımsız). |
| 93-94 | Mavi nokta = Dünya. Finalde Ece'nin göğüs çizimine eklediği yıldız ve katlanmış çizimdeki "onu izleyen küçük mavi yıldız" (76'da aralanan köşede görünen) aynı motif. |
| 97 | Ok dizisinin bittiği yerde, volkanın eteğinde büyük metal kapının silueti (Perde 3 sonunda varılacak). |
| 99 | Gerçek kodlarda 861. |
| 100 | Bölüm sonrası (3b): kırmızı `SIĞINAK — KAPSÜL ENERJİSİ: %31` / `KRİTİK EŞİK: 101 SOL SONRA`; sarı `GÜNEŞ KAVUŞUMU: 94 SOL SONRA` / `DÜNYA BAĞLANTISI 14 GÜN KESİLECEK`; başlıkta ilk kez sinyal göstergesi; Kıvılcım mavi noktaya, sonra volkana bakar, anten ışığını **bir kez** yakar (söz). |

## Renkler (Ragıp, 2026-11-15)

Bölge 10 önceki bölgelerde kullanılmamış renklerle: **mürdüm gece; zümrüt yeşili kutup ışığı ve yeşil yanan meteorlar** (Mars'ta yeşil kutup ışığı gerçekten görülür; meteorlar magnezyumla yeşil yanar); göktaşında meteorun yeşili kalır; **kızgın taş ve kraterin koru akkor-fuşya** (turuncu Defne'nin oklarına ayrılı); **gökyüzündeki tek mavi şey Dünya** (76'daki gizli yıldızla aynı mavi). Kesin renkler: `docs/superpowers/plans/2026-11-15-bolge-9-10-gorunus.md` "Renk paleti".

## Çelişki kontrolü

- Ses kaydı yalnızca Defne; öğrenciye konuşur. Kayıt 6 Ece'den söz eder. ✔
- Robotlar konuşmaz; 99'un ekranı makine yazısı. ✔
- Kimse ölmez. ✔
- Bölüm 100 sayıları (`%31`, `101 SOL`, `94 SOL`, `14 GÜN`) bulmacada kullanılmadı, değişmedi. ✔
- `not` 92'de yeni değil, hatırlatma (Bölüm 32'den beri açık; Bölge 8 planı karar 9). ✔
- Kırmızı kristal (Bölge 2) ile kızgın göktaşı ayrı şeyler: kristal `T`, kızgın taş `X`; ikisi de toplanmaz ama kızgın taşa `smoke_here()` bakar. ✔
- Çanta sırası değişmedi: Bölge 10'da yeni eşya yok. ✔
