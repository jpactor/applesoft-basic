// <copyright file="AddressingModesTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Tests;

using Core.Cpu;
using Core.Interfaces;

using Emulation.Cpu;
using Emulation.Memory;

/// <summary>
/// Comprehensive unit tests for addressing mode implementations.
/// </summary>
[TestFixture]
public class AddressingModesTests
{
    private IMemory memory = null!;
    private Cpu65C02 cpu = null!;

    /// <summary>
    /// Sets up test environment.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        memory = new BasicMemory();
        cpu = new Cpu65C02(memory);
    }

    /// <summary>
    /// Verifies that Implied addressing mode returns zero and doesn't modify state.
    /// </summary>
    [Test]
    public void Implied_ReturnsZeroAndDoesNotModifyState()
    {
        // Arrange
        SetupCpu(pc: 0x1000, cycles: 10);

        // Act
        Addr address = AddressingModes.Implied(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1000)); // PC unchanged
        Assert.That(cpu.GetCycles(), Is.EqualTo(10)); // Cycles unchanged
    }

    /// <summary>
    /// Verifies that Immediate addressing mode returns PC and increments it.
    /// </summary>
    [Test]
    public void Immediate_ReturnsPCAndIncrementsIt()
    {
        // Arrange
        SetupCpu(pc: 0x1000, cycles: 10);

        // Act
        Addr address = AddressingModes.Immediate(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x1000));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001)); // PC incremented
        Assert.That(cpu.GetCycles(), Is.EqualTo(10)); // No extra cycles for immediate
    }

    /// <summary>
    /// Verifies that ZeroPage addressing mode reads address and adds 1 cycle.
    /// </summary>
    [Test]
    public void ZeroPage_ReadsAddressAndAdds1Cycle()
    {
        // Arrange
        memory.Write(0x1000, 0x42);
        SetupCpu(pc: 0x1000, cycles: 10);

        // Act
        Addr address = AddressingModes.ZeroPage(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x0042));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001));
        Assert.That(cpu.GetCycles(), Is.EqualTo(11)); // +1 cycle for ZP fetch
    }

    /// <summary>
    /// Verifies that ZeroPageX addressing mode adds X register and wraps in zero page.
    /// </summary>
    [Test]
    public void ZeroPageX_AddsXRegisterAndWrapsInZeroPage()
    {
        // Arrange
        memory.Write(0x1000, 0xF0);
        SetupCpu(pc: 0x1000, x: 0x20, cycles: 10);

        // Act
        Addr address = AddressingModes.ZeroPageX(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x0010)); // 0xF0 + 0x20 = 0x110, wrapped to 0x10
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001));
        Assert.That(cpu.GetCycles(), Is.EqualTo(12)); // +2 cycles (fetch + index)
    }

    /// <summary>
    /// Verifies that ZeroPageY addressing mode adds Y register and wraps in zero page.
    /// </summary>
    [Test]
    public void ZeroPageY_AddsYRegisterAndWrapsInZeroPage()
    {
        // Arrange
        memory.Write(0x1000, 0x80);
        SetupCpu(pc: 0x1000, y: 0x90, cycles: 10);

        // Act
        Addr address = AddressingModes.ZeroPageY(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x0010)); // 0x80 + 0x90 = 0x110, wrapped to 0x10
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001));
        Assert.That(cpu.GetCycles(), Is.EqualTo(12)); // +2 cycles (fetch + index)
    }

    /// <summary>
    /// Verifies that Absolute addressing mode reads 16-bit address.
    /// </summary>
    [Test]
    public void Absolute_Reads16BitAddress()
    {
        // Arrange
        memory.WriteWord(0x1000, 0x5678);
        SetupCpu(pc: 0x1000, cycles: 10);

        // Act
        Addr address = AddressingModes.Absolute(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x5678));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1002));
        Assert.That(cpu.GetCycles(), Is.EqualTo(12)); // +2 cycles for 16-bit address fetch
    }

    /// <summary>
    /// Verifies that AbsoluteX adds X register without page boundary crossing.
    /// </summary>
    [Test]
    public void AbsoluteX_AddsXRegister_NoPageBoundaryCrossing()
    {
        // Arrange
        memory.WriteWord(0x1000, 0x1234);
        SetupCpu(pc: 0x1000, x: 0x10, cycles: 10);

        // Act
        Addr address = AddressingModes.AbsoluteX(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x1244)); // 0x1234 + 0x10
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1002));
        Assert.That(cpu.GetCycles(), Is.EqualTo(12)); // +2 cycles, no page boundary penalty
    }

    /// <summary>
    /// Verifies that AbsoluteX adds extra cycle when crossing page boundary.
    /// </summary>
    [Test]
    public void AbsoluteX_AddsXRegister_WithPageBoundaryCrossing()
    {
        // Arrange
        memory.WriteWord(0x1000, 0x12FF);
        SetupCpu(pc: 0x1000, x: 0x02, cycles: 10);

        // Act
        Addr address = AddressingModes.AbsoluteX(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x1301)); // 0x12FF + 0x02
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1002));
        Assert.That(cpu.GetCycles(), Is.EqualTo(13)); // +3 cycles (2 base + 1 page boundary)
    }

    /// <summary>
    /// Verifies that AbsoluteY adds Y register without page boundary crossing.
    /// </summary>
    [Test]
    public void AbsoluteY_AddsYRegister_NoPageBoundaryCrossing()
    {
        // Arrange
        memory.WriteWord(0x1000, 0x2000);
        SetupCpu(pc: 0x1000, y: 0x50, cycles: 10);

        // Act
        Addr address = AddressingModes.AbsoluteY(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x2050)); // 0x2000 + 0x50
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1002));
        Assert.That(cpu.GetCycles(), Is.EqualTo(12)); // +2 cycles, no page boundary penalty
    }

    /// <summary>
    /// Verifies that AbsoluteY adds extra cycle when crossing page boundary.
    /// </summary>
    [Test]
    public void AbsoluteY_AddsYRegister_WithPageBoundaryCrossing()
    {
        // Arrange
        memory.WriteWord(0x1000, 0x20F0);
        SetupCpu(pc: 0x1000, y: 0x20, cycles: 10);

        // Act
        Addr address = AddressingModes.AbsoluteY(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x2110)); // 0x20F0 + 0x20
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1002));
        Assert.That(cpu.GetCycles(), Is.EqualTo(13)); // +3 cycles (2 base + 1 page boundary)
    }

    /// <summary>
    /// Verifies that IndirectX uses X-indexed zero page pointer.
    /// </summary>
    [Test]
    public void IndirectX_UsesXIndexedZeroPagePointer()
    {
        // Arrange
        memory.Write(0x1000, 0x40); // ZP address
        memory.WriteWord(0x0045, 0x3000); // Pointer at ZP 0x45 (0x40 + 0x05)
        SetupCpu(pc: 0x1000, x: 0x05, cycles: 10);

        // Act
        Addr address = AddressingModes.IndirectX(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x3000));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001));
        Assert.That(cpu.GetCycles(), Is.EqualTo(14)); // +4 cycles (fetch ZP + index + read pointer)
    }

    /// <summary>
    /// Verifies that IndirectY uses zero page pointer indexed by Y without page crossing.
    /// </summary>
    [Test]
    public void IndirectY_UsesZeroPagePointerIndexedByY_NoPageCrossing()
    {
        // Arrange
        memory.Write(0x1000, 0x50); // ZP address
        memory.WriteWord(0x0050, 0x4000); // Pointer at ZP 0x50
        SetupCpu(pc: 0x1000, y: 0x10, cycles: 10);

        // Act
        Addr address = AddressingModes.IndirectY(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x4010)); // 0x4000 + 0x10
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001));
        Assert.That(cpu.GetCycles(), Is.EqualTo(13)); // +3 cycles, no page boundary penalty
    }

    /// <summary>
    /// Verifies that IndirectY adds extra cycle when crossing page boundary.
    /// </summary>
    [Test]
    public void IndirectY_UsesZeroPagePointerIndexedByY_WithPageCrossing()
    {
        // Arrange
        memory.Write(0x1000, 0x60); // ZP address
        memory.WriteWord(0x0060, 0x40FF); // Pointer at ZP 0x60
        SetupCpu(pc: 0x1000, y: 0x02, cycles: 10);

        // Act
        Addr address = AddressingModes.IndirectY(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x4101)); // 0x40FF + 0x02
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001));
        Assert.That(cpu.GetCycles(), Is.EqualTo(14)); // +4 cycles (3 base + 1 page boundary)
    }

    /// <summary>
    /// Verifies that AbsoluteXWrite always takes maximum cycles regardless of page crossing.
    /// </summary>
    [Test]
    public void AbsoluteXWrite_AlwaysTakesMaximumCycles()
    {
        // Arrange - no page crossing case
        memory.WriteWord(0x1000, 0x2000);
        SetupCpu(pc: 0x1000, x: 0x10, cycles: 10);

        // Act
        Addr address = AddressingModes.AbsoluteXWrite(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x2010));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1002));
        Assert.That(cpu.GetCycles(), Is.EqualTo(13)); // Always +3 cycles for write operations
    }

    /// <summary>
    /// Verifies that AbsoluteYWrite always takes maximum cycles regardless of page crossing.
    /// </summary>
    [Test]
    public void AbsoluteYWrite_AlwaysTakesMaximumCycles()
    {
        // Arrange - no page crossing case
        memory.WriteWord(0x1000, 0x3000);
        SetupCpu(pc: 0x1000, y: 0x20, cycles: 10);

        // Act
        Addr address = AddressingModes.AbsoluteYWrite(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x3020));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1002));
        Assert.That(cpu.GetCycles(), Is.EqualTo(13)); // Always +3 cycles for write operations
    }

    /// <summary>
    /// Verifies that IndirectYWrite always takes maximum cycles regardless of page crossing.
    /// </summary>
    [Test]
    public void IndirectYWrite_AlwaysTakesMaximumCycles()
    {
        // Arrange - no page crossing case
        memory.Write(0x1000, 0x70); // ZP address
        memory.WriteWord(0x0070, 0x5000); // Pointer at ZP 0x70
        SetupCpu(pc: 0x1000, y: 0x30, cycles: 10);

        // Act
        Addr address = AddressingModes.IndirectYWrite(cpu);

        // Assert
        Assert.That(address, Is.EqualTo(0x5030));
        Assert.That(cpu.Registers.PC.GetWord(), Is.EqualTo(0x1001));
        Assert.That(cpu.GetCycles(), Is.EqualTo(14)); // Always +4 cycles for write operations
    }

    /// <summary>
    /// Sets up CPU registers for testing.
    /// </summary>
    private void SetupCpu(
        Word pc = 0,
        byte a = 0,
        byte x = 0,
        byte y = 0,
        byte sp = 0,
        ProcessorStatusFlags p = 0,
        ulong cycles = 0)
    {
        cpu.Registers.PC.SetWord(pc);
        cpu.Registers.A.SetByte(a);
        cpu.Registers.X.SetByte(x);
        cpu.Registers.Y.SetByte(y);
        cpu.Registers.SP.SetByte(sp);
        cpu.Registers.P = p;
        cpu.SetCycles(cycles);
    }
}