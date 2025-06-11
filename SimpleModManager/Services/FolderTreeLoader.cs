using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using SimpleModManager.ViewModels;

namespace SimpleModManager.Services;

public class FolderTreeLoader
{
    public static IEnumerable<FolderItem> LoadFolderContents(string path)
    {
        List<FolderItem> result = new List<FolderItem>();
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        FileInfo[] files = dirInfo.GetFiles();
        DirectoryInfo[] directories = dirInfo.GetDirectories();
        
        foreach (FileInfo file in files)
        {
            result.Add(new FolderItem(file.Name, false, file.FullName));
        }

        foreach (DirectoryInfo directory in directories)
        {
            FolderItem folderItem = new FolderItem(directory.Name, true, directory.FullName)
            {
                Children = new ObservableCollection<FolderItem>(LoadFolderContents(directory.FullName))
            };
            
            result.Add(folderItem);
        }
        
        return result.OrderBy(item => !item.IsDirectory).ThenBy(item => item.Name);
    }
    
    public static IEnumerable<FolderItem> GetAllItems(IEnumerable<FolderItem> rootItems)
    {
        return rootItems.SelectMany(item =>
            new[] { item }.Concat(GetAllItems(item.Children))
        );
    }
}

public class FolderItem : ReactiveObject
{
    public string Name { get; }
    public string Path { get; }
    public bool IsDirectory { get; set; }
    public ObservableCollection<FolderItem> Children { get; set; }
    
    private bool _isChecked = false;
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            this.RaiseAndSetIfChanged(ref _isChecked, value);
            foreach (FolderItem child in Children)
            {
                child.IsChecked = value;
            }
        }
    }

    public FolderItem(string name, bool isDirectory, string path)
    {
        Name = name;
        IsDirectory = isDirectory;
        Path = path;
        Children = new ObservableCollection<FolderItem>();
    }
}