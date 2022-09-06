namespace nlox_tests;

using CraftingInterpreters;

class TestLox
{
    public List<string> Run(string source)
    {
        var scanner = new Scanner(source, (line, msg) => { });
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens, ParserMode.Normal, (tokens, msg) => { });
        var stmts = parser.Parse();

        var output = new List<string>();
        var interpreter = new Interpreter(
            (msg) =>
                {
                    output.Add(msg);
                },
            InterpreterMode.Normal,
            (msg) => { });
        var resolver = new Resolver(interpreter, new ScopeStack(), Scope.Create, (token, msg) => { });
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);
        return output;
    }
}

public class ProgramTests
{
    [Test]
    public void BlocksShadow()
    {
        var lox = new TestLox();
        var source = @"
var a = ""foo"";
var b = ""bar"";
{
    var a = ""car"";
    print a;
    print b;
}
print a;
";
        var output = lox.Run(source);
        Assert.AreEqual(3, output.Count);
        Assert.AreEqual("car", output[0]);
        Assert.AreEqual("bar", output[1]);
        Assert.AreEqual("foo", output[2]);
    }

    [Test]
    public void BlocksNest()
    {
        var lox = new TestLox();
        var source = @"
var a = ""foo"";
var b = ""bar"";
{
    var a = ""car"";
    print a;
    {
        var b = ""star"";
        print a;
        print b;
    }
    print b;
}
print a;
";
        var output = lox.Run(source);
        Assert.AreEqual(5, output.Count);
        Assert.AreEqual("car", output[0]);
        Assert.AreEqual("car", output[1]);
        Assert.AreEqual("star", output[2]);
        Assert.AreEqual("bar", output[3]);
        Assert.AreEqual("foo", output[4]);
    }

    [Test]
    public void FibWithoutRecursion()
    {

        var lox = new TestLox();
        var source = @"
fun fib(n) {
    var a = 1;
    var b = 1;
    var i = 1;
    while (i < n) {
        var c = a;
        a = b;
        b = c + a;
        i = i + 1;
    }
    return b;
}
print fib(1);
print fib(2);
print fib(3);
print fib(4);
";
        var output = lox.Run(source);
        Assert.AreEqual(4, output.Count);
        Assert.AreEqual("1", output[0]);
        Assert.AreEqual("2", output[1]);
        Assert.AreEqual("3", output[2]);
        Assert.AreEqual("5", output[3]);
    }
}
