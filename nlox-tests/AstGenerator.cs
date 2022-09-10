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
                "Assign : Token name, Expr value",
                "Ternary : Expr condition, Expr ifTrue, Expr ifFalse",
                "Binary : Expr left, Token op, Expr right",
                "Grouping : Expr expression",
                "Literal : object? value",
                "Logical : Expr left, Token op, Expr right",
                "Unary : Token op, Expr right",
                "Variable : Token name",
                "Call : Expr callee, Token paren, List<Expr> arguments",
                "Get : Expr obj, Token name",
                "Set : Expr obj, Token name, Expr value",
                "This : Token keyword",
                "Super : Token keyword, Token method"
            })),
            ("Stmt", DefineAst("Stmt", new[]
            {
                "Expression : Expr expression",
                "If : Expr condition, Stmt thenBranch, Stmt? elseBranch",
                "Print : Expr expression",
                "While : Expr condition, Stmt body",
                "Var : Token name, Expr? initializer",
                "Block : List<Stmt> statements",
                "Function : Token name, List<Token> parameters, List<Stmt> body",
                "Return : Token keyword, Expr? value",
                "Class : Token name, Variable? superclass, List<Function> methods"
            }))
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

        writer.WriteLine($"public interface {baseName}Visitor<R>");
        writer.WriteLine("{");
        foreach (var type in types)
        {
            var typeSplit = type.Split(":");
            var className = typeSplit[0].Trim();
            writer.WriteLine($"    R Visit{className}{baseName}({className} {baseName.ToLower()});");
        }
        writer.WriteLine("}");

        writer.WriteLine($"\npublic abstract record {baseName}");
        writer.WriteLine("{");
        writer.WriteLine($"    public abstract R Accept<R>({baseName}Visitor<R> visitor);");
        writer.WriteLine("}");

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
        writer.WriteLine($"\npublic record {className}({fields}) : {baseName}");
        writer.WriteLine("{");
        writer.WriteLine($"    public override R Accept<R>({baseName}Visitor<R> visitor) => visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("}");
    }
}
