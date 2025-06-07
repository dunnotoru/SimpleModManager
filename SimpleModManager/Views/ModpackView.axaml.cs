using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleModManager.ViewModels;

namespace SimpleModManager.Views;

public partial class ModpackView : ReactiveUserControl<ModpackViewModel>
{
    public ModpackView()
    {
        InitializeComponent();
        this.WhenActivated(_ =>
        {
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        });
    }
}