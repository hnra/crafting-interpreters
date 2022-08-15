namespace CraftingInterpreters;

using AstGen;

public class AstPrinter : Visitor<string>
{
    public string Print(Expr expr) => expr.Accept(this);

    public string VisitBinaryExpr(Binary expr) =>
        this.Parenthesize(expr.op.lexeme, expr.left, expr.right);

    public string VisitGroupingExpr(Grouping expr) =>
        this.Parenthesize("group", expr.expression);

    public string VisitLiteralExpr(Literal expr) =>
        expr.value?.ToString() ?? "";

    public string VisitUnaryExpr(Unary expr) =>
        this.Parenthesize(expr.op.lexeme, expr.right);

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
