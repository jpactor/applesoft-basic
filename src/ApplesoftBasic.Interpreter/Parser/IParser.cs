using ApplesoftBasic.Interpreter.AST;

namespace ApplesoftBasic.Interpreter.Parser;

/// <summary>
/// Interface for the BASIC parser
/// </summary>
public interface IParser
{
    /// <summary>
    /// Parses source code into an AST
    /// </summary>
    /// <param name="source">The BASIC source code</param>
    /// <returns>The program AST</returns>
    ProgramNode Parse(string source);
}
