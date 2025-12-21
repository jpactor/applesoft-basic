// <copyright file="Cpu65C02Tests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Tests;

using BadMango.Emulator.Core;
using BadMango.Emulator.Emulation.Cpu;
using BadMango.Emulator.Emulation.Memory;

/// <summary>
/// Unit tests for the <see cref="Cpu65C02"/> class.
/// </summary>
[TestFixture]
public class Cpu65C02Tests
{
    private IMemory memory = null!;
    private Cpu65C02 cpu = null!;

    /// <summary>
    /// Sets up the test environment by initializing memory and CPU.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        memory = new BasicMemory();
        cpu = new Cpu65C02(memory);
    }

    /// <summary>
    /// Verifies that Reset() initializes the CPU to the correct state.
    /// </summary>
    [Test]
    public void Reset_InitializesCpuCorrectly()
    {
        // Arrange: Set reset vector to 0x1000
        memory.WriteWord(0xFFFC, 0x1000);

        // Act
        cpu.Reset();

        // Assert
        var state = cpu.GetState();
        Assert.That(state.PC, Is.EqualTo(0x1000));
        Assert.That(state.S, Is.EqualTo(0xFD));
        Assert.That(state.A, Is.EqualTo(0));
        Assert.That(state.X, Is.EqualTo(0));
        Assert.That(state.Y, Is.EqualTo(0));
        Assert.That(cpu.Halted, Is.False);
    }

    /// <summary>
    /// Verifies that LDA immediate loads the accumulator correctly.
    /// </summary>
    [Test]
    public void LDA_Immediate_LoadsAccumulator()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$42
        memory.Write(0x1001, 0x42);
        cpu.Reset();

        // Act
        int cycles = cpu.Step();

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0x42));
        Assert.That(cycles, Is.EqualTo(2));
        Assert.That(state.PC, Is.EqualTo(0x1002));
    }

    /// <summary>
    /// Verifies that LDA sets the Zero flag correctly.
    /// </summary>
    [Test]
    public void LDA_SetsZeroFlag()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$00
        memory.Write(0x1001, 0x00);
        cpu.Reset();

        // Act
        cpu.Step();

        // Assert
        var state = cpu.GetState();
        Assert.That(state.P & 0x02, Is.EqualTo(0x02)); // Zero flag set
    }

    /// <summary>
    /// Verifies that LDA sets the Negative flag correctly.
    /// </summary>
    [Test]
    public void LDA_SetsNegativeFlag()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$FF
        memory.Write(0x1001, 0xFF);
        cpu.Reset();

        // Act
        cpu.Step();

        // Assert
        var state = cpu.GetState();
        Assert.That(state.P & 0x80, Is.EqualTo(0x80)); // Negative flag set
    }

    /// <summary>
    /// Verifies that STA stores the accumulator to memory.
    /// </summary>
    [Test]
    public void STA_ZeroPage_StoresAccumulator()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$42
        memory.Write(0x1001, 0x42);
        memory.Write(0x1002, 0x85); // STA $10
        memory.Write(0x1003, 0x10);
        cpu.Reset();

        // Act
        cpu.Step(); // LDA
        cpu.Step(); // STA

        // Assert
        Assert.That(memory.Read(0x10), Is.EqualTo(0x42));
    }

    /// <summary>
    /// Verifies that LDA from zero page works correctly.
    /// </summary>
    [Test]
    public void LDA_ZeroPage_LoadsFromMemory()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x10, 0x99); // Value in zero page
        memory.Write(0x1000, 0xA5); // LDA $10
        memory.Write(0x1001, 0x10);
        cpu.Reset();

        // Act
        int cycles = cpu.Step();

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0x99));
        Assert.That(cycles, Is.EqualTo(3));
    }

    /// <summary>
    /// Verifies that LDA absolute addressing works correctly.
    /// </summary>
    [Test]
    public void LDA_Absolute_LoadsFromMemory()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x2000, 0x55); // Value at absolute address
        memory.Write(0x1000, 0xAD); // LDA $2000
        memory.WriteWord(0x1001, 0x2000);
        cpu.Reset();

        // Act
        int cycles = cpu.Step();

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0x55));
        Assert.That(cycles, Is.EqualTo(4));
    }

    /// <summary>
    /// Verifies that NOP executes correctly.
    /// </summary>
    [Test]
    public void NOP_ExecutesCorrectly()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xEA); // NOP
        cpu.Reset();
        var stateBefore = cpu.GetState();

        // Act
        int cycles = cpu.Step();

        // Assert
        var stateAfter = cpu.GetState();
        Assert.That(cycles, Is.EqualTo(2));
        Assert.That(stateAfter.PC, Is.EqualTo(stateBefore.PC + 1));
        Assert.That(stateAfter.A, Is.EqualTo(stateBefore.A));
        Assert.That(stateAfter.X, Is.EqualTo(stateBefore.X));
        Assert.That(stateAfter.Y, Is.EqualTo(stateBefore.Y));
    }

    /// <summary>
    /// Verifies that GetState and SetState work correctly.
    /// </summary>
    [Test]
    public void GetState_SetState_WorkCorrectly()
    {
        // Arrange
        var originalState = new Cpu65C02State
        {
            A = 0x42,
            X = 0x10,
            Y = 0x20,
            S = 0xFF,
            P = 0x30,
            PC = 0x1234,
            Cycles = 100,
        };

        // Act
        cpu.SetState(originalState);
        var retrievedState = cpu.GetState();

        // Assert
        Assert.That(retrievedState.A, Is.EqualTo(originalState.A));
        Assert.That(retrievedState.X, Is.EqualTo(originalState.X));
        Assert.That(retrievedState.Y, Is.EqualTo(originalState.Y));
        Assert.That(retrievedState.S, Is.EqualTo(originalState.S));
        Assert.That(retrievedState.P, Is.EqualTo(originalState.P));
        Assert.That(retrievedState.PC, Is.EqualTo(originalState.PC));
        Assert.That(retrievedState.Cycles, Is.EqualTo(originalState.Cycles));
    }

    /// <summary>
    /// Verifies that BRK halts the CPU.
    /// </summary>
    [Test]
    public void BRK_HaltsCpu()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0x00); // BRK
        cpu.Reset();

        // Act
        cpu.Step();

        // Assert
        Assert.That(cpu.Halted, Is.True);
    }

    /// <summary>
    /// Verifies that cycle counting is accurate.
    /// </summary>
    [Test]
    public void CycleCounting_IsAccurate()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$42 (2 cycles)
        memory.Write(0x1001, 0x42);
        memory.Write(0x1002, 0xEA); // NOP (2 cycles)
        memory.Write(0x1003, 0xA5); // LDA $10 (3 cycles)
        memory.Write(0x1004, 0x10);
        cpu.Reset();

        // Act
        cpu.Step(); // LDA immediate
        cpu.Step(); // NOP
        cpu.Step(); // LDA zero page

        // Assert
        var state = cpu.GetState();
        Assert.That(state.Cycles, Is.EqualTo(2 + 2 + 3));
    }

    /// <summary>
    /// Verifies that GetRegisters returns only register state without cycle count.
    /// </summary>
    [Test]
    public void GetRegisters_ReturnsRegisterStateOnly()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$42
        memory.Write(0x1001, 0x42);
        cpu.Reset();
        cpu.Step();

        // Act
        var registers = cpu.GetRegisters();

        // Assert
        Assert.That(registers.A, Is.EqualTo(0x42));
        Assert.That(registers.PC, Is.EqualTo(0x1002));
        Assert.That(registers.S, Is.EqualTo(0xFD));

        // Verify the registers struct doesn't have a Cycles property
        var registersType = typeof(Cpu65C02Registers);
        Assert.That(registersType.GetProperty("Cycles"), Is.Null);
    }

    /// <summary>
    /// Verifies that LDA Zero Page,X loads correctly.
    /// </summary>
    [Test]
    public void LDA_ZeroPageX_LoadsFromMemory()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x20, 0x99); // Value at ZP $20
        memory.Write(0x1000, 0xA2); // LDX #$10
        memory.Write(0x1001, 0x10);
        memory.Write(0x1002, 0xB5); // LDA $10,X ($10 + $10 = $20)
        memory.Write(0x1003, 0x10);
        cpu.Reset();

        // Act
        cpu.Step(); // LDX #$10
        int cycles = cpu.Step(); // LDA $10,X

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0x99));
        Assert.That(cycles, Is.EqualTo(4)); // 4 cycles for ZP,X
    }

    /// <summary>
    /// Verifies that LDA Absolute,X loads correctly.
    /// </summary>
    [Test]
    public void LDA_AbsoluteX_LoadsFromMemory()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x2050, 0xAA); // Value at $2050
        memory.Write(0x1000, 0xA2); // LDX #$50
        memory.Write(0x1001, 0x50);
        memory.Write(0x1002, 0xBD); // LDA $2000,X
        memory.WriteWord(0x1003, 0x2000);
        cpu.Reset();

        // Act
        cpu.Step(); // LDX #$50
        int cycles = cpu.Step(); // LDA $2000,X

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0xAA));
        Assert.That(cycles, Is.EqualTo(4)); // 4 cycles (no page cross)
    }

    /// <summary>
    /// Verifies that LDA Absolute,X adds cycle on page boundary crossing.
    /// </summary>
    [Test]
    public void LDA_AbsoluteX_PageCrossAddsCycle()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x2100, 0xBB); // Value at $2100 (crosses page from $20FF)
        memory.Write(0x1000, 0xA2); // LDX #$01
        memory.Write(0x1001, 0x01);
        memory.Write(0x1002, 0xBD); // LDA $20FF,X
        memory.WriteWord(0x1003, 0x20FF);
        cpu.Reset();

        // Act
        cpu.Step(); // LDA #$01
        int cycles = cpu.Step(); // LDA $20FF,X

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0xBB));
        Assert.That(cycles, Is.EqualTo(5)); // 5 cycles (page cross)
    }

    /// <summary>
    /// Verifies that LDA Absolute,Y loads correctly.
    /// </summary>
    [Test]
    public void LDA_AbsoluteY_LoadsFromMemory()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x3030, 0xCC); // Value at $3030
        memory.Write(0x1000, 0xA0); // LDY #$30 (set Y)
        memory.Write(0x1001, 0x30);
        memory.Write(0x1002, 0xB9); // LDA $3000,Y
        memory.WriteWord(0x1003, 0x3000);
        cpu.Reset();

        // Act
        cpu.Step(); // LDY #$30
        int cycles = cpu.Step(); // LDA $3000,Y

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0xCC));
        Assert.That(cycles, Is.EqualTo(4)); // 4 cycles (no page cross)
    }

    /// <summary>
    /// Verifies that LDA (Indirect,X) loads correctly.
    /// </summary>
    [Test]
    public void LDA_IndirectX_LoadsFromMemory()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.WriteWord(0x24, 0x4000); // Pointer at ZP $24
        memory.Write(0x4000, 0xDD); // Value at $4000
        memory.Write(0x1000, 0xA2); // LDX #$04
        memory.Write(0x1001, 0x04);
        memory.Write(0x1002, 0xA1); // LDA ($20,X) -> ($20+$04=$24)
        memory.Write(0x1003, 0x20);
        cpu.Reset();

        // Act
        cpu.Step(); // LDX #$04
        int cycles = cpu.Step(); // LDA ($20,X)

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0xDD));
        Assert.That(cycles, Is.EqualTo(6)); // 6 cycles for (Indirect,X)
    }

    /// <summary>
    /// Verifies that LDA (Indirect),Y loads correctly.
    /// </summary>
    [Test]
    public void LDA_IndirectY_LoadsFromMemory()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.WriteWord(0x30, 0x5000); // Pointer at ZP $30
        memory.Write(0x5010, 0xEE); // Value at $5010
        memory.Write(0x1000, 0xA0); // LDY #$10 (set Y)
        memory.Write(0x1001, 0x10);
        memory.Write(0x1002, 0xB1); // LDA ($30),Y -> ($5000+$10=$5010)
        memory.Write(0x1003, 0x30);
        cpu.Reset();

        // Act
        cpu.Step(); // LDY #$10
        int cycles = cpu.Step(); // LDA ($30),Y

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0xEE));
        Assert.That(cycles, Is.EqualTo(5)); // 5 cycles (no page cross)
    }

    /// <summary>
    /// Verifies that LDA (Indirect),Y adds cycle on page boundary crossing.
    /// </summary>
    [Test]
    public void LDA_IndirectY_PageCrossAddsCycle()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.WriteWord(0x40, 0x60FF); // Pointer at ZP $40
        memory.Write(0x6100, 0xFF); // Value at $6100 (crosses page from $60FF)
        memory.Write(0x1000, 0xA0); // LDY #$01 (set Y)
        memory.Write(0x1001, 0x01);
        memory.Write(0x1002, 0xB1); // LDA ($40),Y -> ($60FF+$01=$6100)
        memory.Write(0x1003, 0x40);
        cpu.Reset();

        // Act
        cpu.Step(); // LDY #$01
        int cycles = cpu.Step(); // LDA ($40),Y

        // Assert
        var state = cpu.GetState();
        Assert.That(state.A, Is.EqualTo(0xFF));
        Assert.That(cycles, Is.EqualTo(6)); // 6 cycles (page cross)
    }

    /// <summary>
    /// Verifies that STA Zero Page,X stores correctly.
    /// </summary>
    [Test]
    public void STA_ZeroPageX_StoresAccumulator()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$77
        memory.Write(0x1001, 0x77);
        memory.Write(0x1002, 0xA2); // LDX #$05
        memory.Write(0x1003, 0x05);
        memory.Write(0x1004, 0x95); // STA $10,X ($10+$05=$15)
        memory.Write(0x1005, 0x10);
        cpu.Reset();

        // Act
        cpu.Step(); // LDA #$77
        cpu.Step(); // LDX #$05
        cpu.Step(); // STA $10,X

        // Assert
        Assert.That(memory.Read(0x15), Is.EqualTo(0x77));
    }

    /// <summary>
    /// Verifies that STA Absolute stores correctly.
    /// </summary>
    [Test]
    public void STA_Absolute_StoresAccumulator()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.Write(0x1000, 0xA9); // LDA #$88
        memory.Write(0x1001, 0x88);
        memory.Write(0x1002, 0x8D); // STA $3000
        memory.WriteWord(0x1003, 0x3000);
        cpu.Reset();

        // Act
        cpu.Step(); // LDA #$88
        cpu.Step(); // STA $3000

        // Assert
        Assert.That(memory.Read(0x3000), Is.EqualTo(0x88));
    }

    /// <summary>
    /// Verifies that STA (Indirect,X) stores correctly.
    /// </summary>
    [Test]
    public void STA_IndirectX_StoresAccumulator()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.WriteWord(0x25, 0x7000); // Pointer at ZP $25
        memory.Write(0x1000, 0xA9); // LDA #$99
        memory.Write(0x1001, 0x99);
        memory.Write(0x1002, 0xA2); // LDX #$05
        memory.Write(0x1003, 0x05);
        memory.Write(0x1004, 0x81); // STA ($20,X) -> ($20+$05=$25)
        memory.Write(0x1005, 0x20);
        cpu.Reset();

        // Act
        cpu.Step(); // LDA #$99
        cpu.Step(); // LDX #$05
        cpu.Step(); // STA ($20,X)

        // Assert
        Assert.That(memory.Read(0x7000), Is.EqualTo(0x99));
    }

    /// <summary>
    /// Verifies that STA (Indirect),Y stores correctly.
    /// </summary>
    [Test]
    public void STA_IndirectY_StoresAccumulator()
    {
        // Arrange
        memory.WriteWord(0xFFFC, 0x1000);
        memory.WriteWord(0x50, 0x8000); // Pointer at ZP $50
        memory.Write(0x1000, 0xA9); // LDA #$AB
        memory.Write(0x1001, 0xAB);
        memory.Write(0x1002, 0xA0); // LDY #$20
        memory.Write(0x1003, 0x20);
        memory.Write(0x1004, 0x91); // STA ($50),Y -> ($8000+$20=$8020)
        memory.Write(0x1005, 0x50);
        cpu.Reset();

        // Act
        cpu.Step(); // LDA #$AB
        cpu.Step(); // LDY #$20
        cpu.Step(); // STA ($50),Y

        // Assert
        Assert.That(memory.Read(0x8020), Is.EqualTo(0xAB));
    }
}