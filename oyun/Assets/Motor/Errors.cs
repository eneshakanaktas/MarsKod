// Motorun içinde fırlatılan hatalar.

using System;

namespace MarsKod.Motor
{
    /// <summary>Gerçek Python hatası. Dışarıya PythonError olarak çıkar.</summary>
    public class PythonException : Exception
    {
        /// <summary>Gerçek Python'daki hata türü, örn. "SyntaxError"</summary>
        public readonly string PyType;
        /// <summary>Gerçek Python'un İngilizce mesajıyla birebir aynı metin</summary>
        public readonly string PyMessage;
        /// <summary>
        /// Hatanın olduğu satır (1'den başlar). Yerleşik fonksiyonlar satırı bilmez (null bırakır);
        /// çalıştırıcı, hatanın çıktığı ifadenin satırını sonradan yazar.
        /// </summary>
        public int? Line;

        public PythonException(string pyType, string pyMessage, int? line) : base(pyType + ": " + pyMessage)
        {
            PyType = pyType;
            PyMessage = pyMessage;
            Line = line;
        }
    }

    /// <summary>Gerçek Python'da olan ama bu oyunun motorunun (henüz) desteklemediği özellikler.</summary>
    public static class Feature
    {
        public const string Annotation = "annotation";
        public const string Assert = "assert";
        public const string Async = "async";
        public const string Await = "await";
        public const string Builtin = "builtin";
        public const string Bytes = "bytes";
        public const string Class = "class";
        public const string Complex = "complex";
        public const string Comprehension = "comprehension";
        public const string Decorator = "decorator";
        public const string Del = "del";
        public const string Dict = "dict";
        public const string Ellipsis = "ellipsis";
        public const string FString = "f-string";
        public const string Import = "import";
        public const string Lambda = "lambda";
        public const string Method = "method";
        public const string Raise = "raise";
        public const string Set = "set";
        public const string SliceAssign = "slice-assign";
        public const string StarArgs = "star-args";
        public const string StarParams = "star-params";
        public const string StrFormat = "str-format";
        public const string Try = "try";
        public const string Walrus = "walrus";
        public const string With = "with";
        public const string Yield = "yield";
    }

    /// <summary>"Gerçek Python'da var ama bu oyunda henüz yok" durumu. Python hatası değildir.</summary>
    public class UnsupportedFeature : Exception
    {
        /// <summary>Feature sınıfındaki adlardan biri, örn. "f-string"</summary>
        public readonly string FeatureId;
        public int? Line;
        /// <summary>Örn. feature "builtin" için "input", "method" için "str.format"</summary>
        public readonly string Detail;

        public UnsupportedFeature(string feature, int? line, string detail = null)
            : base("Desteklenmeyen özellik: " + feature + (detail != null ? " (" + detail + ")" : ""))
        {
            FeatureId = feature;
            Line = line;
            Detail = detail;
        }
    }

    /// <summary>Oyunun koruma sınırı aşıldı (bitmeyen döngü, aşırı büyük değer). Python hatası değildir.</summary>
    public class ExecutionLimit : Exception
    {
        /// <summary>"steps" (adım sınırı) ya da "size" (değer çok büyük)</summary>
        public readonly string Reason;
        public int? Line;

        public ExecutionLimit(string reason, int? line)
            : base(reason == "steps" ? "Adım sınırı aşıldı" : "Değer çok büyük")
        {
            Reason = reason;
            Line = line;
        }
    }
}
