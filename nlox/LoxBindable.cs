namespace NLox;

public interface LoxBindable : LoxCallable
{
    public LoxBindable Bind(LoxInstance instance);
}
