// <copyright file="Cpu65C02OpcodeTableBuilderGeneric.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Emulation.Cpu;

using Core;

/// <summary>
/// Builds the opcode table for the 65C02 CPU using the new generic builder pattern.
/// </summary>
/// <remarks>
/// <para>
/// This builder demonstrates using the GenericOpcodeTableBuilder to construct
/// an opcode table with clean, readable code. It serves as an example of the
/// builder pattern in action.
/// </para>
/// <para>
/// Note: This currently only implements the subset of instructions available in
/// InstructionsFor. For a complete opcode table, use Cpu65C02OpcodeTableBuilder.Build().
/// </para>
/// </remarks>
public static class Cpu65C02OpcodeTableBuilderGeneric
{
    /// <summary>
    /// Builds a partial opcode table for the 65C02 CPU using the generic builder pattern.
    /// </summary>
    /// <returns>An <see cref="OpcodeTable{TCpu, TState}"/> configured for the 65C02 CPU.</returns>
    /// <remarks>
    /// This method demonstrates the clean API provided by the GenericOpcodeTableBuilder.
    /// Compare this implementation with Cpu65C02OpcodeTableBuilder.Build() to see the
    /// difference between the old pattern and the new builder pattern.
    /// </remarks>
    public static OpcodeTable<Cpu65C02, Cpu65C02State> BuildWithGenericPattern()
    {
        var handlers = new OpcodeHandler<Cpu65C02, Cpu65C02State>[256];

        // Create the generic builder - this encapsulates all the verbose type parameters
        var builder = OpcodeTableBuilders.ForCpu65C02();

        // Initialize all opcodes to illegal opcode handler
        for (int i = 0; i < 256; i++)
        {
            handlers[i] = IllegalOpcode;
        }

        // LDA - Load Accumulator
        // Compare this clean syntax:
        handlers[0xA9] = builder.Instructions.LDA(builder.AddressingModes.Immediate);
        handlers[0xA5] = builder.Instructions.LDA(builder.AddressingModes.ZeroPage);
        handlers[0xB5] = builder.Instructions.LDA(builder.AddressingModes.ZeroPageX);
        handlers[0xAD] = builder.Instructions.LDA(builder.AddressingModes.Absolute);
        handlers[0xBD] = builder.Instructions.LDA(builder.AddressingModes.AbsoluteX);
        handlers[0xB9] = builder.Instructions.LDA(builder.AddressingModes.AbsoluteY);
        handlers[0xA1] = builder.Instructions.LDA(builder.AddressingModes.IndirectX);
        handlers[0xB1] = builder.Instructions.LDA(builder.AddressingModes.IndirectY);

        // To the old syntax:
        // handlers[0xA9] = Instructions.LDA(AddressingModes.Immediate);
        // The builder pattern is just as clean but provides full generic support!

        // STA - Store Accumulator
        handlers[0x85] = builder.Instructions.STA(builder.AddressingModes.ZeroPage);
        handlers[0x95] = builder.Instructions.STA(builder.AddressingModes.ZeroPageX);
        handlers[0x8D] = builder.Instructions.STA(builder.AddressingModes.Absolute);
        handlers[0x9D] = builder.Instructions.STA(builder.AddressingModes.AbsoluteXWrite);
        handlers[0x99] = builder.Instructions.STA(builder.AddressingModes.AbsoluteYWrite);
        handlers[0x81] = builder.Instructions.STA(builder.AddressingModes.IndirectX);
        handlers[0x91] = builder.Instructions.STA(builder.AddressingModes.IndirectYWrite);

        // LDX - Load X Register
        handlers[0xA2] = builder.Instructions.LDX(builder.AddressingModes.Immediate);
        handlers[0xA6] = builder.Instructions.LDX(builder.AddressingModes.ZeroPage);
        handlers[0xB6] = builder.Instructions.LDX(builder.AddressingModes.ZeroPageY);
        handlers[0xAE] = builder.Instructions.LDX(builder.AddressingModes.Absolute);
        handlers[0xBE] = builder.Instructions.LDX(builder.AddressingModes.AbsoluteY);

        // LDY - Load Y Register
        handlers[0xA0] = builder.Instructions.LDY(builder.AddressingModes.Immediate);
        handlers[0xA4] = builder.Instructions.LDY(builder.AddressingModes.ZeroPage);
        handlers[0xB4] = builder.Instructions.LDY(builder.AddressingModes.ZeroPageX);
        handlers[0xAC] = builder.Instructions.LDY(builder.AddressingModes.Absolute);
        handlers[0xBC] = builder.Instructions.LDY(builder.AddressingModes.AbsoluteX);

        // STX - Store X Register
        handlers[0x86] = builder.Instructions.STX(builder.AddressingModes.ZeroPage);
        handlers[0x96] = builder.Instructions.STX(builder.AddressingModes.ZeroPageY);
        handlers[0x8E] = builder.Instructions.STX(builder.AddressingModes.Absolute);

        // STY - Store Y Register
        handlers[0x84] = builder.Instructions.STY(builder.AddressingModes.ZeroPage);
        handlers[0x94] = builder.Instructions.STY(builder.AddressingModes.ZeroPageX);
        handlers[0x8C] = builder.Instructions.STY(builder.AddressingModes.Absolute);

        // NOP - No Operation
        handlers[0xEA] = builder.Instructions.NOP(builder.AddressingModes.Implied);

        // Note: Additional instructions (BRK, flag operations, jumps, arithmetic, etc.)
        // would be added here following the same pattern once they're implemented in InstructionsFor.
        // For now, this demonstrates the builder pattern with the instructions we have.

        return new OpcodeTable<Cpu65C02, Cpu65C02State>(handlers);
    }

    /// <summary>
    /// Handles illegal/undefined opcodes by halting execution.
    /// </summary>
    private static void IllegalOpcode(Cpu65C02 cpu, IMemory memory, ref Cpu65C02State state)
    {
        state.HaltReason = HaltState.Stp; // Halt on illegal opcode (stop execution)
    }
}
