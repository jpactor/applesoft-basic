// <copyright file="ITrapRegistry.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Interfaces;

using BadMango.Emulator.Core.Interfaces.Cpu;

/// <summary>
/// Registry for ROM routine interception handlers.
/// </summary>
/// <remarks>
/// <para>
/// The trap registry enables native implementations of ROM routines for performance
/// optimization. When the CPU fetches an instruction at a registered address, the
/// trap handler is invoked instead of executing the ROM code.
/// </para>
/// <para>
/// Traps can be:
/// </para>
/// <list type="bullet">
/// <item><description>Registered at specific addresses with metadata.</description></item>
/// <item><description>Enabled/disabled individually or by category.</description></item>
/// <item><description>Slot-dependent (only fire when a specific slot's expansion ROM is active).</description></item>
/// </list>
/// <para>
/// The registry provides O(1) lookup performance using an internal array indexed
/// by address, suitable for the hot path in instruction fetch.
/// </para>
/// </remarks>
public interface ITrapRegistry
{
    /// <summary>
    /// Gets the number of traps currently registered.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Registers a trap handler at a specific address.
    /// </summary>
    /// <param name="address">The ROM address to intercept.</param>
    /// <param name="name">Human-readable name for the trap (e.g., "HOME", "COUT").</param>
    /// <param name="category">Classification of the trap for filtering.</param>
    /// <param name="handler">The native implementation delegate.</param>
    /// <param name="description">Optional detailed description for tooling.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a trap is already registered at the specified address.
    /// </exception>
    void Register(
        Addr address,
        string name,
        TrapCategory category,
        TrapHandler handler,
        string? description = null);

    /// <summary>
    /// Registers a slot-dependent trap handler.
    /// </summary>
    /// <param name="address">The ROM address to intercept (typically in $C800-$CFFF expansion ROM space).</param>
    /// <param name="slot">The slot number (1-7) this trap is associated with.</param>
    /// <param name="name">Human-readable name for the trap.</param>
    /// <param name="category">Classification of the trap for filtering.</param>
    /// <param name="handler">The native implementation delegate.</param>
    /// <param name="description">Optional detailed description for tooling.</param>
    /// <remarks>
    /// <para>
    /// Slot-dependent traps only fire when the specified slot's expansion ROM is active
    /// in the $C800-$CFFF region. The handler should still verify slot state as a
    /// defense-in-depth measure.
    /// </para>
    /// </remarks>
    void RegisterSlotDependent(
        Addr address,
        int slot,
        string name,
        TrapCategory category,
        TrapHandler handler,
        string? description = null);

    /// <summary>
    /// Unregisters a trap at the specified address.
    /// </summary>
    /// <param name="address">The ROM address to unregister.</param>
    /// <returns>
    /// <see langword="true"/> if a trap was unregistered;
    /// <see langword="false"/> if no trap was registered at that address.
    /// </returns>
    bool Unregister(Addr address);

    /// <summary>
    /// Attempts to execute a trap at the specified address.
    /// </summary>
    /// <param name="address">The instruction fetch address.</param>
    /// <param name="cpu">The CPU instance for register access.</param>
    /// <param name="bus">The memory bus for RAM access.</param>
    /// <param name="context">The event context for scheduling and signals.</param>
    /// <returns>
    /// A <see cref="TrapResult"/> with the handler's result, or
    /// <see cref="TrapResult.NotHandled"/> if no trap is registered or enabled
    /// at the address.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called from the CPU's instruction fetch hot path. It performs
    /// O(1) lookup and returns immediately if no trap is registered or enabled.
    /// </para>
    /// </remarks>
    TrapResult TryExecute(Addr address, ICpu cpu, IMemoryBus bus, IEventContext context);

    /// <summary>
    /// Checks if a trap is registered at the specified address.
    /// </summary>
    /// <param name="address">The address to check.</param>
    /// <returns>
    /// <see langword="true"/> if a trap is registered (regardless of enabled state);
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool HasTrap(Addr address);

    /// <summary>
    /// Gets trap information for the specified address.
    /// </summary>
    /// <param name="address">The address to look up.</param>
    /// <returns>
    /// The trap information if registered; otherwise, <see langword="null"/>.
    /// </returns>
    TrapInfo? GetTrapInfo(Addr address);

    /// <summary>
    /// Enables or disables a trap at a specific address.
    /// </summary>
    /// <param name="address">The trap address.</param>
    /// <param name="enabled">
    /// <see langword="true"/> to enable; <see langword="false"/> to disable.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the trap exists and was updated;
    /// <see langword="false"/> if no trap is registered at that address.
    /// </returns>
    bool SetEnabled(Addr address, bool enabled);

    /// <summary>
    /// Enables or disables all traps in a category.
    /// </summary>
    /// <param name="category">The category to update.</param>
    /// <param name="enabled">
    /// <see langword="true"/> to enable; <see langword="false"/> to disable.
    /// </param>
    /// <returns>The number of traps that were updated.</returns>
    int SetCategoryEnabled(TrapCategory category, bool enabled);

    /// <summary>
    /// Gets all registered trap addresses.
    /// </summary>
    /// <returns>An enumerable of all registered trap addresses.</returns>
    IEnumerable<Addr> GetRegisteredAddresses();

    /// <summary>
    /// Gets all registered trap information.
    /// </summary>
    /// <returns>An enumerable of all registered trap information.</returns>
    IEnumerable<TrapInfo> GetAllTraps();

    /// <summary>
    /// Clears all registered traps.
    /// </summary>
    void Clear();
}