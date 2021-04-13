using Microsoft.Extensions.DependencyInjection;
using System;

namespace Tavenem.Blazor.ImageEditor
{
    /// <summary>
    /// A base class that creates a service provider scope, and resolves a service of type <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <remarks>
    /// Use the <see cref="AsyncOwningComponentBase{TService}"/> class as a base class to author components that control
    /// the lifetime of a service or multiple services. This is useful when using a transient or scoped service that
    /// requires disposal such as a repository or database abstraction. Using <see cref="AsyncOwningComponentBase{TService}"/>
    /// as a base class ensures that the service and relates services that share its scope are disposed with the component.
    /// </remarks>
    public abstract class AsyncOwningComponentBase<TService> : AsyncOwningComponentBase where TService : notnull
    {
        private TService _item = default!;

        /// <summary>
        /// Gets the <typeparamref name="TService"/> that is associated with this component.
        /// </summary>
        protected TService Service
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                // We cache this because we don't know the lifetime. We have to assume that it could be transient.
                _item ??= ScopedServices.GetRequiredService<TService>();
                return _item;
            }
        }
    }
}
