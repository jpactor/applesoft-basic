// <copyright file="ClearCommand.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Commands;

/// <summary>
/// Clears the console screen.
/// </summary>
/// <remarks>
/// Clears all text from the console and moves the cursor to the top-left corner.
/// </remarks>
public sealed class ClearCommand : CommandHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearCommand"/> class.
    /// </summary>
    public ClearCommand()
        : base("clear", "Clear the console screen")
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> Aliases { get; } = ["cls"];

    /// <inheritdoc/>
    public override CommandResult Execute(ICommandContext context, string[] args)
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Console.Clear() can throw if output is redirected
            // In that case, we just do nothing
        }

        return CommandResult.Ok();
    }
}