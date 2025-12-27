// <copyright file="IWindowManager.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.UI.Services;

using BadMango.Emulator.UI.Models;

/// <summary>
/// Manages pop-out window lifecycle and state persistence.
/// </summary>
public interface IWindowManager
{
    /// <summary>
    /// Event raised when a pop-out window is created.
    /// </summary>
    event EventHandler<PopOutWindowEventArgs>? WindowCreated;

    /// <summary>
    /// Event raised when a pop-out window is closed.
    /// </summary>
    event EventHandler<PopOutWindowEventArgs>? WindowClosed;

    /// <summary>
    /// Gets all active pop-out windows.
    /// </summary>
    IReadOnlyList<IPopOutWindow> PopOutWindows { get; }

    /// <summary>
    /// Creates a pop-out window for the specified component.
    /// </summary>
    /// <param name="component">The component type to display in the pop-out window.</param>
    /// <param name="machineId">Optional machine ID for machine-specific windows.</param>
    /// <returns>A task containing the created pop-out window.</returns>
    Task<IPopOutWindow> CreatePopOutAsync(PopOutComponent component, string? machineId = null);

    /// <summary>
    /// Docks a pop-out window back into the main window.
    /// </summary>
    /// <param name="window">The window to dock.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DockWindowAsync(IPopOutWindow window);

    /// <summary>
    /// Restores all saved window states for a profile.
    /// </summary>
    /// <param name="profileId">The profile ID to restore window states for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RestoreWindowStatesAsync(string profileId);

    /// <summary>
    /// Saves current window states for a profile.
    /// </summary>
    /// <param name="profileId">The profile ID to save window states for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveWindowStatesAsync(string profileId);

    /// <summary>
    /// Closes all pop-out windows.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CloseAllWindowsAsync();

    /// <summary>
    /// Finds a pop-out window by component type and optional machine ID.
    /// </summary>
    /// <param name="component">The component type to find.</param>
    /// <param name="machineId">Optional machine ID for machine-specific windows.</param>
    /// <returns>The found window, or null if not found.</returns>
    IPopOutWindow? FindWindow(PopOutComponent component, string? machineId = null);
}