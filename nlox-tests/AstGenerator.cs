namespace nlox_tests.AstGenerator;

public class AstGenerator
{
    static string GetNLoxPath()
    {
        var currentDir = "..";
        var dirs = Directory.GetDirectories(currentDir);

        while (!dirs.Any(p => p.EndsWith("nlox")))
        {
            currentDir = Path.Combine("..", currentDir);
            dirs = Directory.GetDirectories(currentDir);
        }

        return Path.Combine(currentDir, "nlox");
    }

    static string outputdir = Path.Combine(GetNLoxPath(), "AstGen");

    [Test, Explicit]
    public void GenerateAst()
    {
        var asts = new[]
        {
            ("Expr", DefineAst("Expr", new[]
            {
                "Binary : Expr left, Token op, Expr right",
                "Grouping : Expr expression",
                "Literal : object value",
                "Unary : Token op, Expr right",
            })),
        };

        if (!Directory.Exists(outputdir))
        {
            Directory.CreateDirectory(outputdir);
        }

        foreach (var (baseName, source) in asts)
        {
            var path = Path.Combine(outputdir, $"{baseName}.cs");
            File.WriteAllText(path, source);
        }
    }

    static string DefineAst(string baseName, string[] types)
    {
        var writer = new StringWriter();

        writer.WriteLine("namespace CraftingInterpreters.AstGen;\n");
        writer.WriteLine($"public abstract record {baseName};");

        foreach (var type in types)
        {
            var typeSplit = type.Split(":");
            var className = typeSplit[0].Trim();
            var fields = typeSplit[1].Trim();
            DefineType(writer, baseName, className, fields);
        }

        return writer.ToString();
    }

    static void DefineType(StringWriter writer, string baseName, string className, string fields)
    {
        writer.WriteLine($"public record {className}({fields}) : {baseName};");
    }
}
