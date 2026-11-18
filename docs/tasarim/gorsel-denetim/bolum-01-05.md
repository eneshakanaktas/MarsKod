# Görsel denetim — Bölüm 1-5 (Bölge 1, iniş ovası)

> Enes, 2026-11-18. Yöntem: `YONTEM.md`. Çekim: `-incele 1 5` (85 görüntü, `ham/01-05/`). Kanıt: `bolum-01-05-kask-ve-izler.jpg`.

## Bulgular
| # | Bölüm | Önem | Ne görünüyor | Neden (kod) | Öneri |
|---|---|---|---|---|---|
| 1 | 4 | **YÜKSEK** | Alanın sağındaki tekerlek izleri (iki uzun çizgi) ufka gidip **tepelerin ve gökyüzünün üstüne** çıkıyor | `Traces.WheelTracks`: izler dümdüz sabit yükseklikte iki uzun çubuk; zemin alanın arkasında aşağı kıvrılıyor, izler kıvrılmayı izlemiyor ve uzakta arka plana karışma kuralını kullanmıyor | İzleri kısa parçalarla zemine oturt (`TerrainHeight`), uzakta zeminle aynı kuralla soluklaşıp ufuk bandında bitsin. Senaryo "çok sayıda iz, hepsi ufuktaki dağlara gidiyor" diyor: 2-3 iz çifti |
| 2 | 3 | ORTA | Alanın sağ kenarında **düz gri bir top**: kask olduğu anlaşılmıyor | `Traces.Helmet`: küre + küçük siperlik; siperlik oyun açısından görünmüyor; kürenin merkezi zemin hizasında → **yarısı toprağa gömülü** | Kask tanınsın (cam yüzü/siperlik kameraya dönük, kenar halkası, dış çizgi), devrik hâlde **yere otursun** |
| 3 | 1, 5 | Soru | Hedef halkasının üstündeki altıgen işaret yandan bakınca **havada süzülüyor**; oyun açısından halkanın kenarına yapışık gibi | Hedef işareti | Kasıtlı bir "işaret ışığı" mı? Öyleyse altına ince bir ışık sütunu ya da halkayla bağ; değilse halkaya indir |
| 4 | hepsi | DÜŞÜK | Alçak/yan açıdan zemin alanın hemen arkasında kıvrılıp bitiyor, arkası boş | Gökyüzü düz resim, zemin gezegen gibi kıvrılıyor (YONTEM.md) | Oyun kamerasında görünmüyor; dokunma. Kameranın alçaldığı sahnelerde (Bölge 6+) ayrıca bakılacak |

Robot, kayalar, enerji hücreleri, köşe direkleri: yerde, tutarlı (sorun yok).

## Ortak kök neden ve kalıcı önlem (öneri)
Bölüm 3 ve 4'teki hatalar aynı alışkanlıktan: nesne **sabit yükseklikle** konmuş, zemin yüksekliği ve uzakta karışma hesaba katılmamış. Oyunun başka yerlerinde de (ayak izleri vb.) aynı hata olması çok olası.
1. **Ortak yardımcı:** "zemine oturt" (nesnenin altı zemine değsin) ve "uzakta arka plana karış" tek yerde; izler, süs nesneleri bunu kullanır.
2. **Otomatik yerleşim ölçümü** (`-incele` içine): her bölümde alan dışındaki nesneler için "havada (> 3 cm)", "gömülü", "ufka taşan (karışma kuralı yok)" listesi log'a. 100 bölüm dakikalar içinde taranır; gözle bakılacak yerleri gösterir, sonraki 5'lilerde ilk bu okunur.

## Enes'in kararı (2026-11-18)
1 ve 2 düzeltilsin, kalıcı önlemler kurulsun. 3: hedef işareti **kasıtlı** (sürekli yukarı-aşağı süzülüyor, iyi duruyor) — bulgu yanlıştı: tek ekran görüntüsünden hareketli nesneye hüküm verildi. Ders YONTEM.md'ye yazıldı.

## Yapılanlar
- **Bölüm 4 izleri** (`Traces.WheelTracks`): 3 araç izi (6 şerit), zemini izleyen şerit (`GroundPlacement.Ribbon`), uzak kaya malzemesi (`Oyun.FarRockMat`: zeminle aynı karışma). Ufuk bandına gelmeden zeminle birlikte soluklaşıyor; tepe/gökyüzü üstüne çıkmıyor (`bolum-01-05-duzeltme-izler.jpg`).
- **Bölüm 3 kask** (`Traces.Helmet`): kabuk + öne çıkan koyu cam + boyun halkası; devrik, cam yukarı ve kameraya dönük; `GroundPlacement.SitOnGround` ile gerçek en alt noktası yere oturuyor (`bolum-01-05-duzeltme-yakin.jpg`).
- **Hedef işareti**: `Airborne` etiketi (bilerek havada; ölçüm saymaz).
- **Kalıcı önlemler**: `GroundPlacement` (zemine oturt; zemini izleyen şerit), `PlacementAudit` (otomatik yerleşim ölçümü: havada / gömülü / uzakta karışmıyor), `-incele` her bölümde ölçer, işaretlenen nesneleri ve hikâye izlerini (`Iz-`) yakından çeker; `-yalniz-olcum` görüntüsüz hızlı tarama.
- Doğrulama: `-incele 3 4` → `YERLESIM DENETIMI: TAMAM`; yakın çekim ve oyun açısı gözle kontrol edildi.
