namespace CraftingInterpreters;

public class LoxInstance
{
    LoxClass klass;
    readonly Dictionary<string, object?> fields = new();

    public LoxInstance(LoxClass klass)
    {
        this.klass = klass;
    }

    public object? Get(Token name)
    {
        if (fields.TryGetValue(name.lexeme, out var value))
        {
            return value;
        }
        var method = klass.FindMethod(name.lexeme);
        if (method != null)
        {
            return method.Bind(this);
        }
        throw new RuntimeException(name, $"Undefined property {name.lexeme}.");
    }

    public void Set(Token name, object? value)
    {
        fields[name.lexeme] = value;
    }

    public override string ToString()
    {
        return $"{klass.Name} instance";
    }
}
