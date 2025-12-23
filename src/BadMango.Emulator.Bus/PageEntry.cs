// <copyright file="PageEntry.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

/// <summary>
/// Represents a single entry in the page table for address routing.
/// </summary>
/// <remarks>
/// <para>
/// The page table is the "spine" of the emulator's memory system. Each page entry
/// maps a 4KB address page to a target device and contains metadata for fast routing
/// and capability checking.
/// </para>
/// <para>
/// The page lookup is O(1): <c>pageTable[address >> 12]</c> gives the entry for any address.
/// All routing decisions (device dispatch, atomic vs decomposed, tracing) flow from this lookup.
/// </para>
/// </remarks>
/// <param name="DeviceId">Structural identifier of the device handling this page.</param>
/// <param name="RegionTag">Classification of the memory region type.</param>
/// <param name="Caps">Capability flags for the target device.</param>
/// <param name="Target">The bus target implementation for this page.</param>
/// <param name="PhysicalBase">The physical base address within the target's address space.</param>
public readonly record struct PageEntry(
    int DeviceId,
    RegionTag RegionTag,
    TargetCaps Caps,
    IBusTarget Target,
    Addr PhysicalBase)
{
    /// <summary>
    /// Gets a value indicating whether this page supports Peek (read without side effects).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the target supports Peek operations;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool SupportsPeek => (Caps & TargetCaps.SupportsPeek) != 0;

    /// <summary>
    /// Gets a value indicating whether this page supports Poke (write without side effects).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the target supports Poke operations;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool SupportsPoke => (Caps & TargetCaps.SupportsPoke) != 0;

    /// <summary>
    /// Gets a value indicating whether this page supports atomic wide access.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the target supports atomic 16-bit and 32-bit operations;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool SupportsWide => (Caps & TargetCaps.SupportsWide) != 0;

    /// <summary>
    /// Gets a value indicating whether this page has side effects on access.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if accessing this page may cause observable state changes;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool HasSideEffects => (Caps & TargetCaps.HasSideEffects) != 0;

    /// <summary>
    /// Gets a value indicating whether this page is timing-sensitive.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the target behavior depends on access timing;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool IsTimingSensitive => (Caps & TargetCaps.TimingSensitive) != 0;
}