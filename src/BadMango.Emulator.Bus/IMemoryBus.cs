// <copyright file="IMemoryBus.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

/// <summary>
/// Main memory bus interface for routing CPU and DMA memory operations.
/// </summary>
/// <remarks>
/// <para>
/// The memory bus is the central routing fabric for all memory operations in the
/// emulated system. It implements page-based address translation, handles atomic
/// vs decomposed access decisions, and provides the foundation for observability.
/// </para>
/// <para>
/// The bus uses 4KB pages for routing, with each page resolving to a target device
/// and physical base address. Cross-page wide accesses are automatically decomposed
/// into individual byte operations.
/// </para>
/// <para>
/// The CPU does not own memory; all memory interactions flow through the bus.
/// The CPU computes intent; the bus enforces consequences.
/// </para>
/// </remarks>
public interface IMemoryBus
{
    /// <summary>
    /// Gets the page shift value for address-to-page translation.
    /// </summary>
    /// <value>
    /// The number of bits to shift an address to get the page index.
    /// For 4KB pages, this value is 12.
    /// </value>
    int PageShift { get; }

    /// <summary>
    /// Gets the page mask for extracting the offset within a page.
    /// </summary>
    /// <value>
    /// A bitmask that extracts the offset portion of an address.
    /// For 4KB pages, this value is 0xFFF.
    /// </value>
    Addr PageMask { get; }

    /// <summary>
    /// Gets the total number of pages in the address space.
    /// </summary>
    /// <value>The count of page table entries.</value>
    int PageCount { get; }

    /// <summary>
    /// Reads a single byte from the specified address.
    /// </summary>
    /// <param name="access">The access context describing the operation.</param>
    /// <returns>The byte value at the specified address.</returns>
    byte Read8(in BusAccess access);

    /// <summary>
    /// Writes a single byte to the specified address.
    /// </summary>
    /// <param name="access">The access context describing the operation.</param>
    /// <param name="value">The byte value to write.</param>
    void Write8(in BusAccess access, byte value);

    /// <summary>
    /// Reads a 16-bit word from the specified address.
    /// </summary>
    /// <param name="access">The access context describing the operation.</param>
    /// <returns>The 16-bit value at the specified address (little-endian).</returns>
    /// <remarks>
    /// <para>
    /// The bus determines whether to use atomic or decomposed access based on:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The <see cref="AccessFlags.Decompose"/> flag (forces byte-wise cycles)</description></item>
    /// <item><description>Whether the access crosses a page boundary (always decomposes)</description></item>
    /// <item><description>The target's <see cref="TargetCaps.SupportsWide"/> capability</description></item>
    /// <item><description>The <see cref="AccessFlags.Atomic"/> flag and CPU mode defaults</description></item>
    /// </list>
    /// </remarks>
    Word Read16(in BusAccess access);

    /// <summary>
    /// Writes a 16-bit word to the specified address.
    /// </summary>
    /// <param name="access">The access context describing the operation.</param>
    /// <param name="value">The 16-bit value to write (little-endian).</param>
    /// <remarks>
    /// <para>
    /// The bus determines whether to use atomic or decomposed access based on:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The <see cref="AccessFlags.Decompose"/> flag (forces byte-wise cycles)</description></item>
    /// <item><description>Whether the access crosses a page boundary (always decomposes)</description></item>
    /// <item><description>The target's <see cref="TargetCaps.SupportsWide"/> capability</description></item>
    /// <item><description>The <see cref="AccessFlags.Atomic"/> flag and CPU mode defaults</description></item>
    /// </list>
    /// </remarks>
    void Write16(in BusAccess access, Word value);

    /// <summary>
    /// Reads a 32-bit double word from the specified address.
    /// </summary>
    /// <param name="access">The access context describing the operation.</param>
    /// <returns>The 32-bit value at the specified address (little-endian).</returns>
    /// <remarks>
    /// <para>
    /// The bus determines whether to use atomic or decomposed access based on:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The <see cref="AccessFlags.Decompose"/> flag (forces byte-wise cycles)</description></item>
    /// <item><description>Whether the access crosses a page boundary (always decomposes)</description></item>
    /// <item><description>The target's <see cref="TargetCaps.SupportsWide"/> capability</description></item>
    /// <item><description>The <see cref="AccessFlags.Atomic"/> flag and CPU mode defaults</description></item>
    /// </list>
    /// </remarks>
    DWord Read32(in BusAccess access);

    /// <summary>
    /// Writes a 32-bit double word to the specified address.
    /// </summary>
    /// <param name="access">The access context describing the operation.</param>
    /// <param name="value">The 32-bit value to write (little-endian).</param>
    /// <remarks>
    /// <para>
    /// The bus determines whether to use atomic or decomposed access based on:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The <see cref="AccessFlags.Decompose"/> flag (forces byte-wise cycles)</description></item>
    /// <item><description>Whether the access crosses a page boundary (always decomposes)</description></item>
    /// <item><description>The target's <see cref="TargetCaps.SupportsWide"/> capability</description></item>
    /// <item><description>The <see cref="AccessFlags.Atomic"/> flag and CPU mode defaults</description></item>
    /// </list>
    /// </remarks>
    void Write32(in BusAccess access, DWord value);

    /// <summary>
    /// Gets the page entry for the specified address.
    /// </summary>
    /// <param name="address">The address to look up.</param>
    /// <returns>The page entry containing routing and capability information.</returns>
    PageEntry GetPageEntry(Addr address);

    /// <summary>
    /// Maps a page to a target device.
    /// </summary>
    /// <param name="pageIndex">The index of the page to map.</param>
    /// <param name="entry">The page entry describing the mapping.</param>
    /// <remarks>
    /// This is a control plane operation used during system initialization,
    /// bank switching, and overlay changes. It should not be called during
    /// normal hot-path execution.
    /// </remarks>
    void MapPage(int pageIndex, PageEntry entry);

    /// <summary>
    /// Maps a range of pages to a target device.
    /// </summary>
    /// <param name="startPage">The index of the first page to map.</param>
    /// <param name="pageCount">The number of consecutive pages to map.</param>
    /// <param name="deviceId">The device identifier for all pages.</param>
    /// <param name="regionTag">The region type for all pages.</param>
    /// <param name="caps">The capabilities for all pages.</param>
    /// <param name="target">The bus target for all pages.</param>
    /// <param name="physicalBase">The physical base address for the first page.</param>
    /// <remarks>
    /// This convenience method maps multiple consecutive pages with incrementing
    /// physical addresses. The physical address for page N is
    /// <paramref name="physicalBase"/> + (N - <paramref name="startPage"/>) * PageSize.
    /// </remarks>
    void MapPageRange(
        int startPage,
        int pageCount,
        int deviceId,
        RegionTag regionTag,
        TargetCaps caps,
        IBusTarget target,
        Addr physicalBase);
}