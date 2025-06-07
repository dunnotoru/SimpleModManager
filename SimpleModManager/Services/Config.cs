using System;
using System.IO;
using System.Text.Json;

namespace SimpleModManager.Services;

public class Config
{
    public static readonly string ModsDirName = "mods";
    public static readonly string ConfigDirName = "config";
    public static readonly string OverridesDirName = "overrides";
    public static readonly string ManifestFileName = "smmanifest.json";
    public static readonly string GameDirectory = @"C:\Users\user\AppData\Roaming\.minecraft\";
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".smmanager"); 
    public static string StorageDirectory => Path.Combine(DataDirectory, "storage");
    public static string ConfigPath => Path.Combine(DataDirectory, "config.json");
    public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
}