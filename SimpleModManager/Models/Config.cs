using System.Collections.Generic;

namespace SimpleModManager.Models;

public class Config
{
    public List<string> LastLoadedFiles { get; set; } = new List<string>();
}