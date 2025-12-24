// <copyright file="PokeCommand.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Commands;

using System.Globalization;

/// <summary>
/// Writes one or more bytes to memory.
/// </summary>
/// <remarks>
/// <para>
/// Supports multiple modes of operation:
/// - Single byte: poke $1234 $AB.
/// - Multiple bytes: poke $1234 $AB $CD $EF.
/// - Byte sequence: poke $1234 "Hello" (writes ASCII bytes).
/// - Interactive mode: poke $1234 -i (enter bytes interactively).
/// </para>
/// <para>
/// In interactive mode, enter hex bytes separated by spaces. Enter an empty line
/// or 'q' to finish. The address auto-increments after each entry.
/// </para>
/// </remarks>
public sealed class PokeCommand : CommandHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PokeCommand"/> class.
    /// </summary>
    public PokeCommand()
        : base("poke", "Write bytes to memory")
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> Aliases { get; } = ["w", "write"];

    /// <inheritdoc/>
    public override string Usage => "poke <address> <byte> [byte...]  or  poke <address> -i  or  poke <address> \"string\"";

    /// <inheritdoc/>
    public override CommandResult Execute(ICommandContext context, string[] args)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context is not IDebugContext debugContext)
        {
            return CommandResult.Error("Debug context required for this command.");
        }

        if (debugContext.Memory is null)
        {
            return CommandResult.Error("No memory attached. Use 'attach' command first.");
        }

        if (args.Length == 0)
        {
            return CommandResult.Error("Address required. Usage: poke <address> <byte> [byte...] or poke <address> -i");
        }

        if (!TryParseAddress(args[0], out uint startAddress))
        {
            return CommandResult.Error($"Invalid address: '{args[0]}'. Use hex format ($1234 or 0x1234) or decimal.");
        }

        // Check for interactive mode
        if (args.Length >= 2 && (args[1].Equals("-i", StringComparison.OrdinalIgnoreCase) ||
                                  args[1].Equals("--interactive", StringComparison.OrdinalIgnoreCase)))
        {
            return ExecuteInteractiveMode(debugContext, startAddress);
        }

        if (args.Length < 2)
        {
            return CommandResult.Error("Address and at least one byte value required. Usage: poke <address> <byte> [byte...] or poke <address> -i");
        }

        // Check if we have a string argument
        if (args.Length == 2 && args[1].StartsWith("\"", StringComparison.Ordinal))
        {
            return WriteString(debugContext, startAddress, args[1]);
        }

        // Parse byte values
        var bytes = new List<byte>();
        for (int i = 1; i < args.Length; i++)
        {
            if (!TryParseByte(args[i], out byte value))
            {
                return CommandResult.Error($"Invalid byte value: '{args[i]}'. Use hex format ($AB or 0xAB) or decimal (0-255).");
            }

            bytes.Add(value);
        }

        // Validate address range
        if (startAddress >= debugContext.Memory.Size)
        {
            return CommandResult.Error($"Address ${startAddress:X4} is out of range (memory size: ${debugContext.Memory.Size:X4}).");
        }

        if (startAddress + (uint)bytes.Count > debugContext.Memory.Size)
        {
            return CommandResult.Error($"Write would exceed memory bounds. Start: ${startAddress:X4}, Count: {bytes.Count}, Memory size: ${debugContext.Memory.Size:X4}");
        }

        // Write bytes
        WriteBytes(debugContext, startAddress, bytes);

        return CommandResult.Ok();
    }

    private static CommandResult ExecuteInteractiveMode(IDebugContext context, uint startAddress)
    {
        if (context.Input is null)
        {
            return CommandResult.Error("Interactive mode not available (no input reader).");
        }

        if (context.Memory is null)
        {
            return CommandResult.Error("No memory attached.");
        }

        context.Output.WriteLine($"Interactive poke mode starting at ${startAddress:X4}");
        context.Output.WriteLine("Enter hex bytes (space-separated), empty line or 'q' to finish.");
        context.Output.WriteLine();

        uint currentAddress = startAddress;
        int totalBytesWritten = 0;

        while (true)
        {
            context.Output.Write($"${currentAddress:X4}: ");

            var line = context.Input.ReadLine();
            if (line is null || string.IsNullOrWhiteSpace(line) ||
                line.Equals("q", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var bytes = new List<byte>();
            bool hasError = false;

            foreach (var part in parts)
            {
                if (TryParseByte(part, out byte value))
                {
                    bytes.Add(value);
                }
                else
                {
                    context.Error.WriteLine($"Invalid byte value: '{part}'. Skipping.");
                    hasError = true;
                }
            }

            if (bytes.Count > 0)
            {
                // Validate address range
                if (currentAddress + (uint)bytes.Count > context.Memory.Size)
                {
                    context.Error.WriteLine($"Write would exceed memory bounds at ${currentAddress:X4}. Stopping.");
                    break;
                }

                // Write bytes
                for (int i = 0; i < bytes.Count; i++)
                {
                    context.Memory.Write(currentAddress + (uint)i, bytes[i]);
                }

                var hexValues = string.Join(" ", bytes.Select(b => $"${b:X2}"));
                context.Output.WriteLine($"  Wrote: {hexValues}");

                currentAddress += (uint)bytes.Count;
                totalBytesWritten += bytes.Count;
            }
            else if (!hasError)
            {
                // Empty line with no errors means quit
                break;
            }
        }

        context.Output.WriteLine();
        context.Output.WriteLine($"Interactive mode complete. Wrote {totalBytesWritten} byte(s) from ${startAddress:X4} to ${currentAddress - 1:X4}.");

        return CommandResult.Ok();
    }

    private static CommandResult WriteString(IDebugContext context, uint startAddress, string quotedString)
    {
        // Remove quotes
        var content = quotedString.Trim('"');
        var bytes = System.Text.Encoding.ASCII.GetBytes(content);

        if (context.Memory is null)
        {
            return CommandResult.Error("No memory attached.");
        }

        if (startAddress + (uint)bytes.Length > context.Memory.Size)
        {
            return CommandResult.Error($"Write would exceed memory bounds.");
        }

        WriteBytes(context, startAddress, bytes);

        return CommandResult.Ok();
    }

    private static void WriteBytes(IDebugContext context, uint startAddress, IReadOnlyList<byte> bytes)
    {
        if (context.Memory is null)
        {
            return;
        }

        for (int i = 0; i < bytes.Count; i++)
        {
            context.Memory.Write(startAddress + (uint)i, bytes[i]);
        }

        // Display confirmation
        context.Output.WriteLine($"Wrote {bytes.Count} byte(s) starting at ${startAddress:X4}:");

        // Show what was written
        var hexValues = string.Join(" ", bytes.Select(b => $"${b:X2}"));
        context.Output.WriteLine($"  {hexValues}");
    }

    private static bool TryParseAddress(string value, out uint result)
    {
        result = 0;

        if (value.StartsWith("$", StringComparison.Ordinal))
        {
            return uint.TryParse(value[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return uint.TryParse(value[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        return uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    private static bool TryParseByte(string value, out byte result)
    {
        result = 0;

        if (value.StartsWith("$", StringComparison.Ordinal))
        {
            return byte.TryParse(value[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return byte.TryParse(value[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        return byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }
}