using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using ReactiveUI;
using SimpleModManager.Services;
using SukiUI.Dialogs;

namespace SimpleModManager.ViewModels;

public class DumpModpackForm : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string _author = string.Empty;

    public string Author
    {
        get => _author;
        set => this.RaiseAndSetIfChanged(ref _author, value);
    }

    private string _version = string.Empty;

    public string Version
    {
        get => _version;
        set => this.RaiseAndSetIfChanged(ref _version, value);
    }
    
    public string IconPath { get; set; } = string.Empty;

    public bool Dump { get; private set; } = false;

    private ObservableCollection<FolderItem> _folderItems = new ObservableCollection<FolderItem>();
    public ObservableCollection<FolderItem> FolderItems
    {
        get => _folderItems;
        set => this.RaiseAndSetIfChanged(ref _folderItems, value);
    }
    
    public ReactiveCommand<Unit, Unit> Dismiss { get; }
    public DumpModpackForm(ISukiDialog dialog)
    {
        Dismiss = ReactiveCommand.Create<Unit>(_ =>
        {
            Dump = true;
            dialog.Dismiss();
        });

        this.WhenActivated((CompositeDisposable d) =>
        {
            Debug.WriteLine("DUMPFORM ACTIVATED");
            FolderItems =
                new ObservableCollection<FolderItem>(FolderTreeLoader.LoadFolderContents(Config.GameDirectory));
        });
    }
}