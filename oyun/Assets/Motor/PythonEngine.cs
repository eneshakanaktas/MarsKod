// Mini-Python motorunun dışa açılan yüzü. Oyundan habersizdir.

using System;

namespace MarsKod.Motor
{
    public sealed class PythonError
    {
        /// <summary>Gerçek Python'daki hata türü, örn. "NameError"</summary>
        public string Type;
        /// <summary>Gerçek Python'un İngilizce mesajıyla birebir aynı metin</summary>
        public string Message;
        /// <summary>Hatanın olduğu satır (1'den başlar); bilinmiyorsa null</summary>
        public int? Line;

        public override string ToString() => Type + ": " + Message + " (satır " + Line + ")";
    }

    /// <summary>Python hatası olmadan durma: desteklenmeyen özellik ya da oyunun koruma sınırı.</summary>
    public sealed class Halt
    {
        /// <summary>"unsupported" ya da "limit"</summary>
        public string Kind;
        /// <summary>Kind "unsupported" ise: Feature sınıfındaki ad</summary>
        public string Feature;
        public string Detail;
        /// <summary>Kind "limit" ise: "steps" ya da "size"</summary>
        public string Reason;
        public int? Line;

        public override string ToString() => Kind + " " + (Feature ?? Reason) + (Detail != null ? " (" + Detail + ")" : "") + " (satır " + Line + ")";
    }

    public sealed class RunResult
    {
        /// <summary>print ile ekrana yazılan her şey</summary>
        public string Output;
        public PythonError Error;
        public Halt Halt;
    }

    public static class PythonEngine
    {
        public static RunResult RunPython(string source, InterpreterOptions options = null)
        {
            Interpreter interpreter = null;
            try
            {
                var module = Parser.Parse(source);
                interpreter = new Interpreter(options ?? new InterpreterOptions());
                foreach (var _ in interpreter.Run(module))
                {
                }
                return new RunResult { Output = interpreter.Output };
            }
            catch (Exception e) when (e is PythonException || e is UnsupportedFeature || e is ExecutionLimit)
            {
                return new RunResult { Output = interpreter?.Output ?? "", Error = ErrorOf(e), Halt = HaltOf(e) };
            }
        }

        /// <summary>Motorun fırlattığı Python hatası; değilse null</summary>
        public static PythonError ErrorOf(Exception e) =>
            e is PythonException p ? new PythonError { Type = p.PyType, Message = p.PyMessage, Line = p.Line } : null;

        /// <summary>Motorun fırlattığı durma sebebi; değilse null</summary>
        public static Halt HaltOf(Exception e)
        {
            switch (e)
            {
                case UnsupportedFeature u:
                    return new Halt { Kind = "unsupported", Feature = u.FeatureId, Detail = u.Detail, Line = u.Line };
                case ExecutionLimit l:
                    return new Halt { Kind = "limit", Reason = l.Reason, Line = l.Line };
                default:
                    return null;
            }
        }
    }
}
