using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleModManager.Services;

namespace SimpleModManager.Models;

public class ManifestInfo
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    
    [JsonIgnore]
    public string OriginDirectory { get; set; }
    [JsonIgnore]
    public string OverrideDirectory => Path.Combine(OriginDirectory, Config.OverridesDirName);

    public ManifestInfo(string originDirectory)
    {
        OriginDirectory = originDirectory;
    }

    public void GenerateDirectories()
    {
        Directory.CreateDirectory(OriginDirectory);
        Directory.CreateDirectory(OverrideDirectory);
        string json = JsonSerializer.Serialize(this, Config.JsonOptions);
        File.WriteAllText(Path.Combine(OriginDirectory, Config.ManifestFileName), json);
    }
}