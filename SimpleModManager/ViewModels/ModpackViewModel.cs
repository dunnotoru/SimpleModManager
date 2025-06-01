using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace SimpleModManager.ViewModels;

public class ModpackViewModel : ViewModelBase
{
    public string ModpackDirectory { get; }
    public string Name { get; }
    public Bitmap? Logo { get; }

    private ObservableCollection<string> _mods = new ObservableCollection<string>();

    public ModpackViewModel(string modpackDirectory, IEnumerable<string> mods, Bitmap? logo = null)
    {
        DirectoryInfo dir = new DirectoryInfo(modpackDirectory);
        ModpackDirectory = dir.FullName;
        Name = dir.Name;
        Logo = logo;
        _mods = new ObservableCollection<string>(mods);
    }

    public ObservableCollection<string> Mods
    {
        get => _mods;
        set => this.RaiseAndSetIfChanged(ref _mods, value);
    }

    protected override void Dispose(bool dispoing)
    {
        Logo?.Dispose();
        base.Dispose(dispoing);
    }
}