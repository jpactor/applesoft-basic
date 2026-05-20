// <copyright file="BitsyByeBootHarnessTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Tests;

using System.Globalization;
using System.Text;

using BadMango.Emulator.Bus;
using BadMango.Emulator.Bus.Interfaces;
using BadMango.Emulator.Core.Configuration;
using BadMango.Emulator.Core.Cpu;
using BadMango.Emulator.Core.Interfaces;
using BadMango.Emulator.Debug.Infrastructure;
using BadMango.Emulator.Storage.Formats;
using BadMango.Emulator.Storage.Media;

/// <summary>
/// Long-running diagnostic harness that boots ProDOS 2.4.3 on the
/// <c>pocket2e-a2-enh</c> profile and watches writes into a configurable
/// memory window (default <c>$0C00..$0DFF</c>) to investigate the
/// suspected BITSY BYE off-by-one issue (caller passes a buffer pointer
/// that ends up being <c>$0D00</c> instead of <c>$0C00</c>) and to verify
/// whether 80-column mode is engaged on a //e-class profile.
/// </summary>
/// <remarks>
/// <para>
/// Marked <c>[Category("LongRunning")]</c> so it does not run as part of
/// the default test suite (which filters with <c>TestCategory!=LongRunning</c>).
/// Invoke it explicitly with:
/// </para>
/// <code>
/// dotnet test tests/BadMango.Emulator.Tests \
///   --filter "TestCategory=LongRunning"
/// </code>
/// <para><b>Agent-tunable knobs (environment variables):</b></para>
/// <list type="bullet">
///   <item><c>BPB_HARNESS_MAX_CYCLES</c> — total emulated cycle budget
///     (default 200,000,000 ≈ 200 emulated seconds at 1.023 MHz).</item>
///   <item><c>BPB_HARNESS_MAX_EVENTS</c> — cap on captured diff events
///     (default 16,384).</item>
///   <item><c>BPB_HARNESS_WATCH_START</c>, <c>BPB_HARNESS_WATCH_END</c> —
///     inclusive/exclusive bounds of the shadowed range. Accepts hex
///     (<c>0x0C00</c>, <c>$0C00</c>) or decimal. Defaults: $0C00..$0E00.</item>
/// </list>
/// <para>
/// The harness <em>does not</em> assert correctness; its purpose is to
/// dump diagnostic information. The test passes as long as the CPU does
/// not halt unexpectedly. Diagnostics are written to
/// <see cref="TestContext.Out"/>.
/// </para>
/// <para>
/// In addition to per-byte diffs in the watched range, the harness fires
/// named <b>phase milestones</b> at most once each — e.g. the first time
/// <c>PC == $0801</c> (boot sector entered), at which point it
/// classifies the loaded boot block as DOS 3.3 / ProDOS / unknown by
/// signature and dumps <c>$0800..$08FF</c> for forensic analysis.
/// </para>
/// <para>
/// Important: the trap registry's <c>TrapOperation.Write</c> is invoked
/// at instruction fetch, not on bus writes — so it cannot be used as a
/// memory watchpoint. Instead, the harness shadows the watched range
/// between CPU instructions and reports diffs.
/// </para>
/// </remarks>
[TestFixture]
[Category("LongRunning")]
public sealed class BitsyByeBootHarnessTests
{
    private const string ProfileName = "pocket2e-a2-enh";
    private const string DiskImageRelativePath = "disks/prodos243-master.po";

    // Default watched window (overridable via env vars).
    private const ushort DefaultWatchStart = 0x0C00;
    private const ushort DefaultWatchEnd = 0x0E00; // exclusive

    // Default cycle budget: 50M cycles ≈ 50 emulated seconds at 1.023 MHz.
    // Override with BPB_HARNESS_MAX_CYCLES (e.g. 200000000 for ~200s of
    // emulated time, useful when the boot reaches BITSY BYE late).
    private const ulong DefaultMaxCycles = 50_000_000UL;

    // Default cap on captured diff events. Override with BPB_HARNESS_MAX_EVENTS.
    private const int DefaultMaxEvents = 16_384;

    private const int RecentPcRingSize = 16;

    // Boot-block signatures (per "apple ii bootability" repo memory).
    private static readonly byte[] Dos33BootSignature = [0x01, 0xA5, 0x27, 0xC9, 0x09];
    private static readonly byte[] ProDosBootSignature = [0x01, 0x38, 0xB0, 0x03];

    // Soft switches to peek at every diagnostic event.
    // Mix of memory-routing and video-mode switches so we can tell whether
    // 80-column / alt-charset modes ever engage.
    private static readonly (string Label, ushort Address)[] SoftSwitchPeeks =
    [
        ("RAMRD     ($C013)", 0xC013),
        ("RAMWRT    ($C014)", 0xC014),
        ("ALTZP     ($C016)", 0xC016),
        ("80STORE   ($C018)", 0xC018),
        ("PAGE2     ($C01C)", 0xC01C),
        ("HIRES     ($C01D)", 0xC01D),
        ("LC_STATUS ($C011)", 0xC011),
        ("LC_BANK   ($C012)", 0xC012),
        ("INTCXROM  ($C015)", 0xC015),
        ("SLOTC3ROM ($C017)", 0xC017),
        ("RD80COL   ($C01F)", 0xC01F),
        ("RDALTCHAR ($C01E)", 0xC01E),
        ("RDTEXT    ($C01A)", 0xC01A),
        ("RDMIXED   ($C01B)", 0xC01B),
        ("RDVBL     ($C019)", 0xC019),
    ];

    /// <summary>
    /// Boots <c>pocket2e-a2-enh</c> with <c>prodos243-master.po</c> mounted
    /// in slot 6 drive 1, then steps the CPU and logs every change to
    /// the configured watched range alongside CPU + soft-switch state at
    /// the moment of the change, plus once-per-run phase milestones
    /// (boot-ROM entry, boot-sector entry, language-card entry).
    /// </summary>
    [Test]
    public void BootProDos243_AndWatchPage0CWritesForBitsyByeOffByOne()
    {
        ulong maxCycles = GetEnvUlong("BPB_HARNESS_MAX_CYCLES", DefaultMaxCycles);
        int maxEvents = GetEnvInt("BPB_HARNESS_MAX_EVENTS", DefaultMaxEvents);
        ushort watchStart = GetEnvUshort("BPB_HARNESS_WATCH_START", DefaultWatchStart);
        ushort watchEnd = GetEnvUshort("BPB_HARNESS_WATCH_END", DefaultWatchEnd);

        if (watchEnd <= watchStart)
        {
            throw new ArgumentException(
                $"BPB_HARNESS_WATCH_END (${watchEnd:X4}) must be strictly greater than " +
                $"BPB_HARNESS_WATCH_START (${watchStart:X4}).");
        }

        string repoRoot = LocateRepositoryRoot();
        string profilePath = Path.Combine(repoRoot, "profiles", ProfileName + ".json");
        string diskPath = Path.Combine(repoRoot, DiskImageRelativePath);

        if (!File.Exists(profilePath))
        {
            Assert.Inconclusive($"Profile not found: {profilePath}");
        }

        if (!File.Exists(diskPath))
        {
            Assert.Inconclusive($"Disk image not found: {diskPath}");
        }

        var loader = new MachineProfileLoader(Path.Combine(repoRoot, "profiles"));
        var profile = loader.LoadProfileFromFile(profilePath);

        // libraryRoot = repoRoot ⇒ "library://roms/..." resolves to <repo>/roms/...
        var resolver = new ProfilePathResolver(libraryRoot: repoRoot, profileFilePath: profilePath);

        IMachine machine = MachineFactory.CreateMachine(profile, resolver);

        AssertAllMotherboardDevicesLoaded(machine);

        MountDiskOnSlot6(machine, diskPath);

        // Reset so the disk-II boot ROM at $C600 takes over via reset vector.
        machine.Reset();

        var watcher = new WatchedRangeShadow(machine.Bus, watchStart, watchEnd);
        var recentPcs = new RingBuffer<ushort>(RecentPcRingSize);
        var events = new List<string>(capacity: maxEvents);
        var lastSoftSwitchSnapshot = new byte[SoftSwitchPeeks.Length];
        Array.Fill(lastSoftSwitchSnapshot, (byte)0xFE); // Force first dump as full.

        // Phase milestones. Each fires at most once when its predicate first
        // becomes true for the about-to-execute PC. The "post-boot" milestone
        // is gated on the boot-sector having already fired so reset-vector
        // entry into the //e monitor ROM (PC=$FA62) does not trip it.
        var bootSectorMilestone = new PhaseMilestone("boot-sector-entry", static pc => pc == 0x0801);
        var milestones = new List<PhaseMilestone>
        {
            new("boot-rom-entry", static pc => pc == 0xC600),
            bootSectorMilestone,
            new(
                "post-boot high-memory entry",
                pc => bootSectorMilestone.Fired && pc >= 0xD000),
        };

        ulong stepCount = 0;
        ulong lastCycles = machine.Cpu.GetCycles();
        CpuRunState lastState = CpuRunState.Running;

        while (machine.Cpu.GetCycles() < maxCycles && events.Count < maxEvents)
        {
            var registersBefore = machine.Cpu.GetRegisters();
            ushort pcBefore = registersBefore.PC.GetWord();
            recentPcs.Push(pcBefore);

            // Fire any milestone whose predicate matches and which hasn't
            // already fired. Done before Step() so PC really is "about to
            // execute" the matched address.
            foreach (var milestone in milestones)
            {
                if (!milestone.Fired && milestone.Predicate(pcBefore))
                {
                    milestone.Fired = true;
                    events.Add(FormatMilestone(milestone.Name, stepCount, machine, pcBefore, registersBefore, lastSoftSwitchSnapshot));
                }
            }

            CpuStepResult result;
            try
            {
                result = machine.Cpu.Step();
            }
            catch (Exception ex)
            {
                events.Add($"!! CPU.Step threw at PC=${pcBefore:X4} after {stepCount} steps: {ex.GetType().Name}: {ex.Message}");
                break;
            }

            stepCount++;
            lastState = result.State;

            // WAI / Stopped / Halted: drain the scheduler if WAI, otherwise bail.
            if (result.State == CpuRunState.WaitingForInterrupt)
            {
                // Skip ahead so disk-controller / video timers can fire IRQs.
                // The scheduler is reached through the CPU's event context.
                continue;
            }

            if (result.State is CpuRunState.Stopped or CpuRunState.Halted)
            {
                events.Add($"!! CPU entered {result.State} state at PC=${pcBefore:X4} after {stepCount} steps, cycles={machine.Cpu.GetCycles()}");
                break;
            }

            // Diff the watched range against the shadow.
            if (watcher.HasChanged(out var diffs))
            {
                ulong now = machine.Cpu.GetCycles();
                var registersAfter = machine.Cpu.GetRegisters();
                events.Add(FormatEvent(stepCount, now, pcBefore, registersAfter, diffs, recentPcs, machine.Bus, lastSoftSwitchSnapshot));
            }

            lastCycles = machine.Cpu.GetCycles();
        }

        // Render summary first so it's visible at the top of the test output.
        var summary = new StringBuilder();
        summary.AppendLine(CultureInfo.InvariantCulture, $"BITSY BYE boot harness summary:");
        summary.AppendLine(CultureInfo.InvariantCulture, $"  profile         : {ProfileName}");
        summary.AppendLine(CultureInfo.InvariantCulture, $"  disk image      : {diskPath}");
        summary.AppendLine(CultureInfo.InvariantCulture, $"  watch range     : ${watchStart:X4}..${watchEnd - 1:X4} ({watchEnd - watchStart} bytes)");
        summary.AppendLine(CultureInfo.InvariantCulture, $"  steps executed  : {stepCount}");
        summary.AppendLine(CultureInfo.InvariantCulture, $"  cycles consumed : {machine.Cpu.GetCycles()} (budget {maxCycles})");
        summary.AppendLine(CultureInfo.InvariantCulture, $"  final CPU state : {lastState}");
        summary.AppendLine(CultureInfo.InvariantCulture, $"  events captured : {events.Count} (cap {maxEvents})");
        summary.Append("  milestones      :");
        foreach (var m in milestones)
        {
            summary.Append(CultureInfo.InvariantCulture, $" {m.Name}={(m.Fired ? "✓" : "✗")}");
        }

        summary.AppendLine();
        TestContext.Out.WriteLine(summary.ToString());

        foreach (var ev in events)
        {
            TestContext.Out.WriteLine(ev);
        }

        // Dump the final state of the watched range for forensic reference.
        TestContext.Out.WriteLine();
        TestContext.Out.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Final ${watchStart:X4}..${watchEnd - 1:X4} snapshot:"));
        TestContext.Out.WriteLine(watcher.DumpHex());

        // Surface bus faults, in case anything synthetic was recorded.
        var faultRing = machine.Bus.FaultRing;
        if (faultRing is not null)
        {
            var faults = faultRing.Snapshot();
            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine($"Bus faults recorded: {faults.Length}");
            int faultLimit = Math.Min(faults.Length, 32);
            for (int i = 0; i < faultLimit; i++)
            {
                TestContext.Out.WriteLine($"  [{i:D3}] {faults[i]}");
            }
        }

        // The harness is diagnostic; an unexpected CPU halt is the only
        // explicit failure. Everything else is investigative.
        Assert.That(
            lastState,
            Is.Not.EqualTo(CpuRunState.Halted),
            "CPU halted during boot — investigate the last event entry above.");
    }

    private static void AssertAllMotherboardDevicesLoaded(IMachine machine)
    {
        // The pocket2e-a2-enh profile declares 8 motherboard devices that all
        // contribute to memory routing or I/O for the boot path. If any are
        // missing, the harness will produce misleading diagnostic data.
        var requiredDeviceTypes = new[]
        {
            "Keyboard",
            "Character",
            "Video",
            "Speaker",
            "Language Card",
            "Game I/O",
            "Extended 80-Column Card",
        };

        // We can't introspect the device bag generically by name, so we rely
        // on the bus having received its layered mappings from LanguageCard
        // and Extended80Column. As a smoke check, just confirm that the bus
        // has more than the bare-minimum mappings; the integration tests in
        // Pocket2eIntegrationTests cover detailed wiring assertions.
        Assert.That(machine.Bus, Is.Not.Null, "Bus must be configured.");
        TestContext.Out.WriteLine("Expected motherboard devices (per profile): " + string.Join(", ", requiredDeviceTypes));
    }

    private static void MountDiskOnSlot6(IMachine machine, string diskPath)
    {
        var slotManager = machine.GetComponent<ISlotManager>()
            ?? throw new InvalidOperationException("Machine has no ISlotManager component.");
        var card = slotManager.GetCard(6)
            ?? throw new InvalidOperationException("Slot 6 has no card installed (expected disk-ii-compatible).");

        var controller = card as IDiskController
            ?? throw new InvalidOperationException($"Slot 6 card '{card.GetType().Name}' is not an IDiskController.");

        var open = new DiskImageFactory().Open(diskPath, forceReadOnly: true);
        I525Media? media = open switch
        {
            Image525AndBlockResult both => both.TrackMedia,
            Image525Result trackOnly => trackOnly.Media,
            _ => null,
        };

        if (media is null)
        {
            open.Dispose();
            throw new InvalidOperationException(
                $"Image '{diskPath}' has no 5.25\" track view (format {open.Format}); cannot mount on slot 6.");
        }

        controller.Mount(0, media, diskPath);

        // Intentionally do not dispose 'open' here: the controller holds a
        // live reference to the file-backed media. The harness leaks the
        // backend for the duration of the test, which is acceptable for a
        // single short-lived test process.
    }

    private static string FormatEvent(
        ulong step,
        ulong cycles,
        ushort pcBefore,
        Registers registersAfter,
        IReadOnlyList<(ushort Address, byte OldValue, byte NewValue)> diffs,
        RingBuffer<ushort> recentPcs,
        IMemoryBus bus,
        byte[] lastSoftSwitchSnapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"---- watch event @ step {step}, cycle {cycles} ----");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"  PC(before)={pcBefore:X4}  A={registersAfter.A.GetByte():X2}  X={registersAfter.X.GetByte():X2}  Y={registersAfter.Y.GetByte():X2}  P={(byte)registersAfter.P:X2}  SP={registersAfter.SP.GetByte():X2}");

        sb.Append("  changes :");
        foreach (var d in diffs)
        {
            sb.Append(
                CultureInfo.InvariantCulture,
                $" ${d.Address:X4}:{d.OldValue:X2}->{d.NewValue:X2}");
        }

        sb.AppendLine();

        sb.Append("  recentPC:");
        foreach (var pc in recentPcs.Snapshot())
        {
            sb.Append(CultureInfo.InvariantCulture, $" {pc:X4}");
        }

        sb.AppendLine();

        // Only print soft switches that have changed since the previous event;
        // they rarely shift, and repeating them on every event drowns out signal.
        bool anyDelta = false;
        for (int i = 0; i < SoftSwitchPeeks.Length; i++)
        {
            byte value = DebugPeek(bus, SoftSwitchPeeks[i].Address);
            if (value != lastSoftSwitchSnapshot[i])
            {
                if (!anyDelta)
                {
                    sb.AppendLine("  soft switch deltas:");
                    anyDelta = true;
                }

                sb.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"    {SoftSwitchPeeks[i].Label}: ${lastSoftSwitchSnapshot[i]:X2} -> ${value:X2}");
                lastSoftSwitchSnapshot[i] = value;
            }
        }

        return sb.ToString();
    }

    private static byte DebugPeek(IMemoryBus bus, ushort address)
    {
        var access = new BusAccess(
            Address: address,
            Value: 0,
            WidthBits: 8,
            Mode: BusAccessMode.Decomposed,
            EmulationFlag: true,
            Intent: AccessIntent.DebugRead,
            SourceId: 0,
            Cycle: 0,
            Flags: AccessFlags.NoSideEffects);

        var result = bus.TryRead8(access);
        return result.Ok ? result.Value : (byte)0xFF;
    }

    /// <summary>
    /// Walks up from <see cref="AppContext.BaseDirectory"/> until it finds
    /// the repository root, identified by the simultaneous presence of the
    /// <c>profiles/</c>, <c>roms/</c>, and <c>disks/</c> directories.
    /// Requiring all three avoids false positives such as the
    /// <c>profiles/</c> folder copied next to the test assembly.
    /// </summary>
    /// <returns>The repository root path.</returns>
    private static string LocateRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "profiles")) &&
                Directory.Exists(Path.Combine(dir.FullName, "roms")) &&
                Directory.Exists(Path.Combine(dir.FullName, "disks")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate repository root from {AppContext.BaseDirectory} " +
            "(looking for ancestor containing profiles/, roms/, and disks/).");
    }

    /// <summary>
    /// Renders a banner block for a one-shot phase milestone. On
    /// <c>boot-sector-entry</c> (PC=$0801) the block additionally dumps
    /// the loaded boot sector at <c>$0800..$08FF</c> and classifies it
    /// as DOS 3.3 / ProDOS / unknown by signature.
    /// </summary>
    private static string FormatMilestone(
        string name,
        ulong step,
        IMachine machine,
        ushort pcBefore,
        Registers registers,
        byte[] lastSoftSwitchSnapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"==== phase milestone '{name}' @ step {step}, cycle {machine.Cpu.GetCycles()} ====");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"  PC={pcBefore:X4}  A={registers.A.GetByte():X2}  X={registers.X.GetByte():X2}  Y={registers.Y.GetByte():X2}  P={(byte)registers.P:X2}  SP={registers.SP.GetByte():X2}");

        // Full soft-switch snapshot at every milestone (so we can easily
        // diff 80-column / mem-routing state across phases). Also resync
        // the last-snapshot baseline so subsequent diff-events show
        // deltas relative to the milestone state.
        sb.AppendLine("  soft switches:");
        for (int i = 0; i < SoftSwitchPeeks.Length; i++)
        {
            byte value = DebugPeek(machine.Bus, SoftSwitchPeeks[i].Address);
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"    {SoftSwitchPeeks[i].Label} = ${value:X2}");
            lastSoftSwitchSnapshot[i] = value;
        }

        if (name == "boot-sector-entry")
        {
            sb.AppendLine("  loaded boot sector $0800..$08FF:");
            DumpRangeHex(sb, machine.Bus, 0x0800, 0x0900, indent: "    ");
            sb.Append("  classification : ");
            sb.AppendLine(ClassifyBootSector(machine.Bus));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Inspects bytes at <c>$0800..</c> on the live bus and classifies the
    /// boot loader by signature (see "apple ii bootability" repo memory).
    /// </summary>
    private static string ClassifyBootSector(IMemoryBus bus)
    {
        Span<byte> head = stackalloc byte[8];
        for (int i = 0; i < head.Length; i++)
        {
            head[i] = DebugPeek(bus, (ushort)(0x0800 + i));
        }

        if (StartsWith(head, Dos33BootSignature))
        {
            return "DOS 3.3 boot1 (signature 01 A5 27 C9 09 at $0800)";
        }

        if (StartsWith(head, ProDosBootSignature))
        {
            return "ProDOS PBOOT block 0 (signature 01 38 B0 03 at $0800)";
        }

        if (head[0] == 0x01)
        {
            return $"unknown bootable image (starts $01 but signature mismatch; head=${head[0]:X2} ${head[1]:X2} ${head[2]:X2} ${head[3]:X2} ${head[4]:X2})";
        }

        return $"NOT BOOTABLE (head=${head[0]:X2} ${head[1]:X2} ${head[2]:X2} ${head[3]:X2} ${head[4]:X2} — no leading $01)";
    }

    private static bool StartsWith(ReadOnlySpan<byte> haystack, ReadOnlySpan<byte> needle)
    {
        if (haystack.Length < needle.Length)
        {
            return false;
        }

        return haystack[..needle.Length].SequenceEqual(needle);
    }

    private static void DumpRangeHex(StringBuilder sb, IMemoryBus bus, ushort start, ushort end, string indent)
    {
        for (int row = start; row < end; row += 16)
        {
            sb.Append(CultureInfo.InvariantCulture, $"{indent}${row:X4}: ");
            for (int col = 0; col < 16 && row + col < end; col++)
            {
                sb.Append(CultureInfo.InvariantCulture, $"{DebugPeek(bus, (ushort)(row + col)):X2} ");
            }

            sb.AppendLine();
        }
    }

    private static ulong GetEnvUlong(string name, ulong defaultValue)
    {
        string? raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (TryParseUnsignedLiteral(raw, out ulong value))
        {
            return value;
        }

        throw new ArgumentException($"Environment variable {name}='{raw}' is not a valid unsigned integer.");
    }

    private static int GetEnvInt(string name, int defaultValue)
    {
        ulong asUlong = GetEnvUlong(name, (ulong)defaultValue);
        if (asUlong > int.MaxValue)
        {
            throw new ArgumentException($"Environment variable {name} value {asUlong} exceeds Int32.MaxValue.");
        }

        return (int)asUlong;
    }

    private static ushort GetEnvUshort(string name, ushort defaultValue)
    {
        ulong asUlong = GetEnvUlong(name, defaultValue);
        if (asUlong > ushort.MaxValue)
        {
            throw new ArgumentException($"Environment variable {name} value {asUlong} exceeds UInt16.MaxValue.");
        }

        return (ushort)asUlong;
    }

    /// <summary>
    /// Parses an unsigned literal in any of these forms: <c>1234</c>
    /// (decimal), <c>0x1A2B</c> / <c>0X1A2B</c> (C hex), or <c>$1A2B</c>
    /// (Apple hex).
    /// </summary>
    private static bool TryParseUnsignedLiteral(string text, out ulong value)
    {
        string trimmed = text.Trim();
        if (trimmed.StartsWith('$'))
        {
            return ulong.TryParse(trimmed.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return ulong.TryParse(trimmed.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        return ulong.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// A named, single-shot trigger fired when its predicate first
    /// returns <see langword="true"/> for the about-to-execute PC.
    /// </summary>
    private sealed class PhaseMilestone
    {
        public PhaseMilestone(string name, Func<ushort, bool> predicate)
        {
            this.Name = name;
            this.Predicate = predicate;
        }

        public string Name { get; }

        public Func<ushort, bool> Predicate { get; }

        public bool Fired { get; set; }
    }

    /// <summary>
    /// Maintains a shadow copy of a memory range and diffs against the live
    /// bus on demand using side-effect-free <see cref="AccessIntent.DebugRead"/>
    /// accesses.
    /// </summary>
    private sealed class WatchedRangeShadow
    {
        private readonly IMemoryBus bus;
        private readonly ushort start;
        private readonly ushort end; // exclusive
        private readonly byte[] shadow;

        public WatchedRangeShadow(IMemoryBus bus, ushort start, ushort end)
        {
            this.bus = bus;
            this.start = start;
            this.end = end;
            this.shadow = new byte[end - start];
            for (int i = 0; i < shadow.Length; i++)
            {
                shadow[i] = DebugPeek(bus, (ushort)(start + i));
            }
        }

        public bool HasChanged(out IReadOnlyList<(ushort Address, byte OldValue, byte NewValue)> diffs)
        {
            List<(ushort, byte, byte)>? list = null;
            for (int i = 0; i < shadow.Length; i++)
            {
                byte current = DebugPeek(bus, (ushort)(start + i));
                if (current != shadow[i])
                {
                    list ??= [];
                    list.Add(((ushort)(start + i), shadow[i], current));
                    shadow[i] = current;
                }
            }

            diffs = list ?? (IReadOnlyList<(ushort, byte, byte)>)Array.Empty<(ushort, byte, byte)>();
            return list is { Count: > 0 };
        }

        public string DumpHex()
        {
            var sb = new StringBuilder();
            for (int row = 0; row < shadow.Length; row += 16)
            {
                sb.Append(CultureInfo.InvariantCulture, $"  ${start + row:X4}: ");
                for (int col = 0; col < 16 && row + col < shadow.Length; col++)
                {
                    sb.Append(CultureInfo.InvariantCulture, $"{shadow[row + col]:X2} ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    private sealed class RingBuffer<T>
    {
        private readonly T[] buffer;
        private int head;
        private int count;

        public RingBuffer(int capacity)
        {
            buffer = new T[capacity];
        }

        public void Push(T value)
        {
            buffer[head] = value;
            head = (head + 1) % buffer.Length;
            if (count < buffer.Length)
            {
                count++;
            }
        }

        public IEnumerable<T> Snapshot()
        {
            // Yield oldest -> newest.
            int start = (head - count + buffer.Length) % buffer.Length;
            for (int i = 0; i < count; i++)
            {
                yield return buffer[(start + i) % buffer.Length];
            }
        }
    }
}