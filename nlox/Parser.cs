namespace CraftingInterpreters;

using AstGen;

/// <summary>
/// Recursive descent parser from <see cref="Scanner"/> output to AST.
/// </summary>
public class Parser
{
    class ParseError : Exception { }
    public delegate void ErrorHandler(Token token, string message);
    public ErrorHandler? OnError;

    #region Fields and Constructors

    readonly List<Token> tokens;
    int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    #endregion

    #region Methods

    public List<Stmt> Parse()
    {
        var stmts = new List<Stmt>();
        while (!IsAtEndAndSkipSemicolons())
        {
            if (Match(TokenType.IMPORT))
            {
                try
                {
                    ImportPath();
                }
                catch (ParseError)
                {
                    Synchronize();
                }
                continue;
            }
            var decl = Declaration();
            if (decl != null)
            {
                stmts.Add(decl);
            }
        }
        return stmts;
    }

    public void ImportPath()
    {
        var pathToken = Consume(TokenType.STRING, "Expected string path after import statement.");
        Consume(TokenType.SEMICOLON, "Expected ';' after import path.");

        if (pathToken.literal is string pathStr)
        {
            try
            {
                var importPath = Path.GetFullPath(pathStr);
                if (!File.Exists(importPath))
                {
                    OnError?.Invoke(pathToken, "Import failed: cannot find file '{importPath}'");
                    throw new ParseError();
                }
                var fileContents = File.ReadAllText(importPath);
                var hadScannerError = false;
                var scanner = new Scanner(fileContents);
                scanner.onError += (line, msg) =>
                {
                    hadScannerError = true;
                    OnError?.Invoke(pathToken, $"Import failed ('{importPath}'[line: {line}]): {msg}");
                };
                var importedTokens = scanner.ScanTokens();
                if (hadScannerError)
                {
                    throw new ParseError();
                }
                if (importedTokens.Count > 0 && importedTokens[^1].type == TokenType.EOF)
                {
                    importedTokens.RemoveAt(importedTokens.Count - 1);
                }
                tokens.InsertRange(current, importedTokens);
            }
            catch (ParseError)
            {
                throw;
            }
            catch (Exception)
            {
                throw new ParseError();
            }
        }
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
            if (Match(TokenType.CLASS))
            {
                return ClassDeclaration();
            }
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    Class ClassDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect class name.");
        Variable? superclass = null;
        if (Match(TokenType.LESS))
        {
            Consume(TokenType.IDENTIFIER, "Expect superclass name.");
            superclass = new Variable(Previous());
        }
        Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");
        var methods = new List<Function>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(FunctionDeclaration("method"));
        }
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");
        return new Class(name, superclass, methods);
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
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration");
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
        if (Match(TokenType.RETURN))
        {
            return ReturnStatement();
        }
        return ExpressionStatement();
    }

    Stmt ReturnStatement()
    {
        var keyword = Previous();
        Expr? value = Check(TokenType.SEMICOLON) ? null : Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new Return(keyword, value);
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
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Print(val);
    }

    Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
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
            else if (expr is Get get)
            {
                return new Set(get.obj, get.name, value);
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
            else if (Match(TokenType.DOT))
            {
                var name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new Get(expr, name);
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
        if (Match(TokenType.THIS))
        {
            return new This(Previous());
        }
        if (Match(TokenType.SUPER))
        {
            var keyword = Previous();
            Consume(TokenType.DOT, "Expect '.' after 'super'.");
            var method = Consume(TokenType.IDENTIFIER, "Expect superclass method name.");
            return new Super(keyword, method);
        }

        OnError?.Invoke(Peek(), "Expect expression.");
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
        OnError?.Invoke(token, message);
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

    bool IsAtEndAndSkipSemicolons()
    {
        while (!IsAtEnd() && Match(TokenType.SEMICOLON)) { }
        return IsAtEnd();
    }

    #endregion
}
