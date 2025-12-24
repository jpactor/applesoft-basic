// <copyright file="CommandDispatcher.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Commands;

/// <summary>
/// Default implementation of <see cref="ICommandDispatcher"/>.
/// </summary>
/// <remarks>
/// Routes commands to registered handlers based on command name or alias.
/// Commands are matched case-insensitively.
/// </remarks>
public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly List<ICommandHandler> handlers = [];
    private readonly Dictionary<string, ICommandHandler> commandMap = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public IReadOnlyList<ICommandHandler> Commands => this.handlers.AsReadOnly();

    /// <inheritdoc/>
    public void Register(ICommandHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (this.commandMap.ContainsKey(handler.Name))
        {
            throw new InvalidOperationException($"A command with name '{handler.Name}' is already registered.");
        }

        var conflictingAlias = handler.Aliases.FirstOrDefault(alias => this.commandMap.ContainsKey(alias));
        if (conflictingAlias is not null)
        {
            throw new InvalidOperationException($"A command with alias '{conflictingAlias}' is already registered.");
        }

        this.handlers.Add(handler);
        this.commandMap[handler.Name] = handler;

        foreach (var alias in handler.Aliases)
        {
            this.commandMap[alias] = handler;
        }
    }

    /// <inheritdoc/>
    public bool TryGetHandler(string name, out ICommandHandler? handler)
    {
        return this.commandMap.TryGetValue(name, out handler);
    }

    /// <inheritdoc/>
    public CommandResult Dispatch(ICommandContext context, string commandLine)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return CommandResult.Ok();
        }

        var parts = ParseCommandLine(commandLine);
        if (parts.Length == 0)
        {
            return CommandResult.Ok();
        }

        var commandName = parts[0];
        var args = parts.Length > 1 ? parts[1..] : [];

        if (!this.TryGetHandler(commandName, out var handler) || handler is null)
        {
            return CommandResult.Error($"Unknown command: '{commandName}'. Type 'help' for a list of commands.");
        }

        return handler.Execute(context, args);
    }

    /// <summary>
    /// Parses a command line into command name and arguments.
    /// </summary>
    /// <param name="commandLine">The command line to parse.</param>
    /// <returns>An array of parsed tokens.</returns>
    internal static string[] ParseCommandLine(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return [];
        }

        var tokens = new List<string>();
        var currentToken = new System.Text.StringBuilder();
        var inQuotes = false;
        var escapeNext = false;

        foreach (var c in commandLine)
        {
            if (escapeNext)
            {
                currentToken.Append(c);
                escapeNext = false;
                continue;
            }

            if (c == '\\')
            {
                escapeNext = true;
                continue;
            }

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (currentToken.Length > 0)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                }

                continue;
            }

            currentToken.Append(c);
        }

        if (currentToken.Length > 0)
        {
            tokens.Add(currentToken.ToString());
        }

        return [.. tokens];
    }
}