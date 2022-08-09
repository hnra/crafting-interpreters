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
    private readonly string source;
    private readonly List<Token> tokens = new List<Token>();
    private int start = 0;
    private int current = 0;
    private int line = 1;

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
        var eol = false;

        var token = c switch {
            '(' => CreateToken(TokenType.LEFT_PAREN),
            ')' => CreateToken(TokenType.RIGHT_PAREN),
            '{' => CreateToken(TokenType.LEFT_BRACE),
            '}' => CreateToken(TokenType.RIGHT_BRACE),
            ',' => CreateToken(TokenType.COMMA),
            '.' => CreateToken(TokenType.DOT),
            '-' => CreateToken(TokenType.MINUS),
            '+' => CreateToken(TokenType.PLUS),
            ';' => CreateToken(TokenType.SEMICOLON),
            '*' => CreateToken(TokenType.STAR),
            '!' => CreateToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG),
            '=' => CreateToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL),
            '<' => CreateToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS),
            '>' => CreateToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER),
            '/' => Match('/') ? ReadToEOL() : CreateToken(TokenType.SLASH),
            _ => null,
        };

        if (token == null && !eol) {
            Lox.Error(this.line, "Unexpected character.");
        }

        return token;

        Token? ReadToEOL() {
            while (!IsAtEnd() && Peek() != '\n') {
                Advance();
            }
            eol = true;
            return null;
        }
    }

    char Peek() => IsAtEnd() ? '\0' : source[this.current];

    bool Match(char expected) {
        if (IsAtEnd() || this.source[this.current] != expected) {
            return false;
        }

        this.current++;
        return true;
    }

    char Advance() {
        return this.source[this.current++];
    }

    Token CreateToken(TokenType type) => CreateToken(type, null);
    Token CreateToken(TokenType type, object? literal) {
        var text = this.source.Substring(this.start, this.current);
        return new Token(type, text, literal, this.line);
    }

    bool IsAtEnd() {
        return current >= source.Length;
    }
}
