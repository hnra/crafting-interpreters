namespace CraftingInterpreters;

using AstGen;

public enum ParserMode
{
    Normal, Repl,
}

public class ParseError : Exception { }

/// <summary>
/// Recursive descent parser from <see cref="Scanner"/> output to AST.
/// </summary>
public class Parser
{
    readonly List<Token> tokens;
    int current = 0;
    readonly TokenType[] stmtEnds;
    Action<Token, string> onError;

    public Parser(List<Token> tokens, ParserMode mode, Action<Token, string> onError)
    {
        this.tokens = tokens;
        this.onError = onError;
        this.stmtEnds = mode switch
        {
            ParserMode.Repl => new[] { TokenType.SEMICOLON, TokenType.EOF },
            _ => new[] { TokenType.SEMICOLON },
        };
    }

    public List<Stmt> Parse()
    {
        var stmts = new List<Stmt>();
        while (!IsAtEnd())
        {
            var decl = this.Declaration();
            if (decl != null)
            {
                stmts.Add(decl);
            }
        }
        return stmts;
    }

    public Expr? ParseOneExpr()
    {
        try
        {
            return this.Expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }

    Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.VAR))
            {
                return VarDeclaration();
            }
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    Stmt VarDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
        Expr? initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }
        Consume(this.stmtEnds, "Expect ';' after variable declaration");
        return new Var(name, initializer);
    }

    Stmt Statement()
    {
        if (Match(TokenType.PRINT))
        {
            return this.PrintStatement();
        }
        if (Match(TokenType.LEFT_BRACE))
        {
            return new Block(this.Block());
        }
        if (Match(TokenType.IF))
        {
            return this.IfStatement();
        }
        if (Match(TokenType.WHILE))
        {
            return this.WhileStatement();
        }
        if (Match(TokenType.FOR))
        {
            return this.ForStatement();
        }
        return this.ExpressionStatement();
    }

    Stmt ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(TokenType.VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect to see a ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.SEMICOLON))
        {
            increment = Expression();
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        // Desugar to while-loop.
        var body = this.Statement();
        if (increment != null)
        {
            body = new Block(new() { body, new Expression(increment) });
        }
        condition ??= new Literal(true);
        body = new While(condition, body);
        if (initializer != null)
        {
            body = new Block(new() { initializer, body });
        }

        return body;
    }

    Stmt WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = this.Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");
        var body = this.Statement();
        return new While(condition, body);
    }

    Stmt IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = this.Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");
        var thenBranch = this.Statement();
        var elseBranch = Match(TokenType.ELSE) ? this.Statement() : null;
        return new If(condition, thenBranch, elseBranch);
    }

    List<Stmt> Block()
    {
        var stmts = new List<Stmt>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            var stmt = this.Declaration();
            if (stmt != null)
            {
                stmts.Add(stmt);
            }
        }
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return stmts;
    }

    Stmt PrintStatement()
    {
        var val = this.Expression();
        this.Consume(this.stmtEnds, "Expect ';' after value.");
        return new Print(val);
    }

    Stmt ExpressionStatement()
    {
        var expr = this.Expression();
        this.Consume(this.stmtEnds, "Expect ';' after expression.");
        return new Expression(expr);
    }

    Expr Expression() => this.Assignment();

    Expr Assignment()
    {
        var expr = this.Or();

        if (this.Match(TokenType.EQUAL))
        {
            var equals = this.Previous();
            var value = this.Assignment();
            if (expr is Variable variable)
            {
                var name = variable.name;
                return new Assign(name, value);
            }
            this.Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    Expr Or()
    {
        var expr = this.And();
        while (Match(TokenType.OR))
        {
            var op = this.Previous();
            var right = this.And();
            expr = new Logical(expr, op, right);
        }
        return expr;
    }

    Expr And()
    {
        var expr = this.Ternary();
        while (Match(TokenType.AND))
        {
            var op = this.Previous();
            var right = this.Ternary();
            expr = new Logical(expr, op, right);
        }
        return expr;
    }

    Expr Ternary()
    {
        var expr = this.Equality();
        while (this.Match(TokenType.QUESTION))
        {
            var ifTrue = this.Ternary();
            this.Consume(TokenType.COLON, "Expected ':' in ternary expression.");
            var ifFalse = this.Ternary();
            expr = new Ternary(expr, ifTrue, ifFalse);
        }
        return expr;
    }

    Expr Equality()
    {
        var expr = this.Comparison();
        while (this.Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            var op = this.Previous();
            var right = this.Comparison();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Comparison()
    {
        var expr = this.Term();
        while (this.Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var op = this.Previous();
            var right = this.Term();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Term()
    {
        var expr = this.Factor();
        while (this.Match(TokenType.MINUS, TokenType.PLUS))
        {
            var op = this.Previous();
            var right = this.Factor();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Factor()
    {
        var expr = this.Unary();
        while (this.Match(TokenType.SLASH, TokenType.STAR))
        {
            var op = this.Previous();
            var right = this.Unary();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            var op = this.Previous();
            var right = this.Unary();
            return new Unary(op, right);
        }
        return this.Primary();
    }

    Expr Primary()
    {
        if (this.Match(TokenType.FALSE))
        {
            return new Literal(false);
        }
        if (this.Match(TokenType.TRUE))
        {
            return new Literal(true);
        }
        if (this.Match(TokenType.NIL))
        {
            return new Literal(null);
        }
        if (this.Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Literal(this.Previous().literal);
        }
        if (this.Match(TokenType.LEFT_PAREN))
        {
            var expr = this.Expression();
            this.Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Grouping(expr);
        }
        if (this.Match(TokenType.IDENTIFIER))
        {
            return new Variable(this.Previous());
        }

        this.onError(this.Peek(), "Expect expression.");
        throw new ParseError();
    }

    Token Consume(TokenType type, string message)
    {
        if (this.Check(type))
        {
            return this.Advance();
        }

        throw Error(this.Peek(), message);
    }

    Token Consume(TokenType[] types, string message)
    {
        if (types.Any(this.Check))
        {
            return this.Advance();
        }

        throw Error(this.Peek(), message);
    }

    ParseError Error(Token token, string message)
    {
        this.onError(token, message);
        return new ParseError();
    }

    void Synchronize()
    {
        this.Advance();
        while (!this.IsAtEnd())
        {
            if (this.Previous().type == TokenType.SEMICOLON)
            {
                return;
            }
            switch (this.Peek().type)
            {
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.FUN:
                case TokenType.IF:
                case TokenType.PRINT:
                case TokenType.RETURN:
                case TokenType.VAR:
                case TokenType.WHILE:
                    return;
            }
            this.Advance();
        }
    }

    bool Match(params TokenType[] types)
    {
        if (types.Any(this.Check))
        {
            this.Advance();
            return true;
        }

        return false;
    }

    Token Peek() => this.tokens[this.current];
    Token Previous() => this.tokens[this.current - 1];
    bool IsAtEnd() => this.Peek().type == TokenType.EOF;
    bool Check(TokenType type) =>
        this.Peek().type == type;

    Token Advance()
    {
        if (!IsAtEnd())
        {
            this.current++;
        }
        return this.Previous();
    }
}
