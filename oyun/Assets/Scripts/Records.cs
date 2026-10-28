// Defne'nin ses kayitlari (hikaye-kitabi.md "Kayitlar"). Metinler ve hangi bolumde cikacaklari tek yerde; kayit sahnesi
// (RecordingScene), bolum basi/sonu ve "Kayıtlar" sayfasi hep buradan okur.
public static class Records
{
    public struct Record
    {
        public int Number;          // Kayit 1, 2, 3...
        public int Level;           // kaydin geldigi bolum
        public bool AtLevelStart;   // true: bolume girince (bir kez) calar; false: bolum bitince (final sahnesi kendisi calar)
        public string File;
        public string[] Lines;
        public string Key => "kayit" + Number;   // PlayerPrefs: dinlendi mi
    }

    // Kayit 1 = Perde 1 finali (Bolum 50 kapanisi): metin senaryo-perde-1.md
    public static readonly Record First = new Record
    {
        Number = 1, Level = AntennaTraces.LastLevel, AtLevelStart = false, File = "ogretmene.ses",
        Lines = new[]
        {
            "Bu bir ödev değil.",
            "Adım Defne Aras. Bu koloninin komutanıyım.",
            "Bunu duyuyorsan, onu yeniden öğretecek kişi sensin.",
            "Bir güneş fırtınası geliyor. Biz yeraltına iniyoruz, hepimiz. Uyuyacağız.",
            "Fırtına robotumuzun belleğini silecek. Ona her şeyi baştan öğretmen gerekecek.",
            "Kızım Ece ona Kıvılcım der. Ona iyi bak.",
            "Yolu işaretledik. Okları takip et.",
            "Seni tanımıyorum. Ama sana güveniyorum. Lütfen… ona öğret.",
        },
    };

    // Kayit 2: Bolum 52 basinda, bir kez
    public static readonly Record Second = new Record
    {
        Number = 2, Level = CanyonTraces.RecordingLevel, AtLevelStart = true, File = "rutinler.ses",
        Lines = new[]
        {
            "Ona ezber değil, beceri öğret.",
            "Bir kez öğrettiğini kendisi tekrar edebilir.",
            "Bunlara biz 'rutin' derdik.",
            "İlki sabah turuydu; her sabah yapardı.",
        },
    };

    // Kayit 3: Bolum 64 basinda, bir kez
    public static readonly Record Third = new Record
    {
        Number = 3, Level = 64, AtLevelStart = true, File = "boya-kalemi.ses",
        Lines = new[]
        {
            "Ece yine önden koştu.",
            "Bir anlığına onu kaybettim.",
            "Sonra buldum; bir duvarın önünde, elinde boya kalemi.",
            "Ona kızamadım.",
        },
    };

    public static readonly Record[] All = { First, Second, Third };
}
