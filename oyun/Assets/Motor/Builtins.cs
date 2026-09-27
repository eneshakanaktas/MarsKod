// Yerleşik fonksiyonlar (print, len, range...), türler (int, str...) ve metotlar (liste.append...).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using static MarsKod.Motor.Operators;
using static MarsKod.Motor.Values;

namespace MarsKod.Motor
{
    public interface IBuiltinContext
    {
        void Write(string text);
        /// <summary>Uzun süren işlemlerde adım sayar (bitmeyen hesaplara karşı)</summary>
        void Tick();
    }

    public static class Builtins
    {
        const int MaxList = 10000000;

        // --- yardımcılar ---

        static void NoKwargs(string name, Dictionary<string, object> kwargs)
        {
            if (kwargs.Count > 0) throw PyError("TypeError", name + "() takes no keyword arguments");
        }

        static void ArgCount(string name, List<object> args, int min, int max)
        {
            int n = args.Count;
            if (n >= min && n <= max) return;
            if (min == max)
            {
                if (min == 0) throw PyError("TypeError", name + "() takes no arguments (" + n + " given)");
                if (min == 1) throw PyError("TypeError", name + "() takes exactly one argument (" + n + " given)");
                throw PyError("TypeError", name + " expected " + min + " arguments, got " + n);
            }
            if (n < min) throw PyError("TypeError", name + " expected at least " + min + " argument" + (min == 1 ? "" : "s") + ", got " + n);
            throw PyError("TypeError", name + " expected at most " + max + " argument" + (max == 1 ? "" : "s") + ", got " + n);
        }

        static BigInteger ToIndex(object v)
        {
            if (v is BigInteger i) return i;
            if (v is bool b) return b ? One : Zero;
            throw PyError("TypeError", "'" + TypeName(v) + "' object cannot be interpreted as an integer");
        }

        static int ToSmallInt(object v)
        {
            var i = ToIndex(v);
            if (i > int.MaxValue) return int.MaxValue;
            if (i < int.MinValue) return int.MinValue;
            return (int)i;
        }

        static List<object> Collect(object v, IBuiltinContext ctx)
        {
            if (v is PyRange r && RangeLength(r) > MaxList) throw new ExecutionLimit("size", null);
            var result = new List<object>();
            foreach (var item in Iterate(v))
            {
                ctx.Tick();
                result.Add(item);
            }
            return result;
        }

        static object ArgOrNull(List<object> args, int i) => i < args.Count ? args[i] : null;

        // --- boşluklar (Python'un str.isspace tanımı) ---

        static string TrimPySpace(string s) => TrimPySpaceEnd(TrimPySpaceStart(s));

        static string TrimPySpaceStart(string s)
        {
            int k = 0;
            while (k < s.Length && IsPySpace(s[k])) k++;
            return s.Substring(k);
        }

        static string TrimPySpaceEnd(string s)
        {
            int k = s.Length;
            while (k > 0 && IsPySpace(s[k - 1])) k--;
            return s.Substring(0, k);
        }

        // --- sayı dönüşümleri ---

        static readonly Regex IntDigits = new Regex("^[0-9a-zA-Z](_?[0-9a-zA-Z])*$", RegexOptions.CultureInvariant);

        static BigInteger? ParseIntText(string text, int radix)
        {
            string s = TrimPySpace(text);
            bool negative = false;
            if (s.Length > 0 && (s[0] == '+' || s[0] == '-'))
            {
                negative = s[0] == '-';
                s = s.Substring(1);
            }
            char prefix = radix == 16 ? 'x' : radix == 8 ? 'o' : radix == 2 ? 'b' : '\0';
            if (prefix != '\0' && s.Length >= 2 && s[0] == '0' && char.ToLowerInvariant(s[1]) == prefix)
            {
                s = s.Substring(2);
                if (s.StartsWith("_", StringComparison.Ordinal)) s = s.Substring(1);
            }
            if (!IntDigits.IsMatch(s)) return null;
            BigInteger value = Zero;
            foreach (char ch in s.Replace("_", ""))
            {
                int digit = Numbers.DigitValue(ch);
                if (digit < 0 || digit >= radix) return null;
                value = value * radix + digit;
            }
            return negative ? -value : value;
        }

        static BigInteger IntFromFloat(double x)
        {
            if (double.IsNaN(x)) throw PyError("ValueError", "cannot convert float NaN to integer");
            if (double.IsInfinity(x)) throw PyError("OverflowError", "cannot convert float infinity to integer");
            return new BigInteger(Math.Truncate(x));
        }

        static readonly Regex FloatSpecial = new Regex("^([+-]?)(inf|infinity|nan)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        const string FD = "[0-9](?:_?[0-9])*";
        static readonly Regex FloatText = new Regex("^[+-]?(?:" + FD + @"(?:\.(?:" + FD + @")?)?|\." + FD + ")(?:[eE][+-]?" + FD + ")?$", RegexOptions.CultureInvariant);

        static double? ParseFloatText(string text)
        {
            string s = TrimPySpace(text);
            var special = FloatSpecial.Match(s);
            if (special.Success)
            {
                double v = special.Groups[2].Value.ToLowerInvariant() == "nan" ? double.NaN : double.PositiveInfinity;
                return special.Groups[1].Value == "-" ? -v : v;
            }
            if (!FloatText.IsMatch(s)) return null;
            return Numbers.ParseDouble(s.Replace("_", ""));
        }

        /// <summary>Bir double'ın tam ondalık değeri: |x| = digits × 10^exp</summary>
        static void ExactDecimal(double x, out BigInteger digits, out int exp)
        {
            long bits = BitConverter.DoubleToInt64Bits(Math.Abs(x));
            int biased = (int)((bits >> 52) & 0x7ff);
            BigInteger mantissa = bits & 0xfffffffffffffL;
            int e2;
            if (biased == 0) e2 = -1074;
            else
            {
                mantissa |= BigInteger.One << 52;
                e2 = biased - 1075;
            }
            if (e2 >= 0)
            {
                digits = mantissa << e2;
                exp = 0;
                return;
            }
            digits = mantissa * BigInteger.Pow(5, -e2);
            exp = e2;
        }

        /// <summary>Yarımda çifte yuvarlayan tam sayı bölmesi</summary>
        static BigInteger DivRoundHalfEven(BigInteger n, BigInteger d)
        {
            var q = BigInteger.Divide(n, d);
            var r2 = BigInteger.Remainder(n, d) * 2;
            if (r2 > d || (r2 == d && !q.IsEven)) q += 1;
            return q;
        }

        static object RoundFloat(double x, BigInteger? ndigits)
        {
            if (ndigits == null)
            {
                if (double.IsNaN(x)) throw PyError("ValueError", "cannot convert float NaN to integer");
                if (double.IsInfinity(x)) throw PyError("OverflowError", "cannot convert float infinity to integer");
                double f = Math.Floor(x);
                double d = x - f;
                return new BigInteger(d > 0.5 || (d == 0.5 && f % 2 != 0) ? f + 1 : f);
            }
            if (double.IsNaN(x) || double.IsInfinity(x) || x == 0) return x;
            if (ndigits > 400) return x;
            if (ndigits < -400) return x < 0 ? -0.0 : 0.0;
            int n = (int)ndigits.Value;
            ExactDecimal(x, out BigInteger digits, out int exp);
            int shift = exp + n;
            if (shift >= 0) return x;
            var q = DivRoundHalfEven(digits, BigInteger.Pow(10, -shift));
            double value = Numbers.ParseDouble(IntStr(q) + "e" + (-n).ToString(CultureInfo.InvariantCulture));
            return x < 0 ? -value : value;
        }

        static BigInteger RoundInt(BigInteger x, BigInteger? ndigits)
        {
            if (ndigits == null || ndigits >= 0) return x;
            if (ndigits < -MaxIntBits) return Zero;
            var pow = BigInteger.Pow(10, (int)(-ndigits.Value));
            bool negative = x.Sign < 0;
            var q = DivRoundHalfEven(negative ? -x : x, pow) * pow;
            return negative ? -q : q;
        }

        // --- türler ---

        static Dictionary<string, PyType> MakeTypes(IBuiltinContext ctx)
        {
            var types = new Dictionary<string, PyType>();
            void Def(string name, NativeFn call = null) => types[name] = new PyType { Name = name, Call = call };

            Def("int", (args, kwargs) =>
            {
                bool hasBase = kwargs.ContainsKey("base") || args.Count > 1;
                object b = kwargs.TryGetValue("base", out object kb) ? kb : ArgOrNull(args, 1);
                if (args.Count > 2) throw PyError("TypeError", "int() takes at most 2 arguments (" + args.Count + " given)");
                if (args.Count == 0) return Zero;
                object x = args[0];
                if (hasBase)
                {
                    if (!(x is string xs)) throw PyError("TypeError", "int() can't convert non-string with explicit base");
                    int radix = ToSmallInt(b);
                    var v = ParseIntText(xs, radix);
                    if (v == null) throw PyError("ValueError", "invalid literal for int() with base " + radix + ": " + Repr(x));
                    return v.Value;
                }
                switch (x)
                {
                    case BigInteger i: return i;
                    case bool bo: return bo ? One : Zero;
                    case double f: return IntFromFloat(f);
                    case string s:
                    {
                        var v = ParseIntText(s, 10);
                        if (v == null) throw PyError("ValueError", "invalid literal for int() with base 10: " + Repr(x));
                        return v.Value;
                    }
                }
                throw PyError("TypeError", "int() argument must be a string, a bytes-like object or a real number, not '" + TypeName(x) + "'");
            });

            Def("float", (args, kwargs) =>
            {
                NoKwargs("float", kwargs);
                ArgCount("float", args, 0, 1);
                if (args.Count == 0) return 0.0;
                object x = args[0];
                var n = AsNumber(x);
                if (n != null) return ToFloat(n.Value);
                if (x is string s)
                {
                    var v = ParseFloatText(s);
                    if (v == null) throw PyError("ValueError", "could not convert string to float: " + Repr(x));
                    return v.Value;
                }
                throw PyError("TypeError", "float() argument must be a string or a real number, not '" + TypeName(x) + "'");
            });

            Def("str", (args, kwargs) =>
            {
                NoKwargs("str", kwargs);
                ArgCount("str", args, 0, 1);
                return args.Count == 0 ? "" : Str(args[0]);
            });

            Def("bool", (args, kwargs) =>
            {
                NoKwargs("bool", kwargs);
                ArgCount("bool", args, 0, 1);
                return args.Count != 0 && Truthy(args[0]);
            });

            Def("list", (args, kwargs) =>
            {
                NoKwargs("list", kwargs);
                ArgCount("list", args, 0, 1);
                return new PyList(args.Count == 0 ? new List<object>() : Collect(args[0], ctx));
            });

            Def("tuple", (args, kwargs) =>
            {
                NoKwargs("tuple", kwargs);
                ArgCount("tuple", args, 0, 1);
                return new PyTuple(args.Count == 0 ? new List<object>() : Collect(args[0], ctx));
            });

            Def("range", (args, kwargs) =>
            {
                NoKwargs("range", kwargs);
                if (args.Count == 0) throw PyError("TypeError", "range expected at least 1 argument, got 0");
                if (args.Count > 3) throw PyError("TypeError", "range expected at most 3 arguments, got " + args.Count);
                var n = args.Select(ToIndex).ToList();
                if (args.Count == 1) return new PyRange(Zero, n[0], One);
                if (args.Count == 3 && n[2].IsZero) throw PyError("ValueError", "range() arg 3 must not be zero");
                return new PyRange(n[0], n[1], args.Count == 3 ? n[2] : One);
            });

            Def("type", (args, kwargs) =>
            {
                if (args.Count != 1) throw new UnsupportedFeature(Feature.Class, null);
                return types[TypeName(args[0])];
            });

            Def("NoneType");
            Def("function");
            Def("builtin_function_or_method");
            return types;
        }

        // --- yerleşik fonksiyonlar ---

        public static Dictionary<string, object> Create(IBuiltinContext ctx)
        {
            var types = MakeTypes(ctx);
            var fns = new Dictionary<string, NativeFn>();

            fns["print"] = (args, kwargs) =>
            {
                string Option(string name, string fallback)
                {
                    if (!kwargs.TryGetValue(name, out object v) || v == null) return fallback;
                    if (!(v is string s)) throw PyError("TypeError", name + " must be None or a string, not " + TypeName(v));
                    return s;
                }
                foreach (var key in kwargs.Keys)
                {
                    if (key == "file") throw new UnsupportedFeature(Feature.Builtin, null, "print(file=...)");
                    if (key != "sep" && key != "end" && key != "flush")
                        throw PyError("TypeError", "'" + key + "' is an invalid keyword argument for print()");
                }
                string text = string.Join(Option("sep", " "), args.Select(Str));
                ctx.Write(text + Option("end", "\n"));
                return null;
            };

            fns["len"] = (args, kwargs) =>
            {
                NoKwargs("len", kwargs);
                ArgCount("len", args, 1, 1);
                return Length(args[0]);
            };

            fns["repr"] = (args, kwargs) =>
            {
                NoKwargs("repr", kwargs);
                ArgCount("repr", args, 1, 1);
                return Repr(args[0]);
            };

            fns["abs"] = (args, kwargs) =>
            {
                NoKwargs("abs", kwargs);
                ArgCount("abs", args, 1, 1);
                var n = AsNumber(args[0]);
                if (n == null) throw PyError("TypeError", "bad operand type for abs(): '" + TypeName(args[0]) + "'");
                return n.Value.IsFloat ? (object)Math.Abs(n.Value.F) : BigInteger.Abs(n.Value.I);
            };

            fns["round"] = (args, kwargs) =>
            {
                var all = new List<object>(args);
                if (kwargs.TryGetValue("ndigits", out object nd))
                {
                    while (all.Count < 2) all.Add(null);
                    all[1] = nd;
                }
                if (all.Count == 0) throw PyError("TypeError", "round() missing required argument 'number' (pos 1)");
                object second = ArgOrNull(all, 1);
                BigInteger? ndigits = second == null ? (BigInteger?)null : ToIndex(second);
                var x = AsNumber(all[0]);
                if (x != null && x.Value.IsFloat) return RoundFloat(x.Value.F, ndigits);
                if (x != null) return RoundInt(x.Value.I, ndigits);
                throw PyError("TypeError", "type " + TypeName(all[0]) + " doesn't define __round__ method");
            };

            NativeFn MinMax(string name) => (args, kwargs) =>
            {
                foreach (var key in kwargs.Keys)
                {
                    if (key == "key") throw new UnsupportedFeature(Feature.Builtin, null, name + "(key=...)");
                    if (key != "default") throw PyError("TypeError", name + "() got an unexpected keyword argument '" + key + "'");
                }
                if (args.Count == 0) throw PyError("TypeError", name + " expected at least 1 argument, got 0");
                List<object> items;
                if (args.Count == 1)
                {
                    items = Collect(args[0], ctx);
                    if (items.Count == 0)
                    {
                        if (kwargs.TryGetValue("default", out object d)) return d;
                        throw PyError("ValueError", name + "() iterable argument is empty");
                    }
                }
                else
                {
                    if (kwargs.ContainsKey("default"))
                        throw PyError("TypeError", "Cannot specify a default for " + name + "() with multiple positional arguments");
                    items = args;
                }
                object best = items[0];
                for (int k = 1; k < items.Count; k++)
                {
                    if (OrderCompare(name == "min" ? "<" : ">", items[k], best)) best = items[k];
                }
                return best;
            };
            fns["min"] = MinMax("min");
            fns["max"] = MinMax("max");

            // CPython 3.12 builtin_sum: tam sayılar kesin, ondalıklar Neumaier düzeltmesiyle toplanır.
            fns["sum"] = (args, kwargs) =>
            {
                var all = new List<object>(args);
                if (kwargs.TryGetValue("start", out object st))
                {
                    while (all.Count < 2) all.Add(null);
                    all[1] = st;
                }
                if (all.Count == 0) throw PyError("TypeError", "sum() takes at least 1 positional argument (0 given)");
                object result = ArgOrNull(all, 1) ?? Zero;
                if (result is string) throw PyError("TypeError", "sum() can't sum strings [use ''.join(seq) instead]");
                double? floatSum = null;
                double compensation = 0;
                foreach (var item in Iterate(all[0]))
                {
                    ctx.Tick();
                    if (floatSum == null && result is double rf) floatSum = rf;
                    if (floatSum != null)
                    {
                        double fs = floatSum.Value;
                        if (item is double fi)
                        {
                            double t = fs + fi;
                            if (Math.Abs(fs) >= Math.Abs(fi)) compensation += fs - t + fi;
                            else compensation += fi - t + fs;
                            floatSum = t;
                            continue;
                        }
                        if (item is BigInteger || item is bool)
                        {
                            double n = Numbers.ToDouble(AsNumber(item).Value.I);
                            if (!double.IsInfinity(n) && !double.IsNaN(n))
                            {
                                floatSum = fs + n;
                                continue;
                            }
                        }
                        if (compensation != 0 && !double.IsNaN(compensation) && !double.IsInfinity(compensation)) fs += compensation;
                        result = fs;
                        floatSum = null;
                        compensation = 0;
                    }
                    result = BinaryOp("+", result, item);
                }
                if (floatSum != null)
                {
                    double fs = floatSum.Value;
                    if (compensation != 0 && !double.IsNaN(compensation) && !double.IsInfinity(compensation)) fs += compensation;
                    return fs;
                }
                return result;
            };

            fns["sorted"] = (args, kwargs) =>
            {
                ArgCount("sorted", args, 1, 1);
                foreach (var key in kwargs.Keys)
                {
                    if (key == "key") throw new UnsupportedFeature(Feature.Builtin, null, "sorted(key=...)");
                    if (key != "reverse") throw PyError("TypeError", "sort() got an unexpected keyword argument '" + key + "'");
                }
                var items = Collect(args[0], ctx);
                SortItems(items, kwargs.TryGetValue("reverse", out object rev) && Truthy(rev));
                return new PyList(items);
            };

            fns["isinstance"] = (args, kwargs) =>
            {
                NoKwargs("isinstance", kwargs);
                ArgCount("isinstance", args, 2, 2);
                object value = args[0], spec = args[1];
                var classes = spec is PyTuple t ? t.Items : new List<object> { spec };
                foreach (var cls in classes)
                {
                    if (!(cls is PyType type)) throw PyError("TypeError", "isinstance() arg 2 must be a type, a tuple of types, or a union");
                    string actual = TypeName(value);
                    if (actual == type.Name || (type.Name == "int" && actual == "bool")) return true;
                }
                return false;
            };

            var builtins = new Dictionary<string, object>();
            foreach (var name in PythonNames.BuiltinNames)
            {
                if (types.TryGetValue(name, out PyType type)) builtins[name] = type;
                else if (fns.TryGetValue(name, out NativeFn fn)) builtins[name] = new PyBuiltin(name, fn);
            }
            return builtins;
        }

        /// <summary>Yerleşik isim listesinde olup motorun desteklemediği isimler (input, open...)</summary>
        public static bool IsKnownBuiltinName(string name) => Array.IndexOf(PythonNames.BuiltinNames, name) >= 0;

        // --- sıralama ---

        /// <summary>
        /// Python'un list.sort'u gibi: kararlı, sadece &lt; kullanır; reverse=True kararlılığı korur.
        /// Küçük listelerde karşılaştırma sırası da CPython'la aynıdır (hata mesajı aynı çıksın diye).
        /// </summary>
        public static void SortItems(List<object> items, bool reverse)
        {
            if (reverse) items.Reverse();
            if (items.Count < 64) CPythonSmallSort(items);
            else MergeSort(items, 0, items.Count, new object[items.Count]);
            if (reverse) items.Reverse();
        }

        /// <summary>CPython listsort, 64'ten kısa listeler için: count_run + binarysort</summary>
        static void CPythonSmallSort(List<object> a)
        {
            int n = a.Count;
            if (n < 2) return;
            int run = 2;
            if (PyLess(a[1], a[0]))
            {
                while (run < n && PyLess(a[run], a[run - 1])) run++;
                a.Reverse(0, run);
            }
            else
            {
                while (run < n && !PyLess(a[run], a[run - 1])) run++;
            }
            for (int start = run; start < n; start++)
            {
                object pivot = a[start];
                int l = 0, r = start;
                do
                {
                    int p = l + ((r - l) >> 1);
                    if (PyLess(pivot, a[p])) r = p;
                    else l = p + 1;
                } while (l < r);
                a.RemoveAt(start);
                a.Insert(l, pivot);
            }
        }

        static void MergeSort(List<object> a, int lo, int hi, object[] tmp)
        {
            if (hi - lo < 2) return;
            int mid = (lo + hi) / 2;
            MergeSort(a, lo, mid, tmp);
            MergeSort(a, mid, hi, tmp);
            int i = lo, j = mid, k = lo;
            while (i < mid && j < hi) tmp[k++] = PyLess(a[j], a[i]) ? a[j++] : a[i++];
            while (i < mid) tmp[k++] = a[i++];
            while (j < hi) tmp[k++] = a[j++];
            for (k = lo; k < hi; k++) a[k] = tmp[k];
        }

        // --- metotlar ---

        delegate object Method(object self, List<object> args, Dictionary<string, object> kwargs, IBuiltinContext ctx);

        static List<object> ItemsOf(object self) => self is PyList l ? l.Items : ((PyTuple)self).Items;

        static int FindIndex(List<object> items, object x)
        {
            for (int k = 0; k < items.Count; k++)
            {
                if (PyEquals(items[k], x)) return k;
            }
            return -1;
        }

        static readonly Dictionary<string, Method> ListMethods = new Dictionary<string, Method>
        {
            ["append"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.append", kwargs);
                ArgCount("list.append", args, 1, 1);
                ItemsOf(self).Add(args[0]);
                return null;
            },
            ["extend"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.extend", kwargs);
                ArgCount("list.extend", args, 1, 1);
                ItemsOf(self).AddRange(Collect(args[0], ctx));
                return null;
            },
            ["insert"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.insert", kwargs);
                if (args.Count != 2) throw PyError("TypeError", "insert expected 2 arguments, got " + args.Count);
                var items = ItemsOf(self);
                BigInteger n = items.Count;
                var i = ToIndex(args[0]);
                if (i < 0) i = i + n < 0 ? Zero : i + n;
                if (i > n) i = n;
                items.Insert((int)i, args[1]);
                return null;
            },
            ["pop"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.pop", kwargs);
                ArgCount("pop", args, 0, 1);
                var items = ItemsOf(self);
                if (items.Count == 0) throw PyError("IndexError", "pop from empty list");
                BigInteger i = args.Count > 0 ? ToIndex(args[0]) : BigInteger.MinusOne;
                if (i < 0) i += items.Count;
                if (i < 0 || i >= items.Count) throw PyError("IndexError", "pop index out of range");
                object value = items[(int)i];
                items.RemoveAt((int)i);
                return value;
            },
            ["remove"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.remove", kwargs);
                ArgCount("list.remove", args, 1, 1);
                var items = ItemsOf(self);
                int i = FindIndex(items, args[0]);
                if (i < 0) throw PyError("ValueError", "list.remove(x): x not in list");
                items.RemoveAt(i);
                return null;
            },
            ["index"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.index", kwargs);
                ArgCount("index", args, 1, 3);
                int i = FindIndex(ItemsOf(self), args[0]);
                if (i < 0) throw PyError("ValueError", Repr(args[0]) + " is not in list");
                return new BigInteger(i);
            },
            ["count"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.count", kwargs);
                ArgCount("list.count", args, 1, 1);
                return new BigInteger(ItemsOf(self).Count(x => PyEquals(x, args[0])));
            },
            ["sort"] = (self, args, kwargs, ctx) =>
            {
                if (args.Count > 0) throw PyError("TypeError", "sort() takes no positional arguments");
                foreach (var key in kwargs.Keys)
                {
                    if (key == "key") throw new UnsupportedFeature(Feature.Method, null, "list.sort(key=...)");
                    if (key != "reverse") throw PyError("TypeError", "sort() got an unexpected keyword argument '" + key + "'");
                }
                SortItems(ItemsOf(self), kwargs.TryGetValue("reverse", out object rev) && Truthy(rev));
                return null;
            },
            ["reverse"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.reverse", kwargs);
                ArgCount("list.reverse", args, 0, 0);
                ItemsOf(self).Reverse();
                return null;
            },
            ["clear"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.clear", kwargs);
                ArgCount("list.clear", args, 0, 0);
                ItemsOf(self).Clear();
                return null;
            },
            ["copy"] = (self, args, kwargs, ctx) =>
            {
                NoKwargs("list.copy", kwargs);
                ArgCount("list.copy", args, 0, 0);
                return new PyList(new List<object>(ItemsOf(self)));
            },
        };

        static readonly Dictionary<string, Method> TupleMethods = new Dictionary<string, Method>
        {
            ["index"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("index", args, 1, 3);
                int i = FindIndex(ItemsOf(self), args[0]);
                if (i < 0) throw PyError("ValueError", "tuple.index(x): x not in tuple");
                return new BigInteger(i);
            },
            ["count"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("tuple.count", args, 1, 1);
                return new BigInteger(ItemsOf(self).Count(x => PyEquals(x, args[0])));
            },
        };

        static string StrArg(string method, object v, int position = 1)
        {
            if (!(v is string s)) throw PyError("TypeError", method + "() argument " + position + " must be str, not " + TypeName(v));
            return s;
        }

        static string StripChars(string self, List<object> args, string where)
        {
            object chars = ArgOrNull(args, 0);
            if (chars == null)
            {
                string s = where == "right" ? self : TrimPySpaceStart(self);
                return where == "left" ? s : TrimPySpaceEnd(s);
            }
            var set = new HashSet<string>(StrChars(StrArg("strip", chars)));
            var cs = StrChars(self);
            int start = 0, end = cs.Count;
            if (where != "right") while (start < end && set.Contains(cs[start])) start++;
            if (where != "left") while (end > start && set.Contains(cs[end - 1])) end--;
            return string.Concat(cs.GetRange(start, end - start));
        }

        // Tam büyük/küçük harf dönüşümü (Python gibi): C#'ın dönüşümü tek karakterlik olduğu için
        // birkaç özel durum elle eklenir (ß → SS, İ → i\u0307 ...).
        static readonly Dictionary<char, string> SpecialUpper = new Dictionary<char, string>
        {
            ['ß'] = "SS", ['ŉ'] = "ʼN", ['ǰ'] = "J̌", ['ﬀ'] = "FF", ['ﬁ'] = "FI", ['ﬂ'] = "FL", ['ﬃ'] = "FFI", ['ﬄ'] = "FFL",
            ['ﬅ'] = "ST", ['ﬆ'] = "ST",
        };

        static string PyUpper(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (SpecialUpper.TryGetValue(c, out string up)) sb.Append(up);
                else sb.Append(char.ToUpperInvariant(c));
            }
            return sb.ToString();
        }

        static string PyLower(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (c == 'İ') sb.Append("i\u0307");
                else sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        /// <summary>UTF-16 konumunu Python'un kod noktası konumuna çevirir.</summary>
        static BigInteger CodePointIndex(string s, int utf16Index) => utf16Index < 0 ? utf16Index : StrLength(s.Substring(0, utf16Index));

        static bool AllCategory(string s, Func<UnicodeCategory, bool> ok)
        {
            if (s.Length == 0) return false;
            for (int k = 0; k < s.Length; k++)
            {
                if (!ok(Category(s, k))) return false;
                if (char.IsHighSurrogate(s[k]) && k + 1 < s.Length && char.IsLowSurrogate(s[k + 1])) k++;
            }
            return true;
        }

        static bool IsLetterCategory(UnicodeCategory c) =>
            c == UnicodeCategory.UppercaseLetter || c == UnicodeCategory.LowercaseLetter || c == UnicodeCategory.TitlecaseLetter
            || c == UnicodeCategory.ModifierLetter || c == UnicodeCategory.OtherLetter;

        static List<object> SplitWhitespace(string self, int maxsplit)
        {
            var parts = new List<object>();
            int k = 0, n = self.Length;
            while (true)
            {
                while (k < n && IsPySpace(self[k])) k++;
                if (k >= n) break;
                if (maxsplit >= 0 && parts.Count == maxsplit)
                {
                    parts.Add(self.Substring(k));
                    break;
                }
                int start = k;
                while (k < n && !IsPySpace(self[k])) k++;
                parts.Add(self.Substring(start, k - start));
            }
            return parts;
        }

        static string ReplaceText(string self, string old, string repl, int count)
        {
            if (count < 0) count = int.MaxValue;
            var sb = new StringBuilder();
            if (old.Length == 0)
            {
                // Boş metin her karakterin önüne (ve en sona) eklenir: "ab".replace("", "-") → "-a-b-"
                var chars = StrChars(self);
                for (int k = 0; k < chars.Count; k++)
                {
                    if (count-- > 0) sb.Append(repl);
                    sb.Append(chars[k]);
                }
                if (count > 0) sb.Append(repl);
                return sb.ToString();
            }
            int pos = 0;
            while (count-- > 0)
            {
                int found = self.IndexOf(old, pos, StringComparison.Ordinal);
                if (found < 0) break;
                sb.Append(self, pos, found - pos).Append(repl);
                pos = found + old.Length;
            }
            sb.Append(self, pos, self.Length - pos);
            return sb.ToString();
        }

        static int CountOccurrences(string self, string sub)
        {
            int count = 0, pos = 0;
            while (true)
            {
                int found = self.IndexOf(sub, pos, StringComparison.Ordinal);
                if (found < 0) return count;
                count++;
                pos = found + sub.Length;
            }
        }

        static bool Affix(string self, List<object> args, string method)
        {
            ArgCount(method, args, 1, 3);
            object spec = args[0];
            var options = spec is PyTuple t ? t.Items : new List<object> { spec };
            foreach (var option in options)
            {
                if (!(option is string o))
                    throw PyError("TypeError", method + " first arg must be str or a tuple of str, not " + TypeName(option));
                bool match = method == "startswith"
                    ? self.StartsWith(o, StringComparison.Ordinal)
                    : self.EndsWith(o, StringComparison.Ordinal);
                if (match) return true;
            }
            return false;
        }

        static readonly Dictionary<string, Method> StrMethods = new Dictionary<string, Method>
        {
            ["upper"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("str.upper", args, 0, 0);
                return PyUpper((string)self);
            },
            ["lower"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("str.lower", args, 0, 0);
                return PyLower((string)self);
            },
            ["capitalize"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("str.capitalize", args, 0, 0);
                var cs = StrChars((string)self);
                return cs.Count > 0 ? PyUpper(cs[0]) + PyLower(string.Concat(cs.Skip(1))) : "";
            },
            ["strip"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("strip", args, 0, 1);
                return StripChars((string)self, args, "both");
            },
            ["lstrip"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("lstrip", args, 0, 1);
                return StripChars((string)self, args, "left");
            },
            ["rstrip"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("rstrip", args, 0, 1);
                return StripChars((string)self, args, "right");
            },
            ["split"] = (self, args, kwargs, ctx) =>
            {
                string s = (string)self;
                object sep = kwargs.TryGetValue("sep", out object ks) ? ks : ArgOrNull(args, 0);
                object maxsplitValue = kwargs.TryGetValue("maxsplit", out object km) ? km : args.Count > 1 ? args[1] : (object)BigInteger.MinusOne;
                int maxsplit = ToSmallInt(maxsplitValue);
                if (sep == null) return new PyList(SplitWhitespace(s, maxsplit));
                string separator = StrArg("split", sep);
                if (separator.Length == 0) throw PyError("ValueError", "empty separator");
                var pieces = s.Split(new[] { separator }, StringSplitOptions.None);
                if (maxsplit >= 0 && pieces.Length > maxsplit + 1)
                {
                    var head = pieces.Take(maxsplit).Cast<object>().ToList();
                    head.Add(string.Join(separator, pieces.Skip(maxsplit)));
                    return new PyList(head);
                }
                return new PyList(pieces.Cast<object>().ToList());
            },
            ["join"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("str.join", args, 1, 1);
                var items = Collect(args[0], ctx);
                for (int k = 0; k < items.Count; k++)
                {
                    if (!(items[k] is string))
                        throw PyError("TypeError", "sequence item " + k + ": expected str instance, " + TypeName(items[k]) + " found");
                }
                return string.Join((string)self, items.Cast<string>());
            },
            ["replace"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("replace", args, 2, 3);
                string old = StrArg("replace", args[0], 1);
                string repl = StrArg("replace", args[1], 2);
                int count = args.Count > 2 ? ToSmallInt(args[2]) : -1;
                return ReplaceText((string)self, old, repl, count);
            },
            ["startswith"] = (self, args, kwargs, ctx) => Affix((string)self, args, "startswith"),
            ["endswith"] = (self, args, kwargs, ctx) => Affix((string)self, args, "endswith"),
            ["find"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("find", args, 1, 3);
                string s = (string)self;
                return CodePointIndex(s, s.IndexOf(StrArg("find", args[0]), StringComparison.Ordinal));
            },
            ["index"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("index", args, 1, 3);
                string s = (string)self;
                int i = s.IndexOf(StrArg("index", args[0]), StringComparison.Ordinal);
                if (i < 0) throw PyError("ValueError", "substring not found");
                return CodePointIndex(s, i);
            },
            ["count"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("count", args, 1, 3);
                string s = (string)self;
                string sub = StrArg("count", args[0]);
                if (sub.Length == 0) return new BigInteger(StrLength(s) + 1);
                return new BigInteger(CountOccurrences(s, sub));
            },
            ["isdigit"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("str.isdigit", args, 0, 0);
                return AllCategory((string)self, c => c == UnicodeCategory.DecimalDigitNumber);
            },
            ["isalpha"] = (self, args, kwargs, ctx) =>
            {
                ArgCount("str.isalpha", args, 0, 0);
                return AllCategory((string)self, IsLetterCategory);
            },
            ["isupper"] = (self, args, kwargs, ctx) =>
            {
                string s = (string)self;
                return s != PyLower(s) && s == PyUpper(s);
            },
            ["islower"] = (self, args, kwargs, ctx) =>
            {
                string s = (string)self;
                return s != PyUpper(s) && s == PyLower(s);
            },
        };

        static Dictionary<string, Method> MethodsOf(string type)
        {
            switch (type)
            {
                case "list": return ListMethods;
                case "tuple": return TupleMethods;
                case "str": return StrMethods;
                default: return null;
            }
        }

        /// <summary>değer.isim — metot ya da AttributeError</summary>
        public static object GetAttribute(object value, string name, IBuiltinContext ctx)
        {
            string type = TypeName(value);
            var methods = MethodsOf(type);
            if (methods != null && methods.TryGetValue(name, out Method method))
            {
                return new PyMethod
                {
                    Self = value,
                    Name = name,
                    Call = (args, kwargs) => method(value, args, kwargs, ctx),
                };
            }
            PythonNames.TypeDir.TryGetValue(type, out string[] dir);
            if (dir != null && Array.IndexOf(dir, name) >= 0) throw new UnsupportedFeature(Feature.Method, null, type + "." + name);
            string message = "'" + type + "' object has no attribute '" + name + "'";
            string suggestion = dir != null ? Suggestions.CalculateSuggestion(dir, name) : null;
            if (suggestion != null) message += ". Did you mean: '" + suggestion + "'?";
            throw PyError("AttributeError", message);
        }
    }
}
