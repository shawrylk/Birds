using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class Lock
    {
        public sealed class Guard
        {
            private readonly object _gate = new object();
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
            private async Task<(bool, IDisposable)> TryLockAsync()
            {
                if (_semaphore.CurrentCount > 0)
                {
                    await _semaphore.WaitAsync();
                    return (true, new Releaser(() => _semaphore.Release()));
                }
                return (false, new Releaser(() => { }));
            }
            private (bool, IDisposable) TryLock()
            {
                if (_semaphore.CurrentCount > 0)
                {
                    _semaphore.Wait();
                    return (true, new Releaser(() => _semaphore.Release()));
                }
                return (false, new Releaser(() => { }));
            }
            public AwaittableReleaser<(bool, IDisposable)> TryGetAsync()
            {
                lock (_gate)
                {
                    return new AwaittableReleaser<(bool, IDisposable)>(TryLockAsync());
                }
            }
            public (bool, IDisposable) TryGet()
            {
                lock (_gate)
                {
                    return TryLock();
                }
            }

            public struct AwaittableReleaser<T> : IDisposable
            {
                private readonly Task<T> _task;

                public AwaittableReleaser(Task<T> task) => _task = task;

                public void Dispose() => _task.Dispose();
                public TaskAwaiter<T> GetAwaiter() => _task.GetAwaiter();
                public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCaptureContext) => _task.ConfigureAwait(continueOnCaptureContext);
                public Task<T> AsTask() => _task;
            }

            public struct Releaser : IDisposable
            {
                private readonly Action _action;

                public Releaser(Action action) => _action = action;

                public void Dispose() => _action?.Invoke();
            }
        }
        public sealed class AsyncLock
        {
            private readonly object _gate = new object();
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
            private readonly AsyncLocal<int> _recursionCount = new AsyncLocal<int>();

            public ValueTask<AsyncLockReleaser> LockAsync()
            {
                var shouldAcquire = false;

                lock (_gate)
                {
                    if (_recursionCount.Value == 0)
                    {
                        shouldAcquire = true;
                        _recursionCount.Value = 1;
                    }
                    else
                    {
                        _recursionCount.Value++;
                    }
                }

                if (shouldAcquire)
                {
                    return new ValueTask<AsyncLockReleaser>(_semaphore.WaitAsync().ContinueWith(_ => new AsyncLockReleaser(this)));
                }

                return new ValueTask<AsyncLockReleaser>(new AsyncLockReleaser(this));
            }

            private void Release()
            {
                lock (_gate)
                {
                    //Debug.Assert(_recursionCount.Value > 0);

                    if (--_recursionCount.Value == 0)
                    {
                        _semaphore.Release();
                    }
                }
            }

            public struct AsyncLockReleaser : IDisposable
            {
                private readonly AsyncLock _parent;

                public AsyncLockReleaser(AsyncLock parent) => _parent = parent;

                public void Dispose() => _parent.Release();
            }
        }
    }
}
