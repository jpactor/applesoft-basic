// <copyright file="ThemeService.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.UI.Services;

using Avalonia;
using Avalonia.Styling;

using BadMango.Emulator.Infrastructure.Events;
using BadMango.Emulator.UI.Abstractions.Events;

using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing application themes (dark/light mode).
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService>? logger;
    private readonly IEventAggregator? eventAggregator;
    private bool isDarkTheme = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for theme operations.</param>
    /// <param name="eventAggregator">Optional event aggregator for pub/sub messaging.</param>
    public ThemeService(ILogger<ThemeService>? logger = null, IEventAggregator? eventAggregator = null)
    {
        this.logger = logger;
        this.eventAggregator = eventAggregator;
    }

    /// <inheritdoc />
    public event EventHandler<bool>? ThemeChanged;

    /// <inheritdoc />
    public bool IsDarkTheme => isDarkTheme;

    /// <inheritdoc />
    public void SetTheme(bool isDark)
    {
        if (isDarkTheme == isDark)
        {
            return;
        }

        isDarkTheme = isDark;
        logger?.LogInformation("Theme changed to {Theme}", isDark ? "Dark" : "Light");

        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = isDark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }

        // Raise the direct event
        ThemeChanged?.Invoke(this, isDark);

        // Publish to the event aggregator for loose coupling
        eventAggregator?.Publish(new ThemeChangedEvent(isDark));
    }
}