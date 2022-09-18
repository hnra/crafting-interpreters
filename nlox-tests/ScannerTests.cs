namespace nlox_tests;

using CraftingInterpreters;

public class ScannerTests
{
    Scanner Create(string source) => new Scanner(source);

    [Test]
    public void UnterminatedStringIsNotAToken()
    {
        var scanner = Create("\"Foo");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.EOF, tokens[0].type);
    }

    [Test]
    public void HandlesNewLines()
    {
        var scanner = Create("var\nprint\nfun");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(1, tokens[0].line);
        Assert.AreEqual(2, tokens[1].line);
        Assert.AreEqual(3, tokens[2].line);
    }

    [Test]
    public void HandlesPrintIdentifier()
    {
        var scanner = Create("print hello");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.PRINT, tokens[0].type);
        Assert.AreEqual(TokenType.IDENTIFIER, tokens[1].type);
        Assert.AreEqual("hello", tokens[1].lexeme);
    }

    [Test]
    public void HandlesPrintNumbers()
    {
        var scanner = Create("print 12\nprint 12.42");

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
        var scanner = Create("/* Comment\nOn multiple\nlines */\nprint");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.PRINT, tokens[0].type);
        Assert.AreEqual(4, tokens[0].line);
        Assert.AreEqual(TokenType.EOF, tokens[1].type);
    }

    [Test]
    public void NestedBlockComments()
    {
        var scanner = Create("/* Comment\n/* With comments inside */\n*/\nprint");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.PRINT, tokens[0].type);
        Assert.AreEqual(4, tokens[0].line);
        Assert.AreEqual(TokenType.EOF, tokens[1].type);
    }

    [Test]
    public void HandlesTernaryScanning()
    {
        var scanner = Create("condition ? ontrue : onfalse");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.IDENTIFIER, tokens[0].type);
        Assert.AreEqual(TokenType.QUESTION, tokens[1].type);
        Assert.AreEqual(TokenType.IDENTIFIER, tokens[2].type);
        Assert.AreEqual(TokenType.COLON, tokens[3].type);
        Assert.AreEqual(TokenType.IDENTIFIER, tokens[4].type);
    }

    [Test]
    public void ScansImport()
    {
        var scanner = Create("import \"./path/to/file.lox\";");
        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.IMPORT, tokens[0].type);
        Assert.AreEqual(TokenType.STRING, tokens[1].type);
    }

    [Test]
    public void ScansBrackets()
    {
        var scanner = Create("[1, 2, 3];");
        var tokens = scanner.ScanTokens();

        Assert.AreEqual(TokenType.LEFT_BRACKET, tokens[0].type);
        Assert.AreEqual(TokenType.NUMBER, tokens[1].type);
        Assert.AreEqual(TokenType.COMMA, tokens[2].type);
        Assert.AreEqual(TokenType.NUMBER, tokens[3].type);
        Assert.AreEqual(TokenType.COMMA, tokens[4].type);
        Assert.AreEqual(TokenType.NUMBER, tokens[5].type);
        Assert.AreEqual(TokenType.RIGHT_BRACKET, tokens[6].type);
    }
}