// <copyright file="Instructions.Transfer.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
namespace BadMango.Emulator.Emulation.Cpu;

using System.Runtime.CompilerServices;

using Core.Cpu;
using Core.Interfaces.Cpu;

/// <summary>
/// Register transfer instructions (TAX, TAY, TXA, TYA, TXS, TSX).
/// </summary>
public static partial class Instructions
{
    /// <summary>
    /// TAX - Transfer Accumulator to X instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes TAX.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler TAX(AddressingModeHandler<CpuState> addressingMode)
    {
        return cpu =>
        {
            byte opCycles = 0;
            addressingMode(cpu);
            byte size = cpu.Registers.GetIndexSize();
            var value = cpu.Registers.A.GetValue(size);
            cpu.Registers.X.SetValue(value, size);
            cpu.Registers.P.SetZeroAndNegative(value, size);
            opCycles++;

            if (cpu.IsDebuggerAttached)
            {
                cpu.Trace = cpu.Trace with { Instruction = CpuInstructions.TAX };
                cpu.Trace = cpu.Trace with { InstructionCycles = cpu.Trace.InstructionCycles + opCycles };
            }

            cpu.Registers.TCU += opCycles;
        };
    }

    /// <summary>
    /// TAY - Transfer Accumulator to Y instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes TAY.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler TAY(AddressingModeHandler<CpuState> addressingMode)
    {
        return cpu =>
        {
            byte opCycles = 0;
            addressingMode(cpu);
            byte size = cpu.Registers.GetIndexSize();
            var value = cpu.Registers.A.GetValue(size);
            cpu.Registers.Y.SetValue(value, size);
            cpu.Registers.P.SetZeroAndNegative(value, size);
            opCycles++;

            if (cpu.IsDebuggerAttached)
            {
                cpu.Trace = cpu.Trace with { Instruction = CpuInstructions.TAY };
                cpu.Trace = cpu.Trace with { InstructionCycles = cpu.Trace.InstructionCycles + opCycles };
            }

            cpu.Registers.TCU += opCycles;
        };
    }

    /// <summary>
    /// TXA - Transfer X to Accumulator instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes TXA.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler TXA(AddressingModeHandler<CpuState> addressingMode)
    {
        return cpu =>
        {
            byte opCycles = 0;
            addressingMode(cpu);
            byte size = cpu.Registers.GetAccumulatorSize();
            var value = cpu.Registers.X.GetValue(size);
            cpu.Registers.A.SetValue(value, size);
            cpu.Registers.P.SetZeroAndNegative(value, size);
            opCycles++;

            if (cpu.IsDebuggerAttached)
            {
                cpu.Trace = cpu.Trace with { Instruction = CpuInstructions.TXA };
                cpu.Trace = cpu.Trace with { InstructionCycles = cpu.Trace.InstructionCycles + opCycles };
            }

            cpu.Registers.TCU += opCycles;
        };
    }

    /// <summary>
    /// TYA - Transfer Y to Accumulator instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes TYA.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler TYA(AddressingModeHandler<CpuState> addressingMode)
    {
        return cpu =>
        {
            byte opCycles = 0;
            addressingMode(cpu);
            byte size = cpu.Registers.GetAccumulatorSize();
            var value = cpu.Registers.Y.GetValue(size);
            cpu.Registers.A.SetValue(value, size);
            cpu.Registers.P.SetZeroAndNegative(value, size);
            opCycles++;

            if (cpu.IsDebuggerAttached)
            {
                cpu.Trace = cpu.Trace with { Instruction = CpuInstructions.TYA };
                cpu.Trace = cpu.Trace with { InstructionCycles = cpu.Trace.InstructionCycles + opCycles };
            }

            cpu.Registers.TCU += opCycles;
        };
    }

    /// <summary>
    /// TXS - Transfer X to Stack Pointer instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes TXS.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler TXS(AddressingModeHandler<CpuState> addressingMode)
    {
        return cpu =>
        {
            byte opCycles = 0;
            addressingMode(cpu);
            cpu.Registers.SP.SetByte(cpu.Registers.X.GetByte());
            opCycles++;

            if (cpu.IsDebuggerAttached)
            {
                cpu.Trace = cpu.Trace with { Instruction = CpuInstructions.TXS };
                cpu.Trace = cpu.Trace with { InstructionCycles = cpu.Trace.InstructionCycles + opCycles };
            }

            cpu.Registers.TCU += opCycles;
        };
    }

    /// <summary>
    /// TSX - Transfer Stack Pointer to X instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes TSX.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler TSX(AddressingModeHandler<CpuState> addressingMode)
    {
        return cpu =>
        {
            byte opCycles = 0;
            addressingMode(cpu);
            byte size = cpu.Registers.GetIndexSize();
            byte value = cpu.Registers.SP.GetByte();
            cpu.Registers.X.SetByte(value);
            cpu.Registers.P.SetZeroAndNegative(value, size);
            opCycles++;

            if (cpu.IsDebuggerAttached)
            {
                cpu.Trace = cpu.Trace with { Instruction = CpuInstructions.TSX };
                cpu.Trace = cpu.Trace with { InstructionCycles = cpu.Trace.InstructionCycles + opCycles };
            }

            cpu.Registers.TCU += opCycles;
        };
    }
}