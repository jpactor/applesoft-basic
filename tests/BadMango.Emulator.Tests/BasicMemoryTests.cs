// <copyright file="BasicMemoryTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Tests;

using BadMango.Emulator.Core;
using BadMango.Emulator.Emulation.Memory;

/// <summary>
/// Unit tests for the <see cref="BasicMemory"/> class.
/// </summary>
[TestFixture]
public class BasicMemoryTests
{
    /// <summary>
    /// Verifies that BasicMemory can be created with MemorySizes constants.
    /// </summary>
    [Test]
    public void BasicMemory_CanBeCreatedWithMemorySizesConstants()
    {
        // Arrange & Act
        var mem64K = new BasicMemory(MemorySizes.Size64KB);
        var mem128K = new BasicMemory(MemorySizes.Size128KB);
        var mem4K = new BasicMemory(MemorySizes.Size4KB);

        // Assert
        Assert.That(mem64K.Size, Is.EqualTo(65536));
        Assert.That(mem128K.Size, Is.EqualTo(131072));
        Assert.That(mem4K.Size, Is.EqualTo(4096));
    }

    /// <summary>
    /// Verifies that BasicMemory uses Size64KB as default size.
    /// </summary>
    [Test]
    public void BasicMemory_DefaultSizeIs64KB()
    {
        // Arrange & Act
        var memory = new BasicMemory();

        // Assert
        Assert.That(memory.Size, Is.EqualTo(MemorySizes.Size64KB));
        Assert.That(memory.Size, Is.EqualTo(65536));
    }

    /// <summary>
    /// Verifies that AsReadOnlyMemory returns a snapshot of the memory.
    /// </summary>
    [Test]
    public void AsReadOnlyMemory_ReturnsSnapshot()
    {
        // Arrange
        var memory = new BasicMemory(MemorySizes.Size4KB);
        memory.Write(0x100, 0x42);
        memory.Write(0x200, 0xFF);

        // Act
        var snapshot = memory.AsReadOnlyMemory();

        // Assert
        Assert.That(snapshot.Length, Is.EqualTo(4096));
        Assert.That(snapshot.Span[0x100], Is.EqualTo(0x42));
        Assert.That(snapshot.Span[0x200], Is.EqualTo(0xFF));
    }

    /// <summary>
    /// Verifies that AsMemory returns a mutable snapshot of the memory.
    /// </summary>
    [Test]
    public void AsMemory_ReturnsMutableSnapshot()
    {
        // Arrange
        var memory = new BasicMemory(MemorySizes.Size4KB);
        memory.Write(0x100, 0x42);

        // Act
        var snapshot = memory.AsMemory();
        snapshot.Span[0x200] = 0xAA;

        // Assert
        Assert.That(snapshot.Length, Is.EqualTo(4096));
        Assert.That(snapshot.Span[0x100], Is.EqualTo(0x42));
        Assert.That(snapshot.Span[0x200], Is.EqualTo(0xAA));
        Assert.That(memory.Read(0x200), Is.EqualTo(0xAA));
    }

    /// <summary>
    /// Verifies that AsReadOnlyMemory snapshot reflects current memory state.
    /// </summary>
    [Test]
    public void AsReadOnlyMemory_ReflectsCurrentState()
    {
        // Arrange
        var memory = new BasicMemory(MemorySizes.Size4KB);
        memory.Write(0x100, 0x42);

        var snapshot1 = memory.AsReadOnlyMemory();

        // Act - modify memory after snapshot
        memory.Write(0x100, 0xFF);
        var snapshot2 = memory.AsReadOnlyMemory();

        // Assert - snapshot reflects the backing array state
        Assert.That(snapshot1.Span[0x100], Is.EqualTo(0xFF));
        Assert.That(snapshot2.Span[0x100], Is.EqualTo(0xFF));
    }

    /// <summary>
    /// Verifies that MemorySizes constants have expected values.
    /// </summary>
    [Test]
    public void MemorySizes_ConstantsHaveExpectedValues()
    {
        // Assert standard sizes
        Assert.That(MemorySizes.Size4KB, Is.EqualTo(4096));
        Assert.That(MemorySizes.Size8KB, Is.EqualTo(8192));
        Assert.That(MemorySizes.Size16KB, Is.EqualTo(16384));
        Assert.That(MemorySizes.Size32KB, Is.EqualTo(32768));
        Assert.That(MemorySizes.Size48KB, Is.EqualTo(49152));
        Assert.That(MemorySizes.Size64KB, Is.EqualTo(65536));
        Assert.That(MemorySizes.Size128KB, Is.EqualTo(131072));
        Assert.That(MemorySizes.Size256KB, Is.EqualTo(262144));
        Assert.That(MemorySizes.Size512KB, Is.EqualTo(524288));
        Assert.That(MemorySizes.Size1MB, Is.EqualTo(1048576));
        Assert.That(MemorySizes.Size8MB, Is.EqualTo(8388608));
        Assert.That(MemorySizes.Size16MB, Is.EqualTo(16777216));
    }
}