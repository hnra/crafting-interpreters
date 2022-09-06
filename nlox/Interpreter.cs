namespace CraftingInterpreters;

using AstGen;

public enum InterpreterMode
{
    Normal, Repl,
}

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
    readonly Environment globals = new();
    readonly Dictionary<Expr, int> locals = new();

    Environment environment;

    readonly Action<string> stdout;
    readonly InterpreterMode mode;
    readonly Action<RuntimeException> onError;

    public Interpreter(Action<string> stdout, InterpreterMode mode, Action<RuntimeException> onError)
    {
        this.stdout = stdout;
        this.mode = mode;
        this.onError = onError;
        this.environment = globals;

        this.globals.Define("clock", new Clock());
    }

    public string? Interpret(Expr expr)
    {
        try
        {
            var value = Evaluate(expr);
            return Stringify(value);
        }
        catch (RuntimeException e)
        {
            onError(e);
            return null;
        }
    }

    public void Interpret(List<Stmt> stmts)
    {
        try
        {
            foreach (var stmt in stmts)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeException e)
        {
            onError(e);
        }
    }

    void Execute(Stmt stmt) => stmt.Accept(this);

    object? Evaluate(Expr expr) => expr.Accept(this);

    public void Resolve(Expr expr, int depth)
    {
        locals[expr] = depth;
    }

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

        throw new RuntimeException(op, $"Operands must be a numbers.");
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

    public object? VisitReturnStmt(Return stmt)
    {
        var value = stmt.value == null ? null : Evaluate(stmt.value);
        throw new ReturnException { Value = value };
    }

    public object? VisitFunctionStmt(Function stmt)
    {
        var func = new LoxFunction(stmt, environment);
        environment.Define(stmt.name.lexeme, func);
        return null;
    }

    public object? VisitWhileStmt(While stmt)
    {
        while (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.body);
        }
        return null;
    }

    public object? VisitIfStmt(If stmt)
    {
        if (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.thenBranch);
        }
        else if (stmt.elseBranch != null)
        {
            Execute(stmt.elseBranch);
        }
        return null;
    }

    public object? VisitBlockStmt(Block stmt)
    {
        ExecuteBlock(stmt.statements, new Environment(environment));
        return null;
    }

    public void ExecuteBlock(List<Stmt> stmts, Environment environment)
    {
        var prevEnv = this.environment;
        try
        {
            this.environment = environment;
            foreach (var stmt in stmts)
            {
                Execute(stmt);
            }
        }
        finally
        {
            this.environment = prevEnv;
        }
    }

    public object? VisitCallExpr(Call expr)
    {
        var callee = Evaluate(expr.callee);
        var arguments = new List<object?>();
        foreach (var arg in expr.arguments)
        {
            arguments.Add(Evaluate(arg));
        }
        if (callee is LoxCallable callable)
        {
            if (callable.Arity() != arguments.Count)
            {
                throw new RuntimeException(expr.paren, $"Expected {callable.Arity()} arguments but got {arguments.Count}.");
            }
            return callable.Call(this, arguments);
        }
        else
        {
            throw new RuntimeException(expr.paren, "Can only call functions and classes.");
        }
    }

    public object? VisitLogicalExpr(Logical expr)
    {
        var left = Evaluate(expr.left);
        if (expr.op.type == TokenType.OR)
        {
            if (IsTruthy(left))
            {
                return left;
            }
        }
        else
        {
            if (!IsTruthy(left))
            {
                return left;
            }
        }
        return Evaluate(expr.right);
    }

    public object? VisitAssignExpr(Assign expr)
    {
        var value = Evaluate(expr.value);
        if (locals.TryGetValue(expr, out var distance))
        {
            environment.AssignAt(distance, expr.name, value);
        }
        else
        {
            globals.Assign(expr.name, value);
        }
        return value;
    }

    public object? VisitVariableExpr(Variable expr)
    {
        return LookUpVariable(expr.name, expr);
    }

    object? LookUpVariable(Token name, Expr expr)
    {
        object? variable;
        if (locals.TryGetValue(expr, out var distance))
        {
            variable = environment.GetAt(distance, name.lexeme);
        }
        else
        {
            variable = globals.Get(name);
        }
        if (variable is Unassigned)
        {
            throw new RuntimeException(name, "Variable must be initialized before use.");
        }
        return variable;
    }

    public object? VisitVarStmt(Var stmt)
    {
        var val = stmt.initializer != null ? Evaluate(stmt.initializer) : Environment.unassigned;
        environment.Define(stmt.name.lexeme, val);
        return null;
    }

    public object? VisitPrintStmt(Print stmt)
    {
        var val = Evaluate(stmt.expression);
        stdout(Stringify(val));
        return null;
    }

    public object? VisitExpressionStmt(Expression stmt)
    {
        var val = Evaluate(stmt.expression);
        if (mode == InterpreterMode.Repl)
        {
            stdout(Stringify(val));
        }
        return null;
    }

    public object? VisitLiteralExpr(Literal literal) => literal.value;

    public object? VisitTernaryExpr(Ternary ternary)
    {
        var condition = Evaluate(ternary.condition);

        if (IsTruthy(condition))
        {
            return Evaluate(ternary.ifTrue);
        }
        else
        {
            return Evaluate(ternary.ifFalse);
        }
    }

    public object? VisitGroupingExpr(Grouping grouping) => Evaluate(grouping.expression);

    public object? VisitUnaryExpr(Unary unary)
    {
        var right = Evaluate(unary.right);

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
        var left = Evaluate(binary.left);
        var right = Evaluate(binary.right);

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
