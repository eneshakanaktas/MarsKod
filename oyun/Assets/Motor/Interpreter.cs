// Çalıştırıcı: derleyicinin ürettiği komutları tek bir döngüde işler.
//
// Her cümleden önce durak (Step) verir: adım adım modunda oyun burada bekler, normal
// çalıştırmada duraklar hemen geçilir. Fonksiyon çağrıları C# yığınını kullanmaz;
// çağrı çerçeveleri kendi listemizde tutulur (Python'un 1000 derinlik sınırı telefonda da geçerli).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MarsKod.Motor.Operators;
using static MarsKod.Motor.Values;

namespace MarsKod.Motor
{
    /// <summary>Değişkenlerin yaşadığı yer: modül ya da bir fonksiyon çağrısı</summary>
    public sealed class Frame
    {
        public readonly Dictionary<string, object> Vars = new Dictionary<string, object>();
        public readonly PyFunction Fn;
        /// <summary>Tanımlandığı (dıştaki) fonksiyonun çerçevesi</summary>
        public readonly Frame Parent;

        public Frame(PyFunction fn, Frame parent)
        {
            Fn = fn;
            Parent = parent;
        }
    }

    /// <summary>Durak: şu an çalışacak satır ve o anki değişkenler</summary>
    public sealed class Step
    {
        public readonly int Line;
        public readonly Frame Frame;

        public Step(int line, Frame frame)
        {
            Line = line;
            Frame = frame;
        }
    }

    public sealed class InterpreterOptions
    {
        /// <summary>Bu kadar adımdan sonra durdurulur (bitmeyen döngüye karşı)</summary>
        public int MaxSteps = 200000;
        /// <summary>CPython'daki gibi en fazla iç içe çağrı (modül dahil)</summary>
        public int RecursionLimit = 1000;
        /// <summary>Oyunun verdiği komutlar (move, collect...)</summary>
        public Dictionary<string, object> Externals;
    }

    /// <summary>Kapsam bilgisi: hangi isim yerel, hangisi global</summary>
    internal sealed class ScopeInfo
    {
        public List<string> LocalNames;
        public HashSet<string> LocalSet;
        public HashSet<string> GlobalNames;
        public HashSet<string> NonlocalNames;
    }

    public sealed class Interpreter : IBuiltinContext
    {
        /// <summary>Çalışmakta olan bir kod parçası: komutlar, sıradaki komut, ara değerler yığını</summary>
        sealed class Activation
        {
            public readonly Code Code;
            public int Pc;
            public readonly List<object> Stack = new List<object>();
            public readonly Frame Frame;

            public Activation(Code code, Frame frame)
            {
                Code = code;
                Frame = frame;
            }
        }

        readonly StringBuilder output = new StringBuilder();
        int steps;
        int? currentLine;
        public readonly Frame Globals = new Frame(null, null);
        readonly Dictionary<string, object> builtins;
        readonly List<string> builtinNames;
        readonly InterpreterOptions options;
        List<Activation> calls;

        /// <summary>print ile ekrana yazılan her şey</summary>
        public string Output => output.ToString();

        public Interpreter(InterpreterOptions options)
        {
            this.options = options;
            foreach (var name in PythonNames.ModuleGlobalNames) Globals.Vars[name] = name == "__name__" ? "__main__" : null;
            builtins = Builtins.Create(this);
            builtinNames = new List<string>(PythonNames.BuiltinNames);
            if (options.Externals != null)
            {
                foreach (var pair in options.Externals)
                {
                    builtins[pair.Key] = pair.Value;
                    builtinNames.Add(pair.Key);
                }
            }
        }

        void IBuiltinContext.Write(string text) => output.Append(text);

        void IBuiltinContext.Tick() => Tick();

        void Tick()
        {
            if (++steps > options.MaxSteps) throw new ExecutionLimit("steps", currentLine);
        }

        /// <summary>Satırı olmayan hataya, çıktığı komutun satırını yazar.</summary>
        static void FillLine(Exception e, int line)
        {
            switch (e)
            {
                case PythonException p when p.Line == null:
                    p.Line = line;
                    break;
                case UnsupportedFeature u when u.Line == null:
                    u.Line = line;
                    break;
                case ExecutionLimit l when l.Line == null:
                    l.Line = line;
                    break;
            }
        }

        /// <summary>
        /// Kodu çalıştırır; her cümleden önce bir durak verir. Sonuna kadar gezilince kod biter.
        /// Python hatası PythonException, desteklenmeyen özellik UnsupportedFeature, sınır aşımı
        /// ExecutionLimit olarak fırlatılır.
        /// </summary>
        public IEnumerable<Step> Run(Module module)
        {
            calls = new List<Activation> { new Activation(Compiler.CompileModule(module), Globals) };
            while (true)
            {
                var step = Advance();
                if (step == null) yield break;
                yield return step;
            }
        }

        static object Pop(List<object> stack)
        {
            object top = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            return top;
        }

        static List<object> Splice(List<object> stack, int count)
        {
            var items = stack.GetRange(stack.Count - count, count);
            stack.RemoveRange(stack.Count - count, count);
            return items;
        }

        /// <summary>Bir sonraki durağa kadar çalışır; kod bittiyse null.</summary>
        Step Advance()
        {
            int line = 0;
            try
            {
                while (true)
                {
                    var act = calls[calls.Count - 1];
                    var stack = act.Stack;
                    var ins = act.Code.Instrs[act.Pc++];
                    line = ins.Line;

                    switch (ins.Op)
                    {
                        case OpCode.STEP:
                            currentLine = ins.Line;
                            Tick();
                            return new Step(ins.Line, act.Frame);

                        case OpCode.CONST:
                            stack.Add(ins.Value);
                            break;
                        case OpCode.LOAD:
                            stack.Add(Load(ins.Name, act.Frame));
                            break;
                        case OpCode.STORE:
                            Store(ins.Name, Pop(stack), act.Frame);
                            break;
                        case OpCode.LOAD_ATTR:
                            stack.Add(Builtins.GetAttribute(Pop(stack), ins.Name, this));
                            break;
                        case OpCode.STORE_ATTR:
                            throw AttributeAssignError(Pop(stack), ins.Name);

                        case OpCode.BINARY:
                        {
                            object right = Pop(stack);
                            stack.Add(BinaryOp(ins.Name, Pop(stack), right));
                            break;
                        }
                        case OpCode.INPLACE:
                        {
                            object right = Pop(stack);
                            stack.Add(InplaceOp(ins.Name, Pop(stack), right));
                            break;
                        }
                        case OpCode.UNARY:
                            stack.Add(UnaryOperation(ins.Name, Pop(stack)));
                            break;
                        case OpCode.COMPARE:
                        {
                            object right = Pop(stack);
                            stack.Add(CompareOp(ins.Name, Pop(stack), right));
                            break;
                        }

                        case OpCode.BUILD_LIST:
                            stack.Add(new PyList(Splice(stack, ins.Count)));
                            break;
                        case OpCode.BUILD_TUPLE:
                            stack.Add(new PyTuple(Splice(stack, ins.Count)));
                            break;
                        case OpCode.BUILD_SLICE:
                        {
                            var parts = Splice(stack, 3);
                            stack.Add(new SliceValue { Lower = parts[0], Upper = parts[1], Step = parts[2] });
                            break;
                        }
                        case OpCode.SUBSCR:
                        {
                            object index = Pop(stack);
                            stack.Add(GetItem(Pop(stack), index));
                            break;
                        }
                        case OpCode.STORE_SUBSCR:
                        {
                            object index = Pop(stack);
                            object container = Pop(stack);
                            SetItem(container, index, Pop(stack));
                            break;
                        }
                        case OpCode.UNPACK:
                        {
                            var items = Unpack(Pop(stack), ins.Count);
                            for (int i = items.Count - 1; i >= 0; i--) stack.Add(items[i]);
                            break;
                        }

                        case OpCode.POP:
                            Pop(stack);
                            break;
                        case OpCode.DUP:
                            stack.Add(stack[stack.Count - 1]);
                            break;
                        case OpCode.DUP2:
                            stack.Add(stack[stack.Count - 2]);
                            stack.Add(stack[stack.Count - 2]);
                            break;
                        case OpCode.ROT2:
                        {
                            object top = Pop(stack);
                            stack.Insert(stack.Count - 1, top);
                            break;
                        }
                        case OpCode.ROT3:
                        {
                            object top = Pop(stack);
                            stack.Insert(stack.Count - 2, top);
                            break;
                        }

                        case OpCode.JUMP:
                            act.Pc = ins.Target;
                            break;
                        case OpCode.JUMP_IF_FALSE:
                            if (!Truthy(Pop(stack))) act.Pc = ins.Target;
                            break;
                        case OpCode.JUMP_IF_FALSE_OR_POP:
                            if (!Truthy(stack[stack.Count - 1])) act.Pc = ins.Target;
                            else Pop(stack);
                            break;
                        case OpCode.JUMP_IF_TRUE_OR_POP:
                            if (Truthy(stack[stack.Count - 1])) act.Pc = ins.Target;
                            else Pop(stack);
                            break;

                        case OpCode.GET_ITER:
                            stack.Add(GetIterator(Pop(stack)));
                            break;
                        case OpCode.FOR_ITER:
                        {
                            var iterator = (IEnumerator<object>)stack[stack.Count - 1];
                            if (iterator.MoveNext())
                            {
                                stack.Add(iterator.Current);
                            }
                            else
                            {
                                Pop(stack);
                                act.Pc = ins.Target;
                            }
                            break;
                        }

                        case OpCode.MAKE_FUNCTION:
                        {
                            var def = ins.Def;
                            var withDefaults = def.Params.Where(p => p.Default != null).ToList();
                            var values = Splice(stack, withDefaults.Count);
                            var defaults = new Dictionary<string, object>();
                            for (int i = 0; i < withDefaults.Count; i++) defaults[withDefaults[i].Name] = values[i];
                            var scope = ScopeOf(def);
                            var outer = act.Frame.Fn;
                            stack.Add(new PyFunction
                            {
                                Def = def,
                                Qualname = outer != null ? outer.Qualname + ".<locals>." + def.Name : def.Name,
                                Defaults = defaults,
                                Closure = outer != null ? act.Frame : null,
                                LocalNames = scope.LocalNames,
                                GlobalNames = scope.GlobalNames,
                                NonlocalNames = scope.NonlocalNames,
                            });
                            break;
                        }

                        case OpCode.CALL:
                        {
                            var kwvalues = Splice(stack, ins.KwNames.Length);
                            var args = Splice(stack, ins.Count);
                            object func = Pop(stack);
                            var kwargs = new Dictionary<string, object>();
                            for (int i = 0; i < ins.KwNames.Length; i++) kwargs[ins.KwNames[i]] = kwvalues[i];
                            if (func is PyFunction fn)
                            {
                                var frame = BindArguments(fn, args, kwargs);
                                if (calls.Count >= options.RecursionLimit) throw PyError("RecursionError", "maximum recursion depth exceeded");
                                calls.Add(new Activation(Compiler.CompileFunction(fn.Def), frame));
                            }
                            else
                            {
                                stack.Add(CallNative(func, args, kwargs));
                            }
                            break;
                        }

                        case OpCode.RETURN:
                        {
                            object value = Pop(stack);
                            calls.RemoveAt(calls.Count - 1);
                            if (calls.Count == 0) return null;
                            calls[calls.Count - 1].Stack.Add(value);
                            break;
                        }

                        case OpCode.UNSUPPORTED:
                            throw new UnsupportedFeature(ins.Name, ins.Line, ins.Detail);
                    }
                }
            }
            catch (Exception e)
            {
                FillLine(e, line);
                throw;
            }
        }

        object CallNative(object func, List<object> args, Dictionary<string, object> kwargs)
        {
            switch (func)
            {
                case PyBuiltin b:
                    Tick();
                    return b.Call(args, kwargs);
                case PyMethod m:
                    Tick();
                    return m.Call(args, kwargs);
                case PyType t:
                    if (t.Call == null) throw PyError("TypeError", "cannot create '" + t.Name + "' instances");
                    Tick();
                    return t.Call(args, kwargs);
            }
            throw PyError("TypeError", "'" + TypeName(func) + "' object is not callable");
        }

        // --- isimler ---

        object Load(string name, Frame frame)
        {
            var fn = frame.Fn;
            if (fn != null && !fn.GlobalNames.Contains(name))
            {
                if (ScopeOf(fn.Def).LocalSet.Contains(name))
                {
                    if (frame.Vars.TryGetValue(name, out object local)) return local;
                    throw PyError("UnboundLocalError", "cannot access local variable '" + name + "' where it is not associated with a value");
                }
                for (var p = frame.Parent; p != null && p.Fn != null; p = p.Parent)
                {
                    if (p.Fn.GlobalNames.Contains(name)) break;
                    if (ScopeOf(p.Fn.Def).LocalSet.Contains(name))
                    {
                        if (p.Vars.TryGetValue(name, out object free)) return free;
                        throw PyError(
                            "NameError",
                            "cannot access free variable '" + name + "' where it is not associated with a value in enclosing scope");
                    }
                }
            }
            if (Globals.Vars.TryGetValue(name, out object global)) return global;
            if (builtins.TryGetValue(name, out object builtin)) return builtin;
            if (Builtins.IsKnownBuiltinName(name)) throw new UnsupportedFeature(Feature.Builtin, null, name);
            var scopes = new List<IList<string>>
            {
                fn != null ? fn.LocalNames : new List<string>(),
                Globals.Vars.Keys.ToList(),
                builtinNames,
            };
            throw PyError("NameError", Suggestions.NameErrorMessage(name, scopes));
        }

        void Store(string name, object value, Frame frame)
        {
            var fn = frame.Fn;
            if (fn == null || fn.GlobalNames.Contains(name))
            {
                Globals.Vars[name] = value;
                return;
            }
            if (fn.NonlocalNames.Contains(name))
            {
                for (var p = frame.Parent; p != null && p.Fn != null; p = p.Parent)
                {
                    if (ScopeOf(p.Fn.Def).LocalSet.Contains(name))
                    {
                        p.Vars[name] = value;
                        return;
                    }
                }
            }
            frame.Vars[name] = value;
        }

        // --- çağrı ---

        /// <summary>Argümanları parametrelere bağlar; hata mesajları CPython ile aynı.</summary>
        static Frame BindArguments(PyFunction fn, List<object> args, Dictionary<string, object> kwargs)
        {
            var parameters = fn.Def.Params;
            string name = fn.Qualname;
            if (args.Count > parameters.Count)
            {
                int required = parameters.Count(p => p.Default == null);
                string given = args.Count + " " + (args.Count == 1 ? "was" : "were") + " given";
                if (required == parameters.Count)
                {
                    string s = parameters.Count == 1 ? "" : "s";
                    throw PyError("TypeError", name + "() takes " + parameters.Count + " positional argument" + s + " but " + given);
                }
                throw PyError("TypeError", name + "() takes from " + required + " to " + parameters.Count + " positional arguments but " + given);
            }

            var frame = new Frame(fn, fn.Closure);
            for (int i = 0; i < args.Count; i++) frame.Vars[parameters[i].Name] = args[i];
            foreach (var pair in kwargs)
            {
                if (!parameters.Any(p => p.Name == pair.Key))
                    throw PyError("TypeError", name + "() got an unexpected keyword argument '" + pair.Key + "'");
                if (frame.Vars.ContainsKey(pair.Key))
                    throw PyError("TypeError", name + "() got multiple values for argument '" + pair.Key + "'");
                frame.Vars[pair.Key] = pair.Value;
            }
            var missing = parameters
                .Where(p => !frame.Vars.ContainsKey(p.Name) && !fn.Defaults.ContainsKey(p.Name))
                .Select(p => "'" + p.Name + "'")
                .ToList();
            if (missing.Count > 0)
            {
                string list = missing.Count == 1
                    ? missing[0]
                    : missing.Count == 2
                        ? missing[0] + " and " + missing[1]
                        : string.Join(", ", missing.Take(missing.Count - 1)) + ", and " + missing[missing.Count - 1];
                string s = missing.Count == 1 ? "" : "s";
                throw PyError("TypeError", name + "() missing " + missing.Count + " required positional argument" + s + ": " + list);
            }
            foreach (var pair in fn.Defaults)
            {
                if (!frame.Vars.ContainsKey(pair.Key)) frame.Vars[pair.Key] = pair.Value;
            }
            return frame;
        }

        // --- yardımcılar ---

        static IEnumerator<object> GetIterator(object v)
        {
            if (!IsIterable(v)) throw PyError("TypeError", "'" + TypeName(v) + "' object is not iterable");
            return Iterate(v).GetEnumerator();
        }

        static List<object> Unpack(object value, int expected)
        {
            List<object> items;
            try
            {
                items = Iterate(value).ToList();
            }
            catch (PythonException e) when (e.PyType == "TypeError")
            {
                throw PyError("TypeError", "cannot unpack non-iterable " + TypeName(value) + " object");
            }
            if (items.Count > expected) throw PyError("ValueError", "too many values to unpack (expected " + expected + ")");
            if (items.Count < expected)
                throw PyError("ValueError", "not enough values to unpack (expected " + expected + ", got " + items.Count + ")");
            return items;
        }

        static PythonException AttributeAssignError(object obj, string attr)
        {
            string type = TypeName(obj);
            if (PythonNames.TypeDir.TryGetValue(type, out string[] dir) && Array.IndexOf(dir, attr) >= 0)
                return PyError("AttributeError", "'" + type + "' object attribute '" + attr + "' is read-only");
            return PyError("AttributeError", "'" + type + "' object has no attribute '" + attr + "'");
        }

        // --- kapsam (hangi isim yerel, hangisi global) ---

        /// <summary>
        /// Fonksiyonun yerel isimleri: parametreler + gövdede atanan isimler (global/nonlocal hariç).
        /// Sıra CPython'un co_varnames sırasına yakındır: parametreler, sonra ilk geçtikleri sıra.
        /// ("Did you mean" önerisinde eşit yakınlıktaki isimlerden hangisinin seçileceğini bu sıra belirler.)
        /// </summary>
        static ScopeInfo ScopeOf(FunctionDef def)
        {
            if (def.Scope != null) return def.Scope;

            var bound = new HashSet<string>();
            var globalNames = new HashSet<string>();
            var nonlocalNames = new HashSet<string>();

            void BindTarget(Expr t)
            {
                if (t is Name n) bound.Add(n.Id);
                else if (t is TupleExpr tu) tu.Elts.ForEach(BindTarget);
                else if (t is ListExpr li) li.Elts.ForEach(BindTarget);
            }

            void CollectBindings(List<Stmt> body)
            {
                foreach (var s in body)
                {
                    switch (s)
                    {
                        case Assign a:
                            a.Targets.ForEach(BindTarget);
                            break;
                        case AugAssign a:
                            BindTarget(a.Target);
                            break;
                        case For f:
                            BindTarget(f.Target);
                            CollectBindings(f.Body);
                            CollectBindings(f.OrElse);
                            break;
                        case If i:
                            CollectBindings(i.Body);
                            CollectBindings(i.OrElse);
                            break;
                        case While w:
                            CollectBindings(w.Body);
                            CollectBindings(w.OrElse);
                            break;
                        case FunctionDef d:
                            bound.Add(d.Name);
                            break;
                        case Global g:
                            foreach (var n in g.Names) globalNames.Add(n);
                            break;
                        case Nonlocal nl:
                            foreach (var n in nl.Names) nonlocalNames.Add(n);
                            break;
                    }
                }
            }
            CollectBindings(def.Body);

            var parameters = def.Params.Select(p => p.Name).ToList();
            bool IsLocal(string n) => parameters.Contains(n) || (bound.Contains(n) && !globalNames.Contains(n) && !nonlocalNames.Contains(n));
            var order = new List<string>(parameters);
            void Note(string n)
            {
                if (!order.Contains(n) && IsLocal(n)) order.Add(n);
            }
            void VisitExpr(Expr e)
            {
                if (e is Name n) Note(n.Id);
                Parser.ForEachChild(e, VisitExpr);
            }
            void VisitBody(List<Stmt> body)
            {
                foreach (var s in body)
                {
                    switch (s)
                    {
                        case ExprStmt e:
                            VisitExpr(e.Value);
                            break;
                        case Assign a:
                            VisitExpr(a.Value);
                            a.Targets.ForEach(VisitExpr);
                            break;
                        case AugAssign a:
                            VisitExpr(a.Target);
                            VisitExpr(a.Value);
                            break;
                        case If i:
                            VisitExpr(i.Test);
                            VisitBody(i.Body);
                            VisitBody(i.OrElse);
                            break;
                        case While w:
                            VisitExpr(w.Test);
                            VisitBody(w.Body);
                            VisitBody(w.OrElse);
                            break;
                        case For f:
                            VisitExpr(f.Iter);
                            VisitExpr(f.Target);
                            VisitBody(f.Body);
                            VisitBody(f.OrElse);
                            break;
                        case FunctionDef d:
                            foreach (var p in d.Params) if (p.Default != null) VisitExpr(p.Default);
                            Note(d.Name);
                            break;
                        case Return r:
                            if (r.Value != null) VisitExpr(r.Value);
                            break;
                    }
                }
            }
            VisitBody(def.Body);

            def.Scope = new ScopeInfo
            {
                LocalNames = order,
                LocalSet = new HashSet<string>(order),
                GlobalNames = globalNames,
                NonlocalNames = nonlocalNames,
            };
            return def.Scope;
        }
    }
}
