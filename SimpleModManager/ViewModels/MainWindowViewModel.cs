using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using ReactiveUI;

namespace SimpleModManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
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

    private string? _selectedModpack;
    public string? SelectedModpack
    {
        get => _selectedModpack;
        set => this.RaiseAndSetIfChanged(ref _selectedModpack, value);
    }

    public ReactiveCommand<string, Unit> SetupModpack { get; }
    public ReactiveCommand<Unit, Unit> CreateModpack { get; }
    
    public MainWindowViewModel()
    {
        SetupModpack = ReactiveCommand.Create<string, Unit>(SetupModpackImpl);
        
        Directory.CreateDirectory(_storageDirectory);
    }

    private Unit SetupModpackImpl(string directory)
    {
        
        return Unit.Default;
    }
}