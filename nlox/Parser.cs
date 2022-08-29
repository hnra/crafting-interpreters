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
            var decl = Declaration();
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
            return Expression();
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
            if (Match(TokenType.FUN))
            {
                return FunctionDeclaration("function");
            }
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    Function FunctionDeclaration(string kind)
    {
        var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
        Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
        var parameters = new List<Token>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }
                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
        var body = Block();
        return new Function(name, parameters, body);
    }

    Stmt VarDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
        Expr? initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }
        Consume(stmtEnds, "Expect ';' after variable declaration");
        return new Var(name, initializer);
    }

    Stmt Statement()
    {
        if (Match(TokenType.PRINT))
        {
            return PrintStatement();
        }
        if (Match(TokenType.LEFT_BRACE))
        {
            return new Block(Block());
        }
        if (Match(TokenType.IF))
        {
            return IfStatement();
        }
        if (Match(TokenType.WHILE))
        {
            return WhileStatement();
        }
        if (Match(TokenType.FOR))
        {
            return ForStatement();
        }
        return ExpressionStatement();
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
        var body = Statement();
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
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");
        var body = Statement();
        return new While(condition, body);
    }

    Stmt IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");
        var thenBranch = Statement();
        var elseBranch = Match(TokenType.ELSE) ? Statement() : null;
        return new If(condition, thenBranch, elseBranch);
    }

    List<Stmt> Block()
    {
        var stmts = new List<Stmt>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            var stmt = Declaration();
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
        var val = Expression();
        Consume(stmtEnds, "Expect ';' after value.");
        return new Print(val);
    }

    Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(stmtEnds, "Expect ';' after expression.");
        return new Expression(expr);
    }

    Expr Expression() => Assignment();

    Expr Assignment()
    {
        var expr = Or();

        if (Match(TokenType.EQUAL))
        {
            var equals = Previous();
            var value = Assignment();
            if (expr is Variable variable)
            {
                var name = variable.name;
                return new Assign(name, value);
            }
            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    Expr Or()
    {
        var expr = And();
        while (Match(TokenType.OR))
        {
            var op = Previous();
            var right = And();
            expr = new Logical(expr, op, right);
        }
        return expr;
    }

    Expr And()
    {
        var expr = Ternary();
        while (Match(TokenType.AND))
        {
            var op = Previous();
            var right = Ternary();
            expr = new Logical(expr, op, right);
        }
        return expr;
    }

    Expr Ternary()
    {
        var expr = Equality();
        while (Match(TokenType.QUESTION))
        {
            var ifTrue = Ternary();
            Consume(TokenType.COLON, "Expected ':' in ternary expression.");
            var ifFalse = Ternary();
            expr = new Ternary(expr, ifTrue, ifFalse);
        }
        return expr;
    }

    Expr Equality()
    {
        var expr = Comparison();
        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Comparison()
    {
        var expr = Term();
        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Term()
    {
        var expr = Factor();
        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            var op = Previous();
            var right = Factor();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Factor()
    {
        var expr = Unary();
        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            var op = Previous();
            var right = Unary();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            var op = Previous();
            var right = Unary();
            return new Unary(op, right);
        }
        return Call();
    }

    Expr Call()
    {
        var expr = Primary();
        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }
        return expr;
    }

    Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }
        var paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
        return new Call(callee, paren, arguments);
    }

    Expr Primary()
    {
        if (Match(TokenType.FALSE))
        {
            return new Literal(false);
        }
        if (Match(TokenType.TRUE))
        {
            return new Literal(true);
        }
        if (Match(TokenType.NIL))
        {
            return new Literal(null);
        }
        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Literal(Previous().literal);
        }
        if (Match(TokenType.LEFT_PAREN))
        {
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Grouping(expr);
        }
        if (Match(TokenType.IDENTIFIER))
        {
            return new Variable(Previous());
        }

        onError(Peek(), "Expect expression.");
        throw new ParseError();
    }

    Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        throw Error(Peek(), message);
    }

    Token Consume(TokenType[] types, string message)
    {
        if (types.Any(Check))
        {
            return Advance();
        }

        throw Error(Peek(), message);
    }

    ParseError Error(Token token, string message)
    {
        onError(token, message);
        return new ParseError();
    }

    void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().type == TokenType.SEMICOLON)
            {
                return;
            }
            switch (Peek().type)
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
            Advance();
        }
    }

    bool Match(params TokenType[] types)
    {
        if (types.Any(Check))
        {
            Advance();
            return true;
        }

        return false;
    }

    Token Peek() => tokens[current];
    Token Previous() => tokens[current - 1];
    bool IsAtEnd() => Peek().type == TokenType.EOF;
    bool Check(TokenType type) =>
        Peek().type == type;

    Token Advance()
    {
        if (!IsAtEnd())
        {
            current++;
        }
        return Previous();
    }
}
