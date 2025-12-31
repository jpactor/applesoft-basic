// <copyright file="OpcodeHandler.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Core.Cpu;

using Interfaces.Cpu;

/// <summary>
/// Delegate for opcode handlers that operate on the CPU instance.
/// </summary>
/// <param name="cpu">The CPU instance providing memory access and state.</param>
/// <remarks>
/// <para>
/// Handlers access CPU state via <see cref="ICpu.State"/> and memory via
/// <see cref="ICpu.Read8"/>, <see cref="ICpu.Write8"/>, etc.
/// </para>
/// <para>
/// The CPU internally routes memory operations through the bus architecture,
/// handling cycle counting, bus faults, and permission checks.
/// </para>
/// </remarks>
public delegate void OpcodeHandler(ICpu cpu);