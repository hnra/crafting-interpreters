namespace NLox;

public class CallException : Exception
{
    public CallException(string message) : base(message) { }
}

public interface LoxCallable
{
    int Arity();
    object? Call(Interpreter interpreter, List<object?> arguments);
}
