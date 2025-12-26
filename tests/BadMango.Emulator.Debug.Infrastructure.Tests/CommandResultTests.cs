// <copyright file="CommandResultTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Debug.Infrastructure.Tests;

/// <summary>
/// Unit tests for the <see cref="CommandResult"/> class.
/// </summary>
[TestFixture]
public class CommandResultTests
{
    /// <summary>
    /// Verifies that Ok() creates a successful result with no message.
    /// </summary>
    [Test]
    public void Ok_CreatesSuccessfulResultWithNoMessage()
    {
        var result = CommandResult.Ok();

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.Null);
            Assert.That(result.ShouldExit, Is.False);
        });
    }

    /// <summary>
    /// Verifies that Ok(message) creates a successful result with message.
    /// </summary>
    [Test]
    public void OkWithMessage_CreatesSuccessfulResultWithMessage()
    {
        var result = CommandResult.Ok("Operation completed");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Operation completed"));
            Assert.That(result.ShouldExit, Is.False);
        });
    }

    /// <summary>
    /// Verifies that Error(message) creates a failed result with message.
    /// </summary>
    [Test]
    public void Error_CreatesFailedResultWithMessage()
    {
        var result = CommandResult.Error("Something went wrong");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Something went wrong"));
            Assert.That(result.ShouldExit, Is.False);
        });
    }

    /// <summary>
    /// Verifies that Exit() creates a result that signals exit.
    /// </summary>
    [Test]
    public void Exit_CreatesExitResult()
    {
        var result = CommandResult.Exit();

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.Null);
            Assert.That(result.ShouldExit, Is.True);
        });
    }

    /// <summary>
    /// Verifies that Exit(message) creates a result that signals exit with message.
    /// </summary>
    [Test]
    public void ExitWithMessage_CreatesExitResultWithMessage()
    {
        var result = CommandResult.Exit("Goodbye!");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Goodbye!"));
            Assert.That(result.ShouldExit, Is.True);
        });
    }
}