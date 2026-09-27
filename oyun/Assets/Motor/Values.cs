// Python değerlerinin motordaki karşılıkları ve ekrana yazılış biçimleri (repr/str).
//
// Değerler "object" olarak tutulur. Basit türler C#'ın kendi türleriyle:
//   int → BigInteger (Python tam sayıları sınırsızdır), float → double, str → string,
//   bool → bool, None → null
// Diğerleri aşağıdaki Py... sınıflarıdır.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace MarsKod.Motor
{
    /// <summary>Yerleşik fonksiyonların ve oyunun dışarıdan verdiği komutların biçimi</summary>
    public delegate object NativeFn(List<object> args, Dictionary<string, object> kwargs);

    public abstract class PyObject
    {
    }

    public sealed class PyList : PyObject
    {
        public List<object> Items;

        public PyList(List<object> items)
        {
            Items = items;
        }
    }

    public sealed class PyTuple : PyObject
    {
        public List<object> Items;

        public PyTuple(List<object> items)
        {
            Items = items;
        }
    }

    public sealed class PyRange : PyObject
    {
        public BigInteger Start;
        public BigInteger Stop;
        public BigInteger Step;

        public PyRange(BigInteger start, BigInteger stop, BigInteger step)
        {
            Start = start;
            Stop = stop;
            Step = step;
        }
    }

    /// <summary>Oyuncunun def ile yazdığı fonksiyon</summary>
    public sealed class PyFunction : PyObject
    {
        public FunctionDef Def;
        /// <summary>Hata mesajlarındaki ad, örn. "dis.&lt;locals&gt;.ic"</summary>
        public string Qualname;
        /// <summary>Varsayılan değerler (tanım anında hesaplanır), parametre adıyla</summary>
        public Dictionary<string, object> Defaults;
        /// <summary>Tanımlandığı fonksiyonun çerçevesi (iç içe fonksiyonlar için); modül düzeyindeyse null</summary>
        public Frame Closure;
        /// <summary>Fonksiyonun yerel değişkenleri (parametreler + atananlar, CPython co_varnames sırasıyla)</summary>
        public List<string> LocalNames;
        public HashSet<string> GlobalNames;
        public HashSet<string> NonlocalNames;
    }

    /// <summary>Yerleşik fonksiyon (print, len...) ve oyunun dışarıdan verdiği komutlar</summary>
    public sealed class PyBuiltin : PyObject
    {
        public string Name;
        public NativeFn Call;

        public PyBuiltin(string name, NativeFn call)
        {
            Name = name;
            Call = call;
        }
    }

    /// <summary>int, str, list gibi türler: hem çağrılabilir hem type(x) sonucudur</summary>
    public sealed class PyType : PyObject
    {
        public string Name;
        /// <summary>Çağrılamayan türlerde (NoneType...) null</summary>
        public NativeFn Call;
    }

    /// <summary>Bir değere bağlı yerleşik metot, örn. liste.append</summary>
    public sealed class PyMethod : PyObject
    {
        public object Self;
        public string Name;
        public NativeFn Call;
    }

    /// <summary>Sayı: tam sayı ya da ondalıklı (bool da tam sayı sayılır)</summary>
    public struct Num
    {
        public bool IsFloat;
        public BigInteger I;
        public double F;

        public static Num Int(BigInteger i) => new Num { I = i };
        public static Num Float(double f) => new Num { IsFloat = true, F = f };

        public object Box() => IsFloat ? (object)F : I;
    }

    public static class Values
    {
        public static readonly BigInteger Zero = BigInteger.Zero;
        public static readonly BigInteger One = BigInteger.One;

        public static PyList PyList(List<object> items) => new PyList(items);
        public static PyTuple PyTuple(List<object> items) => new PyTuple(items);

        public static bool IsInt(object v) => v is BigInteger;

        /// <summary>Python'da bool da bir tam sayıdır (True == 1). Sayı değilse null.</summary>
        public static Num? AsNumber(object v)
        {
            switch (v)
            {
                case BigInteger i: return Num.Int(i);
                case double f: return Num.Float(f);
                case bool b: return Num.Int(b ? One : Zero);
                default: return null;
            }
        }

        public static string TypeName(object v)
        {
            switch (v)
            {
                case null: return "NoneType";
                case BigInteger _: return "int";
                case double _: return "float";
                case string _: return "str";
                case bool _: return "bool";
                case PyList _: return "list";
                case PyTuple _: return "tuple";
                case PyRange _: return "range";
                case PyFunction _: return "function";
                case PyBuiltin _:
                case PyMethod _:
                    return "builtin_function_or_method";
                case PyType _: return "type";
            }
            throw new InvalidOperationException("Bilinmeyen değer: " + v.GetType());
        }

        public static PythonException PyError(string type, string message) => new PythonException(type, message, null);

        public static bool Truthy(object v)
        {
            switch (v)
            {
                case null: return false;
                case BigInteger i: return !i.IsZero;
                case double f: return f != 0;
                case string s: return s.Length > 0;
                case bool b: return b;
                case PyList l: return l.Items.Count > 0;
                case PyTuple t: return t.Items.Count > 0;
                case PyRange r: return RangeLength(r) > 0;
                default: return true;
            }
        }

        public static BigInteger RangeLength(PyRange r)
        {
            if (r.Step > 0) return r.Stop > r.Start ? (r.Stop - r.Start + r.Step - 1) / r.Step : Zero;
            return r.Start > r.Stop ? (r.Start - r.Stop - r.Step - 1) / -r.Step : Zero;
        }

        /// <summary>Tam sayının yazılışı (kültürden bağımsız: eksi işareti hep '-')</summary>
        public static string IntStr(BigInteger i) => i.ToString(CultureInfo.InvariantCulture);

        // --- metinler: Python kod noktası (code point) sayar, C# UTF-16 birimi ---

        /// <summary>Metni kod noktalarına böler (vekil çiftler tek karakter sayılır).</summary>
        public static List<string> StrChars(string s)
        {
            var chars = new List<string>(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsHighSurrogate(s[i]) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
                {
                    chars.Add(s.Substring(i, 2));
                    i++;
                }
                else chars.Add(s[i].ToString());
            }
            return chars;
        }

        public static int StrLength(string s)
        {
            int n = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsHighSurrogate(s[i]) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1])) i++;
                n++;
            }
            return n;
        }

        /// <summary>s[i]'deki kod noktası (vekil çiftse ikisi birden)</summary>
        public static int CodePointAt(string s, int i)
        {
            if (char.IsHighSurrogate(s[i]) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
                return char.ConvertToUtf32(s[i], s[i + 1]);
            return s[i];
        }

        public static string FromCodePoint(int cp)
        {
            if (cp >= 0xD800 && cp <= 0xDFFF) return ((char)cp).ToString(); // tek vekil (Python'da geçerli)
            return char.ConvertFromUtf32(cp);
        }

        public static UnicodeCategory Category(string s, int i) => CharUnicodeInfo.GetUnicodeCategory(s, i);

        /// <summary>Unicode C (denetim, biçim, atanmamış...) ya da Z (boşluk, ayraç) sınıfında mı</summary>
        public static bool IsNonPrintable(string s, int i)
        {
            switch (Category(s, i))
            {
                case UnicodeCategory.Control:
                case UnicodeCategory.Format:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.OtherNotAssigned:
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.ParagraphSeparator:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Python'un str.isspace() tanımıyla boşluk karakteri</summary>
        public static bool IsPySpace(char c)
        {
            if (c == ' ' || (c >= '\t' && c <= '\r') || (c >= '\x1c' && c <= '\x1f') || c == '\x85') return true;
            if (c < 0x80) return false;
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            return cat == UnicodeCategory.SpaceSeparator || c == '\u2028' || c == '\u2029';
        }

        // --- ekrana yazılış ---

        /// <summary>CPython'un float repr'i: en kısa geri dönüşümlü basamaklar, 1e16 ve üstü / 1e-5 ve altı üslü.</summary>
        public static string FloatRepr(double x)
        {
            if (double.IsNaN(x)) return "nan";
            if (double.IsInfinity(x)) return x > 0 ? "inf" : "-inf";
            if (x == 0) return IsNegativeZero(x) ? "-0.0" : "0.0";

            ShortestDigits(Math.Abs(x), out string digits, out int decpt);
            string sign = x < 0 ? "-" : "";

            if (decpt > -4 && decpt <= 16)
            {
                if (decpt <= 0) return sign + "0." + new string('0', -decpt) + digits;
                if (decpt >= digits.Length) return sign + digits + new string('0', decpt - digits.Length) + ".0";
                return sign + digits.Substring(0, decpt) + "." + digits.Substring(decpt);
            }
            string m = digits.Length > 1 ? digits[0] + "." + digits.Substring(1) : digits;
            int e = decpt - 1;
            return sign + m + "e" + (e < 0 ? "-" : "+") + Math.Abs(e).ToString("00", CultureInfo.InvariantCulture);
        }

        public static bool IsNegativeZero(double x) => x == 0 && BitConverter.DoubleToInt64Bits(x) < 0;

        /// <summary>
        /// Pozitif bir double'ı geri dönüşümlü en kısa basamaklarla yazar: değer = 0.digits × 10^decpt.
        /// (Her çalışma ortamında aynı sonucu vermesi için hazır "R" biçimine güvenilmez.)
        /// </summary>
        static void ShortestDigits(double x, out string digits, out int decpt)
        {
            string text = null;
            for (int p = 1; p <= 17; p++)
            {
                text = x.ToString("E" + (p - 1), CultureInfo.InvariantCulture);
                if (double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture) == x) break;
            }
            int e = text.IndexOf('E');
            digits = text.Substring(0, e).Replace(".", "").TrimEnd('0');
            if (digits.Length == 0) digits = "0";
            decpt = int.Parse(text.Substring(e + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) + 1;
        }

        public static string StrRepr(string s)
        {
            char quote = s.IndexOf('\'') >= 0 && s.IndexOf('"') < 0 ? '"' : '\'';
            var sb = new StringBuilder();
            sb.Append(quote);
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == quote || ch == '\\') sb.Append('\\').Append(ch);
                else if (ch == '\n') sb.Append("\\n");
                else if (ch == '\r') sb.Append("\\r");
                else if (ch == '\t') sb.Append("\\t");
                else if (ch != ' ' && IsNonPrintable(s, i))
                {
                    int cp = CodePointAt(s, i);
                    if (cp < 0x100) sb.Append("\\x").Append(cp.ToString("x2"));
                    else if (cp < 0x10000) sb.Append("\\u").Append(cp.ToString("x4"));
                    else sb.Append("\\U").Append(cp.ToString("x8"));
                    if (cp >= 0x10000) i++;
                }
                else
                {
                    sb.Append(ch);
                    if (char.IsHighSurrogate(ch) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1])) sb.Append(s[++i]);
                }
            }
            sb.Append(quote);
            return sb.ToString();
        }

        public static string Repr(object v) => Repr(v, new HashSet<PyObject>());

        static string Repr(object v, HashSet<PyObject> seen)
        {
            switch (v)
            {
                case null: return "None";
                case BigInteger i: return IntStr(i);
                case double f: return FloatRepr(f);
                case string s: return StrRepr(s);
                case bool b: return b ? "True" : "False";
                case PyList l: return SequenceRepr(l, l.Items, "[", "]", "[...]", seen);
                case PyTuple t:
                    if (t.Items.Count == 1 && !seen.Contains(t)) return SequenceRepr(t, t.Items, "(", ",)", "(...)", seen);
                    return SequenceRepr(t, t.Items, "(", ")", "(...)", seen);
                case PyRange r:
                    return r.Step == 1
                        ? "range(" + IntStr(r.Start) + ", " + IntStr(r.Stop) + ")"
                        : "range(" + IntStr(r.Start) + ", " + IntStr(r.Stop) + ", " + IntStr(r.Step) + ")";
                case PyFunction fn: return "<function " + fn.Qualname + ">";
                case PyBuiltin bi: return "<built-in function " + bi.Name + ">";
                case PyMethod m: return "<built-in method " + m.Name + " of " + TypeName(m.Self) + " object>";
                case PyType ty: return "<class '" + ty.Name + "'>";
            }
            throw new InvalidOperationException("Bilinmeyen değer: " + v.GetType());
        }

        static string SequenceRepr(PyObject obj, List<object> items, string open, string close, string recursive, HashSet<PyObject> seen)
        {
            if (seen.Contains(obj)) return recursive;
            seen.Add(obj);
            var parts = new string[items.Count];
            for (int i = 0; i < items.Count; i++) parts[i] = Repr(items[i], seen);
            seen.Remove(obj);
            return open + string.Join(", ", parts) + close;
        }

        public static string Str(object v) => v is string s ? s : Repr(v);
    }

    /// <summary>Sayı okuma ve tam sayı yardımcıları</summary>
    public static class Numbers
    {
        /// <summary>Ondalıklı sayı metnini okur; çok büyükse sonsuz (her çalışma ortamında aynı).</summary>
        public static double ParseDouble(string text)
        {
            try
            {
                return double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                return text.TrimStart().StartsWith("-", StringComparison.Ordinal) ? double.NegativeInfinity : double.PositiveInfinity;
            }
        }

        /// <summary>Rakam değeri (0-9, a-z); rakam değilse -1</summary>
        public static int DigitValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'z') return c - 'a' + 10;
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            return -1;
        }

        /// <summary>Verilen tabanda (2-36) rakamlardan tam sayı; geçersiz rakam varsa null</summary>
        public static BigInteger? ParseRadix(string digits, int radix)
        {
            if (digits.Length == 0) return null;
            if (radix == 10) return BigInteger.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);
            BigInteger value = BigInteger.Zero;
            foreach (char c in digits)
            {
                int d = DigitValue(c);
                if (d < 0 || d >= radix) return null;
                value = value * radix + d;
            }
            return value;
        }

        /// <summary>Tam sayının bit uzunluğu (işaret hariç), Python'daki int.bit_length()</summary>
        public static long BitLength(BigInteger x)
        {
            if (x.Sign < 0) x = -x;
            if (x.IsZero) return 0;
            byte[] bytes = x.ToByteArray(); // küçükten büyüğe, sonda işaret için 0 olabilir
            int last = bytes.Length - 1;
            while (last > 0 && bytes[last] == 0) last--;
            int top = bytes[last];
            int bits = 0;
            while (top > 0)
            {
                bits++;
                top >>= 1;
            }
            return (long)last * 8 + bits;
        }

        /// <summary>
        /// Tam sayıyı en yakın double'a çevirir (eşitlikte çift olana; Python gibi). C#'ın hazır
        /// dönüşümü büyük sayılarda yuvarlamak yerine keser: float(10**30) 9.999999999999999e+29 çıkar.
        /// Sığmıyorsa sonsuz verir.
        /// </summary>
        public static double ToDouble(BigInteger x)
        {
            bool negative = x.Sign < 0;
            if (negative) x = -x;
            long n = BitLength(x);
            double result;
            if (n <= 53)
            {
                result = (long)x;
            }
            else if (n > 1025)
            {
                result = double.PositiveInfinity;
            }
            else
            {
                int shift = (int)(n - 53);
                BigInteger top = x >> shift;
                BigInteger rest = x - (top << shift);
                BigInteger half = BigInteger.One << (shift - 1);
                if (rest > half || (rest == half && !top.IsEven)) top += 1;
                result = (long)top * Math.Pow(2, shift); // 2'nin kuvvetiyle çarpma tamdır
            }
            return negative ? -result : result;
        }
    }
}
