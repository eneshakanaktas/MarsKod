# Canlı çevre ve oyun hissi (Bölge 1-10)

> Ragıp, 2026-11-19 (Enes'le ortak karar: "her bölgeye oyunu daha eğlenceli yapacak yeni animasyonlar ve çizimler"). Claude (Opus 5.5) yaptı.
> Kural: **yalnız görünüş.** Bulmacaya, dünya kurallarına, bölüm dosyalarına dokunulmaz. Oyun alanının kareleri her zaman net okunur.

## Sorun
Sahne durgundu: zemin, kayalar, toplananlar ve robot vardı ama çevrede hiçbir şey kıpırdamıyordu. Robotun kendi hareketleri (süzülme, göz kırpma, toplama, sevinme) zaten vardı. Eksik olan **çevrenin canlılığı** ve **oyuncunun eylemine verilen tepki** (adım, çarpma, toplama, bitiş).

## İki katman

### 1. Oyun hissi (her bölgede, `PlayFx.cs`)
| An | Ne olur |
|---|---|
| Robot bir kareye varır | İticisi zeminden halka şeklinde alçak bir toz kaldırır (bölgenin rengi: kar, kum, tuz, toz...) |
| Engele çarpar | Çarpma noktasında bir avuç toz + birkaç kıvılcım |
| Bir şey toplar / onarır | Nesnenin yerinden her yöne parıltı saçılır, bir kısmı yukarı süzülür |
| Bölüm biter | Robotun çevresinden üç dalga parıltı fışkırır (bölgenin rengi + turuncu) |

Oyuncu eylemine tepki olduğu için "Arka plan animasyonları" ayarından bağımsızdır. Performans modunda tane sayısı yarıya iner. Renkler: `RegionFx`.

### 2. Canlı çevre (bölgeye göre, `AmbientLife.cs`)
Her bölge kendi sınıfını yazar (`PlainAmbience`, `PolarAmbience`...). Ortak parçalar `AmbientPieces.cs`'te:

| Parça | Ne | Kullanan bölgeler |
|---|---|---|
| `Snowfall` | Gökten yavaşça düşen taneler | 2 |
| `GroundDrift` | Yere yakın, rüzgârla akan yumuşak pus (kar, kum, tuz, toz) | 1, 2, 3, 4, 9 |
| `Motes` | Havada süzülen zerreler (toz, ışıkta parlayan nokta) | 1, 3, 6, 7, 8 |
| `Glints` | Bir an yanıp sönen parıltı (buz, tuz, ıslak çakıl) | 2, 7, 9 |
| `DustDevil` | Ovada gezen, dönen toz sütunu | 1, 4 |
| `Embers` | Yukarı süzülen akkor kıvılcım | 10 |
| `BlinkLamp`, `Spinner`, `Pennant` | Yanıp sönen lamba, rüzgârla dönen parça, dalgalanan flama | yeni çizimlerde |

- **Ortak rüzgâr** (`Wind.Gust`): ara sıra gelip giden rüzgâr dalgası. Bayrak, rüzgâr ölçer, savrulan kar/kum aynı rüzgâra uyar; sahne "birlikte" kıpırdar.
- **Gökyüzü** (bölgenin `*Sky.hlsl` dosyası): kayan yıldız (`shootingStar`, MarsSky.hlsl; Bölge 1, 2, 3, 10), Bölge 2'de kutup ışığı (`polarAurora`; Mars'ta da görülür, Perseverance 2024).
- **Ayarlar:** "Arka plan animasyonları" kapalıyken hiçbir şey kıpırdamaz, yeni tane atılmaz; donmuş gökyüzünde kayan yıldız görünmez (`_SkyMotion`). Performans modunda tane sayısı ~yarıya iner, gökyüzü zaten durgun.
- **Yerleşim:** yeni çizimler `GroundPlacement.SitOnGround` ile oturtulur; z > 6 olanlar uzakta arka plana karışan malzemeyle (`Oyun.FarRockMat`) çizilir. Oyun alanının içine hiçbir şey konmaz; alanın önüne (kameraya yakın) yüksek nesne konmaz (kareleri örter). Görünen yer: alanın arkası (z ≈ 3,5-7) ve iki arka köşenin yanı.
- Hikâye izleriyle çakışmaz: Bölge 2'de aracın 20 kapanışındaki yolu (`ResearchRover.DriveOffset`) boş bırakıldı.

## Bölge bölge

| Bölge | Canlı çevre | Yeni çizimler |
|---|---|---|
| 1 İniş ovası (şafak öncesi) | Havada kızıl toz zerreleri, yerde savrulan toz, alanın arkasında gezen toz hortumu, kayan yıldız | — (Bölge 1 görsel denetimi yeni bitti; koloni karanlık kalır) |
| 2 Kutup buzulu | Kar yağışı, yerde savrulan kar, buz parıltısı, kutup ışığı (aurora), kayan yıldız | Kutup ekibinin hava direği (dönen rüzgâr ölçer, rüzgâr gülü, flama, kırmızı lamba; sol arka köşe), buz kuleleri (sağ arka köşe + uzakta) |
| 3 Kraterli düzlük (şafak) | Şafak ışığında parlayan zerreler, savrulan toz, sol yarıda küçük toz hortumu, kayan yıldız | Rüzgâr çorabı (sol arka köşe), küçük rüzgâr türbini, yeşil "çalışıyor" lambası (sağ arka köşe) |
| 4 Kum tepeleri (gündüz) | Sık kum savrulması, güneşte parlayan kum zerreleri, iki koyu toz hortumu (arka kenara yakın) | — (işaret direkleri ve kurdele hikâyenin kendisi, kalabalık etmesin) |
| 5 Anten tepesi | Sert rüzgâr: sık savrulma, parlayan zerreler | Rüzgâr çorabı (sağ arka köşe; anten solda) |
| 6 Kanyon girişi (gün batımı) | Alçak güneşte parlayan sıcak zerreler, savrulan toz | — |
| 7 Nehir yatağı (sabah) | Yere yakın yavaş sabah sisi, güneşte parlayan zerreler | — |
| 8 İstasyon | İçeride: lamba ışığında süzülen toz (yalnız odanın içinde). Dışarıda: ince toz | — (lambalar, ekranlar hikâyenin parçası) |
| 9 Tuz gölü | Altın tuz parıltıları (yalnız yakın zeminde), savrulan tuz tozu | — |
| 10 Gece ovası | Sık kayan yıldız (meteor gecesi), için için yanan kor taşları, onlardan yükselen kıvılcımlar | Kor taşları (alanın dışında, soluk: bölümün kızgın göktaşlarıyla karışmasın) |

## 3. Bölüm girişi (Ragıp'ın isteği, aynı gün; `LevelArrival.cs`, `ArrivalShows.cs`)
Bölüm açılınca toplanacak parçalar (bölgeye göre engeller de) sahneye bölgenin temasına uygun bir gösteriyle gelir (~2-3 sn).

| Bölge | Gösteri | Katılan |
|---|---|---|
| 1 İniş ovası | Gökten ateş kuyruklu meteor gibi düşer; çarpınca kıvılcım, toz halkası, ışık halkası | parçalar + kayalar |
| 2 Kutup | Soğuk bir dalga alanı çapraz geçer; geçtiği karede ışık halkası, kar tozu, buz yerden büyür | hepsi (buz blokları daha derinden) |
| 3 Krater | Batıdan toz rüzgârı geçer; panel parçaları onunla takla atarak gelir, sekip oturur | parçalar (bazalt kayalar yerinde) |
| 4 Kum tepeleri | Kum girdap yapar, parça kumun altından dönerek fırlar, kum fışkırır | parçalar |
| 5 Anten tepesi | Kablo makaraları alanın arkasından yuvarlanarak, sekerek gelir | parçalar |
| 6 Kanyon | Parçalar kanyonun yukarısından kırıntı dökerek düşer, sekip oturur | parçalar |
| 7 Nehir yatağı | Kurumuş nehrin bir anlık hayali arkadan öne akar; geçtiği yerde çakılları bırakır | parçalar |
| 8 İstasyon | Veri kartları tavandan inen mavi ışık sütununda titreyerek belirir | parçalar |
| 9 Tuz gölü | Altın halka dalgalanır, tuz kristali ortadan dışarı sırayla yerden büyür | parçalar |
| 10 Gece ovası | Ufuktaki volkan patlar (`NightSky.hlsl` `_VolcanoBurst`: ağız parlar, lav fışkırır, tepe kızarır); ağzından çıkan lav topları ateş ve duman iziyle yay çizip tahtaya düşer | parçalar + kayalar |

- **Kurallar:** bilgi kartı, geçiş yazısı, kayıt, sınav, bölüm seçme açıkken ya da 51'in roket inişi sürerken bekler (`Hud.StageCovered`). Çalıştır / Adım / Baştan al / düzen değişimi gösteriyi anında bitirir, her şey yerine oturur (`LevelArrival.Finish`). Bölüm her açıldığında oynar; bir koşudan sonra "Baştan al"da oynamaz.
- Denetim turlarında (`-shots`, `-izle`, `-incele`, `-duzenler`) oynamaz: nesneler hemen yerinde olmalı. `-canli`'de oynar ve başından çekilir.
- Volkanın 3B ağzı: gökyüzü resmindeki noktanın 3B karşılığı (`SkyPicture.WorldPoint`; koloninin lambaları da bunu kullanır).
- Sesler: `Sound.Thud` (yere düşüş), `Sound.Eruption`, `Sound.Shimmer`.

## Öğrenilenler (görüntüden)
- **Uzak toz hortumu havada asılı görünür:** alanın çok arkasında dibi pusla aynı renkte kalıp kaybolur, yalnız tepesi koyu gökyüzünde görünür. Hortumlar arka kenara yakın (z ≈ 4-5,5), koyu renkli ve dibinde yere yayılan toz eteğiyle.
- **Kulübenin arkasından yükselen hortum bacadan çıkan duman gibi okunur** ("kulübede biri var" yanlış ipucu). Bölge 3'te hortum yalnız sol yarıda.
- **Açık zeminde ışık gibi parlayan beyaz görünmez** (tuz gölü): orada düz renkli altın parıltı (`Glints` `glow: false`).
- **Tuz gölünde uzak zemin göl resmine karışır:** parıltı z > 5'e konursa havada belirir.
- Kum rengi zeminde kum rengi toz görünmez: hortum ve savrulma zeminden koyu/açık seçilmeli.

## Ölçüm
`-canli` turunda 10 bölgenin hepsinde 60 kare/sn (bilgisayar). Telefonda denenmedi (A6 telefon testlerine eklendi).

## Denetim
- **Hareket:** `MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -canli <ilk> <son> <mutlak klasör> [-adim <n>]` her bölümü açıp oyun kamerasından kare serisi çeker (5 sn boş sahne + doğru çözüm çalışırken; adım tozu, toplama, kutlama). Log: `CANLI CEVRE: bolum=… fps=…`, sonda `CANLI CEVRE DENETIMI: TAMAM`. Örnek: `-canli 5 95 <klasör> -adim 10` her bölgeden bir bölüm (~6 dk). Kod: `Oyun.CanliCevreDenetimi.cs`.
- Bir bölgenin görünüşü değişince: o bölgeden birkaç bölüm `-incele <ilk> <son> <klasör>` (yerleşim ölçümü `YERLESIM DENETIMI: TAMAM` olmalı; hikâye izleriyle çakışma gözle bakılır).
- Tane hareketli olduğu için tek kareden hüküm verilmez (YONTEM.md "Hareketli nesneler").
