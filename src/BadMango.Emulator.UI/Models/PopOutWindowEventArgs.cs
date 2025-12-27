// <copyright file="PopOutWindowEventArgs.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.UI.Models;

using BadMango.Emulator.UI.Services;

/// <summary>
/// Event arguments for pop-out window lifecycle events.
/// </summary>
public class PopOutWindowEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PopOutWindowEventArgs"/> class.
    /// </summary>
    /// <param name="window">The pop-out window associated with this event.</param>
    public PopOutWindowEventArgs(IPopOutWindow window)
    {
        Window = window;
    }

    /// <summary>
    /// Gets the pop-out window associated with this event.
    /// </summary>
    public IPopOutWindow Window { get; }
}