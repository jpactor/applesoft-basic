// <copyright file="PascalFileSystem.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Storage.FileSystems;

using System.Text;

using BadMango.Emulator.Storage.Media;

/// <summary>
/// Basic Apple Pascal (UCSD Pascal) file system support for debug/MCP.
/// Pascal uses 512-byte blocks. This is a minimal implementation focused on volume detection and basic directory listing.
/// </summary>
public sealed class PascalFileSystem : IAppleFileSystem
{
    private readonly Func<int, byte[]> _readBlock;
    private readonly List<FileEntry> _entries = new();

    public PascalFileSystem(Func<int, byte[]> readBlock)
    {
        _readBlock = readBlock ?? throw new ArgumentNullException(nameof(readBlock));
        VolumeName = "PASCAL-VOL";
        ParseVolume();
    }

    public string VolumeName { get; private set; }

    public string FileSystemType => "Pascal";

    public IReadOnlyList<FileEntry> ListDirectory(string path = "")
    {
        return _entries;
    }

    public byte[] ReadFile(string path)
    {
        if (!TryReadFile(path, out var data) || data is null)
            throw new FileNotFoundException(path);
        return data;
    }

    public bool TryReadFile(string path, out byte[]? data)
    {
        data = null;
        // Pascal file reading is more complex (file info blocks, etc.). Stub for now.
        // For full support, would need to parse file info block for first block and length.
        return false;
    }

    private void ParseVolume()
    {
        _entries.Clear();
        try
        {
            // Pascal volumes often have volume info in block 2 (first dir block).
            // Header layout (approximate): volume name starts around offset 0x06, length byte before.
            var dirBlock = _readBlock(2);
            if (dirBlock.Length >= 0x20)
            {
                // Try to extract volume name - Pascal names are up to 7 chars, high bit or specific.
                int nameLen = dirBlock[0x06] & 0x0F;
                if (nameLen > 0 && nameLen <= 7 && 0x07 + nameLen < dirBlock.Length)
                {
                    var nameBytes = new byte[nameLen];
                    for (int i = 0; i < nameLen; i++)
                        nameBytes[i] = (byte)(dirBlock[0x07 + i] & 0x7F);
                    var volName = Encoding.ASCII.GetString(nameBytes).Trim();
                    if (!string.IsNullOrWhiteSpace(volName))
                        VolumeName = volName;
                }
            }

            // Basic "directory" listing: we can scan subsequent blocks for file entries, but for minimal, add the volume itself and a note.
            _entries.Add(new FileEntry
            {
                Name = VolumeName,
                Type = "VOL",
                Size = 0,
                IsLocked = false,
                IsDirectory = true,
                FullPath = "/"
            });

            // TODO: parse actual file entries in dir blocks (Pascal dir entries are 26 bytes? with name, first block, etc.)
            // For now, this provides detection and basic info for agent use.
        }
        catch
        {
            // Best effort
        }
    }
}
