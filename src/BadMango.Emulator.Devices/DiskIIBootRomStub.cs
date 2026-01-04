// <copyright file="DiskIIBootRomStub.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Devices;

using BadMango.Emulator.Bus;
using BadMango.Emulator.Bus.Interfaces;

/// <summary>
/// Stub boot ROM for Disk II controller ($Cn00-$CnFF).
/// </summary>
/// <remarks>
/// <para>
/// This is a minimal implementation for testing slot infrastructure.
/// It provides the standard Apple II peripheral card identification bytes
/// and a simple ROM that returns to the caller.
/// </para>
/// <para>
/// The real Disk II boot ROM contains the boot code that loads the boot sector
/// from the disk into memory and transfers control to it.
/// </para>
/// </remarks>
public sealed class DiskIIBootRomStub : IBusTarget
{
    private readonly byte[] rom = new byte[256];

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskIIBootRomStub"/> class.
    /// </summary>
    public DiskIIBootRomStub()
    {
        // Initialize ROM with $FF (empty)
        Array.Fill(rom, (byte)0xFF);

        // Standard Apple II peripheral card identification bytes
        // These allow ProDOS and other software to identify the card type
        rom[0x05] = 0x38;  // SEC - Standard ID byte 1
        rom[0x07] = 0x18;  // CLC - Standard ID byte 2
        rom[0x0B] = 0x00;  // Device type: 0 = Disk II
        rom[0x0C] = 0x20;  // JSR instruction (identifies as bootable)

        // Simple boot code that just returns
        // In a real implementation, this would load and execute the boot sector
        rom[0x00] = 0x60;  // RTS - Return to caller
    }

    /// <inheritdoc />
    public TargetCaps Capabilities => TargetCaps.SupportsPeek;

    /// <inheritdoc />
    public byte Read8(uint offset, in BusAccess context)
    {
        return rom[offset & 0xFF];
    }

    /// <inheritdoc />
    public void Write8(uint offset, byte value, in BusAccess context)
    {
        // ROM ignores writes
    }
}