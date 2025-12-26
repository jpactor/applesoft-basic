# Authoritative Emulator Architecture Specification v1.0

## Executive Summary

This document consolidates the complete architectural vision for the Back Pocket BASIC emulator framework, from the immediate **Pocket2e** (Apple IIe-class) target through the speculative **PocketME** (65832-based "Maximum Effort" machine). It serves as the single source of truth for all design decisions, providing a stable foundation for Issue #51 (Page-Based Bus Architecture) and all subsequent work.

---

## Part I: Project Hierarchy & Target Machines

### 1.1 Machine Taxonomy

| Machine      | CPU    | Address Space | Target Fidelity    | Status         |
| ------------ | ------ | ------------- | ------------------ | -------------- |
| **Pocket2e** | 65C02  | 128KB         | Apple IIe Enhanced | Primary target |
| **PocketGS** | 65C816 | 16MB          | Apple IIgs         | Future         |
| **PocketME** | 65832  | 4GB           | Speculative modern | Vision target  |

### 1.2 Compatibility Layers

```
┌─────────────────────────────────────────────────────────┐
│                    PocketME (65832)                     │
│  ┌─────────────────────────────────────────────────┐    │
│  │              PocketGS (65C816)                  │    │
│  │  ┌─────────────────────────────────────────┐    │    │
│  │  │     Pocket2e/2c (65C02)                 │    │    │
│  │  │  ┌───────────────────────────────────┐  │    │    │
│  │  │  │     6502 Emulation Mode           │  │    │    │
│  │  │  └───────────────────────────────────┘  │    │    │
│  │  └─────────────────────────────────────────┘    │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

---

## Part II:  CPU Architecture

### 2.1 CPU Family Inheritance

```csharp
public interface ICpu
{
    CpuFamily Family { get; }
    CpuMode CurrentMode { get; }
    bool Halted { get; }
    ulong CycleCount { get; }
    
    CpuStepResult Step();
    void Reset();
    void SignalIRQ();
    void SignalNMI();
}

public enum CpuFamily
{
    Cpu6502,      // Original MOS 6502
    Cpu65C02,     // WDC 65C02 (Pocket2e target)
    Cpu65C816,    // WDC 65C816 (PocketGS target)
    Cpu65832      // Speculative 32-bit (PocketME target)
}

public enum CpuMode :  byte
{
    // 65C02: Always in this mode
    Compat6502 = 0,
    
    // 65C816 modes
    Emulation = 1,      // E=1:  6502 compatible
    Native16 = 2,       // E=0: 16-bit native
    
    // 65832 modes (from privileged spec v0. 6)
    M0_65C02 = 0x10,    // Legacy 6502 semantics
    M1_65816 = 0x11,    // Legacy 65816 semantics
    M2_65832 = 0x12     // Native 32-bit mode
}
```

### 2.2 Register Models

#### Design Philosophy: Unified Registers with Multi-Size Views

Instead of separate register structs for each CPU variant (65C02, 65C816, 65832), the emulator uses a **single unified `Registers` struct** with maximum-sized registers (`uint`/32-bit). The instruction and addressing handlers determine the effective register width at runtime based on:

- **E flag** (Emulation): `true` = 65C02-compatible 8-bit mode
- **CP flag** (Compatibility): `true` = compatibility mode, `false` = 65832 native
- **M flag** (in P register): Memory/Accumulator width in 65816 native mode
- **X flag** (in P register): Index register width in 65816 native mode

This approach provides several benefits:
- Single codebase handles all CPU variants
- No combinatorial explosion of instruction implementations
- Runtime mode switching without register conversion
- x86-style register views (RAX/EAX/AX/AL pattern)

#### Unified Registers Structure

```csharp
/// <summary>
/// Represents the CPU's register set for all CPU variants (65C02, 65C816, 65832).
/// Registers use maximum size (32-bit) with multi-size view accessors.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Registers
{
    // ─── Classic Registers (Multi-Size Views) ───────────────────────────
    public readonly DWord ZR;           // Constant zero register
    public RegisterAccumulator A;       // Accumulator (8/16/32-bit views)
    public RegisterIndex X;             // X Index (8/16/32-bit views)
    public RegisterIndex Y;             // Y Index (8/16/32-bit views)
    public RegisterDirectPage D;        // Direct Page (16/32-bit views)
    public RegisterStackPointer SP;     // Stack Pointer (8/16/32-bit views)
    public RegisterProgramCounter PC;   // Program Counter (16/32-bit views)
    
    // ─── Status and Mode Flags ──────────────────────────────────────────
    public ProcessorStatusFlags P;      // Processor Status (N V M X D I Z C)
    public bool E;                      // Emulation flag (65C02 mode when true)
    public bool CP;                     // Compatibility mode flag
    
    // ─── Bank Registers (65C816+) ───────────────────────────────────────
    public byte DBR;                    // Data Bank Register
    public byte PBR;                    // Program Bank Register
    
    // ─── General Purpose Registers (65832 only) ─────────────────────────
    public DWord R0, R1, R2, R3, R4, R5, R6, R7;
    // R7 serves as Frame Pointer (FP) by calling convention
    
    // ─── System/Privileged Registers (65832 K-mode only) ────────────────
    public SystemRegisters System;
}

/// <summary>
/// System registers for 65832 privileged operations.
/// Only accessible in Kernel or Hypervisor modes.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SystemRegisters
{
    public DWord CR0;   // Control: PG, NXE, privilege level, ASID
    public DWord PTBR;  // Page Table Base Register (physical)
    public DWord VBAR;  // Vector Base Address Register
    public DWord FAR;   // Fault Address Register
    public DWord FSC;   // Fault Status Code
    public DWord TLS;   // Thread-Local Storage pointer
}
```

#### Register View Types

Each register type provides multiple size views through embedded struct methods:

```csharp
/// <summary>Accumulator with 8/16/32-bit views.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RegisterAccumulator
{
    public DWord acc;   // Full 32-bit storage
    
    // Size-specific accessors
    public readonly byte GetByte() => (byte)(acc & 0xFF);
    public readonly Word GetWord() => (Word)(acc & 0xFFFF);
    public readonly DWord GetDWord() => acc;
    
    public void SetByte(byte value) => acc = (acc & 0xFFFFFF00) | value;
    public void SetWord(Word value) => acc = (acc & 0xFFFF0000) | value;
    public void SetDWord(DWord value) => acc = value;
}

/// <summary>Index register (X, Y) with 8/16/32-bit views.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RegisterIndex
{
    public DWord index;
    
    public readonly byte GetByte() => (byte)(index & 0xFF);
    public readonly Word GetWord() => (Word)(index & 0xFFFF);
    public readonly DWord GetDWord() => index;
    
    public void SetByte(byte value) => index = (index & 0xFFFFFF00) | value;
    public void SetWord(Word value) => index = (index & 0xFFFF0000) | value;
    public void SetDWord(DWord value) => index = value;
}

/// <summary>Stack Pointer with 8/16/32-bit views.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RegisterStackPointer
{
    public DWord stack;
    
    public readonly byte GetByte() => (byte)(stack & 0xFF);
    public readonly Word GetWord() => (Word)(stack & 0xFFFF);
    public readonly DWord GetDWord() => stack;
    
    public void SetByte(byte value) => stack = (stack & 0xFFFFFF00) | value;
    public void SetWord(Word value) => stack = (stack & 0xFFFF0000) | value;
    public void SetDWord(DWord value) => stack = value;
}

/// <summary>Direct Page register with 16/32-bit views.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RegisterDirectPage
{
    public Addr direct;
    
    public readonly Word GetWord() => (Word)(direct & 0xFFFF);
    public readonly Addr GetAddr() => direct;
    
    public void SetWord(Word value) => direct = (direct & 0xFFFF0000) | value;
    public void SetAddr(Addr value) => direct = value;
}

/// <summary>Program Counter with 16/32-bit views and bank support.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RegisterProgramCounter
{
    public Addr addr;
    
    public readonly Addr GetAddr() => addr;
    public readonly Word GetWord() => (Word)(addr & 0xFFFF);
    public readonly byte GetBank() => (byte)((addr >> 16) & 0xFF);
    
    public void SetWord(Word value) => addr = (addr & 0xFFFF0000) | value;
    public void SetBank(byte value) => addr = (addr & 0x00FFFFFF) | ((Addr)value << 16);
    public void SetBankAndWord(byte bank, Word value) => addr = ((Addr)bank << 16) | value;
    public void SetAddr(Addr value) => addr = value;
    
    public void Advance() => addr++;
    public void Advance(int count) => addr = (Addr)(addr + count);
}
```

#### Mode Detection Helpers

Extension methods determine effective register sizes at runtime:

```csharp
extension(ref Registers registers)
{
    /// <summary>Gets the architectural mode based on CP and E flags.</summary>
    public ArchitecturalMode GetArchitecturalMode() => registers switch
    {
        { CP: true, E: true }  => ArchitecturalMode.Mode65C02,
        { CP: true, E: false } => ArchitecturalMode.Mode65816,
        _                      => ArchitecturalMode.Mode65832,
    };
    
    /// <summary>Gets effective accumulator size (8, 16, or 32 bits).</summary>
    public byte GetAccumulatorSize()
    {
        if (registers.Is65C02Mode()) return 8;
        if (registers.Is65816Mode()) 
            return registers.P.IsMemorySize8Bit() ? (byte)8 : (byte)16;
        return 32; // 65832 mode
    }
    
    /// <summary>Gets effective index register size (8, 16, or 32 bits).</summary>
    public byte GetIndexSize()
    {
        if (registers.Is65C02Mode()) return 8;
        if (registers.Is65816Mode())
            return registers.P.IsIndexSize8Bit() ? (byte)8 : (byte)16;
        return 32; // 65832 mode
    }
    
    public bool Is65C02Mode() => registers is { CP: true, E: true };
    public bool Is65816Mode() => registers is { CP: true, E: false };
    public bool Is65832Mode() => registers is { CP: false };
}
```

#### Mode Behavior Summary

| Mode     | E   | CP    | A Size | X/Y Size | SP Size | PC Size | D Range | Banks |
|----------|-----|-------|--------|----------|---------|---------|---------|-------|
| 65C02    | 1   | 1     | 8-bit  | 8-bit    | 8-bit   | 16-bit  | $00     | N/A   |
| 65816 E  | 1   | 1     | 8-bit  | 8-bit    | 8-bit*  | 16-bit  | 16-bit  | DBR/PBR |
| 65816 N  | 0   | 1     | M flag | X flag   | 16-bit  | 16-bit  | 16-bit  | DBR/PBR |
| 65832    | 0   | 0     | 32-bit | 32-bit   | 32-bit  | 32-bit  | 32-bit  | N/A   |

*65816 emulation mode forces SP high byte to $01

#### Initialization Examples

```csharp
// 65C02 compatibility mode (Pocket2e boot)
var registers = new Registers(compat: true, resetVector: 0xFFFC);
// Sets: E=true, CP=true, SP=$01FF, P=Reset flags

// 65832 native mode (PocketME boot)
var registers = new Registers();
// Sets: E=false, CP=false, all registers at 32-bit width
```

#### Usage in Instructions

Instructions use mode-aware accessors for portable implementation:

```csharp
// LDA implementation works for all CPU modes
public static OpcodeHandler LDA(AddressingMode<CpuState> mode)
{
    return (memory, ref state) =>
    {
        Addr address = mode(memory, ref state);
        byte size = state.Registers.GetAccumulatorSize();  // 8, 16, or 32
        var value = memory.ReadValue(address, size);
        
        state.Registers.P.SetZeroAndNegative(value, size);
        state.Registers.A.SetValue(value, size);
    };
}
```

#### Privilege Levels (65832)

```csharp
public enum PrivilegeLevel : byte
{
    User = 0,       // U: Unprivileged code
    Kernel = 1,     // K: OS kernel
    Hypervisor = 2  // H: Reserved for future
}
```

The privilege level is encoded in `CR0` bits 0-1. System registers are only accessible in Kernel or Hypervisor mode; User-mode access triggers a privilege violation trap.

````````

---

## Part III:  Memory & MMU Architecture

### 3.1 Address Space Models

| Machine  | Virtual Bits | Physical Bits | Page Size | Page Table     |
| -------- | ------------ | ------------- | --------- | -------------- |
| Pocket2e | 16           | 16 (128KB)    | N/A       | Soft switches  |
| PocketGS | 24           | 24 (16MB)     | N/A       | Mega II + FPI  |
| PocketME | 32           | 32 (4GB)      | 4KB       | 2-level tables |

### 3.2 Page Table Entry Format (65832/PocketME)

From privileged spec v0.6:

```csharp
[Flags]
public enum PageTableEntryFlags : uint
{
    Present     = 1 << 0,   // P: Page is valid
    Readable    = 1 << 1,   // R: Read allowed
    Writable    = 1 << 2,   // W: Write allowed
    Executable  = 1 << 3,   // X: Execute allowed
    User        = 1 << 4,   // U: User-mode accessible
    Accessed    = 1 << 5,   // A: Has been read (optional HW)
    Dirty       = 1 << 6,   // D: Has been written (optional HW)
    Global      = 1 << 7,   // G:  Ignore ASID for TLB
    Device      = 1 << 8,   // DEV: Device page (not RAM)
    
    // Bits 9-11 reserved for future attributes
    // Bits 12-31:  Physical Frame Number (or Device Page ID if DEV=1)
}

public readonly record struct PageTableEntry(uint Raw)
{
    public bool Present => (Raw & 0x001) != 0;
    public bool Readable => (Raw & 0x002) != 0;
    public bool Writable => (Raw & 0x004) != 0;
    public bool Executable => (Raw & 0x008) != 0;
    public bool User => (Raw & 0x010) != 0;
    public bool Device => (Raw & 0x100) != 0;
    
    public uint PhysicalFrameNumber => Raw >> 12;
    
    // When DEV=1, PFN is interpreted as Device Page ID: 
    // Bits [19:16] = Class (4 bits, 16 classes)
    // Bits [15:8]  = Instance (8 bits, 256 per class)
    // Bits [7:0]   = Page (8 bits, 256 per instance)
    public byte DeviceClass => (byte)((Raw >> 28) & 0xF);
    public byte DeviceInstance => (byte)((Raw >> 20) & 0xFF);
    public byte DevicePage => (byte)((Raw >> 12) & 0xFF);
}
```

### 3.3 Device Page Classes (65832)

```csharp
public enum DevicePageClass : byte
{
    Invalid = 0x0,
    CompatIO = 0x1,       // Apple II $C000-$CFFF I/O page
    SlotROM = 0x2,        // Expansion ROM windows
    Framebuffer = 0x3,    // Video aperture
    Storage = 0x4,        // Disk controller MMIO
    Network = 0x5,        // Network controller
    Timer = 0x6,          // Timer/interrupt controller
    Debug = 0x7,          // Semihosting/debug console
    // 0x8-0xF reserved
}
```

### 3.4 Physical Memory Map (PocketME)

From privileged spec v0.4:

```
0x0000_0000 - 0x0003_FFFF :  Boot ROM (256KB)
0x0004_0000 - ...          : RAM begins
... 
0xFFFC_0000 - 0xFFFF_FFFF :  High ROM alias (256KB mirror)
```

### 3.5 Fault Status Codes

```csharp
public enum FaultStatusCode : uint
{
    NotPresent = 0,
    ReadViolation = 1,
    WriteViolation = 2,
    ExecuteViolation = 3,
    PrivilegeViolation = 4,
    ReservedBitViolation = 5,
    DeviceFault = 6  // DEV page unmapped or device rejected access
}
```

---

## Part IV: Main Bus Architecture

This is the core of Issue #51 and the foundation for all device interaction.

### 4.1 Design Philosophy

From "Main bus architecture. md":

> The bus that's fast on the hot path, but still rich in introspection and cross-talk. 

**Three Planes:**
1. **Data Plane**: O(1) page lookup, direct dispatch, minimal overhead
2. **Control Plane**: Device registration, bank switching, configuration
3. **Observability Plane**:  Tracing, watchpoints, debugging (zero cost when disabled)

### 4.2 Access Context

Every bus operation carries complete context:

```csharp
/// <summary>
/// Complete context for a single bus access. 
/// The CPU computes intent; the bus enforces consequences.
/// </summary>
public readonly record struct BusAccess(
    uint Address,           // Virtual address being accessed
    uint Value,             // Write payload (low bits used by Width)
    byte WidthBits,         // 8, 16, or 32 (effective width after E/M/X)
    CpuMode Mode,           // Compat or Native
    bool EmulationE,        // E flag state (Compat mode only)
    AccessIntent Intent,    // Data/Fetch/Debug/DMA
    int SourceId,           // Who initiated (CPU=0, DMA channels, debugger)
    ulong Cycle,            // Current machine cycle
    AccessFlags Flags       // Atomic/Decompose/NoSideFx/etc. 
);

public enum AccessIntent : byte
{
    DataRead = 0,
    DataWrite = 1,
    InstructionFetch = 2,
    DebugRead = 3,      // Peek:  no side effects
    DebugWrite = 4,     // Poke: no side effects (rare)
    DmaRead = 5,
    DmaWrite = 6
}

[Flags]
public enum AccessFlags : uint
{
    None         = 0,
    NoSideFx     = 1 << 0,  // Peek/Poke semantics
    LittleEndian = 1 << 1,  // (always true for 65xx)
    Atomic       = 1 << 2,  // Request atomic wide op
    Decompose    = 1 << 3   // Force byte-wise cycles
}
```

### 4.3 Page Table Structure

```csharp
/// <summary>
/// Capabilities advertised by a bus target.
/// </summary>
[Flags]
public enum TargetCaps : uint
{
    None          = 0,
    SupportsPeek  = 1 << 0,  // Can handle NoSideFx reads safely
    SupportsWide  = 1 << 1,  // Can do atomic 16/32-bit ops
    SideEffects   = 1 << 2,  // Reads/writes mutate state
    TimingSense   = 1 << 3   // Behavior depends on cycle timing
}

/// <summary>
/// A single 4KB page table entry. 
/// </summary>
public readonly record struct PageEntry(
    int DeviceId,           // Structural instance ID
    ushort RegionTag,       // Classification for tooling
    TargetCaps Caps,        // What this target supports
    IBusTarget Target,      // The actual handler
    uint PhysBase           // Base address within the device
);

/// <summary>
/// Region tags for observability and debugging.
/// </summary>
public enum RegionTag : ushort
{
    Unknown = 0,
    MainRAM = 1,
    AuxRAM = 2,
    ROM = 3,
    IOPage = 4,
    SlotROM = 5,
    ExpansionRAM = 6,
    Unmapped = 0xFFFF
}
```

### 4.4 Bus Target Interface

```csharp
/// <summary>
/// Any device that can respond to bus reads/writes.
/// </summary>
public interface IBusTarget
{
    /// <summary>8-bit read (always required).</summary>
    byte Read8(uint physicalAddress, in BusAccess context);
    
    /// <summary>8-bit write (always required).</summary>
    void Write8(uint physicalAddress, byte value, in BusAccess context);
    
    /// <summary>16-bit read (optional; bus decomposes if not implemented).</summary>
    ushort Read16(uint physicalAddress, in BusAccess context)
        => (ushort)(Read8(physicalAddress, context) | 
                   (Read8(physicalAddress + 1, context) << 8));
    
    /// <summary>16-bit write (optional).</summary>
    void Write16(uint physicalAddress, ushort value, in BusAccess context)
    {
        Write8(physicalAddress, (byte)value, context);
        Write8(physicalAddress + 1, (byte)(value >> 8), context);
    }
    
    /// <summary>32-bit read (optional).</summary>
    uint Read32(uint physicalAddress, in BusAccess context)
        => Read16(physicalAddress, context) | 
           ((uint)Read16(physicalAddress + 2, context) << 16);
    
    /// <summary>32-bit write (optional).</summary>
    void Write32(uint physicalAddress, uint value, in BusAccess context)
    {
        Write16(physicalAddress, (ushort)value, context);
        Write16(physicalAddress + 2, (ushort)(value >> 16), context);
    }
}
```

### 4.5 Main Bus Implementation

```csharp
/// <summary>
/// The main memory bus.  All CPU memory access flows through here.
/// </summary>
public sealed class MainBus :  IMemoryBus
{
    private const int PageShift = 12;        // 4KB pages
    private const uint PageMask = 0xFFF;
    private const int MaxPages = 0x100000;   // 4GB / 4KB = 1M pages
    
    private readonly PageEntry[] _pageTable;
    private readonly ITraceSink?  _trace;
    private bool _traceEnabled;
    
    public MainBus(int addressSpaceBits = 16)
    {
        int pageCount = 1 << (addressSpaceBits - PageShift);
        _pageTable = new PageEntry[pageCount];
    }
    
    public byte Read8(in BusAccess ctx)
    {
        ref readonly var page = ref _pageTable[ctx.Address >> PageShift];
        uint phys = page.PhysBase + (ctx.Address & PageMask);
        
        byte value = page.Target.Read8(phys, ctx);
        
        if (_traceEnabled)
            EmitTrace(ctx, value, page);
        
        return value;
    }
    
    public ushort Read16(in BusAccess ctx)
    {
        // Cross-page check:  always decompose
        if (CrossesPage(ctx.Address, 2))
            return DecomposeRead16(ctx);
        
        // Decompose flag forces byte-wise
        if ((ctx.Flags & AccessFlags.Decompose) != 0)
            return DecomposeRead16(ctx);
        
        ref readonly var page = ref _pageTable[ctx.Address >> PageShift];
        
        // Atomic request + target supports it
        if ((ctx.Flags & AccessFlags.Atomic) != 0 && 
            (page. Caps & TargetCaps. SupportsWide) != 0)
        {
            uint phys = page.PhysBase + (ctx.Address & PageMask);
            return page.Target.Read16(phys, ctx);
        }
        
        // Default:  Compat decomposes, Native tries atomic
        if (ctx.Mode == CpuMode. Compat6502)
            return DecomposeRead16(ctx);
        
        // Native mode: use wide if available
        if ((page.Caps & TargetCaps. SupportsWide) != 0)
        {
            uint phys = page.PhysBase + (ctx.Address & PageMask);
            return page.Target.Read16(phys, ctx);
        }
        
        return DecomposeRead16(ctx);
    }
    
    private bool CrossesPage(uint addr, int bytes)
        => ((addr & PageMask) + (uint)(bytes - 1)) > PageMask;
    
    private ushort DecomposeRead16(in BusAccess ctx)
    {
        var ctx0 = ctx with { WidthBits = 8 };
        var ctx1 = ctx with { Address = ctx.Address + 1, WidthBits = 8 };
        return (ushort)(Read8(ctx0) | (Read8(ctx1) << 8));
    }
    
    // Similar patterns for Write8, Write16, Read32, Write32... 
}
```

### 4.6 Access Policy Rules

**The bus decides, based on these ordered rules:**

1. **Cross-page wide access** → Always decompose
2. **`Decompose` flag set** → Force byte cycles
3. **`Atomic` flag set + `SupportsWide`** → Use wide handler
4. **Compat mode default** → Decompose (Apple II expects byte-visible cycles)
5. **Native mode default** → Atomic when target supports it

**Peek/Poke Rules:**
- `DebugRead`/`DebugWrite` intent → Must not trigger side effects
- If target lacks `SupportsPeek` → Return floating bus or debug fault
- RAM/ROM always support Peek; I/O depends on device declaration

---

## Part V:  Signal Bus (Interrupts & Control Lines)

### 5.1 Signal Model

```csharp
public enum SignalLine
{
    IRQ,        // Maskable interrupt (directly from IFlag/I flag)
    NMI,        // Non-maskable (edge-sensitive)
    Reset,      // System reset
    RDY,        // Ready (clock stretching)
    DmaReq,     // DMA request
    Sync        // Instruction fetch indicator (output)
}

/// <summary>
/// Signal hub manages device-to-CPU lines. 
/// Devices assert/deassert; CPU samples. 
/// </summary>
public interface ISignalBus
{
    void Assert(SignalLine line, int deviceId);
    void Deassert(SignalLine line, int deviceId);
    bool IsAsserted(SignalLine line);
    
    // Edge detection for NMI
    bool ConsumeNmiEdge();
    
    // Observability
    event Action<SignalLine, bool, int, ulong>? SignalChanged;
}

/// <summary>
/// Implementation tracks multiple asserters per line.
/// </summary>
public sealed class SignalBus : ISignalBus
{
    private readonly HashSet<int>[] _asserters;
    private bool _nmiEdgePending;
    private bool _nmiPreviousLevel;
    
    public SignalBus()
    {
        _asserters = new HashSet<int>[Enum.GetValues<SignalLine>().Length];
        for (int i = 0; i < _asserters.Length; i++)
            _asserters[i] = new HashSet<int>();
    }
    
    public void Assert(SignalLine line, int deviceId)
    {
        bool wasAsserted = IsAsserted(line);
        _asserters[(int)line].Add(deviceId);
        bool nowAsserted = IsAsserted(line);
        
        // NMI edge detection (low-to-high transition)
        if (line == SignalLine. NMI && ! wasAsserted && nowAsserted)
            _nmiEdgePending = true;
        
        if (wasAsserted != nowAsserted)
            SignalChanged?. Invoke(line, nowAsserted, deviceId, /* cycle */0);
    }
    
    public void Deassert(SignalLine line, int deviceId)
    {
        bool wasAsserted = IsAsserted(line);
        _asserters[(int)line].Remove(deviceId);
        bool nowAsserted = IsAsserted(line);
        
        if (wasAsserted != nowAsserted)
            SignalChanged?. Invoke(line, nowAsserted, deviceId, 0);
    }
    
    public bool IsAsserted(SignalLine line)
        => _asserters[(int)line].Count > 0;
    
    public bool ConsumeNmiEdge()
    {
        bool pending = _nmiEdgePending;
        _nmiEdgePending = false;
        return pending;
    }
    
    public event Action<SignalLine, bool, int, ulong>? SignalChanged;
}
```

---

## Part VI:  Scheduler & Timing

From `bus-scheduler-spec.md`:

### 6.1 Core Concepts

```csharp
/// <summary>
/// Cycle is the single authoritative unit of simulated time.
/// </summary>
public readonly record struct Cycle(ulong Value)
{
    public static Cycle Zero => new(0);
    public static Cycle operator +(Cycle a, Cycle b) => new(a. Value + b.Value);
    public static Cycle operator -(Cycle a, Cycle b) => new(a.Value - b.Value);
    public static bool operator <(Cycle a, Cycle b) => a.Value < b. Value;
    public static bool operator >(Cycle a, Cycle b) => a.Value > b. Value;
    public static bool operator <=(Cycle a, Cycle b) => a.Value <= b. Value;
    public static bool operator >=(Cycle a, Cycle b) => a.Value >= b. Value;
}

/// <summary>
/// Event classification for profiling and diagnostics.
/// </summary>
public enum ScheduledEventKind
{
    DeviceTimer,
    InterruptLineChange,
    DmaPhase,
    AudioTick,
    VideoScanline,
    DeferredWork,
    Custom
}
```

### 6.2 Scheduler Interface

```csharp
public interface IScheduler
{
    Cycle Now { get; }
    
    /// <summary>Schedule an event at an absolute cycle. </summary>
    EventHandle ScheduleAt(Cycle due, ScheduledEventKind kind, int priority, 
                           Action<IEventContext> callback, object?  tag = null);
    
    /// <summary>Schedule an event relative to now.</summary>
    EventHandle ScheduleAfter(Cycle delta, ScheduledEventKind kind, int priority,
                              Action<IEventContext> callback, object?  tag = null);
    
    /// <summary>Cancel a pending event.</summary>
    bool Cancel(EventHandle handle);
    
    /// <summary>Advance time, dispatching due events.</summary>
    void Advance(Cycle delta);
    
    /// <summary>Dispatch all events due at current cycle.</summary>
    void DispatchDue();
    
    /// <summary>Get next event time (for WAI fast-forward).</summary>
    Cycle?  PeekNextDue();
    
    /// <summary>Jump to next event and dispatch (WAI support).</summary>
    bool JumpToNextEventAndDispatch();
}

public readonly record struct EventHandle(ulong Id);

public interface IEventContext
{
    Cycle Now { get; }
    IScheduler Scheduler { get; }
    ISignalBus Signals { get; }
    IMemoryBus Bus { get; }
}
```

### 6.3 CPU-Scheduler Integration

```csharp
public enum CpuRunState
{
    Running,
    WaitingForInterrupt,  // WAI instruction
    Stopped               // STP instruction
}

public readonly record struct CpuStepResult(
    CpuRunState State,
    Cycle CyclesConsumed
);

/// <summary>
/// Machine loop (conceptual):
/// 1. CPU. Step() → returns cycles consumed and state
/// 2. Scheduler. Advance(cycles)
/// 3. If WAI: jump to next event until wake condition
/// </summary>
```

---

## Part VII:  Device & Peripheral Architecture

### 7.1 Peripheral Interface

```csharp
/// <summary>
/// A peripheral device that can be installed in a slot.
/// </summary>
public interface IPeripheral :  IScheduledDevice
{
    string Name { get; }
    string DeviceType { get; }  // "DiskII", "MockingBoard", etc. 
    
    /// <summary>MMIO region (slot I/O space $C0n0-$C0nF).</summary>
    IBusTarget?  MMIORegion { get; }
    
    /// <summary>Firmware ROM region ($Cn00-$CnFF).</summary>
    IBusTarget? ROMRegion { get; }
    
    /// <summary>Expansion ROM region ($C800-$CFFF when selected).</summary>
    IBusTarget?  ExpansionROMRegion { get; }
    
    void Reset();
}

/// <summary>
/// Device that participates in the scheduler.
/// </summary>
public interface IScheduledDevice
{
    void Initialize(IEventContext context);
}
```

### 7.2 Slot Manager (Apple II Peripheral Bus)

```csharp
/// <summary>
/// Manages the 7 expansion slots of an Apple II. 
/// </summary>
public interface ISlotManager
{
    /// <summary>Installed cards by slot (1-7).</summary>
    IReadOnlyDictionary<int, IPeripheral> Slots { get; }
    
    /// <summary>Currently selected slot for $C800-$CFFF.</summary>
    int?  ActiveExpansionSlot { get; }
    
    void Install(int slot, IPeripheral card);
    void Remove(int slot);
    IPeripheral? GetCard(int slot);
    
    /// <summary>Select a slot for expansion ROM access.</summary>
    void SelectExpansionSlot(int slot);
    
    void Reset();
}
```

### 7.3 Apple II Memory Map

For Pocket2e, the bus must implement these regions:

```
$0000-$01FF : Zero Page + Stack
$0200-$03FF : Input buffer, misc
$0400-$07FF : Text Page 1 / Lo-res Page 1
$0800-$0BFF : Text Page 2 / Lo-res Page 2
$0C00-$1FFF : Free RAM
$2000-$3FFF : Hi-res Page 1
$4000-$5FFF : Hi-res Page 2
$6000-$BFFF : Free RAM (Applesoft, programs)
$C000-$C0FF : Soft switches (I/O page)
$C100-$C7FF : Peripheral card ROM ($Cn00 for slot n)
$C800-$CFFF : Expansion ROM (selected slot)
$D000-$FFFF : ROM / Language Card RAM
```

### 7.4 Soft Switch Handler (Composite Device Page)

The $C000-$C0FF page is a "composite page" that dispatches internally:

```csharp
/// <summary>
/// Handles the Apple II soft switch / I/O page. 
/// </summary>
public sealed class AppleIISoftSwitchPage : IBusTarget
{
    private readonly IVideoController _video;
    private readonly IKeyboard _keyboard;
    private readonly ISpeaker _speaker;
    private readonly IAnnunciators _annunciators;
    private readonly ISlotManager _slots;
    
    public byte Read8(uint physicalAddress, in BusAccess context)
    {
        byte offset = (byte)(physicalAddress & 0xFF);
        
        return offset switch
        {
            // Keyboard
            0x00 => _keyboard. ReadKeyData(),
            0x10 => _keyboard. ReadKeyStrobe(),
            
            // Video switches
            0x50 => _video. SetGraphics(),
            0x51 => _video. SetText(),
            0x52 => _video.SetFullScreen(),
            0x53 => _video.SetMixed(),
            0x54 => _video.SetPage1(),
            0x55 => _video.SetPage2(),
            0x56 => _video. SetLoRes(),
            0x57 => _video.SetHiRes(),
            
            // Speaker
            0x30 => _speaker. Toggle(),
            
            // Annunciators
            >= 0x58 and <= 0x5F => _annunciators.Access(offset),
            
            // Slot I/O ($C080-$C0FF, 16 bytes per slot)
            >= 0x80 => ReadSlotIO(offset),
            
            _ => FloatingBus()
        };
    }
    
    private byte ReadSlotIO(byte offset)
    {
        int slot = (offset - 0x80) >> 4;  // 0-7
        int subOffset = offset & 0x0F;
        
        var card = _slots.GetCard(slot);
        if (card?. MMIORegion is { } mmio)
            return mmio.Read8((uint)subOffset, default);
        
        return FloatingBus();
    }
    
    private byte FloatingBus() => 0xFF;  // Or last data bus value
    
    // Write8 similar pattern... 
}
```

---

## Part VIII: Compatibility Personalities (65832)

From the privileged spec:  the 65832 can run Apple II code in sandboxed contexts.

### 8.1 Compatibility Context

```csharp
/// <summary>
/// A compatibility personality for running legacy code.
/// </summary>
public enum CompatibilityPersonality : uint
{
    None = 0,           // No legacy ROM mapping
    AppleIIe = 1,       // Apple IIe Enhanced
    AppleIIc = 2,       // Apple IIc
    AppleIIgs = 3       // Apple IIgs (M1 mode)
}

/// <summary>
/// COMPATID system register controls which personality is active.
/// </summary>
public sealed class CompatibilityController
{
    public CompatibilityPersonality CurrentPersonality { get; private set; }
    
    /// <summary>
    /// Maps a 64KB compat window for a legacy guest.
    /// The guest sees its traditional memory layout;
    /// the MMU translates to host physical addresses.
    /// </summary>
    public void SetupCompatWindow(uint compatBase, CompatibilityPersonality personality)
    {
        CurrentPersonality = personality;
        // Map RAM region:  compatBase + $0000 - $BFFF
        // Map I/O page:    compatBase + $C000 - $CFFF (device page)
        // Map ROM region: compatBase + $D000 - $FFFF
    }
}
```

### 8.2 Device Page Object Model

```csharp
/// <summary>
/// A device page object for the compat I/O region.
/// Each guest gets its own instance.
/// </summary>
public interface IDevicePageObject
{
    int DevicePageId { get; }  // Encoded as Class|Instance|Page
    DevicePageClass Class { get; }
    
    byte Read8(uint offset, in BusAccess context);
    void Write8(uint offset, byte value, in BusAccess context);
    
    /// <summary>
    /// Called when access fails (invalid offset, unsupported width).
    /// Raises DeviceFault.
    /// </summary>
    void RejectAccess(in BusAccess context);
}
```

---

## Part IX:  Observability & Tracing

### 9.1 Trace Events

```csharp
/// <summary>
/// Compact trace event for bus access logging.
/// No allocations; fits in a ring buffer.
/// </summary>
public readonly struct BusTraceEvent
{
    public readonly ulong Cycle;
    public readonly uint Address;
    public readonly uint Value;
    public readonly byte WidthBits;
    public readonly AccessIntent Intent;
    public readonly AccessFlags Flags;
    public readonly int SourceId;
    public readonly int DeviceId;
    public readonly ushort RegionTag;
}

/// <summary>
/// Ring buffer trace sink. 
/// </summary>
public sealed class TraceRingBuffer
{
    private readonly BusTraceEvent[] _buffer;
    private int _writeIndex;
    
    public TraceRingBuffer(int capacity = 65536)
    {
        // Capacity must be power of 2 for fast wrap
        _buffer = new BusTraceEvent[capacity];
    }
    
    public void Emit(in BusTraceEvent evt)
    {
        _buffer[_writeIndex & (_buffer.Length - 1)] = evt;
        _writeIndex++;
    }
}
```

### 9.2 Device Registry

```csharp
/// <summary>
/// Maps structural IDs to human-readable device info.
/// Write-once at wiring time; read-many for tooling.
/// </summary>
public sealed class DeviceRegistry
{
    private readonly Dictionary<int, DeviceInfo> _devices = new();
    private int _nextId = 1;
    
    public int Register(string kind, string name, string path)
    {
        int id = _nextId++;
        _devices[id] = new DeviceInfo(id, kind, name, path);
        return id;
    }
    
    public bool TryGet(int id, out DeviceInfo info)
        => _devices.TryGetValue(id, out info);
}

public readonly record struct DeviceInfo(
    int Id,
    string Kind,      // "RAM", "ROM", "SlotCard", "SoftSwitch"
    string Name,      // "Main RAM", "Disk II Controller"
    string Path       // "main/memory/ram", "main/slots/6/disk2"
);
```

---

## Part X:  Boot & Reset Sequence

### 10.1 Pocket2e Boot Sequence

```
1. Power-on reset
2. CPU reads reset vector from $FFFC-$FFFD
3. ROM code at reset vector executes
4. Firmware initializes soft switches
5. Autostart ROM searches for bootable disk
6. If found:  load boot sector, jump to $0801
7. If not:  enter monitor or BASIC prompt
```

### 10.2 PocketME Boot Sequence

From privileged spec v0.4:

```
1. Hard reset: 
   - CPU enters K privilege, M2 mode
   - CR0.PG = 0, CR0.NXE = 0
   - VBAR = 0x00000000
   
2. CPU reads RESET vector from VBAR + 0
   - With PG=0, this is physical address 0x00000000
   - Boot ROM must be mapped here
   
3. Boot ROM code: 
   - Initializes minimal hardware state
   - Probes memory, builds memory map
   - Loads stage-2 loader (or kernel) into RAM
   - Builds initial page tables at RAM base
   - Sets PTBR to page table physical address
   - Sets VBAR to kernel's vector page
   - Sets CR0.PG = 1 (enable paging)
   - Sets CR0.NXE = 1 (enable NX)
   - Jumps to kernel entry point
   
4. Kernel entry: 
   - Still in K privilege, M2 mode
   - Full MMU and protection now active
   - Sets up interrupt handlers
   - Initializes devices
   - Optionally:  sets up compatibility contexts for Apple II guests
   - Launches init process or shell
```

### 10.3 Boot Handoff Structure

```csharp
/// <summary>
/// Boot ROM passes this structure to the kernel.
/// Located at physical 0x00040000 (first RAM after ROM).
/// Pointer passed in R0 at kernel entry.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BootHandoff
{
    public const uint Magic = 0x4F484D42;  // "BMHO"
    
    public uint magic;
    public ushort versionMajor;     // 1
    public ushort versionMinor;     // 0
    public uint totalSize;
    public uint flags;
    
    public uint bootRomPhysBase;    // 0x00000000
    public uint bootRomSize;        // 256KB
    public uint ramPhysBase;        // 0x00040000
    public uint ramSize;
    
    public uint compatIdDefault;    // Default personality
    
    public uint cmdlineOffset;      // Offset to command line string
    public uint cmdlineLength;
    
    public uint memMapOffset;       // Offset to memory map entries
    public uint memMapCount;
    
    public uint romInvOffset;       // Offset to ROM inventory
    public uint romInvCount;
}
```

---

## Part XI:  Implementation Roadmap

### Phase 1: Foundation (Issue #51 Phases 1-3)
**Goal:** Core interfaces, basic implementations, page-table routing

- [ ] Define all core types (`BusAccess`, `PageEntry`, `TargetCaps`, etc.)
- [ ] Implement `MainBus` with page table routing
- [ ] Implement `SignalBus` for interrupts
- [ ] Implement `DeviceRegistry`
- [ ] Unit tests for bus routing logic

### Phase 2: Apple II Integration (Issue #51 Phases 4-5)
**Goal:** Pocket2e can boot to ROM prompt

- [ ] Implement `AppleIISoftSwitchPage` (composite page)
- [ ] Implement `SlotManager` for peripheral bus
- [ ] Implement basic RAM/ROM as `IBusTarget`
- [ ] Wire 65C02 to bus architecture
- [ ] Integration test:  boot Apple II ROM

### Phase 3: Devices & Scheduler (Issue #51 Phases 6-7)
**Goal:** Timing-accurate emulation

- [ ] Implement `Scheduler` with cycle-accurate events
- [ ] Video timing (scanlines, VBL)
- [ ] Keyboard handling
- [ ] Speaker output
- [ ] Disk II controller (basic)

### Phase 4: Observability & Debug (Issue #51 Phase 8 + Issue #64)
**Goal:** Developer tooling

- [ ] Trace ring buffer
- [ ] Debug console commands
- [ ] CPU state inspection
- [ ] Memory dumping with Peek semantics
- [ ] Breakpoints and watchpoints

### Phase 5: 65C816 Extension
**Goal:** PocketGS foundation

- [ ] Implement 65C816 CPU core
- [ ] 24-bit addressing in bus
- [ ] Bank switching
- [ ] Mega II emulation

### Phase 6: 65832 Fantasy CPU
**Goal:** PocketME vision

- [ ] Prefix opcode decoding ($42, $43, $44)
- [ ] 32-bit registers and addressing
- [ ] Page-table MMU
- [ ] Privilege levels (U/K)
- [ ] Trap/exception handling
- [ ] Compatibility contexts

---

## Part XII:  Coding Standards & Conventions

### 12.1 Naming Conventions

```csharp
// Interfaces:  I-prefix
public interface IBusTarget { }
public interface IMemoryBus { }

// Enums:  PascalCase, no prefix
public enum AccessIntent { DataRead, DataWrite }

// Structs: readonly record struct for immutable data
public readonly record struct BusAccess(... );

// Constants: Within class, PascalCase
private const int PageShift = 12;
```

### 12.2 Performance Guidelines

1. **Hot path first**: The data plane must be allocation-free
2. **Ref readonly for large structs**: Pass `BusAccess` as `in` parameter
3. **Avoid virtual dispatch in tight loops**: Use concrete types or delegates
4. **Page table is array-indexed**: O(1) lookup, no dictionary
5. **Tracing is guarded**: Single `if (enabled)` check

### 12.3 Testing Strategy

```
Unit Tests:
  - Bus routing logic
  - Page table management
  - Signal assertion/deassertion
  - Scheduler ordering

Integration Tests: 
  - CPU + Bus:  instruction execution
  - Device interaction through soft switches
  - Boot sequence to ROM prompt

Conformance Tests:
  - 6502/65C02 instruction accuracy
  - Apple II memory map behavior
  - Timing accuracy (optional strict mode)
```

---

## Appendix A: Quick Reference

### A. 1 Key Types Summary

| Type          | Purpose                                |
| ------------- | -------------------------------------- |
| `BusAccess`   | Complete context for any bus operation |
| `PageEntry`   | 4KB page routing entry                 |
| `IBusTarget`  | Device that handles reads/writes       |
| `IMemoryBus`  | Main bus interface for CPU             |
| `ISignalBus`  | IRQ/NMI/Reset line management          |
| `IScheduler`  | Cycle-accurate event scheduling        |
| `IPeripheral` | Expansion card interface               |

### A.2 Memory Map Quick Reference

**Pocket2e (64KB):**
```
$0000-$BFFF : RAM
$C000-$C0FF :  Soft switches (I/O)
$C100-$CFFF : Slot ROM / Expansion
$D000-$FFFF : ROM or LC RAM
```

**PocketME (4GB):**
```
$00000000-$0003FFFF : Boot ROM (256KB)
$00040000-...        : RAM
$FFFC0000-$FFFFFFFF : High ROM alias
```

### A.3 Soft Switch Summary (Pocket2e)

| Address | Read          | Write        | Function              |
| ------- | ------------- | ------------ | --------------------- |
| $C000   | Keyboard data |              | Last key pressed      |
| $C010   |               | Clear strobe | Clear keyboard strobe |
| $C030   | Toggle        | Toggle       | Speaker click         |
| $C050   |               | Set          | Graphics mode         |
| $C051   |               | Set          | Text mode             |
| $C052   |               | Set          | Full screen           |
| $C053   |               | Set          | Mixed mode            |
| $C054   |               | Set          | Page 1                |
| $C055   |               | Set          | Page 2                |
| $C056   |               | Set          | Lo-res                |
| $C057   |               | Set          | Hi-res                |

---

## Document History

| Version | Date       | Changes                            |
| ------- | ---------- | ---------------------------------- |
| 1.0     | 2025-12-26 | Initial consolidated specification |

---

This specification consolidates all architectural decisions from the reference documents and provides a clear implementation path from Pocket2e through PocketME.  It serves as the authoritative source for Issue #51 and all subsequent emulator development work. 