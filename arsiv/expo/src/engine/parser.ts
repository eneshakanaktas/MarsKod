// Cümle çözücü: parçalardan (token) kodun yapı ağacını (AST) çıkarır.
//
// CPython 3.12'nin hata davranışı taklit edilir:
// - Genel hata "invalid syntax", okunan en ileri parçanın satırında bildirilir.
// - Sık yapılan hatalar için CPython'un özel mesajları (invalid_* kuralları) aynen üretilir.
// - Kelime ayırıcı hatası varsa çoğu durumda o öne geçer (bkz. parse()).
// - Ağaç kurulduktan sonra derleme aşaması hataları ('return' outside function...) denetlenir.

import type * as A from './ast';
import { PythonException, UnsupportedFeature, type UnsupportedFeatureId } from './errors';
import { tokenize, type Token } from './tokenizer';

const KEYWORDS = new Set([
  'False', 'None', 'True', 'and', 'as', 'assert', 'async', 'await', 'break', 'class', 'continue', 'def', 'del',
  'elif', 'else', 'except', 'finally', 'for', 'from', 'global', 'if', 'import', 'in', 'is', 'lambda', 'nonlocal',
  'not', 'or', 'pass', 'raise', 'return', 'try', 'while', 'with', 'yield',
]);

const SOFT_KEYWORDS = new Set(['_', 'case', 'match', 'type']);

const AUG_ASSIGN: Record<string, A.BinOperator> = {
  '+=': '+', '-=': '-', '*=': '*', '/=': '/', '//=': '//', '%=': '%', '**=': '**', '@=': '@',
  '<<=': '<<', '>>=': '>>', '&=': '&', '|=': '|', '^=': '^',
};

const COMPARE_OPS = new Set(['==', '!=', '<', '<=', '>', '>=']);

/** Kelime ayırıcının hata yüzünden kestiği yere ulaşıldı. */
class OutOfTokens extends Error {}

export function parse(source: string): A.Module {
  const { tokens, error: tokenError } = tokenize(source);
  const parser = new Parser(tokens);
  let module: A.Module;
  try {
    module = parser.parseModule();
  } catch (e) {
    if (e instanceof OutOfTokens) throw tokenError!;
    // CPython cümle hatasından sonra dosyanın geri kalanını da parçalar; orada bir kelime hatası
    // çıkarsa o öne geçer. Tek istisna kapanmamış parantez: sadece okumanın ulaştığı en ileri
    // satır, parantezin açıldığı satırdan sonraysa öne geçer.
    if (tokenError && e instanceof PythonException) {
      const unclosed = tokenError.pyMessage.endsWith('was never closed');
      if (!unclosed || parser.furthestLine() > (tokenError.line ?? 0)) throw tokenError;
    }
    throw e;
  }
  checkModule(module);
  // Python önce bütün dosyayı derler: yazım hataları desteklenmeyen özellikten önce bildirilir.
  if (parser.firstUnsupported) throw parser.firstUnsupported;
  return module;
}

class Parser {
  private pos = 0;
  /** Okunan en ileri parça: genel hatanın yeri */
  private furthest = 0;
  /** İç içe parantez derinliği ("Perhaps you forgot a comma?" sadece parantez içinde) */
  private depth = 0;
  /** Parantez içine alınmış ifadeler: (x > 3) = 5 gibi durumların ayrımı için */
  private readonly parenthesized = new WeakSet<A.Expr>();

  constructor(private readonly tokens: Token[]) {}

  furthestLine(): number {
    return this.tokens[this.furthest].line;
  }

  // --- parça okuma ---

  private peek(offset = 0): Token {
    const index = this.pos + offset;
    if (index >= this.tokens.length) throw new OutOfTokens();
    if (index > this.furthest) this.furthest = index;
    return this.tokens[index];
  }

  private next(): Token {
    const token = this.peek();
    this.pos++;
    return token;
  }

  private prev(): Token {
    return this.tokens[this.pos - 1];
  }

  private isOp(value: string, offset = 0): boolean {
    const t = this.peek(offset);
    return t.type === 'OP' && t.value === value;
  }

  private isKeyword(value: string, offset = 0): boolean {
    const t = this.peek(offset);
    return t.type === 'NAME' && t.value === value;
  }

  private acceptOp(value: string): Token | null {
    return this.isOp(value) ? this.next() : null;
  }

  private expectOp(value: string): Token {
    if (!this.isOp(value)) this.fail();
    return this.next();
  }

  /** CPython'un "zorunlu parça" hatası: 'else' ve 'def' sonrası ':' gibi */
  private expectForced(value: string): Token {
    if (!this.isOp(value)) this.error(`expected '${value}'`, this.peek().line);
    return this.next();
  }

  private isStatementEnd(): boolean {
    const t = this.peek();
    return t.type === 'NEWLINE' || (t.type === 'OP' && t.value === ';');
  }

  private startsExpression(t: Token): boolean {
    if (t.type === 'NUMBER' || t.type === 'STRING') return true;
    if (t.type === 'NAME') {
      return !KEYWORDS.has(t.value) || ['True', 'False', 'None', 'not', 'lambda', 'await', 'yield'].includes(t.value);
    }
    return t.type === 'OP' && ['(', '[', '{', '-', '+', '~', '*', '...'].includes(t.value);
  }

  // --- hata ---

  private fail(): never {
    const t = this.tokens[this.furthest];
    if (t.type === 'INDENT') throw new PythonException('IndentationError', 'unexpected indent', t.line);
    if (t.type === 'DEDENT') throw new PythonException('IndentationError', 'unexpected unindent', t.line);
    throw new PythonException('SyntaxError', 'invalid syntax', t.line);
  }

  private error(message: string, line: number): never {
    throw new PythonException('SyntaxError', message, line);
  }

  /** İlk desteklenmeyen özellik. Okuma durmaz; dosya sonunda bildirilir. */
  firstUnsupported: UnsupportedFeature | null = null;

  private note(feature: UnsupportedFeatureId, line: number): void {
    this.firstUnsupported ??= new UnsupportedFeature(feature, line);
  }

  /** Satır sonuna kadar atlar ve satır sonunu tüketir. */
  private skipLine(): void {
    while (this.peek().type !== 'NEWLINE') this.next();
    this.next();
  }

  /** Desteklenmeyen bileşik cümleyi (class, try, with...) başlığı ve girintili bloklarıyla atlar. */
  private skipCompound(clauses: string[] = []): void {
    do {
      this.skipLine();
      if (this.peek().type === 'INDENT') {
        let depth = 0;
        do {
          const t = this.next();
          if (t.type === 'INDENT') depth++;
          else if (t.type === 'DEDENT') depth--;
        } while (depth > 0);
      }
    } while (clauses.some((c) => this.isKeyword(c)));
  }

  /** Basit cümlenin sonuna (satır sonu ya da ';') kadar atlar. */
  private skipSimpleStatement(): void {
    let depth = 0;
    while (true) {
      const t = this.peek();
      if (t.type === 'NEWLINE' || (depth === 0 && t.type === 'OP' && t.value === ';')) return;
      if (t.type === 'OP' && '([{'.includes(t.value)) depth++;
      if (t.type === 'OP' && ')]}'.includes(t.value)) depth--;
      this.next();
    }
  }

  /** Açılmış bir parantezin kapanışına kadar atlar ve kapanışı tüketir. */
  private skipToCloser(): void {
    let depth = 1;
    while (depth > 0) {
      const t = this.next();
      if (t.type === 'OP' && '([{'.includes(t.value)) depth++;
      if (t.type === 'OP' && ')]}'.includes(t.value)) depth--;
    }
  }

  /** yield ifadesi: not alınır, yerine None konur. */
  private yieldExpr(): A.Expr {
    const start = this.next();
    this.note('yield', start.line);
    if (this.isKeyword('from')) this.next();
    if (!this.isStatementEnd() && !this.isOp(')') && this.startsExpression(this.peek())) this.starExpressions();
    return { kind: 'Constant', value: { type: 'none' }, ...this.loc(start) };
  }

  /** Liste üreteci kuyruğu: for x in y if z ... (desteklenmez ama yazım denetlenir) */
  private comprehensionTail(): void {
    this.note('comprehension', this.peek().line);
    while (this.isKeyword('for') || this.isKeyword('async')) {
      if (this.isKeyword('async')) this.next();
      this.next();
      this.targetList();
      if (!this.isKeyword('in')) this.fail();
      this.next();
      this.disjunction();
      while (this.isKeyword('if')) {
        this.next();
        this.disjunction();
      }
    }
  }

  /** Tür ipucu (x: int) desteklenmez ama yazımı denetlenir. */
  private skipAnnotation(): void {
    if (!this.isOp(':')) return;
    this.note('annotation', this.next().line);
    this.expression();
  }

  /** Bir şeyi denemek için okur; başarısızsa hiçbir şey olmamış gibi geri döner. */
  private attempt<T>(fn: () => T): T | null {
    const pos = this.pos;
    try {
      return fn();
    } catch (e) {
      if (e instanceof OutOfTokens) throw e;
      return null;
    } finally {
      this.pos = pos;
    }
  }

  private loc(start: { line: number; col: number }): A.Loc {
    const end = this.prev();
    return { line: start.line, col: start.col, endLine: end.endLine, endCol: end.endCol };
  }

  private span(start: A.Loc, end: A.Loc): A.Loc {
    return { line: start.line, col: start.col, endLine: end.endLine, endCol: end.endCol };
  }

  // --- cümleler ---

  parseModule(): A.Module {
    const body: A.Stmt[] = [];
    while (this.peek().type !== 'ENDMARKER') body.push(...this.statement());
    return { kind: 'Module', body };
  }

  private statement(): A.Stmt[] {
    const t = this.peek();
    if (t.type === 'NAME') {
      switch (t.value) {
        case 'if':
          return [this.ifStatement()];
        case 'while':
          return [this.whileStatement()];
        case 'for':
          return [this.forStatement()];
        case 'def':
          return [this.functionDef()];
        case 'class':
        case 'try':
        case 'with':
        case 'async':
          this.note(t.value, t.line);
          this.skipCompound(t.value === 'try' ? ['except', 'else', 'finally'] : []);
          return [];
      }
    }
    if (t.type === 'OP' && t.value === '@') {
      // Dekoratör satırı atlanır; altındaki def normal okunur.
      this.note('decorator', t.line);
      this.skipLine();
      return [];
    }
    return this.simpleStatements();
  }

  private simpleStatements(): A.Stmt[] {
    const stmts = [this.simpleStatement()];
    while (this.acceptOp(';')) {
      if (this.peek().type === 'NEWLINE') break;
      stmts.push(this.simpleStatement());
    }
    if (this.peek().type !== 'NEWLINE') this.fail();
    this.next();
    return stmts;
  }

  private simpleStatement(): A.Stmt {
    const t = this.peek();
    if (t.type === 'NAME') {
      switch (t.value) {
        case 'pass':
          this.next();
          return { kind: 'Pass', ...this.loc(t) };
        case 'break':
          this.next();
          return { kind: 'Break', ...this.loc(t) };
        case 'continue':
          this.next();
          return { kind: 'Continue', ...this.loc(t) };
        case 'return': {
          this.next();
          const value = this.isStatementEnd() ? null : this.starExpressions();
          return { kind: 'Return', value, ...this.loc(t) };
        }
        case 'global':
        case 'nonlocal': {
          this.next();
          const names = [this.name()];
          while (this.acceptOp(',')) names.push(this.name());
          return { kind: t.value === 'global' ? 'Global' : 'Nonlocal', names, ...this.loc(t) };
        }
        case 'import':
        case 'from':
        case 'raise':
        case 'assert':
        case 'del':
        case 'yield':
          this.note(t.value === 'from' ? 'import' : t.value, t.line);
          this.skipSimpleStatement();
          return { kind: 'Pass', ...this.loc(t) };
      }
    }
    return this.expressionStatement();
  }

  private name(): string {
    const t = this.peek();
    if (t.type !== 'NAME' || KEYWORDS.has(t.value)) this.fail();
    return this.next().value;
  }

  private expressionStatement(): A.Stmt {
    const start = this.peek();
    const first = this.starExpressions();

    if (this.isOp('=')) {
      const exprs = [first];
      const afterFirstEq = this.pos + 1;
      while (this.acceptOp('=')) {
        exprs.push(this.isKeyword('yield') ? this.yieldExpr() : this.starExpressions());
      }
      const value = exprs.pop()!;
      exprs.forEach((target, index) => {
        const invalid = invalidTarget(target, 'star');
        if (!invalid) return;
        if (index === 0 && this.assignHereRule(target, afterFirstEq)) {
          this.error(`cannot assign to ${exprName(target)} here. Maybe you meant '==' instead of '='?`, target.line);
        }
        this.error(`cannot assign to ${exprName(invalid)}`, invalid.line);
      });
      return { kind: 'Assign', targets: exprs, value, ...this.loc(start) };
    }

    const t = this.peek();
    if (t.type === 'OP' && t.value in AUG_ASSIGN) {
      if (first.kind !== 'Name' && first.kind !== 'Attribute' && first.kind !== 'Subscript') {
        this.error(`'${exprName(first)}' is an illegal expression for augmented assignment`, first.line);
      }
      this.next();
      const value = this.isKeyword('yield') ? this.yieldExpr() : this.starExpressions();
      return { kind: 'AugAssign', target: first, op: AUG_ASSIGN[t.value], value, ...this.loc(start) };
    }

    // x: int = 5 → tür ipucu desteklenmez; atama olarak okunur
    if (this.isOp(':')) {
      this.skipAnnotation();
      if (this.acceptOp('=')) {
        const value = this.isKeyword('yield') ? this.yieldExpr() : this.starExpressions();
        return { kind: 'Assign', targets: [first], value, ...this.loc(start) };
      }
      return { kind: 'Pass', ...this.loc(start) };
    }

    // print "merhaba" → Python 2 alışkanlığı
    if (!this.isStatementEnd() && first.kind === 'Name' && (first.id === 'print' || first.id === 'exec')) {
      if (!this.parenthesized.has(first) && this.startsExpression(t) && this.attempt(() => this.starExpressions())) {
        this.error(`Missing parentheses in call to '${first.id}'. Did you mean ${first.id}(...)?`, first.line);
      }
    }
    return { kind: 'Expr', value: first, ...this.loc(start) };
  }

  /**
   * CPython invalid_named_expression kuralı: `hedef = değer` biçiminde, hedef atanamaz bir
   * ifadeyse "cannot assign to X here. Maybe you meant '==' instead of '='?" der.
   * eqPos: '=' işaretinden sonraki parçanın yeri.
   */
  private assignHereRule(target: A.Expr, eqPos: number): boolean {
    if (!isBitwiseOrLevel(target, this.parenthesized)) return false;
    if (target.kind === 'List' || target.kind === 'Tuple') return false;
    if (target.kind === 'Constant' && (target.value.type === 'bool' || target.value.type === 'none')) return false;
    const firstToken = this.tokens.find((tok) => tok.line === target.line && tok.col === target.col);
    if (firstToken && (firstToken.value === '[' || ['True', 'False', 'None'].includes(firstToken.value))) return false;

    const saved = this.pos;
    this.pos = eqPos;
    try {
      const ok = this.attempt(() => {
        this.bitwiseOr();
        return !this.isOp('=') && !this.isOp(':=');
      });
      return ok === true;
    } finally {
      this.pos = saved;
    }
  }

  private headerColon(): void {
    if (this.acceptOp(':')) return;
    if (this.peek().type === 'NEWLINE') this.error("expected ':'", this.peek().line);
    this.fail();
  }

  private block(header: string, headerLine: number): A.Stmt[] {
    if (this.peek().type !== 'NEWLINE') return this.simpleStatements();
    this.next();
    if (this.peek().type !== 'INDENT') {
      throw new PythonException(
        'IndentationError',
        `expected an indented block after ${header} on line ${headerLine}`,
        this.peek().line,
      );
    }
    this.next();
    const body: A.Stmt[] = [];
    while (this.peek().type !== 'DEDENT') body.push(...this.statement());
    this.next();
    return body;
  }

  private elseBlock(): A.Stmt[] {
    const elseToken = this.next();
    this.expectForced(':');
    return this.block("'else' statement", elseToken.line);
  }

  private ifStatement(): A.Stmt {
    const start = this.next(); // 'if' ya da 'elif'
    const test = this.namedExpression();
    this.headerColon();
    const loc = this.loc(start);
    const body = this.block(`'${start.value}' statement`, start.line);
    let orelse: A.Stmt[] = [];
    if (this.isKeyword('elif')) orelse = [this.ifStatement()];
    else if (this.isKeyword('else')) orelse = this.elseBlock();
    return { kind: 'If', test, body, orelse, ...loc };
  }

  private whileStatement(): A.Stmt {
    const start = this.next();
    const test = this.namedExpression();
    this.headerColon();
    const loc = this.loc(start);
    const body = this.block("'while' statement", start.line);
    const orelse = this.isKeyword('else') ? this.elseBlock() : [];
    return { kind: 'While', test, body, orelse, ...loc };
  }

  private forStatement(): A.Stmt {
    const start = this.next();
    const target = this.targetList();
    const invalid = invalidTarget(target, 'for');
    if (invalid) this.error(`cannot assign to ${exprName(invalid)}`, invalid.line);
    if (!this.isKeyword('in')) this.fail();
    this.next();
    const iter = this.starExpressions();
    this.headerColon();
    const loc = this.loc(start);
    const body = this.block("'for' statement", start.line);
    const orelse = this.isKeyword('else') ? this.elseBlock() : [];
    return { kind: 'For', target, iter, body, orelse, ...loc };
  }

  /** for döngüsünün hedefi: `in`'i karşılaştırma sanmamak için karşılaştırmanın altındaki seviyede okunur. */
  private targetList(): A.Expr {
    const first = this.bitwiseOr();
    if (!this.isOp(',')) return first;
    const elts = [first];
    while (this.acceptOp(',')) {
      if (this.isKeyword('in') || !this.startsExpression(this.peek())) break;
      elts.push(this.bitwiseOr());
    }
    return { kind: 'Tuple', elts, ...this.span(first, this.prev()) };
  }

  private functionDef(): A.Stmt {
    const start = this.next();
    const name = this.name();
    this.expectForced('(');
    const params: A.Param[] = [];
    let sawDefault = false;
    while (!this.isOp(')')) {
      const t = this.peek();
      if (t.type === 'OP' && (t.value === '*' || t.value === '**' || t.value === '/')) {
        this.note('star-params', t.line);
        this.next();
        if (t.value !== '/' && this.peek().type === 'NAME') {
          this.name();
          this.skipAnnotation();
        }
      } else {
        const paramName = this.name();
        this.skipAnnotation();
        let defaultValue: A.Expr | null = null;
        if (this.acceptOp('=')) {
          defaultValue = this.expression();
          sawDefault = true;
        } else if (sawDefault) {
          this.error('parameter without a default follows parameter with a default', t.line);
        }
        params.push({ name: paramName, default: defaultValue, ...this.loc(t) });
      }
      if (!this.acceptOp(',')) break;
    }
    this.expectOp(')');
    if (this.isOp('->')) {
      this.note('annotation', this.next().line);
      this.expression();
    }
    this.expectForced(':');
    const loc = this.loc(start);
    const body = this.block('function definition', start.line);
    return { kind: 'FunctionDef', name, params, body, ...loc };
  }

  // --- ifadeler (öncelik sırası CPython dilbilgisiyle aynı) ---

  private starExpressions(): A.Expr {
    const first = this.expression();
    if (!this.isOp(',')) return first;
    const elts = [first];
    while (this.acceptOp(',')) {
      if (!this.startsExpression(this.peek())) break;
      elts.push(this.expression());
    }
    return { kind: 'Tuple', elts, ...this.span(first, this.prev()) };
  }

  /** Koşullarda ve parantez içi öğelerde kullanılır; yanlışlıkla yazılan '=' burada yakalanır. */
  private namedExpression(): A.Expr {
    const expr = this.expression();
    if (this.isOp(':=')) {
      this.note('walrus', this.next().line);
      return this.expression();
    }
    if (this.isOp('=')) {
      const eqPos = this.pos + 1;
      if (expr.kind === 'Name' && !this.parenthesized.has(expr)) {
        const ok = this.attempt(() => {
          this.pos = eqPos;
          this.bitwiseOr();
          return !this.isOp('=') && !this.isOp(':=');
        });
        if (ok) this.error("invalid syntax. Maybe you meant '==' or ':=' instead of '='?", expr.line);
      } else if (this.assignHereRule(expr, eqPos)) {
        this.error(`cannot assign to ${exprName(expr)} here. Maybe you meant '==' instead of '='?`, expr.line);
      }
    }
    return expr;
  }

  private expression(): A.Expr {
    const t = this.peek();
    if (t.type === 'NAME' && t.value === 'lambda') {
      // lambda desteklenmez: parametreler atlanır, gövdenin yazımı denetlenir
      this.note('lambda', this.next().line);
      while (!this.isOp(':')) {
        if (this.peek().type === 'NEWLINE') this.fail();
        this.next();
      }
      this.next();
      this.expression();
      return { kind: 'Constant', value: { type: 'none' }, ...this.loc(t) };
    }
    const body = this.disjunction();
    if (!this.isKeyword('if')) return body;
    this.next();
    const test = this.disjunction();
    if (!this.isKeyword('else')) {
      if (!this.isOp(':')) this.error("expected 'else' after 'if' expression", body.line);
      this.fail();
    }
    this.next();
    const orelse = this.expression();
    return { kind: 'IfExp', test, body, orelse, ...this.span(body, orelse) };
  }

  private disjunction(): A.Expr {
    const first = this.conjunction();
    if (!this.isKeyword('or')) return first;
    const values = [first];
    while (this.isKeyword('or')) {
      this.next();
      values.push(this.conjunction());
    }
    return { kind: 'BoolOp', op: 'or', values, ...this.span(first, values[values.length - 1]) };
  }

  private conjunction(): A.Expr {
    const first = this.inversion();
    if (!this.isKeyword('and')) return first;
    const values = [first];
    while (this.isKeyword('and')) {
      this.next();
      values.push(this.inversion());
    }
    return { kind: 'BoolOp', op: 'and', values, ...this.span(first, values[values.length - 1]) };
  }

  private inversion(): A.Expr {
    if (this.isKeyword('not')) {
      const start = this.next();
      const operand = this.inversion();
      return { kind: 'UnaryOp', op: 'not', operand, ...this.span(start, operand) };
    }
    return this.comparison();
  }

  private comparison(): A.Expr {
    const left = this.bitwiseOr();
    const ops: A.CompareOperator[] = [];
    const comparators: A.Expr[] = [];
    while (true) {
      const t = this.peek();
      let op: A.CompareOperator | null = null;
      if (t.type === 'OP' && COMPARE_OPS.has(t.value)) {
        op = t.value as A.CompareOperator;
        this.next();
      } else if (t.type === 'NAME' && t.value === 'in') {
        op = 'in';
        this.next();
      } else if (t.type === 'NAME' && t.value === 'not' && this.isKeyword('in', 1)) {
        op = 'not in';
        this.next();
        this.next();
      } else if (t.type === 'NAME' && t.value === 'is') {
        this.next();
        if (this.isKeyword('not')) {
          this.next();
          op = 'is not';
        } else op = 'is';
      }
      if (!op) break;
      ops.push(op);
      comparators.push(this.bitwiseOr());
    }
    if (ops.length === 0) return left;
    return { kind: 'Compare', left, ops, comparators, ...this.span(left, comparators[comparators.length - 1]) };
  }

  private binaryLevel(ops: string[], operand: () => A.Expr): A.Expr {
    let left = operand();
    while (true) {
      const t = this.peek();
      if (t.type !== 'OP' || !ops.includes(t.value)) return left;
      this.next();
      const right = operand();
      left = { kind: 'BinOp', op: t.value as A.BinOperator, left, right, ...this.span(left, right) };
    }
  }

  private bitwiseOr(): A.Expr {
    return this.binaryLevel(['|'], () => this.bitwiseXor());
  }

  private bitwiseXor(): A.Expr {
    return this.binaryLevel(['^'], () => this.bitwiseAnd());
  }

  private bitwiseAnd(): A.Expr {
    return this.binaryLevel(['&'], () => this.shift());
  }

  private shift(): A.Expr {
    return this.binaryLevel(['<<', '>>'], () => this.sum());
  }

  private sum(): A.Expr {
    return this.binaryLevel(['+', '-'], () => this.term());
  }

  private term(): A.Expr {
    return this.binaryLevel(['*', '/', '//', '%', '@'], () => this.factor());
  }

  private factor(): A.Expr {
    const t = this.peek();
    if (t.type === 'OP' && (t.value === '-' || t.value === '+' || t.value === '~')) {
      this.next();
      const operand = this.factor();
      return { kind: 'UnaryOp', op: t.value, operand, ...this.span(t, operand) };
    }
    return this.power();
  }

  private power(): A.Expr {
    const base = this.primary();
    if (!this.isOp('**')) return base;
    this.next();
    const exponent = this.factor();
    return { kind: 'BinOp', op: '**', left: base, right: exponent, ...this.span(base, exponent) };
  }

  private primary(): A.Expr {
    if (this.isKeyword('await')) {
      this.note('await', this.next().line);
      return this.primary();
    }
    let expr = this.atom();
    while (true) {
      if (this.acceptOp('.')) {
        const attr = this.peek();
        if (attr.type !== 'NAME' || KEYWORDS.has(attr.value)) this.fail();
        this.next();
        expr = { kind: 'Attribute', value: expr, attr: attr.value, ...this.span(expr, attr) };
      } else if (this.isOp('(')) {
        expr = this.call(expr);
      } else if (this.isOp('[')) {
        this.next();
        this.depth++;
        const index = this.slices();
        this.expectOp(']');
        this.depth--;
        expr = { kind: 'Subscript', value: expr, index, ...this.span(expr, this.prev()) };
      } else {
        return expr;
      }
    }
  }

  private call(func: A.Expr): A.Expr {
    this.next(); // (
    this.depth++;
    const args: A.Expr[] = [];
    const keywords: A.Keyword[] = [];
    while (!this.isOp(')')) {
      const t = this.peek();
      if (t.type === 'OP' && (t.value === '*' || t.value === '**')) {
        this.note('star-args', this.next().line);
        args.push(this.expression());
      } else if (t.type === 'NAME' && this.isOp('=', 1)) {
        if (t.value === 'True' || t.value === 'False' || t.value === 'None') {
          this.error(`cannot assign to ${t.value}`, t.line);
        }
        const name = this.name();
        this.next(); // =
        const value = this.expression();
        keywords.push({ name, value, ...this.span(t, value) });
      } else {
        let arg = this.expression();
        if (this.isOp(':=')) {
          this.note('walrus', this.next().line);
          arg = this.expression();
        }
        if (this.isKeyword('for')) this.comprehensionTail();
        if (this.isOp('=')) this.error('expression cannot contain assignment, perhaps you meant "=="?', arg.line);
        if (keywords.length > 0) this.error('positional argument follows keyword argument', this.peek().line);
        args.push(arg);
        if (!this.isOp(',') && !this.isOp(')')) this.forgotComma(arg);
      }
      if (!this.acceptOp(',')) break;
    }
    this.expectOp(')');
    this.depth--;
    return { kind: 'Call', func, args, keywords, ...this.span(func, this.prev()) };
  }

  private slices(): A.Expr {
    const first = this.slice();
    if (!this.isOp(',')) return first;
    const elts = [first];
    while (this.acceptOp(',')) {
      if (this.isOp(']')) break;
      elts.push(this.slice());
    }
    return { kind: 'Tuple', elts, ...this.span(first, this.prev()) };
  }

  private slice(): A.Expr {
    const start = this.peek();
    const lower = this.isOp(':') ? null : this.namedExpression();
    if (!this.isOp(':')) return lower!;
    this.next();
    const upper = this.isOp(':') || this.isOp(']') || this.isOp(',') ? null : this.expression();
    let step: A.Expr | null = null;
    if (this.acceptOp(':')) step = this.isOp(']') || this.isOp(',') ? null : this.expression();
    return { kind: 'Slice', lower, upper, step, ...this.loc(start) };
  }

  /** CPython: parantez içinde yan yana iki ifade → "Perhaps you forgot a comma?" */
  private forgotComma(a: A.Expr): void {
    if (this.depth === 0 || a.kind === 'IfExp') return;
    if (a.kind === 'Name' && (a.id === 'print' || a.id === 'exec')) return;
    const first = this.tokens.findIndex((tok) => tok.line === a.line && tok.col === a.col);
    const firstToken = this.tokens[first];
    if (firstToken?.type === 'NAME' && (this.tokens[first + 1]?.type === 'STRING' || SOFT_KEYWORDS.has(firstToken.value))) {
      return;
    }
    if (!this.startsExpression(this.peek())) return;
    if (this.attempt(() => this.expression())) this.error('invalid syntax. Perhaps you forgot a comma?', a.line);
  }

  private atom(): A.Expr {
    const t = this.peek();
    switch (t.type) {
      case 'NAME':
        if (t.value === 'yield') return this.yieldExpr();
        this.next();
        if (t.value === 'True' || t.value === 'False') {
          return { kind: 'Constant', value: { type: 'bool', value: t.value === 'True' }, ...this.loc(t) };
        }
        if (t.value === 'None') return { kind: 'Constant', value: { type: 'none' }, ...this.loc(t) };
        if (KEYWORDS.has(t.value)) this.fail();
        return { kind: 'Name', id: t.value, ...this.loc(t) };

      case 'NUMBER':
        this.next();
        return { kind: 'Constant', value: this.number(t), ...this.loc(t) };

      case 'STRING': {
        let value = '';
        while (this.peek().type === 'STRING') {
          const s = this.next();
          if (s.prefix!.includes('f')) this.note('f-string', s.line);
          if (s.prefix!.includes('b')) this.note('bytes', s.line);
          value += s.value;
        }
        return { kind: 'Constant', value: { type: 'str', value }, ...this.loc(t) };
      }

      case 'OP':
        if (t.value === '(') return this.parenthesizedAtom();
        if (t.value === '[') return this.listAtom();
        if (t.value === '{') return this.braceAtom();
        if (t.value === '...') {
          this.note('ellipsis', this.next().line);
          return { kind: 'Constant', value: { type: 'none' }, ...this.loc(t) };
        }
    }
    this.fail();
  }

  private number(t: Token): A.Constant {
    const text = t.value.replace(/_/g, '');
    if (t.numberKind === 'imaginary') {
      this.note('complex', t.line);
      return { type: 'float', value: 0 };
    }
    if (t.numberKind === 'float') return { type: 'float', value: Number(text) };
    return { type: 'int', value: BigInt(text) };
  }

  /** Parantez içindeki öğeleri okur; kapanış yerine başka bir şey gelirse virgül ipucunu dener. */
  private elements(closer: string, element: () => A.Expr, first?: A.Expr): A.Expr[] {
    const elts: A.Expr[] = first ? [first] : [];
    if (first) {
      if (!this.isOp(',') && !this.isOp(closer)) this.forgotComma(first);
      if (!this.acceptOp(',')) return elts;
    }
    while (!this.isOp(closer)) {
      if (this.isOp('*')) this.note('star-args', this.next().line);
      const e = element();
      if (this.isKeyword('for')) this.comprehensionTail();
      elts.push(e);
      if (!this.isOp(',') && !this.isOp(closer)) this.forgotComma(e);
      if (!this.acceptOp(',')) break;
    }
    return elts;
  }

  private parenthesizedAtom(): A.Expr {
    const start = this.next();
    this.depth++;
    if (this.acceptOp(')')) {
      this.depth--;
      return { kind: 'Tuple', elts: [], ...this.loc(start) };
    }
    if (this.isOp('*')) this.note('star-args', this.next().line);
    const first = this.isKeyword('yield') ? this.yieldExpr() : this.namedExpression();
    if (this.isKeyword('for')) this.comprehensionTail();
    if (this.isOp(',')) {
      const elts = this.elements(')', () => this.namedExpression(), first);
      this.expectOp(')');
      this.depth--;
      return { kind: 'Tuple', elts, ...this.loc(start) };
    }
    if (!this.isOp(')')) this.forgotComma(first);
    this.expectOp(')');
    this.depth--;
    const inner = { ...first, ...this.loc(start) } as A.Expr;
    this.parenthesized.add(inner);
    return inner;
  }

  private listAtom(): A.Expr {
    const start = this.next();
    this.depth++;
    const elts = this.elements(']', () => this.namedExpression());
    this.expectOp(']');
    this.depth--;
    return { kind: 'List', elts, ...this.loc(start) };
  }

  private braceAtom(): A.Expr {
    const start = this.next();
    this.depth++;
    if (this.acceptOp('}')) {
      this.depth--;
      return { kind: 'Dict', keys: [], values: [], ...this.loc(start) };
    }
    if (this.isOp('**')) {
      // {**a, ...}: desteklenmez; içerik atlanır
      this.note('star-args', this.peek().line);
      this.skipToCloser();
      this.depth--;
      return { kind: 'Dict', keys: [], values: [], ...this.loc(start) };
    }
    if (this.isOp('*')) this.note('star-args', this.next().line);
    const first = this.namedExpression();
    if (this.isKeyword('for')) this.comprehensionTail();

    if (!this.acceptOp(':')) {
      const elts = this.elements('}', () => this.namedExpression(), first);
      this.expectOp('}');
      this.depth--;
      return { kind: 'Set', elts, ...this.loc(start) };
    }

    const keys = [first];
    const values = [this.expression()];
    if (this.isKeyword('for')) this.comprehensionTail();
    while (this.acceptOp(',')) {
      if (this.isOp('}')) break;
      if (this.isOp('**')) {
        this.note('star-args', this.next().line);
        this.bitwiseOr();
        continue;
      }
      keys.push(this.expression());
      this.expectOp(':');
      values.push(this.expression());
    }
    this.expectOp('}');
    this.depth--;
    return { kind: 'Dict', keys, values, ...this.loc(start) };
  }
}

// --- atanabilirlik ve adlandırma (CPython pegen yardımcılarının karşılığı) ---

/** Atanamayan ilk alt ifadeyi döndürür; hepsi atanabilirse null. (_PyPegen_get_invalid_target) */
function invalidTarget(e: A.Expr, targets: 'star' | 'for'): A.Expr | null {
  switch (e.kind) {
    case 'Name':
    case 'Attribute':
    case 'Subscript':
      return null;
    case 'List':
    case 'Tuple':
      for (const elt of e.elts) {
        const invalid = invalidTarget(elt, targets);
        if (invalid) return invalid;
      }
      return null;
    case 'Compare':
      // "for x in y" içindeki "x in y" karşılaştırma gibi okunmuş olabilir
      if (targets === 'for' && e.ops[0] === 'in') return invalidTarget(e.left, targets);
      return e;
    default:
      return e;
  }
}

/** Hata mesajlarındaki ifade adı (_PyPegen_get_expr_name) */
function exprName(e: A.Expr): string {
  switch (e.kind) {
    case 'Attribute':
      return 'attribute';
    case 'Subscript':
      return 'subscript';
    case 'Name':
      return 'name';
    case 'List':
      return 'list';
    case 'Tuple':
      return 'tuple';
    case 'Call':
      return 'function call';
    case 'BoolOp':
    case 'BinOp':
    case 'UnaryOp':
      return 'expression';
    case 'Dict':
      return 'dict literal';
    case 'Set':
      return 'set display';
    case 'Compare':
      return 'comparison';
    case 'IfExp':
      return 'conditional expression';
    case 'Slice':
      return 'slice';
    case 'Constant':
      if (e.value.type === 'none') return 'None';
      if (e.value.type === 'bool') return e.value.value ? 'True' : 'False';
      return 'literal';
  }
}

/** İfade, CPython dilbilgisinde bitwise_or seviyesinde mi (karşılaştırma/mantık işlemi değil mi)? */
function isBitwiseOrLevel(e: A.Expr, parenthesized: WeakSet<A.Expr>): boolean {
  if (parenthesized.has(e)) return true;
  switch (e.kind) {
    case 'Compare':
    case 'BoolOp':
    case 'IfExp':
    case 'Tuple':
      return false;
    case 'UnaryOp':
      return e.op !== 'not';
    default:
      return true;
  }
}

// --- derleme aşaması denetimleri ---

function checkModule(module: A.Module): void {
  // 1) CPython'da önce sembol tablosu kurulur: yinelenen parametre adları
  forEachStmt(module.body, (s) => {
    if (s.kind !== 'FunctionDef') return;
    const seen = new Set<string>();
    for (const p of s.params) {
      if (seen.has(p.name)) {
        throw new PythonException('SyntaxError', `duplicate argument '${p.name}' in function definition`, s.line);
      }
      seen.add(p.name);
    }
  });

  // 2) Sonra derleyici: return/break/continue yerleri, yinelenen anahtar kelime argümanları
  checkBody(module.body, { inFunction: false, inLoop: false });
}

function checkBody(body: A.Stmt[], ctx: { inFunction: boolean; inLoop: boolean }): void {
  for (const s of body) {
    forEachExprOfStmt(s, (e) => forEachExpr(e, checkCall));
    switch (s.kind) {
      case 'Return':
        if (!ctx.inFunction) throw new PythonException('SyntaxError', "'return' outside function", s.line);
        break;
      case 'Break':
        if (!ctx.inLoop) throw new PythonException('SyntaxError', "'break' outside loop", s.line);
        break;
      case 'Continue':
        if (!ctx.inLoop) throw new PythonException('SyntaxError', "'continue' not properly in loop", s.line);
        break;
      case 'If':
        checkBody(s.body, ctx);
        checkBody(s.orelse, ctx);
        break;
      case 'While':
      case 'For':
        checkBody(s.body, { ...ctx, inLoop: true });
        checkBody(s.orelse, ctx);
        break;
      case 'FunctionDef':
        checkBody(s.body, { inFunction: true, inLoop: false });
        break;
    }
  }
}

function checkCall(e: A.Expr): void {
  if (e.kind !== 'Call') return;
  const seen = new Set<string>();
  for (const k of e.keywords) {
    if (seen.has(k.name)) throw new PythonException('SyntaxError', `keyword argument repeated: ${k.name}`, k.line);
    seen.add(k.name);
  }
}

function forEachStmt(body: A.Stmt[], fn: (s: A.Stmt) => void): void {
  for (const s of body) {
    fn(s);
    if (s.kind === 'If' || s.kind === 'While' || s.kind === 'For') {
      forEachStmt(s.body, fn);
      forEachStmt(s.orelse, fn);
    } else if (s.kind === 'FunctionDef') {
      forEachStmt(s.body, fn);
    }
  }
}

/** Bir cümlenin doğrudan içerdiği ifadeler (alt bloklar hariç) */
function forEachExprOfStmt(s: A.Stmt, fn: (e: A.Expr) => void): void {
  switch (s.kind) {
    case 'Expr':
      fn(s.value);
      break;
    case 'Assign':
      s.targets.forEach(fn);
      fn(s.value);
      break;
    case 'AugAssign':
      fn(s.target);
      fn(s.value);
      break;
    case 'If':
    case 'While':
      fn(s.test);
      break;
    case 'For':
      fn(s.target);
      fn(s.iter);
      break;
    case 'FunctionDef':
      s.params.forEach((p) => p.default && fn(p.default));
      break;
    case 'Return':
      if (s.value) fn(s.value);
      break;
  }
}

export function forEachExpr(e: A.Expr, fn: (e: A.Expr) => void): void {
  fn(e);
  const visit = (child: A.Expr | null) => child && forEachExpr(child, fn);
  switch (e.kind) {
    case 'BinOp':
      visit(e.left);
      visit(e.right);
      break;
    case 'UnaryOp':
      visit(e.operand);
      break;
    case 'BoolOp':
      e.values.forEach(visit);
      break;
    case 'Compare':
      visit(e.left);
      e.comparators.forEach(visit);
      break;
    case 'IfExp':
      visit(e.test);
      visit(e.body);
      visit(e.orelse);
      break;
    case 'Call':
      visit(e.func);
      e.args.forEach(visit);
      e.keywords.forEach((k) => visit(k.value));
      break;
    case 'Attribute':
      visit(e.value);
      break;
    case 'Subscript':
      visit(e.value);
      visit(e.index);
      break;
    case 'Slice':
      visit(e.lower);
      visit(e.upper);
      visit(e.step);
      break;
    case 'List':
    case 'Tuple':
    case 'Set':
      e.elts.forEach(visit);
      break;
    case 'Dict':
      e.keys.forEach(visit);
      e.values.forEach(visit);
      break;
  }
}
