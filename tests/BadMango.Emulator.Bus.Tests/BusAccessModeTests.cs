// <copyright file="BusAccessModeTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

/// <summary>
/// Unit tests for the <see cref="BusAccessMode"/> enum.
/// </summary>
[TestFixture]
public class BusAccessModeTests
{
    /// <summary>
    /// Verifies that BusAccessMode.Atomic has value 0.
    /// </summary>
    [Test]
    public void BusAccessMode_Atomic_HasValueZero()
    {
        Assert.That((int)BusAccessMode.Atomic, Is.EqualTo(0));
    }

    /// <summary>
    /// Verifies that BusAccessMode.Decomposed has value 1.
    /// </summary>
    [Test]
    public void BusAccessMode_Decomposed_HasValueOne()
    {
        Assert.That((int)BusAccessMode.Decomposed, Is.EqualTo(1));
    }

    /// <summary>
    /// Verifies that the enum has exactly two values.
    /// </summary>
    [Test]
    public void BusAccessMode_HasTwoValues()
    {
        var values = Enum.GetValues<BusAccessMode>();
        Assert.That(values, Has.Length.EqualTo(2));
    }
}