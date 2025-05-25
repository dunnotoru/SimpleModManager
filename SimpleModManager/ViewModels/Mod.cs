namespace SimpleModManager.ViewModels;

public class Mod : ViewModelBase
{
    public string Name { get; }
    public bool IsNew { get; }

    public Mod(string modFile)
    {
        
    }
}