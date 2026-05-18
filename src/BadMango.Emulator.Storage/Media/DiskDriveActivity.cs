// <copyright file="DiskDriveActivity.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Storage.Media;

/// <summary>
/// Per-drive activity counters surfaced as part of <see cref="DiskActivitySnapshot"/>.
/// </summary>
/// <remarks>
/// <para>
/// Captures the stream-observed address-field state for a single drive: when the
/// controller serves a fresh nibble whose sliding three-byte window matches the GCR
/// address-field prologue (<c>$D5 $AA $96</c>), the next eight bytes are parsed as
/// 4-and-4 volume / track / sector / checksum and recorded here. This lets the
/// debug console show <em>what RWTS actually sees</em> as the byte stream flows past
/// the head, even without instrumenting the CPU side, which is crucial for diagnosing
/// "I/O ERROR" failures where the encoding round-trips correctly but the live byte
/// stream is mis-timed, mis-positioned, or off-track.
/// </para>
/// </remarks>
/// <param name="ObservedAddressFields">Total address-field prologues recognised in the live byte stream.</param>
/// <param name="ObservedAddressFieldChecksumErrors">Address-field prologues whose volume/track/sector/checksum failed verification (<c>vol ^ trk ^ sec ≠ chk</c>).</param>
/// <param name="LastObservedVolume">Volume number from the most recent address field, or <see langword="null"/> if none observed.</param>
/// <param name="LastObservedTrack">Track number from the most recent address field, or <see langword="null"/> if none observed.</param>
/// <param name="LastObservedSector">Sector number from the most recent address field, or <see langword="null"/> if none observed.</param>
/// <param name="LastObservedChecksum">Checksum nibble decoded from the most recent address field, or <see langword="null"/> if none observed.</param>
/// <param name="LastObservedChecksumValid">Whether the most recent address field's checksum verified, or <see langword="null"/> if none observed.</param>
/// <param name="BytesServedOnCurrentTrack">Nibbles served (fresh bytes) since the head last arrived at the current quarter-track.</param>
public readonly record struct DiskDriveActivity(
    long ObservedAddressFields,
    long ObservedAddressFieldChecksumErrors,
    int? LastObservedVolume,
    int? LastObservedTrack,
    int? LastObservedSector,
    int? LastObservedChecksum,
    bool? LastObservedChecksumValid,
    long BytesServedOnCurrentTrack);