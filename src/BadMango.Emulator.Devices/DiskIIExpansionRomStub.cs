// <copyright file="DiskIIExpansionRomStub.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Devices;

using BadMango.Emulator.Bus;
using BadMango.Emulator.Bus.Interfaces;

/// <summary>
/// Stub expansion ROM for Disk II controller ($C800-$CFFF when selected).
/// </summary>
/// <remarks>
/// <para>
/// This is a minimal implementation for testing slot infrastructure.
/// The Disk II expansion ROM contains the disk controller firmware including
/// read/write routines and format code.
/// </para>
/// <para>
/// The real expansion ROM is 2KB and contains the RWTS (Read/Write Track/Sector)
/// routines used by DOS 3.3 and ProDOS for disk access.
/// </para>
/// </remarks>
public sealed class DiskIIExpansionRomStub : IBusTarget
{
    private readonly byte[] rom = new byte[2048];

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskIIExpansionRomStub"/> class.
    /// </summary>
    public DiskIIExpansionRomStub()
    {
        // Initialize ROM with $FF (empty)
        Array.Fill(rom, (byte)0xFF);

        // In a real implementation, this would contain the P5/P6 PROM data
        // for disk read/write operations. For the stub, we just fill with
        // a simple pattern that can be detected in tests.
        rom[0x00] = 0xA9;  // LDA immediate
        rom[0x01] = 0x00;  // #$00
        rom[0x02] = 0x60;  // RTS
    }

    /// <inheritdoc />
    public TargetCaps Capabilities => TargetCaps.SupportsPeek;

    /// <inheritdoc />
    public byte Read8(uint offset, in BusAccess context)
    {
        return rom[offset & 0x7FF]; // Mask to 2KB
    }

    /// <inheritdoc />
    public void Write8(uint offset, byte value, in BusAccess context)
    {
        // ROM ignores writes
    }
}