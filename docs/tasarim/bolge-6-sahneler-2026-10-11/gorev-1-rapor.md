# Task 1 raporu: Sahne kamerasi + 60'in yarigi

Durum: DONE_WITH_CONCERNS (gorsel onay Enes'te).

## Yapilan
- `CameraShot.cs` (brief ile ayni; ek: `Cut(..., float fieldOfView = 0f)` ve `Cut` ufuk farkini sifirlar, `MoveTo` ufuk farkini sifira indirir).
- `CanyonRift.cs`: 3B yarik (iki duvar, 5 katmanli profil, iki renk, dip, sag duvarda raf/patika, donemec kayasi, Trail/BendIndex/ViewPoint/ViewTarget). Malzeme FarRock (zeminle ayni isik + arka plana karisma), `ShadowCastingMode.TwoSided` (sol duvar golge dusursun), `outline: false`.
- `Oyun.SahneDenetimi.cs` + `Oyun.cs` (partial, `shot`, LateUpdate, Release, `-sahneler`, `FarRockMat` internal).
- `Ground.shader`: `riftClip`, `canyonDescent` patikasi kenarda biter, kivrilma silindi. Tek frag var (SADE yalniz fonksiyonlari degistiriyor); ShadowCaster/DepthOnly gecisleri zemini cizmiyor/zemin golge dusurmuyor, onlara eklenmedi.
- `CanyonSky.hlsl`: `canyonRiftToMouth` (koyu ince cizgi), `CANYON_PATH` silindi.
- `MarsSky.hlsl` + `FarRock.shader`: `MarsFadeToBackdropBand` (+ `_FadeBand`), yarik ufka kadar gorunur kalsin diye.
- `CanyonTraces.cs` (Rift), `CanyonLedge.cs` (kenar kayalari yarik agzini kapatmaz; `RimRock` internal YAPILMADI, yarik kendi FarRock malzemesini kullaniyor).

## Brief'e gore sapmalar (nedenleri)
1. `riftClip`: brief'teki `clip(hw - abs(q.x - cx))` seridin DISINI siliyordu (ters). `clip(abs(q.x - cx) - hw)` yapildi.
2. Yarik malzemesi Lit degil FarRock: Lit ile yarik ufkun ustunde, gokyuzunde yelken gibi asili kaliyordu (gercek zemin ufka dogru solup gidiyor, mesh solmuyor).
3. `CameraShot.Cut` ufuk farkini sifirlar (normal kameranin sahte ufku ekranin altinda kaliyor, asagi bakista tum zemin gokyuzune karisip bos ekran veriyordu). `MoveTo` ayni farki varista sifira indirir. Sonraki gorevler icin bilmek onemli.
4. `ViewPoint = (pathX, zemin+3.0, startZ-1.2)`, `ViewTarget = Trail[11]` (raf dibi), `CanyonRift.ViewFov = 50` (normal kamera 32 derece cok dardi; `Cut` yeni opsiyonel fov parametresi aldi).
5. Yarik solma bandi `_FadeBand = (0.05, 0)`, `_FogRange = (60, 80)` (RiftMat).

## Derleme / test
- Build: `error CS` yok, `shader error` yok (son build). `dotnet test`: 1395 gecti.
- `-sahneler` (normal + performans): `SAHNE DENETIMI: TAMAM`.
- Unity/oyun surecleri kapatildi; `ProjectSettings.asset`, `UniversalRenderPipelineGlobalSettings.asset` degisiklikleri geri alindi, `PerformanceTestRun*` silindi.

## Goruntuler (task-1-shots/)
- `s60-0-normal.png`: direklerin ardinda kisa, koyu seritli yarik; ufka dogru kademeli gokyuzu rengine karisiyor; boyali ince cizgi ufuktaki kanyon agzina gidiyor (soluk ama dikissiz). Kenar kayalari agzi kapatmiyor. Yarik normal bakista zaten cok kisa gorunur (60 derece yukaridan bakis); asil okunurluk asagi bakista.
- `s60-1-asagi.png`: sol duvar golgede (koyu), sag duvar gunes alip katman katman (iki ton), sag duvarda acik renkli patika rafi, ust kisimda zemin; siyah bosluk yok; ufuk ekran disinda.
- Performans modu kareleri ayni gorunuyor (zemin kesigi var).
- `b60-1-bekleme.png`: normal oyun karesi (kod kartiyla).
- 41/51/59: gorsel karsilastirma (docs referanslariyla) ayni sahneler, degisiklik yok (referans goruntu boyutlari farkli oldugu icin piksel farki alinmadi).

## Endiseler
- Yarigin dibi (zemin) ve donemec kayasi asagi bakista gorunmuyor: duvarlar cok dik/derin. Dibi gostermek icin kamera daha dik/yakin ya da yarik sigi (Depth) gerekebilir.
- Raf, asagi bakista "kagit serit" gibi duz/acik renkli; duvara oyulmus hissi zayif (profil kaymasi kucuk). Enes onayinda bakilmali.
- Normal bakista yarik soluk; boyali cizgi ince. Daha belirgin istenirse `canyonRiftToMouth` genisligi arttirilabilir.
- Her build ~10-25 dk surdu (shader derlemesi).
