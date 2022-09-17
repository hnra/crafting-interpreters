namespace CraftingInterpreters;

public class Clock : LoxCallable
{
    public int Arity() => 0;
    public object Call(Interpreter interpreter, List<object?> arguments)
        => (double)DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000;
    public override string ToString()
        => $"<native func '{nameof(Clock)}'>";
}

public class TypeOf : LoxCallable
{
    public int Arity() => 1;
    public object Call(Interpreter interpreter, List<object?> arguments) =>
        arguments[0]!.GetType().ToString();
    public override string ToString() =>
        $"<native func '{nameof(TypeOf)}'>";
}
