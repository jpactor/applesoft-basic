// <copyright file="MemorySizes.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Core;

/// <summary>
/// Provides commonly used memory size constants for emulated systems.
/// </summary>
/// <remarks>
/// This static class defines standard memory sizes used in various 6502-family systems,
/// making it easier to configure memory without using magic numbers.
/// </remarks>
public static class MemorySizes
{
    /// <summary>
    /// 4 KB of memory (4,096 bytes).
    /// </summary>
    public const int Size4KB = 4096;

    /// <summary>
    /// 8 KB of memory (8,192 bytes).
    /// </summary>
    public const int Size8KB = 8192;

    /// <summary>
    /// 16 KB of memory (16,384 bytes).
    /// </summary>
    public const int Size16KB = 16384;

    /// <summary>
    /// 32 KB of memory (32,768 bytes).
    /// </summary>
    public const int Size32KB = 32768;

    /// <summary>
    /// 48 KB of memory (49,152 bytes).
    /// </summary>
    public const int Size48KB = 49152;

    /// <summary>
    /// 64 KB of memory (65,536 bytes) - Standard Apple II and 6502 address space.
    /// </summary>
    public const int Size64KB = 65536;

    /// <summary>
    /// 128 KB of memory (131,072 bytes) - Apple IIe enhanced with auxiliary memory.
    /// </summary>
    public const int Size128KB = 131072;

    /// <summary>
    /// 256 KB of memory (262,144 bytes).
    /// </summary>
    public const int Size256KB = 262144;

    /// <summary>
    /// 512 KB of memory (524,288 bytes).
    /// </summary>
    public const int Size512KB = 524288;

    /// <summary>
    /// 1 MB of memory (1,048,576 bytes).
    /// </summary>
    public const int Size1MB = 1048576;

    /// <summary>
    /// 8 MB of memory (8,388,608 bytes) - Apple IIgs maximum.
    /// </summary>
    public const int Size8MB = 8388608;

    /// <summary>
    /// 16 MB of memory (16,777,216 bytes) - 65816 maximum addressable.
    /// </summary>
    public const int Size16MB = 16777216;
}