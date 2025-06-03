using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using SimpleModManager.Models;
using SimpleModManager.ViewModels;

namespace SimpleModManager.Services;

public class ModpackService
{
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };
    
    public void EnsureDataDirectoryCreated()
    {
        Directory.CreateDirectory(Config.StorageDirectory);
        if (File.Exists(Config.ConfigPath) == false)
        {
            using StreamWriter sw = File.CreateText(Config.ConfigPath);
            ConfigData data = new ConfigData();
            sw.WriteLine(JsonSerializer.Serialize(data, _jsonOptions));
        }
        else
        {
            using StreamReader sr = File.OpenText(Config.ConfigPath);
            ConfigData? data = JsonSerializer.Deserialize<ConfigData>(sr.ReadToEnd(), _jsonOptions);
            if (data is null)
            {
                data = new ConfigData();
                using StreamWriter sw = File.CreateText(Config.ConfigPath);
                sw.WriteLine(JsonSerializer.Serialize(data, _jsonOptions));
            }
        }
    }

    public void LoadModpack(ManifestInfo manifest)
    {
        EnsureDataDirectoryCreated();
        string json = File.ReadAllText(Config.ConfigPath);
        ConfigData config = JsonSerializer.Deserialize<ConfigData>(json) ?? new ConfigData();

        foreach (string file in config.LastLoadedFiles.ToList())
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                    config.LastLoadedFiles.Remove(file);
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                config.LastLoadedFiles.Remove(file);
            }
        }
        
        string[] files = Directory.GetFiles(manifest.OverrideDirectory, "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string copy = file.Replace(manifest.OverrideDirectory, Config.GameDirectory);
            string? dir = Path.GetDirectoryName(copy);
            Debug.WriteLine(dir);
            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }
            
            File.Copy(file, copy, true);
            config.LastLoadedFiles.Add(copy);
        }

        File.WriteAllText(Config.ConfigPath, JsonSerializer.Serialize(config, _jsonOptions));
    }
    
    public void GenerateModpack(string directory, string[] filesToCopy)
    {
        Directory.CreateDirectory(directory);
        
        ManifestInfo manifest = new ManifestInfo(directory)
        {
            Name = directory,
            Author = "Dev",
        };

        foreach (string file in filesToCopy)
        {
            string copy = Path.Combine(directory, Config.OverridesDirName, file.Replace(Config.GameDirectory, string.Empty));
            string? dir = Path.GetDirectoryName(copy);
            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }
            
            File.Copy(file, copy, true);
            manifest.Files.Add(copy);
        }

        string serializedManifest = JsonSerializer.Serialize(manifest, _jsonOptions);
        File.WriteAllText(Path.Combine(directory, Config.ManifestFileName), serializedManifest);
    }
    
    public bool TryReadModpack(string directory, out ManifestInfo? manifest)
    {
        manifest = null;
        string[] manifests = Directory.GetFiles(directory, Config.ManifestFileName, SearchOption.TopDirectoryOnly);
        if (manifests.Length != 1)
        {
            return false;
        }

        try
        {
            using StreamReader sr = new StreamReader(manifests[0]);
            manifest = JsonSerializer.Deserialize<ManifestInfo>(sr.ReadToEnd());
            if (manifest is null)
            {
                return false;
            }
            
            manifest.OriginDirectory = directory;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return false;
        }

        return true;
    }
}