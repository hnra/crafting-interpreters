namespace CraftingInterpreters.AstGen;

public interface ExprVisitor<R>
{
    R VisitAssignExpr(Assign expr);
    R VisitTernaryExpr(Ternary expr);
    R VisitBinaryExpr(Binary expr);
    R VisitGroupingExpr(Grouping expr);
    R VisitLiteralExpr(Literal expr);
    R VisitLogicalExpr(Logical expr);
    R VisitUnaryExpr(Unary expr);
    R VisitVariableExpr(Variable expr);
    R VisitCallExpr(Call expr);
    R VisitGetExpr(Get expr);
    R VisitSetExpr(Set expr);
}

public abstract record Expr
{
    public abstract R Accept<R>(ExprVisitor<R> visitor);
}

public record Assign(Token name, Expr value) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitAssignExpr(this);
}

public record Ternary(Expr condition, Expr ifTrue, Expr ifFalse) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitTernaryExpr(this);
}

public record Binary(Expr left, Token op, Expr right) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitBinaryExpr(this);
}

public record Grouping(Expr expression) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitGroupingExpr(this);
}

public record Literal(object? value) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitLiteralExpr(this);
}

public record Logical(Expr left, Token op, Expr right) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitLogicalExpr(this);
}

public record Unary(Token op, Expr right) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitUnaryExpr(this);
}

public record Variable(Token name) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitVariableExpr(this);
}

public record Call(Expr callee, Token paren, List<Expr> arguments) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitCallExpr(this);
}

public record Get(Expr obj, Token name) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitGetExpr(this);
}

public record Set(Expr obj, Token name, Expr value) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitSetExpr(this);
}
