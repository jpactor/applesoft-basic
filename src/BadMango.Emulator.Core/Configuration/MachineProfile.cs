// <copyright file="MachineProfile.cs" company="Bad Mango Solutions">
// Copyright (c) Bad Mango Solutions. All rights reserved.
// </copyright>

namespace BadMango.Emulator.Core.Configuration;

using System.Text.Json.Serialization;

/// <summary>
/// Defines a machine configuration profile for the emulator.
/// </summary>
/// <remarks>
/// Machine profiles describe the hardware configuration of an emulated system,
/// including CPU type, memory size, and other hardware characteristics.
/// Profiles are loaded from JSON files and can be extended as the emulator evolves.
/// </remarks>
public sealed class MachineProfile
{
    /// <summary>
    /// Gets or sets the unique identifier for this profile.
    /// </summary>
    /// <remarks>
    /// Used as the key when referencing profiles (e.g., "simple-65c02", "apple2e").
    /// Should be lowercase with hyphens, no spaces.
    /// </remarks>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the human-readable display name.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets an optional description of the profile.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the CPU configuration.
    /// </summary>
    [JsonPropertyName("cpu")]
    public required CpuProfileSection Cpu { get; set; }

    /// <summary>
    /// Gets or sets the memory configuration.
    /// </summary>
    [JsonPropertyName("memory")]
    public required MemoryProfileSection Memory { get; set; }

    // TODO: Potential future expansion points (commented for now):
    // TODO: public RomProfileSection[]? Roms { get; set; }
    // TODO: public IoProfileSection?  Io { get; set; }
    // TODO: public DisplayProfileSection? Display { get; set; }
}