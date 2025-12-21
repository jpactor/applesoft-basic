# Addressing Mode and Opcode Deduplication - Implementation Summary

## Overview
This implementation provides a reusable infrastructure for addressing modes and instruction logic that eliminates code duplication across 6502-family CPU variants (65C02, 65816, 65832).

## Architecture

### AddressingModes.cs
A static class providing reusable addressing mode implementations:

**Read Operations:**
- `ReadImmediate()` - Immediate addressing
- `ReadZeroPage()` - Zero Page addressing
- `ReadZeroPageX()` / `ReadZeroPageY()` - Indexed Zero Page
- `ReadAbsolute()` - Absolute addressing
- `ReadAbsoluteX()` / `ReadAbsoluteY()` - Indexed Absolute
- `ReadIndirectX()` / `ReadIndirectY()` - Indirect addressing

**Write Operations:**
- `WriteZeroPage()` - Zero Page addressing
- `WriteZeroPageX()` / `WriteZeroPageY()` - Indexed Zero Page
- `WriteAbsolute()` - Absolute addressing
- `WriteAbsoluteX()` / `WriteAbsoluteY()` - Indexed Absolute
- `WriteIndirectX()` / `WriteIndirectY()` - Indirect addressing

**Key Features:**
- All methods accept memory, PC, and cycles by reference for direct manipulation
- Indexed modes accept register values (x/y) as parameters
- Proper cycle counting including page boundary detection
- Aggressive inlining for performance (`MethodImpl(MethodImplOptions.AggressiveInlining)`)

### Instructions.cs
A static class providing reusable instruction logic:

**Implemented Instructions:**
- `LDA()` - Load Accumulator
- `LDX()` - Load X Register
- `LDY()` - Load Y Register

**Design Pattern:**
- Instructions accept a value and manipulate registers/flags by reference
- Separated instruction semantics from addressing mode logic
- Can be composed with any addressing mode
- Private `SetZN()` helper handles flag updates

## Usage Example

### Before (Duplicated Code)
```csharp
internal void LDA_Immediate()
{
    a = FetchByte();
    SetZN(a);
}

internal void LDA_ZeroPage()
{
    a = ReadZeroPage();
    SetZN(a);
}
```

### After (Shared Infrastructure)
```csharp
internal void LDA_Immediate()
{
    byte value = AddressingModes.ReadImmediate(memory, ref pc, ref cycles);
    Instructions.LDA(value, ref a, ref p);
}

internal void LDA_ZeroPage()
{
    byte value = AddressingModes.ReadZeroPage(memory, ref pc, ref cycles);
    Instructions.LDA(value, ref a, ref p);
}
```

## Benefits

1. **Code Reuse**: Addressing mode logic is centralized and can be used by all CPU variants
2. **Maintainability**: Bug fixes and enhancements to addressing modes benefit all CPUs
3. **Type Safety**: Uses C# ref parameters for compile-time safety
4. **Performance**: Aggressive inlining maintains performance
5. **Extensibility**: New CPU variants can immediately leverage existing infrastructure
6. **Testability**: Addressing modes and instructions can be tested independently

## Testing

### Test Coverage
- **AddressingModesTests**: 21 tests covering all addressing modes (read and write)
- **InstructionsTests**: 8 tests covering instruction logic and flag handling
- **Cpu65C02Tests**: 36 existing tests verify backward compatibility

### Test Results
- ✅ All 65 emulator tests passing
- ✅ All 429 BASIC interpreter tests passing
- ✅ No warnings or errors
- ✅ Backward compatible with existing Cpu65C02 implementation

## Future Work

### Potential Extensions
1. Expand Instructions.cs with more 6502 instructions (ADC, SBC, AND, ORA, EOR, etc.)
2. Implement 65816-specific addressing modes (24-bit addressing)
3. Implement 65832-specific addressing modes (32-bit addressing)
4. Consider creating generic opcode table builders that use the shared infrastructure
5. Add performance benchmarks to ensure inlining effectiveness

### CPU Variant Implementation Pattern
New CPU variants can follow this pattern:

```csharp
internal void LDA_ZeroPage()
{
    byte value = AddressingModes.ReadZeroPage(memory, ref pc, ref cycles);
    Instructions.LDA(value, ref a, ref p);
}
```

This ensures consistency across CPU variants while allowing CPU-specific state management.

## Technical Notes

### Cycle Counting
The implementation maintains cycle counts matching the original Cpu65C02:
- Addressing mode helpers update the cycles counter directly
- Page boundary detection adds extra cycles where appropriate
- Total cycle counts match 6502 specifications

### Memory and State Access
- Memory access goes through the `IMemory` interface
- PC and cycles are passed by reference for direct updates
- Register values (x, y) are passed by value to addressing modes
- Register updates (a, x, y, p) are done by reference in Instructions

### Performance Considerations
- All hot-path methods use `MethodImpl(MethodImplOptions.AggressiveInlining)`
- ref parameters avoid unnecessary copying
- Static methods avoid virtual dispatch overhead
- Design allows JIT compiler to optimize across method boundaries

## Files Changed
- **New**: `src/BadMango.Emulator.Emulation/Cpu/AddressingModes.cs`
- **New**: `src/BadMango.Emulator.Emulation/Cpu/Instructions.cs`
- **Modified**: `src/BadMango.Emulator.Emulation/Cpu/Cpu65C02.cs`
- **New**: `tests/BadMango.Emulator.Tests/AddressingModesTests.cs`
- **New**: `tests/BadMango.Emulator.Tests/InstructionsTests.cs`

## Conclusion
This implementation successfully deduplicates addressing mode and instruction logic while maintaining backward compatibility, type safety, and performance. The architecture provides a solid foundation for implementing the 65816 and 65832 CPU variants with minimal code duplication.
