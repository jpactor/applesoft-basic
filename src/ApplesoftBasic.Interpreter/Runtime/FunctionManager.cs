using ApplesoftBasic.Interpreter.AST;

namespace ApplesoftBasic.Interpreter.Runtime;

/// <summary>
/// Manages user-defined functions (DEF FN)
/// </summary>
public interface IFunctionManager
{
    /// <summary>
    /// Defines a user function
    /// </summary>
    void DefineFunction(string name, string parameter, IExpression body);
    
    /// <summary>
    /// Gets a user-defined function
    /// </summary>
    UserFunction? GetFunction(string name);
    
    /// <summary>
    /// Checks if a function is defined
    /// </summary>
    bool FunctionExists(string name);
    
    /// <summary>
    /// Clears all user-defined functions
    /// </summary>
    void Clear();
}

/// <summary>
/// Represents a user-defined function
/// </summary>
public class UserFunction
{
    public string Name { get; }
    public string Parameter { get; }
    public IExpression Body { get; }

    public UserFunction(string name, string parameter, IExpression body)
    {
        Name = name;
        Parameter = parameter;
        Body = body;
    }
}

/// <summary>
/// Default implementation of function manager
/// </summary>
public class FunctionManager : IFunctionManager
{
    private readonly Dictionary<string, UserFunction> _functions = new(StringComparer.OrdinalIgnoreCase);

    public void DefineFunction(string name, string parameter, IExpression body)
    {
        // Normalize function name (first 2 chars only, like variables)
        string normalizedName = NormalizeFunctionName(name);
        _functions[normalizedName] = new UserFunction(normalizedName, parameter, body);
    }

    public UserFunction? GetFunction(string name)
    {
        string normalizedName = NormalizeFunctionName(name);
        return _functions.TryGetValue(normalizedName, out var func) ? func : null;
    }

    public bool FunctionExists(string name)
    {
        return _functions.ContainsKey(NormalizeFunctionName(name));
    }

    public void Clear()
    {
        _functions.Clear();
    }

    private static string NormalizeFunctionName(string name)
    {
        if (name.Length > 2)
        {
            return name[..2].ToUpperInvariant();
        }
        return name.ToUpperInvariant();
    }
}
