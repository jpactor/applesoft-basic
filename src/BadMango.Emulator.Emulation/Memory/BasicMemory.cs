// <copyright file="BasicMemory.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Emulation.Memory;

using System.Runtime.CompilerServices;

using BadMango.Emulator.Core;

/// <summary>
/// Simple memory implementation for emulated systems.
/// </summary>
/// <remarks>
/// Provides a basic 64KB memory space without special handling for I/O or ROM regions.
/// This implementation is suitable for testing and simple emulation scenarios.
/// Optimized with aggressive inlining for performance.
/// </remarks>
public class BasicMemory : IMemory
{
    private readonly byte[] memory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicMemory"/> class.
    /// </summary>
    /// <param name="size">The size of the memory in bytes. Defaults to 64KB (65536 bytes).</param>
    public BasicMemory(int size = 65536)
    {
        memory = new byte[size];
    }

    /// <inheritdoc/>
    public int Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => memory.Length;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(int address)
    {
        return memory[address];
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int address, byte value)
    {
        memory[address] = value;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadWord(int address)
    {
        return (ushort)(memory[address] | (memory[address + 1] << 8));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteWord(int address, ushort value)
    {
        memory[address] = (byte)(value & 0xFF);
        memory[address + 1] = (byte)(value >> 8);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(memory, 0, memory.Length);
    }
}