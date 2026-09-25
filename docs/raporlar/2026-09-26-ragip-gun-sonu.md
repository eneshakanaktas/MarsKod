# Gün sonu raporu — 26 Eylül 2026 (Ragıp)

## Kısaca
Bugün oyunun görünüşü için **Godot oyun motoruyla 4 taslak** yapıldı. Sonuncusu (**godottaslak4**) telefonda da çalışıyor. Godot denemeleri burada bitti; sıradaki adım aynı sahneyi **Unity** ile denemek.

## Yapılanlar
- **Taslak sistemi:** Her tasarım denemesi silinmeden saklanıyor ve motora göre adlandırılıyor: `expotaslak1-2`, `godottaslak1-4`, ileride `unitytaslak1`… Hepsinin listesi, görüntüleri ve durumu: `docs/tasarim/taslaklar.md`. Eski bir taslağa dönmek için: `git checkout godottaslak2` (geri gelmek için `git checkout main`).
- **Godot kuruldu** (ücretsiz oyun motoru). 4 taslak:
  1. **godottaslak1** — havada yüzen ada.
  2. **godottaslak2** — Mars yüzeyi, koloni, buzlu cam arayüz. Beğenildi ama birinciye benzedi.
  3. **godottaslak3** — gerçekçi yön: gerçek fotoğraflardan dokular, NASA'nın Perseverance gezgini, mavi Mars gün batımı, düşen meteorlar. Buzlar fazla parlak ve görüntü öncekine benzer bulundu.
  4. **godottaslak4 (son)** — tamamen yeni sahne: katmanlı kayalıklarla çevrili Mars kanyonu, sinema görünümü (girişte kamera kanyona süzülür, film şeritleri, gren, buz toplanırken yakınlaşma), toz hortumu, meteor, doğal tozlu buzlar. **Giriş ekranında ve oyun içinde tek tuşla "Animasyon ve efektler" kapatma.**
- **Telefon paketi:** godottaslak4 Android telefona kurulabilir hâle getirildi, bilgisayardaki sanal telefonda denendi (düğmeler çalışıyor). Arkadaşların indirebilmesi için 57 MB'a küçültülüp `paketler/marskod-godottaslak4.apk` olarak GitHub'a eklendi. Kurulum anlatımı `paketler/BENIOKU.md`.

## Görüntüler
`docs/tasarim/` klasöründe: `godottaslak4-giris-ekrani.png`, `godottaslak4-giris-sahnesi.png`, `godottaslak4-meteor.png`, `godottaslak4-efektler-kapali.png`, `godottaslak4-telefon-toplama.png` (önceki taslakların görüntüleri de orada).

## Bilgisayarda açmak
Godot kuruluysa `prototipler/godot/calistir.bat` (son taslak). Eski taslaklar `prototipler/arsiv/` içinde, her birinin kendi `calistir.bat` dosyası var.

## Açık konular
- **NASA gezgin modeli:** NASA logosu kullanılamaz, "NASA onayladı" izlenimi verilemez; satıştan önce lisans kontrolü gerek. Model telefonda ağır (~12 MB); gerçek oyunda sadeleştirilmeli ya da kendi robotumuz yapılmalı.
- **GitHub'a yüklenmeyenler:** `build/` klasöründeki ~106 MB'lık sanal telefon paketleri (GitHub 100 MB üstünü almıyor). Gerekirse yeniden üretilir.
- Küçültülmüş telefon paketi gerçek bir telefonda henüz denenmedi — ilk kuran haber versin.
- Sanal telefonda ilk açılış ~1 dakika sürdü; gerçek telefonda ölçülmeli.

## Sıradaki adım
1. Ekip godottaslak4'e baksın (telefona kurarak).
2. Ragıp: **Unity** ile aynı sahne → `unitytaslak1`. (Unity Hub + ücretsiz Unity hesabı gerekli; kurulum ~5–10 GB.)
3. İki motor karşılaştırılıp hangisiyle devam edileceğine karar verilecek.

## Kota notu
Uzun tek bir sohbet çok kota harcıyor (Claude her mesajda tüm geçmişi okuyor). Yeni büyük işlere `/clear` ile temiz sohbet açarak başlamak daha verimli.
