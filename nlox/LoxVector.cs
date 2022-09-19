namespace NLox;

public class LoxVector : LoxClass
{
    static Dictionary<string, LoxBindable> CreateMethods()
    {
        return new Dictionary<string, LoxBindable>
        {
            ["append"] = new Append(),
            ["length"] = new Length(),
            ["at"] = new At(),
        };
    }

    public LoxVector() : base("vec", null, CreateMethods()) { }

    public override object Call(Interpreter interpreter, List<object?> arguments)
    {
        if (arguments.Count > 0)
        {
            return new Instance(this, arguments);
        }
        else
        {
            return new Instance(this);
        }
    }

    public class Instance : LoxInstance
    {
        readonly List<object?> elements;
        public Instance(LoxVector vector) : base(vector)
        {
            this.elements = new();
        }
        public Instance(LoxVector vector, List<object?> elements)
            : base(vector)
        {
            this.elements = elements;
        }
        public void Append(object? obj)
        {
            elements.Add(obj);
        }
        public int Length()
        {
            return elements.Count;
        }
        public object? At(int i)
        {
            var j = i;
            if (i < 0)
            {
                j = elements.Count + i;
            }
            if (j >= elements.Count)
            {
                throw new CallException("Index cannot be greater than the length of the vector.");
            }
            else if (j < 0)
            {
                throw new CallException("Negative index cannot be greater than the length of the vector.");
            }
            return elements[j];
        }
    }

    class Append : LoxBindable
    {
        public int Arity() => 1;
        Instance? vector = null;
        public LoxBindable Bind(LoxInstance instance)
        {
            if (instance is Instance vectorInstance)
            {
                vector = vectorInstance;
            }
            return this;
        }
        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            vector?.Append(arguments[0]);
            return null;
        }
    }

    class Length : LoxBindable
    {
        public int Arity() => 0;
        Instance? vector = null;
        public LoxBindable Bind(LoxInstance instance)
        {
            if (instance is Instance vectorInstance)
            {
                vector = vectorInstance;
            }
            return this;
        }
        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return vector?.Length() ?? 0;
        }
    }

    class At : LoxBindable
    {
        public int Arity() => 1;
        Instance? vector = null;
        public LoxBindable Bind(LoxInstance instance)
        {
            if (instance is Instance vectorInstance)
            {
                vector = vectorInstance;
            }
            return this;
        }
        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            if (arguments[0] is double i)
            {
                return vector?.At((int)i);
            }
            else
            {
                throw new CallException($"Vectors can only be indexed with integers, index is: {arguments[0].GetType()}");
            }
        }
    }
}
