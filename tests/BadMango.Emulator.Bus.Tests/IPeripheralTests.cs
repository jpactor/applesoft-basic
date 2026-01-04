// <copyright file="IPeripheralTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

using Interfaces;

/// <summary>
/// Unit tests for the <see cref="IPeripheral"/> interface contract.
/// </summary>
[TestFixture]
public class IPeripheralTests
{
    /// <summary>
    /// Verifies that IPeripheral interface inherits from IScheduledDevice.
    /// </summary>
    [Test]
    public void Interface_InheritsFromIScheduledDevice()
    {
        Assert.That(typeof(IScheduledDevice).IsAssignableFrom(typeof(IPeripheral)), Is.True);
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines Name property from IScheduledDevice.
    /// </summary>
    [Test]
    public void Interface_HasNameProperty()
    {
        // Name is inherited from IScheduledDevice, so we check the base interface
        var baseProperty = typeof(IScheduledDevice).GetProperty(nameof(IScheduledDevice.Name));
        Assert.That(baseProperty, Is.Not.Null);
        Assert.That(baseProperty.PropertyType, Is.EqualTo(typeof(string)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines DeviceType property.
    /// </summary>
    [Test]
    public void Interface_HasDeviceTypeProperty()
    {
        var property = typeof(IPeripheral).GetProperty(nameof(IPeripheral.DeviceType));
        Assert.That(property, Is.Not.Null);
        Assert.That(property.PropertyType, Is.EqualTo(typeof(string)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines SlotNumber property with getter and setter.
    /// </summary>
    [Test]
    public void Interface_HasSlotNumberPropertyWithGetAndSet()
    {
        var property = typeof(IPeripheral).GetProperty(nameof(IPeripheral.SlotNumber));
        Assert.That(property, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(property.PropertyType, Is.EqualTo(typeof(int)));
            Assert.That(property.CanRead, Is.True);
            Assert.That(property.CanWrite, Is.True);
        });
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines IOHandlers property.
    /// </summary>
    [Test]
    public void Interface_HasIOHandlersProperty()
    {
        var property = typeof(IPeripheral).GetProperty(nameof(IPeripheral.IOHandlers));
        Assert.That(property, Is.Not.Null);
        Assert.That(property.PropertyType, Is.EqualTo(typeof(SlotIOHandlers)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines ROMRegion property.
    /// </summary>
    /// <remarks>
    /// Note: At runtime, nullable reference types (IBusTarget?) are the same CLR type as IBusTarget.
    /// </remarks>
    [Test]
    public void Interface_HasROMRegionProperty()
    {
        var property = typeof(IPeripheral).GetProperty(nameof(IPeripheral.ROMRegion));
        Assert.That(property, Is.Not.Null);
        Assert.That(property.PropertyType, Is.EqualTo(typeof(IBusTarget)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines ExpansionROMRegion property.
    /// </summary>
    /// <remarks>
    /// Note: At runtime, nullable reference types (IBusTarget?) are the same CLR type as IBusTarget.
    /// </remarks>
    [Test]
    public void Interface_HasExpansionROMRegionProperty()
    {
        var property = typeof(IPeripheral).GetProperty(nameof(IPeripheral.ExpansionROMRegion));
        Assert.That(property, Is.Not.Null);
        Assert.That(property.PropertyType, Is.EqualTo(typeof(IBusTarget)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines OnExpansionROMSelected method.
    /// </summary>
    [Test]
    public void Interface_HasOnExpansionROMSelectedMethod()
    {
        var method = typeof(IPeripheral).GetMethod(nameof(IPeripheral.OnExpansionROMSelected));
        Assert.That(method, Is.Not.Null);
        Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines OnExpansionROMDeselected method.
    /// </summary>
    [Test]
    public void Interface_HasOnExpansionROMDeselectedMethod()
    {
        var method = typeof(IPeripheral).GetMethod(nameof(IPeripheral.OnExpansionROMDeselected));
        Assert.That(method, Is.Not.Null);
        Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines Reset method.
    /// </summary>
    [Test]
    public void Interface_HasResetMethod()
    {
        var method = typeof(IPeripheral).GetMethod(nameof(IPeripheral.Reset));
        Assert.That(method, Is.Not.Null);
        Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
    }

    /// <summary>
    /// Verifies that IPeripheral interface defines Initialize method from IScheduledDevice.
    /// </summary>
    [Test]
    public void Interface_HasInitializeMethod()
    {
        var method = typeof(IScheduledDevice).GetMethod(nameof(IScheduledDevice.Initialize));
        Assert.That(method, Is.Not.Null);
        Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
    }
}