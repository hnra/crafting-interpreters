namespace CraftingInterpreters;

public interface LoxCallable
{
    int Arity();
    object Call(Interpreter interpreter, List<object?> arguments);
}
