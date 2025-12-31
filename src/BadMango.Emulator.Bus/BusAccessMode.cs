// <copyright file="BusAccessMode.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

/// <summary>
/// Defines the bus access semantics for memory operations.
/// </summary>
/// <remarks>
/// This determines whether the bus prefers atomic wide operations or
/// decomposes multi-byte accesses into individual byte cycles.
/// </remarks>
public enum BusAccessMode : byte
{
    /// <summary>
    /// Native mode: prefers atomic wide operations when the target supports them.
    /// </summary>
    /// <remarks>
    /// Used by 65832 native mode for better performance with modern memory.
    /// </remarks>
    Atomic = 0,

    /// <summary>
    /// Compatibility mode: decomposes wide accesses into byte-wise cycles.
    /// </summary>
    /// <remarks>
    /// Matches 65C02/65816 expectations where peripherals observe individual
    /// memory access cycles. Required for accurate emulation of devices
    /// that depend on seeing each byte access separately.
    /// </remarks>
    Decomposed = 1,
}