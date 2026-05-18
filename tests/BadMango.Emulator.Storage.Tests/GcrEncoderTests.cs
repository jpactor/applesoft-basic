// <copyright file="GcrEncoderTests.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Storage.Tests;

/// <summary>
/// GCR 6-and-2 encoder/decoder tests (PRD §6.1 row 3 acceptance).
/// </summary>
[TestFixture]
public class GcrEncoderTests
{
    /// <summary>
    /// Verifies the WriteTable / ReadTable round-trip for every 6-bit value.
    /// </summary>
    [Test]
    public void WriteTable_RoundTripsThroughReadTable()
    {
        var write = GcrEncoder.GetWriteTable();
        var read = GcrEncoder.GetReadTable();
        Assert.That(write.Length, Is.EqualTo(64));
        for (var v = 0; v < 64; v++)
        {
            var nibble = write[v];
            Assert.That(read[nibble], Is.EqualTo((byte)v), $"6-bit value {v} did not round-trip via nibble 0x{nibble:X2}.");
        }
    }

    /// <summary>
    /// Verifies that an encoded track decodes back to the original sector data for every
    /// (volume, track) tuple in the standard 5.25" range.
    /// </summary>
    /// <param name="volume">DOS volume number written into address fields.</param>
    /// <param name="track">Track number written into address fields.</param>
    [TestCase(254, 0)]
    [TestCase(254, 17)]
    [TestCase(254, 34)]
    [TestCase(1, 0)]
    [TestCase(255, 34)]
    [TestCase(0, 0)]
    public void EncodeDecode_RoundTripsAllSectors(int volume, int track)
    {
        var sectors = new byte[16 * 256];
        var rng = new Random(volume + (track * 4096));
        rng.NextBytes(sectors);

        var nibbles = new byte[GcrEncoder.StandardTrackLength];
        GcrEncoder.EncodeTrack(volume, track, sectors, nibbles);

        var decoded = new byte[16 * 256];
        var mask = GcrEncoder.DecodeTrack(nibbles, decoded);

        Assert.That(mask, Is.EqualTo(0xFFFF), $"Not every physical sector decoded for vol={volume}, track={track}.");
        Assert.That(decoded, Is.EqualTo(sectors));
    }

    /// <summary>
    /// Verifies that the encoded track contains a valid address-field prologue at the
    /// expected offsets and that the embedded sector numbers go 0…15.
    /// </summary>
    [Test]
    public void EncodeTrack_AddressFields_HavePhysicalSectorOrder()
    {
        var sectors = new byte[16 * 256];
        var nibbles = new byte[GcrEncoder.StandardTrackLength];
        GcrEncoder.EncodeTrack(254, 17, sectors, nibbles);

        // Address field starts 48 bytes into each 416-byte sector slot. Sector
        // number is 4-and-4 encoded at bytes 7..8 of the address field.
        for (var phys = 0; phys < 16; phys++)
        {
            var addrStart = (phys * 416) + 48;
            Assert.That(nibbles[addrStart], Is.EqualTo(GcrEncoder.AddressPrologue1), $"prologue byte 1 of phys {phys}");
            Assert.That(nibbles[addrStart + 1], Is.EqualTo(GcrEncoder.AddressPrologue2), $"prologue byte 2 of phys {phys}");
            Assert.That(nibbles[addrStart + 2], Is.EqualTo(GcrEncoder.AddressPrologue3), $"prologue byte 3 of phys {phys}");

            var secH = nibbles[addrStart + 7];
            var secL = nibbles[addrStart + 8];
            var sec = (byte)(((secH << 1) | 1) & secL);
            Assert.That(sec, Is.EqualTo((byte)phys));
        }
    }

    /// <summary>
    /// Verifies that decoding all-zeros nibbles yields no sectors but does not throw.
    /// </summary>
    [Test]
    public void DecodeTrack_NoAddressFields_ReturnsZeroMask()
    {
        var nibbles = new byte[GcrEncoder.StandardTrackLength];
        nibbles.AsSpan().Fill(0xFF);
        var decoded = new byte[16 * 256];
        var mask = GcrEncoder.DecodeTrack(nibbles, decoded);
        Assert.That(mask, Is.EqualTo(0));
    }

    /// <summary>
    /// Verifies that the 6-and-2 auxiliary buffer packs each sector byte's low
    /// 2 bits into aux index <c>85 - (i % 86)</c> — the layout the real Apple II
    /// RWTS POSTNIB16 routine and the $C600 boot ROM both expect. A previous
    /// bug packed into <c>i % 86</c>, which left the top 6 bits decoding
    /// correctly but permuted the low 2 bits of every sector byte and caused
    /// DOS 3.3 to fail with "I/O ERROR" on boot even though the XOR-chain
    /// checksum (which is order-independent) still validated.
    /// </summary>
    [Test]
    public void EncodeTrack_DataFieldAuxBytes_AreInRwtsOrder()
    {
        // Sector 0, byte 85 = 0x02 (low2 = 10), byte 171 = 0x01 (low2 = 01).
        // Both pack into twoBit[85 - (85 % 86)] = twoBit[0] at shifts 0 and 2.
        // After per-pair bit-reverse:
        //   reversed(10) = 01 at shift 0 -> 0x01
        //   reversed(01) = 10 at shift 2 -> 0x08
        // twoBit[0] = 0x09, and xorChain[0] = twoBit[0] ^ 0 = 0x09.
        var sectors = new byte[16 * 256];
        sectors[85] = 0x02;
        sectors[171] = 0x01;

        var nibbles = new byte[GcrEncoder.StandardTrackLength];
        GcrEncoder.EncodeTrack(254, 0, sectors, nibbles);

        // Find sector-0's data field: 48 gap + 14 addr + 5 gap = 67 bytes in,
        // then 3 bytes of data prologue.
        const int sector0DataStart = 48 + 14 + 5 + 3;
        var read = GcrEncoder.GetReadTable();
        var firstAux6Bit = read[nibbles[sector0DataStart]];

        Assert.That(firstAux6Bit, Is.Not.EqualTo((byte)0xFF), "First aux nibble must translate via the read table.");
        Assert.That(firstAux6Bit, Is.EqualTo((byte)0x09), "First aux 6-bit value must encode the low two bits of sector bytes 85 and 171 (the real-RWTS layout), not byte 0's bits.");

        // Symmetric case: sector byte 0's low 2 bits must live in the LAST
        // aux byte on disk (twoBit[85]). Unwind the first 86 aux nibbles to
        // recover twoBit[85] directly.
        Array.Clear(sectors);
        sectors[0] = 0x03;   // low2 = 11
        sectors[86] = 0x02;  // low2 = 10
        sectors[172] = 0x01; // low2 = 01
        GcrEncoder.EncodeTrack(254, 0, sectors, nibbles);

        byte twoBit85 = 0;
        for (var i = 0; i < 86; i++)
        {
            var v = read[nibbles[sector0DataStart + i]];
            Assert.That(v, Is.Not.EqualTo((byte)0xFF));
            twoBit85 ^= v;
        }

        // Expected twoBit[85] for sector bytes 0/86/172 (all idx = 85):
        //   reversed(11) = 11 at shift 0 -> 0x03
        //   reversed(10) = 01 at shift 2 -> 0x04
        //   reversed(01) = 10 at shift 4 -> 0x20
        // Total = 0x27.
        Assert.That(twoBit85, Is.EqualTo((byte)0x27), "Last aux byte on disk (twoBit[85]) must carry the low 2 bits of sector bytes 0, 86, and 172.");

        // And a full round-trip must still recover the original sector.
        var decoded = new byte[16 * 256];
        var mask = GcrEncoder.DecodeTrack(nibbles, decoded);
        Assert.That(mask & 1, Is.EqualTo(1), "Sector 0 must decode.");
        Assert.That(decoded[0], Is.EqualTo((byte)0x03));
        Assert.That(decoded[86], Is.EqualTo((byte)0x02));
        Assert.That(decoded[172], Is.EqualTo((byte)0x01));
    }

    /// <summary>
    /// Empirical-oracle regression test: encodes a captured DOS 3.3 boot sector
    /// and decodes it back through a faithful port of the real Apple II $C600
    /// boot ROM 6-and-2 decoder, asserting a byte-for-byte recovery.
    /// <para>
    /// The decoder implementation here mirrors what the Apple II boot ROM and
    /// RWTS POSTNIB16 actually do (sector byte i's low 2 bits come from aux
    /// byte 85 - (i % 86), with per-pair bit-reverse). It is intentionally
    /// implemented independently of <see cref="GcrEncoder"/> so it cannot
    /// mask an encoder bug the way a symmetric round-trip via
    /// <c>DecodeTrack</c> would.
    /// </para>
    /// </summary>
    [Test]
    public void EncodeDataField_DecodesViaAppleConvention_RecoversBootSectorByteForByte()
    {
        // Verbatim DOS 3.3 boot1 sector (T0/S0) captured during real-disk testing.
        // This is the data that previously round-tripped through our symmetric
        // encoder/decoder but failed to boot under the $C600 ROM because the
        // low 2 bits were permuted.
        byte[] source =
        [
            0x01, 0xA5, 0x27, 0xC9, 0x09, 0xD0, 0x18, 0xA5, 0x2B, 0x4A, 0x4A, 0x4A, 0x4A, 0x09, 0xC0, 0x85,
            0x3F, 0xA9, 0x5C, 0x85, 0x3E, 0x18, 0xAD, 0xFE, 0x08, 0x6D, 0xFF, 0x08, 0x8D, 0xFE, 0x08, 0xAE,
            0xFF, 0x08, 0x30, 0x15, 0xBD, 0x4D, 0x08, 0x85, 0x3D, 0xCE, 0xFF, 0x08, 0xAD, 0xFE, 0x08, 0x85,
            0x27, 0xCE, 0xFE, 0x08, 0xA6, 0x2B, 0x6C, 0x3E, 0x00, 0xEE, 0xFE, 0x08, 0xEE, 0xFE, 0x08, 0x20,
            0x89, 0xFE, 0x20, 0x93, 0xFE, 0x20, 0x2F, 0xFB, 0xA6, 0x2B, 0x6C, 0xFD, 0x08, 0x00, 0x0D, 0x0B,
            0x09, 0x07, 0x05, 0x03, 0x01, 0x0E, 0x0C, 0x0A, 0x08, 0x06, 0x04, 0x02, 0x0F, 0x00, 0x20, 0x64,
            0xA7, 0xB0, 0x08, 0xA9, 0x00, 0xA8, 0x8D, 0x5D, 0xB6, 0x91, 0x40, 0xAD, 0xC5, 0xB5, 0x4C, 0xD2,
            0xA6, 0xAD, 0x5D, 0xB6, 0xF0, 0x08, 0xEE, 0xBD, 0xB5, 0xD0, 0x03, 0xEE, 0xBE, 0xB5, 0xA9, 0x00,
            0x8D, 0x5D, 0xB6, 0x4C, 0x46, 0xA5, 0x8D, 0xBC, 0xB5, 0x20, 0xA8, 0xA6, 0x20, 0xEA, 0xA2, 0x4C,
            0x7D, 0xA2, 0xA0, 0x13, 0xB1, 0x42, 0xD0, 0x14, 0xC8, 0xC0, 0x17, 0xD0, 0xF7, 0xA0, 0x19, 0xB1,
            0x42, 0x99, 0xA4, 0xB5, 0xC8, 0xC0, 0x1D, 0xD0, 0xF6, 0x4C, 0xBC, 0xA6, 0xA2, 0xFF, 0x8E, 0x5D,
            0xB6, 0xD0, 0xF6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x20, 0x58, 0xFC, 0xA9, 0xC2, 0x20, 0xED, 0xFD, 0xA9, 0x01, 0x20, 0xDA, 0xFD, 0xA9, 0xAD, 0x20,
            0xED, 0xFD, 0xA9, 0x00, 0x20, 0xDA, 0xFD, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB6, 0x09,
        ];

        var sectors = new byte[16 * 256];
        Buffer.BlockCopy(source, 0, sectors, 0, 256);

        var nibbles = new byte[GcrEncoder.StandardTrackLength];
        GcrEncoder.EncodeTrack(254, 0, sectors, nibbles);

        var read = GcrEncoder.GetReadTable();

        // Locate sector-0's data prologue ($D5 $AA $AD) by scanning forward.
        var dataStart = -1;
        for (var i = 0; i < nibbles.Length - 3; i++)
        {
            if (nibbles[i] == 0xD5 && nibbles[i + 1] == 0xAA && nibbles[i + 2] == 0xAD)
            {
                dataStart = i + 3;
                break;
            }
        }

        Assert.That(dataStart, Is.GreaterThan(0), "Data prologue D5 AA AD must be present.");

        // Faithful Apple II RWTS POSTNIB16 / $C600 boot ROM 6-and-2 decoder.
        // Implemented independently of GcrEncoder so an encoder bug cannot
        // hide here.
        Span<byte> twoBit = stackalloc byte[86];
        Span<byte> sixBit = stackalloc byte[256];
        byte last = 0;
        for (var i = 0; i < 86; i++)
        {
            var v = read[nibbles[dataStart + i]];
            Assert.That(v, Is.Not.EqualTo((byte)0xFF));
            last ^= v;
            twoBit[i] = last;
        }

        for (var i = 0; i < 256; i++)
        {
            var v = read[nibbles[dataStart + 86 + i]];
            Assert.That(v, Is.Not.EqualTo((byte)0xFF));
            last ^= v;
            sixBit[i] = last;
        }

        var checksumNibble = read[nibbles[dataStart + 342]];
        Assert.That(checksumNibble, Is.Not.EqualTo((byte)0xFF));
        last ^= checksumNibble;
        Assert.That(last, Is.EqualTo((byte)0), "XOR-chain residual must be zero.");

        var recovered = new byte[256];
        for (var i = 0; i < 256; i++)
        {
            var idx = 85 - (i % 86);
            var shift = (i / 86) * 2;
            var packed = (twoBit[idx] >> shift) & 0x03;
            var twoBits = ((packed & 0x01) << 1) | ((packed & 0x02) >> 1);
            recovered[i] = (byte)((sixBit[i] << 2) | twoBits);
        }

        Assert.That(recovered, Is.EqualTo(source), "Recovered sector must match the original boot1 bytes byte-for-byte when decoded via the real Apple II convention.");
    }

    /// <summary>
    /// Verifies that out-of-range <paramref name="volume"/> or <paramref name="track"/>
    /// throws <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <param name="volume">Volume to attempt.</param>
    /// <param name="track">Track to attempt.</param>
    [TestCase(-1, 0)]
    [TestCase(256, 0)]
    [TestCase(0, -1)]
    [TestCase(0, 256)]
    public void EncodeTrack_OutOfRangeVolumeOrTrack_Throws(int volume, int track)
    {
        var sectors = new byte[16 * 256];
        var nibbles = new byte[GcrEncoder.StandardTrackLength];
        Assert.Throws<ArgumentOutOfRangeException>(() => GcrEncoder.EncodeTrack(volume, track, sectors, nibbles));
    }
}