namespace nlox_tests;

using CraftingInterpreters;
using CraftingInterpreters.AstGen;

public static class TestUtilties
{
    public static string GetTestFilePath(string file)
    {
        var path = Directory.GetFiles("TestFiles", $"*{file}", SearchOption.AllDirectories).First();
        if (path == null)
        {
            throw new FileNotFoundException();
        }
        return path;
    }

    public static List<Stmt> ParseStmts(string source)
    {
        var scanner = new Scanner(source);
        var parser = new Parser(scanner.ScanTokens());
        return parser.Parse();
    }
}
