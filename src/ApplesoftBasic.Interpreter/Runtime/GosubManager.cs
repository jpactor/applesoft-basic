namespace ApplesoftBasic.Interpreter.Runtime;

/// <summary>
/// Manages GOSUB/RETURN stack
/// </summary>
public interface IGosubManager
{
    /// <summary>
    /// Pushes a return address onto the stack
    /// </summary>
    void Push(GosubReturnAddress address);
    
    /// <summary>
    /// Pops and returns the last return address
    /// </summary>
    GosubReturnAddress Pop();
    
    /// <summary>
    /// Clears the stack
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Gets the current stack depth
    /// </summary>
    int Depth { get; }
}

/// <summary>
/// Represents a GOSUB return address
/// </summary>
public class GosubReturnAddress
{
    public int LineIndex { get; }
    public int StatementIndex { get; }

    public GosubReturnAddress(int lineIndex, int statementIndex)
    {
        LineIndex = lineIndex;
        StatementIndex = statementIndex;
    }
}

/// <summary>
/// Default implementation of GOSUB manager
/// </summary>
public class GosubManager : IGosubManager
{
    private readonly Stack<GosubReturnAddress> _stack = new();

    public int Depth => _stack.Count;

    public void Push(GosubReturnAddress address)
    {
        _stack.Push(address);
    }

    public GosubReturnAddress Pop()
    {
        if (_stack.Count == 0)
        {
            throw new BasicRuntimeException("?RETURN WITHOUT GOSUB ERROR");
        }
        return _stack.Pop();
    }

    public void Clear()
    {
        _stack.Clear();
    }
}
