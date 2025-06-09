using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SimpleModManager.ViewModels;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace SimpleModManager.Views;

public partial class MainWindow : SukiWindow, IViewFor<MainWindowViewModel>
{
    private ISukiDialogManager DialogManager { get; set; } = new SukiDialogManager();
    public MainWindow()
    {
        InitializeComponent();
        DialogHost.Manager = DialogManager;
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(v => v.DataContext).BindTo(this, v => v.ViewModel);
            ViewModel?.Activator.Activate();
            Debug.WriteLine($"DATA CONTEXT {DataContext}");
            Debug.WriteLine($"VIEWMODEL {ViewModel}");
            ViewModel?.DumpInteraction.RegisterHandler(DumpModpackHandler);
        });
    }

    private async Task DumpModpackHandler(IInteractionContext<Unit, DumpModpackForm> obj)
    {
        SukiDialogBuilder builder = DialogManager.CreateDialog();
        DumpModpackForm form = new DumpModpackForm(builder.Dialog);
        await builder.WithViewModel(_ => form)
            .WithOkResult(null)
            .Dismiss().ByClickingBackground()
            .TryShowAsync();
        
        obj.SetOutput(form);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainWindowViewModel?)value;
    }

    public MainWindowViewModel? ViewModel { get; set; }
}