// <copyright file="ExitCommand.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Commands;

/// <summary>
/// Exits the debug console.
/// </summary>
/// <remarks>
/// Signals the REPL to terminate gracefully.
/// </remarks>
public sealed class ExitCommand : CommandHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExitCommand"/> class.
    /// </summary>
    public ExitCommand()
        : base("exit", "Exit the debug console")
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> Aliases { get; } = ["quit", "q"];

    /// <inheritdoc/>
    public override CommandResult Execute(ICommandContext context, string[] args)
    {
        ArgumentNullException.ThrowIfNull(context);

        return CommandResult.Exit("Goodbye!");
    }
}