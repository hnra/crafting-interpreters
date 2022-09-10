namespace CraftingInterpreters;

public record Unassigned;

public class Environment
{
    public static readonly Unassigned unassigned = new();

    readonly Dictionary<string, object?> values = new();
    public readonly Environment? enclosing = null;

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

    public void AssignAt(int distance, Token name, object? val)
    {
        var env = Ancestor(distance);
        if (env != null)
        {
            env.values[name.lexeme] = val;
        }
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

    public object? GetAt(int distance, string name)
    {
        var env = Ancestor(distance);
        if (env != null && env.values.TryGetValue(name, out var val))
        {
            return val;
        }
        return null;
    }

    Environment? Ancestor(int distance)
    {
        Environment? environment = this;
        for (var i = 0; i < distance; i++)
        {
            environment = environment?.enclosing;
        }
        return environment;
    }
}
