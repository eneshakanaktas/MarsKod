// İşlemler: aritmetik, karşılaştırma, indeksleme, yineleme. Hata mesajları CPython 3.12 ile aynı.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static MarsKod.Motor.Values;

namespace MarsKod.Motor
{
    /// <summary>a[alt:üst:adım] içindeki dilim (yığında geçici olarak durur)</summary>
    public sealed class SliceValue
    {
        public object Lower;
        public object Upper;
        public object Step;
    }

    public static class Operators
    {
        /// <summary>Tek seferde üretilebilecek en uzun metin/liste (telefonun belleğini korumak için)</summary>
        public const int MaxSequence = 10000000;
        /// <summary>Bir tam sayının en fazla bit uzunluğu (2 ** 1000000 gibi hesaplar oyunu dondurmasın)</summary>
        public const int MaxIntBits = 1000000;

        static readonly object NoResult = new object();

        // --- sayılar ---

        public static double ToFloat(Num x)
        {
            if (x.IsFloat) return x.F;
            double f = Numbers.ToDouble(x.I);
            if (double.IsInfinity(f) || double.IsNaN(f)) throw PyError("OverflowError", "int too large to convert to float");
            return f;
        }

        static BigInteger CheckIntSize(BigInteger x)
        {
            if (Numbers.BitLength(x) > MaxIntBits) throw new ExecutionLimit("size", null);
            return x;
        }

        static BigInteger IntFloorDiv(BigInteger a, BigInteger b)
        {
            var q = BigInteger.Divide(a, b);
            return !BigInteger.Remainder(a, b).IsZero && (a.Sign < 0) != (b.Sign < 0) ? q - 1 : q;
        }

        static BigInteger IntMod(BigInteger a, BigInteger b)
        {
            var m = BigInteger.Remainder(a, b);
            return !m.IsZero && (m.Sign < 0) != (b.Sign < 0) ? m + b : m;
        }

        static bool IsTruthyDouble(double d) => d != 0 && !double.IsNaN(d);

        /// <summary>CPython float_divmod</summary>
        static void FloatDivMod(double vx, double wx, out double floordiv, out double mod)
        {
            mod = vx % wx;
            double div = (vx - mod) / wx;
            if (IsTruthyDouble(mod))
            {
                if (wx < 0 != mod < 0)
                {
                    mod += wx;
                    div -= 1;
                }
            }
            else
            {
                mod = CopySign(0, wx);
            }
            if (IsTruthyDouble(div))
            {
                floordiv = Math.Floor(div);
                if (div - floordiv > 0.5) floordiv += 1;
            }
            else
            {
                floordiv = CopySign(0, vx / wx);
            }
        }

        static double CopySign(double x, double sign)
        {
            bool negative = sign < 0 || IsNegativeZero(sign);
            return negative ? -Math.Abs(x) : Math.Abs(x);
        }

        static bool IsFinite(double d) => !double.IsNaN(d) && !double.IsInfinity(d);

        static bool IsWhole(double d) => IsFinite(d) && Math.Floor(d) == d;

        static double FloatPow(double a, double b)
        {
            if (a == 0 && b < 0) throw PyError("ZeroDivisionError", "0.0 cannot be raised to a negative power");
            if (a < 0 && IsFinite(b) && !IsWhole(b)) throw new UnsupportedFeature(Feature.Complex, null);
            double r = Math.Pow(a, b);
            if (!IsFinite(r) && IsFinite(a) && IsFinite(b)) throw PyError("OverflowError", "(34, 'Numerical result out of range')");
            return r;
        }

        static object IntPow(BigInteger x, BigInteger y)
        {
            if (y.Sign < 0)
            {
                if (x.IsZero) throw PyError("ZeroDivisionError", "0.0 cannot be raised to a negative power");
                return FloatPow(ToFloat(Num.Int(x)), ToFloat(Num.Int(y)));
            }
            if (x.IsZero) return y.IsZero ? One : Zero;
            if (x.IsOne) return One;
            if (x == BigInteger.MinusOne) return y.IsEven ? One : BigInteger.MinusOne;
            if (Numbers.BitLength(x) * y > MaxIntBits) throw new ExecutionLimit("size", null);
            return BigInteger.Pow(x, (int)y);
        }

        static object NumericOp(string op, Num x, Num y)
        {
            if (!x.IsFloat && !y.IsFloat)
            {
                BigInteger a = x.I, b = y.I;
                switch (op)
                {
                    case "+": return CheckIntSize(a + b);
                    case "-": return CheckIntSize(a - b);
                    case "*": return CheckIntSize(a * b);
                    case "/":
                        if (b.IsZero) throw PyError("ZeroDivisionError", "division by zero");
                        return ToFloat(x) / ToFloat(y);
                    case "//":
                        if (b.IsZero) throw PyError("ZeroDivisionError", "integer division or modulo by zero");
                        return IntFloorDiv(a, b);
                    case "%":
                        if (b.IsZero) throw PyError("ZeroDivisionError", "integer modulo by zero");
                        return IntMod(a, b);
                    case "**":
                        return IntPow(a, b);
                    case "<<":
                        if (b.Sign < 0) throw PyError("ValueError", "negative shift count");
                        if (a.IsZero) return Zero;
                        if (b > MaxIntBits) throw new ExecutionLimit("size", null);
                        return CheckIntSize(a << (int)b);
                    case ">>":
                        if (b.Sign < 0) throw PyError("ValueError", "negative shift count");
                        if (b > int.MaxValue) return a.Sign < 0 ? BigInteger.MinusOne : Zero;
                        return a >> (int)b;
                    case "&": return a & b;
                    case "|": return a | b;
                    case "^": return a ^ b;
                }
                return NoResult;
            }

            double fa = ToFloat(x);
            double fb = ToFloat(y);
            switch (op)
            {
                case "+": return fa + fb;
                case "-": return fa - fb;
                case "*": return fa * fb;
                case "/":
                    if (fb == 0) throw PyError("ZeroDivisionError", "float division by zero");
                    return fa / fb;
                case "//":
                {
                    if (fb == 0) throw PyError("ZeroDivisionError", "float floor division by zero");
                    FloatDivMod(fa, fb, out double div, out _);
                    return div;
                }
                case "%":
                {
                    if (fb == 0) throw PyError("ZeroDivisionError", "float modulo");
                    FloatDivMod(fa, fb, out _, out double mod);
                    return mod;
                }
                case "**":
                    return FloatPow(fa, fb);
            }
            return NoResult;
        }

        static BigInteger? RepeatCount(object n)
        {
            if (n is BigInteger i) return i;
            if (n is bool b) return b ? One : Zero;
            return null;
        }

        static List<object> Repeat(List<object> items, BigInteger count)
        {
            if (count <= 0 || items.Count == 0) return new List<object>();
            if (items.Count * count > MaxSequence) throw new ExecutionLimit("size", null);
            int n = (int)count;
            var result = new List<object>(items.Count * n);
            for (int k = 0; k < n; k++) result.AddRange(items);
            return result;
        }

        static object SequenceRepeat(object seq, BigInteger count)
        {
            switch (seq)
            {
                case string s:
                {
                    if (count <= 0 || s.Length == 0) return "";
                    if (s.Length * count > MaxSequence) throw new ExecutionLimit("size", null);
                    int n = (int)count;
                    var sb = new StringBuilder(s.Length * n);
                    for (int k = 0; k < n; k++) sb.Append(s);
                    return sb.ToString();
                }
                case PyList l: return new PyList(Repeat(l.Items, count));
                case PyTuple t: return new PyTuple(Repeat(t.Items, count));
            }
            throw new InvalidOperationException("dizi bekleniyordu");
        }

        static bool IsSequence(object v) => v is string || v is PyList || v is PyTuple;

        public static object BinaryOp(string op, object a, object b)
        {
            var x = AsNumber(a);
            var y = AsNumber(b);
            if (x != null && y != null)
            {
                var result = NumericOp(op, x.Value, y.Value);
                if (result != NoResult) return result;
            }
            else if (op == "+")
            {
                if (a is string sa)
                {
                    if (b is string sb)
                    {
                        if (sa.Length + sb.Length > MaxSequence) throw new ExecutionLimit("size", null);
                        return sa + sb;
                    }
                    throw PyError("TypeError", "can only concatenate str (not \"" + TypeName(b) + "\") to str");
                }
                if (a is PyList la)
                {
                    if (b is PyList lb) return new PyList(la.Items.Concat(lb.Items).ToList());
                    throw PyError("TypeError", "can only concatenate list (not \"" + TypeName(b) + "\") to list");
                }
                if (a is PyTuple ta)
                {
                    if (b is PyTuple tb) return new PyTuple(ta.Items.Concat(tb.Items).ToList());
                    throw PyError("TypeError", "can only concatenate tuple (not \"" + TypeName(b) + "\") to tuple");
                }
            }
            else if (op == "*")
            {
                if (IsSequence(a))
                {
                    var count = RepeatCount(b);
                    if (count == null) throw PyError("TypeError", "can't multiply sequence by non-int of type '" + TypeName(b) + "'");
                    return SequenceRepeat(a, count.Value);
                }
                if (IsSequence(b))
                {
                    var count = RepeatCount(a);
                    if (count == null) throw PyError("TypeError", "can't multiply sequence by non-int of type '" + TypeName(a) + "'");
                    return SequenceRepeat(b, count.Value);
                }
            }
            else if (op == "%" && a is string)
            {
                throw new UnsupportedFeature(Feature.StrFormat, null);
            }
            throw PyError("TypeError", "unsupported operand type(s) for " + op + ": '" + TypeName(a) + "' and '" + TypeName(b) + "'");
        }

        /// <summary>x += y: listelerde yerinde değiştirir (Python'daki gibi aynı liste nesnesi kalır).</summary>
        public static object InplaceOp(string op, object a, object b)
        {
            if (a is PyList list)
            {
                if (op == "+")
                {
                    var items = Iterate(b).ToList(); // önce hepsi alınır: x += x de doğru çalışsın
                    list.Items.AddRange(items);
                    return list;
                }
                if (op == "*")
                {
                    var count = RepeatCount(b);
                    if (count != null)
                    {
                        list.Items = Repeat(list.Items, count.Value);
                        return list;
                    }
                }
            }
            return BinaryOp(op, a, b);
        }

        public static object UnaryOperation(string op, object v)
        {
            if (op == "not") return !Truthy(v);
            var n = AsNumber(v);
            if (n != null)
            {
                var num = n.Value;
                if (op == "-") return num.IsFloat ? (object)(-num.F) : -num.I;
                if (op == "+") return num.Box();
                if (!num.IsFloat) return -(num.I + 1); // ~x
            }
            throw PyError("TypeError", "bad operand type for unary " + op + ": '" + TypeName(v) + "'");
        }

        // --- karşılaştırma ---

        /// <summary>İki sayıyı tam (matematiksel) karşılaştırır; NaN varsa null.</summary>
        static int? NumCompare(Num x, Num y)
        {
            if (!x.IsFloat && !y.IsFloat) return x.I.CompareTo(y.I);
            if (x.IsFloat && y.IsFloat)
            {
                if (double.IsNaN(x.F) || double.IsNaN(y.F)) return null;
                return x.F.CompareTo(y.F);
            }
            if (x.IsFloat)
            {
                int? r = IntFloatCompare(y.I, x.F);
                return r == null ? null : -r;
            }
            return IntFloatCompare(x.I, y.F);
        }

        static int? IntFloatCompare(BigInteger i, double d)
        {
            if (double.IsNaN(d)) return null;
            if (double.IsPositiveInfinity(d)) return -1;
            if (double.IsNegativeInfinity(d)) return 1;
            double floor = Math.Floor(d);
            int c = i.CompareTo(new BigInteger(floor));
            if (c != 0) return c;
            return d > floor ? -1 : 0;
        }

        public static bool PyEquals(object a, object b)
        {
            var x = AsNumber(a);
            var y = AsNumber(b);
            if (x != null && y != null) return NumCompare(x.Value, y.Value) == 0;
            if (a is string || b is string) return a is string sa && b is string sb && string.Equals(sa, sb, StringComparison.Ordinal);
            if (a == null || b == null) return a == null && b == null;
            if (!(a is PyObject) || !(b is PyObject)) return false;
            if (ReferenceEquals(a, b)) return true;
            if (a is PyList la && b is PyList lb) return ItemsEqual(la.Items, lb.Items);
            if (a is PyTuple ta && b is PyTuple tb) return ItemsEqual(ta.Items, tb.Items);
            if (a is PyRange ra && b is PyRange rb)
            {
                var len = RangeLength(ra);
                if (len != RangeLength(rb)) return false;
                if (len.IsZero) return true;
                if (ra.Start != rb.Start) return false;
                return len.IsOne || ra.Step == rb.Step;
            }
            return false;
        }

        static bool ItemsEqual(List<object> a, List<object> b)
        {
            if (a.Count != b.Count) return false;
            for (int k = 0; k < a.Count; k++)
            {
                if (!PyEquals(a[k], b[k])) return false;
            }
            return true;
        }

        static bool Ordered(string op, int? cmp)
        {
            if (cmp == null) return false;
            switch (op)
            {
                case "<": return cmp < 0;
                case ">": return cmp > 0;
                case "<=": return cmp <= 0;
                case ">=": return cmp >= 0;
            }
            throw new InvalidOperationException(op);
        }

        /// <summary>Sıralama karşılaştırması; listeler/demetler ilk farklı öğeye göre karşılaştırılır.</summary>
        public static bool OrderCompare(string op, object a, object b)
        {
            var x = AsNumber(a);
            var y = AsNumber(b);
            if (x != null && y != null) return Ordered(op, NumCompare(x.Value, y.Value));
            if (a is string sa && b is string sb) return Ordered(op, string.CompareOrdinal(sa, sb));
            List<object> ia = null, ib = null;
            if (a is PyList la && b is PyList lb)
            {
                ia = la.Items;
                ib = lb.Items;
            }
            else if (a is PyTuple ta && b is PyTuple tb)
            {
                ia = ta.Items;
                ib = tb.Items;
            }
            if (ia != null)
            {
                int n = Math.Min(ia.Count, ib.Count);
                for (int k = 0; k < n; k++)
                {
                    if (!PyEquals(ia[k], ib[k])) return OrderCompare(op, ia[k], ib[k]);
                }
                return Ordered(op, ia.Count.CompareTo(ib.Count));
            }
            throw PyError("TypeError", "'" + op + "' not supported between instances of '" + TypeName(a) + "' and '" + TypeName(b) + "'");
        }

        /// <summary>a &lt; b; sıralama (sorted, min, max) için</summary>
        public static bool PyLess(object a, object b) => OrderCompare("<", a, b);

        /// <summary>Python'daki `is`: nesnelerde aynılık, basit değerlerde eşitlik (aynı tür şartıyla)</summary>
        public static bool PyIs(object a, object b)
        {
            if (a == null || b == null) return a == null && b == null;
            if (a is PyObject || b is PyObject) return ReferenceEquals(a, b);
            if (a is double da && b is double db) return da == db;
            return a.GetType() == b.GetType() && a.Equals(b);
        }

        public static bool CompareOp(string op, object a, object b)
        {
            switch (op)
            {
                case "==": return PyEquals(a, b);
                case "!=": return !PyEquals(a, b);
                case "<":
                case ">":
                case "<=":
                case ">=":
                    return OrderCompare(op, a, b);
                case "in": return Contains(b, a);
                case "not in": return !Contains(b, a);
                case "is": return PyIs(a, b);
                case "is not": return !PyIs(a, b);
            }
            throw new InvalidOperationException(op);
        }

        public static bool Contains(object container, object item)
        {
            if (container is string s)
            {
                if (!(item is string sub)) throw PyError("TypeError", "'in <string>' requires string as left operand, not " + TypeName(item));
                return s.IndexOf(sub, StringComparison.Ordinal) >= 0;
            }
            if (container is PyList l) return l.Items.Any(x => PyEquals(x, item));
            if (container is PyTuple t) return t.Items.Any(x => PyEquals(x, item));
            if (container is PyRange r)
            {
                var n = AsNumber(item);
                if (n == null) return false;
                if (!n.Value.IsFloat) return RangeIndex(r, n.Value.I) != null;
                return IsWhole(n.Value.F) && RangeIndex(r, new BigInteger(n.Value.F)) != null;
            }
            throw PyError("TypeError", "argument of type '" + TypeName(container) + "' is not iterable");
        }

        static BigInteger? RangeIndex(PyRange r, BigInteger n)
        {
            var len = RangeLength(r);
            if (len.IsZero) return null;
            var offset = n - r.Start;
            if (!BigInteger.Remainder(offset, r.Step).IsZero) return null;
            var i = BigInteger.Divide(offset, r.Step);
            return i >= 0 && i < len ? (BigInteger?)i : null;
        }

        // --- yineleme ---

        public static bool IsIterable(object v) => v is string || v is PyList || v is PyTuple || v is PyRange;

        /// <summary>Değerin öğelerini verir; yinelenemiyorsa hemen TypeError.</summary>
        public static IEnumerable<object> Iterate(object v)
        {
            if (!IsIterable(v)) throw PyError("TypeError", "'" + TypeName(v) + "' object is not iterable");
            return IterateItems(v);
        }

        static IEnumerable<object> IterateItems(object v)
        {
            switch (v)
            {
                case string s:
                    foreach (var ch in StrChars(s)) yield return ch;
                    break;
                case PyList l:
                    // Python listeyi sıra numarasıyla gezer: döngüde liste değişirse bu görülür.
                    for (int k = 0; k < l.Items.Count; k++) yield return l.Items[k];
                    break;
                case PyTuple t:
                    for (int k = 0; k < t.Items.Count; k++) yield return t.Items[k];
                    break;
                case PyRange r:
                    if (r.Step > 0)
                        for (var k = r.Start; k < r.Stop; k += r.Step) yield return k;
                    else
                        for (var k = r.Start; k > r.Stop; k += r.Step) yield return k;
                    break;
            }
        }

        // --- indeksleme ---

        static BigInteger IndexOf(object index, string container)
        {
            if (index is BigInteger i) return i;
            if (index is bool b) return b ? One : Zero;
            if (container == "str") throw PyError("TypeError", "string indices must be integers, not '" + TypeName(index) + "'");
            throw PyError("TypeError", container + " indices must be integers or slices, not " + TypeName(index));
        }

        const long ClampLimit = 1L << 60;

        static long ClampToLong(BigInteger n)
        {
            if (n > ClampLimit) return ClampLimit;
            if (n < -ClampLimit) return -ClampLimit;
            return (long)n;
        }

        static void SliceIndices(SliceValue slice, int length, out long start, out long stop, out long step)
        {
            long ToInt(object v, long fallback)
            {
                if (v == null) return fallback;
                var n = AsNumber(v);
                if (n == null || n.Value.IsFloat)
                    throw PyError("TypeError", "slice indices must be integers or None or have an __index__ method");
                return ClampToLong(n.Value.I);
            }
            long Clamp(long k, long lo, long hi)
            {
                if (k < 0) k += length;
                return Math.Min(Math.Max(k, lo), hi);
            }
            step = ToInt(slice.Step, 1);
            if (step == 0) throw PyError("ValueError", "slice step cannot be zero");
            if (step > 0)
            {
                start = Clamp(ToInt(slice.Lower, 0), 0, length);
                stop = Clamp(ToInt(slice.Upper, length), 0, length);
                return;
            }
            start = Clamp(ToInt(slice.Lower, length - 1), -1, length - 1);
            stop = Clamp(ToInt(slice.Upper, -length - 1), -1, length - 1);
        }

        static List<T> ApplySlice<T>(List<T> items, SliceValue slice)
        {
            SliceIndices(slice, items.Count, out long start, out long stop, out long step);
            var result = new List<T>();
            if (step > 0) for (long k = start; k < stop; k += step) result.Add(items[(int)k]);
            else for (long k = start; k > stop; k += step) result.Add(items[(int)k]);
            return result;
        }

        public static object GetItem(object container, object index)
        {
            var slice = index as SliceValue;
            switch (container)
            {
                case string s:
                {
                    var chars = StrChars(s);
                    if (slice != null) return string.Concat(ApplySlice(chars, slice));
                    int k = NormalizeIndex(IndexOf(index, "str"), chars.Count, "string index out of range");
                    return chars[k];
                }
                case PyList l:
                {
                    if (slice != null) return new PyList(ApplySlice(l.Items, slice));
                    return l.Items[NormalizeIndex(IndexOf(index, "list"), l.Items.Count, "list index out of range")];
                }
                case PyTuple t:
                {
                    if (slice != null) return new PyTuple(ApplySlice(t.Items, slice));
                    return t.Items[NormalizeIndex(IndexOf(index, "tuple"), t.Items.Count, "tuple index out of range")];
                }
                case PyRange r:
                {
                    if (slice != null) throw new UnsupportedFeature(Feature.Method, null, "range slice");
                    var len = RangeLength(r);
                    var k = IndexOf(index, "range");
                    if (k < 0) k += len;
                    if (k < 0 || k >= len) throw PyError("IndexError", "range object index out of range");
                    return r.Start + k * r.Step;
                }
            }
            throw PyError("TypeError", "'" + TypeName(container) + "' object is not subscriptable");
        }

        static int NormalizeIndex(BigInteger k, int length, string message)
        {
            var n = k < 0 ? k + length : k;
            if (n < 0 || n >= length) throw PyError("IndexError", message);
            return (int)n;
        }

        public static void SetItem(object container, object index, object value)
        {
            if (container is PyList l)
            {
                if (index is SliceValue) throw new UnsupportedFeature(Feature.SliceAssign, null);
                int k = NormalizeIndex(IndexOf(index, "list"), l.Items.Count, "list assignment index out of range");
                l.Items[k] = value;
                return;
            }
            throw PyError("TypeError", "'" + TypeName(container) + "' object does not support item assignment");
        }

        public static BigInteger Length(object v)
        {
            switch (v)
            {
                case string s: return StrLength(s);
                case PyList l: return l.Items.Count;
                case PyTuple t: return t.Items.Count;
                case PyRange r: return RangeLength(r);
            }
            throw PyError("TypeError", "object of type '" + TypeName(v) + "' has no len()");
        }
    }
}
