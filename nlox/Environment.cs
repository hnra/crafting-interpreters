namespace CraftingInterpreters;

public record Unassigned;

public class Environment
{
    public static readonly Unassigned unassigned = new();

    readonly Dictionary<string, object?> values = new();
    readonly Environment? enclosing = null;

    public Environment() { }

    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }

    public void Define(string name, object? val)
    {
        values[name] = val;
    }

    public void Assign(Token name, object? val)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = val;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, val);
            return;
        }

        throw new RuntimeException(name, $"Undefined variable {name.lexeme}.");
    }

    public object? Get(Token name)
    {
        if (values.TryGetValue(name.lexeme, out var val))
        {
            return val;
        }

        if (enclosing != null)
        {
            return enclosing.Get(name);
        }

        throw new RuntimeException(name, $"Undefined variable {name.lexeme}.");
    }
}
