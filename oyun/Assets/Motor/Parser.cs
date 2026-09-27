// Cümle çözücü: parçalardan (token) kodun yapı ağacını (AST) çıkarır.
//
// CPython 3.12'nin hata davranışı taklit edilir:
// - Genel hata "invalid syntax", okunan en ileri parçanın satırında bildirilir.
// - Sık yapılan hatalar için CPython'un özel mesajları (invalid_* kuralları) aynen üretilir.
// - Kelime ayırıcı hatası varsa çoğu durumda o öne geçer (bkz. Parse()).
// - Ağaç kurulduktan sonra derleme aşaması hataları ('return' outside function...) denetlenir.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace MarsKod.Motor
{
    public sealed class Parser
    {
        static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "False", "None", "True", "and", "as", "assert", "async", "await", "break", "class", "continue", "def", "del",
            "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal",
            "not", "or", "pass", "raise", "return", "try", "while", "with", "yield",
        };

        static readonly HashSet<string> SoftKeywords = new HashSet<string> { "_", "case", "match", "type" };

        static readonly Dictionary<string, string> AugAssignOps = new Dictionary<string, string>
        {
            ["+="] = "+", ["-="] = "-", ["*="] = "*", ["/="] = "/", ["//="] = "//", ["%="] = "%", ["**="] = "**", ["@="] = "@",
            ["<<="] = "<<", [">>="] = ">>", ["&="] = "&", ["|="] = "|", ["^="] = "^",
        };

        static readonly HashSet<string> CompareOps = new HashSet<string> { "==", "!=", "<", "<=", ">", ">=" };

        static readonly HashSet<string> ExpressionNames = new HashSet<string> { "True", "False", "None", "not", "lambda", "await", "yield" };
        static readonly HashSet<string> ExpressionOps = new HashSet<string> { "(", "[", "{", "-", "+", "~", "*", "..." };

        /// <summary>Kelime ayırıcının hata yüzünden kestiği yere ulaşıldı.</summary>
        sealed class OutOfTokens : Exception
        {
        }

        public static Module Parse(string source)
        {
            var tokenized = Tokenizer.Tokenize(source);
            var tokenError = tokenized.Error;
            var parser = new Parser(tokenized.Tokens);
            Module module;
            try
            {
                module = parser.ParseModule();
            }
            catch (OutOfTokens)
            {
                throw tokenError;
            }
            catch (PythonException) when (tokenError != null && TokenErrorWins(tokenError, parser))
            {
                // CPython cümle hatasından sonra dosyanın geri kalanını da parçalar; orada bir kelime hatası
                // çıkarsa o öne geçer. Tek istisna kapanmamış parantez: sadece okumanın ulaştığı en ileri
                // satır, parantezin açıldığı satırdan sonraysa öne geçer.
                throw tokenError;
            }
            CheckModule(module);
            // Python önce bütün dosyayı derler: yazım hataları desteklenmeyen özellikten önce bildirilir.
            if (parser.FirstUnsupported != null) throw parser.FirstUnsupported;
            return module;
        }

        static bool TokenErrorWins(PythonException tokenError, Parser parser)
        {
            bool unclosed = tokenError.PyMessage.EndsWith("was never closed", StringComparison.Ordinal);
            return !unclosed || parser.FurthestLine() > (tokenError.Line ?? 0);
        }

        readonly List<Token> tokens;
        int pos;
        /// <summary>Okunan en ileri parça: genel hatanın yeri</summary>
        int furthest;
        /// <summary>İç içe parantez derinliği ("Perhaps you forgot a comma?" sadece parantez içinde)</summary>
        int depth;
        /// <summary>Parantez içine alınmış ifadeler: (x > 3) = 5 gibi durumların ayrımı için</summary>
        readonly HashSet<Expr> parenthesized = new HashSet<Expr>();

        /// <summary>İlk desteklenmeyen özellik. Okuma durmaz; dosya sonunda bildirilir.</summary>
        public UnsupportedFeature FirstUnsupported;

        Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        int FurthestLine() => tokens[furthest].Line;

        // --- parça okuma ---

        Token Peek(int offset = 0)
        {
            int index = pos + offset;
            if (index >= tokens.Count) throw new OutOfTokens();
            if (index > furthest) furthest = index;
            return tokens[index];
        }

        Token Next()
        {
            var token = Peek();
            pos++;
            return token;
        }

        Token Prev() => tokens[pos - 1];

        bool IsOp(string value, int offset = 0)
        {
            var t = Peek(offset);
            return t.Type == TokenType.OP && t.Value == value;
        }

        bool IsKeyword(string value, int offset = 0)
        {
            var t = Peek(offset);
            return t.Type == TokenType.NAME && t.Value == value;
        }

        Token AcceptOp(string value) => IsOp(value) ? Next() : null;

        Token ExpectOp(string value)
        {
            if (!IsOp(value)) throw Fail();
            return Next();
        }

        /// <summary>CPython'un "zorunlu parça" hatası: 'else' ve 'def' sonrası ':' gibi</summary>
        Token ExpectForced(string value)
        {
            if (!IsOp(value)) throw Error("expected '" + value + "'", Peek().Line);
            return Next();
        }

        bool IsStatementEnd()
        {
            var t = Peek();
            return t.Type == TokenType.NEWLINE || (t.Type == TokenType.OP && t.Value == ";");
        }

        bool StartsExpression(Token t)
        {
            if (t.Type == TokenType.NUMBER || t.Type == TokenType.STRING) return true;
            if (t.Type == TokenType.NAME) return !Keywords.Contains(t.Value) || ExpressionNames.Contains(t.Value);
            return t.Type == TokenType.OP && ExpressionOps.Contains(t.Value);
        }

        // --- hata ---

        PythonException Fail()
        {
            var t = tokens[furthest];
            if (t.Type == TokenType.INDENT) return new PythonException("IndentationError", "unexpected indent", t.Line);
            if (t.Type == TokenType.DEDENT) return new PythonException("IndentationError", "unexpected unindent", t.Line);
            return new PythonException("SyntaxError", "invalid syntax", t.Line);
        }

        static PythonException Error(string message, int line) => new PythonException("SyntaxError", message, line);

        void Note(string feature, int line)
        {
            if (FirstUnsupported == null) FirstUnsupported = new UnsupportedFeature(feature, line);
        }

        /// <summary>Satır sonuna kadar atlar ve satır sonunu tüketir.</summary>
        void SkipLine()
        {
            while (Peek().Type != TokenType.NEWLINE) Next();
            Next();
        }

        /// <summary>Desteklenmeyen bileşik cümleyi (class, try, with...) başlığı ve girintili bloklarıyla atlar.</summary>
        void SkipCompound(params string[] clauses)
        {
            do
            {
                SkipLine();
                if (Peek().Type == TokenType.INDENT)
                {
                    int d = 0;
                    do
                    {
                        var t = Next();
                        if (t.Type == TokenType.INDENT) d++;
                        else if (t.Type == TokenType.DEDENT) d--;
                    } while (d > 0);
                }
            } while (clauses.Any(c => IsKeyword(c)));
        }

        static bool IsOpenBracket(Token t) => t.Type == TokenType.OP && (t.Value == "(" || t.Value == "[" || t.Value == "{");
        static bool IsCloseBracket(Token t) => t.Type == TokenType.OP && (t.Value == ")" || t.Value == "]" || t.Value == "}");

        /// <summary>Basit cümlenin sonuna (satır sonu ya da ';') kadar atlar.</summary>
        void SkipSimpleStatement()
        {
            int d = 0;
            while (true)
            {
                var t = Peek();
                if (t.Type == TokenType.NEWLINE || (d == 0 && t.Type == TokenType.OP && t.Value == ";")) return;
                if (IsOpenBracket(t)) d++;
                if (IsCloseBracket(t)) d--;
                Next();
            }
        }

        /// <summary>Açılmış bir parantezin kapanışına kadar atlar ve kapanışı tüketir.</summary>
        void SkipToCloser()
        {
            int d = 1;
            while (d > 0)
            {
                var t = Next();
                if (IsOpenBracket(t)) d++;
                if (IsCloseBracket(t)) d--;
            }
        }

        /// <summary>yield ifadesi: not alınır, yerine None konur.</summary>
        Expr YieldExpr()
        {
            var start = Next();
            Note(Feature.Yield, start.Line);
            if (IsKeyword("from")) Next();
            if (!IsStatementEnd() && !IsOp(")") && StartsExpression(Peek())) StarExpressions();
            return At(new Constant { Value = null }, LocOf(start));
        }

        /// <summary>Liste üreteci kuyruğu: for x in y if z ... (desteklenmez ama yazım denetlenir)</summary>
        void ComprehensionTail()
        {
            Note(Feature.Comprehension, Peek().Line);
            while (IsKeyword("for") || IsKeyword("async"))
            {
                if (IsKeyword("async")) Next();
                Next();
                TargetList();
                if (!IsKeyword("in")) throw Fail();
                Next();
                Disjunction();
                while (IsKeyword("if"))
                {
                    Next();
                    Disjunction();
                }
            }
        }

        /// <summary>Tür ipucu (x: int) desteklenmez ama yazımı denetlenir.</summary>
        void SkipAnnotation()
        {
            if (!IsOp(":")) return;
            Note(Feature.Annotation, Next().Line);
            Expression();
        }

        /// <summary>
        /// Bir şeyi denemek için okur ve hiçbir şey olmamış gibi geri döner.
        /// Okuma hatasız biter ve fn true verirse true.
        /// </summary>
        bool Attempt(Func<bool> fn)
        {
            int saved = pos;
            try
            {
                return fn();
            }
            catch (OutOfTokens)
            {
                throw;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                pos = saved;
            }
        }

        static T At<T>(T node, Loc loc) where T : Node
        {
            node.SetLoc(loc);
            return node;
        }

        Loc LocOf(Token start)
        {
            var end = Prev();
            return new Loc(start.Line, start.Col, end.EndLine, end.EndCol);
        }

        Loc LocOf(Node start)
        {
            var end = Prev();
            return new Loc(start.Line, start.Col, end.EndLine, end.EndCol);
        }

        static Loc Span(Node start, Node end) => new Loc(start.Line, start.Col, end.EndLine, end.EndCol);
        static Loc Span(Token start, Node end) => new Loc(start.Line, start.Col, end.EndLine, end.EndCol);
        static Loc Span(Node start, Token end) => new Loc(start.Line, start.Col, end.EndLine, end.EndCol);

        // --- cümleler ---

        Module ParseModule()
        {
            var body = new List<Stmt>();
            while (Peek().Type != TokenType.ENDMARKER) body.AddRange(Statement());
            return new Module { Body = body };
        }

        List<Stmt> Statement()
        {
            var t = Peek();
            if (t.Type == TokenType.NAME)
            {
                switch (t.Value)
                {
                    case "if":
                        return new List<Stmt> { IfStatement() };
                    case "while":
                        return new List<Stmt> { WhileStatement() };
                    case "for":
                        return new List<Stmt> { ForStatement() };
                    case "def":
                        return new List<Stmt> { FunctionDefinition() };
                    case "class":
                    case "try":
                    case "with":
                    case "async":
                        Note(t.Value, t.Line);
                        if (t.Value == "try") SkipCompound("except", "else", "finally");
                        else SkipCompound();
                        return new List<Stmt>();
                }
            }
            if (t.Type == TokenType.OP && t.Value == "@")
            {
                // Dekoratör satırı atlanır; altındaki def normal okunur.
                Note(Feature.Decorator, t.Line);
                SkipLine();
                return new List<Stmt>();
            }
            return SimpleStatements();
        }

        List<Stmt> SimpleStatements()
        {
            var stmts = new List<Stmt> { SimpleStatement() };
            while (AcceptOp(";") != null)
            {
                if (Peek().Type == TokenType.NEWLINE) break;
                stmts.Add(SimpleStatement());
            }
            if (Peek().Type != TokenType.NEWLINE) throw Fail();
            Next();
            return stmts;
        }

        Stmt SimpleStatement()
        {
            var t = Peek();
            if (t.Type == TokenType.NAME)
            {
                switch (t.Value)
                {
                    case "pass":
                        Next();
                        return At(new Pass(), LocOf(t));
                    case "break":
                        Next();
                        return At(new Break(), LocOf(t));
                    case "continue":
                        Next();
                        return At(new Continue(), LocOf(t));
                    case "return":
                    {
                        Next();
                        var value = IsStatementEnd() ? null : StarExpressions();
                        return At(new Return { Value = value }, LocOf(t));
                    }
                    case "global":
                    case "nonlocal":
                    {
                        Next();
                        var names = new List<string> { ParseName() };
                        while (AcceptOp(",") != null) names.Add(ParseName());
                        if (t.Value == "global") return At(new Global { Names = names }, LocOf(t));
                        return At(new Nonlocal { Names = names }, LocOf(t));
                    }
                    case "import":
                    case "from":
                    case "raise":
                    case "assert":
                    case "del":
                    case "yield":
                        Note(t.Value == "from" ? Feature.Import : t.Value, t.Line);
                        SkipSimpleStatement();
                        return At(new Pass(), LocOf(t));
                }
            }
            return ExpressionStatement();
        }

        string ParseName()
        {
            var t = Peek();
            if (t.Type != TokenType.NAME || Keywords.Contains(t.Value)) throw Fail();
            return Next().Value;
        }

        Stmt ExpressionStatement()
        {
            var start = Peek();
            var first = StarExpressions();

            if (IsOp("="))
            {
                var exprs = new List<Expr> { first };
                int afterFirstEq = pos + 1;
                while (AcceptOp("=") != null) exprs.Add(IsKeyword("yield") ? YieldExpr() : StarExpressions());
                var value = exprs[exprs.Count - 1];
                exprs.RemoveAt(exprs.Count - 1);
                for (int index = 0; index < exprs.Count; index++)
                {
                    var target = exprs[index];
                    var invalid = InvalidTarget(target, false);
                    if (invalid == null) continue;
                    if (index == 0 && AssignHereRule(target, afterFirstEq))
                        throw Error("cannot assign to " + ExprName(target) + " here. Maybe you meant '==' instead of '='?", target.Line);
                    throw Error("cannot assign to " + ExprName(invalid), invalid.Line);
                }
                return At(new Assign { Targets = exprs, Value = value }, LocOf(start));
            }

            var t = Peek();
            if (t.Type == TokenType.OP && AugAssignOps.TryGetValue(t.Value, out string augOp))
            {
                if (!(first is Name) && !(first is Attribute) && !(first is Subscript))
                    throw Error("'" + ExprName(first) + "' is an illegal expression for augmented assignment", first.Line);
                Next();
                var value = IsKeyword("yield") ? YieldExpr() : StarExpressions();
                return At(new AugAssign { Target = first, Op = augOp, Value = value }, LocOf(start));
            }

            // x: int = 5 → tür ipucu desteklenmez; atama olarak okunur
            if (IsOp(":"))
            {
                SkipAnnotation();
                if (AcceptOp("=") != null)
                {
                    var value = IsKeyword("yield") ? YieldExpr() : StarExpressions();
                    return At(new Assign { Targets = new List<Expr> { first }, Value = value }, LocOf(start));
                }
                return At(new Pass(), LocOf(start));
            }

            // print "merhaba" → Python 2 alışkanlığı
            if (!IsStatementEnd() && first is Name n && (n.Id == "print" || n.Id == "exec"))
            {
                if (!parenthesized.Contains(first) && StartsExpression(t) && Attempt(() => { StarExpressions(); return true; }))
                    throw Error("Missing parentheses in call to '" + n.Id + "'. Did you mean " + n.Id + "(...)?", first.Line);
            }
            return At(new ExprStmt { Value = first }, LocOf(start));
        }

        /// <summary>
        /// CPython invalid_named_expression kuralı: `hedef = değer` biçiminde, hedef atanamaz bir
        /// ifadeyse "cannot assign to X here. Maybe you meant '==' instead of '='?" der.
        /// eqPos: '=' işaretinden sonraki parçanın yeri.
        /// </summary>
        bool AssignHereRule(Expr target, int eqPos)
        {
            if (!IsBitwiseOrLevel(target, parenthesized)) return false;
            if (target is ListExpr || target is TupleExpr) return false;
            if (target is Constant c && (c.Value == null || c.Value is bool)) return false;
            var firstToken = tokens.Find(tok => tok.Line == target.Line && tok.Col == target.Col);
            if (firstToken != null && (firstToken.Value == "[" || firstToken.Value == "True" || firstToken.Value == "False" || firstToken.Value == "None"))
                return false;

            int saved = pos;
            pos = eqPos;
            try
            {
                return Attempt(() =>
                {
                    BitwiseOr();
                    return !IsOp("=") && !IsOp(":=");
                });
            }
            finally
            {
                pos = saved;
            }
        }

        void HeaderColon()
        {
            if (AcceptOp(":") != null) return;
            if (Peek().Type == TokenType.NEWLINE) throw Error("expected ':'", Peek().Line);
            throw Fail();
        }

        List<Stmt> Block(string header, int headerLine)
        {
            if (Peek().Type != TokenType.NEWLINE) return SimpleStatements();
            Next();
            if (Peek().Type != TokenType.INDENT)
            {
                throw new PythonException(
                    "IndentationError",
                    "expected an indented block after " + header + " on line " + headerLine,
                    Peek().Line);
            }
            Next();
            var body = new List<Stmt>();
            while (Peek().Type != TokenType.DEDENT) body.AddRange(Statement());
            Next();
            return body;
        }

        List<Stmt> ElseBlock()
        {
            var elseToken = Next();
            ExpectForced(":");
            return Block("'else' statement", elseToken.Line);
        }

        Stmt IfStatement()
        {
            var start = Next(); // 'if' ya da 'elif'
            var test = NamedExpression();
            HeaderColon();
            var loc = LocOf(start);
            var body = Block("'" + start.Value + "' statement", start.Line);
            var orelse = new List<Stmt>();
            if (IsKeyword("elif")) orelse = new List<Stmt> { IfStatement() };
            else if (IsKeyword("else")) orelse = ElseBlock();
            return At(new If { Test = test, Body = body, OrElse = orelse }, loc);
        }

        Stmt WhileStatement()
        {
            var start = Next();
            var test = NamedExpression();
            HeaderColon();
            var loc = LocOf(start);
            var body = Block("'while' statement", start.Line);
            var orelse = IsKeyword("else") ? ElseBlock() : new List<Stmt>();
            return At(new While { Test = test, Body = body, OrElse = orelse }, loc);
        }

        Stmt ForStatement()
        {
            var start = Next();
            var target = TargetList();
            var invalid = InvalidTarget(target, true);
            if (invalid != null) throw Error("cannot assign to " + ExprName(invalid), invalid.Line);
            if (!IsKeyword("in")) throw Fail();
            Next();
            var iter = StarExpressions();
            HeaderColon();
            var loc = LocOf(start);
            var body = Block("'for' statement", start.Line);
            var orelse = IsKeyword("else") ? ElseBlock() : new List<Stmt>();
            return At(new For { Target = target, Iter = iter, Body = body, OrElse = orelse }, loc);
        }

        /// <summary>for döngüsünün hedefi: `in`'i karşılaştırma sanmamak için karşılaştırmanın altındaki seviyede okunur.</summary>
        Expr TargetList()
        {
            var first = BitwiseOr();
            if (!IsOp(",")) return first;
            var elts = new List<Expr> { first };
            while (AcceptOp(",") != null)
            {
                if (IsKeyword("in") || !StartsExpression(Peek())) break;
                elts.Add(BitwiseOr());
            }
            return At(new TupleExpr { Elts = elts }, Span(first, Prev()));
        }

        Stmt FunctionDefinition()
        {
            var start = Next();
            var name = ParseName();
            ExpectForced("(");
            var parameters = new List<Param>();
            bool sawDefault = false;
            while (!IsOp(")"))
            {
                var t = Peek();
                if (t.Type == TokenType.OP && (t.Value == "*" || t.Value == "**" || t.Value == "/"))
                {
                    Note(Feature.StarParams, t.Line);
                    Next();
                    if (t.Value != "/" && Peek().Type == TokenType.NAME)
                    {
                        ParseName();
                        SkipAnnotation();
                    }
                }
                else
                {
                    var paramName = ParseName();
                    SkipAnnotation();
                    Expr defaultValue = null;
                    if (AcceptOp("=") != null)
                    {
                        defaultValue = Expression();
                        sawDefault = true;
                    }
                    else if (sawDefault)
                    {
                        throw Error("parameter without a default follows parameter with a default", t.Line);
                    }
                    parameters.Add(At(new Param { Name = paramName, Default = defaultValue }, LocOf(t)));
                }
                if (AcceptOp(",") == null) break;
            }
            ExpectOp(")");
            if (IsOp("->"))
            {
                Note(Feature.Annotation, Next().Line);
                Expression();
            }
            ExpectForced(":");
            var loc = LocOf(start);
            var body = Block("function definition", start.Line);
            return At(new FunctionDef { Name = name, Params = parameters, Body = body }, loc);
        }

        // --- ifadeler (öncelik sırası CPython dilbilgisiyle aynı) ---

        Expr StarExpressions()
        {
            var first = Expression();
            if (!IsOp(",")) return first;
            var elts = new List<Expr> { first };
            while (AcceptOp(",") != null)
            {
                if (!StartsExpression(Peek())) break;
                elts.Add(Expression());
            }
            return At(new TupleExpr { Elts = elts }, Span(first, Prev()));
        }

        /// <summary>Koşullarda ve parantez içi öğelerde kullanılır; yanlışlıkla yazılan '=' burada yakalanır.</summary>
        Expr NamedExpression()
        {
            var expr = Expression();
            if (IsOp(":="))
            {
                Note(Feature.Walrus, Next().Line);
                return Expression();
            }
            if (IsOp("="))
            {
                int eqPos = pos + 1;
                if (expr is Name && !parenthesized.Contains(expr))
                {
                    bool ok = Attempt(() =>
                    {
                        pos = eqPos;
                        BitwiseOr();
                        return !IsOp("=") && !IsOp(":=");
                    });
                    if (ok) throw Error("invalid syntax. Maybe you meant '==' or ':=' instead of '='?", expr.Line);
                }
                else if (AssignHereRule(expr, eqPos))
                {
                    throw Error("cannot assign to " + ExprName(expr) + " here. Maybe you meant '==' instead of '='?", expr.Line);
                }
            }
            return expr;
        }

        Expr Expression()
        {
            var t = Peek();
            if (t.Type == TokenType.NAME && t.Value == "lambda")
            {
                // lambda desteklenmez: parametreler atlanır, gövdenin yazımı denetlenir
                Note(Feature.Lambda, Next().Line);
                while (!IsOp(":"))
                {
                    if (Peek().Type == TokenType.NEWLINE) throw Fail();
                    Next();
                }
                Next();
                Expression();
                return At(new Constant { Value = null }, LocOf(t));
            }
            var body = Disjunction();
            if (!IsKeyword("if")) return body;
            Next();
            var test = Disjunction();
            if (!IsKeyword("else"))
            {
                if (!IsOp(":")) throw Error("expected 'else' after 'if' expression", body.Line);
                throw Fail();
            }
            Next();
            var orelse = Expression();
            return At(new IfExp { Test = test, Body = body, OrElse = orelse }, Span(body, orelse));
        }

        Expr Disjunction()
        {
            var first = Conjunction();
            if (!IsKeyword("or")) return first;
            var values = new List<Expr> { first };
            while (IsKeyword("or"))
            {
                Next();
                values.Add(Conjunction());
            }
            return At(new BoolOp { Op = "or", Values = values }, Span(first, values[values.Count - 1]));
        }

        Expr Conjunction()
        {
            var first = Inversion();
            if (!IsKeyword("and")) return first;
            var values = new List<Expr> { first };
            while (IsKeyword("and"))
            {
                Next();
                values.Add(Inversion());
            }
            return At(new BoolOp { Op = "and", Values = values }, Span(first, values[values.Count - 1]));
        }

        Expr Inversion()
        {
            if (IsKeyword("not"))
            {
                var start = Next();
                var operand = Inversion();
                return At(new UnaryOp { Op = "not", Operand = operand }, Span(start, operand));
            }
            return Comparison();
        }

        Expr Comparison()
        {
            var left = BitwiseOr();
            var ops = new List<string>();
            var comparators = new List<Expr>();
            while (true)
            {
                var t = Peek();
                string op = null;
                if (t.Type == TokenType.OP && CompareOps.Contains(t.Value))
                {
                    op = t.Value;
                    Next();
                }
                else if (t.Type == TokenType.NAME && t.Value == "in")
                {
                    op = "in";
                    Next();
                }
                else if (t.Type == TokenType.NAME && t.Value == "not" && IsKeyword("in", 1))
                {
                    op = "not in";
                    Next();
                    Next();
                }
                else if (t.Type == TokenType.NAME && t.Value == "is")
                {
                    Next();
                    if (IsKeyword("not"))
                    {
                        Next();
                        op = "is not";
                    }
                    else op = "is";
                }
                if (op == null) break;
                ops.Add(op);
                comparators.Add(BitwiseOr());
            }
            if (ops.Count == 0) return left;
            return At(new Compare { Left = left, Ops = ops, Comparators = comparators }, Span(left, comparators[comparators.Count - 1]));
        }

        Expr BinaryLevel(string[] ops, Func<Expr> operand)
        {
            var left = operand();
            while (true)
            {
                var t = Peek();
                if (t.Type != TokenType.OP || Array.IndexOf(ops, t.Value) < 0) return left;
                Next();
                var right = operand();
                left = At(new BinOp { Op = t.Value, Left = left, Right = right }, Span(left, right));
            }
        }

        static readonly string[] OrOps = { "|" };
        static readonly string[] XorOps = { "^" };
        static readonly string[] AndOps = { "&" };
        static readonly string[] ShiftOps = { "<<", ">>" };
        static readonly string[] SumOps = { "+", "-" };
        static readonly string[] TermOps = { "*", "/", "//", "%", "@" };

        Expr BitwiseOr() => BinaryLevel(OrOps, BitwiseXor);
        Expr BitwiseXor() => BinaryLevel(XorOps, BitwiseAnd);
        Expr BitwiseAnd() => BinaryLevel(AndOps, Shift);
        Expr Shift() => BinaryLevel(ShiftOps, Sum);
        Expr Sum() => BinaryLevel(SumOps, Term);
        Expr Term() => BinaryLevel(TermOps, Factor);

        Expr Factor()
        {
            var t = Peek();
            if (t.Type == TokenType.OP && (t.Value == "-" || t.Value == "+" || t.Value == "~"))
            {
                Next();
                var operand = Factor();
                return At(new UnaryOp { Op = t.Value, Operand = operand }, Span(t, operand));
            }
            return Power();
        }

        Expr Power()
        {
            var b = Primary();
            if (!IsOp("**")) return b;
            Next();
            var exponent = Factor();
            return At(new BinOp { Op = "**", Left = b, Right = exponent }, Span(b, exponent));
        }

        Expr Primary()
        {
            if (IsKeyword("await"))
            {
                Note(Feature.Await, Next().Line);
                return Primary();
            }
            var expr = Atom();
            while (true)
            {
                if (AcceptOp(".") != null)
                {
                    var attr = Peek();
                    if (attr.Type != TokenType.NAME || Keywords.Contains(attr.Value)) throw Fail();
                    Next();
                    expr = At(new Attribute { Value = expr, Attr = attr.Value }, Span(expr, attr));
                }
                else if (IsOp("("))
                {
                    expr = ParseCall(expr);
                }
                else if (IsOp("["))
                {
                    Next();
                    depth++;
                    var index = Slices();
                    ExpectOp("]");
                    depth--;
                    expr = At(new Subscript { Value = expr, Index = index }, Span(expr, Prev()));
                }
                else
                {
                    return expr;
                }
            }
        }

        Expr ParseCall(Expr func)
        {
            Next(); // (
            depth++;
            var args = new List<Expr>();
            var keywords = new List<Keyword>();
            while (!IsOp(")"))
            {
                var t = Peek();
                if (t.Type == TokenType.OP && (t.Value == "*" || t.Value == "**"))
                {
                    Note(Feature.StarArgs, Next().Line);
                    args.Add(Expression());
                }
                else if (t.Type == TokenType.NAME && IsOp("=", 1))
                {
                    if (t.Value == "True" || t.Value == "False" || t.Value == "None")
                        throw Error("cannot assign to " + t.Value, t.Line);
                    var name = ParseName();
                    Next(); // =
                    var value = Expression();
                    keywords.Add(At(new Keyword { Name = name, Value = value }, Span(t, value)));
                }
                else
                {
                    var arg = Expression();
                    if (IsOp(":="))
                    {
                        Note(Feature.Walrus, Next().Line);
                        arg = Expression();
                    }
                    if (IsKeyword("for")) ComprehensionTail();
                    if (IsOp("=")) throw Error("expression cannot contain assignment, perhaps you meant \"==\"?", arg.Line);
                    if (keywords.Count > 0) throw Error("positional argument follows keyword argument", Peek().Line);
                    args.Add(arg);
                    if (!IsOp(",") && !IsOp(")")) ForgotComma(arg);
                }
                if (AcceptOp(",") == null) break;
            }
            ExpectOp(")");
            depth--;
            return At(new Call { Func = func, Args = args, Keywords = keywords }, Span(func, Prev()));
        }

        Expr Slices()
        {
            var first = ParseSlice();
            if (!IsOp(",")) return first;
            var elts = new List<Expr> { first };
            while (AcceptOp(",") != null)
            {
                if (IsOp("]")) break;
                elts.Add(ParseSlice());
            }
            return At(new TupleExpr { Elts = elts }, Span(first, Prev()));
        }

        Expr ParseSlice()
        {
            var start = Peek();
            var lower = IsOp(":") ? null : NamedExpression();
            if (!IsOp(":")) return lower;
            Next();
            var upper = IsOp(":") || IsOp("]") || IsOp(",") ? null : Expression();
            Expr step = null;
            if (AcceptOp(":") != null) step = IsOp("]") || IsOp(",") ? null : Expression();
            return At(new Slice { Lower = lower, Upper = upper, Step = step }, LocOf(start));
        }

        /// <summary>CPython: parantez içinde yan yana iki ifade → "Perhaps you forgot a comma?"</summary>
        void ForgotComma(Expr a)
        {
            if (depth == 0 || a is IfExp) return;
            if (a is Name n && (n.Id == "print" || n.Id == "exec")) return;
            int first = tokens.FindIndex(tok => tok.Line == a.Line && tok.Col == a.Col);
            var firstToken = first >= 0 ? tokens[first] : null;
            if (firstToken != null && firstToken.Type == TokenType.NAME
                && ((first + 1 < tokens.Count && tokens[first + 1].Type == TokenType.STRING) || SoftKeywords.Contains(firstToken.Value)))
                return;
            if (!StartsExpression(Peek())) return;
            if (Attempt(() => { Expression(); return true; })) throw Error("invalid syntax. Perhaps you forgot a comma?", a.Line);
        }

        Expr Atom()
        {
            var t = Peek();
            switch (t.Type)
            {
                case TokenType.NAME:
                    if (t.Value == "yield") return YieldExpr();
                    Next();
                    if (t.Value == "True" || t.Value == "False") return At(new Constant { Value = t.Value == "True" }, LocOf(t));
                    if (t.Value == "None") return At(new Constant { Value = null }, LocOf(t));
                    if (Keywords.Contains(t.Value)) throw Fail();
                    return At(new Name { Id = t.Value }, LocOf(t));

                case TokenType.NUMBER:
                    Next();
                    return At(new Constant { Value = ParseNumber(t) }, LocOf(t));

                case TokenType.STRING:
                {
                    string value = "";
                    while (Peek().Type == TokenType.STRING)
                    {
                        var s = Next();
                        if (s.Prefix.Contains("f")) Note(Feature.FString, s.Line);
                        if (s.Prefix.Contains("b")) Note(Feature.Bytes, s.Line);
                        value += s.Value;
                    }
                    return At(new Constant { Value = value }, LocOf(t));
                }

                case TokenType.OP:
                    if (t.Value == "(") return ParenthesizedAtom();
                    if (t.Value == "[") return ListAtom();
                    if (t.Value == "{") return BraceAtom();
                    if (t.Value == "...")
                    {
                        Note(Feature.Ellipsis, Next().Line);
                        return At(new Constant { Value = null }, LocOf(t));
                    }
                    break;
            }
            throw Fail();
        }

        object ParseNumber(Token t)
        {
            string text = t.Value.Replace("_", "");
            if (t.NumberKind == NumberKind.Imaginary)
            {
                Note(Feature.Complex, t.Line);
                return 0.0;
            }
            if (t.NumberKind == NumberKind.Float) return Numbers.ParseDouble(text);
            if (text.Length > 1 && text[0] == '0' && char.IsLetter(text[1]))
            {
                char letter = char.ToLowerInvariant(text[1]);
                int radix = letter == 'x' ? 16 : letter == 'o' ? 8 : 2;
                return Numbers.ParseRadix(text.Substring(2), radix);
            }
            return BigInteger.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture);
        }

        /// <summary>Parantez içindeki öğeleri okur; kapanış yerine başka bir şey gelirse virgül ipucunu dener.</summary>
        List<Expr> Elements(string closer, Func<Expr> element, Expr first = null)
        {
            var elts = first != null ? new List<Expr> { first } : new List<Expr>();
            if (first != null)
            {
                if (!IsOp(",") && !IsOp(closer)) ForgotComma(first);
                if (AcceptOp(",") == null) return elts;
            }
            while (!IsOp(closer))
            {
                if (IsOp("*")) Note(Feature.StarArgs, Next().Line);
                var e = element();
                if (IsKeyword("for")) ComprehensionTail();
                elts.Add(e);
                if (!IsOp(",") && !IsOp(closer)) ForgotComma(e);
                if (AcceptOp(",") == null) break;
            }
            return elts;
        }

        Expr ParenthesizedAtom()
        {
            var start = Next();
            depth++;
            if (AcceptOp(")") != null)
            {
                depth--;
                return At(new TupleExpr { Elts = new List<Expr>() }, LocOf(start));
            }
            if (IsOp("*")) Note(Feature.StarArgs, Next().Line);
            var first = IsKeyword("yield") ? YieldExpr() : NamedExpression();
            if (IsKeyword("for")) ComprehensionTail();
            if (IsOp(","))
            {
                var elts = Elements(")", NamedExpression, first);
                ExpectOp(")");
                depth--;
                return At(new TupleExpr { Elts = elts }, LocOf(start));
            }
            if (!IsOp(")")) ForgotComma(first);
            ExpectOp(")");
            depth--;
            var inner = first.CopyWithLoc(LocOf(start));
            parenthesized.Add(inner);
            return inner;
        }

        Expr ListAtom()
        {
            var start = Next();
            depth++;
            var elts = Elements("]", NamedExpression);
            ExpectOp("]");
            depth--;
            return At(new ListExpr { Elts = elts }, LocOf(start));
        }

        Expr BraceAtom()
        {
            var start = Next();
            depth++;
            if (AcceptOp("}") != null)
            {
                depth--;
                return At(new DictExpr { Keys = new List<Expr>(), Values = new List<Expr>() }, LocOf(start));
            }
            if (IsOp("**"))
            {
                // {**a, ...}: desteklenmez; içerik atlanır
                Note(Feature.StarArgs, Peek().Line);
                SkipToCloser();
                depth--;
                return At(new DictExpr { Keys = new List<Expr>(), Values = new List<Expr>() }, LocOf(start));
            }
            if (IsOp("*")) Note(Feature.StarArgs, Next().Line);
            var first = NamedExpression();
            if (IsKeyword("for")) ComprehensionTail();

            if (AcceptOp(":") == null)
            {
                var elts = Elements("}", NamedExpression, first);
                ExpectOp("}");
                depth--;
                return At(new SetExpr { Elts = elts }, LocOf(start));
            }

            var keys = new List<Expr> { first };
            var values = new List<Expr> { Expression() };
            if (IsKeyword("for")) ComprehensionTail();
            while (AcceptOp(",") != null)
            {
                if (IsOp("}")) break;
                if (IsOp("**"))
                {
                    Note(Feature.StarArgs, Next().Line);
                    BitwiseOr();
                    continue;
                }
                keys.Add(Expression());
                ExpectOp(":");
                values.Add(Expression());
            }
            ExpectOp("}");
            depth--;
            return At(new DictExpr { Keys = keys, Values = values }, LocOf(start));
        }

        // --- atanabilirlik ve adlandırma (CPython pegen yardımcılarının karşılığı) ---

        /// <summary>Atanamayan ilk alt ifadeyi döndürür; hepsi atanabilirse null. (_PyPegen_get_invalid_target)</summary>
        static Expr InvalidTarget(Expr e, bool forTarget)
        {
            switch (e)
            {
                case Name _:
                case Attribute _:
                case Subscript _:
                    return null;
                case ListExpr l:
                    return FirstInvalid(l.Elts, forTarget);
                case TupleExpr t:
                    return FirstInvalid(t.Elts, forTarget);
                case Compare c:
                    // "for x in y" içindeki "x in y" karşılaştırma gibi okunmuş olabilir
                    if (forTarget && c.Ops[0] == "in") return InvalidTarget(c.Left, forTarget);
                    return e;
                default:
                    return e;
            }
        }

        static Expr FirstInvalid(List<Expr> elts, bool forTarget)
        {
            foreach (var elt in elts)
            {
                var invalid = InvalidTarget(elt, forTarget);
                if (invalid != null) return invalid;
            }
            return null;
        }

        /// <summary>Hata mesajlarındaki ifade adı (_PyPegen_get_expr_name)</summary>
        static string ExprName(Expr e)
        {
            switch (e)
            {
                case Attribute _: return "attribute";
                case Subscript _: return "subscript";
                case Name _: return "name";
                case ListExpr _: return "list";
                case TupleExpr _: return "tuple";
                case Call _: return "function call";
                case BoolOp _:
                case BinOp _:
                case UnaryOp _:
                    return "expression";
                case DictExpr _: return "dict literal";
                case SetExpr _: return "set display";
                case Compare _: return "comparison";
                case IfExp _: return "conditional expression";
                case Slice _: return "slice";
                case Constant c:
                    if (c.Value == null) return "None";
                    if (c.Value is bool b) return b ? "True" : "False";
                    return "literal";
            }
            throw new InvalidOperationException(e.GetType().Name);
        }

        /// <summary>İfade, CPython dilbilgisinde bitwise_or seviyesinde mi (karşılaştırma/mantık işlemi değil mi)?</summary>
        static bool IsBitwiseOrLevel(Expr e, HashSet<Expr> parenthesized)
        {
            if (parenthesized.Contains(e)) return true;
            switch (e)
            {
                case Compare _:
                case BoolOp _:
                case IfExp _:
                case TupleExpr _:
                    return false;
                case UnaryOp u:
                    return u.Op != "not";
                default:
                    return true;
            }
        }

        // --- derleme aşaması denetimleri ---

        static void CheckModule(Module module)
        {
            // 1) CPython'da önce sembol tablosu kurulur: yinelenen parametre adları
            ForEachStmt(module.Body, s =>
            {
                if (!(s is FunctionDef f)) return;
                var seen = new HashSet<string>();
                foreach (var p in f.Params)
                {
                    if (!seen.Add(p.Name))
                        throw new PythonException("SyntaxError", "duplicate argument '" + p.Name + "' in function definition", s.Line);
                }
            });

            // 2) Sonra derleyici: return/break/continue yerleri, yinelenen anahtar kelime argümanları
            CheckBody(module.Body, false, false);
        }

        static void CheckBody(List<Stmt> body, bool inFunction, bool inLoop)
        {
            foreach (var s in body)
            {
                ForEachExprOfStmt(s, e => ForEachExpr(e, CheckCall));
                switch (s)
                {
                    case Return _:
                        if (!inFunction) throw new PythonException("SyntaxError", "'return' outside function", s.Line);
                        break;
                    case Break _:
                        if (!inLoop) throw new PythonException("SyntaxError", "'break' outside loop", s.Line);
                        break;
                    case Continue _:
                        if (!inLoop) throw new PythonException("SyntaxError", "'continue' not properly in loop", s.Line);
                        break;
                    case If i:
                        CheckBody(i.Body, inFunction, inLoop);
                        CheckBody(i.OrElse, inFunction, inLoop);
                        break;
                    case While w:
                        CheckBody(w.Body, inFunction, true);
                        CheckBody(w.OrElse, inFunction, inLoop);
                        break;
                    case For f:
                        CheckBody(f.Body, inFunction, true);
                        CheckBody(f.OrElse, inFunction, inLoop);
                        break;
                    case FunctionDef d:
                        CheckBody(d.Body, true, false);
                        break;
                }
            }
        }

        static void CheckCall(Expr e)
        {
            if (!(e is Call c)) return;
            var seen = new HashSet<string>();
            foreach (var k in c.Keywords)
            {
                if (!seen.Add(k.Name)) throw new PythonException("SyntaxError", "keyword argument repeated: " + k.Name, k.Line);
            }
        }

        static void ForEachStmt(List<Stmt> body, Action<Stmt> fn)
        {
            foreach (var s in body)
            {
                fn(s);
                switch (s)
                {
                    case If i:
                        ForEachStmt(i.Body, fn);
                        ForEachStmt(i.OrElse, fn);
                        break;
                    case While w:
                        ForEachStmt(w.Body, fn);
                        ForEachStmt(w.OrElse, fn);
                        break;
                    case For f:
                        ForEachStmt(f.Body, fn);
                        ForEachStmt(f.OrElse, fn);
                        break;
                    case FunctionDef d:
                        ForEachStmt(d.Body, fn);
                        break;
                }
            }
        }

        /// <summary>Bir cümlenin doğrudan içerdiği ifadeler (alt bloklar hariç)</summary>
        static void ForEachExprOfStmt(Stmt s, Action<Expr> fn)
        {
            switch (s)
            {
                case ExprStmt e:
                    fn(e.Value);
                    break;
                case Assign a:
                    a.Targets.ForEach(fn);
                    fn(a.Value);
                    break;
                case AugAssign a:
                    fn(a.Target);
                    fn(a.Value);
                    break;
                case If i:
                    fn(i.Test);
                    break;
                case While w:
                    fn(w.Test);
                    break;
                case For f:
                    fn(f.Target);
                    fn(f.Iter);
                    break;
                case FunctionDef d:
                    foreach (var p in d.Params) if (p.Default != null) fn(p.Default);
                    break;
                case Return r:
                    if (r.Value != null) fn(r.Value);
                    break;
            }
        }

        /// <summary>İfadeyi ve bütün alt ifadelerini gezer.</summary>
        public static void ForEachExpr(Expr e, Action<Expr> fn)
        {
            fn(e);
            ForEachChild(e, child => ForEachExpr(child, fn));
        }

        /// <summary>İfadenin doğrudan alt ifadeleri</summary>
        public static void ForEachChild(Expr e, Action<Expr> fn)
        {
            switch (e)
            {
                case BinOp b:
                    fn(b.Left);
                    fn(b.Right);
                    break;
                case UnaryOp u:
                    fn(u.Operand);
                    break;
                case BoolOp b:
                    b.Values.ForEach(fn);
                    break;
                case Compare c:
                    fn(c.Left);
                    c.Comparators.ForEach(fn);
                    break;
                case IfExp i:
                    fn(i.Test);
                    fn(i.Body);
                    fn(i.OrElse);
                    break;
                case Call c:
                    fn(c.Func);
                    c.Args.ForEach(fn);
                    foreach (var k in c.Keywords) fn(k.Value);
                    break;
                case Attribute a:
                    fn(a.Value);
                    break;
                case Subscript s:
                    fn(s.Value);
                    fn(s.Index);
                    break;
                case Slice s:
                    if (s.Lower != null) fn(s.Lower);
                    if (s.Upper != null) fn(s.Upper);
                    if (s.Step != null) fn(s.Step);
                    break;
                case ListExpr l:
                    l.Elts.ForEach(fn);
                    break;
                case TupleExpr t:
                    t.Elts.ForEach(fn);
                    break;
                case SetExpr s:
                    s.Elts.ForEach(fn);
                    break;
                case DictExpr d:
                    d.Keys.ForEach(fn);
                    d.Values.ForEach(fn);
                    break;
            }
        }
    }
}
