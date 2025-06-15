using System;
using System.IO;
using System.Text.Json;

namespace SimpleModManager.Services;

public static class AppConstants
{
    public static readonly string GameDirectory = @"C:\Users\user\AppData\Roaming\.minecraft\";
    
    public const string DataDirectoryName = ".smmanager";
    public const string OverridesDirName = "overrides";
    public const string ManifestFileName = "smmanifest.json";
    public const string IconName = "pack.png";
    
    public static string RoamingDataDirectory => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DataDirectoryName); 
    public static string LocalDataDirectory => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataDirectoryName); 
    public static string StorageDirectory => Path.Join(LocalDataDirectory, "storage");
    public static string ConfigPath => Path.Join(RoamingDataDirectory, "config.json");
    public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
}