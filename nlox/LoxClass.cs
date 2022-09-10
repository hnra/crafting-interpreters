namespace CraftingInterpreters;

public class LoxClass : LoxCallable
{
    #region Fields and Constructors

    readonly Dictionary<string, LoxFunction> methods;
    readonly LoxClass? superclass;
    public string Name { get; }

    public LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> methods)
    {
        this.Name = name;
        this.methods = methods;
        this.superclass = superclass;
    }

    #endregion

    #region Methods

    public object Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);
        var initializer = FindMethod("init");
        if (initializer != null)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }
        return instance;
    }

    public int Arity()
    {
        var initializer = FindMethod("init");
        return initializer?.Arity() ?? 0;
    }

    public LoxFunction? FindMethod(string name)
    {
        if (methods.ContainsKey(name))
        {
            return methods[name];
        }
        if (superclass != null)
        {
            return superclass.FindMethod(name);
        }
        return null;
    }

    public override string ToString() => Name;

    #endregion
}
