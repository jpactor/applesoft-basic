// <copyright file="VersionCommand.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Commands;

using System.Reflection;

/// <summary>
/// Displays version information for the debug console.
/// </summary>
/// <remarks>
/// Shows the assembly version and other build metadata.
/// </remarks>
public sealed class VersionCommand : CommandHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionCommand"/> class.
    /// </summary>
    public VersionCommand()
        : base("version", "Display version information")
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> Aliases { get; } = ["ver", "v"];

    /// <inheritdoc/>
    public override CommandResult Execute(ICommandContext context, string[] args)
    {
        ArgumentNullException.ThrowIfNull(context);

        var assembly = typeof(VersionCommand).Assembly;
        var version = assembly.GetName().Version?.ToString() ?? "Unknown";
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? version;

        context.Output.WriteLine($"Emulator Debug Console v{informationalVersion}");
        context.Output.WriteLine("Copyright (c) Bad Mango Solutions. All rights reserved.");

        return CommandResult.Ok();
    }
}