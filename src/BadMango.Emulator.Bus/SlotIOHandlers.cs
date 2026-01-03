// <copyright file="SlotIOHandlers.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

/// <summary>
/// Handler arrays for a slot card's 16-byte I/O region ($C0n0-$C0nF).
/// </summary>
/// <remarks>
/// <para>
/// Each peripheral slot on the Apple IIe is allocated 16 bytes of I/O space
/// within the $C080-$C0FF region. This class provides arrays to hold
/// read and write handlers for these 16 addresses.
/// </para>
/// <para>
/// Slot I/O Layout:
/// </para>
/// <list type="bullet">
/// <item><description>$C080-$C08F (offset 0x80): Slot 0 (Language Card)</description></item>
/// <item><description>$C090-$C09F (offset 0x90): Slot 1</description></item>
/// <item><description>$C0A0-$C0AF (offset 0xA0): Slot 2</description></item>
/// <item><description>$C0B0-$C0BF (offset 0xB0): Slot 3</description></item>
/// <item><description>$C0C0-$C0CF (offset 0xC0): Slot 4</description></item>
/// <item><description>$C0D0-$C0DF (offset 0xD0): Slot 5</description></item>
/// <item><description>$C0E0-$C0EF (offset 0xE0): Slot 6</description></item>
/// <item><description>$C0F0-$C0FF (offset 0xF0): Slot 7</description></item>
/// </list>
/// <para>
/// Peripheral cards create a <see cref="SlotIOHandlers"/> instance,
/// populate the handler arrays, and install them via
/// <see cref="IOPageDispatcher.InstallSlotHandlers"/>.
/// </para>
/// </remarks>
public sealed class SlotIOHandlers
{
    private const int SlotIOSize = 16;

    private readonly SoftSwitchReadHandler?[] readHandlers;
    private readonly SoftSwitchWriteHandler?[] writeHandlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotIOHandlers"/> class.
    /// </summary>
    public SlotIOHandlers()
    {
        readHandlers = new SoftSwitchReadHandler?[SlotIOSize];
        writeHandlers = new SoftSwitchWriteHandler?[SlotIOSize];
    }

    /// <summary>
    /// Gets the read handler array for the slot's 16-byte I/O region.
    /// </summary>
    /// <value>An array of 16 read handlers (some may be null).</value>
    public SoftSwitchReadHandler?[] ReadHandlers => readHandlers;

    /// <summary>
    /// Gets the write handler array for the slot's 16-byte I/O region.
    /// </summary>
    /// <value>An array of 16 write handlers (some may be null).</value>
    public SoftSwitchWriteHandler?[] WriteHandlers => writeHandlers;

    /// <summary>
    /// Sets read and/or write handlers for a specific offset within the slot's I/O region.
    /// </summary>
    /// <param name="offset">
    /// The offset within the slot's 16-byte region (0x00-0x0F).
    /// </param>
    /// <param name="read">
    /// The read handler, or <see langword="null"/> to leave unhandled.
    /// </param>
    /// <param name="write">
    /// The write handler, or <see langword="null"/> to leave unhandled.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="offset"/> is greater than 0x0F.
    /// </exception>
    public void Set(byte offset, SoftSwitchReadHandler? read, SoftSwitchWriteHandler? write)
    {
        if (offset >= SlotIOSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                offset,
                $"Slot I/O offset must be 0x00-0x0F, was 0x{offset:X2}.");
        }

        readHandlers[offset] = read;
        writeHandlers[offset] = write;
    }
}