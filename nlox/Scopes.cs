namespace CraftingInterpreters;

public interface IScope
{
    void Define(string variable);
    void Declare(string variable);
    bool IsDefined(string variable);
    bool IsDeclared(string variable);
}

public class Scope : IScope
{
    readonly Dictionary<string, bool> scope = new();
    public static Scope Create() => new();
    public void Define(string variable)
    {
        scope[variable] = false;
    }
    public void Declare(string variable)
    {
        scope[variable] = true;
    }
    public bool IsDefined(string variable) => scope.ContainsKey(variable);
    public bool IsDeclared(string variable) =>
        scope.TryGetValue(variable, out var isDeclared) && isDeclared;
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
    public bool IsEmpty() => stack.Count == 0;
    public int Depth => stack.Count;
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
