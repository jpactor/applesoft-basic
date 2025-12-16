using ApplesoftBasic.Interpreter.Tokens;

namespace ApplesoftBasic.Interpreter.AST;

/// <summary>
/// Base interface for all expressions
/// </summary>
public interface IExpression : IAstNode
{
}

/// <summary>
/// Numeric literal
/// </summary>
public class NumberLiteral : IExpression
{
    public double Value { get; }

    public NumberLiteral(double value)
    {
        Value = value;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitNumberLiteral(this);
}

/// <summary>
/// String literal
/// </summary>
public class StringLiteral : IExpression
{
    public string Value { get; }

    public StringLiteral(string value)
    {
        Value = value;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitStringLiteral(this);
}

/// <summary>
/// Variable reference
/// </summary>
public class VariableExpression : IExpression
{
    public string Name { get; }
    public bool IsString => Name.EndsWith('$');
    public bool IsInteger => Name.EndsWith('%');

    public VariableExpression(string name)
    {
        Name = name;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitVariableExpression(this);
}

/// <summary>
/// Binary operation (e.g., 1 + 2, A * B)
/// </summary>
public class BinaryExpression : IExpression
{
    public IExpression Left { get; }
    public TokenType Operator { get; }
    public IExpression Right { get; }

    public BinaryExpression(IExpression left, TokenType op, IExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
}

/// <summary>
/// Unary operation (e.g., -X, NOT flag)
/// </summary>
public class UnaryExpression : IExpression
{
    public TokenType Operator { get; }
    public IExpression Operand { get; }

    public UnaryExpression(TokenType op, IExpression operand)
    {
        Operator = op;
        Operand = operand;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
}

/// <summary>
/// Built-in function call (e.g., SIN(X), LEN(A$))
/// </summary>
public class FunctionCallExpression : IExpression
{
    public TokenType Function { get; }
    public List<IExpression> Arguments { get; } = new();

    public FunctionCallExpression(TokenType function)
    {
        Function = function;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitFunctionCallExpression(this);
}

/// <summary>
/// Array element access (e.g., A(1), B$(I,J))
/// </summary>
public class ArrayAccessExpression : IExpression
{
    public string ArrayName { get; }
    public List<IExpression> Indices { get; } = new();

    public ArrayAccessExpression(string arrayName)
    {
        ArrayName = arrayName;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitArrayAccessExpression(this);
}

/// <summary>
/// User-defined function call (FN name)
/// </summary>
public class UserFunctionExpression : IExpression
{
    public string FunctionName { get; }
    public IExpression Argument { get; }

    public UserFunctionExpression(string functionName, IExpression argument)
    {
        FunctionName = functionName;
        Argument = argument;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitUserFunctionExpression(this);
}
