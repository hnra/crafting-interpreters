namespace nlox_tests;

using CraftingInterpreters;
using CraftingInterpreters.AstGen;

public class ResolverTests
{
    class MockResolve : IResolve
    {
        Action<Expr, int>? onResolve;
        public MockResolve() { }
        public MockResolve(Action<Expr, int> onResolve)
        {
            this.onResolve = onResolve;
        }
        public void Resolve(Expr expr, int depth)
        {
            if (onResolve != null)
            {
                onResolve(expr, depth);
            }
        }
    }

    static (Resolver Resolver, Func<bool> hadError) CreateResolver(Action<Expr, int>? onResolve)
    {
        var hadError = false;
        var resolver = new Resolver(new MockResolve(), new ScopeStack(), Scope.Create);
        resolver.OnError += (token, msg) =>
        {
            hadError = true;
        };
        return (resolver, () => hadError);
    }

    [Test]
    public void CannotReadLocalVarInInitializer()
    {
        var source = @"
var a = 44;
{
    var a = a + 4;
}
";
        var stmts = TestUtilties.ParseStmts(source);
        var (resolver, hadError) = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(hadError());
    }

    [Test]
    public void CannotUseThisInGlobalScope()
    {
        var source = @"
this.apa = 44;
";
        var stmts = TestUtilties.ParseStmts(source);
        var (resolver, hadError) = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(hadError());
    }

    [Test]
    public void CannotUseThisInFunction()
    {
        var source = @"
fun foo() {
    return this;
}
";
        var stmts = TestUtilties.ParseStmts(source);
        var (resolver, hadError) = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(hadError());
    }

    [Test]
    public void CannotUseSuperWithoutSuperclass()
    {
        var source = @"
class Eclair {
    cook() {
        super.cook();
    }
}
";
        var stmts = TestUtilties.ParseStmts(source);
        var (resolver, hadError) = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(hadError());
    }

    [Test]
    public void CannotUseSuperOutsideOfClass()
    {
        var source = @"
super.notEvenInAClass();
";
        var stmts = TestUtilties.ParseStmts(source);
        var (resolver, hadError) = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(hadError());
    }
}
