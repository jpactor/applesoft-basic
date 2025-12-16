namespace ApplesoftBasic.Interpreter.AST;

/// <summary>
/// Represents the entire BASIC program
/// </summary>
public class ProgramNode : IAstNode
{
    public List<LineNode> Lines { get; } = new();
    public List<object> DataValues { get; } = new();

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitProgram(this);
}

/// <summary>
/// Represents a single numbered line in the program
/// </summary>
public class LineNode : IAstNode
{
    public int LineNumber { get; }
    public List<IStatement> Statements { get; } = new();

    public LineNode(int lineNumber)
    {
        LineNumber = lineNumber;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitLine(this);
}
