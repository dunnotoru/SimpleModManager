using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SimpleModManager.Models;
using SimpleModManager.Services;

namespace SimpleModManager.ViewModels;



public class ModpackViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();
    public ManifestInfo Manifest { get; }
    public string ModpackDirectory { get; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string Name { get; }
    public Bitmap? Logo { get; private set; }

    private ObservableCollection<FolderItem> _folderContents = new ObservableCollection<FolderItem>();

    public ModpackViewModel(ManifestInfo manifest)
    {
        Manifest = manifest;
        ModpackDirectory = manifest.OriginDirectory;
        Name = manifest.Name ?? "Unnamed";
        Author = manifest.Author ?? "None";
        Version = manifest.Version ?? "None";
        
        Debug.WriteLine("MODPACK CTOR YAYYYYYY");
        this.WhenActivated((CompositeDisposable d) =>
        {
            Debug.WriteLine("MODPACK ACTIVATED YAYYYYYY");
            LoadFolderContents();
        });
    }

    public void LoadFolderContents()
    {
        Directory.CreateDirectory(ModpackDirectory);
        FolderContents = new ObservableCollection<FolderItem>(FolderTreeLoader.LoadFolderContents(ModpackDirectory));
        string iconPath = Path.Combine(ModpackDirectory, "pack.png");
        if (File.Exists(iconPath))
        {
            Logo = new Bitmap(iconPath);
        }
    }
    
    public ObservableCollection<FolderItem> FolderContents
    {
        get => _folderContents;
        set => this.RaiseAndSetIfChanged(ref _folderContents, value);
    }

    protected override void Dispose(bool dispoing)
    {
        Logo?.Dispose();
        base.Dispose(dispoing);
    }
}