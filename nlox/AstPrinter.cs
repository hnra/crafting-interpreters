namespace CraftingInterpreters;

using AstGen;

public class AstPrinter : ExprVisitor<string>
{
    public string Print(Expr expr) => expr.Accept(this);

    public string VisitCallExpr(Call expr) =>
        throw new NotImplementedException();

    public string VisitLogicalExpr(Logical expr) =>
        Parenthesize(expr.op.lexeme, expr.left, expr.right);

    public string VisitAssignExpr(Assign expr) =>
        Parenthesize($"{expr.name}=", expr.value);

    public string VisitVariableExpr(Variable expr) =>
        expr.name.lexeme;

    public string VisitTernaryExpr(Ternary expr) =>
        $"(? {expr.condition.Accept(this)} {Parenthesize(":", expr.ifTrue, expr.ifFalse)})";

    public string VisitBinaryExpr(Binary expr) =>
        Parenthesize(expr.op.lexeme, expr.left, expr.right);

    public string VisitGroupingExpr(Grouping expr) =>
        Parenthesize("group", expr.expression);

    public string VisitLiteralExpr(Literal expr) =>
        expr.value?.ToString() ?? "";

    public string VisitUnaryExpr(Unary expr) =>
        Parenthesize(expr.op.lexeme, expr.right);

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var sw = new StringWriter();

        sw.Write($"({name}");
        foreach (var expr in exprs)
        {
            sw.Write($" {expr.Accept(this)}");
        }
        sw.Write(")");

        return sw.ToString();
    }
}
