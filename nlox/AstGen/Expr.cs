namespace CraftingInterpreters.AstGen;

public interface Visitor<R>
{
    R VisitBinaryExpr(Binary expr);
    R VisitGroupingExpr(Grouping expr);
    R VisitLiteralExpr(Literal expr);
    R VisitUnaryExpr(Unary expr);
}

public abstract record Expr
{
    public abstract R Accept<R>(Visitor<R> visitor);
}

public record Binary(Expr left, Token op, Expr right) : Expr
{
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBinaryExpr(this);
}

public record Grouping(Expr expression) : Expr
{
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitGroupingExpr(this);
}

public record Literal(object? value) : Expr
{
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitLiteralExpr(this);
}

public record Unary(Token op, Expr right) : Expr
{
    public override R Accept<R>(Visitor<R> visitor) => visitor.VisitUnaryExpr(this);
}
