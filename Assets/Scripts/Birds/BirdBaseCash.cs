using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace Assets.Scripts.Birds
{
    public static class BirdBaseCashExtensions
    {
        public sealed class CashOptions
        {
            public (float timeStep, float variationRange) CoroutineTime;
            public List<(float timeOut, float variationRange, GameObject cash)> CashInfo;
            public Func<int, IEnumerator> HzedOutHandler;
        }

        public static Func<CancellationToken, IEnumerator> GetProduceCashCoroutine(
            this BirdBase birdBase,
            Action<CashOptions> optionsCallback)
        {
            var options = new CashOptions();
            optionsCallback?.Invoke(options);

            (float timeStep, float variationRange) coroutineTime = options.CoroutineTime;
            List<(float timeOut, float variationRange, GameObject cash)> cashInfo = options.CashInfo;
            Func<int, IEnumerator> hzedOutHandler = options.HzedOutHandler;

            (Func<float, float> sToHz, Func<int, float> getHzOut) setUpTime()
            {
                var sToHz = coroutineTime.timeStep.GetSToHzHandler();
                float getHzOut(int index) => sToHz(cashInfo[index].timeOut + Range(-cashInfo[index].variationRange, cashInfo[index].variationRange));
                return (sToHz, getHzOut);
            }

            IEnumerable<int> getIndexIncrementHandler()
            {
                var index = 0;
                var count = cashInfo.Count - 1;
                while (index <= count)
                {
                    yield return index++;
                }
            }

            var indices = getIndexIncrementHandler().GetEnumerator();
            indices.MoveNext();

            var (sToHz, getHzOut) = setUpTime();
            var hzOut = getHzOut(indices.Current);
            var coinManager = Global.GameObjects.GetGameObject(Global.CASH_MANAGER_TAG);
            var currentCoin = cashInfo[indices.Current].cash;
            var hz = 0;
            var hzedOutHandlerChanged = true;

            IEnumerator produceCash(CancellationToken token)
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    yield return new WaitForSeconds(coroutineTime.timeStep + Range(-coroutineTime.variationRange, coroutineTime.variationRange));

                    if (currentCoin != null)
                    {
                        var coin = GameObject.Instantiate(
                            original: currentCoin,
                            position: birdBase.transform.position,
                            rotation: Quaternion.identity,
                            parent: coinManager.transform);
                    }

                    if (hzedOutHandlerChanged && ++hz >= hzOut)
                    {
                        yield return hzedOutHandler?.Invoke(indices.Current);
                        hzedOutHandlerChanged = indices.MoveNext();
                        hzOut = getHzOut(indices.Current);
                        currentCoin = cashInfo[indices.Current].cash;
                    }
                }
            }

            return produceCash;
        }
    }
}
