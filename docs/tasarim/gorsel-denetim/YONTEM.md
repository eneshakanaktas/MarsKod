# Sezon 1 görsel denetimi — yöntem

> Enes, 2026-11-18. Amaç: oyundaki **görsel mantık hatalarını** bulup düzeltmek (havada duran / zemine gömülen / ufka, dağa, gökyüzüne taşan / tanınmayan nesneler, ışık-gölge tutarsızlığı). Metin denetimi (`docs/tasarim/sezon-1-denetim/`) gibi 5'er bölüm; rapor → Enes onayı → düzeltme.
> Model: **Opus 5.5 · high** (görsel/3B iş).

## Neden yeni yöntem
Eski denetimler yalnız **oyun kamerasından** baktı. Bölüm 70'teki el bu açıdan doğru görünüyordu, yandan bakınca havadaydı: hiçbir denetim yakalamadı. Bu denetim her bölümü birçok açıdan çeker.

## Alet: `-incele`
`oyun/Build/Win/MarsKod.exe -screen-width 450 -screen-height 975 -screen-fullscreen 0 -incele <ilk> <son> <mutlak klasör>` (kod: `Oyun.GorselDenetim.cs`).
Her bölüm iki anda (başlangıç `a-`, doğru çözüm bitince `c-`) + yolda (`b-`, oyun kamerası):
`1-oyun` (oyuncunun gördüğü), `2-genis` (klavye açık: kamera genişler, alanın çevresi görünür), sonra arayüz ve gökyüzü resmi gizlenerek `3-sol`, `4-sag` (alçak yan), `5-on-alcak` (göz hizası), `6-arka`, `7-ust` (tam tepe), `8-genel` (uzak genel bakış).
Ham görüntüler `docs/tasarim/gorsel-denetim/ham/` (git'e girmez; 5 bölüm ~85 görüntü, ~2 dk).
Ayrıca `9-yakin…`: ölçümün işaretlediği nesneler ve her hikâye izi (`Iz-` adlı) yer hizasından, iki yandan yakın çekilir.

## Otomatik yerleşim ölçümü
`-incele` her bölümde başlangıçta ve bitişte bölüm nesnelerini zemine göre ölçer (`PlacementAudit.cs`); log'da `YERLESIM: bolum=… an=… <nesne> (x, z): havada | gomulu | uzaga uzaniyor ama arka plana karismiyor`, sonda `YERLESIM DENETIMI: TAMAM | N sorun, bolumler: …`.
Görüntüsüz hızlı tarama: `-incele 1 100 <klasör> -yalniz-olcum`.
- Bilerek havada duran nesne (süzülen hedef işareti vb.) `Airborne` bileşeniyle etiketlenir, sayılmaz.
- Ölçüm yalnız **nereye bakılacağını** gösterir; hükmü görüntü verir (yanlış alarm olabilir).
- Yeni nesne yerleştirirken: yerdeki nesne `GroundPlacement.SitOnGround`, yere serili iz/şerit `GroundPlacement.Ribbon` + uzağa gidiyorsa `Oyun.FarRockMat`.

## Hareketli nesneler (ders, Bölüm 1-5)
Tek ekran görüntüsü hareketi ve derinliği göstermez: hedef işareti tek kareden "halkaya yapışık / havada" sanıldı, oysa sürekli süzülen kasıtlı bir işaretti. Şüpheli nesnede önce kodda animasyonu var mı bakılır; gerekirse kare serisi ve birçok açı. "Hareketli mi, kasıtlı mı?" sorulmadan bulgu yazılmaz.

## Bilinmesi gereken yapı (hataların çoğunun kaynağı)
- **Ufuk, tepeler, koloni, gökyüzü 3B değil:** kameraya yapışık düz bir resim (`MarsSky.hlsl` ve bölge gökyüzleri). Yalnız oyun kamerasından bakınca 3B zeminle üst üste oturur.
- **Zemin** alanın dışında hafif inişli çıkışlı (`Oyun.TerrainHeight`) ve alanın arkasında aşağı kıvrılır (küçük gezegen gibi); uzaklaştıkça (z ≈ 9 → 19) kendini o düz resme karıştırır (`Ground.shader`). Tepeden bakınca uzak zeminde gökyüzü ve koloni "boyalı" görünür (`mekanizma-ustten-bakis.jpg`).
- Sonuç: zemine konan bir nesne **zemin yüksekliğini** (`TerrainHeight`) ve **uzakta resme karışma kuralını** (`FarRock.shader` / `Ground.shader` yolu) kullanmazsa, uzak ucu zeminden kalkar ve dağların, gökyüzünün üstüne çizilir (Bölüm 4 tekerlek izleri; Enes'in gördüğü ayak izleri).

## Önem dereceleri
- **YÜKSEK:** oyun kamerasında (normal ya da geniş) açıkça görülen mantık hatası.
- **ORTA:** sahne kameralarında ya da dikkatli bakınca görülen; nesne tanınmıyor.
- **DÜŞÜK:** yalnız denetim açısından görülen (ileride kamera değişirse önem kazanır).

## Sıra
5'er bölüm. Sahneli bölümler (51, 55, 60, 63, 68, 70, 76, 78, 80 ve bölge finalleri) kendi grubunda ayrıca kare kare (sahne kameraları da çekilir). Ortak koddan gelen hata bir kez, kökünden düzeltilir (bölgenin bütün bölümleri birlikte düzelir).
Düzeltmeden sonra yalnız ilgili bölümler yeniden `-incele` ile çekilir; paket yeniden üretilir, Enes oyunda bakar.
