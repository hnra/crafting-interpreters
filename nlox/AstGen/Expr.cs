namespace CraftingInterpreters.AstGen;

public interface ExprVisitor<R>
{
    R VisitTernaryExpr(Ternary expr);
    R VisitBinaryExpr(Binary expr);
    R VisitGroupingExpr(Grouping expr);
    R VisitLiteralExpr(Literal expr);
    R VisitUnaryExpr(Unary expr);
    R VisitVariableExpr(Variable expr);
}

public abstract record Expr
{
    public abstract R Accept<R>(ExprVisitor<R> visitor);
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

public record Unary(Token op, Expr right) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitUnaryExpr(this);
}

public record Variable(Token name) : Expr
{
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitVariableExpr(this);
}
