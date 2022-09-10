namespace CraftingInterpreters;

using CraftingInterpreters.AstGen;

public class LoxFunction : LoxCallable
{
    readonly Function declaration;
    readonly Environment closure;
    readonly bool isInitializer;

    public LoxFunction(Function declaration, Environment closure, bool isInitializer)
    {
        this.declaration = declaration;
        this.closure = closure;
        this.isInitializer = isInitializer;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(closure);
        for (var i = 0; i < declaration.parameters.Count; i++)
        {
            environment.Define(declaration.parameters[i].lexeme, arguments[i]);
        }
        try
        {
            interpreter.ExecuteBlock(declaration.body, environment);
        }
        catch (ReturnException returnValue)
        {
            if (isInitializer)
            {
                return closure.GetAt(0, "this");
            }
            return returnValue.Value;
        }
        if (isInitializer)
        {
            return closure.GetAt(0, "this");
        }
        return null;
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Environment(closure);
        environment.Define("this", instance);
        return new LoxFunction(declaration, environment, isInitializer);
    }

    public int Arity() => declaration.parameters.Count;

    public override string ToString()
    {
        return $"<fn {declaration.name.lexeme}>";
    }
}
