# Bölüm dosyası nasıl yazılır

Her bölüm tek bir dosyadır: `oyun/Assets/Resources/Bolumler/bolum-NN.json` (örn. `bolum-04.json`).
Kod bilmeden yazılabilir: bir metin düzenleyicide (VS Code, Not Defteri) açıp aşağıdaki kalıba göre doldurmak yeter.
En kolayı: var olan bir bölümü kopyala, adını ve numarasını değiştir, içini düzenle.

## Örnek (Bölüm 2)

```json
{
  "numara": 2,
  "baslik": "Kaya engeli",
  "gorev": "2 buz topla",
  "harita": [
    ". . . . . .",
    ". . . . . .",
    ". . . B . .",
    "R . B K . .",
    ". . . . K .",
    ". . . . . ."
  ],
  "komutlar": ["move", "collect"],
  "konular": ["komut sırası", "yönler"],
  "ipuclari": [
    "1. ipucu: yön gösterir",
    "2. ipucu: konuyu hatırlatır",
    "3. ipucu: kodun bir kısmını verir"
  ],
  "tipik_hatalar": [
    {
      "kod": ["move(East)", "move(East)", "collect()", "move(East)"],
      "aciklama": "Robot kayaya çarpar."
    }
  ],
  "baslangic_kodu": ["move(East)", "move(East)"],
  "cozum": [
    "move(East)",
    "move(East)",
    "collect()",
    "move(North)",
    "move(East)",
    "collect()"
  ]
}
```

## Alanlar

| Alan | Ne yazılır |
|---|---|
| `numara` | Bölümün sırası (tırnaksız sayı). Dosya adıyla aynı olmalı: 4 → `bolum-04.json`. |
| `baslik` | Kısa ad, ekranın üstünde küçük harflerle görünür ("BÖLÜM 2 · KAYA ENGELİ"). |
| `gorev` | Tek cümle görev, üstte büyük yazıyla görünür ("2 buz topla"). |
| `harita` | Alanın "resmi" (aşağıda). |
| `komutlar` | Bu bölümde açık olan oyun komutları. Şimdilik: `move`, `collect`. Listede olmayan komutu kullanan oyuncuya "Bu komut henüz açılmadı" denir. |
| `konular` | Bölümün öğrettiği konular (ileride oyuncu profili bunlarla tutulacak). |
| `ipuclari` | En az 1, ideali 3 ipucu: 1. yön gösterir, 2. konuyu hatırlatır, 3. kodun bir kısmını verir. `<b>kalın</b>` yazılabilir. Şimdilik ampul düğmesinde 1. ipucu görünür. |
| `tipik_hatalar` | Oyuncuların sık yapacağı hatalar: hatalı kod + ne olduğunun açıklaması. İsteğe bağlı ama önerilir. |
| `baslangic_kodu` | Oyuncunun önüne ilk gelen kod (isteğe bağlı; kod yazma alanı gelince kullanılacak). |
| `cozum` | Bir doğru çözüm. Kod yazma alanı gelene kadar kartta bu görünür. |

Kodlar (`cozum`, `baslangic_kodu`, `kod`) her satırı ayrı tırnakta olan bir listedir. Girinti için satırın başına 4 boşluk: `"    move(East)"`.

## Harita

Her satır alanın bir sırasıdır; **en üstteki satır kuzey** (ekranda alanın arkası), sol taraf batı.
Harfler arasındaki boşluklar önemsizdir, sadece okumayı kolaylaştırır. Şimdilik harita **6×6** olmalı.

| İşaret | Anlamı |
|---|---|
| `R` | Robot (tam olarak bir tane) |
| `B` | Buz (toplanacak) |
| `K` | Kaya (robot içinden geçemez) |
| `H` | Hedef kare (en fazla bir tane; kod bitince robot burada durmalı) |
| `.` | Boş kare |

Görev: tüm buzlar toplanmış **ve** (hedef varsa) robot hedef karede olmalı. Kod hatayla ya da oyun kuralıyla (kaya, alan sınırı) durursa görev tamam sayılmaz.

## Denetim

Bilgisayarda `motor-test` klasöründe `dotnet test` çalıştırınca her bölüm dosyası kendiliğinden denetlenir:

- Dosya doğru yazılmış mı (eksik virgül, tırnak, bilinmeyen alan...). Yanlışta satır numarasıyla Türkçe mesaj verir.
- Doğru çözüm görevi gerçekten bitiriyor mu.
- Tipik hatalar ve başlangıç kodu görevi bitirmiyor mu (bitiriyorsa örnek gerçekten hatalı değildir).
- Numaralar 1'den başlayıp boşluksuz mu, dosya adı numarayla uyumlu mu.

Kod bilmiyorsan: dosyayı yazıp Claude'a "bölüm dosyasını denetle" demen yeterli.
