using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using SimpleModManager.Services;

namespace SimpleModManager.Models;

public class ManifestInfo
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public List<string> Files { get; set; } = new List<string>();
    
    [JsonIgnore]
    public string OriginDirectory { get; set; }
    [JsonIgnore]
    public string OverrideDirectory => Path.Combine(OriginDirectory, Config.OverridesDirName);

    public ManifestInfo(string originDirectory)
    {
        OriginDirectory = originDirectory;
    }
}