using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using ReactiveUI;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.MessageBox;

namespace SimpleModManager.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private string _gameDirectory = "C:/Users/user/AppData/Roaming/.minecraft/";
    private string _storageDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".smmanager");

    private const string ModsDirName = "mods";
    private const string ConfigDirName = "config";

    private string GameModsDir => Path.Combine(_gameDirectory, ModsDirName);
    private string GameConfigDir => Path.Combine(_gameDirectory, ConfigDirName);

    public string GameDirectory
    {
        get => _gameDirectory;
        set => this.RaiseAndSetIfChanged(ref _gameDirectory, value);
    }

    private ObservableCollection<Modpack> _modpacks = new ObservableCollection<Modpack>();

    public ObservableCollection<Modpack> Modpacks
    {
        get => _modpacks;
        set => this.RaiseAndSetIfChanged(ref _modpacks, value);
    }
    
    private Modpack? _selectedModpack;

    public Modpack? SelectedModpack
    {
        get => _selectedModpack;
        set => this.RaiseAndSetIfChanged(ref _selectedModpack, value);
    }

    public ReactiveCommand<Modpack, Unit> SaveModpack { get; }
    public ReactiveCommand<Modpack, Unit> LoadModpack { get; }
    public ReactiveCommand<Unit, Unit> CreateModpack { get; }
    public ReactiveCommand<Unit, Unit> DumpCurrent { get; }
    public ISukiDialogManager DialogManager { get; }

    private FileSystemWatcher _watcher;

    public MainWindowViewModel()
    {
        DialogManager = new SukiDialogManager();
        LoadModpack = ReactiveCommand.Create<Modpack, Unit>(LoadModpackImpl);
        CreateModpack = ReactiveCommand.Create(CreateModpackImpl);
        DumpCurrent = ReactiveCommand.Create(DumpCurrentImpl);
        
        _watcher = new FileSystemWatcher(_storageDirectory);
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.EnableRaisingEvents = true;

        string[] dirs = Directory.GetDirectories(_storageDirectory);
        Modpacks = new ObservableCollection<Modpack>(ReadFromStorage());

        Directory.CreateDirectory(_storageDirectory);
    }

    private IEnumerable<Modpack> ReadFromStorage()
    {
        return Directory.GetDirectories(_storageDirectory).Select(dir => new Modpack(dir));
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        Modpack? stored = Modpacks.FirstOrDefault(m => m.ModpackDirectory == e.FullPath);
        if (stored is not null)
        {
            Modpacks.Remove(stored);
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!Directory.Exists(e.FullPath))
        {
            return;
        }
        
        Modpacks.Add(new Modpack(e.FullPath));
    }

    private void DumpCurrentImpl()
    {
        DumpModpackForm form = new DumpModpackForm();
        DialogManager.CreateDialog()
            .WithTitle("Dump Current Modpack")
            .WithContent(form)
            .Dismiss().ByClickingBackground()
            .WithActionButton("Dump", dialog =>
            {
                //validate dirname
                
                string path = Path.Combine(_storageDirectory, form.DirName);
                DirectoryInfo createdDir = Directory.CreateDirectory(path);
                CopyDirectory(Path.Combine(_gameDirectory, ModsDirName), Path.Combine(createdDir.FullName, ModsDirName), true);
                CopyDirectory(Path.Combine(_gameDirectory, ConfigDirName), Path.Combine(createdDir.FullName, ConfigDirName), true);
            }, true, "Flat", "Accent")
            .TryShow();
    }
    
    private void CreateModpackImpl()
    {
        
    }

    private Unit LoadModpackImpl(Modpack modpack)
    {
        return Unit.Default;
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        _watcher.Dispose();
        LoadModpack.Dispose();
        CreateModpack.Dispose();
        DumpCurrent.Dispose();
    }
}
