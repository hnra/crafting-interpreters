namespace CraftingInterpreters;

public enum TokenType
{
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

    BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,

    IDENTIFIER, STRING, NUMBER,

    AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
    PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

    QUESTION, COLON,

    EOF
}

public record Token(TokenType type, string lexeme, object? literal, int line)
{
    public override string ToString()
    {
        return $"{type} {lexeme} {literal}";
    }
}

public class Scanner
{
    readonly string source;
    readonly List<Token> tokens = new List<Token>();
    readonly Action<int, string> onError;
    int start = 0;
    int current = 0;
    int line = 1;

    static readonly Dictionary<string, TokenType> Keywords = new() {
        {"and", TokenType.AND},
        {"class", TokenType.CLASS},
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"for", TokenType.FOR},
        {"fun", TokenType.FUN},
        {"if", TokenType.IF},
        {"nil", TokenType.NIL},
        {"or", TokenType.OR},
        {"print", TokenType.PRINT},
        {"return", TokenType.RETURN},
        {"super", TokenType.SUPER},
        {"this", TokenType.THIS},
        {"true", TokenType.TRUE},
        {"var", TokenType.VAR},
        {"while", TokenType.WHILE},
    };

    public Scanner(string source, Action<int, string> onError)
    {
        this.source = source ?? "";
        this.onError = onError;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanAndAddToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }

    void ScanAndAddToken()
    {
        var token = ScanToken();

        if (token != null)
        {
            tokens.Add(token);
        }
    }

    Token? ScanToken()
    {
        var c = Advance();

        switch (c)
        {
            case '(':
                return CreateToken(TokenType.LEFT_PAREN);
            case ')':
                return CreateToken(TokenType.RIGHT_PAREN);
            case '{':
                return CreateToken(TokenType.LEFT_BRACE);
            case '}':
                return CreateToken(TokenType.RIGHT_BRACE);
            case ',':
                return CreateToken(TokenType.COMMA);
            case '.':
                return CreateToken(TokenType.DOT);
            case '-':
                return CreateToken(TokenType.MINUS);
            case '+':
                return CreateToken(TokenType.PLUS);
            case ';':
                return CreateToken(TokenType.SEMICOLON);
            case '*':
                return CreateToken(TokenType.STAR);
            case '!':
                return CreateToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
            case '=':
                return CreateToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
            case '<':
                return CreateToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
            case '>':
                return CreateToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
            case '/':
                if (Match('/'))
                {
                    return ParseComment();
                }
                else if (Match('*'))
                {
                    return ParseBlockComment();
                }
                return CreateToken(TokenType.SLASH);
            case '\n':
                line++;
                return null;
            case ' ':
            case '\r':
            case '\t':
                return null;
            case '"':
                return ParseString();
            case '?':
                return CreateToken(TokenType.QUESTION);
            case ':':
                return CreateToken(TokenType.COLON);
            default:
                if (IsDigit(c))
                {
                    return ParseNumber();
                }
                else if (IsAlpha(c))
                {
                    return ParseIdentifier();
                }
                onError(line, $"Unexpected character '{c}'.");
                return null;
        }
    }

    Token? ParseComment()
    {
        while (!IsAtEnd() && Peek() != '\n')
        {
            Advance();
        }
        return null;
    }

    Token? ParseBlockComment()
    {
        var depth = 1;

        while (!IsAtEnd())
        {
            if (Peek() == '/' && DoublePeek() == '*')
            {
                depth++;
            }

            if (Peek() == '*' && DoublePeek() == '/')
            {
                depth--;

                if (depth == 0)
                {
                    Advance();
                    Advance();
                    break;
                }
            }

            if (Peek() == '\n')
            {
                line++;
            }

            Advance();
        }

        if (depth > 0)
        {
            onError(line, "Unterminated block comment.");
        }

        return null;
    }

    bool IsAlpha(char c) =>
        (c >= 'a' && c <= 'z') ||
        (c >= 'A' && c <= 'Z') ||
        c == '_';

    bool IsDigit(char c) => c >= '0' && c <= '9';

    bool IsAlphaNumeric(char c) => IsDigit(c) || IsAlpha(c);

    Token ParseIdentifier()
    {
        while (IsAlphaNumeric(Peek()))
        {
            Advance();
        }

        var text = source[start..current];
        if (Keywords.TryGetValue(text, out var type))
        {
            return CreateToken(type);
        }

        return CreateToken(TokenType.IDENTIFIER);
    }

    Token ParseNumber()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        }

        if (Peek() == '.' && IsDigit(DoublePeek()))
        {
            Advance();

            while (IsDigit(Peek()))
            {
                Advance();
            }
        }

        return CreateToken(TokenType.NUMBER, double.Parse(source[start..current]));
    }

    Token? ParseString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                line++;
            }
            Advance();
        }

        if (IsAtEnd())
        {
            onError(line, "Unterminated string.");
            return null;
        }

        Advance();

        var value = source[(start + 1)..(current - 1)];
        return CreateToken(TokenType.STRING, value);
    }

    char DoublePeek()
        => current + 1 >= source.Length ? '\0' : source[current + 1];

    char Peek() => IsAtEnd() ? '\0' : source[current];

    bool Match(char expected)
    {
        if (IsAtEnd() || source[current] != expected)
        {
            return false;
        }

        current++;
        return true;
    }

    char Advance() => source[current++];

    Token CreateToken(TokenType type) => CreateToken(type, null);

    Token CreateToken(TokenType type, object? literal)
    {
        var text = source[start..current];
        return new Token(type, text, literal, line);
    }

    bool IsAtEnd() => current >= source.Length;
}
