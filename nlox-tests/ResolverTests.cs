namespace NLoxTests;

using NLox;
using NLox.AstGen;

public class ResolverTests
{
    class MockResolver
    {
        public readonly Resolver resolver;
        public readonly ScopeStack scopeStack;
        public bool hadError = false;

        public MockResolver(Resolver resolver, ScopeStack scopeStack)
        {
            this.resolver = resolver;
            this.scopeStack = scopeStack;

            resolver.OnError += (token, msg) =>
            {
                this.hadError = true;
            };
        }

        public void Resolve(List<Stmt> stmts)
        {
            this.resolver.Resolve(stmts);
        }
    }

    class MockResolve : IResolve
    {
        Action<Expr, int>? onResolve;
        public MockResolve(Action<Expr, int>? onResolve)
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

    static MockResolver CreateResolver(Action<Expr, int>? onResolve)
    {
        var mockResolve = new MockResolve(onResolve);
        var scopeStack = new ScopeStack();
        var resolver = new Resolver(mockResolve, scopeStack, Scope.Create);
        return new MockResolver(resolver, scopeStack);
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
        var resolver = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(resolver.hadError);
    }

    [Test]
    public void CannotUseThisInGlobalScope()
    {
        var source = @"
this.apa = 44;
";
        var stmts = TestUtilties.ParseStmts(source);
        var resolver = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(resolver.hadError);
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
        var resolver = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(resolver.hadError);
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
        var resolver = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(resolver.hadError);
    }

    [Test]
    public void CannotUseSuperOutsideOfClass()
    {
        var source = @"
super.notEvenInAClass();
";
        var stmts = TestUtilties.ParseStmts(source);
        var resolver = CreateResolver(null);
        resolver.Resolve(stmts);
        Assert.IsTrue(resolver.hadError);
    }

    [Test]
    public void IdentifiersDoNotOverwrite()
    {
        var source = @"
{
    var i = 0; while (i < 10) { i = i + 1;
        {
        }
    }
}
";
        var stmts = TestUtilties.ParseStmts(source);
        var resolutions = new Dictionary<Expr, int>();
        var resolver = CreateResolver((expr, distance) =>
        {
            resolutions[expr] = distance;
        });
        resolver.Resolve(stmts);
        Assert.IsFalse(resolver.hadError);
        Assert.AreEqual(3, resolutions.Count);
    }

    [Test]
    public void ForLoopHaveCorrectDistance()
    {
        var source = @"
for (var i = 0; i < 10; i = i + 1) {
}
";
        var stmts = TestUtilties.ParseStmts(source);
        var resolutions = new List<(Expr Expr, int Distance)>();
        var resolver = CreateResolver((expr, distance) =>
        {
            resolutions.Add((expr, distance));
        });
        resolver.Resolve(stmts);
        Assert.AreEqual(3, resolutions.Count);
    }
}
