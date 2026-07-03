// <copyright file="Program.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

using System.IO;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using BadMango.Emulator.Debug.Infrastructure;
using BadMango.Emulator.Debug.UI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    ////.MinimumLevel.Override("BadMango.Emulator.Devices", LogEventLevel.Verbose) // Uncomment for verbose device-level logging
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        restrictedToMinimumLevel: LogEventLevel.Warning)
    .WriteTo.File(
        "logs/emudbg-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

static void PrintUsage()
{
    Console.WriteLine("""
        emudbg - Emulator Debug Console for BackPocketBASIC

        Usage:
          emudbg [options]

        Options:
          -p, --profile <name>   Load the specified machine profile on startup
                                 (e.g. pocket2e-a2-enh, pocket2e-lite, simple-65c02)
          -e, --exec <commands>  Execute the given commands then exit (non-interactive).
                                 Separate multiple commands with ; or \n
          -f, --file <path>      Read and execute commands from a text file (one command per line)
          --no-banner            Do not display the startup banner
          -h, --help             Show this help and exit

        Examples:
          emudbg --profile pocket2e-lite
          emudbg --exec "boot;regs;step 20;regs;exit"
          emudbg -e "profile list\nboot\nregs" --no-banner
          emudbg --file my-debug-script.txt

        When no options are given, starts an interactive REPL.
        See AGENTS.md for guidance on using emudbg from AI agents and tools.
        """);
}

try
{
    var options = EmudbgOptions.Parse(args);

    if (options.ShowHelp)
    {
        PrintUsage();
        return 0;
    }

    // Validate script file early (before container) so we can exit with message
    if (!string.IsNullOrWhiteSpace(options.ScriptFile) && !File.Exists(options.ScriptFile))
    {
        Console.WriteLine($"Error: Script file not found: {options.ScriptFile}");
        return 1;
    }

    Log.Information("Starting Emulator Debug Console");

    // Build the host
    var host = Host.CreateDefaultBuilder(args)
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>(builder =>
        {
            // Register startup options so modules and components can read CLI overrides (profile, etc.)
            builder.RegisterInstance(options).AsSelf().SingleInstance();

            builder.RegisterModule<DebugConsoleModule>();
            builder.RegisterModule<DebugUiModule>();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
        })
        .UseSerilog(Log.Logger)
        .Build();

    // Run the REPL (or script)
    using var scope = host.Services.CreateScope();
    var repl = scope.ServiceProvider.GetRequiredService<DebugRepl>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

    // Configure non-interactive / scripting behavior
    bool isScriptMode = !string.IsNullOrEmpty(options.ExecCommands) || !string.IsNullOrEmpty(options.ScriptFile);
    if (options.NoBanner || isScriptMode || options.JsonOutput)
    {
        repl.ShowBanner = false;
        repl.ShowPrompt = false;
    }

    logger.Information("Debug console initialized");
    repl.Run();

    logger.Information("Debug console exited normally");
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Debug console terminated unexpectedly");
    Console.WriteLine($"Fatal error: {ex.Message}");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}