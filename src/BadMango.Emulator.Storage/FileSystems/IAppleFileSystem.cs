// <copyright file="IAppleFileSystem.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Storage.FileSystems;

/// <summary>
/// Represents a high-level view of an Apple II file system on a disk volume.
/// Supports listing directories and reading files for debug/MCP use.
/// Implementations exist for DOS 3.3, ProDOS, and Pascal.
/// </summary>
public interface IAppleFileSystem
{
    /// <summary>
    /// The name of the volume (if available).
    /// </summary>
    string VolumeName { get; }

    /// <summary>
    /// Human-readable file system type (e.g. "DOS 3.3", "ProDOS 8", "Pascal").
    /// </summary>
    string FileSystemType { get; }

    /// <summary>
    /// Lists entries in the root or specified directory.
    /// Path syntax is FS-specific ("/" or "" for root, names case-insensitive for most).
    /// </summary>
    IReadOnlyList<FileEntry> ListDirectory(string path = "");

    /// <summary>
    /// Reads the entire contents of a file as bytes.
    /// Throws if not found or unreadable.
    /// </summary>
    byte[] ReadFile(string path);

    /// <summary>
    /// Tries to read a file; returns false on failure.
    /// </summary>
    bool TryReadFile(string path, out byte[]? data);
}

/// <summary>
/// Lightweight description of a file or directory entry.
/// </summary>
public sealed class FileEntry
{
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public int Size { get; init; }
    public bool IsLocked { get; init; }
    public bool IsDirectory { get; init; }
    public string? FullPath { get; init; }

    public override string ToString() => $"{Name} ({Type}, {Size} bytes{(IsLocked ? ", locked" : "")})";
}
