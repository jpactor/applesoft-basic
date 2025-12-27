// <copyright file="IPopOutWindow.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.UI.Services;

using Avalonia.Controls;

using BadMango.Emulator.UI.Models;

/// <summary>
/// Represents a detached pop-out window containing a UI component.
/// </summary>
public interface IPopOutWindow
{
    /// <summary>
    /// Gets the unique identifier for this window instance.
    /// </summary>
    string WindowId { get; }

    /// <summary>
    /// Gets the component type displayed in this window.
    /// </summary>
    PopOutComponent ComponentType { get; }

    /// <summary>
    /// Gets or sets the associated machine ID for display/debug windows.
    /// </summary>
    string? MachineId { get; set; }

    /// <summary>
    /// Gets the current window state.
    /// </summary>
    WindowState State { get; }

    /// <summary>
    /// Gets the window title.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Brings the window to the foreground.
    /// </summary>
    void BringToFront();

    /// <summary>
    /// Closes the window.
    /// </summary>
    /// <param name="dockContent">If true, docks the content back to the main window instead of closing.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CloseAsync(bool dockContent = false);

    /// <summary>
    /// Gets the current window state information for persistence.
    /// </summary>
    /// <returns>The current window state info.</returns>
    WindowStateInfo GetStateInfo();

    /// <summary>
    /// Restores window state from persisted information.
    /// </summary>
    /// <param name="stateInfo">The state information to restore.</param>
    void RestoreState(WindowStateInfo stateInfo);
}