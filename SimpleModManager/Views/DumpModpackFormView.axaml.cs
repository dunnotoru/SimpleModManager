using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleModManager.ViewModels;

namespace SimpleModManager.Views;

public partial class DumpModpackFormView : ReactiveUserControl<DumpModpackForm>
{
    public DumpModpackFormView()
    {
        InitializeComponent();
        
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(v => v.DataContext).BindTo(this, v => v.ViewModel);

            DropButton.AddDisposableHandler(DragDrop.DropEvent, (s, e) =>
            {
                string? iconPath = e.Data.GetFiles()?.FirstOrDefault()?.Path.AbsolutePath;
                if (iconPath is not null)
                {
                    ViewModel!.IconPath = iconPath;
                }
        
                DropButton.Classes.Remove("dropping");
            }).DisposeWith(d);
            DropButton.AddDisposableHandler(DragDrop.DragOverEvent, (s, e) =>
            {
                e.DragEffects = ValidateDrop(e);
            }).DisposeWith(d);
            DropButton.AddDisposableHandler(DragDrop.DragEnterEvent, (s,e) =>
            {
                e.DragEffects = ValidateDrop(e);
                if (e.DragEffects != DragDropEffects.None)
                {
                    DropButton.Classes.Add("dropping");
                }
            }).DisposeWith(d);
            DropButton.AddDisposableHandler(DragDrop.DragLeaveEvent, (_, _) =>
            {
                DropButton.Classes.Remove("dropping");
            }).DisposeWith(d);
        });
    }

    private static DragDropEffects ValidateDrop(DragEventArgs e)
    {
        DragDropEffects result = DragDropEffects.None;
        if (e.Data.Contains(DataFormats.Files) == false)
        {
            return result;
        }

        IStorageItem[] items = e.Data.GetFiles()?.ToArray() ?? Array.Empty<IStorageItem>();
        if (items.Length != 1)
        {
            return result;
        }

        if (items[0].Path.IsFile && Path.Exists(items[0].Path.AbsolutePath) &&
            string.Equals(Path.GetExtension(items[0].Path.AbsolutePath), ".png", StringComparison.Ordinal))
        {
            result = DragDropEffects.Copy;
        }

        return result;
    }

    private async void DropButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel? top = TopLevel.GetTopLevel(this);
        if (top is null)
        {
            return;
        }

        IReadOnlyList<IStorageFile> result = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImagePng],
            Title = "Select logo image",
        });

        if (result.Count == 1)
        {
            ViewModel!.IconPath = result[0].Path.AbsolutePath;
            Debug.WriteLine($"{ViewModel.IconPath}");
        }
    }
}