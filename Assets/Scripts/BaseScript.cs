using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class BaseScript: MonoBehaviour
    {
        protected CancellationTokenSource _cancelSource = new CancellationTokenSource();
        protected List<IDisposable> _disposables = new List<IDisposable>();
        protected virtual void OnDestroy()
        {
            _cancelSource.Cancel();
            _disposables.ForEach(d => d.Dispose());
            StopAllCoroutines();
        }

        protected System.Collections.IEnumerator Delay(TimeSpan time, Action action)
        {
            yield return new WaitForSeconds((float)time.TotalSeconds);
            action?.Invoke();
        }
    }
}
