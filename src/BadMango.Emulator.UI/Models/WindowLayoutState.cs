// <copyright file="WindowLayoutState.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.UI.Models;

/// <summary>
/// Represents the complete window layout state for a profile.
/// </summary>
public record WindowLayoutState
{
    /// <summary>
    /// Gets the layout version for migration support.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Gets the window states for this layout.
    /// </summary>
    public IReadOnlyList<WindowStateInfo> Windows { get; init; } = [];

    /// <summary>
    /// Gets the main window state.
    /// </summary>
    public WindowStateInfo? MainWindow { get; init; }
}