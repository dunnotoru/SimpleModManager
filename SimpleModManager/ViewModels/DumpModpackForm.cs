using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SimpleModManager.Services;
using SukiUI.Dialogs;

namespace SimpleModManager.ViewModels;

public partial class DumpModpackForm : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();
    
    [Reactive] private string _name = string.Empty;
    [Reactive] private string _author = string.Empty;
    [Reactive] private string _version = string.Empty;
    [Reactive] private string? _iconPath = null;
    [Reactive] private ObservableCollection<FolderItem> _folderItems = new ObservableCollection<FolderItem>();

    public bool Dump { get; private set; } = false;

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
            Debug.WriteLine("DUMP FORM ACTIVATED");
            FolderItems =
                new ObservableCollection<FolderItem>(FolderTreeLoader.LoadFolderContents(Config.GameDirectory));
        });
    }
}