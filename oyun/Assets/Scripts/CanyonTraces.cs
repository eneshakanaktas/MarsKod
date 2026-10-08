// Bolge 6 · Kanyon girisi (Bolum 51-60, fonksiyonlar): docs/tasarim/senaryo-bolge-06.md.
// Simdilik yalnizca hikaye: bolge acilisi ve Bolum 52 basindaki Kayit 2. Gorunus (roket, depo, Serce) 3. parcada eklenir.
public static class CanyonTraces
{
    public const int FirstLevel = 51, RecordingLevel = 52, LastLevel = 60;

    // Bolge 6 acilisi: kara ekranda iki satir
    public static readonly string[] TransitionLines = { "Yeni bölge: Kanyon girişi", "Hedef: kanyon deposu" };

    // Kayit 2 (Defne'nin sesi, hikaye-kitabi.md "Kayitlar"): Bolum 52 basinda, bir kez
    public const string RecordingFile = "rutinler.ses";
    public static readonly string[] RecordingLines =
    {
        "Ona ezber değil, beceri öğret.",
        "Bir kez öğrettiğini kendisi tekrar edebilir.",
        "Bunlara biz 'rutin' derdik.",
        "İlki sabah turuydu; her sabah yapardı.",
    };
}
