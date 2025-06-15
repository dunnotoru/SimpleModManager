using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SimpleModManager.Utils;

public partial class CopyProgressValue : ReactiveObject, IProgress<double>
{
    [Reactive] private double _value = 0;
    
    public void Report(double value)
    {
        Value = value;
    }
}
