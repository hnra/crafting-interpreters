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
    R VisitThisExpr(This expr);
    R VisitSuperExpr(Super expr);
    R VisitVecExpr(Vec expr);
}

public abstract class Expr
{
    public abstract R Accept<R>(ExprVisitor<R> visitor);
}

public class Assign : Expr
{
    public readonly Token name;
    public readonly Expr value;
    public Assign(Token name, Expr value)
    {
        this.name = name;
        this.value = value;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitAssignExpr(this);
}

public class Ternary : Expr
{
    public readonly Expr condition;
    public readonly Expr ifTrue;
    public readonly Expr ifFalse;
    public Ternary(Expr condition, Expr ifTrue, Expr ifFalse)
    {
        this.condition = condition;
        this.ifTrue = ifTrue;
        this.ifFalse = ifFalse;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitTernaryExpr(this);
}

public class Binary : Expr
{
    public readonly Expr left;
    public readonly Token op;
    public readonly Expr right;
    public Binary(Expr left, Token op, Expr right)
    {
        this.left = left;
        this.op = op;
        this.right = right;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitBinaryExpr(this);
}

public class Grouping : Expr
{
    public readonly Expr expression;
    public Grouping(Expr expression)
    {
        this.expression = expression;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitGroupingExpr(this);
}

public class Literal : Expr
{
    public readonly object? value;
    public Literal(object? value)
    {
        this.value = value;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitLiteralExpr(this);
}

public class Logical : Expr
{
    public readonly Expr left;
    public readonly Token op;
    public readonly Expr right;
    public Logical(Expr left, Token op, Expr right)
    {
        this.left = left;
        this.op = op;
        this.right = right;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitLogicalExpr(this);
}

public class Unary : Expr
{
    public readonly Token op;
    public readonly Expr right;
    public Unary(Token op, Expr right)
    {
        this.op = op;
        this.right = right;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitUnaryExpr(this);
}

public class Variable : Expr
{
    public readonly Token name;
    public Variable(Token name)
    {
        this.name = name;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitVariableExpr(this);
}

public class Call : Expr
{
    public readonly Expr callee;
    public readonly Token paren;
    public readonly List<Expr> arguments;
    public Call(Expr callee, Token paren, List<Expr> arguments)
    {
        this.callee = callee;
        this.paren = paren;
        this.arguments = arguments;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitCallExpr(this);
}

public class Get : Expr
{
    public readonly Expr obj;
    public readonly Token name;
    public Get(Expr obj, Token name)
    {
        this.obj = obj;
        this.name = name;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitGetExpr(this);
}

public class Set : Expr
{
    public readonly Expr obj;
    public readonly Token name;
    public readonly Expr value;
    public Set(Expr obj, Token name, Expr value)
    {
        this.obj = obj;
        this.name = name;
        this.value = value;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitSetExpr(this);
}

public class This : Expr
{
    public readonly Token keyword;
    public This(Token keyword)
    {
        this.keyword = keyword;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitThisExpr(this);
}

public class Super : Expr
{
    public readonly Token keyword;
    public readonly Token method;
    public Super(Token keyword, Token method)
    {
        this.keyword = keyword;
        this.method = method;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitSuperExpr(this);
}

public class Vec : Expr
{
    public readonly Token bracket;
    public readonly List<Expr> elements;
    public Vec(Token bracket, List<Expr> elements)
    {
        this.bracket = bracket;
        this.elements = elements;
    }
    public override R Accept<R>(ExprVisitor<R> visitor) => visitor.VisitVecExpr(this);
}
