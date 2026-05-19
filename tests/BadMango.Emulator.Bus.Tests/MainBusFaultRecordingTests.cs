// <copyright file="MainBusFaultRecordingTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

using BadMango.Emulator.Bus.Interfaces;

/// <summary>
/// Tests for the fault-recording behavior that <see cref="MainBus"/> performs
/// when constructed with an <see cref="IBusFaultRecorder"/>.
/// </summary>
[TestFixture]
public class MainBusFaultRecordingTests
{
    /// <summary>
    /// Verifies that the bus exposes the supplied recorder through
    /// <see cref="IMemoryBus.FaultRing"/> when the recorder also implements
    /// <see cref="IBusFaultRing"/>.
    /// </summary>
    [Test]
    public void Constructor_ExposesRingWhenRecorderIsAlsoARing()
    {
        var ring = new BusFaultRing();
        var bus = new MainBus(addressSpaceBits: 16, faultRecorder: ring);

        Assert.That(bus.FaultRing, Is.SameAs(ring));
    }

    /// <summary>
    /// Verifies that <see cref="IMemoryBus.FaultRing"/> is <see langword="null"/>
    /// when no recorder is supplied (the default).
    /// </summary>
    [Test]
    public void Constructor_NullRingWhenRecorderOmitted()
    {
        var bus = new MainBus();

        Assert.That(bus.FaultRing, Is.Null);
    }

    /// <summary>
    /// Verifies that <c>TryRead8</c> on an unmapped address pushes the fault
    /// into the configured recorder.
    /// </summary>
    [Test]
    public void TryRead8_UnmappedAddress_RecordsFault()
    {
        var ring = new BusFaultRing();
        var bus = new MainBus(addressSpaceBits: 16, faultRecorder: ring);

        var access = CreateAccess(0x5000, AccessIntent.DataRead);
        var result = bus.TryRead8(access);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(ring.Count, Is.EqualTo(1));
            Assert.That(ring.Last, Is.Not.Null);
            Assert.That(ring.Last!.Value.Kind, Is.EqualTo(FaultKind.Unmapped));
            Assert.That(ring.Last.Value.Address, Is.EqualTo(0x5000u));
        });
    }

    /// <summary>
    /// Verifies that <c>TryWrite8</c> on an unmapped address pushes the fault
    /// into the configured recorder.
    /// </summary>
    [Test]
    public void TryWrite8_UnmappedAddress_RecordsFault()
    {
        var ring = new BusFaultRing();
        var bus = new MainBus(addressSpaceBits: 16, faultRecorder: ring);

        var access = CreateAccess(0x6000, AccessIntent.DataWrite);
        var result = bus.TryWrite8(access, 0x42);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(ring.Count, Is.EqualTo(1));
            Assert.That(ring.Last!.Value.Kind, Is.EqualTo(FaultKind.Unmapped));
            Assert.That(ring.Last.Value.Address, Is.EqualTo(0x6000u));
        });
    }

    /// <summary>
    /// Verifies that a 17-bit overflow address (which arises when a buggy
    /// addressing-mode implementation forgets to wrap base+index to 16 bits)
    /// is reported as a recorded Unmapped fault rather than silently
    /// throwing or wrapping. This is the regression that caused ProDOS to
    /// halt on <c>LDA $FF48,Y</c> with Y=$FE.
    /// </summary>
    [Test]
    public void TryRead8_AddressBeyondPageTable_RecordsUnmappedFault()
    {
        var ring = new BusFaultRing();
        var bus = new MainBus(addressSpaceBits: 16, faultRecorder: ring);

        var access = CreateAccess(0x10046u, AccessIntent.DataRead);
        var result = bus.TryRead8(access);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Fault.Kind, Is.EqualTo(FaultKind.Unmapped));
            Assert.That(ring.Count, Is.EqualTo(1));
            Assert.That(ring.Last!.Value.Address, Is.EqualTo(0x10046u));
        });
    }

    private static BusAccess CreateAccess(Addr address, AccessIntent intent) =>
        new(
            Address: address,
            Value: 0,
            WidthBits: 8,
            Mode: BusAccessMode.Decomposed,
            EmulationFlag: true,
            Intent: intent,
            SourceId: 0,
            Cycle: 0,
            Flags: AccessFlags.None);
}