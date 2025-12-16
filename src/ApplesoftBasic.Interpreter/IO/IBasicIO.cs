namespace ApplesoftBasic.Interpreter.IO;

/// <summary>
/// Interface for BASIC I/O operations
/// </summary>
public interface IBasicIO
{
    /// <summary>
    /// Writes output (PRINT)
    /// </summary>
    void Write(string text);
    
    /// <summary>
    /// Writes a line (PRINT with newline)
    /// </summary>
    void WriteLine(string text = "");
    
    /// <summary>
    /// Reads a line of input (INPUT)
    /// </summary>
    string ReadLine(string? prompt = null);
    
    /// <summary>
    /// Reads a single character (GET)
    /// </summary>
    char ReadChar();
    
    /// <summary>
    /// Clears the screen (HOME)
    /// </summary>
    void ClearScreen();
    
    /// <summary>
    /// Sets cursor position
    /// </summary>
    void SetCursorPosition(int column, int row);
    
    /// <summary>
    /// Gets current cursor column (for POS function)
    /// </summary>
    int GetCursorColumn();
    
    /// <summary>
    /// Gets current cursor row
    /// </summary>
    int GetCursorRow();
    
    /// <summary>
    /// Sets text output mode (NORMAL, INVERSE, FLASH)
    /// </summary>
    void SetTextMode(TextMode mode);
    
    /// <summary>
    /// Produces a beep
    /// </summary>
    void Beep();
}

/// <summary>
/// Text output modes
/// </summary>
public enum TextMode
{
    Normal,
    Inverse,
    Flash
}

/// <summary>
/// Console-based I/O implementation
/// </summary>
public class ConsoleBasicIO : IBasicIO
{
    private TextMode _currentMode = TextMode.Normal;
    private int _cursorColumn;

    public void Write(string text)
    {
        if (_currentMode == TextMode.Inverse)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        
        Console.Write(text);
        _cursorColumn += text.Length;
        
        if (_currentMode == TextMode.Inverse)
        {
            Console.ResetColor();
        }
    }

    public void WriteLine(string text = "")
    {
        if (_currentMode == TextMode.Inverse)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        
        Console.WriteLine(text);
        _cursorColumn = 0;
        
        if (_currentMode == TextMode.Inverse)
        {
            Console.ResetColor();
        }
    }

    public string ReadLine(string? prompt = null)
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            Write(prompt);
        }
        
        var result = Console.ReadLine() ?? string.Empty;
        _cursorColumn = 0;
        return result;
    }

    public char ReadChar()
    {
        var key = Console.ReadKey(true);
        return key.KeyChar;
    }

    public void ClearScreen()
    {
        try
        {
            Console.Clear();
        }
        catch
        {
            // Console.Clear may not work in all environments
            for (int i = 0; i < 24; i++)
            {
                Console.WriteLine();
            }
        }
        _cursorColumn = 0;
    }

    public void SetCursorPosition(int column, int row)
    {
        try
        {
            // Apple II is 1-based, Console is 0-based
            int col = Math.Max(0, Math.Min(column - 1, Console.WindowWidth - 1));
            int r = Math.Max(0, Math.Min(row - 1, Console.WindowHeight - 1));
            Console.SetCursorPosition(col, r);
            _cursorColumn = col;
        }
        catch
        {
            // Ignore cursor positioning errors
        }
    }

    public int GetCursorColumn()
    {
        try
        {
            return Console.CursorLeft;
        }
        catch
        {
            return _cursorColumn;
        }
    }

    public int GetCursorRow()
    {
        try
        {
            return Console.CursorTop;
        }
        catch
        {
            return 0;
        }
    }

    public void SetTextMode(TextMode mode)
    {
        _currentMode = mode;
        
        if (mode == TextMode.Normal)
        {
            Console.ResetColor();
        }
    }

    public void Beep()
    {
        try
        {
            Console.Beep(800, 100);
        }
        catch
        {
            // Beep may not work in all environments
        }
    }
}
