// <copyright file="SoftSwitchReadHandler.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

/// <summary>
/// Delegate for soft switch read operations in the $C000-$C0FF I/O page.
/// </summary>
/// <remarks>
/// <para>
/// Soft switch read handlers are invoked when the CPU reads from an address
/// in the I/O page ($C000-$C0FF). The handler receives the offset within
/// this page (0x00-0xFF) and the bus access context.
/// </para>
/// <para>
/// Handlers may trigger side effects such as clearing keyboard strobe,
/// toggling speaker output, or changing memory mapping states. The
/// <see cref="BusAccess.IsSideEffectFree"/> property should be checked
/// if the operation should suppress side effects (e.g., during debugging).
/// </para>
/// </remarks>
/// <param name="offset">Offset within $C000-$C0FF (0x00-0xFF).</param>
/// <param name="context">Bus access context providing cycle information and access flags.</param>
/// <returns>The value read from the soft switch.</returns>
public delegate byte SoftSwitchReadHandler(byte offset, in BusAccess context);