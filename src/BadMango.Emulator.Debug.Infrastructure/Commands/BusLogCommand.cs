// <copyright file="BusLogCommand.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Commands;

using System.Globalization;

/// <summary>
/// Displays the bus fault log captured by the memory bus and lets the
/// operator clear it.
/// </summary>
/// <remarks>
/// <para>
/// The memory bus pushes every fault it observes (unmapped, permission,
/// NX, misaligned, device, plus synthetic faults for silent floating-bus
/// reads from composite targets with no sub-handler) into a fixed-capacity
/// ring buffer. This command renders that ring buffer.
/// </para>
/// <para>
/// This command requires a bus to be attached to the debug context.
/// </para>
/// </remarks>
public sealed class BusLogCommand : CommandHandlerBase, ICommandHelp
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusLogCommand"/> class.
    /// </summary>
    public BusLogCommand()
        : base("buslog", "Show or clear the bus fault log")
    {
    }

    /// <inheritdoc/>
    public override IReadOnlyList<string> Aliases { get; } = ["bl"];

    /// <inheritdoc/>
    public override string Usage => "buslog [show|tail|clear|status] [N]";

    /// <inheritdoc/>
    public string Synopsis => "buslog [show|tail|clear|status] [N]";

    /// <inheritdoc/>
    public string DetailedDescription =>
        "Inspects the bus fault ring buffer. The bus records every fault it " +
        "observes (Unmapped, Permission, Nx, Misaligned, DeviceFault) plus " +
        "synthetic Unmapped entries for silent floating-bus reads where a " +
        "composite target had no sub-handler for the requested offset.\n" +
        "  show [N] | tail [N]   Print the last N faults (default 20).\n" +
        "  clear                 Empty the buffer and reset counters.\n" +
        "  status                Show buffer capacity / counters.";

    /// <inheritdoc/>
    public IReadOnlyList<CommandOption> Options { get; } = [];

    /// <inheritdoc/>
    public IReadOnlyList<string> Examples { get; } =
    [
        "buslog                   Show the last 20 faults",
        "buslog show 100          Show the last 100 faults",
        "buslog clear             Empty the buffer",
        "buslog status            Show buffer capacity and counters",
    ];

    /// <inheritdoc/>
    public string? SideEffects =>
        "'clear' discards the captured fault history.";

    /// <inheritdoc/>
    public IReadOnlyList<string> SeeAlso { get; } = ["fault", "switches", "regions"];

    /// <inheritdoc/>
    public override CommandResult Execute(ICommandContext context, string[] args)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context is not IDebugContext debugContext)
        {
            return CommandResult.Error("Debug context required for this command.");
        }

        if (!debugContext.IsBusAttached || debugContext.Bus is null)
        {
            return CommandResult.Error("No bus attached. This command requires a bus-based system.");
        }

        var ring = debugContext.Bus.FaultRing;
        if (ring is null)
        {
            return CommandResult.Error("Bus fault recording is not enabled for this bus.");
        }

        string subcommand = args.Length > 0 ? args[0].ToLowerInvariant() : "show";

        return subcommand switch
        {
            "show" or "tail" or "" => ShowLog(debugContext, ring, args),
            "clear" => ClearLog(debugContext, ring),
            "status" => ShowStatus(debugContext, ring),
            _ => CommandResult.Error($"Unknown subcommand: '{args[0]}'. Use: show, tail, clear, or status."),
        };
    }

    private static CommandResult ShowLog(IDebugContext context, BadMango.Emulator.Bus.Interfaces.IBusFaultRing ring, string[] args)
    {
        int n = 20;
        if (args.Length > 1 && int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) && parsed > 0)
        {
            n = parsed;
        }

        var snapshot = ring.Snapshot();
        if (snapshot.Length == 0)
        {
            context.Output.WriteLine("Bus fault log is empty.");
            return CommandResult.Ok();
        }

        int start = Math.Max(0, snapshot.Length - n);
        int shown = snapshot.Length - start;

        context.Output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Bus fault log (showing {shown} of {ring.Count}; {ring.TotalFaults} total recorded):"));
        context.Output.WriteLine();
        context.Output.WriteLine("  Cycle           Kind        Addr  W  Intent             Device  Region");
        context.Output.WriteLine("  --------------- ----------- ----- -- ------------------ ------- ------------");
        for (int i = start; i < snapshot.Length; i++)
        {
            var f = snapshot[i];
            context.Output.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"  {f.Cycle,15} {f.Kind,-11} ${f.Address:X4} {f.WidthBits,2} {f.Intent,-18} {f.DeviceId,7} {f.RegionTag}"));
        }

        return CommandResult.Ok();
    }

    private static CommandResult ClearLog(IDebugContext context, BadMango.Emulator.Bus.Interfaces.IBusFaultRing ring)
    {
        ring.Clear();
        context.Output.WriteLine("Bus fault log cleared.");
        return CommandResult.Ok();
    }

    private static CommandResult ShowStatus(IDebugContext context, BadMango.Emulator.Bus.Interfaces.IBusFaultRing ring)
    {
        context.Output.WriteLine("Bus Fault Log Status:");
        context.Output.WriteLine();
        context.Output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"  Buffer capacity:  {ring.Capacity}"));
        context.Output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"  Faults in buffer: {ring.Count}"));
        context.Output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"  Total recorded:   {ring.TotalFaults}"));
        return CommandResult.Ok();
    }
}