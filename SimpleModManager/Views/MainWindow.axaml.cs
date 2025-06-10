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
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(v => v.DataContext).BindTo(this, v => v.ViewModel);
            ViewModel?.Activator.Activate();
            Debug.WriteLine($"DATA CONTEXT {DataContext}");
            Debug.WriteLine($"VIEWMODEL {ViewModel}");
        });
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainWindowViewModel?)value;
    }

    public MainWindowViewModel? ViewModel { get; set; }
}