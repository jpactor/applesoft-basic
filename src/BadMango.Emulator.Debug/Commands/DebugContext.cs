// <copyright file="DebugContext.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Commands;

using BadMango.Emulator.Core;
using BadMango.Emulator.Core.Configuration;

/// <summary>
/// Implementation of <see cref="IDebugContext"/> providing access to emulator components.
/// </summary>
/// <remarks>
/// Provides command handlers with access to the CPU, memory, and disassembler
/// for debugging operations. The emulator components can be attached dynamically
/// after the context is created.
/// </remarks>
public sealed class DebugContext : IDebugContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DebugContext"/> class.
    /// </summary>
    /// <param name="dispatcher">The command dispatcher.</param>
    /// <param name="output">The output writer.</param>
    /// <param name="error">The error writer.</param>
    /// <param name="input">The input reader for interactive commands.</param>
    public DebugContext(ICommandDispatcher dispatcher, TextWriter output, TextWriter error, TextReader? input = null)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        this.Dispatcher = dispatcher;
        this.Output = output;
        this.Error = error;
        this.Input = input;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugContext"/> class with emulator components.
    /// </summary>
    /// <param name="dispatcher">The command dispatcher.</param>
    /// <param name="output">The output writer.</param>
    /// <param name="error">The error writer.</param>
    /// <param name="cpu">The CPU instance.</param>
    /// <param name="memory">The memory instance.</param>
    /// <param name="disassembler">The disassembler instance.</param>
    /// <param name="machineInfo">The machine information.</param>
    /// <param name="tracingListener">The tracing debug listener.</param>
    /// <param name="input">The input reader for interactive commands.</param>
    public DebugContext(
        ICommandDispatcher dispatcher,
        TextWriter output,
        TextWriter error,
        ICpu? cpu,
        IMemory? memory,
        IDisassembler? disassembler,
        MachineInfo? machineInfo = null,
        TracingDebugListener? tracingListener = null,
        TextReader? input = null)
        : this(dispatcher, output, error, input)
    {
        this.Cpu = cpu;
        this.Memory = memory;
        this.Disassembler = disassembler;
        this.MachineInfo = machineInfo;
        this.TracingListener = tracingListener;
    }

    /// <inheritdoc/>
    public ICommandDispatcher Dispatcher { get; }

    /// <inheritdoc/>
    public TextWriter Output { get; }

    /// <inheritdoc/>
    public TextWriter Error { get; }

    /// <inheritdoc/>
    public TextReader? Input { get; }

    /// <inheritdoc/>
    public ICpu? Cpu { get; private set; }

    /// <inheritdoc/>
    public IMemory? Memory { get; private set; }

    /// <inheritdoc/>
    public IDisassembler? Disassembler { get; private set; }

    /// <inheritdoc/>
    public MachineInfo? MachineInfo { get; private set; }

    /// <inheritdoc/>
    public TracingDebugListener? TracingListener { get; private set; }

    /// <inheritdoc/>
    public bool IsSystemAttached => this.Cpu is not null && this.Memory is not null && this.Disassembler is not null;

    /// <summary>
    /// Creates a debug context using the standard console streams.
    /// </summary>
    /// <param name="dispatcher">The command dispatcher.</param>
    /// <returns>A new <see cref="DebugContext"/> using console streams.</returns>
    public static DebugContext CreateConsoleContext(ICommandDispatcher dispatcher)
    {
        return new DebugContext(dispatcher, Console.Out, Console.Error, Console.In);
    }

    /// <summary>
    /// Attaches a CPU to this debug context.
    /// </summary>
    /// <param name="cpu">The CPU to attach.</param>
    public void AttachCpu(ICpu cpu)
    {
        ArgumentNullException.ThrowIfNull(cpu);
        this.Cpu = cpu;
    }

    /// <summary>
    /// Attaches memory to this debug context.
    /// </summary>
    /// <param name="memory">The memory to attach.</param>
    public void AttachMemory(IMemory memory)
    {
        ArgumentNullException.ThrowIfNull(memory);
        this.Memory = memory;
    }

    /// <summary>
    /// Attaches a disassembler to this debug context.
    /// </summary>
    /// <param name="disassembler">The disassembler to attach.</param>
    public void AttachDisassembler(IDisassembler disassembler)
    {
        ArgumentNullException.ThrowIfNull(disassembler);
        this.Disassembler = disassembler;
    }

    /// <summary>
    /// Attaches machine information to this debug context.
    /// </summary>
    /// <param name="machineInfo">The machine information to attach.</param>
    public void AttachMachineInfo(MachineInfo machineInfo)
    {
        ArgumentNullException.ThrowIfNull(machineInfo);
        this.MachineInfo = machineInfo;
    }

    /// <summary>
    /// Attaches a tracing debug listener to this debug context.
    /// </summary>
    /// <param name="tracingListener">The tracing listener to attach.</param>
    public void AttachTracingListener(TracingDebugListener tracingListener)
    {
        ArgumentNullException.ThrowIfNull(tracingListener);
        this.TracingListener = tracingListener;
    }

    /// <summary>
    /// Attaches all emulator components to this debug context.
    /// </summary>
    /// <param name="cpu">The CPU to attach.</param>
    /// <param name="memory">The memory to attach.</param>
    /// <param name="disassembler">The disassembler to attach.</param>
    public void AttachSystem(ICpu cpu, IMemory memory, IDisassembler disassembler)
    {
        this.AttachCpu(cpu);
        this.AttachMemory(memory);
        this.AttachDisassembler(disassembler);
    }

    /// <summary>
    /// Attaches all emulator components and machine information to this debug context.
    /// </summary>
    /// <param name="cpu">The CPU to attach.</param>
    /// <param name="memory">The memory to attach.</param>
    /// <param name="disassembler">The disassembler to attach.</param>
    /// <param name="machineInfo">The machine information to attach.</param>
    public void AttachSystem(ICpu cpu, IMemory memory, IDisassembler disassembler, MachineInfo machineInfo)
    {
        this.AttachSystem(cpu, memory, disassembler);
        this.AttachMachineInfo(machineInfo);
    }

    /// <summary>
    /// Attaches all emulator components, machine information, and tracing listener to this debug context.
    /// </summary>
    /// <param name="cpu">The CPU to attach.</param>
    /// <param name="memory">The memory to attach.</param>
    /// <param name="disassembler">The disassembler to attach.</param>
    /// <param name="machineInfo">The machine information to attach.</param>
    /// <param name="tracingListener">The tracing listener to attach.</param>
    public void AttachSystem(ICpu cpu, IMemory memory, IDisassembler disassembler, MachineInfo machineInfo, TracingDebugListener tracingListener)
    {
        this.AttachSystem(cpu, memory, disassembler, machineInfo);
        this.AttachTracingListener(tracingListener);
    }

    /// <summary>
    /// Detaches all emulator components from this debug context.
    /// </summary>
    public void DetachSystem()
    {
        this.Cpu = null;
        this.Memory = null;
        this.Disassembler = null;
        this.MachineInfo = null;
        this.TracingListener = null;
    }
}