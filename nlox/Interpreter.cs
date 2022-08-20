namespace CraftingInterpreters;

using AstGen;

public class RuntimeException : Exception
{
    public readonly Token token;

    public RuntimeException(Token token, string message) : base(message)
    {
        this.token = token;
    }
}

public class Interpreter : ExprVisitor<object?>, StmtVisitor<object?>
{
    public string? Interpret(Expr expr)
    {
        try
        {
            var value = this.Evaluate(expr);
            return Stringify(value);
        }
        catch (RuntimeException e)
        {
            Lox.RuntimeError(e);
            return null;
        }
    }

    public void Interpret(List<Stmt> stmts)
    {
        try
        {
            foreach (var stmt in stmts)
            {
                this.Execute(stmt);
            }
        }
        catch (RuntimeException e)
        {
            Lox.RuntimeError(e);
        }
    }

    void Execute(Stmt stmt) => stmt.Accept(this);

    object? Evaluate(Expr expr) => expr.Accept(this);

    static string Stringify(object? obj) =>
        obj switch
        {
            null => "nil",
            bool b => b.ToString().ToLower(),
            double d => StringifyDouble(d),
            _ => obj.ToString() ?? ""
        };

    static string StringifyDouble(double d)
    {
        var text = d.ToString();

        if (text.EndsWith(".0"))
        {
            return text.Substring(0, text.Length - 2);
        }

        return text;
    }

    static bool IsTruthy(object? obj) =>
        obj switch
        {
            null => false,
            bool b => b,
            _ => true
        };

    static void CheckNumberOperand(Token op, object? operand)
    {
        if (operand is double)
        {
            return;
        }

        throw new RuntimeException(op, "Operand must be a number.");
    }

    static void CheckNumberOperands(Token op, object? left, object? right)
    {
        if (left is double && right is double)
        {
            return;
        }

        throw new RuntimeException(op, "Operands must be a numbers.");
    }

    static bool AreEqual(object a, object b)
    {
        if (a is null && b is null)
        {
            return true;
        }
        if (a is null)
        {
            return false;
        }
        return a.Equals(b);
    }

    public object? VisitVariableExpr(Variable expr)
    {
        throw new NotImplementedException();
    }

    public object? VisitVarStmt(Var stmt)
    {
        throw new NotImplementedException();
    }

    public object? VisitPrintStmt(Print stmt)
    {
        var val = this.Evaluate(stmt.expression);
        Console.WriteLine(Stringify(val));
        return null;
    }

    public object? VisitExpressionStmt(Expression stmt)
    {
        this.Evaluate(stmt.expression);
        return null;
    }

    public object? VisitLiteralExpr(Literal literal) => literal.value;

    public object? VisitTernaryExpr(Ternary ternary)
    {
        var condition = this.Evaluate(ternary.condition);

        if (IsTruthy(condition))
        {
            return this.Evaluate(ternary.ifTrue);
        }
        else
        {
            return this.Evaluate(ternary.ifFalse);
        }
    }

    public object? VisitGroupingExpr(Grouping grouping) => this.Evaluate(grouping.expression);

    public object? VisitUnaryExpr(Unary unary)
    {
        var right = this.Evaluate(unary.right);

        switch (unary.op.type)
        {
            case TokenType.MINUS:
                CheckNumberOperand(unary.op, right);
#pragma warning disable CS8605
                return -(double)right;
#pragma warning restore CS8605
            case TokenType.BANG:
                return !IsTruthy(right);
        }

        return null;
    }

    public object? VisitBinaryExpr(Binary binary)
    {
        var left = this.Evaluate(binary.left);
        var right = this.Evaluate(binary.right);

        switch (binary.op.type)
        {
            case TokenType.PLUS:
                if (left is double dl && right is double dr)
                {
                    return dl + dr;
                }
                if (left is string sl && right is string sr)
                {
                    return sl + sr;
                }
                throw new RuntimeException(binary.op, "Operands must be two numbers or two strings.");
#pragma warning disable CS8604, CS8605
            case TokenType.MINUS:
                CheckNumberOperands(binary.op, left, right);
                return (double)left - (double)right;
            case TokenType.STAR:
                CheckNumberOperands(binary.op, left, right);
                return (double)left * (double)right;
            case TokenType.SLASH:
                CheckNumberOperands(binary.op, left, right);
                return (double)left / (double)right;
            case TokenType.GREATER:
                CheckNumberOperands(binary.op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(binary.op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(binary.op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(binary.op, left, right);
                return (double)left <= (double)right;
            case TokenType.EQUAL_EQUAL:
                return AreEqual(left, right);
            case TokenType.BANG_EQUAL:
                return !AreEqual(left, right);
#pragma warning restore CS8604, CS8605
        }

        return null;
    }
}
