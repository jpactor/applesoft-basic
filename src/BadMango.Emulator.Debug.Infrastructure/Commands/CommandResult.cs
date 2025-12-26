// <copyright file="CommandResult.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Commands;

/// <summary>
/// Represents the result of a command execution.
/// </summary>
/// <remarks>
/// Command results indicate success or failure and can carry
/// optional messages or signals to control the REPL flow.
/// </remarks>
public sealed class CommandResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResult"/> class.
    /// </summary>
    /// <param name="success">Indicates whether the command succeeded.</param>
    /// <param name="message">An optional message describing the result.</param>
    /// <param name="shouldExit">Indicates whether the REPL should exit.</param>
    private CommandResult(bool success, string? message, bool shouldExit)
    {
        this.Success = success;
        this.Message = message;
        this.ShouldExit = shouldExit;
    }

    /// <summary>
    /// Gets a value indicating whether the command executed successfully.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets an optional message describing the result.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets a value indicating whether the REPL should exit after this command.
    /// </summary>
    public bool ShouldExit { get; }

    /// <summary>
    /// Creates a successful result with no message.
    /// </summary>
    /// <returns>A successful <see cref="CommandResult"/>.</returns>
    public static CommandResult Ok() => new(true, null, false);

    /// <summary>
    /// Creates a successful result with a message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>A successful <see cref="CommandResult"/> with a message.</returns>
    public static CommandResult Ok(string message) => new(true, message, false);

    /// <summary>
    /// Creates a failure result with an error message.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <returns>A failed <see cref="CommandResult"/>.</returns>
    public static CommandResult Error(string message) => new(false, message, false);

    /// <summary>
    /// Creates a result that signals the REPL to exit.
    /// </summary>
    /// <returns>A <see cref="CommandResult"/> that signals exit.</returns>
    public static CommandResult Exit() => new(true, null, true);

    /// <summary>
    /// Creates a result that signals the REPL to exit with a message.
    /// </summary>
    /// <param name="message">The message to display before exiting.</param>
    /// <returns>A <see cref="CommandResult"/> that signals exit with a message.</returns>
    public static CommandResult Exit(string message) => new(true, message, true);
}