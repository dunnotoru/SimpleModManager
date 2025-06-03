using System.Collections.Generic;

namespace SimpleModManager.Models;

public class ConfigData
{
    public List<string> LastLoadedFiles { get; set; } = new List<string>();
}