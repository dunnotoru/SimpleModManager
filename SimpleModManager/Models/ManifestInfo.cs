using System.IO;
using System.Text.Json.Serialization;
using SimpleModManager.Services;

namespace SimpleModManager.Models;

public class ManifestInfo
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }

    [JsonIgnore] public string OriginDirectory { get; set; }
    [JsonIgnore] public string OverrideDirectory => Path.Join(OriginDirectory, AppConstants.OverridesDirName);
    [JsonIgnore] public string FilePath => Path.Join(OriginDirectory, AppConstants.ManifestFileName);

    public ManifestInfo(string originDirectory)
    {
        OriginDirectory = Path.GetFullPath(originDirectory);
    }

    [JsonConstructor]
    private ManifestInfo()
    {
        OriginDirectory = string.Empty;
    }
}