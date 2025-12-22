// <copyright file="Cpu65C02GenericBuilderIntegrationTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Tests;

using BadMango.Emulator.Core;
using BadMango.Emulator.Emulation.Cpu;
using BadMango.Emulator.Emulation.Memory;

/// <summary>
/// Integration tests demonstrating the generic builder pattern integrated into CPU opcode table construction.
/// </summary>
[TestFixture]
public class Cpu65C02GenericBuilderIntegrationTests
{
    private IMemory memory = null!;

    /// <summary>
    /// Sets up test environment.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        memory = new BasicMemory();
    }

    /// <summary>
    /// Demonstrates that the generic builder produces a working opcode table.
    /// </summary>
    [Test]
    public void GenericBuilder_ProducesWorkingOpcodeTable()
    {
        // Arrange
        var opcodeTable = Cpu65C02OpcodeTableBuilderGeneric.BuildWithGenericPattern();

        // Assert - Table should be created
        Assert.That(opcodeTable, Is.Not.Null);

        // Verify we can get handlers for opcodes we know we implemented
        var ldaImmediateHandler = opcodeTable.GetHandler(0xA9);
        Assert.That(ldaImmediateHandler, Is.Not.Null);

        var staZeroPageHandler = opcodeTable.GetHandler(0x85);
        Assert.That(staZeroPageHandler, Is.Not.Null);
    }

    /// <summary>
    /// Demonstrates that opcodes built with generic pattern execute correctly.
    /// </summary>
    [Test]
    public void GenericBuilder_Opcodes_ExecuteCorrectly()
    {
        // Arrange
        var opcodeTable = Cpu65C02OpcodeTableBuilderGeneric.BuildWithGenericPattern();
        var cpu = new Cpu65C02(memory);
        memory.Write(0x1000, 0x42); // Value to load
        var state = new Cpu65C02State { PC = 0x1000, A = 0x00, P = 0x00, Cycles = 0 };

        // Act - Execute LDA Immediate (opcode 0xA9)
        var handler = opcodeTable.GetHandler(0xA9);
        handler(cpu, memory, ref state);

        // Assert
        Assert.That(state.A, Is.EqualTo(0x42), "LDA should load the value");
        Assert.That(state.PC, Is.EqualTo(0x1001), "PC should be incremented");
    }

    /// <summary>
    /// Verifies LDA instruction works across multiple addressing modes.
    /// </summary>
    [Test]
    public void GenericBuilder_LDA_WorksAcrossAddressingModes()
    {
        // Arrange
        var opcodeTable = Cpu65C02OpcodeTableBuilderGeneric.BuildWithGenericPattern();
        var cpu = new Cpu65C02(memory);

        // Test LDA Zero Page (0xA5)
        memory.Write(0x1000, 0x50); // ZP address
        memory.Write(0x0050, 0x99); // Value at ZP
        var state = new Cpu65C02State { PC = 0x1000, A = 0x00, P = 0x00, Cycles = 0 };

        // Act
        var handler = opcodeTable.GetHandler(0xA5);
        handler(cpu, memory, ref state);

        // Assert
        Assert.That(state.A, Is.EqualTo(0x99), "LDA ZP should load from zero page");
    }

    /// <summary>
    /// Verifies STA instruction stores values correctly.
    /// </summary>
    [Test]
    public void GenericBuilder_STA_StoresValuesCorrectly()
    {
        // Arrange
        var opcodeTable = Cpu65C02OpcodeTableBuilderGeneric.BuildWithGenericPattern();
        var cpu = new Cpu65C02(memory);
        memory.Write(0x1000, 0x50); // ZP address
        var state = new Cpu65C02State { PC = 0x1000, A = 0x42, P = 0x00, Cycles = 0 };

        // Act - Execute STA Zero Page (0x85)
        var handler = opcodeTable.GetHandler(0x85);
        handler(cpu, memory, ref state);

        // Assert
        Assert.That(memory.Read(0x0050), Is.EqualTo(0x42), "STA should store accumulator value");
    }

    /// <summary>
    /// Demonstrates the clean syntax of the builder pattern.
    /// </summary>
    [Test]
    public void GenericBuilder_DemonstratesCleanSyntax()
    {
        // This test exists primarily for documentation purposes
        // It shows how the builder pattern provides clean, readable code

        // The builder encapsulates all the verbose type parameters:
        var builder = OpcodeTableBuilders.ForCpu65C02();

        // Usage is clean and intuitive:
        var handler1 = builder.Instructions.LDA(builder.AddressingModes.Immediate);
        var handler2 = builder.Instructions.STA(builder.AddressingModes.ZeroPage);

        // Compare to the verbose alternative:
        // InstructionsFor<Cpu65C02, Cpu65C02Registers, byte, byte, byte, Word, Cpu65C02State>
        //     .LDA(AddressingModesFor<Cpu65C02Registers, byte, byte, byte, Word>.Immediate);

        Assert.That(handler1, Is.Not.Null);
        Assert.That(handler2, Is.Not.Null);
    }

    /// <summary>
    /// Verifies that multiple instructions work together.
    /// </summary>
    [Test]
    public void GenericBuilder_MultipleInstructions_WorkTogether()
    {
        // Arrange
        var opcodeTable = Cpu65C02OpcodeTableBuilderGeneric.BuildWithGenericPattern();
        var cpu = new Cpu65C02(memory);

        // Set up a simple program: LDA #$42, STA $50
        memory.Write(0x1000, 0x42); // Value for LDA immediate
        memory.Write(0x1001, 0x50); // ZP address for STA
        var state = new Cpu65C02State { PC = 0x1000, A = 0x00, P = 0x00, Cycles = 0 };

        // Act - Execute LDA #$42 (opcode 0xA9)
        var ldaHandler = opcodeTable.GetHandler(0xA9);
        ldaHandler(cpu, memory, ref state);

        // Then execute STA $50 (opcode 0x85)
        var staHandler = opcodeTable.GetHandler(0x85);
        staHandler(cpu, memory, ref state);

        // Assert
        Assert.That(state.A, Is.EqualTo(0x42), "Accumulator should contain loaded value");
        Assert.That(memory.Read(0x0050), Is.EqualTo(0x42), "Memory should contain stored value");
    }
}
