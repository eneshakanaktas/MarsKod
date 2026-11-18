# Görsel denetim — Bölüm 6-10 (Bölge 1 sonu, iniş ovası)

> Enes, 2026-11-18. Yöntem: `YONTEM.md`. Çekim: `-incele 6 10` (97 görüntü, `ham/06-10/`); yerleşim ölçümü: Bölüm 8 `Iz-Panel` %75 gömülü.
> Kanıt: `bolum-06-10-telsiz.jpg`, `bolum-06-10-firtina-izi.jpg`.

## Bulgular
| # | Bölüm | Önem | Ne görünüyor | Neden (kod) | Öneri |
|---|---|---|---|---|---|
| 1 | 10 | **YÜKSEK** | Telsiz direği hedef halkasının **tam ortasında**; robot hedefe varınca direk **robotun içinden geçiyor**. Kapanış sahnesinde telsizin SOS ışığı (bölgenin son anı) robotun gövdesinin içinde yanıp sönüyor, görünmüyor. Ayrıca süzülen hedef işareti direğin hemen üstünde: ikisi "sopa üstünde elmas" gibi tek nesne görünüyor | `Traces.RadioMast(targetPos)`: direk hedefin merkezine konmuş; direk 0,5 m, robot ~0,85 m | Senaryo "hedef kare: telsiz direğinin **dibi**". Direk karenin arka köşesine (robotun yanına), robottan **uzun** (~0,95 m) olsun: ışığı robotun başının üstünde, kapanışta görünür. Yere oturtulur |
| 2 | 8 (ve 21, 22) | ORTA | "Fırtınanın izi" oyun açısından alanın solunda **pembe-gri düz bir kare** ve ince bir çizgi: devrik güneş paneli olduğu anlaşılmıyor. Yandan: panelin dörtte üçü toprağın içinde; "kum yığını" köşeleri yuvarlatılmış bir **minder** gibi; kablo dümdüz katı bir çubuk | `Traces.StormDebris`: panel merkezi zemin hizasında 58° eğik (çoğu toprağın altında); kum = yuvarlatılmış kutu; kablo = tek düz silindir | Panel kameraya dönük devrik dursun, yüzünde hücre çizgileri ve çerçeve görünsün, alt kenarı birkaç cm kuma gömülü; kum yumuşak bir tümsek; kablo kıvrılarak yer yer kuma girip çıksın. Aynı iz Bölüm 21-22'de de kullanılıyor: birlikte düzelir |

Bölüm 6, 7, 9: sorun yok (6'nın açık kapısı ve 7'nin serası gökyüzü resminde, oyun açısından doğru). Bölüm 7 ve 8'deki "yan açıda iki robot/levha" görüntüleri robotun kendisi (kameraya çok yakın), sorun değil.

## Enes'in kararı (2026-11-18)
İkisi de düzeltilsin.

## Yapılanlar
- **Bölüm 10 telsiz** (`Traces.RadioMast`, adı `Iz-Telsiz`): direk hedefin 0,62 m arkasında (alanın kenarının hemen dışında), 1,1 m boyunda, tabanlı, zemine oturuyor. Bitişte robot halkada, SOS ışığı robotun başının üstünde görünüyor (`bolum-06-10-duzeltme-telsiz.jpg`).
- **Fırtına izi** (`Traces.StormDebris` → `DebrisPanel`, `DebrisSand`, `DebrisCable`): panel kameraya dönük devrik, çerçeve + hücre çizgileri, alt kenarı 4 cm kumda; kum basık iki tümsek, zemin renginde; kablo 9 parça kıvrılarak alana uzanıyor, yer yer kuma giriyor. `CraterTraces.Build` artık zemin yüksekliğini alıyor (Bölüm 21-22 aynı izi kullanıyor, birlikte düzeldi) (`bolum-06-10-duzeltme-firtina.jpg`).
- Doğrulama: `-incele 8 10` ve `-incele 21 22` → `YERLESIM DENETIMI: TAMAM`; yakın çekim ve oyun açısı gözle kontrol edildi.
