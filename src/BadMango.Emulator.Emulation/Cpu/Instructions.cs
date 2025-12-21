// <copyright file="Instructions.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Emulation.Cpu;

using System.Runtime.CompilerServices;

using BadMango.Emulator.Core;

/// <summary>
/// Provides instruction implementations that compose with addressing modes.
/// </summary>
/// <remarks>
/// Instructions are higher-order functions that take addressing mode delegates
/// and return opcode handlers. This enables true composition and eliminates
/// the need for separate methods for each instruction/addressing-mode combination.
/// </remarks>
public static class Instructions
{
    private const byte FlagZ = 0x02;
    private const byte FlagN = 0x80;
    private const byte FlagB = 0x10;
    private const byte FlagI = 0x04;
    private const ushort StackBase = 0x0100;

    /// <summary>
    /// LDA - Load Accumulator instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use.</param>
    /// <returns>An opcode handler that executes LDA with the given addressing mode.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler<Cpu65C02, Cpu65C02State> LDA(AddressingMode<Cpu65C02State> addressingMode)
    {
        return (cpu, memory, ref state) =>
        {
            ushort address = addressingMode(memory, ref state);
            byte value = memory.Read(address);
            state.Cycles++; // Memory read cycle
            
            byte a = value;
            byte p = state.P;
            SetZN(value, ref p);
            
            state.A = a;
            state.P = p;
        };
    }

    /// <summary>
    /// LDX - Load X Register instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use.</param>
    /// <returns>An opcode handler that executes LDX with the given addressing mode.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler<Cpu65C02, Cpu65C02State> LDX(AddressingMode<Cpu65C02State> addressingMode)
    {
        return (cpu, memory, ref state) =>
        {
            ushort address = addressingMode(memory, ref state);
            byte value = memory.Read(address);
            state.Cycles++; // Memory read cycle
            
            byte x = value;
            byte p = state.P;
            SetZN(value, ref p);
            
            state.X = x;
            state.P = p;
        };
    }

    /// <summary>
    /// LDY - Load Y Register instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use.</param>
    /// <returns>An opcode handler that executes LDY with the given addressing mode.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler<Cpu65C02, Cpu65C02State> LDY(AddressingMode<Cpu65C02State> addressingMode)
    {
        return (cpu, memory, ref state) =>
        {
            ushort address = addressingMode(memory, ref state);
            byte value = memory.Read(address);
            state.Cycles++; // Memory read cycle
            
            byte y = value;
            byte p = state.P;
            SetZN(value, ref p);
            
            state.Y = y;
            state.P = p;
        };
    }

    /// <summary>
    /// STA - Store Accumulator instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use.</param>
    /// <returns>An opcode handler that executes STA with the given addressing mode.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler<Cpu65C02, Cpu65C02State> STA(AddressingMode<Cpu65C02State> addressingMode)
    {
        return (cpu, memory, ref state) =>
        {
            ushort address = addressingMode(memory, ref state);
            memory.Write(address, state.A);
            state.Cycles++; // Memory write cycle
        };
    }

    /// <summary>
    /// NOP - No Operation instruction.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes NOP.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler<Cpu65C02, Cpu65C02State> NOP(AddressingMode<Cpu65C02State> addressingMode)
    {
        return (cpu, memory, ref state) =>
        {
            addressingMode(memory, ref state); // Call addressing mode (usually does nothing for Implied)
            state.Cycles++; // NOP takes 2 cycles total (1 from fetch + 1 here)
        };
    }

    /// <summary>
    /// BRK - Force Break instruction. Causes a software interrupt.
    /// </summary>
    /// <param name="addressingMode">The addressing mode function to use (typically Implied).</param>
    /// <returns>An opcode handler that executes BRK.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OpcodeHandler<Cpu65C02, Cpu65C02State> BRK(AddressingMode<Cpu65C02State> addressingMode)
    {
        return (cpu, memory, ref state) =>
        {
            addressingMode(memory, ref state); // Call addressing mode (usually does nothing for Implied)
            
            // BRK causes a software interrupt
            // Total 7 cycles: 1 (opcode fetch) + 1 (PC increment) + 2 (push PC) + 1 (push P) + 2 (read IRQ vector)
            ushort pc = state.PC;
            byte s = state.S;
            byte p = state.P;

            pc++;
            memory.Write((ushort)(StackBase + s--), (byte)(pc >> 8));
            memory.Write((ushort)(StackBase + s--), (byte)(pc & 0xFF));
            memory.Write((ushort)(StackBase + s--), (byte)(p | FlagB));
            p |= FlagI;
            pc = memory.ReadWord(0xFFFE);
            state.Cycles += 6; // 6 cycles in handler + 1 from opcode fetch in Step()

            state.PC = pc;
            state.S = s;
            state.P = p;
            state.Halted = true; // Halt on BRK
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetZN(byte value, ref byte p)
    {
        if (value == 0)
        {
            p |= FlagZ;
        }
        else
        {
            p &= unchecked((byte)~FlagZ);
        }

        if ((value & 0x80) != 0)
        {
            p |= FlagN;
        }
        else
        {
            p &= unchecked((byte)~FlagN);
        }
    }
}

