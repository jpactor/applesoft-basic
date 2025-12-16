namespace ApplesoftBasic.Interpreter.Emulation;

/// <summary>
/// Represents the 6502/65816 CPU registers
/// </summary>
public class CpuRegisters
{
    /// <summary>Accumulator</summary>
    public byte A { get; set; }
    
    /// <summary>X Index Register</summary>
    public byte X { get; set; }
    
    /// <summary>Y Index Register</summary>
    public byte Y { get; set; }
    
    /// <summary>Stack Pointer</summary>
    public byte SP { get; set; } = 0xFF;
    
    /// <summary>Program Counter</summary>
    public ushort PC { get; set; }
    
    /// <summary>Processor Status Register</summary>
    public byte P { get; set; } = 0x20; // Bit 5 always set

    // Status flag bit positions
    private const byte CarryFlag = 0x01;
    private const byte ZeroFlag = 0x02;
    private const byte InterruptDisable = 0x04;
    private const byte DecimalMode = 0x08;
    private const byte BreakCommand = 0x10;
    private const byte OverflowFlag = 0x40;
    private const byte NegativeFlag = 0x80;

    public bool Carry
    {
        get => (P & CarryFlag) != 0;
        set => P = value ? (byte)(P | CarryFlag) : (byte)(P & ~CarryFlag);
    }

    public bool Zero
    {
        get => (P & ZeroFlag) != 0;
        set => P = value ? (byte)(P | ZeroFlag) : (byte)(P & ~ZeroFlag);
    }

    public bool InterruptDisabled
    {
        get => (P & InterruptDisable) != 0;
        set => P = value ? (byte)(P | InterruptDisable) : (byte)(P & ~InterruptDisable);
    }

    public bool Decimal
    {
        get => (P & DecimalMode) != 0;
        set => P = value ? (byte)(P | DecimalMode) : (byte)(P & ~DecimalMode);
    }

    public bool Break
    {
        get => (P & BreakCommand) != 0;
        set => P = value ? (byte)(P | BreakCommand) : (byte)(P & ~BreakCommand);
    }

    public bool Overflow
    {
        get => (P & OverflowFlag) != 0;
        set => P = value ? (byte)(P | OverflowFlag) : (byte)(P & ~OverflowFlag);
    }

    public bool Negative
    {
        get => (P & NegativeFlag) != 0;
        set => P = value ? (byte)(P | NegativeFlag) : (byte)(P & ~NegativeFlag);
    }

    public void Reset()
    {
        A = 0;
        X = 0;
        Y = 0;
        SP = 0xFF;
        PC = 0;
        P = 0x20;
    }

    public void SetNZ(byte value)
    {
        Zero = value == 0;
        Negative = (value & 0x80) != 0;
    }
}

/// <summary>
/// Extended registers for 65816 mode
/// </summary>
public class Cpu65816Registers : CpuRegisters
{
    /// <summary>16-bit Accumulator (65816 mode)</summary>
    public ushort C { get; set; }
    
    /// <summary>Direct Page Register</summary>
    public ushort D { get; set; }
    
    /// <summary>Data Bank Register</summary>
    public byte DBR { get; set; }
    
    /// <summary>Program Bank Register</summary>
    public byte PBR { get; set; }
    
    /// <summary>Emulation mode flag</summary>
    public bool EmulationMode { get; set; } = true;

    public new void Reset()
    {
        base.Reset();
        C = 0;
        D = 0;
        DBR = 0;
        PBR = 0;
        EmulationMode = true;
    }
}
