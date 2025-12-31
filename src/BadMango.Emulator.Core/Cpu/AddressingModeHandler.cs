// <copyright file="AddressingModeHandler.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Core.Cpu;

using Interfaces.Cpu;

/// <summary>
/// Delegate representing an addressing mode that computes an effective address.
/// </summary>
/// <typeparam name="TState">The CPU state type.</typeparam>
/// <param name="cpu">The CPU instance providing memory access and state.</param>
/// <returns>The effective address computed by the addressing mode.</returns>
/// <remarks>
/// <para>
/// Addressing modes access CPU state via <see cref="ICpu.State"/> and memory via
/// <see cref="ICpu.Read8"/>, <see cref="ICpu.Read16"/>, etc.
/// </para>
/// <para>
/// The TState type parameter is retained for flexibility in supporting different
/// CPU state structures across 6502, 65C02, 65816, and 65832 implementations.
/// </para>
/// </remarks>
public delegate Addr AddressingModeHandler<TState>(ICpu cpu)
    where TState : struct;