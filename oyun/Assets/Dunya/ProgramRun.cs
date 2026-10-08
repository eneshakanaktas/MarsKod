// Oyuncunun kodunu dünyada baştan sona çalıştırır ve olanları satır satır kaydeder.
// Sahne kodu önce bununla anında çalıştırır, sonra kaydı animasyonla oynatır: böylece bitmeyen
// döngü oyunu dondurmaz, "adım adım" modu da aynı kayıtla çalışabilir.

using System;
using System.Collections.Generic;
using MarsKod.Motor;

namespace MarsKod.Dunya
{
    /// <summary>Çalışan bir satır ve o satırda dünyada olanlar</summary>
    public sealed class TraceEntry
    {
        /// <summary>1'den başlar</summary>
        public int Line;
        public readonly List<WorldEvent> Events = new List<WorldEvent>();
        /// <summary>Satır çalıştıktan sonra değişkenler: ad ve Python'daki görünüşü (fonksiyon, sınıf gibi değer olmayanlar yok).
        /// Adım adım modunda satırın yanında gösterilir.</summary>
        public readonly List<KeyValuePair<string, string>> Vars = new List<KeyValuePair<string, string>>();

        /// <summary>Satırın yanında gösterilecek metin: "i = 2 · adim = 5"; uzun değer kısaltılır. Değişken yoksa boş.</summary>
        public string VarsText(int maxValueLength = 14)
        {
            var parts = new List<string>(Vars.Count);
            foreach (var pair in Vars)
            {
                string value = pair.Value.Length > maxValueLength ? pair.Value.Substring(0, maxValueLength - 1) + "…" : pair.Value;
                parts.Add(pair.Key + " = " + value);
            }
            return string.Join(" · ", parts);
        }
    }

    public sealed class RunReport
    {
        public readonly List<TraceEntry> Trace = new List<TraceEntry>();
        /// <summary>print ile yazılanlar</summary>
        public string Output = "";
        /// <summary>Python hatası (gerçek Python'daki gibi); yoksa null</summary>
        public PythonError Error;
        /// <summary>Desteklenmeyen özellik ya da adım sınırı; yoksa null</summary>
        public Halt Halt;
        /// <summary>Oyun kuralı yüzünden durma; yoksa null</summary>
        public GameRuleStop Rule;
        /// <summary>Durmanın olduğu satır (1'den başlar); düzgün bittiyse ya da bilinmiyorsa null</summary>
        public int? StopLine;
        /// <summary>Görev tamamlandı mı: kod durmadan bitti ve görev yerine geldi. Kod hatayla ya da oyun kuralıyla durduysa
        /// görev yerine gelmiş olsa bile tamam sayılmaz (örn. son buzu toplayıp alanın dışına çıkmak isteyen kod yanlıştır).</summary>
        public bool Complete;
        /// <summary>Oyuncunun def ile yazdığı (modül düzeyindeki) rutinler</summary>
        public readonly HashSet<string> DefinedRoutines = new HashSet<string>();
        /// <summary>Her rutinin kaç kez çalıştığı (döngüden ya da başka rutinden çağrılar dahil)</summary>
        public readonly Dictionary<string, int> RoutineRuns = new Dictionary<string, int>();
        /// <summary>Bölümün rutin şartı karşılanmadıysa açıklaması; şart yoksa, sağlandıysa ya da kod durduysa null</summary>
        public RoutineProblem RoutineProblem;

        public bool Stopped => Error != null || Halt != null || Rule != null;
    }

    public static class ProgramRun
    {
        /// <summary>Oyunda bir çalıştırmanın en fazla adımı (bitmeyen döngü bu sınırda durur)</summary>
        public const int DefaultMaxSteps = 20000;

        public static RunReport Execute(string source, World world, int maxSteps = DefaultMaxSteps, RoutineRule routines = null)
        {
            var report = new RunReport();
            Interpreter interpreter = null;
            TraceEntry current = null;
            Frame frame = null; // current satirinin calistigi cerceve (degiskenleri satir bitince okunur)
            var calls = new HashSet<Frame>(); // her rutin cagrisi yeni bir cerceve: ilk adiminda bir kez sayilir
            world.Events.Clear();
            try
            {
                var module = Parser.Parse(source);
                interpreter = new Interpreter(new InterpreterOptions { MaxSteps = maxSteps, Externals = world.Commands() });
                foreach (var step in interpreter.Run(module))
                {
                    Flush(world, current, frame);
                    current = new TraceEntry { Line = step.Line };
                    frame = step.Frame;
                    CountCall(report, calls, step.Frame);
                    report.Trace.Add(current);
                }
            }
            catch (Exception e) when (e is PythonException || e is UnsupportedFeature || e is ExecutionLimit)
            {
                report.Error = PythonEngine.ErrorOf(e);
                report.Halt = PythonEngine.HaltOf(e);
                report.StopLine = report.Error?.Line ?? report.Halt?.Line ?? current?.Line;
            }
            catch (GameRuleStop e)
            {
                report.Rule = e;
                report.StopLine = current?.Line;
            }
            Flush(world, current, frame);
            report.Output = interpreter?.Output ?? "";
            if (interpreter != null) ReadDefinedRoutines(report, interpreter.Globals);
            if (!report.Stopped && routines != null && !routines.IsEmpty)
                report.RoutineProblem = routines.Check(report.DefinedRoutines, report.RoutineRuns);
            report.Complete = !report.Stopped && world.Complete && report.RoutineProblem == null;
            return report;
        }

        // Rutin cagrisinin ilk adimi: cerceve yeniyse o rutinin calisma sayisini bir artirir
        static void CountCall(RunReport report, HashSet<Frame> calls, Frame frame)
        {
            if (frame?.Fn == null || !calls.Add(frame)) return;
            string name = frame.Fn.Def.Name;
            report.RoutineRuns[name] = report.RoutineRuns.TryGetValue(name, out var n) ? n + 1 : 1;
        }

        // Kod bitince modul duzeyinde fonksiyon olarak duran adlar = oyuncunun def ile yazdigi rutinler
        static void ReadDefinedRoutines(RunReport report, Frame globals)
        {
            foreach (var pair in globals.Vars)
                if (pair.Value is PyFunction) report.DefinedRoutines.Add(pair.Key);
        }

        // Biten satira dunyada olanlari ve degiskenlerin son halini yazar
        static void Flush(World world, TraceEntry entry, Frame frame)
        {
            if (entry != null)
            {
                entry.Events.AddRange(world.Events);
                if (frame != null)
                    foreach (var pair in frame.Vars)
                        if (!pair.Key.StartsWith("__", StringComparison.Ordinal) && IsValue(pair.Value)) entry.Vars.Add(new KeyValuePair<string, string>(pair.Key, Values.Repr(pair.Value)));
            }
            world.Events.Clear();
        }

        // Oyuncuya degisken olarak gosterilecek deger mi (fonksiyon, sinif, yerlesik fonksiyon degil; Python'un
        // kendi __name__ gibi adlari yukarida ayiklanir)
        static bool IsValue(object v) => !(v is PyFunction || v is PyBuiltin || v is PyMethod || v is PyType);
    }
}
