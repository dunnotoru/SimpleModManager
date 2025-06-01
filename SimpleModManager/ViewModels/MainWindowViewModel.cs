using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SimpleModManager.ViewModels;

public class MainWindowViewModel : ViewModelBase, IActivatableViewModel, IDisposable
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    private string _gameDirectory = "C:/Users/user/AppData/Roaming/.minecraft/";

    private string _storageDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".smmanager");

    private const string ModsDirName = "mods";
    private const string ConfigDirName = "config";
    private const string OverridesDirName = "overrides";

    public string GameDirectory => _gameDirectory;

    private ObservableCollection<ModpackViewModel> _modpacks = new ObservableCollection<ModpackViewModel>();

    public ObservableCollection<ModpackViewModel> Modpacks
    {
        get => _modpacks;
        set => this.RaiseAndSetIfChanged(ref _modpacks, value);
    }

    private ModpackViewModel? _selectedModpack;

    public ModpackViewModel? SelectedModpack
    {
        get => _selectedModpack;
        set => this.RaiseAndSetIfChanged(ref _selectedModpack, value);
    }

    public ReactiveCommand<ModpackViewModel, Unit> LoadModpack { get; }
    public ReactiveCommand<Unit, Unit> DumpCurrent { get; }

    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
    public ISukiToastManager ToastManager { get; } = new SukiToastManager();

    private readonly FileSystemWatcher _watcher;

    public MainWindowViewModel()
    {
        LoadModpack = ReactiveCommand.Create<ModpackViewModel, Unit>(LoadModpackImpl,
            this.WhenAnyValue(vm => vm.SelectedModpack).Select(s => s is not null));
        DumpCurrent = ReactiveCommand.Create(DumpCurrentImpl);

        _watcher = new FileSystemWatcher(_storageDirectory);
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.EnableRaisingEvents = true;

        this.WhenActivated(d =>
        {
            Debug.WriteLine("ACTIVATED");
            _watcher.DisposeWith(d);
            Modpacks = new ObservableCollection<ModpackViewModel>(Directory.GetDirectories(_storageDirectory)
                .Select(ModpackFactory));
        });

        Directory.CreateDirectory(_storageDirectory);
    }

    private ModpackViewModel ModpackFactory(string modpackDirectory)
    {
        DirectoryInfo dir = new DirectoryInfo(modpackDirectory);
        if (dir.Exists == false)
        {
            throw new DirectoryNotFoundException("modpack directory not found");
        }

        //ensure basic structure generated
        DirectoryInfo overrideDir = dir.CreateSubdirectory(OverridesDirName);
        overrideDir.CreateSubdirectory(ModsDirName);
        overrideDir.CreateSubdirectory(ConfigDirName);

        FileInfo[] mods = dir.GetFiles("*.jar", SearchOption.AllDirectories);
        FileInfo[] logos = dir.GetFiles("pack.png", SearchOption.TopDirectoryOnly);

        Bitmap? logo = null;
        if (logos.Length > 0)
        {
            logo = new Bitmap(logos[0].FullName);
        }

        return new ModpackViewModel(modpackDirectory, mods.Select(m => m.Name), logo);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        ModpackViewModel? stored = Modpacks.FirstOrDefault(m => m.ModpackDirectory == e.FullPath);
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

        Modpacks.Add(ModpackFactory(e.FullPath));
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
                string path = Path.Combine(_storageDirectory, form.DirName);

                try
                {
                    DirectoryInfo createdDir = Directory.CreateDirectory(path);
                    createdDir.CreateSubdirectory(OverridesDirName);
                    
                    CopyDirectory(Path.Combine(_gameDirectory, ModsDirName),
                        Path.Combine(createdDir.FullName, OverridesDirName, ModsDirName), true);
                    CopyDirectory(Path.Combine(_gameDirectory, ConfigDirName),
                        Path.Combine(createdDir.FullName, OverridesDirName, ConfigDirName), true);
                }
                catch
                {
                    dialog.Dismiss();
                    ToastManager.CreateToast()
                        .WithTitle("Error while dumping current modpack")
                        .Dismiss().After(TimeSpan.FromSeconds(2), true)
                        .Queue();
                }
            }, true, "Flat", "Accent")
            .TryShow();
    }

    private Unit LoadModpackImpl(ModpackViewModel modpackViewModel)
    {
        var toast = ToastManager.CreateToast()
            .Dismiss().After(TimeSpan.FromSeconds(2), true);
        
        try
        {
            CopyDirectory(Path.Combine(modpackViewModel.ModpackDirectory, OverridesDirName), _gameDirectory, true);
            toast.SetTitle("Success");
        }
        catch (Exception ex)
        {
            toast.SetTitle("Error while loading modpack");
            Debug.WriteLine(ex);
        }

        toast.Queue();

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
    }
}