namespace CraftingInterpreters;

enum TokenType {
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

    BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL, LESS, LESS_EQUAL,

    IDENTIFIER, STRING, NUMBER,

    AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
    PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

    EOF
}

record Token(TokenType type, string lexeme, object? literal, int line) {
    public override string ToString() {
        return $"{type} {lexeme} {literal}";
    }
}

class Scanner {
    readonly string source;
    readonly List<Token> tokens = new List<Token>();
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

    public Scanner(string source) {
        this.source = source ?? "";
    }

    public List<Token> ScanTokens() {
        while (!IsAtEnd()) {
            this.start = this.current;
            ScanAndAddToken();
        }

        this.tokens.Add(new Token(TokenType.EOF, "", null, this.line));
        return this.tokens;
    }

    void ScanAndAddToken() {
        var token = ScanToken();
        
        if (token != null) {
            this.tokens.Add(token);
        }
    }

    Token? ScanToken() {
        var c = Advance();

        switch (c) {
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
                if (Match('/')) {
                    while (!IsAtEnd() && Peek() != '\n') {
                        Advance();
                    }
                    return null;
                }
                return CreateToken(TokenType.SLASH);
            case '\n':
                this.line++;
                return null;
            case ' ':
            case '\r':
            case '\t':
                return null;
            case '"':
                return ParseString();
            default:
                if (IsDigit(c)) {
                    return ParseNumber();
                } else if (IsAlpha(c)) {
                    return ParseIdentifier();
                }
                Lox.Error(this.line, $"Unexpected character '{c}'.");
                return null;
        }
    }

    bool IsAlpha(char c) =>
        (c >= 'a' && c <= 'z') ||
        (c >= 'A' && c <= 'Z') ||
        c == '_';
    
    bool IsDigit(char c) => c >= '0' && c <= '9';

    bool IsAlphaNumeric(char c) => IsDigit(c) || IsAlpha(c);
    
    Token ParseIdentifier() {
        while (IsAlphaNumeric(Peek())) {
            Advance();
        }

        var text = this.source[this.start..this.current];
        if (Keywords.TryGetValue(text, out var type)) {
            return CreateToken(type);
        }

        return CreateToken(TokenType.IDENTIFIER);
    }

    Token ParseNumber() {
        while (IsDigit(Peek())) {
            Advance();
        }

        if (Peek() == '.' && IsDigit(DoublePeek())) {
            Advance();

            while (IsDigit(Peek())) {
                Advance();
            }
        }

        return CreateToken(TokenType.NUMBER, double.Parse(this.source[this.start..this.current]));
    }

    Token? ParseString() {
        while (Peek() != '"' && !IsAtEnd()) {
            if (Peek() == '\n') {
                this.line++;
            }
            Advance();
        }

        if (IsAtEnd()) {
            Lox.Error(this.line, "Unterminated string.");
            return null;
        }

        Advance();

        var value = this.source[(this.start + 1)..(this.current-1)];
        return CreateToken(TokenType.STRING, value);
    }

    char DoublePeek()
        => this.current + 1 >= this.source.Length ? '\0' : this.source[this.current + 1];

    char Peek() => IsAtEnd() ? '\0' : source[this.current];

    bool Match(char expected) {
        if (IsAtEnd() || this.source[this.current] != expected) {
            return false;
        }

        this.current++;
        return true;
    }

    char Advance() => this.source[this.current++];

    Token CreateToken(TokenType type) => CreateToken(type, null);

    Token CreateToken(TokenType type, object? literal) {
        var text = this.source[this.start..this.current];
        return new Token(type, text, literal, this.line);
    }

    bool IsAtEnd() => current >= source.Length;
}
