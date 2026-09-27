// Kodun yapı ağacı (AST): cümle çözücünün çıktısı, çalıştırıcının girdisi.
// Adlar CPython'un ast modülüyle aynı tutuldu; böylece Python belgeleriyle karşılaştırmak kolay.
// (List, Tuple, Dict, Set ve Expr cümlesi, C#'taki adlarla karışmasın diye ...Expr / ExprStmt diye adlandırıldı.)

using System.Collections.Generic;

namespace MarsKod.Motor
{
    public struct Loc
    {
        /// <summary>1'den başlar</summary>
        public int Line;
        /// <summary>0'dan başlar</summary>
        public int Col;
        public int EndLine;
        public int EndCol;

        public Loc(int line, int col, int endLine, int endCol)
        {
            Line = line;
            Col = col;
            EndLine = endLine;
            EndCol = endCol;
        }
    }

    public abstract class Node
    {
        public int Line;
        public int Col;
        public int EndLine;
        public int EndCol;

        public Loc Loc => new Loc(Line, Col, EndLine, EndCol);

        public void SetLoc(Loc loc)
        {
            Line = loc.Line;
            Col = loc.Col;
            EndLine = loc.EndLine;
            EndCol = loc.EndCol;
        }
    }

    // --- ifadeler ---

    public abstract class Expr : Node
    {
        /// <summary>Aynı ifadenin başka konumlu kopyası (parantez içine alınmış ifadeler için)</summary>
        public Expr CopyWithLoc(Loc loc)
        {
            var copy = (Expr)MemberwiseClone();
            copy.SetLoc(loc);
            return copy;
        }
    }

    public sealed class Name : Expr
    {
        public string Id;
    }

    /// <summary>Sabit değer: BigInteger (int), double (float), string, bool ya da null (None)</summary>
    public sealed class Constant : Expr
    {
        public object Value;
    }

    /// <summary>İşleç: + - * / // % ** @ &lt;&lt; &gt;&gt; &amp; | ^</summary>
    public sealed class BinOp : Expr
    {
        public string Op;
        public Expr Left;
        public Expr Right;
    }

    /// <summary>İşleç: - + ~ not</summary>
    public sealed class UnaryOp : Expr
    {
        public string Op;
        public Expr Operand;
    }

    /// <summary>İşleç: and, or</summary>
    public sealed class BoolOp : Expr
    {
        public string Op;
        public List<Expr> Values;
    }

    /// <summary>İşleçler: == != &lt; &lt;= &gt; &gt;= in, not in, is, is not</summary>
    public sealed class Compare : Expr
    {
        public Expr Left;
        public List<string> Ops;
        public List<Expr> Comparators;
    }

    public sealed class IfExp : Expr
    {
        public Expr Test;
        public Expr Body;
        public Expr OrElse;
    }

    public sealed class Keyword : Node
    {
        public string Name;
        public Expr Value;
    }

    public sealed class Call : Expr
    {
        public Expr Func;
        public List<Expr> Args;
        public List<Keyword> Keywords;
    }

    public sealed class Attribute : Expr
    {
        public Expr Value;
        public string Attr;
    }

    public sealed class Subscript : Expr
    {
        public Expr Value;
        public Expr Index;
    }

    public sealed class Slice : Expr
    {
        public Expr Lower;
        public Expr Upper;
        public Expr Step;
    }

    public sealed class ListExpr : Expr
    {
        public List<Expr> Elts;
    }

    public sealed class TupleExpr : Expr
    {
        public List<Expr> Elts;
    }

    public sealed class DictExpr : Expr
    {
        public List<Expr> Keys;
        public List<Expr> Values;
    }

    public sealed class SetExpr : Expr
    {
        public List<Expr> Elts;
    }

    // --- cümleler ---
    // Bileşik cümlelerde (if, for...) konum sadece başlık satırını kapsar; adım adım modunda
    // "şu an çalışan satır" olarak bu gösterilir.

    public abstract class Stmt : Node
    {
    }

    public sealed class ExprStmt : Stmt
    {
        public Expr Value;
    }

    public sealed class Assign : Stmt
    {
        public List<Expr> Targets;
        public Expr Value;
    }

    public sealed class AugAssign : Stmt
    {
        public Expr Target;
        public string Op;
        public Expr Value;
    }

    public sealed class If : Stmt
    {
        public Expr Test;
        public List<Stmt> Body;
        public List<Stmt> OrElse;
    }

    public sealed class While : Stmt
    {
        public Expr Test;
        public List<Stmt> Body;
        public List<Stmt> OrElse;
    }

    public sealed class For : Stmt
    {
        public Expr Target;
        public Expr Iter;
        public List<Stmt> Body;
        public List<Stmt> OrElse;
    }

    public sealed class Param : Node
    {
        public string Name;
        public Expr Default;
    }

    public sealed class FunctionDef : Stmt
    {
        public string Name;
        public List<Param> Params;
        public List<Stmt> Body;

        // Çalıştırıcının bir kez hesaplayıp sakladığı bilgiler
        internal Code CompiledCode;
        internal ScopeInfo Scope;
    }

    public sealed class Return : Stmt
    {
        public Expr Value;
    }

    public sealed class Global : Stmt
    {
        public List<string> Names;
    }

    public sealed class Nonlocal : Stmt
    {
        public List<string> Names;
    }

    public sealed class Pass : Stmt
    {
    }

    public sealed class Break : Stmt
    {
    }

    public sealed class Continue : Stmt
    {
    }

    public sealed class Module
    {
        public List<Stmt> Body;
    }
}
