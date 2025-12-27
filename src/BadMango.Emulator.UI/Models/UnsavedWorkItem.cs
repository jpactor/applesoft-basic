// <copyright file="UnsavedWorkItem.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.UI.Models;

/// <summary>
/// Represents an item with unsaved work that should be handled during shutdown.
/// </summary>
public record UnsavedWorkItem
{
    /// <summary>
    /// Gets the display name of the unsaved item.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets a description of the unsaved changes.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the component type containing the unsaved work.
    /// </summary>
    public PopOutComponent? ComponentType { get; init; }

    /// <summary>
    /// Gets the window ID containing the unsaved work.
    /// </summary>
    public string? WindowId { get; init; }
}