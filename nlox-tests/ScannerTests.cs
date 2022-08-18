namespace nlox_tests;

using CraftingInterpreters;

public class ScannerTests
{
    [Test]
    public void UnterminatedStringIsNotAToken()
    {
        var scanner = new Scanner("\"Foo");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.EOF, tokens[0].type);
    }

    [Test]
    public void HandlesNewLines()
    {
        var scanner = new Scanner("var\nprint\nfun");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(1, tokens[0].line);
        Assert.AreEqual(2, tokens[1].line);
        Assert.AreEqual(3, tokens[2].line);
    }

    [Test]
    public void HandlesPrintIdentifier()
    {
        var scanner = new Scanner("print hello");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.PRINT, tokens[0].type);
        Assert.AreEqual(TokenType.IDENTIFIER, tokens[1].type);
        Assert.AreEqual("hello", tokens[1].lexeme);
    }

    [Test]
    public void HandlesPrintNumbers()
    {
        var scanner = new Scanner("print 12\nprint 12.42");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.PRINT, tokens[0].type);
        Assert.AreEqual(TokenType.NUMBER, tokens[1].type);
        Assert.AreEqual(12.0, tokens[1].literal);

        Assert.AreEqual(TokenType.PRINT, tokens[2].type);
        Assert.AreEqual(TokenType.NUMBER, tokens[3].type);
        Assert.AreEqual(12.42, tokens[3].literal);
    }

    [Test]
    public void HandlesUnNestedBlockComments()
    {
        var scanner = new Scanner("/* Comment\nOn multiple\nlines */\nprint");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.PRINT, tokens[0].type);
        Assert.AreEqual(4, tokens[0].line);
        Assert.AreEqual(TokenType.EOF, tokens[1].type);
    }

    [Test]
    public void NestedBlockComments()
    {
        var scanner = new Scanner("/* Comment\n/* With comments inside */\n*/\nprint");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.PRINT, tokens[0].type);
        Assert.AreEqual(4, tokens[0].line);
        Assert.AreEqual(TokenType.EOF, tokens[1].type);
    }

    [Test]
    public void HandlesTernaryScanning()
    {
        var scanner = new Scanner("condition ? ontrue : onfalse");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.IDENTIFIER, tokens[0].type);
        Assert.AreEqual(TokenType.QUESTION, tokens[1].type);
        Assert.AreEqual(TokenType.IDENTIFIER, tokens[2].type);
        Assert.AreEqual(TokenType.COLON, tokens[3].type);
        Assert.AreEqual(TokenType.IDENTIFIER, tokens[4].type);
    }
}