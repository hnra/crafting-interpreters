namespace CraftingInterpreters;

class ReturnException : Exception
{
    public object? Value { get; init; }
}
