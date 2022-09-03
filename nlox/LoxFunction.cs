namespace CraftingInterpreters;

using CraftingInterpreters.AstGen;

public class LoxFunction : LoxCallable
{
    readonly Function declaration;

    public LoxFunction(Function declaration)
    {
        this.declaration = declaration;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(interpreter.Globals);
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
            return returnValue.Value;
        }
        return null;
    }

    public int Arity() => declaration.parameters.Count;

    public override string ToString()
    {
        return $"<fn {declaration.name.lexeme}>";
    }
}
