// <copyright file="SlotIOHandlersTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

/// <summary>
/// Unit tests for the <see cref="SlotIOHandlers"/> class.
/// </summary>
[TestFixture]
public class SlotIOHandlersTests
{
    /// <summary>
    /// Verifies that ReadHandlers array has 16 elements.
    /// </summary>
    [Test]
    public void ReadHandlers_NewInstance_Has16Elements()
    {
        var handlers = new SlotIOHandlers();

        Assert.That(handlers.ReadHandlers, Has.Length.EqualTo(16));
    }

    /// <summary>
    /// Verifies that WriteHandlers array has 16 elements.
    /// </summary>
    [Test]
    public void WriteHandlers_NewInstance_Has16Elements()
    {
        var handlers = new SlotIOHandlers();

        Assert.That(handlers.WriteHandlers, Has.Length.EqualTo(16));
    }

    /// <summary>
    /// Verifies that all handlers are null by default.
    /// </summary>
    [Test]
    public void Handlers_NewInstance_AllNull()
    {
        var handlers = new SlotIOHandlers();

        Assert.Multiple(() =>
        {
            for (int i = 0; i < 16; i++)
            {
                Assert.That(handlers.ReadHandlers[i], Is.Null, $"ReadHandlers[{i}]");
                Assert.That(handlers.WriteHandlers[i], Is.Null, $"WriteHandlers[{i}]");
            }
        });
    }

    /// <summary>
    /// Verifies that Set correctly sets read handler.
    /// </summary>
    [Test]
    public void Set_ReadHandler_SetsCorrectly()
    {
        var handlers = new SlotIOHandlers();
        SoftSwitchReadHandler readHandler = (offset, in ctx) => 0x42;

        handlers.Set(0x00, readHandler, null);

        Assert.That(handlers.ReadHandlers[0x00], Is.SameAs(readHandler));
    }

    /// <summary>
    /// Verifies that Set correctly sets write handler.
    /// </summary>
    [Test]
    public void Set_WriteHandler_SetsCorrectly()
    {
        var handlers = new SlotIOHandlers();
        SoftSwitchWriteHandler writeHandler = (offset, value, in ctx) => { };

        handlers.Set(0x00, null, writeHandler);

        Assert.That(handlers.WriteHandlers[0x00], Is.SameAs(writeHandler));
    }

    /// <summary>
    /// Verifies that Set correctly sets both handlers.
    /// </summary>
    [Test]
    public void Set_BothHandlers_SetsBoth()
    {
        var handlers = new SlotIOHandlers();
        SoftSwitchReadHandler readHandler = (offset, in ctx) => 0x42;
        SoftSwitchWriteHandler writeHandler = (offset, value, in ctx) => { };

        handlers.Set(0x05, readHandler, writeHandler);

        Assert.Multiple(() =>
        {
            Assert.That(handlers.ReadHandlers[0x05], Is.SameAs(readHandler));
            Assert.That(handlers.WriteHandlers[0x05], Is.SameAs(writeHandler));
        });
    }

    /// <summary>
    /// Verifies that Set works for all valid offsets.
    /// </summary>
    [Test]
    public void Set_AllValidOffsets_Succeeds()
    {
        var handlers = new SlotIOHandlers();
        SoftSwitchReadHandler readHandler = (offset, in ctx) => offset;

        Assert.Multiple(() =>
        {
            for (byte i = 0; i < 16; i++)
            {
                Assert.DoesNotThrow(() => handlers.Set(i, readHandler, null));
            }
        });
    }

    /// <summary>
    /// Verifies that Set throws for offset 0x10 (out of range).
    /// </summary>
    [Test]
    public void Set_Offset16_ThrowsArgumentOutOfRangeException()
    {
        var handlers = new SlotIOHandlers();
        SoftSwitchReadHandler readHandler = (offset, in ctx) => 0;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            handlers.Set(0x10, readHandler, null));
    }

    /// <summary>
    /// Verifies that Set throws for high offset value.
    /// </summary>
    [Test]
    public void Set_HighOffset_ThrowsArgumentOutOfRangeException()
    {
        var handlers = new SlotIOHandlers();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            handlers.Set(0xFF, null, null));
    }

    /// <summary>
    /// Verifies that Set can clear handlers by setting null.
    /// </summary>
    [Test]
    public void Set_NullHandlers_ClearsHandlers()
    {
        var handlers = new SlotIOHandlers();
        handlers.Set(0x00, (offset, in ctx) => 0x42, (offset, value, in ctx) => { });

        handlers.Set(0x00, null, null);

        Assert.Multiple(() =>
        {
            Assert.That(handlers.ReadHandlers[0x00], Is.Null);
            Assert.That(handlers.WriteHandlers[0x00], Is.Null);
        });
    }

    /// <summary>
    /// Verifies that Set at one offset does not affect other offsets.
    /// </summary>
    [Test]
    public void Set_OneOffset_DoesNotAffectOthers()
    {
        var handlers = new SlotIOHandlers();
        SoftSwitchReadHandler readHandler = (offset, in ctx) => 0x42;

        handlers.Set(0x05, readHandler, null);

        Assert.Multiple(() =>
        {
            for (int i = 0; i < 16; i++)
            {
                if (i == 0x05)
                {
                    Assert.That(handlers.ReadHandlers[i], Is.SameAs(readHandler));
                }
                else
                {
                    Assert.That(handlers.ReadHandlers[i], Is.Null, $"Offset {i}");
                }
            }
        });
    }

    /// <summary>
    /// Verifies that handlers can be overwritten.
    /// </summary>
    [Test]
    public void Set_Overwrite_ReplacesHandler()
    {
        var handlers = new SlotIOHandlers();
        SoftSwitchReadHandler handler1 = (offset, in ctx) => 0x11;
        SoftSwitchReadHandler handler2 = (offset, in ctx) => 0x22;

        handlers.Set(0x00, handler1, null);
        handlers.Set(0x00, handler2, null);

        Assert.That(handlers.ReadHandlers[0x00], Is.SameAs(handler2));
    }
}