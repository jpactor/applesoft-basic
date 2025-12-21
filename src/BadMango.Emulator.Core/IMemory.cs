// <copyright file="IMemory.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Core;

/// <summary>
/// Base interface for memory management in emulated systems.
/// </summary>
/// <remarks>
/// This interface provides the foundation for various memory models including
/// standard 64KB configurations, banked memory, and extended memory systems.
/// </remarks>
public interface IMemory
{
    /// <summary>
    /// Gets the size of the emulated memory in bytes.
    /// </summary>
    uint Size { get; }

    /// <summary>
    /// Reads a byte from the specified memory address.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <returns>The byte value at the specified address.</returns>
    byte Read(int address);

    /// <summary>
    /// Reads a byte from the specified memory address.
    /// </summary>
    /// <param name="address">The memory address to read from.</param>
    /// <returns>The byte value at the specified address.</returns>
    /// <remarks>
    /// This method is functionally identical to <see cref="Read"/> and is provided
    /// for future-proofing to support additional read methods (e.g., Read16, Read32).
    /// </remarks>
    byte ReadByte(int address) => Read(address);

    /// <summary>
    /// Writes a byte to the specified memory address.
    /// </summary>
    /// <param name="address">The memory address to write to.</param>
    /// <param name="value">The byte value to write.</param>
    void Write(int address, byte value);

    /// <summary>
    /// Writes a byte to the specified memory address.
    /// </summary>
    /// <param name="address">The memory address to write to.</param>
    /// <param name="value">The byte value to write.</param>
    /// <remarks>
    /// This method is functionally identical to <see cref="Write"/> and is provided
    /// for future-proofing to support additional write methods (e.g., Write16, Write32).
    /// </remarks>
    void WriteByte(int address, byte value) => Write(address, value);

    /// <summary>
    /// Reads a 16-bit word from the specified memory address.
    /// </summary>
    /// <param name="address">The starting address to read from.</param>
    /// <returns>The 16-bit word value (low byte first, high byte second).</returns>
    ushort ReadWord(int address);

    /// <summary>
    /// Writes a 16-bit word to the specified memory address.
    /// </summary>
    /// <param name="address">The starting address to write to.</param>
    /// <param name="value">The 16-bit word value to write (low byte first, high byte second).</param>
    void WriteWord(int address, ushort value);

    /// <summary>
    /// Clears all memory, setting all bytes to zero.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets a snapshot of the entire memory as a <see cref="ReadOnlyMemory{T}"/>.
    /// </summary>
    /// <returns>A read-only snapshot of the memory contents.</returns>
    /// <remarks>
    /// This method provides a safe, read-only view of the memory that can be used
    /// for debugging, serialization, or analysis without risking modifications.
    /// </remarks>
    ReadOnlyMemory<byte> AsReadOnlyMemory();

    /// <summary>
    /// Gets a snapshot of the entire memory as a <see cref="Memory{T}"/>.
    /// </summary>
    /// <returns>A mutable snapshot of the memory contents.</returns>
    /// <remarks>
    /// This method provides direct access to the underlying memory buffer.
    /// Use with caution as modifications will affect the emulated system's state.
    /// </remarks>
    Memory<byte> AsMemory();
}