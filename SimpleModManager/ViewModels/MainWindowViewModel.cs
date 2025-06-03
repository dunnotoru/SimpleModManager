using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using ReactiveUI;
using SimpleModManager.Models;
using SimpleModManager.Services;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SimpleModManager.ViewModels;

public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();

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
    
    private bool _isListLoaded = false;
    public bool IsListLoaded
    {
        get => _isListLoaded;
        set => this.RaiseAndSetIfChanged(ref _isListLoaded, value);
    }
    
    public ReactiveCommand<ModpackViewModel, Unit> LoadModpack { get; }
    public ReactiveCommand<Unit, Unit> DumpCurrent { get; }

    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
    public ISukiToastManager ToastManager { get; } = new SukiToastManager();

    private readonly FileSystemWatcher _watcher;
    private readonly ModpackService _modpackService;
    
    public MainWindowViewModel()
    {
        LoadModpack = ReactiveCommand.Create<ModpackViewModel, Unit>(LoadModpackImpl,
            this.WhenAnyValue(vm => vm.SelectedModpack).Select(s => s is not null));
        DumpCurrent = ReactiveCommand.Create(DumpCurrentImpl);

        _modpackService = new ModpackService();
        _watcher = new FileSystemWatcher();
        this.WhenActivated(d =>
        {
            Debug.WriteLine("ACTIVATED");
            _modpackService.EnsureDataDirectoryCreated();
            
            _watcher.Path = Config.StorageDirectory;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.DisposeWith(d);
            ScanStorage();
            IsListLoaded = true;
        });
    }

    private void ScanStorage()
    {
        _modpackService.EnsureDataDirectoryCreated();
        string[] directories = Directory.GetDirectories(Config.StorageDirectory);
        foreach (string dir in directories)
        {
            if (_modpackService.TryReadModpack(dir, out ManifestInfo? manifest))
            {
                Modpacks.Add(new ModpackViewModel(manifest!));
            }
            else
            {
                Debug.WriteLine("Failed to read modpack");
                ToastManager.CreateToast()
                    .WithTitle("Failed to read modpack")
                    .WithContent(dir);
            }
        }
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
        Debug.WriteLine("ON CREATED {0} {1} {2}", e.Name, e.ChangeType, e.FullPath);
        if (!Directory.Exists(e.FullPath))
        {
            return;
        }

        if (_modpackService.TryReadModpack(e.FullPath, out ManifestInfo? manifest))
        {
            Modpacks.Add(new ModpackViewModel(manifest!));
        }
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
                string path = Path.Combine(Config.StorageDirectory, form.DirName);
                _watcher.Created -= OnCreated;
                
                try
                {
                    string[] filesToCopy = Directory.GetFiles(Path.Combine(Config.GameDirectory, Config.ModsDirName))
                        .Concat(Directory.GetFiles(Path.Combine(Config.GameDirectory, Config.ConfigDirName))).ToArray();
                    
                    _modpackService.GenerateModpack(path, filesToCopy);
                    
                    if (_modpackService.TryReadModpack(path, out ManifestInfo? manifest))
                    {
                        Modpacks.Add(new ModpackViewModel(manifest!));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    dialog.Dismiss();
                    ToastManager.CreateToast()
                        .WithTitle("Error while dumping current modpack")
                        .Dismiss().After(TimeSpan.FromSeconds(2))
                        .Queue();
                }
                finally
                {
                    _watcher.Created += OnCreated;
                }
            }, true, "Flat", "Accent")
            .TryShow();
    }

    private Unit LoadModpackImpl(ModpackViewModel modpackViewModel)
    {
        SukiToastBuilder toast = ToastManager.CreateToast()
            .Dismiss().After(TimeSpan.FromSeconds(2));

        try
        {
            _modpackService.LoadModpack(modpackViewModel.Manifest);
            
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
}