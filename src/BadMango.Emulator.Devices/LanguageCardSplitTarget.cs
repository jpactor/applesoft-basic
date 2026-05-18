// <copyright file="LanguageCardSplitTarget.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Devices;

using BadMango.Emulator.Bus;
using BadMango.Emulator.Bus.Interfaces;

/// <summary>
/// A composite bus target used by the Language Card to implement the hardware
/// "split" read/write mode at $D000-$FFFF: reads come from ROM, writes go to LC RAM.
/// </summary>
/// <remarks>
/// <para>
/// On a real Apple II, soft switches $C081/$C089 (and the write-enabled state of
/// $C083/$C08B before the second R*2 read promotes them) select a mode where the
/// CPU reads the underlying system ROM but writes to the Language Card RAM for the
/// same virtual address. This is the mode DOS 3.3's $BFCB sequence uses to copy
/// ROM contents into the LC RAM banks during boot.
/// </para>
/// <para>
/// The bus's page table only stores a single target per page, so this composite
/// target acts as the page target for the affected pages while the LC is in
/// split mode. It implements <see cref="ICompositeTarget"/> so the bus's read /
/// write hot paths dispatch through <see cref="ResolveTarget"/>; this target
/// returns itself, and then forwards the access to either the read or write
/// underlying target according to <see cref="BusAccess.Intent"/> as observed in
/// <see cref="Read8"/> and <see cref="Write8"/>.
/// </para>
/// <para>
/// Address translation is handled internally: callers configure the split target
/// with the virtual base of the region it covers plus the per-target physical
/// base offsets, and accesses are routed using the original
/// <see cref="BusAccess.Address"/> (ignoring the bus-computed
/// <c>physicalAddress</c> parameter, which is necessarily a single value and
/// cannot satisfy both the ROM and RAM targets simultaneously).
/// </para>
/// </remarks>
internal sealed class LanguageCardSplitTarget : ICompositeTarget
{
    private readonly IBusTarget readTarget;
    private readonly IBusTarget writeTarget;
    private readonly Addr readPhysBaseAtRegion;
    private readonly Addr writePhysBaseAtRegion;
    private readonly Addr regionVirtualBase;
    private readonly Addr regionSize;
    private readonly string name;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageCardSplitTarget"/> class.
    /// </summary>
    /// <param name="readTarget">The target that services reads (typically the system ROM).</param>
    /// <param name="readPhysBaseAtRegion">
    /// The physical address within <paramref name="readTarget"/> that corresponds to
    /// <paramref name="regionVirtualBase"/>. For example, if <paramref name="readTarget"/>
    /// is a 16 KB system ROM mapped at $C000 and this split target covers $E000-$FFFF,
    /// pass <c>0x2000</c>.
    /// </param>
    /// <param name="writeTarget">The target that services writes (typically Language Card RAM).</param>
    /// <param name="writePhysBaseAtRegion">
    /// The physical address within <paramref name="writeTarget"/> that corresponds to
    /// <paramref name="regionVirtualBase"/>. Pass <c>0</c> when the LC RAM target is
    /// sized exactly to the covered region.
    /// </param>
    /// <param name="regionVirtualBase">
    /// The starting virtual address of the region this split target covers (for example, $E000).
    /// </param>
    /// <param name="regionSize">
    /// The size in bytes of the region this split target covers (for example, $2000 for $E000-$FFFF).
    /// </param>
    /// <param name="name">A human-readable name for diagnostics.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="readTarget"/>, <paramref name="writeTarget"/>, or <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="regionSize"/> is zero.
    /// </exception>
    public LanguageCardSplitTarget(
        IBusTarget readTarget,
        Addr readPhysBaseAtRegion,
        IBusTarget writeTarget,
        Addr writePhysBaseAtRegion,
        Addr regionVirtualBase,
        Addr regionSize,
        string name)
    {
        ArgumentNullException.ThrowIfNull(readTarget);
        ArgumentNullException.ThrowIfNull(writeTarget);
        ArgumentNullException.ThrowIfNull(name);
        if (regionSize == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(regionSize), regionSize, "Region size must be greater than zero.");
        }

        this.readTarget = readTarget;
        this.readPhysBaseAtRegion = readPhysBaseAtRegion;
        this.writeTarget = writeTarget;
        this.writePhysBaseAtRegion = writePhysBaseAtRegion;
        this.regionVirtualBase = regionVirtualBase;
        this.regionSize = regionSize;
        this.name = name;
    }

    /// <inheritdoc />
    public string Name => name;

    /// <inheritdoc />
    public TargetCaps Capabilities => readTarget.Capabilities | writeTarget.Capabilities;

    /// <summary>
    /// Gets the underlying read target (ROM).
    /// </summary>
    /// <value>The bus target servicing reads in split mode.</value>
    public IBusTarget ReadTarget => readTarget;

    /// <summary>
    /// Gets the underlying write target (Language Card RAM).
    /// </summary>
    /// <value>The bus target servicing writes in split mode.</value>
    public IBusTarget WriteTarget => writeTarget;

    /// <inheritdoc />
    /// <remarks>
    /// Forwards the read to <see cref="ReadTarget"/> at the translated ROM physical
    /// address. The bus-supplied <paramref name="physicalAddress"/> is ignored because
    /// it reflects only the write target's physical base; this method recomputes the
    /// correct ROM address from <see cref="BusAccess.Address"/>.
    /// </remarks>
    public byte Read8(Addr physicalAddress, in BusAccess access)
    {
        Addr offset = (Addr)(access.Address - regionVirtualBase);
        return readTarget.Read8(readPhysBaseAtRegion + offset, access);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Forwards the write to <see cref="WriteTarget"/> at the translated RAM physical
    /// address. The bus-supplied <paramref name="physicalAddress"/> is ignored in
    /// favor of an explicit translation from <see cref="BusAccess.Address"/> so the
    /// behavior is symmetric with <see cref="Read8"/>.
    /// </remarks>
    public void Write8(Addr physicalAddress, byte value, in BusAccess access)
    {
        Addr offset = (Addr)(access.Address - regionVirtualBase);
        writeTarget.Write8(writePhysBaseAtRegion + offset, value, access);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Always returns <see langword="this"/>; routing between the read and write
    /// underlying targets is performed inside <see cref="Read8"/> and
    /// <see cref="Write8"/> rather than by returning different sub-targets here.
    /// Returning a different target per intent would not work because the bus calls
    /// <c>subTarget.Read8(physicalAddress, ...)</c> with the page's single
    /// <c>PhysicalBase</c>, which cannot simultaneously address both ROM and RAM.
    /// </remarks>
    public IBusTarget? ResolveTarget(Addr offset, AccessIntent intent) => this;

    /// <inheritdoc />
    /// <remarks>
    /// Returns <see cref="RegionTag.Ram"/> because the split target is, from the bus
    /// page table's perspective, a single overlay region. The underlying read target's
    /// ROM nature is observable through <see cref="ReadTarget"/> for tooling that needs
    /// it; the page-level tag tracks the overlay (LC RAM) rather than the read pass-through.
    /// </remarks>
    public RegionTag GetSubRegionTag(Addr offset) => RegionTag.Ram;

    /// <inheritdoc />
    public IEnumerable<(Addr StartOffset, Addr Size, RegionTag Tag, string TargetName)> EnumerateSubRegions()
    {
        // The split target is logically a single sub-region from the bus's perspective.
        // Report it once so observability tools can describe it.
        yield return (0, regionSize, RegionTag.Ram, name);
    }
}