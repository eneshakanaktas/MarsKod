# Kod sözlüğü (tasarım notu)

> Tarih: 2026-09-29 · Fikir ve karar: Ragıp (Claude ile) · Durum: **ilk sürüm yapıldı (09-30, Ragıp)**: oyun ekranında sol üstte 📖 düğmesi (bölümler düğmesinin altı), her kelime için kart (açıklama + renkli örnek), üstte kısayollar, oynanan bölümde yeni olanlar "YENİ". Dosya `Resources/Sozluk/sozluk.json`, kılavuz `docs/tasarim/sozluk-dosyasi.md`. Henüz yok: kilitli sayfalar (bütün bölümler açık olduğu için), canlandırma, turuncu parçaya basılı tutunca sayfa açılması. Açık sorular hâlâ Enes/Hamza ile konuşulmalı (aşağıda; 1 ve 2 şimdilik Ragıp'ın kararıyla cevaplandı: yer sol üst, önce metin + örnek).
> Yapım planında: Aşama 7'nin ilk maddesi, ama ilk insan testinden **önce** yapılmalı (aşağıda "Ne zaman").

## Neden

Oyuncu yeni bir komutla ya da Python kelimesiyle karşılaşınca (örn. Bölüm 3'te `for`) oyun bugün ne işe yaradığını hiç anlatmıyor: palette turuncu kenarlı bir düğme belirir, o kadar. Tasarımdaki eğitim bölümü (ana tasarım belgesi §4–§6: kısa görsel ders + mini oyun, bilgi haritası, mini sınav) **ders** üzerine kurulu: bir kez oturup öğretir. Eksik olan **başvuru yeri**: oyuncu istediği an "`for` neydi?" deyip bakabilmeli, iki cümle + küçük örnek görüp oyuna dönebilmeli. (The Farmer Was Replaced'de de açılan her komutun oyun içi açıklaması var.)

Ders bir kez öğretir, sözlük her unuttuğunda hatırlatır. İkisi birbirini tamamlar.

## Ne olacak

- **Her zaman açılabilir:** ekranda sözlük düğmesi (yeri tasarlanacak; üst sıra ya da alt sıra).
- **Her sayfa:** kelime/komut, bir cümle "ne işe yarar", 2–3 satırlık örnek; mümkünse örneği robotla oynayan küçük bir canlandırma. Dil sade (yeni başlayan için; jargon yok).
- **İçerik:** oyun komutları (`move`, `collect`…), yönler, Python kelimeleri (`for`, `range`, `if`, `while`, `def`, `return`…), ileride `print`, listeler vb.
- **Kilit (seçenek C):** açılmış olanlar tam görünür; henüz açılmamışlar kilitli görünür, yalnızca adı ve ne zaman açılacağı ("🔒 `while` — Bölüm 9'da açılır"). Oyuncu nereye gittiğini görür, merak eder, ama cevap önceden söylenmez. Kilit yalnızca **anlatımı** kilitler, kodu yasaklamaz (Python bilen Usta oyuncu `return`'ü yine yazabilir; motor çalıştırır).
  - Karşılaştırılan seçenekler: A) yalnızca açılanlar (sade ama yol haritası yok), B) hepsi açık (yeni başlayanı boğar, bölümlerdeki "ihtiyaç doğsun" keşfini önceden söyler).
- **Yeni parça:** bölümde ilk kez görülen parça (Acemi paletinde turuncu kenarlı) için sözlük düğmesinde "yeni" işareti; turuncu düğmeye basılı tutunca o kelimenin sayfası açılır (böylece ayrı bir "yeni parça kartı" gerekmez). Orta/Usta'da öneri satırındaki ya da koddaki kelimeye basılı tutmak da aynı sayfayı açabilir (karar verilecek).
- **Veri dosyası:** metinler bölüm dosyaları gibi ayrı bir dosyada (ör. `Resources/Sozluk/*.json`); kod bilmeyen ekip üyeleri yazıp düzeltebilir. Hangi bölümde açıldığı bölüm dosyalarından (`komutlar`, `python_kelimeleri`) türetilir; iki yerde yazılmaz. `dotnet test` her sayfayı denetler (örnek kod motorda çalışıyor mu, her açılan kelimenin sayfası var mı).
- **Derslerle bağ:** eğitim bölümü (dersler) gelince her sözlük sayfasına "bunun dersi" bağlantısı; 2. ipucu (derse götüren) da oraya bağlanır.

## Ne zaman

Yapım planında Aşama 7'nin ilk maddesi, ama sıra olarak **kod klavyesi Parça 1'den (Görev 8–9) sonra, ilk insan testinden önce**: kod bilmeyen deneyici Bölüm 3'te `for`'u sözlüksüz anlayamaz.

## Açık sorular (ekiple)

1. Sözlük düğmesinin yeri ve görünüşü.
2. Canlandırma ilk sürümde olsun mu, yoksa önce metin + örnek mi?
3. Kilitli sayfada "ne zaman açılır" bilgisi bölüm numarası mı, yoksa daha belirsiz mi ("ileride")?
4. Orta/Usta'da koddaki kelimeye basılı tutmak sözlüğü açsın mı (seçimle çakışabilir: basılı tut = seçim)?
