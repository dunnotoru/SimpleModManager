using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SimpleModManager.Models;

namespace SimpleModManager.ViewModels;

public class ModpackViewModel : ViewModelBase
{
    public ManifestInfo Manifest { get; }
    public string ModpackDirectory { get; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string Name { get; }
    public Bitmap? Logo { get; }

    private ObservableCollection<string> _mods = new ObservableCollection<string>();

    public ModpackViewModel(ManifestInfo manifest)
    {
        Manifest = manifest;
        ModpackDirectory = manifest.OriginDirectory;
        Name = manifest.Name ?? "Unnamed";
        Author = manifest.Author ?? "None";
        Version = manifest.Version ?? "None";
        _mods = new ObservableCollection<string>(manifest.Files);
    }


    public ObservableCollection<string> Mods
    {
        get => _mods;
        set => this.RaiseAndSetIfChanged(ref _mods, value);
    }

    protected override void Dispose(bool dispoing)
    {
        Logo?.Dispose();
        base.Dispose(dispoing);
    }
}