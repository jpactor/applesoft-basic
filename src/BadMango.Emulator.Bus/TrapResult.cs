// <copyright file="TrapResult.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus;

using Core;

/// <summary>
/// Result of a trap handler execution.
/// </summary>
/// <remarks>
/// <para>
/// When the CPU fetches an instruction at a trapped address, the registered handler
/// is invoked. The handler can either handle the operation natively (returning
/// <see cref="Handled"/> = <see langword="true"/>) or decline (returning
/// <see cref="Handled"/> = <see langword="false"/>), allowing the ROM code to execute.
/// </para>
/// <para>
/// If a handler returns <see langword="true"/> for <see cref="Handled"/>, the CPU
/// will charge the <see cref="CyclesConsumed"/> cycles and optionally redirect
/// execution to <see cref="ReturnAddress"/> (or continue to the next instruction
/// if <see langword="null"/>).
/// </para>
/// </remarks>
/// <param name="Handled">
/// <see langword="true"/> if the trap was handled natively; <see langword="false"/>
/// to fall through to ROM execution.
/// </param>
/// <param name="CyclesConsumed">
/// The number of cycles to charge for this operation. Only meaningful when
/// <see cref="Handled"/> is <see langword="true"/>.
/// </param>
/// <param name="ReturnAddress">
/// Override return address. If <see langword="null"/>, the CPU continues to the
/// next instruction (after stack RTS if applicable). If set, the CPU jumps to
/// this address after trap completion.
/// </param>
public readonly record struct TrapResult(
    bool Handled,
    Cycle CyclesConsumed,
    Addr? ReturnAddress)
{
    /// <summary>
    /// Gets a result indicating the trap was not handled and ROM should execute.
    /// </summary>
    public static TrapResult NotHandled => new(Handled: false, default, null);

    /// <summary>
    /// Creates a result indicating the trap was handled with the specified cycle cost.
    /// </summary>
    /// <param name="cycles">The number of cycles consumed by the trap handler.</param>
    /// <returns>A <see cref="TrapResult"/> indicating successful handling.</returns>
    public static TrapResult Success(Cycle cycles) => new(Handled: true, cycles, null);

    /// <summary>
    /// Creates a result indicating the trap was handled with a redirect to a different address.
    /// </summary>
    /// <param name="cycles">The number of cycles consumed by the trap handler.</param>
    /// <param name="returnAddress">The address to jump to after trap completion.</param>
    /// <returns>A <see cref="TrapResult"/> indicating successful handling with redirection.</returns>
    public static TrapResult SuccessWithRedirect(Cycle cycles, Addr returnAddress) =>
        new(Handled: true, cycles, returnAddress);
}