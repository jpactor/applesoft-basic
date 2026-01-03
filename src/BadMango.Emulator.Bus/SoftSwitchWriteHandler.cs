// <copyright file="SoftSwitchWriteHandler.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

/// <summary>
/// Delegate for soft switch write operations in the $C000-$C0FF I/O page.
/// </summary>
/// <remarks>
/// <para>
/// Soft switch write handlers are invoked when the CPU writes to an address
/// in the I/O page ($C000-$C0FF). The handler receives the offset within
/// this page (0x00-0xFF), the value being written, and the bus access context.
/// </para>
/// <para>
/// Many soft switches are toggle-on-access and ignore the written value,
/// responding only to the access itself. Others may use the value to set
/// specific states or control peripheral devices.
/// </para>
/// <para>
/// The <see cref="BusAccess.IsSideEffectFree"/> property should be checked
/// if the operation should suppress side effects (e.g., during debugging).
/// </para>
/// </remarks>
/// <param name="offset">Offset within $C000-$C0FF (0x00-0xFF).</param>
/// <param name="value">Value being written.</param>
/// <param name="context">Bus access context providing cycle information and access flags.</param>
public delegate void SoftSwitchWriteHandler(byte offset, byte value, in BusAccess context);