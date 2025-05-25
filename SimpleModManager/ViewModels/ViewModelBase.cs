using System;
using System.Reactive.Disposables;
using ReactiveUI;

namespace SimpleModManager.ViewModels;

public class ViewModelBase : ReactiveObject, IDisposable
{
    protected readonly CompositeDisposable Disposables = new CompositeDisposable();
    private bool _isDisposed = false;

    protected virtual void Dispose(bool dispoing)
    {
        if (_isDisposed)
        {
            return;
        }
        
        if (dispoing)
        {
            Disposables.Dispose();
        }

        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}