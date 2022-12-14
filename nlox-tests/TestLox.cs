namespace NLoxTests;

using NLox;

class TestLox
{
    public bool hadError = false;

    public List<string> Run(string source)
    {
        var scanner = new Scanner(source);
        scanner.onError += (line, msg) =>
        {
            hadError = true;
        };
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        parser.OnError += (tokens, msg) =>
        {
            hadError = true;
        };
        var stmts = Prelude.GetPrelude();
        stmts.AddRange(parser.Parse());

        var output = new List<string>();
        var interpreter = new Interpreter();
        interpreter.OnStdOut += (msg) =>
        {
            output.Add(msg);
        };
        interpreter.OnError += (msg) =>
        {
            hadError = true;
        };
        var resolver = new Resolver(interpreter, new ScopeStack(), Scope.Create);
        resolver.OnError += (token, msg) =>
        {
            hadError = true;
        };
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);
        return output;
    }
}
