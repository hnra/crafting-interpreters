namespace nlox_tests;

using CraftingInterpreters;
using CraftingInterpreters.AstGen;

public class ResolverTests
{
    static List<Stmt> ParseStmts(string source)
    {
        var scanner = new Scanner(source, (line, msg) => { });
        var parser = new Parser(scanner.ScanTokens(), ParserMode.Normal, (token, msg) => { });
        return parser.Parse();
    }

    class MockResolve : IResolve
    {
        public void Resolve(Expr expr, int depth) { }
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
        var stmts = ParseStmts(source);
        var hadError = false;
        var resolver = new Resolver(new MockResolve(), new ScopeStack(), Scope.Create, (token, msg) =>
        {
            hadError = true;
        });
        resolver.Resolve(stmts);
        Assert.IsTrue(hadError);
    }
}
