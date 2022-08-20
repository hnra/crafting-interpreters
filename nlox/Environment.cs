namespace CraftingInterpreters;

public class Environment
{
    readonly Dictionary<string, object?> values = new();

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

        throw new RuntimeException(name, $"Undefined variable {name.lexeme}.");
    }

    public object? Get(Token name)
    {
        if (this.values.TryGetValue(name.lexeme, out var val))
        {
            return val;
        }

        throw new RuntimeException(name, $"Undefined variable {name.lexeme}.");
    }
}
