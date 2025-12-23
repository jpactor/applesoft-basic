// <copyright file="BusResultTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

/// <summary>
/// Unit tests for the <see cref="BusResult{T}"/> struct.
/// </summary>
[TestFixture]
public class BusResultTests
{
    /// <summary>
    /// Verifies that BusResult can be created with value and fault.
    /// </summary>
    [Test]
    public void BusResult_CanBeCreatedWithValueAndFault()
    {
        var fault = BusFault.Success();
        var result = new BusResult<byte>(0x42, fault);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo(0x42));
            Assert.That(result.Fault.Kind, Is.EqualTo(FaultKind.None));
        });
    }

    /// <summary>
    /// Verifies Ok returns true when fault kind is None.
    /// </summary>
    [Test]
    public void BusResult_Ok_TrueWhenFaultKindIsNone()
    {
        var result = BusResult<byte>.Success(0x42);
        Assert.That(result.Ok, Is.True);
    }

    /// <summary>
    /// Verifies Ok returns false when fault kind is not None.
    /// </summary>
    [Test]
    public void BusResult_Ok_FalseWhenFaultKindIsNotNone()
    {
        var access = CreateTestAccess();
        var fault = BusFault.Unmapped(in access);
        var result = BusResult<byte>.FromFault(fault);

        Assert.That(result.Ok, Is.False);
    }

    /// <summary>
    /// Verifies Failed returns true when fault kind is not None.
    /// </summary>
    [Test]
    public void BusResult_Failed_TrueWhenFaultKindIsNotNone()
    {
        var access = CreateTestAccess();
        var fault = BusFault.PermissionDenied(in access, 1, RegionTag.Ram);
        var result = BusResult<ushort>.FromFault(fault);

        Assert.That(result.Failed, Is.True);
    }

    /// <summary>
    /// Verifies Failed returns false when fault kind is None.
    /// </summary>
    [Test]
    public void BusResult_Failed_FalseWhenFaultKindIsNone()
    {
        var result = BusResult<uint>.Success(0x12345678);
        Assert.That(result.Failed, Is.False);
    }

    /// <summary>
    /// Verifies Success factory creates successful result.
    /// </summary>
    [Test]
    public void BusResult_Success_CreatesSuccessfulResult()
    {
        var result = BusResult<byte>.Success(0xAB);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo(0xAB));
            Assert.That(result.Ok, Is.True);
            Assert.That(result.Fault.Kind, Is.EqualTo(FaultKind.None));
        });
    }

    /// <summary>
    /// Verifies Success with context creates successful result with context.
    /// </summary>
    [Test]
    public void BusResult_SuccessWithContext_HasCorrectValues()
    {
        var access = CreateTestAccess();
        var result = BusResult<byte>.Success(0xCD, in access, 5, RegionTag.Ram);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value, Is.EqualTo(0xCD));
            Assert.That(result.Ok, Is.True);
            Assert.That(result.Fault.DeviceId, Is.EqualTo(5));
            Assert.That(result.Fault.RegionTag, Is.EqualTo(RegionTag.Ram));
        });
    }

    /// <summary>
    /// Verifies FromFault factory creates faulted result.
    /// </summary>
    [Test]
    public void BusResult_FromFault_CreatesFaultedResult()
    {
        var access = CreateTestAccess(AccessIntent.InstructionFetch);
        var fault = BusFault.NoExecute(in access, 1, RegionTag.Rom);
        var result = BusResult<byte>.FromFault(fault);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Fault.Kind, Is.EqualTo(FaultKind.Nx));
        });
    }

    /// <summary>
    /// Verifies implicit conversion from BusFault works.
    /// </summary>
    [Test]
    public void BusResult_ImplicitConversionFromFault_Works()
    {
        var access = CreateTestAccess();
        var fault = BusFault.Unmapped(in access);
        BusResult<byte> result = fault;

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.Fault.Kind, Is.EqualTo(FaultKind.Unmapped));
        });
    }

    /// <summary>
    /// Verifies BusResult works with different value types.
    /// </summary>
    [Test]
    public void BusResult_WorksWithDifferentTypes()
    {
        var byteResult = BusResult<byte>.Success(0xFF);
        var wordResult = BusResult<ushort>.Success(0xFFFF);
        var dwordResult = BusResult<uint>.Success(0xFFFFFFFFu);

        Assert.Multiple(() =>
        {
            Assert.That(byteResult.Value, Is.EqualTo(0xFF));
            Assert.That(wordResult.Value, Is.EqualTo(0xFFFF));
            Assert.That(dwordResult.Value, Is.EqualTo(0xFFFFFFFFu));
        });
    }

    private static BusAccess CreateTestAccess(AccessIntent intent = AccessIntent.DataRead)
    {
        return new BusAccess(
            Address: 0x1000u,
            Value: 0,
            WidthBits: 8,
            Mode: CpuMode.Native,
            EmulationFlag: false,
            Intent: intent,
            SourceId: 1,
            Cycle: 100ul,
            Flags: AccessFlags.None);
    }
}