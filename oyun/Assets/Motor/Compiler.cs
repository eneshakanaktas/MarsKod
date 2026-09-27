// Derleyici: yapı ağacını (AST) basit komutlar listesine çevirir (CPython'un bytecode'u gibi).
//
// Neden: Çalıştırıcı bu listeyi tek bir döngüde işler. C# yığını büyümez; böylece
// iç içe 1000 fonksiyon çağrısı (Python'un sınırı) telefonda da çalışır ve adım adım mod kolaylaşır.
// Her komut, hatası çıkarsa bildirilecek satırı taşır (CPython'daki gibi komutun kendi satırı).

using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Motor
{
    public enum OpCode
    {
        STEP, CONST, LOAD, STORE, LOAD_ATTR, STORE_ATTR, BINARY, INPLACE, UNARY, COMPARE,
        BUILD_LIST, BUILD_TUPLE, BUILD_SLICE, SUBSCR, STORE_SUBSCR, UNPACK, CALL,
        POP, DUP, DUP2, ROT2, ROT3,
        JUMP, JUMP_IF_FALSE, JUMP_IF_FALSE_OR_POP, JUMP_IF_TRUE_OR_POP,
        GET_ITER, FOR_ITER, MAKE_FUNCTION, RETURN, UNSUPPORTED,
    }

    /// <summary>Tek komut. Hangi alanların kullanıldığı komuta göre değişir.</summary>
    public sealed class Instr
    {
        public OpCode Op;
        public int Line;
        /// <summary>CONST</summary>
        public object Value;
        /// <summary>LOAD, STORE, LOAD_ATTR, STORE_ATTR: isim. BINARY, INPLACE, UNARY, COMPARE: işleç.
        /// UNSUPPORTED: özellik adı.</summary>
        public string Name;
        /// <summary>UNSUPPORTED: ayrıntı</summary>
        public string Detail;
        /// <summary>BUILD_*, UNPACK: öğe sayısı. CALL: sıralı argüman sayısı</summary>
        public int Count;
        /// <summary>CALL: isimli argümanların adları</summary>
        public string[] KwNames;
        /// <summary>JUMP*, FOR_ITER: atlanacak komut</summary>
        public int Target;
        /// <summary>MAKE_FUNCTION</summary>
        public FunctionDef Def;

        public override string ToString() => Op + " " + (Name ?? Value?.ToString() ?? "") + " @" + Line;
    }

    public sealed class Code
    {
        public readonly List<Instr> Instrs;

        public Code(List<Instr> instrs)
        {
            Instrs = instrs;
        }
    }

    public sealed class Compiler
    {
        sealed class Loop
        {
            public int ContinueTarget;
            public readonly List<int> BreakJumps = new List<int>();
            /// <summary>for döngüsünde yığında yineleyici durur; break öncesi atılmalı</summary>
            public bool IsFor;
        }

        readonly List<Instr> instrs = new List<Instr>();
        readonly List<Loop> loops = new List<Loop>();

        int Emit(OpCode op, int line, string name = null, object value = null, int count = 0, int target = -1)
        {
            instrs.Add(new Instr { Op = op, Line = line, Name = name, Value = value, Count = count, Target = target });
            return instrs.Count - 1;
        }

        int Here => instrs.Count;

        void Patch(int index, int target) => instrs[index].Target = target;

        // --- cümleler ---

        void Body(List<Stmt> stmts)
        {
            foreach (var s in stmts) Statement(s);
        }

        void Statement(Stmt s)
        {
            Emit(OpCode.STEP, s.Line);
            switch (s)
            {
                case ExprStmt e:
                    Expression(e.Value);
                    Emit(OpCode.POP, s.Line);
                    return;

                case Assign a:
                    Expression(a.Value);
                    for (int i = 0; i < a.Targets.Count; i++)
                    {
                        if (i < a.Targets.Count - 1) Emit(OpCode.DUP, s.Line);
                        Store(a.Targets[i]);
                    }
                    return;

                case AugAssign a:
                    AugmentedAssign(a);
                    return;

                case If i:
                {
                    Expression(i.Test);
                    int toElse = Emit(OpCode.JUMP_IF_FALSE, s.Line);
                    Body(i.Body);
                    if (i.OrElse.Count == 0)
                    {
                        Patch(toElse, Here);
                        return;
                    }
                    int toEnd = Emit(OpCode.JUMP, s.Line);
                    Patch(toElse, Here);
                    Body(i.OrElse);
                    Patch(toEnd, Here);
                    return;
                }

                case While w:
                {
                    int toTest = Emit(OpCode.JUMP, s.Line);
                    int cont = Emit(OpCode.STEP, s.Line); // her yeni turda başlık satırı
                    Patch(toTest, Here);
                    Expression(w.Test);
                    int toElse = Emit(OpCode.JUMP_IF_FALSE, s.Line);
                    var loop = new Loop { ContinueTarget = cont, IsFor = false };
                    loops.Add(loop);
                    Body(w.Body);
                    loops.RemoveAt(loops.Count - 1);
                    Emit(OpCode.JUMP, s.Line, target: cont);
                    Patch(toElse, Here);
                    Body(w.OrElse);
                    foreach (int j in loop.BreakJumps) Patch(j, Here);
                    return;
                }

                case For f:
                {
                    Expression(f.Iter);
                    Emit(OpCode.GET_ITER, f.Iter.Line);
                    int toNext = Emit(OpCode.JUMP, s.Line);
                    int cont = Emit(OpCode.STEP, s.Line);
                    Patch(toNext, Here);
                    int forIter = Emit(OpCode.FOR_ITER, s.Line);
                    Store(f.Target);
                    var loop = new Loop { ContinueTarget = cont, IsFor = true };
                    loops.Add(loop);
                    Body(f.Body);
                    loops.RemoveAt(loops.Count - 1);
                    Emit(OpCode.JUMP, s.Line, target: cont);
                    Patch(forIter, Here);
                    Body(f.OrElse);
                    foreach (int j in loop.BreakJumps) Patch(j, Here);
                    return;
                }

                case Break _:
                {
                    var loop = loops[loops.Count - 1];
                    if (loop.IsFor) Emit(OpCode.POP, s.Line);
                    loop.BreakJumps.Add(Emit(OpCode.JUMP, s.Line));
                    return;
                }

                case Continue _:
                    Emit(OpCode.JUMP, s.Line, target: loops[loops.Count - 1].ContinueTarget);
                    return;

                case FunctionDef d:
                    foreach (var p in d.Params) if (p.Default != null) Expression(p.Default);
                    instrs.Add(new Instr { Op = OpCode.MAKE_FUNCTION, Line = s.Line, Def = d });
                    Emit(OpCode.STORE, s.Line, d.Name);
                    return;

                case Return r:
                    if (r.Value != null) Expression(r.Value);
                    else Emit(OpCode.CONST, s.Line);
                    Emit(OpCode.RETURN, s.Line);
                    return;

                case Pass _:
                case Global _:
                case Nonlocal _:
                    return;
            }
        }

        void AugmentedAssign(AugAssign s)
        {
            switch (s.Target)
            {
                case Name n:
                    Emit(OpCode.LOAD, n.Line, n.Id);
                    Expression(s.Value);
                    Emit(OpCode.INPLACE, s.Line, s.Op);
                    Emit(OpCode.STORE, s.Line, n.Id);
                    break;
                case Subscript t:
                    Expression(t.Value);
                    Index(t.Index);
                    Emit(OpCode.DUP2, t.Line);
                    Emit(OpCode.SUBSCR, t.Line);
                    Expression(s.Value);
                    Emit(OpCode.INPLACE, s.Line, s.Op);
                    Emit(OpCode.ROT3, s.Line);
                    Emit(OpCode.STORE_SUBSCR, t.Line);
                    break;
                case Attribute t:
                    Expression(t.Value);
                    Emit(OpCode.DUP, t.Line);
                    Emit(OpCode.LOAD_ATTR, t.Line, t.Attr);
                    Expression(s.Value);
                    Emit(OpCode.INPLACE, s.Line, s.Op);
                    Emit(OpCode.ROT2, s.Line);
                    Emit(OpCode.STORE_ATTR, t.Line, t.Attr);
                    break;
            }
        }

        /// <summary>Yığının tepesindeki değeri hedefe yazar.</summary>
        void Store(Expr target)
        {
            switch (target)
            {
                case Name n:
                    Emit(OpCode.STORE, n.Line, n.Id);
                    return;
                case TupleExpr t:
                    Emit(OpCode.UNPACK, t.Line, count: t.Elts.Count);
                    foreach (var elt in t.Elts) Store(elt);
                    return;
                case ListExpr l:
                    Emit(OpCode.UNPACK, l.Line, count: l.Elts.Count);
                    foreach (var elt in l.Elts) Store(elt);
                    return;
                case Subscript s:
                    Expression(s.Value);
                    Index(s.Index);
                    Emit(OpCode.STORE_SUBSCR, s.Line);
                    return;
                case Attribute a:
                    Expression(a.Value);
                    Emit(OpCode.STORE_ATTR, a.Line, a.Attr);
                    return;
            }
            throw new InvalidOperationException("atanamaz hedef: " + target.GetType().Name);
        }

        // --- ifadeler ---

        void Expression(Expr e)
        {
            switch (e)
            {
                case Constant c:
                    Emit(OpCode.CONST, e.Line, value: c.Value);
                    return;
                case Name n:
                    Emit(OpCode.LOAD, e.Line, n.Id);
                    return;
                case BinOp b:
                    Expression(b.Left);
                    Expression(b.Right);
                    Emit(OpCode.BINARY, e.Line, b.Op);
                    return;
                case UnaryOp u:
                    Expression(u.Operand);
                    Emit(OpCode.UNARY, e.Line, u.Op);
                    return;
                case BoolOp b:
                {
                    var jumps = new List<int>();
                    for (int i = 0; i < b.Values.Count; i++)
                    {
                        Expression(b.Values[i]);
                        if (i < b.Values.Count - 1)
                            jumps.Add(Emit(b.Op == "and" ? OpCode.JUMP_IF_FALSE_OR_POP : OpCode.JUMP_IF_TRUE_OR_POP, e.Line));
                    }
                    foreach (int j in jumps) Patch(j, Here);
                    return;
                }
                case Compare c:
                    Comparison(c);
                    return;
                case IfExp i:
                {
                    Expression(i.Test);
                    int toElse = Emit(OpCode.JUMP_IF_FALSE, e.Line);
                    Expression(i.Body);
                    int toEnd = Emit(OpCode.JUMP, e.Line);
                    Patch(toElse, Here);
                    Expression(i.OrElse);
                    Patch(toEnd, Here);
                    return;
                }
                case Call c:
                    Expression(c.Func);
                    foreach (var arg in c.Args) Expression(arg);
                    foreach (var k in c.Keywords) Expression(k.Value);
                    instrs.Add(new Instr
                    {
                        Op = OpCode.CALL, Line = e.Line, Count = c.Args.Count, KwNames = c.Keywords.Select(k => k.Name).ToArray(),
                    });
                    return;
                case Attribute a:
                    Expression(a.Value);
                    Emit(OpCode.LOAD_ATTR, e.Line, a.Attr);
                    return;
                case Subscript s:
                    Expression(s.Value);
                    Index(s.Index);
                    Emit(OpCode.SUBSCR, e.Line);
                    return;
                case ListExpr l:
                    foreach (var elt in l.Elts) Expression(elt);
                    Emit(OpCode.BUILD_LIST, e.Line, count: l.Elts.Count);
                    return;
                case TupleExpr t:
                    foreach (var elt in t.Elts) Expression(elt);
                    Emit(OpCode.BUILD_TUPLE, e.Line, count: t.Elts.Count);
                    return;
                case DictExpr _:
                    Emit(OpCode.UNSUPPORTED, e.Line, Feature.Dict);
                    return;
                case SetExpr _:
                    Emit(OpCode.UNSUPPORTED, e.Line, Feature.Set);
                    return;
                case Slice _:
                    Unsupported(Feature.Method, "slice", e.Line);
                    return;
            }
        }

        void Unsupported(string feature, string detail, int line) =>
            instrs.Add(new Instr { Op = OpCode.UNSUPPORTED, Line = line, Name = feature, Detail = detail });

        void Index(Expr index)
        {
            if (index is Slice s)
            {
                foreach (var part in new[] { s.Lower, s.Upper, s.Step })
                {
                    if (part != null) Expression(part);
                    else Emit(OpCode.CONST, index.Line);
                }
                Emit(OpCode.BUILD_SLICE, index.Line);
                return;
            }
            if (index is TupleExpr t && t.Elts.Any(x => x is Slice))
            {
                Unsupported(Feature.Method, "multi-slice", index.Line);
                return;
            }
            Expression(index);
        }

        /// <summary>a &lt; b &lt; c: CPython gibi ara değer bir kez hesaplanır, ilk yanlışta durulur.</summary>
        void Comparison(Compare e)
        {
            Expression(e.Left);
            if (e.Ops.Count == 1)
            {
                Expression(e.Comparators[0]);
                Emit(OpCode.COMPARE, e.Line, e.Ops[0]);
                return;
            }
            var cleanups = new List<int>();
            for (int i = 0; i < e.Ops.Count; i++)
            {
                Expression(e.Comparators[i]);
                if (i < e.Ops.Count - 1)
                {
                    Emit(OpCode.DUP, e.Line);
                    Emit(OpCode.ROT3, e.Line);
                    Emit(OpCode.COMPARE, e.Line, e.Ops[i]);
                    cleanups.Add(Emit(OpCode.JUMP_IF_FALSE_OR_POP, e.Line));
                }
                else
                {
                    Emit(OpCode.COMPARE, e.Line, e.Ops[i]);
                }
            }
            int toEnd = Emit(OpCode.JUMP, e.Line);
            foreach (int j in cleanups) Patch(j, Here);
            Emit(OpCode.ROT2, e.Line);
            Emit(OpCode.POP, e.Line);
            Patch(toEnd, Here);
        }

        public static Code CompileModule(Module module)
        {
            var c = new Compiler();
            c.Body(module.Body);
            c.Emit(OpCode.CONST, 0);
            c.Emit(OpCode.RETURN, 0);
            return new Code(c.instrs);
        }

        public static Code CompileFunction(FunctionDef def)
        {
            if (def.CompiledCode != null) return def.CompiledCode;
            var c = new Compiler();
            c.Body(def.Body);
            int endLine = def.Body.Count > 0 ? def.Body[def.Body.Count - 1].Line : def.Line;
            c.Emit(OpCode.CONST, endLine);
            c.Emit(OpCode.RETURN, endLine);
            def.CompiledCode = new Code(c.instrs);
            return def.CompiledCode;
        }
    }
}
