// <copyright file="TrapCategory.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

/// <summary>
/// Classification of trap types for diagnostics and filtering.
/// </summary>
/// <remarks>
/// Traps can be enabled or disabled by category, allowing users to selectively
/// enable performance optimizations or force ROM execution for compatibility testing.
/// </remarks>
public enum TrapCategory
{
    /// <summary>
    /// Monitor ROM routines (HOME, COUT, RDKEY, etc.).
    /// </summary>
    MonitorRom,

    /// <summary>
    /// Applesoft BASIC interpreter routines.
    /// </summary>
    ApplesoftBasic,

    /// <summary>
    /// DOS 3.3 file system routines.
    /// </summary>
    Dos33,

    /// <summary>
    /// ProDOS file system routines.
    /// </summary>
    ProDos,

    /// <summary>
    /// Slot firmware (peripheral card ROMs).
    /// </summary>
    SlotFirmware,

    /// <summary>
    /// User-defined traps for custom extensions.
    /// </summary>
    UserDefined,
}