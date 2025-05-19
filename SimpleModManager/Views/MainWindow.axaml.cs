using Avalonia.Controls;
using Avalonia.ReactiveUI;
using SimpleModManager.ViewModels;

namespace SimpleModManager.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        ViewModel?.Dispose();
    }
}