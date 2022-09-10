namespace CraftingInterpreters;

public interface IScope
{
    void Declare(string variable);
    void Define(string variable);
    bool IsDeclared(string variable);
    bool IsDefined(string variable);
}

public class Scope : IScope
{
    readonly Dictionary<string, bool> scope = new();

    public static Scope Create() => new();

    public void Declare(string variable)
    {
        scope[variable] = false;
    }

    public void Define(string variable)
    {
        scope[variable] = true;
    }

    public bool IsDeclared(string variable) => scope.ContainsKey(variable);

    public bool IsDefined(string variable) =>
        scope.TryGetValue(variable, out var isDefined) && isDefined;
}

public interface IScopeStack
{
    bool IsEmpty();
    void Pop();
    IScope Last();
    void Push(IScope scope);
    int Depth { get; }
    IScope At(int i);
}

public class ScopeStack : IScopeStack
{
    readonly List<IScope> stack = new();
    public int Depth => stack.Count;

    public bool IsEmpty() => stack.Count == 0;

    public IScope Last() => stack[stack.Count - 1];

    public void Pop()
    {
        stack.RemoveAt(stack.Count - 1);
    }

    public void Push(IScope scope)
    {
        stack.Add(scope);
    }

    public IScope At(int i) => stack[i];
}
