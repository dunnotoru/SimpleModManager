using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SimpleModManager.Models;
using SimpleModManager.Services;

namespace SimpleModManager.ViewModels;

public partial class ModpackViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();
    public ManifestInfo Manifest { get; }

    [Reactive] private string _modpackDirectory;
    [Reactive] private string _author;
    [Reactive] private string _version;
    [Reactive] private string _name;
    public Bitmap? Logo { get; private set; }

    [Reactive] private ObservableCollection<FolderItem> _folderContents = new ObservableCollection<FolderItem>();
    
    public ReactiveCommand<Unit, Unit> LoadFolderContents { get; }
    
    public ModpackViewModel(ManifestInfo manifest)
    {
        Manifest = manifest;
        _modpackDirectory = manifest.OriginDirectory;
        _name = manifest.Name ?? "Unnamed";
        Author = manifest.Author ?? "None";
        Version = manifest.Version ?? "None";
        
        LoadFolderContents = ReactiveCommand.CreateFromTask(LoadFolderContentsImpl);

        Debug.WriteLine("MODPACK CTOR YAYYYYYY");
        this.WhenActivated(d =>
        {
            Debug.WriteLine("MODPACK ACTIVATED YAYYYYYY");
            LoadFolderContents.Execute().Subscribe();
            Logo?.DisposeWith(d);
        });
    }

    
    private async Task LoadFolderContentsImpl()
    {
        await Task.Run(() =>
        {
            Directory.CreateDirectory(_modpackDirectory);
            FolderContents =
                new ObservableCollection<FolderItem>(FolderTreeLoader.LoadFolderContents(_modpackDirectory));
            string iconPath = Path.Combine(_modpackDirectory, "pack.png");
            if (File.Exists(iconPath))
            {
                Logo?.Dispose();
                Logo = new Bitmap(iconPath);
            }
        });
    }
}