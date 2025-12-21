// <copyright file="CpuState.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Core;

using System.Runtime.InteropServices;

/// <summary>
/// Represents the complete state of a 6502-family CPU.
/// </summary>
/// <remarks>
/// This structure captures all CPU registers and execution state for
/// save states, debugging, and state inspection purposes.
/// Uses explicit layout for optimal memory packing.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CpuState
{
    /// <summary>
    /// Gets or sets the Accumulator register (A).
    /// </summary>
    public byte A { get; set; }

    /// <summary>
    /// Gets or sets the X index register.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the Y index register.
    /// </summary>
    public byte Y { get; set; }

    /// <summary>
    /// Gets or sets the Stack Pointer register (S).
    /// </summary>
    public byte S { get; set; }

    /// <summary>
    /// Gets or sets the Processor Status register (P).
    /// </summary>
    public byte P { get; set; }

    /// <summary>
    /// Gets or sets the Program Counter (PC).
    /// </summary>
    public ushort PC { get; set; }

    /// <summary>
    /// Gets or sets the total number of cycles executed.
    /// </summary>
    public ulong Cycles { get; set; }
}