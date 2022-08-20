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
        this.values[name] = val;
    }

    public void Assign(Token name, object? val)
    {
        if (this.values.ContainsKey(name.lexeme))
        {
            this.values[name.lexeme] = val;
            return;
        }

        if (this.enclosing != null)
        {
            this.enclosing.Assign(name, val);
            return;
        }

        throw new RuntimeException(name, $"Undefined variable {name.lexeme}.");
    }

    public object? Get(Token name)
    {
        if (this.values.TryGetValue(name.lexeme, out var val))
        {
            return val;
        }

        if (this.enclosing != null)
        {
            return this.enclosing.Get(name);
        }

        throw new RuntimeException(name, $"Undefined variable {name.lexeme}.");
    }
}
