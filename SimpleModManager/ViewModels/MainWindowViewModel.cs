using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SimpleModManager.Models;
using SimpleModManager.Services;
using SimpleModManager.Utils;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace SimpleModManager.ViewModels;

public partial class MainWindowViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    [Reactive] private ObservableCollection<ModpackViewModel> _modpacks = new ObservableCollection<ModpackViewModel>();
    [Reactive] private ModpackViewModel? _selectedModpack;
    [Reactive] private bool _isBusyDumping;
    [Reactive] private bool _isBusyLoading;
    [Reactive] private bool _isModpacksLoaded;

    public ReactiveCommand<ModpackViewModel, Unit> LoadModpack { get; }
    public ReactiveCommand<Unit, Unit> DumpCurrent { get; }
    public ReactiveCommand<ModpackViewModel, Unit> OpenDirectory { get; }

    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();
    public ISukiToastManager ToastManager { get; } = new SukiToastManager();

    private readonly FileSystemWatcher _watcher;
    private readonly ModpackService _modpackService;
    
    public MainWindowViewModel()
    {
        _modpackService = new ModpackService();
        _watcher = new FileSystemWatcher();
        
        IObservable<bool> isSelected = this.WhenAnyValue(vm => vm.SelectedModpack).Select(s => s is not null);
        OpenDirectory = ReactiveCommand.Create<ModpackViewModel>(OpenDirectoryImpl, isSelected);

        LoadModpack = ReactiveCommand.CreateFromTask<ModpackViewModel>(LoadModpackImpl, isSelected);
        DumpCurrent = ReactiveCommand.CreateFromTask(DumpCurrentImpl, LoadModpack.IsExecuting.Select(x => !x));

        this.WhenActivated(d =>
        {
            Debug.WriteLine("MAIN VIEWMODEL ACTIVATED");
            _modpackService.EnsureDataDirectoryCreated();

            _watcher.Path = Config.StorageDirectory;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.DisposeWith(d);

            ScanStorage();
        });
    }

    private void OpenDirectoryImpl(ModpackViewModel modpack)
    {
        Process.Start(new ProcessStartInfo(modpack.ModpackDirectory) { UseShellExecute = true });
    }

    private void ScanStorage()
    {
        _modpackService.EnsureDataDirectoryCreated();
        string[] directories = Directory.GetDirectories(Config.StorageDirectory);
        foreach (string dir in directories)
        {
            if (_modpackService.TryReadManifest(dir, out ManifestInfo? manifest))
            {
                ModpackViewModel pack = new ModpackViewModel(manifest!);
                Modpacks.Add(pack);
                pack.Activator.Activate();
            }
            else
            {
                Debug.WriteLine("Failed to read modpack");
                ToastManager.CreateToast()
                    .WithTitle("Failed to read modpack")
                    .WithContent(dir);
            }
        }

        IsModpacksLoaded = true;
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath == Config.StorageDirectory)
        {
            _modpackService.EnsureDataDirectoryCreated();
            return;
        }

        ModpackViewModel? stored = Modpacks.FirstOrDefault(m => m.ModpackDirectory == e.FullPath);
        if (stored is not null)
        {
            stored.Activator.Deactivate();
            Modpacks.Remove(stored);
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        Debug.WriteLine("ON CREATED {0} {1} {2}", e.Name, e.ChangeType, e.FullPath);

        if (_modpackService.TryReadManifest(e.FullPath, out ManifestInfo? manifest))
        {
            ModpackViewModel pack = new ModpackViewModel(manifest!);
            Modpacks.Add(pack);
            pack.Activator.Activate();
        }
    }

    private async Task DumpCurrentImpl()
    {
        SukiDialogBuilder builder = DialogManager.CreateDialog();
        DumpModpackForm form = new DumpModpackForm(builder.Dialog);

        await builder.WithViewModel(_ => form)
            .WithOkResult(null)
            .Dismiss().ByClickingBackground()
            .TryShowAsync();

        if (form.Dump == false)
        {
            return;
        }

        string path = Path.Combine(Config.StorageDirectory, form.Name);
        _watcher.Created -= OnCreated;

        string[] filesToCopy = FolderTreeLoader.GetAllItems(form.FolderItems)
            .Where(i => i is { IsChecked: true, IsDirectory: false })
            .Select(i => i.Path).ToArray();

        Progress<double> progress = new Progress<double>();
        ISukiToast loadingToast = ToastManager.CreateToast()
            .WithTitle("Copying files...")
            .WithContent(new CopyProgressViewModel(progress))
            .Toast;
        
        try
        {
            ToastManager.Queue(loadingToast);
            ManifestInfo manifest = await _modpackService.CreateModpackAsync(progress,
                path,
                filesToCopy,
                form.Name,
                form.Author,
                form.Version,
                form.IconPath);
            Modpacks.Add(new ModpackViewModel(manifest));
            ToastManager.CreateToast()
                .WithTitle("Success")
                .Dismiss().After(TimeSpan.FromSeconds(2))
                .Queue();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            ToastManager.CreateToast()
                .WithTitle("Error while dumping current modpack")
                .Dismiss().After(TimeSpan.FromSeconds(2))
                .Queue();
        }
        finally
        {
            ToastManager.Dismiss(loadingToast);
            _watcher.Created += OnCreated;
        }
    }

    private async Task LoadModpackImpl(ModpackViewModel modpackViewModel)
    {
        SukiToastBuilder toast = ToastManager.CreateToast()
            .Dismiss().After(TimeSpan.FromSeconds(2));

        Progress<double> progress = new Progress<double>();
        ISukiToast loadingToast = ToastManager.CreateToast()
            .WithTitle("Copying files...")
            .WithContent(new CopyProgressViewModel(progress))
            .Toast;

        try
        {
            ToastManager.Queue(loadingToast);
            await _modpackService.LoadModpackAsync(progress, modpackViewModel.Manifest);
            toast.SetTitle("Success");
        }
        catch (Exception ex)
        {
            toast.SetTitle("Error while loading modpack");
            Debug.WriteLine(ex);
        }
        finally
        {
            ToastManager.Dismiss(loadingToast);
        }

        toast.Queue();
    }
}