// <copyright file="IPeripheral.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Bus.Interfaces;

/// <summary>
/// A peripheral device that can be installed in an Apple II slot.
/// </summary>
/// <remarks>
/// <para>
/// Apple II peripheral cards have up to three memory regions:
/// </para>
/// <list type="bullet">
/// <item>
/// <term>Device I/O ($C0n0-$C0nF)</term>
/// <description>16 bytes of I/O space for device registers. Accessed via
/// soft switches. Each slot has 16 bytes starting at $C080 for slot 0,
/// $C090 for slot 1, etc.</description>
/// </item>
/// <item>
/// <term>Slot ROM ($Cn00-$CnFF)</term>
/// <description>256 bytes of identification and boot code. Contains the
/// signature bytes software uses to identify card type, plus minimal boot
/// stub code.</description>
/// </item>
/// <item>
/// <term>Expansion ROM ($C800-$CFFF)</term>
/// <description>2KB of shared ROM space. Selected when slot ROM is accessed,
/// deselected by reading $CFFF. Contains main firmware for complex cards.</description>
/// </item>
/// </list>
/// </remarks>
public interface IPeripheral : IScheduledDevice
{
    /// <summary>
    /// Gets the device type identifier (e.g., "DiskII", "MockingBoard").
    /// </summary>
    /// <value>A string identifying the type of peripheral device.</value>
    string DeviceType { get; }

    /// <summary>
    /// Gets or sets the slot number this card is installed in (1-7).
    /// </summary>
    /// <value>The slot number, set by the slot manager during installation.</value>
    int SlotNumber { get; set; }

    /// <summary>
    /// Gets the I/O handlers for this card's 16-byte region ($C0n0-$C0nF).
    /// </summary>
    /// <value>
    /// The slot I/O handlers for read and write operations,
    /// or <see langword="null"/> if this card has no device I/O registers.
    /// </value>
    SlotIOHandlers? IOHandlers { get; }

    /// <summary>
    /// Gets the firmware ROM region handler ($Cn00-$CnFF).
    /// </summary>
    /// <value>
    /// The bus target for the slot ROM,
    /// or <see langword="null"/> if this card has no slot ROM (unusual).
    /// </value>
    IBusTarget? ROMRegion { get; }

    /// <summary>
    /// Gets the expansion ROM region handler ($C800-$CFFF when selected).
    /// </summary>
    /// <value>
    /// The bus target for the expansion ROM,
    /// or <see langword="null"/> if this card has no expansion ROM.
    /// </value>
    IBusTarget? ExpansionROMRegion { get; }

    /// <summary>
    /// Called when this card's expansion ROM becomes active.
    /// </summary>
    /// <remarks>
    /// This is called when another slot was deselected, or when this slot's
    /// ROM region ($Cn00-$CnFF) was accessed, making this card's expansion
    /// ROM visible at $C800-$CFFF.
    /// </remarks>
    void OnExpansionROMSelected();

    /// <summary>
    /// Called when this card's expansion ROM becomes inactive.
    /// </summary>
    /// <remarks>
    /// This is called when $CFFF is accessed (deselecting all expansion ROMs)
    /// or when another slot's ROM region is accessed.
    /// </remarks>
    void OnExpansionROMDeselected();

    /// <summary>
    /// Resets the peripheral to power-on state.
    /// </summary>
    /// <remarks>
    /// Called during system reset. The peripheral should clear any state
    /// and return to its initial configuration.
    /// </remarks>
    void Reset();
}