# Referans Görseller

> Ekleyen: Enes (Ragıp ve Hamza ile birlikte seçildi), 2026-09-26.
> **Bunlar esin kaynağıdır, birebir kopyalanmaz.** Oyun bu uygulamalara benzemeyecek; aldığımız şey "hava".
> Seçilecek görsel malzemeler (modeller, dokular, simgeler) bu fotoğraflardakilerle aynı olmak zorunda değil; aynı havayı veren **daha iyi ve daha güzel** görünenler tercih edilebilir (Enes).

## The Farmer Was Replaced (ana esin — oyunun PC'deki karşılığı)
Steam, 2025, %95 olumlu (4.200+ inceleme). Oyuncu Python'a benzeyen bir dille drone programlar, drone tarlayı otomatik işler.
- **Sürekli ilerleme, seviye yok:** Hasat → kaynak → yeni teknoloji (yeni bitki, yeni komut, daha büyük tarla, daha hızlı drone). Tekrar eden işi elle yapmak yerine kodla otomatikleştirmek oyunun asıl zevki.
- **Konular tek tek açılır**, fazla el tutulmaz; yeni başlayan kendi çözer, deneyimli oyuncuya ileride zor işler (labirent, dinozor/yılan, çoklu drone) kalır.
- **Verimlilik yarışı:** Başarımlar ve sıralama tabloları (8×8 tarla, tek drone, en kısa süre).
- **Görsel:** Renkli ama sade, "minimalist" etiketli 3 boyut; gerçekçilik değil netlik. Tarla, düz renkli bir arka planda havada duran **kalın bir toprak bloğu**: yanlarında toprak ve gömülü taşlar, üstte her kare gerçek zemin (sürülmüş toprak, çimen, bitki), kareler arasında ince derz. Az köşeli (low-poly), sıcak güneş, belirgin gölge. (Tanıtım videosu ve 9 ekran görüntüsü incelendi, 09-26.) Ekranın bir yanı dünya, diğer yanı kod pencereleri (PC'de).
- Bizim farkımız: telefon, Mars, adım adım eğitim + ders bölümü, kademeli kod klavyesi. (Tasarım belgesi §1, §3.)

## İstenen his (Enes'in sözleriyle özet)
Sade, temiz, şık. **Sadeliğin premium hissettirdiği**, yumuşak ve modern bir oyun.
- Animasyon **olacak** ama ölçülü; efekt sadece gerektiğinde.
- Her yerden bir şeyin uçup kaçtığı, sürekli efekt yağmuru **istenmiyor** (godottaslak3–4'teki meteor, film efekti vb. bu yüzden olmadı).
- Tamamen animasyonsuz, düz, "boş" bir görünüm de **istenmiyor**.

## MyRisale (eğitim / profil ekranları için) — `myrisale-*.jpg`
Okuma takip uygulaması.

Buradan alacaklarımız:
- **Karakter + rütbe.** Ortada sevimli 3 boyutlu bir karakter, altında rütbesi ("Çaylak") ve kaç rütbe olduğunu gösteren noktalar. → MarsKod'da robotumuz ve Python seviyesi.
- **Yumuşak kartlar.** Kırık beyaz zemin, köşeleri yuvarlak kartlar, çok hafif gölge; tek vurgu rengi (turuncu).
- **Basit sayaçlar ve ilerleme çubuğu.** Süre, sayı, "İlerleme %…", günlük hedef.
- **Görev listesi.** "Ne yapmalıyım?", "Görev 1" gibi büyük, tıklanır satırlar.
- Alt kısımda 3 büyük yuvarlak düğmeli sade menü.

## Unity taslaklarına çeviri (unitytaslak1 için yol haritası)
| Konu | Hedef |
|---|---|
| Kamera | Eğik (yaklaşık 40°), sabit; oyun alanı ekranın ortasında. Zemin küçük bir gezegen yüzeyi gibi uzakta hafifçe aşağı kıvrılır, böylece alandan ufuktaki koloniye doğal bir perspektifle uzanır (alana tepeden, koloniye yandan bakıyormuş gibi tutarsızlık olmamalı) |
| Zemin | **Tahta değil, Mars yüzeyi** (Enes, 09-26). Oyun alanı havada durmaz; **çevresiyle aynı hizada, Mars yüzeyine gömülü** bir bölgedir (hedef görünüm: `docs/tasarim/godottaslak2-bitis.png`). Zemin **tek parça** görünür: ayrı fayans/kare yok, kare sınırları zeminde ince ve soluk çizgiler; köşelerde küçük işaret direkleri. Karenin ne olduğunu zeminin kendisi söyler (tozlu, taneli, çakıllı regolit; buzların çevresinde buzlanma; krater). Alan bitince zemin düz ve boş kalmaz: **aynı doku etrafa devam eder**, kayalar ve kraterler serpiştirilir; uzaklaştıkça (koloniye doğru) ayrıntı azalır, hafif inişli çıkışlı olur ve tozlanarak arka plana karışır, ama tamamen düzleşmez; derinlik hissi verir. Satranç tahtası gibi iki renkli kare deseni yok |
| Robot ve nesneler | Tombul, pürüzsüz, oyuncak gibi; ince dış çizgi. Gerçekçi doku yok |
| Arka plan | Koyu tema: Mars'ta şafaktan hemen önce; tepelerin ardından taşan görünmez güneşin ışıltısı, yukarı doğru sıklaşan yıldızlar, ufukta koloni, hafif kum fırtınası sisi. Oyun alanıyla aynı zemin ufka kadar devam eder |
| Işık | Şafak öncesi: güneş görünmez, tepelerin ardındadır; soğuk ve loş gök ışığı. Zemin güneşe (uzağa, koloniye) doğru hafifçe aydınlanır, kameraya/kod kartına doğru kararır. Oyun alanı "güneş alıyor" gibi parlak olmaz; köşe direklerindeki lambalar ve robotun gözlerinden düşen ışık hafifçe aydınlatır |
| Gerçekçilik | Her kararda gerçekçilik esas (Enes): ışık yönü, gölge, parlaklık ve uzaklık birbiriyle tutarlı olmalı (ör. ışık kaynağından uzak yer daha parlak olamaz). Koyu tema korunur |
| Animasyon | Robot adımı yumuşak kayma, buz toplanınca küçük bir "pop", bölüm bitince tek, kısa kutlama |
| Arayüz | Üstte seviye + tek cümle hedef; altta kod paneli; ipucu ve geri al gibi az düğme |
