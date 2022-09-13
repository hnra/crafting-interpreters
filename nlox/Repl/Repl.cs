namespace CraftingInterpreters.Repl;

using AstGen;

public class Repl
{
    readonly Interpreter interpreter = CreateInterpreter();
    readonly ReplConsole console = new();

    static Interpreter CreateInterpreter()
    {
        var interpreter = new Interpreter();
        interpreter.OnStdOut += (output) =>
        {
            Console.WriteLine(output);
        };
        interpreter.OnError += (error) =>
        {
            Console.Error.WriteLine($"Runtime error: {error}");
        };
        return interpreter;
    }

    static Scanner CreateScanner(string source)
    {
        var scanner = new Scanner(source);
        scanner.onError += (line, msg) =>
        {
            Console.Error.WriteLine($"Scanner error: {msg}");
        };
        return scanner;
    }

    static Parser CreateParser(List<Token> tokens)
    {
        if (tokens.Count > 1)
        {
            var lastToken = tokens[^1];
            tokens.Insert(tokens.Count - 1, new Token(TokenType.SEMICOLON, ";", null, lastToken.line));
        }
        var parser = new Parser(tokens);
        parser.OnError += (token, msg) =>
        {
            Console.Error.WriteLine($"Parser error: {msg}");
        };
        return parser;
    }

    void Run(string source)
    {
        var scanner = CreateScanner(source);
        Run(scanner.ScanTokens());
    }

    void Run(List<Token> tokens)
    {
        var parser = CreateParser(tokens);
        Run(parser.Parse());
    }

    void Run(List<Stmt> stmts)
    {
        var resolver = new Resolver(interpreter, new ScopeStack(), () => new Scope());
        resolver.OnError += (token, msg) =>
        {
            Console.Error.WriteLine($"Resolver error: {msg}");
        };
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        if (stmts.Count > 0 &&
            stmts[^1] is Expression expr &&
            !(expr.expression is Assign) &&
            interpreter.LastEval != null)
        {
            Console.WriteLine(interpreter.LastEval);
        }
    }

    public void Start()
    {
        Console.WriteLine("NLox - REPL");

        while (true)
        {
            var input = console.GetInput("> ");
            if (input is ReplConsole.LineResult line)
            {
                Run(line.line);
            }
            else if (input is ReplConsole.ControlResult ctrl)
            {
                switch (ctrl.key)
                {
                    case ConsoleKey.D:
                    case ConsoleKey.C:
                        Console.WriteLine("Exiting REPL! Bye bye!");
                        System.Environment.Exit(0);
                        break;
                }
            }
        }
    }
}
