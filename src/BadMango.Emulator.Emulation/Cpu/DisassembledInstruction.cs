// <copyright file="DisassembledInstruction.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Emulation.Cpu;

using BadMango.Emulator.Core;

/// <summary>
/// Represents a decoded instruction for disassembly and debugging purposes.
/// </summary>
/// <remarks>
/// <para>
/// This structure contains all information needed for a debug console to format
/// disassembly output with richer information. It is designed to be extensible
/// through the <see cref="Metadata"/> dictionary for additional context such as
/// expected register values, memory effects, and comments.
/// </para>
/// <para>
/// Example formatted output:
/// <code>
/// $1000: A9 42    LDA #$42       ; A=$42
/// $1002: 8D 00 02 STA $0200      ; [$0200]=$42
/// </code>
/// </para>
/// </remarks>
public sealed class DisassembledInstruction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisassembledInstruction"/> class.
    /// </summary>
    /// <param name="address">The address where the instruction starts.</param>
    /// <param name="opcode">The opcode byte.</param>
    /// <param name="operandBytes">The operand bytes (may be empty).</param>
    /// <param name="instruction">The instruction mnemonic.</param>
    /// <param name="addressingMode">The addressing mode.</param>
    public DisassembledInstruction(
        uint address,
        byte opcode,
        byte[] operandBytes,
        CpuInstructions instruction,
        CpuAddressingModes addressingMode)
    {
        Address = address;
        Opcode = opcode;
        OperandBytes = operandBytes ?? [];
        Instruction = instruction;
        AddressingMode = addressingMode;
        Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets the address where the instruction starts.
    /// </summary>
    public uint Address { get; }

    /// <summary>
    /// Gets the opcode byte.
    /// </summary>
    public byte Opcode { get; }

    /// <summary>
    /// Gets the operand bytes (may be empty for implied addressing).
    /// </summary>
    public byte[] OperandBytes { get; }

    /// <summary>
    /// Gets the instruction mnemonic.
    /// </summary>
    public CpuInstructions Instruction { get; }

    /// <summary>
    /// Gets the addressing mode.
    /// </summary>
    public CpuAddressingModes AddressingMode { get; }

    /// <summary>
    /// Gets a dictionary for extensible metadata such as register effects, memory effects, and comments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This dictionary allows the debug console to store and retrieve additional context
    /// about the instruction without coupling to a fixed structure. Common keys include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>"Comment" - A string comment for the instruction.</description></item>
    /// <item><description>"EffectiveAddress" - The computed effective address for memory operations.</description></item>
    /// <item><description>"RegisterA" - The value of the A register before/after execution.</description></item>
    /// <item><description>"RegisterX" - The value of the X register before/after execution.</description></item>
    /// <item><description>"RegisterY" - The value of the Y register before/after execution.</description></item>
    /// <item><description>"MemoryValue" - The value at the effective address.</description></item>
    /// </list>
    /// </remarks>
    public Dictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets the total length of the instruction including opcode and operands.
    /// </summary>
    public int TotalLength => 1 + OperandBytes.Length;

    /// <summary>
    /// Gets all bytes of the instruction (opcode followed by operands).
    /// </summary>
    /// <returns>An array containing the opcode byte followed by any operand bytes.</returns>
    public byte[] GetAllBytes()
    {
        var bytes = new byte[TotalLength];
        bytes[0] = Opcode;
        Array.Copy(OperandBytes, 0, bytes, 1, OperandBytes.Length);
        return bytes;
    }

    /// <summary>
    /// Formats the operand according to the addressing mode.
    /// </summary>
    /// <returns>A string representation of the operand in assembly syntax.</returns>
    public string FormatOperand()
    {
        return AddressingMode switch
        {
            CpuAddressingModes.Implied => string.Empty,
            CpuAddressingModes.Accumulator => "A",
            CpuAddressingModes.Immediate => $"#${GetOperandValue():X2}",
            CpuAddressingModes.ZeroPage => $"${GetOperandValue():X2}",
            CpuAddressingModes.ZeroPageX => $"${GetOperandValue():X2},X",
            CpuAddressingModes.ZeroPageY => $"${GetOperandValue():X2},Y",
            CpuAddressingModes.Absolute => $"${GetOperandValue():X4}",
            CpuAddressingModes.AbsoluteX => $"${GetOperandValue():X4},X",
            CpuAddressingModes.AbsoluteY => $"${GetOperandValue():X4},Y",
            CpuAddressingModes.Indirect => $"(${GetOperandValue():X4})",
            CpuAddressingModes.IndirectX => $"(${GetOperandValue():X2},X)",
            CpuAddressingModes.IndirectY => $"(${GetOperandValue():X2}),Y",
            CpuAddressingModes.Relative => FormatRelativeAddress(),
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Formats the instruction as a complete assembly line.
    /// </summary>
    /// <returns>A string in the format "LDA #$42" or "STA $0200".</returns>
    public string FormatInstruction()
    {
        var operand = FormatOperand();
        return string.IsNullOrEmpty(operand)
            ? Instruction.ToString()
            : $"{Instruction} {operand}";
    }

    /// <summary>
    /// Formats the opcode and operand bytes as hex.
    /// </summary>
    /// <returns>A string like "A9 42" or "8D 00 02".</returns>
    public string FormatBytes()
    {
        var allBytes = GetAllBytes();
        return string.Join(" ", allBytes.Select(b => b.ToString("X2")));
    }

    /// <summary>
    /// Gets the operand value as a 16-bit word (little-endian).
    /// </summary>
    /// <returns>The operand value, or 0 if no operands.</returns>
    private ushort GetOperandValue()
    {
        return OperandBytes.Length switch
        {
            0 => 0,
            1 => OperandBytes[0],
            _ => (ushort)(OperandBytes[0] | (OperandBytes[1] << 8)),
        };
    }

    /// <summary>
    /// Formats a relative branch target address.
    /// </summary>
    /// <returns>The branch target address in hex.</returns>
    private string FormatRelativeAddress()
    {
        if (OperandBytes.Length == 0)
        {
            return "$????";
        }

        // Calculate target address: PC + 2 (instruction length) + signed offset
        var offset = (sbyte)OperandBytes[0];
        var targetAddress = (uint)(Address + 2 + offset);
        return $"${targetAddress:X4}";
    }
}