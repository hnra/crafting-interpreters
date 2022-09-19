namespace NLox.AstGen;

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
    R VisitClassStmt(Class stmt);
}

public abstract class Stmt
{
    public abstract R Accept<R>(StmtVisitor<R> visitor);
}

public class Expression : Stmt
{
    public readonly Expr expression;
    public Expression(Expr expression)
    {
        this.expression = expression;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitExpressionStmt(this);
}

public class If : Stmt
{
    public readonly Expr condition;
    public readonly Stmt thenBranch;
    public readonly Stmt? elseBranch;
    public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
    {
        this.condition = condition;
        this.thenBranch = thenBranch;
        this.elseBranch = elseBranch;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitIfStmt(this);
}

public class Print : Stmt
{
    public readonly Expr expression;
    public Print(Expr expression)
    {
        this.expression = expression;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitPrintStmt(this);
}

public class While : Stmt
{
    public readonly Expr condition;
    public readonly Stmt body;
    public While(Expr condition, Stmt body)
    {
        this.condition = condition;
        this.body = body;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitWhileStmt(this);
}

public class Var : Stmt
{
    public readonly Token name;
    public readonly Expr? initializer;
    public Var(Token name, Expr? initializer)
    {
        this.name = name;
        this.initializer = initializer;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitVarStmt(this);
}

public class Block : Stmt
{
    public readonly List<Stmt> statements;
    public Block(List<Stmt> statements)
    {
        this.statements = statements;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitBlockStmt(this);
}

public class Function : Stmt
{
    public readonly Token name;
    public readonly List<Token> parameters;
    public readonly List<Stmt> body;
    public Function(Token name, List<Token> parameters, List<Stmt> body)
    {
        this.name = name;
        this.parameters = parameters;
        this.body = body;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitFunctionStmt(this);
}

public class Return : Stmt
{
    public readonly Token keyword;
    public readonly Expr? value;
    public Return(Token keyword, Expr? value)
    {
        this.keyword = keyword;
        this.value = value;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitReturnStmt(this);
}

public class Class : Stmt
{
    public readonly Token name;
    public readonly Variable? superclass;
    public readonly List<Function> methods;
    public Class(Token name, Variable? superclass, List<Function> methods)
    {
        this.name = name;
        this.superclass = superclass;
        this.methods = methods;
    }
    public override R Accept<R>(StmtVisitor<R> visitor) => visitor.VisitClassStmt(this);
}
