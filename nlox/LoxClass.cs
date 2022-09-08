namespace CraftingInterpreters;

public class LoxClass : LoxCallable
{
    readonly Dictionary<string, LoxFunction> methods;
    public string Name { get; }

    public object Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);
        return instance;
    }

    public int Arity() => 0;

    public LoxClass(string name, Dictionary<string, LoxFunction> methods)
    {
        this.Name = name;
        this.methods = methods;
    }

    public LoxFunction? FindMethod(string name) =>
        methods.GetValueOrDefault(name);

    public override string ToString() => Name;
}
