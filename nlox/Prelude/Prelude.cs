namespace CraftingInterpreters;

using System.Reflection;

using CraftingInterpreters.AstGen;

public static class Prelude
{
    public static List<Stmt> GetPrelude()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Prelude.lox");
        using var reader = new StreamReader(stream!);
        var source = reader.ReadToEnd();
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        return parser.Parse();
    }
}
