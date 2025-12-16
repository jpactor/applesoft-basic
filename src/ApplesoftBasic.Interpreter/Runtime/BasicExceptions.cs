namespace ApplesoftBasic.Interpreter.Runtime;

/// <summary>
/// Exception thrown during BASIC program execution
/// </summary>
public class BasicRuntimeException : Exception
{
    public int? LineNumber { get; }

    public BasicRuntimeException(string message, int? lineNumber = null)
        : base(lineNumber.HasValue ? $"{message} IN {lineNumber}" : message)
    {
        LineNumber = lineNumber;
    }
}

/// <summary>
/// Exception thrown to signal program termination
/// </summary>
public class ProgramEndException : Exception
{
    public ProgramEndException() : base("Program ended") { }
}

/// <summary>
/// Exception thrown to signal STOP command
/// </summary>
public class ProgramStopException : Exception
{
    public int LineNumber { get; }
    
    public ProgramStopException(int lineNumber) 
        : base($"BREAK IN {lineNumber}")
    {
        LineNumber = lineNumber;
    }
}
