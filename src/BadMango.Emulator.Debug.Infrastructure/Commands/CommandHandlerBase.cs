// <copyright file="CommandHandlerBase.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Commands;

/// <summary>
/// Base class for command handlers providing common functionality.
/// </summary>
/// <remarks>
/// Provides a convenient base implementation for command handlers with
/// default values for optional properties. Derived classes must implement
/// the abstract <see cref="Execute"/> method.
/// </remarks>
public abstract class CommandHandlerBase : ICommandHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHandlerBase"/> class.
    /// </summary>
    /// <param name="name">The primary name of the command.</param>
    /// <param name="description">A brief description of the command.</param>
    protected CommandHandlerBase(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        this.Name = name;
        this.Description = description;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public virtual IReadOnlyList<string> Aliases { get; } = [];

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public virtual string Usage => this.Name;

    /// <inheritdoc/>
    public abstract CommandResult Execute(ICommandContext context, string[] args);
}