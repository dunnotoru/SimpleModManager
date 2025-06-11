using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

    public async Task LoadModpackAsync(IProgress<double> progress, ManifestInfo manifest)
    {
        EnsureDataDirectoryCreated();
        string json = await File.ReadAllTextAsync(Config.ConfigPath);
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

        
        await Task.Run(() =>
        {
            string[] files = Directory.GetFiles(manifest.OverrideDirectory, "*.*", SearchOption.AllDirectories);
            double step = 90.0 / files.Length;
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                string relativePath = Path.GetRelativePath(manifest.OverrideDirectory, file);
                string copy = Path.Combine(Config.GameDirectory, relativePath);
                string? dir = Path.GetDirectoryName(copy);
                Debug.WriteLine(dir);
                if (dir is not null)
                {
                    Directory.CreateDirectory(dir);
                }

                File.Copy(file, copy, true);
                config.LastLoadedFiles.Add(copy);
                progress.Report(i * step);
            }
        });

        await File.WriteAllTextAsync(Config.ConfigPath, JsonSerializer.Serialize(config, Config.JsonOptions));
        progress.Report(100);
    }

    public async Task<ManifestInfo> CreateModpackAsync(IProgress<double> progress, string directory,
        string[] filesToCopy, string? name = null,
        string? author = null, string? version = null, string? iconPath = null)
    {
        ManifestInfo manifest = new ManifestInfo(directory)
        {
            Name = name,
            Author = author,
            Version = version
        };

        manifest.GenerateDirectories();
        
        double step = 90.0 / filesToCopy.Length;

        await Task.Run(() =>
        {
            for (var i = 0; i < filesToCopy.Length; i++)
            {
                var file = filesToCopy[i];
                string relativePath = Path.GetRelativePath(Config.GameDirectory, file);
                string copy = Path.Combine(directory, Config.OverridesDirName, relativePath);
                string? dir = Path.GetDirectoryName(copy);
                if (dir is not null)
                {
                    Directory.CreateDirectory(dir);
                }

                File.Copy(file, copy, true);
                progress.Report(i * step);
            }
    
            if (File.Exists(iconPath))
            {
                File.Copy(iconPath, Path.Combine(manifest.OriginDirectory, "pack.png"));
            }
        });

        string serializedManifest = JsonSerializer.Serialize(manifest, Config.JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(directory, Config.ManifestFileName), serializedManifest);
        progress.Report(100);

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