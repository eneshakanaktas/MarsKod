// Değişen yol (Bölge 7, Bölüm 68-70): oyuncunun kodu her izde yeni bir dünyada ayrı çalışır. Görev hepsinde bitince biter.
// Ekranda hangi koşunun oynatılacağı: başarısız olan varsa ilki (oyuncu kodun nerede takıldığını görür), hepsi tamamsa
// her "Çalıştır"da sıradaki iz (oyuncu yolun değiştiğini görür).

using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    /// <summary>Kodun bir izdeki koşusu: o izin dünyası ve olanların kaydı</summary>
    public sealed class TrailRun
    {
        public readonly World World;
        public readonly RunReport Report;

        public TrailRun(World world, RunReport report)
        {
            World = world;
            Report = report;
        }
    }

    public static class TrailRuns
    {
        /// <summary>Gösterilecek koşu. turn: kaçıncı "Çalıştır" (0'dan başlar).</summary>
        public static TrailRun Pick(IReadOnlyList<TrailRun> runs, int turn)
        {
            var failed = runs.FirstOrDefault(r => !r.Report.Complete);
            return failed ?? runs[turn % runs.Count];
        }

        /// <summary>Kod bazı izlerde başarılı, bazılarında değil mi (yol değişince takılan kod)</summary>
        public static bool Mixed(IReadOnlyList<TrailRun> runs) =>
            runs.Any(r => r.Report.Complete) && runs.Any(r => !r.Report.Complete);
    }
}
