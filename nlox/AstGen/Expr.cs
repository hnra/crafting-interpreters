namespace CraftingInterpreters.AstGen;

public abstract record Expr;
public record Binary(Expr left, Token op, Expr right) : Expr;
public record Grouping(Expr expression) : Expr;
public record Literal(object value) : Expr;
public record Unary(Token op, Expr right) : Expr;
