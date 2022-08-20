namespace nlox_tests;

using CraftingInterpreters;
using CraftingInterpreters.AstGen;

public class InterpreterTests
{
    static Interpreter interpreter = new Interpreter((msg) => { }, InterpreterMode.Normal, (err) => { });

    [Test]
    public void LiteralNilExpr()
        => Assert.AreEqual("nil", interpreter.Interpret(new Literal(null)));

    [Test]
    public void LiteralTrueExpr()
        => Assert.AreEqual("true", interpreter.Interpret(new Literal(true)));

    [Test]
    public void LiteralFalseExpr()
        => Assert.AreEqual("false", interpreter.Interpret(new Literal(false)));

    [Test]
    public void LiteralIntExpr()
        => Assert.AreEqual("10", interpreter.Interpret(new Literal(10.0)));

    [Test]
    public void LiteralDoubleExpr()
        => Assert.AreEqual("12.3", interpreter.Interpret(new Literal(12.3)));

    [Test]
    public void LiteralStringExpr()
        => Assert.AreEqual("foo", interpreter.Interpret(new Literal("foo")));

    [Test]
    public void GroupingExpr()
        => Assert.AreEqual("foo", interpreter.Interpret(new Grouping(new Literal("foo"))));

    [Test]
    public void UnaryBangTruthExpr()
    {
        var truthy = new Literal(true);
        var unaryToken = new Token(TokenType.BANG, "!", null, 1);
        var unary = new Unary(unaryToken, truthy);

        var output = interpreter.Interpret(unary);

        Assert.AreEqual("false", output);
    }

    [Test]
    public void UnaryBangFalseExpr()
    {
        var falsy = new Literal(false);
        var unaryToken = new Token(TokenType.BANG, "!", null, 1);
        var unary = new Unary(unaryToken, falsy);

        var output = interpreter.Interpret(unary);

        Assert.AreEqual("true", output);
    }

    [Test]
    public void UnaryBangNilIsTrue()
    {
        var falsy = new Literal(null);
        var unaryToken = new Token(TokenType.BANG, "!", null, 1);
        var unary = new Unary(unaryToken, falsy);

        var output = interpreter.Interpret(unary);

        Assert.AreEqual("true", output);
    }

    [Test]
    public void UnaryMinusDoubleExpr()
    {
        var d = new Literal(12.3);
        var unaryToken = new Token(TokenType.MINUS, "-", null, 1);
        var unary = new Unary(unaryToken, d);

        var output = interpreter.Interpret(unary);

        Assert.AreEqual("-12.3", output);
    }

    [Test]
    public void UnaryMinusBoolThrows()
    {
        var falsy = new Literal(false);
        var unaryToken = new Token(TokenType.MINUS, "-", null, 1);
        var unary = new Unary(unaryToken, falsy);

        Assert.Throws(typeof(RuntimeException), () => interpreter.VisitUnaryExpr(unary));
    }

    [Test]
    public void BinaryAddition()
    {
        var left = new Literal(1.0);
        var right = new Literal(2.0);
        var op = new Token(TokenType.PLUS, "+", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("3", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryAdditionNumberStringThrows()
    {
        var left = new Literal(1.0);
        var right = new Literal("henrik");
        var op = new Token(TokenType.PLUS, "+", null, 1);
        var binary = new Binary(left, op, right);

        Assert.Throws(typeof(RuntimeException), () => interpreter.VisitBinaryExpr(binary));
    }

    [Test]
    public void BinaryConcat()
    {
        var left = new Literal("apa");
        var right = new Literal("bepa");
        var op = new Token(TokenType.PLUS, "+", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("apabepa", interpreter.Interpret(binary));
    }

    [Test]
    public void BinarySubtraction()
    {
        var left = new Literal(1.0);
        var right = new Literal(2.0);
        var op = new Token(TokenType.MINUS, "-", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("-1", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryMultiplication()
    {
        var left = new Literal(2.0);
        var right = new Literal(4.5);
        var op = new Token(TokenType.STAR, "*", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("9", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryDivision()
    {
        var left = new Literal(5.0);
        var right = new Literal(2.0);
        var op = new Token(TokenType.SLASH, "/", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("2.5", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryEq()
    {
        var left = new Literal(null);
        var right = new Literal(null);
        var op = new Token(TokenType.EQUAL_EQUAL, "==", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("true", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryNeq()
    {
        var left = new Literal("apa");
        var right = new Literal("bepa");
        var op = new Token(TokenType.BANG_EQUAL, "!=", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("true", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryGeq()
    {
        var left = new Literal(1.0);
        var right = new Literal(2.0);
        var op = new Token(TokenType.GREATER_EQUAL, ">=", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("false", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryGe()
    {
        var left = new Literal(1.0);
        var right = new Literal(2.0);
        var op = new Token(TokenType.GREATER, ">", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("false", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryLeq()
    {
        var left = new Literal(1.0);
        var right = new Literal(2.0);
        var op = new Token(TokenType.LESS_EQUAL, "<=", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("true", interpreter.Interpret(binary));
    }

    [Test]
    public void BinaryLe()
    {
        var left = new Literal(1.0);
        var right = new Literal(2.0);
        var op = new Token(TokenType.LESS, "<", null, 1);
        var binary = new Binary(left, op, right);

        Assert.AreEqual("true", interpreter.Interpret(binary));
    }

    [Test]
    public void TernaryIfTrue()
    {
        var op = new Token(TokenType.EQUAL_EQUAL, "==", null, 1);
        var condition = new Binary(new Literal(1.0), op, new Literal(1.0));
        var ifTrue = new Literal(true);
        var ifFalse = new Literal(false);
        var ternary = new Ternary(condition, ifTrue, ifFalse);

        Assert.AreEqual("true", interpreter.Interpret(ternary));
    }

    [Test]
    public void TernaryIfFalse()
    {
        var op = new Token(TokenType.EQUAL_EQUAL, "==", null, 1);
        var condition = new Binary(new Literal(1.0), op, new Literal(2.0));
        var ifTrue = new Literal(true);
        var ifFalse = new Literal(false);
        var ternary = new Ternary(condition, ifTrue, ifFalse);

        Assert.AreEqual("false", interpreter.Interpret(ternary));
    }

    [Test]
    public void TernaryIfFalseDoesntEvalIfTrue()
    {
        var op = new Token(TokenType.EQUAL_EQUAL, "==", null, 1);
        var condition = new Binary(new Literal(1.0), op, new Literal(1.0));
        var ifTrue = new Literal(true);
        var ifFalse = new Unary(new Token(TokenType.BANG, "!", null, 1), new Literal(null));
        var ternary = new Ternary(condition, ifTrue, ifFalse);

        Assert.AreEqual("true", interpreter.Interpret(ternary));
    }

    [Test]
    public void TernaryIfTrueDoesntEvalIfFalse()
    {
        var op = new Token(TokenType.EQUAL_EQUAL, "==", null, 1);
        var condition = new Binary(new Literal(1.0), op, new Literal(14.0));
        var ifTrue = new Unary(new Token(TokenType.BANG, "!", null, 1), new Literal(null));
        var ifFalse = new Literal(false);
        var ternary = new Ternary(condition, ifTrue, ifFalse);

        Assert.AreEqual("false", interpreter.Interpret(ternary));
    }

    [Test]
    public void TernaryIfFalseThrows()
    {
        var op = new Token(TokenType.EQUAL_EQUAL, "==", null, 1);
        var condition = new Binary(new Literal(1.0), op, new Literal(2.0));
        var ifTrue = new Literal(true);
        var ifFalse = new Unary(new Token(TokenType.MINUS, "-", null, 1), new Literal(null));
        var ternary = new Ternary(condition, ifTrue, ifFalse);

        Assert.Throws(typeof(RuntimeException), () => interpreter.VisitTernaryExpr(ternary));
    }
}