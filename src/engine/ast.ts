// Kodun yapı ağacı (AST): cümle çözücünün çıktısı, çalıştırıcının girdisi.
// Adlar CPython'un ast modülüyle aynı tutuldu; böylece Python belgeleriyle karşılaştırmak kolay.

export interface Loc {
  /** 1'den başlar */
  line: number;
  /** 0'dan başlar */
  col: number;
  endLine: number;
  endCol: number;
}

export type Constant =
  | { type: 'int'; value: bigint }
  | { type: 'float'; value: number }
  | { type: 'str'; value: string }
  | { type: 'bool'; value: boolean }
  | { type: 'none' };

export type BinOperator = '+' | '-' | '*' | '/' | '//' | '%' | '**' | '@' | '<<' | '>>' | '&' | '|' | '^';
export type UnaryOperator = '-' | '+' | '~' | 'not';
export type CompareOperator = '==' | '!=' | '<' | '<=' | '>' | '>=' | 'in' | 'not in' | 'is' | 'is not';

export interface Keyword extends Loc {
  name: string;
  value: Expr;
}

export type Expr = Loc &
  (
    | { kind: 'Name'; id: string }
    | { kind: 'Constant'; value: Constant }
    | { kind: 'BinOp'; op: BinOperator; left: Expr; right: Expr }
    | { kind: 'UnaryOp'; op: UnaryOperator; operand: Expr }
    | { kind: 'BoolOp'; op: 'and' | 'or'; values: Expr[] }
    | { kind: 'Compare'; left: Expr; ops: CompareOperator[]; comparators: Expr[] }
    | { kind: 'IfExp'; test: Expr; body: Expr; orelse: Expr }
    | { kind: 'Call'; func: Expr; args: Expr[]; keywords: Keyword[] }
    | { kind: 'Attribute'; value: Expr; attr: string }
    | { kind: 'Subscript'; value: Expr; index: Expr }
    | { kind: 'Slice'; lower: Expr | null; upper: Expr | null; step: Expr | null }
    | { kind: 'List'; elts: Expr[] }
    | { kind: 'Tuple'; elts: Expr[] }
    | { kind: 'Dict'; keys: Expr[]; values: Expr[] }
    | { kind: 'Set'; elts: Expr[] }
  );

export interface Param extends Loc {
  name: string;
  default: Expr | null;
}

// Bileşik cümlelerde (if, for...) konum sadece başlık satırını kapsar; adım adım modunda
// "şu an çalışan satır" olarak bu gösterilir.
export type Stmt = Loc &
  (
    | { kind: 'Expr'; value: Expr }
    | { kind: 'Assign'; targets: Expr[]; value: Expr }
    | { kind: 'AugAssign'; target: Expr; op: BinOperator; value: Expr }
    | { kind: 'If'; test: Expr; body: Stmt[]; orelse: Stmt[] }
    | { kind: 'While'; test: Expr; body: Stmt[]; orelse: Stmt[] }
    | { kind: 'For'; target: Expr; iter: Expr; body: Stmt[]; orelse: Stmt[] }
    | { kind: 'FunctionDef'; name: string; params: Param[]; body: Stmt[] }
    | { kind: 'Return'; value: Expr | null }
    | { kind: 'Global'; names: string[] }
    | { kind: 'Nonlocal'; names: string[] }
    | { kind: 'Pass' }
    | { kind: 'Break' }
    | { kind: 'Continue' }
  );

export interface Module {
  kind: 'Module';
  body: Stmt[];
}

export type ExprOf<K extends Expr['kind']> = Extract<Expr, { kind: K }>;
export type StmtOf<K extends Stmt['kind']> = Extract<Stmt, { kind: K }>;
