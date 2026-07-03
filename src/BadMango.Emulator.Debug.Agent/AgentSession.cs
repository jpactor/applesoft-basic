// <copyright file="AgentSession.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Agent;

using System.Text;
using System.Text.Json;

using BadMango.Emulator.Debug.Infrastructure.Commands;

/// <summary>
/// Represents a single agent session with a live emulator instance.
/// Captures command output for structured tool responses.
/// </summary>
public sealed class AgentSession : IDisposable
{
    private readonly ICommandDispatcher dispatcher;
    private readonly IDebugContext debugContext;
    private bool disposed;

    public AgentSession(ICommandDispatcher dispatcher, IDebugContext debugContext)
    {
        this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        this.debugContext = debugContext ?? throw new ArgumentNullException(nameof(debugContext));
    }

    /// <summary>
    /// Executes a command line and captures the output (text or JSON depending on context).
    /// </summary>
    public async Task<string> ExecuteAsync(string commandLine, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return string.Empty;

        // Use capture writers for this call
        var outputCapture = new StringWriter();
        var errorCapture = new StringWriter();

        // Temporarily redirect (note: for production we'd make setters or use a wrapper context)
        var originalOutput = debugContext.Output;
        var originalError = debugContext.Error;

        // Since properties are get-only currently, we create a new context for capture if possible.
        // For now, assume we can swap via internal or use a proxy.
        // Simplified: use the dispatch which writes to the context's writers.
        // In practice, the agent host will create contexts with capture writers per call.

        // For this skeleton, we'll dispatch and assume capture is set up externally or enhance later.
        var result = dispatcher.Dispatch(debugContext, commandLine);

        // If result has message, include it
        if (!string.IsNullOrEmpty(result.Message))
        {
            if (result.Success)
                debugContext.Output.WriteLine(result.Message);
            else
                debugContext.Error.WriteLine("Error: " + result.Message);
        }

        // In real impl, we'd capture from the writers.
        // For skeleton, return placeholder + any direct result.
        await Task.CompletedTask;

        // Reset (in real, use scoped)
        // For demo, just return the command result message if present.
        return result.Message ?? (result.Success ? "ok" : "error");
    }

    /// <summary>
    /// Executes and returns structured result (for specific tools).
    /// </summary>
    public Task<object?> ExecuteStructuredAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken ct = default)
    {
        // Placeholder: for generic, fall to exec.
        // Specific tools will be implemented to call handlers directly and return typed data.
        return Task.FromResult<object?>(null);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            (debugContext as IDisposable)?.Dispose();
            disposed = true;
        }
    }
}
