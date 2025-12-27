// <copyright file="GlobalUsings.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

#pragma warning disable SA1200 // Using directives should be placed correctly

// Re-export types from Abstractions for backward compatibility
global using BadMango.Emulator.UI.Abstractions;
global using BadMango.Emulator.UI.Abstractions.Events;

// Type aliases for backward compatibility with existing namespaces (sorted alphabetically)
global using BreakpointHitEvent = BadMango.Emulator.UI.Abstractions.Events.BreakpointHitEvent;
global using DisplayModeChangedEvent = BadMango.Emulator.UI.Abstractions.Events.DisplayModeChangedEvent;
global using IEventAggregator = BadMango.Emulator.UI.Abstractions.IEventAggregator;
global using IThemeService = BadMango.Emulator.UI.Abstractions.IThemeService;
global using MachineStateChangedEvent = BadMango.Emulator.UI.Abstractions.Events.MachineStateChangedEvent;
global using PopOutComponent = BadMango.Emulator.UI.Abstractions.PopOutComponent;
global using UnsavedWorkItem = BadMango.Emulator.UI.Abstractions.UnsavedWorkItem;
global using WindowFocusRequestEvent = BadMango.Emulator.UI.Abstractions.Events.WindowFocusRequestEvent;