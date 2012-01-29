namespace LocalReportsEngine
{
    using System;

    public class ResolvableResource<TResource, TResult> : IDisposable
    {
        public readonly TResource Resource;

        public bool IsResolved { get; set; }

        public TResult Result { get; set; }

        public bool DisposeResult { get; set; }

        public bool IsDisposed { get; private set; }

        protected ResolvableResource(TResource resource)
        {
            Resource = resource;
        }

        ~ResolvableResource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                if (DisposeResult)
                {
                    var disposable = Result as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }

            IsDisposed = true;
        }

        protected virtual void OnResolved()
        {
        }
    }
}
