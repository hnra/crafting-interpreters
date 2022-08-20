namespace CraftingInterpreters.AstGen;

public interface StmtVisitor<R>
{
    R VisitExpressionStmt(Expression stmt);
    R VisitPrintStmt(Print stmt);
    R VisitVarStmt(Var stmt);
}

public abstract record Stmt
{
    public abstract R Accept<R>(StmtVisitor<R> visitor);
}

public record Expression(Expr expression) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitExpressionStmt(this);
}

public record Print(Expr expression) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitPrintStmt(this);
}

public record Var(Token name, Expr? initializer) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitVarStmt(this);
}
