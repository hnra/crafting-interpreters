namespace CraftingInterpreters;

using AstGen;

public class ParseError : Exception { }

/// <summary>
/// Recursive descent parser from <see cref="Scanner"/> output to AST.
/// </summary>
public class Parser
{
    readonly List<Token> tokens;
    int current = 0;

    Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public Expr? Parse()
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

    Expr Expression() => this.Equality();

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

        Lox.Error(this.Peek(), "Expect expression.");
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

    ParseError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError();
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
        this.IsAtEnd() ? false : this.Peek().type == type;

    Token Advance()
    {
        if (!IsAtEnd())
        {
            this.current++;
        }
        return this.Previous();
    }
}
