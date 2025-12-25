// <copyright file="ICommandDispatcher.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Commands;

/// <summary>
/// Dispatches commands to their registered handlers.
/// </summary>
/// <remarks>
/// The command dispatcher maintains a registry of command handlers and routes
/// incoming commands to the appropriate handler based on the command name or alias.
/// It supports dynamic registration of new commands to enable extensibility.
/// </remarks>
public interface ICommandDispatcher
{
    /// <summary>
    /// Gets all registered command handlers.
    /// </summary>
    IReadOnlyList<ICommandHandler> Commands { get; }

    /// <summary>
    /// Registers a command handler.
    /// </summary>
    /// <param name="handler">The command handler to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a command with the same name or alias is already registered.</exception>
    void Register(ICommandHandler handler);

    /// <summary>
    /// Attempts to find a command handler by name or alias.
    /// </summary>
    /// <param name="name">The command name or alias to look up.</param>
    /// <param name="handler">When this method returns, contains the handler if found; otherwise, null.</param>
    /// <returns><see langword="true"/> if a handler was found; otherwise, <see langword="false"/>.</returns>
    bool TryGetHandler(string name, out ICommandHandler? handler);

    /// <summary>
    /// Dispatches a command line to the appropriate handler.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="commandLine">The full command line including command name and arguments.</param>
    /// <returns>The result of the command execution.</returns>
    CommandResult Dispatch(ICommandContext context, string commandLine);
}