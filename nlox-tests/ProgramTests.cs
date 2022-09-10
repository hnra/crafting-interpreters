namespace nlox_tests;

using CraftingInterpreters;

class TestLox
{
    public bool hadError = false;

    public List<string> Run(string source)
    {
        var scanner = new Scanner(source, (line, msg) =>
        {
            hadError = true;
        });
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens, ParserMode.Normal, (tokens, msg) =>
        {
            hadError = true;
        });
        var stmts = parser.Parse();

        var output = new List<string>();
        var interpreter = new Interpreter(
            (msg) =>
                {
                    output.Add(msg);
                },
            InterpreterMode.Normal,
            (msg) =>
            {
                hadError = true;
            });
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
    public void ClassInitializer()
    {

        var lox = new TestLox();
        var source = @"
class Henrik {
    init(lastname, age) {
        this.lastname = lastname;
        this.age = age;
    }
}
var henrik = Henrik(""Andersson"", 26);
print henrik.lastname;
print henrik.age;
";
        var output = lox.Run(source);
        Assert.AreEqual(2, output.Count);
        Assert.AreEqual("Andersson", output[0]);
        Assert.AreEqual("26", output[1]);
    }

    [Test]
    public void CanReturnEmptyFromInitializer()
    {

        var lox = new TestLox();
        var source = @"
class Foo {
    init(shouldInit) {
        this.hasInited = false;
        if (!shouldInit) {
            return;
        }
        this.hasInited = true;
    }
}
var foo = Foo(false);
print foo.hasInited;
var foo2 = Foo(true);
print foo2.hasInited;
";
        var output = lox.Run(source);
        Assert.AreEqual(2, output.Count);
        Assert.AreEqual("false", output[0]);
        Assert.AreEqual("true", output[1]);
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

    [Test]
    public void ClassCanInherit()
    {

        var lox = new TestLox();
        var source = @"
class Donut { }
class GlazedDonut < Donut { }
var glazed = GlazedDonut();
print glazed;
";
        var output = lox.Run(source);
        Assert.AreEqual(1, output.Count);
        Assert.AreEqual("GlazedDonut instance", output[0]);
    }

    [Test]
    public void ClassCannotInheritFromItself()
    {

        var lox = new TestLox();
        var source = @"
class Donut < Donut { }
";
        var output = lox.Run(source);
        Assert.IsTrue(lox.hadError);
    }

    [Test]
    public void ClassCannotInheritFromVariable()
    {

        var lox = new TestLox();
        var source = @"
var apa = 1;
class Donut < apa { }
";
        var output = lox.Run(source);
        Assert.IsTrue(lox.hadError);
    }

    [Test]
    public void ClassCanInheritFromVariableWhichIsAClass()
    {

        var lox = new TestLox();
        var source = @"
class Donut { }
var apa = Donut;
class GlazedDonut < apa { }
";
        var output = lox.Run(source);
        Assert.IsFalse(lox.hadError);
    }

    [Test]
    public void MethodsAreInherited()
    {

        var lox = new TestLox();
        var source = @"
class Donut {
    cook() {
        print ""Fry until golden brown."";
    }
}
class GlazedDonut < Donut { }
var glazed = GlazedDonut();
glazed.cook();
";
        var output = lox.Run(source);
        Assert.AreEqual(1, output.Count);
        Assert.AreEqual("Fry until golden brown.", output[0]);
    }
}
