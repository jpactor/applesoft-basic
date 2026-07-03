// <copyright file="AppleFileSystemFactory.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Storage.FileSystems;

using BadMango.Emulator.Storage.Formats;
using BadMango.Emulator.Storage.Gcr;
using BadMango.Emulator.Storage.Media;

/// <summary>
/// Factory to create an IAppleFileSystem view over a mounted or image disk.
/// Supports auto-detection for DOS 3.3, ProDOS, and basic Pascal.
/// </summary>
public static class AppleFileSystemFactory
{
    /// <summary>
    /// Attempts to open a file system view over the given 5.25" sector image (for DOS 3.3 primarily).
    /// </summary>
    public static IAppleFileSystem? TryOpenDos33(SectorImageMedia media)
    {
        if (media == null) return null;
        try
        {
            return new Dos33FileSystem(media);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Open ProDOS over an IBlockMedia.
    /// </summary>
    public static IAppleFileSystem? TryOpenProDos(IBlockMedia media)
    {
        if (media == null || media.BlockSize != 512) return null;

        try
        {
            byte[] ReadBlock(int block) 
            {
                var buf = new byte[512];
                media.ReadBlock(block, buf);
                return buf;
            }
            return new ProDosFileSystem(ReadBlock);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Try to open any supported FS from a DiskImageOpenResult (offline or mounted).
    /// </summary>
    public static IAppleFileSystem? TryOpen(DiskImageOpenResult open)
    {
        if (open is Image525AndBlockResult r)
        {
            if (r.SectorImage != null)
            {
                var dos = TryOpenDos33(r.SectorImage);
                if (dos != null) return dos;
            }
            if (r.BlockMedia != null)
            {
                var prodos = TryOpenProDos(r.BlockMedia);
                if (prodos != null) return prodos;
                var pascal = TryOpenPascal(r.BlockMedia);
                if (pascal != null) return pascal;
            }
        }
        else if (open is ImageBlockResult br && br.Media != null)
        {
            return TryOpenProDos(br.Media);
        }
        return null;
    }

    public static IAppleFileSystem? TryOpenPascal(IBlockMedia media)
    {
        if (media == null || media.BlockSize != 512) return null;
        try
        {
            byte[] ReadBlock(int block)
            {
                var buf = new byte[512];
                media.ReadBlock(block, buf);
                return buf;
            }
            return new PascalFileSystem(ReadBlock);
        }
        catch
        {
            return null;
        }
    }
}
