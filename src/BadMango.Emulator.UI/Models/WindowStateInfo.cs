// <copyright file="WindowStateInfo.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.UI.Models;

using Avalonia;

/// <summary>
/// Represents persisted window state information for a component window.
/// </summary>
public record WindowStateInfo
{
    /// <summary>
    /// Gets the component type displayed in this window.
    /// </summary>
    public required PopOutComponent ComponentType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the window is popped out or docked.
    /// </summary>
    public bool IsPopOut { get; init; }

    /// <summary>
    /// Gets the window position in screen coordinates.
    /// </summary>
    public Point? Position { get; init; }

    /// <summary>
    /// Gets the window size.
    /// </summary>
    public Size? Size { get; init; }

    /// <summary>
    /// Gets the monitor identifier for multi-monitor setups.
    /// </summary>
    public string? MonitorId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the window is maximized.
    /// </summary>
    public bool IsMaximized { get; init; }

    /// <summary>
    /// Gets the associated machine profile ID for machine-specific windows.
    /// </summary>
    public string? MachineProfileId { get; init; }
}