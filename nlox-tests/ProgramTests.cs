namespace nlox_tests;

using CraftingInterpreters;

class TestLox
{
    public List<string> Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens, ParserMode.Normal);
        var stmts = parser.Parse();

        var output = new List<string>();
        var interpreter = new Interpreter((msg) =>
        {
            output.Add(msg);
        }, InterpreterMode.Normal);
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
}