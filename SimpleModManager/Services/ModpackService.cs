using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using SimpleModManager.Models;

namespace SimpleModManager.Services;

public class ModpackService
{
    public void EnsureDataDirectoryCreated()
    {
        Directory.CreateDirectory(Config.StorageDirectory);
        if (File.Exists(Config.ConfigPath) == false)
        {
            using StreamWriter sw = File.CreateText(Config.ConfigPath);
            ConfigData data = new ConfigData();
            sw.WriteLine(JsonSerializer.Serialize(data, Config.JsonOptions));
        }
        else
        {
            using StreamReader sr = File.OpenText(Config.ConfigPath);
            ConfigData? data = JsonSerializer.Deserialize<ConfigData>(sr.ReadToEnd(), Config.JsonOptions);
            if (data is null)
            {
                data = new ConfigData();
                using StreamWriter sw = File.CreateText(Config.ConfigPath);
                sw.WriteLine(JsonSerializer.Serialize(data, Config.JsonOptions));
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

        File.WriteAllText(Config.ConfigPath, JsonSerializer.Serialize(config, Config.JsonOptions));
    }
    
    public ManifestInfo CreateModpack(string directory, string[] filesToCopy, string? name = null, string? author = null, string? version = null, string? iconPath = null)
    {
        ManifestInfo manifest = new ManifestInfo(directory)
        {
            Name = name,
            Author = author,
            Version = version
        };
        
        manifest.GenerateDirectories();
        
        foreach (string file in filesToCopy)
        {
            string copy = Path.Combine(directory, Config.OverridesDirName, file.Replace(Config.GameDirectory, string.Empty));
            string? dir = Path.GetDirectoryName(copy);
            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }
            
            File.Copy(file, copy, true);
        }

        if (File.Exists(iconPath))
        {
            File.Copy(iconPath, Path.Combine(manifest.OriginDirectory, "pack.png"));
        }

        string serializedManifest = JsonSerializer.Serialize(manifest, Config.JsonOptions);
        File.WriteAllText(Path.Combine(directory, Config.ManifestFileName), serializedManifest);

        return manifest;
    }
    
    public bool TryReadManifest(string directory, out ManifestInfo? manifest)
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