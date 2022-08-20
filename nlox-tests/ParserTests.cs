namespace nlox_tests;

using CraftingInterpreters;
using CraftingInterpreters.AstGen;

public class ParserTests
{
    [Test]
    public void SimpleTernaryParses()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.QUESTION, "?", null, 1),
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.COLON, ":", null, 1),
            new Token(TokenType.FALSE, "false", false, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens, ParserMode.Normal);

        var expr = parser.ParseOneExpr();

        Assert.IsNotNull(expr);
        Assert.IsTrue(expr is Ternary);
    }

    [Test]
    public void TernaryWithoutColonFailsToParse()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.QUESTION, "?", null, 1),
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.FALSE, "false", false, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens, ParserMode.Normal);

        var expr = parser.ParseOneExpr();

        Assert.IsNull(expr);
    }

    [Test]
    public void TernaryBindsWeakerThanEquals()
    {
        var trueEqualsFalse = new List<Token>
        {
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.EQUAL_EQUAL, "==", null, 1),
            new Token(TokenType.FALSE, "false", false, 1),
        };
        var tokens = new List<Token>();
        tokens.AddRange(trueEqualsFalse.ToList());
        tokens.Add(new Token(TokenType.QUESTION, "?", null, 1));
        tokens.AddRange(trueEqualsFalse.ToList());
        tokens.Add(new Token(TokenType.COLON, ":", null, 1));
        tokens.AddRange(trueEqualsFalse.ToList());
        tokens.Add(new Token(TokenType.EOF, "", null, 1));
        var parser = new Parser(tokens, ParserMode.Normal);

        var expr = parser.ParseOneExpr();

        if (expr is Ternary ternary)
        {
            Assert.IsTrue(ternary.condition is Binary);
            Assert.IsTrue(ternary.ifTrue is Binary);
            Assert.IsTrue(ternary.ifFalse is Binary);
            Assert.IsTrue((ternary.condition as Binary).left is Literal);
            Assert.IsTrue((ternary.condition as Binary).op.type is TokenType.EQUAL_EQUAL);
            Assert.IsTrue((ternary.condition as Binary).right is Literal);
            Assert.IsTrue((ternary.ifTrue as Binary).left is Literal);
            Assert.IsTrue((ternary.ifTrue as Binary).right is Literal);
            Assert.IsTrue((ternary.ifFalse as Binary).left is Literal);
            Assert.IsTrue((ternary.ifFalse as Binary).right is Literal);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void NestledOnTrueTernary()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.QUESTION, "?", null, 1),
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.QUESTION, "?", null, 1),
            new Token(TokenType.FALSE, "false", false, 1),
            new Token(TokenType.COLON, ":", null, 1),
            new Token(TokenType.FALSE, "false", false, 1),
            new Token(TokenType.COLON, ":", null, 1),
            new Token(TokenType.FALSE, "false", false, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens, ParserMode.Normal);

        var expr = parser.ParseOneExpr();

        Assert.IsNotNull(expr);
        Assert.IsTrue(expr is Ternary);
    }

    [Test]
    public void NestledOnFalseTernary()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.QUESTION, "?", null, 1),
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.COLON, ":", null, 1),
            new Token(TokenType.TRUE, "true", true, 1),
            new Token(TokenType.QUESTION, "?", null, 1),
            new Token(TokenType.FALSE, "false", false, 1),
            new Token(TokenType.COLON, ":", null, 1),
            new Token(TokenType.FALSE, "false", false, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens, ParserMode.Normal);

        var expr = parser.ParseOneExpr();

        Assert.IsNotNull(expr);
        Assert.IsTrue(expr is Ternary);

        var ternary = expr as Ternary;
        Assert.IsTrue(ternary.condition is Literal, $"condition is: {ternary.condition.GetType()}, expected Literal.");
        Assert.IsTrue(ternary.ifTrue is Literal, $"ifTrue is: {ternary.ifTrue.GetType()}, expected Literal.");
        Assert.IsTrue(ternary.ifFalse is Ternary, $"ifFalse is: {ternary.ifFalse.GetType()}, expected Ternary.");
    }
}
