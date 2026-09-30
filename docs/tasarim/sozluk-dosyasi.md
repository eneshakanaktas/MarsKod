# Kod sözlüğü dosyası — nasıl yazılır

Sözlük, oyuncunun oyun ekranındaki 📖 düğmesiyle her an açtığı sayfalardır. Metinler tek bir dosyada:
`oyun/Assets/Resources/Sozluk/sozluk.json`. Kod bilmeden düzenlenebilir; yanlış yazılırsa `dotnet test` Türkçe olarak nerede olduğunu söyler.

## Örnek sayfa

```json
{
  "sayfalar": [
    {
      "baslik": "for",
      "kelimeler": ["for", "in"],
      "aciklama": "Bir işi istediğin kadar tekrarlatır. Altındaki girintili satırlar her turda bir kez çalışır.",
      "ornek": [
        "for i in range(3):",
        "    move(East)   # 3 kez"
      ]
    }
  ]
}
```

| Alan | Ne yazılır |
|---|---|
| `baslik` | Kartın ve kısayolun adı (`move`, `Yönler`…). |
| `kelimeler` | Sayfanın anlattığı kelimeler, bölüm dosyalarındaki adlarıyla (`komutlar`, `python_kelimeleri`, yönler). Sayfanın **hangi bölümde açıldığı buradan kendiliğinden bulunur**; bölüm numarası ayrıca yazılmaz. |
| `aciklama` | Bir iki cümle, yeni başlayanın anlayacağı dille; jargon yok. |
| `ornek` | Küçük örnek kod, her satır ayrı metin. Girinti 4 boşluk. `#` ile Türkçe not eklenebilir. |

## Kurallar (`dotnet test` denetler)

- Bölümlerde açılan her kelimenin (komut, yön, Python kelimesi) bir sayfası olmalı. Yeni bölüm yeni kelime açıyorsa buraya sayfası da eklenir.
- Her sayfanın kelimelerinden en az biri bir bölümde açılmalı (henüz oyunda olmayan kelimenin sayfası yazılmaz; ileride "🔒 kilitli sayfa" gelince değişecek).
- Örnek kod geçerli Python olmalı.
- Oyuncunun bulunduğu bölümde açılan sayfalar "YENİ" diye işaretlenir.
- Metinde ⏭ gibi özel işaretler kullanma: oyunun yazı tipinde yok, boş kutu çıkar.
