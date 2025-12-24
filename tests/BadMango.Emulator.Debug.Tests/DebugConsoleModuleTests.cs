// <copyright file="DebugConsoleModuleTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Tests;

using Autofac;

/// <summary>
/// Unit tests for the <see cref="DebugConsoleModule"/> class.
/// </summary>
[TestFixture]
public class DebugConsoleModuleTests
{
    /// <summary>
    /// Verifies that the module registers ICommandDispatcher.
    /// </summary>
    [Test]
    public void Module_RegistersCommandDispatcher()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<DebugConsoleModule>();
        using var container = builder.Build();

        var dispatcher = container.Resolve<ICommandDispatcher>();

        Assert.That(dispatcher, Is.Not.Null);
        Assert.That(dispatcher, Is.InstanceOf<CommandDispatcher>());
    }

    /// <summary>
    /// Verifies that the module registers ICommandContext.
    /// </summary>
    [Test]
    public void Module_RegistersCommandContext()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<DebugConsoleModule>();
        using var container = builder.Build();

        var context = container.Resolve<ICommandContext>();

        Assert.That(context, Is.Not.Null);
        Assert.That(context, Is.InstanceOf<CommandContext>());
    }

    /// <summary>
    /// Verifies that the module registers all built-in command handlers.
    /// </summary>
    [Test]
    public void Module_RegistersBuiltInCommandHandlers()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<DebugConsoleModule>();
        using var container = builder.Build();

        var handlers = container.Resolve<IEnumerable<ICommandHandler>>().ToList();

        Assert.That(handlers, Has.Count.EqualTo(4));
        Assert.That(handlers.Select(h => h.Name), Is.EquivalentTo(new[] { "help", "exit", "version", "clear" }));
    }

    /// <summary>
    /// Verifies that the module registers DebugRepl with all handlers wired up.
    /// </summary>
    [Test]
    public void Module_RegistersDebugReplWithHandlers()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<DebugConsoleModule>();
        using var container = builder.Build();

        var repl = container.Resolve<DebugRepl>();
        var dispatcher = container.Resolve<ICommandDispatcher>();

        Assert.Multiple(() =>
        {
            Assert.That(repl, Is.Not.Null);
            Assert.That(dispatcher.Commands, Has.Count.EqualTo(4));
        });
    }

    /// <summary>
    /// Verifies that ICommandDispatcher is registered as singleton.
    /// </summary>
    [Test]
    public void Module_CommandDispatcherIsSingleton()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<DebugConsoleModule>();
        using var container = builder.Build();

        var dispatcher1 = container.Resolve<ICommandDispatcher>();
        var dispatcher2 = container.Resolve<ICommandDispatcher>();

        Assert.That(dispatcher1, Is.SameAs(dispatcher2));
    }

    /// <summary>
    /// Verifies that custom command handlers can be added via the module.
    /// </summary>
    [Test]
    public void Module_AllowsAdditionalCommandHandlerRegistration()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<DebugConsoleModule>();
        builder.RegisterType<TestCustomCommand>()
            .As<ICommandHandler>()
            .SingleInstance();
        using var container = builder.Build();

        var handlers = container.Resolve<IEnumerable<ICommandHandler>>().ToList();

        Assert.That(handlers, Has.Count.EqualTo(5));
        Assert.That(handlers.Select(h => h.Name), Does.Contain("testcmd"));
    }

    private sealed class TestCustomCommand : CommandHandlerBase
    {
        public TestCustomCommand()
            : base("testcmd", "A test custom command")
        {
        }

        public override CommandResult Execute(ICommandContext context, string[] args)
        {
            return CommandResult.Ok("Custom command executed");
        }
    }
}