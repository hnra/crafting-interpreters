namespace CraftingInterpreters;

public static class Lox
{
    static bool hadError = false;
    static bool hadRuntimeError = false;

    static void StdOut(string msg)
    {
        Console.WriteLine(msg);
    }

    static void StdErr(string msg)
    {
        Console.Error.WriteLine(msg);
    }

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            StdOut("Usage: nlox [script]");
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
                var interpreter = new Interpreter(StdOut, InterpreterMode.Repl, RuntimeError);
                Run(args[0], ParserMode.Repl, interpreter);
            }
        }
        else
        {
            RunPrompt();
        }
    }

    public static void RunFile(string path)
    {
        string source = File.ReadAllText(path);
        var interpreter = new Interpreter(StdOut, InterpreterMode.Normal, RuntimeError);
        Run(source, ParserMode.Normal, interpreter);

        if (hadError)
        {
            System.Environment.Exit(65);
        }
        if (hadRuntimeError)
        {
            System.Environment.Exit(70);
        }
    }

    public static void RunPrompt()
    {
        var interpreter = new Interpreter(StdOut, InterpreterMode.Repl, RuntimeError);
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
            {
                break;
            }

            Run(line, ParserMode.Repl, interpreter);
            hadError = false;
        }
    }

    public static void Run(string source, ParserMode parserMode, Interpreter interpreter)
    {
        var scanner = new Scanner(source, Error);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens, parserMode, Error);
        var stmts = parser.Parse();

        if (hadError)
        {
            return;
        }

        interpreter.Interpret(stmts);
    }

    static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    static void Error(Token token, string message)
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
        StdErr($"[line {line}] Error {location}: {message}");
        hadError = true;
    }

    static void RuntimeError(RuntimeException error)
    {
        StdErr(error.Message);
        StdErr($"[line {error.token.line}]");
        hadRuntimeError = true;
    }
}
