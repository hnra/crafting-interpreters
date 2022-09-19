namespace NLoxTests;

using NLox;
using NLox.AstGen;

public class AstPrinterTests
{
    static readonly AstPrinter astPrinter = new AstPrinter();

    [Test]
    public void TestSimpleBinaryExpr()
    {
        var binaryExpr = new Binary(
            new Literal("left"),
            new Token(TokenType.AND, "and", null, 0),
            new Literal("right"));

        Assert.AreEqual("(and left right)", astPrinter.Print(binaryExpr));
    }

    [Test]
    public void TestSimpleUnaryExpr()
    {
        var unaryExpr = new Unary(
            new Token(TokenType.BANG, "!", null, 0),
            new Literal("right"));

        Assert.AreEqual("(! right)", astPrinter.Print(unaryExpr));
    }

    [Test]
    public void TestNestledExpr()
    {
        var unaryExpr = new Unary(
            new Token(TokenType.BANG, "!", null, 0),
            new Literal("right"));

        var binaryExpr = new Binary(
            unaryExpr,
            new Token(TokenType.AND, "and", null, 0),
            new Literal("right"));

        Assert.AreEqual("(and (! right) right)", astPrinter.Print(binaryExpr));
    }

    [Test]
    public void TestGroupingExpr()
    {
        var grouping = new Grouping(
            new Literal("right"));

        Assert.AreEqual("(group right)", astPrinter.Print(grouping));
    }
}
