// See https://aka.ms/new-console-template for more information

namespace CraftingInterpreters;

static class Lox {
    static bool hadError = false;

    public static void Main(string[] args) {
        if (args.Length > 2) {
            Console.WriteLine("Usage: nlox [script]");
            Environment.Exit(64);
        } else if (args.Length == 2) {
            RunFile(args[1]);
        } else {
            RunPrompt();
        }
    }

    static void RunFile(string path) {
        string source = File.ReadAllText(path);
        Run(source);

        if (hadError) {
            Environment.Exit(65);
        }
    }

    static void RunPrompt() {
        while (true) {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null) {
                break;
            }

            Run(line);
            hadError = false;
        }
    }

    static void Run(string source) {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens) {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message) {
        Report(line, "", message);
    }

    static void Report(int line, string location, string message) {
        Console.Error.WriteLine($"[line {line}] Error {location}: {message}");
        hadError = true;
    }
}
