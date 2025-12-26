// <copyright file="HelpCommand.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Commands;

/// <summary>
/// Displays help information for available commands.
/// </summary>
/// <remarks>
/// When invoked without arguments, lists all available commands.
/// When invoked with a command name, shows detailed help for that command.
/// </remarks>
public sealed class HelpCommand : CommandHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HelpCommand"/> class.
    /// </summary>
    public HelpCommand()
        : base("help", "Display help information for available commands")
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> Aliases { get; } = ["?", "h"];

    /// <inheritdoc/>
    public override string Usage => "help [command]";

    /// <inheritdoc/>
    public override CommandResult Execute(ICommandContext context, string[] args)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (args.Length > 0)
        {
            return this.ShowCommandHelp(context, args[0]);
        }

        return this.ShowAllCommands(context);
    }

    private CommandResult ShowAllCommands(ICommandContext context)
    {
        context.Output.WriteLine("Available commands:");
        context.Output.WriteLine();

        var commands = context.Dispatcher.Commands.OrderBy(c => c.Name);
        var maxNameLength = commands.Max(c => c.Name.Length);

        foreach (var command in commands)
        {
            var padding = new string(' ', maxNameLength - command.Name.Length + 2);
            context.Output.WriteLine($"  {command.Name}{padding}{command.Description}");
        }

        context.Output.WriteLine();
        context.Output.WriteLine("Type 'help <command>' for more information on a specific command.");

        return CommandResult.Ok();
    }

    private CommandResult ShowCommandHelp(ICommandContext context, string commandName)
    {
        if (!context.Dispatcher.TryGetHandler(commandName, out var handler) || handler is null)
        {
            return CommandResult.Error($"Unknown command: '{commandName}'");
        }

        context.Output.WriteLine($"Command: {handler.Name}");
        context.Output.WriteLine($"Description: {handler.Description}");
        context.Output.WriteLine($"Usage: {handler.Usage}");

        if (handler.Aliases.Count > 0)
        {
            context.Output.WriteLine($"Aliases: {string.Join(", ", handler.Aliases)}");
        }

        return CommandResult.Ok();
    }
}