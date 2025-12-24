// <copyright file="ICommandContext.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Commands;

/// <summary>
/// Provides context for command execution including access to emulator components.
/// </summary>
/// <remarks>
/// The command context serves as the bridge between command handlers and the
/// emulator subsystems (CPU, bus, memory, etc.). It provides a consistent
/// interface for commands to interact with the emulator state.
/// </remarks>
public interface ICommandContext
{
    /// <summary>
    /// Gets the command dispatcher for accessing other commands.
    /// </summary>
    ICommandDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the console output writer.
    /// </summary>
    TextWriter Output { get; }

    /// <summary>
    /// Gets the console error writer.
    /// </summary>
    TextWriter Error { get; }
}