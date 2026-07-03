// <copyright file="Program.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

using System.Text.Json;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using BadMango.Emulator.Debug.Agent;
using BadMango.Emulator.Debug.Infrastructure;
using BadMango.Emulator.Debug.Infrastructure.Commands;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

// Basic MCP stdio host for emudbg agent.
// Supports initialize, tools/list, tools/call for generic exec and a few structured tools.
// This is the starting point for full MCP support (per emudbg-agent-plan.md).

var builder = Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        // Register modules similar to main debug for reuse of commands, context etc.
        // For agent, we can provide minimal options.
        containerBuilder.RegisterInstance(new EmudbgOptions()).AsSelf().SingleInstance();

        containerBuilder.RegisterModule<DebugConsoleModule>();
        // No UI module for agent.
    })
    .Build();

using var scope = builder.Services.CreateScope();
var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
var baseContext = scope.ServiceProvider.GetRequiredService<IDebugContext>();

// For agent, we will create per-call contexts with capture.
Console.Error.WriteLine("Emudbg Agent MCP host starting (stdio JSON-RPC).");

// Simple MCP loop
var mcpServer = new McpServer(dispatcher, baseContext);
await mcpServer.RunAsync();

public sealed class McpServer
{
    private readonly ICommandDispatcher dispatcher;
    private readonly IDebugContext templateContext;
    private bool initialized;

    public McpServer(ICommandDispatcher dispatcher, IDebugContext templateContext)
    {
        this.dispatcher = dispatcher;
        this.templateContext = templateContext;
    }

    public async Task RunAsync()
    {
        using var reader = new StreamReader(Console.OpenStandardInput());
        using var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var request = JsonSerializer.Deserialize<JsonRpcRequest>(line);
                if (request == null) continue;

                var response = await HandleRequestAsync(request);
                if (response != null)
                {
                    var json = JsonSerializer.Serialize(response);
                    await writer.WriteLineAsync(json);
                }
            }
            catch (Exception ex)
            {
                // Send error response if possible
                var errorResp = new { jsonrpc = "2.0", id = (object?)null, error = new { code = -32603, message = ex.Message } };
                await writer.WriteLineAsync(JsonSerializer.Serialize(errorResp));
            }
        }
    }

    private async Task<object?> HandleRequestAsync(JsonRpcRequest req)
    {
        switch (req.Method)
        {
            case "initialize":
                initialized = true;
                return new
                {
                    jsonrpc = "2.0",
                    id = req.Id,
                    result = new
                    {
                        protocolVersion = "2024-11-05",
                        capabilities = new { tools = new object() },
                        serverInfo = new { name = "emudbg-agent", version = "0.1.0" }
                    }
                };

            case "tools/list":
                return new
                {
                    jsonrpc = "2.0",
                    id = req.Id,
                    result = new
                    {
                        tools = new object[]
                        {
                            new { name = "emudbg_exec", description = "Execute any emudbg command line (supports ; separated). Returns captured output.", inputSchema = new { type = "object", properties = new { command = new { type = "string" } } } },
                            new { name = "emudbg_regs", description = "Get CPU registers (structured).", inputSchema = new { type = "object", properties = new { } } },
                            // Add more structured tools as we implement (bp, trace, run_until, etc.)
                        }
                    }
                };

            case "tools/call":
                if (req.Params is JsonElement paramsEl && paramsEl.TryGetProperty("name", out var nameEl))
                {
                    var toolName = nameEl.GetString();
                    var arguments = new Dictionary<string, object?>();
                    if (paramsEl.TryGetProperty("arguments", out var argsEl))
                    {
                        // Simple parse
                        if (argsEl.TryGetProperty("command", out var cmdEl))
                            arguments["command"] = cmdEl.GetString();
                    }

                    var result = await CallToolAsync(toolName!, arguments);
                    return new { jsonrpc = "2.0", id = req.Id, result };
                }
                return new { jsonrpc = "2.0", id = req.Id, error = new { code = -32602, message = "Invalid params" } };

            default:
                return new { jsonrpc = "2.0", id = req.Id, error = new { code = -32601, message = "Method not found" } };
        }
    }

    private async Task<object> CallToolAsync(string toolName, Dictionary<string, object?> args)
    {
        // Create a fresh capture context for this call.
        var output = new StringWriter();
        var error = new StringWriter();

        // Use wrapper around the template's ICommandContext part.
        var captureContext = new CaptureContext((ICommandContext)templateContext, output, error);

        if (toolName == "emudbg_exec")
        {
            var cmd = args.TryGetValue("command", out var c) ? c?.ToString() ?? "" : "";
            // Use batch style or dispatch
            var result = dispatcher.Dispatch(captureContext, cmd);
            var captured = output.ToString() + error.ToString();
            if (!string.IsNullOrEmpty(result.Message))
                captured += (result.Success ? "" : "Error: ") + result.Message;

            return new
            {
                content = new[] { new { type = "text", text = captured.Trim() } }
            };
        }
        else if (toolName == "emudbg_regs")
        {
            // Example structured tool: call the regs handler and force JSON
            var regsContext = new CaptureContext(templateContext, output, error) { ForceJson = true };
            var result = dispatcher.Dispatch(regsContext, "regs --json");
            var text = output.ToString();
            return new
            {
                content = new[] { new { type = "text", text = text.Trim() } }
            };
        }

        return new { content = new[] { new { type = "text", text = $"Unknown tool {toolName}" } } };
    }

    private record JsonRpcRequest(string? Id, string Method, object? Params);
}

// Helper to provide a context with swapped writers for capture during tool calls.
internal sealed class CaptureContext : ICommandContext
{
    private readonly ICommandContext inner;

    public CaptureContext(ICommandContext inner, TextWriter output, TextWriter error)
    {
        this.inner = inner;
        Output = output;
        Error = error;
        Input = inner.Input;
        // JsonOutput is computed from ForceJson || inner
    }

    public bool ForceJson { get; set; }

    public ICommandDispatcher Dispatcher => inner.Dispatcher;
    public TextWriter Output { get; set; }
    public TextWriter Error { get; set; }
    public TextReader? Input { get; set; }
    public bool JsonOutput => ForceJson || inner.JsonOutput;
}
