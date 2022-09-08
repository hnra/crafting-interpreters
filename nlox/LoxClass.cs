namespace CraftingInterpreters;

public class LoxClass : LoxCallable
{
    public string Name { get; }

    public object Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);
        return instance;
    }

    public int Arity() => 0;

    public LoxClass(string name)
    {
        this.Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
