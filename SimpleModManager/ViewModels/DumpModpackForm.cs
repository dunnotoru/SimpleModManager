using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using SimpleModManager.Services;
using SukiUI.Dialogs;

namespace SimpleModManager.ViewModels;

public partial class DumpModpackForm : ReactiveValidationObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new ViewModelActivator();

    [Reactive] private string _name = "New Modpack";
    [Reactive] private string _author = string.Empty;
    [Reactive] private string _version = string.Empty;
    [Reactive] private string? _iconPath = null;
    [Reactive] private ObservableCollection<FolderItem> _folderItems = new ObservableCollection<FolderItem>();

    public bool Dump { get; private set; } = false;

    public ReactiveCommand<Unit, Unit> Submit { get; }

    public DumpModpackForm(ISukiDialog dialog)
    {
        Submit = ReactiveCommand.Create<Unit>(_ =>
        {
            Dump = true;
            dialog.Dismiss();
        }, this.IsValid());

        this.WhenActivated((CompositeDisposable d) =>
        {
            Debug.WriteLine("DUMP FORM ACTIVATED");
            FolderItems =
                new ObservableCollection<FolderItem>(FolderTreeLoader.LoadFolderContents(AppConstants.GameDirectory));
        });
        
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        this.ValidationRule(vm => vm.Name, n => !string.IsNullOrWhiteSpace(n), "No name(");
        this.ValidationRule(vm => vm.Name, n => n!.Length < 30, "Too long name");
        this.ValidationRule(vm => vm.Name, n => n is not null && FileNameRegex().IsMatch(n), "Bad bad bad name");
        this.ValidationRule(vm => vm.Name, n => !Directory.Exists(Path.Combine(AppConstants.StorageDirectory, n!)), "Already Exists");
        
        this.ValidationRule(vm => vm.Author, a => a!.Length < 30, "Too long Author name");
        this.ValidationRule(vm => vm.Version, v => v!.Length < 30, "Too long Version");
    }

    [GeneratedRegex("^[^<>:;,?\"*|/]+$")]
    private static partial Regex FileNameRegex();
}