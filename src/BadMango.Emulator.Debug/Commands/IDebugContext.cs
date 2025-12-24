// <copyright file="IDebugContext.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Commands;

using BadMango.Emulator.Core;

/// <summary>
/// Provides extended context for debug command execution including access to emulator components.
/// </summary>
/// <remarks>
/// The debug context extends <see cref="ICommandContext"/> to provide commands with
/// access to the CPU, memory, and disassembler for debugging operations.
/// </remarks>
public interface IDebugContext : ICommandContext
{
    /// <summary>
    /// Gets the CPU instance for the debug session.
    /// </summary>
    /// <remarks>
    /// May be null if no CPU has been attached to the debug context.
    /// Commands should check for null before accessing CPU operations.
    /// </remarks>
    ICpu? Cpu { get; }

    /// <summary>
    /// Gets the memory instance for the debug session.
    /// </summary>
    /// <remarks>
    /// May be null if no memory has been attached to the debug context.
    /// Commands should check for null before accessing memory operations.
    /// </remarks>
    IMemory? Memory { get; }

    /// <summary>
    /// Gets the disassembler instance for the debug session.
    /// </summary>
    /// <remarks>
    /// May be null if no disassembler has been attached to the debug context.
    /// Commands should check for null before accessing disassembly operations.
    /// </remarks>
    IDisassembler? Disassembler { get; }

    /// <summary>
    /// Gets a value indicating whether a system is attached to this debug context.
    /// </summary>
    /// <remarks>
    /// Returns true if CPU, Memory, and Disassembler are all available.
    /// </remarks>
    bool IsSystemAttached { get; }
}