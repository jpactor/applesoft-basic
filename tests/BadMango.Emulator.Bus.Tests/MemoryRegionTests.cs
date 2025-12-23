// <copyright file="MemoryRegionTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

/// <summary>
/// Unit tests for the <see cref="MemoryRegion"/> class.
/// </summary>
[TestFixture]
public class MemoryRegionTests
{
    /// <summary>
    /// Verifies that a RAM region can be created with factory method.
    /// </summary>
    [Test]
    public void CreateRam_CreatesValidRegion()
    {
        var physicalMemory = new PhysicalMemory(4096, "Test RAM");

        var region = MemoryRegion.CreateRam("ram0", "Main RAM", 0x00040000, physicalMemory);

        Assert.Multiple(() =>
        {
            Assert.That(region.Id, Is.EqualTo("ram0"));
            Assert.That(region.Name, Is.EqualTo("Main RAM"));
            Assert.That(region.PreferredBase, Is.EqualTo(0x00040000U));
            Assert.That(region.Size, Is.EqualTo(4096U));
            Assert.That(region.Tag, Is.EqualTo(RegionTag.Ram));
            Assert.That(region.PhysicalMemory, Is.SameAs(physicalMemory));
            Assert.That(region.Target, Is.Not.Null);
            Assert.That(region.DefaultPermissions, Is.EqualTo(PagePerms.Read | PagePerms.Write | PagePerms.Execute));
        });
    }

    /// <summary>
    /// Verifies that a ROM region can be created with factory method.
    /// </summary>
    [Test]
    public void CreateRom_CreatesValidRegion()
    {
        var physicalMemory = new PhysicalMemory(8192, "Boot ROM");

        var region = MemoryRegion.CreateRom("rom0", "Boot ROM", 0x00000000, physicalMemory);

        Assert.Multiple(() =>
        {
            Assert.That(region.Id, Is.EqualTo("rom0"));
            Assert.That(region.Name, Is.EqualTo("Boot ROM"));
            Assert.That(region.PreferredBase, Is.EqualTo(0x00000000U));
            Assert.That(region.Size, Is.EqualTo(8192U));
            Assert.That(region.Tag, Is.EqualTo(RegionTag.Rom));
            Assert.That(region.PhysicalMemory, Is.SameAs(physicalMemory));
            Assert.That(region.IsRelocatable, Is.False);
            Assert.That(region.Priority, Is.EqualTo(0));
            Assert.That(region.DefaultPermissions, Is.EqualTo(PagePerms.Read | PagePerms.Execute));
        });
    }

    /// <summary>
    /// Verifies that constructor validates ID.
    /// </summary>
    [Test]
    public void Constructor_ThrowsForNullId()
    {
        var memory = new PhysicalMemory(4096, "Test");
        var target = new RamTarget(memory.Slice(0, 4096));

        Assert.Throws<ArgumentNullException>(() => new MemoryRegion(
            null!,
            "Test",
            0,
            target,
            4096,
            RegionTag.Ram,
            PagePerms.Read,
            TargetCaps.SupportsPeek));
    }

    /// <summary>
    /// Verifies that constructor validates Name.
    /// </summary>
    [Test]
    public void Constructor_ThrowsForNullName()
    {
        var memory = new PhysicalMemory(4096, "Test");
        var target = new RamTarget(memory.Slice(0, 4096));

        Assert.Throws<ArgumentNullException>(() => new MemoryRegion(
            "test",
            null!,
            0,
            target,
            4096,
            RegionTag.Ram,
            PagePerms.Read,
            TargetCaps.SupportsPeek));
    }

    /// <summary>
    /// Verifies that constructor validates target.
    /// </summary>
    [Test]
    public void Constructor_ThrowsForNullTarget()
    {
        Assert.Throws<ArgumentNullException>(() => new MemoryRegion(
            "test",
            "Test",
            0,
            null!,
            4096,
            RegionTag.Ram,
            PagePerms.Read,
            TargetCaps.SupportsPeek));
    }

    /// <summary>
    /// Verifies that constructor validates size.
    /// </summary>
    [Test]
    public void Constructor_ThrowsForZeroSize()
    {
        var memory = new PhysicalMemory(4096, "Test");
        var target = new RamTarget(memory.Slice(0, 4096));

        Assert.Throws<ArgumentOutOfRangeException>(() => new MemoryRegion(
            "test",
            "Test",
            0,
            target,
            0,
            RegionTag.Ram,
            PagePerms.Read,
            TargetCaps.SupportsPeek));
    }

    /// <summary>
    /// Verifies that region properties are correctly set.
    /// </summary>
    [Test]
    public void Properties_AreCorrectlySet()
    {
        var memory = new PhysicalMemory(4096, "Test");
        var target = new RamTarget(memory.Slice(0, 4096));

        var region = new MemoryRegion(
            id: "custom",
            name: "Custom Region",
            preferredBase: 0x10000,
            target: target,
            size: 4096,
            tag: RegionTag.Shadow,
            defaultPermissions: PagePerms.Read | PagePerms.Write,
            capabilities: TargetCaps.SupportsPeek | TargetCaps.SupportsWide,
            physicalMemory: memory,
            isRelocatable: false,
            supportsOverlay: true,
            priority: 50);

        Assert.Multiple(() =>
        {
            Assert.That(region.Id, Is.EqualTo("custom"));
            Assert.That(region.Name, Is.EqualTo("Custom Region"));
            Assert.That(region.PreferredBase, Is.EqualTo(0x10000U));
            Assert.That(region.Size, Is.EqualTo(4096U));
            Assert.That(region.Tag, Is.EqualTo(RegionTag.Shadow));
            Assert.That(region.DefaultPermissions, Is.EqualTo(PagePerms.Read | PagePerms.Write));
            Assert.That(region.Capabilities, Is.EqualTo(TargetCaps.SupportsPeek | TargetCaps.SupportsWide));
            Assert.That(region.PhysicalMemory, Is.SameAs(memory));
            Assert.That(region.IsRelocatable, Is.False);
            Assert.That(region.SupportsOverlay, Is.True);
            Assert.That(region.Priority, Is.EqualTo(50));
        });
    }
}