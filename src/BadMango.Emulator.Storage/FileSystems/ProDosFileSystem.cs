// <copyright file="ProDosFileSystem.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Storage.FileSystems;

using System.Text;

using BadMango.Emulator.Storage.Media;

/// <summary>
/// Basic ProDOS 8 file system reader for debug/MCP use.
/// Supports volume directory listing and reading seedling/sapling files.
/// </summary>
public sealed class ProDosFileSystem : IAppleFileSystem
{
    private readonly Func<int, byte[]> _readBlock;
    private readonly List<FileEntry> _entries = new();

    public ProDosFileSystem(Func<int, byte[]> readBlock)
    {
        _readBlock = readBlock ?? throw new ArgumentNullException(nameof(readBlock));

        var keyBlock = _readBlock(2);
        if (!LooksLikeProDosVolumeDir(keyBlock))
            throw new InvalidOperationException("Not a ProDOS volume directory key block.");

        VolumeName = ReadProDosName(keyBlock, 5, keyBlock[4] & 0x0F);
        ParseVolumeDirectory();
    }

    public string VolumeName { get; }

    public string FileSystemType => "ProDOS 8";

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
        var entry = _entries.FirstOrDefault(e => e.Name.Equals(path, StringComparison.OrdinalIgnoreCase) && !e.IsDirectory);
        if (entry is null) return false;

        // For simplicity, support only seedling (storage 1) and sapling (2) for now.
        // Full tree support can be added.

        try
        {
            data = ReadProDosFileData(entry);
            return data != null;
        }
        catch
        {
            return false;
        }
    }

    private void ParseVolumeDirectory()
    {
        _entries.Clear();

        // Simple walk of the volume dir blocks (usually starts at block 2, linked via prev/next)
        int block = 2;
        int safety = 0;
        while (block != 0 && safety++ < 10)
        {
            var buf = _readBlock(block);

            // Each block has up to ~13 entries after header
            for (int i = 0; i < 13; i++)
            {
                int off = 4 + (i * 39); // entry size in dir
                byte storageAndLen = buf[off];
                if (storageAndLen == 0) continue;

                int storageType = (storageAndLen >> 4) & 0xF;
                int nameLen = storageAndLen & 0xF;
                if (nameLen == 0) continue;

                string name = ReadProDosName(buf, off + 1, nameLen);

                byte fileType = buf[off + 0x10];
                int keyBlock = buf[off + 0x11] | (buf[off + 0x12] << 8);
                int size = buf[off + 0x15] | (buf[off + 0x16] << 8) | (buf[off + 0x17] << 16); // blocks or bytes?

                bool isDir = storageType == 0xD; // subdirectory

                _entries.Add(new FileEntry
                {
                    Name = name,
                    Type = isDir ? "DIR" : GetProDosTypeName(fileType),
                    Size = size,
                    IsLocked = false,
                    IsDirectory = isDir,
                    FullPath = name
                });
            }

            // next block pointer (simple forward link in volume dir)
            block = buf[0x02] | (buf[0x03] << 8);
        }
    }

    private byte[]? ReadProDosFileData(FileEntry entry)
    {
        // Re-scan volume directory to get storage type and key block for the file.
        int block = 2;
        int safety = 0;
        while (block != 0 && safety++ < 10)
        {
            var buf = _readBlock(block);
            for (int i = 0; i < 13; i++)
            {
                int off = 4 + (i * 39);
                byte storageAndLen = buf[off];
                if (storageAndLen == 0) continue;

                int nameLen = storageAndLen & 0x0F;
                string name = ReadProDosName(buf, off + 1, nameLen);
                if (!string.Equals(name, entry.Name, StringComparison.OrdinalIgnoreCase)) continue;

                int storageType = (storageAndLen >> 4) & 0xF;
                int keyBlock = buf[off + 0x11] | (buf[off + 0x12] << 8);
                int eof = buf[off + 0x15] | (buf[off + 0x16] << 8) | (buf[off + 0x17] << 16);

                if (storageType == 1) // seedling
                {
                    var data = _readBlock(keyBlock);
                    if (eof > 0 && data.Length > eof) Array.Resize(ref data, eof);
                    return data;
                }
                else if (storageType == 2) // sapling
                {
                    var index = _readBlock(keyBlock);
                    var data = new List<byte>();
                    for (int p = 0; p < 256; p += 2)
                    {
                        int db = index[p] | (index[p + 1] << 8);
                        if (db == 0) break;
                        data.AddRange(_readBlock(db));
                    }
                    if (eof > 0 && data.Count > eof) data.RemoveRange(eof, data.Count - eof);
                    return data.ToArray();
                }
                else
                {
                    return Encoding.ASCII.GetBytes($"[ProDOS {entry.Name}: storage type {storageType} not fully supported]");
                }
            }
            block = buf[0x02] | (buf[0x03] << 8);
        }
        return null;
    }

    private static string ReadProDosName(byte[] buf, int start, int len)
    {
        var sb = new StringBuilder(len);
        for (int i = 0; i < len; i++)
        {
            byte b = buf[start + i];
            sb.Append((char)(b & 0x7F)); // high bit usually set on disk for names
        }
        return sb.ToString();
    }

    private static bool LooksLikeProDosVolumeDir(ReadOnlySpan<byte> block)
    {
        if (block.Length < 5) return false;
        if (block[0] != 0 || block[1] != 0) return false; // prev ptr 0
        return (block[4] & 0xF0) == 0xF0; // storage type F for volume dir
    }

    private static string GetProDosTypeName(byte t) => t switch
    {
        0x01 => "BAD",
        0x04 => "TXT",
        0x06 => "BIN",
        0x0F => "DIR",
        0x19 => "ADB",
        0x1A => "AWP",
        0x1B => "ASP",
        0xFC => "BAS",
        0xFD => "VAR",
        0xFE => "REL",
        0xFF => "SYS",
        _ => $"${t:X2}"
    };
}
