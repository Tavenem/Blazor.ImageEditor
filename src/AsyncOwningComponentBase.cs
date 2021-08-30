using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Tavenem.Blazor.ImageEditor;

/// <summary>
/// A base class that creates a service provider scope.
/// </summary>
/// <remarks>
/// Use the <see cref="AsyncOwningComponentBase"/> class as a base class to author components that control
/// the lifetime of a service provider scope. This is useful when using a transient or scoped service that
/// requires disposal such as a repository or database abstraction. Using <see cref="AsyncOwningComponentBase"/>
/// as a base class ensures that the service provider scope is disposed with the component.
/// </remarks>
public abstract class AsyncOwningComponentBase : ComponentBase, IAsyncDisposable
{
    private IServiceScope? _scope;

    [Inject] private protected IServiceScopeFactory ScopeFactory { get; set; } = default!;

    /// <summary>
    /// Gets a value determining if the component and associated services have been disposed.
    /// </summary>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the scoped <see cref="IServiceProvider"/> that is associated with this component.
    /// </summary>
    protected IServiceProvider ScopedServices
    {
        get
        {
            if (ScopeFactory is null)
            {
                throw new InvalidOperationException("Services cannot be accessed before the component is initialized.");
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            _scope ??= ScopeFactory.CreateScope();
            return _scope.ServiceProvider;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous dispose operation.
    /// </returns>
    protected virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous dispose operation.
    /// </returns>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (!IsDisposed)
        {
            if (_scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _scope?.Dispose();
            }
            _scope = null;
            await DisposeAsync().ConfigureAwait(false);
            IsDisposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
