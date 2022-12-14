namespace NLox;

using NLox.AstGen;

public interface IResolve
{
    void Resolve(Expr expr, int depth);
}

public class Resolver : StmtVisitor<Resolver.Unit>, ExprVisitor<Resolver.Unit>
{
    public sealed record Unit;
    enum FunctionType { NONE, FUNCTION, METHOD, INITIALIZER };
    enum ClassType { NONE, CLASS, SUBCLASS };

    public delegate void ErrorHandler(Token token, string message);
    public event ErrorHandler? OnError;

    #region Fields and Constructors

    static Unit unit = new();
    readonly IResolve interpreter;
    readonly IScopeStack scopes;
    readonly Func<IScope> createScope;
    FunctionType currentFunction = FunctionType.NONE;
    ClassType currentClass = ClassType.NONE;

    public Resolver(IResolve interpreter, IScopeStack scopes, Func<IScope> createScope)
    {
        this.interpreter = interpreter;
        this.scopes = scopes;
        this.createScope = createScope;
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
            OnError?.Invoke(name, $"Already a variable with the name '{name.lexeme}' in this scope.");
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
        var enclosingClass = currentClass;
        currentClass = ClassType.CLASS;
        Declare(stmt.name);
        Define(stmt.name);
        if (stmt.superclass != null)
        {
            if (stmt.name.lexeme == stmt.superclass.name.lexeme)
            {
                OnError?.Invoke(stmt.superclass.name, "A class can't inherit from itself.");
            }
            currentClass = ClassType.SUBCLASS;
            Resolve(stmt.superclass);
            BeginScope();
            scopes.Last().Define("super");
        }
        BeginScope();
        scopes.Last().Define("this");
        foreach (var method in stmt.methods)
        {
            var declaration = FunctionType.METHOD;
            if (method.name.lexeme == "this")
            {
                declaration = FunctionType.INITIALIZER;
            }
            ResolveFunction(method, declaration);
        }
        EndScope();
        if (stmt.superclass != null)
        {
            EndScope();
        }
        currentClass = enclosingClass;
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
            OnError?.Invoke(stmt.keyword, "Can't return from top-level code.");
        }
        if (currentFunction == FunctionType.INITIALIZER)
        {
            OnError?.Invoke(stmt.keyword, "Ca't return a value from an initializer.");
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

    public Unit VisitVecExpr(Vec expr)
    {
        foreach (var element in expr.elements)
        {
            Resolve(element);
        }
        return unit;
    }

    public Unit VisitSuperExpr(Super expr)
    {
        if (currentClass == ClassType.NONE)
        {
            OnError?.Invoke(expr.keyword, "Can't use 'super' outside of a class.");
        }
        if (currentClass == ClassType.CLASS)
        {
            OnError?.Invoke(expr.keyword, "Can't use 'super' in a class without no superclass.");
        }
        ResolveLocal(expr, expr.keyword);
        return unit;
    }

    public Unit VisitThisExpr(This expr)
    {
        if (currentClass == ClassType.NONE)
        {
            OnError?.Invoke(expr.keyword, "Can't use 'this' outside of a class.");
        }
        ResolveLocal(expr, expr.keyword);
        return unit;
    }

    public Unit VisitSetExpr(Set expr)
    {
        Resolve(expr.value);
        Resolve(expr.obj);
        return unit;
    }

    public Unit VisitGetExpr(Get expr)
    {
        Resolve(expr.obj);
        return unit;
    }

    public Unit VisitVariableExpr(Variable expr)
    {
        if (!scopes.IsEmpty() &&
            scopes.Last().IsDeclared(expr.name.lexeme) &&
            !scopes.Last().IsDefined(expr.name.lexeme))
        {
            OnError?.Invoke(expr.name, "Can't read local variable in its own initializer.");
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
