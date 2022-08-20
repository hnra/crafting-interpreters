namespace CraftingInterpreters;

static class Lox
{
    static bool hadError = false;
    static bool hadRuntimeError = false;
    static Interpreter interpreter = new Interpreter();

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: nlox [script]");
            System.Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            if (File.Exists(args[0]))
            {
                RunFile(args[0]);
            }
            else
            {
                Run(args[0]);
            }
        }
        else
        {
            RunPrompt();
        }
    }

    static void RunFile(string path)
    {
        string source = File.ReadAllText(path);
        Run(source);

        if (hadError)
        {
            System.Environment.Exit(65);
        }
        if (hadRuntimeError)
        {
            System.Environment.Exit(70);
        }
    }

    static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
            {
                break;
            }

            Run(line);
            hadError = false;
        }
    }

    static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var stmts = parser.Parse();

        if (hadError)
        {
            return;
        }

        interpreter.Interpret(stmts);
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, $" at '{token.lexeme}'", message);
        }
    }

    static void Report(int line, string location, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {location}: {message}");
        hadError = true;
    }

    public static void RuntimeError(RuntimeException error)
    {
        Console.Error.WriteLine(error.Message);
        Console.Error.WriteLine($"[line {error.token.line}]");
        hadRuntimeError = true;
    }
}
