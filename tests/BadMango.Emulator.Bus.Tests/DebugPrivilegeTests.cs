// <copyright file="DebugPrivilegeTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

/// <summary>
/// Unit tests for the <see cref="DebugPrivilege"/> class.
/// </summary>
[TestFixture]
public class DebugPrivilegeTests
{
    /// <summary>
    /// Verifies that DebugPrivilege can be instantiated from test assembly.
    /// </summary>
    [Test]
    public void DebugPrivilege_CanBeInstantiated()
    {
        var privilege = new DebugPrivilege();

        Assert.That(privilege, Is.Not.Null);
    }

    /// <summary>
    /// Verifies that DebugPrivilege can be used with WriteBytePhysical.
    /// </summary>
    [Test]
    public void DebugPrivilege_CanBeUsedWithWriteBytePhysical()
    {
        var memory = new PhysicalMemory(16, "Test");
        var privilege = new DebugPrivilege();

        memory.WriteBytePhysical(privilege, 0, 0xAA);

        Assert.That(memory.AsReadOnlySpan()[0], Is.EqualTo(0xAA));
    }

    /// <summary>
    /// Verifies that DebugPrivilege can be used with WritePhysical.
    /// </summary>
    [Test]
    public void DebugPrivilege_CanBeUsedWithWritePhysical()
    {
        var memory = new PhysicalMemory(16, "Test");
        var privilege = new DebugPrivilege();
        var data = new byte[] { 0x11, 0x22, 0x33 };

        memory.WritePhysical(privilege, 0, data);

        Assert.That(memory.AsReadOnlySpan()[0], Is.EqualTo(0x11));
        Assert.That(memory.AsReadOnlySpan()[1], Is.EqualTo(0x22));
        Assert.That(memory.AsReadOnlySpan()[2], Is.EqualTo(0x33));
    }
}