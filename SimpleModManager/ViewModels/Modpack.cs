using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace SimpleModManager.ViewModels;

public class Modpack : ViewModelBase
{
    public string ModpackDirectory { get; }
    public string Name { get; }
    public Bitmap? Logo { get; }

    private ObservableCollection<string> _mods = new ObservableCollection<string>();
    public ObservableCollection<string> Mods
    {
        get => _mods;
        set => this.RaiseAndSetIfChanged(ref _mods, value);
    }

    public Modpack(string modpackDirectory)
    {
        DirectoryInfo dir = new DirectoryInfo(modpackDirectory);
        if (dir.Exists == false)
        {
            throw new DirectoryNotFoundException("modpack directory not found");
        }
        
        ModpackDirectory = modpackDirectory;
        Name = dir.Name;

        Directory.CreateDirectory(Path.Combine(ModpackDirectory, "mods"));

        string[] mods = Directory.GetFiles(Path.Combine(ModpackDirectory, "mods"), "*.jar", SearchOption.AllDirectories); //TODO: make lazy 
        string[] logos = Directory.GetFiles(ModpackDirectory, "pack.png", SearchOption.TopDirectoryOnly); //TODO: make lazy

        if (logos.Length > 0)
        {
            Logo = new Bitmap(logos[0]);
        }
        
        Mods = new ObservableCollection<string>(mods);
    }

    protected override void Dispose(bool dispoing)
    {
        Logo?.Dispose();
        base.Dispose(dispoing);
    }
}