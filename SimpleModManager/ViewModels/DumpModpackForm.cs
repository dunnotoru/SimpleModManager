using ReactiveUI;

namespace SimpleModManager.ViewModels;

public class DumpModpackForm : ViewModelBase
{
    private string _dirName = string.Empty;
    public string DirName
    {
        get => _dirName;
        set => this.RaiseAndSetIfChanged(ref _dirName, value);
    }
}