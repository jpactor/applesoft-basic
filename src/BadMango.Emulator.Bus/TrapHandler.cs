// <copyright file="TrapHandler.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

using BadMango.Emulator.Core.Interfaces.Cpu;

using Interfaces;

/// <summary>
/// Delegate for ROM routine interception handlers.
/// </summary>
/// <remarks>
/// <para>
/// Trap handlers are invoked when the CPU fetches an instruction at a registered
/// address. The handler receives access to the CPU (for register manipulation),
/// the memory bus (for RAM access), and the event context (for scheduling and signals).
/// </para>
/// <para>
/// The handler can inspect the current CPU state to determine if the trap should
/// be handled (e.g., checking if the correct expansion ROM is active for slot-dependent
/// traps). If the trap is not applicable, the handler should return
/// <see cref="TrapResult.NotHandled"/> to allow the ROM code to execute.
/// </para>
/// <para>
/// When a handler successfully processes a trap, it should:
/// </para>
/// <list type="bullet">
/// <item><description>Perform the native implementation of the ROM routine.</description></item>
/// <item><description>Update CPU registers as the ROM routine would.</description></item>
/// <item><description>Return a <see cref="TrapResult"/> with <c>Handled = true</c> and the appropriate cycle count.</description></item>
/// </list>
/// </remarks>
/// <param name="cpu">The CPU instance for register access and state manipulation.</param>
/// <param name="bus">The memory bus for RAM access during trap handling.</param>
/// <param name="context">The event context for scheduling and signal access.</param>
/// <returns>A <see cref="TrapResult"/> indicating whether the trap was handled.</returns>
public delegate TrapResult TrapHandler(ICpu cpu, IMemoryBus bus, IEventContext context);