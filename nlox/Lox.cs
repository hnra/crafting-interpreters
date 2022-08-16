﻿namespace CraftingInterpreters;

static class Lox
{
    static bool hadError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: nlox [script]");
            Environment.Exit(64);
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
            Environment.Exit(65);
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
        var expr = parser.Parse();

        if (hadError || expr == null)
        {
            return;
        }

        Console.WriteLine(new AstPrinter().Print(expr));
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
}
