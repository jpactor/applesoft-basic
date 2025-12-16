namespace ApplesoftBasic.Interpreter.Runtime;

/// <summary>
/// Manages FOR-NEXT loop state
/// </summary>
public interface ILoopManager
{
    /// <summary>
    /// Pushes a new FOR loop onto the stack
    /// </summary>
    void PushFor(ForLoopState state);
    
    /// <summary>
    /// Gets the current FOR loop for a variable
    /// </summary>
    ForLoopState? GetForLoop(string variable);
    
    /// <summary>
    /// Pops FOR loop(s) for NEXT
    /// </summary>
    ForLoopState? PopFor(string? variable);
    
    /// <summary>
    /// Clears all loops
    /// </summary>
    void Clear();
}

/// <summary>
/// Represents the state of a FOR loop
/// </summary>
public class ForLoopState
{
    public string Variable { get; }
    public double EndValue { get; }
    public double StepValue { get; }
    public int ReturnLineIndex { get; }
    public int ReturnStatementIndex { get; }

    public ForLoopState(string variable, double endValue, double stepValue, 
        int returnLineIndex, int returnStatementIndex)
    {
        Variable = variable;
        EndValue = endValue;
        StepValue = stepValue;
        ReturnLineIndex = returnLineIndex;
        ReturnStatementIndex = returnStatementIndex;
    }

    public bool IsComplete(double currentValue)
    {
        if (StepValue >= 0)
        {
            return currentValue > EndValue;
        }
        else
        {
            return currentValue < EndValue;
        }
    }
}

/// <summary>
/// Default implementation of loop manager
/// </summary>
public class LoopManager : ILoopManager
{
    private readonly Stack<ForLoopState> _forStack = new();

    public void PushFor(ForLoopState state)
    {
        // Remove any existing loop for the same variable
        var temp = new Stack<ForLoopState>();
        while (_forStack.Count > 0)
        {
            var top = _forStack.Pop();
            if (top.Variable.Equals(state.Variable, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            temp.Push(top);
        }
        
        // Restore other loops
        while (temp.Count > 0)
        {
            _forStack.Push(temp.Pop());
        }
        
        _forStack.Push(state);
    }

    public ForLoopState? GetForLoop(string variable)
    {
        foreach (var state in _forStack)
        {
            if (state.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase))
            {
                return state;
            }
        }
        return null;
    }

    public ForLoopState? PopFor(string? variable)
    {
        if (_forStack.Count == 0)
        {
            throw new BasicRuntimeException("?NEXT WITHOUT FOR ERROR");
        }
        
        if (string.IsNullOrEmpty(variable))
        {
            return _forStack.Pop();
        }
        
        // Pop until we find the matching variable
        var temp = new Stack<ForLoopState>();
        ForLoopState? found = null;
        
        while (_forStack.Count > 0)
        {
            var top = _forStack.Pop();
            if (top.Variable.Equals(variable, StringComparison.OrdinalIgnoreCase))
            {
                found = top;
                break;
            }
            temp.Push(top);
        }
        
        if (found == null)
        {
            // Restore stack and throw error
            while (temp.Count > 0)
            {
                _forStack.Push(temp.Pop());
            }
            throw new BasicRuntimeException("?NEXT WITHOUT FOR ERROR");
        }
        
        // Don't restore loops that were inside this one
        return found;
    }

    public void Clear()
    {
        _forStack.Clear();
    }
}
