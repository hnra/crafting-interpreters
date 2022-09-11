namespace CraftingInterpreters;

using AstGen;

public static class Repl
{
    static readonly Interpreter interpreter = new Interpreter((output) =>
        {
            Console.WriteLine(output);
        }, (error) =>
        {
            Console.Error.WriteLine($"Runtime error: {error}");
        });

    static Scanner CreateScanner(string source) =>
        new Scanner(source, (line, msg) =>
        {
            Console.Error.WriteLine($"Scanner error: {msg}");
        });

    static Parser CreateParser(List<Token> tokens) =>
        new Parser(tokens, (token, msg) =>
        {
            Console.Error.WriteLine($"Parser error: {msg}");
        });

    static void Run(Source source)
    {
        Run(source.tokens);
    }

    static void Run(string source)
    {
        var scanner = CreateScanner(source);
        Run(scanner.ScanTokens());
    }

    static void Run(List<Token> tokens)
    {
        var parser = CreateParser(tokens);
        Run(parser.Parse());
    }

    static void Run(List<Stmt> stmts)
    {
        var resolver = new Resolver(interpreter, new ScopeStack(), () => new Scope(), (token, msg) =>
        {
            Console.Error.WriteLine($"Resolver error: {msg}");
        });
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);
    }

    public static void Run(params string[] args)
    {
        Console.WriteLine("NLox - REPL");
        var history = new List<string>();

        while (true)
        {
            var input = ReadInput("> ");

            if (input is TextInput text)
            {
                var source = ReadSource(text.input);
                history.AddRange(source.lines.Select(l => l.Trim()));
                Run(source);
            }
            else if (input is ExitAction || input is CancelAction)
            {
                Console.WriteLine("Exiting REPL. Bye!");
                System.Environment.Exit(0);
            }
            else if (input is ClearAction)
            {
                Console.Clear();
            }
            else if (input is SearchAction)
            {
                SearchHistory(history);
            }
        }
    }

    static void SearchHistory(List<string> history)
    {
        ClearConsoleLine();
        Console.Write("Reverse search: ");
        var searchStr = "";
        while (true)
        {
            var k = ReadKey();
            var result = history.FirstOrDefault(l => l.Contains(searchStr));
            if (k.ResultType == KeyResultType.NewLine)
            {
                ClearConsoleLine();
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine($"> {result}");
                    Run(result);
                }
                break;
            }
            else if (k.ResultType == KeyResultType.Delete)
            {
                searchStr = searchStr.Substring(0, searchStr.Length - 1);
            }
            else if (k.ResultType == KeyResultType.Cancel)
            {
                ClearConsoleLine();
                break;
            }
            else if (k.ResultType == KeyResultType.Character)
            {
                searchStr += k.KeyChar;
            }
            ClearConsoleLine();
            Console.Write($"Reverse search ({searchStr}): {result}");
        }
    }

    record Input;
    record Source(List<Token> tokens, List<string> lines);
    record TextInput(string input) : Input;
    record ExitAction() : Input;
    record ClearAction() : Input;
    record SearchAction() : Input;
    record CancelAction() : Input;

    class InputExcetion : Exception { }

    static Source ReadSource(string input)
    {
        var lines = new List<string> { input };
        var scanner = CreateScanner(input);
        var tokens = scanner.ScanTokens();

        while (tokens != null && HasOpenBlock(tokens))
        {
            var nextInput = ReadInput();
            if (nextInput is TextInput text)
            {
                lines.Add(text.input);
                var source = ReadSource(text.input);
                if (tokens[^1].type == TokenType.EOF)
                {
                    tokens.RemoveAt(tokens.Count - 1);
                }
                tokens.AddRange(source.tokens);
            }
            else
            {
                throw new InputExcetion();
            }
        }

        if (tokens == null)
        {
            return ReadSource("");
        }

        return new Source(tokens, lines);
    }

    static Input ReadInput() => ReadInput("");

    static Input ReadInput(string indent)
    {
        Console.Write(indent);
        var str = new StringWriter();
        while (true)
        {
            var key = ReadKey();
            switch (key.ResultType)
            {
                case KeyResultType.Character:
                    str.Write(key.KeyChar);
                    Console.Write(key.KeyChar);
                    break;
                case KeyResultType.Delete:
                    var soFar = str.ToString();
                    if (soFar.Length > 0)
                    {
                        soFar = soFar.Substring(0, soFar.Length - 1);
                    }
                    str.Dispose();
                    str = new StringWriter();
                    ClearConsoleLine();
                    Console.Write(indent);
                    Console.Write(soFar);
                    str.Write(soFar);
                    break;
                case KeyResultType.NewLine:
                    Console.WriteLine();
                    str.Write(key.KeyChar);
                    return new TextInput(str.ToString());
                case KeyResultType.Exit:
                    return new ExitAction();
                case KeyResultType.Clear:
                    return new ClearAction();
                case KeyResultType.Search:
                    return new SearchAction();
                case KeyResultType.Cancel:
                    return new CancelAction();
            }
        }
        throw new InputExcetion();
    }

    static void ClearConsoleLine()
    {
        var emptyLine = "\r" + string.Join("", Enumerable.Repeat(" ", Console.BufferWidth)) + "\r";
        Console.Write(emptyLine);
    }

    static bool HasOpenBlock(List<Token> tokens) =>
        tokens.Aggregate(0, (openBlocks, token) => token.type switch
        {
            TokenType.LEFT_BRACE => openBlocks + 1,
            TokenType.RIGHT_BRACE => openBlocks - 1,
            _ => openBlocks
        }) > 0;

    enum KeyResultType
    {
        Character,
        NewLine,
        Delete,
        Cancel,
        Exit,
        Clear,
        Search,
    }

    record KeyResult(string? KeyChar, KeyResultType ResultType);

    static KeyResult ReadKey()
    {
        var hasCancelled = false;
        Console.CancelKeyPress += OnCancel;

        while (!hasCancelled)
        {
            if (!Console.KeyAvailable)
            {
                Thread.Sleep(50);
                continue;
            }

            var key = Console.ReadKey(true);
            Console.CancelKeyPress -= OnCancel;
            return (key.Modifiers, key.Key) switch
            {
                (ConsoleModifiers.Control, ConsoleKey.D) => new KeyResult("D", KeyResultType.Exit),
                (ConsoleModifiers.Control, ConsoleKey.L) => new KeyResult("L", KeyResultType.Clear),
                (ConsoleModifiers.Control, ConsoleKey.R) => new KeyResult("R", KeyResultType.Search),
                (_, ConsoleKey.Backspace) => new KeyResult("R", KeyResultType.Delete),
                (_, ConsoleKey.Enter) => new KeyResult("\n", KeyResultType.NewLine),
                (_, ConsoleKey.Tab) => new KeyResult("  ", KeyResultType.Character),
                _ => new KeyResult(key.KeyChar.ToString(), KeyResultType.Character)
            };
        }

        Console.CancelKeyPress -= OnCancel;
        return new KeyResult("C", KeyResultType.Cancel);

        void OnCancel(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            hasCancelled = true;
        }
    }
}
