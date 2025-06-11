using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SimpleModManager.Utils;

public partial class CopyProgressViewModel : ReactiveObject
{
    [Reactive] private double _value = 0;

    public CopyProgressViewModel(Progress<double> progress)
    {
        progress.ProgressChanged += ProgressOnProgressChanged; 
    }

    private void ProgressOnProgressChanged(object? sender, double e)
    {
        Value = e;
    }
}
