# MarsKod (geçici isim)

Telefonda oynanan, gerçek Python öğreten bulmaca oyunu (Mars kolonisi teması). Tasarım: `docs/superpowers/specs/2026-09-25-marskod-design.md`.
Teknik yol: Expo (React Native) + kendi mini-Python motorumuz. Önce Android.
Yapım planı: `docs/superpowers/plans/2026-09-25-marskod-yapim-plani.md`. Expo kuralları: @AGENTS.md

## Çalıştırma
- `npm install` → `npx expo start` (telefonda Expo Go ile QR okut, ya da `a` ile Android emülatörü).
- `npm test` (testler), `npm run typecheck` (tip denetimi). İş bitmeden ikisi de yeşil olmalı.
- Motor testleri: `tests/python-cases/<grup>/*.py` örnekleri. Yeni örnek ekleyince `npm run python-referans` (Python 3.12 şart) ile gerçek Python sonucunu üret. Motor bir grubu destekleyince grubu `tests/python-cases.test.ts` içindeki `ENABLED_GROUPS` listesine ekle.
- `src/engine` ve `src/world` saf mantıktır; React Native'e bağımlı olamaz.

## Genel kurallar
- Ekipte kodlama bilgisi yok; her zaman sade Türkçe açıkla, jargon kullanma.
- Token verimliliği önemli: iç işlerde kısa ol; kullanıcıya açıklamayı kısma.
- Yeni skill kurmadan önce SkillSpector ile tara; aynı işi yapan iki skill kurma.
- Fikir/karar değerlendirmede yeri geldiğinde: önce karşı çık, varsayımları sorgula, büyük resmi gör, görülmeyen fırsatları bul, tarafsız dış göz gibi bak. Gerçekçi ol.

## İlerleme notu (kritik)
Kökteki `ILERLEME.md`'yi oturum başında oku; her önemli adımdan sonra ve oturum sonunda güncelle (ne yapıldı, nerede kalındı, sıradaki adım, açık sorular). Kısa tut, eski girdileri özetle.

## Ekip döngüsü (projede birden fazla kişi çalışır: kullanıcı, Hamza, Ragıp)
1. Açılış: Kişi kendini tanıtır ("Merhaba, ben Ragıp"). Tanıtmazsa ilk iş adını sor.
2. "GitHub'dan güncel hâli çekiyorum" de → `git pull`.
3. `ILERLEME.md`'yi oku → "En son [kim], [ne zaman], [ne yaptı]; sıradaki iş; açık sorular" özetini 2-4 satırda ver.
4. Çalışırken ILERLEME.md'yi yerelde güncel tut (girdilere kişinin adını yaz). Push etme.
5. Kapanış: Kişi "bugünlük bitti" (veya benzeri) deyince → ILERLEME.md'ye günün özetini adıyla yaz → commit + push → "GitHub'a yüklendi" diye onayla. Push hata verirse (başkası önce yüklediyse) pull + birleştir, sonra tekrar push; çözemezsen kullanıcıya açıkla.
Kişi "bitti" demeden oturumu bırakacak gibiyse (vedalaşma vb.) push'u hatırlat.
