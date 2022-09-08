namespace CraftingInterpreters;

using CraftingInterpreters.AstGen;

public interface IResolve
{
    void Resolve(Expr expr, int depth);
}

public class Resolver : StmtVisitor<Resolver.Unit>, ExprVisitor<Resolver.Unit>
{
    public sealed record Unit;
    enum FunctionType { NONE, FUNCTION };

    #region Fields and Constructors

    static Unit unit = new();
    readonly IResolve interpreter;
    readonly IScopeStack scopes;
    readonly Func<IScope> createScope;
    readonly Action<Token, string> onError;
    FunctionType currentFunction = FunctionType.NONE;

    public Resolver(IResolve interpreter, IScopeStack scopes, Func<IScope> createScope, Action<Token, string> onError)
    {
        this.interpreter = interpreter;
        this.scopes = scopes;
        this.createScope = createScope;
        this.onError = onError;
    }

    #endregion

    #region Methods

    void ResolveFunction(Function function, FunctionType functionType)
    {
        var enclosingFunction = currentFunction;
        currentFunction = functionType;
        BeginScope();
        foreach (var param in function.parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.body);
        EndScope();
        currentFunction = enclosingFunction;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var stmt in statements)
        {
            Resolve(stmt);
        }
    }

    void Resolve(Stmt statement)
    {
        statement.Accept(this);
    }

    void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    void BeginScope() => scopes.Push(createScope());

    void EndScope()
    {
        scopes.Pop();
    }

    void Declare(Token name)
    {
        if (scopes.IsEmpty())
        {
            return;
        }
        var scope = scopes.Last();
        if (scope.IsDefined(name.lexeme))
        {
            onError(name, $"Already a variable with the name '{name.lexeme}' in this scope.");
        }
        scopes.Last().Declare(name.lexeme);
    }

    void Define(Token name)
    {
        if (scopes.IsEmpty())
        {
            return;
        }
        scopes.Last().Define(name.lexeme);
    }

    void ResolveLocal(Expr expr, Token name)
    {
        for (var i = scopes.Depth - 1; i >= 0; i--)
        {
            if (scopes.At(i).IsDefined(name.lexeme))
            {
                interpreter.Resolve(expr, scopes.Depth - 1 - i);
                return;
            }
        }
    }

    #endregion

    #region StmtVisitor

    public Unit VisitClassStmt(Class stmt)
    {
        Declare(stmt.name);
        Define(stmt.name);
        return unit;
    }

    public Unit VisitBlockStmt(Block stmt)
    {
        BeginScope();
        Resolve(stmt.statements);
        EndScope();
        return unit;
    }

    public Unit VisitVarStmt(Var stmt)
    {
        Declare(stmt.name);
        if (stmt.initializer != null)
        {
            Resolve(stmt.initializer);
        }
        Define(stmt.name);
        return unit;
    }

    public Unit VisitFunctionStmt(Function stmt)
    {
        Declare(stmt.name);
        Define(stmt.name);
        ResolveFunction(stmt, FunctionType.FUNCTION);
        return unit;
    }

    public Unit VisitExpressionStmt(Expression stmt)
    {
        Resolve(stmt.expression);
        return unit;
    }

    public Unit VisitIfStmt(If stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.thenBranch);
        if (stmt.elseBranch != null)
        {
            Resolve(stmt.elseBranch);
        }
        return unit;
    }

    public Unit VisitPrintStmt(Print stmt)
    {
        Resolve(stmt.expression);
        return unit;
    }

    public Unit VisitReturnStmt(Return stmt)
    {
        if (currentFunction == FunctionType.NONE)
        {
            onError(stmt.keyword, "Can't return from top-level code.");
        }
        if (stmt.value != null)
        {
            Resolve(stmt.value);
        }
        return unit;
    }

    public Unit VisitWhileStmt(While stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.body);
        return unit;
    }

    #endregion

    #region ExprVisitor

    public Unit VisitVariableExpr(Variable expr)
    {
        if (!scopes.IsEmpty() && scopes.Last().IsDeclared(expr.name.lexeme) && !scopes.Last().IsDefined(expr.name.lexeme))
        {
            onError(expr.name, "Can't read local variable in its own initializer.");
        }
        ResolveLocal(expr, expr.name);
        return unit;
    }

    public Unit VisitAssignExpr(Assign expr)
    {
        Resolve(expr.value);
        ResolveLocal(expr, expr.name);
        return unit;
    }

    public Unit VisitBinaryExpr(Binary expr)
    {
        Resolve(expr.left);
        Resolve(expr.right);
        return unit;
    }

    public Unit VisitCallExpr(Call expr)
    {
        Resolve(expr.callee);
        foreach (var arg in expr.arguments)
        {
            Resolve(arg);
        }
        return unit;
    }

    public Unit VisitGroupingExpr(Grouping expr)
    {
        Resolve(expr.expression);
        return unit;
    }

    public Unit VisitLiteralExpr(Literal expr) => unit;

    public Unit VisitLogicalExpr(Logical expr)
    {
        Resolve(expr.left);
        Resolve(expr.right);
        return unit;
    }

    public Unit VisitUnaryExpr(Unary expr)
    {
        Resolve(expr.right);
        return unit;
    }

    public Unit VisitTernaryExpr(Ternary expr)
    {
        Resolve(expr.condition);
        Resolve(expr.ifTrue);
        Resolve(expr.ifFalse);
        return unit;
    }

    #endregion
}
