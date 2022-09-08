namespace nlox_tests;

using CraftingInterpreters;
using CraftingInterpreters.AstGen;

public static class TestUtilties
{
    public static List<Stmt> ParseStmts(string source)
    {
        var scanner = new Scanner(source, (line, msg) => { });
        var parser = new Parser(scanner.ScanTokens(), ParserMode.Normal, (token, msg) => { });
        return parser.Parse();
    }
}
