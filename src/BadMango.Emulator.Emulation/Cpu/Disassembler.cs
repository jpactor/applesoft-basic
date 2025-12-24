// <copyright file="Disassembler.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Emulation.Cpu;

using BadMango.Emulator.Core;

/// <summary>
/// Disassembles machine code from memory into a list of structured instruction representations.
/// </summary>
/// <remarks>
/// <para>
/// This class provides methods to walk through memory regions and decode machine code
/// into <see cref="DisassembledInstruction"/> objects. It uses the <see cref="OpcodeTableAnalyzer"/>
/// to dynamically extract opcode information from the CPU's opcode table, ensuring the
/// disassembler stays in sync with the actual instruction implementations.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var opcodeTable = Cpu65C02OpcodeTableBuilder.Build();
/// var disassembler = new Disassembler(opcodeTable, memory);
/// var instructions = disassembler.Disassemble(0x1000, 16);
/// foreach (var instr in instructions)
/// {
///     Console.WriteLine($"${instr.Address:X4}: {instr.FormatBytes(),-12} {instr.FormatInstruction()}");
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class Disassembler
{
    private readonly IMemory memory;
    private readonly OpcodeInfo[] opcodeInfoTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="Disassembler"/> class.
    /// </summary>
    /// <param name="opcodeTable">The opcode table to use for disassembly.</param>
    /// <param name="memory">The memory interface to read from.</param>
    /// <exception cref="ArgumentNullException">Thrown when opcodeTable or memory is null.</exception>
    public Disassembler(OpcodeTable opcodeTable, IMemory memory)
    {
        ArgumentNullException.ThrowIfNull(opcodeTable);
        ArgumentNullException.ThrowIfNull(memory);

        this.memory = memory;
        this.opcodeInfoTable = OpcodeTableAnalyzer.BuildOpcodeInfoArray(opcodeTable);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Disassembler"/> class with a pre-built opcode info array.
    /// </summary>
    /// <param name="opcodeInfoArray">The pre-built opcode info array (256 elements).</param>
    /// <param name="memory">The memory interface to read from.</param>
    /// <exception cref="ArgumentNullException">Thrown when opcodeInfoArray or memory is null.</exception>
    /// <exception cref="ArgumentException">Thrown when opcodeInfoArray is not exactly 256 elements.</exception>
    public Disassembler(OpcodeInfo[] opcodeInfoArray, IMemory memory)
    {
        ArgumentNullException.ThrowIfNull(opcodeInfoArray);
        ArgumentNullException.ThrowIfNull(memory);

        if (opcodeInfoArray.Length != 256)
        {
            throw new ArgumentException("Opcode info array must have exactly 256 entries.", nameof(opcodeInfoArray));
        }

        this.memory = memory;
        this.opcodeInfoTable = opcodeInfoArray;
    }

    /// <summary>
    /// Disassembles a range of memory starting at the specified address.
    /// </summary>
    /// <param name="startAddress">The starting address to disassemble from.</param>
    /// <param name="byteCount">The maximum number of bytes to disassemble.</param>
    /// <returns>A list of <see cref="DisassembledInstruction"/> objects representing the decoded instructions.</returns>
    /// <remarks>
    /// The disassembler will stop when it has processed at least <paramref name="byteCount"/> bytes
    /// or when it encounters the end of a complete instruction that would exceed the byte count.
    /// Instructions are never truncated.
    /// </remarks>
    public IReadOnlyList<DisassembledInstruction> Disassemble(uint startAddress, int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);

        var instructions = new List<DisassembledInstruction>();
        uint currentAddress = startAddress;
        uint endAddress = startAddress + (uint)byteCount;

        while (currentAddress < endAddress)
        {
            var instruction = DisassembleInstruction(currentAddress);
            instructions.Add(instruction);
            currentAddress += (uint)instruction.TotalLength;
        }

        return instructions;
    }

    /// <summary>
    /// Disassembles a specific number of instructions starting at the specified address.
    /// </summary>
    /// <param name="startAddress">The starting address to disassemble from.</param>
    /// <param name="instructionCount">The number of instructions to disassemble.</param>
    /// <returns>A list of <see cref="DisassembledInstruction"/> objects representing the decoded instructions.</returns>
    public IReadOnlyList<DisassembledInstruction> DisassembleInstructions(uint startAddress, int instructionCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(instructionCount);

        var instructions = new List<DisassembledInstruction>(instructionCount);
        uint currentAddress = startAddress;

        for (int i = 0; i < instructionCount; i++)
        {
            var instruction = DisassembleInstruction(currentAddress);
            instructions.Add(instruction);
            currentAddress += (uint)instruction.TotalLength;
        }

        return instructions;
    }

    /// <summary>
    /// Disassembles a single instruction at the specified address.
    /// </summary>
    /// <param name="address">The address of the instruction to disassemble.</param>
    /// <returns>A <see cref="DisassembledInstruction"/> representing the decoded instruction.</returns>
    public DisassembledInstruction DisassembleInstruction(uint address)
    {
        byte opcode = memory.Read(address);
        var opcodeInfo = opcodeInfoTable[opcode];

        // Read operand bytes based on the operand length
        var operandBytes = new byte[opcodeInfo.OperandLength];
        for (int i = 0; i < opcodeInfo.OperandLength; i++)
        {
            operandBytes[i] = memory.Read(address + 1 + (uint)i);
        }

        return new DisassembledInstruction(
            address,
            opcode,
            operandBytes,
            opcodeInfo.Instruction,
            opcodeInfo.AddressingMode);
    }

    /// <summary>
    /// Disassembles memory within a specified address range.
    /// </summary>
    /// <param name="startAddress">The starting address (inclusive).</param>
    /// <param name="endAddress">The ending address (exclusive).</param>
    /// <returns>A list of <see cref="DisassembledInstruction"/> objects representing the decoded instructions.</returns>
    /// <exception cref="ArgumentException">Thrown when endAddress is less than startAddress.</exception>
    public IReadOnlyList<DisassembledInstruction> DisassembleRange(uint startAddress, uint endAddress)
    {
        if (endAddress < startAddress)
        {
            throw new ArgumentException("End address must be greater than or equal to start address.", nameof(endAddress));
        }

        return Disassemble(startAddress, (int)(endAddress - startAddress));
    }
}