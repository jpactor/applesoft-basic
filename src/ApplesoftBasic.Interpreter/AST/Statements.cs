namespace ApplesoftBasic.Interpreter.AST;

/// <summary>
/// Base interface for all statements
/// </summary>
public interface IStatement : IAstNode
{
}

/// <summary>
/// PRINT statement
/// </summary>
public class PrintStatement : IStatement
{
    public List<IExpression> Expressions { get; } = new();
    public List<PrintSeparator> Separators { get; } = new();
    public bool EndsWithSeparator { get; set; }
    
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitPrintStatement(this);
}

public enum PrintSeparator
{
    None,
    Comma,      // Tab to next column
    Semicolon   // No space
}

/// <summary>
/// INPUT statement
/// </summary>
public class InputStatement : IStatement
{
    public string? Prompt { get; set; }
    public List<VariableExpression> Variables { get; } = new();
    
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitInputStatement(this);
}

/// <summary>
/// LET assignment statement (LET keyword is optional)
/// </summary>
public class LetStatement : IStatement
{
    public VariableExpression Variable { get; }
    public IExpression Value { get; }
    public List<IExpression>? ArrayIndices { get; set; }

    public LetStatement(VariableExpression variable, IExpression value)
    {
        Variable = variable;
        Value = value;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitLetStatement(this);
}

/// <summary>
/// IF-THEN statement
/// </summary>
public class IfStatement : IStatement
{
    public IExpression Condition { get; }
    public List<IStatement> ThenBranch { get; } = new();
    public int? GotoLineNumber { get; set; }
    
    public IfStatement(IExpression condition)
    {
        Condition = condition;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitIfStatement(this);
}

/// <summary>
/// GOTO statement
/// </summary>
public class GotoStatement : IStatement
{
    public int LineNumber { get; }

    public GotoStatement(int lineNumber)
    {
        LineNumber = lineNumber;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitGotoStatement(this);
}

/// <summary>
/// GOSUB statement
/// </summary>
public class GosubStatement : IStatement
{
    public int LineNumber { get; }

    public GosubStatement(int lineNumber)
    {
        LineNumber = lineNumber;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitGosubStatement(this);
}

/// <summary>
/// RETURN statement
/// </summary>
public class ReturnStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitReturnStatement(this);
}

/// <summary>
/// FOR statement
/// </summary>
public class ForStatement : IStatement
{
    public string Variable { get; }
    public IExpression Start { get; }
    public IExpression End { get; }
    public IExpression? Step { get; set; }

    public ForStatement(string variable, IExpression start, IExpression end)
    {
        Variable = variable;
        Start = start;
        End = end;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitForStatement(this);
}

/// <summary>
/// NEXT statement
/// </summary>
public class NextStatement : IStatement
{
    public List<string> Variables { get; } = new();

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitNextStatement(this);
}

/// <summary>
/// DIM statement
/// </summary>
public class DimStatement : IStatement
{
    public List<ArrayDeclaration> Arrays { get; } = new();

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitDimStatement(this);
}

public class ArrayDeclaration
{
    public string Name { get; }
    public List<IExpression> Dimensions { get; } = new();

    public ArrayDeclaration(string name)
    {
        Name = name;
    }
}

/// <summary>
/// READ statement
/// </summary>
public class ReadStatement : IStatement
{
    public List<VariableExpression> Variables { get; } = new();

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitReadStatement(this);
}

/// <summary>
/// DATA statement
/// </summary>
public class DataStatement : IStatement
{
    public List<object> Values { get; } = new();

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitDataStatement(this);
}

/// <summary>
/// RESTORE statement
/// </summary>
public class RestoreStatement : IStatement
{
    public int? LineNumber { get; set; }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitRestoreStatement(this);
}

/// <summary>
/// END statement
/// </summary>
public class EndStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitEndStatement(this);
}

/// <summary>
/// STOP statement
/// </summary>
public class StopStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitStopStatement(this);
}

/// <summary>
/// REM (comment) statement
/// </summary>
public class RemStatement : IStatement
{
    public string Comment { get; }

    public RemStatement(string comment)
    {
        Comment = comment;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitRemStatement(this);
}

/// <summary>
/// POKE statement
/// </summary>
public class PokeStatement : IStatement
{
    public IExpression Address { get; }
    public IExpression Value { get; }

    public PokeStatement(IExpression address, IExpression value)
    {
        Address = address;
        Value = value;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitPokeStatement(this);
}

/// <summary>
/// CALL statement
/// </summary>
public class CallStatement : IStatement
{
    public IExpression Address { get; }

    public CallStatement(IExpression address)
    {
        Address = address;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitCallStatement(this);
}

/// <summary>
/// GET statement (single character input)
/// </summary>
public class GetStatement : IStatement
{
    public VariableExpression Variable { get; }

    public GetStatement(VariableExpression variable)
    {
        Variable = variable;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitGetStatement(this);
}

/// <summary>
/// ON ... GOTO statement
/// </summary>
public class OnGotoStatement : IStatement
{
    public IExpression Expression { get; }
    public List<int> LineNumbers { get; } = new();

    public OnGotoStatement(IExpression expression)
    {
        Expression = expression;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitOnGotoStatement(this);
}

/// <summary>
/// ON ... GOSUB statement
/// </summary>
public class OnGosubStatement : IStatement
{
    public IExpression Expression { get; }
    public List<int> LineNumbers { get; } = new();

    public OnGosubStatement(IExpression expression)
    {
        Expression = expression;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitOnGosubStatement(this);
}

/// <summary>
/// DEF FN statement for user-defined functions
/// </summary>
public class DefStatement : IStatement
{
    public string FunctionName { get; }
    public string Parameter { get; }
    public IExpression Body { get; }

    public DefStatement(string functionName, string parameter, IExpression body)
    {
        FunctionName = functionName;
        Parameter = parameter;
        Body = body;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitDefStatement(this);
}

/// <summary>
/// HOME statement - clears screen
/// </summary>
public class HomeStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitHomeStatement(this);
}

/// <summary>
/// HTAB statement - horizontal tab
/// </summary>
public class HtabStatement : IStatement
{
    public IExpression Column { get; }

    public HtabStatement(IExpression column)
    {
        Column = column;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitHtabStatement(this);
}

/// <summary>
/// VTAB statement - vertical tab
/// </summary>
public class VtabStatement : IStatement
{
    public IExpression Row { get; }

    public VtabStatement(IExpression row)
    {
        Row = row;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitVtabStatement(this);
}

/// <summary>
/// TEXT statement - switches to text mode
/// </summary>
public class TextStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitTextStatement(this);
}

/// <summary>
/// GR statement - low-resolution graphics mode
/// </summary>
public class GrStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitGrStatement(this);
}

/// <summary>
/// HGR statement - high-resolution graphics mode
/// </summary>
public class HgrStatement : IStatement
{
    public bool IsHgr2 { get; set; }
    
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitHgrStatement(this);
}

/// <summary>
/// COLOR= statement - sets lo-res color
/// </summary>
public class ColorStatement : IStatement
{
    public IExpression Color { get; }

    public ColorStatement(IExpression color)
    {
        Color = color;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitColorStatement(this);
}

/// <summary>
/// HCOLOR= statement - sets hi-res color
/// </summary>
public class HcolorStatement : IStatement
{
    public IExpression Color { get; }

    public HcolorStatement(IExpression color)
    {
        Color = color;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitHcolorStatement(this);
}

/// <summary>
/// PLOT statement - plots a point in lo-res
/// </summary>
public class PlotStatement : IStatement
{
    public IExpression X { get; }
    public IExpression Y { get; }

    public PlotStatement(IExpression x, IExpression y)
    {
        X = x;
        Y = y;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitPlotStatement(this);
}

/// <summary>
/// HPLOT statement - plots in hi-res
/// </summary>
public class HplotStatement : IStatement
{
    public List<(IExpression X, IExpression Y)> Points { get; } = new();

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitHplotStatement(this);
}

/// <summary>
/// DRAW statement - draws a shape
/// </summary>
public class DrawStatement : IStatement
{
    public IExpression ShapeNumber { get; }
    public IExpression? AtX { get; set; }
    public IExpression? AtY { get; set; }

    public DrawStatement(IExpression shapeNumber)
    {
        ShapeNumber = shapeNumber;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitDrawStatement(this);
}

/// <summary>
/// XDRAW statement - XOR draws a shape
/// </summary>
public class XdrawStatement : IStatement
{
    public IExpression ShapeNumber { get; }
    public IExpression? AtX { get; set; }
    public IExpression? AtY { get; set; }

    public XdrawStatement(IExpression shapeNumber)
    {
        ShapeNumber = shapeNumber;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitXdrawStatement(this);
}

/// <summary>
/// INVERSE statement - sets inverse text mode
/// </summary>
public class InverseStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitInverseStatement(this);
}

/// <summary>
/// FLASH statement - sets flashing text mode
/// </summary>
public class FlashStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitFlashStatement(this);
}

/// <summary>
/// NORMAL statement - sets normal text mode
/// </summary>
public class NormalStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitNormalStatement(this);
}

/// <summary>
/// CLEAR statement - clears variables
/// </summary>
public class ClearStatement : IStatement
{
    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitClearStatement(this);
}

/// <summary>
/// SLEEP statement (custom extension) - pauses execution
/// </summary>
public class SleepStatement : IStatement
{
    public IExpression Milliseconds { get; }

    public SleepStatement(IExpression milliseconds)
    {
        Milliseconds = milliseconds;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitSleepStatement(this);
}

/// <summary>
/// HIMEM: statement - sets top of memory
/// </summary>
public class HimemStatement : IStatement
{
    public IExpression Address { get; }

    public HimemStatement(IExpression address)
    {
        Address = address;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitHimemStatement(this);
}

/// <summary>
/// LOMEM: statement - sets bottom of variable memory
/// </summary>
public class LomemStatement : IStatement
{
    public IExpression Address { get; }

    public LomemStatement(IExpression address)
    {
        Address = address;
    }

    public T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitLomemStatement(this);
}
