using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace SimpleModManager.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private string _gameDirectory = "C:/Users/user/AppData/Roaming/.minecraft/";
    private string _storageDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".smmanager");

    public string GameDirectory
    {
        get => _gameDirectory;
        set => this.RaiseAndSetIfChanged(ref _gameDirectory, value);
    }

    private ObservableCollection<string> _modpackDirectories = new ObservableCollection<string>();

    public ObservableCollection<string> ModpackDirectories
    {
        get => _modpackDirectories;
        set => this.RaiseAndSetIfChanged(ref _modpackDirectories, value);
    }

    private string? _selectedDirectory;

    public string? SelectedDirectory
    {
        get => _selectedDirectory;
        set => this.RaiseAndSetIfChanged(ref _selectedDirectory, value);
    }

    private readonly ObservableAsPropertyHelper<Modpack?> _selectedModpack;
    public Modpack? SelectedModpack => _selectedModpack.Value;

    public ReactiveCommand<string, Unit> SetupModpack { get; }
    public ReactiveCommand<Unit, Unit> CreateModpack { get; }
    public ReactiveCommand<Unit, Unit> DumpCurrent { get; }
    
    private FileSystemWatcher _watcher;

    public MainWindowViewModel()
    {
        SetupModpack = ReactiveCommand.Create<string, Unit>(SetupModpackImpl);
        CreateModpack = ReactiveCommand.Create(CreateModpackImpl);
        DumpCurrent = ReactiveCommand.Create(DumpCurrentImpl);
        _selectedModpack = this.WhenAnyValue(vm => vm.SelectedDirectory)
                       .WhereNotNull()
                       .Select(path => new Modpack(path))
                       .ToProperty(this, vm => vm.SelectedModpack);

        
        _watcher = new FileSystemWatcher(_storageDirectory);
        _watcher.NotifyFilter = NotifyFilters.FileName;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;

        string[] dirs = Directory.GetDirectories(_storageDirectory);
        ModpackDirectories = new ObservableCollection<string>(dirs);

        Directory.CreateDirectory(_storageDirectory);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (!Directory.Exists(e.FullPath))
        {
            return;
        }
        
        ModpackDirectories.Add(e.FullPath);
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!Directory.Exists(e.FullPath))
        {
            return;
        }
        
        ModpackDirectories.Remove(e.FullPath);
    }

    private void DumpCurrentImpl()
    {
        const string modsDir = "mods";
        const string configDir = "config";
        string path = Path.Combine(_storageDirectory, Guid.NewGuid().ToString());
        DirectoryInfo createdDir = Directory.CreateDirectory(path);
        CopyDirectory(Path.Combine(_gameDirectory, modsDir), Path.Combine(createdDir.FullName, modsDir), true);
        CopyDirectory(Path.Combine(_gameDirectory, configDir), Path.Combine(createdDir.FullName, configDir), true);
    }

    private void CreateModpackImpl()
    {
        
    }

    private Unit SetupModpackImpl(string directory)
    {
        DirectoryInfo info = new DirectoryInfo(directory);
        if (info.Exists == false)
        {
            return Unit.Default;
        }

        CopyDirectory(info.FullName, _gameDirectory, true);

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

    public void Dispose()
    {
        _watcher.Dispose();
        _selectedModpack.Dispose();
        SetupModpack.Dispose();
        CreateModpack.Dispose();
        DumpCurrent.Dispose();
    }
}

public class Modpack : ViewModelBase
{
    public string ModpackDirectory;

    private ObservableCollection<string> _mods = new ObservableCollection<string>();

    public ObservableCollection<string> Mods
    {
        get => _mods;
        set => this.RaiseAndSetIfChanged(ref _mods, value);
    }

    public Modpack(string modpackDirectory)
    {
        ModpackDirectory = modpackDirectory;
        string[] mods = Directory.GetFiles(ModpackDirectory, "*.jar", SearchOption.AllDirectories); 
        Mods = new ObservableCollection<string>(mods);
    }
}