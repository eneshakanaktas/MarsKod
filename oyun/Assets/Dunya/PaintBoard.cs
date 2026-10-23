// Çizim kuralı (Bölge 7, parametreli fonksiyonlar): Kıvılcım'ın kalemi başta kalkıktır; kalem inikken geçtiği kareleri boyar.
// Bölüm, boyanacak kareleri ("O" işareti) söyler; görev, hepsi boyanıp fazladan kare boyanmayınca biter.
// Saf mantıktır (Unity'den habersiz); World bunu kullanır ve olayları (Painted, PenChanged) kendisi üretir.

using System.Collections.Generic;

namespace MarsKod.Dunya
{
    /// <summary>Çizim yarım ya da bozuk kaldığında oyuncuya gösterilecek açıklama</summary>
    public sealed class PaintProblem
    {
        public string Title;
        public string Text;
    }

    public sealed class PaintBoard
    {
        readonly HashSet<Cell> wanted;
        readonly HashSet<Cell> painted = new HashSet<Cell>();

        public PaintBoard(IEnumerable<Cell> target)
        {
            wanted = new HashSet<Cell>(target);
        }

        /// <summary>Bu bölümde boyanacak kare var mı (çizimli bölüm)</summary>
        public bool Active => wanted.Count > 0;

        public bool PenDown { get; private set; }

        /// <summary>Kalem bir kez olsun indi mi (hiç inmediyse mesaj bunu söyler)</summary>
        public bool EverLowered { get; private set; }

        /// <summary>Boyanacak ama henüz boyanmamış kare sayısı</summary>
        public int Missing
        {
            get
            {
                int n = 0;
                foreach (var c in wanted)
                    if (!painted.Contains(c)) n++;
                return n;
            }
        }

        /// <summary>Boyanmış ama boyanacak olmayan kare sayısı</summary>
        public int Extra
        {
            get
            {
                int n = 0;
                foreach (var c in painted)
                    if (!wanted.Contains(c)) n++;
                return n;
            }
        }

        /// <summary>Boyanacak karelerin hepsi boyandı ve fazladan kare yok</summary>
        public bool Done => Missing == 0 && Extra == 0;

        public bool WantedAt(Cell c) => wanted.Contains(c);

        public bool PaintedAt(Cell c) => painted.Contains(c);

        /// <summary>Kalemi indirir ve bulunduğu kareyi boyar. O kare ilk kez boyandıysa true.</summary>
        public bool Lower(Cell at)
        {
            PenDown = true;
            EverLowered = true;
            return painted.Add(at);
        }

        public void Raise() => PenDown = false;

        /// <summary>Robot bir kareye girdi: kalem inikse boyar. Kare ilk kez boyandıysa true (kalem kalkıkken hep false).</summary>
        public bool Enter(Cell at) => PenDown && painted.Add(at);

        /// <summary>Çizim tamamsa null; değilse ne eksik olduğunu söyleyen açıklama.</summary>
        public PaintProblem Problem()
        {
            if (Done) return null;
            if (!EverLowered)
                return new PaintProblem
                {
                    Title = "Kalem hiç inmedi",
                    Text = "Kod bitti ama Kıvılcım hiçbir kareyi boyamadı. Çizmek için kalemi indir: pen_down().",
                };
            int extra = Extra;
            if (extra > 0)
                return new PaintProblem
                {
                    Title = "İşaret bozuldu",
                    Text = "Fazladan " + extra + " kare boyadın. Yürürken boyamamak için kalemi kaldır: pen_up().",
                };
            return new PaintProblem
            {
                Title = "İşaret yarım kaldı",
                Text = Missing + " kare boyanmadı. Silik turuncu karelerin hepsini boya.",
            };
        }
    }
}
