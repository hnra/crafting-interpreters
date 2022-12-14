namespace NLox;

public record Token(TokenType type, string lexeme, object? literal, int line)
{
    public override string ToString()
    {
        return $"{type} {lexeme} {literal}";
    }
}
