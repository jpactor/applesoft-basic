// <copyright file="BasicInterpreter.cs" company="Josh Pactor">
// Copyright (c) Josh Pactor. All rights reserved.
// </copyright>

namespace ApplesoftBasic.Interpreter.Execution;

using System;

using AST;
using Emulation;
using Microsoft.Extensions.Logging;
using Parser;
using Runtime;

/// <summary>
/// Applesoft BASIC interpreter that orchestrates program execution.
/// </summary>
/// <remarks>
/// This class is responsible for parsing programs, managing execution flow,
/// and coordinating between the execution visitor and runtime context.
/// </remarks>
public class BasicInterpreter : IBasicInterpreter
{
    private readonly IParser parser;
    private readonly IBasicRuntimeContext runtime;
    private readonly ISystemContext system;
    private readonly ILogger<BasicInterpreter> logger;
    private readonly ExecutionContext context;

    private bool running;
    private bool shouldStop;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicInterpreter"/> class.
    /// </summary>
    /// <param name="parser">The parser for BASIC source code.</param>
    /// <param name="runtime">The BASIC runtime context containing language state managers.</param>
    /// <param name="system">The system context containing hardware and I/O services.</param>
    /// <param name="logger">The logger for diagnostic and debugging output.</param>
    public BasicInterpreter(
        IParser parser,
        IBasicRuntimeContext runtime,
        ISystemContext system,
        ILogger<BasicInterpreter> logger)
    {
        this.parser = parser;
        this.runtime = runtime;
        this.system = system;
        this.logger = logger;
        context = new ExecutionContext();
    }

    /// <inheritdoc/>
    public IAppleSystem AppleSystem => system.System;

    /// <inheritdoc/>
    public ProgramNode LoadFromSource(string source)
    {
        logger.LogInformation("Parsing BASIC source code");
        var program = parser.Parse(source);

        // Load program into execution context (builds line number index)
        context.LoadProgram(program);

        return program;
    }

    /// <inheritdoc/>
    public void Run(ProgramNode program)
    {
        logger.LogInformation("Starting BASIC program execution");

        try
        {
            // Load program into execution context
            context.LoadProgram(program);

            // Initialize runtime
            runtime.Clear();
            runtime.Data.Initialize(program.DataValues);

            // Start execution
            context.ResetPosition();
            running = true;
            shouldStop = false;

            Execute();
        }
        catch (ProgramEndException)
        {
            logger.LogInformation("Program ended normally");
        }
        catch (ProgramStopException ex)
        {
            system.IO.WriteLine();
            system.IO.WriteLine(ex.Message);
        }
        catch (BasicRuntimeException ex)
        {
            system.IO.WriteLine();
            system.IO.WriteLine(ex.Message);
            logger.LogError(ex, "Runtime error");
        }
        catch (ParseException ex)
        {
            system.IO.WriteLine();
            system.IO.WriteLine("?SYNTAX ERROR");
            logger.LogError(ex, "Parse error");
        }
        finally
        {
            running = false;
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        shouldStop = true;
    }

    private void Execute()
    {
        // Create execution visitor
        var executor = new ExecutionVisitor(
            runtime,
            system,
            context,
            AppleSystem,
            logger);

        while (running && !shouldStop && !context.IsAtEnd())
        {
            var statement = context.GetCurrentStatement();
            if (statement == null)
            {
                break;
            }

            if (shouldStop)
            {
                break;
            }

            try
            {
                statement.Accept(executor);
            }
            catch (GotoException ex)
            {
                context.JumpToLine(ex.LineNumber);
                continue;
            }
            catch (NextIterationException)
            {
                // Continue with next iteration (position already set by visitor)
                continue;
            }

            // Advance to next statement
            if (!context.AdvanceStatement())
            {
                // End of line, advance to next line
                if (!context.AdvanceLine())
                {
                    // End of program
                    break;
                }
            }
        }
    }
}
