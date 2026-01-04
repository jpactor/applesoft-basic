// <copyright file="AuxiliaryMemoryControllerTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Tests;

using BadMango.Emulator.Bus.Interfaces;
using Moq;

/// <summary>
/// Unit tests for the <see cref="AuxiliaryMemoryController"/> class.
/// </summary>
[TestFixture]
public class AuxiliaryMemoryControllerTests
{
    private const int PageSize = 4096;

    /// <summary>
    /// Verifies that the controller is created with the correct name.
    /// </summary>
    [Test]
    public void Constructor_CreatesWithCorrectName()
    {
        var controller = new AuxiliaryMemoryController();

        Assert.That(controller.Name, Is.EqualTo("Auxiliary Memory Controller"));
    }

    /// <summary>
    /// Verifies that Initialize throws when context is null.
    /// </summary>
    [Test]
    public void Initialize_NullContext_ThrowsArgumentNullException()
    {
        var controller = new AuxiliaryMemoryController();

        Assert.Throws<ArgumentNullException>(() => controller.Initialize(null!));
    }

    /// <summary>
    /// Verifies that Initialize sets all switches to their disabled state.
    /// </summary>
    [Test]
    public void Initialize_SetsAllSwitchesToDisabled()
    {
        var (controller, _, _) = CreateInitializedController();

        Assert.Multiple(() =>
        {
            Assert.That(controller.Is80StoreEnabled, Is.False, "80STORE should be disabled initially");
            Assert.That(controller.IsRamRdEnabled, Is.False, "RAMRD should be disabled initially");
            Assert.That(controller.IsRamWrtEnabled, Is.False, "RAMWRT should be disabled initially");
            Assert.That(controller.IsAltZpEnabled, Is.False, "ALTZP should be disabled initially");
            Assert.That(controller.IsPage2Selected, Is.False, "PAGE2 should be disabled initially");
            Assert.That(controller.IsHiResEnabled, Is.False, "HIRES should be disabled initially");
        });
    }

    /// <summary>
    /// Verifies that all auxiliary memory layers start inactive.
    /// </summary>
    [Test]
    public void Initialize_AllLayersStartInactive()
    {
        var (_, bus, _) = CreateInitializedController();

        Assert.Multiple(() =>
        {
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameZeroPage), Is.False, "AUX_ZP should be inactive");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameStack), Is.False, "AUX_STACK should be inactive");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameTextPage), Is.False, "AUX_TEXT should be inactive");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.False, "AUX_HIRES1 should be inactive");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.False, "AUX_HIRES2 should be inactive");
        });
    }

    /// <summary>
    /// Verifies that RegisterHandlers throws when dispatcher is null.
    /// </summary>
    [Test]
    public void RegisterHandlers_NullDispatcher_ThrowsArgumentNullException()
    {
        var controller = new AuxiliaryMemoryController();

        Assert.Throws<ArgumentNullException>(() => controller.RegisterHandlers(null!));
    }

    // ─── ALTZP Tests ($C008/$C009) ──────────────────────────────────────────────

    /// <summary>
    /// Verifies that SETSTDZP ($C008) disables ALTZP and deactivates zero page/stack layers.
    /// </summary>
    [Test]
    public void SetStdZp_DisablesAltZp_DeactivatesZeroPageAndStackLayers()
    {
        var (controller, bus, dispatcher) = CreateInitializedController();

        // First enable ALTZP
        SimulateWrite(dispatcher, 0x09, 0x00); // SETALTZP
        Assert.That(controller.IsAltZpEnabled, Is.True, "ALTZP should be enabled");
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameZeroPage), Is.True, "AUX_ZP should be active");
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameStack), Is.True, "AUX_STACK should be active");

        // Now disable via SETSTDZP
        SimulateWrite(dispatcher, 0x08, 0x00); // SETSTDZP
        Assert.Multiple(() =>
        {
            Assert.That(controller.IsAltZpEnabled, Is.False, "ALTZP should be disabled");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameZeroPage), Is.False, "AUX_ZP should be inactive");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameStack), Is.False, "AUX_STACK should be inactive");
        });
    }

    /// <summary>
    /// Verifies that SETALTZP ($C009) enables ALTZP and activates zero page/stack layers.
    /// </summary>
    [Test]
    public void SetAltZp_EnablesAltZp_ActivatesZeroPageAndStackLayers()
    {
        var (controller, bus, dispatcher) = CreateInitializedController();

        SimulateWrite(dispatcher, 0x09, 0x00); // SETALTZP

        Assert.Multiple(() =>
        {
            Assert.That(controller.IsAltZpEnabled, Is.True, "ALTZP should be enabled");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameZeroPage), Is.True, "AUX_ZP should be active");
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameStack), Is.True, "AUX_STACK should be active");
        });
    }

    // ─── 80STORE + PAGE2 Tests ($C000/$C001, $C054/$C055) ───────────────────────

    /// <summary>
    /// Verifies that 80STOREOFF ($C000) disables 80STORE mode.
    /// </summary>
    [Test]
    public void Store80Off_Disables80StoreMode()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Enable first
        SimulateWrite(dispatcher, 0x01, 0x00); // 80STOREON
        Assert.That(controller.Is80StoreEnabled, Is.True);

        // Now disable
        SimulateWrite(dispatcher, 0x00, 0x00); // 80STOREOFF
        Assert.That(controller.Is80StoreEnabled, Is.False);
    }

    /// <summary>
    /// Verifies that 80STOREON ($C001) enables 80STORE mode.
    /// </summary>
    [Test]
    public void Store80On_Enables80StoreMode()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        SimulateWrite(dispatcher, 0x01, 0x00); // 80STOREON

        Assert.That(controller.Is80StoreEnabled, Is.True);
    }

    /// <summary>
    /// Verifies that text page layer activates only when both 80STORE and PAGE2 are enabled.
    /// </summary>
    [Test]
    public void TextPageLayer_ActivatesOnlyWhen80StoreAndPage2Enabled()
    {
        var (controller, bus, dispatcher) = CreateInitializedController();

        // Initially: 80STORE off, PAGE2 off - text layer should be inactive
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameTextPage), Is.False, "Text layer should be inactive initially");

        // Enable PAGE2 only - text layer should still be inactive
        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameTextPage), Is.False, "Text layer should be inactive with only PAGE2");

        // Disable PAGE2, enable 80STORE - text layer should still be inactive
        SimulateWrite(dispatcher, 0x54, 0x00); // PAGE1
        SimulateWrite(dispatcher, 0x01, 0x00); // 80STOREON
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameTextPage), Is.False, "Text layer should be inactive with only 80STORE");

        // Enable PAGE2 with 80STORE - text layer should be active
        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameTextPage), Is.True, "Text layer should be active with 80STORE + PAGE2");

        // Disable 80STORE - text layer should be inactive
        SimulateWrite(dispatcher, 0x00, 0x00); // 80STOREOFF
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameTextPage), Is.False, "Text layer should be inactive after disabling 80STORE");
    }

    /// <summary>
    /// Verifies that PAGE1 ($C054) clears PAGE2 selection.
    /// </summary>
    [Test]
    public void Page1_ClearsPage2Selection()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Enable PAGE2
        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2
        Assert.That(controller.IsPage2Selected, Is.True);

        // Select PAGE1
        SimulateWrite(dispatcher, 0x54, 0x00); // PAGE1
        Assert.That(controller.IsPage2Selected, Is.False);
    }

    /// <summary>
    /// Verifies that PAGE2 ($C055) sets PAGE2 selection.
    /// </summary>
    [Test]
    public void Page2_SetsPage2Selection()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2

        Assert.That(controller.IsPage2Selected, Is.True);
    }

    /// <summary>
    /// Verifies that PAGE1/PAGE2 reads also affect the switch state.
    /// </summary>
    [Test]
    public void PageSwitches_ReadAccessAlsoAffectsState()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Read PAGE2 should enable it
        SimulateRead(dispatcher, 0x55); // PAGE2
        Assert.That(controller.IsPage2Selected, Is.True, "PAGE2 read should enable it");

        // Read PAGE1 should disable it
        SimulateRead(dispatcher, 0x54); // PAGE1
        Assert.That(controller.IsPage2Selected, Is.False, "PAGE1 read should disable PAGE2");
    }

    // ─── 80STORE + HIRES + PAGE2 Tests ($C056/$C057) ────────────────────────────

    /// <summary>
    /// Verifies that LORES ($C056) disables HIRES mode.
    /// </summary>
    [Test]
    public void LoRes_DisablesHiResMode()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Enable HIRES first
        SimulateWrite(dispatcher, 0x57, 0x00); // HIRES
        Assert.That(controller.IsHiResEnabled, Is.True);

        // Now disable
        SimulateWrite(dispatcher, 0x56, 0x00); // LORES
        Assert.That(controller.IsHiResEnabled, Is.False);
    }

    /// <summary>
    /// Verifies that HIRES ($C057) enables HIRES mode.
    /// </summary>
    [Test]
    public void HiRes_EnablesHiResMode()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        SimulateWrite(dispatcher, 0x57, 0x00); // HIRES

        Assert.That(controller.IsHiResEnabled, Is.True);
    }

    /// <summary>
    /// Verifies that hi-res page layers activate only when 80STORE + HIRES + PAGE2 are all enabled.
    /// </summary>
    [Test]
    public void HiResPageLayers_ActivateOnlyWhen80StoreHiResAndPage2Enabled()
    {
        var (controller, bus, dispatcher) = CreateInitializedController();

        // Initially all off - hi-res layers should be inactive
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.False);
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.False);

        // Enable only 80STORE + HIRES (no PAGE2) - should still be inactive
        SimulateWrite(dispatcher, 0x01, 0x00); // 80STOREON
        SimulateWrite(dispatcher, 0x57, 0x00); // HIRES
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.False, "Should be inactive without PAGE2");
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.False, "Should be inactive without PAGE2");

        // Enable PAGE2 - now should be active
        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.True, "Should be active with all three enabled");
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.True, "Should be active with all three enabled");

        // Disable HIRES - should be inactive
        SimulateWrite(dispatcher, 0x56, 0x00); // LORES
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.False, "Should be inactive without HIRES");
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.False, "Should be inactive without HIRES");

        // Re-enable HIRES - should be active again
        SimulateWrite(dispatcher, 0x57, 0x00); // HIRES
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.True);
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.True);

        // Disable 80STORE - should be inactive
        SimulateWrite(dispatcher, 0x00, 0x00); // 80STOREOFF
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.False, "Should be inactive without 80STORE");
        Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.False, "Should be inactive without 80STORE");
    }

    /// <summary>
    /// Verifies that HIRES/LORES reads also affect the switch state.
    /// </summary>
    [Test]
    public void HiresSwitches_ReadAccessAlsoAffectsState()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Read HIRES should enable it
        SimulateRead(dispatcher, 0x57); // HIRES
        Assert.That(controller.IsHiResEnabled, Is.True, "HIRES read should enable it");

        // Read LORES should disable it
        SimulateRead(dispatcher, 0x56); // LORES
        Assert.That(controller.IsHiResEnabled, Is.False, "LORES read should disable HIRES");
    }

    // ─── RAMRD/RAMWRT Tests ($C002-$C005) ───────────────────────────────────────

    /// <summary>
    /// Verifies that RDMAINRAM ($C002) disables RAMRD.
    /// </summary>
    [Test]
    public void RdMainRam_DisablesRamRd()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Enable first
        SimulateWrite(dispatcher, 0x03, 0x00); // RDCARDRAM
        Assert.That(controller.IsRamRdEnabled, Is.True);

        // Now disable
        SimulateWrite(dispatcher, 0x02, 0x00); // RDMAINRAM
        Assert.That(controller.IsRamRdEnabled, Is.False);
    }

    /// <summary>
    /// Verifies that RDCARDRAM ($C003) enables RAMRD.
    /// </summary>
    [Test]
    public void RdCardRam_EnablesRamRd()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        SimulateWrite(dispatcher, 0x03, 0x00); // RDCARDRAM

        Assert.That(controller.IsRamRdEnabled, Is.True);
    }

    /// <summary>
    /// Verifies that WRMAINRAM ($C004) disables RAMWRT.
    /// </summary>
    [Test]
    public void WrMainRam_DisablesRamWrt()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Enable first
        SimulateWrite(dispatcher, 0x05, 0x00); // WRCARDRAM
        Assert.That(controller.IsRamWrtEnabled, Is.True);

        // Now disable
        SimulateWrite(dispatcher, 0x04, 0x00); // WRMAINRAM
        Assert.That(controller.IsRamWrtEnabled, Is.False);
    }

    /// <summary>
    /// Verifies that WRCARDRAM ($C005) enables RAMWRT.
    /// </summary>
    [Test]
    public void WrCardRam_EnablesRamWrt()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        SimulateWrite(dispatcher, 0x05, 0x00); // WRCARDRAM

        Assert.That(controller.IsRamWrtEnabled, Is.True);
    }

    // ─── Reset Tests ────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that Reset restores all switches to their disabled state.
    /// </summary>
    [Test]
    public void Reset_RestoresPowerOnState()
    {
        var (controller, bus, dispatcher) = CreateInitializedController();

        // Enable everything
        SimulateWrite(dispatcher, 0x01, 0x00); // 80STOREON
        SimulateWrite(dispatcher, 0x03, 0x00); // RDCARDRAM
        SimulateWrite(dispatcher, 0x05, 0x00); // WRCARDRAM
        SimulateWrite(dispatcher, 0x09, 0x00); // SETALTZP
        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2
        SimulateWrite(dispatcher, 0x57, 0x00); // HIRES

        Assert.Multiple(() =>
        {
            Assert.That(controller.Is80StoreEnabled, Is.True);
            Assert.That(controller.IsRamRdEnabled, Is.True);
            Assert.That(controller.IsRamWrtEnabled, Is.True);
            Assert.That(controller.IsAltZpEnabled, Is.True);
            Assert.That(controller.IsPage2Selected, Is.True);
            Assert.That(controller.IsHiResEnabled, Is.True);
        });

        // Reset
        controller.Reset();

        Assert.Multiple(() =>
        {
            Assert.That(controller.Is80StoreEnabled, Is.False, "80STORE should be disabled after reset");
            Assert.That(controller.IsRamRdEnabled, Is.False, "RAMRD should be disabled after reset");
            Assert.That(controller.IsRamWrtEnabled, Is.False, "RAMWRT should be disabled after reset");
            Assert.That(controller.IsAltZpEnabled, Is.False, "ALTZP should be disabled after reset");
            Assert.That(controller.IsPage2Selected, Is.False, "PAGE2 should be disabled after reset");
            Assert.That(controller.IsHiResEnabled, Is.False, "HIRES should be disabled after reset");
        });

        // Verify all layers are inactive after reset
        Assert.Multiple(() =>
        {
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameZeroPage), Is.False);
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameStack), Is.False);
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameTextPage), Is.False);
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage1), Is.False);
            Assert.That(bus.IsLayerActive(AuxiliaryMemoryController.LayerNameHiResPage2), Is.False);
        });
    }

    // ─── Side-Effect-Free Access Tests ──────────────────────────────────────────

    /// <summary>
    /// Verifies that side-effect-free writes do not change state.
    /// </summary>
    [Test]
    public void SideEffectFreeWrite_DoesNotChangeState()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Initial state: all disabled
        Assert.That(controller.Is80StoreEnabled, Is.False);

        // Side-effect-free write should not change state
        SimulateSideEffectFreeWrite(dispatcher, 0x01, 0x00); // 80STOREON
        Assert.That(controller.Is80StoreEnabled, Is.False, "Side-effect-free write should not change state");
    }

    /// <summary>
    /// Verifies that side-effect-free reads do not change state.
    /// </summary>
    [Test]
    public void SideEffectFreeRead_DoesNotChangeState()
    {
        var (controller, _, dispatcher) = CreateInitializedController();

        // Initial state: PAGE2 disabled
        Assert.That(controller.IsPage2Selected, Is.False);

        // Side-effect-free read should not change state
        SimulateSideEffectFreeRead(dispatcher, 0x55); // PAGE2
        Assert.That(controller.IsPage2Selected, Is.False, "Side-effect-free read should not change state");
    }

    // ─── Layer Constant Tests ───────────────────────────────────────────────────

    /// <summary>
    /// Verifies that layer name constants have expected values.
    /// </summary>
    [Test]
    public void LayerNameConstants_HaveExpectedValues()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AuxiliaryMemoryController.LayerNameZeroPage, Is.EqualTo("AUX_ZP"));
            Assert.That(AuxiliaryMemoryController.LayerNameStack, Is.EqualTo("AUX_STACK"));
            Assert.That(AuxiliaryMemoryController.LayerNameTextPage, Is.EqualTo("AUX_TEXT"));
            Assert.That(AuxiliaryMemoryController.LayerNameHiResPage1, Is.EqualTo("AUX_HIRES1"));
            Assert.That(AuxiliaryMemoryController.LayerNameHiResPage2, Is.EqualTo("AUX_HIRES2"));
        });
    }

    /// <summary>
    /// Verifies that layer priority is set correctly.
    /// </summary>
    [Test]
    public void LayerPriority_IsSetCorrectly()
    {
        Assert.That(AuxiliaryMemoryController.LayerPriority, Is.EqualTo(10));
    }

    // ─── Integration Test ───────────────────────────────────────────────────────

    /// <summary>
    /// Verifies the complete auxiliary memory simulation with actual memory access.
    /// </summary>
    [Test]
    public void AuxiliaryMemorySimulation_EndToEnd()
    {
        var (controller, bus, dispatcher) = CreateInitializedController();

        // Initially, main memory should be visible at zero page
        var readAccessZp = CreateTestAccess(0x0042, AccessIntent.DataRead);
        Assert.That(bus.Read8(readAccessZp), Is.EqualTo(0x00), "Main zero page should be visible initially");

        // Write to main zero page
        var writeAccessZp = CreateTestAccess(0x0042, AccessIntent.DataWrite);
        bus.Write8(writeAccessZp, 0xAA);
        Assert.That(bus.Read8(readAccessZp), Is.EqualTo(0xAA), "Write to main zero page should work");

        // Enable ALTZP - auxiliary zero page should now be visible
        SimulateWrite(dispatcher, 0x09, 0x00); // SETALTZP
        Assert.That(bus.Read8(readAccessZp), Is.EqualTo(0x00), "Auxiliary zero page should be visible after SETALTZP");

        // Write to auxiliary zero page
        bus.Write8(writeAccessZp, 0xBB);
        Assert.That(bus.Read8(readAccessZp), Is.EqualTo(0xBB), "Write to auxiliary zero page should work");

        // Switch back to main - should see original value
        SimulateWrite(dispatcher, 0x08, 0x00); // SETSTDZP
        Assert.That(bus.Read8(readAccessZp), Is.EqualTo(0xAA), "Main zero page should retain original value");

        // Switch to auxiliary again - should see auxiliary value
        SimulateWrite(dispatcher, 0x09, 0x00); // SETALTZP
        Assert.That(bus.Read8(readAccessZp), Is.EqualTo(0xBB), "Auxiliary zero page should retain its value");
    }

    /// <summary>
    /// Verifies text page switching with 80STORE and PAGE2.
    /// </summary>
    [Test]
    public void TextPageSwitching_EndToEnd()
    {
        var (controller, bus, dispatcher) = CreateInitializedController();

        // Text page address
        var readAccessText = CreateTestAccess(0x0500, AccessIntent.DataRead);
        var writeAccessText = CreateTestAccess(0x0500, AccessIntent.DataWrite);

        // Write to main text page
        bus.Write8(writeAccessText, 0x41); // 'A'
        Assert.That(bus.Read8(readAccessText), Is.EqualTo(0x41), "Main text page write should work");

        // Enable 80STORE + PAGE2 - should see auxiliary text page
        SimulateWrite(dispatcher, 0x01, 0x00); // 80STOREON
        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2
        Assert.That(bus.Read8(readAccessText), Is.EqualTo(0x00), "Auxiliary text page should be visible");

        // Write to auxiliary text page
        bus.Write8(writeAccessText, 0x42); // 'B'
        Assert.That(bus.Read8(readAccessText), Is.EqualTo(0x42), "Auxiliary text page write should work");

        // Switch to PAGE1 - should see main text page
        SimulateWrite(dispatcher, 0x54, 0x00); // PAGE1
        Assert.That(bus.Read8(readAccessText), Is.EqualTo(0x41), "Main text page should be visible with PAGE1");

        // Disable 80STORE - PAGE2 should no longer affect text page layer
        SimulateWrite(dispatcher, 0x55, 0x00); // PAGE2
        SimulateWrite(dispatcher, 0x00, 0x00); // 80STOREOFF
        Assert.That(bus.Read8(readAccessText), Is.EqualTo(0x41), "Main text page should remain visible with 80STORE off");
    }

    // ─── Helper Methods ─────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a fully initialized auxiliary memory controller with required infrastructure.
    /// </summary>
    /// <returns>A tuple containing the initialized controller, the configured memory bus, and the I/O dispatcher.</returns>
    private static (AuxiliaryMemoryController Controller, MainBus Bus, IOPageDispatcher Dispatcher) CreateInitializedController()
    {
        var bus = new MainBus();
        var dispatcher = new IOPageDispatcher();

        // Create main RAM for the relevant regions
        var mainRam = new PhysicalMemory(0x10000, "MainRAM");
        var mainRamTarget = new RamTarget(mainRam.Slice(0, 0x10000));

        // Create auxiliary RAM for the overlay regions
        // Note: Layers work at page granularity (4KB), so targets must be sized to cover the full page
        // For testing, we create separate memory for each layer even though real aux memory would share

        // AUX_ZP targets page 0 ($0000-$0FFF)
        var auxZpMemory = new PhysicalMemory(0x1000, "AuxZP");
        var auxZpTarget = new RamTarget(auxZpMemory.Slice(0, 0x1000));

        // AUX_STACK also targets page 0 ($0000-$0FFF) - separate memory for testing
        var auxStackMemory = new PhysicalMemory(0x1000, "AuxStack");
        var auxStackTarget = new RamTarget(auxStackMemory.Slice(0, 0x1000));

        // AUX_TEXT also targets page 0 ($0000-$0FFF) - text page is $0400-$07FF within page 0
        var auxTextMemory = new PhysicalMemory(0x1000, "AuxText");
        var auxTextTarget = new RamTarget(auxTextMemory.Slice(0, 0x1000));

        var auxHires1Memory = new PhysicalMemory(0x2000, "AuxHiRes1");
        var auxHires1Target = new RamTarget(auxHires1Memory.Slice(0, 0x2000));

        var auxHires2Memory = new PhysicalMemory(0x2000, "AuxHiRes2");
        var auxHires2Target = new RamTarget(auxHires2Memory.Slice(0, 0x2000));

        // Map main RAM as base layer (pages 0-11, which is 0x0000-0xBFFF for 4KB pages)
        // Map just the first 12 pages (0x0000-0xBFFF) to main RAM
        bus.MapPageRange(0, 12, 1, RegionTag.Ram, PagePerms.All, TargetCaps.SupportsPeek | TargetCaps.SupportsPoke, mainRamTarget, 0);
        bus.SaveBaseMappingRange(0, 12);

        // Create auxiliary memory layers
        var auxZpLayer = bus.CreateLayer(AuxiliaryMemoryController.LayerNameZeroPage, AuxiliaryMemoryController.LayerPriority);
        var auxStackLayer = bus.CreateLayer(AuxiliaryMemoryController.LayerNameStack, AuxiliaryMemoryController.LayerPriority);
        var auxTextLayer = bus.CreateLayer(AuxiliaryMemoryController.LayerNameTextPage, AuxiliaryMemoryController.LayerPriority);
        var auxHires1Layer = bus.CreateLayer(AuxiliaryMemoryController.LayerNameHiResPage1, AuxiliaryMemoryController.LayerPriority);
        var auxHires2Layer = bus.CreateLayer(AuxiliaryMemoryController.LayerNameHiResPage2, AuxiliaryMemoryController.LayerPriority);

        // Add mappings to layers
        // Note: Zero page and stack are smaller than a page, but layers work at page granularity
        // For testing purposes, we map the entire first page to aux zero page
        // In a real implementation, more sophisticated handling would be needed

        // AUX_ZP: $0000-$00FF (part of page 0)
        bus.AddLayeredMapping(new LayeredMapping(
            VirtualBase: 0x0000,
            Size: 0x1000, // Full page for layer mapping
            Layer: auxZpLayer,
            DeviceId: 2,
            RegionTag: RegionTag.Ram,
            Perms: PagePerms.All,
            Caps: TargetCaps.SupportsPeek | TargetCaps.SupportsPoke,
            Target: auxZpTarget,
            PhysBase: 0));

        // AUX_STACK: $0100-$01FF (part of page 0)
        // Note: Since stack is also in page 0, we use the same target for testing
        // In a real implementation, the zero page layer would handle both regions
        bus.AddLayeredMapping(new LayeredMapping(
            VirtualBase: 0x0000,
            Size: 0x1000, // Full page for layer mapping
            Layer: auxStackLayer,
            DeviceId: 2,
            RegionTag: RegionTag.Ram,
            Perms: PagePerms.All,
            Caps: TargetCaps.SupportsPeek | TargetCaps.SupportsPoke,
            Target: auxStackTarget,
            PhysBase: 0));

        // AUX_TEXT: $0400-$07FF (page 0, offset $400)
        // Note: Text page spans from $0400-$07FF, which is within page 0
        // For proper implementation, we'd need finer-grained mapping
        bus.AddLayeredMapping(new LayeredMapping(
            VirtualBase: 0x0000,
            Size: 0x1000, // Full page for layer mapping
            Layer: auxTextLayer,
            DeviceId: 2,
            RegionTag: RegionTag.Ram,
            Perms: PagePerms.All,
            Caps: TargetCaps.SupportsPeek | TargetCaps.SupportsPoke,
            Target: auxTextTarget,
            PhysBase: 0));

        // AUX_HIRES1: $2000-$3FFF (pages 2-3)
        bus.AddLayeredMapping(new LayeredMapping(
            VirtualBase: 0x2000,
            Size: 0x2000,
            Layer: auxHires1Layer,
            DeviceId: 2,
            RegionTag: RegionTag.Ram,
            Perms: PagePerms.All,
            Caps: TargetCaps.SupportsPeek | TargetCaps.SupportsPoke,
            Target: auxHires1Target,
            PhysBase: 0));

        // AUX_HIRES2: $4000-$5FFF (pages 4-5)
        bus.AddLayeredMapping(new LayeredMapping(
            VirtualBase: 0x4000,
            Size: 0x2000,
            Layer: auxHires2Layer,
            DeviceId: 2,
            RegionTag: RegionTag.Ram,
            Perms: PagePerms.All,
            Caps: TargetCaps.SupportsPeek | TargetCaps.SupportsPoke,
            Target: auxHires2Target,
            PhysBase: 0));

        // Create and initialize controller
        var controller = new AuxiliaryMemoryController();
        controller.RegisterHandlers(dispatcher);

        var context = CreateMockEventContext(bus);
        controller.Initialize(context);

        return (controller, bus, dispatcher);
    }

    /// <summary>
    /// Creates a mock event context with the specified bus.
    /// </summary>
    /// <param name="bus">The memory bus to configure in the mock context.</param>
    /// <returns>A mock <see cref="IEventContext"/> with the specified bus.</returns>
    private static IEventContext CreateMockEventContext(IMemoryBus bus)
    {
        var mockContext = new Mock<IEventContext>();
        mockContext.Setup(c => c.Bus).Returns(bus);
        return mockContext.Object;
    }

    /// <summary>
    /// Simulates a read access to a soft switch.
    /// </summary>
    /// <param name="dispatcher">The I/O page dispatcher.</param>
    /// <param name="offset">The offset within $C000-$C0FF (0x00-0xFF).</param>
    /// <returns>The value returned by the read handler.</returns>
    private static byte SimulateRead(IOPageDispatcher dispatcher, byte offset)
    {
        var context = CreateTestAccess((uint)(0xC000 + offset), AccessIntent.DataRead);
        return dispatcher.Read(offset, in context);
    }

    /// <summary>
    /// Simulates a side-effect-free read access to a soft switch.
    /// </summary>
    /// <param name="dispatcher">The I/O page dispatcher.</param>
    /// <param name="offset">The offset within $C000-$C0FF (0x00-0xFF).</param>
    /// <returns>The value returned by the read handler.</returns>
    private static byte SimulateSideEffectFreeRead(IOPageDispatcher dispatcher, byte offset)
    {
        var context = CreateTestAccess((uint)(0xC000 + offset), AccessIntent.DataRead, AccessFlags.NoSideEffects);
        return dispatcher.Read(offset, in context);
    }

    /// <summary>
    /// Simulates a write access to a soft switch.
    /// </summary>
    /// <param name="dispatcher">The I/O page dispatcher.</param>
    /// <param name="offset">The offset within $C000-$C0FF (0x00-0xFF).</param>
    /// <param name="value">The value to write.</param>
    private static void SimulateWrite(IOPageDispatcher dispatcher, byte offset, byte value)
    {
        var context = CreateTestAccess((uint)(0xC000 + offset), AccessIntent.DataWrite);
        dispatcher.Write(offset, value, in context);
    }

    /// <summary>
    /// Simulates a side-effect-free write access to a soft switch.
    /// </summary>
    /// <param name="dispatcher">The I/O page dispatcher.</param>
    /// <param name="offset">The offset within $C000-$C0FF (0x00-0xFF).</param>
    /// <param name="value">The value to write.</param>
    private static void SimulateSideEffectFreeWrite(IOPageDispatcher dispatcher, byte offset, byte value)
    {
        var context = CreateTestAccess((uint)(0xC000 + offset), AccessIntent.DataWrite, AccessFlags.NoSideEffects);
        dispatcher.Write(offset, value, in context);
    }

    /// <summary>
    /// Helper method to create test bus access structures.
    /// </summary>
    /// <param name="address">The memory address for the access.</param>
    /// <param name="intent">The access intent (read or write).</param>
    /// <param name="flags">Optional access flags.</param>
    /// <returns>A <see cref="BusAccess"/> configured with the specified parameters.</returns>
    private static BusAccess CreateTestAccess(
        Addr address,
        AccessIntent intent,
        AccessFlags flags = AccessFlags.None)
    {
        return new BusAccess(
            Address: address,
            Value: 0,
            WidthBits: 8,
            Mode: BusAccessMode.Decomposed,
            EmulationFlag: true,
            Intent: intent,
            SourceId: 0,
            Cycle: 0,
            Flags: flags);
    }
}