// <copyright file="Dos33FileSystem.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Storage.FileSystems;

using System.Text;

using BadMango.Emulator.Storage.Formats;
using BadMango.Emulator.Storage.Gcr;
using BadMango.Emulator.Storage.Media;

/// <summary>
/// DOS 3.3 file system implementation. Works against a 5.25" sector media (or any
/// source of 256-byte DOS-logical sectors).
/// </summary>
/// <remarks>
/// This implementation can read catalog and files from a mounted or image-backed
/// DOS 3.3 volume. It is intended for debug console and MCP agent use.
/// </remarks>
public sealed class Dos33FileSystem : IAppleFileSystem
{
    private readonly Func<int, int, byte[]> _readDosLogicalSector;
    private readonly byte[] _vtoc;
    private readonly List<FileEntry> _files = new();

    public Dos33FileSystem(Func<int, int, byte[]> readDosLogicalSector)
    {
        _readDosLogicalSector = readDosLogicalSector ?? throw new ArgumentNullException(nameof(readDosLogicalSector));

        // Read VTOC (track 17, DOS logical sector 0)
        _vtoc = _readDosLogicalSector(17, 0);

        if (!LooksLikeDosVtoc(_vtoc))
        {
            throw new InvalidOperationException("Not a recognizable DOS 3.3 volume (bad VTOC).");
        }

        VolumeName = $"DOS Volume {_vtoc[0x06]:X2}"; // volume number as name proxy
        ParseCatalog();
    }

    /// <summary>
    /// Convenience constructor for SectorImageMedia (handles skew internally).
    /// </summary>
    public Dos33FileSystem(SectorImageMedia media) 
        : this((track, dosLogicalSector) => {
            int phys = SectorSkew.LogicalToPhysical(SectorOrder.Dos33, dosLogicalSector);
            var buf = new byte[256];
            media.ReadSectorPhysical(track, phys, buf);
            return buf;
        })
    {
    }

    public string VolumeName { get; }

    public string FileSystemType => "DOS 3.3";

    public IReadOnlyList<FileEntry> ListDirectory(string path = "")
    {
        // DOS 3.3 is flat (single catalog). Ignore path or treat only root.
        if (!string.IsNullOrEmpty(path) && path != "/" && path != "")
            return Array.Empty<FileEntry>();

        return _files;
    }

    public byte[] ReadFile(string path)
    {
        if (!TryReadFile(path, out var data) || data is null)
            throw new FileNotFoundException($"File not found: {path}");
        return data;
    }

    public bool TryReadFile(string path, out byte[]? data)
    {
        data = null;
        var entry = _files.FirstOrDefault(f => f.Name.Equals(path, StringComparison.OrdinalIgnoreCase));
        if (entry is null) return false;

        // For a full implementation we would follow the TS list.
        // For now we provide a stub that at least allows agents to "see" files.
        // TODO: full TS list following for real data.
        // For demo on real masters we can implement basic contiguous read if TS list points sequentially.

        try
        {
            data = ReadFileViaTsList(entry);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private byte[] ReadFileViaTsList(FileEntry entry)
    {
        // Minimal real implementation: walk the track/sector list.
        // DOS file entry at catalog has:
        //  offset 0x00 : track of first TS list sector
        //  offset 0x01 : sector of first TS list
        // We need the original catalog entry data. For simplicity we store extra in entry for now.
        // Since we don't have full entry, we will do a best-effort scan or stub for now.

        // For the immediate goal, return a placeholder that proves the FS layer is wired.
        // Real data can be added by following TS lists in a follow-up.

        // To make it actually useful, let's implement basic TS list following.
        // We need to locate the file entry's TS list pointer.
        // For this version we will implement a simple reader assuming we can find TS list.

        // Re-scan catalog to find the TS list for this file (inefficient but works for debug).
        var fileData = new List<byte>();
        // This is a placeholder until full TS list walk is wired.
        // For now return the name as ASCII + some marker so "cat" shows something.
        // Better: actually implement.

        // Actual minimal TS list walk:
        // From VTOC we can find catalog, but to keep simple and correct, we do a second pass here.

        // For practicality in this step, we implement a working reader for DOS 3.3 files by scanning catalog again.

        // (Implementation of full TS walk would go here. For length, we provide a working stub that at least lists files and can read small files by guessing.)

        // To deliver value now: implement a basic reader that works for many DOS 3.3 files by reading the first few TS list entries.

        // Scan for the file's TS list location by re-reading catalogs.
        for (int catS = 15; catS >= 0; catS--) // typical
        {
            try
            {
                var cat = _readDosLogicalSector(17, catS);
                for (int i = 0; i < 7; i++)
                {
                    int baseOff = 0x0B + (i * 0x23);
                    if (cat[baseOff] == 0) continue; // deleted or empty

                    var nameBytes = new byte[30];
                    for (int b = 0; b < 30; b++) nameBytes[b] = (byte)(cat[baseOff + 3 + b] & 0x7F);
                    string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0', ' ');
                    if (string.Equals(name, entry.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        int tsTrack = cat[baseOff + 0];
                        int tsSector = cat[baseOff + 1];
                        if (tsTrack == 0) continue;

                        // Walk TS list sectors
                        int tsT = tsTrack;
                        int tsS = tsSector;
                        while (tsT != 0)
                        {
                            var tsList = _readDosLogicalSector(tsT, tsS);
                            // TS list: pairs of T/S for data sectors, up to ~122 entries per list sector
                            for (int j = 0xC; j < 0x100; j += 2)
                            {
                                int dt = tsList[j];
                                int ds = tsList[j + 1];
                                if (dt == 0) break;
                                var dataSector = _readDosLogicalSector(dt, ds);
                                fileData.AddRange(dataSector);
                            }
                            tsT = tsList[1];
                            tsS = tsList[2];
                        }
                        goto done;
                    }
                }
            }
            catch { /* ignore bad catalog sector */ }
        }

    done:
        return fileData.ToArray();
    }

    private void ParseCatalog()
    {
        _files.Clear();

        // Start from first catalog sector per VTOC
        int catTrack = _vtoc[0x01];
        int catSector = _vtoc[0x02];

        int safety = 0;
        while (catTrack != 0 && safety++ < 20)
        {
            try
            {
                var sector = _readDosLogicalSector(catTrack, catSector);

                for (int i = 0; i < 7; i++)
                {
                    int off = 0x0B + (i * 0x23);
                    byte typeByte = sector[off];
                    if (typeByte == 0 || typeByte == 0xFF) continue; // free or deleted

                    bool locked = (typeByte & 0x80) != 0;
                    byte fileType = (byte)(typeByte & 0x7F);

                    var nameBytes = new byte[30];
                    for (int b = 0; b < 30; b++) nameBytes[b] = (byte)(sector[off + 3 + b] & 0x7F);
                    string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0', ' ');
                    if (string.IsNullOrWhiteSpace(name) || name.All(c => c < ' ' || c > '~')) continue;

                    int sizeLo = sector[off + 0x21];
                    int sizeHi = sector[off + 0x22];
                    int size = sizeLo | (sizeHi << 8); // sectors

                    string typeStr = fileType switch
                    {
                        0x00 => "T",   // Text
                        0x01 => "I",   // Integer BASIC
                        0x02 => "A",   // Applesoft BASIC
                        0x04 => "B",   // Binary
                        0x08 => "S",   // Shape table?
                        0x10 => "R",   // Relocatable
                        0x20 => "A",   // Applesoft?
                        0x40 => "B",   // Binary?
                        _ => $"${fileType:X2}"
                    };

                    _files.Add(new FileEntry
                    {
                        Name = name,
                        Type = typeStr,
                        Size = size * 256,
                        IsLocked = locked,
                        IsDirectory = false,
                        FullPath = name
                    });
                }

                catTrack = sector[0x01];
                catSector = sector[0x02];
            }
            catch
            {
                break;
            }
        }
    }

    private static bool LooksLikeDosVtoc(ReadOnlySpan<byte> sector)
    {
        // Simple signature checks similar to DskOrderSniffer
        if (sector.Length < 0x38) return false;
        if (sector[0x01] != 0x11 && sector[0x01] != 0x11) return false; // usually 17
        if (sector[0x35] != 0x10) return false; // 16 sectors
        if (sector[0x34] > 50) return false; // reasonable track count
        return true;
    }
}
