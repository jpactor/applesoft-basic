// <copyright file="CpuState.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Core.Cpu;

/// <summary>
/// Represents a snapshot of CPU state for testing and debugging purposes.
/// </summary>
/// <remarks>
/// <para>
/// This structure provides a simple container for CPU state that can be captured
/// and compared during testing. It is NOT the authoritative source of CPU state -
/// that responsibility belongs to the ICpu implementation directly.
/// </para>
/// <para>
/// For runtime CPU operation, use <see cref="Interfaces.Cpu.ICpu.Registers"/> and
/// <see cref="Interfaces.Cpu.ICpu.GetCycles"/> directly.
/// </para>
/// </remarks>
public struct CpuState
{
    /// <summary>
    /// Gets or sets the CPU registers.
    /// </summary>
    public Registers Registers;

    /// <summary>
    /// Gets or sets the cycle count.
    /// </summary>
    public ulong Cycles;
}
