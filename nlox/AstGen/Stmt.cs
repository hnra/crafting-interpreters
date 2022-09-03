namespace CraftingInterpreters.AstGen;

public interface StmtVisitor<R>
{
    R VisitExpressionStmt(Expression stmt);
    R VisitIfStmt(If stmt);
    R VisitPrintStmt(Print stmt);
    R VisitWhileStmt(While stmt);
    R VisitVarStmt(Var stmt);
    R VisitBlockStmt(Block stmt);
    R VisitFunctionStmt(Function stmt);
    R VisitReturnStmt(Return stmt);
}

public abstract record Stmt
{
    public abstract R Accept<R>(StmtVisitor<R> visitor);
}

public record Expression(Expr expression) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitExpressionStmt(this);
}

public record If(Expr condition, Stmt thenBranch, Stmt? elseBranch) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitIfStmt(this);
}

public record Print(Expr expression) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitPrintStmt(this);
}

public record While(Expr condition, Stmt body) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitWhileStmt(this);
}

public record Var(Token name, Expr? initializer) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitVarStmt(this);
}

public record Block(List<Stmt> statements) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitBlockStmt(this);
}

public record Function(Token name, List<Token> parameters, List<Stmt> body) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitFunctionStmt(this);
}

public record Return(Token keyword, Expr? value) : Stmt
{
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitReturnStmt(this);
}
