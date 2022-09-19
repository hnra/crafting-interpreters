namespace NLox;

class ReturnException : Exception
{
    public object? Value { get; init; }
}
