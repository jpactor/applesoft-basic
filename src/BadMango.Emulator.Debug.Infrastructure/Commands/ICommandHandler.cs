// <copyright file="ICommandHandler.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Commands;

/// <summary>
/// Defines a handler for debug console commands.
/// </summary>
/// <remarks>
/// Command handlers are responsible for executing specific debug operations
/// and can be registered with the <see cref="ICommandDispatcher"/> to extend
/// the debug console's functionality.
/// </remarks>
public interface ICommandHandler
{
    /// <summary>
    /// Gets the primary name of the command.
    /// </summary>
    /// <remarks>
    /// This is the main identifier used to invoke the command from the debug console.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Gets the aliases for the command.
    /// </summary>
    /// <remarks>
    /// Aliases provide alternative names that can be used to invoke the command.
    /// </remarks>
    IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Gets a brief description of the command.
    /// </summary>
    /// <remarks>
    /// This description is displayed in the help output.
    /// </remarks>
    string Description { get; }

    /// <summary>
    /// Gets the usage syntax for the command.
    /// </summary>
    /// <remarks>
    /// Shows how to invoke the command with its parameters.
    /// </remarks>
    string Usage { get; }

    /// <summary>
    /// Executes the command with the specified arguments.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="args">The arguments passed to the command.</param>
    /// <returns>The result of the command execution.</returns>
    CommandResult Execute(ICommandContext context, string[] args);
}