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
| `komutlar` | Bu bölümde açık olan oyun komutları. Şimdilik: `move`, `collect`, `ice_here` ("bu karede buz var mı?" diye bakar, True/False verir; `if` için), `rock_ahead` ("bu yöne bir adım atarsam dururum mu?" diye bakar — kaya ya da alanın sınırı; True/False verir; `if`/`else` için), `panel_power` (robotun durduğu paneldeki gücü sayı olarak verir, 0-100; panel yoksa 0; karşılaştırma için), `repair` (çatlak paneli onarır, gücü 100 olur), `dust_here` ("bu karede toz bulutu var mı?" diye bakar, True/False; `while` için), `wait` (toz bulutunun içinde bir tur bekler; toz yokken beklemek robotu durdurur), `cable_length` (robotun durduğu karedeki kablo makarasının uzunluğu, metre; makara yoksa ya da toplandıysa 0), `report` (parantezdeki tam sayıyı antene gönderir; `rapor` alanındaki sayı değilse robot durur). Kırık panelin parçası `collect()` ile toplanır. Listede olmayan komutu kullanan oyuncuya "Bu komut henüz açılmadı" denir. |
| `parcalar` | İsteğe bağlı. Acemi paletindeki düğmeler; her öğe tek satır kod, örn. `["move(East)", "collect()", "for i in range(3):"]`. Yazılmazsa açık komutlardan çıkar (`move` → dört yön, `collect` → `collect()`). Sayıları bilerek çözümdekinden farklı yaz (`range(3)`), oyuncu değiştirsin. `:` ile biten satır tek başına yazılabilir. |
| `python_kelimeleri` | İsteğe bağlı. Bu bölümde açılan Python kelimeleri (`["for", "in", "range"]`); Orta kademedeki öneri satırında çıkar. Önceki bölümlerde açılanlar sonrakilerde de açık kalır. |
| `toz_suresi` | Haritada toz bulutu (`Z`) varsa zorunlu: her bulutun kaç beklemede (`wait()`) dağılacağı, tırnaksız sayı listesi, örn. `[3, 5]`. Sıra haritadaki okuma sırasıdır: üst satırdan başla, soldan sağa. Oyuncu bu sayıları görmez; bu yüzden doğru çözüm `while dust_here(): wait()` olur. |
| `makara_uzunlugu` | Haritada kablo makarası (`M`) varsa zorunlu: her makaranın uzunluğu (metre), tırnaksız sayı listesi, `toz_suresi` gibi haritadaki okuma sırasıyla. Oyuncu bu sayıları görmez; `cable_length()` ölçer. |
| `rapor` | `komutlar` içinde `report` varsa zorunlu (yoksa yazılmaz): antenin `report()` ile beklediği sayı (örn. makara sayısı, toplam uzunluk). Yanlış sayı gönderilirse robot durur; hiç gönderilmezse görev bitmez. |
| `rutinler` | İsteğe bağlı (Bölge 6'dan itibaren, fonksiyonlar). Oyuncunun `def` ile yazması gereken rutin adları, örn. `["sabah_turu"]`. Her biri tanımlı olmalı ve kod çalışırken **en az 2 kez** çalışmalı; yoksa robot işi bitirse de görev bitmez ("Kod bitti, sabah_turu rutini yok"). Ad: harf ya da `_` ile başlar, boşluk yok, Python kelimesi ya da oyun komutu değil. Görev yazısında adı söyle. |
| `rutin_sayisi` | İsteğe bağlı. Adı oyuncuya bırakılmış rutin şartı: en az 2 kez çalışan en az bu kadar farklı rutin olmalı (örn. `1`, `2`). `rutinler` ile birlikte yazılabilir. |
| `konular` | Bölümün öğrettiği konular (ileride oyuncu profili bunlarla tutulacak). |
| `ipuclari` | En az 1, ideali 3 ipucu: 1. yön gösterir, 2. konuyu hatırlatır, 3. kodun bir kısmını verir. Kod parçası ters tırnak içine yazılır (`` `move(East)` ``): tırnaklar görünmez, kod renginde çıkar. Ampule basınca 1. ipucu görünür; oyuncu "Bir ipucu daha" dedikçe 2. ve 3. altına eklenir (şimdilik üçü de bedava). |
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
| `E` | Enerji hücresi (toplanacak; Bölge 1: Bölüm 1-10) |
| `B` | Buz (toplanacak; Bölge 2: Bölüm 11-20) |
| `K` | Kaya (robot içinden geçemez) |
| `T` | Tehlikeli kırmızı kristal (üstünden geçilir; `collect()` ile toplamaya çalışmak robotu durdurur, oyun kuralı) |
| `H` | Hedef kare (en fazla bir tane; kod bitince robot burada durmalı) |
| `P` | Pusula parçası (toplanacak; Bölge 4: Bölüm 31-40) |
| `Z` | Toz bulutu (Bölge 4). Robot içine girebilir ama içindeyken yürüyemez; `wait()` ile bekler, bulut `toz_suresi`ndeki sayı kadar beklemede dağılır. Toz yokken `wait()` robotu durdurur (oyun kuralı). |
| `0`-`9` | Güneş paneli (Bölge 3: Bölüm 21-30). Rakam gücün onda biri: `7` → gücü 70. `0` kırık panel (parçası `collect()` ile toplanır), `1`-`4` çatlak (`repair()` ile onarılmalı), `5`-`9` sağlam (dokunulmaz). Sağlam panele `repair()`/`collect()`, kırık panele `repair()`, boş kareye `repair()` robotu durdurur (oyun kuralı). |
| `M` | Kablo makarası (toplanacak; Bölge 5: Bölüm 41-50). Uzunluğu `makara_uzunlugu` listesinde; `collect()` ile toplanır, toplanınca `cable_length()` 0 verir (önce ölç, sonra topla). |
| `D` | Drone parçası (toplanacak; Bölge 6: Bölüm 51-60) |
| `.` | Boş kare |

Bir bölümde tek tür toplanacak olur (`E` ile `B` aynı haritada olmaz). `ice_here` yalnızca buz (`B`) olan bölümlerde açılabilir. Yeni bölge nesnesi eklemek: `oyun/Assets/Dunya/Collectible.cs` (işaret + Türkçe ad), görünüşü sahnede.

Görev: tüm toplanacaklar toplanmış, tüm çatlak paneller onarılmış, (istendiyse) doğru sayı `report()` ile gönderilmiş (istendiyse) rutin şartı sağlanmış **ve** (hedef varsa) robot hedef karede olmalı. Kod hatayla ya da oyun kuralıyla (kaya, alan sınırı) durursa görev tamam sayılmaz.

## Denetim

Bilgisayarda `motor-test` klasöründe `dotnet test` çalıştırınca her bölüm dosyası kendiliğinden denetlenir:

- Dosya doğru yazılmış mı (eksik virgül, tırnak, bilinmeyen alan...). Yanlışta satır numarasıyla Türkçe mesaj verir.
- Doğru çözüm görevi gerçekten bitiriyor mu.
- Tipik hatalar ve başlangıç kodu görevi bitirmiyor mu (bitiriyorsa örnek gerçekten hatalı değildir).
- Her `parcalar` öğesi geçerli tek satır Python mu, yalnızca o bölümde açık komutları mı kullanıyor; `python_kelimeleri` motorun tanıdığı kelimeler mi.
- Numaralar 1'den başlayıp boşluksuz mu, dosya adı numarayla uyumlu mu.

Kod bilmiyorsan: dosyayı yazıp Claude'a "bölüm dosyasını denetle" demen yeterli.
