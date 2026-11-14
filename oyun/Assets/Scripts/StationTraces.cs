// Bolge 8 · Terk edilmis arastirma istasyonu (Bolum 71-80, return + metin): docs/tasarim/senaryo-bolge-08.md.
// Simdilik yalnizca bolge acilisi; istasyonun gorunusu (ekran, duvar cizimleri, harita) Bolge 8 · 3a isinde buraya eklenir.
public static class StationTraces
{
    public const int FirstLevel = 71, LastLevel = 80;

    // Bolge 8 acilisi: kara ekranda iki satir (listeyi onceden soylemez)
    public static readonly string[] TransitionLines = { "Yeni bölge: Araştırma istasyonu", "Hedef: koloninin kayıtları" };
}
