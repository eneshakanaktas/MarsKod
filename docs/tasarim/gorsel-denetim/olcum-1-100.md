# Otomatik yerleşim ölçümü — Bölüm 1-100 (ilk tarama)

> 2026-11-18, `-incele 1 100 … -yalniz-olcum` (Bölüm 1-5 düzeltmelerinden sonra). 290 uyarı = ~25 ayrı nesne (aynı nesne birçok bölümde).
> Ölçüm yalnız **nereye bakılacağını** gösterir; her biri kendi 5'lisinde görüntüyle karara bağlanır. Bu dosya o 5'lilerin başlangıç listesidir.
> Ölçmediği şeyler: gölgelendiriciyle çizilen izler/lekeler (ör. ayak izleri, zemin dokusundaki işaretler), gökyüzü resmindeki her şey. Onlar yine gözle bakılır.

## Muhtemelen gerçek (5'lisinde bakılacak)
| Bölüm | Nesne | Uyarı |
|---|---|---|
| 8, 21, 22 | `Iz-Panel` (devrik güneş paneli) | %75'i zeminin altında |
| 21, 22 | `Iz-Kablo` | %53 gömülü (senaryo "kuma gömülü kablo" diyor: belki kasıtlı) |
| 19, 20 | `Iz-Arac-KT2` | 7-23 cm havada |
| 25-30 | `Iz-Kulube` | 11 cm havada |
| 37 | `Iz-DirekArabasi` | 10 cm havada |
| 40-50 | `Iz-Anten` (dev anten) | bölgelerinde 0,2-2,5 m havada (çanak/kablo çıkıntısı olabilir) |
| 51 | `Iz-Roket`, `Iz-OkluKaya` | 3-8 cm havada |
| 51-60, 71 | `Obstacle` (Bölge 6 ve 71 engelleri) | 13-36 cm havada (kanyon zemini başka yükseklikte olabilir: ölçüm hatası da olabilir) |
| 56 | `Iz-GorevPanosu` | 5 cm havada |
| 58 | `Iz-KayaBasamagi` | 38 cm havada |
| 67-70 | `Iz-KayaDuvar` | 1,18 m'ye kadar havada (Bölüm 70 el sorunu da bu duvarda) |
| 71 | `Istasyon-Cephe`, `Iz-Kaya` | 4-19 cm havada |
| 76 | `Yataklar` | 29 cm havada |

## Muhtemelen kasıtlı / ölçüm yanlışı (5'lisinde doğrulanıp etiketlenecek)
| Bölüm | Nesne | Neden |
|---|---|---|
| 54-65 | `Serce` | uçan robot kuş: `Airborne` etiketi gerekir |
| 72-80 | `Istasyon-Oda` | iç mekân: tavan ve duvarlar doğal olarak "havada"; Bölge 8 için ölçüm iç mekân zeminine göre ayarlanmalı |
| 32-44 | `Obstacle-Kum`, 71 `Iz-Kum` | kum tümseği: yarısı zemine gömülü tasarlanmış olabilir |
| 60 | `Iz-Yarik` | yarık zeminin altında (tasarım) |

Uyarı çıkmayan bölümler: 1-7, 9-18, 23, 24, 31, 34, 38, 53, 81-100.
