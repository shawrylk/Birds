using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Assets.Scripts.Lock.Guard;

namespace Assets.Scripts.Utilities
{
    public class BlockingList<T> : List<T>
    {
        public BlockingList(IEnumerable<T> list) : base(list) { }
        public BlockingList() : base() { }

        private object _lock = new object();

        public new int Count
        {
            get
            {
                lock (_lock)
                {
                    return base.Count;
                }
            }
        }
        public new void Add(T item)
        {
            lock (_lock)
            {
                base.Add(item);
            }
        }

        public new void Remove(T item)
        {
            lock (_lock)
            {
                base.Remove(item);
            }
        }

        public new void Clear()
        {
            lock (_lock)
            {
                base.Clear();
            }
        }

        public new bool Contains(T item)
        {
            lock (_lock)
            {
                return base.Contains(item);
            }
        }

        public new void ForEach(Action<T> action)
        {
            lock (_lock)
            {
                base.ForEach(action);
            }
        }

        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                return Enumerable.Where(this, predicate);
            }
        }

        public BlockingList<T> ToList()
        {
            lock (_lock)
            {
                return new BlockingList<T>(this);
            }
        }
        public IDisposable GetLock()
        {
            Monitor.Enter(_lock);
            return new Releaser(() =>
            {
                Monitor.Exit(_lock);
            });
        }

    }
}
