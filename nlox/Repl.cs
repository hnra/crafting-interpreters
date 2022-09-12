namespace CraftingInterpreters;

using System.Text;

using AstGen;

public class Repl
{
    readonly Interpreter interpreter = CreateInterpreter();

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

    void Run(Source source)
    {
        Run(source.tokens);
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
        Console.TreatControlCAsInput = true;
        Console.WriteLine("NLox - REPL");
        var history = new List<string>();

        while (true)
        {
            var input = ReadInput($"> ");

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

    void SearchHistory(List<string> history)
    {
        ClearConsoleLine();
        var searchStr = "";
        var consoleBuffer = "Reverse search: ";
        var result = "";
        Console.Write(consoleBuffer);
        while (true)
        {
            var k = ReadKey();
            if (k.ResultType == KeyResultType.NewLine)
            {
                ClearBuffer(consoleBuffer.Length);
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine($"> {result}");
                    Run(result);
                }
                break;
            }
            else if (k.ResultType == KeyResultType.Delete)
            {
                if (searchStr.Length > 0)
                {
                    searchStr = searchStr.Substring(0, searchStr.Length - 1);
                }
            }
            else if (k.ResultType == KeyResultType.Cancel)
            {
                ClearBuffer(consoleBuffer.Length);
                break;
            }
            else if (k.ResultType == KeyResultType.Character)
            {
                searchStr += k.KeyChar;
            }
            ClearBuffer(consoleBuffer.Length);
            result = string.IsNullOrEmpty(searchStr) ? "" : history.FirstOrDefault(l => l.Contains(searchStr));
            consoleBuffer = $"Reverse search ({searchStr}): {result}";
            Console.Write(consoleBuffer);
        }
    }

    record Input;
    record Source(List<Token> tokens, List<string> lines);
    record TextInput(string input) : Input;
    record ExitAction() : Input;
    record ClearAction() : Input;
    record SearchAction() : Input;
    record CancelAction() : Input;
    enum Direction { Up, Down, Left, Right }
    record MoveAction(Direction dir) : Input;

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
        var str = new StringBuilder();
        var i = 0;
        while (true)
        {
            var key = ReadKey();
            switch (key.ResultType)
            {
                case KeyResultType.Character:
                    if (!string.IsNullOrEmpty(key.KeyChar))
                    {
                        var (left, top) = (Console.CursorLeft, Console.CursorTop);
                        MoveForward(str.Length - i);
                        ClearBuffer(indent.Length + str.Length);
                        str.Insert(i, key.KeyChar);
                        i += key.KeyChar.Length;
                        Console.Write(indent + str.ToString());
                        Console.SetCursorPosition(left, top);
                        MoveForward(key.KeyChar.Length);
                    }
                    break;
                case KeyResultType.Delete:
                    if (str.Length > 0)
                    {
                        var (left, top) = (Console.CursorLeft, Console.CursorTop);
                        MoveForward(str.Length - i);
                        ClearBuffer(indent.Length + str.Length);
                        str.Remove(Math.Min(i, str.Length - 1), 1);
                        i -= 1;
                        Console.Write(indent + str.ToString());
                        Console.SetCursorPosition(left - 1, top);
                    }
                    break;
                case KeyResultType.NewLine:
                    Console.WriteLine();
                    str.Insert(i, key.KeyChar);
                    return new TextInput(str.ToString());
                case KeyResultType.Exit:
                    return new ExitAction();
                case KeyResultType.Clear:
                    return new ClearAction();
                case KeyResultType.Search:
                    return new SearchAction();
                case KeyResultType.Cancel:
                    return new CancelAction();
                case KeyResultType.Move:
                    switch (Enum.Parse<Direction>(key.KeyChar!))
                    {
                        case Direction.Left:
                            if (i > 0)
                            {
                                i -= 1;
                                MoveCursorBack();
                            }
                            break;
                        case Direction.Right:
                            if (i < str.Length)
                            {
                                i += 1;
                                MoveCursorForward();
                            }
                            break;
                    }
                    break;
            }
        }
        throw new InputExcetion();
    }

    static void DeleteChar()
    {
        MoveCursorBack();
        Console.Write(" ");
        MoveCursorBack();
    }

    static void MoveCursorBack()
    {
        if (Console.CursorLeft > 0)
        {
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
        else if (Console.CursorTop > 0)
        {
            Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
        }
    }

    static void MoveForward(int distance)
    {
        var distanceLeft = distance;
        var targetLeft = Console.CursorLeft;
        var targetTop = Console.CursorTop;
        while (distanceLeft > 0)
        {
            if (targetLeft >= Console.BufferWidth)
            {
                targetTop += 1;
                targetLeft = 0;
            }
            var moveLeft = Math.Min(distanceLeft, Console.BufferWidth - targetLeft);
            targetLeft += moveLeft;
            distanceLeft -= moveLeft;
        }
        Console.SetCursorPosition(targetLeft, targetTop);
    }

    static void MoveCursorForward()
    {
        if (Console.CursorLeft < Console.BufferWidth)
        {
            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
        }
        else if (Console.CursorTop < Console.BufferHeight)
        {
            Console.SetCursorPosition(0, Console.CursorTop + 1);
        }
    }

    static void ClearBuffer(int bufferLength)
    {
        if (bufferLength <= 0)
        {
            return;
        }

        var (left, top) = (Console.CursorLeft, Console.CursorTop);

        if (left > 0)
        {
            var moveLeft = Math.Min(left, bufferLength);
            Console.SetCursorPosition(left - moveLeft, top);
            ClearConsoleLine(moveLeft);
            ClearBuffer(bufferLength - moveLeft);
        }
        else if (top > 0)
        {
            var moveLeft = Math.Min(Console.BufferWidth, bufferLength);
            Console.SetCursorPosition(Console.BufferWidth - moveLeft, top - 1);
            ClearConsoleLine(moveLeft);
            ClearBuffer(bufferLength - moveLeft);
        }
    }

    static void ClearConsoleLine()
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        ClearConsoleLine(Console.BufferWidth);
    }

    static void ClearConsoleLine(int width)
    {
        var (left, top) = (Console.CursorLeft, Console.CursorTop);
        var emptyLine = string.Join("", Enumerable.Repeat(" ", Math.Min(width, Console.BufferWidth - 1 - left)));
        Console.Write(emptyLine);
        Console.SetCursorPosition(left, top);
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
        Move,
    }

    record KeyResult(string? KeyChar, KeyResultType ResultType);

    static KeyResult ReadKey()
    {
        var key = Console.ReadKey(true);
        return (key.Modifiers, key.Key) switch
        {
            (ConsoleModifiers.Control, ConsoleKey.D) => new KeyResult("D", KeyResultType.Exit),
            (ConsoleModifiers.Control, ConsoleKey.L) => new KeyResult("L", KeyResultType.Clear),
            (ConsoleModifiers.Control, ConsoleKey.R) => new KeyResult("R", KeyResultType.Search),
            (ConsoleModifiers.Control, ConsoleKey.C) => new KeyResult("C", KeyResultType.Cancel),
            (_, ConsoleKey.Backspace) => new KeyResult("R", KeyResultType.Delete),
            (_, ConsoleKey.Enter) => new KeyResult("\n", KeyResultType.NewLine),
            (_, ConsoleKey.Tab) => new KeyResult("  ", KeyResultType.Character),
            (_, ConsoleKey.UpArrow) => new KeyResult(Enum.GetName(typeof(Direction), Direction.Up), KeyResultType.Move),
            (_, ConsoleKey.DownArrow) => new KeyResult(Enum.GetName(typeof(Direction), Direction.Down), KeyResultType.Move),
            (_, ConsoleKey.LeftArrow) => new KeyResult(Enum.GetName(typeof(Direction), Direction.Left), KeyResultType.Move),
            (_, ConsoleKey.RightArrow) => new KeyResult(Enum.GetName(typeof(Direction), Direction.Right), KeyResultType.Move),
            _ => new KeyResult(key.KeyChar.ToString(), KeyResultType.Character)
        };
    }
}
