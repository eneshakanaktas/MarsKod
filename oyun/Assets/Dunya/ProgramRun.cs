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
        /// <summary>Görev tamamlandı mı (tüm buzlar toplandı)</summary>
        public bool Complete;

        public bool Stopped => Error != null || Halt != null || Rule != null;
    }

    public static class ProgramRun
    {
        /// <summary>Oyunda bir çalıştırmanın en fazla adımı (bitmeyen döngü bu sınırda durur)</summary>
        public const int DefaultMaxSteps = 20000;

        public static RunReport Execute(string source, World world, int maxSteps = DefaultMaxSteps)
        {
            var report = new RunReport();
            Interpreter interpreter = null;
            TraceEntry current = null;
            world.Events.Clear();
            try
            {
                var module = Parser.Parse(source);
                interpreter = new Interpreter(new InterpreterOptions { MaxSteps = maxSteps, Externals = world.Commands() });
                foreach (var step in interpreter.Run(module))
                {
                    Flush(world, current);
                    current = new TraceEntry { Line = step.Line };
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
            Flush(world, current);
            report.Output = interpreter?.Output ?? "";
            report.Complete = world.Complete;
            return report;
        }

        static void Flush(World world, TraceEntry entry)
        {
            if (entry != null) entry.Events.AddRange(world.Events);
            world.Events.Clear();
        }
    }
}
