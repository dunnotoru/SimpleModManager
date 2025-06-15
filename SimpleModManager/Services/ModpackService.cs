using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleModManager.Models;

namespace SimpleModManager.Services;

public class ModpackService
{
    private readonly ConfigProvider _configProvider;

    public ModpackService(ConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public void CopyFileWithStructure(string targetDirectory, string sourceDirectory, string file)
    {
        string fullTarget = Path.GetFullPath(targetDirectory);
        string fullSource = Path.GetFullPath(sourceDirectory);
        Directory.CreateDirectory(fullTarget);
        if (!File.Exists(file))
        {
            return;
        }

        string relativePath = Path.GetRelativePath(fullSource, file);
        string copy = Path.Join(fullTarget, relativePath);
        string? copyDir = Path.GetDirectoryName(copy);
        if (copyDir is null)
        {
            return;
        }

        Directory.CreateDirectory(copyDir);
        Debug.WriteLine($"Copying file {file} to {copy}");
        File.Copy(file, copy);
    }

    public async Task ClearLoadedFilesAsync()
    {
        Config config = await _configProvider.GetOrCreateAsync();
        foreach (string file in config.LastLoadedFiles.ToList())
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
    }

    public async Task AddLastLoadedFilesAsync(string[] files)
    {
        Config config = await _configProvider.GetOrCreateAsync();
        config.LastLoadedFiles.AddRange(files);
        await _configProvider.SaveAsync(config);
    }

    public void GenerateModpackFileStructure(ManifestInfo manifest)
    {
        Directory.CreateDirectory(manifest.OverrideDirectory);
        using FileStream fs = File.Open(manifest.FilePath, FileMode.OpenOrCreate);
        JsonSerializer.Serialize(fs, manifest, AppConstants.JsonOptions);
    }

    public bool TryReadManifest(string path, [NotNullWhen(true)] out ManifestInfo? manifest)
    {
        manifest = null;
        if (File.Exists(path) == false)
        {
            return false;
        }

        string directory = Path.GetDirectoryName(path)!;
        using FileStream fs = File.Open(path, FileMode.Open);
        manifest = JsonSerializer.Deserialize<ManifestInfo>(fs, AppConstants.JsonOptions);
        if (manifest is null)
        {
            return false;
        }

        manifest.OriginDirectory = directory;

        return true;
    }
}