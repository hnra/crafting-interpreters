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
        var parser = new Parser(tokens);

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
        var parser = new Parser(tokens);

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
        var parser = new Parser(tokens);

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
        var parser = new Parser(tokens);

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
        var parser = new Parser(tokens);

        var expr = parser.ParseOneExpr();

        Assert.IsNotNull(expr);
        Assert.IsTrue(expr is Ternary);

        var ternary = expr as Ternary;
        Assert.IsTrue(ternary.condition is Literal, $"condition is: {ternary.condition.GetType()}, expected Literal.");
        Assert.IsTrue(ternary.ifTrue is Literal, $"ifTrue is: {ternary.ifTrue.GetType()}, expected Literal.");
        Assert.IsTrue(ternary.ifFalse is Ternary, $"ifFalse is: {ternary.ifFalse.GetType()}, expected Ternary.");
    }

    [Test]
    public void SimpleFunctionCall()
    {
        var tokens = new List<Token> {
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var expr = parser.ParseOneExpr();

        Assert.IsNotNull(expr);
        Assert.IsTrue(expr is Call);
        CollectionAssert.IsEmpty((expr as Call).arguments);
    }

    [Test]
    public void CallWithOneArgument()
    {
        var tokens = new List<Token> {
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.NUMBER, "42", 42, 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var expr = parser.ParseOneExpr();

        Assert.IsNotNull(expr);
        Assert.IsTrue(expr is Call);
        CollectionAssert.IsNotEmpty((expr as Call).arguments);
    }

    [Test]
    public void CallWithTwoArguments()
    {
        var tokens = new List<Token> {
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.NUMBER, "42", 42, 1),
            new Token(TokenType.COMMA, ",", null, 1),
            new Token(TokenType.STRING, "bar", "bar", 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var expr = parser.ParseOneExpr();

        Assert.IsNotNull(expr);
        Assert.IsTrue(expr is Call);
        Assert.AreEqual(2, (expr as Call).arguments.Count);
    }

    [Test]
    public void MaxArgsIs255()
    {
        const int maxArgs = 255;
        var tokens = new List<Token> {
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
        };
        foreach (var i in Enumerable.Range(0, maxArgs + 1))
        {
            tokens.Add(new Token(TokenType.NUMBER, i.ToString(), i, 1));
            if (i < maxArgs)
            {
                tokens.Add(new Token(TokenType.COMMA, ",", null, 1));
            }
        }
        tokens.AddRange(new List<Token>() {
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        });
        var hadError = false;
        var parser = new Parser(tokens);
        parser.OnError += (token, msg) =>
        {
            hadError = true;
        };

        var expr = parser.ParseOneExpr();

        Assert.IsTrue(hadError);
    }

    [Test]
    public void SimpleFunctionDeclaration()
    {
        var tokens = new List<Token> {
            new Token(TokenType.FUN, "fun", null, 1),
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.LEFT_BRACE, "{", null, 1),
            new Token(TokenType.RIGHT_BRACE, "}", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var stmts = parser.Parse();

        CollectionAssert.IsNotEmpty(stmts);
        var declaration = stmts[0];
        Assert.IsTrue(declaration is Function);
    }

    [Test]
    public void DeclarationWithOneArgument()
    {
        var tokens = new List<Token> {
            new Token(TokenType.FUN, "fun", null, 1),
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.IDENTIFIER, "apa", null, 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.LEFT_BRACE, "{", null, 1),
            new Token(TokenType.RIGHT_BRACE, "}", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var stmts = parser.Parse();

        CollectionAssert.IsNotEmpty(stmts);
        var declaration = stmts[0];
        Assert.IsTrue(declaration is Function);
        Assert.AreEqual(1, (declaration as Function).parameters.Count);
    }

    [Test]
    public void DeclarationWithTwoArguments()
    {
        var tokens = new List<Token> {
            new Token(TokenType.FUN, "fun", null, 1),
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.IDENTIFIER, "apa", null, 1),
            new Token(TokenType.COMMA, ",", null, 1),
            new Token(TokenType.IDENTIFIER, "bepa", null, 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.LEFT_BRACE, "{", null, 1),
            new Token(TokenType.RIGHT_BRACE, "}", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var stmts = parser.Parse();

        CollectionAssert.IsNotEmpty(stmts);
        var declaration = stmts[0];
        Assert.IsTrue(declaration is Function);
        Assert.AreEqual(2, (declaration as Function).parameters.Count);
    }

    [Test]
    public void AssignToThisIsDisallowed()
    {
        var tokens = new List<Token> {
            new Token(TokenType.FUN, "fun", null, 1),
            new Token(TokenType.IDENTIFIER, "foo", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.LEFT_BRACE, "{", null, 1),
            new Token(TokenType.THIS, "this", null, 1),
            new Token(TokenType.EQUAL, "=", null, 1),
            new Token(TokenType.IDENTIFIER, "bepa", null, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.RIGHT_BRACE, "}", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var hadError = false;
        var parser = new Parser(tokens);
        parser.OnError += (token, msg) =>
        {
            hadError = true;
        };

        var stmts = parser.Parse();

        Assert.IsTrue(hadError);
    }

    [Test]
    public void CanParseImportStmt()
    {
        var helloLib = TestUtilties.GetTestFilePath("hello-lib.lox");
        var tokens = new List<Token> {
            new Token(TokenType.IMPORT, "import", null, 1),
            new Token(TokenType.STRING, $"\"{helloLib}\"", helloLib, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var stmts = parser.Parse();

        Assert.AreEqual(1, stmts.Count);
        Assert.IsTrue(stmts[0] is Function);
    }

    [Test]
    public void ImportMissingFileFails()
    {
        string badPath = $"./{Guid.NewGuid()}.lox";
        var tokens = new List<Token> {
            new Token(TokenType.IMPORT, "import", null, 1),
            new Token(TokenType.STRING, $"\"{badPath}\"", badPath, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var hasFailed = false;
        var parser = new Parser(tokens);
        parser.OnError += (token, msg) =>
        {
            hasFailed = true;
        };

        var stmts = parser.Parse();

        Assert.IsTrue(hasFailed);
    }

    [Test]
    public void CanParseEmptyFile()
    {
        var tokens = new List<Token> {
            new Token(TokenType.EOF, "", null, 1),
        };
        var hasFailed = false;
        var parser = new Parser(tokens);
        parser.OnError += (token, msg) =>
        {
            hasFailed = true;
        };

        var stmts = parser.Parse();

        Assert.IsFalse(hasFailed);
        CollectionAssert.IsEmpty(stmts);
    }

    [Test]
    public void CanParseLonelySemicolons()
    {
        var tokens = new List<Token> {
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var hasFailed = false;
        var parser = new Parser(tokens);
        parser.OnError += (token, msg) =>
        {
            hasFailed = true;
        };

        var stmts = parser.Parse();

        Assert.IsFalse(hasFailed);
        CollectionAssert.IsEmpty(stmts);
    }

    [Test]
    public void ForLoopsGetDesugared()
    {
        var tokens = new List<Token> {
            new Token(TokenType.FOR, "for", null, 1),
            new Token(TokenType.LEFT_PAREN, "(", null, 1),
            new Token(TokenType.VAR, "var", null, 1),
            new Token(TokenType.IDENTIFIER, "i", null, 1),
            new Token(TokenType.EQUAL, "=", null, 1),
            new Token(TokenType.NUMBER, "0", 0, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.IDENTIFIER, "i", null, 1),
            new Token(TokenType.LESS, "<", null, 1),
            new Token(TokenType.NUMBER, "10", 10, 1),
            new Token(TokenType.SEMICOLON, ";", null, 1),
            new Token(TokenType.IDENTIFIER, "i", null, 1),
            new Token(TokenType.EQUAL, "=", null, 1),
            new Token(TokenType.IDENTIFIER, "i", null, 1),
            new Token(TokenType.PLUS, "+", null, 1),
            new Token(TokenType.NUMBER, "1", 1, 1),
            new Token(TokenType.RIGHT_PAREN, ")", null, 1),
            new Token(TokenType.LEFT_BRACE, "{", null, 1),
            new Token(TokenType.RIGHT_BRACE, "}", null, 1),
            new Token(TokenType.EOF, "", null, 1),
        };
        var parser = new Parser(tokens);

        var stmts = parser.Parse();
        CollectionAssert.IsNotEmpty(stmts);
        var declaration = stmts[0];
        Assert.IsInstanceOf(typeof(Block), declaration);
        var block = (Block)declaration;
        Assert.IsInstanceOf(typeof(Var), block.statements[0]);
        var decl = (Var)block.statements[0];
        Assert.AreEqual("i", decl.name.lexeme);
        Assert.IsInstanceOf(typeof(Literal), decl.initializer);
        Assert.AreEqual(0, ((Literal)decl.initializer).value);
        Assert.IsInstanceOf(typeof(While), block.statements[1]);
        var whileStmt = (While)block.statements[1];
        Assert.IsInstanceOf(typeof(Binary), whileStmt.condition);
        var condition = (Binary)whileStmt.condition;
        Assert.AreEqual(TokenType.LESS, condition.op.type);
        Assert.IsInstanceOf(typeof(Variable), condition.left);
        var condLeft = (Variable)condition.left;
        Assert.IsInstanceOf(typeof(Literal), condition.right);
        var condRight = (Literal)condition.right;
        Assert.AreEqual("i", condLeft.name.lexeme);
        Assert.AreEqual(10, condRight.value);
        Assert.IsInstanceOf(typeof(Block), whileStmt.body);
        var whileBody = (Block)whileStmt.body;
        Assert.IsInstanceOf(typeof(Expression), whileBody.statements[1]);
        var increment = (Expression)whileBody.statements[1];
        Assert.IsInstanceOf(typeof(Assign), increment.expression);
        var assign = (Assign)increment.expression;
        Assert.AreEqual("i", assign.name.lexeme);
        Assert.IsInstanceOf(typeof(Binary), assign.value);
        var binaryIncr = (Binary)assign.value;
        Assert.IsInstanceOf(typeof(Variable), binaryIncr.left);
        var left = (Variable)binaryIncr.left;
        Assert.IsInstanceOf(typeof(Literal), binaryIncr.right);
        var right = (Literal)binaryIncr.right;
        Assert.AreEqual(TokenType.PLUS, binaryIncr.op.type);
        Assert.AreEqual("i", left.name.lexeme);
        Assert.AreEqual(1, right.value);
    }
}
