// Defne'nin kayıtları (hikaye-kitabi.md "Kayıtlar"): metinlerin oyundaki tek yeri.
// Bölüm 50 kapanışı, bölüm başları (52, 64) ve Kayıtlar sayfası buradan okur. Yeni kayıt = listeye bir satır.

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    public sealed class Recording
    {
        /// <summary>Level: kaydın oynadığı bölüm. AtLevelStart: bölüm başında mı (52, 64), yoksa bölüm kapanışında mı (50).</summary>
        public readonly int Number, Region, Level;
        public readonly bool AtLevelStart;
        public readonly string File;
        public readonly IReadOnlyList<string> Lines;

        public Recording(int number, int region, int level, bool atLevelStart, string file, params string[] lines)
        {
            Number = number;
            Region = region;
            Level = level;
            AtLevelStart = atLevelStart;
            File = file;
            Lines = lines;
        }
    }

    public static class Recordings
    {
        public static readonly IReadOnlyList<Recording> All = new[]
        {
            new Recording(1, 5, 50, false, "ogretmene.ses",
                "Bu bir ödev değil.",
                "Adım Defne Aras. Bu koloninin komutanıyım.",
                "Bunu duyuyorsan, onu yeniden öğretecek kişi sensin.",
                "Bir güneş fırtınası geliyor. Biz yeraltına iniyoruz, hepimiz. Uyuyacağız.",
                "Fırtına robotumuzun belleğini silecek. Ona her şeyi baştan öğretmen gerekecek.",
                "Kızım Ece ona Kıvılcım der. Ona iyi bak.",
                "Yolu işaretledik. Okları takip et.",
                "Seni tanımıyorum. Ama sana güveniyorum. Lütfen… ona öğret."),
            new Recording(2, 6, 52, true, "rutinler.ses",
                "Ona ezber değil, beceri öğret.",
                "Bir kez öğrettiğini kendisi tekrar edebilir.",
                "Bunlara biz 'rutin' derdik.",
                "İlki sabah turuydu; her sabah yapardı."),
            new Recording(3, 7, 64, true, "izler.ses",
                "Ece yine önden koştu.",
                "Bir anlığına onu kaybettim.",
                "Sonra buldum; bir duvarın önünde, elinde boya kalemi.",
                "Ona kızamadım."),
        };

        /// <summary>Numarası verilen kayıt; yoksa null.</summary>
        public static Recording ByNumber(int number) => All.FirstOrDefault(r => r.Number == number);

        /// <summary>Bölüm başında oynayan kayıt (52, 64); yoksa null. Bölüm 50'nin kaydı kapanışta oynar, burada çıkmaz.</summary>
        public static Recording AtStartOf(int level) => All.FirstOrDefault(r => r.AtLevelStart && r.Level == level);

        /// <summary>İlk satırın ilk kelimeleri; satır daha uzunsa sonuna "…" gelir (Kayıtlar sayfası).</summary>
        public static string Preview(Recording recording, int words = 4)
        {
            var parts = recording.Lines[0].Split(' ');
            if (parts.Length <= words) return recording.Lines[0];
            return string.Join(" ", parts.Take(words)) + "…";
        }
    }
}
