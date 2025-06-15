using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleModManager.Models;

namespace SimpleModManager.Services;

public class ConfigProvider
{
    public async Task<Config> GetOrCreateAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AppConstants.ConfigPath)!);
        if (File.Exists(AppConstants.ConfigPath) == false)
        {
            Config emptyConfig = new Config();
            await SaveAsync(emptyConfig);
            return emptyConfig;
        }

        try
        {
            await using FileStream fs = File.OpenRead(AppConstants.ConfigPath);
            Config? config = await JsonSerializer.DeserializeAsync<Config>(fs);
            config ??= new Config();
            return config;
        }
        catch (JsonException)
        {
            Debug.WriteLine("NO WAY JSON EXCEPTION WHILE READING CONFIG. CREATING BACKUP BRO");
            string copy = Path.GetFullPath(AppConstants.ConfigPath);
            string copyWithoutExt = Path.GetFileNameWithoutExtension(copy);
            string ext = Path.GetExtension(copy);
            copy = copyWithoutExt + "_backup_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ext;
            File.Copy(AppConstants.ConfigPath, copy);
            
            Config config = new Config();
            await SaveAsync(config);
            return config;
        }
    }

    public async Task SaveAsync(Config config)
    {
        await using FileStream fs = File.Open(AppConstants.ConfigPath, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(fs, config, AppConstants.JsonOptions);
    }
}